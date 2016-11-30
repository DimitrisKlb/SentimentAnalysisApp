namespace SentimentAnalysisApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDataAnnotations : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SearchRequests", "TheSearchKeyword", c => c.String(nullable: false));
            CreateIndex("dbo.MinedTexts", "SearchRequestID");
            AddForeignKey("dbo.MinedTexts", "SearchRequestID", "dbo.SearchRequests", "ID", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MinedTexts", "SearchRequestID", "dbo.SearchRequests");
            DropIndex("dbo.MinedTexts", new[] { "SearchRequestID" });
            AlterColumn("dbo.SearchRequests", "TheSearchKeyword", c => c.String());
        }
    }
}
