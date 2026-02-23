using System.ComponentModel.DataAnnotations;
using ReportsWebApp.Common;

namespace ReportsWebApp.DB.Models
{
    /// <summary>
    /// Stores minimal information needed to retrieve live subscription data from external services.
    /// This replaces the full Subscription entity which stored all data locally.
    /// </summary>
    public class CompanySubscriptionInfo : BaseEntity
    {
        /// <summary>
        /// Serial code used to retrieve subscriptions from external service
        /// </summary>
        [Required]
        public string SerialCode { get; set; }

        /// <summary>
        /// The company this subscription info belongs to
        /// </summary>
        public int CompanyId { get; set; }
        
        public virtual Company Company { get; set; }
    }
}