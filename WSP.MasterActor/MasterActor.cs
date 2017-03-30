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
//using WSP.MinerActor.Interfaces;

namespace WSP.MasterActor {

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

            return this.StateManager.TryAddStateAsync<BESearchRequest>(StateNames.TheSearchRequest, null);
        }

        private Task SetTheSearchRequest(BESearchRequest theSearchRequest) {
            return this.StateManager.SetStateAsync(StateNames.TheSearchRequest, theSearchRequest);
        }
        private Task<BESearchRequest> GetTheSearchRequest() {
            return this.StateManager.GetStateAsync<BESearchRequest>(StateNames.TheSearchRequest);
        }

        public async Task FulfillSearchRequestAsync(BESearchRequest searchRequest) {
            await SetTheSearchRequest(searchRequest);

            try {
                await RegisterReminderAsync(
                    ReminderNames.FulfillSReqReminder,
                    null,
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(10));
            } catch(Exception) {
                throw;
            }

        }

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan duelTIme, TimeSpan period) {
            switch(reminderName) {

                case ReminderNames.FulfillSReqReminder:
                    await mainFulfillSearchRequestAsync();
                    await UnregisterReminderAsync(this.GetReminder(ReminderNames.FulfillSReqReminder));
                    break;

                case ReminderNames.SendResultsReminder:
                    await sendResults();
                    break;

                default:
                    // This point should never be reached. 
                    throw new InvalidOperationException("Unknown Reminder: " + reminderName);
            }
        }

        // Fullfills the SearchRequest. Invoked by a Reminder (FulfillSReqReminder)
        private async Task mainFulfillSearchRequestAsync() {
            //IMinerActor theMiner = ActorProxy.Create<IMinerActor>(new ActorId(id));
            //Thread.Sleep(15 * 1000);
            //int x = await theMiner.MineAsync("try", 1);
            //Thread.Sleep(15 * 1000);

            // Set a Reminder to Send the Results to the Website
            try {
                await RegisterReminderAsync(
                    ReminderNames.SendResultsReminder,
                    null,
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(10));
            } catch(Exception) {
                throw;
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
