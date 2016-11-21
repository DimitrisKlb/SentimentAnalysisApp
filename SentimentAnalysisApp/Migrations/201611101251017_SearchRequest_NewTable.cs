namespace SentimentAnalysisApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SearchRequest_NewTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SearchRequests",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TheSearchKeyword = c.String(),
                        TheStatus = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            AddColumn("dbo.MinedTexts", "SearchRequestID", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MinedTexts", "SearchRequestID");
            DropTable("dbo.SearchRequests");
        }
    }
}
