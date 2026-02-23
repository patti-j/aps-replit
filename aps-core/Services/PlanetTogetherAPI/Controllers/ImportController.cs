using System.Collections.Concurrent;
using System.Data;
using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

using PT.APIDefinitions;
using PT.APIDefinitions.RequestsAndResponses;
using PT.APIDefinitions.RequestsAndResponses.Webapp;
using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Common.Http;
using PT.Common.Http.Json;
using PT.Common.Sql.SqlServer;
using PT.Common.Threading;
using PT.ImportDefintions;
using PT.ImportDefintions.RequestsAndResponses;
using PT.PackageDefinitions.PackageInterfaces;
using PT.PlanetTogetherAPI.APIs;
using PT.PlanetTogetherAPI.Importing;
using PT.PlanetTogetherAPI.Server;
using PT.Scheduler;
using PT.ServerManagerAPIProxy.APIClients;
using PT.Transmissions;

namespace PT.PlanetTogetherAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController : SessionControllerBase
{
    [HttpPost]
    [Route("RunERPImport")]
    public ActionResult<PerformImportResponse> RunERPImport(PerformImportRequest a_request)
    {
        PerformImportResponse response = new ()
        {
            Content = SystemController.ImportingService.RunImport(a_request)
        };

        return Ok(response);
    }

    /// <summary>
    /// Pulls a table's worth of data from the import database using the provided SQL.
    /// Newer import functionality can also provide business-layer validation to that result; use <see cref="GetBrowseTableWithValidate"/> at POST /GetBrowseTableWithValidation
    /// </summary>
    /// <param name="a_commandText"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("GetBrowseTable")]
    public ActionResult<DataTableJson> GetBrowseTable(string a_commandText)
    {
        DataTableJson browseTable = SystemController.ImportingService.GetBrowseTable(a_commandText);
        return Ok(browseTable);
    }

    [HttpPost]
    [Route("GetBrowseTable")]
    public ActionResult<DataTableJson> GetBrowseTableWithValidate(BrowseTableWithValidationRequest a_request)
    {
        if (SystemController.ImportingService is ImportingService)
        {
            return BadRequest("This endpoint is not usable with older versions of the importer service. Use GET /GetBrowseTable");
        }

        DataTableJson browseTable = SystemController.ImportingService.GetBrowseTable(a_request.CommandText, a_request.IncludeValidation, a_request.TableName, a_request.ImportSettings);
        return Ok(browseTable);
    }

    [HttpGet]
    [Route("GetImportSettings")]
    public ActionResult<ImportSettings> GetSettings()
    {
        return Ok(SystemController.ImportingService.GetImportSettings());
    }
    
    [HttpGet]
    [Route("GetNewImportSettings")]
    public ActionResult<NewImportSettings> GetNewImportSettings(int a_scenarioId)
    {
        return Ok(SystemController.ImportingService.GetNewImportSettings(a_scenarioId));
    }
    
    [HttpGet]
    [Route("GetIsUsingNewImport")]
    public ActionResult<BoolResponse> GetIsUsingNewImport()
    {
        if (SystemController.ImportingService is NewImportingService)
        {
            return Ok(new BoolResponse() { Content = true });
        }
        else
        {
            return Ok(new BoolResponse() { Content = false });
        }
    }
    
    [HttpPost]
    [Route("SaveNewImportSettings")]
    public ActionResult<IntResponse> SaveNewImportSettings(SaveImportSettingsRequest a_request)
    {
        int configId = SystemController.ImportingService.SaveNewImportSettings(a_request.ScenarioId, a_request.ImportSettings);
        return Ok(new IntResponse() { Content = configId });
    }

    [HttpPost]
    [Route("SaveImportSettings")]
    public ActionResult<BoolResponse> SaveImportSettings(ImportSettings a_settings)
    {
        SystemController.ImportingService.SaveImportSettings(a_settings);
        return Ok(new BoolResponse { Content = true });
    }
    
    [HttpPost]
    [Route("RefreshStagingData")]
    public ActionResult<BoolResponse> RefreshStagingData((BaseId instigator, BaseId targetScenarioId) a_stagingData)
    {
        if (SystemController.ImportingService is NewImportingService newImportingService) //I dont want to add to the interface, esp since this is never going to get added to the old importer
        {
            newImportingService.RefreshStagingData(a_stagingData.instigator, a_stagingData.targetScenarioId);
            return Ok(new BoolResponse { Content = true });
        }

        return Ok(new BoolResponse { Content = false });
    }
    
    [HttpPost]
    [Route("TriggerStagingDBSchemaRetrieve")]
    public ActionResult<IntResponse> TriggerStagingDBSchemaRetrieve()
    {
        if (SystemController.ImportingService is NewImportingService newImportingService) //I dont want to add to the interface, esp since this is never going to get added to the old importer
        {
            int? val = newImportingService.TriggerStagingDBSchemaRetrieve();
            if (val == null)
            {
                return BadRequest();
            }
            return Ok(new IntResponse() { Content = val.Value });
        }

        return NotFound();
    }
    
    [HttpGet]
    [Route("GetDataConnectorsForCompany")]
    public List<DataConnector> GetDataConnectorsForCompany()
    { 
        return SystemController.WebAppActionsClient.GetDataConnectorsForCompany();
    }
    
    [HttpPost]
    [Route("CreateIntegrationFromStagingDB")]
    public ActionResult<BoolResponse> CreateIntegrationFromStagingDB(CreateIntegrationRequest a_request)
    {
        if (SystemController.ImportingService is NewImportingService newImportingService) //I dont want to add to the interface, esp since this is never going to get added to the old importer
        {
            ConnectedUserData userData = SystemController.ServerSessionManager.GetLoggedInUserData(UserToken);
            Task<bool> runTask = newImportingService.CreateIntegrationFromStagingDB(a_request, userData.ReadableName);
            runTask.Wait();
            bool val = runTask.Result;
            if (!val)
            {
                return BadRequest();
            }
            return Ok(new BoolResponse() { Content = val });
        }

        return NotFound();
    }
    
    [HttpPost]
    [Route("ApplyIntegrationLocally")]
    public ActionResult<IntResponse> ApplyIntegration(ApplyIntegrationRequest a_data)
    {
        if (SystemController.ImportingService is NewImportingService newImportingService) //I dont want to add to the interface, esp since this is never going to get added to the old importer
        {
            ConnectedUserData userData = SystemController.ServerSessionManager.GetLoggedInUserData(UserToken);
            DBIntegrationDTO integration = SystemController.WebAppActionsClient.GetDBIntegration(a_data.IntegrationId, userData.ReadableName);
            int? val = newImportingService.ApplyIntegration(integration);
            if (val == null)
            {
                return BadRequest();
            }
            return Ok(new IntResponse() { Content = val.Value });
        }

        return NotFound();
    }

    [HttpGet]
    [AuthorizeWithClaim("AppUser", "False")]
    [Route("GetImportStatus")]
    public ActionResult<ImportStatusMessage> GetImportStatus()
    {
        ImportStatusMessage status = SystemController.ServerSessionManager.GetCurrentImportStatus();

        if (status == null)
        {
            return NoContent();
        }

        return Ok(status);
    }
    
    [HttpGet]
    [Route("QueryStagingDBSchema")]
    public ActionResult<StagingDBSchemaResponse> QueryStagingDBSchema(int a_retrievalId)
    {
        if (SystemController.ImportingService is NewImportingService newImportingService) //I dont want to add to the interface, esp since this is never going to get added to the old importer
        {
            return Ok(newImportingService.QueryStagingDBSchemaRetrieve(a_retrievalId));
        }

        return NotFound();
    }
    
    
    [HttpGet]
    [Route("GetAllPlanningAreasForIntegrator")]
    public ActionResult<GetAllPlanningAreaDetailsResponse> GetAllPlanningAreasForIntegrator()
    {
        try
        {
            
            ConnectedUserData userData = SystemController.ServerSessionManager.GetLoggedInUserData(UserToken);
            return Ok(SystemController.WebAppActionsClient.GetAllPlanningAreasForIntegrator(userData.ReadableName)); //username is email when SSO is enabled
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "DBIntegration".Localize(), "System".Localize(), apie));
        }
    }
    
    [HttpGet]
    [Route("ApplyIntegrationLocallyFromPA")]
    public ActionResult<BoolResponse> ApplyIntegrationLocallyFromPA(string a_paKey)
    {
        try
        {
            ConnectedUserData userData = SystemController.ServerSessionManager.GetLoggedInUserData(UserToken);
            WebAppActionsClient.GetIntegrationDataResponse integrationAndData = SystemController.WebAppActionsClient.GetDBIntegrationDataFromPAKey(a_paKey, userData.ReadableName);
            if (SystemController.ImportingService is NewImportingService newImportingService) //I dont want to add to the interface, esp since this is never going to get added to the old importer
            {
                bool? isSuccess = newImportingService.ApplyIntegrationAndData(integrationAndData);
                if (isSuccess == null)
                {
                    return BadRequest("Failed to Apply Integration");
                }
                return Ok(new BoolResponse() { Content = isSuccess.Value });
            }
            else
            {
                return BadRequest("This endpoint is only supported in IntegrationV2");
            }
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "DBIntegration".Localize(), "System".Localize(), apie));
        }
    }
    
    [HttpGet]
    [Route("ApplyIntegrationLocallyFromDataConnector")]
    public ActionResult<BoolResponse> ApplyIntegrationLocallyFromDataConnector(int a_dataConnectorId)
    {
        try
        {
            ConnectedUserData userData = SystemController.ServerSessionManager.GetLoggedInUserData(UserToken);
            WebAppActionsClient.GetIntegrationDataResponse integrationAndData = SystemController.WebAppActionsClient.GetDBIntegrationDataFromDataConnector(a_dataConnectorId, userData.ReadableName);
            if (SystemController.ImportingService is NewImportingService newImportingService) 
            {
                bool? isSuccess = newImportingService.ApplyIntegrationAndData(integrationAndData);
                if (isSuccess == null)
                {
                    return BadRequest("Failed to Apply Integration");
                }
                return Ok(new BoolResponse() { Content = isSuccess.Value });
            }
            else
            {
                return BadRequest("This endpoint is only supported in IntegrationV2");
            }
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "DBIntegration".Localize(), "System".Localize(), apie));
        }
    }

    //I'm not really sure how long the cache should be valid for. We don't have any way of knowing when things change externally So a long time isn't good since users would be 
    //frustrated by the WebApp and Client showing different information. But these values are only fetched semi-periodically, so a short cache time could mean that the cache is almost never hit!
    private const int c_cacheExpireTimeInMinutes = 10;

    class IntegrationConfigCacheObject(IntegrationConfigDTO m_dto, DateTime m_cacheTime, object m_lock)
    {
        public IntegrationConfigDTO Dto = m_dto; 
        public DateTime CacheTime = m_cacheTime;
        public object Lock = m_lock;
    };
    
    // These variables have to be static as the ImportController seems to be created and destroyed many many times over the lifetime of the application. I don't know if this is intended behavior or not.
    private static ConcurrentDictionary<int, IntegrationConfigCacheObject> m_integrationConfigCache = new ConcurrentDictionary<int, IntegrationConfigCacheObject>();
    
    [HttpGet]
    [Route("GetIntegrationConfig")]
    public ActionResult<IntegrationConfigDTO> GetIntegrationConfig(int a_configId)
    {
        _ = m_integrationConfigCache.GetOrAdd(a_configId, new IntegrationConfigCacheObject(SystemController.WebAppActionsClient.GetIntegrationConfig(a_configId), DateTime.UtcNow.AddMinutes(c_cacheExpireTimeInMinutes), new ()));
        
        lock (m_integrationConfigCache[a_configId].Lock)
        {
            if (m_integrationConfigCache[a_configId].CacheTime < DateTime.UtcNow)
            {
                try
                {
                    m_integrationConfigCache[a_configId].Dto = SystemController.WebAppActionsClient.GetIntegrationConfig(a_configId);
                    m_integrationConfigCache[a_configId].CacheTime = DateTime.UtcNow.AddMinutes(c_cacheExpireTimeInMinutes);
                }
                catch (ApiException e)
                {
                    throw new PTHandleableException(string.Format("4493".Localize(), "IntegrationConfig".Localize(), "System".Localize(), e));
                }
            }
            return Ok(m_integrationConfigCache[a_configId].Dto);
        }
    }
    
    private static List<IntegrationConfigOptionsDTO> m_configOptionsCache = new List<IntegrationConfigOptionsDTO>();
    private static DateTime? m_configOptionsCacheExpiry = null;
    private static object m_configOptionsCacheLock = new object();
    
    [HttpGet]
    [Route("GetIntegrationConfigOptions")]
    public ActionResult<List<IntegrationConfigOptionsDTO>> GetIntegrationConfigOptions()
    {
        if (m_configOptionsCacheExpiry == null)
        {
            UpdateConfigOptionsCache();
        }

        lock (m_configOptionsCacheLock)
        {
            if (DateTime.UtcNow > m_configOptionsCacheExpiry)
            {
                try
                {
                    UpdateConfigOptionsCache();
                    return Ok(m_configOptionsCache);
                }
                catch (ApiException apie)
                {
                    throw new PTHandleableException(string.Format("4493".Localize(), "IntegrationConfig".Localize(), "System".Localize(), apie));
                }
            }
            else
            {
                return Ok(m_configOptionsCache);
            }
        }
    }

    private void UpdateConfigOptionsCache()
    {
        m_configOptionsCache = SystemController.WebAppActionsClient.GetIntegrationConfigOptions();
        m_configOptionsCacheExpiry = DateTime.UtcNow.AddMinutes(c_cacheExpireTimeInMinutes);
    }
    
    [HttpPost]
    [Route("CreateIntegrationConfig")]
    public ActionResult<IntResponse> CreateIntegrationConfig(IntegrationConfigDTO a_config)
    {
        try
        {
            IntResponse intResponse = new IntResponse() {Content = SystemController.WebAppActionsClient.CreateIntegrationConfig(a_config)};
            Task.Run(() =>
            {
                lock (m_configOptionsCacheLock)
                {
                    UpdateConfigOptionsCache();
                }
            });
            a_config.Id = intResponse.Content;
            m_integrationConfigCache.GetOrAdd(intResponse.Content, new IntegrationConfigCacheObject(a_config, DateTime.UtcNow.AddMinutes(c_cacheExpireTimeInMinutes), new()));
            return Ok(intResponse);
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "IntegrationConfig".Localize(), "System".Localize(), apie));
        }
    }
    
    [HttpPost]
    [Route("RenameIntegrationConfig")]
    public ActionResult<BoolResponse> RenameIntegrationConfig(IntegrationConfigOptionsDTO a_config)
    {
        try
        {
            SystemController.WebAppActionsClient.RenameIntegrationConfig(a_config); //this throws on failure
            Task.Run(() =>
            {
                lock (m_configOptionsCacheLock)
                {
                    UpdateConfigOptionsCache();
                }
            });
            Task.Run(() =>
            {
                lock (m_integrationConfigCache[a_config.IntegrationConfigId].Lock)
                {
                    m_integrationConfigCache[a_config.IntegrationConfigId].Dto = SystemController.WebAppActionsClient.GetIntegrationConfig(a_config.IntegrationConfigId);
                    m_integrationConfigCache[a_config.IntegrationConfigId].CacheTime = DateTime.UtcNow.AddMinutes(c_cacheExpireTimeInMinutes);
                }
            });

            return Ok(new BoolResponse() {Content = true});
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "IntegrationConfig".Localize(), "System".Localize(), apie));
        }
    }
    
    [HttpPost]
    [Route("UpdateIntegrationConfig")]
    public ActionResult<BoolResponse> UpdateIntegrationConfig(IntegrationConfigDTO a_config)
    {
        try
        {
            SystemController.WebAppActionsClient.UpdateIntegrationConfig(a_config); //this throws on failure
            Task.Run(() =>
            {
                lock (m_configOptionsCacheLock)
                {
                    UpdateConfigOptionsCache();
                }
            });
            Task.Run(() =>
            {
                lock (m_integrationConfigCache[a_config.Id].Lock)
                {
                    m_integrationConfigCache[a_config.Id].Dto = SystemController.WebAppActionsClient.GetIntegrationConfig(a_config.Id);
                    m_integrationConfigCache[a_config.Id].CacheTime = DateTime.UtcNow.AddMinutes(c_cacheExpireTimeInMinutes);
                }
            });
            return Ok(new BoolResponse() {Content = true});
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "IntegrationConfig".Localize(), "System".Localize(), apie));
        }
    }

    [HttpPost]
    [Route("DeleteIntegrationConfig")]
    public ActionResult<BoolResponse> DeleteIntegrationConfig(IntegrationConfigDTO a_config)
    {
        try
        {
            SystemController.WebAppActionsClient.DeleteIntegrationConfig(a_config.Id); //this throws on failure
            Task.Run(() =>
            {
                UpdateConfigOptionsCache();

            });
            return Ok(new BoolResponse() { Content = true });
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "IntegrationConfig".Localize(), "System".Localize(), apie));
        }
    }
}