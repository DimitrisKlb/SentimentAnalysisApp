using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using SentimentAnalysisApp.SharedModels;

namespace WSP.Models {

    [Table( "BEMinedTexts" )]
    [DataContract]
    public class BEMinedText: BaseMinedText {

        [ForeignKey( "TheExecution" )]
        [Required]
        [DataMember]
        public int ExecutionID { get; set; }


        // Navigation property (one-to-one)
        public virtual BEExecution TheExecution { get; set; }

        public BEMinedText()
            : base() {
        }

    }
}