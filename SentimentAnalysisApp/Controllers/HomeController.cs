using SentimentAnalysisApp.Models;
using System.Threading.Tasks;
using System.Web.Mvc;

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

       /*
        public ActionResult ExecuteSearchRequest(int id)
        {
            
        }
        */

    }

}
