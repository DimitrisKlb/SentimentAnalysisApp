using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;

using SentimentAnalysisApp.SharedModels;
using WSP.MasterActor.Interfaces;
using WSP.Models;
using WSP.MyActors;
using WSP.MinerActor.Interfaces;
using WSP.DBHandlerService.Interfaces;

namespace WSP.MasterActor {

    [ActorService( Name = "MasterActorService" )]
    [StatePersistence( StatePersistence.Persisted )]
    internal class MasterActor: BaseActor, IMasterActor, IRemindable {

        /************* Helper Classes for String definitions *************/

        protected abstract class NewStateNames: StateNames {
            public const string TheMinersToCreate = "theMinersToCreate";
            public const string TheMinersToFinish = "theMinersToFinish";
        }

        protected abstract class ReminderNames {
            public const string FulfillSReqReminder = "FulfillSearchRequest";
            public const string SendResultsReminder = "SendResults";
        }

        /******************** Fields and Core Methods ********************/

        private HttpClient clientFEserver;
        private IDBHandlerService dbHandlerService;

        public MasterActor(ActorService actorService, ActorId actorId)
            : base( actorService, actorId ) {
        }

        protected override async Task OnActivateAsync() {
            await base.OnActivateAsync();
            ActorEventSource.Current.ActorMessage( this, "MasterActor {0} activated.", this.Id );

            clientFEserver = new HttpClient {
                BaseAddress = new Uri(
                   configSettings.Settings.Sections["WebSiteInfo"].Parameters["WebSiteURI"].Value )
            };

            dbHandlerService = ServiceProxy.Create<IDBHandlerService>(
                new Uri( configSettings.Settings.Sections["ApplicationServicesNames"].Parameters["DBHandlerName"].Value ),
                new ServicePartitionKey( 1 ) );

            await StateManager.TryAddStateAsync<MiningSource>( NewStateNames.TheMinersToCreate, null );
            await StateManager.TryAddStateAsync<MiningSource>( NewStateNames.TheMinersToFinish, null );
        }

        /******************** State Management Methods ********************/

        private async Task SetTheSerchRequestStatus(Status newStatus) {
            BESearchRequest newSearchRequest = await GetTheSearchRequest();
            newSearchRequest.TheStatus = newStatus;
            await SaveTheSearchRequest( newSearchRequest );
        }

        private Task<MiningSource> GetTheMinersLeft(string TheMinerLeftJob) {
            return StateManager.GetStateAsync<MiningSource>( TheMinerLeftJob );
        }

        protected async Task SaveTheMinersLeft(string TheMinerLeftJob, MiningSource minersLeft) {
            await StateManager.SetStateAsync( TheMinerLeftJob, new MiningSource( minersLeft ) );
            await SaveStateAsync();
        }

        private async Task<MiningSource> DecreaseTheMinersLeft(string TheMinerLeftJob, SourceOption finishedMinerID) {
            MiningSource minersLeft = await GetTheMinersLeft( TheMinerLeftJob );
            minersLeft.RemoveSource( finishedMinerID );
            await SaveTheMinersLeft( TheMinerLeftJob, minersLeft );
            return minersLeft;
        }

        /******************** Actor Interface Methods ********************/

        public async Task FulfillSearchRequestAsync(BESearchRequest searchRequest) {
            // Initialize the SearchRequest in the state manager if this MasterActor is called for the first time      
            if(await GetTheSearchRequest() == null) {
                await SaveTheSearchRequest( searchRequest );
                await SaveTheMinersLeft( NewStateNames.TheMinersToCreate, searchRequest.TheSelectedSources );
                await SaveTheMinersLeft( NewStateNames.TheMinersToFinish, searchRequest.TheSelectedSources );
            }

            // Set the Reminder for the method that implements the core logic of the Actor (mainFulfillSearchRequestAsync)
            try {
                await RegisterReminderAsync( ReminderNames.FulfillSReqReminder, null, TimeSpan.FromSeconds( 10 ), TimeSpan.FromSeconds( 10 ) );
            } catch(Exception) {
                throw;
            }

        }

        public async Task UpdateSerchRequestStatus(Status newStatus, SourceOption finishedMinerID) {
            if(newStatus == Status.Mining_Done) { // If a miner finished
                MiningSource minersLeft = await DecreaseTheMinersLeft( NewStateNames.TheMinersToFinish, finishedMinerID );
                if(minersLeft.IsEmpty() != true) { // If there are miners that haven't finished yet
                    return;
                }
            }
            await SetTheSerchRequestStatus( newStatus );

            try {
                await RegisterReminderAsync( ReminderNames.FulfillSReqReminder, null, TimeSpan.FromSeconds( 10 ), TimeSpan.FromSeconds( 10 ) );
            } catch(Exception) {
                throw;
            }
        }

        /******************** Remider Management Method ********************/

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan duelTIme, TimeSpan period) {
            switch(reminderName) {

                case ReminderNames.FulfillSReqReminder:
                    try {
                        await UnregisterReminderAsync( this.GetReminder( ReminderNames.FulfillSReqReminder ) );
                        await mainFulfillSearchRequestAsync();
                    } catch {
                        await RegisterReminderAsync( ReminderNames.FulfillSReqReminder, null, TimeSpan.FromSeconds( 10 ), TimeSpan.FromSeconds( 10 ) );
                    }
                    break;

                case ReminderNames.SendResultsReminder:
                    await submitResults();
                    break;

                default:
                    // This point should never be reached. 
                    throw new InvalidOperationException( "Unknown Reminder: " + reminderName );
            }
        }

        /******************** Actor Logic Implementation Methods ********************/

        // Fullfills the SearchRequest. Invoked by a Reminder (FulfillSReqReminder)
        private async Task mainFulfillSearchRequestAsync() {
            BESearchRequest theSearchRequest = await GetTheSearchRequest();

            Status theStatus = theSearchRequest.TheStatus;
            switch(theStatus) {
                case Status.New:
                    //Call the Miners
                    MiningSource desiredSources = await GetTheMinersLeft( NewStateNames.TheMinersToCreate );
                    Exception exOnMinerCall = null;

                    foreach(var selectedSource in desiredSources.GetAsList()) {
                        var minerUri = MinerActorsFactory.GetMinerUri( selectedSource );
                        IMinerActor theMiner = ActorProxy.Create<IMinerActor>( new ActorId( theSearchRequest.ID ), minerUri );

                        try {
                            await theMiner.StartMiningAsync( theSearchRequest );
                            await DecreaseTheMinersLeft( NewStateNames.TheMinersToCreate, selectedSource );
                        } catch(Exception ex) {
                            exOnMinerCall = ex;
                        }
                    }
                    if(exOnMinerCall != null) { //A Miner failed
                        throw exOnMinerCall;
                    }
                    // All Miners were created successfully
                    await SetTheSerchRequestStatus( Status.Mining );

                    break;

                case Status.Mining:

                    break;

                case Status.Mining_Done:
                    // Update the Fulfilled SearchRequest's DB Entry
                    //await dbHandlerService.UpdateBESearchRequest( await GetTheSearchRequest() );

                    // Set a Reminder to Send the Results to the Website
                    try {
                        await RegisterReminderAsync( ReminderNames.SendResultsReminder, null, TimeSpan.FromSeconds( 10 ), TimeSpan.FromSeconds( 10 ) );
                    } catch(Exception) {
                        throw;
                    }

                    break;
            }
        }

        // Sumbit the Results to the DB and send them to the Website. Invoked by a Reminder (SendResultsReminder)
        private async Task submitResults() {
            await UnregisterReminderAsync( this.GetReminder( ReminderNames.SendResultsReminder ) );

            var receivedSReqID = (await GetTheSearchRequest()).TheReceivedID; // Temporary Result Type

            float negScore = (float)(new System.Random()).NextDouble() * 100.0f;
            Results theResults = new Results( negScore, -negScore ); //Random Results for now

            // Create the Execution object concerning the execution that was just completed, with the calculated Results

            // Send the Results to the WebSite
            theResults.ID = receivedSReqID;
            var response = await clientFEserver.PostAsJsonAsync( "api/Results/Submit", theResults );

            // Upon unsuccessful transmission, retry to send the results
            if(response.IsSuccessStatusCode != true) {
                await RegisterReminderAsync( ReminderNames.SendResultsReminder, null, TimeSpan.FromSeconds( 10 ), TimeSpan.FromSeconds( 10 ) );
            }

        }


    }
}
