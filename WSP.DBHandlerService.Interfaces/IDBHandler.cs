using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.ServiceFabric.Services.Remoting;

using SentimentAnalysisApp.SharedModels;
using WSP.Models;

namespace WSP.DBHandlerService.Interfaces {
    public interface IDBHandlerService: IService {

        Task<BESearchRequest> StoreOrUpdateSearchRequest(BESearchRequest newBESearchRequest);

        // Called to signify that a SearchRequest has been fulfilled. Updates its Status to Fulfilled,
        // updates the corresponding Execution's necessary fields and stores the calculated Results
        Task UpdateSearchRequestFulfilled(int searchRequestID, int executionID, Results theResults);


        Task<BEExecution> StoreExecution(BEExecution newBEExecution);


        Task StoreMinedTexts(IEnumerable<BEMinedText> newBEMinedTexts);
        Task<IEnumerable<BEMinedText>> GetMinedTexts(int executionID, SourceOption source, TextStatus status, int windowSize);
        Task<int> GetMinedTextsCount(int executionID, SourceOption source, TextStatus status);
        Task UpdateMinedTexts(IEnumerable<BEMinedText> updatedTexts);

        Task StoreMinerData(MinerData newMinerData);
        Task<IEnumerable<MinerData>> GetMinerDatum(int executionID);
        Task<MinerData> GetMinerData(int executionID, SourceOption source);
        Task UpdateMinerData(MinerData updatedMinerData);

        Task StoreTwitterData(TwitterData newTwitterData);
        Task<TwitterData> GetTwitterData(int executionID);
        Task<TwitterData> GetLatestTwitterData(int searchRequestID);

    }
}
