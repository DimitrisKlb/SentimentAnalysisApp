﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using SentimentAnalysisApp.SharedModels;

namespace WSP.Models {

    [DataContract]
    public class TwitterData {

        [Key, ForeignKey( "TheBEExecution" )]
        [DataMember]
        public int ID { get; set; }

        // The IDs used by Twitter, belonging to the oldest and
        // newest tweets (MinedText) mined in a previous execution
        [DataMember]
        public long TheIdOldest { get; set; }

        [DataMember]
        public long TheIdNewest { get; set; }

        [DataMember]
        public int TheTextsNum { get; set; }

        // Navigation property (one-to-oneOrZero) 
        public virtual BEExecution TheBEExecution { get; set; }

        public TwitterData()
            : this( -1 ) {
        }

        public TwitterData(int theBEExecID) {
            ID = theBEExecID;
            TheIdOldest = -1;
            TheIdNewest = -1;
            TheTextsNum = 0;
            TheBEExecution = null;
        }

        public void IncreaseTextsNum(int textsNum) {
            TheTextsNum += textsNum;
        }


    }
}
