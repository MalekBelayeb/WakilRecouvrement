namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class iysssszhh : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Formulaires", "MontantDebMAJ", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Formulaires", "MontantDebMAJ", c => c.Double(nullable: false));
        }
    }
}
