using System.Data;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using WebAPI.Common;
using WebAPI.Models;
using WebAPI.Models.CTP;
using WebAPI.Models.Integration;
using WebAPI.RequestsAndResponses;
using System.Data;
using System.Text.RegularExpressions;

namespace WebAPI.DAL
{
    public class CompanyDBService
    {
        private readonly IServiceProvider m_serviceProvider;
        private IConfiguration m_configuration;

        public CompanyDBService(IServiceProvider a_serviceProvider, IConfiguration a_configuration)
        {
            m_serviceProvider = a_serviceProvider;
            m_configuration = a_configuration;
        }

        private CompanyDBContext getDbContext()
        {
            // Resolve a new scoped DbContext using the IServiceProvider
            return m_serviceProvider.CreateScope().ServiceProvider.GetRequiredService<CompanyDBContext>();
        }

        public async Task<Ctp> GetNextCtpRequestAsync(CtpRequest ctpRequest)
        {
            if (string.IsNullOrWhiteSpace(ctpRequest.CompanyId))
            {
                throw new ArgumentException("CompanyId cannot be null or empty.");
            }

            if (!int.TryParse(ctpRequest.CompanyId, out var companyId))
            {
                throw new ArgumentException("Invalid CompanyId format.");
            }

            using var dbContext = getDbContext();

            return await dbContext.Ctps
                .Where(r => r.CompanyId == companyId && r.Status == "New" && r.PADetailsId == ctpRequest.PAId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateCtpRequestAsync(CtpUpdateRequest request)
        {
            using var dbContext = getDbContext();
            var ctp = await dbContext.Ctps.FindAsync(request.RequestID);

            if (ctp == null || ctp.CompanyId != int.Parse(request.CompanyId) || ctp.PADetailsId != request.PAId)
            {
                return false;  // No matching record found, or company/PA mismatch
            }

            // Update fields
            ctp.ScheduledStart = request.SchedulePStart;
            ctp.ScheduledFinish = request.ScheduleFinish;
            ctp.Status = request.Status;

            await dbContext.SaveChangesAsync();
            return true;  // Successfully updated
        }

        public List<string> GetBINotificationGroupUsers(string a_dbName, string a_dbServerName)
        {
            using var dbContext = getDbContext();
            var companyDb = dbContext.CompanyDbs.Where(c => c.DBName == a_dbName && c.DBServerName == a_dbServerName)
                .FirstOrDefault();
            if (companyDb == null) throw new Exception("No company found with the servername and dbname");
            IQueryable<Role> notifiedGroups = dbContext.Roles
                                                        .Include(g => g.Users)
                                                        .Where(g => g.CompanyId == companyDb.CompanyId &&
                                                                    g.Permissions.Any(p => p == "ReceivePowerBINotificationEmails"));

            List<string> userEmails = notifiedGroups.SelectMany(g => g.Users.Select(u => u.Email))
                .Distinct().ToList();

            return userEmails;
        }
        public string GetCompanyKey(StringValues companyId)
        {
            using var dbContext = getDbContext();
            var c = dbContext.Companies.Where(c=>c.Id == int.Parse(companyId.ToString())).FirstOrDefault();
            if (c != null)
            {
                return c.ApiKey;
            }
            return string.Empty;
        }

		public string GetConnectionString(StringValues InstanceName, StringValues CompanyId, out string connectionStringError)
        {
            string connectionStringPasswordKey = string.Empty;
            using var dbContext = getDbContext();
            var i = dbContext.CompanyDbs.Where(i => i.CompanyId ==int.Parse(CompanyId.ToString()) && i.DbType == EDbType.Import && i.Name == InstanceName.ToString()).FirstOrDefault();
            if (i != null && (i.DBPasswordKey != null || i.DBPasswordKey != "" || i.DBPasswordKey != string.Empty))
            {
                connectionStringPasswordKey = i.DBPasswordKey;
            }
            else
            {
                connectionStringError = "No records found";
                return null;
            }
            string keyvalutError = string.Empty;
            string connectionstringPassword = CommonMethods.GetValueFromAzureKeyVault(connectionStringPasswordKey, out keyvalutError);
            connectionStringError = keyvalutError;
            if (connectionStringError != string.Empty) return null;

            string connectionstring = $"Data Source={i.DBServerName};Initial Catalog={i.DBName};User ID={i.DBUserName};Password={connectionstringPassword};MultipleActiveResultSets=true;Encrypt=True";
            return connectionstring;
        }

		internal bool GetTriggerImportDetails(out string ptUserName, out string ptPassword, out string serverManagerUrl, int CompanyId, string InstanceName)
		{
			using var dbContext = getDbContext();
			var cdb = dbContext.CompanyDbs.Where(i => i.CompanyId ==CompanyId && i.DbType == EDbType.Import && i.Name == InstanceName).FirstOrDefault();
			if (cdb != null)
			{
				if (cdb.ImportUserName == null || cdb.ImportUserName == string.Empty)
					throw new Exception("ImportUserName not found in DB");
				ptUserName = cdb.ImportUserName;
				if (cdb.ImportUserPasswordKey == null || cdb.ImportUserPasswordKey == string.Empty)
					throw new Exception("PasswordKey not found in DB");
                string keyvalutError = String.Empty;
				ptPassword = CommonMethods.GetValueFromAzureKeyVault(cdb.ImportUserPasswordKey, out keyvalutError);
				if (cdb.ServerManagerUrl == null || cdb.ServerManagerUrl == string.Empty)
					throw new Exception("ServerManagerURL not found in DB");
				serverManagerUrl = cdb.ServerManagerUrl;
				return true;
			}
			else
            {
                throw new Exception("Cannot find a record for this in CompanyDB");
            } 
		}

        public List<IntegrationConfigDetailDTO> GetIntegrationConfigDescriptions(int companyId)
        {
            using var dbContext = getDbContext();
            var c = dbContext.IntegrationConfigs
                .Where(c => c.CompanyId == companyId).ToList();

            return c.Select(integrationConfig => new IntegrationConfigDetailDTO(integrationConfig)).ToList();
        }

        public IntegrationConfigDTO? GetIntegrationConfig(int id)
        {
            using var dbContext = getDbContext();
            var c = dbContext.IntegrationConfigs.Include(x => x.Features).Include(x => x.Properties).FirstOrDefault(c => c.Id == id);
            if (c == null)
            {
                throw new InvalidDataException("No Integration Config found with the specified ID");
            }

            return c.ToDTO();
        }

        public List<IntegrationConfig> GetIntegrationConfigs(string planningAreaKey)
        {
            using var dbContext = getDbContext();
            var c = dbContext.IntegrationConfigs
                             .Include(x => x.PlanningAreas)
                             .ToList().Where(c => c.PlanningAreas
                                                   .Any(x => x.PlanningAreaKey == planningAreaKey)).ToList();

            return c;
        }

        public List<int> GetAllDBIntegrationIds()
        {
            using var dbContext = getDbContext();
            return dbContext.DBIntegrations.Select(x => x.Id).ToList();
        }
        
        public DBIntegration? GetDBIntegration(int integrationId)
        {
            using var dbContext = getDbContext();
            var c = dbContext.DBIntegrations.Include(x => x.IntegrationDBObjects)
                .Where(x => x.Id == integrationId);
            
            return c.FirstOrDefault();
        }
        
        public bool CreateDBIntegration(DBIntegration integration, string a_createdByUserEmail)
        {
            using var dbContext = getDbContext();

            User? user = dbContext.Users.FirstOrDefault(u => u.Email == a_createdByUserEmail);
            if (user == null)
            {
                return false;
            }

            var groups = dbContext.Roles.Include(g => g.Users).Where(g => g.Permissions.Any(p => p == "PullAndPublishIntegration") && g.Users.Any(u => u.Id == user.Id)).Any();
            if (!groups) //user isn't in any groups with pull and publish permission
            {
                return false;
            }
            
            integration.Id = 0;
            integration.CreatedBy = user.Id;
            integration.CreatedDate = DateTime.UtcNow;
            var c = dbContext.DBIntegrations.Add(integration);
            dbContext.SaveChanges();
            return true;
        }
        
        public bool CreateNewVersionForDBIntegration(DBIntegration dbIntegration, string a_createdByUserEmail)
        {
            using var dbContext = getDbContext();
            
            User? user = dbContext.Users.FirstOrDefault(u => u.Email == a_createdByUserEmail);
            if (user == null)
            {
                return false;
            }

            var groups = dbContext.Roles.Include(g => g.Users).Where(g => g.Permissions.Any(p => p == "PullAndPublishIntegration") && g.Users.Any(u => u.Id == user.Id)).Any();
            if (!groups) //user isn't in any groups with pull and publish permission
            {
                return false;
            }
            
            var existing = dbContext.DBIntegrations.Where(x => x.Id == dbIntegration.Id).FirstOrDefault();
            if (existing == null)
            {
                return false;
            }

            existing.Id = 0;
            existing.CreatedBy = user.Id;
            existing.CreatedDate = DateTime.UtcNow;
            existing.IntegrationDBObjects = dbIntegration.IntegrationDBObjects;
            existing.Version = dbIntegration.Version; // the version should have been incremented by whoever sent the api request
            existing.VersionNotes = dbIntegration.VersionNotes;
            dbContext.DBIntegrations.Add(existing);
            dbContext.SaveChanges();
            return true;
        }
        
        public bool DeleteDBIntegration(int integrationId)
        {
            using var dbContext = getDbContext();
            var existing = dbContext.DBIntegrations.Where(x => x.Id == integrationId).FirstOrDefault();
            if (existing == null)
            {
                return false;
            }

            dbContext.DBIntegrations.Remove(existing);
            dbContext.SaveChanges();
            return true;
        }
        

        public int CreateIntegrationConfig(IntegrationConfig config, bool isUpgrade = false)
        {
            config = PrepareForCreate(config);
            if (!isUpgrade)
            {
                config.UpgradedFromConfigId = null;
            }
            using var dbContext = getDbContext();
            var c = dbContext.IntegrationConfigs.Add(config);
            dbContext.SaveChanges();

            return config.Id;
        }

        /// <summary>
        /// All Features and Properties in a config should only be for that config. As such, clear any relationships that might be keyed from old data.
        /// </summary>
        /// <param name="a_config"></param>
        private static IntegrationConfig PrepareForCreate(IntegrationConfig a_config)
        {
            a_config.Id = 0;
            a_config.Features.ForEach(feature =>
            {
                feature.IntegrationConfigId = 0;
                feature.Id = 0;
            });
            a_config.Properties.ForEach(property => property.Id = 0);

            return a_config;
        }

        public void UpdateIntegrationConfig(IntegrationConfig dto)
        {
            using var dbContext = getDbContext();
            var existing = dbContext.IntegrationConfigs.Include(x => x.Features).Include(x => x.Properties).Include(x => x.PlanningAreas).FirstOrDefault(c => c.Id == dto.Id); ;

            if (existing == null || existing.CompanyId != dto.CompanyId)
            {
                throw new InvalidDataException("Integration Config Not Found");
            }

            existing.Name = dto.Name;
            existing.LastEditedDate = DateTime.UtcNow;
            existing.LastEditingUserId = dto.LastEditingUserId;
            var props = dbContext.Properties.Where(x => x.IntegrationConfigId == dto.Id).ToList();
            dbContext.RemoveRange(props);
            var features = dbContext.Features.Where(x => x.IntegrationConfigId == dto.Id).ToList();
            dbContext.RemoveRange(features);
            existing.Features.Clear();
            existing.Features.AddRange(dto.Features);
            existing.Properties.Clear();
            existing.Properties.AddRange(dto.Properties);
            dbContext.Update(existing);
            dbContext.SaveChanges();
        }

        /// <summary>
        /// Lightweight request that doesn't require the client to know the details of a particular 
        /// </summary>
        /// <param name="dto"></param>
        /// <exception cref="InvalidDataException"></exception>
        public void RenameIntegrationConfig(IntegrationConfigDetailDTO integrationDetail, Company integrationCompany)
        {
            using var dbContext = getDbContext();
            var existing = dbContext.IntegrationConfigs.Include(x => x.Features).Include(x => x.Properties).Include(x => x.PlanningAreas).FirstOrDefault(c => c.Id == integrationDetail.IntegrationConfigId); ;

            if (existing == null || existing.CompanyId != integrationCompany.Id)
            {
                throw new InvalidDataException("Integration Config Not Found");
            }

            existing.Name = integrationDetail.IntegrationName;

            dbContext.SaveChanges();
        }

        public void MapIntegrationConfig(int id, string planningAreaKey, bool removeMapping = false)
        {
            using var dbContext = getDbContext();
            var set = dbContext.Set<PlanningAreaIntegrationConfig>("PlanningAreaIntegrationConfig");
            var pa = dbContext.PlanningAreas.FirstOrDefault(x =>  x.PlanningAreaKey == planningAreaKey);
            if (removeMapping)
            {
                var c = set.Remove(new PlanningAreaIntegrationConfig { PlanningAreaId = pa.Id, IntegrationConfigId = id });
            } else
            {
                var c = set.Add(new PlanningAreaIntegrationConfig { PlanningAreaId = pa.Id, IntegrationConfigId = id });
            }
            dbContext.SaveChanges();
        }

        public bool DeleteIntegrationConfig(int id)
        {
            using var dbContext = getDbContext();
            var c = dbContext.IntegrationConfigs.Include(x => x.PlanningAreas).Include(x => x.Properties).Include(x => x.Features).FirstOrDefault(c => c.Id == id);
            c.PlanningAreas.Clear();
            c.Features.Clear();
            c.Properties.Clear();
            dbContext.IntegrationConfigs.Remove(c);
            dbContext.SaveChanges();
            return true;
        }

        public CodeStatus ValidateInstallCode(string installCode)
        {
            using var dbContext = getDbContext();
            var code = dbContext.InstallCodes.FirstOrDefault(x => x.Code == installCode.Trim());

            if (code == null)
            {
                return CodeStatus.NotFound;
            }
            if (code.Used)
            {
                return CodeStatus.Invalid;
            }
            if (code.CreationDate < DateTime.UtcNow.AddMinutes(-10))
            {
                return CodeStatus.Expired;
            }
            return CodeStatus.Valid;
        }

        public enum CodeStatus
        {
            NotFound, Invalid, Expired, Valid
        }

        public bool ActivateInstallCode(ActivateCodeMessage message)
        {
            using var dbContext = getDbContext();
            var code = dbContext.InstallCodes.FirstOrDefault(x => x.Code == message.Code.Trim());

            if (code == null)
            {
                return false;
            }
            else
            {
                code.Used = true;
                var user = dbContext.Users.FirstOrDefault(x => x.Email == code.CreatedBy);
                if (user == null)
                {
                    //this should (ideally) never happen
                    throw new InvalidOperationException("The user that created the install code no longer exists or has changed their email.");
                }
                dbContext.CompanyServers.Add(new CompanyServer
                {
                    ManagingCompanyId = code.CompanyId,
                    Name = message.Name,
                    Thumbprint = message.Thumbprint,
                    AuthToken = message.AuthToken,
                    ApiPort = message.ApiPort,
                    ComputerNameOrIP = message.Name,
                    ServerManagerPath = message.ServerManagerPath,
                    SsoClientId = message.SsoClientId,
                    SsoDomain = message.SsoDomain,
                    ServerName = message.Name,
                    SystemId = message.SystemId,
                    Url = $"https://{message.Name}:{message.ApiPort}",  // Webapp shouldn't need this in future (no communication in that direction)
                    IPAddress = "",
                    Location = "",
                    AdminMessage = "",
                    CertificateName = "",
                    CreationDate = DateTime.UtcNow,
                    CreatedBy = code.CreatedBy,
                    Version = message.Version,
                    LocalVersions = message.Version,
                    OwningUserId = user.Id,
                });

                dbContext.SaveChanges();
                return true;
            }
        }

        private object _lock = new object();
        public int? UpgradeIntegrationConfig(IntegrationConfig a_config)
        {
            using var dbContext = getDbContext();
            lock (_lock)
            {
                var integrationConfigs = dbContext.IntegrationConfigs.Where(c =>
                    c.UpgradedFromConfigId == a_config.UpgradedFromConfigId &&
                    c.VersionNumber == a_config.VersionNumber);
                if (integrationConfigs.Count() == 0) 
                { 
                    return CreateIntegrationConfig(a_config, true);
                }
                else if (integrationConfigs.Count() == 1)
                {
                    return integrationConfigs.First().Id;
                }
                else
                {
                    throw new Exception("More than one Integration Config found for the specified ID");
                }
            }
        }

        internal async Task<Company> GetCompanyForInstanceId(string a_planningAreaKey)
        {
            using var dbContext = getDbContext();
            var planningArea = await dbContext.PlanningAreas
                .Include(pa => pa.Company)
                .FirstOrDefaultAsync(pa => pa.PlanningAreaKey.Equals(a_planningAreaKey));

            return planningArea?.Company;
        }

        internal async Task<CompanyServer> GetServer(string a_serverAuthToken)
        {
            using var dbContext = getDbContext();
            var server = await dbContext.CompanyServers
                .Include(server => server.ManagingCompany)
                .FirstOrDefaultAsync(server => server.AuthToken.Equals(a_serverAuthToken));

            return server;
        }

        internal string? GetInstances(string companyId)
        {
            using var dbContext = getDbContext();
            var list = dbContext.CompanyDbs.Where(c => c.CompanyId == Convert.ToInt32(companyId)).Select(i => i.Name).Distinct().ToList();
            string result = JsonConvert.SerializeObject(list) ?? string.Empty;
            return result;
        }

        /// <summary>
        /// Gets a planning area on a particular server, based on its name/version.
        /// This is needed because the Server Manager does not know the instance's InstanceId before receiving these settings.
        /// </summary>
        /// <param name="a_instanceName"></param>
        /// <param name="a_instanceVersion"></param>
        /// <param name="a_ServerId"></param>
        /// <returns></returns>
        public string GetPlanningAreaSettings(string a_instanceName, string a_instanceVersion, int a_ServerId)
        {
            using var dbContext = getDbContext();
            PADetails planningAreaSettings = dbContext.PlanningAreas.Include(x => x.Server).FirstOrDefault(pa => 
                pa.Name.Equals(a_instanceName) &&
                pa.Version.Equals(a_instanceVersion) &&
                pa.ServerId == a_ServerId &&
                (pa.RegistrationStatus == ERegistrationStatus.Created.ToString() || pa.RegistrationStatus == ERegistrationStatus.Deleting.ToString())
            );

            if (planningAreaSettings == null)
            {
                throw new DataException($"Settings were not found for {a_instanceName} {a_instanceVersion}");
            }

            string updatedPlanningAreaSettings = InjectConstants(planningAreaSettings?.Settings);
            updatedPlanningAreaSettings = InjectServerSettings(updatedPlanningAreaSettings, planningAreaSettings.Server);
            return updatedPlanningAreaSettings;
        }

        public PADetails? GetPlanningArea(string a_planningAreaKey)
        {
            using var dbContext = getDbContext();
            var planningArea = dbContext.PlanningAreas.FirstOrDefault(pa =>
                pa.PlanningAreaKey == a_planningAreaKey &&
                (pa.RegistrationStatus == ERegistrationStatus.Created.ToString() || pa.RegistrationStatus == ERegistrationStatus.Deleting.ToString()));

            return planningArea;
        }
        
        public PADetails GetPlanningAreaWithCompany(string a_planningAreaKey)
        {
            using var dbContext = getDbContext();

            return dbContext.PlanningAreas.Include(pa => pa.Company).FirstOrDefault(pa =>
                pa.PlanningAreaKey == a_planningAreaKey &&
                (pa.RegistrationStatus == ERegistrationStatus.Created.ToString() || pa.RegistrationStatus == ERegistrationStatus.Deleting.ToString()));
        }

        public async Task<PADetails> GetPlanningAreaByIdAsync(int planningAreaId)
        {
            using var dbContext = getDbContext();

            return await dbContext.PlanningAreas
                .Include(pa => pa.Company)
                .FirstOrDefaultAsync(pa => pa.Id == planningAreaId &&
                    (pa.RegistrationStatus == ERegistrationStatus.Created.ToString() || pa.RegistrationStatus == ERegistrationStatus.Deleting.ToString()));
        }
        
        public async Task<PADetails> GetPlanningAreaByKeyAsync(string planningAreaKey)
        {
            using var dbContext = getDbContext();

            return await dbContext.PlanningAreas
                .Include(pa => pa.Company)
                .FirstOrDefaultAsync(pa => pa.PlanningAreaKey == planningAreaKey &&
                    (pa.RegistrationStatus == ERegistrationStatus.Created.ToString() || pa.RegistrationStatus == ERegistrationStatus.Deleting.ToString()));
        }

        public string GetPlanningAreaSettings(string a_planningAreaKey)
        {
            using var dbContext = getDbContext();

            PADetails planningAreaSettings = dbContext.PlanningAreas.Include(x => x.Server).FirstOrDefault(pa =>
                pa.PlanningAreaKey == a_planningAreaKey &&
                (pa.RegistrationStatus == ERegistrationStatus.Created.ToString() || pa.RegistrationStatus == ERegistrationStatus.Deleting.ToString())
            );

            string updatedPlanningAreaSettings = InjectConstants(planningAreaSettings?.Settings);
            updatedPlanningAreaSettings = InjectServerSettings(updatedPlanningAreaSettings, planningAreaSettings?.Server);
            return updatedPlanningAreaSettings;
        }

        /// <summary>
        /// We should (and are starting to) store some company-wide constants in a way that is controlled by the webapp. This is an opportunity to inject these in.
        /// </summary>
        /// <param name="a_planningAreaSettings"></param>
        /// <returns></returns>
        private string InjectConstants(string? a_planningAreaSettings)
        {
            JObject jsonBlob = ((JObject)JsonConvert.DeserializeObject<JObject>(a_planningAreaSettings));

            string sentryDsnFromConfig = m_configuration["SoftwareSentryDsn"];
            jsonBlob["Settings"]["SentryDsn"] = sentryDsnFromConfig;

            return jsonBlob.ToString();
        }

        /// <summary>
        /// Some settings exist on the PlanningAreaSettings, but are controlled at the Server level. Update them here.
        /// </summary>
        /// <param name="a_planningAreaSettings"></param>
        /// <returns></returns>
        private string InjectServerSettings(string? a_planningAreaSettings, CompanyServer server)
        {
            JObject jsonBlob = ((JObject)JsonConvert.DeserializeObject<JObject>(a_planningAreaSettings));

            string ssoThumbprint = server.SsoThumbprint;
            string ssoDomain = server.SsoDomain;
            string ssoClient= server.SsoClientId;

            // SsoLogin should always be allowed for webapp PAs
            jsonBlob["Settings"]["AllowSsoLogin"] = "true";
            jsonBlob["PublicInfo"]["SystemServiceUrl"] = $"https://{server.ComputerNameOrIP}:{jsonBlob["Settings"]["SystemServiceSettings"]["Port"]}";
            jsonBlob["Settings"]["DisableHttps"] = server.Thumbprint.IsNullOrEmpty();
            if (jsonBlob["Settings"]["SsoValidationCertificateThumbprint"].ToString().IsNullOrEmpty())
            {
                jsonBlob["Settings"]["SsoValidationCertificateThumbprint"] = ssoThumbprint;
            }
            if (jsonBlob["Settings"]["SystemServiceSettings"]["SsoDomain"].ToString().IsNullOrEmpty())
            {
                jsonBlob["Settings"]["SystemServiceSettings"]["SsoDomain"] = ssoDomain;
            }
            if (jsonBlob["Settings"]["SystemServiceSettings"]["SsoClientId"].ToString().IsNullOrEmpty())
            {
                jsonBlob["Settings"]["SystemServiceSettings"]["SsoClientId"] = ssoClient;
            }

            return jsonBlob.ToString();
        }


        /// <summary>
        /// Gets all planning area settings for a particular server.
        /// Note it is possible for a server to have planning areas with different companies - it is expected to get all of them at once, and they are currently only differentiated on the webapp.
        /// (It would be fine to have a method to get all PAs for a server/company combo, but there isn't any current use case)
        /// </summary>
        /// <param name="a_companyId"></param>
        /// <param name="a_serverId"></param>
        /// <returns></returns>
        public List<PADetails> GetAllPlanningAreasForServer(/*int a_companyId, */int a_serverId)
        {
            using var dbContext = getDbContext();
            return dbContext.PlanningAreas.Where(pa =>
                pa.ServerId == a_serverId).ToList();

            // TODO: I don't think we need to call InjectConstants on this yet, but we probably should at some point as we add more/
        }
        
        public List<PADetails> GetAllPlanningAreasForIntegrator(string a_userEmail)
        {
            using var dbContext = getDbContext();
            User? user = dbContext.Users.FirstOrDefault(u  => u.Email == a_userEmail);
            if (user == null)
            {
                return null;
            }
            var companyIds = dbContext.Roles.Where(g => g.Permissions.Any(p => p == "PullAndPublishIntegration") && g.Users.Any(u => u.Id == user.Id)).Select(g => g.CompanyId).ToList();
            return dbContext.PlanningAreas.Include(pa => pa.Company).Where(pa => companyIds.Any(g => g == pa.UsedByCompanyId)).ToList();
        }

        public List<DBIntegration> GetAllIntegrationsForCompany(int a_companyId)
        {
            using var dbContext = getDbContext();
            return dbContext.Companies.Include(c => c.Integrations).Where(c => c.Id == a_companyId).Select(c => c.Integrations).FirstOrDefault();
        }
        
        public List<DBIntegration> GetAllIntegrationsForCompanies(List<int> a_companyIds)
        {
            using var dbContext = getDbContext();
            return dbContext.Companies.Include(c => c.Integrations).Where(c => a_companyIds.Contains(c.Id)).SelectMany(c => c.Integrations).ToList();
        }

        public CompanyServer GetServerSettings(int a_serverId)
        {
            using var dbContext = getDbContext();
            return dbContext.CompanyServers.FirstOrDefault(server => server.Id == a_serverId);
        }

        public CompanyServer GetServerSettings(string a_planningAreaKey)
        {
            using var dbContext = getDbContext();
            var planningArea = dbContext.PlanningAreas.FirstOrDefault(pa => pa.PlanningAreaKey.Equals(a_planningAreaKey));

            if (planningArea == null)
            {
                return null;
            }

            return dbContext.CompanyServers.FirstOrDefault(server => server.Id == planningArea.ServerId);
        }

        public DBIntegration? GetIntegrationForUser(int integrationId, string a_userEmail)
        {
            using var dbContext = getDbContext();
            User? user = dbContext.Users.FirstOrDefault(u  => u.Email == a_userEmail);
            if (user == null)
            {
                return null;
            }
            var companyIds = dbContext.Roles.Where(g => g.Permissions.Any(p => p == "PullAndPublishIntegration") && g.Users.Any(u => u.Id == user.Id)).Select(g => g.CompanyId).ToList();
            var integration = dbContext.DBIntegrations.Include(i => i.IntegrationDBObjects).FirstOrDefault(i => i.Id == integrationId && companyIds.Any(c => c == i.CompanyId));
            return integration;
        }

        public bool SetActiveDBIntegration(string paPlanningAreaKey, int integrationId)
        {
            using var dbContext = getDbContext();

            var pa = dbContext.PlanningAreas.FirstOrDefault(pa => pa.PlanningAreaKey == paPlanningAreaKey);

            if (pa == null)
            {
                return false;
            }
            
            pa.DBIntegrationId = integrationId;

            dbContext.PlanningAreas.Update(pa);
            dbContext.SaveChanges();
            return true;
        }

        public DBIntegration? GetIntegrationForPA(string paKey, string a_userIdentifier)
        {
            using var dbContext = getDbContext();
            User? user = dbContext.Users.FirstOrDefault(u  => u.Email == a_userIdentifier);
            if (user == null)
            {
                return null;
            }
            var companyIds = dbContext.Roles.Where(g => g.Permissions.Any(p => p == "PullAndPublishIntegration") && g.Users.Any(u => u.Id == user.Id)).Select(g => g.CompanyId).ToList();

            return dbContext.PlanningAreas.Include(pa => pa.CurrentIntegration).Include(pa => pa.CurrentIntegration.IntegrationDBObjects).FirstOrDefault(pa =>
                pa.PlanningAreaKey == paKey && companyIds.Any(c => c == pa.UsedByCompanyId))?.CurrentIntegration;
        }

        public GetIntegrationDataResponse? GetIntegrationDataForPA(string paKey, string a_userIdentifier)
        {
            using var dbContext = getDbContext();
            User? user = dbContext.Users.AsNoTracking().Include(u => u.Groups).FirstOrDefault(u  => u.Email == a_userIdentifier);
            if (user == null)
            {
                return null;
            }
            var companiesUserCanPullFrom = dbContext.Roles
                                      .Where(g => g.Permissions.Any(p => p == "PullAndPublishIntegration") && g.Users.Any(u => u.Id == user.Id))
                                      .Select(g => g.CompanyId).ToList();

            var planningArea = dbContext.PlanningAreas.FirstOrDefault(pa =>
                pa.PlanningAreaKey == paKey && companiesUserCanPullFrom.Any(c => c == pa.UsedByCompanyId));

            if (planningArea == null)
            {
                return null;
            }
            
            var settingsString = GetPlanningAreaSettings(planningArea.PlanningAreaKey);
            var obj = ((JObject)JsonConvert.DeserializeObject<JObject>(settingsString));
                
            string conString = (string)(obj["Settings"]["ErpDatabaseSettings"]["ConnectionString"]); //is it worth it bring over all of the classes from core to avoid doing JObject crap

            using SqlConnection con = new SqlConnection(conString);
            DBIntegrationDTO dbIntegrationDto;
            try
            {
                dbIntegrationDto = DbSchemaExtractor.RetrieveDBSchema(con);
            }
            catch (Exception e)
            {
                //todo handle exceptions
                throw e;
            }

            
            if (user.Groups.Any(g => g.Permissions.Any(p => p == "PullIntegrationData")))
            {
                return GetData(con, dbIntegrationDto);
            }
            GetIntegrationDataResponse result = new GetIntegrationDataResponse();
            result.Integration = dbIntegrationDto;
            return result;
        }

        GetIntegrationDataResponse GetData(SqlConnection con, DBIntegrationDTO dto)
        {
            con.Open();
            GetIntegrationDataResponse result = new GetIntegrationDataResponse();
            foreach (var table in dto.IntegrationTableObjects)
            {
                try
                {
                    var dataTableJson = new DataTableJson();
                    dataTableJson.ConstructTable(RetrieveDataTable(con, table.ObjectName));
                    result.TableData.Add(table.ObjectName, dataTableJson);
                }
                catch (Exception e)
                {
                    //not sure what to do other than die (or how to properly log in the webapp) TODO hook this up to sentry (and everywhere else its needed)
                    throw;
                }
            }

            result.Integration = dto;
            con.Close();
            return result;
        }

        DataTable RetrieveDataTable(SqlConnection con, string tableName)
        {
            using var sqlCommand = con.CreateCommand();
            sqlCommand.CommandText = "SELECT * FROM " + tableName;
            using var reader = sqlCommand.ExecuteReader();
            DataTable table = new DataTable();
            var schema = reader.GetColumnSchema();
            foreach (var dbColumn in schema)
            {
                table.Columns.Add(dbColumn.ColumnName, typeof(string)); //store everything as strings, getting the actual types would be annoying and isnt needed, sql server will auto cast from strings when we insert
            }

            while (reader.Read())
            {
                var row = table.NewRow();
                foreach (var dbColumn in schema)
                {
                    row[dbColumn.ColumnName] = reader[dbColumn.ColumnName];
                }
                table.Rows.Add(row);
            }
            return table;
        }

        public List<DataConnector> GetDataConnectorsForCompany(int a_companyId)
        {
            using var dbContext = getDbContext();
            var connectors = dbContext.DataConnectors.AsNoTracking().Where(c => c.CompanyId == a_companyId).ToList();
            foreach (DataConnector dataConnector in connectors)
            {
                dataConnector.ConnectionString = ""; //wipe the connection strings since this is going to the PA
            }

            return connectors;
        }

        public DataConnector? GetDataConnector(int a_connectorId)
        {
            using var dbContext = getDbContext();
            return dbContext.DataConnectors.AsNoTracking().FirstOrDefault(c => c.Id == a_connectorId);
        }

        public GetIntegrationDataResponse? GetIntegrationDataForDataConnector(int a_connectorId, string a_userIdentifier)
        {
            using var dbContext = getDbContext();
            User? user = dbContext.Users.AsNoTracking().FirstOrDefault(u => u.Email == a_userIdentifier);
            if (user == null)
            {
                return null;
            }
            var companiesUserCanPullFrom = dbContext.Roles
                                                    .Where(g => g.Permissions.Any(p => p == "adminIntegrations") && g.Users.Any(u => u.Id == user.Id))
                                                    .Select(g => g.CompanyId).ToList();

            var dataConnector = dbContext.DataConnectors.FirstOrDefault(c =>
                c.Id == a_connectorId && companiesUserCanPullFrom.Any(company => company == c.CompanyId));

            if (dataConnector == null)
            {
                return null;
            }
            
            string conString = dataConnector.ConnectionString;

            conString = Regex.Replace(
                conString,
                @"(?<=Data Source=[^;]*)\\{2,}",
                @"\"
            ); //Fix issues with pasting in connection string generated from ssms

            using SqlConnection con = new SqlConnection(conString);
            DBIntegrationDTO dbIntegrationDto;
            try
            {
                dbIntegrationDto = DbSchemaExtractor.RetrieveDBSchema(con);
            }
            catch (Exception e)
            {
                //todo handle exceptions
                throw e;
            }

            //if (user.Groups.Any(g => g.Permissions.Any(p => p == "adminIntegrations")))
            if(
               dbContext.Roles.Any(r =>
                   r.Permissions.Any(p => p == "adminIntegrations") &&
                   r.Users.Any(u => u.Id == user.Id)
               ))
            {
                return GetData(con, dbIntegrationDto);
            }
            GetIntegrationDataResponse result = new GetIntegrationDataResponse();
            result.Integration = dbIntegrationDto;
            return result;
        }

        public bool VerifyCompanyApiKey(int companyId, string apiKey)
        {
            using var dbContext = getDbContext();
            string apiKeyFromDb = dbContext.Companies.FirstOrDefault(c => c.Id == companyId).ApiKey;
            if (apiKeyFromDb == null || apiKeyFromDb != apiKey) 
                return false;
            
            return true;
        }
        
        public bool UpdateLicenseStatus(string paKey, ELicenseStatus licenseStatus)
        {
            using var dbContext = getDbContext();
            PADetails? pa = dbContext.PlanningAreas.FirstOrDefault(pa => pa.PlanningAreaKey == paKey);
            if (pa == null)
            {
                return false;
            }
            
            pa.LicenseStatus = licenseStatus;
            dbContext.SaveChanges();
            return true;
        }

        public PADetails? GetPAWithApiKey(string apiKey)
        {
            using var dbContext = getDbContext();
            return dbContext.PlanningAreas.FirstOrDefault(pa => pa.ApiKey == apiKey);
        }

        /// <summary>
        /// Gets planning areas that have the specified Site ID in their settings
        /// </summary>
        /// <param name="siteId">The Site ID to search for</param>
        /// <returns>List of planning areas with the specified Site ID</returns>
        public List<PADetails> GetPlanningAreasBySiteId(string siteId)
        {
            using var dbContext = getDbContext();
            
            // Get all planning areas and filter by SiteId in settings
            var planningAreas = dbContext.PlanningAreas
                .Include(pa => pa.Company)
                .Where(pa => pa.RegistrationStatus == ERegistrationStatus.Created.ToString() || 
                           pa.RegistrationStatus == ERegistrationStatus.Deleting.ToString())
                .ToList();

            // Filter by SiteId in the JSON settings
            var matchingPlanningAreas = new List<PADetails>();
            
            foreach (var pa in planningAreas)
            {
                if (!string.IsNullOrEmpty(pa.Settings))
                {
                    try
                    {
                        var settingsJson = JsonConvert.DeserializeObject<dynamic>(pa.Settings);
                        var settingSiteId = settingsJson?.LicenseInfo?.SiteId?.ToString();
                        
                        if (!string.IsNullOrEmpty(settingSiteId) && settingSiteId.Equals(siteId, StringComparison.OrdinalIgnoreCase))
                        {
                            matchingPlanningAreas.Add(pa);
                        }
                    }
                    catch
                    {
                        // Skip planning areas with malformed JSON settings
                        continue;
                    }
                }
            }

            return matchingPlanningAreas;
        }

        /// <summary>
        /// Gets all companies from the database
        /// </summary>
        /// <returns>List of all companies</returns>
        public async Task<List<Company>> GetAllCompaniesAsync()
        {
            using var dbContext = getDbContext();
            return await dbContext.Companies
                .Include(c => c.Integrations)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a specific company by ID
        /// </summary>
        /// <param name="companyId">Company ID</param>
        /// <returns>Company or null if not found</returns>
        public async Task<Company?> GetCompanyByIdAsync(int companyId)
        {
            using var dbContext = getDbContext();
            return await dbContext.Companies
                .Include(c => c.Integrations)
                .FirstOrDefaultAsync(c => c.Id == companyId);
        }
    }
}
