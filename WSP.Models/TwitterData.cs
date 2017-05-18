using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using SentimentAnalysisApp.SharedModels;

namespace WSP.Models {

    [DataContract]
    public class TwitterData {

        [Key, ForeignKey( "TheBESearchRequest" )]
        [DataMember]
        public int ID { get; set; }

        // The IDs used by Twitter, belonging to the oldest and
        // newest tweets (MinedText) mined in a previous execution
        [DataMember]
        public long TheIdOldest { get; set; }

        [DataMember]
        public long TheIdNewest { get; set; }

        // Navigation property (one-to-oneOrZero) 
        public virtual BESearchRequest TheBESearchRequest { get; set; }

        public TwitterData(int theBESReqID) {
            ID = theBESReqID;
            TheIdOldest = -1;
            TheIdNewest = -1;
            TheBESearchRequest = null;
        }

    }
}
