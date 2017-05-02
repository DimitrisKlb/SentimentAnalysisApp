using System.Threading.Tasks;

using WSP.Models;

namespace WSP.DBHandlerService {
    public class BESearchRequestsController {
        private BEMainDBContext db = new BEMainDBContext();

        public async Task<BESearchRequest> PostBESearchRequest(BESearchRequest newBESearchRequest) { 
            var createdBESearchRequest = db.BESearchRequests.Add( newBESearchRequest );
            await db.SaveChangesAsync();
            return createdBESearchRequest;
        }

    }
}