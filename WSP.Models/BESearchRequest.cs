using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using SentimentAnalysisApp.SharedModels;

namespace WSP.Models {
    public enum Status: int {
        New,
        Mining,
        Mining_Done,
        StagesAll_Done,
        Fulfilled
    }

    [Table("BESearchRequests")]
    [DataContract]
    public class BESearchRequest: BaseSearchRequest {

        [DataMember]
        public Status TheStatus { get; set; }

        // The ID used by the Twitter, belonging to the last
        // tweet (MinedText) mined in a previous execution
        [DataMember]
        public long TwitterIDLast { get; set; }

        public BESearchRequest() {
        }

        public BESearchRequest(BaseSearchRequest baseSource)
            : base(baseSource) {
            TheStatus = Status.New;
            TwitterIDLast = -1;
        }

    }
}