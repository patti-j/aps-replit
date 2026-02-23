using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Integration;

public class DBIntegrationDTO
{
    [Key]
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public string Version { get; set; }
    
    public DateTime CreatedDate { get; set; }
    
    public int CreatedBy { get; set; }
    
    public int? CompanyId { get; set; }
    
    public string VersionNotes { get; set; }

    public List<DBIntegrationObjectDTO> IntegrationTableObjects { get; set; } = new();
    
    public List<DBIntegrationObjectDTO> IntegrationStoredProcObjects { get; set; } = new();
    
    public List<DBIntegrationObjectDTO> IntegrationFunctionObjects { get; set; } = new();
    
    public List<DBIntegrationObjectDTO> IntegrationViewObjects { get; set; } = new();

    public DBIntegration ToModel()
    {
        IntegrationTableObjects.ForEach(x => x.ObjectType = EIntegrationDBObjectType.Table);
        IntegrationViewObjects.ForEach(x => x.ObjectType = EIntegrationDBObjectType.View);
        IntegrationFunctionObjects.ForEach(x => x.ObjectType = EIntegrationDBObjectType.Function);
        IntegrationStoredProcObjects.ForEach(x => x.ObjectType = EIntegrationDBObjectType.StoredProcedure);
        return new DBIntegration()
        {
            Id = Id,
            Name = Name,
            CreatedBy = CreatedBy,
            CreatedDate = CreatedDate,
            Version = Version,
            VersionNotes = VersionNotes,
            CompanyId = CompanyId,
            IntegrationDBObjects = IntegrationTableObjects.Select(x => x.ToModel())
                .Concat(IntegrationStoredProcObjects.Select(x => x.ToModel()))
                .Concat(IntegrationFunctionObjects.Select(x => x.ToModel()))
                .Concat(IntegrationViewObjects.Select(x => x.ToModel())).ToList(),
        };
    }
}

