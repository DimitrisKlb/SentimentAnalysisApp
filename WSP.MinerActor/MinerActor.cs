using System.Threading;
using System.Threading.Tasks;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Tweetinvi.Exceptions;

using WSP.MinerActor.Interfaces;

namespace WSP.MinerActor {
    
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    [StatePersistence(StatePersistence.Persisted)]
    internal class MinerActor: Actor, IMinerActor {

        public MinerActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId) {
        }

        //This method is called whenever an actor is activated.
        // An actor is activated the first time any of its methods are invoked.
        protected override Task OnActivateAsync() {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.
            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization

            return this.StateManager.TryAddStateAsync("count", 0);
        }

        // Basic Twitter miner, to get tweets that contain searchKeyword
        public async Task<int> MineAsync(string searchKeyword, int searchRequestID) {
            /*
            string twitterConsumerKey, twitterConsumerSecret, twitterAccessToken, twitterAccessTokenSecret;
            SearchTweetsParameters searchParameters;
            IEnumerable<ITweet> theTweets;
            ushort windowSize;
            int tweetsReturned, totalTweets;

            // Disable the exception swallowing to allow exception to be thrown by Tweetinvi
            ExceptionHandler.SwallowWebExceptions = false;

            // Enable RateLimit Tracking
            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackOnly;

            // Event handler executed before each TwitterAPI call
            TweetinviEvents.QueryBeforeExecute += (sender, args) => {
                var queryRateLimits = RateLimit.GetQueryRateLimit(args.QueryURL);

                // Some methods are not RateLimited. Invoking such a method will result in the queryRateLimits to be null
                if(queryRateLimits != null) {
                    if(queryRateLimits.Remaining > 0) {
                        // We have enough resource to execute the query
                        return;
                    }

                    // Wait for RateLimits to be available
                    //Thread.Sleep((int)queryRateLimits.ResetDateTimeInMilliseconds);

                    // Cancel Query
                    args.Cancel = true;
                }
            };

            // Set up your credentials
            //twitterConsumerKey = WebConfigurationManager.AppSettings["twitterConsumerKey"];
            //twitterConsumerSecret = WebConfigurationManager.AppSettings["twitterConsumerSecret"];
            //twitterAccessToken = WebConfigurationManager.AppSettings["twitterAccessToken"];
            //twitterAccessTokenSecret = WebConfigurationManager.AppSettings["twitterAccessTokenSecret"];

            //var storageConfig = ActorService.Context.CodePackageActivationContext.GetConfigurationPackageObject("Config");
            //var x = storageConfig.Settings.Sections["TwitterCredentials"].Parameters["twitterConsumerKey"].Value;


            //Auth.SetUserCredentials(twitterConsumerKey, twitterConsumerSecret, twitterAccessToken, twitterAccessTokenSecret);

            // Basic Search Parameters 
            windowSize = 1000;
            searchParameters = new SearchTweetsParameters("\"" + searchKeyword + "\"") {
                Lang = LanguageFilter.English,
                SearchType = SearchResultType.Recent,
                MaximumNumberOfResults = windowSize
            };

            // Find relevant tweets iteratively, in windows of a certains size (windowSize)
            totalTweets = 0;
            tweetsReturned = windowSize;
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
                } catch(ArgumentException ex) {
                    var msg = ex.Message;
                    break;
                } catch(TwitterException ex) {
                    var msg = ex.Message;
                    var msg2 = ex.TwitterDescription;
                    var msg3 = ex.TwitterExceptionInfos;
                    break;
                } catch(Exception ex) {
                    var msg = ex.Message;
                    break;
                }
            } while(tweetsReturned != 0);
            */

            Thread.Sleep(10 * 1000);
            return 100;
        }
    }
}
