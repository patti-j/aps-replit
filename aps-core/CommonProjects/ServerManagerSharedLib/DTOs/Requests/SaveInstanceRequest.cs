using PT.ServerManagerSharedLib.Azure;
using System.Collections.Generic;

namespace PT.ServerManagerSharedLib.DTOs.Requests
{
    public class SaveInstanceRequest
    {
        public string version { get; set; }
        public string instanceName { get; set; }
        public string integrationCode { get; set; }
        public string scenarioFile { get; set; }
        public string scenarioFilePath { get; set; }
        public string serialCode { get; set; }
        public string packageName { get; set; }
        public int environmentType { get; set; } //  Dev, 1 for QA, 2 for Production
        public bool startWhenCreated { get; set; }
        public List<string> WorkspaceFilenames { get; set; }
    }
}
