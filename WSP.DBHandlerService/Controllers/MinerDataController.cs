using System.Threading.Tasks;
using System.Linq;

using SentimentAnalysisApp.SharedModels;
using WSP.Models;
using System;

namespace WSP.DBHandlerService {
    public class MinerDataController {

        public async Task PostMinerData(MinerData newMinerData) {
            using(var db = new BEMainDBContext()) {
                db.MinerData.Add( newMinerData );
                await db.SaveChangesAsync();
            }
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

    }
}