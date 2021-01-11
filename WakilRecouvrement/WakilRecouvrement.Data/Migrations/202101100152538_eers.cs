namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class eers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Lettres",
                c => new
                    {
                        LettreId = c.Int(nullable: false, identity: true),
                        DateExtrait = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateDeb = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateFin = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        LettrePathName = c.String(),
                        LettreAdressPathName = c.String(),
                    })
                .PrimaryKey(t => t.LettreId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Lettres");
        }
    }
}
