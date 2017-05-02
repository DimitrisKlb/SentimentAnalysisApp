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
            TwitterIdOldest = -1;
            TwitterIdNewest = -1;
        }

    }
}