namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class iuytr : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "AddedIn", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "AddedIn");
        }
    }
}
