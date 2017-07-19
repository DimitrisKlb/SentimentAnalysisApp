using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Fabric;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Client;

using SentimentAnalysisApp.SharedModels;
using WSP.AnalyserActor.Interfaces;
using WSP.MasterActor.Interfaces;
using WSP.Models;
using WSP.MyActors;
using WSP.DBHandlerService.Interfaces;

namespace WSP.MyActors {

    [StatePersistence( StatePersistence.Persisted )]
    public abstract class BaseAnalyserActor: BaseActor, IAnalyserActor, IRemindable {

        /************* Helper Classes for String definitions *************/

        protected abstract class ReminderNames {
            public const string AnalyseBeginReminder = "AnalyseBegin";
            public const string AnalyseReminder = "Analyse";
            public const string AnalyseCompleteReminder = "AnalyseComplete";
        }

        protected abstract class NewStateNames: StateNames {
            public const string TheMinerData = "theMinerData";
        }

        /******************** Fields and Core Methods ********************/

        protected IDBHandlerService dbHandlerService;

        protected int textsRequestWindowSize = 1000;
        // The time the actor will wait before requesting texts again, after a request has wielded no results
        protected double textsRequestRetryTime = 1000; // In msec
        //protected TextAnalyser theTextAnalyser;

        protected abstract SourceOption TheSourceID { get; }

        public BaseAnalyserActor(ActorService actorService, ActorId actorId)
            : base( actorService, actorId ) {
        }

        protected override async Task OnActivateAsync() {
            await base.OnActivateAsync();

            dbHandlerService = ServiceProxy.Create<IDBHandlerService>(
                new Uri( configSettings.Settings.Sections["ApplicationServicesNames"].Parameters["DBHandlerName"].Value ),
                new ServicePartitionKey( 1 ) );

            await StateManager.TryAddStateAsync<MinerData>( NewStateNames.TheMinerData, null );
        }

        /******************** State Management Methods ********************/

        protected Task<MinerData> GetTheMinerData() {
            return StateManager.GetStateAsync<MinerData>( NewStateNames.TheMinerData );
        }

        protected async Task SaveTheMinerData(MinerData theMinerData) {
            await StateManager.SetStateAsync( NewStateNames.TheMinerData, theMinerData );
            await SaveStateAsync();
        }

        /******************** Actor Interface Methods ********************/

        public async Task StartAnalysingAsync(BESearchRequest searchRequest) {
            // Initialize the values stored in the State Manager
            await SaveTheSearchRequest( searchRequest );
            // Set the Reminder for the method that implements the core logic of the Actor ()
            await RegisterReminderAsync( ReminderNames.AnalyseBeginReminder );
        }


        /******************** Reminder Management Method ********************/

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan duelTIme, TimeSpan period) {
            // Unregister the Received Reminder
            await UnregisterReminderAsync( GetReminder( reminderName ) );

            switch(reminderName) {
                case ReminderNames.AnalyseBeginReminder:
                    try {
                        await onAnalyseBeginAsync();
                        // Set the Reminder for the method that implements the core logic of the Actor (mainAnalyseAsync)
                        await RegisterReminderAsync( ReminderNames.AnalyseReminder );
                    } catch {
                        await RegisterReminderAsync( ReminderNames.AnalyseBeginReminder );
                    }
                    break;

                case ReminderNames.AnalyseReminder:
                    try {
                        bool isDone = await mainAnalyseAsync();
                        if(isDone == true) {
                            await RegisterReminderAsync( ReminderNames.AnalyseCompleteReminder );
                        } else {
                            await RegisterReminderAsync( ReminderNames.AnalyseReminder, null, TimeSpan.FromMilliseconds( textsRequestRetryTime ), TimeSpan.FromMilliseconds( textsRequestRetryTime ) );
                        }
                    } catch {
                        await RegisterReminderAsync( ReminderNames.AnalyseReminder );
                    }
                    break;

                case ReminderNames.AnalyseCompleteReminder:
                    try {
                        IMasterActor theMasterActor = ActorProxy.Create<IMasterActor>( this.Id );
                        await theMasterActor.UpdateSearchRequestStatus( Status.Analysing_Done, TheSourceID );
                    } catch {
                        await RegisterReminderAsync( ReminderNames.AnalyseCompleteReminder );
                    }
                    break;

                default:
                    // This point should never be reached. 
                    throw new InvalidOperationException( "Unknown Reminder: " + reminderName );
            }
        }

        /******************** Actor Logic Implementation Methods ********************/

        // Called before the Analyser starts its job (mainAnalyseAsync)
        protected async Task onAnalyseBeginAsync() {
            //theTextAnalyser = new TextAnalyser( dataPackage.Path );
        }

        // Basic Actor method.
        private async Task<bool> mainAnalyseAsync() {
            BESearchRequest theSearchRequest;
            MinerData theMinerData;
            IEnumerable<BEMinedText> theMinedTexts;

            theSearchRequest = await GetTheSearchRequest();
            theMinerData = await GetTheMinerData();

            while(true) {
                // Request unprocessed texts from the DB
                theMinedTexts = await dbHandlerService.GetMinedTexts( theSearchRequest.ActiveExecutionID, TheSourceID, TextStatus.New, textsRequestWindowSize );

                if(theMinedTexts != null && theMinedTexts.Count() != 0) {

                    // Analyse each text
                    foreach(BEMinedText minedText in theMinedTexts) {
                        float posScoreSum, negScoreSum;

                        try {
                            //theTextAnalyser.analyseText( minedText.TheText, out posScoreSum, out negScoreSum );
                            //minedText.ThePosScoreSum = posScoreSum;
                            //minedText.TheNegScoreSum = negScoreSum;
                            minedText.TheStatus = TextStatus.Processed;
                        } catch {
                        }
                    }
                    // Write back the analysed texts to the DB
                    await dbHandlerService.UpdateMinedTexts( theMinedTexts );

                } else {    // No texts were returned
                    // Determine whether there are no more texts right now, or all of the texts were successfully analysed.
                    if(theMinerData == null) {
                        theMinerData = await dbHandlerService.GetMinerData( theSearchRequest.ActiveExecutionID, TheSourceID );
                        if(theMinerData == null) {
                            return false;
                        } else {
                            await SaveTheMinerData( theMinerData );
                        }
                    }

                    int processedTextsCount = await dbHandlerService.GetMinedTextsCount( theSearchRequest.ActiveExecutionID, TheSourceID, TextStatus.Processed );
                    if(processedTextsCount < theMinerData.TheTextsNum) { // There are more texts
                        return false;
                    } else { // All texts were processed
                        return true;
                    }
                }

            }

        }



    }


}
