using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.ServerManagerSharedLib.DTOs.Entities
{
    public  class InstanceFromWebAppDto
    {
        public string Settings { get; set; }

        // TODO: When the PlanningArea Settings are split up out of the json, we'll no longer need to send these settings separately
        public ServerWideInstanceSettings ServerWideInstanceSettings { get; set; }
        public bool IsBackup { get; set; }
        public bool IsActive { get; set; }

    }
}
