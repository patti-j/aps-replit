using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.SchedulerHelpers;

namespace ReportsWebApp.DB.Services.Interfaces;

public interface IGanttService
{
    public BryntumSettings ActiveSettings { get; set; }
    public BryntumProject ActiveProject { get; set; }
    public GanttFavoriteData? ActiveFavorite { get; set; }
    public HashSet<string>? ResourceVisibilityAndOrder { get; set; }
    public HashSet<string>? ResourceFiltering { get; set; }
    public SchedulerConfiguration PresetConfig { get; }
    public TemplateSegmentContext TemplateSegmentContext { get; }
    public BryntumGridSettings? ActiveGridSettings { get; set; }
    public event Action OnGanttConfigChanged;
    public bool GanttConfigModified { get; set; }
    public void SetFavorite(GanttFavoriteData favorite);
    public void ClearFavorite();
    public Task RecolorDiskImageFromScenarioAsync(Scenario scenario, Action<string>? updateStatus = null);
    public Task CreateProjectFromScenarioAsync(Scenario scenario, Action<string>? updateStatus = null);
    public Task UpdateResourceVisibilityAsync(BryntumProject scheduler, HashSet<string> visibleIds);
    public void UpdateColumnVisibility(List<ColumnVisibility> visibilities);
    public void SaveCurrentGanttConfig();
    public void SaveCurrentGanttConfigToNewFavorite(SchedulerFavorite favorite);
}