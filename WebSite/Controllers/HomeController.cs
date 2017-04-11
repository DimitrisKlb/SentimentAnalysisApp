using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Http.Results;
using System.Net.Http;
using System.Web.Configuration;

using SentimentAnalysisApp.SharedModels;
using WebSite.Models;

namespace WebSite.Controllers {

    [Authorize]
    public class HomeController: Controller {

        private FESearchRequestsController SReqController = new FESearchRequestsController();
        private static HttpClient clientBEserver = new HttpClient {
            BaseAddress = new System.Uri(WebConfigurationManager.AppSettings["WebApiProviderURI"])
        };


        [HttpGet]
        public ActionResult Index() {
            HomeViewModel homeVM = new HomeViewModel();
            homeVM.TheSearchRequests = SReqController.GetFESearchRequests();
            if(TempData["bannerMsgCode"] == null) {
                homeVM.TheBannerCode = BannnerMsgCode.None;
            } else {
                homeVM.TheBannerCode = (BannnerMsgCode)TempData["bannerMsgCode"];
            }

            return View(homeVM);

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
                    return RedirectToIndex(BannnerMsgCode.ErrorNotCreated);
                }

            } else {
                return RedirectToIndex(BannnerMsgCode.ErrorNotCreated);
            }
        }

        public async Task<ActionResult> ExecuteSearchRequest(int searchRequestID) {

            var response = await SReqController.GetFESearchRequest(searchRequestID);
            if(response.GetType() == typeof(OkNegotiatedContentResult<FESearchRequest>)) {
                FESearchRequest searchRequest = ((OkNegotiatedContentResult<FESearchRequest>)response).Content;
                return await ExecuteSearchRequest(searchRequest);
            } else {
                await SReqController.UpdateSearchRequestStatus(searchRequestID, Status.Open);              
                return RedirectToIndex(BannnerMsgCode.ErrorNotExecuted);
            }

        }

        private async Task<ActionResult> ExecuteSearchRequest(FESearchRequest searchRequest) {

            try {
                var response = await clientBEserver.PostAsJsonAsync("api/Service", (BaseSearchRequest)searchRequest);
                if(response.IsSuccessStatusCode) {
                    await SReqController.UpdateSearchRequestStatus(searchRequest.ID, Status.Pending);
                    return RedirectToIndex(BannnerMsgCode.CreateOk);
                } else {
                    await SReqController.UpdateSearchRequestStatus(searchRequest.ID, Status.Open);
                    return RedirectToIndex(BannnerMsgCode.ErrorNotExecuted);
                }
            } catch {
                await SReqController.UpdateSearchRequestStatus(searchRequest.ID, Status.Open);
                return RedirectToIndex(BannnerMsgCode.ErrorNotExecuted);
            }
        }

        System.Web.Mvc.RedirectToRouteResult RedirectToIndex(BannnerMsgCode bannerMsgCode) {
            TempData["bannerMsgCode"] = bannerMsgCode;
            return RedirectToAction("Index");
        }

    }
}