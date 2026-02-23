using System.Data;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using WebAPI.Models;
using WebAPI.Models.Integration;
using WebAPI.RequestsAndResponses;

using static WebAPI.Models.ServiceStatus;

namespace WebAPI.DAL
{
    public class ServerManagerActionDbService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AzureTableService _tableService;

        public ServerManagerActionDbService(IServiceProvider serviceProvider, AzureTableService tableService)
        {
            _serviceProvider = serviceProvider;
            _tableService = tableService;
        }

        private CompanyDBContext GetDbContext()
        {
            // Resolve a new scoped DbContext using the IServiceProvider
            return _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<CompanyDBContext>();
        }

        public async Task<ServerManagerActionRequest> GetNextRequest(string ServerId)
        {
            ServerManagerActionRequest result;
            using (var dbContext = GetDbContext())
            {
                result = await dbContext.ServerManagerActionRequests.Include(s=>s.Server).Where(t => t.Server.AuthToken == ServerId && t.RequestStatus.ToLower() == "new").OrderBy(s=>s.RequestedDateTime).FirstOrDefaultAsync();
                if (result != null)
                {
                    result.RequestStatus = "Processing";
                    result.UpdatedDateTime = DateTime.UtcNow;
                    dbContext.Update(result);
                    await dbContext.SaveChangesAsync();
                }
            }
            return result;
        }

        public record ActionStatusList(List<ActionStatus> StatusList);
        public record ActionStatus(Guid TransactionId, string RequestStatus, string? ErrorMessage, DateTime? UpdatedDateTime);
        public async Task<ActionStatus> UpdateRequest(WebApiActionFollowup a_updatedRequest, CompanyServer a_server)
        {
            ServerManagerActionRequest? dbRequest;
            using (var dbContext = GetDbContext())
            {
                dbRequest = await dbContext.ServerManagerActionRequests.Where(r => r.TransactionId == a_updatedRequest.TransactionId).FirstOrDefaultAsync();
                if (dbRequest != null)
                {

                    dbRequest.RequestStatus = a_updatedRequest.Status;
                    dbRequest.ErrorMessage = a_updatedRequest.ErrorMessage;
                    dbRequest.UpdatedDateTime = DateTime.UtcNow;
                    ActionStatus result = new(a_updatedRequest.TransactionId, dbRequest.RequestStatus, dbRequest.ErrorMessage, dbRequest.UpdatedDateTime);
                    
                    if (a_updatedRequest.Status.Equals(EActionRequestStatuses.Success.ToString(),
                            StringComparison.OrdinalIgnoreCase))
                    {
                        HandleActionUpdateStep(a_updatedRequest, dbRequest, dbContext, a_server);
                    }

                    dbContext.Update(dbRequest);
                    await dbContext.SaveChangesAsync();
                    return result;
                }
                throw new Exception("TransactionId not found");
            }
        }

        public async Task UpdateAgentStatus(WebApiAgentStatusUpdate a_updatedRequest, CompanyServer a_server)
        {
            using (var dbContext = GetDbContext())
            {
                // Check if version is new
                var server = dbContext.CompanyServers.First(x => x.Id == a_server.Id);
                server.SystemId = a_updatedRequest.SystemId;

                if (Version.TryParse(server.Version, out var webVersion) && webVersion < a_updatedRequest.Version)
                {
                    // Update server version
                    server.Version = a_updatedRequest.Version.ToString();

                    // Close any existing upgrade/restart requests for this server
                    var dbRequests = await dbContext.ServerManagerActionRequests
                        .Where(r => r.ServerId == a_server.Id
                                    && (r.Action == EServerActionTypes.UpgradeServerAgent.ToString() || r.Action == EServerActionTypes.RestartServer.ToString())
                                    && (r.RequestStatus == EActionRequestStatuses.Processing.ToString() || r.RequestStatus == EActionRequestStatuses.New.ToString()))
                        .ToListAsync();
                    foreach (var dbRequest in dbRequests)
                    {
                        dbRequest.RequestStatus = EActionRequestStatuses.Success.ToString();
                        dbRequest.UpdatedDateTime = DateTime.Now.ToUniversalTime();

                        dbContext.Update(dbRequest);
                    }
                }
                
                // Update certs
                var existingServerCertificates = await dbContext.ServerCertificates.Where(x => x.CompanyServerId == a_server.Id).ToListAsync();
                // Create a copy of the certs to track whether each one is present in the update
                var unprocessedCerts = existingServerCertificates.ToList();

                foreach (ServerCertificateDTO cert in a_updatedRequest.Certificates)
                {
                    ServerCertificate existingCert = existingServerCertificates.FirstOrDefault(x => x.Thumbprint == cert.Thumbprint);
                    if (existingCert != null)
                    {
                        existingCert.Thumbprint = cert.Thumbprint;
                        existingCert.Issuer = cert.Issuer;
                        existingCert.Name = cert.Name;
                        existingCert.SubjectName = cert.SubjectName;
                        existingCert.ValidFrom = cert.ValidFrom;
                        dbContext.ServerCertificates.Update(existingCert);
                    }
                    else
                    {
                        dbContext.ServerCertificates.Add(new ServerCertificate()
                        {
                            CompanyServerId = a_server.Id,
                            Thumbprint = cert.Thumbprint,
                            Issuer = cert.Issuer,
                            Name = cert.Name,
                            SubjectName = cert.SubjectName,
                            ValidFrom = cert.ValidFrom
                        });
                    }

                    // Mark this cert as processed
                    unprocessedCerts.Remove(existingCert);
                }

                // Any certs in the DB that weren't processed above are no longer present on the Server Machine. Remove them
                dbContext.RemoveRange(unprocessedCerts);

                await dbContext.SaveChangesAsync();
            }
        }

        public async Task<PlanningAreaStatusList> UpdateStatuses(WebApiStatusUpdate a_statuses)
        {
            using (var dbContext = GetDbContext())
            {
                var server =
                    await dbContext.CompanyServers.FirstOrDefaultAsync(x => x.AuthToken == a_statuses.ServerAuthToken);

                if (server == null)
                {
                    throw new DataException("Could not validate server.");
                }

                if (a_statuses.Port != 0)
                {
                    server.ApiPort = a_statuses.Port.ToString();
                    dbContext.Update(server);
                    await dbContext.SaveChangesAsync();
                }

                if (a_statuses.AvailableVersions != null)
                {
                    var versionString = string.Join(",", a_statuses.AvailableVersions);
                    if (a_statuses?.AvailableVersions != null && server.LocalVersions != versionString)
                    {
                        server.LocalVersions = versionString;
                        await dbContext.SaveChangesAsync();
                    }
                }
                
                List<PlanningAreaStatus> statusList = new ();
                // SA is reporting issues with PA backup linking, correct any backup linking issues
                if (a_statuses.Statuses == null)
                {
                    var groups = dbContext.PlanningAreas.Where(x => x.ServerId == server.Id).GroupBy(x => x.PlanningAreaKey);
                    foreach (var group in groups)
                    {
                        // If there is only one PA with a specific key, it should not be marked as a backup
                        if (group.Count() == 1 && group.First().IsBackup)
                        {
                            var pa = group.First();
                            pa.BackupOf = null;
                            dbContext.Update(pa);
                            continue;
                        }
                        // Multiple PAs with the same key are marked as versions. Mark older versions as backups
                        if (group.Count(x => !x.IsBackup) > 1)
                        {
                            // Try to find a running instance to mark as active
                            var primary = group.Where(x => !x.IsBackup 
                                                           && x.ServiceState != ServiceStatus.EServiceState.Stopped
                                                           && x.ServiceState != ServiceStatus.EServiceState.NotFound
                                                           && x.ServiceState != ServiceStatus.EServiceState.NotFound
                                                           && x.ServiceState != ServiceStatus.EServiceState.Stopping).OrderByDescending(x => Version.Parse(x.Version)).FirstOrDefault();
                            // If none of the instances are running, mark the latest version as active
                            primary ??= group.Where(x => !x.IsBackup).OrderByDescending(x => Version.Parse(x.Version)).First();
                            foreach (PADetails pa in group)
                            {
                                if (pa.Id != primary.Id)
                                {
                                    pa.BackupOf = primary.Id;
                                    dbContext.Update(pa);
                                }
                            }
                            continue;
                        }
                        // If none of the PAs with a given key are active, mark the last one as active
                        if (group.Count(x => !x.IsBackup) == 0)
                        {
                            var primary = group.OrderByDescending(x => Version.Parse(x.Version)).First();
                            primary.BackupOf = null;
                            dbContext.Update(primary);
                            foreach (PADetails pa in group)
                            {
                                if (pa.Id != primary.Id)
                                {
                                    pa.BackupOf = primary.Id;
                                    dbContext.Update(pa);
                                }
                            }
                            continue;
                        }
                    }

                    await dbContext.SaveChangesAsync();
                    return new PlanningAreaStatusList(statusList, server.Id);
                }

                foreach (var pa in dbContext.PlanningAreas.Where(x => x.ServerId == server.Id && x.BackupOf == null).ToList())
                {
                    if (a_statuses.Statuses.TryGetValue(pa.PlanningAreaKey, out var status))
                    {
                        if (pa.ServiceState != status.State)
                        {
                            statusList.Add(new PlanningAreaStatus(pa.PlanningAreaKey, status.State));
                        }
                        pa.ServiceState = status.State;
                        pa.ServiceStateUpdated = DateTime.UtcNow;
                        if (status.LicenseStatus != ELicenseStatus.Unknown)
                        {
                            //since the license status can only be known while the system is running we can just persist the last known license state
                            //TODO: Update this once we can get license state while instance is offline
                            pa.LicenseStatus = status.LicenseStatus;
                        }
                    }
                    else
                    {
                        if (pa.ServiceState != EServiceState.NotFound)
                        {
                            statusList.Add(new PlanningAreaStatus(pa.PlanningAreaKey, EServiceState.Unknown));
                        }
                        pa.ServiceState = EServiceState.NotFound;
                        pa.ServiceStateUpdated = DateTime.UtcNow;
                        pa.LicenseStatus = ELicenseStatus.Unknown;
                    }
                }

                // Update server activity
                server.LastActivity = DateTime.UtcNow;
                dbContext.Update(server);

                await dbContext.SaveChangesAsync();

                // Check for software updates
                if (ShouldCheckForUpdates(server) && await CheckForUpdates(server))
                {
                    await CreateUpdateRequest(server);
                }

                return new PlanningAreaStatusList(statusList, server.Id);
            }
        }

        public async Task FixBackupLinks(Company company)
        {
            using (var dbContext = GetDbContext())
            {
                
                
            }
        }

        private bool ShouldCheckForUpdates(CompanyServer server)
        {
            if (server.AutomaticUpdateFrequency == "Weekly")
            {
                // Make sure the server has not checked for updates yet this week, add one hour to prevent drift
                if (server.LastUpdateCheck.HasValue && server.LastUpdateCheck.Value > DateTime.UtcNow.AddDays(-7).AddHours(1))
                {
                    return false;
                }

                var currentDay = Enum.GetName(typeof(DayOfWeek), DateTime.UtcNow.DayOfWeek);
                if (server.AutomaticUpdateDay == currentDay)
                {
                    if (server.AutomaticUpdateHour == null || server.AutomaticUpdateHour == DateTime.UtcNow.Hour)
                    {
                        return true;
                    }
                }
            } 
            else if (server.AutomaticUpdateFrequency == "Daily")
            {
                // Make sure the server has not checked for updates yet today
                if (server.LastUpdateCheck.HasValue && server.LastUpdateCheck.Value > DateTime.UtcNow.AddHours(-23))
                {
                    return false;
                }

                if (server.AutomaticUpdateHour == null || server.AutomaticUpdateHour == DateTime.UtcNow.Hour)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> CheckForUpdates(CompanyServer server)
        {
            using (var dbContext = GetDbContext())
            {
                server.LastUpdateCheck = DateTime.UtcNow;
                dbContext.Update(server);
                await dbContext.SaveChangesAsync();
            }

            var currentVersion = Version.Parse(server.Version);
            var latestVersion = await _tableService.GetLatestServerAgentVersion();

            if (latestVersion == null) return false;

            var latest = Version.Parse(latestVersion.VersionNumber);
            if (currentVersion < latest)
            {
                return true;
            }
            // Version is equal or newer than the latest
            return false;
        }

        private async Task CreateUpdateRequest(CompanyServer server)
        {
            using (var dbContext = GetDbContext())
            {
                var req = new ServerManagerActionRequest()
                {
                    ServerId = server.Id,
                    TransactionId = Guid.NewGuid(),
                    Action = EServerActionTypes.UpgradeServerAgent.ToString(),
                    RequestedDateTime = DateTime.UtcNow,
                    RequestStatus = EActionRequestStatuses.New.ToString(),
                    ParameterJson = "",
                    RequestedBy = "Automatic Update"
                };

                dbContext.ServerManagerActionRequests.Add(req);
                await dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Completes WebApp-side work for an action/ action update.
        /// </summary>
        /// <param name="a_actionRequest"></param>
        /// <param name="a_dbRequest"></param>
        /// <param name="a_dbContext"></param>
        /// <param name="a_server"></param>
        private void HandleActionUpdateStep(WebApiActionUpdate a_actionRequest, ServerManagerActionRequest a_dbRequest,
            CompanyDBContext a_dbContext, CompanyServer a_server)
        {
            try
            {
                switch (a_dbRequest.Action)
                {
                    case string act when act.Equals(EServerActionTypes.AddPlanningArea.ToString(), StringComparison.OrdinalIgnoreCase):
                        HandleAddUpdatePlanningAreaUpdate(a_actionRequest, a_dbContext, a_server);
                        break; 
                    case string act when act.Equals(EServerActionTypes.CopyPlanningArea.ToString(), StringComparison.OrdinalIgnoreCase):
                        HandleAddUpdatePlanningAreaUpdate(a_actionRequest, a_dbContext, a_server);
                        break;
                    case string act when act.Equals(EServerActionTypes.UpdatePlanningAreaSettings.ToString(), StringComparison.OrdinalIgnoreCase):
                        HandleAddUpdatePlanningAreaUpdate(a_actionRequest, a_dbContext, a_server);
                        break;
                    case string act when act.Equals(EServerActionTypes.DeletePlanningArea.ToString(), StringComparison.OrdinalIgnoreCase):
                        HandleDeletePlanningAreaActionUpdate(a_actionRequest, a_dbContext, a_server);
                        break;
                    case string act when act.Equals(EServerActionTypes.UpdateServerSettings.ToString(), StringComparison.OrdinalIgnoreCase):
                        HandleUpdateServerSettingsActionUpdate(a_actionRequest, a_server, a_dbContext);
                        break;
                    case string act when act.Equals(EServerActionTypes.UnregisterServer.ToString(), StringComparison.OrdinalIgnoreCase):
                        HandleUnregisterServerActionUpdate(a_actionRequest, a_server, a_dbContext);
                        break;
                    case string act when act.Equals(EServerActionTypes.UpgradePlanningArea.ToString(), StringComparison.OrdinalIgnoreCase):
                        HandleUpgradeInstanceActionUpdate(a_actionRequest, a_server, a_dbContext);
                        break;
                }
            }
            catch (Exception ex)
            {
                // The action was done on the SM. We shouldn't do it there again, everything left to do should be here in the Webapp (and in a_updatedRequest)
                // TODO: Implement a retry mechanism?
                a_dbRequest.RequestStatus = EActionRequestStatuses.Error.ToString();
                a_dbRequest.ErrorMessage = $"The action completed, but an error occurred handling the response:{Environment.NewLine}" +
                                         $"{ex.Message}";
            }
        }

        /// <summary>
        /// Creates a new request from the SM to the webapp, for actions originating from the SM.
        /// The <see cref="EServerActionTypes"/> of the request will determine what followup actions the WebApp takes -
        /// this can be any existing action, but it's assumed that the SM has already taken any steps that it would have done
        /// if the action had originated from the WebApp.
        /// (e.g. If the SM sends a request of type <see cref="EServerActionTypes.DeleteAction"/>, it should have already done the work to remove the server service/ directories etc.)
        /// </summary>
        /// <param name="a_newAction"></param>
        /// <param name="a_server"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task HandleSMSideRequest(WebApiActionFromServer a_newAction, CompanyServer a_server)
        {
            ServerManagerActionRequest request = await RequestServerAction(a_server.Id, a_newAction.ActionType, a_newAction.Parameters, 
                a_server.ServerName, EActionRequestStatuses.NewFromServer);

            CompanyDBContext dbContext = GetDbContext();

            HandleActionUpdateStep(a_newAction, request, dbContext, a_server);

            request.RequestStatus = EActionRequestStatuses.Success.ToString();
            dbContext.Update(request);
            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Adds new records in the db for any new permissions
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="command"></param>
        /// <param name="jsonParams"></param>
        /// <param name="requesterEmail"></param>
        /// <param name="requestStatus"></param>
        /// <param name="permissions">The permissions to add</param>
        private async Task<ServerManagerActionRequest> RequestServerAction(int serverId, string command, string jsonParams, string requesterEmail,
            EActionRequestStatuses requestStatus = EActionRequestStatuses.New)
        {
            using var context = GetDbContext();
            var action = new ServerManagerActionRequest()
            {
                TransactionId = Guid.NewGuid(),
                ServerId = serverId,
                Action = command,
                ParameterJson = jsonParams,
                RequestedBy = requesterEmail,
                RequestStatus = requestStatus.ToString(),
                ErrorMessage = null,
                RequestedDateTime = DateTime.UtcNow,
                UpdatedDateTime = null,
            };

            context.ServerManagerActionRequests.Add(action);

            await context.SaveChangesAsync();

            return action;
        }

        /// <summary>
        /// Update a planning area settings after the service was created in an Add/Copy action
        /// </summary>
        /// <param name="a_actionRequest"></param>
        /// <param name="a_dbContext"></param>
        /// <param name="a_server"></param>
        /// <exception cref="Exception"></exception>
        private void HandleAddUpdatePlanningAreaUpdate(WebApiActionUpdate a_actionRequest, CompanyDBContext a_dbContext, CompanyServer a_server)
        {
            WebCreateInstanceResponseParams updatedPlanningArea = JsonSerializer.Deserialize<WebCreateInstanceResponseParams>(a_actionRequest.Parameters);

            var dbPlanningArea = updatedPlanningArea.NewVersion == null ? 
                a_dbContext.PlanningAreas.FirstOrDefault(pa => pa.PlanningAreaKey == updatedPlanningArea.PlanningAreaKey) :
                a_dbContext.PlanningAreas.FirstOrDefault(pa => pa.PlanningAreaKey == updatedPlanningArea.PlanningAreaKey && pa.Version == updatedPlanningArea.NewVersion);

            if (dbPlanningArea == null)
            {
                // While generally, we'd expect the PA to exist from now if created from the WebApp, that isn't true if the SM is creating the instance. In that case, add it now.
                if (a_actionRequest.Status.Equals(EActionRequestStatuses.NewFromServer.ToString()))
                {
                    dbPlanningArea = CreateNewPAModel(a_server, updatedPlanningArea);
                    a_dbContext.Add(dbPlanningArea);
                    return;
                }
                else
                {
                    throw new Exception("Planning area settings can no longer be found in Webapp database.");
                }
            }

            dbPlanningArea.RegistrationStatus = ERegistrationStatus.Created.ToString();
            dbPlanningArea.ServiceState = updatedPlanningArea?.Starting == true ? ServiceStatus.EServiceState.Starting : ServiceStatus.EServiceState.Stopped;
            dbPlanningArea.ServiceStateUpdated = DateTime.UtcNow;
            dbPlanningArea.Settings = updatedPlanningArea.PlanningAreaSettings;
            // EF saves single transaction outside of this method.
        }

        private PADetails CreateNewPAModel(CompanyServer a_server, WebCreateInstanceResponseParams? updatedPlanningArea)
        {
            PADetails dbPlanningArea;
            var envTypeName = GetEnvTypeFromJson(updatedPlanningArea);
            dbPlanningArea = new PADetails()
            {
                PlanningAreaKey = GetStringValFromJson(updatedPlanningArea.PlanningAreaSettings, "InstanceId", "Settings"),
                ServerId = a_server.Id,
                CompanyId = a_server.ManagingCompanyId,
                CreationDate = DateTime.UtcNow,
                Environment = envTypeName,
                Name = GetStringValFromJson(updatedPlanningArea.PlanningAreaSettings, "InstanceName", "PublicInfo"),
                Version = GetStringValFromJson(updatedPlanningArea.PlanningAreaSettings, "SoftwareVersion", "PublicInfo"),
                RegistrationStatus = ERegistrationStatus.Created.ToString(),
                Settings = updatedPlanningArea.PlanningAreaSettings
            };
            return dbPlanningArea;
        }

        private string? GetEnvTypeFromJson(WebCreateInstanceResponseParams? updatedPlanningArea)
        {
            string envIdx = GetStringValFromJson(updatedPlanningArea.PlanningAreaSettings, "EnvironmentType", "PublicInfo");
            string envTypeName = null;
            if (!envIdx.IsNullOrEmpty() && int.TryParse(envIdx, out int idx))
            {
                envTypeName = Enum.GetName(typeof(EnvironmentType), idx);
            }

            return envTypeName;
        }

        /// <summary>
        /// Pulls out string values from Json object.
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="a_key"></param>
        /// <param name="a_subDir1">The direct parent of the desired key, if applicable</param>
        /// <param name="a_subDir2">The grandparent of the desired key, if applicable</param>
        /// <returns></returns>
        private string GetStringValFromJson(string jsonString, string a_key, string a_subDir1 = null, string a_subDir2 = null)
        {
            using JsonDocument doc = JsonDocument.Parse(jsonString);

            JsonElement jsonElement;
            if (!a_subDir2.IsNullOrEmpty() && !a_subDir1.IsNullOrEmpty())
            {
                if (doc.RootElement.GetProperty(a_subDir2).GetProperty(a_subDir1).TryGetProperty(a_key, out jsonElement))
                {
                    return jsonElement.ToString();
                }
            }
            else if (!a_subDir1.IsNullOrEmpty())
            {
                if (doc.RootElement.GetProperty(a_subDir1).TryGetProperty(a_key, out jsonElement))
                {
                    return jsonElement.ToString();
                }
            }
            else
            {
                if (doc.RootElement.TryGetProperty(a_key, out jsonElement))
                {
                    return jsonElement.ToString();
                }
            }

            return null;
        }


        /// <summary>
        /// Takes a request from the server to uninstall and clears WebApp data for that server.
        /// Dev note: I know this doesn't match the other signatures - we don't want to include the AuthToken in params
        /// in case we want to show that data in the webapp UI.
        /// </summary>
        /// <param name="_"></param>
        /// <param name="a_server"></param>
        /// <param name="a_dbContext"></param>
        /// <exception cref="Exception"></exception>
        private void HandleUnregisterServerActionUpdate(WebApiActionUpdate _, CompanyServer a_server, CompanyDBContext a_dbContext)
        {
            // Get server ref from current db context, since we need to update it.
            var server = a_dbContext.CompanyServers.FirstOrDefault(server => server.AuthToken == a_server.AuthToken);
            if (server == null)
            {
                throw new Exception($"Attempted to unregister a server that does not exist.");
            }

            IQueryable<PADetails> serverPlanningAreas = a_dbContext.PlanningAreas
                .Where(pa => pa.ServerId == server.Id);

            // TODO: We may want to store these in an undeleted state for records (or get audit logging implemented soon).
            a_dbContext.RemoveRange(serverPlanningAreas);
            a_dbContext.Remove(server);
        }

        private void HandleUpgradeInstanceActionUpdate(WebApiActionUpdate a_actionRequest, CompanyServer a_server, CompanyDBContext a_dbContext)
        {
            var upgradedInstanceParams = JsonSerializer.Deserialize<WebUpgradeInstanceResponseParams>(a_actionRequest.Parameters);

            // Get server ref from current db context, since we need to update it.
            var oldPa = a_dbContext.PlanningAreas.FirstOrDefault(pa => pa.ServerId == a_server.Id 
                                                                        && pa.Name == upgradedInstanceParams.InstanceName
                                                                        && pa.Version == upgradedInstanceParams.OldVersion);

            var newPa = a_dbContext.PlanningAreas.FirstOrDefault(pa => pa.ServerId == a_server.Id
                                                                       && pa.Name == upgradedInstanceParams.InstanceName
                                                                       && pa.Version == upgradedInstanceParams.NewVersion);

            if (oldPa == null || newPa == null)
            {
                throw new Exception($"Attempted to upgrade a PA that doesn't exist");
            }

            oldPa.BackupOf = newPa.Id;
            oldPa.UpdateDate = DateTime.UtcNow;

            // Ensure all existing backups point at the upgraded PA
            var backups = a_dbContext.PlanningAreas.Where(x => x.BackupOf == oldPa.Id);
            foreach (var backup in backups)
            {
                backup.BackupOf = newPa.Id;
                a_dbContext.Update(backup);
            }

            newPa.BackupOf = null;
            newPa.RegistrationStatus = ERegistrationStatus.Created.ToString();
            a_dbContext.Update(oldPa);
            a_dbContext.Update(newPa);
            a_dbContext.SaveChanges();
        }

        private void HandleUpdateServerSettingsActionUpdate(WebApiActionUpdate a_actionRequest, CompanyServer a_server, CompanyDBContext a_dbContext)
        {
            ServerSettingsDto updatedServerSettings = JsonSerializer.Deserialize<WebUpdateServerSettingsRequestParams>(a_actionRequest.Parameters).ServerSettings;

            // Get server ref from current db context, since we need to update it.
            var server = a_dbContext.CompanyServers.FirstOrDefault(server => server.AuthToken == a_server.AuthToken);
            if (server == null)
            {
                throw new Exception($"Attempted to update settings for a server that does not exist.");
            }

            server.ServerName= updatedServerSettings.ServerName;
            server.ComputerNameOrIP = updatedServerSettings.ComputerNameOrIP;
            server.Thumbprint = updatedServerSettings.Thumbprint;
            server.ApiPort = updatedServerSettings.ApiPort;
            server.SsoClientId = updatedServerSettings.SsoClientId;
            server.SsoDomain = updatedServerSettings.SsoDomain;
            server.SystemId = updatedServerSettings.SystemId;
            server.AdminMessage = updatedServerSettings.AdminMessage;
            server.Version = updatedServerSettings.Version;
        }

        private void HandleDeletePlanningAreaActionUpdate(WebApiActionUpdate a_completedRequest, CompanyDBContext a_dbContext, CompanyServer a_server)
        {
            if (a_completedRequest.Parameters.IsNullOrEmpty()) throw new DataException("The Planning Area was not found.");

            var paToRemove = JsonSerializer.Deserialize<WebInstanceIdentifierResponseParams>(a_completedRequest.Parameters);

            var dbPlanningArea = a_dbContext.PlanningAreas.FirstOrDefault(pa => pa.PlanningAreaKey == paToRemove.PlanningAreaKey && pa.Version == paToRemove.Version);

            if (dbPlanningArea == null)
            {
                throw new Exception("Planning area settings can no longer be found in Webapp database.");
            }

            if (a_completedRequest.Status.Equals(EActionRequestStatuses.Success.ToString(), StringComparison.OrdinalIgnoreCase) ||
                a_completedRequest.Status.Equals(EActionRequestStatuses.NewFromServer.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                a_dbContext.PlanningAreas.Remove(dbPlanningArea);
            }
            else
            {
                // Revert the PA's status so that it shows up again. TODO: Notify the user that the delete failed
                dbPlanningArea.RegistrationStatus = ERegistrationStatus.Created.ToString();
            }
            // EF saves single transaction outside of this method.
        }
    }
}
