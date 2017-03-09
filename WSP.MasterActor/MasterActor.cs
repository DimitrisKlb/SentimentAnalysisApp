using System.Threading;
using System.Threading.Tasks;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;

using WSP.MasterActor.Interfaces;
using WSP.MinerActor.Interfaces;

namespace WSP.MasterActor {

    [StatePersistence(StatePersistence.Persisted)]
    internal class MasterActor: Actor, IMasterActor {

        public MasterActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId) {
        }

        //This method is called whenever an actor is activated.
        // An actor is activated the first time any of its methods are invoked.
        protected override Task OnActivateAsync() {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            return this.StateManager.TryAddStateAsync("count", 0);
        }

        public async Task<int> FulfillSearchRequestAsync(int id) {
            IMinerActor theMiner = ActorProxy.Create<IMinerActor>(new ActorId(id));
            Thread.Sleep(15 * 1000);
            int x = await theMiner.MineAsync("try", 1);
            Thread.Sleep(15 * 1000);

            return id + x;
        }
    }
}
