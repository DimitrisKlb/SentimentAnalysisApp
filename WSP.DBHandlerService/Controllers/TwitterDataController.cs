using System;
using System.Threading.Tasks;

using WSP.Models;

namespace WSP.DBHandlerService {
    public class TwitterDataController {
        private BEMainDBContext db = new BEMainDBContext();

        public async Task PostTwitterData(TwitterData newTwitterData) {
            db.TwitterData.Add( newTwitterData );
            await db.SaveChangesAsync();
        }

    }
}