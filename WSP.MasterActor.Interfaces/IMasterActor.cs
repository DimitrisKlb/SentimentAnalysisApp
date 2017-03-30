using System.Threading.Tasks;

using Microsoft.ServiceFabric.Actors;

using WSP.Models;

namespace WSP.MasterActor.Interfaces {

    public interface IMasterActor: IActor {

        Task FulfillSearchRequestAsync(BESearchRequest searchRequest);
    }
}
