using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class UpdateWebhooksRequest
    {
        public string InstanceName { get; set; }
        public string InstanceVersion { get; set; }
        public string PreImport { get; set; }
        public string PostImport { get; set; }
        public string PreExport { get; set; }
        public string PostExport { get; set; }
    }
}
