using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.ModelView
{
    public class ExpenseVM
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }

        public int ExpenseCategoryId { get; set; }
        public string ExpenseDate { get; set; }
        public string ExpenseAmount { get; set; }

        public int TotalRow { get; set; }
    }
}
