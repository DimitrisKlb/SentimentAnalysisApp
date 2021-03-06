﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using SentimentAnalysisApp.SharedModels;
using TextAnalysers;

namespace WSP.Models {

    public enum TextStatus: int {
        New,
        BeingProcessed,
        Processed
    }

    [Table( "BEMinedTexts" )]
    [DataContract]
    public class BEMinedText: BaseMinedText {

        [DataMember]
        public TextStatus TheStatus { get; set; }

        [DataMember]
        public SentiClass TheClass { get; set; }     


        [ForeignKey( "TheExecution" )]
        [Required]
        [DataMember]
        public int ExecutionID { get; set; }

        // Navigation property (many-to-one)
        public virtual BEExecution TheExecution { get; set; }

        public BEMinedText()
            : base() {
            TheStatus = TextStatus.New;
            TheClass = SentiClass.Neutral;
        }

    }
}