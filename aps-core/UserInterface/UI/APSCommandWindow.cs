using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using PT.APSCommon;
using PT.APSCommon.Windows.CommandWindow;
using PT.Common.Http;
using PT.Scheduler;
using PT.SchedulerDefinitions;
using PT.Transmissions;
using PT.Transmissions.Interfaces;

namespace PT.UI;

public partial class APSCommandWindow : Form
{
    private readonly IClientSession m_clientSession;
    private readonly CommandPrompt m_commandPrompt = new ();

    public APSCommandWindow(IClientSession a_clientSession)
        : this("", true, a_clientSession) { }

    public APSCommandWindow(bool a_topmost, IClientSession a_clientSession)
        : this("", a_topmost, a_clientSession) { }

    public APSCommandWindow(string a_command, bool a_topmost, IClientSession a_clientSession)
    {
        m_clientSession = a_clientSession;

        InitializeComponent();
        m_commandPrompt.Location = new Point(0, 0);
        m_commandPrompt.Dock = DockStyle.Fill;
        m_commandPrompt.AcceptsReturn = true;
        m_commandPrompt.AcceptsTab = true;
        m_commandPrompt.ScrollBars = ScrollBars.Both;
        m_commandPrompt.Multiline = true;
        m_commandPrompt.BackColor = Color.Black;
        m_commandPrompt.ForeColor = Color.FromArgb(192, 192, 192);
        m_commandPrompt.Font = new Font("Lucida Console", 12f);
        TopMost = a_topmost;

        Controls.Add(m_commandPrompt);
        m_commandPrompt.m_commandEvent += new CommandPrompt.CommandEnteredEvent(CommandEvent);

        if (!string.IsNullOrEmpty(a_command))
        {
            ExecuteCommand(a_command);
        }
    }

    protected void ExecuteCommand(string a_command)
    {
        m_commandPrompt.ReplaceAndExecuteCommand(a_command);
    }

    #if DEBUG
    private bool m_debugOn = true;
    #else
        bool m_debugOn = false;
    #endif

    protected bool Stopped { get; private set; }

    private const string c_FindSubComponents = "FindSubComponents";

    private void CommandEvent(string a_command)
    {
        if (a_command == null)
        {
            return;
        }

        string command = a_command.Trim();

        if (command == "")
        {
            return;
        }

        if (StringHelpers.StringEqualsAnyOf(command, "Clear", "CLS", "c"))
        {
            m_commandPrompt.Clear();
        }
        else if (StringHelpers.StringEqualsAnyOf(command, "Exit", "x", "quit", "q"))
        {
            Exit();
        }
        else if (StringHelpers.StringEqualsAnyOf(command, "Kill", "k"))
        {
            Stopped = true;
            Exit();
        }
        else if (StringHelpers.StringEqualsAnyOf(command, "RestartNormal", "rn"))
        {
            RestartNormal();
        }
        else if (StringHelpers.StringEqualsAnyOf(command, "HELP", "?"))
        {
            m_commandPrompt.Write(Help);
        }
        else if (StringHelpers.StringEqualsAnyOf(command, "Connection", "con"))
        {
            Connection();
        }
        else if (StringHelpers.StringEqualsAnyOf(command, "SimulationTiming", "st"))
        {
            if (!PlayingBack())
            {
                SimulationTiming();
            }
            else
            {
                RecordingTiming();
            }
        }
        else if (StringHelpers.StringEqualsAnyOf(command, "TransmissionTiming", "tt"))
        {
            if (!PlayingBack())
            {
                TranmissionTiming();
            }
            else
            {
                RecordingTiming();
            }
        }
        else if (StringHelpers.StringEqualsAnyOf(command, "RecordingTiming", "rt"))
        {
            RecordingTiming();
        }
        else if (StringHelpers.StringEqualsAnyOf(command, "DebugOn", "de", "do"))
        {
            m_debugOn = true;
        }
        else if (m_debugOn)
        {
            if (StringHelpers.StringEqualsAnyOf(command, "Summary", "smy"))
            {
                if (!PlayingBack())
                {
                    Summary();
                }
                else
                {
                    RecordingTiming();
                }
            }
            else if (StringHelpers.StringEqualsAnyOf(command, "PlayBack", "pb"))
            {
                if (!PlayingBack())
                {
                    PlayBack(TriggerRecordingPlaybackT.EPlayBackModes.Full);
                }
                else
                {
                    RecordingTiming();
                }
            }
            else if (StringHelpers.StringEqualsAnyOf(command, "PlayBackUntilSimulation", "pbs"))
            {
                PlayBack(TriggerRecordingPlaybackT.EPlayBackModes.SingleSimulation);
            }
            else if (StringHelpers.StringEqualsAnyOf(command, "PlayBackUntilLogin", "pbl"))
            {
                PlayBack(TriggerRecordingPlaybackT.EPlayBackModes.Login);
            }
            else if (StringHelpers.StringEqualsAnyOf(command, "PlayBackSkipLogin", "pbsl"))
            {
                PlayBack(TriggerRecordingPlaybackT.EPlayBackModes.SkipLogin);
            }
            else if (StringHelpers.StringEqualsAnyOf(command, "PlayBackAndExit", "pbe"))
            {
                if (!PlayingBack())
                {
                    WriteLine("Starting Playback and exit....");
                    PlayBack(TriggerRecordingPlaybackT.EPlayBackModes.Full);
                    Thread.Sleep(2000);
                    WriteLine("Awake...");
                    Exit();
                }
                else
                {
                    RecordingTiming();
                }
            }
            else if (StringHelpers.StringEqualsAnyOf(command, "SetOnline", "online"))
            {
                SendSetOnline();
            }
            else if (StringHelpers.StringEqualsAnyOf(command, "SetOffline", "offline"))
            {
                SendSetOffline();
            }
            else if (StringHelpers.StringEqualsAnyOf(command, "Lots"))
            {
                Lots();
            }
            else if (StringHelpers.ContainsAnyString(command, c_FindSubComponents))
            {
                string jobName = command.Substring(c_FindSubComponents.Length);
                jobName = jobName.Trim();
                FindSubcomponentJobs(jobName);
            }
            else if (command.ToLower().StartsWith(c_setNumberOfActivities))
            {
                // [BATCH_CODE]
                string nbrOfActivitiesString;

                try
                {
                    nbrOfActivitiesString = command.Substring(c_setNumberOfActivities.Length);
                }
                catch
                {
                    m_commandPrompt.Write("You must enter a number.");
                    return;
                }

                int nbrOfActivities;

                try
                {
                    nbrOfActivities = int.Parse(nbrOfActivitiesString);
                }
                catch (Exception)
                {
                    m_commandPrompt.Write("The number was unparsable.");
                    return;
                }

                if (nbrOfActivities < 1)
                {
                    m_commandPrompt.Write("Must be greater than 0.");
                    return;
                }

                BatchTesting.s_numberOfActivitiesToIncludeInBatch = nbrOfActivities;
            }
            else if (string.Compare(command, "set batch join true", true) == 0)
            {
                // [BATCH_CODE]
                BatchTesting.s_joinWithBatchDroppedOntoIfPossible = true;
            }
            else if (string.Compare(command, "set batch join false", true) == 0)
            {
                // [BATCH_CODE]
                BatchTesting.s_joinWithBatchDroppedOntoIfPossible = false;
            }
            else if (string.Compare(command, "show move values", true) == 0)
            {
                // [BATCH_CODE]
                m_commandPrompt.Write("Number of activities = " + BatchTesting.s_numberOfActivitiesToIncludeInBatch);
                m_commandPrompt.Write(Environment.NewLine);
                m_commandPrompt.Write("Batch join = " + BatchTesting.s_joinWithBatchDroppedOntoIfPossible);
            }
            else if (string.Compare(command, "res notes as volume true", true) == 0)
            {
                BatchTesting.s_parseResourceNotesAsVolume = true;
            }
            else if (string.Compare(command, "res notes as volume false", true) == 0)
            {
                BatchTesting.s_parseResourceNotesAsVolume = false;
            }
            else if (command.StartsWith("PathLock", true, System.Globalization.CultureInfo.CurrentCulture))
            {
                string[] split = command.Split(' ');
                if (split.Length != 5)
                {
                    m_commandPrompt.Write(string.Format("Need 5 arguments. Only {0} were provided.", split.Length));
                }
                else
                {
                    string jobName = split[1];
                    string moName = split[2];
                    string pathName = split[3];
                    bool lockUnlockPath = bool.Parse(split[4]);
                    SendLockAlternatePath(jobName, moName, pathName, lockUnlockPath);
                }
            }
            else if (command.StartsWith("MRPOptimize", true, System.Globalization.CultureInfo.CurrentCulture))
            {
                SendOptimize(false);
            }
            else if (command.StartsWith("Optimize", true, System.Globalization.CultureInfo.CurrentCulture))
            {
                SendOptimize(false);
            }
            else if (command.StartsWith("Touch", true, System.Globalization.CultureInfo.CurrentCulture))
            {
                SendTouch();
            }
            else if (string.Equals(command, c_mrp_cmd, StringComparison.CurrentCultureIgnoreCase))
            {
                SendMRPOptimize();
            }
            else if (StringHelpers.StringEqualsAnyOf(command, "progress", "prog"))
            {
                ScenarioManager sm;
                using (SystemController.Sys.ScenariosLock.EnterRead(out sm))
                {
                    Scenario s = sm.GetByIndex(0);
                    ScenarioEvents se;
                    using (s.ScenarioEventsLock.EnterWrite(out se))
                    {
                        se.SimulationProgressEvent += new ScenarioEvents.SimulationProgressDelegate(se_SimulationProgressEvent);
                    }
                }
            }
            else if (command.StartsWith("ck", true, System.Globalization.CultureInfo.CurrentCulture))
            {
                //Create a DataModelActivationKey
                ScenarioManager sm;
                using (SystemController.Sys.ScenariosLock.EnterRead(out sm))
                {
                    Scenario s = sm.GetByIndex(0);
                    using (s.ScenarioDetailLock.EnterRead(out ScenarioDetail sd))
                    {
                        string key = PTSystem.CreateActivationKey(sd);
                        string keyString = $"dmakey|{key}|";
                        m_commandPrompt.Write(keyString);
                        Clipboard.SetText(keyString);
                    }
                }
            }
            else if (command.StartsWith("vk", true, System.Globalization.CultureInfo.CurrentCulture))
            {
                string[] split = command.Split(' ');
                if (split.Length != 2)
                {
                    m_commandPrompt.Write("There should only be 1 argument");
                    return;
                }

                string keyText = split[1];
                keyText = LicenseKey.ParseDMAElement(keyText);
                m_commandPrompt.Write(SystemController.Sys.ValidateKey(keyText).ToString());
            }
            else if (command.StartsWith("save", true, System.Globalization.CultureInfo.CurrentCulture))
            {
                if (!PTSystem.Server)
                {
                    m_commandPrompt.Write("You can not save while running externally.");
                    return;
                }

                SaveScenario();
            }
            else if (command.StartsWith("Unschedule", true, System.Globalization.CultureInfo.CurrentCulture))
            {
                SendUnschedule();
            }

            else if (StringHelpers.StringEqualsAnyOf(command, "JobList", "jl"))
            {
                DisplayJobs();
            }
            else if (StringHelpers.StringEqualsAnyOf(command, "OperationList", "ol"))
            {
                DisplayOperations();
            }
            //else if (PT.Common.StringHelpers.ContainsAnyString(command, "ScenarioChecksum", "checksum"))
            //{
            //    ChecksumCommandHandler();
            //}
            else if (StringHelpers.StringEqualsAnyOf(command, "export", "publish"))
            {
                ExportCommandHandler();
            }
            else if (AddOnCommands(command))
            {
                // handled
            }
            else
            {
                if (a_command.Length > 0)
                {
                    m_commandPrompt.Write(string.Format("'{0}' is not recognized as a command or was entered incorrectly.", a_command));
                }
            }
        }
        else
        {
            m_commandPrompt.Write(string.Format("'{0}' is not recognized as a command or was entered incorrectly.", a_command));
        }
    }

    private void SaveScenario()
    {
        try
        {
            bool content = m_clientSession.MakePostRequest<BoolResponse>("SaveScenarioToDisk", null, "api/SystemService/").Content;
            if (content)
            {
                m_commandPrompt.Write("Saved!");
            }
            else
            {
                m_commandPrompt.Write("Failed to save scenario");
            }
        }
        catch (Exception e)
        {
            m_commandPrompt.Write("Error saving scenario");
        }
    }

    private void RestartNormal()
    {
        Process process = Process.GetCurrentProcess();
        ProcessStartInfo startInfo = Process.GetCurrentProcess().StartInfo;

        IEnumerable<string> args = Environment.GetCommandLineArgs().Skip(1);
        string newArgs = string.Concat(string.Join(" ", args.Where(x => !x.Contains(@"\StartType:")).Select(x => @"""" + x + @"""")), " \"\\StartType:Normal\" ");
        startInfo.Arguments = newArgs;
        startInfo.FileName = Application.ExecutablePath;

        while (true)
        {
            bool exit = false;

            bool allScenariosUnlocked = true;
            ScenarioManager sm;
            using (SystemController.Sys.ScenariosLock.EnterRead(out sm))
            {
                Scenario scn;
                for (int scnI = 0; scnI < sm.LoadedScenarioCount; ++scnI)
                {
                    scn = sm.GetByIndex(scnI);
                    ScenarioDetail sd = null;
                    bool locked = false;

                    while (!locked)
                    {
                        try
                        {
                            using (scn.ScenarioDetailLock.TryEnterRead(out sd, 10))
                            {
                                locked = true;
                            }
                        }
                        catch (AutoTryEnterException)
                        {
                            allScenariosUnlocked = false;
                            break;
                        }
                    }

                    if (!allScenariosUnlocked)
                    {
                        break;
                    }
                }

                if (allScenariosUnlocked)
                {
                    exit = true;
                }
            }

            if (exit)
            {
                break;
            }
        }

        Exit();

        Process.Start(startInfo);
    }

    //void ChecksumCommandHandler()
    //{
    //    ScenarioChecksumT checksumT;
    //    ScenarioManager sm;
    //    using (SystemController.Sys.ScenariosLock.EnterRead(out sm))
    //    {
    //        checksumT = new ScenarioChecksumT(sm.LiveScenarioId, true);
    //    }
    //    checksumT.sequenced = false;
    //    SendTransmission(checksumT);
    //}

    private void ExportCommandHandler()
    {
        ScenarioDetailExportT exportT;
        ScenarioManager sm;
        using (SystemController.Sys.ScenariosLock.EnterRead(out sm))
        {
            exportT = new ScenarioDetailExportT(sm.GetFirstProductionScenario().Id, EExportDestinations.ToDatabase);
        }

        SendTransmission(exportT);
    }

    private void SendTransmission(ScenarioBaseT a_t)
    {
        int connectionNbr = -1;
        long creationTicks = -1;
        m_clientSession.SendClientAction(a_t);
    }

    private void se_SimulationProgressEvent(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_type, ScenarioBaseT a_t, long a_simNbr, decimal a_percentComplete, SimulationProgress.Status a_status)
    {
        string s = string.Format("{0}; {1}; {2}; SimNbr={3} ", DateTime.Now.ToShortTimeString(), a_percentComplete * 100, a_status, a_simNbr);
        WriteLine_Invoke(s, true);
    }

    private void Exit()
    {
        Console.WriteLine("Exiting...");
        Close();
    }

    public virtual bool AddOnCommands(string a_command)
    {
        return false;
    }

    private const string c_setNumberOfActivities = "set number of activities";

    /// <summary>
    /// Allows overridden instances to indicate that the recording is still being played back so some commands shouldn't be allowed to run.
    /// </summary>
    protected virtual bool PlayingBack()
    {
        return false;
    }

    public virtual string Help
    {
        get
        {
            // [BATCH_CODE]
            string help =
                @"***GENERAL***
Exit, quit, x, q:   Exit the command interpreter.
Kill or k:          Kill the command interpreter (data won't be saved if the server is running in the command interpreter).
Clear, Cls or c:    Clears the screen.
help or ?:          This help text.

***TEST***
Connection or con: Information about connection to server.";

            if (m_debugOn)
            {
                help += @"

---------------------------------------------------
***Commands below are only displayed in DEBUG mode.
---------------------------------------------------

***DEBUG***
Summary or smy: High level view of system and details of the Live Scenario.

SimulationTiming or st: Display timing results of the last few simulations in the *1st scenario*. This works in non debug mode too but isn't documented there since it's not for customer use and is unsupported. Multiple values may be reported per action. For instance an Optimize will report the Simulation function time as well as the total Optimize function time. A ""."" is used to offset nested functionality such as "".Simulate"" within a ""Move"". The simulation number follows each timing result so you can tell which lines are part of the same action.

TransmissionTiming or tt: Display how long the last few tranmissions took in the *1st scenario*. This works in non debug mode too but isn't documented there since it's not for customer use and is unsupported.

***Play Back***
PlayBack or pb:              Start playback of the recording.
PlayBackSimulation or pbs:   Playback recordings until a ScenarioBaseT or ERPTransmission os processed.
PlayBackAndExit or pbe:      Start playback of a recording and exit when the playback has completed.
Progress or prog:            Show progress during any simulation in percent of activities scheduled.

*** Optimize Commands ***
Optimize: Optimize the first scenario.
Touch: Touch all the scenarios.
MRP: MRP Optimize the first scenario. RunMrpDuringOptimizations, RegenerateJobs, RegeneratePurchaseOrders.

*** Reports***
Lots: Display a list of lots, their quantities, and their unallocated quantities.

***AlternatePath***
PathLock JobName MoName [PathName]: if PathName isn't set, the DefaultPath will be cleared.

***BATCH*** this commands will be deleted soon since batch functionality is available in the user interface.
set number of activities #: Set the maximum number of activities to include
                            in a move.
set batch join true:        If possible, join with the batch being dropped
                            onto. Start time and resource requirements might be adjusted.
set batch join false:       Normal move. Rebatching may or may not occur.
show move values:           Show settings related to moving batches.
res notes as volume true    Parse resource notes as volume. When a resource change transmissions occurs change it to treat the notes as a number the volume should be set to.
res notes as volume false   Don't modify the ResourceChangeTransmission.
";
            }

            return help;
        }
    }

    public delegate void SendDelegate(PTTransmission a_t);

    public event SendDelegate SendEvent;

    public void Send(PTTransmission a_t)
    {
        if (SendEvent != null)
        {
            SendEvent(a_t);
        }
        //else if (SystemController.ServerSessionManager != null)
        //{
        //    SystemController.ServerSessionManager.TransmissionReceived
        //}
    }

    protected void PlayBack(TriggerRecordingPlaybackT.EPlayBackModes a_mode)
    {
        Cursor.Current = Cursors.WaitCursor;
        TriggerRecordingPlaybackT t = new ();
        t.PlayBackMode = a_mode;
        Send(t);
    }

    internal void Summary()
    {
        ScenarioManager sm;
        string summary = "Error";

        using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 1000))
        {
            summary = sm.GetSummary();
        }


        m_commandPrompt.Write(summary);
    }

    internal void Connection()
    {
        m_commandPrompt.Write(SystemController.GetServerConnectionDescription());
    }

    internal void SimulationTiming()
    {
        ScenarioManager sm;
        using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 1000))
        {
            Scenario s = sm.GetByIndex(0);
            ScenarioDetail sd;
            WriteLine("TimeSpan: Simulation Number; Measured Function; ");
            using (s.ScenarioDetailLock.EnterWrite(out sd))
            {
                Common.Collections.CircularQueue<ScenarioDetail.SimulationTimingMeasurement> timingQueue = sd.GetCopyOfSimulationTimingQueue();
                if (timingQueue.Count > 0)
                {
                    WriteSeperatorLine();
                }

                while (timingQueue.Count > 0)
                {
                    ScenarioDetail.SimulationTimingMeasurement timing = timingQueue.Dequeue();
                    WriteLine(timing.Timing + ": " + timing.Timing.Name);
                    if (timing.EndOfTimingBlock)
                    {
                        WriteSeperatorLine();
                    }
                }
            }
        }
    }

    private void FindSubcomponentJobs(string a_jobName)
    {
        ScenarioManager sm;
        using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 1000))
        {
            Scenario s = sm.GetByIndex(0);
            ScenarioDetail sd;
            using (s.ScenarioDetailLock.EnterWrite(out sd))
            {
                Job j = sd.JobManager.GetByName(a_jobName);
                if (j != null)
                {
                    List<JobManager.SubComponentSource> sources = sd.JobManager.FindAllSubComponents(j);
                }
                else
                {
                    WriteLine(string.Format("Job {0} not found.", a_jobName));
                }
            }
        }
    }

    internal void TranmissionTiming()
    {
        ScenarioManager sm;
        using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 1000))
        {
            Scenario s = sm.GetByIndex(0);
            ScenarioDetail sd;
            WriteLine("TimeSpan: TranmissionNumber: Tranmission");

            using (s.ScenarioDetailLock.EnterWrite(out sd))
            {
                Common.Collections.CircularQueue<Common.Testing.Timing> timingQueue = sd.GetCopyOfTransmissionTimingQueue();
                if (timingQueue.Count > 0)
                {
                    WriteSeperatorLine();
                }

                while (timingQueue.Count > 0)
                {
                    Common.Testing.Timing timing = timingQueue.Dequeue();
                    WriteLine(timing + ": " + timing.Name);
                }
            }
        }
    }

    protected virtual void RecordingTiming()
    {
        WriteLine("The RecordingTiming function hasn't been overridden.");
    }

    private void WriteSeperatorLine()
    {
        WriteLine("----------------------------------------");
    }

    private void WriteSubSeperator()
    {
        WriteLine("  --------------------------------------");
    }

    public void SendOptimize(bool a_mrpOptimize)
    {
        try
        {
            List<ScenarioDetailOptimizeT> optimizeList = new ();

            ScenarioManager sm;
            using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 1000))
            {
                for (int smI = 0; smI < sm.LoadedScenarioCount; smI++)
                {
                    Scenario s = sm.GetByIndex(smI);
                    ScenarioDetail sd;
                    using (s.ScenarioDetailLock.TryEnterRead(out sd, 1000))
                    {
                        ScenarioDetailOptimizeT optimizeT = new (s.Id, null, a_mrpOptimize);
                        optimizeList.Add(optimizeT);
                    }
                }
            }

            foreach (ScenarioDetailOptimizeT optimizeT in optimizeList)
            {
                SendTransmission(optimizeT);
            }
        }
        catch (AutoTryEnterException)
        {
            WriteBusyMessage();
        }
    }

    public void Lots()
    {
        try
        {
            ScenarioManager sm;
            using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 1000))
            {
                Scenario s = sm.GetByIndex(0);
                ScenarioDetail sd;
                using (s.ScenarioDetailLock.TryEnterRead(out sd, 1000))
                {
                    for (int pI = 0; pI < sd.PlantManager.Count; ++pI)
                    {
                        for (int wI = 0; wI < sd.PlantManager[pI].WarehouseCount; ++wI)
                        {
                            Warehouse wh = sd.PlantManager[pI].GetWarehouseAtIndex(wI);
                            for (int iI = 0; iI < wh.Inventories.Count; ++iI)
                            {
                                Inventory inv = wh.Inventories.GetByIndex(iI);

                                IEnumerator<Lot> lotEtr = inv.Lots.GetEnumerator();
                                while (lotEtr.MoveNext())
                                {
                                    Lot lot = lotEtr.Current;
                                    string desc = $"Lot: {lot}; Qty={lot.Qty};";
                                    WriteLine(desc);
                                }
                                
                            }
                        }
                    }
                }
            }
        }
        catch (AutoTryEnterException)
        {
            WriteBusyMessage();
        }
    }

    public void SendTouch()
    {
        ScenarioTouchT tt = new ();
        SendTransmission(tt);
    }

    private const string c_mrp_cmd = "mrp";

    public void SendMRPOptimize()
    {
        try
        {
            ScenarioManager sm;
            using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 1000))
            {
                Scenario s = sm.GetByIndex(0);
                ScenarioDetail sd;
                using (s.ScenarioDetailLock.TryEnterRead(out sd, 1000))
                {
                    //OptimizeSettings mrpSettings = sd.OptimizeSettings.Clone();
                    //mrpSettings.RunMrpDuringOptimizations = true;
                    //mrpSettings.RegenerateJobs = true;
                    //mrpSettings.RegeneratePurchaseOrders = true;
                    //mrpSettings.PreserveFirmJobs = true;
                    //mrpSettings.PreserveReleasedJobs = true;

                    ScenarioDetailOptimizeT optimizeT = new (s.Id, null, true);
                    SendTransmission(optimizeT);
                }
            }
        }
        catch (AutoTryEnterException err)
        {
            WriteLine_Invoke("Either ScenarioManager or ScenarioDetail were locked. Try again\n" + err.Message, true);
        }
    }

    public void DisplayJobs()
    {
        ScenarioManager sm;
        using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 1000))
        {
            Scenario s = sm.GetByIndex(0);
            ScenarioDetail sd;

            using (s.ScenarioDetailLock.EnterWrite(out sd))
            {
                for (int i = 0; i < sd.JobManager.Count; i++)
                {
                    Job job = sd.JobManager[i];
                    WriteLine($"Job Id: {job.Id}" + Environment.NewLine + $"Job Name: {job.Name}" + Environment.NewLine + $"{"Scheduled: ",-11}" + $"Start  {job.ScheduledStartDate}" + Environment.NewLine + $"{" ",-11}" + $"  End  {job.ScheduledEndDate}");
                    WriteSeperatorLine();
                }
            }
        }
    }

    public void DisplayOperations()
    {
        ScenarioManager sm;
        using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 1000))
        {
            Scenario s = sm.GetByIndex(0);
            ScenarioDetail sd;
            using (s.ScenarioDetailLock.EnterWrite(out sd))
            {
                for (int i = 0; i < sd.JobManager.Count; i++)
                {
                    Job job = sd.JobManager[i];
                    WriteLine($"Job Id: {job.Id}" + Environment.NewLine + $"Job Name: {job.Name}");
                    WriteSubSeperator();
                    foreach (BaseOperation op in job.GetOperations())
                    {
                        WriteLine($"  Op Name: {op.Name}     Op Id: {op.Id}" + Environment.NewLine + $"    Scheduled: Start  {op.ScheduledStartDate}" + Environment.NewLine + $"{" ",-17}" + $"End  {op.ScheduledEndDate}");
                        if (op is InternalOperation)
                        {
                            InternalOperation iOp = (InternalOperation)op;
                            int actCount = iOp.Activities.Count;
                            if (actCount > 0)
                            {
                                string actLabel = "    Activity Id";
                                actLabel = actCount > 1 ? string.Concat(actLabel, "s: ") : string.Concat(actLabel, ": ");
                                m_commandPrompt.Write(actLabel);
                                for (int actI = 0; actI < iOp.Activities.Count; actI++)
                                {
                                    InternalActivity activity = iOp.Activities.GetByIndex(actI);
                                    if (actI == 0)
                                    {
                                        m_commandPrompt.Write($"{activity.Id}" + Environment.NewLine);
                                    }
                                    else
                                    {
                                        WriteLine($"{" ",-18}" + activity.Id);
                                    }
                                }
                            }
                        }

                        WriteSubSeperator();
                    }

                    WriteSeperatorLine();
                }
            }
        }
    }

    public void SendUnschedule()
    {
        try
        {
            ScenarioManager sm;
            using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 1000))
            {
                for (int smI = 0; smI < sm.LoadedScenarioCount; smI++)
                {
                    Scenario s = sm.GetByIndex(smI);
                    ScenarioDetail sd;
                    using (s.ScenarioDetailLock.TryEnterRead(out sd, 1000))
                    {
                        BaseIdList jobKeyList = new ();

                        for (int i = 0; i < sd.JobManager.Count; i++)
                        {
                            Job job = sd.JobManager[i];
                            jobKeyList.Add(job.Id);
                        }

                        ScenarioDetailScheduleJobsT unscheduleJobT = new (s.Id, jobKeyList, false);
                        SendTransmission(unscheduleJobT);
                    }
                }
            }
        }
        catch (AutoTryEnterException)
        {
            WriteBusyMessage();
        }
    }

    #region Compression Test
    //        const string c_test_compression = "compressiontest";

    //        public void CompressionTest()
    //        {
    //            try
    //            {
    //                PT.Common.Testing.Timing serializeTiming = new Common.Testing.Timing(true, "Serialize Timing");
    //                PTBroadcaster.WritePTSystemToBytesParameters o = new PTBroadcaster.WritePTSystemToBytesParameters();
    //                SystemController.ServerSessionManager.PerformActionOnSystemData(true, false, SystemController.ServerSessionManager.WritePTSystemToBytes, o);
    //                serializeTiming.Stop();
    //                WriteLine_Invoke(string.Format(@"
    //Time to Serialize: {0}
    //System Bytes Length: {1}
    //", TimeSpan.FromTicks(serializeTiming.Length).ToString(), o.PTSystemBytes.Length));

    //                TestNormalCompression(o.PTSystemBytes);
    //                TestLZ4Compression(o.PTSystemBytes);
    //            }
    //            catch (PT.Common.AutoTryEnterException)
    //            {
    //                WriteBusyMessage();
    //            }
    //        }

    //        void TestNormalCompression(byte[] a_systemBytes)
    //        {
    //            PT.Common.Testing.Timing compressTiming = new Common.Testing.Timing();

    //            compressTiming.Start();
    //            byte[] compressedBytes = PT.Common.Compress.Deflate.Compress(a_systemBytes);
    //            compressTiming.Stop();

    //            PT.Common.Testing.Timing decompressTiming = new Common.Testing.Timing();
    //            decompressTiming.Start();
    //            byte[] decompressedBytes = PT.Common.Compress.Deflate.Decompress(compressedBytes);
    //            decompressTiming.Stop();

    //            this.WriteLine_Invoke(string.Format(c_compressionTestMsg, "Deflate", TimeSpan.FromTicks(compressTiming.Length).ToString(), TimeSpan.FromTicks(decompressTiming.Length).ToString(),
    //                compressedBytes.Length, Math.Round((double)compressedBytes.Length / a_systemBytes.Length, 2).ToString()));
    //        }

    //        void TestLZ4Compression(byte[] a_systemBytes)
    //        {
    //            PT.Common.Testing.Timing compressTiming = new Common.Testing.Timing();

    //            compressTiming.Start();
    //            byte[] compressedBytes = PT.Common.Compress.LZ4Algorithm.Compress(a_systemBytes);
    //            compressTiming.Stop();

    //            PT.Common.Testing.Timing decompressTiming = new Common.Testing.Timing();
    //            decompressTiming.Start();
    //            byte[] decompressedBytes = PT.Common.Compress.LZ4Algorithm.Decompress(compressedBytes);
    //            decompressTiming.Stop();

    //            this.WriteLine_Invoke(string.Format(c_compressionTestMsg, "LZ4", TimeSpan.FromTicks(compressTiming.Length).ToString(), TimeSpan.FromTicks(decompressTiming.Length).ToString(),
    //                compressedBytes.Length, Math.Round((double)compressedBytes.Length / a_systemBytes.Length, 2).ToString()));
    //        }

    //        const string c_compressionTestMsg = @"
    //Compression Type: {0}
    //
    //Time to Compress: {1}
    //Time to Decompress: {2}
    //
    //Compressed Bytes Length: {3}
    //Ratio: {4}
    //";
    #endregion

    private void WriteBusyMessage()
    {
        WriteLine_Invoke("Scheduler was busy. Try again.", true);
    }

    public void SendSetOnline()
    {
        ScenarioManager sm;
        using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 1000))
        {
            Scenario s = sm.GetByIndex(0);
            ScenarioDetailOnlineT onT = new (s.Id);
            SendTransmission(onT);
        }
    }

    public void SendSetOffline()
    {
        ScenarioManager sm;
        using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 1000))
        {
            Scenario s = sm.GetByIndex(0);
            ScenarioDetailOfflineT onT = new (s.Id);
            SendTransmission(onT);
        }
    }

    public void SendLockAlternatePath(string a_jobName, string a_moName, string a_pathName, bool a_lock)
    {
        ScenarioManager sm;
        using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 1000))
        {
            Scenario s = sm.GetByIndex(0);
            ScenarioDetail sd;
            using (s.ScenarioDetailLock.EnterWrite(out sd))
            {
                Job job = sd.JobManager.GetByNameSubstring(a_jobName, false);
                if (job == null)
                {
                    Console.WriteLine("Job not found.");
                    return;
                }

                ManufacturingOrder mo = job.ManufacturingOrders.GetByName(a_moName);
                if (mo == null)
                {
                    Console.WriteLine("MO not found");
                    return;
                }

                ScenarioDetailAlternatePathLockT pathLockT = null;

                if (a_pathName != null)
                {
                    for (int i = 0; i < mo.AlternatePaths.Count; ++i)
                    {
                        if (mo.AlternatePaths[i].Name == a_pathName)
                        {
                            pathLockT = new ScenarioDetailAlternatePathLockT(s.Id, job.Id, mo.Id, mo.AlternatePaths[i].Id, a_lock);
                            break;
                        }
                    }

                    if (pathLockT == null)
                    {
                        Console.WriteLine("Path not found");
                        return;
                    }
                }

                SendTransmission(pathLockT);
            }
        }
    }

    /// <summary>
    /// Write text to the command line followed by a new line.
    /// </summary>
    /// <param name="a_msg">Text to display in the command window.</param>
    /// <param name="a_forceRefresh">Whether to force the text to draw itself immediately. The default is false. And usually you'll want it to be false unless you're displaying text for debugging purposes.</param>
    public void Write(string a_msg, bool a_forceRefresh = false)
    {
        WriteLine(a_msg, a_forceRefresh);
    }

    /// <summary>
    /// Write a new line to the command window.
    /// </summary>
    public void WriteLine()
    {
        m_commandPrompt.Write(Environment.NewLine);
    }

    /// <summary>
    /// Write text to the command line followed by a new line.
    /// </summary>
    /// <param name="a_msg">Text to display in the command window.</param>
    /// <param name="a_forceRefresh">Whether to force the text to draw itself immediately. The default is false. And usually you'll want it to be false unless you're displaying text for debugging purposes.</param>
    public void WriteLine(string a_msg, bool a_forceRefresh = false)
    {
        m_commandPrompt.Write(a_msg);
        m_commandPrompt.Write(Environment.NewLine);
        //m_commandPrompt.ScrollToCaret();
        //if (a_forceRefresh)
        //{
        //    m_commandPrompt.Refresh();
        //}
    }

    private delegate void WriteLineDelegate(string a_msg, bool a_forceRefresh);

    private delegate void WritePrompt();

    public void WriteLine_Invoke(string a_msg, bool a_forceRefresh = false)
    {
        Invoke(new WriteLineDelegate(WriteLine), a_msg, a_forceRefresh);
        Invoke(new WritePrompt(m_commandPrompt.WritePrompt));
    }

    private delegate void WriteDelegate(string a_msg);

    public void Write_Invoke(string a_msg)
    {
        Invoke(new WriteDelegate(Write), a_msg);
    }

    /// <summary>
    /// Write text to the command line, overwriting last line.
    /// </summary>
    /// <param name="a_msg">Text to display in the command window.</param>
    public void Write(string a_msg)
    {
        m_commandPrompt.Write(a_msg, true);
    }

    public void WritePrompt_Invoke()
    {
        Invoke(new WritePrompt(m_commandPrompt.WritePrompt));
    }

    public string ScreenText => m_commandPrompt.Text;
}

// [BATCH_CODE]
internal class BatchTesting
{
    internal static int s_numberOfActivitiesToIncludeInBatch = 1;
    internal static bool s_joinWithBatchDroppedOntoIfPossible;
    internal static bool s_parseResourceNotesAsVolume;

    internal static ScenarioDetailMoveT GetMove(ScenarioDetailMoveT a_t)
    {
        IEnumerator<MoveBlockKeyData> moveBlockIterator = a_t.GetEnumerator();
        while (moveBlockIterator.MoveNext())
        {
            BlockKey currentBlock = moveBlockIterator.Current.BlockKey;
            List<InternalActivity> activities = GetAllActivities(a_t.ScenarioId, a_t.ToResourceKey, currentBlock);

            int nbrOfActivitiesToInclude = Math.Min(activities.Count, s_numberOfActivitiesToIncludeInBatch);
            ActivityKeyList activityList = new ();

            for (int actI = 0; actI < nbrOfActivitiesToInclude; ++actI)
            {
                ActivityKey activityKey = new (currentBlock.JobId, currentBlock.MOId, currentBlock.OperationId, activities[actI].Id);
                activityList.Add(activityKey);
            }

            MoveBlockKeyData moveBlock = new (moveBlockIterator.Current.ResourceKey, currentBlock, activityList);
            a_t.AddMoveBlock(moveBlock);
        }

        a_t.JoinWithBatchDroppedOntoIfPossible = s_joinWithBatchDroppedOntoIfPossible;

        return a_t;
    }

    internal static List<InternalActivity> GetAllActivities(BaseId a_scenarioId, ResourceKey a_resKey, BlockKey a_blockKey)
    {
        List<InternalActivity> activities = new ();
        ScenarioManager sm;

        using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 1000))
        {
            Scenario s;
            s = sm.Find(a_scenarioId);
            ScenarioDetail sd;
            using (s.ScenarioDetailLock.TryEnterWrite(out sd, 1000))
            {
                InternalOperation op = (InternalOperation)sd.JobManager.FindOperation(a_blockKey.JobId, a_blockKey.MOId, a_blockKey.OperationId);
                InternalActivity act = op.Activities.FindActivity(a_blockKey.ActivityId);
                ResourceBlock rb = act.GetResourceBlock(a_blockKey.BlockId);
                if (rb != null)
                {
                    Resource res = rb.ScheduledResource;
                    IEnumerator<InternalActivity> iterator = rb.Batch.GetEnumerator();

                    while (iterator.MoveNext())
                    {
                        activities.Add(iterator.Current);
                    }
                }
            }
        }

        return activities;
    }
}