using System.Diagnostics;

using PT.Scheduler;
using PT.ServerManagerSharedLib.Data;
using PT.ServerManagerSharedLib.DTOs.Entities;

namespace TestLogin;

internal class Program
{
    internal static string s_clientExePath = @"D:\APSSet\APS\APS\APS\UI\bin\Debug\APSClient.exe";
    private static string s_clientArgs;
    private static int s_numOfClientsToStart;
    private static bool s_playbackStarted = false;

    /// <summary>
    /// Two running modes determined by ClientStartPoint.
    /// No ClientStartPoint (2 args provided): Starts Server, runs recordings one by one, for each UserLoginT encountered in the recording a new client is launched.
    /// x as ClientStartPoint: Starts Server, runs the first x recordings, starts client and runs the rest. If no desyncs, it starts DesyncFinder with x+1 as ClientStartPoint.
    /// </summary>
    /// <param name="args">
    /// InstanceName SoftwareVersion StartClientRecordingNumber
    /// Example: rel 0000.00.00.0
    /// </param>
    private static void Main(string[] args)
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        if (args == null || args.Length != 2)
        {
            throw new ArgumentException("InstanceName, InstaceSoftwareVersion are required as arguments (in that order). Optionally, a third argument can specify the ClientStartPoint.");
        }

        s_clientArgs = string.Format("\\InstanceName:{0} \\SoftwareVersion:{1} \\UserName:test2 \\Password:chicago \\skipClientUpdater: \\EndQuickly: \\SkipLoginScreen:", args[0], args[1]);

        Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
        Trace.AutoFlush = true;
        Trace.Indent();

        Console.WriteLine("Starting Server");
        string serverName = args[0];
        string softwareVersion = args[1];

        // TODO: If we start using this again, pull config like Service.cs does and use InstanceSettingsManagerFactory (or add new command line args above for InstanceId/WebAppEnv)
        IInstanceSettingsManager instanceSettingsManager = new DbInstanceSettingsManager(Environment.MachineName, null);
        StartupVals constructorVals = instanceSettingsManager.GetStartupVals(serverName, softwareVersion);
        constructorVals.MaxNbrSystemBackupsToStorePerSession = 0;
        constructorVals.StartType = StartTypeEnum.Normal;
        SystemController.StartAPSService(constructorVals, "Errors.txt");
        Console.WriteLine("Server Started");

        Console.WriteLine("Starting Clients");

        StartClients(1, TimeSpan.FromSeconds(5));
        SendScenarioPublishes(TimeSpan.FromSeconds(8));

        Console.ReadLine();
    }

    private static void StartClients(int a_count, TimeSpan a_waitTime)
    {
        s_numOfClientsToStart = a_count;
        System.Timers.Timer timer = new (a_waitTime.TotalMilliseconds);
        timer.Elapsed += StartClientTimerElapsed;
        timer.AutoReset = false;
        timer.Start();
    }

    private static void StartClientTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        RunClient();
        s_numOfClientsToStart--;
        if (s_numOfClientsToStart > 0)
        {
            ((System.Timers.Timer)sender).Start();
        }

        //if (!s_playbackStarted)
        //{
        //    PT.Scheduler.SystemController.Broadcaster.PlayBack();
        //    s_playbackStarted = true;
        //}
    }

    private static void RunClient()
    {
        Process clientprocess = new ();
        clientprocess.StartInfo.FileName = s_clientExePath;
        clientprocess.StartInfo.Arguments = s_clientArgs;
        clientprocess.StartInfo.CreateNoWindow = false;
        clientprocess.EnableRaisingEvents = true;
        clientprocess.Start();
    }

    private static void SendScenarioPublishes(TimeSpan a_durationBetweenEachPublish)
    {
        System.Timers.Timer timer = new (a_durationBetweenEachPublish.TotalMilliseconds);
        timer.AutoReset = true;
        timer.Elapsed += SendScenarioPublishT;
        timer.Start();
    }

    private static void SendScenarioPublishT(object sender, System.Timers.ElapsedEventArgs e)
    {
        PT.Transmissions.ScenarioPublishT publishT = new ();

        ScenarioManager sm;
        while (true)
        {
            try
            {
                using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, AutoExiter.THREAD_TRY_WAIT_MS))
                {
                    publishT.LastTransmissionNbr = sm.LastTransmissionNbr;
                }

                break;
            }
            catch (AutoTryEnterException exception) { }
        }

        SystemController.Broadcaster.Broadcast(PT.Transmissions.Transmission.Compress(publishT), SystemController.Receiver.ConnectionNbr);
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Console.WriteLine();
        Console.WriteLine(e.ToString());
    }
}