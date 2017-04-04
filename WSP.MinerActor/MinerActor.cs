using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;

using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Tweetinvi.Exceptions;

using WSP.MinerActor.Interfaces;
using WSP.Models;
using WSP.MasterActor.Interfaces;

namespace WSP.MinerActor {

    /******************** Helper Classes for String definitions ********************/

    static internal class StateNames {
        public const string TheSearchRequest = "theSearchRequest";
    }
    static internal class ReminderNames {
        public const string MineReminder = "Mine";
    }

    /******************** The Actor ********************/

    [StatePersistence(StatePersistence.Persisted)]
    internal class MinerActor: Actor, IMinerActor, IRemindable {

        public MinerActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId) {
            
        }

        protected override Task OnActivateAsync() {
            ActorEventSource.Current.ActorMessage(this, "MinerActor {0} activated.", this.Id);
        
            return this.StateManager.TryAddStateAsync<BESearchRequest>(StateNames.TheSearchRequest, null);
        }

        /******************** State Management Methods ********************/

        private Task<BESearchRequest> GetTheSearchRequest() {
            return this.StateManager.GetStateAsync<BESearchRequest>(StateNames.TheSearchRequest);
        }

        private async Task SaveTheSearchRequest(BESearchRequest theSearchRequest) {
            await this.StateManager.SetStateAsync(StateNames.TheSearchRequest, theSearchRequest);
            await this.SaveStateAsync();
        }       

        /******************** Actor Interface Methods ********************/

        public async Task StartMiningAsync(BESearchRequest searchRequest) {
            // Initialize the SearchRequest in the state manager if this MinerActor is called for the first time      
            if(await GetTheSearchRequest() == null) {
                await SaveTheSearchRequest(searchRequest);
            }

            // Set the Reminder for the method that implements the core logic of the Actor (mainMineAsync)
            try {
                await RegisterReminderAsync(ReminderNames.MineReminder, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
            } catch(Exception) {
                throw;
            }
        }

        /******************** Remider Management Method ********************/

        public async Task ReceiveReminderAsync(string reminderName, byte[] context, TimeSpan duelTIme, TimeSpan period) {
            switch(reminderName) {

                case ReminderNames.MineReminder:
                    await UnregisterReminderAsync(this.GetReminder(ReminderNames.MineReminder));
                    await mainMineAsync();                    

                    IMasterActor theMasterActor = ActorProxy.Create<IMasterActor>(this.Id);
                    await theMasterActor.UpdateSerchRequestStatus(Status.Mining_Done);
                    break;

                default:
                    // This point should never be reached. 
                    throw new InvalidOperationException("Unknown Reminder: " + reminderName);
                    break;
            }
        }

        /******************** Actor Logic Implementation Methods ********************/

        // Basic Twitter miner, to get tweets that contain a certain keyword. Invoked by a Reminder (MineReminder)
        private async Task mainMineAsync() {
            string twitterConsumerKey, twitterConsumerSecret, twitterAccessToken, twitterAccessTokenSecret;
            SearchTweetsParameters searchParameters;
            IEnumerable<ITweet> theTweets;
            ushort windowSize;
            int tweetsReturned, totalTweets;

           BESearchRequest theSearchRequest = await GetTheSearchRequest();

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
            windowSize = 10;
            searchParameters = new SearchTweetsParameters("\"" + theSearchRequest.TheSearchKeyword + "\"") {
                Lang = LanguageFilter.English,
                SearchType = SearchResultType.Recent,
                MaximumNumberOfResults = windowSize
            };
            if(theSearchRequest.TwitterIDLast != -1) {
                searchParameters.MaxId = theSearchRequest.TwitterIDLast;
            }

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

                        // Update the searchRequest object with last Twitter to indicate the additional job done
                        theSearchRequest.TwitterIDLast = theTweets.Last().Id;
                        await SaveTheSearchRequest(theSearchRequest);
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

        }
    }
}
