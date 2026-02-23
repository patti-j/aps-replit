using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetTogetherContext.Entities
{
    public class Department
    {
        [Column(TypeName = "NVARCHAR(250)")] public string ExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string Name { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string PlantExternalId { get; set; }
    }
}