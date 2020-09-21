namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class iuytreea : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Notifications", "AffectationId", "dbo.Affectations");
            DropIndex("dbo.Notifications", new[] { "AffectationId" });
            AddColumn("dbo.Notifications", "FormulaireId", c => c.Int(nullable: false));
            CreateIndex("dbo.Notifications", "FormulaireId");
            AddForeignKey("dbo.Notifications", "FormulaireId", "dbo.Formulaires", "FormulaireId", cascadeDelete: true);
            DropColumn("dbo.Notifications", "AffectationId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Notifications", "AffectationId", c => c.Int(nullable: false));
            DropForeignKey("dbo.Notifications", "FormulaireId", "dbo.Formulaires");
            DropIndex("dbo.Notifications", new[] { "FormulaireId" });
            DropColumn("dbo.Notifications", "FormulaireId");
            CreateIndex("dbo.Notifications", "AffectationId");
            AddForeignKey("dbo.Notifications", "AffectationId", "dbo.Affectations", "AffectationId", cascadeDelete: true);
        }
    }
}
