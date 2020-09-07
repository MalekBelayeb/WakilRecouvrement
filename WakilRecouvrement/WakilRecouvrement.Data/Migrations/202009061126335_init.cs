namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {

            CreateTable(
                "dbo.Lots",
                c => new
                    {
                        LotId = c.Int(nullable: false, identity: true),
                        NumLot = c.String(),
                        IDClient = c.String(),
                        NomClient = c.String(),
                        TelPortable = c.String(),
                        TelFixe = c.String(),
                        Fax = c.String(),
                        SoldeDebiteur = c.String(),
                        DescIndustry = c.String(),
                        Adresse = c.String(),
                        PostCode = c.String(),
                        EmployeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.LotId)
                .ForeignKey("dbo.Employes", t => t.EmployeId, cascadeDelete: true)
                .Index(t => t.EmployeId);

            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Employes", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.Lots", "EmployeId", "dbo.Employes");
            DropIndex("dbo.Lots", new[] { "EmployeId" });
            DropIndex("dbo.Employes", new[] { "RoleId" });
            DropTable("dbo.Roles");
            DropTable("dbo.Lots");
            DropTable("dbo.Employes");
        }
    }
}
