namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migooffff : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserNotifications",
                c => new
                    {
                        UserNotificationId = c.Int(nullable: false, identity: true),
                        EmployeId = c.Int(nullable: false),
                        TokenId = c.String(),
                    })
                .PrimaryKey(t => t.UserNotificationId)
                .ForeignKey("dbo.Employes", t => t.EmployeId, cascadeDelete: true)
                .Index(t => t.EmployeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserNotifications", "EmployeId", "dbo.Employes");
            DropIndex("dbo.UserNotifications", new[] { "EmployeId" });
            DropTable("dbo.UserNotifications");
        }
    }
}
