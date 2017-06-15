using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Http.Results;
using System.Net.Http;
using System.Web.Configuration;

using Microsoft.AspNet.Identity;

using SentimentAnalysisApp.SharedModels;
using WebSite.Models;
using WebSite.ViewModels;

namespace WebSite.Controllers {

    [Authorize]
    public class MyRequestsController: FluentDisplayController {

        private FESearchRequestsController TheSReqController = new FESearchRequestsController();
        private FEExecutionsController TheExecsController = new FEExecutionsController();
        private static HttpClient clientBEserver = new HttpClient {
            BaseAddress = new System.Uri( WebConfigurationManager.AppSettings["WebApiProvider-URI"] )
        };


        [HttpGet]
        public ActionResult Index() {
            MyRequestsViewModel theVM = new MyRequestsViewModel();
            theVM.TheSearchRequests = TheSReqController.GetFESearchRequests( User.Identity.GetUserId() );
            theVM.TheBannerMsg = (MR_BannerMsg)LoadBannerMsg();

            return View( theVM );
        }

        [HttpGet]
        [ChildActionOnly]
        public PartialViewResult CreateSearchRequest() {
            if(ModelErrorHappened() == false) {
                return PartialView( "_CreateSearchRequest", new CreateSReqViewModel() );
            } else {
                LoadModelState();

                //Retrieve the previous CheckboxList selection 
                var p = ModelState["SelectedSources"];
                if(p != null && p.Value != null) {
                    string[] previousOptions = (string[])p.Value.RawValue;
                    return PartialView( "_CreateSearchRequest", new CreateSReqViewModel( previousOptions ) );
                } else {
                    return PartialView( "_CreateSearchRequest", new CreateSReqViewModel() );
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateSearchRequest(CreateSReqViewModel theVM) {
            if(ModelState.IsValid) {
                FESearchRequest newSearchRequest = theVM.TheSearchRequest;

                newSearchRequest.TheStatus = Status.New;
                newSearchRequest.TheUserID = User.Identity.GetUserId();
                newSearchRequest.TheSelectedSources.SetFromList( theVM.SelectedSources );

                var response = await TheSReqController.PostFESearchRequest( newSearchRequest );
                if(response.GetType() == typeof( CreatedAtRouteNegotiatedContentResult<FESearchRequest> )) {
                    FESearchRequest createdSearchRequest = ((CreatedAtRouteNegotiatedContentResult<FESearchRequest>)response).Content;
                    return await ExecuteSearchRequest( createdSearchRequest );
                } else {
                    return RedirectToIndex( MR_BannerMsg.ErrorNotCreated );
                }

            } else {
                return RedirectToIndex();
            }
        }

        public async Task<ActionResult> ExecuteSearchRequest(int searchRequestID) {

            var response = await TheSReqController.GetFESearchRequest( searchRequestID );
            if(response.GetType() == typeof( OkNegotiatedContentResult<FESearchRequest> )) {
                FESearchRequest searchRequest = ((OkNegotiatedContentResult<FESearchRequest>)response).Content;
                return await ExecuteSearchRequest( searchRequest );
            } else {
                return RedirectToIndex( MR_BannerMsg.ErrorNotExecuted );
            }

        }

        private async Task<ActionResult> ExecuteSearchRequest(FESearchRequest searchRequest) {

            Status previousStatus = searchRequest.TheStatus;
            if(previousStatus == Status.Executing) {
                return RedirectToIndex( MR_BannerMsg.ErrorAlreadyExecuting );
            }
            await TheSReqController.UpdateFESearchRequestStatus( searchRequest.ID, Status.Executing );

            // Send the Search Request to the BE Server for execution
            try {

                var response = await clientBEserver.PostAsJsonAsync(
                                WebConfigurationManager.AppSettings["WebApiProvider-SubmitRoute"],
                                (BaseSearchRequest)searchRequest );

                if(response.IsSuccessStatusCode) {
                    searchRequest.LastExecutionCreatedOn = DateTime.Now;
                    await TheSReqController.UpdateFESearchRequest( searchRequest.ID, searchRequest );
                    return RedirectToIndex( MR_BannerMsg.CreateOk );
                } else {
                    await TheSReqController.UpdateFESearchRequestStatus( searchRequest.ID, previousStatus );
                    return RedirectToIndex( MR_BannerMsg.ErrorNotExecuted );
                }
            } catch {
                await TheSReqController.UpdateFESearchRequestStatus( searchRequest.ID, previousStatus );
                return RedirectToIndex( MR_BannerMsg.ErrorNotExecuted );
            }
        }

        public PartialViewResult ViewExecutions(int searchRequestID) {
            var theExecutions = TheExecsController.GetFEExecutions( searchRequestID );
            return PartialView( "_ViewExecutions", theExecutions );
        }



    }
}