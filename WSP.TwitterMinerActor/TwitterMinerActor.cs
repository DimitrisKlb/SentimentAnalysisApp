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

        /************* Helper Classes for String definitions *************/

        protected abstract class NewStateNames: StateNames {
            public const string TheTwitterData = "theTwitterData";
        }

        /******************** Fields and Core Methods ********************/
        protected override SourceOption TheSourceID {
            get {
                return SourceOption.Twitter;
           }
        }         
        private const int minMinedTextsNum = 10; // The minimum number of texts that must be mined before successful return       
        private const int minMinedTextsNumFirstTime = 100;
        private bool miningForFirstTime = false;
        private double rateLimitsResetTime;

        public TwitterMinerActor(ActorService actorService, ActorId actorId)
            : base( actorService, actorId ) {
            rateLimitsResetTime = 0;
        }

        protected override async Task OnActivateAsync() {
            await base.OnActivateAsync();
            ActorEventSource.Current.ActorMessage( this, "TwitterMinerActor {0} activated.", this.Id );
        }

        /******************** State Management Methods ********************/

        protected Task<TwitterData> GetTheTwitterData() {
            return StateManager.GetStateAsync<TwitterData>( NewStateNames.TheTwitterData );
        }

        protected async Task SaveTheTwitterData(TwitterData theTwitterData) {
            await StateManager.SetStateAsync( NewStateNames.TheTwitterData, theTwitterData );
            await SaveStateAsync();
        }

        /******************** Actor Interface Methods ********************/
        
        public override async Task StartMiningAsync(BESearchRequest searchRequest) {
            await base.StartMiningAsync( searchRequest );
        }

        /******************** Actor Logic Implementation Methods ********************/

        // Called before the Miner starts its job (mainMineAsync)
        protected override async Task onMineBeginAsync() {
            // Initialize the TwitterData for this Execution, starting from where the previous execution finished
            BESearchRequest theSearchRequest = await GetTheSearchRequest();            
            TwitterData theTwitterData = await dbHandlerService.GetLatestTwitterData( theSearchRequest.ID );
            if(theTwitterData == null) {
                miningForFirstTime = true;
                theTwitterData = new TwitterData( theSearchRequest.ActiveExecutionID );
            } else {
                miningForFirstTime = false;
                theTwitterData.TheTextsNum = 0;
                theTwitterData.TheExecutionID = theSearchRequest.ActiveExecutionID;
            }
            await SaveTheTwitterData( theTwitterData );
        }
        
        // Register a reminder to rerun mainMineAsync after the rateLimits of Twitter are reset, if that was the reason
        // it failed, or after a certain ammount of time in any other case
        protected override async Task onMineEndAsync() {
            int minedTextsNum = (await GetTheTwitterData()).TheTextsNum;
            int minMinedTextsLimit = minedTextsNum;
            if(miningForFirstTime == true) {
                minMinedTextsLimit = minMinedTextsNumFirstTime;
            }

            if(minedTextsNum > minMinedTextsLimit) { // If the necessary number of texts has been mined
                await RegisterReminderAsync(ReminderNames.MineCompleteReminder);
            } else {    // Else continue the mining of texts
                await RegisterReminderAsync( ReminderNames.MineReminder);              
            }
        }

        // Called after the Miner sucesfully finished its job
        protected override async Task onMineCompleteAsync() {
            // Store the TwitterData in the DB
            await dbHandlerService.StoreTwitterData( await GetTheTwitterData() );
        }

        // Basic Twitter miner, to get tweets that contain a certain keyword. Invoked by a Reminder (MineReminder)
        protected override async Task<bool> mainMineAsync() {
            BESearchRequest theSearchRequest;
            TwitterData theTwitterData;
            bool miningToOlder;

            string twitterConsumerKey, twitterConsumerSecret, twitterAccessToken, twitterAccessTokenSecret;
            SearchTweetsParameters searchParameters;
            IEnumerable<ITweet> theTweets;
            ushort windowSize;
            int tweetsReturned;

            // Get the SearchRequest saved in the Stage Manager
            theSearchRequest = await GetTheSearchRequest();
            theTwitterData = await GetTheTwitterData();

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
            windowSize = 50;
            searchParameters = new SearchTweetsParameters(theSearchRequest.TheSearchKeyword) {
                Lang = LanguageFilter.English,
                SearchType = SearchResultType.Recent,
                MaximumNumberOfResults = windowSize
            };

            if(theTwitterData.TheIdOldest == -1 && theTwitterData.TheIdNewest == -1) {
                miningToOlder = true;
            } else if(theTwitterData.TheIdOldest != -1 && theTwitterData.TheIdNewest == -1) {
                miningToOlder = true;
                searchParameters.MaxId = theTwitterData.TheIdOldest - 1;
            } else {
                miningToOlder = false;
                searchParameters.SinceId = theTwitterData.TheIdNewest;
            }

            // Find relevant tweets iteratively, in windows of a certains size (windowSize)
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
                                             TheSource = TheSourceID,
                                             ExecutionID = theSearchRequest.ActiveExecutionID
                                         };
                        await dbHandlerService.StoreMinedTexts( minedTexts );


                        // Update the the TwitterData with the oldest-newest mined tweets to indicate the additional job done
                        if(theTwitterData.IsNew() == true) {     // The first time tweets were mined                
                            theTwitterData.TheIdNewest = theTweets.First().Id; // Save the id of the newest tweet
                        }
                        theTwitterData.IncreaseTextsNum( tweetsReturned );
                       
                        // Determine whether the mining should be done newer to older or inverse
                        if(miningToOlder == true) {
                            searchParameters.MaxId = theTweets.Last().Id - 1;
                            theTwitterData.TheIdOldest = theTweets.Last().Id;
                            if(tweetsReturned < windowSize) {
                                miningToOlder = false;
                                searchParameters.MaxId = -1;
                                searchParameters.SinceId = theTwitterData.TheIdNewest;
                            }
                        } else {
                            searchParameters.SinceId = theTweets.First().Id;
                            theTwitterData.TheIdNewest = theTweets.First().Id;
                        }

                        await SaveTheSearchRequest( theSearchRequest );
                        await SaveTheTwitterData( theTwitterData );
                    }
                } catch {
                    return false;
                }
            } while(tweetsReturned != 0);

            return true;
        }




    }

}
