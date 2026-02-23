namespace WebAPI.RequestsAndResponses;

/// <summary>
/// Response model for license validation
/// </summary>
public class LicenseValidationResponse
{
    public bool Valid { get; set; }

    public string? Reason { get; set; }

    public int? PlanningAreaId { get; set; }

    public int? CompanyId { get; set; }

    public string? CompanyName { get; set; }

    public string? SerialCode { get; set; }

    public int? LicenseStatus { get; set; }
}