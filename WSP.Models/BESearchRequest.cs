using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using SentimentAnalysisApp.SharedModels;
using System.Collections.Generic;

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

        // The ID of the last execution
        [DataMember]
        public int LastExecutionID { get; set; }
                
        public BESearchRequest() {
        }

        public BESearchRequest(BaseSearchRequest baseSource)
            : base(baseSource) {
            TheStatus = Status.New;
            TheReceivedID = baseSource.ID;            
        }

        public BaseSearchRequest GetReceivedSearchRequest() {
            BaseSearchRequest receivedSReq = (BaseSearchRequest)this;
            receivedSReq.ID = TheReceivedID;
            return receivedSReq;
        }

    }
}