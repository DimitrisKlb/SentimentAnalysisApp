using System.Net;
using System.Web.Http;
using System.Web.Configuration;
using Swashbuckle.Swagger.Annotations;

using Tweetinvi;
using Tweetinvi.Parameters;
using Tweetinvi.Models;

using SentimentAnalysisApp.Models;

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

        // Basic Twitter miner, to get tweets that contain searchKeyword
        private static string getTweets(string searchKeyword)
        {
            string twitterConsumerKey, twitterConsumerSecret, twitterAccessToken, twitterAccessTokenSecret;
        
            // Set up your credentials
            twitterConsumerKey = WebConfigurationManager.AppSettings["twitterConsumerKey"];
            twitterConsumerSecret = WebConfigurationManager.AppSettings["twitterConsumerSecret"];
            twitterAccessToken = WebConfigurationManager.AppSettings["twitterAccessToken"];
            twitterAccessTokenSecret = WebConfigurationManager.AppSettings["twitterAccessTokenSecret"];

            Auth.SetUserCredentials(twitterConsumerKey, twitterConsumerSecret, twitterAccessToken, twitterAccessTokenSecret);

            // Get relevant tweets
            SearchTweetsParameters searchParameters = new SearchTweetsParameters(searchKeyword)
            {
                Lang = LanguageFilter.English,
                SearchType = SearchResultType.Mixed,
                MaximumNumberOfResults = 5
            };

            searchParameters.SinceId = 0;
            searchParameters.MaxId = 0;             
            var theTweets = Search.SearchTweets(searchParameters);

            // Store tweets in the database
            using (var db = new MinedDataContext())
            {
                foreach (var tweet in theTweets)
                {
                    db.MinedTexts.Add(new MinedText() { TheText = tweet.FullText, TheSource = Source.Twitter });
                }
                db.SaveChanges();
            }
            
                return searchKeyword + " in text.";
        }

    }
}
