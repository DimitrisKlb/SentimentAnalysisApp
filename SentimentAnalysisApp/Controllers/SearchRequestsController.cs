using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using SentimentAnalysisApp.Models;

namespace SentimentAnalysisApp.Controllers
{
    public class SearchRequestsController : ApiController
    {
        private MinedDataContext db = new MinedDataContext();

        // GET: api/SearchRequests
        public IQueryable<SearchRequest> GetSearchRequests()
        {
            return db.SearchRequests;
        }

        // GET: api/SearchRequests/5
        [ResponseType(typeof(SearchRequest))]
        public async Task<IHttpActionResult> GetSearchRequest(int id)
        {
            SearchRequest searchRequest = await db.SearchRequests.FindAsync(id);
            if (searchRequest == null)
            {
                return NotFound();
            }

            return Ok(searchRequest);
        }

        // GET: api/SearchRequests/status/1
        [ActionName("Status")]
        public IQueryable<SearchRequest> GetSearchRequestsByStatus(Status status)
        {
            var SRequestsByStatus = from SRequests in db.SearchRequests
                      where SRequests.TheStatus == status
                      select SRequests;
            return SRequestsByStatus;
        }

        // POST: api/SearchRequests
        [ResponseType(typeof(SearchRequest))]
        public async Task<IHttpActionResult> PostSearchRequest(SearchRequest searchRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.SearchRequests.Add(searchRequest);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = searchRequest.ID }, searchRequest);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool SearchRequestExists(int id)
        {
            return db.SearchRequests.Count(e => e.ID == id) > 0;
        }
    }
}