﻿
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
        public int ID { get; set; }
        public string TheSearchKeyword { get; set; }
        public Status TheStatus { get; set; }
    }
}