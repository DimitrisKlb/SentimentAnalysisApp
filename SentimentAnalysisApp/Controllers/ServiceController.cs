using System.Net;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;

namespace SentimentAnalysisApp.Controllers
{
    public class ServiceController : ApiController
    {
        static string defaultSearchKeywork = "coca cola";
         
        // GET api/values
        [SwaggerOperation("GetAll")]
        public string Get()
        {
            return getTweets(defaultSearchKeywork);
        }

        // GET api/values/searchKeywork
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public string Get(string searchKeyword)
        {
            return getTweets(searchKeyword);
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public void Post([FromBody]string value)
        {
            defaultSearchKeywork = value;
        }

        // Naive mine from Twitter, tweets thats contain searchKeyword
        private static string getTweets(string searchKeyword)
        {
            return searchKeyword + " in text.";
        }

    }
}
