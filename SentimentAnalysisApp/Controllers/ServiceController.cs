using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Configuration;
using System.Web.Http;

using Swashbuckle.Swagger.Annotations;

using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

using SentimentAnalysisApp.Models;

namespace SentimentAnalysisApp.Controllers {
    public class ServiceController: ApiController {
        static string defaultSearchKeywork = "coca-cola";

        // GET api/values
        [SwaggerOperation("GetDefaultKeyword")]
        public string Get() {
            getTweets(defaultSearchKeywork, 1);
            return "";
        }

        // GET api/values/searchKeywork
        [SwaggerOperation("GetByKeyword")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public string Get(SearchRequest searchRequest) {
            getTweets(searchRequest.TheSearchKeyword, searchRequest.ID);
            return "";
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public void Post([FromBody]string value) {
            defaultSearchKeywork = value;
        }

        // Basic Twitter miner, to get tweets that contain searchKeyword
        public static string getTweets(string searchKeyword, int searchRequestID) {
            string twitterConsumerKey, twitterConsumerSecret, twitterAccessToken, twitterAccessTokenSecret;
            SearchTweetsParameters searchParameters;
            IEnumerable<ITweet> theTweets;
            ushort windowSize;
            int tweetsReturned, totalTweets;

            // Disable the exception swallowing to allow exception to be thrown by Tweetinvi
            ExceptionHandler.SwallowWebExceptions = false;

            // Set up your credentials
            twitterConsumerKey = WebConfigurationManager.AppSettings["twitterConsumerKey"];
            twitterConsumerSecret = WebConfigurationManager.AppSettings["twitterConsumerSecret"];
            twitterAccessToken = WebConfigurationManager.AppSettings["twitterAccessToken"];
            twitterAccessTokenSecret = WebConfigurationManager.AppSettings["twitterAccessTokenSecret"];

            Auth.SetUserCredentials(twitterConsumerKey, twitterConsumerSecret, twitterAccessToken, twitterAccessTokenSecret);

            // Basic Search Parameters 
            windowSize = 2000;
            searchParameters = new SearchTweetsParameters("\"" + searchKeyword + "\"") {
                Lang = LanguageFilter.English,
                SearchType = SearchResultType.Recent,
                MaximumNumberOfResults = windowSize
            };

            // Find relevant tweets iteratively, in windows of a certains size (windowSize)
            totalTweets = 0;
            tweetsReturned = 10;
            theTweets = null;
            do {
                try {
                    theTweets = Search.SearchTweets(searchParameters);
                    if(theTweets == null) {
                        string f = ExceptionHandler.GetLastException().TwitterDescription;
                    }

                    tweetsReturned = theTweets.Count();
                    if(tweetsReturned != 0) {                        
                        totalTweets += tweetsReturned;
                        searchParameters.MaxId = theTweets.Last().Id - 1;

                        // Store tweets in the database
                        using(var db = new MinedDataContext()) {
                            foreach(var tweet in theTweets) {
                                db.MinedTexts.Add(new MinedText() {
                                    TheText = tweet.FullText,
                                    TheSource = Source.Twitter,
                                    SearchRequestID = searchRequestID,
                                    TwitterID = tweet.Id
                                });
                            }
                            db.SaveChanges();
                        }
                    }
                } catch(Exception e) {
                    var x = e.Data;
                    var y = e.Message;
                    
                }
            } while(tweetsReturned != 0);




            return searchKeyword + "in text";
        }

    }
}
