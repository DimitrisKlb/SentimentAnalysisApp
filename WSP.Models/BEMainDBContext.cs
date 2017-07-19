using System.Data.Entity;

using SentimentAnalysisApp.SharedModels;

namespace WSP.Models {

    public partial class BEMainDBContext: DbContext {
        public BEMainDBContext() : base( "name=BEMainDBContext" ) {
            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<BESearchRequest> BESearchRequests { get; set; }

        public DbSet<BEExecution> BEExecutions { get; set; }

        public DbSet<BEMinedText> BEMinedTexts { get; set; }

        public DbSet<Results> Results { get; set; }

        public DbSet<MinerData> MinerData { get; set; }
        public DbSet<TwitterData> TwitterData { get; set; }
    }
}
