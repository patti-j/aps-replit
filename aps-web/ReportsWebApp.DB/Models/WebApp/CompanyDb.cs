using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportsWebApp.DB.Models
{
    public class CompanyDb : BaseEntity
    {
        public string? Environment { get; set; }
        //public string ConnectionStringKey { get; set; }
        public string? ServerManagerUrl { get; set; }
        public string? ImportUserName { get; set; }
        public string? ImportUserPasswordKey { get; set; }
        [Required]
        public string DBServerName { get; set; }
        [Required]
        public string DBName { get; set; }
        [Required]
        public string DBUserName { get; set; }
        [Required]
        public string DBPasswordKey { get; set; }
        [NotMapped]
        public string ConnectionString { get; set; }

        [ForeignKey("Company")]
        public int CompanyId { get; set; } // Changed to int?

        public virtual Company Company { get; set; }
        
        public CompanyDb()
        {
            CompanyId = 0; // Set a default value (e.g., 0) for CompanyId
        }

        public EDbType DbType { get; set; }

        [NotMapped]
        public override string TypeDisplayName => "Company Database";
    }

    public enum EDbType
    {
        Import = 0,
        Publish = 1,
        Analytical = 2,
        APS_Instances = 3,
    }
}
