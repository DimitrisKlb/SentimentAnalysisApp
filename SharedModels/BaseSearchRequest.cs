using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SentimentAnalysisApp.SharedModels {

    [DataContract]
    public class BaseSearchRequest {

        [Key]
        [DataMember]
        public int ID { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        [DataMember]
        public string TheSearchKeyword { get; set; }

        // Navigation property
        [DataMember]
        public virtual ICollection<BaseMinedText> MinedTexts { get; set; }

        public BaseSearchRequest() {
        }

        public BaseSearchRequest(BaseSearchRequest baseSource) {
            ID = baseSource.ID;
            TheSearchKeyword = baseSource.TheSearchKeyword;
        }       

    }
}