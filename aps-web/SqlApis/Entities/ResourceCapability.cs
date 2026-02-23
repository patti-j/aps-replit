using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetTogetherContext.Entities
{
    public class ResourceCapability
    {
        [Column(TypeName = "NVARCHAR(250)")] public string CapabilityExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string PlantExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string DepartmentExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string ResourceExternalId { get; set; }
    }
}