using Microsoft.JSInterop;
using ReportsWebApp.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Services;
using ReportsWebApp.DB.Services.Interfaces;
using ReportsWebApp.Resources.Shared;
using ReportsWebApp.Shared;

public class SchedulerInitializer
{
    private readonly IJSRuntime _jsRuntime;
    private readonly TemplateSegmentContext _templateSegmentContext;
    private readonly ServiceContainer _serviceContainer;
    private readonly IAppInsightsLogger _logger;
    private Scenario _currentScenario;
    private SchedulerInterop _schedulerInterop;
    private IGanttDataService _ganttDataService;
    private DotNetObjectReference<GanttNoteInteropContext> _noteCtxRef;
    GanttNoteInteropContext _noteCtx;

    public SchedulerInitializer(
        IJSRuntime jsRuntime,
        ServiceContainer serviceContainer,
        IAppInsightsLogger logger,
        IGanttDataService a_ganttDataService)
    {
        _jsRuntime = jsRuntime;
        _serviceContainer = serviceContainer;
        _logger = logger;
        _ganttDataService = a_ganttDataService;
    }

    public async Task<List<Scenario>> LoadScenariosAsync(User user)
    {
        try
        {
            return (await _serviceContainer.PlanningAreaDataService.GetScenariosByCompanyIdAndUserIdAsync(user.CompanyId, user.Id)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load scenarios.");
            return new List<Scenario>();
        }
    }

    public Task<BryntumGridSettings> CollectBryntumGridSettings()
    {
        return _schedulerInterop?.CollectBryntumGridSettings() ?? Task.FromResult(new BryntumGridSettings());
    }

    public async Task<SchedulerInitializationResult> RecolorScenarioDataAsync(
      Scenario scenario,
      Action<bool, string> showLoadingState,
      Dictionary<string, string> scenarioStatuses)
    {
        _currentScenario = scenario;
        var result = new SchedulerInitializationResult();

        try
        {
            showLoadingState(true, $"Reformating data for {scenario.Name}...");
            scenarioStatuses[scenario.Id] = "Loading...";

            await _serviceContainer.GanttService.RecolorDiskImageFromScenarioAsync(
                scenario,
                message =>
                {
                    showLoadingState(true, message);
                    scenarioStatuses[scenario.Id] = message;
                });

            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to load scenario data for {scenario.Name}.");
            result.ErrorMessage = ex.Message;
            scenarioStatuses[scenario.Id] = $"Error: {ex.Message}";
        }
        finally
        {
            showLoadingState(false, "");
        }

        return result;
    }


    public async Task<SchedulerInitializationResult> LoadScenarioStructureAsync(
      Scenario scenario,
      Action<bool, string> showLoadingState)
    {
        _currentScenario = scenario;
        var result = new SchedulerInitializationResult();

        try
        {
            showLoadingState(true, $"Generating data for {scenario.Name}...");


            await _serviceContainer.GanttService.CreateProjectFromScenarioAsync(
                scenario,
                message =>
                {
                    showLoadingState(true, message);
                });


            _serviceContainer.GanttService.UpdateColumnVisibility(_serviceContainer.GanttService.ActiveProject.ColumnVisibilities);
            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to load scenario data for {scenario.Name}.");
            result.ErrorMessage = ex.Message;
        }
        finally
        {
            showLoadingState(false, "");
        }

        return result;
    }

    public async Task<SchedulerInitializationResult> LoadScenarioDataAsync(
      Scenario scenario,
      Action<bool, string> showLoadingState,
      Dictionary<string, string> scenarioStatuses)
    {
        _currentScenario = scenario;
        var result = new SchedulerInitializationResult();

        try
        {
            showLoadingState(true, $"Generating data for {scenario.Name}...");
            scenarioStatuses[scenario.Id] = "Loading...";

            await _serviceContainer.GanttService.CreateProjectFromScenarioAsync(
                scenario,
                message =>
                {
                    showLoadingState(true, message);
                    scenarioStatuses[scenario.Id] = message;
                });

            _serviceContainer.GanttService.ActiveProject.ScenarioId = scenario.Id;
            _serviceContainer.GanttService.ActiveSettings = _serviceContainer.GanttService.ActiveProject.Settings;
            _serviceContainer.GanttService.UpdateColumnVisibility(_serviceContainer.GanttService.ActiveProject.ColumnVisibilities);

            result.Success = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to load scenario data for {scenario.Name}.");
            result.ErrorMessage = ex.Message;
            scenarioStatuses[scenario.Id] = $"Error: {ex.Message}";
        }
        finally
        {
            showLoadingState(false, "");
        }

        return result;
    }

    public async Task<SchedulerInitializationResult> InitializeSchedulerAsync(int companyId, BryntumProject scheduler, BryntumGridSettings? a_bryntumGridSettings = null)
    {
        var result = new SchedulerInitializationResult();
        try
        {
            //only process visible resources and enabled resources
            new VisibleEventProcessor(await SegmentUtils.GetAllSegments(), scheduler, _serviceContainer.GanttService.TemplateSegmentContext).Process();

            // TODO: Externalize this timeout value to a configuration setting (e.g., appsettings.json) 
            // to allow dynamic adjustment based on environment (e.g., dev/test/prod) or performance tuning.
            var timeout = Task.Delay(2 * 60 * 1000); // 2-minute timeout
            if (_noteCtx != null)
            {
                _noteCtx.Dispose();
                if (_noteCtxRef != null)
                {
                    _noteCtxRef.Dispose();
                }
            }

            _noteCtx = new (companyId, scheduler.ScenarioId, _ganttDataService, scheduler.Events);
            await _noteCtx.Init();
            _noteCtxRef = DotNetObjectReference.Create(_noteCtx);
            var createTask = _schedulerInterop.InitializeScheduler(scheduler, a_bryntumGridSettings, _noteCtxRef);

            var completedTask = await Task.WhenAny(createTask, timeout);
            if (completedTask == timeout)
            {
                result.ErrorMessage = "Timeout occurred while creating scheduler";
                return result;
            }

            var schedulerCreationResult = await createTask;
            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"An error occurred: {ex.Message}";
        }

        return result;
    }

    public async Task UpdateHierarchyFilterAsync(int companyId, BryntumProject scheduler, HashSet<string> enabledNodeIds, BryntumGridSettings? a_bryntumGridSettings = null)
    {
        if (scheduler?.Resources != null)
        {
            scheduler.Resources.ForEach(r => r.Enabled = enabledNodeIds.Contains($"Resource-{r.Id}"));
            await InitializeSchedulerAsync(companyId, scheduler, a_bryntumGridSettings);
        }
    }

    public async Task PrepareScheduler(SchedulerInterop schedulerInterop)
    {
        _schedulerInterop = schedulerInterop;
    }

    public class SchedulerInitializationResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
