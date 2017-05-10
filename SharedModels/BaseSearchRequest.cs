using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace SentimentAnalysisApp.SharedModels {

    [DataContract]
    public class BaseSearchRequest {

        [Key]
        [DataMember]
        public int ID { get; set; }

        [Required]
        [StringLength( 20, MinimumLength = 3 )]
        [DataMember]
        public string TheSearchKeyword { get; set; }

        
        [ForeignKey( "TheSelectedSources" )]
        [Required]
        [DataMember]
        public int MiningSourceID { get; set; }

        [DataMember]
        // Navigation property (one-to-one)
        public MiningSource TheSelectedSources { get; set; } // The desired sources from which to mine texts        


        // Navigation property (one-to-many)
        [DataMember]
        public virtual ICollection<BaseMinedText> MinedTexts { get; set; }

        public BaseSearchRequest() {
            TheSearchKeyword = "";
            TheSelectedSources = new MiningSource();
        }

        public BaseSearchRequest(BaseSearchRequest baseSource) {
            ID = baseSource.ID;
            TheSearchKeyword = baseSource.TheSearchKeyword;
            TheSelectedSources = new MiningSource( baseSource.TheSelectedSources);
        }

    }
}