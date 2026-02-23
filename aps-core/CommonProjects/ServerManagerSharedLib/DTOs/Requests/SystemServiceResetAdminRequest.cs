using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class SystemServiceResetAdminRequest
    {
        public string SecurityToken { get; set; }

        /// <summary>
        /// The specific admin user to reset. If null, resets the default admin.
        /// </summary>
        public long? UserId { get; set; }

        public string NewPassword { get; set; }
    }
}
