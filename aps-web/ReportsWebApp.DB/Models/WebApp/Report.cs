using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ReportsWebApp.Common;

namespace ReportsWebApp.DB.Models
{
    public class Report : BaseEntity
    {
        [ConcurrencyCheck]
        [Timestamp]
        public byte[] Version { get; set; }
        public string UpdatedBy { get; set; }
        public string PBIReportId { get; set; }
        public string PBIReportName { get; set; }
        public int PBIWorkspaceId { get; set; }
        [CommonUtils.ForeignType]
        public PBIWorkspace PBIWorkspace { get; set; }
        public List<Category> Categories { get; set; } = new();
        public bool ShowOnOverview { get; set; }
    }
}