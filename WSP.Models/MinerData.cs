﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using SentimentAnalysisApp.SharedModels;

namespace WSP.Models {

    [Table( "MinerDatas" )]
    [DataContract]
    public class MinerData {

        [Key, ForeignKey( "TheBEExecution" )]
        [Column( Order = 1 )]
        [DataMember]
        public int TheExecutionID { get; set; }

        [Key]
        [Column( Order = 2 )]
        [DataMember]
        public SourceOption TheSource { get; set; }

        [DataMember]
        public int TheTextsNum { get; set; }

        [DataMember]
        public int ThePositivesNum { get; set; }

        [DataMember]
        public int TheNegativesNum { get; set; }


        // Navigation property (many-to-one) 
        public virtual BEExecution TheBEExecution { get; set; }

        public MinerData()
            : this( -1, (SourceOption)(-1) ) {
        }

        public MinerData(int theBEExecID, SourceOption theSource) {
            TheExecutionID = theBEExecID;
            TheSource = theSource;
            TheTextsNum = 0;
            ThePositivesNum = 0;
            TheNegativesNum = 0;
            TheBEExecution = null;
        }

        public MinerData(MinerData source) {
            TheExecutionID = source.TheExecutionID;
            TheSource = source.TheSource;
            TheTextsNum = source.TheTextsNum;
            ThePositivesNum = source.ThePositivesNum;
            TheNegativesNum = source.TheNegativesNum;
            TheBEExecution = null;
        }

        public void IncreaseTextsNum(int textsNum) {
            TheTextsNum += textsNum;
        }


    }
}
