using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

using WSP.Models;

namespace WSP.DBHandlerService {
    public class BEExecutionsController {
        private BEMainDBContext db = new BEMainDBContext();

        public async Task<BEExecution> GetBEExecution(int id) {
            BEExecution bEExecution = await db.BEExecutions.FindAsync( id );
            return bEExecution;
        }

        public IEnumerable<BEExecution> GetBEExecutions(int searchRequestID) {
            var executionsOfSReq = db.BEExecutions
                                        .Where( exec => exec.SearchRequestID == searchRequestID )
                                        .Include( exec => exec.TheTwitterData )
                                        .OrderBy( exec => exec.StartedOn )
                                        .ToList();
            return executionsOfSReq;
        }

        public BEExecution GetBEExecutionLatest(int searchRequestID) {
            return GetBEExecutions( searchRequestID ).Last();
        }

        public async Task<BEExecution> PostBEExecution(BEExecution newBEExecution) {
            var createdBEExecution = db.BEExecutions.Add( newBEExecution );
            await db.SaveChangesAsync();
            return createdBEExecution;
        }

        public async Task UpdateBEExecution(BEExecution updatedBEExecution) {
            db.BEExecutions.AddOrUpdate( updatedBEExecution );
            await db.SaveChangesAsync();
        }

    }
}