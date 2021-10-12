namespace WakilRecouvrement.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ssss : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Formulaires", "AgentUsername", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Formulaires", "AgentUsername");
        }
    }
}
