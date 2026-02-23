using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.ServerManagerSharedLib.DTOs.Responses
{
    public class WebhooksResponse
    {
        public string PreImportURL { get; set; }
        public string PostImportURL { get; set; }
        public string PreExportURL { get; set; }
        public string PostExportURL { get; set; }
    }
}
