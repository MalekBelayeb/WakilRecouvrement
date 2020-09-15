namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migoofffffffffffffffzzzz : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "EmployeId", c => c.Int(nullable: false));
            AddColumn("dbo.Notifications", "ToSingle", c => c.String());
            AddColumn("dbo.Notifications", "ToRole", c => c.String());
            AddColumn("dbo.Notifications", "AffectationId", c => c.Int(nullable: false));
            CreateIndex("dbo.Notifications", "AffectationId");
            AddForeignKey("dbo.Notifications", "AffectationId", "dbo.Affectations", "AffectationId", cascadeDelete: true);
            DropColumn("dbo.Notifications", "Status");
            DropColumn("dbo.Notifications", "Message");
            DropColumn("dbo.Notifications", "ExtraColumn");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Notifications", "ExtraColumn", c => c.String());
            AddColumn("dbo.Notifications", "Message", c => c.String());
            AddColumn("dbo.Notifications", "Status", c => c.String());
            DropForeignKey("dbo.Notifications", "AffectationId", "dbo.Affectations");
            DropIndex("dbo.Notifications", new[] { "AffectationId" });
            DropColumn("dbo.Notifications", "AffectationId");
            DropColumn("dbo.Notifications", "ToRole");
            DropColumn("dbo.Notifications", "ToSingle");
            DropColumn("dbo.Notifications", "EmployeId");
        }
    }
}
