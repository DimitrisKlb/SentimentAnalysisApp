using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using SentimentAnalysisApp.SharedModels;

namespace WebSite.Models {
    public enum Status: int {
        New,
        Executing,
        Fulfilled
    }

    [Table( "FESearchRequests" )]
    [DataContract]
    public class FESearchRequest: BaseSearchRequest {

        [DataMember]
        public Status TheStatus { get; set; }

        // Each Search Request belongs to a single user (managed and stored by Identity)
        [DataMember]
        public string TheUserID { get; set; }

        [DataMember]
        public DateTime? CreatedOn { get; set; }
        
        [DataMember]
        public DateTime? LastExecutionCreatedOn { get; set; }

        [ForeignKey( "TheLatestExecution" )]
        [DataMember]
        public int? LatestExecutionID { get; set; }

        public virtual FEExecution TheLatestExecution { get; set; }

        public FESearchRequest() : base() {
            CreatedOn = DateTime.Now;
            LatestExecutionID = null;
        }
    }


}
