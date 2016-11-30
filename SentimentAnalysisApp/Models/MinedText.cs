﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SentimentAnalysisApp.Models
{
    public enum Source
    {
        Twitter,
        Other
    }

    public class MinedText
    {
        [Key]
        public int ID { get; set; }
 
        public string TheText { get; set; }

        public Source TheSource { get; set; }

        [ForeignKey("SearchRequest")]
        [Required]
        public int SearchRequestID { get; set; }

        // Navigation property
        public virtual SearchRequest SearchRequest { get; set; }
    }

}