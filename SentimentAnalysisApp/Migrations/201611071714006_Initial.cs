namespace SentimentAnalysisApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MinedTexts",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        TheText = c.String(),
                        TheCreator = c.String(),
                        TheSource = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.MinedTexts");
        }
    }
}
