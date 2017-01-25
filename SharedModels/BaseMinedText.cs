using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SentimentAnalysisApp.SharedModels {
    public enum Source {
        Twitter,
        Other
    }

    public class BaseMinedText {
        [Key]
        public int ID { get; set; }

        public string TheText { get; set; }

        public Source TheSource { get; set; }

        // The ID used by Twitter. Currently used for debugging
        public long TwitterID { get; set; }

        [ForeignKey("SearchRequest")]
        [Required]
        public int SearchRequestID { get; set; }

        // Navigation property
        public virtual BaseSearchRequest SearchRequest { get; set; }
    }

}