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

    public class FESearchRequestsController: ApiController {
        //private FEMainDBContext db = new FEMainDBContext();

        [NonAction]
        public IEnumerable<FESearchRequest> GetFESearchRequests() {
            IEnumerable<FESearchRequest> sReqs = null;
            using(var db = new FEMainDBContext()) {
                sReqs = db.FESearchRequests;
            }
            return sReqs;
        }

        [NonAction]
        public IEnumerable<FESearchRequest> GetFESearchRequests(string userID) {
            IEnumerable<FESearchRequest> searchRequestsByUser = null;
            using(var db = new FEMainDBContext()) {
                try {
                    searchRequestsByUser = db.FESearchRequests
                                                .Where( sReq => sReq.TheUserID == userID )
                                                .Include( sReq => sReq.TheLatestExecution.TheResults )
                                                .ToList();
                    return searchRequestsByUser;
                } catch {
                }
            }
            return searchRequestsByUser;
        }

        [NonAction]
        public IEnumerable<FESearchRequest> GetFESearchRequests(string userID, Status status) {
            IEnumerable<FESearchRequest> searchRequestsByUser = null;
            try {
                var sreqs = GetFESearchRequests( userID );
                searchRequestsByUser = sreqs
                                            .Where( sReq => sReq.TheStatus == status )
                                            .ToList();
            } catch {
            }
            return searchRequestsByUser;
        }

        [NonAction]
        [ResponseType( typeof( FESearchRequest ) )]
        public async Task<IHttpActionResult> GetFESearchRequest(int id) {
            FESearchRequest fESearchRequest = null;
            using(var db = new FEMainDBContext()) {
                fESearchRequest = db.FESearchRequests
                                                .Where( sReq => sReq.ID == id )
                                                .Include( sReq => sReq.TheLatestExecution.TheResults )
                                                .First();
            }
            if(fESearchRequest == null) {
                return NotFound();
            }
            return Ok( fESearchRequest );
        }

        [NonAction]
        [ResponseType( typeof( void ) )]
        public async Task<IHttpActionResult> UpdateFESearchRequest(int id, FESearchRequest fESearchRequest) {
            if(!ModelState.IsValid) {
                return BadRequest( ModelState );
            }
            if(id != fESearchRequest.ID) {
                return BadRequest();
            }

            using(var db = new FEMainDBContext()) {
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
            }
            return StatusCode( HttpStatusCode.NoContent );
        }

        [NonAction]
        [ResponseType( typeof( void ) )]
        public async Task<IHttpActionResult> UpdateFESearchRequestStatus(int id, Status newStatus) {
            FESearchRequest oldSearchRequest = null;

            using(var db = new FEMainDBContext()) {
                oldSearchRequest = await db.FESearchRequests.FindAsync( id );
                if(oldSearchRequest == null) {
                    return NotFound();
                }
                oldSearchRequest.TheStatus = newStatus;
            }
            return await UpdateFESearchRequest( id, oldSearchRequest );
        }

        [NonAction]
        [ResponseType( typeof( FESearchRequest ) )]
        public async Task<IHttpActionResult> PostFESearchRequest(FESearchRequest fESearchRequest) {
            if(!ModelState.IsValid) {
                return BadRequest( ModelState );
            }

            using(var db = new FEMainDBContext()) {
                db.FESearchRequests.Add( fESearchRequest );
                await db.SaveChangesAsync();
            }
            return CreatedAtRoute( "DefaultApi", new { id = fESearchRequest.ID }, fESearchRequest );
        }

        /*
        protected override void Dispose(bool disposing) {
            if(disposing) {
                db.Dispose();
            }
            base.Dispose( disposing );
        }
        */

        private bool FESearchRequestExists(int id) {
            using(var db = new FEMainDBContext()) {
                return db.FESearchRequests.Count( e => e.ID == id ) > 0;
            }
        }

    }
}
