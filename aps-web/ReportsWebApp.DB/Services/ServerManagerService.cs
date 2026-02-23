using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ReportsWebApp.Common;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;
using ReportsWebApp.Shared;

namespace ReportsWebApp.DB.Services
{
    public class ServerManagerService : IServerManagerService
    {
        private readonly IDbContextFactory<DbReportsContext> _factory;
        private readonly IAppInsightsLogger _logger;
        private readonly IPlanningAreaDataService _paService;

        public ServerManagerService(IDbContextFactory<DbReportsContext> factory, IAppInsightsLogger logger, IPlanningAreaDataService paService)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _paService = paService;
        }

        /// <summary>
        /// Creates a new DbContext instance.
        /// </summary>
        private DbReportsContext CreateDbContext() => _factory.CreateDbContext();

        /// <summary>
        /// Retrieves all servers managed by a specific company.
        /// Optionally includes the related using companies.
        /// </summary>
        /// <param name="managingCompanyId">The ID of the managing company.</param>
        /// <param name="includeUsingCompanies">Whether to include related using companies.</param>
        /// <returns>A list of servers managed by the given company.</returns>
        public async Task<List<CompanyServer>> GetServersByManagingCompanyAsync(int managingCompanyId, bool includeUsingCompanies = false)
        {
            using var dbContext = CreateDbContext();

            var query = dbContext.CompanyServers
                .Where(cs => cs.ManagingCompanyId == managingCompanyId)
                .Include(cs => cs.Folders).ThenInclude(f => f.Children)
                .Include(cs => cs.Folders).ThenInclude(f => f.PlanningAreas).Include(cs => cs.ServerCertificates)
                .Include(x => x.OwningUser)
                .Include(cs => cs.ManagingCompany);

            if (includeUsingCompanies)
            {
                query = query.Include(cs => cs.UsingCompanies)
                             .ThenInclude(uc => uc.Company);
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Retrieves details of a specific server by its ID.
        /// </summary>
        /// <param name="serverId">The server ID.</param>
        /// <returns>The server entity or null if not found.</returns>
        public async Task<CompanyServer?> GetServerByIdAsync(int serverId)
        {
            using var dbContext = CreateDbContext();
            return await dbContext.CompanyServers
                .Include(cs => cs.ManagingCompany)
                .Include(x => x.OwningUser)
                .Include(cs => cs.UsingCompanies)
                    .ThenInclude(uc => uc.Company)
                .FirstOrDefaultAsync(cs => cs.Id == serverId);
        }

        /// <summary>
        /// Adds a new server or updates an existing one.
        /// </summary>
        /// <param name="server">The server entity.</param>
        /// <returns>The updated or newly added server entity.</returns>
        public async Task<CompanyServer> AddOrUpdateServerAsync(CompanyServer server)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));

            using var dbContext = CreateDbContext();

            var newCompanies = server.UsingCompanies.ToList();

            server.UsingCompanies = new List<ServerUsingCompany>();

            // sanitize inputs
            server.ApiPort = server.ApiPort.Trim();
            server.IPAddress = server.IPAddress.Trim();
            server.AuthToken = server.AuthToken.Trim();
            server.CertificateName = server.CertificateName.Trim();
            server.Thumbprint = server.Thumbprint.Trim();
            server.ComputerNameOrIP = server.ComputerNameOrIP.Trim();
            server.ServerName = server.ServerName.Trim();
            server.SsoClientId = server.SsoClientId.Trim();
            server.SsoDomain = server.SsoDomain.Trim();
            server.Folders = null;

            if (server.Id == 0)
            {
                if (server.OwningUserId == null || server.OwningUserId <= 0)
                {
                    throw new ArgumentException("Newly created servers must have an owner.");
                }
                dbContext.CompanyServers.Add(server);
                await dbContext.SaveChangesAsync();
                newCompanies.ForEach(x => x.CompanyServerId = server.Id);
                dbContext.ServerUsingCompanies.AddRange(newCompanies);

                _logger.LogEvent("Server Added", server.CreatedBy,
                    KeyValuePair.Create("ServerName", server.Name),
                    KeyValuePair.Create("UsingCompaniesAdded", string.Join(',', newCompanies))
                );
            }
            else
            {
                var existingCompanies = dbContext.ServerUsingCompanies
                    .AsNoTracking()
                    .Where(x => x.CompanyServerId == server.Id)
                    .ToList();

                var companyDiff = existingCompanies.Synchronize(newCompanies);
                dbContext.ServerUsingCompanies.AddRange(companyDiff.added);
                dbContext.ServerUsingCompanies.RemoveRange(companyDiff.removed);
                // Ensure PAs are up to date with latest settings changes
                var pas = await dbContext.PlanningAreas.Where(x => x.ServerId == server.Id).ToListAsync();
                foreach (var pa in pas)
                {
                    // Set server to facilitate JSON deserialization
                    pa.Server = server;

                    if (pa.UsedByCompanyId != null && !newCompanies.Any(x => x.CompanyId == pa.UsedByCompanyId))
                    {
                        pa.UsedByCompanyId = null;
                    }
                    
                    var uri = $"https://{server.ComputerNameOrIP}:{pa.PlanningArea.Settings.SystemServiceSettings.Port}/";

                    // Check if update is needed, set new value if necessary, and save
                    if (pa.PlanningArea.PublicInfo.SystemServiceUrl != uri)
                    {
                        pa.PlanningArea.PublicInfo.SystemServiceUrl = uri;

                        var updatedPa = _paService.UpdatePlanningAreaJSON(pa);

                        // Null server to prevent EF from inserting entities
                        pa.Server = null;
                        dbContext.PlanningAreas.Update(updatedPa);
                    }
                }
                 

                dbContext.CompanyServers.Update(server);
            }

            await dbContext.SaveChangesAsync();
            return server;
        }

        /// <summary>
        /// Deletes a server by its ID.
        /// </summary>
        /// <param name="serverId">The ID of the server to delete.</param>
        public async Task DeleteServerAsync(int serverId)
        {
            using var dbContext = CreateDbContext();

            var server = dbContext.CompanyServers.Where(s => s.Id == serverId).ToList();
            
            if (server != null && server.Any())
            {
                if (server.Count > 1)
                {
                    //this should not be possible
                    _logger.Log($"Failed to remove server from EF. Multiple Servers exist with the same Id! Servers Retrieved: {JsonConvert.SerializeObject(server)}", "Unknown", "ServerManagerService");
                    throw new Exception($"Failed to remove server: There is more than 1 server with this Id.");
                }

                var planingareas = dbContext.PlanningAreas.Where(p => p.ServerId == serverId).ToList();
                if (planingareas != null && planingareas.Count > 0)
                    throw new Exception("Cannot delete, there are PlanningAreas associated with this server");
                try
                {
                    dbContext.CompanyServers.Remove(server.First());
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unknown", "ServerManagerService");
                    _logger.Log($"Failed to remove server from EF. Server: {JsonConvert.SerializeObject(server)}", "Unknown", "ServerManagerService");
                    throw new Exception($"Failed to remove server from EF.", e);
                }

                try
                {
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unknown", "ServerManagerService");
                    _logger.Log($"Failed to save changes to EF after removing server. Server: {JsonConvert.SerializeObject(server)}", "Unknown", "ServerManagerService");
                    throw new Exception($"Failed to save changes to EF after removing server.", e);
                }
            }
        }

        /// <summary>
        /// Reaches out to the Server Agent to get a list of local plannning areas, and imports them to the webapp
        /// This should not be used except for local dev testing, as Server Agent is not reachable this way when deployed
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        public static async Task<InstanceMigrationDto?> GetLegacyInstancesFromServerManager(CompanyServer server)
        {
            try
            {
                // Ensure the Server.Url has no trailing slash
                string baseUrl = server.Url.TrimEnd('/');
                string defaultBaseUrl = baseUrl;
                string publicBaseUrl = baseUrl.Replace("7980", "7981");

                // Initialize a single HttpClient instance
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };

                HttpClient httpClient = new HttpClient(handler)
                {
                    DefaultRequestVersion = HttpVersion.Version20,
                    DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower
                };

                httpClient.DefaultRequestHeaders.Add("Authorization", $"ServerManagerToken {server.AuthToken}");
                var response = await httpClient.GetAsync(Path.Join(defaultBaseUrl, "/api/WebApp/GetInstancesFromDb"));
                response.EnsureSuccessStatusCode();

                var instances = JsonConvert.DeserializeObject<InstanceMigrationDto>(await response.Content.ReadAsStringAsync());

                return instances;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
