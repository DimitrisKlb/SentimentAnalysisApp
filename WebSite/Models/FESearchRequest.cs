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
    }
}