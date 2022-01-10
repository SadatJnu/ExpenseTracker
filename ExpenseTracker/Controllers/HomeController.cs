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
using System.IO;
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
        private readonly ILogger<HomeController> _logger;

        public HomeController(IConfiguration _configuration, ILogger<HomeController> logger)
        {
            Configuration = _configuration;
            _logger = logger;
            ConnectionStrings.connString = this.Configuration.GetConnectionString("DefaultConnection");
        }

        #region ExpenseDataList

        [HttpGet]
        public IActionResult Index()
        {            
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
                _logger.LogError(ex.Message);
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
                _logger.LogError(ex.Message);
                return Json(new { Status = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult CategoryNameEdit(int id)
        {
            try
            {
                List<ExpenseVM> lst = new List<ExpenseVM>();

                var result = db.ExpenseCategories.Where(x => x.Id == id && x.IsDeleted == false).ToList();
                if(result.Count > 0)
                {
                    foreach (var item in result)
                    {
                        lst.Add(
                        new ExpenseVM
                        {
                            Id = Convert.ToInt32(item.Id),
                            CategoryName = item.CategoryName,
                        });
                    }
                }
                else
                {
                    return Json(new { Status = "Record Not Found!" });
                }
                
                return Json(lst);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
                _logger.LogError(ex.Message);
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
                List<ExpenseVM> lst = new List<ExpenseVM>();

                var result = db.ExpenseCategories.Where(x => x.IsDeleted == false).ToList();
                if (result.Count > 0)
                {
                    foreach (var item in result)
                    {
                        lst.Add(
                        new ExpenseVM
                        {
                            Id = Convert.ToInt32(item.Id),
                            CategoryName = item.CategoryName,
                        });
                    }
                }
                else
                {
                    return Json(new { Status = "Record Not Found!" });
                }
                
                return Json(lst);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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

                var joinQuery = from DE in db.DailyExpenses
                                join EC in db.ExpenseCategories on DE.ExpenseCategoryId equals EC.Id
                                select new { Id = DE.Id, ExpenseCategoryId = DE.ExpenseCategoryId, CategoryName = EC.CategoryName, ExpenseAmount = DE.ExpenseAmount, ExpenseDate = DE.ExpenseDate };

                var result = joinQuery.ToList();

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
                _logger.LogError(ex.Message);
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
                _logger.LogError(ex.Message);
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
                _logger.LogError(ex.Message);
                return Json(new { Status = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult DailyExpenseEdit(int id)
        {
            try
            {
                List<ExpenseVM> lst = new List<ExpenseVM>();

                var result = db.DailyExpenses.Where(x => x.Id == id && x.IsDeleted == false).ToList();
                if (result.Count > 0)
                {
                    foreach (var item in result)
                    {
                        DateTime dt = Convert.ToDateTime(item.ExpenseDate.ToString());

                        lst.Add(
                        new ExpenseVM
                        {
                            Id = Convert.ToInt32(item.Id),
                            ExpenseCategoryId = Convert.ToInt32(item.ExpenseCategoryId),
                            ExpenseDate = dt.ToString("yyyy-MM-dd"),
                            ExpenseAmount = item.ExpenseAmount.ToString(),
                        });
                    }
                }
                else
                {
                    return Json(new { Status = "Record Not Found!" });
                }

                return Json(lst);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
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
                _logger.LogError(ex.Message);
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
