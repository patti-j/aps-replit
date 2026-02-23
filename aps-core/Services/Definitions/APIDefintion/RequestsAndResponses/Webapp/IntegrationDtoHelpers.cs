using System.ComponentModel.DataAnnotations;

namespace PT.APIDefinitions.RequestsAndResponses.Webapp;

public class IntegrationConfigDTO
{
    [Key]
    public int Id { get; set; }
    
    public int? UpgradedFromConfigId { get; set; }
    public string VersionNumber { get; set; }
    public string Name { get; set; }
    public virtual List<FeatureDTO> Features { get; set; }
    public virtual List<PropertyDTO> Properties { get; set; }
}