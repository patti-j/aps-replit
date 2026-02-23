using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Integration;

public class DBIntegrationObject
{
    [Key]
    public int Id { get; set; }
    
    public string ObjectName { get; set; }
    public string CreateCommand { get; set; }
    
    [Required]
    public EIntegrationDBObjectType ObjectType { get; set; }

    public DBIntegrationObjectDTO ToDto()
    {
        return new DBIntegrationObjectDTO()
        {
            Id = Id,
            ObjectName = ObjectName,
            CreateCommand = CreateCommand,
            ObjectType = ObjectType
        };
    }
}