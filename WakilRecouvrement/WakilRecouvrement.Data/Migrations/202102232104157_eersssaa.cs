namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class eersssaa : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Lots", "Fax", c => c.String());
            DropColumn("dbo.Lots", "Emploi");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Lots", "Emploi", c => c.String());
            DropColumn("dbo.Lots", "Fax");
        }
    }
}
