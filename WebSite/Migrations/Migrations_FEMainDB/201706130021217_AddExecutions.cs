namespace WebSite.Migrations.Migrations_FEMainDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExecutions : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.BaseMinedTexts", "SearchRequestID", "dbo.BaseSearchRequests");
            DropIndex("dbo.BaseMinedTexts", new[] { "SearchRequestID" });
            CreateTable(
                "dbo.BaseExecutions",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        SearchRequestID = c.Int(nullable: false),
                        StartedOn = c.DateTime(),
                        FinishedOn = c.DateTime(),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.BaseSearchRequests", t => t.SearchRequestID, cascadeDelete: true)
                .Index(t => t.SearchRequestID);
            
            CreateTable(
                "dbo.Results",
                c => new
                    {
                        ID = c.Int(nullable: false),
                        ThePositiveScore = c.Single(nullable: false),
                        TheNegativeScore = c.Single(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.BaseExecutions", t => t.ID)
                .Index(t => t.ID);
            
            AddColumn("dbo.FESearchRequests", "LastExecutionCreatedOn", c => c.DateTime());
            DropTable("dbo.BaseMinedTexts");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.BaseMinedTexts",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TheText = c.String(),
                        TheSource = c.Short(nullable: false),
                        SearchRequestID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            DropForeignKey("dbo.Results", "ID", "dbo.BaseExecutions");
            DropForeignKey("dbo.BaseExecutions", "SearchRequestID", "dbo.BaseSearchRequests");
            DropIndex("dbo.Results", new[] { "ID" });
            DropIndex("dbo.BaseExecutions", new[] { "SearchRequestID" });
            DropColumn("dbo.FESearchRequests", "LastExecutionCreatedOn");
            DropTable("dbo.Results");
            DropTable("dbo.BaseExecutions");
            CreateIndex("dbo.BaseMinedTexts", "SearchRequestID");
            AddForeignKey("dbo.BaseMinedTexts", "SearchRequestID", "dbo.BaseSearchRequests", "ID", cascadeDelete: true);
        }
    }
}
