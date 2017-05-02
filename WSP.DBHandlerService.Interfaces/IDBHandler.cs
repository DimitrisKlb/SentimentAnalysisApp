using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.ServiceFabric.Services.Remoting;
using WSP.Models;

namespace WSP.DBHandlerService.Interfaces {
    public interface IDBHandlerService: IService {
        Task<BESearchRequest> StoreBESearchRequest(BESearchRequest newBESearchRequest);
        Task StoreMinedTexts(IEnumerable<BEMinedText> newBEMinedTexts);
    }
}
