using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using SentimentAnalysisApp.SharedModels;

namespace WSP.Models {

    [Table("BEMinedTexts")]
    [DataContract]
    public class BEMinedText: BaseMinedText {

    }
}