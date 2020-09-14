namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mp1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Formulaires", "TraiteLe", c => c.DateTime(nullable: false, precision: 7, storeType: "datetime2"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Formulaires", "TraiteLe");
        }
    }
}
