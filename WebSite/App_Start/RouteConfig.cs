using System.Web.Mvc;
using System.Web.Routing;

namespace WebSite {
    public class RouteConfig {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute( "{resource}.axd/{*pathInfo}" );

            routes.MapRoute(
                name: "RequestsByStatus",
                url: "MyRequests/Index/{category}",
                defaults: new { controller = "MyRequests", action = "Index", category = "All" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
