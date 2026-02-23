using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using AhaIntegration;
using AhaIntegration.ResponseObjects;

using NetsuiteIntegration;

using ExternalIntegrations.WebAppClients;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Linq;

using PlanetTogetherContext.Contexts;
using PlanetTogetherContext.Entities;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExternalIntegrations.Controllers
{
    [ApiController]
    [Route("")]
    public class ImportController : ControllerBase
    {
        private readonly ILogger<ImportController> m_logger;
        private readonly PTContext m_ptContext;

        //TODO: no need for these?
        //private static TimeSpan c_defaultSprintDuration = TimeSpan.FromDays(12);
        //private const string c_notPlannedRelease = "Not Planned Backlog";
        //private const string c_integratedProductId = "7491062429825496584";

        public ImportController(ILogger<ImportController> a_logger)
        {
            m_logger = a_logger;            
        }

        [HttpPost("{CompanyId}/{PlanningAreaId}/{IntegrationId}/api/Rest/Import")]
        public async Task<IActionResult> Import(int CompanyId,int PlanningAreaId, int IntegrationId)
        {
            WebAppClient webAppClient = new WebAppClient();            
            ImportIntegrationExecutionPolicy importIntegrationExecution = new ImportIntegrationExecutionPolicy();

            ImportIntegrationExecutionPolicy policy = new ImportIntegrationExecutionPolicy();
            ImportIntegrationExecutionPolicy.ImportIntegrationExecutionResult result = await policy.GetImportUserAndLogic(CompanyId, PlanningAreaId);

            bool useSeparate = result.a_useSeparateDatabases;
            string? creds = result.a_importIntegrationUserAndPass;            
            string schemaName = result.a_useSeparateDatabases == true ? "import" : "dbo";

            switch (IntegrationId)
            {
                case 1:
                    AhaProperties ahaProperties = await webAppClient.WebAppProperties<AhaProperties>(CompanyId, PlanningAreaId, "Import");
                    AhaImport ahaImport = new AhaImport();                    

                    SqlConnectionStringBuilder ahaBuilder = new SqlConnectionStringBuilder(ahaProperties.DbConnectionString);                    

                    if (!string.IsNullOrWhiteSpace(creds))
                    {
                        string[] userPass = creds.Split(new[] { "||" }, StringSplitOptions.None);

                        if (userPass.Length == 2 &&
                            !string.IsNullOrWhiteSpace(userPass[0]) &&
                            !string.IsNullOrWhiteSpace(userPass[1]))
                        {
                            ahaBuilder.UserID = userPass[0];
                            ahaBuilder.Password = userPass[1];
                            ahaProperties.DbConnectionString = ahaBuilder.ConnectionString;
                        }
                    }                    

                    //

                    Task ahaResult = await ahaImport.ImportData(ahaProperties.Username, ahaProperties.Password, ahaProperties.DbConnectionString, ahaProperties.APIToken, schemaName);
                    break;

                case 2:
                    //Maestro
                    break;

                case 3:
                    NetsuiteProperties netsuiteProperties = await webAppClient.WebAppProperties<NetsuiteProperties>(CompanyId, PlanningAreaId, "Import");
                    NetsuiteImport netsuiteImport = new NetsuiteImport();

                    SqlConnectionStringBuilder netsuiteBuilder = new SqlConnectionStringBuilder(netsuiteProperties.DbConnectionString);

                    if (!string.IsNullOrWhiteSpace(creds))
                    {
                        string[] userPass = creds.Split(new[] { "||" }, StringSplitOptions.None);

                        if (userPass.Length == 2 &&
                            !string.IsNullOrWhiteSpace(userPass[0]) &&
                            !string.IsNullOrWhiteSpace(userPass[1]))
                        {
                            netsuiteBuilder.UserID = userPass[0];
                            netsuiteBuilder.Password = userPass[1];
                            netsuiteProperties.DbConnectionString = netsuiteBuilder.ConnectionString;
                        }
                    }                    

                    //                    

                    Task netsuiteResult = await netsuiteImport.ImportData(netsuiteProperties.Credentials.AccountId, netsuiteProperties.Credentials.ClientId, netsuiteProperties.Credentials.ClientSecret, netsuiteProperties.Credentials.TokenId, netsuiteProperties.Credentials.TokenSecret, netsuiteProperties.DbConnectionString, netsuiteProperties.ImportEndpoints.URLs, netsuiteProperties.PublishEndpoint.URL, netsuiteProperties.WorkOrders, netsuiteProperties.BOMs, netsuiteProperties.Routings, netsuiteProperties.Items, netsuiteProperties.PurchaseOrders, netsuiteProperties.SalesOrders, schemaName);
                    break;

                case 4:
                    //D365
                    break;
            }
            
            return Ok();
        }

    }
}
