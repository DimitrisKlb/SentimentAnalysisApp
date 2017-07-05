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
        private FEMainDBContext db = new FEMainDBContext();

        [NonAction]
        public IQueryable<FESearchRequest> GetFESearchRequests() {
            return db.FESearchRequests;
        }

        [NonAction]
        public IEnumerable<FESearchRequest> GetFESearchRequests(string userID) {
            try {
                var searchRequestsByUser = db.FESearchRequests
                                            .Where( sReq => sReq.TheUserID == userID )
                                            .Include( sReq => sReq.TheLatestExecution.TheResults )
                                            .ToList();
                return searchRequestsByUser;
            } catch(Exception e) {
                return null;
            }
        }

        [NonAction]
        public IEnumerable<FESearchRequest> GetFESearchRequests(string userID, Status status) {
            try {
                var g = GetFESearchRequests(userID );
                var searchRequestsByUser = g
                                            .Where( sReq => sReq.TheStatus == status )
                                            .ToList();

                return searchRequestsByUser;
            } catch(Exception e) {
                return null;
            }
        }

        [NonAction]
        [ResponseType( typeof( FESearchRequest ) )]
        public async Task<IHttpActionResult> GetFESearchRequest(int id) {
            FESearchRequest fESearchRequest = db.FESearchRequests
                                                .Where( sReq => sReq.ID == id )
                                                .Include( sReq => sReq.TheLatestExecution.TheResults )
                                                .First();
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
        public async Task<IHttpActionResult> UpdateFESearchRequestStatus(int id, Status newStatus) {
            FESearchRequest oldSearchRequest = await db.FESearchRequests.FindAsync( id );
            if(oldSearchRequest == null) {
                return NotFound();
            }

            oldSearchRequest.TheStatus = newStatus;
            return await UpdateFESearchRequest( id, oldSearchRequest );
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
