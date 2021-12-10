using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Models
{
    public class BaseEntity
    {
        public bool IsDeleted { get; set; }

        [StringLength(64)]
        public string REG_USER_ID { get; set; }

        [StringLength(64)]
        public string UPD_USER_ID { get; set; }
        public DateTime? REG_DATE { get; set; }
        public DateTime? UPD_DATE { get; set; }
    }
}
