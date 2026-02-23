using Microsoft.EntityFrameworkCore;

using ReportsWebApp.Common;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;

public class CategoryService : ICategoryService
{
    private readonly IDbContextFactory<DbReportsContext> _factory;

    public CategoryService(IDbContextFactory<DbReportsContext> factory)
    {
        _factory = factory;
    }

    public  List<Category> GetCategories(int CompanyId)
    {
        using var _dbContext = _factory.CreateDbContext();
        return _dbContext.Categories.Where(c => c.CompanyId == CompanyId)
                    .Include(c => c.Roles)
                    .Include(c => c.Reports).ThenInclude(r => r.PBIWorkspace)
                     .ToList();
    }

    // Get categories ignoring the EFCore cache
    public List<Category> GetCategoriesForNav(int CompanyId, List<Role> groups)
    {
        using var _dbContext = _factory.CreateDbContext();
        var categoryIds = groups.SelectMany(x => x.Categories).Select(x => x.Id);
        return _dbContext.Categories.AsNoTracking().Where(c => c.CompanyId == CompanyId && categoryIds.Contains(c.Id))
                    .Include(c => c.Roles)
                    .Include(c => c.Reports).ThenInclude(r => r.PBIWorkspace)
                     .ToList();
    }

    public List<Category> GetCategoriesForGroups(int CompanyId, List<Role> groups)
    {
        using var _dbContext = _factory.CreateDbContext();
        var categoryIds = groups.SelectMany(x => x.Categories).Select(x => x.Id);
        return _dbContext.Categories.Where(c => c.CompanyId == CompanyId && categoryIds.Contains(c.Id))
                       .Include(c => c.Roles)
                       .Include(c => c.Reports).ThenInclude(r => r.PBIWorkspace)
                       .ToList();
    }

    public Category GetCategory(int categoryId)
    {
        using var _dbContext = _factory.CreateDbContext();
        return _dbContext.Categories.Include(x => x.Reports).FirstOrDefault(c=>c.Id == categoryId) ?? throw new Exception("Report Category not found.");
    }

    public async Task<Category> SaveAsync(Category category)
    {
        using var _dbContext = _factory.CreateDbContext();
        if (category == null)
        {
            throw new ArgumentNullException(nameof(category));
        }

        var groupIds = category.Roles.Select(x => x.Id);

        if (category.Id == 0)
        {
            // This is a new category; add it to the context
            _dbContext.Categories.Add(category);
        }
        else
        {
            var groupCatSet = _dbContext.Set<RoleCategory>("RoleCategory");
            var existingGroups = groupCatSet.Where(x => x.CategoriesId == category.Id).ToList();

            var diff = existingGroups.Synchronize(category.Roles.Select(x => new RoleCategory() {CategoriesId = category.Id, RolesId = x.Id}));
            groupCatSet.RemoveRange(diff.removed);
            groupCatSet.AddRange(diff.added);
            category.Roles = null;

            // This is an existing category; update it in the context
            _dbContext.Categories.Update(category);
        }

        await _dbContext.SaveChangesAsync();
        var newGroups = _dbContext.Roles.Where(x => groupIds.Contains(x.Id)).ToList();
        category.Roles = newGroups;
        return category;
    }

    public async Task<bool> DeleteAsync(int categoryId)
    {
        using var _dbContext = _factory.CreateDbContext();
        var category = await _dbContext.Categories.Where(c => c.Id == categoryId).Include(r => r.Reports)
                                       .Include(g => g.Roles).FirstOrDefaultAsync();
        //var category = await _dbContext.Categories.FindAsync(categoryId);
        category.Roles.Clear();
        category.Reports.Clear();
        if (category == null)
        {
            return false; // Category not found
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();
        return true; // Category successfully deleted
    }
}
