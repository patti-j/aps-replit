using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace PlanetTogetherContext.Entities
{
    public class SalesOrder
    {
        [Key] [Column(TypeName = "NVARCHAR(250)")] public string ExternalId { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string Name { get; set; }
        [Column(TypeName = "VARCHAR(250)")] public string ReferenceNum { get; set; }
        [Column(TypeName = "DATETIME")] public DateTime CreatedAt { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string ProductId { get; set; }
        [Column(TypeName = "INT")] public int Votes { get; set; }
        [Column(TypeName = "NVARCHAR(250)")] public string WorkflowStatus { get; set; }
        [Column(TypeName = "NVARCHAR(MAX)")] public string Description { get; set; }
        [Column(TypeName = "VARCHAR(250)")] public string IdeaUrl { get; set; }
        [Column(TypeName = "INT")] public int EndorsementsCount { get; set; }
        [Column(TypeName = "NVARCHAR(MAX)")] public string Categories { get; set; }

    }
}