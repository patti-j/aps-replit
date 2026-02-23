using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models.Integration
{
    public class IntegrationConfig
    {
        [Key]
        public int Id { get; set; }

        public int? UpgradedFromConfigId { get; set; }
        public string VersionNumber { get; set; }
        public string Name { get; set; }
        public virtual List<Feature> Features { get; set; }
        public virtual List<Property> Properties { get; set; }

        // Additional props for your entity, not needed in the DTOs
        public virtual List<PADetails> PlanningAreas { get; set; } // The instances using this config
        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public virtual Company? Company { get; set; } = null;
        public DateTime LastEditedDate { get; set; } = DateTime.UtcNow;// You can set when accepting updates via api
        [ForeignKey("LastEditingUser")]
        public int? LastEditingUserId { get; set; } // We might send the user email over api, you can map to users however is convenient
        public virtual User? LastEditingUser { get; set; }
        public virtual string? LastEditingUserEmail
        {
            get
            {
                return LastEditingUser?.Email;
            }
        }

        public IntegrationConfigDTO ToDTO()
        {
            return new IntegrationConfigDTO
            {
                Id = Id,
                Name = Name,
                Features = Features.Select(x => x.ToDTO()).ToList(),
                Properties = Properties.Select(x => x.ToDTO()).ToList(),
                VersionNumber = VersionNumber,
                UpgradedFromConfigId = UpgradedFromConfigId
            };
        }
    }
}
