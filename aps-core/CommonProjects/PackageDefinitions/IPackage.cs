using PT.Common.Localization;
using PT.PackageDefinitions.PackageInterfaces;
using PT.SchedulerDefinitions;

namespace PT.PackageDefinitions;

/// <summary>
/// An element created from a module.
/// This key value must be unique among all package elements (even in other packages)
/// </summary>
public interface IPackageElement
{
    string PackageObjectId { get; }
}

public interface IPackageModule { }

public interface ICoreElement
{
    bool Singleton { get; }
    bool AllowOverride { get; }
}

/// <summary>
/// This class should be loaded as a package
/// </summary>
public interface IPackage
{
    string PackageName { get; }
    string DisplayVersion => "";

    /// <summary>
    /// All packages have been loaded. They are now being initialized one by one
    /// </summary>
    void Initialize() { }
}

/// <summary>
/// This class should be loaded as a package
/// </summary>
public interface ILicensedPackage : IPackage
{
    void SetLicenseOptions(LicenseOptions a_licenseOptions);

    List<ILicenseValidationModule> GetLicenseValidationModules();
}

public interface IPermissionValidationPackage : IPackage
{
    List<IPermissionValidationModule> GetPermissionValidationModules();
}

/// <summary>
/// This package contains localization modules
/// </summary>
public interface ILocalizationPackage : IPackage
{
    List<ILocalizationLanguageModule> GetLocalizationModules();
    List<ILocalizationLanguageModuleTranslations> GetLocalizationTranslations();
}

public interface IPlanningHorizonPackage : IPackage
{
    IHorizonModule GetHorizonModule { get; }
}

public interface IHorizonModule : IPackageModule
{
    IHorizonelement GetHorizonElement { get; }
}

public interface IHorizonelement : IPackageElement
{
    TimeSpan PlanningHorizonMaxLength { get; }
}

public interface ISchedulingPackage : IPackage
{
    bool ServerOnly { get; }
}

public interface IExtensionPackage : ISchedulingPackage
{
    List<IExtensionModule> GetExtensionModules();
    ISettingsManager SettingsManager { set; }
}

public interface IOptimizeRulePackage : ISchedulingPackage
{
    List<IOptimizeRuleModule> GetOptimizeRuleModules();
}

public interface IKpiPackage : ISchedulingPackage
{
    List<IKpiCalculatorModule> GetKpiCalculatorModules();
}

public interface IBackgroundProcessPackage : IPackage
{
    List<IBackgroundProcessModule> GetBackgroundProcessModules(bool a_isServer);
}

public interface IBackgroundProcessModule
{
    List<IBackgroundProcessElement> GetBackgroundProcessElements(bool a_isServer);
    List<IStartupElement> GetStartupElements(bool a_isServer);
}

public interface IStartupElement : IPackageElement
{
    void PostDataLoadInit(object a_ptSystem);
}

public interface IBackgroundProcessElement : IPackageElement
{
    void Start();

    void Stop();
}