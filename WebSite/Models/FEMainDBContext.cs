using System.Data.Entity;

using SentimentAnalysisApp.SharedModels;

namespace WebSite.Models {

    public class FEMainDBContext: DbContext {
        public FEMainDBContext() : base( "name=FEMainDBContext" ) {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<FESearchRequest> FESearchRequests { get; set; }

        public DbSet<FEExecution> FEExecutions { get; set; }

        public DbSet<Results> Results { get; set; }

    }


}
