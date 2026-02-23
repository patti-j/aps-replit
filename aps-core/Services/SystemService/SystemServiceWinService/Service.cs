using System.ServiceProcess;

using Microsoft.Extensions.Configuration;

using PT.Common.Debugging;
using PT.Common.File;
using PT.Common.Http;
using PT.Common.Windows.File;
using PT.PlanetTogetherAPI;
using PT.PlanetTogetherAPI.Importing;
using PT.PlanetTogetherAPI.Server;
using PT.Scheduler;
using PT.Scheduler.Sessions;
using PT.SchedulerData;
using PT.ServerManagerAPIProxy;
using PT.ServerManagerSharedLib.Data;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.ServerManagerSharedLib.Helpers;
using PT.SystemServiceDefinitions;
using PT.SystemServiceProxy.APIClients;

using InstanceKey = PT.Common.Http.InstanceKey;

namespace PT.SystemServiceWinService;

public partial class Service : ServiceBase
{
    private const string c_startupLogFileName = "APS System STARTUP ERRORS.txt";
    private const int c_serviceAcceptPreShutdown = 0x100;
    private const int c_serviceControlPreshutdown = 0xf;

    //Import vars
    private readonly ImportUtilities m_importUtilities;

    /// <summary>
    /// variable to store the command line arguments. This service can be started by passing the parameters either as
    /// command line arguments or Start Parameter.
    /// </summary>
    private readonly string[] m_arguments;

    /// <summary>
    /// Other configuration outside of those stored in <see cref="m_arguments"/>. Since command line arguments are baked in at install,
    /// this provides another vehicle to configure startup.
    /// </summary>
    private IConfiguration m_configuration;

    private bool m_preShutDownSubscribed;
    private readonly bool m_isConfigMode;

    private DateTime m_startDate = DateTime.Now;

    private TimeSpan m_uptimeDuration => DateTime.Now - m_startDate;
    
    /// <summary>
    /// Instance settings at time of startup. These need to be fetched from the database during construction, and are members so that data can be accessed immediately after in OnStart.
    /// Later calls to the instance should get the latest value from the database (via InstanceSettingsManager);
    /// </summary>
    private readonly InstanceSettingsEntity m_instanceSettings;

    private readonly StartupValsAdapter m_startupVals;
    private readonly string m_thumbprint;

    public Service(string[] a_args)
    {
        InitializeComponent();
        m_arguments = a_args;
         AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

        if (Program.RunningAsConsole)
        {
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;
        }

        try
        {
            if (a_args.Length < 1)
            {
                CommonException exception = new ("The System Service Name must be specified as the first command line argument.");
                LogToStartupFolder(exception);
                throw exception;
            }

            PTHttpClient.RegisterOverrideThumbprint("*");

            ServiceName = a_args[0];
            InstanceKey instanceKey = new (ServiceName);
            SimpleExceptionEventLogger.RegisterEventSource(ServiceName);

            IInstanceSettingsManager instanceSettingsManager;
            try
            {
                LoadConfiguration(instanceKey);
                instanceSettingsManager = InstanceSettingManagerFactory.CreateInstanceSettingsManagerForInstance(m_configuration["InstanceDataConnectionString"], Environment.MachineName, m_configuration["InstanceIdentifier"], m_configuration["ApiKey"], m_configuration["WebAppEnv"]);

                // InstanceDataFolder must be obtained before other instance data, as their construction relies on the static Paths class.
                Paths.InstanceDataFolder = instanceSettingsManager.GetServerManagerFolder(instanceKey.InstanceName, instanceKey.SoftwareVersion);
                m_instanceSettings = instanceSettingsManager.GetInstanceSettingsEntity(instanceKey.InstanceName, instanceKey.SoftwareVersion);
                StartupVals startupVals = instanceSettingsManager.GetStartupVals(instanceKey.InstanceName, instanceKey.SoftwareVersion);
                m_startupVals = new StartupValsAdapter(startupVals, m_configuration["WebAppEnv"], m_configuration["InstanceDataConnectionString"], m_configuration["ApiKey"]);
                m_thumbprint = instanceSettingsManager.GetCertificateThumbprint(instanceKey.InstanceName, instanceKey.SoftwareVersion);
            }
            catch (Exception e)
            {
                CommonException exception = new ("Unable to get instance configuration: " + e);
                LogToStartupFolder(exception);
                throw exception;
            }

            //Setup import settings
            m_importUtilities = new ImportUtilities(ServiceName, m_instanceSettings, instanceSettingsManager);

            m_isConfigMode = m_arguments.Any(x => x.Contains("ConfigMode"));

            // Register for PRESHUTDOWN notification through reflection.
            //FieldInfo acceptedCommandsFieldInfo = typeof(ServiceBase).GetField("acceptedCommands", BindingFlags.Instance | BindingFlags.NonPublic);
            //if (acceptedCommandsFieldInfo == null)
            //{
            //    PT.Common.CommonException exception = new PT.Common.CommonException("4029");
            //    LogToStartupFolder(exception);
            //    m_preShutDownSubscribed = false;
            //}
            //else
            //{
            //    int value = (int)acceptedCommandsFieldInfo.GetValue(this);
            //    acceptedCommandsFieldInfo.SetValue(this, value | c_serviceAcceptPreShutdown);
            //    m_preShutDownSubscribed = true;
            //}

            CanHandlePowerEvent = false;
            CanPauseAndContinue = false;
            CanShutdown = true;
            CanStop = true;
            AutoLog = true;
        }
        catch (Exception e)
        {
            LogToStartupFolder(e);
            throw;
        }
    }

    /// <summary>
    /// Loads configuration from all registered sources into a single object.
    /// TODO: We could merge the current command line args into this object as well, to allow them to be passed from other sources -
    /// TODO: however, right now, our command line args are somewhat custom in their format, and the CommandLineArguementsHelper
    /// TODO: class doesn't have the same generic Lookup pattern that IConfiguration uses.
    /// TODO: If we ever want to pull traditional command line args from other sources, we can pipe them into this config (or vice versa)
    /// </summary>
    /// <param name="a_args"></param>
    /// <param name="a_instanceKey"></param>
    private void LoadConfiguration(InstanceKey a_instanceKey)
    {
        // TODO: If we change our instance file structure so that instances no longer have version-specific directories (see ARC-112), this will need to be updated.
        string instanceId = $"{a_instanceKey.InstanceName} {a_instanceKey.SoftwareVersion}";
        string configPathFromStartArgs = GetConfigPathFromStartArgs();
        string pathToConfigFile = configPathFromStartArgs ?? StartupInstanceSettingsHelper.GetPathToConfigFile(instanceId);

        m_configuration = new ConfigurationBuilder()
                          .SetBasePath(Path.GetDirectoryName(pathToConfigFile))
                          .AddJsonFile(Path.GetFileName(pathToConfigFile), optional: true, reloadOnChange: true)
                          //.AddCommandLine(a_args) // ConfigurationBuilder cares about order of sources - e.g. CommandLine values will overwrite any matching keys in the JsonFile.
                          .Build();
    }

    /// <summary>
    /// Load configuration from a specific source other than the standard one.
    /// In a development environment, the default is to use a configuration in the SolutionMiscFiles directory of the repo;
    /// a reasonable alternative would be to use \InstanceConfigPath:"C:\ProgramData\PlanetTogether\Debug 0.0\System", (adjusting for your setup) where production config would be.
    /// </summary>
    /// <returns></returns>
    private string GetConfigPathFromStartArgs()
    {
        string instanceConfigPathArg = m_arguments.FirstOrDefault(x => x.Contains("InstanceConfigPath"));

        if (!string.IsNullOrEmpty(instanceConfigPathArg) &&
            !instanceConfigPathArg.Contains(".json"))
        {
            DebugException.ThrowInDebug("The InstanceConfigPath arg should include the file name to load (default 'configuration.json').");
        }

        if (string.IsNullOrEmpty(instanceConfigPathArg))
        {
            // This is normal; can return null here and use StartupInstanceSettingsHelper.GetPathToConfigFile() 
            return null;
        }

        string instanceRootDirectoryPath = instanceConfigPathArg.Substring(instanceConfigPathArg.IndexOf(":") + 1);

        return instanceRootDirectoryPath;
    }

    private void ConsoleOnCancelKeyPress(object a_sender, ConsoleCancelEventArgs a_e)
    {
        Console.WriteLine("Logging out...");
        OnStop();
        Console.WriteLine("Shutting down...");
        Program.AllowClose();
    }

    public void CallOnStart(string[] a_args)
    {
        OnStart(a_args);
    }

    protected override void OnStart(string[] a_args)
    {
        OnStartHelper();
        m_startDate = DateTime.Now;
        SimpleExceptionEventLogger.LogStartMessage(ServiceName);
    }

    private string GetDevPackagesDirectory()
    {
        string devArgument = m_arguments.FirstOrDefault(x => x.Contains("LoadDevPackages"));
        if (string.IsNullOrEmpty(devArgument))
        {
            return string.Empty;
        }

        return devArgument.Substring(devArgument.IndexOf(":") + 1);
    }

    private ClientSession m_startAPSServer;

    private void OnStartHelper()
    {
        try
        {
            //Set dev package path.  this will be empty unless debugging
            string packagesDirectory = GetDevPackagesDirectory();
            m_startupVals.DevPackagePath = packagesDirectory;

            UriBuilder serverHostUri = new(m_startupVals.SystemServiceUrl);
            if (m_startupVals.DisableHttps)
            {
                serverHostUri.Scheme = "http";
            }

            //TimeSpan clientTimeout = TimeSpan.FromSeconds(m_startupVals.ClientTimeoutSeconds);
            SystemActionsClient selfClient = new (m_instanceSettings.Name, m_instanceSettings.Version, serverHostUri.ToString());

            //Set timezone.  Default to UTC
            TimeZoneInfo tzi = TimeZoneInfo.Utc;
            try
            {
                if (m_startupVals.ServerTimeZoneId != null)
                {
                    tzi = TimeZoneInfo.FindSystemTimeZoneById(m_startupVals.ServerTimeZoneId);
                }
            }
            catch
            {
                tzi = TimeZoneInfo.Utc;
            }

            TimeZoneAdjuster.SetTimeZoneInfo(tzi);

            IImportingService importingService = m_startupVals.NewImport 
                ? new NewImportingService(m_importUtilities) 
                : new ImportingService(m_importUtilities);
            
            m_startAPSServer = SystemController.StartAPSServer(selfClient, m_startupVals, c_startupLogFileName, importingService, new PublishHelper(m_instanceSettings, m_startupVals), new PTHttpServer(serverHostUri, m_instanceSettings.EnableDiagnostics, m_thumbprint, new StaticAuthorizer(), m_isConfigMode, null), m_isConfigMode);

            if (!m_isConfigMode)
            {
                SystemController.StartReceiving();
            }
        }
        catch (Exception e)
        {
            LogToStartupFolder(e);
            throw;
        }
    }

    protected override void OnStop()
    {
        Stop("Received STOP Command");
    }

    protected override void OnShutdown()
    {
        SimpleExceptionEventLogger.LogShutdownMessage(ServiceName, m_uptimeDuration);
        Stop("System Shutting Down");
    }

    private void Stop(string a_reason)
    {
        Task logOutTask = Task.Run(() => m_startAPSServer?.LogOff());
        logOutTask.Wait();
        SystemController.StopAPSService(a_reason);
        SimpleExceptionEventLogger.LogStopMessage(ServiceName, m_uptimeDuration, a_reason);
        ExitCode = 0;
    }

    /// <summary>
    /// call this when starting the program as a console rather than service
    /// </summary>
    /// <param name="a_args"></param>
    internal void OnStartConsoleHelper(string[] a_args)
    {
        OnStartHelper();
    }

    /// <summary>
    /// call this when stopping the program if it was started as a console.
    /// </summary>
    internal void OnStopConsoleHelper()
    {
        OnStop();
    }

    private static string GetUrl(string a_url)
    {
        UriBuilder uriBuilder = new (a_url) { Scheme = "https" };
        return uriBuilder.Uri.AbsoluteUri;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        LogToStartupFolder((Exception)e.ExceptionObject, false);
        SimpleExceptionEventLogger.LogUnhandledException(ServiceName, (Exception)e.ExceptionObject, m_uptimeDuration);
    }

    /// <summary>
    /// This can be used for logging before the working folder is setup for the regular logs.
    /// </summary>
    /// <param name="message"></param>
    private void LogToStartupFolder(Exception a_e, bool a_logToEventViewer = true)
    {
        SimpleExceptionLogger.LogExceptionToStartupFolder(c_startupLogFileName, a_e);

        if (a_logToEventViewer)
        {
            SimpleExceptionEventLogger.LogExceptionToEventLog(a_e, SimpleExceptionLogger.PTEventId.EXCEPTION);
        }
    }
}