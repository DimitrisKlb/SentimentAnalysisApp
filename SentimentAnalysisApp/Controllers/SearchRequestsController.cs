using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

using SentimentAnalysisApp.Models;

namespace SentimentAnalysisApp.Controllers {
    public class SearchRequestsController: ApiController {
        private MinedDataContext db = new MinedDataContext();

        // GET: api/SearchRequests
        public IQueryable<SearchRequest> GetSearchRequests() {
            return db.SearchRequests;
        }

        // GET: api/SearchRequests/5
        [ResponseType(typeof(SearchRequest))]
        public async Task<IHttpActionResult> GetSearchRequest(int id) {
            SearchRequest searchRequest = await db.SearchRequests.FindAsync(id);
            if(searchRequest == null) {
                return NotFound();
            }

            return Ok(searchRequest);
        }

        // GET: api/SearchRequests/status/1
        [ActionName("Status")]
        public IQueryable<SearchRequest> GetSearchRequestsByStatus(Status status) {
            var SRequestsByStatus = from SRequests in db.SearchRequests
                                    where SRequests.TheStatus == status
                                    select SRequests;
            return SRequestsByStatus;
        }

        // PUT: api/SearchRequests/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutSearchRequest(int id, SearchRequest searchRequest) {
            if(!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            if(id != searchRequest.ID) {
                return BadRequest();
            }

            db.Entry(searchRequest).State = EntityState.Modified;

            try {
                await db.SaveChangesAsync();
            } catch(DbUpdateConcurrencyException) {
                if(!SearchRequestExists(id)) {
                    return NotFound();
                } else {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // PUT:
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> UpdateSearchRequestStatus(int id, Status newStatus) {
            SearchRequest oldSearchRequest = await db.SearchRequests.FindAsync(id);
            if(oldSearchRequest == null) {
                return NotFound();
            }

            oldSearchRequest.TheStatus = newStatus;
            return await PutSearchRequest(id, oldSearchRequest);
        }

        // POST: api/SearchRequests
        [ResponseType(typeof(SearchRequest))]
        public async Task<IHttpActionResult> PostSearchRequest(SearchRequest searchRequest) {
            if(!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            db.SearchRequests.Add(searchRequest);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = searchRequest.ID }, searchRequest);
        }

        // DELETE: api/SearchRequests/5
        [ResponseType(typeof(SearchRequest))]
        public async Task<IHttpActionResult> DeleteSearchRequest(int id) {
            SearchRequest searchRequest = await db.SearchRequests.FindAsync(id);
            if(searchRequest == null) {
                return NotFound();
            }

            db.SearchRequests.Remove(searchRequest);
            await db.SaveChangesAsync();

            return Ok(searchRequest);
        }

        protected override void Dispose(bool disposing) {
            if(disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool SearchRequestExists(int id) {
            return db.SearchRequests.Count(e => e.ID == id) > 0;
        }
    }
}