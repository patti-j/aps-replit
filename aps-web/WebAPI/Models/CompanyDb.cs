using System.ComponentModel.DataAnnotations.Schema;

namespace WebAPI.Models
{
    public class CompanyDb : BaseEntity
    {
        public string Environment { get; set; }
        public string ServerManagerUrl { get; set; }
        public string ImportUserName { get; set; }
        public string ImportUserPasswordKey { get; set; }
        public string DBServerName { get; set; }
        public string DBName { get; set; }
        public string DBUserName { get; set; }
        public string DBPasswordKey { get; set; }

        [ForeignKey("Company")]
        public int CompanyId { get; set; } // Changed to int?

        public virtual Company Company { get; set; }

        public CompanyDb()
        {
            CompanyId = 0; // Set a default value (e.g., 0) for CompanyId
        }

        public EDbType DbType { get; set; }
    }

    public enum EDbType
    {
        Import = 0,
        Publish = 1
    }
}
