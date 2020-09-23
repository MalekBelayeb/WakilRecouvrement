namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class iyssssz : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Formulaires", "MontantDebInitial", c => c.Double(nullable: false));
            AddColumn("dbo.Formulaires", "MontantDebMAJ", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Formulaires", "MontantDebMAJ");
            DropColumn("dbo.Formulaires", "MontantDebInitial");
        }
    }
}
