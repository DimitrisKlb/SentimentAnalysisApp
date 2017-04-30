using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SentimentAnalysisApp.SharedModels;

namespace WebSite.Models {
    public enum Status: int {
        Open,
        Pending,
        Fulfilled
    }

    [Table("FESearchRequests")]
    public class FESearchRequest: BaseSearchRequest {
        public Status TheStatus { get; set; }

        // Each Search Request belongs to a single user (managed and stored by Identity)
        public string TheUserID { get; set; }
    }
}