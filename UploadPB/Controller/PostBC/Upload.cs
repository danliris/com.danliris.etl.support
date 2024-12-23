using System;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using UploadPB.Models;
using UploadPB.Services.Interfaces.IPostBC40Service;
using UploadPB.Services.Class;
using UploadPB.Services.Interfaces.IPostBC23Service;
using UploadPB.Tools;
using UploadPB.Services.Interfaces.IPostBC261Service;
using Microsoft.Extensions.DependencyInjection;
using UploadPB.Services.Interfaces.IPostBC262Service;
using UploadPB.Services.Interfaces.IPostBC30Service;
using UploadPB.Services.Interfaces.IPostBC27Service;
using System.Collections.Generic;
using UploadPB.Services.Interfaces.IPostBC41Service;
using UploadPB.Services.Interfaces.IPostBC25Service;
using UploadPB.Services.Interfaces.IPostBC33Service;

namespace UploadPB.Controller.PostBC
{
    public class Upload
    {
        public IUploadExcel40 _uploadExcel40;
        public IUploadExcel23 _uploadExcel23;
        public IUploadExcel261 _uploadExcel261;
        public IUploadExcel262 _uploadExcel262;
        public IUploadExcel30 _uploadExcel30;
        public IUploadExcel27 _uploadExcel27;
        public IUploadExcel41 _uploadExcel41;
        public IUploadExcel25 _uploadExcel25;
        public IUploadExcel33 _uploadExcel33;
        ConverterChecker converterChecker = new ConverterChecker();

        public Upload(IUploadExcel40 uploadExcel40, IUploadExcel23 uploadExcel23, IUploadExcel261 uploadExcel261, IUploadExcel262 uploadExcel262, IUploadExcel30 uploadExcel30, IUploadExcel27 uploadExcel27, IUploadExcel41 uploadExcel41, IUploadExcel25 uploadExcel25, IUploadExcel33 uploadExcel33)
        {
            _uploadExcel40 = uploadExcel40;
            _uploadExcel23 = uploadExcel23;
            _uploadExcel261 = uploadExcel261;
            _uploadExcel262 = uploadExcel262;
            _uploadExcel30 = uploadExcel30;
            _uploadExcel27 = uploadExcel27;
            _uploadExcel41 = uploadExcel41;
            _uploadExcel25 = uploadExcel25;
            _uploadExcel33 = uploadExcel33;
        }

        [FunctionName("UploadBC")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            req.HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");

            //string[] EXTENSION = { ".xlsx",".XLSX" };

            //string[] EXTENSION = { ".xlsx", ".XLSX" };
            List<string> EXTENSION = new List<string>(new string[] { ".xlsx", ".XLSX" });
            try
            {
                var formdata = await req.ReadFormAsync();
                IFormFile file = req.Form.Files["file"];
                req.Form.TryGetValue("type", out Microsoft.Extensions.Primitives.StringValues type);


                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                if (EXTENSION.Contains(System.IO.Path.GetExtension(file.FileName)))
                {

                    using (var excelPack = new ExcelPackage())
                    {
                        using (var stream = file.OpenReadStream())
                        {
                            excelPack.Load(stream);

                        }
                        var sheet = excelPack.Workbook.Worksheets;
                        var sheetData = sheet[0];
                        var typeFromSheet = converterChecker.GenerateValueString(sheetData.Cells[2, 2]);
                        try
                        {
                            if (type == "40" && typeFromSheet == "40")
                            {
                                await _uploadExcel40.Upload(sheet);
                            }
                            else if (type == "23" && typeFromSheet == "23")
                            {
                                await _uploadExcel23.Upload(sheet);
                            }
                            else if (type == "261" && typeFromSheet == "261")
                            {
                                await _uploadExcel261.Upload(sheet);
                            }
                            else if (type == "262" && typeFromSheet == "262")
                            {
                                await _uploadExcel262.Upload(sheet);
                            }
                            else if (type == "30" && typeFromSheet == "30")
                            {
                                await _uploadExcel30.Upload(sheet);
                            }
                            else if (type == "27" && typeFromSheet == "27")
                            {
                                await _uploadExcel27.Upload(sheet);
                            }
                            else if (type == "41" && typeFromSheet == "41")
                            {
                                await _uploadExcel41.Upload(sheet);
                            } else if (type == "25" && typeFromSheet == "25")
                            {
                                await _uploadExcel25.Upload(sheet);
                            }
                            else if (type == "33" && typeFromSheet == "33")
                            {
                                await _uploadExcel33.Upload(sheet);
                            }
                            else if (type != typeFromSheet)
                            {
                                throw new Exception("Harap Upload File Sesuai Dengan Tipe BC yang Dipilih");
                            }
                            return new OkObjectResult(new ResponseSuccess("success"));
                        }
                        catch (Exception ex)
                        {
                            return new BadRequestObjectResult(new ResponseFailed(ex.Message, ex.Data));
                        }
                    }
                }             
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new ResponseFailed("Gagal menyimpan data", ex));
            }

            return new OkObjectResult(new ResponseSuccess("success"));
        }
    }
}
