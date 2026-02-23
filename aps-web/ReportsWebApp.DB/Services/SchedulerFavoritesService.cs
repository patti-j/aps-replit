using Microsoft.EntityFrameworkCore;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.Shared;

public class SchedulerFavoritesService : ISchedulerFavoritesService
{
    private readonly IDbContextFactory<DbReportsContext> _factory;
    //private readonly DbReportsContext _context;
    private readonly IAppInsightsLogger _logger;

    public SchedulerFavoritesService(IDbContextFactory<DbReportsContext> factory, IAppInsightsLogger logger)
    {
        _factory = factory;
        _logger = logger;
    }
    private DbReportsContext GetDbContext()
    {
        return _factory.CreateDbContext();
    }
    public async Task SaveFavoriteAsync(SchedulerFavorite favorite)
    {
        using var dbContext = GetDbContext();
        if (favorite == null)
        {
            var ex = new ArgumentNullException(nameof(favorite));
            _logger.LogError(ex);
            throw ex;
        }

        favorite.Company = null;
        var allGroups = dbContext.Roles.Where(g => g.CompanyId == favorite.CompanyId).ToList();
        var filteredGroups = allGroups.Where(g => favorite.Roles.Any(g2 => g.Id == g2.Id)).ToList();
        favorite.Roles = filteredGroups;

        dbContext.GanttFavorites.Add(favorite);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Log the error, or handle it as needed
            _logger.LogError(ex);
            throw new InvalidOperationException("Could not save the Gantt favorite.", ex);
        }
    }

    public async Task<List<SchedulerFavorite>> GetAllFavoritesAsync()
    {
        using var dbContext = GetDbContext();
        return await dbContext.GanttFavorites.Include(gf => gf.Roles).Include(gf => gf.User).ToListAsync();
    }

    public async Task DeleteFavoriteAsync(int favoriteId)
    {
        using var dbContext = GetDbContext();
        var favorite = await dbContext.GanttFavorites.FindAsync(favoriteId);
        if (favorite == null)
        {
            // Optionally handle or log the "not found" case
            var ex = new KeyNotFoundException("Favorite not found with ID: " + favoriteId);
            _logger.LogError(ex);
            throw ex;
        }

        dbContext.GanttFavorites.Remove(favorite);
        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Log the error, or handle it as needed
            _logger.LogError(ex);
            throw new InvalidOperationException("Could not delete the Gantt favorite.", ex);
        }
    }

    public async Task<SchedulerFavorite> GetFavoriteByIdAsync(Guid favoriteId)
    {
        using var dbContext = GetDbContext();
        var favorite = await dbContext.GanttFavorites
                                      .Include(gf => gf.Roles)
                                      .FirstOrDefaultAsync(gf => gf.CosmosDbRecordId == favoriteId);

        if (favorite == null)
        {
            var ex = new KeyNotFoundException("Favorite not found with ID: " + favoriteId);
            _logger.LogError(ex);
            throw ex;
        }

        return favorite;
    }
}
