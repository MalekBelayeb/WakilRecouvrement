namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mpoidsq : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Formulaires", "ContacteBanque", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Formulaires", "ContacteBanque");
        }
    }
}
