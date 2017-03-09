using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;

using SentimentAnalysisApp.SharedModels;
using WSP.Models;

namespace WSP.WebAPI.Controllers {
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
                BESearchRequest createdSearchRequest = ((CreatedAtRouteNegotiatedContentResult<BESearchRequest>)response).Content;

                //IMasterActor theMasterActor = ActorProxy.Create<IMasterActor>(new ActorId(createdSearchRequest.ID), WSPServiceFaFabricAppName);
                //theMasterActor.HandleSearchRequestAsync(3);

                return Ok();
            } else {
                return InternalServerError();
            }           
        }

    }
}