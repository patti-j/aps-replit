using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models.Integration;

public class DBIntegration
{
    [Key]
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Version { get; set; }
    
    public DateTime CreatedDate { get; set; }
    
    [ForeignKey("CreatedByUser")]
    public int CreatedBy { get; set; }
    
    public virtual User? CreatedByUser { get; set; }
    
    [ForeignKey("Company")] 
    public int? CompanyId { get; set; }
    public virtual Company? Company { get; set; }
    
    public string VersionNotes { get; set; }

    /// <summary>
    /// Tables, Stored Procedures, Functions, and Views are all stored as a collection of DBObjects, which contain the name
    /// and the create command for the object. The DTO splits this into its separate db objects, e.g. <see cref="DBIntegrationDTO.IntegrationTableObjects"/>.
    /// </summary>
    public List<DBIntegrationObject> IntegrationDBObjects { get; set; } = new();
    
    public DBIntegrationDTO ToDTO()
    {
        return new DBIntegrationDTO()
        {
            Id = Id,
            Name = Name,
            Version = Version,
            VersionNotes = VersionNotes,
            CreatedBy = CreatedBy,
            CreatedDate = CreatedDate,
            CompanyId = CompanyId,
            IntegrationTableObjects = IntegrationDBObjects.Where(x => x.ObjectType == EIntegrationDBObjectType.Table).Select(x => x.ToDto()).ToList(),
            IntegrationStoredProcObjects = IntegrationDBObjects.Where(x => x.ObjectType == EIntegrationDBObjectType.StoredProcedure).Select(x => x.ToDto()).ToList(),
            IntegrationFunctionObjects = IntegrationDBObjects.Where(x => x.ObjectType == EIntegrationDBObjectType.Function).Select(x => x.ToDto()).ToList(),
            IntegrationViewObjects = IntegrationDBObjects.Where(x => x.ObjectType == EIntegrationDBObjectType.View).Select(x => x.ToDto()).ToList(),
        };
    }
}