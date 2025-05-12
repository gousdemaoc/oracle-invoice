using System.Collections.Generic;

namespace OracleAPInvoiceAttachmentExtract.Models
{
    public class ExportInvoiceModel
    {
        public List<SelecedInvoice> SelectedItems { get; set; }
        public bool Combined { get; set; }
    }
}
