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
            public const string TheResults = "theResults";
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

            // Flags to track wich Miners are left to be Created or Finish
            await StateManager.TryAddStateAsync<MiningSource>( NewStateNames.TheMinersToCreate, null );
            await StateManager.TryAddStateAsync<MiningSource>( NewStateNames.TheMinersToFinish, null );
            // The Results that will be calculated in order to Fulfill the SearchRequest
            await StateManager.TryAddStateAsync<Results>( NewStateNames.TheResults, null );
        }

        /******************** State Management Methods ********************/

        private async Task SetTheSearchRequestStatus(Status newStatus) {
            BESearchRequest newSearchRequest = await GetTheSearchRequest();
            newSearchRequest.TheStatus = newStatus;
            await SaveTheSearchRequest( newSearchRequest );
        }

        private Task<MiningSource> GetTheMinersLeft(string TheMinerLeftJob) {
            return StateManager.GetStateAsync<MiningSource>( TheMinerLeftJob );
        }

        private async Task SaveTheMinersLeft(string TheMinerLeftJob, MiningSource minersLeft) {
            await StateManager.SetStateAsync( TheMinerLeftJob, new MiningSource( minersLeft ) );
            await SaveStateAsync();
        }

        private async Task<MiningSource> DecreaseTheMinersLeft(string TheMinerLeftJob, SourceOption finishedMinerID) {
            MiningSource minersLeft = await GetTheMinersLeft( TheMinerLeftJob );
            minersLeft.RemoveSource( finishedMinerID );
            await SaveTheMinersLeft( TheMinerLeftJob, minersLeft );
            return minersLeft;
        }

        private Task<Results> GetTheResults() {
            return StateManager.GetStateAsync<Results>( NewStateNames.TheResults );
        }

        private async Task SaveTheResults(Results theResults) {
            await StateManager.SetStateAsync( NewStateNames.TheResults, theResults );
            await SaveStateAsync();
        }

        /******************** Actor Interface Methods ********************/

        public async Task FulfillSearchRequestAsync(BESearchRequest searchRequest) {
            // Initialize the values stored in the State Manager
            await SaveTheSearchRequest( searchRequest );
            await SaveTheMinersLeft( NewStateNames.TheMinersToCreate, searchRequest.TheSelectedSources );
            await SaveTheMinersLeft( NewStateNames.TheMinersToFinish, searchRequest.TheSelectedSources );

            // Set the Reminder for the method that implements the core logic of the Actor (mainFulfillSearchRequestAsync)
            await RegisterReminderAsync( ReminderNames.FulfillSReqReminder );
        }

        public async Task UpdateSearchRequestStatus(Status newStatus, SourceOption finishedMinerID) {
            Status currentStatus = (await GetTheSearchRequest()).TheStatus;
            // Avoid going backwards
            if(newStatus <= currentStatus) { 
                return;
            }

            // In the case that the Miners are being awaited, progress when all of them have finished
            if(newStatus == Status.Mining_Done) { // If a miner finished
                MiningSource minersLeft = await DecreaseTheMinersLeft( NewStateNames.TheMinersToFinish, finishedMinerID );
                if(minersLeft.IsEmpty() != true) { // If there are miners that haven't finished yet
                    return;
                }
            }

            await SetTheSearchRequestStatus( newStatus );
            await RegisterReminderAsync( ReminderNames.FulfillSReqReminder );
        }

        /******************** Remider Management Method ********************/

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan duelTIme, TimeSpan period) {
            switch(reminderName) {

                case ReminderNames.FulfillSReqReminder:
                    try {
                        await UnregisterReminderAsync( GetReminder( ReminderNames.FulfillSReqReminder ) );
                        await mainFulfillSearchRequestAsync();
                    } catch {
                        await RegisterReminderAsync( ReminderNames.FulfillSReqReminder );
                    }
                    break;

                case ReminderNames.SendResultsReminder:
                    try {
                        await UnregisterReminderAsync( GetReminder( ReminderNames.SendResultsReminder ) );
                        await submitResults();
                    } catch {
                        await RegisterReminderAsync( ReminderNames.SendResultsReminder );
                    }
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
                    await initExecution( theSearchRequest );
                    await activateMiners( theSearchRequest );
                    break;

                case Status.Mining:
                    break;

                case Status.Mining_Done:
                    // Create temporary random Results
                    float negScore = (float)(new System.Random()).NextDouble() * 100.0f;
                    await SaveTheResults( new Results( negScore, -negScore ) );
                    await UpdateSearchRequestStatus( Status.Fulfilled, 0 );
                    break;

                case Status.Fulfilled:
                    await updateDBEntries();
                    // Set a Reminder to Send the Results to the Website
                    await RegisterReminderAsync( ReminderNames.SendResultsReminder );
                    break;
            }
        }

        /************************* Internal  Methods *************************/

        // Creates the BEExecution, in a Reliable way (if it hasn't already been created)
        private async Task initExecution(BESearchRequest theSearchRequest) {
            if(theSearchRequest.hasCreatedLastExecution() == false) {
                BEExecution createdBEExecution = await dbHandlerService.StoreExecution(
                    new BEExecution( theSearchRequest.ID, DateTime.Now, null ) );
                theSearchRequest.ActiveExecutionID = createdBEExecution.ID;
                await SaveTheSearchRequest( theSearchRequest );
            }
        }

        // Creates and Activate all the MinerActors, in a Reliable way. 
        // Throws exception in case a Miner failed to activate, so that is can be retried.
        private async Task activateMiners(BESearchRequest theSearchRequest) {
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
            await SetTheSearchRequestStatus( Status.Mining );
        }

        // Updates all the necessary DB Entries when the SearchRequest is successfully fulfilled
        private async Task updateDBEntries() {
            BESearchRequest theSearchRequest = await GetTheSearchRequest();
            Results theResults = await GetTheResults();
            await dbHandlerService.UpdateSearchRequestFulfilled( theSearchRequest.ID, theSearchRequest.ActiveExecutionID, theResults );
        }

        // Sumbits the Results to the DB and send them to the Website. Invoked by a Reminder (SendResultsReminder)
        private async Task submitResults() {
            var receivedSReqID = (await GetTheSearchRequest()).TheReceivedID;
            // Send the Results to the WebSite
            Results theResults = await GetTheResults();
            theResults.ID = receivedSReqID;
            var response = await clientFEserver.PostAsJsonAsync( "api/Results/Submit", theResults );

            // Upon unsuccessful transmission, throw exception so that it will be attempted again
            if(response.IsSuccessStatusCode != true) {
                throw new Exception();
            }
        }



    }
}
