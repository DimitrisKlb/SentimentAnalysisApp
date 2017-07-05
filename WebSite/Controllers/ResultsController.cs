using System;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

using SentimentAnalysisApp.SharedModels;
using WebSite.Models;

namespace WebSite.Controllers {

    public class ResultsController: ApiController {
        private FESearchRequestsController TheSReqController = new FESearchRequestsController();
        private FEExecutionsController TheExecsController = new FEExecutionsController();

        [Route( "api/Results/Submit" )]
        [HttpPost]
        public async Task<IHttpActionResult> SubmitResults(Results theResults) {
            if(!ModelState.IsValid) {
                return BadRequest( ModelState );
            }
            int baseSearchRequestID = theResults.ID;

            // Find the corresponding Search Request
            var response = await TheSReqController.GetFESearchRequest( baseSearchRequestID );
            if(response.GetType() == typeof( OkNegotiatedContentResult<FESearchRequest> )) {
                FESearchRequest searchRequest = ((OkNegotiatedContentResult<FESearchRequest>)response).Content;

                // Create the Execution object concerning the execution that was just completed, with the returned Results
                FEExecution newExecution = new FEExecution( baseSearchRequestID, searchRequest.LastExecutionCreatedOn, DateTime.Now );
                newExecution.TheResults = theResults;
                response = await TheExecsController.PostFEExecution( newExecution );
                if(response.GetType() == typeof( CreatedAtRouteNegotiatedContentResult<FEExecution> )) {
                    FEExecution createdExecution = ((CreatedAtRouteNegotiatedContentResult<FEExecution>)response).Content;

                    // Update the Search Request
                    searchRequest.TheStatus = Status.Fulfilled;
                    searchRequest.LatestExecutionID = createdExecution.ID;
                    await TheSReqController.UpdateFESearchRequest( searchRequest.ID, searchRequest );

                    return Ok();
                } else {
                    return InternalServerError();
                }                
            } else {
                return InternalServerError();
            }
        }


    }
}
