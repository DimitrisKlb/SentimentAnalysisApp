namespace WebServiceProvider.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
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
                "dbo.BaseSearchRequests",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TheSearchKeyword = c.String(nullable: false, maxLength: 20),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.BEMinedTexts",
                c => new
                    {
                        ID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.BaseMinedTexts", t => t.ID)
                .Index(t => t.ID);
            
            CreateTable(
                "dbo.BESearchRequests",
                c => new
                    {
                        ID = c.Int(nullable: false),
                        TwitterIDLast = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.BaseSearchRequests", t => t.ID)
                .Index(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BESearchRequests", "ID", "dbo.BaseSearchRequests");
            DropForeignKey("dbo.BEMinedTexts", "ID", "dbo.BaseMinedTexts");
            DropForeignKey("dbo.BaseMinedTexts", "SearchRequestID", "dbo.BaseSearchRequests");
            DropIndex("dbo.BESearchRequests", new[] { "ID" });
            DropIndex("dbo.BEMinedTexts", new[] { "ID" });
            DropIndex("dbo.BaseMinedTexts", new[] { "SearchRequestID" });
            DropTable("dbo.BESearchRequests");
            DropTable("dbo.BEMinedTexts");
            DropTable("dbo.BaseSearchRequests");
            DropTable("dbo.BaseMinedTexts");
        }
    }
}
