namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class diies : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Formulaires", "NotifieBanque", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Formulaires", "NotifieBanque");
        }
    }
}
