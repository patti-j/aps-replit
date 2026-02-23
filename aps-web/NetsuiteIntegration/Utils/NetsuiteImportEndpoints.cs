using System;
using System.Collections.Generic;

namespace NetsuiteIntegration.Utils
{
    public record Endpoint(string Name, string Url, string TargetTable);

    public static class NetsuiteImportEndpoints
    {
        // Builds host from NetSuite account id (e.g., 123_SB1 -> 123-sb1.restlets.api.netsuite.com)
        private static string HostFromAccount(string a_accountId)
        {
            string sub = a_accountId.Replace("_", "-").ToLowerInvariant();
            return $"{sub}.restlets.api.netsuite.com";
        }

        // Composes a RESTlet URL
        private static string BuildUrl(string a_host, string a_script, string a_deploy)
        {
            return $"https://{a_host}/app/site/hosting/restlet.nl?script={a_script}&deploy={a_deploy}";
        }

        // Returns all RESTlet endpoints for the given account
        public static IEnumerable<Endpoint> AllFor(string a_accountId)
        {
            string host = HostFromAccount(a_accountId);
            return new List<Endpoint>
            {
                new("WorkOrders",
                    BuildUrl(host, "customscript_workorders", "customdeploy_workorders"),
                    "Netsuite_WorkOrders"),

                new("BillOfMaterials",
                    BuildUrl(host, "customscript_boms", "customdeploy_boms"),
                    "Netsuite_BillOfMaterials"),

                new("StockOnHand",
                    BuildUrl(host, "customscript_stockonhand", "customdeploy_stockonhand"),
                    "Netsuite_StockOnHand"),

                new("FinishedGoods",
                    BuildUrl(host, "customscript_finishedgoods", "customdeploy_finishedgoods"),
                    "Netsuite_FinishedGoods"),

                new("PurchaseOrders",
                    BuildUrl(host, "customscript_purchaseorders", "customdeploy_purchaseorders"),
                    "Netsuite_PurchaseOrders"),

                new("SalesOrders",
                    BuildUrl(host, "customscript_salesorders", "customdeploy_salesorders"),
                    "Netsuite_SalesOrders"),

                new("BOMRevisions",
                    BuildUrl(host, "customscript_bomrevisions", "customdeploy_bomrevisions"),
                    "Netsuite_BOMRevisions"),

                new("MFGRouting",
                    BuildUrl(host, "customscript_mfgrouting", "customdeploy_mfgrouting"),
                    "Netsuite_MFGRouting"),

                new("RawMaterials",
                    BuildUrl(host, "customscript_rawmaterials", "customdeploy_rawmaterials"),
                    "Netsuite_RawMaterials"),

                new("MaterialConsumptions",
                    BuildUrl(host, "customscript_materialconsumptions", "customdeploy_materialconsumptions"),
                    "Netsuite_MaterialConsumptions"),
            };
        }
    }
}
