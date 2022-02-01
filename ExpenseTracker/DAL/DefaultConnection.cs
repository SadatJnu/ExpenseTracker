namespace ExpenseTracker.Models
{
    using ExpenseTracker.Models;
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity;
    using System.Linq;
    using System.Web;

    public class DefaultConnection : DbContext
    {       
        static DefaultConnection()
        {
            Database.SetInitializer<DefaultConnection>(null);
        }

        public DefaultConnection()
            : base("Data Source=DESKTOP-N4VHHO0;Initial Catalog=ExpenseDB;Persist Security Info=True;User ID=sa;Password=123;MultipleActiveResultSets=True")
        {
        }

        public virtual DbSet<ExpenseCategories> ExpenseCategories { get; set; }
        public virtual DbSet<DailyExpenses> DailyExpenses { get; set; }
    }
}
