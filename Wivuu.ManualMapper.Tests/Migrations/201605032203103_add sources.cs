namespace Wivuu.ManualMapper.Tests.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addsources : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TestSourceTypes",
                c => new
                    {
                        Value = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Value);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TestSourceTypes");
        }
    }
}
