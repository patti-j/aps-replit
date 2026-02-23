namespace PT.Common.Attributes.Assembly;

/// <summary>
/// Stores the software version
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class PackageFrameworkVersion : Attribute
{
    private readonly string m_version;

    public PackageFrameworkVersion(string a_versionNumber)
    {
        m_version = a_versionNumber;
    }

    public string GetVersion()
    {
        return m_version;
    }
}

/// <summary>
/// Stores the software version
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class TargetPackageFrameworkVersion : Attribute
{
    private readonly string m_version;

    public TargetPackageFrameworkVersion(string a_versionNumber)
    {
        m_version = a_versionNumber;
    }

    public string GetVersion()
    {
        return m_version;
    }
}

/// <summary>
/// Stores the name of the scheduling package.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class AssemblySchedulingPackage : Attribute
{
    private readonly string m_name;

    public AssemblySchedulingPackage(string a_schedulingPackageName)
    {
        m_name = a_schedulingPackageName;
    }

    public string GetSchedulingPackageName()
    {
        return m_name;
    }
}

/// <summary>
/// Stores the name of the scheduling package.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class AssemblyPackageId : Attribute
{
    private readonly int m_packageId;

    public AssemblyPackageId(int a_packageId)
    {
        m_packageId = a_packageId;
    }

    public int GetPackageId()
    {
        return m_packageId;
    }
}

/// <summary>
/// Stores the license exemption of the package.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public class RequiresLicense : Attribute
{
    private readonly bool m_requiresLicense;

    public RequiresLicense(bool a_requiresLicense)
    {
        m_requiresLicense = a_requiresLicense;
    }

    public bool GetRequiresLicense()
    {
        return m_requiresLicense;
    }
}