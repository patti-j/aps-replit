using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetTogetherContext.Entities
{
    public class Lots
    {
        [Column(TypeName = "NVARCHAR(250)")] public string ExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string ItemExternalId { get; set; }
        [Column(TypeName = "INT")] public int Qty { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string WarehouseExternalId { get; set; }

    }
}