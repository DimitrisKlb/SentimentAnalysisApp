namespace WebSite.Migrations.Migrations_FEMainDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial_FEMainDB : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BaseSearchRequests",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TheSearchKeyword = c.String(nullable: false, maxLength: 20),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.BaseMinedTexts",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TheText = c.String(),
                        TheSource = c.Int(nullable: false),
                        TwitterID = c.Long(nullable: false),
                        SearchRequestID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.BaseSearchRequests", t => t.SearchRequestID, cascadeDelete: true)
                .Index(t => t.SearchRequestID);
            
            CreateTable(
                "dbo.FESearchRequests",
                c => new
                    {
                        ID = c.Int(nullable: false),
                        TheStatus = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.BaseSearchRequests", t => t.ID)
                .Index(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FESearchRequests", "ID", "dbo.BaseSearchRequests");
            DropForeignKey("dbo.BaseMinedTexts", "SearchRequestID", "dbo.BaseSearchRequests");
            DropIndex("dbo.FESearchRequests", new[] { "ID" });
            DropIndex("dbo.BaseMinedTexts", new[] { "SearchRequestID" });
            DropTable("dbo.FESearchRequests");
            DropTable("dbo.BaseMinedTexts");
            DropTable("dbo.BaseSearchRequests");
        }
    }
}
