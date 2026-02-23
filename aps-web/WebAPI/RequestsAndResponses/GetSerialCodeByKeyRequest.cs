using System.ComponentModel.DataAnnotations;

namespace WebAPI.RequestsAndResponses;

/// <summary>
/// Request model for getting serial code from planning area
/// </summary>
public class GetSerialCodeByKeyRequest
{
    [Required]
    public string PlanningAreaKey { get; set; } = string.Empty;
}