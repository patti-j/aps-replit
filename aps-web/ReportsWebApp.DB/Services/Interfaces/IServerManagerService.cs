using ReportsWebApp.DB.Models;
using System.Net.Http;

namespace ReportsWebApp.DB.Services.Interfaces;

public interface IServerManagerService
{
    /// <summary>
    /// Retrieves servers managed by a specific company. Optionally includes the companies that use those servers.
    /// </summary>
    /// <param name="managingCompanyId">The ID of the managing company.</param>
    /// <param name="includeUsingCompanies">Whether to include related using companies.</param>
    /// <returns>A list of servers.</returns>
    Task<List<CompanyServer>> GetServersByManagingCompanyAsync(int managingCompanyId, bool includeUsingCompanies = false);

    /// <summary>
    /// Retrieves details of a specific server by its ID.
    /// </summary>
    Task<CompanyServer> GetServerByIdAsync(int serverId);

    /// <summary>
    /// Adds or updates a server.
    /// </summary>
    Task<CompanyServer> AddOrUpdateServerAsync(CompanyServer server);

    /// <summary>
    /// Deletes a server by its ID.
    /// </summary>
    Task DeleteServerAsync(int serverId);
}