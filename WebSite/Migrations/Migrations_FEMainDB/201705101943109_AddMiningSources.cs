namespace WebSite.Migrations.Migrations_FEMainDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddMiningSources : DbMigration
    {
        public override void Up()
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
            AlterColumn("dbo.BaseMinedTexts", "TheSource", c => c.Short(nullable: false));
            CreateIndex("dbo.BaseSearchRequests", "MiningSourceID");
            AddForeignKey("dbo.BaseSearchRequests", "MiningSourceID", "dbo.MiningSources", "ID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BaseSearchRequests", "MiningSourceID", "dbo.MiningSources");
            DropIndex("dbo.BaseSearchRequests", new[] { "MiningSourceID" });
            AlterColumn("dbo.BaseMinedTexts", "TheSource", c => c.Int(nullable: false));
            DropColumn("dbo.BaseSearchRequests", "MiningSourceID");
            DropTable("dbo.MiningSources");
        }
    }
}
