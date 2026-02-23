using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Services
{
    public class DBIntegrationService : IDBIntegrationService
    {
        private readonly IDbContextFactory<DbReportsContext> _dbContextFactory;
        private readonly ILogger<DBIntegrationService> _logger;
        private readonly IPlanningAreaDataService _planningAreaDataService;

        public DBIntegrationService(
            IDbContextFactory<DbReportsContext> dbContextFactory,
            ILogger<DBIntegrationService> logger,
            IPlanningAreaDataService planningAreaDataService)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
            _planningAreaDataService = planningAreaDataService;
        }

        /// <summary>
        /// Retrieves all planning areas for a given company with their current integration.
        /// </summary>
        public async Task<List<PADetails>> GetCompanyPlanningAreas(int companyId)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            return await _planningAreaDataService.GetPlanningAreasByUsingCompany(companyId);
        }

        /// <summary>
        /// Retrieves all DB integrations available to a specific company.
        /// </summary>
        public async Task<List<DBIntegration>> GetAvailableIntegrations(int companyId)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            return await db.DBIntegrations
                .Where(di => di.CompanyId == companyId)
                .OrderByDescending(di => di.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Applies a selected DBIntegration to a planning area.
        /// </summary>
        public async Task ApplyIntegrationToPlanningArea(int planningAreaId, int integrationId)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();

            var pa = await db.PlanningAreas.FindAsync(planningAreaId);
            if (pa == null)
                throw new InvalidOperationException($"PlanningArea {planningAreaId} not found.");

            var integration = await db.DBIntegrations.FindAsync(integrationId);
            if (integration == null)
                throw new InvalidOperationException($"Integration {integrationId} not found.");

            if (pa.DBIntegrationChangeInProgress ?? false) return;
            pa.DBIntegrationChangeInProgress = true;

            // TODO: Replace simulated logic with real integration processing.
            // This should include transferring DB objects based on integrationId,
            // likely using shared logic (or duplicated temporarily) from aps-api.
            // TODO: [Follow-up] Replace mock logic with actual transfer of DB objects as per integration definition.
            await Task.Delay(2000); // simulate integration delay

            // Apply the selected integration to the PA
            pa.DBIntegrationId = integration.Id;
            pa.DBIntegrationLastAppliedTime = DateTime.UtcNow;

            await db.SaveChangesAsync();
        }
    }
}
