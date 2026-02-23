using System.Reflection;

namespace PT.ServerManagerSharedLib.Definitions
{
    public class AssemblyPackageInfo
    {
        public int PackageId { get; set; }
        public string AssemblyTitleName{ get; set; }
        public string AssemblyFileName{ get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string PathOnDisk{ get; set; }
        public bool IsSchedulingPackage{ get; set; }
        public string SchedulingPackageName{ get; set; }
        public string Version{ get; set; }
        public bool RequiresLicensing{ get; set; }
        public string PackageName{ get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public Assembly Assembly{ get; set; }
        public AssemblyPackageInfo()
        {

        }
    }
}
