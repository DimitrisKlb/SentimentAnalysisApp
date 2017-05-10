using System.Data.Entity;

using SentimentAnalysisApp.SharedModels;

namespace WebSite.Models {

    public class FEMainDBContext: DbContext {
        public FEMainDBContext() : base("name=FEMainDBContext") {
        }

        public DbSet<FESearchRequest> FESearchRequests { get; set; }

        public DbSet<BaseMinedText> MinedTexts { get; set; }

        public DbSet<MiningSource> MiningSources { get; set; }

    }
}
