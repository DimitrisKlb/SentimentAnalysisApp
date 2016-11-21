using System.Web.Http;

namespace SentimentAnalysisApp
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ServiceApi",
                routeTemplate: "api/Service/{searchKeyword}",
                defaults: new { controller = "Service", searchKeyword = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "SearchRequestsApi",
                routeTemplate: "api/SearchRequests/status/{status}",
                defaults: new { controller = "SearchRequests", action = "status" }
            );
        }
    }
}
