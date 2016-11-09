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

        protected override void Seed(SentimentAnalysisApp.Models.MinedDataContext context)
        {
            context.MinedTexts.AddOrUpdate(x => x.ID,
                new MinedText() { ID = 1, TheText = "Just a default text", TheSource=Source.Twitter}                
                );
        }
    }
}
