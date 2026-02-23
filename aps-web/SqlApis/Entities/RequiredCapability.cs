using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetTogetherContext.Entities
{
    public class RequiredCapability
    {
        [Column(TypeName = "NVARCHAR(250)")] public string CapabilityExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string JobExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string MoExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string OpExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string ResourceRequirementExternalId { get; set; }
    }
}