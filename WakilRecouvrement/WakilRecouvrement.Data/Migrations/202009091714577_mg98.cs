namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mg98 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Affectations",
                c => new
                    {
                        AffectationId = c.Int(nullable: false, identity: true),
                        DateAffectation = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        AffectePar = c.String(),
                        AffecteA = c.String(),
                        LotId = c.Int(nullable: false),
                        EmployeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AffectationId)
                .ForeignKey("dbo.Employes", t => t.EmployeId, cascadeDelete: true)
                .ForeignKey("dbo.Lots", t => t.LotId, cascadeDelete: true)
                .Index(t => t.LotId)
                .Index(t => t.EmployeId);

            
            CreateTable(
                "dbo.Lots",
                c => new
                    {
                        LotId = c.Int(nullable: false, identity: true),
                        NumLot = c.String(),
                        Compte = c.String(),
                        IDClient = c.String(),
                        NomClient = c.String(),
                        TelPortable = c.String(),
                        TelFixe = c.String(),
                        Fax = c.String(),
                        SoldeDebiteur = c.String(),
                        DescIndustry = c.String(),
                        Adresse = c.String(),
                        Type = c.String(),
                        Numero = c.String(),
                        PostCode = c.String(),
                        Employe_EmployeId = c.Int(),
                    })
                .PrimaryKey(t => t.LotId)
                .ForeignKey("dbo.Employes", t => t.Employe_EmployeId)
                .Index(t => t.Employe_EmployeId);
            

            
            CreateTable(
                "dbo.Formulaires",
                c => new
                    {
                        FormulaireId = c.Int(nullable: false, identity: true),
                        EtatClient = c.String(),
                        AffectationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.FormulaireId)
                .ForeignKey("dbo.Affectations", t => t.AffectationId, cascadeDelete: true)
                .Index(t => t.AffectationId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Formulaires", "AffectationId", "dbo.Affectations");
            DropForeignKey("dbo.Affectations", "LotId", "dbo.Lots");
            DropForeignKey("dbo.Affectations", "EmployeId", "dbo.Employes");
            DropForeignKey("dbo.Employes", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.Lots", "Employe_EmployeId", "dbo.Employes");
            DropIndex("dbo.Formulaires", new[] { "AffectationId" });
            DropIndex("dbo.Lots", new[] { "Employe_EmployeId" });
            DropIndex("dbo.Employes", new[] { "RoleId" });
            DropIndex("dbo.Affectations", new[] { "EmployeId" });
            DropIndex("dbo.Affectations", new[] { "LotId" });
            DropTable("dbo.Formulaires");
            DropTable("dbo.Roles");
            DropTable("dbo.Lots");
            DropTable("dbo.Employes");
            DropTable("dbo.Affectations");
        }
    }
}
