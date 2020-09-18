namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class poii : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Formulaires", "IsVerified", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Formulaires", "IsVerified");
        }
    }
}
