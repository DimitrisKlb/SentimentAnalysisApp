using System.Data.Entity;

namespace WSP.Models {

    public partial class BEMainDBContext: DbContext {
        public BEMainDBContext()
            : base( "name=BEMainDBContext" ) {
        }

        public DbSet<BESearchRequest> BESearchRequests { get; set; }

        public DbSet<BEMinedText> BEMinedTexts { get; set; }
    }
}
