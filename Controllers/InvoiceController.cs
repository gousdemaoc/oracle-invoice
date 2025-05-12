using Microsoft.AspNetCore.Mvc;
using OracleAPInvoiceAttachmentExtract.Models;
using OracleAPInvoiceAttachmentExtract.Service;
using OracleAPInvoiceAttachmentExtract.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;



namespace OracleInvoice.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly ILogger<InvoiceController> _logger;
        private readonly InvoiceService _invoiceService;
        public string version = string.Empty;
        public InvoiceController(InvoiceService invoiceService, ILogger<InvoiceController> logger)
        {
            _invoiceService = invoiceService;
            _logger = logger;

        }


        public IActionResult Index()
        {
            var invoice = _invoiceService.GetInvoices();
            var user = GetUserInfo();
            GetVersion();

            if (user == null || invoice.Count < 1)
            {
                return View("Error");
            }

            var model = new InvoiceViewModel
            {
                Results = invoice,
                User = user,
                Version = version,
            };

            return View("Index", model);

        }

        [HttpPost]
        public IActionResult Search(InvoiceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Return the view with errors if the model state is invalid
                return View("Index", model);
            }
            try
            {
                var searchResults = _invoiceService.SearchInvoices(
                    model.VendorName,
                    model.InvoiceNumber,
                    model.VendorNumber,
                    model.StartDate,
                    model.EndDate
                    );

                var user = GetUserInfo();

                var resModel = new InvoiceViewModel
                {
                    Results = searchResults,
                    User = user,
                    VendorName = model.VendorName,
                    InvoiceNumber = model.InvoiceNumber,
                    VendorNumber = model.VendorNumber,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    SearchPerformed = true,
                };

                return View("Index", resModel);
            }
            catch (Exception ex)
            {
                // Log the exception
                _logger.LogError(ex, "Error occurred while searching invoices.");

                // Optionally, show a user-friendly error message
                TempData["Error"] = "An error occurred while processing your request. Please try again.";
                return RedirectToAction("Index");
            }
        }

        //public IActionResult GetDataSheet(string invoiceId, string docName)
        //{
        //    try
        //    {
        //        byte[] pdfData = _invoiceService.GetInvoicePdf(invoiceId);

        //        if (pdfData == null || pdfData.Length == 0)
        //        {
        //            return NotFound("Document not found.");
        //        }
        //        Response.Headers.Add("Content-Disposition", "inline; filename=" + $"{docName}.pdf");
        //        return File(pdfData, "application/pdf", $"{docName}.pdf");
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the error and return a server error response
        //        return StatusCode(500, "Internal server error");
        //    }
        //}

        public IActionResult GetDataSheet(string invoiceId, string docName)
        {
            try
            {
                byte[] fileData = _invoiceService.GetInvoicePdf(invoiceId);

                if (fileData == null || fileData.Length == 0)
                {
                    return NotFound("Document not found.");
                }

                string fileExtension = Path.GetExtension(docName)?.ToLower();
                string contentType;
                string disposition;

                if (fileExtension == ".pdf")
                {
                    contentType = "application/pdf";
                    disposition = $"inline; filename=\"{docName}\"";
                }
                else if (fileExtension == ".msg")
                {
                    contentType = "application/vnd.ms-outlook";
                    disposition = $"attachment; filename=\"{docName}\""; // Force download
                }
                else
                {
                    return BadRequest("Unsupported file type.");
                }

                Response.Headers.Add("Content-Disposition", disposition);
                return File(fileData, contentType, docName);
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost]
        public IActionResult ExportToPdf([FromBody] ExportInvoiceModel request)
        {
            if (request.SelectedItems == null || !request.SelectedItems.Any())
            {
                return BadRequest("No documents selected.");
            }

            //if (request.Combined)
            //{
            //    var pdfBytes = GetDataSheet(request.SelectedItems.Select(item => item.Id).ToList());
            //    string fileName = $"Combined_{DateTime.Now:MMddyyyy_HHmmss}.pdf";
            //    return File(pdfBytes, "application/pdf", fileName);
            //}
            //else
            //{
            byte[] zipBytes = CreateZipOfPdfs(request.SelectedItems);
            return File(zipBytes, "application/zip", $"OracleInvoice_{DateTime.Now:yyyyMMdd}.zip");
            //}
        }

        //public byte[] CreateZipOfPdfs(List<SelecedInvoice> invoices)
        //{
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        //        {
        //            foreach (var invoice in invoices)
        //            {
        //                var pdfBytes = _invoiceService.GetInvoicePdf(invoice.Id);
        //                if (pdfBytes != null && pdfBytes.Length > 0)
        //                {
        //                    var entry = archive.CreateEntry($"{invoice.InvoiceNumber}.pdf");
        //                    using (var entryStream = entry.Open())
        //                    {
        //                        entryStream.Write(pdfBytes, 0, pdfBytes.Length);
        //                    }
        //                }
        //            }
        //        }

        //        return memoryStream.ToArray();
        //    }
        //}

        public byte[] CreateZipOfPdfs(List<SelecedInvoice> invoices)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    foreach (var invoice in invoices)
                    {
                        var fileBytes = _invoiceService.GetInvoicePdf(invoice.Id); // Assuming this method returns both PDF and MSG files
                        if (fileBytes != null && fileBytes.Length > 0)
                        {
                            string fileExtension = Path.GetExtension(invoice.InvoiceNumber)?.ToLower(); // Ensure correct extension
                            string fileName = $"{invoice.InvoiceNumber}{fileExtension}";

                            // Ensure valid file extension
                            if (fileExtension != ".pdf" && fileExtension != ".msg")
                            {
                                continue; // Skip unsupported file types
                            }

                            var entry = archive.CreateEntry(fileName, CompressionLevel.Fastest);
                            using (var entryStream = entry.Open())
                            {
                                entryStream.Write(fileBytes, 0, fileBytes.Length);
                            }
                        }
                    }
                }

                return memoryStream.ToArray();
            }
        }



        private UserInfo GetUserInfo()
        {
            string userName = String.Empty;
#if DEBUG
           //userName = "Shaun.McCaffrey";
          userName = "georges.deme";
#else
                userName = HttpContext.User.Identity.Name.Substring(HttpContext.User.Identity.Name.LastIndexOf("\\", StringComparison.CurrentCulture) + 1);
#endif

            return _invoiceService.GetUser(userName);

        }

        public void GetVersion()
        {
            version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

    }
}

