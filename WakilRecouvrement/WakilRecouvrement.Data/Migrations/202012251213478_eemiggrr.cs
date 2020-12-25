namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class eemiggrr : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Formulaires", "RappelLe", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Formulaires", "RappelLe");
        }
    }
}
