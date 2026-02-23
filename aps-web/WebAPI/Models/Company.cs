using System.ComponentModel.DataAnnotations;

using WebAPI.Models.Integration;

namespace WebAPI.Models
{
    public class Company: BaseEntity
    {
        [Required]
        public string Email { get; set; }
        public bool Active { get; set; }
        public string? ApiKey { get; set; }
        public string? AllowedDomains { get; set; }
        public virtual List<DBIntegration> Integrations { get; set; } = new();
    }
}
