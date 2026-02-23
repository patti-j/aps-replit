using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetTogetherContext.Entities
{
    public class Plant
    {
        [Column(TypeName = "NVARCHAR(250)")] public string Name { get; set; }
        [Key] [Column(TypeName = "NVARCHAR(250)")] public string ExternalId { get; set; }

    }
}