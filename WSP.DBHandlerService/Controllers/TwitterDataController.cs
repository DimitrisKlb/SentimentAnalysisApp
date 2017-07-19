using System.Data.Entity;
using System.Threading.Tasks;
using System.Linq;

using SentimentAnalysisApp.SharedModels;
using WSP.Models;

namespace WSP.DBHandlerService {
    public class TwitterDataController {

        public async Task PostTwitterData(TwitterData newTwitterData) {
            using(var db = new BEMainDBContext()) {
                db.TwitterData.Add( newTwitterData );
                await db.SaveChangesAsync();
            }
        }

        public async Task<TwitterData> GetTwitterData(int executionID) {
            TwitterData twitterData = null;
            using(var db = new BEMainDBContext()) {
                twitterData = db.TwitterData
                                .Where( td => td.TheExecutionID == executionID )
                                .ToList()
                                .FirstOrDefault();
            }
            return twitterData;
        }

        public async Task<TwitterData> GetLatestTwitterData(int searchRequestID) {
            TwitterData lastTwitterData = null;

            try {
                using(var db = new BEMainDBContext()) {
                    var execsBySreq = db.BEExecutions                                        
                                        .Where( exec => (exec.SearchRequestID == searchRequestID) )
                                        .Include( exec => exec.TheMinerData )
                                        .Where(exec => exec.TheMinerData.Count != 0)
                                        .OrderBy( exec => exec.StartedOn )
                                        .ToList();

                    var lastMinerData = execsBySreq.Select( exec => exec.TheMinerData
                                                                    .Where( md => md.TheSource == SourceOption.Twitter )
                                                                    .Last() )
                                                .Where(md => md != null)
                                                .ToList()
                                                .Last();                   

                    lastTwitterData = db.TwitterData
                                         .Where( td => td.TheExecutionID == lastMinerData.TheExecutionID )
                                         .ToList()
                                         .FirstOrDefault();
                }
            } catch {
                lastTwitterData = null;
            }
            return lastTwitterData;
        }



    }
}