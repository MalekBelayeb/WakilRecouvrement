namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class eeer : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Formulaires", "MontantDebMAJ", c => c.Double(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Formulaires", "MontantDebMAJ", c => c.String());
        }
    }
}
