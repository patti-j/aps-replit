using System.Collections.Generic;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class DownloadFilesRequest
    {
        public string WorkingFolder { get; set; }
        public Dictionary<string, long> LastUpdatedFiles { get; set; }
    }
}
