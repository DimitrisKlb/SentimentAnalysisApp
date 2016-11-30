using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace SentimentAnalysisApp.Models
{
    public enum Status : int
    {
        Open, 
        Pending,
        Fulfilled
    }

    public class SearchRequest
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string TheSearchKeyword { get; set; }

        public Status TheStatus { get; set; }

        // Navigation property
        public virtual ICollection<MinedText> MinedTexts { get; set; }
    }
}