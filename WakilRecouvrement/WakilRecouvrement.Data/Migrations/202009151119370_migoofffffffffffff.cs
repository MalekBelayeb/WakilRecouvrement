namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migoofffffffffffff : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserNotifications", "EmployeId", "dbo.Employes");
            DropIndex("dbo.UserNotifications", new[] { "EmployeId" });
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        NotificationId = c.Int(nullable: false, identity: true),
                        Status = c.String(),
                        Message = c.String(),
                        ExtraColumn = c.String(),
                    })
                .PrimaryKey(t => t.NotificationId);
            
            DropTable("dbo.UserNotifications");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.UserNotifications",
                c => new
                    {
                        UserNotificationId = c.Int(nullable: false, identity: true),
                        EmployeId = c.Int(nullable: false),
                        TokenId = c.String(),
                    })
                .PrimaryKey(t => t.UserNotificationId);
            
            DropTable("dbo.Notifications");
            CreateIndex("dbo.UserNotifications", "EmployeId");
            AddForeignKey("dbo.UserNotifications", "EmployeId", "dbo.Employes", "EmployeId", cascadeDelete: true);
        }
    }
}
