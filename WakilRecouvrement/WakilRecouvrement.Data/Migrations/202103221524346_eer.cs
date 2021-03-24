namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class eer : DbMigration
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
                        LotId = c.Int(nullable: false),
                        EmployeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AffectationId)
                .ForeignKey("dbo.Employes", t => t.EmployeId, cascadeDelete: true)
                .ForeignKey("dbo.Lots", t => t.LotId, cascadeDelete: true)
                .Index(t => t.LotId)
                .Index(t => t.EmployeId);
            
            CreateTable(
                "dbo.Employes",
                c => new
                    {
                        EmployeId = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false, maxLength: 25),
                        Password = c.String(nullable: false),
                        RoleId = c.Int(nullable: false),
                        IsVerified = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.EmployeId)
                .ForeignKey("dbo.Roles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.RoleId);
            
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
                        TelPortableFN = c.Boolean(nullable: false),
                        TelFixe = c.String(),
                        TelFixeFN = c.Boolean(nullable: false),
                        Emploi = c.String(),
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
                "dbo.Roles",
                c => new
                    {
                        RoleId = c.Int(nullable: false, identity: true),
                        role = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.RoleId);
            
            CreateTable(
                "dbo.Formulaires",
                c => new
                    {
                        FormulaireId = c.Int(nullable: false, identity: true),
                        EtatClient = c.Int(nullable: false),
                        DescriptionAutre = c.String(),
                        DateRDV = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateRDVReporte = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        TraiteLe = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        VerifieLe = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        RappelLe = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        MontantVerseDeclare = c.Double(nullable: false),
                        MontantDebInitial = c.Double(nullable: false),
                        MontantDebMAJ = c.Double(nullable: false),
                        Status = c.Int(nullable: false),
                        ContacteBanque = c.Boolean(nullable: false),
                        NotifieBanque = c.Boolean(nullable: false),
                        AffectationId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.FormulaireId)
                .ForeignKey("dbo.Affectations", t => t.AffectationId, cascadeDelete: true)
                .Index(t => t.AffectationId);
            
            CreateTable(
                "dbo.Factures",
                c => new
                    {
                        FactureId = c.Int(nullable: false, identity: true),
                        DateExtrait = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateDeb = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        DateFin = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                        FacturePathName = c.String(),
                        AnnexePathName = c.String(),
                    })
                .PrimaryKey(t => t.FactureId);
            
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
            
            CreateTable(
                "dbo.Notifications",
                c => new
                    {
                        NotificationId = c.Int(nullable: false, identity: true),
                        ToSingle = c.String(),
                        ToRole = c.String(),
                        From = c.String(),
                        Status = c.String(),
                        Type = c.String(),
                        Message = c.String(),
                        FormulaireId = c.Int(nullable: false),
                        AddedIn = c.DateTime(nullable: false, precision: 7, storeType: "datetime2"),
                    })
                .PrimaryKey(t => t.NotificationId)
                .ForeignKey("dbo.Formulaires", t => t.FormulaireId, cascadeDelete: true)
                .Index(t => t.FormulaireId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Notifications", "FormulaireId", "dbo.Formulaires");
            DropForeignKey("dbo.Affectations", "LotId", "dbo.Lots");
            DropForeignKey("dbo.Formulaires", "AffectationId", "dbo.Affectations");
            DropForeignKey("dbo.Affectations", "EmployeId", "dbo.Employes");
            DropForeignKey("dbo.Employes", "RoleId", "dbo.Roles");
            DropForeignKey("dbo.Lots", "Employe_EmployeId", "dbo.Employes");
            DropIndex("dbo.Notifications", new[] { "FormulaireId" });
            DropIndex("dbo.Formulaires", new[] { "AffectationId" });
            DropIndex("dbo.Lots", new[] { "Employe_EmployeId" });
            DropIndex("dbo.Employes", new[] { "RoleId" });
            DropIndex("dbo.Affectations", new[] { "EmployeId" });
            DropIndex("dbo.Affectations", new[] { "LotId" });
            DropTable("dbo.Notifications");
            DropTable("dbo.Lettres");
            DropTable("dbo.Factures");
            DropTable("dbo.Formulaires");
            DropTable("dbo.Roles");
            DropTable("dbo.Lots");
            DropTable("dbo.Employes");
            DropTable("dbo.Affectations");
        }
    }
}
