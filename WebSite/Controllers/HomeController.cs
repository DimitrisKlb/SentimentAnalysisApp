using System.Threading.Tasks;
using System.Web.Mvc;

using WebSite.Models;

namespace WebSite.Controllers {
    public class HomeController: Controller {
        private FESearchRequestsController SReqController = new FESearchRequestsController();

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
        public async Task<ActionResult> CreateSearchRequest([Bind(Include = "ID,TheSearchKeyword,TheStatus")] FESearchRequest searchRequest) {
            if(ModelState.IsValid) {
                searchRequest.TheStatus = Status.Fulfilled;
                await SReqController.PostFESearchRequest(searchRequest);
            }

            return RedirectToAction("Index");
        }

        /*
        public async Task<ActionResult> ExecuteSearchRequest(int searchRequestID) {
            await SReqController.UpdateSearchRequestStatus(searchRequestID, Status.Pending);
            return RedirectToAction("Index");
        }

        // Will be called by server threads-jobs
        public async Task<ActionResult> EndSearchRequest(int searchRequestID) {
            var client = new HttpClient { BaseAddress = new System.Uri("http://localhost:60835/") };
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