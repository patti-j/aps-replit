using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetTogetherContext.Entities
{
    public class SalesOrderLineDistribution
    {
        [Column(TypeName = "NVARCHAR(250)")] public string LineNumber { get; set; }
        [Column(TypeName = "DECIMAL")] public decimal QtyOrdered { get; set; }
        [Column(TypeName = "DATETIME")] public DateTime RequiredAvailableDate { get; set; }
        [Key] [Column(TypeName = "NVARCHAR(250)")] public string SalesOrderExternalId { get; set; }

    }
}