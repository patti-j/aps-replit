using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetTogetherContext.Entities
{
    public class SalesOrderLine
    {
        [Column(TypeName = "NVARCHAR(250)")] public string ItemExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string LineNumber { get; set; }
        [Key][Column(TypeName = "NVARCHAR(250)")] public string SalesOrderExternalId { get; set; }

    }
}