using Newtonsoft.Json;
using PT.Common.Extensions;
using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.ServerManagerSharedLib.DTOs.Responses;
using PT.ServerManagerSharedLib.Utils;

namespace PT.ServerManagerSharedLib.Data
{
    /// <summary>
    /// Manages instance-level settings for an instance, stored in a local database file.
    /// </summary>
    public class WebAppInstanceSettingsManager : IInstanceSettingsManager
    {
        protected readonly WebAppHttpClient m_httpClient;
        protected char m_webAppEnvCode;

        public WebAppInstanceSettingsManager(string a_instanceId, string a_apiKey, string a_env)
        {
            if (a_env.IsNullOrEmpty())
            {
                throw new ArgumentNullException("WebAppEnv must be specified in the appsettings file.");
            }
            m_webAppEnvCode = a_env.FirstOrDefault();
            m_httpClient = new WebAppHttpClient(m_webAppEnvCode);

            SetAuth(a_instanceId, a_apiKey);
        }

        protected virtual void SetAuth(string a_instanceId, string a_authToken)
        {
            m_httpClient.AddInstanceAuthorization(a_instanceId, a_authToken);
        }

        // A dictionary mapping the env letter code to the appropiate server url
        private static readonly Dictionary<char, string> WebAppEnvironmentUrls = new Dictionary<char, string>{
            { 'l', "https://localhost:7010/" },
            { 'd', "https://pt-api-dev.azurewebsites.net/" },
            { 'q', "https://pt-api-qa.azurewebsites.net/" },
            { 'p', "https://pt-api-prod.azurewebsites.net" },
        };

        /// <summary>
        /// Gets the URL associated with a particular WebApp env based on the code provided
        /// </summary>
        /// <param name="a_installCode">The code to decode</param>
        public static string GetURLForCode(string a_installCode)
        {
            if (a_installCode.Replace("-", "").Length == 33)
            {
                return GetURLForEnv(a_installCode.Last());
            }
            else
            {
                throw new ArgumentException("Wrong code format");
            }
        }

        /// <summary>
        /// Gets the URL associated with a particular WebApp env based on the env code
        /// </summary>
        /// <param name="a_env">The env letter code</param>
        public static string GetURLForEnv(char a_env)
        {
            if (WebAppEnvironmentUrls.ContainsKey(a_env))
            {
                return WebAppEnvironmentUrls[a_env];
            }
            else
            {
                throw new ArgumentException("Wrong code format");
            }
        }

        /// <summary>
        /// The SSO Client Id for the WebApp environment being connected to.
        /// TODO: This would be better to set within webapp so that it's independent of the ServerManagerSharedLibs nuget being used here.
        /// TODO: However, we don't currently deserialize the instance settings there, nor does the webapp api know this value (it's set in aps-web), so it's here for now.
        /// </summary>
        public string WebAppClientId
        {
            get
            {
                switch (m_webAppEnvCode)
                {
                    case 'p':
                        return SsoConstants.Auth0ClientIdWebAppProd;
                    case 'l':
                    case 'd':
                    case 'q':
                    default:
                        return SsoConstants.Auth0ClientIdWebAppDebug;
                }
            }
        }

        /// <summary>
        /// Attempts to connect to the database and creates it if required.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="Exception"></exception>
        public bool EnsureConnection()
        {
            return Task.Run(m_httpClient.Ping).Result;
        }

        /// <summary>
        /// Checks to see if database version meets version of the running application (Server Manager or Instance, which use same versioning).
        /// If the DB is older, add any missing columns.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void EnsureDbVersion()
        {
            // TODO: Report Version
        }

        /// <summary>
        /// Creates new instance settings file. This puts the data needed for webapp connection in a fixed place, so the instance can connect to the database without any other knowledge of its settings.
        /// </summary>
        /// <param name="instance"></param>
        public void CreateInstanceConnectionFile(APSInstanceEntity a_instance, string _)
        {
            string instanceSettingsDirectory = a_instance.ServicePaths.SystemInstanceSettingsDirectory;

            if (!Directory.Exists(instanceSettingsDirectory))
            {
                Directory.CreateDirectory(instanceSettingsDirectory);
            }

            using FileStream fs = File.Open(a_instance.ServicePaths.SystemInstanceConnectionFilePath, FileMode.Create);
            {
                using StreamWriter sw = new StreamWriter(fs);
                {
                    using (JsonWriter writer = new JsonTextWriter(sw))
                    {
                        JsonSerializer serializer = new() { Formatting = Formatting.Indented };

                        serializer.Serialize(writer, new
                        {
                            InstanceIdentifier = a_instance.Settings.InstanceId,
                            a_instance.ApiKey,
                            WebAppEnv = m_webAppEnvCode
                        });
                    }
                }
            }
        }


        public APSInstanceEntity GetInstance(string a_instanceName, string a_instanceVersion)
        {
            APSInstanceEntity instance = GetInstanceFromWebApp(a_instanceName, a_instanceVersion);

            return instance;
        }

        /// <summary>
        /// Pulls the instance from the webapp.
        /// Overloaded because there are two paths to getting this based on the context:
        /// Instances need to use their (secure) InstanceIdentifier, but Servers can just provide name/version.
        /// </summary>
        /// <param name="a_instanceName"></param>
        /// <param name="a_instanceVersion"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected virtual APSInstanceEntity GetInstanceFromWebApp(string a_instanceName, string a_instanceVersion)
        {
            InstanceFromWebAppDto instanceDto = Task.Run(m_httpClient.GetInstanceByIdentifier).Result;
            if (instanceDto == null)
            {
                throw new Exception("No instance found");
            }

            return BuildInstanceFromDto(instanceDto);
        }

        public void SaveInstance(APSInstanceEntity a_instance)
        {
            // TODO: This has some danger of concurrency, should be watched in testing

            string paramsJson = System.Text.Json.JsonSerializer.Serialize(new WebCreateInstanceResponseParams()
            {
                PlanningAreaKey = a_instance.Settings.InstanceId,
                PlanningAreaSettings = System.Text.Json.JsonSerializer.Serialize<APSInstanceEntity>(a_instance), // cast to base class to remove unneeded junk from serialization
                NewVersion = a_instance.PublicInfo.SoftwareVersion
            });

            try
            {
                WebApiActionFromServer webAppAction = new WebApiActionFromServer()
                {
                    Parameters = paramsJson,
                    Status = EActionRequestStatuses.NewFromServer.ToString(),
                    ActionType = EServerActionTypes.UpdatePlanningAreaSettings.ToString()
                };

                m_httpClient.SendNewAction(webAppAction).Wait();
            }
            catch
            {
                // TODO: log
            }
        }


        /// <summary>
        /// Recreates the instance entity from DTO components.
        /// TODO: This will be more straightforward in future. The instance should include data that comes from its Server's settings -
        /// but because the Webapp's PA settings are just stored as a big json blob, there's no convenient way to do that on its side.
        /// When we split up PA settings data into a proper schema (ARC-126), we should be able to send over the object without needing to rebuild things here
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        protected static APSInstanceEntity BuildInstanceFromDto(InstanceFromWebAppDto x)
        {
            APSInstanceEntity instance = JsonConvert.DeserializeObject<APSInstanceEntity>(x.Settings);
            instance.ServerWideSettings = x.ServerWideInstanceSettings;
            instance.IsActive = x.IsActive;
            instance.IsBackup = x.IsBackup;
            return instance;
        }


        public ServerWideInstanceSettings GetServerSettings()
        {
            var settingsDto = Task.Run(m_httpClient.GetServerSettings).Result;
            if (settingsDto == null)
            {
                throw new Exception("Failed to connect to WebApp");
            }

            return new ServerWideInstanceSettings(settingsDto);
        }


        public InstanceSettingsEntity GetInstanceSettingsEntity(string a_instanceName, string a_instanceVersion)
        {
            APSInstanceEntity instance = GetInstanceFromWebApp(a_instanceName, a_instanceVersion);

            return new InstanceSettingsEntity(instance);
        }

        public StartupVals GetStartupVals(string a_instanceName, string a_instanceVersion)
        {
            APSInstanceEntity instance = GetInstanceFromWebApp(a_instanceName, a_instanceVersion);

            StartupValsModel startupValsModel = new StartupValsModel(instance);

            if (startupValsModel.WebAppClientId.IsNullOrEmpty())
            {
                startupValsModel.WebAppClientId = WebAppClientId;
            }

            return startupValsModel;
        }

        public ErpDatabase GetErpDatabaseSettings(string a_instanceName, string a_instanceVersion)
        {
            APSInstanceEntity instance = GetInstanceFromWebApp(a_instanceName, a_instanceVersion);

            return new ErpDatabase(instance.Settings.ErpDatabaseSettings);
        }

        public string GetCertificateThumbprint(string a_instanceName, string a_instanceVersion)
        {
            var settingsDto = Task.Run(m_httpClient.GetServerSettings).Result;
            if (settingsDto == null)
            {
                throw new Exception("No instance found");
            }

            return settingsDto.Thumbprint;
        }

        public string GetServerManagerFolder(string a_instanceName, string a_instanceVersion)
        {
            var settingsDto = Task.Run(m_httpClient.GetServerSettings).Result;
            if (settingsDto == null)
            {
                throw new Exception("No instance found");
            }

            return settingsDto.ServerManagerPath;
        }
    }
}
