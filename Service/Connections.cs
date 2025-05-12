using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace OracleAPInvoiceAttachmentExtract.Service
{
    public class Connections
    {
        private static string getAppSettings()
        {

            if (isProduction())
                return "appsettings.json";
            else
                return "appsettings.Development.json";

        }

        public static string get1(string name)
        {


            JToken jAppSettings = JToken.Parse(
                File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
                    getAppSettings())));



            return jAppSettings.SelectToken("ConnectionStrings").SelectToken(name).Value<string>();


        }

        public static string get(string name)
        {
            if (isProduction())
            {
                switch (name)
                {
                    case "sds":
                        return "User Id=msds_om; Password=msds_om; Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=tcp)(HOST=aoccol-72.aoc-resins.com)(PORT=1531)))(CONNECT_DATA=(SERVICE_NAME=ebs_TACPR12)(INSTANCE_NAME=CTACPR12)));Persist Security Info=false;Connection Timeout=300;";

                    case "aocp":
                        return "User Id=AOCPORT; Password=AOCPORT; Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=aoccol-72.aoc-resins.com)(PORT=1531))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ebs_TACPR12)(INSTANCE_NAME=CTACPR12)));Persist Security Info=false;Connection Timeout=300;";
                    default:
                        return null;

                }
            }
            else if (isUAT())
            {
                switch (name)
                {
                    case "sds":
                        return "User Id=msds_om; Password=uat2023msds; Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=tcp)(HOST=aocuat-72.aoc-resins.com)(PORT=1536)))(CONNECT_DATA=(SERVICE_NAME=ebs_TACSR12)(INSTANCE_NAME=CTACSR12)));Persist Security Info=false;Connection Timeout=300;";

                    default:
                        return null;
                }
            }
            else if (isDev() || isLocal())
            {
                switch (name)
                {
                    case "sds":
                        return "User Id=msds_om; Password=dev2023msds; Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=tcp)(HOST=aocdev-72.aoc-resins.com)(PORT=1533)))(CONNECT_DATA=(SERVICE_NAME=ebs_TACDR12)(INSTANCE_NAME=CTACDR12)));Persist Security Info=false;Connection Timeout=300;";

                    case "aocp":
                        return "User Id=AOCPORT; Password=AOCPORT; Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=tcp)(HOST=aocdev-72.aoc-resins.com)(PORT=1533)))(CONNECT_DATA=(SID=TACDR12)));Persist Security Info=false;Connection Timeout=300;";
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }


        //public static bool isProduction()
        //{
        //    JToken jAppSettings = JToken.Parse(
        //       File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
        //           "appsettings.json")));

        //    return jAppSettings.SelectToken("UseProd").Value<string>().ToUpper() == "TRUE";

        //}


        //public static bool isUAT()
        //{
        //    JToken jAppSettings = JToken.Parse(
        //       File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
        //           "appsettings.json")));

        //    return jAppSettings.SelectToken("UseUAT").Value<string>().ToUpper() == "TRUE";

        //}


        //public static bool isDev()
        //{
        //    JToken jAppSettings = JToken.Parse(
        //       File.ReadAllText(Path.Combine(Environment.CurrentDirectory,
        //           "appsettings.json")));

        //    return jAppSettings.SelectToken("UseDev").Value<string>().ToUpper() == "TRUE";

        //}

        public static bool isProduction()
        {
            return Environment.MachineName.ToUpper() == "AOCCOL-181";

        }

        public static bool isDev()
        {
            return Environment.MachineName.ToUpper() == "AOCCOL-181X";

        }

        public static bool isUAT()
        {
            return Environment.MachineName.ToUpper() == "AOCCOL-181S";

        }

        public static bool isLocal()
        {
            return Environment.MachineName.ToUpper() == "COL-GDEME-SUR";

        }
        





    }
}
