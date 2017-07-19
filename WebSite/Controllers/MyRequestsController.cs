using System;
using System.Linq;
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
        public ActionResult Index(string category) {
            MyRequestsViewModel theVM = new MyRequestsViewModel();
            Status theStatus;

            try {
                theVM.TheCategory = (Category)Enum.Parse( typeof( Category ), category );
                switch(theVM.TheCategory) {
                    case Category.Drafts:
                        theStatus = Status.New;
                        break;
                    case Category.Pending:
                        theStatus = Status.Executing;
                        break;
                    case Category.Completed:
                        theStatus = Status.Fulfilled;
                        break;
                    default:
                        throw new Exception();
                }

                theVM.TheSearchRequests = TheSReqController.GetFESearchRequests( User.Identity.GetUserId(), theStatus );
            } catch {
                theVM.TheCategory = Category.All;
                theVM.TheSearchRequests = TheSReqController.GetFESearchRequests( User.Identity.GetUserId() );
            }
            theVM.TheSearchRequests.OrderByDescending( sReq => sReq.TheLatestExecution.StartedOn );
            theVM.TheBannerMsg = (MR_BannerMsg)LoadBannerMsg();

            return View( theVM );
        }


        [HttpGet]
        public async Task<ActionResult> Inspect(int id = -1) {
            if(id == -1) {
                return RedirectTo( "Index" );
            }
            InspectSReqsViewModel theVM = new InspectSReqsViewModel();

            var response = await TheSReqController.GetFESearchRequest( id );
            if(response.GetType() == typeof( OkNegotiatedContentResult<FESearchRequest> )) {
                theVM.TheSearchRequest = ((OkNegotiatedContentResult<FESearchRequest>)response).Content;
                theVM.TheBannerMsg = theVM.TheBannerMsg = (MR_BannerMsg)LoadBannerMsg();

                return View( "Inspect", theVM );
            } else {
                return RedirectTo( "Index", MR_BannerMsg.ErrorCannotInspect );
            }
        }

        [HttpGet]
        [ChildActionOnly]
        public PartialViewResult ViewExecutions(int searchRequestID) {
            var theExecutions = TheExecsController.GetFEExecutions( searchRequestID );
            return PartialView( "_ViewExecutions", theExecutions );
        }

        [HttpGet]
        public ActionResult Create() {
            if(ModelErrorHappened() == false) {
                return View( new CreateSReqViewModel() );
            } else {
                LoadModelState();

                //Retrieve the previous CheckboxList selection 
                var p = ModelState["SelectedSources"];
                if(p != null && p.Value != null) {
                    string[] previousOptions = (string[])p.Value.RawValue;
                    return View( new CreateSReqViewModel( previousOptions ) );
                } else {
                    return View( new CreateSReqViewModel() );
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateSReqViewModel theVM) {
            if(ModelState.IsValid) {
                FESearchRequest newSearchRequest = theVM.TheSearchRequest;

                newSearchRequest.TheStatus = Status.New;
                newSearchRequest.TheUserID = User.Identity.GetUserId();
                newSearchRequest.TheSelectedSources.SetFromList( theVM.SelectedSources );

                var response = await TheSReqController.PostFESearchRequest( newSearchRequest );
                if(response.GetType() == typeof( CreatedAtRouteNegotiatedContentResult<FESearchRequest> )) {
                    FESearchRequest createdSearchRequest = ((CreatedAtRouteNegotiatedContentResult<FESearchRequest>)response).Content;
                    return await Execute( createdSearchRequest );
                } else {
                    return RedirectTo( "Index", MR_BannerMsg.ErrorNotCreated );
                }

            } else {
                return RedirectTo( "Create" );
            }
        }

        public async Task<ActionResult> Execute(int searchRequestID) {

            var response = await TheSReqController.GetFESearchRequest( searchRequestID );
            if(response.GetType() == typeof( OkNegotiatedContentResult<FESearchRequest> )) {
                FESearchRequest searchRequest = ((OkNegotiatedContentResult<FESearchRequest>)response).Content;
                return await Execute( searchRequest );
            } else {
                return RedirectTo( "Inspect", MR_BannerMsg.ErrorNotExecuted, routeValues: new { id = searchRequestID } );
            }

        }

        private async Task<ActionResult> Execute(FESearchRequest searchRequest) {

            Status previousStatus = searchRequest.TheStatus;
            if(previousStatus == Status.Executing) {
                return RedirectTo( "Inspect", MR_BannerMsg.ErrorAlreadyExecuting, routeValues: new { id = searchRequest.ID } );
            }
            searchRequest.TheStatus = Status.Executing;
            await TheSReqController.UpdateFESearchRequestStatus( searchRequest.ID, Status.Executing );

            // Send the Search Request to the BE Server for execution
            try {

                var response = await clientBEserver.PostAsJsonAsync(
                                WebConfigurationManager.AppSettings["WebApiProvider-SubmitRoute"],
                                (BaseSearchRequest)searchRequest );

                if(response.IsSuccessStatusCode) {
                    searchRequest.LastExecutionCreatedOn = DateTime.Now;
                    await TheSReqController.UpdateFESearchRequest( searchRequest.ID, searchRequest );
                    return RedirectTo( "Inspect", MR_BannerMsg.CreateOk, routeValues: new { id = searchRequest.ID } );
                } else {
                    await TheSReqController.UpdateFESearchRequestStatus( searchRequest.ID, previousStatus );
                    return RedirectTo( "Inspect", MR_BannerMsg.ErrorNotExecuted, routeValues: new { id = searchRequest.ID } );
                }
            } catch {
                await TheSReqController.UpdateFESearchRequestStatus( searchRequest.ID, previousStatus );
                return RedirectTo( "Inspect", MR_BannerMsg.ErrorNotExecuted, routeValues: new { id = searchRequest.ID } );
            }
        }


    }
}