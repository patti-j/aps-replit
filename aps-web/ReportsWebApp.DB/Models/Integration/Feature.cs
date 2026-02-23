using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportsWebApp.DB.Models
{
    public class Feature
    {
        [Key]
        public int Id { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public bool? Distinct { get; set; }
        public bool? AutoDelete { get; set; }
        [ForeignKey(nameof(IntegrationConfig))]
        public int IntegrationConfigId { get; set; }
        public virtual IntegrationConfig IntegrationConfig { get; set; }

    }
}
