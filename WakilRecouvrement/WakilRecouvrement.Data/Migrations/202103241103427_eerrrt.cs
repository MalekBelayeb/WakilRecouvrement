namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class eerrrt : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Affectations", "AncienAgent", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Affectations", "AncienAgent");
        }
    }
}
