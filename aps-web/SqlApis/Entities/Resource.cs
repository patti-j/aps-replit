using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetTogetherContext.Entities
{
    public class Resource
    {
        [Key] [Column(TypeName = "NVARCHAR(250)")] public string ExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string Name { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string PlantExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string DepartmentExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string CapacityType { get; set; }
    }
}