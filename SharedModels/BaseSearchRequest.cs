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

        [Display( Name = "Word of Interest" )]
        [Required]
        [StringLength( 20, MinimumLength = 3 )]
        [DataMember]
        public string TheSearchKeyword { get; set; }      

        [DataMember]        
        public MiningSource TheSelectedSources { get; set; } // The desired sources from which to mine texts        

        // Navigation property (one-to-many)
        public virtual ICollection<BaseExecution> TheExecutions { get; set; } // The desired sources from which to mine texts        


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