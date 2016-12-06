namespace SentimentAnalysisApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FieldAddedMinedTextTwitterID : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MinedTexts", "TwitterID", c => c.Long(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.MinedTexts", "TwitterID");
        }
    }
}
