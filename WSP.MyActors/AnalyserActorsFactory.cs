using System;
using System.Collections.Generic;

using SentimentAnalysisApp.SharedModels;

namespace WSP.MyActors {
    public static class AnalyserActorsFactory {
        private static string ApplicationName = "fabric:/WebServiceProvider/";

        private static Dictionary<SourceOption, string> analyserNames = new Dictionary<SourceOption, string>() {
            { SourceOption.Twitter, "TwitterAnalyserActorService" }
        };

        public static string GetAnalyserName(SourceOption analyserSourceID) {
            return analyserNames[analyserSourceID];
        }

        public static Uri GetAnalyserUri(SourceOption analyserSourceID) {
            return new Uri( ApplicationName + GetAnalyserName( analyserSourceID ) );
        }
    }
}
