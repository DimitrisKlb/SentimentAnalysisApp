using System.Threading.Tasks;
using System.Fabric;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

using WSP.Models;

namespace WSP.MyActors {

    [StatePersistence( StatePersistence.Persisted )]
    public abstract class BaseActor : Actor{

        /************* Helper Classes for String definitions *************/

        protected abstract class StateNames {
            public const string TheSearchRequest = "theSearchRequest";
        }

        /******************** Fields and Core Methods ********************/

        protected ConfigurationPackage configSettings;

        public BaseActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId) {
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
        

    }
}
