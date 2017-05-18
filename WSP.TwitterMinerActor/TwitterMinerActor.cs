using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

using SentimentAnalysisApp.SharedModels;
using WSP.MinerActor.Interfaces;
using WSP.Models;
using WSP.MyActors;
using WSP.DBHandlerService.Interfaces;

namespace WSP.TwitterMinerActor {

    [ActorService( Name = "TwitterMinerActorService" )]
    [StatePersistence( StatePersistence.Persisted )]
    internal class TwitterMinerActor: BaseMinerActor, ITwitterMinerActor {

        /******************** Fields and Core Methods ********************/
        protected override SourceOption MinerSourceID {
            get {
                return SourceOption.Twitter;
            }
        }
        private double rateLimitsResetTime;

        public TwitterMinerActor(ActorService actorService, ActorId actorId)
            : base( actorService, actorId ) {
            rateLimitsResetTime = 0;
        }

        protected override async Task OnActivateAsync() {
            await base.OnActivateAsync();
            ActorEventSource.Current.ActorMessage( this, "TwitterMinerActor {0} activated.", this.Id );
        }

        /******************** Actor Interface Methods ********************/

        // Initialize the TwitterData object and thenc all the inhereted StartMiningAsync method
        public override async Task StartMiningAsync(BESearchRequest searchRequest) {
            searchRequest.TheTwitterData = new TwitterData(searchRequest.ID);
            await base.StartMiningAsync( searchRequest );
        }

        /******************** Actor Logic Implementation Methods ********************/


        // Register a reminder to rerun mainMineAsync after the rateLimits of Twitter are reset, if that was the reason
        // it failed, or after a  certain ammount of time in any other case
        protected override async Task OnMineFailAsync() {
            await RegisterReminderAsync( ReminderNames.MineReminder, null, TimeSpan.FromSeconds( rateLimitsResetTime ), TimeSpan.FromSeconds( 10 ) );
        }

        // Basic Twitter miner, to get tweets that contain a certain keyword. Invoked by a Reminder (MineReminder)
        protected override async Task<bool> mainMineAsync() {
            
            BESearchRequest theSearchRequest;
            bool miningToOlder;

            string twitterConsumerKey, twitterConsumerSecret, twitterAccessToken, twitterAccessTokenSecret;
            SearchTweetsParameters searchParameters;
            IEnumerable<ITweet> theTweets;
            ushort windowSize;
            int tweetsReturned, totalTweets;
            
            // Get the SearchRequest saved in the Stage Manager to show the job needed to be done
            theSearchRequest = await GetTheSearchRequest();
            
            // Disable the exception swallowing to allow exception to be thrown by Tweetinvi
            ExceptionHandler.SwallowWebExceptions = false;

            // Enable RateLimit Tracking
            RateLimit.RateLimitTrackerMode = RateLimitTrackerMode.TrackOnly;

            // Event handler executed before each TwitterAPI call
            TweetinviEvents.QueryBeforeExecute += (sender, args) => {
                var queryRateLimits = RateLimit.GetQueryRateLimit( args.QueryURL );

                // Some methods are not RateLimited. Invoking such a method will result in the queryRateLimits to be null
                if(queryRateLimits != null) {
                    if(queryRateLimits.Remaining > 0) {
                        // We have enough resource to execute the query                        
                        return;
                    }
                    // Different policies to handle rate limit expiry

                    // Wait for RateLimits to be available
                    //Task.Delay((int)queryRateLimits.ResetDateTimeInMilliseconds);

                    // Cancel the Query
                    //args.Cancel = true;

                    // Set the value of the time needed by Twitter to reset the RateLimits
                    // so that it sets a Reminder to rerun after this time
                    rateLimitsResetTime = queryRateLimits.ResetDateTimeInSeconds;
                    args.Cancel = true;

                }
            };

            // Set up your credentials
            var twitterSettings = configSettings.Settings.Sections["TwitterCredentials"];

            twitterConsumerKey = twitterSettings.Parameters["ConsumerKey"].Value;
            twitterConsumerSecret = twitterSettings.Parameters["ConsumerSecret"].Value;
            twitterAccessToken = twitterSettings.Parameters["AccessToken"].Value;
            twitterAccessTokenSecret = twitterSettings.Parameters["AccessTokenSecret"].Value;

            Auth.SetUserCredentials( twitterConsumerKey, twitterConsumerSecret, twitterAccessToken, twitterAccessTokenSecret );

            // Basic Search Parameters 
            windowSize = 10;
            searchParameters = new SearchTweetsParameters( "\"" + theSearchRequest.TheSearchKeyword + "\"" ) {
                Lang = LanguageFilter.English,
                SearchType = SearchResultType.Recent,
                MaximumNumberOfResults = windowSize
            };

            if(theSearchRequest.TheTwitterData.TheIdOldest == -1 && theSearchRequest.TheTwitterData.TheIdNewest == -1) {
                miningToOlder = true;
            } else if(theSearchRequest.TheTwitterData.TheIdOldest != -1) {
                miningToOlder = true;
                searchParameters.MaxId = theSearchRequest.TheTwitterData.TheIdOldest - 1;
            } else {
                miningToOlder = false;
                searchParameters.SinceId = theSearchRequest.TheTwitterData.TheIdNewest;
            }

            // Find relevant tweets iteratively, in windows of a certains size (windowSize)
            totalTweets = 0;
            tweetsReturned = windowSize;
            theTweets = null;
            do {
                try {
                    theTweets = Search.SearchTweets( searchParameters );
                    if(theTweets == null) {
                        throw new Exception();
                    }

                    tweetsReturned = theTweets.Count();
                    if(tweetsReturned != 0) {

                        // Store tweets in the database
                        var minedTexts = from t in theTweets
                                         select new BEMinedText() {
                                             TheText = t.FullText,
                                             TheSource = MinerSourceID,
                                             SearchRequestID = theSearchRequest.ID
                                         };
                        await dbHandlerService.StoreMinedTexts( minedTexts );


                        // Update the the TwitterData with the oldest-newest mined tweets to indicate the additional job done
                        if(totalTweets == 0) {     // The first time tweets were mined                
                            theSearchRequest.TheTwitterData.TheIdNewest = theTweets.First().Id; // Save the id of the newest tweet
                        }
                        totalTweets += tweetsReturned;

                        // Determine whether the mining should be done newer to older or inverse
                        if(miningToOlder == true) {
                            searchParameters.MaxId = theTweets.Last().Id - 1;
                            theSearchRequest.TheTwitterData.TheIdOldest = theTweets.Last().Id;
                            if(tweetsReturned < windowSize) {
                                miningToOlder = false;
                                searchParameters.MaxId = -1;
                                searchParameters.SinceId = theSearchRequest.TheTwitterData.TheIdNewest;
                            }
                        } else {
                            searchParameters.SinceId = theTweets.First().Id;
                            theSearchRequest.TheTwitterData.TheIdNewest = theTweets.First().Id;
                        }

                        await SaveTheSearchRequest( theSearchRequest );
                    }
                } catch {
                    return false;
                }
            } while(tweetsReturned != 0);
            
            // Store the TwitterData in the db
            await dbHandlerService.StoreTwitterData( theSearchRequest.TheTwitterData );
            return true;
        }

    }

}
