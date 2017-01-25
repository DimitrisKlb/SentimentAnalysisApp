using System.ComponentModel.DataAnnotations.Schema;

using SentimentAnalysisApp.SharedModels;

namespace WebServiceProvider.Models {

    [Table("BESearchRequests")]
    public class BESearchRequest: BaseSearchRequest {

        // The ID used by the Twitter, belonging to the last
        // tweet (MinedText) mined in a previous execution
        public long TwitterIDLast { get; set; }
    }
}