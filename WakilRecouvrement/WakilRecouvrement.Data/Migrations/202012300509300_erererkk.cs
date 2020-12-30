namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class erererkk : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Factures",
                c => new
                    {
                        FactureId = c.Int(nullable: false, identity: true),
                        DateExtrait = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        FacturePathName = c.String(),
                        AnnexePathName = c.String(),
                    })
                .PrimaryKey(t => t.FactureId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Factures");
        }
    }
}
