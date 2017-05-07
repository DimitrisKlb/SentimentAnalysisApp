using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

using WSP.DBHandlerService.Interfaces;
using WSP.Models;

namespace WSP.DBHandlerService {
    /******************** Helper Classes for String definitions ********************/

    static internal class StateNames {
        public const string TheMinedTextsQueue = "theMinedTextsQueue";
    }

    /******************** The Service ********************/

    internal sealed class DBHandlerService: StatefulService, IDBHandlerService {
        BESearchRequestsController TheSReqController = new BESearchRequestsController();
        BEMinedTextsController TheMinedTextsController = new BEMinedTextsController();
        private static int clusterSizeMax = 100;
        

        public DBHandlerService(StatefulServiceContext context)
            : base( context ) { }


        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners() {
            return new[] {
                new ServiceReplicaListener( context => this.CreateServiceRemotingListener( context ) )
            };
        }

        /******************** State Management Methods ********************/

        private Task<IReliableQueue<IEnumerable<BEMinedText>>> GetTheTextsQueue() {
            return StateManager.GetOrAddAsync<IReliableQueue<IEnumerable<BEMinedText>>>( StateNames.TheMinedTextsQueue );
        }

        /******************** Service Interface Methods ********************/

        public async Task<BESearchRequest> StoreBESearchRequest(BESearchRequest newBESearchRequest) {
            return await TheSReqController.PostBESearchRequest( newBESearchRequest );
        }

        public async Task StoreMinedTexts(IEnumerable<BEMinedText> newBEMinedTexts) {
            var theQueue = await GetTheTextsQueue();

            using(var tx = StateManager.CreateTransaction()) {
                await theQueue.EnqueueAsync( tx, newBEMinedTexts );

                await tx.CommitAsync();
            }
        }

        /******************** Service Main Run Method ********************/

        protected override async Task RunAsync(CancellationToken cancellationToken) {
            var theQueue = await GetTheTextsQueue();
            int clusterSize = 0;
            bool addMoreTexts;

            while(true) {
                cancellationToken.ThrowIfCancellationRequested();

                using(var tx = StateManager.CreateTransaction()) {
                    List<BEMinedText> allTexts;

                    var result = await theQueue.TryDequeueAsync( tx );
                    if(result.HasValue) {
                        allTexts = result.Value.ToList();
                        clusterSize = allTexts.Count();
                        addMoreTexts = true;
                        
                        while(addMoreTexts) {
                            result = await theQueue.TryPeekAsync( tx );
                            if(result.HasValue) {
                                var texts = result.Value;
                                if(clusterSize + texts.Count() <= clusterSizeMax) {
                                    await theQueue.TryDequeueAsync( tx );
                                    clusterSize += texts.Count();
                                    allTexts.AddRange( texts );
                                } else {
                                    addMoreTexts = false;
                                }
                            } else {
                                addMoreTexts = false;
                            }
                        }
                        
                        await TheMinedTextsController.PostBEMinedTexts(allTexts);
                    }   

                    await tx.CommitAsync();
                }

                await Task.Delay( TimeSpan.FromSeconds( 60 ), cancellationToken );
            }
        }
    }
}
