namespace PT.Common.Sql.SqlServer;


public class DBIntegrationDTO
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Version { get; set; }

    public DateTime CreatedDate { get; set; }

    public int CreatedBy { get; set; }

    public string VersionNotes { get; set; }
    
    public int? CompanyId { get; set; }

    public List<DBIntegrationObjectDTO> IntegrationTableObjects { get; set; } = new();

    public List<DBIntegrationObjectDTO> IntegrationStoredProcObjects { get; set; } = new();

    public List<DBIntegrationObjectDTO> IntegrationFunctionObjects { get; set; } = new();

    public List<DBIntegrationObjectDTO> IntegrationViewObjects { get; set; } = new();
    
}

public class DBIntegrationObjectDTO
{
    public int Id { get; set; }
    
    public string ObjectName { get; set; }
    public string CreateCommand { get; set; }
    
    public EIntegrationDBObjectType ObjectType { get; set; }
}

public enum EIntegrationDBObjectType
{
    Table = 1,
    StoredProcedure,
    Function,
    View
}

