using System;
using System.Data.Entity;

namespace SentimentAnalysisApp.Models
{
    public enum Source
    {
        Twitter,
        Other
    }

    public class MinedText
    {
        public int ID { get; set; }
        public string TheText { get; set; }
        public Source TheSource { get; set; }

        // Foreign key, pointing to the one SearchRequest that each MinedText belongs to
        public int SearchRequestID { get; set; }
        // Navigation property
        public SearchRequest TheSearchRequest;
    }

}