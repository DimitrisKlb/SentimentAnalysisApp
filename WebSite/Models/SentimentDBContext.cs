using System.Data.Entity;

using SentimentAnalysisApp.SharedModels;

namespace WebSite.Models {
    public class SentimentDBContext: DbContext {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx

        public SentimentDBContext() : base("name=SentimentDBContext") {
        }

        public DbSet<FESearchRequest> FESearchRequests { get; set; }

        public DbSet<BaseMinedText> MinedTexts { get; set; }
    }
}
