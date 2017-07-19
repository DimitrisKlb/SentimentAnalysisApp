using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

using WSP.Models;

namespace WSP.DBHandlerService {
    public class BESearchRequestsController {

        public async Task<BESearchRequest> GetBESearchRequest(int id) {
            BESearchRequest bESearchRequest = null;
            using(var db = new BEMainDBContext()) {
                bESearchRequest = await db.BESearchRequests.FindAsync( id );
            }
            return bESearchRequest;
        }

        public BESearchRequest GetBESearchRequestByReceived(int receivedID) {
            BESearchRequest theBESReq = null;
            using(var db = new BEMainDBContext()) {
                theBESReq = db.BESearchRequests
                            .Where( sReq => sReq.TheReceivedID == receivedID )
                            .ToList()
                            .FirstOrDefault();
            }
            return theBESReq;
        }

        public async Task<BESearchRequest> PostBESearchRequest(BESearchRequest newBESearchRequest) {
            BESearchRequest createdBESearchRequest;
            using(var db = new BEMainDBContext()) {
                createdBESearchRequest = db.BESearchRequests.Add( newBESearchRequest );
                await db.SaveChangesAsync();
            }
            return createdBESearchRequest;
        }

        public async Task UpdateBESearchRequest(BESearchRequest updatedBESearchRequest) {
            using(var db = new BEMainDBContext()) {
                db.BESearchRequests.AddOrUpdate( updatedBESearchRequest );
                await db.SaveChangesAsync();
            }
        }



    }
}