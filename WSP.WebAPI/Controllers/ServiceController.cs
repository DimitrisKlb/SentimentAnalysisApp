using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Results;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

using SentimentAnalysisApp.SharedModels;
using WSP.Models;
using WSP.MasterActor.Interfaces;

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

                IMasterActor theMasterActor = ActorProxy.Create<IMasterActor>(new ActorId(createdSearchRequest.ID));
                theMasterActor.FulfillSearchRequestAsync(3);

                return Ok();
            } else {
                return InternalServerError();
            }           
        }

    }
}