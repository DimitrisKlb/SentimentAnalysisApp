using System.Threading.Tasks;
using System.Web.Http;

using SentimentAnalysisApp.SharedModels;
using WebSite.Models;

namespace WebSite.Controllers {

    public class ResultsController: ApiController {
        private FESearchRequestsController SReqController = new FESearchRequestsController();

        // POST: api/Results
        public async Task<IHttpActionResult> PostResults(BaseSearchRequest baseSearchRequest) {
            if(!ModelState.IsValid) {
                return BadRequest( ModelState );
            }

            await SReqController.UpdateSearchRequestStatus( baseSearchRequest.ID, Status.Fulfilled );
            return Ok();

        }
    }
}
