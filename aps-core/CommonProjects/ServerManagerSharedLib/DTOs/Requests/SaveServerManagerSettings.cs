 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class SaveServerManagerSettingsRequest
    {
        public string ServerMessage { get; set; }
        public string ComputerNameorIP { get; set; }
        public string SsoDomain { get; set; }
        public string SsoClientId { get; set; }
        /// <summary>
        /// Currently, the UI toggles this, then the backend sets a WebAppUrl accordingly. 
        /// </summary>
        public bool AllowWebAppConnection { get; set; }
        public string WebAppClientId { get; set; }
    }
}
