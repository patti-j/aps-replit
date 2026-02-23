using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using WebAPI.Common;
using WebAPI.DAL;
using WebAPI.Models;
using WebAPI.Models.Integration;
using WebAPI.RequestsAndResponses;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IntegrationsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly CompanyDBService _dbService;
        private const string AccessTokenHeader = "Access-Token";

        public IntegrationsController(IConfiguration configuration, CompanyDBService service)
        {
            _configuration = configuration;
            _dbService = service;
        }
        
        [HttpGet("[action]")]
        public async Task<ActionResult<IntegrationConfigDTO>> GetDataConnectorsForPACompany()
        {
            try
            {
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out Company company, out string _))
                {
                    return Unauthorized();
                }

                return Ok(_dbService.GetDataConnectorsForCompany(company.Id));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpGet("[action]")]
        public async Task<ActionResult<IntegrationConfigDTO>> GetDataConnector(int connectorId)
        {
            try
            {
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out Company _, out string _))
                {
                    return Unauthorized();
                }

                return Ok(_dbService.GetDataConnector(connectorId));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IntegrationConfigDetailsDTO>> ListIntegrationConfigs()
        {
            try
            {
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out Company integrationCompany, out string _))
                {
                    return Unauthorized();
                }

                IntegrationConfigDetailsDTO response = new IntegrationConfigDetailsDTO()
                {
                    IntegrationConfigs = _dbService.GetIntegrationConfigDescriptions(integrationCompany.Id)
                };

                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IntegrationConfigDTO>> GetIntegrationConfig(int integrationId)
        {
            try
            {
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out Company _, out string _))
                {
                    return Unauthorized();
                }

                return Ok(_dbService.GetIntegrationConfig(integrationId));
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        /// <summary>
        /// Gets all integration configs mapped to a specific planning area
        /// </summary>
        /// <param name="planningAreaKey"></param>
        /// <returns></returns>
        [HttpGet("[action]")]
        public async Task<ActionResult<List<IntegrationConfigDTO>>> GetIntegrationConfigsForInstance(string planningAreaKey)
        {
            try
            {
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out Company _, out string _))
                {
                    return Unauthorized();
                }

                return Ok(_dbService.GetIntegrationConfigs(planningAreaKey));

            } catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<int>> CreateIntegrationConfig(IntegrationConfigDTO config)
        {
            try
            {
                Company integrationCompany = null;
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out integrationCompany, out string _))
                {
                    return Unauthorized();
                }

                var id = _dbService.CreateIntegrationConfig(config.ToModel(integrationCompany.Id));

                return Ok(new { Content = id });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> UpdateIntegrationConfig(IntegrationConfigDTO config)
        {
            try
            {
                Company integrationCompany = null;
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out integrationCompany, out string _))
                {
                    return Unauthorized();
                }

                if (config.Id == 0)
                {
                    return BadRequest("Id must be non-zero");
                }

                _dbService.UpdateIntegrationConfig(config.ToModel(integrationCompany.Id));

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpPost("[action]")]
        public async Task<ActionResult> RenameIntegrationConfig(IntegrationConfigDetailDTO configDetail)
        {
            try
            {
                Company integrationCompany = null;
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out integrationCompany, out string _))
                {
                    return Unauthorized();
                }

                if (configDetail.IntegrationConfigId == 0)
                {
                    return BadRequest("Id must be non-zero");
                }

                _dbService.RenameIntegrationConfig(configDetail, integrationCompany);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult> MapIntegrationConfig(int integrationId, string planningAreaKey, bool deleteMapping = false)
        {
            try
            {
                Company integrationCompany = null;
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out integrationCompany, out string _))
                {
                    return Unauthorized();
                }

                if (integrationId == 0)
                {
                    return BadRequest("Id must be non-zero");
                }

                _dbService.MapIntegrationConfig(integrationId, planningAreaKey, deleteMapping);

                return Ok();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        [HttpPost("[action]")]
        public async Task<ActionResult<bool>> DeleteIntegrationConfig([FromBody] int integrationId)
        {
            try
            {
                Company integrationCompany = null;
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out integrationCompany, out string _))
                {
                    return Unauthorized();
                }

                if (integrationId == 0)
                {
                    return BadRequest("Id must be non-zero");
                }

                bool didDelete = _dbService.DeleteIntegrationConfig(integrationId);

                return Ok(new { Content = didDelete });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// This endpoint is used for whenever the PT system is creating a new config as an upgraded version of another config
        /// </summary>
        /// <param name="a_config">The upgraded version of a previous config, UpgradedFromConfigId must be set</param>
        /// <returns>The id of the config which has been created, or an error</returns>
        [HttpPost("[action]")]
        public async Task<ActionResult<int>> UpgradeIntegrationConfig(IntegrationConfigDTO a_config)
        {
            try
            {
                Company integrationCompany = null;
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService,
                        out integrationCompany, out string _))
                {
                    return Unauthorized();
                }

                if (a_config.UpgradedFromConfigId == null)
                {
                    return BadRequest("UpgradedFromConfigId must be non-zero");
                }
                var config = _dbService.GetIntegrationConfig(a_config.UpgradedFromConfigId.Value);
                if (config == null)
                {
                    return BadRequest("UpgradedFromConfigId is invalid");
                }

                int? configId = null;
                try
                {
                    configId = _dbService.UpgradeIntegrationConfig(a_config.ToModel(integrationCompany.Id));
                }
                catch (Exception e)
                {
                    return Problem(e.Message, title: "Failed to upgrade Integration Config");
                }

                return Ok(configId);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        // for dev testing only, needs to be updated to include some kind of permissions checking, uncomment if needed
        // [HttpGet("[action]")]
        // public async Task<ActionResult<DBIntegration>> GetAllDBIntegrationIds()
        // {
        //     try
        //     {
        //         Company integrationCompany = null;
        //         if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService,
        //                 out integrationCompany, out string _))
        //         {
        //             return Unauthorized();
        //         }
        //
        //         List<int> dbIntegrations = _dbService.GetAllDBIntegrationIds();
        //         
        //         return Ok(dbIntegrations);
        //     }
        //     catch (Exception e)
        //     {
        //         return BadRequest(e.Message);
        //     }
        // }
        
        [HttpGet("[action]")]
        public async Task<ActionResult<DBIntegration>> GetDBIntegration(int integrationId)
        {
            try
            {
                Company integrationCompany = null;
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService,
                        out integrationCompany, out string _))
                {
                    return Unauthorized();
                }
        
                DBIntegration dbIntegration = _dbService.GetDBIntegration(integrationId);
                if (dbIntegration == null)
                {
                    return BadRequest("Integration Id is invalid");
                }
                
                return Ok(dbIntegration);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        public record IntegrationCreateRequest(DBIntegrationDTO IntegrationDto, string a_email);
        //both this and the new version api are just going to use emails for auth for now, replace with real authentication as soon as we have a way to actually authenticate users
        [HttpPost("[action]")]
        public async Task<ActionResult<string>> CreateDBIntegration(IntegrationCreateRequest a_req)
        {
            try
            {
                Company integrationCompany = null;
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService,
                        out integrationCompany, out string _))
                {
                    return Unauthorized();
                }
        
                bool success = _dbService.CreateDBIntegration(a_req.IntegrationDto.ToModel(), a_req.a_email);
                if (success)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Failed to create Integration"); //should probably return 401 for permissions issues
                }
                
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        /// <summary>
        /// used for creating new versions of a specific integration, dbintegration should have the same id as an existing integration
        /// </summary>
        /// <param name="dbIntegration"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        public async Task<ActionResult> CreateNewVersionForDBIntegration(DBIntegration dbIntegration, string a_createdByUserEmail)
        {
            try
            {
                Company integrationCompany = null;
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService,
                        out integrationCompany, out string _))
                {
                    return Unauthorized();
                }
        
                bool success = _dbService.CreateNewVersionForDBIntegration(dbIntegration, a_createdByUserEmail);
                if (success)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Failed to update Integration");
                }
                
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        //currently unused
        [HttpPost("[action]")]
        public async Task<ActionResult<bool>> DeleteDBIntegration([FromBody] int integrationId)
        {
            try
            {
                Company integrationCompany = null;
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out integrationCompany, out string _))
                {
                    return Unauthorized();
                }

                if (integrationId == 0)
                {
                    return BadRequest("Id must be non-zero");
                }

                bool didDelete = _dbService.DeleteDBIntegration(integrationId);

                return Ok(new { Content = didDelete });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        public record BaseUserRequest(string userIdentifier);
        
        [HttpPost("GetAllPAsForIntegrator")]
        public async Task<ActionResult<GetAllPlanningAreaDetailsResponse>> GetPlanningAreaDetailsForIntegrator(BaseUserRequest a_request)
        {
            try
            {
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out Company _, out string _))
                {
                    return Unauthorized();
                } //TODO User level auth

                List<PADetails> pasForIntegrator = _dbService.GetAllPlanningAreasForIntegrator(a_request.userIdentifier) ?? new();
                var allIntegrationsForCompanies = _dbService.GetAllIntegrationsForCompanies(pasForIntegrator.Where(pa => pa.UsedByCompanyId != null).Select(pa => pa.UsedByCompanyId ?? 0).Distinct().ToList())
                                                            .GroupBy(i => i.CompanyId);
                var response = new GetAllPlanningAreaDetailsResponse()
                {
                    Companies = pasForIntegrator.GroupBy(pa => pa.CompanyId).Select(g => new PlanningAreaDetailsResponseCompany()
                    {   
                        CompanyName = g.FirstOrDefault()?.Company?.Name ?? string.Empty, //this shouldn't ever be null but *just in case*
                        CompanyId = g.Key,
                        PlanningAreaDetails = g.Select(pa => new GetPlanningAreaDetailsResponse()
                        {
                            Name = pa.Name,
                            Key = pa.PlanningAreaKey,
                            Version = pa.Version,
                            Environment = pa.Environment,
                            ActiveIntegrationId = pa.DBIntegrationId
                        }).ToList(),
                        DBIntegrations = allIntegrationsForCompanies.FirstOrDefault(e => e.Key == g.Key)?.Select(i => i.ToDTO()).ToList() ?? new()
                    }).ToList()
                };
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        public record IntegrationUserRequest(int integrationId, string userIdentifier);
        
        [HttpPost("GetDBIntegration")]
        public async Task<ActionResult<DBIntegrationDTO>> GetIntegration(IntegrationUserRequest a_request)
        {
            try
            {
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out Company _, out string _))
                {
                    return Unauthorized();
                } //TODO User level auth

                DBIntegrationDTO? integration = _dbService.GetIntegrationForUser(a_request.integrationId, a_request.userIdentifier)?.ToDTO();
                if (integration == null)
                {
                    return BadRequest("Failed to get integration");
                }
                return Ok(integration);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        public record IntegrationPARequest(string paKey, string userIdentifier);
        
        // [HttpPost("GetIntegrationFromPA")]
        // public async Task<ActionResult<DBIntegrationDTO>> GetIntegrationFromPA(IntegrationPARequest a_request)
        // {
        //     try
        //     {
        //         if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out Company _, out string _))
        //         {
        //             return Unauthorized();
        //         } //TODO User level auth
        //
        //         DBIntegrationDTO? integration = _dbService.GetIntegrationForPA(a_request.paKey, a_request.userIdentifier)?.ToDTO();
        //         if (integration == null)
        //         {
        //             return BadRequest("Failed to get integration");
        //         }
        //         return Ok(integration);
        //     }
        //     catch (InvalidDataException e)
        //     {
        //         return BadRequest(e.Message);
        //     }
        // }
        
        public record IntegrationApplyRequest(int integrationId, string userIdentifier, string PAKey);
        
        [HttpPost("[action]")]
        public async Task<ActionResult<bool>> ApplyIntegration(IntegrationApplyRequest a_request)
        {
            try
            {
                Company integrationCompany = null;
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out integrationCompany, out string _))
                {
                    return Unauthorized();
                } //todo user level auth

                if (a_request.integrationId == 0)
                {
                    return BadRequest("Id must be non-zero");
                }

                var dbIntegration = _dbService.GetDBIntegration(a_request.integrationId);

                if (dbIntegration == null)
                {
                    return BadRequest("Failed to get integration");
                }

                List<PADetails> PAs = _dbService.GetAllPlanningAreasForIntegrator(a_request.userIdentifier);

                PADetails? pa = PAs.FirstOrDefault(pa => pa.PlanningAreaKey == a_request.PAKey);

                if (pa == null)
                {
                    return BadRequest("Could not find Planning Area");
                }

                //TODO: we may want to check PA status and require a restart if started/active
                
                var settingsString = _dbService.GetPlanningAreaSettings(pa.PlanningAreaKey);

                var obj = ((JObject)JsonConvert.DeserializeObject<JObject>(settingsString));
                
                string conString = (string)(obj["Settings"]["ErpDatabaseSettings"]["ConnectionString"]); //is it worth it bring over all of the classes from core to avoid doing JObject crap

                if (conString == null)
                {
                    return BadRequest("Connection string not configured");
                }
                
                string IntegrationUserCreds = (string)(obj["Settings"]["ErpDatabaseSettings"]["IntegrationUserCreds"]);

                if (!string.IsNullOrEmpty(IntegrationUserCreds))
                {
                    var builder = new SqlConnectionStringBuilder(conString);
                    builder.UserID = IntegrationUserCreds.Split('\0')[0];
                    builder.Password = IntegrationUserCreds.Split('\0')[1];
                    builder.IntegratedSecurity = false;
                    conString = builder.ToString();
                }

                using SqlConnection con = new SqlConnection(conString);
                try
                {
                    DbSchemaExtractor.UploadSchemaToDb(con, dbIntegration.ToDTO());
                }
                catch (Exception e)
                {
                    //todo handle exceptions
                    throw e;
                }

                bool updated = _dbService.SetActiveDBIntegration(pa.PlanningAreaKey, a_request.integrationId);

                if (!updated)
                {
                    //if we get here and this fails something *very* bad has happened, no idea how to handle it
                }

                return Ok(new { Content = true});
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpPost("GetIntegrationDataFromPA")]
        public async Task<ActionResult<GetIntegrationDataResponse>> GetIntegrationDataFromPA(IntegrationPARequest a_request)
        {
            try
            {
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out Company _, out string _))
                {
                    return Unauthorized();
                } //TODO User level auth

                var data = _dbService.GetIntegrationDataForPA(a_request.paKey, a_request.userIdentifier);
                if (data == null)
                {
                    return BadRequest("Failed to get integration");
                }
                return Ok(data);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        
        [HttpPost("GetIntegrationDataFromDataConnector")]
        public async Task<ActionResult<GetIntegrationDataResponse>> GetIntegrationDataFromDataConnector(IntegrationUserRequest a_request)
        {
            try
            {
                if (!CommonMethods.ValidateInstanceApiRequest(Request.Headers, _dbService, out Company _, out string _))
                {
                    return Unauthorized();
                } //TODO User level auth

                
                var data = _dbService.GetIntegrationDataForDataConnector(a_request.integrationId, a_request.userIdentifier);
                if (data == null)
                {
                    return BadRequest("Failed to get integration");
                }
                return Ok(data);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    
    }
    
    
    
}
