using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Models.DTOs;

namespace ReportsWebApp.DB.Services.Interfaces
{
    public interface ISubscriptionService
    {
        /// <summary>
        /// Retrieves live subscriptions from the external application for a given serial code
        /// </summary>
        /// <param name="serialCode">The serial code to look up</param>
        /// <returns>A list of live subscription information</returns>
        Task<List<SubscriptionInfo>> GetSubscriptionsBySerialCodeAsync(string serialCode);

        /// <summary>
        /// Gets live subscription information for a company using their stored serial code
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <returns>List of live subscription information for the company</returns>
        Task<List<SubscriptionInfo>> GetLiveSubscriptionsForCompanyAsync(int companyId);
        
        Task<List<SubscriptionInfo>> GetLiveSubscriptionsForServerAsync(int serverId);

        /// <summary>
        /// Updates or creates the company's subscription configuration (serial code)
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <param name="serialCode">The serial code for retrieving subscriptions</param>
        /// <returns>The updated subscription configuration</returns>
        Task<CompanySubscriptionInfo> UpdateCompanySubscriptionInfoAsync(int companyId, string serialCode);

        /// <summary>
        /// Gets the company's subscription configuration (serial code, URLs, etc.)
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <returns>The company's subscription configuration</returns>
        Task<CompanySubscriptionInfo> GetCompanySubscriptionInfoAsync(int companyId);

        /// <summary>
        /// Removes the company's subscription configuration
        /// </summary>
        /// <param name="companyId">The company ID</param>
        /// <returns>True if successful</returns>
        Task<bool> RemoveCompanySubscriptionInfoAsync(int companyId);
    }
}