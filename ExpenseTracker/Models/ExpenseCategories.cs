using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Models
{
    [Table("ExpenseCategories")]
    public partial class ExpenseCategories : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [StringLength(20)]
        [Required]
        public string CategoryName { get; set; }        
    }
}
