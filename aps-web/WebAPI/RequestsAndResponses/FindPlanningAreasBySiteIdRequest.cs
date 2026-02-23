using System.ComponentModel.DataAnnotations;

namespace WebAPI.RequestsAndResponses;

/// <summary>
/// Request model for finding planning areas by site ID
/// </summary>
public class FindPlanningAreasBySiteIdRequest
{
    [Required]
    public string SiteId { get; set; } = string.Empty;
}