namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Formulaires", "TrancheSolde", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Formulaires", "TrancheSolde");
        }
    }
}
