using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using NetsuiteIntegration.ResponseObjects;
using NetsuiteIntegration.Utils;
using Newtonsoft.Json.Linq;

namespace NetsuiteIntegration
{
    public class NetsuiteImport
    {
        // Imports all configured NetSuite endpoints into SQL.
        public async Task<Task> ImportData(string a_accountId, string a_clientId, string a_clientSecret, string a_tokenId, string a_tokenSecret, string a_dbConnectionString, List<string> a_importEndpoints, string a_publishEndpoint, bool a_workOrders, bool a_boms, bool a_routings, bool a_items, bool a_purchaseOrders, bool a_salesOrders, string a_schema)
        {
            using HttpClient http = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };

            NetSuiteApiConfig cfg = new NetSuiteApiConfig
            {
                AccountId = a_accountId,
                ClientId = a_clientId,
                ClientSecret = a_clientSecret,
                TokenId = a_tokenId,
                TokenSecret = a_tokenSecret
            };

            NetSuiteApiLibrary netSuiteApiLibrary = new NetSuiteApiLibrary(cfg, http);

            //Import data from endpoints
            if (a_importEndpoints != null || a_importEndpoints.Count > 0)
            {
                foreach (string endpoint in a_importEndpoints)
                {
                    Uri uri = new Uri(endpoint);
                    NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);
                    string tableName = query["script"];
                    JArray arr = await netSuiteApiLibrary.GetArrayAsync(endpoint);
                    await SqlDump.DumpAsync(arr, a_dbConnectionString, tableName, a_schema);

                }
            }

            //Import data from SuiteQL
            SuiteQlReader suiteReader = new NetsuiteIntegration.Utils.SuiteQlReader(
                Environment.GetEnvironmentVariable("SUITEQL_DIR")
            );
            
            
            if (a_workOrders)
            {
                string sql = suiteReader.Read("Netsuite_WorkOrders.sql");
                JArray data = await netSuiteApiLibrary.RunSuiteQlPagedAsync(a_accountId, sql, 1000);
                await SqlDump.DumpAsync(data, a_dbConnectionString, "Netsuite_WorkOrders", a_schema);
            }
            
            if (a_boms)
            {
                string sql = suiteReader.Read("Netsuite_BOMs.sql");
                JArray data = await netSuiteApiLibrary.RunSuiteQlPagedAsync(a_accountId, sql, 1000);
                await SqlDump.DumpAsync(data, a_dbConnectionString, "Netsuite_BOMs", a_schema);

                sql = suiteReader.Read("Netsuite_BOMRevision.sql");
                data = await netSuiteApiLibrary.RunSuiteQlPagedAsync(a_accountId, sql, 1000);
                await SqlDump.DumpAsync(data, a_dbConnectionString, "Netsuite_BOMRevision", a_schema);

                sql = suiteReader.Read("Netsuite_BOMRevisionComponents.sql");
                data = await netSuiteApiLibrary.RunSuiteQlPagedAsync(a_accountId, sql, 1000);
                await SqlDump.DumpAsync(data, a_dbConnectionString, "Netsuite_BOMRevisionComponents", a_schema);

            }

            if (a_routings)
            {
                string sql = suiteReader.Read("Netsuite_Routings.sql");
                JArray data = await netSuiteApiLibrary.RunSuiteQlPagedAsync(a_accountId, sql, 1000);
                await SqlDump.DumpAsync(data, a_dbConnectionString, "Netsuite_Routings", a_schema);
            }

            if (a_items)
            {
                string sql = suiteReader.Read("Netsuite_Items.sql");
                JArray data = await netSuiteApiLibrary.RunSuiteQlPagedAsync(a_accountId, sql, 1000);
                await SqlDump.DumpAsync(data, a_dbConnectionString, "Netsuite_Items", a_schema);
            }

            if (a_salesOrders)
            {
                string sql = suiteReader.Read("Netsuite_SalesOrders.sql");
                JArray data = await netSuiteApiLibrary.RunSuiteQlPagedAsync(a_accountId, sql, 1000);
                await SqlDump.DumpAsync(data, a_dbConnectionString, "Netsuite_SalesOrders", a_schema);
            }

            if (a_purchaseOrders)
            {
                string sql = suiteReader.Read("Netsuite_PurchaseOrders.sql");
                JArray data = await netSuiteApiLibrary.RunSuiteQlPagedAsync(a_accountId, sql, 1000);
                await SqlDump.DumpAsync(data, a_dbConnectionString, "Netsuite_PurchaseOrders", a_schema);
            }


            //foreach (Endpoint ep in NetsuiteImportEndpoints.AllFor(a_accountId))
            //{
            //    switch (ep.Name)
            //    {
            //        case "WorkOrders":
            //            {
            //                List<WorkOrder> rows = await netSuiteApiLibrary.GetListAsync<WorkOrder>(ep.Url);
            //                await SqlDump.DumpAsync(rows, a_dbConnectionString, ep.TargetTable);
            //                break;
            //            }

            //        case "BillOfMaterials":
            //            {
            //                List<BillOfMaterial> rows = await netSuiteApiLibrary.GetListAsync<BillOfMaterial>(ep.Url);
            //                await SqlDump.DumpAsync(rows, a_dbConnectionString, ep.TargetTable);
            //                break;
            //            }

            //        case "StockOnHand":
            //            {
            //                List<StockOnHand> rows = await netSuiteApiLibrary.GetListAsync<StockOnHand>(ep.Url);
            //                await SqlDump.DumpAsync(rows, a_dbConnectionString, ep.TargetTable);
            //                break;
            //            }

            //        case "FinishedGoods":
            //            {
            //                List<FinishedGood> rows = await netSuiteApiLibrary.GetListAsync<FinishedGood>(ep.Url);
            //                await SqlDump.DumpAsync(rows, a_dbConnectionString, ep.TargetTable);
            //                break;
            //            }

            //        case "PurchaseOrders":
            //            {
            //                List<PurchaseOrder> rows = await netSuiteApiLibrary.GetListAsync<PurchaseOrder>(ep.Url);
            //                await SqlDump.DumpAsync(rows, a_dbConnectionString, ep.TargetTable);
            //                break;
            //            }

            //        case "SalesOrders":
            //            {
            //                List<SalesOrder> rows = await netSuiteApiLibrary.GetListAsync<SalesOrder>(ep.Url);
            //                await SqlDump.DumpAsync(rows, a_dbConnectionString, ep.TargetTable);
            //                break;
            //            }

            //        case "BOMRevisions":
            //            {
            //                List<BillOfMaterialsRevision> rows = await netSuiteApiLibrary.GetListAsync<BillOfMaterialsRevision>(ep.Url);
            //                await SqlDump.DumpAsync(rows, a_dbConnectionString, ep.TargetTable);
            //                break;
            //            }

            //        case "MFGRouting":
            //            {
            //                List<Mfgrouting> rows = await netSuiteApiLibrary.GetListAsync<Mfgrouting>(ep.Url);
            //                await SqlDump.DumpAsync(rows, a_dbConnectionString, ep.TargetTable);
            //                break;
            //            }

            //        case "RawMaterials":
            //            {
            //                List<RawMaterial> rows = await netSuiteApiLibrary.GetListAsync<RawMaterial>(ep.Url);
            //                await SqlDump.DumpAsync(rows, a_dbConnectionString, ep.TargetTable);
            //                break;
            //            }

            //        case "MaterialConsumptions":
            //            {
            //                List<MaterialConsumption> rows = await netSuiteApiLibrary.GetListAsync<MaterialConsumption>(ep.Url);
            //                await SqlDump.DumpAsync(rows, a_dbConnectionString, ep.TargetTable);
            //                break;
            //            }
            //    }
            //}

            return Task.CompletedTask;
        }
        
    }
}
