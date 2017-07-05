namespace WebSite.Migrations.Migrations_FEMainDB
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateFESearchRequest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.FESearchRequests", "CreatedOn", c => c.DateTime());
            AddColumn("dbo.FESearchRequests", "LatestExecutionID", c => c.Int());
            CreateIndex("dbo.FESearchRequests", "LatestExecutionID");
            AddForeignKey("dbo.FESearchRequests", "LatestExecutionID", "dbo.BaseExecutions", "ID");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.FESearchRequests", "LatestExecutionID", "dbo.BaseExecutions");
            DropIndex("dbo.FESearchRequests", new[] { "LatestExecutionID" });
            DropColumn("dbo.FESearchRequests", "LatestExecutionID");
            DropColumn("dbo.FESearchRequests", "CreatedOn");
        }
    }
}
