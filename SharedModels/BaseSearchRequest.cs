using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace SentimentAnalysisApp.SharedModels {

    public class BaseSearchRequest {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string TheSearchKeyword { get; set; }

        // Navigation property
        public virtual ICollection<BaseMinedText> MinedTexts { get; set; }
    }
}