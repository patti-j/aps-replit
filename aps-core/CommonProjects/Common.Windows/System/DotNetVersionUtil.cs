using System.Runtime.Versioning;

using Microsoft.Win32;

namespace PT.Common.Windows.System;

/// <summary>
/// Checks the registry to determine the installed .net version
/// </summary>
[SupportedOSPlatform("windows")]
public static class DotNetVersionUtil
{
    /// <summary>
    /// Note: Code from Microsoft
    /// </summary>
    /// <returns></returns>
    public static SoftwareVersion GetCore6PlusFromRegistry()
    {
        const string c_subkey = @"SOFTWARE\Microsoft\ASP.NET Core\Shared Framework\v6.0";

        try
        {
            SoftwareVersion highestVersion = new (0, 0, 0);
            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(c_subkey))
            {
                foreach (string subKeyName in ndpKey.GetSubKeyNames())
                {
                    string versionString = ndpKey.OpenSubKey(subKeyName).GetValue("Version").ToString();
                    string[] versionParts = versionString.Split('.');
                    if (versionParts.Length == 4)
                    {
                        SoftwareVersion version = new (Convert.ToInt32(versionParts[0]), Convert.ToInt32(versionParts[1]), Convert.ToInt32(versionParts[2]), Convert.ToInt32(versionParts[3]));
                        if (highestVersion < version)
                        {
                            highestVersion = version;
                        }
                    }
                }

                return highestVersion;
            }
        }
        catch
        {
            return new SoftwareVersion(0, 0, 0);
        }
    }

    public static SoftwareVersion GetCore8PlusFromRegistry()
    {  
        const string c_subkey = @"SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedhost";

        try
        {
            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(c_subkey))
            {
                string ver = ndpKey.GetValue("Version").ToString();
                string[] versionParts = ver.Split('.');
                if (versionParts.Length == 4)
                {
                    SoftwareVersion sVer = new(Convert.ToInt32(versionParts[0]), Convert.ToInt32(versionParts[1]), Convert.ToInt32(versionParts[2]), Convert.ToInt32(versionParts[3]));
                    return sVer;
                }
                else if (versionParts.Length == 3)
                {
                    SoftwareVersion sVer = new(Convert.ToInt32(versionParts[0]), Convert.ToInt32(versionParts[1]), Convert.ToInt32(versionParts[2]));
                    return sVer;
                }
                return new SoftwareVersion(0, 0, 0);
                
            }
        }
        catch
        {
            return new SoftwareVersion(0, 0, 0);
        }
    }
    
    public static SoftwareVersion GetCore8HostingBundleFromRegistry()
    {
        const string c_subkey = @"SOFTWARE\Microsoft\ASP.NET Core\Shared Framework\v8.0";

        try
        {
            SoftwareVersion highestVersion = new (0, 0, 0);
            using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(c_subkey))
            {
                foreach (string subKeyName in ndpKey.GetSubKeyNames())
                {
                    string versionString = ndpKey.OpenSubKey(subKeyName).GetValue("Version").ToString();
                    string[] versionParts = versionString.Split('.');
                    if (versionParts.Length == 4)
                    {
                        SoftwareVersion version = new (Convert.ToInt32(versionParts[0]), Convert.ToInt32(versionParts[1]), Convert.ToInt32(versionParts[2]), Convert.ToInt32(versionParts[3]));
                        if (highestVersion < version)
                        {
                            highestVersion = version;
                        }
                    }
                }

                return highestVersion;
            }
        }
        catch
        {
            return new SoftwareVersion(0, 0, 0);
        }
    }

    ///// <summary>
    ///// Returns a SoftwareVersion that represents the highest version of .net installed.
    ///// </summary>
    ///// <returns></returns>
    //public static SoftwareVersion GetDotNetVersion()
    //{
    //    int releaseKey = GetCore6PlusFromRegistry();

    //    if (releaseKey >= 528040)
    //    {
    //        return new SoftwareVersion(4, 8, 0);
    //    }
    //    if (releaseKey >= 461808)
    //    {
    //        return new SoftwareVersion(4, 7, 2);
    //    }
    //    if (releaseKey >= 461308)
    //    {
    //        return new SoftwareVersion(4, 7, 1);
    //    }
    //    if (releaseKey >= 460798)
    //    {
    //        return new SoftwareVersion(4, 7, 0);
    //    }
    //    if (releaseKey >= 394802)
    //    {
    //        return new SoftwareVersion(4, 6, 2);
    //    }
    //    if (releaseKey >= 394254)
    //    {
    //        return new SoftwareVersion(4, 6, 1);
    //    }
    //    if (releaseKey >= 393295)
    //    {
    //        return new SoftwareVersion(4, 6, 0);
    //    }
    //    if (releaseKey >= 379893)
    //    {
    //        return new SoftwareVersion(4, 5, 2);
    //    }
    //    if (releaseKey >= 378675)
    //    {
    //        return new SoftwareVersion(4, 5, 1);
    //    }
    //    if (releaseKey >= 378389)
    //    {
    //        return new SoftwareVersion(4, 5, 0);
    //    }
    //    // This code should never execute. A non-null release key should mean
    //    // that 4.5 or later is installed.
    //    return new SoftwareVersion(0, 0, 0);
    //}
}