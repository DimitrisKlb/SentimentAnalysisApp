using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace SentimentAnalysisApp.SharedModels {

    [DataContract]
    public class BaseExecution {

        [Key]
        [DataMember]
        public int ID { get; set; }

        [ForeignKey( "TheSearchRequest" )]
        [Required]
        [DataMember]
        public int SearchRequestID { get; set; }


        // Navigation property (one-to-one)
        public virtual BaseSearchRequest TheSearchRequest { get; set; }

        // Navigation property (one-to-oneOrZero) 
        [DataMember]
        public Results TheResults { get; set; }

        public BaseExecution() {
            SearchRequestID = -1;
            TheSearchRequest = null;
        }

        public BaseExecution(int searchRequestID) {
            SearchRequestID = searchRequestID;
            TheSearchRequest = null;
        }

    }
}
