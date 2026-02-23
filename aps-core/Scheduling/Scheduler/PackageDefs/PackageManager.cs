using System.Reflection;
using System.Runtime.Loader;

using PT.Common.Debugging;
using PT.Common.Localization;
using PT.PackageDefinitions;
using PT.PackageDefinitions.PackageInterfaces;
using PT.SchedulerDefinitions;
using PT.ServerManagerSharedLib.Definitions;
using PT.Transmissions;

namespace PT.Scheduler.PackageDefs;

/// <summary>
/// Stores all the scheduling packages.
/// </summary>
internal class PackageManager : PackageManagerBase, IPackageManager
{
    private readonly IPackageAssemblyLoader m_packageLoader;
    public event Action PackageStateChanged;
    public event Action PackagesLoaded;

    internal PackageManager(IPackageAssemblyLoader a_packageLoader)
    {
        m_packageLoader = a_packageLoader;
        m_packageLoader.ValidLicensedPackagesChanged += PackageLoaderOnValidLicensedPackagesChanged;
    }

    private readonly List<LoadedPackage> m_schedulingPackages = new ();
    private HashSet<int> m_disabledPackageIds = new ();
    private ISettingsManager m_settingsManager;

    internal void SetSettingsManager(ISettingsManager a_settingsManager)
    {
        m_settingsManager = a_settingsManager;
        m_packageLoader.ValidateAndCreatePackageList();
    }

    /// <summary>
    /// Loads scheduling packages from dll files. It also create a zip file
    /// containing the dlls and stores it in binary so the packages can be
    /// reloaded on the clients.
    /// </summary>
    internal void LoadPackagesFromDisk()
    {
        #if DEBUG
        if (m_settingsManager == null)
        {
            throw new DebugException("Setting Manager was not set. Must call SetSettingsManager first");
        }
        #endif

        m_schedulingPackages.Clear(); //This could be a reload
        AssemblyLoadContext context = null;
        try
        {
            if (!m_packageLoader.HasPackages)
            {
                return; // no packages exist.
            }

            // create new app domain to load package assemblies.
            context = new PackageAssemblyContext("PackageContext", true);

            IEnumerable<AssemblyPackageInfo> validPackageInfos = m_packageLoader.GetValidatedAndLicensedPackageInfos();
            foreach (AssemblyPackageInfo packageInfo in validPackageInfos)
            {
                if (packageInfo.IsSchedulingPackage)
                {
                    Assembly packageAssembly;
                    try
                    {
                        packageAssembly = context.LoadFromAssemblyPath(packageInfo.PathOnDisk);
                    }
                    catch (Exception e)
                    {
                        m_packageLoader.PackageLoadException(new PTPackageException("Assembly.LoadFrom({0}) error in LoadFromAssembliesHelper", e, new object[] { packageInfo.PathOnDisk }));
                        continue;
                    }

                    Type schedPackType = null;

                    try
                    {
                        schedPackType = packageAssembly.GetType(packageInfo.SchedulingPackageName, true);
                    }
                    catch (TypeLoadException tLoadErr)
                    {
                        PTPackageException schedLoadException = new ($"Package assembly '{packageInfo.AssemblyFileName}' specifies that scheduler package '{packageInfo.SchedulingPackageName}' should exist. But this package could not be loaded.", tLoadErr);
                        m_packageLoader.PackageLoadException(schedLoadException);
                    }

                    try
                    {
                        object schedPackInstance = Activator.CreateInstance(schedPackType);
                        if (schedPackInstance is ISchedulingPackage schedPack)
                        {
                            m_schedulingPackages.Add(new LoadedPackage(packageInfo.PackageId, schedPack.PackageName, packageInfo.RequiresLicensing, schedPack, packageInfo.Version));
                        }
                    }
                    catch (Exception err)
                    {
                        PTPackageException schedLoadException = new ($"Unable to load package assembly '{packageInfo.AssemblyFileName}'.", err);
                        m_packageLoader.PackageLoadException(schedLoadException);
                    }
                }
            }

            LicenseOptions licenseOptions = m_packageLoader.GetPackageLicenseOptions;
            foreach (LoadedPackage loadedPackage in m_schedulingPackages)
            {
                if (loadedPackage.Package is ILicensedPackage lp)
                {
                    lp.SetLicenseOptions(licenseOptions);
                }
            }
        }
        catch (Exception e)
        {
            PTPackageException schedLoadException = new ("Assembly app domain error", e);
            m_packageLoader.PackageLoadException(schedLoadException);
        }
        finally
        {
            //In case we didn't load any scheduling packages
            if (context != null)
            {
                context.Unload();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        //Search and sort packages by type. Add to collections
        lock (m_packageCollectionLock)
        {
            m_extensionModules.Clear();
            m_optimizeRuleModules.Clear();
            m_licenseValidationModules.Clear();
            m_permissionValidationModules.Clear();
            m_automaticActionsModules.Clear();
            m_backgroundProcessModules.Clear();
            m_apiModules.Clear();

            foreach (LoadedPackage schedPackage in m_schedulingPackages)
            {
                if (!m_disabledPackageIds.Contains(schedPackage.PackageId))
                {
                    LoadSchedulingPackage(schedPackage.Package as ISchedulingPackage);
                }
            }
        }

        PackagesLoaded?.Invoke();
    }

    /// <summary>
    /// Extracts modules from the package and stores them for quicker access.
    /// </summary>
    /// <param name="a_schedPackage"></param>
    private void LoadSchedulingPackage(ISchedulingPackage a_schedPackage)
    {
        if (a_schedPackage is IConfigurablePackage configurablePackage)
        {
            configurablePackage.PackageManagerServer = this;
            configurablePackage.SettingsManager = m_settingsManager;
        }

        if (a_schedPackage is IExtensionPackage schedCustPackage)
        {
            m_extensionModules.AddRange(schedCustPackage.GetExtensionModules());
        }

        if (a_schedPackage is IOptimizeRulePackage optimizeRulePackage)
        {
            m_optimizeRuleModules.AddRange(optimizeRulePackage.GetOptimizeRuleModules());
        }

        if (a_schedPackage is IKpiPackage kpiPackage)
        {
            m_kpiCalculatorModules.AddRange(kpiPackage.GetKpiCalculatorModules());
        }

        if (a_schedPackage is ILocalizationPackage localizationPackage)
        {
            foreach (ILocalizationLanguageModule module in localizationPackage.GetLocalizationModules())
            {
                Localizer.LoadLanguage(module);
            }

            foreach (ILocalizationLanguageModuleTranslations module in localizationPackage.GetLocalizationTranslations())
            {
                Localizer.LoadLanguageTranslations(module);
            }
        }

        if (a_schedPackage is ILicensedPackage licensedPackage)
        {
            m_licenseValidationModules.AddRange(licensedPackage.GetLicenseValidationModules());
        }

        if (a_schedPackage is IUsersPackage permissionPackage)
        {
            m_permissionModules.AddRange(permissionPackage.GetPermissionModules());
        }

        if (a_schedPackage is IPermissionValidationPackage permissionValidationPackage)
        {
            m_permissionValidationModules.AddRange(permissionValidationPackage.GetPermissionValidationModules());
        }

        if (a_schedPackage is IAutomaticActionsPackage autoActionsPackage)
        {
            if (PTSystem.Server) //Only the server needs to load these
            {
                m_automaticActionsModules.AddRange(autoActionsPackage.GetAutomaticActionsModules());
            }
        }

        if (a_schedPackage is IBackgroundProcessPackage backgroundProcessPackage)
        {
            m_backgroundProcessModules.AddRange(backgroundProcessPackage.GetBackgroundProcessModules(PTSystem.Server));
        }

        if (a_schedPackage is IApiPackage apiPackage)
        {
            m_apiModules.AddRange(apiPackage.GetApiModules());
        }
    }

    private void PackageLoaderOnValidLicensedPackagesChanged(PackageStateT a_packageStateT)
    {
        m_disabledPackageIds = a_packageStateT.DisabledPackages;
        LoadPackagesFromDisk();
        PackageStateChanged?.Invoke();
    }

    public IEnumerable<LoadedPackage> GetLoadedPackageInfos()
    {
        foreach (LoadedPackage package in m_schedulingPackages)
        {
            yield return package;
        }
    }

    public void UpdatePackageStates(PackageStateT a_packageStateT)
    {
        m_packageLoader.UpdatePackageStates(a_packageStateT);
    }

    public IEnumerable<AssemblyPackageInfo> GetValidatedPackageAssemblyInfos()
    {
        return m_packageLoader.GetValidatedAndLicensedPackageInfos();
    }

    public PackedAssembly GetAssembly(AssemblyPackageInfo a_AssemblyPackageInfo)
    {
        return m_packageLoader.GetPackedAssembly(a_AssemblyPackageInfo);
    }
}