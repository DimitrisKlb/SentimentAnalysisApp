using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;

using SentimentAnalysisApp.SharedModels;
using WSP.Models;

namespace WSP.DBHandlerService {
    public class ResultsController {        

        public async Task PostResults(Results newResults) {
            using(var db = new BEMainDBContext()) {
                db.Results.Add( newResults );
                await db.SaveChangesAsync();
            }
        }      
        

    }
}