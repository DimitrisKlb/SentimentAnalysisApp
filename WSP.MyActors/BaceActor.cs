using System;
using System.Threading.Tasks;
using System.Fabric;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

using WSP.Models;

namespace WSP.MyActors {

    [StatePersistence( StatePersistence.Persisted )]
    public abstract class BaseActor: Actor {

        /************* Helper Classes for String definitions *************/

        protected abstract class StateNames {
            public const string TheSearchRequest = "theSearchRequest";
        }

        /******************** Fields and Core Methods ********************/

        protected ConfigurationPackage configSettings;

        public BaseActor(ActorService actorService, ActorId actorId)
            : base( actorService, actorId ) {
        }

        protected override Task OnActivateAsync() {
            configSettings = ActorService.Context.CodePackageActivationContext.GetConfigurationPackageObject( "Config" );

            return StateManager.TryAddStateAsync<BESearchRequest>( StateNames.TheSearchRequest, null );
        }

        /******************** State Management Methods ********************/

        protected Task<BESearchRequest> GetTheSearchRequest() {
            return StateManager.GetStateAsync<BESearchRequest>( StateNames.TheSearchRequest );
        }

        protected async Task SaveTheSearchRequest(BESearchRequest theSearchRequest) {
            await StateManager.SetStateAsync( StateNames.TheSearchRequest, theSearchRequest );
            await SaveStateAsync();
        }

        // Method to Register Reminders with default Time parameters (duelTime and period)
        protected Task<IActorReminder> RegisterReminderAsync(string reminderName) {
            int defaultDuelTime = 10; // In Seconds
            int defaultPeriod = 10;
            return RegisterReminderAsync( reminderName, null, TimeSpan.FromSeconds( defaultDuelTime ), TimeSpan.FromSeconds( defaultPeriod ) );
        }

    }
}
