using Microsoft.IdentityModel.Tokens;
using PT.Common.Http;
using PT.ServerManagerSharedLib.Data;
using PT.ServerManagerSharedLib.DTOs;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.ServerManagerSharedLib.DTOs.Responses;

namespace PT.ServerManagerSharedLib.Utils
{
    // TODO: Add logging to this class.
    // TODO: We could split this up along the same lines as the InstanceSettingsManager. Not strictly necessary,
    // TODO: since not all public methods are authorized unless you provide Server Auth, but would help clean up public interface
    public class WebAppHttpClient : PTHttpClient
    {
        private const string c_API_KEY = "Pl@n3t10geth3r";
        private const string c_instanceTokenSchemeName = "InstanceTokenScheme";
        private const string c_webAppInstanceApiKeySchemeAuth = "ApiKey";
        private const string c_serverTokenSchemeName = "ServerTokenScheme";

        /// <summary>
        /// Initializes an Http Client with the default Utility Endpoint, for the production WebApp
        /// </summary>
        public WebAppHttpClient() : base("api/Utility/", WebAppInstanceSettingsManager.GetURLForEnv('p'))
        {
            SetDefaultHeaders();
            m_httpClient.BaseAddress = new Uri(WebAppInstanceSettingsManager.GetURLForEnv('p'));
        }


        /// <summary>
        /// Initializes an Http Client with the default Utility Endpoint, for the provided install code
        /// </summary>
        /// <param name="a_code">An installation code from the WebApp</param>
        public WebAppHttpClient(string a_code) : base("api/Utility/", WebAppInstanceSettingsManager.GetURLForCode(a_code))
        {
            SetDefaultHeaders();
            m_httpClient.BaseAddress = new Uri(WebAppInstanceSettingsManager.GetURLForCode(a_code));
        }

        /// <summary>
        /// Initializes an Http Client with the default Utility Endpoint, for the provided env char
        /// </summary>
        /// <param name="a_env">A char that specifies the WebApp env. (p)roduction, (q)a, (d)ev, or (l)ocalhost</param>
        public WebAppHttpClient(char a_env) : base("api/Utility/", WebAppInstanceSettingsManager.GetURLForEnv(a_env))
        {
            SetDefaultHeaders();
            m_httpClient.BaseAddress = new Uri(WebAppInstanceSettingsManager.GetURLForEnv(a_env));
        }

        /// <summary>
        /// Initializes an Http Client with the default Utility Endpoint, for the provided env char and controller
        /// </summary>
        /// <param name="a_env"></param>
        /// <param name="a_controller"></param>
        public WebAppHttpClient(char a_env, string a_controller) : base(a_controller, WebAppInstanceSettingsManager.GetURLForEnv(a_env))
        {
            SetDefaultHeaders();
            m_httpClient.BaseAddress = new Uri(WebAppInstanceSettingsManager.GetURLForEnv(a_env));
        }

        /// <summary>
        /// Creates the controller using a preconfigured HttpClient, applying standard settings atop upon construction.
        /// </summary>
        /// <param name="a_env"></param>
        /// <param name="a_controller"></param>
        /// <param name="a_configuredClient"></param>
        // This helps bypass some configuration that isn't straightforward to accomplish using the existing PTHttpClient in Core.
        public WebAppHttpClient(char a_env, string a_controller, HttpClient a_configuredClient) : base(a_controller, WebAppInstanceSettingsManager.GetURLForEnv(a_env))
        {
            m_httpClient = a_configuredClient;
            SetDefaultHeaders();
            m_httpClient.BaseAddress = new Uri(WebAppInstanceSettingsManager.GetURLForEnv(a_env));

            if (!a_controller.IsNullOrEmpty())
            {
                m_defaultController = a_controller;
            }
        }

        private void SetDefaultHeaders()
        {
            m_httpClient.DefaultRequestHeaders.Add("AzureAPIKey", c_API_KEY);
        }

        /// <summary>
        /// Ensure Client is configured to send requests using the Server Auth token, allow for certain elevated requests
        /// </summary>
        /// <param name="a_serverAuthToken"></param>
        public void AddServerManagerAuthorization(string a_serverAuthToken)
        {
            m_httpClient.DefaultRequestHeaders.Add(c_serverTokenSchemeName, a_serverAuthToken);
        }

        /// <summary>
        /// Ensure Client is configured to send requests using the Instance Auth token, designed to allow requests specific to the calling instance
        /// </summary>
        /// <param name="a_serverAuthToken"></param>
        public void AddInstanceAuthorization(string a_instanceId, string a_serverAuthToken)
        {
            m_httpClient.DefaultRequestHeaders.Add(c_instanceTokenSchemeName, a_instanceId);
            m_httpClient.DefaultRequestHeaders.Add(c_webAppInstanceApiKeySchemeAuth, a_serverAuthToken);
        }

        public Uri BaseAddress => m_httpClient.BaseAddress;

        public async Task<bool> Ping()
        {
            try
            {
                var response = await SendGetRequest("/Ping");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ServerSettingsDto> GetServerSettings()
        {
            GetServerSettingsResponse response = await MakePostRequestAsync<GetServerSettingsResponse>("/GetSettings", null, "api/Servers");
            return response?.ServerSettings;
        }

        public async Task<InstanceFromWebAppDto> GetInstanceByNameVersion(string name, string version)
        {
            InstanceFromWebAppDto response;
            try
            {
                response = await MakePostRequestAsync<InstanceFromWebAppDto>("/GetSettingsByName", new GetPASettingsRequest(name, version), "api/PlanningAreas");

            }
            catch (Exception e)
            {
                return null;
            }

            return response;
            //var response = await SendPostRequestAsync("/GetSettingsByName", new GetPASettingsRequest(name, version), "api/PlanningAreas");
            //response.EnsureSuccessStatusCode();
            //return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Get the instance based on its instanceIdentifier (this should already be set for authorization purposes in the client default headers).
        /// </summary>
        /// <returns></returns>
        public async Task<InstanceFromWebAppDto> GetInstanceByIdentifier()
        {
            InstanceFromWebAppDto response = await MakePostRequestAsync<InstanceFromWebAppDto>("/GetSettings", null, "api/PlanningAreas");
            return response;
        }

        private record GetPASettingsRequest(string InstanceName, string InstanceVersion);


        public async Task<List<InstanceFromWebAppDto>> GetAllInstancesFromWebApp()
        {
            GetAllPASettingsResponse response = await MakePostRequestAsync<GetAllPASettingsResponse>("/GetAllForServer", null, "api/PlanningAreas");

            return response.PlanningAreaSettings;
        }

        private record GetAllPASettingsResponse(List<InstanceFromWebAppDto> PlanningAreaSettings);

        public async Task<string?> ValidateInstallCode(string code)
        {
            string response;
            try
            {
                response = await MakePostRequestAsync<string>("/ValidateInstallCode", code);
                return string.Empty; // no error
            }
            catch (Exception e)
            {
                if (!e.Message.IsNullOrEmpty())
                {
                    string[] errorPieces = e.Message.Split(": ");  // format is "Status Code XXX: Foo Message"; we don't need the code
                    if (errorPieces.Length > 1)
                    {
                        return errorPieces[1];
                    }
                }
            }

            return "An unknown error occurred during validation.";
        }

        public async Task<byte[]> GetClientAgent()
        {
            StandardVersionResponse response = await MakeGetRequestAsync<StandardVersionResponse>("/GetClientAgentVersions", "api/Deployment", null);

            // Find the latest version
            VersionDto? latestVersion = null;
            foreach (VersionDto version in response.Versions)
            {
                if (version.VersionDate > (latestVersion?.VersionDate ?? DateTime.MinValue))
                {
                    latestVersion = version;
                }
            }

            return await GetClientAgent(latestVersion.VersionNumber);
        }

        // TODO: Hook following methods in where needed, and do something useful with the file other than dumping it on my lawn
        // TODO: These don't have to live in this class if there's a better place, but they do require base header set in SetDefaultHeaders()
        public async Task<byte[]> GetClientAgent(string a_version)
        {
            string clientAgentFileName = "PlanetTogetherClientAgent.zip"; // use the standard name the webinstaller would expect from download
            string myDownloadPath = "C:\\Users\\EricBiondic\\drop";
            try
            {
                byte[] response = await MakeGetRequestAsync<byte[]>("/GetClientAgent", "api/Deployment",
                    new GetParam[] { new GetParam() { Name = "version", Value = a_version, } });
                File.WriteAllBytes($"{myDownloadPath}\\{clientAgentFileName}", response);

                return response;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<byte[]> GetServerAgent()
        {
            StandardVersionResponse response = await MakeGetRequestAsync<StandardVersionResponse>("/GetServerAgentVersions", "api/Deployment", null);

            // Find the latest version
            VersionDto? latestVersion = null;
            foreach (VersionDto version in response.Versions)
            {
                if (version.VersionDate > (latestVersion?.VersionDate ?? DateTime.MinValue))
                {
                    latestVersion = version;
                }
            }

            return await GetServerAgent(latestVersion.VersionNumber);
        }


        public async Task<byte[]> GetServerAgent(string a_version)
        {
            string serverAgentFileName = "ServerAgent.zip"; // use the standard name the webinstaller would expect from download
            try
            {
                byte[] response = await MakeGetRequestAsync<byte[]>("/GetServerAgent", "api/Deployment",
                    new GetParam[] { new GetParam() { Name = "version", Value = a_version, } });

                return response;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<byte[]> GetSoftwareZip(string a_version)
        {
            string clientAgentFileName = $"{a_version}.zip"; // use the standard name the webinstaller would expect from download

            try
            {
                byte[] response = await MakeGetRequestAsync<byte[]>("/GetSoftwareZip", "api/Deployment",
                    new GetParam[] { new GetParam() { Name = "version", Value = a_version, } });

                return response;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<List<VersionDto>> GetClientAgentVersions()
        {
            try
            {
                StandardVersionResponse response = await MakeGetRequestAsync<StandardVersionResponse>("/GetClientAgentVersions", "api/Deployment", null);
                return response.Versions;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public async Task<List<VersionDto>> GetServerAgentVersions()
        {
            try
            {
                StandardVersionResponse response = await MakeGetRequestAsync<StandardVersionResponse>("/GetServerAgentVersions", "api/Deployment", null);
                return response.Versions;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        /// <summary>
        /// Sends off a new action, originating from the Server Manager, that the WebApp needs to followup on (e.g. updating the webapp database).
        /// </summary>
        /// <param name="a_webAppAction"></param>
        /// <returns></returns>
        public async Task SendNewAction(WebApiActionFromServer a_webAppAction)
        {
            await MakePostRequestAsync<string>("/CreateNewRequest", a_webAppAction, "api/ServerManagerActions");
        }

        public async Task<bool> ActivateInstallCode(ActivateCodeMessage a_request)
        {
            var response = await SendPostRequestAsync("/ActivateInstallCode", a_request);
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Adds header to httpClient
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void AddHeader(string key, string value)
        {
            m_httpClient.DefaultRequestHeaders.Add(key, value);
        }

        public class ActivateCodeMessage
        {
            public string Code { get; set; }
            public string Thumbprint { get; set; }
            public string AuthToken { get; set; }
            public string Name { get; set; }
            public string SystemId { get; set; }
            public string Version { get; set; }
            public string ApiPort { get; set; }
            public string ServerManagerPath { get; set; }
            public string SsoClientId { get; set; }
            public string SsoDomain { get; set; }


            public ActivateCodeMessage(string code, string thumbprint, string authToken, string version, string a_apiPort, string a_serverManagerPath, string a_ssoClientId, string a_ssoDomain, string a_systemId)
            {
                Code = code;
                Thumbprint = thumbprint;
                AuthToken = authToken;
                Name = Environment.MachineName;
                Version = version;
                ApiPort = a_apiPort;
                ServerManagerPath = a_serverManagerPath;
                SsoClientId = a_ssoClientId;
                SsoDomain = a_ssoDomain;
                SystemId = a_systemId;
            }
        }
    }
}
