namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class errrr : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Notifications", "From", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Notifications", "From");
        }
    }
}
