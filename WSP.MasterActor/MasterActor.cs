using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;

using SentimentAnalysisApp.SharedModels;
using WSP.MasterActor.Interfaces;
using WSP.Models;
using WSP.MinerActor.Interfaces;

namespace WSP.MasterActor {

    /******************** Helper Classes for String definitions ********************/

    static internal class StateNames {
        public const string TheSearchRequest = "theSearchRequest";
    }
    static internal class ReminderNames {
        public const string FulfillSReqReminder = "FulfillSearchRequest";
        public const string SendResultsReminder = "SendResults";
    }

    /******************** The Actor ********************/

    [StatePersistence(StatePersistence.Persisted)]
    internal class MasterActor: Actor, IMasterActor, IRemindable {
        private HttpClient clientFEserver;

        public MasterActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId) {
        }

        protected override Task OnActivateAsync() {
            ActorEventSource.Current.ActorMessage(this, "MasterActor {0} activated.", this.Id);

            var configPackage = ActorService.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            clientFEserver = new HttpClient {
                BaseAddress = new System.Uri(
                   configPackage.Settings.Sections["WebSiteInfo"].Parameters["WebSiteURI"].Value)
            };

            return StateManager.TryAddStateAsync<BESearchRequest>(StateNames.TheSearchRequest, null);
        }

        /******************** State Management Methods ********************/
    
        private Task<BESearchRequest> GetTheSearchRequest() {
            return StateManager.GetStateAsync<BESearchRequest>(StateNames.TheSearchRequest);
        }

        private async Task SaveTheSearchRequest(BESearchRequest theSearchRequest) {
            await StateManager.SetStateAsync(StateNames.TheSearchRequest, theSearchRequest);
            await SaveStateAsync();
        }        

        private async Task SetTheSerchRequestStatus(Status newStatus) {
            BESearchRequest newSearchRequest = await GetTheSearchRequest();
            newSearchRequest.TheStatus = newStatus;
            await SaveTheSearchRequest(newSearchRequest);            
        }

        /******************** Actor Interface Methods ********************/

        public async Task FulfillSearchRequestAsync(BESearchRequest searchRequest) {
            // Initialize the SearchRequest in the state manager if this MasterActor is called for the first time      
            if(await GetTheSearchRequest() == null) {
                await SaveTheSearchRequest(searchRequest);
            }

            // Set the Reminder for the method that implements the core logic of the Actor (mainFulfillSearchRequestAsync)
            try {
                await RegisterReminderAsync(ReminderNames.FulfillSReqReminder, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
            } catch(Exception) {
                throw;
            }

        }

        public async Task UpdateSerchRequestStatus(Status newStatus) {
            await SetTheSerchRequestStatus(newStatus);
            try {
                await RegisterReminderAsync(ReminderNames.FulfillSReqReminder, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
            } catch(Exception) {
                throw;
            }
        }

        /******************** Remider Management Method ********************/

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan duelTIme, TimeSpan period){
            switch(reminderName) {

                case ReminderNames.FulfillSReqReminder:
                    try {
                        await UnregisterReminderAsync(this.GetReminder(ReminderNames.FulfillSReqReminder));
                        await mainFulfillSearchRequestAsync();                       
                    } catch {

                    }                    
                    break;

                case ReminderNames.SendResultsReminder:
                    await sendResults();
                    break;

                default:
                    // This point should never be reached. 
                    throw new InvalidOperationException("Unknown Reminder: " + reminderName);
                    break;
            }
        }

        /******************** Actor Logic Implementation Methods ********************/

        // Fullfills the SearchRequest. Invoked by a Reminder (FulfillSReqReminder)
        private async Task mainFulfillSearchRequestAsync() {
            BESearchRequest theSearchRequest = await GetTheSearchRequest();

            Status theStatus = theSearchRequest.TheStatus;
            switch(theStatus) {
                case Status.New:
                    //Call the Miner
                    IMinerActor theMiner = ActorProxy.Create<IMinerActor>(new ActorId(theSearchRequest.ID));
                    try {
                        await theMiner.StartMiningAsync(theSearchRequest);
                        await SetTheSerchRequestStatus(Status.Mining);
                    } catch {
                        throw;
                    }
                    break;

                case Status.Mining:

                    break;

                case Status.Mining_Done:
                    // Set a Reminder to Send the Results to the Website
                    try {
                        await RegisterReminderAsync(ReminderNames.SendResultsReminder, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
                    } catch(Exception) {
                        throw;
                    }
                    break;               
            }
        }

        // Send the Results to the Website. Invoked by a Reminder (SendResultsReminder)
        private async Task sendResults() {
            var theResults = (BaseSearchRequest)(await GetTheSearchRequest()); // Temporary Type
            var response = await clientFEserver.PostAsJsonAsync("api/Results", theResults);

            // Upon successful transmission, delete the reminder. Else the sendResults method will be invoked again
            if(response.IsSuccessStatusCode) {
                await UnregisterReminderAsync(this.GetReminder(ReminderNames.SendResultsReminder));
            }

        }


    }
}
