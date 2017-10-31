using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using System.Linq;

using SentimentAnalysisApp.SharedModels;
using WSP.Models;

namespace WSP.DBHandlerService {
    public class MinerDataController {

        public async Task PostMinerData(MinerData newMinerData) {
            using(var db = new BEMainDBContext()) {
                db.MinerData.Add( newMinerData );
                await db.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<MinerData>> GetMinerDatum(int executionID) {
            IEnumerable<MinerData> minerDatum = null;
            using(var db = new BEMainDBContext()) {
                minerDatum = db.MinerData
                                .Where( md => md.TheExecutionID == executionID )
                                .ToList();
            }
            return minerDatum;
        }


        public async Task<MinerData> GetMinerData(int executionID, SourceOption source) {
            MinerData minerData = null;
            using(var db = new BEMainDBContext()) {
                minerData = db.MinerData
                                .Where( md => (md.TheExecutionID == executionID) && (md.TheSource == source) )
                                .ToList()
                                .FirstOrDefault();
            }
            return minerData;
        }

        public async Task UpdateMinerData(MinerData updatedMinerData) {
            using(var db = new BEMainDBContext()) {
                db.MinerData.AddOrUpdate( updatedMinerData );
                await db.SaveChangesAsync();
            }
        }

    }
}