using System.Data.Entity;

using SentimentAnalysisApp.SharedModels;

namespace WebSite.Models {

    public class FEMainDBContext: DbContext {
        public FEMainDBContext() : base("name=FEMainDBContext") {
        }

        public DbSet<FESearchRequest> FESearchRequests { get; set; }

        public DbSet<FEExecution> FEExecutions { get; set; }

        public DbSet<Results> Results { get; set; }

    }
}
