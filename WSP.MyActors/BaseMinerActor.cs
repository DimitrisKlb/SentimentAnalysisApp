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
            public const string MineBeginReminder = "MineBegin";
            public const string MineReminder = "Mine";
            public const string MineCompleteReminder = "MineComplete";
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
            await SaveTheSearchRequest( searchRequest );

            // Set the Reminder for the method to begin Mining
            await RegisterReminderAsync( ReminderNames.MineBeginReminder);
        }

        /******************** Remider Management Method ********************/

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan duelTIme, TimeSpan period) {
            // Unregister the Received Reminder
            await UnregisterReminderAsync( GetReminder( reminderName ) );

            switch(reminderName) {
                case ReminderNames.MineBeginReminder:
                    try {
                        await onMineBeginAsync();
                        // Set the Reminder for the method that implements the core logic of the Actor (mainMineAsync)
                        await RegisterReminderAsync( ReminderNames.MineReminder);
                    } catch {
                        await RegisterReminderAsync( ReminderNames.MineBeginReminder);
                    }
                    break;

                case ReminderNames.MineReminder:
                    try {
                        bool g = await mainMineAsync();
                        await onMineEndAsync();
                    } catch {
                        await RegisterReminderAsync( ReminderNames.MineReminder);
                    }
                    break;

                case ReminderNames.MineCompleteReminder:
                    try {
                        // Notify the MasterActor that the job was done
                        IMasterActor theMasterActor = ActorProxy.Create<IMasterActor>( this.Id );
                        await theMasterActor.UpdateSearchRequestStatus( Status.Mining_Done, MinerSourceID );
                        
                        await onMineCompleteAsync();
                    } catch {
                        await RegisterReminderAsync( ReminderNames.MineCompleteReminder );
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

        // Called before the Miner starts its job (mainMineAsync)
        protected abstract Task onMineBeginAsync();

        // Define the strategy that will be followed when mainMineAsync() ended 
        // (Both in case of problematic exit or correct return)
        protected abstract Task onMineEndAsync();

        // Called after the Miner sucesfully finished its job (mainMineAsync)
        protected abstract Task onMineCompleteAsync();
        
    }
}
