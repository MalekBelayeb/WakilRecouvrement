namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mpeeed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Formulaires", "VerifieLe", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Formulaires", "VerifieLe");
        }
    }
}
