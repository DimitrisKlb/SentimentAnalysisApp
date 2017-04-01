using WSP.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Tweetinvi.Exceptions;

using SentimentAnalysisApp.SharedModels;
using WSP.MinerActor.Interfaces;
using System.Linq;
using System;

namespace WSP.MinerActor {
    static internal class StateNames {
        public const string TheSearchRequest = "theSearchRequest";
    }

    [StatePersistence(StatePersistence.Persisted)]
    internal class MinerActor: Actor, IMinerActor {

        public MinerActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId) {
        }

        protected override Task OnActivateAsync() {
            ActorEventSource.Current.ActorMessage(this, "MinerActor {0} activated.", this.Id);

            return this.StateManager.TryAddStateAsync<BESearchRequest>(StateNames.TheSearchRequest, null);
        }

        private Task SetTheSearchRequest(BESearchRequest theSearchRequest) {
            return this.StateManager.SetStateAsync(StateNames.TheSearchRequest, theSearchRequest);
        }
        private Task<BESearchRequest> GetTheSearchRequest() {
            return this.StateManager.GetStateAsync<BESearchRequest>(StateNames.TheSearchRequest);
        }


        // Basic Twitter miner, to get tweets that contain searchKeyword
        public async Task<int> MineAsync(BESearchRequest searchRequest) {
            await SetTheSearchRequest(searchRequest);

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
            var configPackage = ActorService.Context.CodePackageActivationContext.
                                GetConfigurationPackageObject("Config").Settings.Sections["TwitterCredentials"];

            twitterConsumerKey = configPackage.Parameters["ConsumerKey"].Value;
            twitterConsumerSecret = configPackage.Parameters["ConsumerSecret"].Value;
            twitterAccessToken = configPackage.Parameters["AccessToken"].Value;
            twitterAccessTokenSecret = configPackage.Parameters["AccessTokenSecret"].Value;

            Auth.SetUserCredentials(twitterConsumerKey, twitterConsumerSecret, twitterAccessToken, twitterAccessTokenSecret);

            // Basic Search Parameters 
            windowSize = 100;
            searchParameters = new SearchTweetsParameters("\"" + searchRequest.TheSearchKeyword + "\"") {
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
                        /*
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
                        */
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


            return tweetsReturned;
        }
    }
}
