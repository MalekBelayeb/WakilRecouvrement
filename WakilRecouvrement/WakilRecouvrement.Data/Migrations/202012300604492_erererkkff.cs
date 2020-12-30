namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class erererkkff : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Factures", "DateDeb", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AddColumn("dbo.Factures", "DateFin", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Factures", "DateFin");
            DropColumn("dbo.Factures", "DateDeb");
        }
    }
}
