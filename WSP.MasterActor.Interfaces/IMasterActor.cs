using System.Threading.Tasks;

using Microsoft.ServiceFabric.Actors;

namespace WSP.MasterActor.Interfaces {

    public interface IMasterActor: IActor {

        Task<int> FulfillSearchRequestAsync(int id);
    }
}
