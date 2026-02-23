using System.ComponentModel.DataAnnotations;

namespace WebAPI.RequestsAndResponses;

/// <summary>
/// Request model for getting serial code from planning area
/// </summary>
public class GetSerialCodeRequest
{
    [Required]
    public string PlanningAreaId { get; set; } = string.Empty;
}