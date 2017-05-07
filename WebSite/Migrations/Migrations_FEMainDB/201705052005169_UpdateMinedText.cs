namespace WebSite.Migrations.Migrations_FEMainDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateMinedText : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.BaseMinedTexts", "TwitterID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.BaseMinedTexts", "TwitterID", c => c.Long(nullable: false));
        }
    }
}
