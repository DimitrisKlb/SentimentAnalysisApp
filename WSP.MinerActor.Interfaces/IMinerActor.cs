using System.Threading.Tasks;

using Microsoft.ServiceFabric.Actors;

using WSP.Models;

namespace WSP.MinerActor.Interfaces {

    public interface IMinerActor: IActor {

        Task<int> MineAsync(BESearchRequest searchRequest);

    }
}
