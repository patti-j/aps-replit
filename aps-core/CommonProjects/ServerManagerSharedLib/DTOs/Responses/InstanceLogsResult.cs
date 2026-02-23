

using System.Collections.Generic;
using PT.ServerManagerSharedLib.Definitions;

namespace PT.ServerManagerSharedLib.DTOs.Responses
{
    public class InstanceLogsResult
    {
        public bool Success { get; set; }
        public List<LogInfo> LogInfos { get; set; }
    }
}
