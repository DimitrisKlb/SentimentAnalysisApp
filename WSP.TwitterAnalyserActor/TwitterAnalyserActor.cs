using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

using SentimentAnalysisApp.SharedModels;
using WSP.AnalyserActor.Interfaces;
using WSP.Models;
using WSP.MyActors;
using WSP.DBHandlerService.Interfaces;

namespace WSP.TwitterAnalyserActor {

    [ActorService( Name = "TwitterAnalyserActorService" )]
    [StatePersistence( StatePersistence.Persisted )]
    internal class TwitterAnalyserActor: BaseAnalyserActor, ITwitterAnalyserActor {

        /******************** Fields and Core Methods ********************/
        protected override SourceOption TheSourceID {
            get {
                return SourceOption.Twitter;
            }
        }

        public TwitterAnalyserActor(ActorService actorService, ActorId actorId)
            : base( actorService, actorId ) {
        }

        protected override async Task OnActivateAsync() {
            await base.OnActivateAsync();
            ActorEventSource.Current.ActorMessage( this, "TwitterAnalyserActor {0} activated.", this.Id );
        }

    }
}
