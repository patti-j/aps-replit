using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetTogetherContext.Entities
{
    public class ResourceOperation
    {
        [Key] [Column(TypeName = "NVARCHAR(250)")] public string ExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string JobExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string ManufacturingOrderExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string Name { get; set; }
        //[Column(TypeName = "DATETIME")] public DateTime NeedDateTime { get; set; }
        [Column(TypeName = "DECIMAL")] public decimal RequiredFinishQty { get; set; }
        [Column(TypeName = "DECIMAL")] public decimal CycleHours { get; set; }
    }
}