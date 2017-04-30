using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

using WSP.Models;

namespace WSP.WebAPI.Controllers {
    public class BESearchRequestsController: ApiController {
        //private MainDBContext db = new MainDBContext();

        [NonAction]
        [ResponseType( typeof( BESearchRequest ) )]
        public async Task<IHttpActionResult> PostBESearchRequest(BESearchRequest newBESearchRequest) {
            if(!ModelState.IsValid) {
                return BadRequest( ModelState );
            }

            //db.BESearchRequests.Add(newBESearchRequest);
            //await db.SaveChangesAsync();

            return CreatedAtRoute( "DefaultApi", new { id = newBESearchRequest.ID }, newBESearchRequest );
        }

        private bool BESearchRequestExists(int id) {
            return false;
            //return db.BESearchRequests.Count(e => e.ID == id) > 0;
        }
    }
}