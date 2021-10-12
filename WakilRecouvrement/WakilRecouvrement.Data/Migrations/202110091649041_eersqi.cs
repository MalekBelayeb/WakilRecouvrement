namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class eersqi : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RecuImages", "ImageName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RecuImages", "ImageName");
        }
    }
}
