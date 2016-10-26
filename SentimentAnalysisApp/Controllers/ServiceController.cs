using System.Net;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;

using Tweetinvi;
using Tweetinvi.Parameters;
using Tweetinvi.Models;
using System.Web.Configuration;

namespace SentimentAnalysisApp.Controllers
{
    public class ServiceController : ApiController
    {
        static string defaultSearchKeywork = "coca-cola";
         
    // GET api/values
        [SwaggerOperation("GetDefaultKeyword")]
        public string Get()
        {
            return getTweets(defaultSearchKeywork);
        }

    // GET api/values/searchKeywork
        [SwaggerOperation("GetByKeyword")]
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

    // Naive miner from Twitter, to get tweets that contain searchKeyword
        private static string getTweets(string searchKeyword)
        {
            string twitterConsumerKey, twitterConsumerSecret, twitterAccessToken, twitterAccessTokenSecret;
            twitterConsumerKey = WebConfigurationManager.AppSettings["twitterConsumerKey"];
            twitterConsumerSecret = WebConfigurationManager.AppSettings["twitterConsumerSecret"];
            twitterAccessToken = WebConfigurationManager.AppSettings["twitterAccessToken"];
            twitterAccessTokenSecret = WebConfigurationManager.AppSettings["twitterAccessTokenSecret"];

            // Set up your credentials
            Auth.SetUserCredentials(twitterConsumerKey, twitterConsumerSecret, twitterAccessToken, twitterAccessTokenSecret);

            // Get relevant tweets
            var searchParameters = new SearchTweetsParameters(searchKeyword)
            {
                Lang = LanguageFilter.English,
                SearchType = SearchResultType.Mixed
            };
            var theTweets1 = Search.SearchTweets(searchParameters);
            var theTweets2 = Search.SearchTweets(searchParameters);

            return searchKeyword + " in text.";
        }

    }
}
