namespace WebAPI.Models.Integration;

public class IntegrationConfigDetailDTO
{
    public int IntegrationConfigId { get; set; }
    public string IntegrationName { get; set; }
    public string VersionNumber { get; set; }
    public int? UpgradedFromConfigId { get; set; }

    public IntegrationConfigDetailDTO() { }
    public IntegrationConfigDetailDTO(IntegrationConfig config)
    {
        IntegrationConfigId = config.Id;
        IntegrationName = config.Name;
        UpgradedFromConfigId = config.UpgradedFromConfigId;
        VersionNumber = config.VersionNumber;
    }
}

public class IntegrationConfigDetailsDTO
{
    public List<IntegrationConfigDetailDTO> IntegrationConfigs { get; set; }

    public IntegrationConfigDetailsDTO() { }
    public IntegrationConfigDetailsDTO(List<IntegrationConfig> configs)
    {
        IntegrationConfigs = configs.Select(x => new IntegrationConfigDetailDTO(x)).ToList();
    }
}

