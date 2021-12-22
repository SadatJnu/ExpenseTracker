using AspNetCore.Reporting;
using ExpenseTracker.Models;
using ExpenseTracker.ModelView;
using ExpenseTracker.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ExpenseTracker.Controllers
{
    public class HomeController : Controller
    {
        DefaultConnection db = new DefaultConnection();
        private IConfiguration Configuration;
        public HomeController(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        #region ExpenseDataList

        [HttpGet]
        public IActionResult Index()
        {
            ConnectionStrings.connString = this.Configuration.GetConnectionString("DefaultConnection");
            return View();
        }

        [HttpGet]
        public IActionResult GetExpenseDataList(string pageSize, string pageNo)
        {
            try
            {
                DataSet DS = new DataSet();
                DS = SqlHelper.ExecuteDataset(ConnectionStrings.connString, CommandType.Text, @"EXEC SP_Get_ExpenseCategory_Data_List  @PageSize = '" + pageSize + "', @PageNo = '" + pageNo + "' ");
                
                List<ExpenseVM> lst = new List<ExpenseVM>();

                foreach (DataRow dr in DS.Tables[0].Rows)
                {
                    lst.Add(
                    new ExpenseVM
                    {
                        Id = Convert.ToInt32(dr["Id"]),
                        CategoryName = dr["CategoryName"].ToString(),
                        TotalRow = Convert.ToInt32(dr["TotalRow"]),
                    });
                }
                var events = lst.ToArray();

                return Json(events);
            }
            catch (Exception ex)
            {
                return Json(new { Status = "ERROR", ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Add_Update_ExpenseCategory(ExpenseCategories expCat)
        {
            try
            {              
                var data = db.ExpenseCategories.Where(x => x.Id == expCat.Id && x.IsDeleted == false).FirstOrDefault();

                expCat.CategoryName = expCat.CategoryName.Trim();

                if (data != null)
                {
                    data.CategoryName = expCat.CategoryName;
                    data.UPD_USER_ID = HttpContext.Connection.LocalPort.ToString();
                    data.UPD_DATE = DateTime.Now;
                }
                else
                {
                    var validRow = db.ExpenseCategories.Where(c => c.CategoryName == expCat.CategoryName && c.IsDeleted == false).FirstOrDefault();
                    if (validRow == null)
                    {
                        ExpenseCategories expense = new ExpenseCategories();

                        expense.CategoryName = expCat.CategoryName;
                        expense.IsDeleted = false;
                        expense.REG_USER_ID = HttpContext.Connection.LocalPort.ToString();
                        expense.REG_DATE = DateTime.Now;
                        db.ExpenseCategories.Add(expense);
                    }
                    else
                    {
                        return Json(new { Status = "Already Exist!" });
                    }
                }

                db.SaveChanges();

                return Json(new { Status = "success", Message = "Successfull." });
            }
            catch (Exception ex)
            {
                return Json(new { Status = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult CategoryNameEdit(int id)
        {
            try
            {
                DataSet DS = new DataSet();
                DS = SqlHelper.ExecuteDataset(ConnectionStrings.connString, CommandType.Text, @"SELECT Id,CategoryName FROM ExpenseCategories WHERE  Id =  '" + id + "'");

                List<ExpenseVM> lst = new List<ExpenseVM>();

                foreach (DataRow dr in DS.Tables[0].Rows)
                {
                    lst.Add(
                    new ExpenseVM
                    {
                        Id = Convert.ToInt32(dr["Id"]),
                        CategoryName = dr["CategoryName"].ToString(),
                    });
                }               
                return Json(lst);
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult ExpenseItemDelete(int id)
        {
            try
            {
                var isExist = db.DailyExpenses.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();
                if (isExist != null)
                {
                    return Json(new { Status = "Already Used" });
                }
                var data = db.ExpenseCategories.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();               
                if (data != null)
                {
                    data.IsDeleted = true;
                }
                else
                {
                    return Json(new { Status = "Error" });
                }
                db.SaveChanges();
                return Json(new { Status = "success" });
            }
            catch (Exception ex)
            {
                return Json(new { Status = ex.Message });
            }
        }
        #endregion

        public IActionResult Privacy()
        {            
            return View();
        }

        #region DailyExpenses
                
        public IActionResult DailyExpenses()
        {           
            return View();
        }

        [HttpGet]
        public IActionResult GetCategoryNameList()
        {
            try
            {
                DataSet DS = new DataSet();
                DS = SqlHelper.ExecuteDataset(ConnectionStrings.connString, CommandType.Text, @"SELECT Id,CategoryName FROM ExpenseCategories WHERE  IsDeleted = 0");
                
                List<ExpenseVM> lst = new List<ExpenseVM>();

                foreach (DataRow dr in DS.Tables[0].Rows)
                {
                    lst.Add(
                    new ExpenseVM
                    {
                        Id = Convert.ToInt32(dr["Id"]),
                        CategoryName = dr["CategoryName"].ToString(),
                    });
                }

                return Json(lst);
            }
            catch (Exception ex)
            {
                return Json(new { Status = "ERROR", ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetDailyExpenseDataList(string pageSize, string pageNo)
        {
            try
            {
                DataSet DS = new DataSet();
                DS = SqlHelper.ExecuteDataset(ConnectionStrings.connString, CommandType.Text, @"EXEC SP_Get_DailyExpense_Data_List  @PageSize = '" + pageSize + "', @PageNo = '" + pageNo + "' ");

                List<ExpenseVM> lst = new List<ExpenseVM>();

                foreach (DataRow dr in DS.Tables[0].Rows)
                {
                    lst.Add(
                    new ExpenseVM
                    {
                        Id = Convert.ToInt32(dr["Id"]),
                        CategoryName = dr["CategoryName"].ToString(),
                        ExpenseDate = dr["ExpenseDate"].ToString(),
                        ExpenseAmount = dr["ExpenseAmount"].ToString(),
                        TotalRow = Convert.ToInt32(dr["TotalRow"]),
                    });
                }
                var events = lst.ToArray();

                return Json(events);
            }
            catch (Exception ex)
            {
                return Json(new { Status = "ERROR", ex.Message });
            }
        }

        [HttpGet]
        public IActionResult FilteringExpenseData(string pageSize, string pageNo, string FromDate, string ToDate)
        {
            try
            {
                DataSet DS = new DataSet();
                DS = SqlHelper.ExecuteDataset(ConnectionStrings.connString, CommandType.Text, @"EXEC SP_FilteringExpenseData  @PageSize = '" + pageSize + "', @PageNo = '" + pageNo + "',@FromDate = '" + Convert.ToDateTime(FromDate).ToString("ddMMyyyy") + "', @ToDate = '" + Convert.ToDateTime(ToDate).ToString("ddMMyyyy") + "' ");

                List<ExpenseVM> lst = new List<ExpenseVM>();

                foreach (DataRow dr in DS.Tables[0].Rows)
                {
                    lst.Add(
                    new ExpenseVM
                    {
                        Id = Convert.ToInt32(dr["Id"]),
                        CategoryName = dr["CategoryName"].ToString(),
                        ExpenseDate = dr["ExpenseDate"].ToString(),
                        ExpenseAmount = dr["ExpenseAmount"].ToString(),
                        TotalRow = Convert.ToInt32(dr["TotalRow"]),
                    });
                }
                var events = lst.ToArray();

                return Json(events);
            }
            catch (Exception ex)
            {
                return Json(new { Status = "ERROR", ex.Message });
            }
        }

        [HttpPost]
        public IActionResult Add_Update_DailyExpense(DailyExpenses dailyExp)
        {
            try
            {
                var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                var pickDate = dailyExp.ExpenseDate.ToString("yyyy-MM-dd");

                if (Convert.ToDateTime(pickDate) > Convert.ToDateTime(currentDate))
                {
                    return Json(new { Status = "Date Error" });
                }

                var data = db.DailyExpenses.Where(x => x.Id == dailyExp.Id && x.IsDeleted == false).FirstOrDefault();

                if (data != null)
                {
                    data.ExpenseCategoryId = dailyExp.ExpenseCategoryId;
                    data.ExpenseDate = dailyExp.ExpenseDate;
                    data.ExpenseAmount = dailyExp.ExpenseAmount;
                    data.UPD_USER_ID = HttpContext.Connection.LocalPort.ToString();
                    data.UPD_DATE = DateTime.Now;
                }
                else
                {
                    DailyExpenses expense = new DailyExpenses();

                    expense.ExpenseCategoryId = dailyExp.ExpenseCategoryId;
                    expense.ExpenseDate = dailyExp.ExpenseDate;
                    expense.ExpenseAmount = dailyExp.ExpenseAmount;
                    expense.IsDeleted = false;
                    expense.REG_USER_ID = HttpContext.Connection.LocalPort.ToString();
                    expense.REG_DATE = DateTime.Now;
                    db.DailyExpenses.Add(expense);
                }

                db.SaveChanges();

                return Json(new { Status = "success", Message = "Successfull." });
            }
            catch (Exception ex)
            {
                return Json(new { Status = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult DailyExpenseEdit(int id)
        {
            try
            {
                DataSet DS = new DataSet();
                DS = SqlHelper.ExecuteDataset(ConnectionStrings.connString, CommandType.Text, @"SELECT Id,ExpenseCategoryId,FORMAT(ExpenseDate,'yyyy-MM-dd')ExpenseDate,ExpenseAmount FROM DailyExpenses WHERE  Id =  '" + id + "'");

                List<ExpenseVM> lst = new List<ExpenseVM>();

                foreach (DataRow dr in DS.Tables[0].Rows)
                {
                    lst.Add(
                    new ExpenseVM
                    {
                        Id = Convert.ToInt32(dr["Id"]),
                        ExpenseCategoryId = Convert.ToInt32(dr["ExpenseCategoryId"]),
                        ExpenseDate = dr["ExpenseDate"].ToString(),
                        ExpenseAmount = dr["ExpenseAmount"].ToString(),
                    });
                }
                return Json(lst);
            }
            catch (Exception ex)
            {
                return View(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult DailyExpenseDelete(int id)
        {
            try
            {
                var data = db.DailyExpenses.Where(x => x.Id == id && x.IsDeleted == false).FirstOrDefault();

                if (data != null)
                {
                    data.IsDeleted = true;
                }
                else
                {
                    return Json(new { Status = "Error" });
                }
                db.SaveChanges();
                return Json(new { Status = "success" });
            }
            catch (Exception ex)
            {
                return Json(new { Status = ex.Message });
            }
        }

        #endregion              

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
