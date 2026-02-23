using System;
using Microsoft.Extensions.Configuration;

namespace PT.ServerManagerSharedLib.DTOs
{
    public class SettingsDto
    {
        public string ThumbPrint { get; set; }
        public int ApiPort { get; set; }
        public SettingsDto() { }
        public SettingsDto(IConfiguration config)
        {
            ThumbPrint = config["Thumbprint"].ToUpper();
            ApiPort = Convert.ToInt32(config["ApiPort"]);
        }
    }
}