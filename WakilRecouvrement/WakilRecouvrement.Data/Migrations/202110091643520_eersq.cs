namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class eersq : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Notifications", "FormulaireId", "dbo.Formulaires");
            DropIndex("dbo.Notifications", new[] { "FormulaireId" });
            CreateTable(
                "dbo.RecuImages",
                c => new
                    {
                        RecuImageId = c.Int(nullable: false, identity: true),
                        FormulaireId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.RecuImageId)
                .ForeignKey("dbo.Formulaires", t => t.FormulaireId, cascadeDelete: true)
                .Index(t => t.FormulaireId);
            
            AddColumn("dbo.Lots", "FormulaireId", c => c.Int());
            CreateIndex("dbo.Lots", "FormulaireId");
            AddForeignKey("dbo.Lots", "FormulaireId", "dbo.Formulaires", "FormulaireId");
            DropTable("dbo.Notifications");
        }
        
        public override void Down()
        {
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
                .PrimaryKey(t => t.NotificationId);
            
            DropForeignKey("dbo.Lots", "FormulaireId", "dbo.Formulaires");
            DropForeignKey("dbo.RecuImages", "FormulaireId", "dbo.Formulaires");
            DropIndex("dbo.RecuImages", new[] { "FormulaireId" });
            DropIndex("dbo.Lots", new[] { "FormulaireId" });
            DropColumn("dbo.Lots", "FormulaireId");
            DropTable("dbo.RecuImages");
            CreateIndex("dbo.Notifications", "FormulaireId");
            AddForeignKey("dbo.Notifications", "FormulaireId", "dbo.Formulaires", "FormulaireId", cascadeDelete: true);
        }
    }
}
