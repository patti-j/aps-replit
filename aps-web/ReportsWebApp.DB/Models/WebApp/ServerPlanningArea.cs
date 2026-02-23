using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportsWebApp.DB.Models
{
    public class ServerPlanningArea : BaseEntity
    {
        public int Port { get; set; }
        public string Version { get; set; }
		public string InstanceIdentifier { get; set; }
        [ForeignKey("CompanyServer")]
        public int CompanyServerId { get; set; }
        public virtual CompanyServer CompanyServer { get; set; }
        public override string DetailDisplayValue => $"{this.CompanyServer?.Name ?? "[invalid server]"} - {this.Name}";
    }
}
