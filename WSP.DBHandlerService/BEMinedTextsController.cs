using System.Collections.Generic;
using System.Threading.Tasks;

using WSP.Models;

namespace WSP.DBHandlerService {
    public class BEMinedTextsController {
        private BEMainDBContext db = new BEMainDBContext();

        public async Task PostBEMinedTexts(IEnumerable<BEMinedText> newBEMinedTexts) {
            db.BEMinedTexts.AddRange( newBEMinedTexts );
            await db.SaveChangesAsync();
        }

    }
}