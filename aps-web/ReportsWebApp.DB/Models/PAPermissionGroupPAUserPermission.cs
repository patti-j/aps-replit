using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportsWebApp.DB.Models
{
    public class PAPermissionGroupPAUserPermission
    {
        public int PAPermissionGroupId { get; set; }
        public PAPermissionGroup PAPermissionGroup { get; set; }
        public int PAUserPermissionId { get; set; }
        public PAUserPermission PAUserPermission { get; set; }
    }
}
