using System.ComponentModel.DataAnnotations;
using ReportsWebApp.Common;

namespace ReportsWebApp.DB.Models
{
    public class Subscription : BaseEntity
    {
        [Required]
        public string Name { get; set; }
        
        public DateTime? Expiration { get; set; }
        
        [Required]
        public string SerialCode { get; set; }
        
        public string? Edition { get; set; }
        
        public string? Description { get; set; }
        
        public int CompanyId { get; set; }
        
        public virtual Company Company { get; set; }
    }
}