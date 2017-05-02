using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

using WebSite.Models;

namespace WebSite.Controllers {
    
    public class FESearchRequestsController: ApiController {
        private FEMainDBContext db = new FEMainDBContext();

        [NonAction]
        public IQueryable<FESearchRequest> GetFESearchRequests() {
            return db.FESearchRequests;
        }

        [NonAction]
        public IQueryable<FESearchRequest> GetFESearchRequests(string userID) {
            var searchRequestsByUser = from searchRequests in db.FESearchRequests
                                       where searchRequests.TheUserID == userID
                                       select searchRequests;
            return searchRequestsByUser;
        }

        [NonAction]
        [ResponseType( typeof( FESearchRequest ) )]
        public async Task<IHttpActionResult> GetFESearchRequest(int id) {
            FESearchRequest fESearchRequest = await db.FESearchRequests.FindAsync( id );
            if(fESearchRequest == null) {
                return NotFound();
            }
            return Ok( fESearchRequest );
        }

        [NonAction]
        [ResponseType( typeof( void ) )]
        public async Task<IHttpActionResult> PutFESearchRequest(int id, FESearchRequest fESearchRequest) {
            if(!ModelState.IsValid) {
                return BadRequest( ModelState );
            }

            if(id != fESearchRequest.ID) {
                return BadRequest();
            }

            db.Entry( fESearchRequest ).State = EntityState.Modified;

            try {
                await db.SaveChangesAsync();
            } catch(DbUpdateConcurrencyException) {
                if(!FESearchRequestExists( id )) {
                    return NotFound();
                } else {
                    throw;
                }
            }

            return StatusCode( HttpStatusCode.NoContent );
        }

        [NonAction]
        [ResponseType( typeof( void ) )]
        public async Task<IHttpActionResult> UpdateSearchRequestStatus(int id, Status newStatus) {
            FESearchRequest oldSearchRequest = await db.FESearchRequests.FindAsync( id );
            if(oldSearchRequest == null) {
                return NotFound();
            }

            oldSearchRequest.TheStatus = newStatus;
            return await PutFESearchRequest( id, oldSearchRequest );
        }

        [NonAction]
        [ResponseType( typeof( FESearchRequest ) )]
        public async Task<IHttpActionResult> PostFESearchRequest(FESearchRequest fESearchRequest) {
            if(!ModelState.IsValid) {
                return BadRequest( ModelState );
            }

            db.FESearchRequests.Add( fESearchRequest );
            await db.SaveChangesAsync();

            return CreatedAtRoute( "DefaultApi", new { id = fESearchRequest.ID }, fESearchRequest );
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
