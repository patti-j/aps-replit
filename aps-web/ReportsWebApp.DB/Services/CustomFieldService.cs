using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.Common;
using ReportsWebApp.DB.Services;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Internal;

public class CustomFieldService : ICustomFieldService
{
    private readonly IDbContextFactory<DbReportsContext> _factory;
    private readonly IPlanningAreaDataService _paDataService;

    public CustomFieldService(IDbContextFactory<DbReportsContext> factory, IPlanningAreaDataService permissionService)
    {
        _factory = factory;
        _paDataService = permissionService;
    }

    private DbReportsContext GetDbContext()
    {
        // Resolve a new scoped DbContext using the IServiceProvider
        return _factory.CreateDbContext();
    }
    public async Task<List<Company>> GetCompaniesAsync()
    {
        using var dbContext = GetDbContext();
        // Use Entity Framework Core or other data access methods to retrieve categories
        return await dbContext.Companies.Include(c => c.Workspaces).ToListAsync();
    }

    public async Task<CFGroup> CreateOrUpdateCFGroupAsync(CFGroup group)
    {
        using var dbContext = GetDbContext();

        var cfs = group.CustomFields;
        group.CustomFields = null;
        
        // Check for duplicate Name/Object
        if (cfs.Any(x => cfs.Any(y => { 
            if (y.Id != x.Id && y.Name == x.Name && y.Object == x.Object)
            {
                throw new InvalidDataException($"You cannot assign two Custom Fields with the same name and object to a Planning Area. {x.Name}, {y.Name} on {x.Object}");
            }
            else
            {
                return false;
            }
        })))
        {
        }

        if (group.Id == 0)
        {
            dbContext.CFGroups.Add(group);

            await dbContext.SaveChangesAsync();

            var set = dbContext.Set<CustomFieldCFGroup>("CustomFieldCFGroup");
            set.AddRange(cfs.Select(x => new CustomFieldCFGroup { CustomFieldId = x.Id, CFGroupId = group.Id }));
        } else
        {
            var existingGroup = dbContext.CFGroups.AsNoTracking().Include(x => x.CustomFields).AsNoTracking().FirstOrDefault(x => x.Id == group.Id);
            existingGroup.Name = group.Name;
            await dbContext.SaveChangesAsync();

            var set = dbContext.Set<CustomFieldCFGroup>("CustomFieldCFGroup");
            var diff = existingGroup.CustomFields.Synchronize(cfs);
            
            set.AddRange(diff.added.Select(x => new CustomFieldCFGroup { CustomFieldId = x.Id, CFGroupId = group.Id }));

            var mappingObjects = set.ToList();

            set.RemoveRange(diff.removed.Select(x => mappingObjects.First(y => y.CustomFieldId == x.Id && y.CFGroupId == group.Id)));
        }
        await dbContext.SaveChangesAsync();
        return dbContext.CFGroups.Include(x => x.CustomFields).First(x => x.Id == group.Id);
    }

    public async Task<List<CustomField>> GetCFsForCompanyAsync(int companyId)
    {
        await using var dbContext = GetDbContext();
        return await dbContext.CustomFields.Where(x => x.CompanyId == companyId).Include(x => x.Company).ToListAsync();
    }

    public async Task<CustomField> AddOrUpdateCFAsync(CustomField cf)
    {
        await using var dbContext = GetDbContext();
        var set = dbContext.Set<PlanningAreaPATag>("PlanningAreaPAGroup");

        if (dbContext.CustomFields.Any(x => x.CompanyId == cf.CompanyId && x.ExternalId == cf.ExternalId && x.Id != cf.Id))
        {
            throw new InvalidDataException("Duplicate External IDs are not allowed.");
        }

        if (cf.Id == 0)
        {
            dbContext.CustomFields.Add(cf);
            await dbContext.SaveChangesAsync();
            return cf;
        }
        else
        {
            var existingCF = dbContext.CustomFields.FirstOrDefault(p => p.Id == cf.Id);

            if (existingCF != null)
            {
                existingCF.Name = cf.Name;
                existingCF.ExternalId = cf.ExternalId;
                existingCF.Type = cf.Type;
                existingCF.Object = cf.Object;
                existingCF.ShowInGantt = cf.ShowInGantt;
                existingCF.ShowInGrids = cf.ShowInGrids;
                existingCF.CanPublish = cf.CanPublish;
            }

            await dbContext.SaveChangesAsync();
            return existingCF;
        }

    }

    public async Task DeleteCFAsync(CustomField cf)
    {
        await using var dbContext = GetDbContext();

        var set = dbContext.Set<CustomFieldCFGroup>("CustomFieldCFGroup");
        var cfMaps = set.Where(x => x.CustomFieldId == cf.Id);
        set.RemoveRange(cfMaps);

        dbContext.CustomFields.Remove(cf);
        await dbContext.SaveChangesAsync();
    }

}
