namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class iisdddsq : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Formulaires", "RecuName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Formulaires", "RecuName", c => c.String());
        }
    }
}
