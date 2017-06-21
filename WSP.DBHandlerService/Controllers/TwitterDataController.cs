using System.Threading.Tasks;
using System.Linq;

using WSP.Models;

namespace WSP.DBHandlerService {
    public class TwitterDataController {
        private BEMainDBContext db = new BEMainDBContext();

        public async Task PostTwitterData(TwitterData newTwitterData) {
            db.TwitterData.Add( newTwitterData );
            await db.SaveChangesAsync();
        }

        public async Task<TwitterData> GetLatestTwitterData(int searchRequestID) {
            BEExecutionsController execsControl = new BEExecutionsController();
            var execsBySreq = execsControl.GetBEExecutions( searchRequestID );
            TwitterData lastTwitterData;
            try {
                lastTwitterData = execsBySreq.Select( ex => ex.TheTwitterData )
                                                        .Where( t => t != null )
                                                        .Last();
            } catch {
                lastTwitterData = null;
            }
            return lastTwitterData;
        }

    }
}