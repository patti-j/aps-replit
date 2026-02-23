using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetTogetherContext.Entities
{
    public class Capability
    {
        [Key] [Column(TypeName = "NVARCHAR(250)")] public string ExternalId { get; set; }

        [Column(TypeName = "NVARCHAR(250)")] public string Name { get; set; }
    }
}