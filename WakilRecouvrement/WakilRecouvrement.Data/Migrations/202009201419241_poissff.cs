namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class poissff : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "Status", c => c.String());
            AddColumn("dbo.Notifications", "Type", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "Type");
            DropColumn("dbo.Notifications", "Status");
        }
    }
}
