namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class iisddd : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Formulaires", "RecuName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Formulaires", "RecuName");
        }
    }
}
