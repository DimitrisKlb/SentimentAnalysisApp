using System;
using System.Threading.Tasks;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

using SentimentAnalysisApp.SharedModels;
using WSP.Models;
using WSP.MasterActor.Interfaces;
using WSP.MinerActor.Interfaces;
using WSP.DBHandlerService.Interfaces;

namespace WSP.MyActors {

    [StatePersistence( StatePersistence.Persisted )]
    public abstract class BaseMinerActor: BaseActor, IMinerActor, IRemindable {

        /************* Helper Classes for String definitions *************/

        protected abstract class ReminderNames {
            public const string MineReminder = "Mine";
        }

        /******************** Fields and Core Methods ********************/

        protected abstract SourceOption MinerSourceID { get; }

        protected IDBHandlerService dbHandlerService;
        

        public BaseMinerActor(ActorService actorService, ActorId actorId)
            : base( actorService, actorId ) {

        }

        protected override async Task OnActivateAsync() {
            await base.OnActivateAsync();

            dbHandlerService = ServiceProxy.Create<IDBHandlerService>(
                new Uri( configSettings.Settings.Sections["ApplicationServicesNames"].Parameters["DBHandlerName"].Value ),
                new ServicePartitionKey( 1 ) );
        }

        /******************** Actor Interface Methods ********************/

        public virtual async Task StartMiningAsync(BESearchRequest searchRequest) {
            // Initialize the SearchRequest in the state manager if this MinerActor is called for the first time      
            if(await GetTheSearchRequest() == null) {
                await SaveTheSearchRequest( searchRequest );
            }

            // Set the Reminder for the method that implements the core logic of the Actor (mainMineAsync)
            try {
                await RegisterReminderAsync( ReminderNames.MineReminder, null, TimeSpan.FromSeconds( 10 ), TimeSpan.FromSeconds( 10 ) );
            } catch(Exception) {
                throw;
            }
        }

        /******************** Remider Management Method ********************/

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan duelTIme, TimeSpan period) {
            switch(reminderName) {

                case ReminderNames.MineReminder:
                    await UnregisterReminderAsync( this.GetReminder( ReminderNames.MineReminder ) );
                    bool done = await mainMineAsync();

                    if(done == true) {
                        IMasterActor theMasterActor = ActorProxy.Create<IMasterActor>( this.Id );
                        await theMasterActor.UpdateSerchRequestStatus( Status.Mining_Done, MinerSourceID );
                    } else {
                        await OnMineFailAsync();
                    }
                    break;

                default:
                    // This point should never be reached. 
                    throw new InvalidOperationException( "Unknown Reminder: " + reminderName );                   
            }
        }

        /******************** Actor Logic Implementation Methods ********************/

        // Basic method that implements the Miner's logic. Each specific Miner subclass will override this method to mine texts
        // from each individual source. Invoked by a Reminder (MineReminder). 
        protected abstract Task<bool> mainMineAsync();

        // Define the strategy that will be followed if mainMineAsync() method "failed" (returned false).
        protected abstract Task OnMineFailAsync();
    }
}
