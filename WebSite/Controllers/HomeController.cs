using System.Web.Mvc;

namespace WebSite.Controllers {

    public class HomeController: Controller {

        [HttpGet]
        public ActionResult Index() {
            return View();
        }

    }
}