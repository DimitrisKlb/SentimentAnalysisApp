﻿using System.Threading.Tasks;
using System.Web.Mvc;

using SentimentAnalysisApp.Models;
using System.Net.Http;

namespace SentimentAnalysisApp.Controllers
{
    public class HomeController : Controller
    {
        private SearchRequestsController SReqController = new SearchRequestsController();

        // GET: Home
        [HttpGet]
        public ActionResult Index()
        {
            var searchRequests = SReqController.GetSearchRequests();

            return View(searchRequests);
        }

        [HttpGet]
        [ChildActionOnly]
        public PartialViewResult CreateSearchRequest()
        {
            return PartialView("_CreateSearchRequest", new SearchRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateSearchRequest([Bind(Include = "ID,TheSearchKeyword,TheStatus")] SearchRequest searchRequest)
        {
            if (ModelState.IsValid)
            {
                searchRequest.TheStatus = Status.Open;
                await SReqController.PostSearchRequest(searchRequest);
            }
            return RedirectToAction("Index");

        }

        public async Task<ActionResult> ExecuteSearchRequest(int searchRequestID)
        {
            await SReqController.UpdateSearchRequestStatus(searchRequestID, Status.Pending);
            return RedirectToAction("Index");
        }

        // Will be called by server threads-jobs
        public async Task<ActionResult> EndSearchRequest(int searchRequestID)
        {
            var client = new HttpClient { BaseAddress = new System.Uri("http://localhost:60835/") };
            SearchRequest searchRequest = client.GetAsync("api/SearchRequests/" + searchRequestID).Result.Content.ReadAsAsync<SearchRequest>().Result;

            ServiceController.getTweets(searchRequest.TheSearchKeyword, searchRequestID);

            await SReqController.UpdateSearchRequestStatus(searchRequestID, Status.Fulfilled);
            return RedirectToAction("Index");
        }


    }

}
