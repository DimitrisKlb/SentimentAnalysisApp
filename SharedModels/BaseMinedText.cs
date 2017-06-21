using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace SentimentAnalysisApp.SharedModels {
 
    [DataContract]
    public class BaseMinedText {

        [Key]
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string TheText { get; set; }

        [DataMember]
        public SourceOption TheSource { get; set; } 


        public BaseMinedText() {
            ID = 0;
        }

    }

}