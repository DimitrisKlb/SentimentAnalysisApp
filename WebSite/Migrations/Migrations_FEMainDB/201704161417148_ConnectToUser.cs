namespace WebSite.Migrations.Migrations_FEMainDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConnectToUser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FESearchRequests", "TheUserID", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.FESearchRequests", "TheUserID");
        }
    }
}
