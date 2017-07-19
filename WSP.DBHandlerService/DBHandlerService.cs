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
using SentimentAnalysisApp.SharedModels;
using WSP.Models;

namespace WSP.DBHandlerService {
    /******************** Helper Classes for String definitions ********************/

    static internal class StateNames {
        public const string TheMinedTextsQueue = "theMinedTextsQueue";
    }

    /******************** The Service ********************/

    internal sealed class DBHandlerService: StatefulService, IDBHandlerService {
        BESearchRequestsController TheSReqsContr = new BESearchRequestsController();
        BEExecutionsController TheExecsContr = new BEExecutionsController();
        BEMinedTextsController TheMinedTextsContr = new BEMinedTextsController();
        MinerDataController TheMinerDataContr = new MinerDataController();
        TwitterDataController TheTwitterDataContr = new TwitterDataController();
        ResultsController TheResultsContr = new ResultsController();

        private const int clusterSizeMax = 100;
        private const int waitTimeJobDone = 5;
        private const int waitTimeQueueEmpty = 60;

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

        /*---------- SearchRequest Management ----------*/
        public async Task<BESearchRequest> StoreOrUpdateSearchRequest(BESearchRequest newBESearchRequest) {
            // Check if this SearchRequest has been received again
            BESearchRequest theSearchRequest = TheSReqsContr.GetBESearchRequestByReceived( newBESearchRequest.TheReceivedID );

            if(theSearchRequest == null) { // If not, create a new SearchRequest
                theSearchRequest = await TheSReqsContr.PostBESearchRequest( newBESearchRequest );

            } else { // If it has, update its Status to new, since it will be executed again
                theSearchRequest.TheStatus = Status.New;
                await TheSReqsContr.UpdateBESearchRequest( theSearchRequest );
            }
            return theSearchRequest;
        }

        public async Task UpdateSearchRequestFulfilled(int searchRequestID, int executionID, Results theResults) {
            BESearchRequest theSReq = await TheSReqsContr.GetBESearchRequest( searchRequestID );
            theSReq.TheStatus = Status.Fulfilled;
            await TheSReqsContr.UpdateBESearchRequest( theSReq );

            BEExecution theExec = await TheExecsContr.GetBEExecution( executionID );
            theExec.FinishedOn = DateTime.Now;
            await TheExecsContr.UpdateBEExecution( theExec );

            theResults.ID = executionID;
            await TheResultsContr.PostResults( theResults );
        }

        /*---------- Execution Management ----------*/
        public async Task<BEExecution> StoreExecution(BEExecution newBEExecution) {
            return await TheExecsContr.PostBEExecution( newBEExecution );
        }

        /*---------- MinedText Management ----------*/
        public async Task StoreMinedTexts(IEnumerable<BEMinedText> newBEMinedTexts) {
            var theQueue = await GetTheTextsQueue();
            using(var tx = StateManager.CreateTransaction()) {
                await theQueue.EnqueueAsync( tx, newBEMinedTexts );
                await tx.CommitAsync();
            }
        }

        public async Task<IEnumerable<BEMinedText>> GetMinedTexts(int executionID, SourceOption source, TextStatus status, int windowSize) {
            var texts = await TheMinedTextsContr.GetBEMinedTexts( executionID, source, status, windowSize );
            if(texts != null && texts.Count() != 0) {
                var textIDs = texts.Select( t => t.ID );
                await TheMinedTextsContr.UpdateBEMinedTextsStatus( textIDs, TextStatus.BeingProcessed );
            }
            return texts;
        }

        public async Task<int> GetMinedTextsCount(int executionID, SourceOption source, TextStatus status) {
            return await TheMinedTextsContr.GetBEMinedTextsCount( executionID, source, status );
        }

        public async Task UpdateMinedTexts(IEnumerable<BEMinedText> updatedTexts) {
            await TheMinedTextsContr.UpdateBEMinedTexts( updatedTexts );
        }

        /*---------- MinerData Management ----------*/
        public async Task StoreMinerData(MinerData newMinerData) {
            await TheMinerDataContr.PostMinerData( newMinerData );
        }

        public async Task<MinerData> GetMinerData(int executionID, SourceOption source) {
            MinerData minerData = await TheMinerDataContr.GetMinerData( executionID, source );
            // Return only the needed fields as an instance of the base class
            return new MinerData() {
                TheSource = minerData.TheSource,
                TheTextsNum = minerData.TheTextsNum
            };
        }

        /*---------- TwitterData Management ----------*/
        public async Task StoreTwitterData(TwitterData newTwitterData) {
            await TheTwitterDataContr.PostTwitterData( newTwitterData );
        }

        public async Task<TwitterData> GetTwitterData(int executionID) {
            return await TheTwitterDataContr.GetTwitterData( executionID );
        }

        public async Task<TwitterData> GetLatestTwitterData(int searchRequestID) {
            return await TheTwitterDataContr.GetLatestTwitterData( searchRequestID );
        }

        /******************** Service Main Run Method ********************/

        protected override async Task RunAsync(CancellationToken cancellationToken) {
            var theQueue = await GetTheTextsQueue();
            int waitTime;
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
                        waitTime = waitTimeJobDone;
                        await TheMinedTextsContr.PostBEMinedTexts( allTexts );

                    } else {
                        waitTime = waitTimeQueueEmpty;
                    }

                    await tx.CommitAsync();
                }

                await Task.Delay( TimeSpan.FromSeconds( waitTime ), cancellationToken );
            }
        }




    }
}
