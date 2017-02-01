using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;

using SentimentAnalysisApp.SharedModels;
using WebServiceProvider.Models;

namespace WebServiceProvider.Controllers {
    public class ServiceController: ApiController {
        private BESearchRequestsController SReqController = new BESearchRequestsController();

        // POST: api/Service
        [ResponseType(typeof(BaseSearchRequest))]
        public async Task<IHttpActionResult> PostSearchRequest(BaseSearchRequest baseSearchRequest) {
            if(!ModelState.IsValid) {
                return BadRequest(ModelState);
            }

            BESearchRequest newBESearchRequest = new BESearchRequest(baseSearchRequest);
            var response = await SReqController.PostBESearchRequest(newBESearchRequest);

            if(response.GetType() == typeof(CreatedAtRouteNegotiatedContentResult<BESearchRequest>)) {                
                return Ok();
            } else {
                return InternalServerError();
            }           
        }

    }
}
