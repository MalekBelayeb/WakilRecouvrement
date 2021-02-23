namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class eeeerdff : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Lots", "Emploi", c => c.String());
            DropColumn("dbo.Lots", "Fax");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Lots", "Fax", c => c.String());
            DropColumn("dbo.Lots", "Emploi");
        }
    }
}
