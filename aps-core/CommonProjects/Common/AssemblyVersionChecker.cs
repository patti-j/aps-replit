using System.Reflection;

namespace PT.Common;

/// <summary>
/// Names of PT DLLs.
/// </summary>
public class AssemblyVersionChecker
{
    /// <summary>
    /// Returns the assembly version of a specified file, or null if the file coculd not be loaded
    /// </summary>
    public static SoftwareVersion GetAssemblyVersion(string a_assemblyName)
    {
        try
        {
            Assembly a = Assembly.Load(a_assemblyName);
            return GetAssemblyVersion(a);
        }
        catch { }

        return null;
    }

    /// <summary>
    /// Returns the version of the current running assembly that immediately called this method.
    /// If you need to guarantee the assembly that acts as the entry point to the current process, use <see cref="GetEntryAssemblyVersionSafe"/>
    /// </summary>
    public static SoftwareVersion GetAssemblyVersion()
    {
        Assembly assembly = Assembly.GetCallingAssembly();
        return GetAssemblyVersion(assembly);
    }


    /// <summary>
    /// Returns the version of the current running assembly (ie, the one calling this method).
    /// NOTE: This differs from <see cref="GetAssemblyVersion()"/> in two meaningful ways:
    /// 1. It returns the version for the process calling this method (the EntryAssembly) rather than the immediate caller (CallingAssembly)
    /// 2. It handles situations where the entry assembly is a single-file deployment, which may not otherwise work.
    /// </summary>
    public static SoftwareVersion GetEntryAssemblyVersionSafe()
    {
        try
        {
            // This gets the path to the program executing this method (even if it's a single-file deployment, which notably Assembly.GetExecutingAssembly() doesn't work with).
            string executingAssemblyPath = Environment.ProcessPath;
            Version version = AssemblyName.GetAssemblyName(executingAssemblyPath).Version; // doesn't load assembly into memory
            if (version != null)
            {
                return new SoftwareVersion(version?.ToString());
            }
            else
            {
                return GetEntryAssemblyVersion();
            }
        }
        catch
        {
            // The above may fail in certain cases
            return GetEntryAssemblyVersion();
        }
    }

    private static SoftwareVersion GetEntryAssemblyVersion()
    {
        Version version = Assembly.GetEntryAssembly()?.GetName().Version;
        return new SoftwareVersion(version?.ToString());
    }

    /// <summary>
    /// Returns the version of the specified assembly or Version 0 if the version couldn't be determined
    /// </summary>
    public static SoftwareVersion GetAssemblyVersion(Assembly a_assembly)
    {
        try
        {
            AssemblyName assemblyName = a_assembly.GetName();
            return new SoftwareVersion(assemblyName.Version);
        }
        catch
        {
            //Version version = GetAssemblyVersion("PT.Common");
            //return version;
        }

        return new SoftwareVersion();
    }

    public static DateTime GetAssemblyBuildDateTime(Assembly a_assembly)
    {
        //try
        //{
        //    Type type = typeof(Attributes.Assembly.AssemblyBuildDateTime);
        //    object[] customAttributes = a_assembly.GetCustomAttributes(type, true);
        //    DateTime buildTime = (customAttributes[0] as Attributes.Assembly.AssemblyBuildDateTime).GetBuildDateTime();
        //    return buildTime;
        //}
        //catch
        //{
        //    //Version version = GetAssemblyVersion("PT.Common");
        //    //return version;
        //}

        //return PTDateTime.MaxDateTime;

        return GetLinkerTime(a_assembly);
    }

    public static bool ValidateBuildVersion(SoftwareVersion a_target, SoftwareVersion a_source)
    {
        //Possibly do extra comparisons based on hotfix version.
        return a_target.Equals(a_source);
    }

    private static DateTime GetLinkerTime(Assembly a_assembly, TimeZoneInfo a_target = null)
    {
        string filePath = a_assembly.Location;
        const int c_PeHeaderOffset = 60;
        const int c_LinkerTimestampOffset = 8;

        byte[] buffer = new byte[2048];

        using (FileStream stream = new (filePath, FileMode.Open, FileAccess.Read))
        {
            stream.Read(buffer, 0, 2048);
        }

        int offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
        int secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
        DateTime epoch = new (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        DateTime linkTimeUtc = epoch.AddSeconds(secondsSince1970);

        TimeZoneInfo tz = a_target ?? TimeZoneInfo.Local;
        DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

        return localTime;
    }
}