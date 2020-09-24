namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class poeis : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Formulaires", "Status", c => c.Int(nullable: false));
            DropColumn("dbo.Formulaires", "IsVerified");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Formulaires", "IsVerified", c => c.Boolean(nullable: false));
            DropColumn("dbo.Formulaires", "Status");
        }
    }
}
