using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Integration;

public class DBIntegrationObjectDTO
{
    [Key]
    public int Id { get; set; }
    
    public string ObjectName { get; set; }
    public string CreateCommand { get; set; }
    
    [Required]
    public EIntegrationDBObjectType ObjectType { get; set; }
    public DBIntegrationObject ToModel()
    {
        return new DBIntegrationObject()
        {
            Id = Id,
            ObjectName = ObjectName,
            CreateCommand = CreateCommand,
            ObjectType = ObjectType
        };
    }
}

public enum EIntegrationDBObjectType
{
    Table = 1,
    StoredProcedure,
    Function,
    View
}