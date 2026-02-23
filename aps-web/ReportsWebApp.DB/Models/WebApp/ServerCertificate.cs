using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class ServerCertificate : BaseEntity
    {
        [ForeignKey("CompanyServer")] public int CompanyServerId { get; set; }

        [Required] public virtual CompanyServer CompanyServer { get; set; }
        public string Name { get; set; }
        public string Thumbprint { get; set; }
        public string SubjectName { get; set; }
        public string Issuer { get; set; }

        // Use string for dates to avoid deserialization errors
        public string ValidFrom { get; set; }

        // Computed property to parse the start date
        public DateTime? ValidFromDate
        {
            get
            {
                var dates = SplitValidFromDates();
                return DateTime.TryParse(dates.startDate, out var date) ? date : (DateTime?)null;
            }
        }

        // Computed property to parse the end date
        public DateTime? ValidToDate
        {
            get
            {
                var dates = SplitValidFromDates();
                return DateTime.TryParse(dates.endDate, out var date) ? date : (DateTime?)null;
            }
        }

        // Helper method to split the start and end dates from ValidFrom string
        private (string startDate, string endDate) SplitValidFromDates()
        {
            if (!string.IsNullOrWhiteSpace(ValidFrom))
            {
                var parts = ValidFrom.Split(new[] { " to " }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    return (parts[0].Trim(), parts[1].Trim());
                }
            }
            return (null, null); // Return nulls if format is incorrect
        }
    }
}
