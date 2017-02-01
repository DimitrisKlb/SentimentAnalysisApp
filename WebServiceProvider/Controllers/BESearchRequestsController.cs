using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

using WebServiceProvider.Models;

namespace WebServiceProvider.Controllers {
    public class BESearchRequestsController: ApiController {
        private MainDBContext db = new MainDBContext();

        // POST: api/BESearchRequests
        [ResponseType(typeof(BESearchRequest))]
        public async Task<IHttpActionResult> PostBESearchRequest(BESearchRequest newBESearchRequest) {
            if(!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            db.BESearchRequests.Add(newBESearchRequest);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = newBESearchRequest.ID }, newBESearchRequest);
        }

        private bool BESearchRequestExists(int id) {
            return db.BESearchRequests.Count(e => e.ID == id) > 0;
        }
    }
}