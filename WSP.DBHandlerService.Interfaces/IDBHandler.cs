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

        Task StoreTwitterData(TwitterData newTwitterData);
        Task<TwitterData> GetLatestTwitterData(int searchRequestID);

    }
}
