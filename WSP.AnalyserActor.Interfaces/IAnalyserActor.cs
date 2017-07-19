using System.Threading.Tasks;

using Microsoft.ServiceFabric.Actors;

using WSP.Models;

namespace WSP.AnalyserActor.Interfaces {

    public interface IAnalyserActor: IActor {
        Task StartAnalysingAsync(BESearchRequest searchRequest);
    }

    public interface ITwitterAnalyserActor: IAnalyserActor {
    }

}
