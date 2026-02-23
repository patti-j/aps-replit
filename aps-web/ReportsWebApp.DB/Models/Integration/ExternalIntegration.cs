using Microsoft.PowerBI.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportsWebApp.DB.Models;

public class ExternalIntegration
{
    [Key]
    [Required]
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; }

    [Required]
    [ForeignKey(nameof(Company))]
    public int CompanyId { get; set; }

    public virtual Company? Company { get; set; }

    [Required]
    public EExternalIntegrationType Type { get; set; }

    public string DisplayType => Type.ToUserString();

    [Required]
    public string SettingsJson { get; set; } = "";
    
    public int? ImportDataConnector {get; set;}
    
    public int? PublishDataConnector {get; set;}

    public string? PreImportProgramArgs { get; set; } = "";

    public string? PreImportProgramPath { get; set; } = "";

    public string? PreImportURL { get; set; } = "";

    public string? PostImportURL { get; set; } = "";

    public bool? IsIntegrationV2Enabled { get; set; } = true;

    public bool? RunPreImportSQL { get; set; } = false;

    public string? PreExportURL { get; set; } = "";

    public string? PostExportURL { get; set; } = "";

    public string? AnalyticsURL { get; set; } = "";
}

public enum EExternalIntegrationType
{
    Null = 0,
    Aha,
    Maestro, //( RR )
    Netsuite,
    D365,
}

public static class EExternalIntegrationTypeExtensions
{
    public static string ToUserString(this EExternalIntegrationType a_enum)
    {
        switch (a_enum)
        {
            case EExternalIntegrationType.Null:
                return "SQL";
            case EExternalIntegrationType.Aha:
                return "Aha";
            case EExternalIntegrationType.Maestro:
                return "Maestro";
            case EExternalIntegrationType.Netsuite:
                return "Netsuite";
            case EExternalIntegrationType.D365:
                return "Dynamics 365";
        }
     
        return a_enum.ToString();
    }
}