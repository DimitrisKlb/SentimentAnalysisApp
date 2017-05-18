using System.Data.Entity;

using SentimentAnalysisApp.SharedModels;

namespace WSP.Models {

    public partial class BEMainDBContext: DbContext {
        public BEMainDBContext()
            : base( "name=BEMainDBContext" ) {
        }

        public DbSet<BESearchRequest> BESearchRequests { get; set; }

        public DbSet<BEMinedText> BEMinedTexts { get; set; }

        public DbSet<MiningSource> MiningSources { get; set; }

        public DbSet<TwitterData> TwitterData { get; set; }
    }
}
