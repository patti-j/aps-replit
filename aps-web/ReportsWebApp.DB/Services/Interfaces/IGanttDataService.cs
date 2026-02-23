using ReportsWebApp.Common;
using ReportsWebApp.DB.Models;

namespace ReportsWebApp.DB.Services.Interfaces;

public interface IGanttDataService
{
    public void SetSettings(BryntumSettings settings, TemplateSegmentContext context = null);
    public void ClearSettings();
    public Task RecolorJobsAsync(List<SegmentConfig> segmentsConfigs, Scenario scenario, BryntumProject project, Action<string>? updateStatus = null);
    
    public Task<(DashtPlanning ExampleJobDetail, DashtCapacityPlanningShiftsCombined ExampleCapacityDetail, Dasht_Materials ExampleMaterial)> GetJobsAsync(CompanyDb db, List<SegmentConfig> segmentsConfigs, Scenario scenario, Action<BryntumProject> assignProject, Action<string>? updateStatus = null);
    
    Task<List<BryntumResource>> GetResourcesAsync(CompanyDb db, Scenario scenario);
    
    Task<DashtResource> GetRandomResourceAsync(CompanyDb db);
    public Task<List<GanttNote>> GetNotesForScenarioAsync(int companyId, string scenarioId);
    public Task SaveNote(GanttNote note);
    public Task DeleteNote(GanttNote note);
}
public class GanttNoteUpdatedEvent(GanttNote a_Note) : IEvent
{
    public GanttNote Note { get; set; } = a_Note;
}