namespace OracleAPInvoiceAttachmentExtract.Models
{
    public class InvoiceResult
    {
        public string VendorNumber { get; set; }
        public string VendorName { get; set; }
        public string VendorSiteCode { get; set; }
        public string InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }   
        public string PdfFile { get; set; }
        public string InvoiceDate { get; set; }
        
    }
}
