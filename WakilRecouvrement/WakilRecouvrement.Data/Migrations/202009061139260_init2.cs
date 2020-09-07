namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Lots", "Compte", c => c.String());
            AddColumn("dbo.Lots", "Type", c => c.String());
            AddColumn("dbo.Lots", "Numero", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Lots", "Numero");
            DropColumn("dbo.Lots", "Type");
            DropColumn("dbo.Lots", "Compte");
        }
    }
}
