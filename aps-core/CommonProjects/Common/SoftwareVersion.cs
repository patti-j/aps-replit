namespace PT.Common;

/// <summary>
/// Stores version components relating to software version.
/// Format is Major.Minor.Hotfix.Revision
/// </summary>
public class SoftwareVersion : IComparable<SoftwareVersion>
{
    public int Major => m_major;
    private readonly int m_major;
    public int Minor => m_minor;
    private readonly int m_minor;
    public int Hotfix => m_hotfix;
    private readonly int m_hotfix;
    public int Revision => m_revision;
    private readonly int m_revision;
    public bool CompatibilityVersion => m_compatibilityVersion;
    private readonly bool m_compatibilityVersion;

    public SoftwareVersion()
    {
        m_major = 0;
        m_minor = 0;
        m_hotfix = 0;
        m_revision = 0;
    }

    public SoftwareVersion(Version a_systemVersion)
    {
        m_major = a_systemVersion.Major;
        m_compatibilityVersion = m_major >= 2010;
        m_minor = a_systemVersion.Minor;
        m_hotfix = a_systemVersion.Build;
        m_revision = a_systemVersion.Revision;
    }

    public SoftwareVersion(int a_major, int a_minor, int a_hotfix, int a_revision = 0)
    {
        m_major = a_major;
        m_compatibilityVersion = m_major >= 2010;
        m_minor = a_minor;
        m_hotfix = a_hotfix;
        m_revision = a_revision;
    }

    public SoftwareVersion(string a_version)
    {
        string[] components = a_version.Split('.');
        if (components.Length == 2)
        {
            m_major = int.Parse(components[0]);
            m_compatibilityVersion = m_major >= 2010;
            m_minor = int.Parse(components[1]);
        }
        else if (components.Length == 3)
        {
            m_major = int.Parse(components[0]);
            m_compatibilityVersion = m_major >= 2010;
            m_minor = int.Parse(components[1]);
            m_hotfix = int.Parse(components[2]);
        }
        else if (components.Length == 4)
        {
            m_major = int.Parse(components[0]);
            m_compatibilityVersion = m_major >= 2010;
            m_minor = int.Parse(components[1]);
            m_hotfix = int.Parse(components[2]);
            m_revision = int.Parse(components[3]);
        }
        else
        {
            throw new Exception("Error interpreting DisplayVersion");
        }
    }

    public override string ToString()
    {
        string components = m_major + "." + m_minor;

        // Omit all trailing 0s
        if (m_hotfix != 0 || m_revision != 0)
        {
            components += "." + m_hotfix;
        }

        if (m_revision != 0)
        {
            components += "." + m_revision;
        }

        return components;
    }

    public int CompareTo(SoftwareVersion a_otherVersion)
    {
        if (!MeetsMinimum(a_otherVersion))
        {
            return -1;
        }

        if (!a_otherVersion.MeetsMinimum(this))
        {
            return 1;
        }

        return 0;
    }

    public static bool operator >(SoftwareVersion a_version1, SoftwareVersion a_version2)
    {
        return a_version1.CompareTo(a_version2) > 0;
    }

    public static bool operator <(SoftwareVersion a_version1, SoftwareVersion a_version2)
    {
        return a_version1.CompareTo(a_version2) < 0;
    }

    /// <summary>
    /// Converts components to a System.Version for comparison
    /// </summary>
    /// <returns></returns>
    public Version ToSimpleVersion()
    {
        return new Version(m_major, m_minor, m_hotfix, m_revision);
    }

    /// <summary>
    /// Validate that the software version we are comparing to would not introduce serialization conflicts.
    /// </summary>
    /// <param name="a_versionToCompare"></param>
    /// <returns></returns>
    public bool ValidateSerialization(SoftwareVersion a_versionToCompare)
    {
        //Check if source version is a legacy version (major component version is greater than 2000)
        if (m_compatibilityVersion)
        {
            if (!a_versionToCompare.m_compatibilityVersion)
            {
                //If source software is legacy, any non legacy software is newer and would not introduce serialization conflicts.
                return true;
            }

            //If comparing a legacy software to another legacy software there would be no serialization conflicts if the comparison one is either newer or has at least
            //the same serialization version (hotfix component) if the major and minor components are equal.
            if (a_versionToCompare.m_major > m_major ||
                (a_versionToCompare.m_major == m_major && a_versionToCompare.m_minor > m_minor) ||
                (a_versionToCompare.m_major == m_major && a_versionToCompare.m_minor == m_minor && a_versionToCompare.m_hotfix >= m_hotfix))
            {
                return true;
            }
        }
        else
        {
            //If the source software is non legacy, any software that is either newer or has at least the same serialization version (Minor component) if the Major components are equal.
            //would not introduce serialization conflicts.
            if (!a_versionToCompare.m_compatibilityVersion && (a_versionToCompare.m_major > m_major || (a_versionToCompare.m_major == m_major && a_versionToCompare.m_minor >= m_minor)))
            {
                return true;
            }
        }

        return false;
    }

    public bool Equals(SoftwareVersion a_compareVersion)
    {
        return m_major == a_compareVersion.m_major && m_minor == a_compareVersion.m_minor && m_hotfix == a_compareVersion.m_hotfix && m_revision == a_compareVersion.m_revision;
    }

    public static int CompareVersionStringsInt(SoftwareVersion a_currentVersion, SoftwareVersion a_versionTocompare)
    {
        if (a_versionTocompare.MeetsMinimum(a_currentVersion))
        {
            return -1;
        }

        return 1;
    }

    public static int CompareVersionStringsInt(Tuple<int, SoftwareVersion, DateTime> a_currentVersion, Tuple<int, SoftwareVersion, DateTime> a_versionToCompare)
    {
        if (a_versionToCompare.Item2.MeetsMinimum(a_currentVersion.Item2))
        {
            return -1;
        }

        return 1;
    }

    /// <summary>
    /// Will return whether this version is greater than or equal to another SoftwareVersion
    /// </summary>
    public bool MeetsMinimum(SoftwareVersion a_minVersion)
    {
        if (m_compatibilityVersion && !a_minVersion.m_compatibilityVersion)
        {
            //this is an older versioning number. 
            return false;
        }

        if (!m_compatibilityVersion && a_minVersion.m_compatibilityVersion)
        {
            //the min version is an older versioning number
            return true;
        }

        //Check each component
        if (m_major > a_minVersion.m_major)
        {
            return true;
        }

        if (m_major < a_minVersion.m_major)
        {
            return false;
        }

        if (m_minor > a_minVersion.m_minor)
        {
            return true;
        }

        if (m_minor < a_minVersion.m_minor)
        {
            return false;
        }

        if (m_hotfix > a_minVersion.m_hotfix)
        {
            return true;
        }

        if (m_hotfix < a_minVersion.m_hotfix)
        {
            return false;
        }

        if (m_revision >= a_minVersion.m_revision)
        {
            return true;
        }

        return false;
    }

    public static bool IsValidVersion(string a_version)
    {
        if (string.IsNullOrEmpty(a_version))
        {
            return false;
        }

        string[] components = a_version.Split('.');
        if (components.Length == 2)
        {
            if (!int.TryParse(components[0], out int major))
            {
                return false;
            }

            if (!int.TryParse(components[1], out int minor))
            {
                return false;
            }

            return true;
        }

        if (components.Length == 3)
        {
            if (!int.TryParse(components[0], out int major))
            {
                return false;
            }

            if (!int.TryParse(components[1], out int minor))
            {
                return false;
            }

            if (!int.TryParse(components[2], out int hotfix))
            {
                return false;
            }

            return true;
        }

        if (components.Length == 4)
        {
            if (!int.TryParse(components[0], out int major))
            {
                return false;
            }

            if (!int.TryParse(components[1], out int minor))
            {
                return false;
            }

            if (!int.TryParse(components[2], out int hotfix))
            {
                return false;
            }

            if (!int.TryParse(components[3], out int revision))
            {
                return false;
            }

            return true;
        }

        return false;
    }
}