namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class miggr : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Lots", "TelPortableFN", c => c.Boolean(nullable: false));
            AddColumn("dbo.Lots", "TelFixeFN", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Lots", "TelFixeFN");
            DropColumn("dbo.Lots", "TelPortableFN");
        }
    }
}
