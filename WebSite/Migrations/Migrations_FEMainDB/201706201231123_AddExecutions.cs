namespace WebSite.Migrations.Migrations_FEMainDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExecutions : DbMigration
    {
        public override void Up()
        {
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
            
            CreateTable(
                "dbo.BaseSearchRequests",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TheSearchKeyword = c.String(nullable: false, maxLength: 20),
                        TheSelectedSources_TheSelection = c.Short(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.FESearchRequests",
                c => new
                    {
                        ID = c.Int(nullable: false),
                        TheStatus = c.Int(nullable: false),
                        TheUserID = c.String(),
                        LastExecutionCreatedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.BaseSearchRequests", t => t.ID)
                .Index(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FESearchRequests", "ID", "dbo.BaseSearchRequests");
            DropForeignKey("dbo.Results", "ID", "dbo.BaseExecutions");
            DropForeignKey("dbo.BaseExecutions", "SearchRequestID", "dbo.BaseSearchRequests");
            DropIndex("dbo.FESearchRequests", new[] { "ID" });
            DropIndex("dbo.Results", new[] { "ID" });
            DropIndex("dbo.BaseExecutions", new[] { "SearchRequestID" });
            DropTable("dbo.FESearchRequests");
            DropTable("dbo.BaseSearchRequests");
            DropTable("dbo.Results");
            DropTable("dbo.BaseExecutions");
        }
    }
}
