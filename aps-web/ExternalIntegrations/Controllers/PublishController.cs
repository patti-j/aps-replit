using AhaIntegration;
using AhaIntegration.ResponseObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PlanetTogetherContext.Contexts;
using SqlLibrary;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ExternalIntegrations.WebAppClients;
using NetsuiteIntegration;

namespace ExternalIntegrations.Controllers
{
    [ApiController]
    [Route("")]
    public class PublishController : ControllerBase
    {
        private readonly ILogger<PublishController> m_logger;
        private readonly IConfiguration m_configuration;
        private readonly PTContext m_ptContext;
        private readonly string m_token;
        private readonly string m_sqlConnectionString;
        //private const string c_plannedBacklogRelease = "Planned Backlog";
        //private const string c_integratedProductId = "6786756851059831831";

        public PublishController(ILogger<PublishController> a_logger, IConfiguration a_configuration, PTContext a_ptContext)
        {
            m_logger = a_logger;
            m_configuration = a_configuration;
            m_ptContext = a_ptContext;
            m_token = m_configuration.GetValue<string>("ApiToken");
            m_sqlConnectionString = m_configuration.GetConnectionString("PublishedSQLConnection");
        }

        [HttpPost("{CompanyId}/{PlanningAreaId}/{IntegrationId}/api/Rest/Publish")]
        public async Task<IActionResult> Publish(int CompanyId, int PlanningAreaId, int IntegrationId)
        {
            WebAppClient webAppClient = new WebAppClient();

            //WebAppDBProperties properties = await webAppClient.WebAppProperties(CompanyId, PlanningAreaId, "Publish");

            switch (IntegrationId)
            {
                case 1:
                    AhaProperties ahaProperties = await webAppClient.WebAppProperties<AhaProperties>(CompanyId, PlanningAreaId, "Publish");
                    AhaPublish ahaPublish = new AhaPublish();
                    Task ahaResult = await ahaPublish.PublishData(ahaProperties.Username, ahaProperties.Password, ahaProperties.DbConnectionString, ahaProperties.APIToken);
                    break;

                case 2:
                    //Maestro
                    break;

                case 3:
                    //Netsuite
                    NetsuiteProperties netsuiteProperties = await webAppClient.WebAppProperties<NetsuiteProperties>(CompanyId, PlanningAreaId, "Publish");
                    NetsuitePublish netsuitePublish = new NetsuitePublish();
                    Task netsuiteResult = await netsuitePublish.PublishData(netsuiteProperties.Credentials.AccountId, netsuiteProperties.Credentials.ClientId, netsuiteProperties.Credentials.ClientSecret, netsuiteProperties.Credentials.TokenId, netsuiteProperties.Credentials.TokenSecret, netsuiteProperties.DbConnectionString, netsuiteProperties.PublishEndpoint.URL);
                    break;

                case 4:
                    //D365
                    break;
            }

            return Ok();
        }

    }
}
