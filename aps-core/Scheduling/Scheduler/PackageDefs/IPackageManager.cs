using PT.PackageDefinitions;
using PT.PackageDefinitions.PackageInterfaces;

namespace PT.Scheduler.PackageDefs;

public interface IPackageManager
{
    event Action PackageStateChanged;

    List<IExtensionModule> GetExtensionModules();

    List<IOptimizeRuleModule> GetOptimizeRuleModules();

    List<IPermissionModule> GetPermissionModules();

    List<IPermissionValidationModule> GetPermissionValidationModules();

    List<ILicenseValidationModule> GetLicenseValidationModules();

    List<IKpiCalculatorModule> GetKpiCalculatorModules();

    List<IAutomaticActionsModule> GetAutomaticActionsModules();

    List<IApiModule> GetApiModules();
    
    IEnumerable<LoadedPackage> GetLoadedPackageInfos();

}

public interface IConfigurablePackage : ISchedulingPackage
{
    IPackageManager PackageManagerServer { set; }
    ISettingsManager SettingsManager { set; }
}

/// <summary>
/// Basic information about the package and the IPackage itself
/// </summary>
public struct LoadedPackage
{
    public readonly int PackageId;
    public readonly string PackageName;
    public readonly bool RequiresLicensing;
    public readonly IPackage Package;
    public readonly string Version;

    public LoadedPackage(int a_packageId, string a_packageName, bool a_requiresLicensing, IPackage a_package, string a_version)
    {
        PackageId = a_packageId;
        PackageName = a_packageName;
        RequiresLicensing = a_requiresLicensing;
        Package = a_package;
        Version = a_version;
    }
}