using System;
using System.Collections.Generic;

using SentimentAnalysisApp.SharedModels;

namespace WSP.MyActors {
    public static class MinerActorsFactory {
        private static string ApplicationName = "fabric:/WebServiceProvider/";

        private static Dictionary<SourceOption, string> minerNames = new Dictionary<SourceOption, string>() {
            { SourceOption.Twitter, "TwitterMinerActorService" }
        };

        public static string GetMinerName(SourceOption minerSourceID) {
            return minerNames[minerSourceID];
        }

        public static Uri GetMinerUri(SourceOption minerSourceID) {
            return new Uri( ApplicationName + GetMinerName( minerSourceID ) );
        }
    }
}
