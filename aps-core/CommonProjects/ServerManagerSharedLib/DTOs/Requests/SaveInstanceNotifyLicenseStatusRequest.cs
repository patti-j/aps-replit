using System;
using PT.ServerManagerSharedLib.DTOs.Entities;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class SaveInstanceNotifyLicenseStatusRequest : InstanceKey
    {
        public Boolean ReadOnly { get; set; }
    }
}
