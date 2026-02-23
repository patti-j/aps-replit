namespace PT.ServerManagerSharedLib.Definitions
{
    public class PackedAssembly
    {
        public AssemblyPackageInfo PackageInfo { get; set; }
        public byte[] PackageBytes { get; set; }

        public PackedAssembly() { }

        public PackedAssembly(AssemblyPackageInfo a_packageInfo, byte[] a_packageBytes)
        {
            PackageInfo = a_packageInfo;
            PackageBytes = a_packageBytes;
        }
    }
}
