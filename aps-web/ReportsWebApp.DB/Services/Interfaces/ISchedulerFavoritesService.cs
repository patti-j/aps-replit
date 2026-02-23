using ReportsWebApp.DB.Models;

public interface ISchedulerFavoritesService
{
    Task SaveFavoriteAsync(SchedulerFavorite favorite);
    Task<List<SchedulerFavorite>> GetAllFavoritesAsync();
    Task DeleteFavoriteAsync(int favoriteId);
    Task<SchedulerFavorite> GetFavoriteByIdAsync(Guid favoriteId);
}