using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PlanetTogetherContext.Entities
{
    public class OperationAttribute
    {
        [Column(TypeName = "NVARCHAR(250)")] public string JobExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string MOExternalId { get; set; }
        [Key]
        [Column(TypeName = "NVARCHAR(250)")] public string OpExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string Name { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string Description { get; set; }
        [Column(TypeName = "DECIMAL")] public decimal SetupHrs { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string Code { get; set; }
    }
}
