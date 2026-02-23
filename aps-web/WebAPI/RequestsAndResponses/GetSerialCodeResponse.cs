namespace WebAPI.RequestsAndResponses;

/// <summary>
/// Response model for getting serial code from planning area
/// </summary>
public class GetSerialCodeResponse
{
    public bool Success { get; set; }
    
    public string? SerialCode { get; set; }
    
    public string? Reason { get; set; }
    
    public int? PlanningAreaId { get; set; }
}