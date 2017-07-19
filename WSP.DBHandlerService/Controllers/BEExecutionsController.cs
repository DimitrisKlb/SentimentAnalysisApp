using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

using WSP.Models;

namespace WSP.DBHandlerService {
    public class BEExecutionsController {

        public async Task<BEExecution> GetBEExecution(int id) {
            BEExecution bEExecution = null;
            using(var db = new BEMainDBContext()) {
                bEExecution = await db.BEExecutions.FindAsync( id );
            }
            return bEExecution;
        }

        public IEnumerable<BEExecution> GetBEExecutions(int searchRequestID) {
            IEnumerable<BEExecution> executionsOfSReq = null;
            using(var db = new BEMainDBContext()) {
                executionsOfSReq = db.BEExecutions
                                        .Where( exec => exec.SearchRequestID == searchRequestID )
                                        .OrderBy( exec => exec.StartedOn )
                                        .ToList();
            }
            return executionsOfSReq;
        }

        public BEExecution GetBEExecutionLatest(int searchRequestID) {
            return GetBEExecutions( searchRequestID ).Last();
        }

        public async Task<BEExecution> PostBEExecution(BEExecution newBEExecution) {
            BEExecution createdBEExecution = null;
            using(var db = new BEMainDBContext()) {
                createdBEExecution = db.BEExecutions.Add( newBEExecution );
                await db.SaveChangesAsync();
            }
            return createdBEExecution;
        }

        public async Task UpdateBEExecution(BEExecution updatedBEExecution) {
            using(var db = new BEMainDBContext()) {
                db.BEExecutions.AddOrUpdate( updatedBEExecution );
                await db.SaveChangesAsync();
            }
        }

    }
}