namespace WebSite.Migrations.Migrations_FEMainDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateUserIdField : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.FESearchRequests", "TheUserID", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.FESearchRequests", "TheUserID", c => c.String(nullable: false));
        }
    }
}
