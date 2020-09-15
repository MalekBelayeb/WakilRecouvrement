namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migoofffffffffffffffzzzzfffffffzzz : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Notifications", "EmployeId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Notifications", "EmployeId", c => c.Int(nullable: false));
        }
    }
}
