using ReportsWebApp.DB.Models;
using Microsoft.EntityFrameworkCore;
using ReportsWebApp.DB.Data;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Text.Json.Serialization;
using System.Text.Json;
using ReportsWebApp.Common;
using Microsoft.Data.SqlClient;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Xml.Linq;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Dapper;
using Microsoft.EntityFrameworkCore.Storage;

using ReportsWebApp.DB.Services.Interfaces;
using ReportsWebApp.DB.Services.SchedulerHelpers;

public class PlanningAreaDataService : IPlanningAreaDataService
{
    private readonly IDbContextFactory<DbReportsContext> _dbContextFactory;
    private readonly IConfiguration _configuration;
    const string c_errorLogsTableName = "InstanceLogs";
    private readonly ICosmosDbService<GanttFavoriteData> _cosmosDbService;

    public PlanningAreaDataService(IDbContextFactory<DbReportsContext> dbContextFactory, IConfiguration configuration,  ICosmosDbService<GanttFavoriteData> cosmosDbService)
    {
        _dbContextFactory = dbContextFactory;
        _configuration = configuration;
        _cosmosDbService = cosmosDbService;
    }

    /// <summary>
    /// Selects a planning area and updates the favorite planning area for the specified user and company.
    /// </summary>
    /// <param name="planningArea">The planning area details to select.</param>
    /// <param name="companyId">The ID of the company.</param>
    /// <param name="userId">The ID of the user selecting the planning area.</param>
    public async Task SelectPlanningAreaAsync(PADetails planningArea, int companyId, int userId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var company = await dbContext.Companies.FirstOrDefaultAsync(c => c.Id == companyId);

        if (company != null)
        {
            var favoriteSettings = string.IsNullOrEmpty(company.FavoriteSettingPlanningAreaIds)
                ? new List<(int, int)>()
                : DeserializeFromCsv(company.FavoriteSettingPlanningAreaIds);

            var existingFavorite = favoriteSettings.FirstOrDefault(f => f.Item1 == userId);

            if (existingFavorite != default)
            {
                // Update existing favorite
                favoriteSettings.Remove(existingFavorite);
                favoriteSettings.Add((userId, planningArea.Id));
            }
            else
            {
                // Add new favorite
                favoriteSettings.Add((userId, planningArea.Id));
            }

            company.FavoriteSettingPlanningAreaIds = SerializeToCsv(favoriteSettings);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            throw new InvalidOperationException($"Company with ID {companyId} not found.");
        }
    }

    public async Task<DateTime?> GetPublishDate(int companyId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var companyDb = await dbContext.CompanyDbs
                                            .Where(x => x.CompanyId == companyId && x.DbType == EDbType.Analytical)
                                            .FirstOrDefaultAsync();
        if (companyDb == null) return null;
        var dbPassword = GetValueFromAzureKeyVault(companyDb.DBPasswordKey);
        companyDb.ConnectionString = $"Data Source={companyDb.DBServerName};Initial Catalog={companyDb.DBName};User ID={companyDb.DBUserName};Password={dbPassword};MultipleActiveResultSets=true;Encrypt=False";

        using (var connection = new SqlConnection(companyDb.ConnectionString))
        {
            var query = "SELECT MAX([PublishDate]) FROM [publish].[DASHt_HistoricalKPIs]";
            var publishDate = (await connection.QueryAsync<DateTime>(query)).FirstOrDefault();
            return publishDate;
        }
    }
    /// <summary>
    /// Retrieves scenarios for a given company ID and user ID.
    /// Scenarios are fetched from multiple databases associated with analytical data.
    /// </summary>
    /// <param name="companyId">The ID of the company.</param>
    /// <param name="userId">The ID of the user (not used in current logic, but included for future use).</param>
    /// <returns>A list of scenarios retrieved from the associated databases.</returns>
    public async Task<List<Scenario>> GetScenariosByCompanyIdAndUserIdAsync(int companyId, int userId)
    {
        // Create the DbContext
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var scenarios = new List<Scenario>();
        // Retrieve the associated databases for the company in Analytical mode
        var companyDbsList = await dbContext.CompanyDbs
            .Where(x => x.CompanyId == companyId && x.DbType == EDbType.Analytical)
            .ToListAsync();

        foreach (var companyDb in companyDbsList)
        {
            // Retrieve the database password from Azure Key Vault
            var dbPassword = GetValueFromAzureKeyVault(companyDb.DBPasswordKey);

            // Build the connection string (assuming it shouldn't be modified directly, but we construct it here for use)
            companyDb.ConnectionString = $"Data Source={companyDb.DBServerName};Initial Catalog={companyDb.DBName};User ID={companyDb.DBUserName};Password={dbPassword};MultipleActiveResultSets=true;Encrypt=False";

            try
            {
                // Retrieve scenarios from the external database
                using (var connection = new SqlConnection(companyDb.ConnectionString))
                {
                    await connection.OpenAsync();

                    var query = "SELECT DISTINCT [NewScenarioId], [ScenarioName], [PlanningAreaName] FROM [publish].[DASHt_Planning]";

                    // Execute the query and map results to ScenarioDto list
                    var scenarioDtos = (await connection.QueryAsync<ScenarioDto>(query)).ToList();

                    var items = 
                        (
                            await Task
                            .WhenAll(
                                 scenarioDtos.Select
                                        (
                                            (dto) => Task.Run(async () => 
                                            {
                                                try
                                                {
                                                    return (dto.NewScenarioId, (await _cosmosDbService.GetItemAsync(dto.NewScenarioId))?.Settings);
                                                }
                                                catch
                                                {
                                                    return (dto.NewScenarioId, null); //i dont expect this to happen but just in case
                                                }
                                            })
                                        )
                                    )
                        )
                        .Where(item => item.Item2 != null).ToList();
                    
                    // Map ScenarioDto to Scenario and add to the result
                    foreach (var scenarioDto in scenarioDtos)
                    {
                        scenarios.Add(new Scenario
                        {
                            Id = scenarioDto.NewScenarioId,
                            PlanningAreaName = scenarioDto.PlanningAreaName,
                            Environment = companyDb.Environment,
                            Name = scenarioDto.ScenarioName,
                            AnalyticalDb = companyDb,
                            BryntumSettings = items.FirstOrDefault(item => item.NewScenarioId == scenarioDto.NewScenarioId).Item2
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving scenarios: {ex.Message}");
            }
        }

        return scenarios;
    }

    /// <summary>
    /// Retrieves the selected planning area by company ID and user ID.
    /// If the favorite planning area is not found, throws an exception.
    /// </summary>
    /// <param name="companyId">The ID of the company.</param>
    /// <param name="userId">The ID of the user requesting the favorite planning area.</param>
    /// <returns>The selected planning area details.</returns>
    public async Task<PADetails> GetSelectedPlanningAreaByCompanyIdAndUserIdAsync(int companyId, int userId)
    {
        var favoritePlanningAreaId = await GetFavoritePlanningAreaIdByCompanyIdAndUserIdAsync(companyId, userId);

        // If no favorite planning area is found, select the first available one
        if (favoritePlanningAreaId == null)
        {
            // Get the first available planning area for the company
            var firstPlanningArea = await GetFirstAvailablePlanningAreaAsync(companyId);

            if (firstPlanningArea != null)
            {
                // Automatically set this as the favorite for the user
                await SelectPlanningAreaAsync(firstPlanningArea, companyId, userId);

                // Return the newly selected planning area
                return (firstPlanningArea);
            }
            else
            {
                throw new InvalidOperationException($"No Planning Areas found for CompanyId {companyId}");
            }
        }

        var planningArea = await GetPlanningAreaByIdAsync(favoritePlanningAreaId.Value);

        if (planningArea == null)
        {
            throw new InvalidOperationException($"No Planning Area found with ID {favoritePlanningAreaId}");
        }
        return planningArea;
    }

    /// <summary>
    /// Retrieves the first available planning area for the given company.
    /// </summary>
    /// <param name="companyId">The ID of the company.</param>
    /// <returns>The first available planning area or null if none exist.</returns>
    private async Task<PADetails> GetFirstAvailablePlanningAreaAsync(int companyId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.PlanningAreas
            .Where(p => p.CompanyId == companyId)
            .OrderBy(p => p.Id) // You can change the order criteria if necessary
            .FirstOrDefaultAsync();
    }

    // <summary>
    /// Checks if a given name and version combination is a duplicate, excluding a specific ID.
    /// </summary>
    /// <param name="a_paDetails"></param>
    /// <returns>True if a duplicate exists, otherwise false.</returns>
    public async Task<bool> IsPlanningAreaNameDuplicateAsync(PADetails a_paDetails)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        // Use FirstOrDefaultAsync to exit early when a match is found
        var duplicateExists = dbContext.PlanningAreas
            .Where(pa => a_paDetails.CompanyId == pa.CompanyId && 
                         a_paDetails.ServerId == pa.ServerId &&
                         (pa.Id != a_paDetails.Id && pa.Name == a_paDetails.Name && pa.Version == a_paDetails.Version))
            .FirstOrDefault();
        return duplicateExists != null;
    }
    
    public async Task<List<PADetails>> GetPlanningAreasByManagingCompanyIdAsync(int companyId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        // Show all PAs for this company, and any PAs hosted on this company's servers
        return await dbContext.PlanningAreas.AsNoTracking()
                                  .Where(x => x.BackupOf == null)
                                  .Include(p => p.Company)
                                  .Include(p => p.UsedByCompany)
                                  .Include(p => p.Tags)
                                  .Include(p => p.Location)
                                  .Include(p => p.Server)
                                  .ThenInclude(s => s.ManagingCompany)
                                  .Include(p => p.Server)
                                  .ThenInclude(s => s.UsingCompanies)
                                  .ThenInclude(s => s.Company)
                                  .Include(p => p.CFGroup).ThenInclude(p => p.CustomFields)
                                  .Where(p => p.CompanyId == companyId 
                                              || p.Server.ManagingCompanyId == companyId).OrderByDescending(x => x.UpdateDate)
                                  .ToListAsync();
    }

    /// <summary>
    /// Retrieves all planning areas belonging to a specific company ID.
    /// </summary>
    /// <param name="companyId">The ID of the company.</param>
    /// <returns>A list of planning area details.</returns>
    public async Task<List<PADetails>> GetPlanningAreasByCompanyIdAsync(int companyId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        // Show all PAs for this company, and any PAs hosted on this company's servers
        var pas =  await dbContext.PlanningAreas.AsNoTracking()
                                           .Where(x => x.BackupOf == null)
                                           .Include(p => p.Company)
                                           .Include(p => p.UsedByCompany)
                                           .Include(p => p.Tags)
                                           .Include(p => p.Location)
                                           .Include(p => p.Server)
                                           .ThenInclude(s => s.ManagingCompany)
                                           .Include(p => p.Server)
                                           .ThenInclude(s => s.UsingCompanies)
                                           .ThenInclude(s => s.Company)
                                           .Include(p => p.CFGroup).ThenInclude(p => p.CustomFields)
                                           .Where(p => p.CompanyId == companyId 
                                                       || p.Server.ManagingCompanyId == companyId 
                                                       || p.Server.UsingCompanies.Select(x => x.CompanyId).Contains(companyId)).OrderByDescending(x => x.UpdateDate)
                                           .ToListAsync();
        
        pas.RemoveAll(c =>
        {
            if (c.Server.UsingCompanies == null || c.Server.UsingCompanies.Count == 0)
            {
                //if no using companies then managing company is a using company
            }
            else
            {
                //if we are the managing company and also not in the using companies list
                if (!c.Server.UsingCompanies.Select(x => x.CompanyId).Contains(companyId))
                {
                    return true;
                }
            }

            return false;
        });
        
        return pas;
    }

    /// <summary>
    /// Retrieves all planning areas that are used by a specific company ID.
    /// </summary>
    /// <param name="companyId">The ID of the company.</param>
    /// <returns>A list of planning area details.</returns>
    public async Task<List<PADetails>> GetPlanningAreasByUsingCompany(int companyId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        // Show all PAs for this company, and any PAs hosted on this company's servers
        var pas = await dbContext.PlanningAreas.AsNoTracking()
                              .Where(x => x.BackupOf == null)
                              .Include(p => p.Company)
                              .Include(p => p.UsedByCompany)
                              .Include(p => p.Tags)
                              .Include(p => p.Server)
                              .ThenInclude(s => s.ManagingCompany)
                              .Include(p => p.Server)
                              .ThenInclude(s => s.UsingCompanies)
                              .ThenInclude(s => s.Company)
                              .Include(p => p.CFGroup).ThenInclude(p => p.CustomFields)
                              .Where(p => p.CompanyId == companyId 
                                          || p.Server.ManagingCompanyId == companyId 
                                          || p.Server.UsingCompanies.Select(x => x.CompanyId).Contains(companyId))
                              .OrderByDescending(x => x.UpdateDate)
                              .ToListAsync();
        pas.RemoveAll(c =>
        {
            if (c.Server.UsingCompanies == null || c.Server.UsingCompanies.Count == 0)
            {
                //if no using companies then managing company is a using company
            }
            else
            {
                //if we are the managing company and also not in the using companies list
                if (!c.Server.UsingCompanies.Select(x => x.CompanyId).Contains(companyId))
                {
                    return true;
                }
            }

            return false;
        });
        
        return pas;
    }

    public async Task<List<PADetails>> GetAllBackupsForCompany(int companyId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var planningAreas = await dbContext.PlanningAreas.AsNoTracking()
                                           .Where(x => x.CompanyId == companyId && x.BackupOf != null)
                                           .Include(p => p.Company)
                                           .Include(p => p.UsedByCompany)
                                           .Include(p => p.Tags)
                                           .Include(p => p.Server)
                                           .ThenInclude(s => s.ManagingCompany)
                                           .Include(p => p.Server)
                                           .ThenInclude(s => s.UsingCompanies)
                                           .ThenInclude(s => s.Company)
                                           .Include(p => p.CFGroup).ThenInclude(p => p.CustomFields)
                                           .ToListAsync();

        return planningAreas.OrderByDescending(x => x.UpdateDate).ToList();
    }

    public async Task<List<PADetails>> GetBackups(PADetails planningArea)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var planningAreas = await dbContext.PlanningAreas.AsNoTracking()
           .Where(x => x.BackupOf == planningArea.Id)
           .Include(p => p.Company)
           .Include(p => p.UsedByCompany)
           .Include(p => p.Tags)
           .Include(p => p.Server)
           .ThenInclude(s => s.ManagingCompany)
           .Include(p => p.Server)
           .ThenInclude(s => s.UsingCompanies)
           .ThenInclude(s => s.Company)
           .Include(p => p.CFGroup).ThenInclude(p => p.CustomFields)
           .ToListAsync();

        return planningAreas.OrderByDescending(x => x.UpdateDate).ToList();
    }

    public async Task<PlanningAreaLiteModel> GetPlanningAreaStatus(int planningAreaId)
    {
        await using DbReportsContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        var planningArea = await dbContext.PlanningAreas.Include(x => x.Server).AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == planningAreaId);

        if (planningArea == null)
        {
            return null;
        }

        return new PlanningAreaLiteModel(planningArea);
    }

    public async Task<bool> WaitForPAStatus(PADetails pa, EServiceState state, int timeout = 30000, int interval = 500)
    {
        var time = DateTime.UtcNow;
        while (time.AddMilliseconds(timeout) > DateTime.UtcNow)
        {
            // Create dbContext new each cycle so that it will retrieve updated data
            await using DbReportsContext dbContext = await _dbContextFactory.CreateDbContextAsync();
            var dbPa = dbContext.PlanningAreas.FirstOrDefault(x => x.CompanyId == pa.CompanyId && x.Name == pa.Name && x.Version == pa.Version);
            if (dbPa != null && dbPa.ServiceState == state)
            {
                return true;
            }

            await Task.Delay(interval);
        }
        return false;
    }

    public async Task<List<PlanningAreaLiteModel>> GetPlanningAreaStatusesForServerAsync(int serverId)
    {
        await using DbReportsContext dbContext = await _dbContextFactory.CreateDbContextAsync();
        List<PADetails> planningAreas = await dbContext.PlanningAreas.Include(x => x.Server).AsNoTracking()
            .Where(p => p.BackupOf == null && p.ServerId == serverId)
            .ToListAsync();

        return planningAreas.Select(pa => new PlanningAreaLiteModel(pa)).ToList();
    }

    public async Task<List<PADetails>> GetPlanningAreasForUserAsync(User user)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var authorizedPas = await dbContext.PlanningAreaAccesses.Include(x => x.PlanningArea).Where(x => x.UserId == user.Id).ToListAsync();
        var authorizedIds = authorizedPas.Select(x => x.PlanningArea.PlanningAreaKey);
        var planningAreas = dbContext.PlanningAreas.Include(x => x.Server)
                                     .ThenInclude(x => x.UsingCompanies).Where(x => x.BackupOf == null && authorizedIds.Contains(x.PlanningAreaKey)).ToList();
        // PAs with the same key are technically the same PA, so only one should be selected from each key group 
        var paGroups = planningAreas.GroupBy(x => x.PlanningAreaKey);
        // If any PAs in a grouping are started, select the latest version that is started. If none are started, just show the latest
        var paList = paGroups.Select(x => x.Any(y => y.IsStarted) ? x.Where(y => y.IsStarted).MaxBy(y => Version.Parse(y.Version)) : x.MaxBy(y => Version.Parse(y.Version))).ToList();
        
        paList.RemoveAll(c =>
        {
            if (c.Server.UsingCompanies == null || c.Server.UsingCompanies.Count == 0)
            {
                if (c.Server.ManagingCompanyId != user.CompanyId)
                {
                    return true;
                }
            }
            else
            {
                if (!c.Server.UsingCompanies.Select(x => x.CompanyId).Contains(user.CompanyId))
                {
                    return true;
                }
            }

            return false;
        });
        
        return paList.ToList();
    }

    /// <summary>
    /// Saves the planning area details. If the area already exists, it updates the existing one; otherwise, it creates a new one.
    /// </summary>
    /// <param name="planningAreaDetails">The planning area details to save.</param>
    public async Task SaveAsync(PADetails planningAreaDetails)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        UpdatePlanningAreaJSON(planningAreaDetails); // Ensure the settings JSON is updated

        // Prevent EF from adding duplicate tags
        planningAreaDetails.Tags = null;
        planningAreaDetails.ServerId = planningAreaDetails.Server.Id;
        planningAreaDetails.Server = null;

        if (planningAreaDetails.Id > 0)
        {
            dbContext.PlanningAreas.Update(planningAreaDetails);
        }
        else
        {
            dbContext.PlanningAreas.Add(planningAreaDetails);
        }

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Saves the planning area details, deleting any existing pa with the same identifier. This is used for import/migration from server manager
    /// </summary>
    /// <param name="planningAreaDetails">The planning area details to save.</param>
    public async Task OverwriteAsync(PADetails planningAreaDetails)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var existingPa = dbContext.PlanningAreas.FirstOrDefault(x => x.PlanningAreaKey == planningAreaDetails.PlanningAreaKey && x.Version == planningAreaDetails.Version);
        if (existingPa != null)
        {
            await HardDeletePlanningAreaAsync(existingPa);
        }

        // Prevent EF from adding duplicate tags
        planningAreaDetails.Tags = null;
        planningAreaDetails.Server = null;

        if (planningAreaDetails.Id > 0)
        {
            dbContext.PlanningAreas.Update(planningAreaDetails);
        }
        else
        {
            dbContext.PlanningAreas.Add(planningAreaDetails);
        }

        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes the specified planning and all related information, including backups, from the database.
    /// </summary>
    /// <param name="planningAreaDetails">The planning area details to delete.</param>
    public async Task HardDeletePlanningAreaAsync(PADetails planningAreaDetails)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        // Remove tag maps
        var tagSet = dbContext.Set<PlanningAreaPATag>("PlanningAreaPATag");
        var tagMaps = await tagSet.Where(p => p.PlanningAreaId == planningAreaDetails.Id).ToListAsync();
        tagSet.RemoveRange(tagMaps);

        // Remove backups
        var existingPAs = dbContext.PlanningAreas.Where(x => x.PlanningAreaKey == planningAreaDetails.PlanningAreaKey && x.BackupOf == planningAreaDetails.Id);
        dbContext.PlanningAreas.RemoveRange(existingPAs);

        // Remove the PA
        dbContext.PlanningAreas.Remove(planningAreaDetails);
        await dbContext.SaveChangesAsync();
    }

    public async Task<bool> DeleteBackupAsync(PADetails planningAreaDetails)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var existingPA = dbContext.PlanningAreas.FirstOrDefault(x => x.PlanningAreaKey == planningAreaDetails.PlanningAreaKey && x.BackupOf == null);

        // Ensure the PA exists in the DB and is marked as a backup
        if (existingPA == null || existingPA.BackupOf != null)
        {
            return false;
        }

        var tagSet = dbContext.Set<PlanningAreaPATag>("PlanningAreaPATag");
        var tagMaps = await tagSet.Where(p => p.PlanningAreaId == existingPA.Id).ToListAsync();
        tagSet.RemoveRange(tagMaps);
        dbContext.PlanningAreas.Remove(existingPA);
        await dbContext.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Deletes the specified planning area.
    /// </summary>
    /// <param name="planningAreaDetails">The planning area details to delete.</param>
    public async Task DeletePlanningAreaAsync(PADetails planningAreaDetails)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        // Remove tag maps
        var tagSet = dbContext.Set<PlanningAreaPATag>("PlanningAreaPATag");
        var tagMaps = await tagSet.Where(p => p.PlanningAreaId == planningAreaDetails.Id).ToListAsync();
        tagSet.RemoveRange(tagMaps);

        // Remove tag maps
        var accessSet = dbContext.Set<PlanningAreaAccess>();
        var accessMaps = await accessSet.Where(p => p.PlanningAreaId == planningAreaDetails.Id).ToListAsync();
        accessSet.RemoveRange(accessMaps);

        var pa = dbContext.PlanningAreas.FirstOrDefault(x => x.PlanningAreaKey == planningAreaDetails.PlanningAreaKey && x.Version == planningAreaDetails.Version);
        dbContext.PlanningAreas.Remove(pa);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<PlanningAreaTag>> GetGroupsForCompanyAsync(int companyId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.PlanningAreaTags.Where(x => x.CompanyId == companyId).Include(x => x.Company).Include(x => x.PlanningAreas).ToListAsync();
    }

    /// <summary>
    /// Saves the planning area tag. If the area already exists, it updates the existing one; otherwise, it creates a new one.
    /// </summary>
    /// <param name="tag">The planning area tag to save.</param>
    public async Task<PlanningAreaTag> AddOrUpdateTagAsync(PlanningAreaTag tag)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var set = dbContext.Set<PlanningAreaPATag>("PlanningAreaPATag");
        var pas = tag.PlanningAreas;
        tag.PlanningAreas = null;

        if (tag.Id == 0)
        {
            dbContext.PlanningAreaTags.Add(tag);
            await dbContext.SaveChangesAsync();

            set.AddRange(pas.Select(x => new PlanningAreaPATag
            {
                PAGroupId = tag.Id,
                PlanningAreaId = x.Id
            }));
            

            await dbContext.SaveChangesAsync();
            return dbContext.PlanningAreaTags.Include(x => x.PlanningAreas).First(x => x.Id == tag.Id);
        } else
        {
            var existingGroup = dbContext.PlanningAreaTags.FirstOrDefault(p => p.Id == tag.Id);

            if (existingGroup != null)
            {
                existingGroup.Name = tag.Name;

                var existingRelations = set.Where(x => x.PAGroupId == tag.Id).ToList();
                var diffPa = existingRelations.Synchronize(pas.Select(x => new PlanningAreaPATag
                    {
                        PAGroupId = tag.Id,
                        PlanningAreaId = x.Id
                    }),
                    // Compare based on both ids
                    (x, y) => x.PAGroupId == y.PAGroupId && x.PlanningAreaId == y.PlanningAreaId);

                set.AddRange(diffPa.added);
                set.RemoveRange(diffPa.removed);
            }

            await dbContext.SaveChangesAsync();
            PlanningAreaTag ret = dbContext.PlanningAreaTags.Include(x => x.PlanningAreas).First(x => x.Id == existingGroup.Id);
            return ret;
        }
    }

    public async Task UpdateStatusAsync(PADetails model, string status)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var pa = dbContext.PlanningAreas.FirstOrDefault(x => x.Id == model.Id);
        if (pa == null)
        {
            // Already deleted
            throw new DataException("Planning Area not found");
        }
        pa.RegistrationStatus = status;
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes the specified planning area tag.
    /// </summary>
    /// <param name="tag">The planning area tag to delete.</param>
    public async Task DeleteTagAsync(PlanningAreaTag tag)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        
        var set = dbContext.Set<PlanningAreaPATag>("PlanningAreaPATag");
        set.RemoveRange(set.Where(x => x.PAGroupId == tag.Id));
        
        dbContext.PlanningAreaTags.Remove(tag);
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Updates the planning area by serializing its settings.
    /// </summary>
    /// <param name="pa">The planning area details.</param>
    /// <returns>The updated planning area details.</returns>
    public PADetails UpdatePlanningAreaJSON(PADetails pa)
    {
        var options = new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        pa.Settings = System.Text.Json.JsonSerializer.Serialize(pa.PlanningArea, options);
        if (string.IsNullOrEmpty(pa.Settings))
        {
            throw new InvalidOperationException("Failed to serialize PADetails.Settings");
        }

        return pa;
    }

    /// <summary>
    /// Retrieves the favorite planning area ID for a specific company and user from the database.
    /// </summary>
    /// <param name="companyId">The company ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <returns>The favorite planning area ID or null if not set.</returns>
    private async Task<int?> GetFavoritePlanningAreaIdByCompanyIdAndUserIdAsync(int companyId, int userId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var company = await dbContext.Companies
            .Where(c => c.Id == companyId)
            .Select(c => c.FavoriteSettingPlanningAreaIds)
            .FirstOrDefaultAsync();

        var favoriteSettings = string.IsNullOrEmpty(company) ? new List<(int, int)>() : DeserializeFromCsv(company);
        var favorite = favoriteSettings.FirstOrDefault(f => f.Item1 == userId);
        return favorite != default ? (int?)favorite.Item2 : null;
    }

    /// <summary>
    /// Retrieves the planning area details by its ID from the database.
    /// </summary>
    /// <param name="planningAreaId">The planning area ID.</param>
    /// <returns>The planning area details.</returns>
    public async Task<PADetails> GetPlanningAreaByIdAsync(int planningAreaId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.PlanningAreas
            .Include(pa => pa.Server)
            .ThenInclude(x => x.ManagingCompany)
            .Include(x => x.Server)
            .ThenInclude(x => x.UsingCompanies)
            .ThenInclude(x => x.Company)
            .FirstOrDefaultAsync(pa => pa.Id == planningAreaId);
    }

    public async Task RestoreBackup(PADetails current, PADetails toRestore)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var currentDbPa = dbContext.PlanningAreas.FirstOrDefault(x => x.CompanyId == current.CompanyId && x.PlanningAreaKey == current.PlanningAreaKey && x.Version == current.Version);
        var toRestoreDbPa = dbContext.PlanningAreas.FirstOrDefault(x => x.CompanyId == toRestore.CompanyId && x.PlanningAreaKey == toRestore.PlanningAreaKey && x.Version == toRestore.Version);

        if (currentDbPa == null || toRestoreDbPa == null)
        {
            throw new DataException("Couldn't find the Planning Area to restore");
        }

        currentDbPa.BackupOf = toRestoreDbPa.Id;
        currentDbPa.UpdateDate = DateTime.UtcNow;
        currentDbPa.ServiceState = EServiceState.Restoring;
        toRestoreDbPa.BackupOf = null;
        toRestoreDbPa.UpdateDate = DateTime.UtcNow;
        currentDbPa.ServiceState = EServiceState.Restoring;

        // Ensure all existing backups point at the active PA
        var backups = await GetBackups(currentDbPa);
        // Remove the PAs we already updated
        backups.RemoveAll(x => x.Id == currentDbPa.Id || x.Id == toRestoreDbPa.Id);
        foreach (var backup in backups)
        {
            backup.BackupOf = toRestoreDbPa.Id;
            backup.Company = null;
            backup.UsedByCompany = null;
            backup.Server = null;
            backup.ExternalIntegration = null;
            dbContext.Update(backup);
        }

        dbContext.Update(currentDbPa);
        dbContext.Update(toRestoreDbPa);
        await dbContext.SaveChangesAsync();
    }

    public async Task<List<PlanningAreaLocation>> GetFolders(int serverId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        return await dbContext.PlanningAreaLocations.Include(x => x.PlanningAreas)
                              .Include(x => x.PlanningAreas)
                              .Include(x => x.Children)
                              .Where(x => x.ServerId == serverId).ToListAsync();
    }

    public async Task CreateRootFolder(User editor, CompanyServer server, string name)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var existingFolder = dbContext.PlanningAreaLocations.Where(x => x.ParentId == null && x.Name == name);

        if (existingFolder.Any())
        {
            throw new DataException("Folder Already Exists");
        }

        var folder = new PlanningAreaLocation()
        {
            Environment = "",
            CreatedBy = editor.Email,
            CreationDate = DateTime.UtcNow,
            Name = name,
            ParentId = null,
            ServerId = server.Id
        };

        await dbContext.PlanningAreaLocations.AddAsync(folder);
        await dbContext.SaveChangesAsync();
    }

    public async Task CreateFolder(User editor, PlanningAreaLocation parent, string name)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var existingFolder = dbContext.PlanningAreaLocations.Where(x => x.ParentId == parent.Id && x.Name == name);

        if (existingFolder.Any())
        {
            throw new DataException("Folder Already Exists");
        }

        var folder = new PlanningAreaLocation()
        {
            Environment = parent.Environment,
            CreatedBy = editor.Email,
            CreationDate = DateTime.UtcNow,
            Name = name,
            ParentId = parent.Id,
            ServerId = parent.ServerId
        };

        await dbContext.PlanningAreaLocations.AddAsync(folder);
        await dbContext.SaveChangesAsync();
    }

    public async Task RenameFolder(User editor, PlanningAreaLocation a_location, string name)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var dbFolder = dbContext.PlanningAreaLocations.SingleOrDefault(x => x.Id == a_location.Id);
        var existingFolder = dbContext.PlanningAreaLocations.Where(x => x.ParentId == a_location.Id && x.Name == name);

        if (existingFolder.Any())
        {
            throw new DataException("Folder Already Exists");
        }

        dbFolder.Name = name;

        dbContext.PlanningAreaLocations.Update(dbFolder);
        await dbContext.SaveChangesAsync();
    }

    public async Task MoveFolder(User editor, PlanningAreaLocation a_location, PlanningAreaLocation? newParent)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        var dbFolder = dbContext.PlanningAreaLocations.SingleOrDefault(x => x.Id == a_location.Id);

        dbFolder.PlanningAreas = null;
        dbFolder.Parent = null;
        dbFolder.Children = null;

        dbFolder.ParentId = newParent?.Id;

        dbContext.Update(dbFolder);
        await dbContext.SaveChangesAsync();
    }

    public async Task MoveFolder(User editor, PADetails pa, PlanningAreaLocation? newParent)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        var dbPA = dbContext.PlanningAreas.SingleOrDefault(x => x.Id == pa.Id);
        dbPA.LocationId = newParent?.Id;

        dbContext.Update(dbPA);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteFolder(User editor, PlanningAreaLocation a_location)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        // Ensure Folder is empty. Recursive delete should be handled in the Component Code
        var pas = dbContext.PlanningAreas.Where(x => x.LocationId == a_location.Id);

        if (pas.Any())
        {
            throw new DataException("Folder must have no PAs before it can be deleted");
        }

        // The delete action will already delete child PAs and Folders, so we don't need to recursively delete here

        a_location.PlanningAreas = null;
        a_location.Parent = null;
        a_location.Children = null;

        dbContext.Remove(a_location);
        await dbContext.SaveChangesAsync();
    }

    public async Task<Dictionary<string, int>?> GetTotalErrors(PADetails pa, int a_retryCount)
    {
        Dictionary<string, int> errors = new ();
        var connectionString = pa.PlanningArea?.Settings.SystemServiceSettings.LogDbConnectionString;

        a_retryCount = Math.Max(0, a_retryCount);
        try
        {
            await using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                while (a_retryCount >= 0)
                {
                    try
                    {
                        await using (SqlCommand sqlCmd = connection.CreateCommand())
                        {

                            sqlCmd.CommandText = $"SELECT LogType, COUNT(*) FROM {c_errorLogsTableName} WHERE InstanceName = @InstanceName AND SoftwareVersion = @SoftwareVersion GROUP BY LogType\r\n";
                            sqlCmd.Parameters.AddWithValue("@InstanceName", pa.Name);
                            sqlCmd.Parameters.AddWithValue("@SoftwareVersion", pa.Version);
                            var reader = await sqlCmd.ExecuteReaderAsync();

                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    if (!reader.IsDBNull(0) && !reader.IsDBNull(1))
                                    {
                                        errors[reader.GetString(0)] = reader.GetInt32(1);
                                    }
                                }

                                return errors;
                            }
                        }

                        break;
                    }
                    catch (SqlException e) when (e.Number == -2 || e.Number == 1205 || e.Number == 11)
                    {
                        a_retryCount--;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return null;
    }

    public record ErrorRow(string InstanceName, string SoftwareVersion, string TypeName, string Message, string StackTrace, string Source, string InnerExceptionMessage, string InnerExceptionStackTrace, string LogType, string HeaderMessage, DateTime Timestamp);

    public async Task<List<ErrorRow>> GetLogs(PADetails pa, string category, int start, int amt, int a_retryCount)
    {
        int affectedRows = 0;
        var connectionString = pa.PlanningArea?.Settings.SystemServiceSettings.LogDbConnectionString;

        a_retryCount = Math.Max(0, a_retryCount);
        try
        {
            await using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                while (a_retryCount >= 0)
                {
                    try
                    {
                        await using (SqlCommand sqlCmd = connection.CreateCommand())
                        {

                            sqlCmd.CommandText = $"SELECT * FROM {c_errorLogsTableName} WHERE InstanceName = @InstanceName AND SoftwareVersion = @SoftwareVersion AND LogType = @LogType ORDER BY [Timestamp] DESC OFFSET @Start ROWS FETCH NEXT @Page ROWS ONLY";
                            sqlCmd.Parameters.AddWithValue("@InstanceName", pa.Name);
                            sqlCmd.Parameters.AddWithValue("@SoftwareVersion", pa.Version);
                            sqlCmd.Parameters.AddWithValue("@LogType", category);
                            sqlCmd.Parameters.AddWithValue("@Start", start);
                            sqlCmd.Parameters.AddWithValue("@Page", amt);
                            var reader = await sqlCmd.ExecuteReaderAsync();

                            var errors = new List<ErrorRow>();
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    errors.Add(new ErrorRow(reader.IsDBNull(0) ? null : reader.GetString(0),
                                        reader.IsDBNull(1) ? null : reader.GetString(1),
                                        reader.IsDBNull(2) ? null : reader.GetString(2),
                                        reader.IsDBNull(3) ? null : reader.GetString(3),
                                        reader.IsDBNull(4) ? null : reader.GetString(4),
                                        reader.IsDBNull(5) ? null : reader.GetString(5),
                                        reader.IsDBNull(6) ? null : reader.GetString(6),
                                        reader.IsDBNull(7) ? null : reader.GetString(7),
                                        reader.IsDBNull(8) ? null : reader.GetString(8),
                                        reader.IsDBNull(9) ? null : reader.GetString(9),
                                        reader.GetDateTime(10)));
                                }
                            }

                            return errors.ToList();
                        }

                        break;
                    }
                    catch (SqlException e) when (e.Number == -2 || e.Number == 1205 || e.Number == 11)
                    {
                        a_retryCount--;
                    }
                }
            }
        }
        catch (Exception e) 
        {
            Console.WriteLine(e);
        }

        return null;
    }

    /// <summary>
    /// Serializes a list of tuples to a CSV string.
    /// </summary>
    /// <param name="list">The list of tuples to serialize.</param>
    /// <returns>A CSV string representing the list.</returns>
    private string SerializeToCsv(List<(int, int)> list)
    {
        return string.Join(";", list.Select(item => $"{item.Item1},{item.Item2}"));
    }

    /// <summary>
    /// Deserializes a CSV string to a list of tuples.
    /// </summary>
    /// <param name="csv">The CSV string to deserialize.</param>
    /// <returns>A list of tuples.</returns>
    private List<(int, int)> DeserializeFromCsv(string csv)
    {
        return csv.Split(';')
                  .Select(pair => pair.Split(','))
                  .Select(parts => (int.Parse(parts[0]), int.Parse(parts[1])))
                  .ToList();
    }

    private string GetValueFromAzureKeyVault(string key)
    {
        string kvURL = _configuration["KeyValultUrl"];
        string tenentId = _configuration["TenantId"];
        string clientId = _configuration["ClientId"];
        string clientSecret = _configuration["ClientSecret"];

        var credential = new ClientSecretCredential(tenentId, clientId, clientSecret);

        var client = new SecretClient(new Uri(kvURL), credential);
        KeyVaultSecret secret = client.GetSecret(key);
        return secret.Value;
    }
}
