using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Reporting;
using ExpenseTracker.Models;
using ExpenseTracker.ModelView;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;
using System.IO;
using ExpenseTracker.Service;

namespace ExpenseTracker.Controllers
{
    public class ReportController : Controller
    {
        [HttpGet]
        public IActionResult DownloadDailyExpensesData(string reportType, string pageSize, string pageNo)
        {
            string reportName = "DailyExpenses";            
            var returnString = GenerateReportAsync(reportType,reportName, pageSize, pageNo);
            return File(returnString, System.Net.Mime.MediaTypeNames.Application.Octet, reportName + ".pdf");
        }

        public byte[] GenerateReportAsync(string reportType, string reportName, string pageSize, string pageNo)
        {
            try
            {
                string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("ExpenseTracker.dll", string.Empty);
                string rdlcFilePath = string.Format("{0}ReportFiles\\{1}.rdlc", fileDirPath, reportName);
                Directory.CreateDirectory(fileDirPath + "\\ReportFiles");
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                Encoding.GetEncoding("windows-1252");                
                if (!System.IO.File.Exists(rdlcFilePath))
                {
                    System.IO.File.Create(rdlcFilePath);
                }
                LocalReport report = new LocalReport(rdlcFilePath);

                DataTable dt = new DataTable();
                dt = SqlHelper.ExecuteDataTable(ConnectionStrings.connString, CommandType.Text, @"EXEC SP_Get_DailyExpense_Data_List  @PageSize = '" + pageSize + "', @PageNo = '" + pageNo + "' "); 

                report.AddDataSource("SPResults", dt);
                parameters.Add("@ReportName", "Daily Expenses List");
                var result = report.Execute(GetRenderType(reportType), 1, parameters);
                return result.MainStream;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private RenderType GetRenderType(string reportType)
        {
            var renderType = RenderType.Pdf;
            switch (reportType.ToLower())
            {
                default:
                case "pdf":
                    renderType = RenderType.Pdf;
                    break;
                case "word":
                    renderType = RenderType.Word;
                    break;
                case "excel":
                    renderType = RenderType.Excel;
                    break;
            }

            return renderType;
        }

    }
}
