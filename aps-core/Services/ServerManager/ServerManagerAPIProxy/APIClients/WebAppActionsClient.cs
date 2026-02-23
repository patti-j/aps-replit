using System.Net;

using Microsoft.AspNetCore.Mvc;

using PT.APIDefinitions.RequestsAndResponses;
using PT.APIDefinitions.RequestsAndResponses.Webapp;
using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Common.Http;
using PT.Common.Http.Json;
using PT.Common.Sql.SqlServer;
using PT.ServerManagerSharedLib.Data;
using PT.ServerManagerSharedLib.Definitions;

namespace PT.ServerManagerAPIProxy.APIClients;

// TODO: This client should only be used Server-side. Remove calls in packages (other than the ones in CTPPackage, which use it in server-only components), and introduce middle api layer on server
public class WebAppActionsClient : PTHttpClient
{
    private const string c_webAppAzureKeyName = "AzureAPIKey";
    private const string c_webAppAzureKeyValue = "Pl@n3t10geth3r";
    private const string c_webAppUtilityController = "api/Utility";
    private const string c_webAppUsersController = "api/WebAppUsers";
    private const string c_webAppCtpController = "api/Ctp";
    private const string c_webAppIntegrationsController = "api/Integrations";
    private const string c_webAppInstanceApiKeySchemeAuth = "ApiKey";
    private const string c_webAppInstanceIdSchemeAuth = "InstanceTokenScheme";
    private const string c_loggingController = "api/Logging";
    private const string c_webAppPlanningAreasController = "/api/PlanningAreas";
    private readonly string m_instanceId;

    public WebAppActionsClient(string a_serverAddress, string a_instanceId, string a_apiKey) 
        : base(c_webAppUsersController, a_serverAddress)
    {
        m_instanceId = a_instanceId;
        m_httpClient.DefaultRequestHeaders.Add(c_webAppAzureKeyName, c_webAppAzureKeyValue);
        m_httpClient.DefaultRequestHeaders.Add(c_webAppInstanceApiKeySchemeAuth, a_apiKey);
        m_httpClient.DefaultRequestHeaders.Add(c_webAppInstanceIdSchemeAuth, a_instanceId);
    }

    /// <summary>
    /// Wrapper for the constructor, returns null if the system is not properly configured to receive WebApp traffic or it cannot be reached.
    /// In such a case, this returns null - the system is designed to fall back on previous behavior whenever this is the case.
    /// </summary>
    /// <returns></returns>
    public static WebAppActionsClient TryInitWebAppClient(string a_serverEnvironmentCode, string a_instanceId, string a_apiKey)
    {
        WebAppActionsClient client;
        if (!string.IsNullOrEmpty(a_serverEnvironmentCode))
        {
            try
            {
                string webAppUrlForEnv = WebAppInstanceSettingsManager.GetURLForEnv(a_serverEnvironmentCode[0]);
                client = new WebAppActionsClient(webAppUrlForEnv, a_instanceId, a_apiKey);
                if (client.Ping())
                {
                    return client;
                }
            }
            catch
            {
                // Fall through
                DebugException.ThrowInDebug("Webapp is configured but not reachable with provided values.");
            }
        }
        //TODO uncomment line when ready to fully login using the web app
        //throw new ServerManagerException("4503");
        return null;
    }


    public bool Ping()
    {
        try
        {
            MakeGetRequest<object>("Ping", c_webAppUtilityController, null);
            return true;
        }
        catch (ApiException apie)
        {
            return false;
        }
    }

    #region Users

    public UserDto GetUserForLogin(string a_email)
    {
        UserDto userReponse;
        UserRequest userRequest = new UserRequest()
        {
            PlanningAreaKey = m_instanceId,
            Email = a_email
        };

        try
        {
            userReponse = MakePostRequest<UserDto>("AuthenticateWebAppUser", userRequest);
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4492".Localize(), userRequest.Email, apie));
        }

        return userReponse;
    }
    
    public UserDto GetUserForLogin(int a_id)
    {
        UserDto userReponse;
        UserRequest userRequest = new UserRequest()
        {
            PlanningAreaKey = m_instanceId,
            UserId = a_id
        };

        try
        {
            userReponse = MakePostRequest<UserDto>("AuthenticateWebAppUser", userRequest);
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4492".Localize(), userRequest.Email, apie));
        }

        return userReponse;
    }

    public List<UserDto> GetAllUsers()
    {
        List<UserDto> usersReponse;

        try
        {
            usersReponse = MakeGetRequest<List<UserDto>>("GetWebAppUsers", null, new GetParam { Name = "planningAreaKey", Value = m_instanceId });
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4492".Localize(), "All Users".Localize(), apie));
        }

        return usersReponse;
    }

    #endregion
    
    #region Ctp

    public CtpRequest GetNextCtpRequest()
    {
        CtpRequest nextRequest = null;

        try
        {
            WebAppPlanningAreaRequest request = new WebAppPlanningAreaRequest() { PlanningAreaKey = m_instanceId };
            return MakePostRequest<CtpRequest>("getNextRequest", request, c_webAppCtpController);
        }
        catch (ApiException apie)
        {
            if (apie.StatusCode == (int)HttpStatusCode.NoContent)
            {
                // Success, but no requests in the queue
                return null;
            }
            else
            {
                throw new PTHandleableException(string.Format("4493".Localize(), "CTP".Localize(), "System".Localize(), apie));
            }
        }
    }

    public void SendCompletedCtpRequest(CtpResponse a_ctpResponse)
    {
        try
        {
            WebAppPlanningAreaRequest request = new WebAppPlanningAreaRequest() { PlanningAreaKey = m_instanceId };
            MakePutRequest("updateCTPRequest", request, c_webAppCtpController);
        }
        catch (ApiException apie)
        {
                throw new PTHandleableException(string.Format("4493".Localize(), "CTP".Localize(), "System".Localize(), apie));
        }
    }

    #endregion

    #region Integration

    public List<IntegrationConfigOptionsDTO> GetIntegrationConfigOptions()
    {
        List<IntegrationConfigOptionsDTO> options;

        try
        {
            options = MakeGetRequest<IntegrationConfigOptionsResponse>("ListIntegrationConfigs", c_webAppIntegrationsController)?.IntegrationConfigs;
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "IntegrationConfigList".Localize(), "System".Localize(), apie));
        }

        return options;
    }
    
    public List<DataConnector> GetDataConnectorsForCompany()
    {
        List<DataConnector> options;

        try
        {
            options = MakeGetRequest<List<DataConnector>>("GetDataConnectorsForPACompany", c_webAppIntegrationsController);
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "DataConnectorList".Localize(), "System".Localize(), apie));
        }

        return options;
    }

    public IntegrationConfigDTO GetIntegrationConfig(int a_integrationId)
    {
        IntegrationConfigDTO nextRequest = null;

        try
        {
            return MakeGetRequest<IntegrationConfigDTO>("GetIntegrationConfig", c_webAppIntegrationsController, new GetParam { Name = "integrationId", Value = a_integrationId.ToString() });
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "IntegrationConfig".Localize(), "System".Localize(), apie));
        }
    }

    public int CreateIntegrationConfig(IntegrationConfigDTO a_newIntegrationConfig)
    {
        try
        {
            a_newIntegrationConfig.Id = 0;
            IntResponse newConfigId = MakePostRequest<IntResponse>("CreateIntegrationConfig", a_newIntegrationConfig, c_webAppIntegrationsController);
            return newConfigId.Content;
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "CreateIntegrationConfig".Localize(), "System".Localize(), apie));
        }
    }

    public void UpdateIntegrationConfig(IntegrationConfigDTO a_updatedIntegrationConfig)
    {
        try
        {
            MakePostRequest<IntegrationConfigDTO>("UpdateIntegrationConfig", a_updatedIntegrationConfig, c_webAppIntegrationsController);
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "UpdateIntegrationConfig".Localize(), "System".Localize(), apie));
        }
    }

    public void RenameIntegrationConfig(IntegrationConfigOptionsDTO a_integrationOption)
    {
        try
        {
            MakePostRequest<IntegrationConfigOptionsDTO>("RenameIntegrationConfig", a_integrationOption, c_webAppIntegrationsController);
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "UpdateIntegrationConfig".Localize(), "System".Localize(), apie));
        }
    }

    public void DeleteIntegrationConfig(int a_integrationId)
    {
        try
        {
            MakePostRequest<IntegrationConfigDTO>("DeleteIntegrationConfig", a_integrationId, c_webAppIntegrationsController);
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "DeleteIntegrationConfig".Localize(), "System".Localize(), apie));
        }
    }
    
    public int UpgradeIntegrationConfig(IntegrationConfigDTO a_newIntegrationConfig)
    {
        try
        {
            a_newIntegrationConfig.Id = 0;
            object response = MakePostRequest<object>("UpgradeIntegrationConfig", a_newIntegrationConfig, c_webAppIntegrationsController);
            int newConfigId = int.Parse(response.ToString());
            return newConfigId;
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "UpgradeIntegrationConfig".Localize(), "System".Localize(), apie));
        }
    }
    public record BaseUserRequest(string userIdentifier);
    public GetAllPlanningAreaDetailsResponse GetAllPlanningAreasForIntegrator(string a_email)
    {
        try
        {
            return MakePostRequest<GetAllPlanningAreaDetailsResponse>("GetAllPAsForIntegrator", new BaseUserRequest(a_email) , c_webAppIntegrationsController);
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "DBIntegration".Localize(), "System".Localize(), apie));
        }
    }
    public record IntegrationUserRequest(int integrationId, string userIdentifier);
    public DBIntegrationDTO GetDBIntegration(int a_integrationId, string a_email)
    {
        try
        {
            return MakePostRequest<DBIntegrationDTO>("GetDBIntegration", new IntegrationUserRequest(a_integrationId, a_email) , c_webAppIntegrationsController);
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "DBIntegration".Localize(), "System".Localize(), apie));
        }
    }
    public record IntegrationPARequest(string paKey, string userIdentifier);
    //this endpoint was disabled in aps-api, make sure to uncomment it there as well as here if you need it
    // public DBIntegrationDTO GetDBIntegrationFromPAKey(string a_paKey, string a_email)
    // {
    //     try
    //     {
    //         return MakePostRequest<DBIntegrationDTO>("GetIntegrationFromPA", new IntegrationPARequest(a_paKey, a_email) , c_webAppIntegrationsController);
    //     }
    //     catch (ApiException apie)
    //     {
    //         throw new PTHandleableException(string.Format("4493".Localize(), "DBIntegration".Localize(), "System".Localize(), apie));
    //     }
    // }
    
    public class GetIntegrationDataResponse
    {
        public DBIntegrationDTO Integration { get; set; }
        public Dictionary<string, DataTableJson> TableData { get; set; } //maps table name to data in table
    }
    
    public GetIntegrationDataResponse GetDBIntegrationDataFromPAKey(string a_paKey, string a_email)
    {
        try
        {
            return MakePostRequest<GetIntegrationDataResponse>("GetIntegrationDataFromPA", new IntegrationPARequest(a_paKey, a_email) , c_webAppIntegrationsController);
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "DBIntegration".Localize(), "System".Localize(), apie));
        }
    }
    
    public GetIntegrationDataResponse GetDBIntegrationDataFromDataConnector(int a_connectorId, string a_email)
    {
        try
        {
            return MakePostRequest<GetIntegrationDataResponse>("GetIntegrationDataFromDataConnector", new IntegrationUserRequest(a_connectorId, a_email) , c_webAppIntegrationsController);
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "DBIntegration".Localize(), "System".Localize(), apie));
        }
    }
    
    public record IntegrationCreate(DBIntegrationDTO IntegrationDto, string a_email);
    public void CreateIntegration(DBIntegrationDTO a_integration, string a_email)
    {
        try
        {
            MakePostRequest<string>("CreateDBIntegration", new IntegrationCreate(a_integration, a_email), c_webAppIntegrationsController);
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException(string.Format("4493".Localize(), "DBIntegration".Localize(), "System".Localize(), apie));
        }
    }

    #endregion

    #region License
    public BoolResponse NotifyLicenseStatus(ELicenseStatus a_status)
    {
        try
        {
            BoolResponse response = MakePostRequest<BoolResponse>("UpdateLicenseStatus", a_status, c_webAppPlanningAreasController);
            return response;
        }
        catch (ApiException e)
        {
            return new BoolResponse
            {
                Content = false
            };
        }
    }
    #endregion

    #region Logging
    public void LogErrorToWebApp(LogErrorRequest a_logErrorRequest)
    {
        try
        {
            MakePostRequest<Task<IActionResult>>("LogError", a_logErrorRequest, c_loggingController);
        }
        catch (ApiException apie)
        {
            throw new PTHandleableException("4495", new object[] { apie.Message.Localize(), a_logErrorRequest.UserId, a_logErrorRequest.ErrorLog.Message.Localize() });
        }
    }

    public void LogUserAction(LogUserActionRequest a_logUserActionRequest)
    {
        try
        {
            MakePostRequest<Task<IActionResult>>("LogUserAction", a_logUserActionRequest, c_loggingController);

        }
        catch (ApiException apie)
        {
            throw new PTHandleableException("4495", new object[] { apie.Message.Localize(), a_logUserActionRequest.UserId, a_logUserActionRequest.ActionDescription.Localize() });
        }
    }

    public void LogAuditAction(LogAuditRequest a_logAuditRequest)
    {
        try
        {
            MakePostRequest<Task<IActionResult>>("LogAudit", a_logAuditRequest, c_loggingController);
        }
        catch (ApiException apiException)
        {
            throw new PTHandleableException("4495", new object[] { apiException.Message.Localize(), a_logAuditRequest });
        }
    }
    #endregion
}