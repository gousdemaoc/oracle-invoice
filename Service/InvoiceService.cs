using Oracle.ManagedDataAccess.Client;
using OracleAPInvoiceAttachmentExtract.Models;
using OracleAPInvoiceAttachmentExtract.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OracleAPInvoiceAttachmentExtract.Service
{
    public class InvoiceService
    {
        public List<InvoiceResult> GetInvoices()
        {
            DataSet ds = new DataSet();
            DataTable dt = null;
            List<InvoiceResult> lstInvoiceDetails = new List<InvoiceResult>();

            // Update the SQL query to include a condition for the last 2 years
            string sql = @"
        SELECT INVOICE_ID, VENDOR_NAME, VENDOR_NUM, VENDOR_SITE_CODE,INVOICE_NUM, INVOICE_DATE, FILE_NAME, INVOICE_NUM
        FROM apps.xxaoc_fnd_attach_ap_invoice_vw
        WHERE INVOICE_DATE >= ADD_MONTHS(SYSDATE, -5)";

            using (OracleConnection con = new OracleConnection(Connections.get("sds")))
            using (OracleCommand cmd = new OracleCommand(sql, con))
            {
                cmd.CommandTimeout = 90;
                try
                {
                    con.Open();
                    using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                    {
                        ds.EnforceConstraints = false;
                        da.Fill(ds);
                        dt = ds.Tables[0];
                    }

                    // Loop through the data table and map it to the DeliveryInfo model
                    foreach (DataRow row in dt.Rows)
                    {
                        lstInvoiceDetails.Add(new InvoiceResult
                        {
                            InvoiceId = row["INVOICE_ID"]?.ToString(),
                            VendorName = row["VENDOR_NAME"]?.ToString(),
                            VendorSiteCode = row["VENDOR_SITE_CODE"]?.ToString(),
                            PdfFile = row["FILE_NAME"]?.ToString(),
                            InvoiceDate = row["INVOICE_DATE"]?.ToString(),
                            InvoiceNumber = row["INVOICE_NUM"]?.ToString(),
                            VendorNumber = row["VENDOR_NUM"]?.ToString()
                        });
                    }

                    return lstInvoiceDetails;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error fetching invoices: {ex.Message}");
                    return new List<InvoiceResult>();
                }
            }
        }

        public List<InvoiceResult> SearchInvoices(string VendorName, string InvoiceNumber, string VendorNumber, DateTime? startDate, DateTime? endDate)
        {
            DataSet ds = new DataSet();
            List<InvoiceResult> lstSDSResults = new List<InvoiceResult>();

            string productSql = @"
        SELECT INVOICE_ID, VENDOR_NAME, VENDOR_NUM, VENDOR_SITE_CODE,INVOICE_NUM, INVOICE_DATE, FILE_NAME, INVOICE_NUM
        FROM apps.xxaoc_fnd_attach_ap_invoice_vw
        WHERE  1=1";

            if (startDate.HasValue)
            {
                productSql += " AND INVOICE_DATE >= :startDate";
            }

            if (endDate.HasValue)
            {
                productSql += " AND INVOICE_DATE <= :endDate";
            }

            if (!string.IsNullOrWhiteSpace(InvoiceNumber))
            {
                productSql += " AND INVOICE_NUM = :invoiceNumber";
            }

            if (!string.IsNullOrWhiteSpace(VendorName))
            {
                productSql += " AND UPPER(VENDOR_NAME) LIKE UPPER(:VendorName) || '%'";
            }

            if (!string.IsNullOrWhiteSpace(InvoiceNumber))
            {
                productSql += " AND INVOICE_NUM = :invoiceNumber";
            }

            if (!string.IsNullOrWhiteSpace(VendorNumber))
            {
                productSql += " AND VENDOR_NUM = :vendorNumber";
            }

            using (OracleConnection con = new OracleConnection(Connections.get("sds")))
            using (OracleCommand cmd = new OracleCommand(productSql, con))
            {
                cmd.CommandTimeout = 90;
                if(startDate.HasValue)
                {
                    cmd.Parameters.Add("startDate", OracleDbType.Date).Value = startDate;
                }
                if(endDate.HasValue)
                {
                    cmd.Parameters.Add("endDate", OracleDbType.Date).Value = endDate;
                }

                if (!string.IsNullOrWhiteSpace(VendorName))
                {
                    cmd.Parameters.Add("VendorName", OracleDbType.Varchar2).Value = VendorName;
                }

                if (!string.IsNullOrWhiteSpace(InvoiceNumber))
                {
                    cmd.Parameters.Add("invoiceNumber", OracleDbType.Varchar2).Value = InvoiceNumber;
                }

                if (!string.IsNullOrWhiteSpace(VendorNumber))
                {
                    cmd.Parameters.Add("vendorNumber", OracleDbType.Varchar2).Value = VendorNumber;
                }

                try
                {
                    con.Open();
                    using (OracleDataAdapter da = new OracleDataAdapter(cmd))
                    {
                        ds.EnforceConstraints = false;
                        da.Fill(ds);

                        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                        {
                            foreach (DataRow row in ds.Tables[0].Rows)
                            {
                                lstSDSResults.Add(new InvoiceResult
                                {
                                    InvoiceId = row["INVOICE_ID"]?.ToString(),
                                    VendorName = row["VENDOR_NAME"]?.ToString(),
                                    VendorSiteCode = row["VENDOR_SITE_CODE"]?.ToString(),
                                    PdfFile = row["FILE_NAME"]?.ToString(),
                                    InvoiceDate = row["INVOICE_DATE"]?.ToString(),
                                    InvoiceNumber = row["INVOICE_NUM"]?.ToString(),
                                    VendorNumber = row["VENDOR_NUM"]?.ToString()
                                });
                            }
                        }
                    }

                    return lstSDSResults;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return new List<InvoiceResult>();
                }
            }
        }

        public byte[] GetInvoicePdf(string invoiceId)
        {
            if (string.IsNullOrEmpty(invoiceId))
                throw new ArgumentException("Invoice ID cannot be null or empty.", nameof(invoiceId));

            try
            {
                byte[] pdf = null;

                using (OracleConnection oraConn = new OracleConnection(Connections.get("sds")))
                using (OracleCommand oraCmd = oraConn.CreateCommand())
                {
                    oraConn.Open();

                    oraCmd.CommandText = "Select LOB_FILE_DATA from  apps.xxaoc_fnd_attach_ap_invoice_vw where INVOICE_ID = :invoiceId";
                    oraCmd.Parameters.Add(new OracleParameter("invoiceId", invoiceId));

                    var result = oraCmd.ExecuteScalar();

                    if (result != DBNull.Value && result is byte[])
                    {
                        pdf = (byte[])result;
                    }
                    else
                    {
                        throw new Exception("The document with the provided ID could not be found.");
                    }
                }

                return pdf;
            }
            catch (OracleException ex)
            {
                throw new Exception("An error occurred while accessing the database.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the data sheet.", ex);
            }
        }

        public UserInfo GetUser(string username)
        {
            string sql = $"select appid, login, fullname, email from INTRANET.APPSECURITYVIEW where NLSSORT(LOGIN, 'NLS_SORT=BINARY_CI') = NLSSORT(:username, 'NLS_SORT=BINARY_CI') AND ROWNUM = 1 AND APPID = 270";

            string dbVersion = String.Empty;
            UserInfo user = null;

            if (Connections.isProduction())
            {
                dbVersion = "PROD";
            }
            else if (Connections.isDev() || Connections.isLocal())
            {
                dbVersion = "DEV-TACDR12";
            }
            else if (Connections.isUAT())
            {
                dbVersion = "UAT-TACSR12";
            }

            using (OracleConnection con = new OracleConnection(Connections.get("sds")))
            using (OracleCommand cmd = new OracleCommand(sql, con))
            {
                cmd.CommandTimeout = 90;
                cmd.Parameters.Add(new OracleParameter("username", username));

                try
                {
                    con.Open();
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read()) // Only process if there is data
                        {
                            user = new UserInfo
                            {
                                Login = reader.GetString(reader.GetOrdinal("login")),
                                FullName = reader.GetString(reader.GetOrdinal("fullname")),
                                Email = reader.GetString(reader.GetOrdinal("email")),
                                DbVersion = dbVersion,
                                ServerName = Environment.MachineName
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return new UserInfo();
                }
            }

            return user;
        }
    }
}
