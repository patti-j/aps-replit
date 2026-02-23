using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanetTogetherContext.Entities
{
    public class ResourceRequirement
    {
        [Column(TypeName = "NVARCHAR(250)")] public string ExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string JobExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string MoExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string OpExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string DefaultResourcePlantExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string DefaultResourceDepartmentExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string DefaultResourceExternalId { get; set; }
        [Column(TypeName = "INT")] public Int32 AttentionPercent { get; set; }
    }
}