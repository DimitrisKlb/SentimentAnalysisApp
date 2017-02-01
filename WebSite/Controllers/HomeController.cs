using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Http.Results;
using System.Net.Http;

using SentimentAnalysisApp.SharedModels;
using WebSite.Models;
using System.Web.Configuration;

namespace WebSite.Controllers {
    public class HomeController: Controller {
        private FESearchRequestsController SReqController = new FESearchRequestsController();
        private HttpClient clientBEserver = new HttpClient {
            BaseAddress = new System.Uri(WebConfigurationManager.AppSettings["WebApiProviderURI"])
        };

        [HttpGet]
        public ActionResult Index() {
            var searchRequests = SReqController.GetFESearchRequests();

            return View(searchRequests);

        }

        [HttpGet]
        [ChildActionOnly]
        public PartialViewResult CreateSearchRequest() {
            return PartialView("_CreateSearchRequest", new FESearchRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateSearchRequest([Bind(Include = "ID,TheSearchKeyword,TheStatus")] FESearchRequest newSearchRequest) {
            if(ModelState.IsValid) {
                newSearchRequest.TheStatus = Status.Pending;
                var response = await SReqController.PostFESearchRequest(newSearchRequest);

                if(response.GetType() == typeof(CreatedAtRouteNegotiatedContentResult<FESearchRequest>)) {
                    FESearchRequest createdSearchRequest = ((CreatedAtRouteNegotiatedContentResult<FESearchRequest>)response).Content;
                    return await ExecuteSearchRequest(createdSearchRequest);
                } else {
                    //Error: SearchRequest could not be created
                    return RedirectToAction("Index");
                }

            } else {
                //Error: Error while creating SearchRequest
                return RedirectToAction("Index");
            }
        }

        public async Task<ActionResult> ExecuteSearchRequest(int searchRequestID) {
            var response = await SReqController.GetFESearchRequest(searchRequestID);

            if(response.GetType() == typeof(OkNegotiatedContentResult<FESearchRequest>)) {
                FESearchRequest searchRequest = ((OkNegotiatedContentResult<FESearchRequest>)response).Content;
                return await ExecuteSearchRequest(searchRequest);
            } else {
                await SReqController.UpdateSearchRequestStatus(searchRequestID, Status.Open);
                //bale to swsto error (save sta drafts)
                return RedirectToAction("Index");
            }

        }

        private async Task<ActionResult> ExecuteSearchRequest(FESearchRequest searchRequest) {
            var response = await clientBEserver.PostAsJsonAsync("api/Service", (BaseSearchRequest)searchRequest);
            if(response.IsSuccessStatusCode) {
                //All ok
            } else {
                await SReqController.UpdateSearchRequestStatus(searchRequest.ID, Status.Open);
                //Error: SearchRequest could not be send for execution. Saved in drafts
            }

            return RedirectToAction("Index");
        }

        /*
        public async Task<ActionResult> EndSearchRequest(int searchRequestID) {           
            SearchRequest searchRequest = client.GetAsync("api/SearchRequests/" + searchRequestID).Result.Content.ReadAsAsync<SearchRequest>().Result;

            int returnedTweets = ServiceController.getTweets(searchRequest.TheSearchKeyword, searchRequestID);

            if(returnedTweets >= 0) {
                await SReqController.UpdateSearchRequestStatus(searchRequestID, Status.Fulfilled);
            }
            return RedirectToAction("Index");
        }
        */

    }
}