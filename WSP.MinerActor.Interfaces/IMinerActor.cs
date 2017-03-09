using System.Threading.Tasks;

using Microsoft.ServiceFabric.Actors;

namespace WSP.MinerActor.Interfaces {

    public interface IMinerActor: IActor {

        Task<int> MineAsync(string searchKeyword, int searchRequestID);

    }
}
