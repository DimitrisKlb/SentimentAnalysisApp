using System.Threading.Tasks;

using Microsoft.ServiceFabric.Actors;

using SentimentAnalysisApp.SharedModels;
using WSP.Models;

namespace WSP.MasterActor.Interfaces {

    public interface IMasterActor: IActor {
        Task FulfillSearchRequestAsync(BESearchRequest searchRequest);
        Task UpdateSearchRequestStatus(Status newStatus, SourceOption finishedSourceID);
    }
}
