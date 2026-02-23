using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetTogetherContext.Entities
{
    public class ManufacturingOrder
    {
        [Key] [Column(TypeName = "NVARCHAR(250)")] public string ExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string JobExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string Name { get; set; }
        [Column(TypeName = "DATETIME")] public DateTime NeedDateTime { get; set; }
        //[Column(TypeName = "DATETIME")] public DateTime ReleaseDateTime { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string ProductName { get; set; }
        [Column(TypeName = "DECIMAL")] public decimal RequiredQty { get; set; }
    }
}