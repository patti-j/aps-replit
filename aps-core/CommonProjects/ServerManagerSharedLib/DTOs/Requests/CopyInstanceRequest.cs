using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class CopyInstanceRequest
    {
        public SaveInstanceRequest NewInstance { get; set; }
        public InstanceKey OriginInstance { get; set; }
        public bool StartWhenCreated { get; set; }
    }

}
