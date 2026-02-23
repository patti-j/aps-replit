namespace PT.ServerManagerSharedLib.Definitions
{
    public class AssemblyInfoVersion
    {
        public string Name { get; set; }
        public string Version { get; set; }

        public AssemblyInfoVersion() { }

        public AssemblyInfoVersion(string a_name, string a_version)
        {
            Name = a_name;
            Version = a_version;
        }
    }
}
