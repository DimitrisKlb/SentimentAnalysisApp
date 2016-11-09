using System;
using System.Data.Entity;

namespace SentimentAnalysisApp.Models
{
    public enum Source
    {
        Twitter, Other
    }

    public class MinedText
    {
        public int ID { get; set; }
        public string TheText { get; set; }
        public Source TheSource { get; set; }

    }

}