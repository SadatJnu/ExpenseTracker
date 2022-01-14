using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker
{
    [Table("DailyExpenses")]
    public partial class DailyExpenses : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ExpenseCategoryId { get; set; }

        [Required]
        public DateTime ExpenseDate { get; set; }

        [Required]
        public Decimal ExpenseAmount { get; set; }
    }
}
