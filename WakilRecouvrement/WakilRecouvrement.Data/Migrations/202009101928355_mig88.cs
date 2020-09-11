namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mig88 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Formulaires", "DescriptionAutre", c => c.String());
            AddColumn("dbo.Formulaires", "DateRDV", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AddColumn("dbo.Formulaires", "DateRDVReporte", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
            AlterColumn("dbo.Formulaires", "EtatClient", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Formulaires", "EtatClient", c => c.String());
            DropColumn("dbo.Formulaires", "DateRDVReporte");
            DropColumn("dbo.Formulaires", "DateRDV");
            DropColumn("dbo.Formulaires", "DescriptionAutre");
        }
    }
}
