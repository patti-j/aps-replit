using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Drawing;

namespace PlanetTogetherContext.Entities
{
    public class PTProduct
    {
        [Key]
        [Column(TypeName = "NVARCHAR(250)")] public string ExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string JobExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string MoExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string OpExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string ItemExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string WarehouseExternalId { get; set; }
        [Column(TypeName = "DECIMAL")] public int TotalOutputQty { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string InventoryAvailableTiming { get; set; }
    }
}