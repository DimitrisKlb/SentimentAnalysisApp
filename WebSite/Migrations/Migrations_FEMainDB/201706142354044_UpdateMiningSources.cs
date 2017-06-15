namespace WebSite.Migrations.Migrations_FEMainDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateMiningSources : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.BaseSearchRequests", "MiningSourceID", "dbo.MiningSources");
            DropIndex("dbo.BaseSearchRequests", new[] { "MiningSourceID" });
            AddColumn("dbo.BaseSearchRequests", "TheSelectedSources_TheSelection", c => c.Short(nullable: false));
            DropColumn("dbo.BaseSearchRequests", "MiningSourceID");
            DropTable("dbo.MiningSources");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.MiningSources",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TheSelection = c.Short(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.BaseSearchRequests", "MiningSourceID", c => c.Int(nullable: false));
            DropColumn("dbo.BaseSearchRequests", "TheSelectedSources_TheSelection");
            CreateIndex("dbo.BaseSearchRequests", "MiningSourceID");
            AddForeignKey("dbo.BaseSearchRequests", "MiningSourceID", "dbo.MiningSources", "ID", cascadeDelete: true);
        }
    }
}
