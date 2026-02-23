using System.Reflection;

using PT.PackageDefinitions.PackageInterfaces;

namespace PT.PackageDefinitions;

public class PackageManagerBase
{
    protected object m_packageCollectionLock = new ();

    protected readonly List<string> ExcludedPackages;
    protected readonly List<IExtensionModule> m_extensionModules = new ();
    protected readonly List<IOptimizeRuleModule> m_optimizeRuleModules = new ();
    protected readonly List<IKpiCalculatorModule> m_kpiCalculatorModules = new ();
    protected readonly List<IPermissionModule> m_permissionModules = new ();
    protected readonly List<ILicenseValidationModule> m_licenseValidationModules = new ();
    protected readonly List<IPermissionValidationModule> m_permissionValidationModules = new ();
    protected readonly List<IAutomaticActionsModule> m_automaticActionsModules = new ();
    protected readonly List<IBackgroundProcessModule> m_backgroundProcessModules = new ();
    protected readonly List<IApiModule> m_apiModules = new ();

    public List<IExtensionModule> GetExtensionModules()
    {
        lock (m_packageCollectionLock)
        {
            return m_extensionModules;
        }
    }

    public List<IOptimizeRuleModule> GetOptimizeRuleModules()
    {
        lock (m_packageCollectionLock)
        {
            return m_optimizeRuleModules;
        }
    }

    public List<IKpiCalculatorModule> GetKpiCalculatorModules()
    {
        lock (m_packageCollectionLock)
        {
            return m_kpiCalculatorModules;
        }
    }

    public List<IPermissionModule> GetPermissionModules()
    {
        lock (m_packageCollectionLock)
        {
            return m_permissionModules;
        }
    }

    public List<ILicenseValidationModule> GetLicenseValidationModules()
    {
        lock (m_packageCollectionLock)
        {
            return m_licenseValidationModules;
        }
    }

    public List<IPermissionValidationModule> GetPermissionValidationModules()
    {
        lock (m_packageCollectionLock)
        {
            return m_permissionValidationModules;
        }
    }

    public List<IAutomaticActionsModule> GetAutomaticActionsModules()
    {
        lock (m_packageCollectionLock)
        {
            return m_automaticActionsModules;
        }
    }

    public List<IBackgroundProcessModule> GetBackgroundProcessModules()
    {
        lock (m_packageCollectionLock)
        {
            return m_backgroundProcessModules;
        }
    }

    public List<IApiModule> GetApiModules()
    {
        lock (m_packageCollectionLock)
        {
            return m_apiModules;
        }
    }
    
    protected int GetPackageId(Assembly a_assembly)
    {
        object[] customAttributes = a_assembly.GetCustomAttributes(typeof(PT.Common.Attributes.Assembly.AssemblyPackageId), true);
        if (customAttributes == null || customAttributes.Length == 0 || !(customAttributes[0] is PT.Common.Attributes.Assembly.AssemblyPackageId packageIdAttr))
        {
            return -1;
        }

        return packageIdAttr.GetPackageId();
    }
}