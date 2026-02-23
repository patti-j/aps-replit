using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetTogetherContext.Entities
{
    public class PlantWarehouse
    {
        [Column(TypeName = "NVARCHAR(250)")] public string PlantExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string WarehouseExternalId { get; set; }

    }
}