using System.ComponentModel.DataAnnotations;

using ReportsWebApp.Shared;

namespace ReportsWebApp.Data;

public class MaestroIntegrationModel
{
    [Hidden]
    public int CompanyId { get; set; }
    public string PTUserId { get; set; } = "";
    [Password]
    public string PTPassword { get; set; } = "";
    public string AppClientId { get; set; } = "";
    public string AppToken { get; set; } = "";
    [Display(Name = "Planning Area Identifier")]
    public string InstanceIdentifier { get; set; } = "";
    public string PTCloudAppUri { get; set; } = "";
    public string PTPublishTableName { get; set; } = "";
    public string RRPublishTableName { get; set; } = "";
    public string AuthorizationToken { get; set; } = "";
    public int PollingSpanInHours { get; set; }
    public int RRTimeoutOverrideMinutes { get; set; }
    public string PTAppUserToken { get; set; } = "";
}