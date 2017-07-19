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
using WSP.AnalyserActor.Interfaces;
using WSP.DBHandlerService.Interfaces;

namespace WSP.MasterActor {

    [ActorService( Name = "MasterActorService" )]
    [StatePersistence( StatePersistence.Persisted )]
    internal class MasterActor: BaseActor, IMasterActor, IRemindable {

        /************* Helper Classes for String definitions *************/

        protected abstract class NewStateNames: StateNames {
            public const string TheMinersToCreate = "theMinersToCreate";
            public const string TheMinersToFinish = "theMinersToFinish";
            public const string TheAnalysersToCreate = "theAnalysersToCreate";
            public const string TheAnalysersToFinish = "theAnalysersToFinish";
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

            // Flags to track which Miners, Analysers are left to be Created or Finish
            await StateManager.TryAddStateAsync<MiningSource>( NewStateNames.TheMinersToCreate, null );
            await StateManager.TryAddStateAsync<MiningSource>( NewStateNames.TheMinersToFinish, null );
            await StateManager.TryAddStateAsync<MiningSource>( NewStateNames.TheAnalysersToCreate, null );
            await StateManager.TryAddStateAsync<MiningSource>( NewStateNames.TheAnalysersToFinish, null );
            // The Results that will be calculated in order to Fulfill the SearchRequest
            await StateManager.TryAddStateAsync<Results>( NewStateNames.TheResults, null );
        }

        /******************** State Management Methods ********************/

        private async Task SetTheSearchRequestStatus(Status newStatus) {
            BESearchRequest newSearchRequest = await GetTheSearchRequest();
            newSearchRequest.TheStatus = newStatus;
            await SaveTheSearchRequest( newSearchRequest );
        }

        private Task<MiningSource> GetTheSourcesLeft(string TheSourcesLeftJob) {
            return StateManager.GetStateAsync<MiningSource>( TheSourcesLeftJob );
        }

        private async Task SaveTheSourcesLeft(string TheMinerLeftJob, MiningSource minersLeft) {
            await StateManager.SetStateAsync( TheMinerLeftJob, new MiningSource( minersLeft ) );
            await SaveStateAsync();
        }

        private async Task<MiningSource> DecreaseTheSourcesLeft(string TheSourcesLeftJob, SourceOption finishedSourceID) {
            MiningSource sourcesLeft = await GetTheSourcesLeft( TheSourcesLeftJob );
            sourcesLeft.RemoveSource( finishedSourceID );
            await SaveTheSourcesLeft( TheSourcesLeftJob, sourcesLeft );
            return sourcesLeft;
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
            await SaveTheSourcesLeft( NewStateNames.TheMinersToCreate, searchRequest.TheSelectedSources );
            await SaveTheSourcesLeft( NewStateNames.TheMinersToFinish, searchRequest.TheSelectedSources );
            await SaveTheSourcesLeft( NewStateNames.TheAnalysersToCreate, searchRequest.TheSelectedSources );
            await SaveTheSourcesLeft( NewStateNames.TheAnalysersToFinish, searchRequest.TheSelectedSources );

            // Set the Reminder for the method that implements the core logic of the Actor (mainFulfillSearchRequestAsync)
            await RegisterReminderAsync( ReminderNames.FulfillSReqReminder );
        }

        public async Task UpdateSearchRequestStatus(Status newStatus, SourceOption finishedSourceID) {
            Status currentStatus = (await GetTheSearchRequest()).TheStatus;
            // Avoid going backwards
            if(newStatus <= currentStatus) {
                return;
            }

            // In the case that the Miners are being awaited, progress when all of them have finished
            if(newStatus == Status.Mining_Done) { // If a miner finished
                MiningSource minersLeft = await DecreaseTheSourcesLeft( NewStateNames.TheMinersToFinish, finishedSourceID );
                if(minersLeft.IsEmpty() != true) { // If there are miners that haven't finished yet
                    return;
                }
            }

            // In the case that the Analysers are being awaited, progress when all of them have finished
            if(newStatus == Status.Analysing_Done) { // If an analysers finished
                MiningSource analysersLeft = await DecreaseTheSourcesLeft( NewStateNames.TheAnalysersToFinish, finishedSourceID );
                if(analysersLeft.IsEmpty() != true) { // If there are analysers that haven't finished yet
                    return;
                }
            }

            await SetTheSearchRequestStatus( newStatus );
            await RegisterReminderAsync( ReminderNames.FulfillSReqReminder );
        }

        /******************** Reminder Management Method ********************/

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

        // Fullfills the SearchRequest. 
        private async Task mainFulfillSearchRequestAsync(){
            BESearchRequest theSearchRequest = await GetTheSearchRequest();

            Status theStatus = theSearchRequest.TheStatus;
            switch(theStatus) {
                case Status.New:
                    await initExecution( theSearchRequest );
                    await activateMiners( theSearchRequest );
                    // All Miners were created successfully
                    await SetTheSearchRequestStatus( Status.Mining );
                    break;

                case Status.Mining:
                    await activateAnalysers( theSearchRequest );                    
                    break;

                case Status.Mining_Done:
                    await activateAnalysers( theSearchRequest );
                    // All Analysers were created successfully
                    await UpdateSearchRequestStatus( Status.Analysing, 0 );
                    break;

                case Status.Analysing:
                    break;

                case Status.Analysing_Done:
                    if((await GetTheResults() == null)) {
                        await calculateResults();
                    }
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
            MiningSource desiredSources = await GetTheSourcesLeft( NewStateNames.TheMinersToCreate );
            Exception exOnMinerCall = null;

            foreach(var selectedSource in desiredSources.GetAsList()) {
                var minerUri = MinerActorsFactory.GetMinerUri( selectedSource );
                IMinerActor theMiner = ActorProxy.Create<IMinerActor>( new ActorId( theSearchRequest.ID ), minerUri );

                try {
                    await theMiner.StartMiningAsync( theSearchRequest );
                    await DecreaseTheSourcesLeft( NewStateNames.TheMinersToCreate, selectedSource );
                } catch(Exception ex) {
                    exOnMinerCall = ex;
                }
            }
            if(exOnMinerCall != null) { //A Miner failed
                throw exOnMinerCall;
            }            
        }

        private async Task activateAnalysers(BESearchRequest theSearchRequest) {
            MiningSource desiredSources = await GetTheSourcesLeft( NewStateNames.TheAnalysersToCreate );
            Exception exOnAnalyserCall = null;

            foreach(var selectedSource in desiredSources.GetAsList()) {
                var analyserUri = AnalyserActorsFactory.GetAnalyserUri( selectedSource );
                IAnalyserActor theAnalyser = ActorProxy.Create<IAnalyserActor>( new ActorId( theSearchRequest.ID ), analyserUri );

                try {
                    await theAnalyser.StartAnalysingAsync( theSearchRequest );
                    await DecreaseTheSourcesLeft( NewStateNames.TheAnalysersToCreate, selectedSource );
                } catch(Exception ex) {
                    exOnAnalyserCall = ex;
                }
            }
            if(exOnAnalyserCall != null) { //An Analyser failed
                throw exOnAnalyserCall;
            }
            
        }

        private async Task calculateResults() {
            // Temporary random Results
            float posScore = (float)(new System.Random()).NextDouble() * 100.0f;
            await SaveTheResults( new Results( posScore, 100 - posScore - 5 ) );
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
