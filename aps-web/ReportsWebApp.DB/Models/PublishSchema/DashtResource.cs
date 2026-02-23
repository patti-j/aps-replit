using System;
using System.Collections.Generic;

namespace ReportsWebApp.DB.Models;

public class DashtResource
{
    public string? PlanningAreaName { get; set; }

    public string? PlantId { get; set; }

    public string? PlantName { get; set; }

    public string? DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    public string? ResourceId { get; set; }

    public string? ResourceName { get; set; }

    public string? ResourceDescription { get; set; }

    public string? ResourceType { get; set; }

    public string? WorkcenterName { get; set; }

    public string? CellName { get; set; }

    public bool? ResourceExcludeFromGantts { get; set; }

    public string? ResourceNotes { get; set; }

    public string? ResourceCapacityType { get; set; }

    public bool? Bottleneck { get; set; }

    public bool? Drum { get; set; }

    public string? ResourceImageFileName { get; set; }

    public string? ResourceNormalOptimizeRule { get; set; }

    public bool? ResourceIsTank { get; set; }

    public DateTime? PublishDate { get; set; }
    
    public string? ScenarioName { get; set; }

    public string? ScenarioType { get; set; }

    public string? PlanningAreaId { get; set; }
    public string? NewScenarioId { get; set; }
    public long ScenarioId { get; set; }
}
