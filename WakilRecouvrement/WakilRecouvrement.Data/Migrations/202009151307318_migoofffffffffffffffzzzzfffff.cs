namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class migoofffffffffffffffzzzzfffff : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "Message", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "Message");
        }
    }
}
