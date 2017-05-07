using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace SentimentAnalysisApp.SharedModels {
    public enum Source {
        Twitter,
        Other
    }

    [DataContract]
    public class BaseMinedText {

        [Key]
        [DataMember]
        public int ID { get; set; }

        [DataMember]
        public string TheText { get; set; }

        [DataMember]
        public Source TheSource { get; set; }     

        [ForeignKey("SearchRequest")]
        [Required]
        [DataMember]
        public int SearchRequestID { get; set; }

        // Navigation property
        public virtual BaseSearchRequest SearchRequest { get; set; }
    }

}