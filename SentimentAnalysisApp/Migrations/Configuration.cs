using SentimentAnalysisApp.Models;

namespace SentimentAnalysisApp.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<SentimentAnalysisApp.Models.MinedDataContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(MinedDataContext context)
        {
            // Delete all previous data           
            context.Database.ExecuteSqlCommand("TRUNCATE TABLE [MinedTexts]");
            // context.Database.ExecuteSqlCommand("TRUNCATE TABLE [SearchRequests]");

            // Reset auto-increment IDs to 0
            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT('SearchRequests', RESEED, 0)");
            context.Database.ExecuteSqlCommand("DBCC CHECKIDENT('MinedTexts', RESEED, 0)");
            
            /*
            context.SearchRequests.AddOrUpdate(x => x.ID,
                new SearchRequest() { ID = 1, TheSearchKeyword = "open1", TheStatus = Status.Open },
                new SearchRequest() { ID = 2, TheSearchKeyword = "open2", TheStatus = Status.Open },
                new SearchRequest() { ID = 3, TheSearchKeyword = "open3", TheStatus = Status.Open },
                new SearchRequest() { ID = 4, TheSearchKeyword = "Pending cause under process", TheStatus = Status.Pending },
                new SearchRequest() { ID = 5, TheSearchKeyword = "fullfilled1", TheStatus = Status.Fulfilled },
                new SearchRequest() { ID = 6, TheSearchKeyword = "fullfilled2", TheStatus = Status.Fulfilled },
                new SearchRequest() { ID = 7, TheSearchKeyword = "fullfilled3", TheStatus = Status.Fulfilled },
                new SearchRequest() { ID = 8, TheSearchKeyword = "fullfilled4", TheStatus = Status.Fulfilled }
                );

            context.MinedTexts.AddOrUpdate(x => x.ID,
                new MinedText() { ID = 1, TheText = "Just a default text", TheSource = Source.Twitter, SearchRequestID = 6}                
                );
               */
        }
    }
}
