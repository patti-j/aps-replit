using ReportsWebApp.Common;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services.Interfaces;
using ReportsWebApp.DB.Services.SchedulerHelpers;

namespace ReportsWebApp.DB.Services;

public class GanttService : IGanttService
{
    private readonly IGanttDataService ganttDataService;
    private readonly HierarchyFilterService _hierarchyFilterService;
    private readonly ICosmosDbService<GanttFavoriteData> _cosmosDbService;
    private readonly ISchedulerFavoritesService _favoritesService;
    private TemplateSegmentContext _templateSegmentContext;
    private BryntumProject _project;
    private SchedulerConfiguration _presetConfig = new() { InfiniteScroll = true };
    public SchedulerConfiguration PresetConfig => _presetConfig;
    public TemplateSegmentContext TemplateSegmentContext => _templateSegmentContext;
    public BryntumSettings? ActiveSettings { get; set; }
    public BryntumGridSettings? ActiveGridSettings { get; set; }
    public bool GanttConfigModified { get; set; }
    public event Action OnGanttConfigChanged;

    public BryntumProject? ActiveProject
    {
        get { return _project;}
        set {_project = value;}
    }

    public GanttFavoriteData? ActiveFavorite { get; set; }

    public HashSet<string>? ResourceVisibilityAndOrder { get; set; }
    public HashSet<string>? ResourceFiltering { get; set; }

    public GanttService(IGanttDataService dataService, HierarchyFilterService hierarchyFilterService, ICosmosDbService<GanttFavoriteData> cosmosDbService,  ISchedulerFavoritesService favoritesService)
    {
        ganttDataService = dataService;
        _hierarchyFilterService = hierarchyFilterService;
        _cosmosDbService = cosmosDbService;
        _favoritesService = favoritesService;
    }

    public void SetFavorite(GanttFavoriteData favorite)
    {
        ActiveFavorite = favorite;
        ActiveSettings =  favorite.Settings;
    }

    public void ClearFavorite()
    {
        ActiveFavorite = null;
        ActiveSettings = null;
        ActiveProject = null;
        ResourceVisibilityAndOrder = null;
        ResourceFiltering = null;
        ganttDataService.ClearSettings();
    }

    public void SaveCurrentGanttConfig()
    {
        if (ActiveFavorite == null)
        {
            GanttFavoriteData data = new();
            data.FavoriteName = ActiveProject.FavoriteName;
            data.ScenarioId = ActiveProject.ScenarioId;
            data.Settings = ActiveSettings;
            data.GridSettings = ActiveGridSettings;
            data.ResourceFiltering = ResourceFiltering;
            data.ResourceVisibilityAndOrder = ResourceVisibilityAndOrder;
            data.id = ActiveProject.ScenarioId;
            _cosmosDbService.UpdateItemAsync(data, data.id, true);
            GanttConfigModified = false;
            return;
        }

        GanttFavoriteData fav = ActiveFavorite.Copy();
        _cosmosDbService.UpdateItemAsync(fav, fav.id);
        GanttConfigModified = false;
    }
    public void SaveCurrentGanttConfigToNewFavorite(SchedulerFavorite favorite)
    {
        if (ActiveFavorite == null)
        {
            GanttFavoriteData data = new();
            data.FavoriteName = ActiveProject.FavoriteName;
            data.ScenarioId = ActiveProject.ScenarioId;
            data.Settings = ActiveSettings;
            data.GridSettings = ActiveGridSettings;
            data.ResourceFiltering = ResourceFiltering;
            data.ResourceVisibilityAndOrder = ResourceVisibilityAndOrder;
            data.id = favorite.CosmosDbRecordId.ToString();
            _cosmosDbService.AddItemAsync(data, data.id);
            _favoritesService.SaveFavoriteAsync(favorite);
            return;
        }
        GanttFavoriteData fav = ActiveFavorite.Copy();
        fav.id = favorite.CosmosDbRecordId.ToString();
        _cosmosDbService.AddItemAsync(fav, fav.id);
        _favoritesService.SaveFavoriteAsync(favorite);
    }

    public async Task CreateProjectFromScenarioAsync(Scenario scenario, Action<string>? updateStatus = null)
    {
        if (scenario == null)
            throw new ArgumentNullException(nameof(scenario), "Scenario cannot be null.");
        _templateSegmentContext = new TemplateSegmentContext(ActiveSettings);
        var db = scenario.AnalyticalDb ?? throw new Exception("Scenario does not contain valid DB info.");

        if (ActiveFavorite == null)
        {
            if (scenario.BryntumSettings != null)
            {
                ActiveSettings = scenario.BryntumSettings;
            }
            else
            {
                ActiveSettings = new BryntumSettings(true);
            }
        }
        
        updateStatus?.Invoke($"Loading segment config...");
        var segmentConfigs = await SegmentUtils.GetAllSegments();
    
        string fname = ActiveFavorite?.FavoriteName ?? scenario.Name;
        updateStatus?.Invoke($"Loading job data...");
        ganttDataService.ClearSettings();
        ganttDataService.SetSettings(ActiveSettings, _templateSegmentContext);
        var (exampleJob, exampleCap, exampleMat) =
            await ganttDataService.GetJobsAsync(db, segmentConfigs, scenario, proj => { ActiveProject = proj;}, msg => updateStatus?.Invoke($"{msg}"));
    
        updateStatus?.Invoke($"Selecting random resource...");
        var exampleResource = await ganttDataService.GetRandomResourceAsync(db);
        _templateSegmentContext.SetExamples(exampleJob, exampleResource, exampleCap, exampleMat);
    
        updateStatus?.Invoke($"Loading all resources...");
        var allResources = await ganttDataService.GetResourcesAsync(db, scenario);

        var relevantResources = allResources
                                .Where(r => ActiveProject.Events.Select(e => e.ResourceId).Contains(r.Id))
                                .DistinctBy(r => r.Id)
                                .ToList();
        ActiveProject.Resources = relevantResources;
        if (ActiveFavorite != null)
        {
            if (ActiveFavorite.ResourceVisibilityAndOrder != null)
            {
                await UpdateResourceVisibilityAsync(ActiveProject, ActiveFavorite.ResourceVisibilityAndOrder);
            }

            if (ActiveFavorite.ResourceFiltering != null)
            {
                ActiveProject.Resources.ForEach(r => r.Enabled = ActiveFavorite.ResourceFiltering.Contains($"Resource-{r.Id}"));
            }

            if (ActiveFavorite.GridSettings?.columnVisibilities != null)
            {
                ActiveProject.ColumnVisibilities =  ActiveFavorite.GridSettings.columnVisibilities;
            }
        }
        else 
        {
            if (ResourceVisibilityAndOrder != null)
            {
                await UpdateResourceVisibilityAsync(ActiveProject, ResourceVisibilityAndOrder);
            }

            if (ResourceFiltering != null)
            {
                ActiveProject.Resources.ForEach(r => r.Enabled = ResourceFiltering.Contains($"Resource-{r.Id}"));
            }
        }
        await _hierarchyFilterService.LoadDataAsync(ActiveProject);

        updateStatus?.Invoke($"Finishing up...");
        UpdateColumnVisibility(ActiveProject.ColumnVisibilities);
        CalculateEventStatistics(ActiveProject.Events);
        ActiveProject.ScenarioId = scenario.Id;
        ActiveProject.FavoriteName = fname;
        ActiveProject.Configuration = _presetConfig;

        ActiveProject.Settings = ActiveSettings;
        updateStatus?.Invoke($"Done");
    }
    
    public async Task UpdateResourceVisibilityAsync(BryntumProject scheduler, HashSet<string> visibleIds)
    {
        if (scheduler?.Resources != null)
        {
            scheduler.Resources.ForEach(r => r.Visible = visibleIds.Contains(r.Id.ToString()));
            scheduler.Resources = scheduler.Resources
                                           .OrderBy(r => visibleIds.ToList().IndexOf(r.Id.ToString()))
                                           .ThenBy(r => !r.Visible)
                                           .ToList();
        }
    }
    
    public async Task RecolorDiskImageFromScenarioAsync(Scenario scenario, Action<string>? updateStatus = null)
    {
        var db = scenario.AnalyticalDb ?? throw new Exception("Scenario does not contain valid DB info.");

        var segmentConfigs = await SegmentUtils.GetAllSegments();
        ganttDataService.SetSettings(ActiveSettings, _templateSegmentContext);

        CalculateEventStatistics(ActiveProject.Events);
        await ganttDataService.RecolorJobsAsync(segmentConfigs, scenario, ActiveProject, msg => updateStatus?.Invoke($"{msg}"));

        updateStatus?.Invoke($"Done");
    }

    

    private void CalculateEventStatistics(IEnumerable<Event> events)
    {
        if (events != null && events.Any())
        {
            _presetConfig.RowHeight = 150;
            var totalDuration = events.Sum(e => (e.EndDate - e.StartDate).TotalDays); 
            _presetConfig.AverageTaskDuration = totalDuration / events.Count();
            _presetConfig.StartDate = DateTime.Today;
            _presetConfig.EndDate = DateTime.Today.AddDays(30);
        }
        _presetConfig.ShowActivityTitle = ActiveSettings.ShowEventTooltips;
        _presetConfig.Columns.Clear();
        var columns = GetColumns();
        foreach (var column in columns)
        {
            _presetConfig.Columns.Add(column);
        }
    }
    
    #region ColumnService
    private List<ColumnVisibility> columnVisibilities; // Store column visibility settings
    private Dictionary<string, string> columnNames = new Dictionary<string, string>
    {
        { "name", "Resource Name" },
        { "workcenter", "Workcenter" },
        { "resourceType", "Resource Type" },
        { "description", "Description" },
        { "department", "Department" }
    };
    private Dictionary<string, int> columnWidths = new Dictionary<string, int>
    {
        { "name", 150 },
        { "workcenter", 100 },
        { "resourceType", 100 },
        { "description", 100 },
        { "department", 100 }
    };
    public IEnumerable<dynamic> GetColumns()
    {
        if (!columnVisibilities.Any())
        {
            return GetDefaultColumns();
        }
        return columnVisibilities.Where(cv => columnNames.ContainsKey(cv.Id)).Select(cv => new
        {
            field = cv.Id,
            text = columnNames[cv.Id],
            width = columnWidths.ContainsKey(cv.Id) ? columnWidths[cv.Id] : 100, // Default to 100 if not specified
            hidden = cv.Hidden,
            readOnly = true
        });
    }

    private IEnumerable<dynamic> GetDefaultColumns()
    {
        return columnNames.Select(cn => new
        {
            field = cn.Key,
            text = cn.Value,
            width = columnWidths[cn.Key], // Assume all default columns have predefined widths
            readOnly = true
        });
    }

    public void UpdateColumnVisibility(List<ColumnVisibility> visibilities)
    {
        columnVisibilities = visibilities;
    }
    #endregion
    
}