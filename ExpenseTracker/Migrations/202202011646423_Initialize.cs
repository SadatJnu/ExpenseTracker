namespace ExpenseTracker.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initialize : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.DailyExpenses",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExpenseCategoryId = c.Int(nullable: false),
                        ExpenseDate = c.DateTime(nullable: false),
                        ExpenseAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        IsDeleted = c.Boolean(nullable: false),
                        REG_USER_ID = c.String(maxLength: 64),
                        UPD_USER_ID = c.String(maxLength: 64),
                        REG_DATE = c.DateTime(),
                        UPD_DATE = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ExpenseCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CategoryName = c.String(nullable: false, maxLength: 20),
                        IsDeleted = c.Boolean(nullable: false),
                        REG_USER_ID = c.String(maxLength: 64),
                        UPD_USER_ID = c.String(maxLength: 64),
                        REG_DATE = c.DateTime(),
                        UPD_DATE = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ExpenseCategories");
            DropTable("dbo.DailyExpenses");
        }
    }
}
