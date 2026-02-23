using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ReportsWebApp.Common;
using ReportsWebApp.DB.Data;
using ReportsWebApp.DB.Models;
using ReportsWebApp.DB.Models.BryntumObjects;
using ReportsWebApp.DB.Services.Interfaces;
using ReportsWebApp.Shared;

namespace ReportsWebApp.DB.Services;

public class GanttDataService : IGanttDataService
{
    private readonly IJobRetrievalService _jobRetrievalService;
    private readonly IEventService _eventService;
    private TemplateSegmentContext m_templateSegmentContext;
    private readonly ICompanyDbService m_companyDbService;
    private readonly IServiceProvider _serviceProvider;
    private BryntumSettings _settings;

    public GanttDataService(
        IJobRetrievalService jobRetrievalService,
        IEventService eventService, ICompanyDbService companyDbService, IServiceProvider serviceProvider)
    {
        _jobRetrievalService = jobRetrievalService;
        _eventService = eventService;
        m_companyDbService = companyDbService;
        _serviceProvider = serviceProvider;
    }
    private DbReportsContext GetDbContext()
    {
        // Resolve a new scoped DbContext using the IServiceProvider
        return _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DbReportsContext>();
    }

    public void SetSettings(BryntumSettings settings, TemplateSegmentContext context = null)
    {
        if (m_templateSegmentContext == null && context == null)
        {
            m_templateSegmentContext = new TemplateSegmentContext(settings);
        }
        else if (context != null)
        {
            m_templateSegmentContext = context;
        }
        _settings = settings;
        m_templateSegmentContext.SetSettings(settings);
    }

    public void ClearSettings()
    {
        m_templateSegmentContext = null;
        _settings = null;
    }

    public async Task
        RecolorJobsAsync(List<SegmentConfig> segmentsConfigs, Scenario scenario, BryntumProject project, Action<string>? updateStatus = null )
    {
        updateStatus?.Invoke("Building segment color table...");
        var segmentColorLookup = SegmentGenerator.BuildSegmentColorLookup(_settings.SegmentTypes);
        m_templateSegmentContext.SetSettings(_settings);

        updateStatus?.Invoke("Reconfiguring events...");
        project.Events = CreateWithProgress(scenario.DetailDashtPlanning, updateStatus, "event", detail =>
        {
            if (int.TryParse(detail.ResourceId, out int resourceId) && resourceId != 0)
            {
                return CreateEventFromJobDetail(detail, segmentsConfigs, segmentColorLookup, _settings.SegmentTypes, _settings);
            }
            return null;
        });

        updateStatus?.Invoke("Creating capacity definitions...");
        project.Concurrencies = CreateWithProgress(scenario.DetailDashtCapacityPlanningShiftsCombined, updateStatus, "concurrency", detail =>
            CreateCapacityIntervalFromDetail(detail));

        updateStatus?.Invoke("Creating material assignments...");
        var materials = CreateWithProgress(scenario.DetailDasht_Materials, updateStatus, "material", detail =>
            CreateMaterialFromMaterialDetail(detail, project.Events));

        updateStatus?.Invoke("Linking dependencies...");
        LinkDependencies(project.Events, materials);

        updateStatus?.Invoke("Job loading completed.");

        
        
    }

    public async Task<(DashtPlanning ExampleJobDetail, DashtCapacityPlanningShiftsCombined ExampleCapacityDetail, Dasht_Materials ExampleMaterial)>
        GetJobsAsync(CompanyDb db, List<SegmentConfig> segmentsConfigs, Scenario scenario, Action<BryntumProject> assignProject, Action<string>? updateStatus = null)
    {
        BryntumProject project = new BryntumProject();
        assignProject(project);
        project.id = Guid.NewGuid();
        updateStatus?.Invoke("Retrieving job details...");
        await _jobRetrievalService.GetJobDetailsAsync(db, scenario);

        updateStatus?.Invoke("Retrieving capacity details...");
        await _jobRetrievalService.GetConcurrenciesDetailsAsync(db, scenario);

        updateStatus?.Invoke("Retrieving material details...");
        await _jobRetrievalService.GetMaterialsDetails(db, scenario);

        var exampleJob = GetRandomItem(scenario.DetailDashtPlanning);
        var exampleCapacity = GetRandomItem(scenario.DetailDashtCapacityPlanningShiftsCombined);
        var exampleMaterial = GetRandomItem(scenario.DetailDasht_Materials);

        if (scenario.DetailDashtPlanning == null || !scenario.DetailDashtPlanning.Any())
        {
            updateStatus?.Invoke("No jobs were found.");
            return (exampleJob, exampleCapacity, exampleMaterial);
        }

        updateStatus?.Invoke("Building segment color table...");
        m_templateSegmentContext.SetSettings(_settings);
        var segmentColorLookup = SegmentGenerator.BuildSegmentColorLookup(_settings.SegmentTypes);

        updateStatus?.Invoke("Creating events...");
        project.Events = CreateWithProgress(scenario.DetailDashtPlanning, updateStatus, "event", detail =>
        {
            if (int.TryParse(detail.ResourceId, out int resourceId) && resourceId != 0)
            {
                return CreateEventFromJobDetail(detail, segmentsConfigs, segmentColorLookup, _settings.SegmentTypes, _settings);
            }
            return null;
        });

        updateStatus?.Invoke("Creating concurrency definitions...");
        project.Concurrencies = CreateWithProgress(scenario.DetailDashtCapacityPlanningShiftsCombined, updateStatus, "concurrency", detail =>
            CreateCapacityIntervalFromDetail(detail));

        updateStatus?.Invoke("Creating material assignments...");
        BryntumProject prj = project;
        var materials = CreateWithProgress(scenario.DetailDasht_Materials, updateStatus, "material", detail =>
            CreateMaterialFromMaterialDetail(detail, prj.Events));

        updateStatus?.Invoke("Linking dependencies...");
        LinkDependencies(project.Events, materials);

        updateStatus?.Invoke("Job loading completed.");
        return (exampleJob, exampleCapacity, exampleMaterial);
    }

    private List<TOut> CreateWithProgress<TIn, TOut>(
        List<TIn> items,
        Action<string> updateStatus,
        string itemType,
        Func<TIn, TOut> processor)
        where TOut : class
    {
        var results = new List<TOut>();
        var progress = new ProgressReporter(items.Count, updateStatus);

        foreach (var item in items)
        {
            var result = processor(item);
            if (result != null)
                results.Add(result);

            string itemName = item switch
            {
                DashtPlanning j => j.Opname ?? j.JobName ?? "Unnamed job",
                DashtCapacityPlanningShiftsCombined c => c.IntervalName ?? "Unnamed interval",
                Dasht_Materials m => m.MaterialName ?? "Unnamed material",
                _ => itemType
            };

            progress.ItemProcessed(itemName);
        }

        return results;
    }

    private T GetRandomItem<T>(List<T> items)
    {
        return (items == null || !items.Any()) ? default : items[new Random().Next(items.Count)];
    }

    private Event CreateEventFromJobDetail(DashtPlanning detail, List<SegmentConfig> configs, Dictionary<string, string> colorLookup, List<SegmentType> segmentTypes, BryntumSettings settings)
    {
        var segments = SegmentGenerator.CreateSegmentsFromConfigs(configs, detail, colorLookup, segmentTypes, settings, m_templateSegmentContext);
        var eventDetail = _eventService.CreateEventFromDetail(detail, segments);
        m_templateSegmentContext.SetExamples(detail);
        return eventDetail;
    }

    private ResourceTimeRange CreateCapacityIntervalFromDetail(DashtCapacityPlanningShiftsCombined detail)
    {
        string text = _settings.ShowCapacityLabels
            ? m_templateSegmentContext.GetCapacityTemplateValued(detail, _settings.CapacityLabelsTemplate)
            : detail.IntervalName;

        string color = _settings.IntervalColors.Values
            .FirstOrDefault(c => c.Type.Equals(detail.IntervalType, StringComparison.OrdinalIgnoreCase))?.Color ?? "#000000";

        return _eventService.CreateCapacityIntervalFromDetail(detail, text, color);
    }

    private Material CreateMaterialFromMaterialDetail(Dasht_Materials detail, List<Event> events)
    {
        return _eventService.CreateMaterialFromDetail(detail, events);
    }

    private void LinkDependencies(List<Event> events, List<Material> materials)
    {
        if (events == null || materials == null)
            throw new ArgumentNullException("events or materials", "The events or materials list cannot be null.");

        foreach (var ev in events.Where(e => e != null))
        {
            ev.DependenciesAsSuccessor = events
                .Where(e => e.DashtPlanning.NewScenarioId == ev.DashtPlanning.NewScenarioId &&
                            e.OperationId == ev.SuccessorDependency &&
                            e.DashtPlanning.BlockId == ev.DashtPlanning.SuccessorBlockId)
                .Select(e => e.Id)
                .ToList();

            ev.MaterialsAsPredecessor = materials
                .Where(m => m.Supplier == ev)
                .Select(m => m.Receiver?.Id ?? 0)
                .ToList();

            ev.MaterialsAsSuccessor = materials
                .Where(m => m.Receiver == ev)
                .Select(m => m.Supplier?.Id ?? 0)
                .ToList();
        }
    }

    // Asynchronous version of GetResources
    public async Task<List<BryntumResource>> GetResourcesAsync(CompanyDb db, Scenario scenario)
    {
        return (await m_companyDbService.GetResources(db, scenario))
               .Select(r => new BryntumResource
               {
                   Id = Convert.ToInt32(r.ResourceId),
                   Name = r.ResourceName ?? "", 
                   Workcenter = r.WorkcenterName ?? "",
                   ResourceType = r.ResourceType ?? "", 
                   Description = r.ResourceDescription ?? "",
                   Department = r.DepartmentName ?? "", 
                   Scenario = r.ScenarioName ?? ""
               }).ToList();
    }
    public async Task<DashtResource> GetRandomResourceAsync(CompanyDb db)
    {
        return await m_companyDbService.GetRandomResource(db);
    }

    public async Task<List<GanttNote>> GetNotesForScenarioAsync(int companyId, string scenarioId)
    {
        await using var dbContext = GetDbContext();
        var ganttNotes = await dbContext.GanttNotes.Where(x => x.CompanyId == companyId && x.NewScenarioId == scenarioId).ToListAsync();
        return ganttNotes;
    }
    public async Task SaveNote(GanttNote note)
    {
        await using var dbContext = GetDbContext();
        if (note.Id == 0) //new
        {
            dbContext.GanttNotes.Add(note);
        }
        else
        {
            GanttNote? maybeNote = dbContext.GanttNotes.FirstOrDefault(x => x.Id == note.Id);
            if (maybeNote != null)
            {
                maybeNote.Text = note.Text; //its only really valid to update the text on a note, notes should otherwise only be deleted
            }
        }
        await dbContext.SaveChangesAsync();
        GanttNote? savedNote = dbContext.GanttNotes.FirstOrDefault(x => x.Id == note.Id);
        EventBus.Main.PostEventSync(new GanttNoteUpdatedEvent(savedNote));
    }
    public async Task DeleteNote(GanttNote note)
    {
        await using var dbContext = GetDbContext();
        GanttNote? maybeNote = dbContext.GanttNotes.FirstOrDefault(x => x.Id == note.Id);
        if (maybeNote != null)
        {
            dbContext.GanttNotes.Remove(maybeNote);
        }
        await dbContext.SaveChangesAsync();
    }
}