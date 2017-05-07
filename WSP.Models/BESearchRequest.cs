using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using SentimentAnalysisApp.SharedModels;

namespace WSP.Models {

    public enum Status: int {
        New,
        Mining,
        Mining_Done,
        Fulfilled
    }

    [Table("BESearchRequests")]
    [DataContract]
    public class BESearchRequest: BaseSearchRequest {

        [DataMember]
        public Status TheStatus { get; set; }

        // The ID of the BaseSearchRequest object, 
        // that was received by whoever called the API and submited it
        [Required]
        [DataMember]        
        public int TheReceivedID { get; set; }

        // The IDs used by the Twitter, belonging to the oldest and
        // newest tweets (MinedText) mined in a previous execution
        [DataMember]
        public long TwitterIdOldest { get; set; }

        [DataMember]
        public long TwitterIdNewest { get; set; }

        public BESearchRequest() {
        }

        public BESearchRequest(BaseSearchRequest baseSource)
            : base(baseSource) {
            TheStatus = Status.New;
            TheReceivedID = baseSource.ID;
            TwitterIdOldest = -1;
            TwitterIdNewest = -1;
        }

        public BaseSearchRequest GetReceivedSearchRequest() {
            BaseSearchRequest receivedSReq = (BaseSearchRequest)this;
            receivedSReq.ID = TheReceivedID;
            return receivedSReq;
        }

    }
}