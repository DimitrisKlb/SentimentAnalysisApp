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
                routeTemplate: "api/{controller}/{searchKeyword}",
                defaults: new { searchKeyword = RouteParameter.Optional }
            );
        }
    }
}
