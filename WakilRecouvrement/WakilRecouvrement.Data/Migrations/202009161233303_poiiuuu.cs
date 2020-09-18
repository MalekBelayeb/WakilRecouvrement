namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class poiiuuu : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Formulaires", "MontantVerseDeclare", c => c.Double(nullable: false));
            DropColumn("dbo.Formulaires", "TrancheSolde");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Formulaires", "TrancheSolde", c => c.Double(nullable: false));
            DropColumn("dbo.Formulaires", "MontantVerseDeclare");
        }
    }
}
