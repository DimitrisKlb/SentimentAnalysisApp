using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

using SentimentAnalysisApp.SharedModels;

namespace WebSite.Models {

    [DataContract]
    public class FEExecution: BaseExecution {

        [DataMember]
        public DateTime? StartedOn { get; set; }

        [DataMember]
        public DateTime? FinishedOn { get; set; }

        public FEExecution() 
            :base(){
            StartedOn = null;
            FinishedOn = null;
        }

        public FEExecution(int searchRequestID, DateTime? startedOn, DateTime? finishedOn)
            : base( searchRequestID ) {
            FinishedOn = finishedOn;
            StartedOn = startedOn;
        }


    }
}