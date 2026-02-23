using ReportsWebApp.DB.Models;

namespace ReportsWebApp.DB.Services.SchedulerHelpers;

public class GanttFavoriteData
{
    public string id { get; set; }
    public string ScenarioId { get; set; }
    public string FavoriteName { get; set; }
    public BryntumSettings Settings { get; set; }
    public HashSet<string>? ResourceVisibilityAndOrder { get; set; }
    public HashSet<string>? ResourceFiltering { get; set; }
    public BryntumGridSettings? GridSettings { get; set; }
}