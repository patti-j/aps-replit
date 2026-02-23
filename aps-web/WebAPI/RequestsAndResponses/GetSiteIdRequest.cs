using System.ComponentModel.DataAnnotations;

namespace WebAPI.RequestsAndResponses;

/// <summary>
/// Request model for getting site ID from planning area
/// </summary>
public class GetSiteIdRequest
{
    [Required]
    public string PlanningAreaId { get; set; } = string.Empty;
}