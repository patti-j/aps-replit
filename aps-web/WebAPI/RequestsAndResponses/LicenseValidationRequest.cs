using System.ComponentModel.DataAnnotations;

namespace WebAPI.RequestsAndResponses;

/// <summary>
/// Request model for license validation
/// </summary>
public class LicenseValidationRequest
{
    [Required]
    public string PlanningAreaId { get; set; }

    [Required]
    public long Timestamp { get; set; }

    [Required]
    public string Nonce { get; set; }

    [Required]
    public string Signature { get; set; }
}