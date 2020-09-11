namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mg9 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Affectations", "AffecteA");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Affectations", "AffecteA", c => c.String());
        }
    }
}
