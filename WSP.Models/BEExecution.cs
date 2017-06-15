using System;
using System.Collections.Generic;

using System.Runtime.Serialization;

using SentimentAnalysisApp.SharedModels;

namespace WSP.Models {

    [DataContract]
    public class BEExecution : BaseExecution{
        [DataMember]
        public DateTime? StartedOn { get; set; }

        [DataMember]
        public DateTime? FinishedOn { get; set; }

        // Navigation property (one-to-many)
        public virtual ICollection<BEMinedText> TheMinedTexts { get; set; }

        // Navigation property (one-to-oneOrZero)       
        public virtual TwitterData TheTwitterData { get; set; }

        public BEExecution(int searchRequestID, DateTime? startedOn, DateTime? finishedOn)
            : base( searchRequestID ) {
            FinishedOn = finishedOn;
            StartedOn = startedOn;
            TheTwitterData = null;
        }

    }
}
