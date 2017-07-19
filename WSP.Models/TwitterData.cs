using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using SentimentAnalysisApp.SharedModels;

namespace WSP.Models {
    
    [Table( "TwitterDatas" )]
    [DataContract]
    public class TwitterData : MinerData{

        // The IDs used by Twitter, belonging to the oldest and
        // newest tweets (MinedText) mined in a previous execution
        [DataMember]
        public long TheIdOldest { get; set; }

        [DataMember]
        public long TheIdNewest { get; set; }

        public TwitterData()
            : this( -1 ) {
        }

        public TwitterData(int theBEExecID) 
            :base(theBEExecID, SourceOption.Twitter){
            TheIdOldest = -1;
            TheIdNewest = -1;
        }

        public bool IsNew() {
            if( TheTextsNum == 0 && TheIdOldest == -1 && TheIdNewest == -1) {
                return true;
            }
            return false;
        }

    }
}
