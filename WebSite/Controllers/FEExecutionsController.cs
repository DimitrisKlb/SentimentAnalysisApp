using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

using WebSite.Models;

namespace WebSite.Controllers {

    public class FEExecutionsController: ApiController {
        private FEMainDBContext db = new FEMainDBContext();

        [NonAction]
        public IEnumerable<FEExecution> GetFEExecutions(int searchRequestID) {
            var executionsOfSReq = db.FEExecutions
                                        .Where( exec => exec.SearchRequestID == searchRequestID )
                                        .Include( exec => exec.TheResults )
                                        .OrderBy(exec=> exec.StartedOn)
                                        .ToList();                                        
            return executionsOfSReq;
        }

        [NonAction]
        [ResponseType( typeof( FESearchRequest ) )]
        public async Task<IHttpActionResult> PostFEExecution(FEExecution fEExecution) {           
            if(!ModelState.IsValid) {
                return BadRequest( ModelState );
            }
            db.FEExecutions.Add( fEExecution );
            await db.SaveChangesAsync();

            return CreatedAtRoute( "DefaultApi", new { id = fEExecution.ID }, fEExecution );
        }

        protected override void Dispose(bool disposing) {
            if(disposing) {
                db.Dispose();
            }
            base.Dispose( disposing );
        }

        private bool FESearchRequestExists(int id) {
            return db.FESearchRequests.Count( e => e.ID == id ) > 0;
        }


    }

}
