namespace SentimentAnalysisApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddDataAnnotations2 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.SearchRequests", "TheSearchKeyword", c => c.String(nullable: false, maxLength: 20));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.SearchRequests", "TheSearchKeyword", c => c.String(nullable: false));
        }
    }
}
