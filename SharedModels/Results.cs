using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace SentimentAnalysisApp.SharedModels {

    [DataContract]
    public class Results {

        [Key, ForeignKey( "TheExecution" )]
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public float ThePositiveScore { get; set; }

        [DataMember]
        public float TheNegativeScore { get; set; }


        // Navigation property (one-to-oneOrZero) 
        public virtual BaseExecution TheExecution { get; set; }

        public Results() {
            ThePositiveScore = 0;
            TheNegativeScore = 0;
        }

        public Results(float thePositiveScore, float theNegativeScore) {
            ThePositiveScore = thePositiveScore;
            TheNegativeScore = theNegativeScore;
        }
    }
}
