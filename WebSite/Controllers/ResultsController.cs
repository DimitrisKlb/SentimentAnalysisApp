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

                await TheSReqController.UpdateFESearchRequestStatus( baseSearchRequestID, Status.Fulfilled );

                // Create the Execution object concerning the execution that was just completed, with the returned Results
                FEExecution newExecution = new FEExecution( baseSearchRequestID, searchRequest.LastExecutionCreatedOn, DateTime.Now );
                newExecution.TheResults = theResults;
                await TheExecsController.PostFEExecution( newExecution );

                return Ok();
            } else {
                return InternalServerError();
            }              
        }


    }
}
