using OracleAPInvoiceAttachmentExtract.Models;
using System;
using System.Collections.Generic;

namespace OracleAPInvoiceAttachmentExtract.ViewModel
{
    public class InvoiceViewModel
    {
        public UserInfo User { get; set; }

        public List<InvoiceResult> Results { get; set; }
        public List<int> SelectedIds { get; set; }
        public string Version { get; set; }
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
        public string VendorName { get; set; }
        public string VendorNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool SearchPerformed { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    }
     
}
