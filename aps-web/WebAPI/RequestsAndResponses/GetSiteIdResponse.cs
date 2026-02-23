namespace WebAPI.RequestsAndResponses;

/// <summary>
/// Response model for getting site ID from planning area
/// </summary>
public class GetSiteIdResponse
{
    public bool Success { get; set; }
    
    public string? SiteId { get; set; }
    
    public string? Reason { get; set; }
    
    public int? PlanningAreaId { get; set; }
}