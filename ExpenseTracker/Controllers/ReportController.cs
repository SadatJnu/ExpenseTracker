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
using Microsoft.AspNetCore.Authorization;
using System.Drawing;
using ZXing;
using ZXing.QrCode;

namespace ExpenseTracker.Controllers
{
    [AllowAnonymous]
    public class ReportController : Controller
    {
        private readonly ILogger<ReportController> _logger;

        public ReportController(ILogger<ReportController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult DownloadDailyExpensesData_BKP(string reportType, string pageSize, string pageNo)
        {
            try
            {
                string reportName = "DailyExpenses";
                var returnString = GenerateReportAsync(reportType, reportName, pageSize, pageNo);
                return File(returnString, System.Net.Mime.MediaTypeNames.Application.Octet, reportName + ".pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);                
                throw;
            }
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
                //parameters.Add("@ReportName", "Daily Expenses List");
                var result = report.Execute(GetRenderType(reportType), 1, parameters);
                return result.MainStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<IActionResult> DownloadDailyExpensesData(string reportType, string pageSize, string pageNo)
        {
            try
            {
                //var BarCode = GetBarCode(reportType);

                string reportName = "DailyExpenses";
                string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("ExpenseTracker.dll", string.Empty);
                string rdlcFilePath = string.Format("{0}RDLC\\{1}.rdlc", fileDirPath, reportName);
                //Directory.CreateDirectory(fileDirPath + "\\ReportFiles");
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
                return FileRenderType(reportType, report, reportName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        private FileContentResult FileRenderType(string reportType, LocalReport localReport, string reportName)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            var renderType = "";
            string FileName = "";
            byte[] mainStrem = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
            if (reportType == "pdf")
            {
                var result = localReport.Execute(GetRenderType(reportType), 1, parameters);
                mainStrem = result.MainStream;
                renderType = "application/pdf";
            }
            if (reportType == "excel")
            {
                var result = localReport.Execute(RenderType.Excel);
                mainStrem = result.MainStream;
                renderType = "application/vnd.ms-excel";
            }
            if (reportType == "word")
            {
                var result = localReport.Execute(RenderType.Word);
                mainStrem = result.MainStream;
                if (!String.IsNullOrEmpty(reportName))
                {
                    FileName = reportName + ".doc";
                }
                else
                {
                    FileName = $"Reports_{DateTime.Now.ToString("dd-MMMM-yyyy")}.doc";
                }
                renderType = "application/vnd.ms-word";
            }            

            return File(mainStrem, renderType, FileName);
        }

        //private byte[] GetBarCode(string Content)
        //{
        //    var br = new BarcodeWriter();
        //    br.Options = new QrCodeEncodingOptions
        //    {
        //        DisableECI = true,
        //        CharacterSet = "UTF-8",
        //        Width = 230,
        //        Height = 500,
        //        PureBarcode = false
        //    };
        //    br.Format = BarcodeFormat.CODE_128;

        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        using (Bitmap bm1 = new Bitmap(br.Write(Content)))
        //        {
        //            bm1.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        //            return ms.ToArray();
        //        }
        //    }
        //}

        private RenderType GetRenderType(string reportType)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }



    }
}
