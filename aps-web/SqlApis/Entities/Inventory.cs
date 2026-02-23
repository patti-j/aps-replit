using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetTogetherContext.Entities
{
    public class Inventory
    {
        [Column(TypeName = "NVARCHAR(250)")] public string ItemExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string WarehouseExternalId { get; set; }
        [Column(TypeName = "DECIMAL")] public decimal OnHandQty { get; set; }

    }
}