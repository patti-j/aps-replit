using ReportsWebApp.DB.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class DataConnector
{
    [Key]
    [Required]
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = "";

    [Required]
    [ForeignKey(nameof(Company))]
    public int CompanyId { get; set; }

    public virtual Company? Company { get; set; }

    [Required]
    public string ConnectionString { get; set; } = "";

    // IMPORT DB
    [Required]
    public string ImportConnectionString { get; set; } = "";
    public string? ImportIntegrationUserAndPass { get; set; } // optional

    // PUBLISH DB
    public string? PublishConnectionString { get; set; } = "";

    public string? PreImportSQL { get; set; }
    public string? PostExportSQL { get; set; }
    
    public string? PreImportProgramArgs { get; set; } = "";

    public string? PreImportProgramPath { get; set; } = "";

    public string? PreImportURL { get; set; } = "";

    public string? PostImportURL { get; set; } = "";

    public bool? IsIntegrationV2Enabled { get; set; } = true;

    public bool? RunPreImportSQL { get; set; } = false;

    public string? PreExportURL { get; set; } = "";

    public string? PostExportURL { get; set; } = "";
    
    public bool? UseSeparateDatabases { get; set; } = false;

}
