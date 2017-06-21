using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

using WSP.Models;

namespace WSP.DBHandlerService {
    public class BESearchRequestsController {
        private BEMainDBContext db = new BEMainDBContext();

        public async Task<BESearchRequest> GetBESearchRequest(int id) {
            BESearchRequest bESearchRequest = await db.BESearchRequests.FindAsync( id );
            return bESearchRequest;
        }

        public BESearchRequest GetBESearchRequestByReceived(int receivedID) {
            BESearchRequest theBESReq = null;
            theBESReq = db.BESearchRequests
                            .Where( sReq => sReq.TheReceivedID == receivedID )
                            .ToList()
                            .FirstOrDefault();
            return theBESReq;
        }

        public async Task<BESearchRequest> PostBESearchRequest(BESearchRequest newBESearchRequest) {
            var createdBESearchRequest = db.BESearchRequests.Add( newBESearchRequest );
            await db.SaveChangesAsync();
            return createdBESearchRequest;
        }

        public async Task UpdateBESearchRequest(BESearchRequest updatedBESearchRequest) {
            db.BESearchRequests.AddOrUpdate( updatedBESearchRequest );
            await db.SaveChangesAsync();
        }        
                


    }
}