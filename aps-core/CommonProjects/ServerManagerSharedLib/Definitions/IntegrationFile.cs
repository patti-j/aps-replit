
public class IntegrationFile
{
    public string connectionString { get; set; }
    public string databaseName { get; set; }
    public string erpDatabaseName { get; set; }
    public string preImportSQL { get; set; }
    public string sqlServerConnectionString { get; set; }
    public bool runPreImportSQL { get; set; }
    public string userName { get; set; }
    public string erpServerName { get; set; }
    public string erpUserName { get; set; }
    public string erpPassword { get; set; }
    public ScenarioInfo[] Scenarios { get; set; }
    public Configurations Configurations { get; set; }
    public string DefaultConfiguration { get; set; }
}

public class Configurations
{
    public string[] Demo { get; set; }
    public string[] Starter { get; set; }
    public string[] Full { get; set; }
    public string[] Minimal { get; set; }
    public string[] ProductionScheduling { get; set; }
    public string[] SolutionModeling { get; set; }
}

public class ScenarioInfo
{
    public string Name { get; set; }
    public string Path { get; set; }
}
