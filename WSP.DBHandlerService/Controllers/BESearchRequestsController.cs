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

        public async Task<BESearchRequest> PostBESearchRequest(BESearchRequest newBESearchRequest) {
            var createdBESearchRequest = db.BESearchRequests.Add( newBESearchRequest );
            await db.SaveChangesAsync();
            return createdBESearchRequest;
        }

        public async Task PutBESearchRequest(BESearchRequest updatedBESearchRequest) {
            db.BESearchRequests.AddOrUpdate( updatedBESearchRequest );
            await db.SaveChangesAsync();
        }

    }
}