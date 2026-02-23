using System.Collections.Generic;
using PT.ServerManagerSharedLib.Definitions;

namespace PT.ServerManagerSharedLib.DTOs.Responses
{
    public class InstanceLogsCountResult
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public int LogInfosCount { get; set; }
    }
}
