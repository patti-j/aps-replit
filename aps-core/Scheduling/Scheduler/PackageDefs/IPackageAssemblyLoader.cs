using PT.APSCommon;
using PT.Common.Exceptions;
using PT.ServerManagerSharedLib.Definitions;
using PT.Transmissions;

namespace PT.Scheduler.PackageDefs;

public interface IPackageAssemblyLoader
{
    #region Properties
    string PackagesOnDiskPath { get; }
    string PackageErrorLogPath { get; }
    bool HasPackages { get; }
    SchedulerDefinitions.LicenseOptions GetPackageLicenseOptions { get; }
    #endregion

    #region Events
    event Action<PackageStateT> ValidLicensedPackagesChanged;
    #endregion

    #region Validation
    bool ValidPackagePath(string a_filePath);
    IEnumerable<AssemblyPackageInfo> GetValidatedAndLicensedPackageInfos();
    #endregion

    #region Error Logging
    void PackageLoadException(PTPackageException a_exception);
    #endregion

    void ValidateAndCreatePackageList();
    void UpdatePackageStates(PackageStateT a_packageStateT);
    PackedAssembly GetPackedAssembly(AssemblyPackageInfo a_AssemblyPackageInfo);
    void AddAssemblyInfo(AssemblyPackageInfo a_packedAssemblyPackageInfo);
    void ClearAssemblyInfos();
    void UpdatePackageOnDiskPath(string a_newPath);
    AssemblyPackageInfo[] GetLatestPackageInfos();
}

public class PTPackageException : PTHandleableException
{
    public PTPackageException(string a_message, Exception a_innerException) : base(a_message, a_innerException) { }

    public PTPackageException(string a_message, Exception a_innerException, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
        : base(a_message, a_innerException, a_stringParameters, a_appendHelpUrl) { }
}