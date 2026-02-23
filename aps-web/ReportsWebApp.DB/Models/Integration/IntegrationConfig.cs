using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using ReportsWebApp.DB.Models.WebApp;

namespace ReportsWebApp.DB.Models
{
    public class IntegrationConfig : INamedEntity
    {
        [Key]
        public int Id { get; set; }

        public int? UpgradedFromConfigId { get; set; }

        [Required]
        [MaxLength(50)]
        public string VersionNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        public string TypeDisplayName => "Integration Configuration";

        // Related Data
        public virtual List<Feature> Features { get; set; } = new();
        public virtual List<Property> Properties { get; set; } = new();
        public virtual List<PADetails> PlanningAreas { get; set; } = new(); // Instances using this config

        // Company Relationship
        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public virtual Company? Company { get; set; }

        // Tracking Changes
        public DateTime LastEditedDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("LastEditingUser")]
        public int? LastEditingUserId { get; set; } // Maps user making the last change
        public virtual User? LastEditingUser { get; set; }
        public virtual string? LastEditingUserEmail { 
            get {
                return LastEditingUser?.Email;
            }
        }
    }
}
