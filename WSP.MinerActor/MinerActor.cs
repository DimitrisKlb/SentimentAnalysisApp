using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Fabric;

using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;

using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

using SentimentAnalysisApp.SharedModels;
using WSP.MinerActor.Interfaces;
using WSP.Models;
using WSP.MasterActor.Interfaces;
using WSP.DBHandlerService.Interfaces;

namespace WSP.MinerActor {

    /******************** Helper Classes for String definitions ********************/

    static internal class StateNames {
        public const string TheSearchRequest = "theSearchRequest";
    }
    static internal class ReminderNames {
        public const string MineReminder = "Mine";
    }

    /******************** The Actor ********************/

    [StatePersistence( StatePersistence.Persisted )]
    internal class MinerActor: Actor, IMinerActor, IRemindable {
        private ConfigurationPackage configSettings;
        private IDBHandlerService dbHandlerService;
        private double rateLimitsResetTime;

        public MinerActor(ActorService actorService, ActorId actorId)
            : base( actorService, actorId ) {

        }

        protected override Task OnActivateAsync() {
            ActorEventSource.Current.ActorMessage( this, "MinerActor {0} activated.", this.Id );

            configSettings = ActorService.Context.CodePackageActivationContext.GetConfigurationPackageObject( "Config" );
            dbHandlerService = ServiceProxy.Create<IDBHandlerService>(
                new Uri( configSettings.Settings.Sections["ApplicationServicesNames"].Parameters["DBHandlerName"].Value ),
                new ServicePartitionKey( 1 ) );

            return StateManager.TryAddStateAsync<BESearchRequest>( StateNames.TheSearchRequest, null );
        }

        /******************** State Management Methods ********************/

        private Task<BESearchRequest> GetTheSearchRequest() {
            return StateManager.GetStateAsync<BESearchRequest>( StateNames.TheSearchRequest );
        }

        private async Task SaveTheSearchRequest(BESearchRequest theSearchRequest) {
            await StateManager.SetStateAsync( StateNames.TheSearchRequest, theSearchRequest );
            await SaveStateAsync();
        }

        /******************** Actor Interface Methods ********************/

        public async Task StartMiningAsync(BESearchRequest searchRequest) {
            // Initialize the SearchRequest in the state manager if this MinerActor is called for the first time      
            if(await GetTheSearchRequest() == null) {
                await SaveTheSearchRequest( searchRequest );
            }

            // Set the Reminder for the method that implements the core logic of the Actor (mainMineAsync)
            try {
                await RegisterReminderAsync( ReminderNames.MineReminder, null, TimeSpan.FromSeconds( rateLimitsResetTime ), TimeSpan.FromSeconds( 10 ) );
            } catch(Exception) {
                throw;
            }
        }

        /******************** Remider Management Method ********************/

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan duelTIme, TimeSpan period) {
            switch(reminderName) {

                case ReminderNames.MineReminder:
                    await UnregisterReminderAsync( this.GetReminder( ReminderNames.MineReminder ) );
                    bool done = await mainMineAsync();

                    if(done == true) {
                        IMasterActor theMasterActor = ActorProxy.Create<IMasterActor>( this.Id );
                        await theMasterActor.UpdateSerchRequestStatus( Status.Mining_Done );
                    } else {
                        await RegisterReminderAsync( ReminderNames.MineReminder, null, TimeSpan.FromSeconds( rateLimitsResetTime ), TimeSpan.FromSeconds( 10 ) );
                    }
                    break;

                default:
                    // This point should never be reached. 
                    throw new InvalidOperationException( "Unknown Reminder: " + reminderName );
                    break;
            }
        }

        /******************** Actor Logic Implementation Methods ********************/

        // Basic Twitter miner, to get tweets that contain a certain keyword. Invoked by a Reminder (MineReminder)
        private async Task<bool> mainMineAsync() {
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

            if(theSearchRequest.TwitterIdOldest == -1 && theSearchRequest.TwitterIdNewest == -1) {
                miningToOlder = true;
            } else if(theSearchRequest.TwitterIdOldest != -1) {
                miningToOlder = true;
                searchParameters.MaxId = theSearchRequest.TwitterIdOldest - 1;
            } else {
                miningToOlder = false;
                searchParameters.SinceId = theSearchRequest.TwitterIdNewest;
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
                                             TheSource = SourceOption.Twitter,
                                             SearchRequestID = theSearchRequest.ID
                                         };
                        await dbHandlerService.StoreMinedTexts( minedTexts );


                        // Update the searchRequest object with the oldest-newest mined tweets to indicate the additional job done
                        if(totalTweets == 0) {     // The first time tweets were mined                
                            theSearchRequest.TwitterIdNewest = theTweets.First().Id; // Save the id of the newest tweet
                        }
                        totalTweets += tweetsReturned;

                        // Determine whether the mining should be done newer to older or inverse
                        if(miningToOlder == true) {
                            searchParameters.MaxId = theTweets.Last().Id - 1;
                            theSearchRequest.TwitterIdOldest = theTweets.Last().Id;
                            if(tweetsReturned < windowSize) {
                                miningToOlder = false;
                                searchParameters.MaxId = -1;
                                searchParameters.SinceId = theSearchRequest.TwitterIdNewest;
                            }
                        } else {
                            searchParameters.SinceId = theTweets.First().Id;
                            theSearchRequest.TwitterIdNewest = theTweets.First().Id;
                        }

                        await SaveTheSearchRequest( theSearchRequest );
                    }
                } catch(Exception ex) {
                    return false;
                }
            } while(tweetsReturned != 0);

            return true;
        }
    }
}
