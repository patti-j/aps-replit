namespace WebAPI.RequestsAndResponses;

/// <summary>
/// Response model for finding planning areas by site ID
/// </summary>
public class FindPlanningAreasBySiteIdResponse
{
    public bool Success { get; set; }
    
    public string? Reason { get; set; }
    
    public List<PlanningAreaInfo> PlanningAreas { get; set; } = new List<PlanningAreaInfo>();
}

/// <summary>
/// Information about a planning area for the site ID search response
/// </summary>
public class PlanningAreaInfo
{
    public int Id { get; set; }
        
    public string Name { get; set; } = string.Empty;
    
    public string Version { get; set; } = string.Empty;
    
    public string Environment { get; set; } = string.Empty;
    
    public int CompanyId { get; set; }
    
    public string? CompanyName { get; set; }
    
    public string SiteId { get; set; } = string.Empty;
    
    public string LicenseStatus { get; set; } = string.Empty;
}