using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportsWebApp.DB.Models
{
    public class InstallCode
    {
        [Key]
        public int Id { get; set; }
        public DateTime? CreationDate { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public string Code { get; set; }
        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }
        public bool Used { get; set; }

        public InstallCode() { }

        public InstallCode(User requestingUser, string env)
        {
            CreatedBy = requestingUser.Email;
            CreationDate = DateTime.UtcNow;
            CompanyId = requestingUser.CompanyId;
            Code = Guid.NewGuid().ToString() + env.FirstOrDefault();
            Used = false;
        }
    }

}
