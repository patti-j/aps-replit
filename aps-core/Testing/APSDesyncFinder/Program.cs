using PT.ServerManagerSharedLib.Data;
using PT.ServerManagerSharedLib.DTOs.Entities;

namespace APSDesyncFinder;

internal class Program
{
    internal static string s_clientExePath = @"C:\APSSet\APS\APS\APS\UserInterface\UI\bin\Debug\APSClient.exe";
    private static string[] m_args;

    /// <summary>
    /// Program
    /// </summary>
    /// <param name="args">
    /// InstanceName SoftwareVersion APSClientExePath
    /// Example: rel 0000.00.00.0 "C:\APSSet\APS\APS\APS\UserInterface\UI\bin\Debug\APSClient.exe"
    /// </param>
    private static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        if (args == null || args.Length != 3)
        {
            throw new ArgumentException("InstanceName, InstaceSoftwareVersion and path to APS Client executable are required.");
        }

        m_args = args;

        string serverName = args[0];
        string softwareVersion = args[1];
        string clientArgs = string.Format("\\InstanceName:{0} \\SoftwareVersion:{1} \\UserName:admin \\Password:chicago \\skipClientUpdater: \\EndQuickly: \\SkipLoginScreen:", serverName, softwareVersion);

        // TODO: If we start using this again, pull config like Service.cs does and use InstanceSettingsManagerFactory (or add new command line args above for InstanceId/WebAppEnv)
        IInstanceSettingsManager instanceSettingsManager = new DbInstanceSettingsManager(Environment.MachineName, null);
        StartupVals constructorVals = instanceSettingsManager.GetStartupVals(serverName, softwareVersion);
        constructorVals.MaxNbrSystemBackupsToStorePerSession = 0;
        constructorVals.StartType = StartTypeEnum.RecordingClientDelayed;
        Finder finder = new (constructorVals, args[2], clientArgs);
        finder.Start();
        Console.ReadLine();
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Console.WriteLine();
        Console.WriteLine(e.ToString());
    }

    internal static void Restart(int a_num)
    {
        System.Diagnostics.Process p = new ();
        p.StartInfo.FileName = System.Reflection.Assembly.GetEntryAssembly().Location;
        p.StartInfo.Arguments = string.Format("{0} {1} {2}", m_args[0], m_args[1], a_num);
        p.EnableRaisingEvents = true;
        p.Start();
        Environment.Exit(0);
    }
}