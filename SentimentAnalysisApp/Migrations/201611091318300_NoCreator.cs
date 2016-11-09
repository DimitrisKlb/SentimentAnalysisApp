namespace SentimentAnalysisApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NoCreator : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.MinedTexts", "TheCreator");
        }
        
        public override void Down()
        {
            AddColumn("dbo.MinedTexts", "TheCreator", c => c.String());
        }
    }
}
