using System.ComponentModel.DataAnnotations;

namespace ReportsWebApp.DB.Models;

public class DBIntegrationObject
{
    [Required]
    public int Id { get; set; }
    [Required]
    public string ObjectName { get; set; }
    [Required]
    public string CreateCommand { get; set; }
    [Required]
    public EIntegrationDBObjectType ObjectType { get; set; }
}

public enum EIntegrationDBObjectType
{
    Table = 1,
    StoredProcedure,
    Function,
    View
}