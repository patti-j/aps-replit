using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Configuration; //Needed for ConfigurationManager
using System.IO;            //Needed for ConfigurationManager

namespace PT.SystemServiceWinService;

internal static class Program
{
    // to run as console, change application type in project properties, then rename "Console" below to "DEBUG"
    #if Console
        public static void Main(string[] a_args)
        {
            Service svc = null;

            try
            {
                //---------------------- Test --------------------------//


                //------------------------------------------------------//
                svc = new Service(a_args);
                svc.OnStartConsoleHelper(a_args);

                Console.WriteLine("Press Enter to Exit");
                Console.ReadLine();
                Console.WriteLine("Exiting...");
            }
            finally
            {
                svc.OnStopConsoleHelper();
            }

            Console.WriteLine("Exit Complete");
        }
    #else
    //Properties needed to handle shutdown of console app
    public static bool RunningAsConsole;
    private static CancellationTokenSource m_tokenSource;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    private static void Main(string[] a_args)
    {
        //Only load dlls from base windows folder and the current directory.
        if (!SystemDefinitions.AuthenticodeVerifier.SetDefaultDllDirectories())
            throw new Win32Exception(Marshal.GetLastWin32Error());

        #if RELEASE
        if (bool.TryParse(ConfigurationManager.AppSettings["SecureLoad"], out bool secureParse) && secureParse)
        {
            SystemDefinitions.AuthenticodeVerifier.InitSecureLoad();
        }
        #endif

        RunningAsConsole = a_args.Select(x => x.ToLower()).Contains("console");
        if (!RunningAsConsole)
        {
            System.ServiceProcess.ServiceBase[] ServicesToRun;

            // More than one user Service may run within the same process. To add
            // another service to this process, change the following line to
            // create a second service object. For example,
            //
            //   ServicesToRun = new System.ServiceProcess.ServiceBase[] {new Service1(), new MySecondUserService()};
            //
            ServicesToRun = new System.ServiceProcess.ServiceBase[] { new Service(a_args) };
            System.ServiceProcess.ServiceBase.Run(ServicesToRun);
        }
        else
        {
            m_tokenSource = new CancellationTokenSource();
            Service instanceService = new (a_args);
            instanceService.CallOnStart(a_args);

            Task keepAlive = Task.Run(DebugHold, m_tokenSource.Token);
            Task.WaitAll(keepAlive);
        }
    }
    #endif

    /// <summary>
    /// Allows the console to continue and close the application
    /// </summary>
    public static void AllowClose()
    {
        m_tokenSource.Cancel();
    }

    /// <summary>
    /// Keep the app alive and don't close the console
    /// </summary>
    private static void DebugHold()
    {
        while (!m_tokenSource.Token.IsCancellationRequested)
        {
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    }
}