using PT.Common.Exceptions;
using PT.Common.File;

namespace PT.APSCommon;

/// <summary>
/// Names of PT DLLs.
/// </summary>
public class PTAssemblyVersionChecker : AssemblyVersionChecker
{
    public class PTAssemblyVersionCheckerException : PTException
    {
        public PTAssemblyVersionCheckerException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_stringParameters, a_appendHelpUrl) { }
    }

    //// Name of the DLL the scheduler code is in.
    //private const string c_schedulerDLL = "PT.Scheduler";

    //// These are DLLs required by the scheduler.
    //private static readonly string[] s_variousDLLAssemblies ={
    //				        "PT.APSCommon",
    //				        "PT.Broadcasting",
    //				        "PT.Common",
    //				        "PT.ComponentLibrary",
    //				        "PT.ConstantDefinitions",
    //				        "PT.CustomInterface",
    //				        "PT.Gantt",
    //				        "PT.GanttDotNet",
    //				        "PT.InterfaceDefinitions",
    //				        "PT.ERPLibrary",
    //				        "PT.ERPTransmissions",
    //				        "PT.Scheduler",
    //				        "PT.SchedulerDefinitions",
    //				        "ServerManagerProxyLib",
    //				        "PT.Transmissions",
    //				        "UIDefinitions"
    //			        };

    /// <summary>
    /// Get the version number of the server dll.
    /// </summary>
    /// <returns></returns>
    public static SoftwareVersion GetServerProductVersion()
    {
        SoftwareVersion version = GetAssemblyVersion(System.Reflection.Assembly.GetExecutingAssembly());

        if (version == null)
        {
            throw new PTAssemblyVersionCheckerException("2882");
        }

        return version;
    }

    public static DateTime GetServerBuildDate()
    {
        DateTime buildDate = GetAssemblyBuildDateTime(System.Reflection.Assembly.GetExecutingAssembly());

        if (buildDate == PTDateTime.MaxDateTime)
        {
            throw new PTAssemblyVersionCheckerException("2882");
        }

        return buildDate;
    }

    /// <summary>
    /// Validate the PlanetTogether components match the server DLL version number. An exception of type ProductVersionCheckException is thrown in the event the versions don't match.
    /// </summary>
    /// <param name="a_validateProductVersion">The version number of the server DLL.</param>
    public static void CheckAssemblies(SoftwareVersion a_validateProductVersion)
    {
        try
        {
            System.Reflection.Assembly currentAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            SoftwareVersion currentVersion = GetAssemblyVersion(currentAssembly);
            string assemblyName = currentAssembly.FullName;
            if (!ValidateBuildVersion(currentVersion, a_validateProductVersion))
            {
                SimpleExceptionLogger.LogException(SimpleExceptionLogger.STARTUP_ERROR_LOG_NAME, new PTAssemblyVersionCheckerException("2737", new object[] { a_validateProductVersion, assemblyName, currentVersion }, true), SimpleExceptionLogger.STARTUP_ERROR_TITLE);
                throw new PTAssemblyVersionCheckerException("4449", new object[] { "-50" });
            }
        }
        catch
        {
            SimpleExceptionLogger.LogException(SimpleExceptionLogger.STARTUP_ERROR_LOG_NAME, new PTAssemblyVersionCheckerException("2738", new object[] { a_validateProductVersion }), SimpleExceptionLogger.STARTUP_ERROR_TITLE);
            throw new PTAssemblyVersionCheckerException("4449", new object[] { "-55" });
        }
    }
}