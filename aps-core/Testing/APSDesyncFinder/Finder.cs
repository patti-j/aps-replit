using System.Diagnostics;

using PT.Scheduler;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.Transmissions;

namespace APSDesyncFinder;

public class Finder : IDisposable
{
    private readonly StartupVals m_ctorValues;
    private readonly string m_clientExePath;
    private readonly string m_clientArgs;

    private SortedStringList m_recordings;
    private int m_totalNbrOfRecordings;
    private int m_nbrOfRecordingsRan;
    private int m_playedIdx;
    private int m_connNbr = -1;
    private long m_connCreationTicks = -1;
    private Queue<Process> m_clientProcesses;
    private readonly object m_lock = new ();
    private readonly System.Timers.Timer m_timeoutTimer;
    private readonly System.Timers.Timer m_clientStartedTimer;

    public Finder(StartupVals a_ctorValues, string a_clientExePath, string a_clientArgs)
    {
        m_ctorValues = a_ctorValues;
        m_clientExePath = a_clientExePath;
        m_clientArgs = a_clientArgs;

        m_timeoutTimer = new System.Timers.Timer();
        m_timeoutTimer.AutoReset = false;
        m_timeoutTimer.Elapsed += TimeoutTimer_Elapsed;

        m_clientStartedTimer = new System.Timers.Timer(TimeSpan.FromSeconds(20).TotalMilliseconds);
        m_clientStartedTimer.AutoReset = false;
        m_clientStartedTimer.Elapsed += ClientStartedTimer;
    }

    internal void Start()
    {
        try
        {
            Console.WriteLine("- Starting Server");
            SystemController.StartAPSService(m_ctorValues, "Errors.txt");
            SystemController.StartSystemReceiver();
            Console.WriteLine("- Server Started");

            string[] files = Directory.GetFiles(PTBroadcaster.BroadcasterInstance.LoadRecordingDirectory, "*.*.bin");
            m_recordings = new SortedStringList(files);
            m_totalNbrOfRecordings = m_recordings.Count;
            Console.WriteLine("- There are {0} recordings.", m_totalNbrOfRecordings);

            if (SystemController.Sys != null)
            {
                SystemController.Sys.TransmissionProcessedEvent += TransmissionProcessed;
            }

            ScenarioManager sm;
            using (PTBroadcaster.BroadcasterInstance.LiveSystem.ScenariosLock.EnterRead(out sm))
            {
                sm.TransmissionProcessedEvent += new ScenarioManager.TransmissionProcessedDelegate(ScenarioManager_TranmissionProcessed);
                ScenarioEvents se;
                for (int i = 0; i < sm.OpenCount; i++)
                {
                    Scenario s = sm.GetByIndex(i);
                    using (s.AutoEnterScenarioEvents(out se))
                    {
                        se.TransmissionProcessedEvent += new ScenarioEvents.TransmissionProcessedCompleteDelegate(TransmissionProcessed);
                    }
                }
            }

            UserManagerEvents ume;
            using (PTBroadcaster.BroadcasterInstance.LiveSystem.UserManagerEventsLock.EnterRead(out ume))
            {
                ume.TransmissionProcessedEvent += TransmissionProcessed;
            }

            if (!HasUserLogOnT(m_recordings))
            {
                StartClient(m_clientExePath, m_clientArgs); // there are no UserLogOnTs in the recordings, start the client right now.
            }
            else
            {
                PlayBackSingle();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("- Error:");
            Console.WriteLine(e.Message);
        }
    }

    private bool HasUserLogOnT(SortedStringList a_recordings)
    {
        for (int i = 0; i < a_recordings.Count; i++)
        {
            string filePath = a_recordings[i];
            if (filePath.Contains("UserLogOnT"))
            {
                return true;
            }
        }

        return false;
    }

    private void ScenarioManager_TranmissionProcessed(ScenarioBaseT a_t, TimeSpan a_processingTime, int a_scenarioToProcess)
    {
        //TODO: add logic to handle sm sending multiple scenarios transmissions to processed.
        TransmissionProcessed(a_t, a_processingTime);
    }

    private void TransmissionProcessed(PT.Broadcasting.Transmission a_t, TimeSpan a_processingTime, Exception a_e)
    {
        PTTransmission ptT = a_t as PTTransmission;
        if (a_t != null)
        {
            TransmissionProcessed(ptT, a_processingTime);
        }
    }

    private void TransmissionProcessed(PTTransmission a_t, TimeSpan a_processingTime)
    {
        lock (m_lock)
        {
            m_lastTransmissionNbr = a_t.TransmissionNbr;
            if (a_t.Recording) // recording was played
            {
                m_transmissionProcessing = false;
                m_nbrOfRecordingsRan++;
                Console.WriteLine($" - PROCESSED in {a_processingTime.ToString()}");

                if (a_t is UserLogOnT)
                {
                    StartClient(m_clientExePath, m_clientArgs);
                    return;
                }
                //else if (a_t is UserLogOffT)
                //{
                //    //StopAClient();
                //}

                PlayBackSingle();
            }
            else
            {
                if (a_t is ScenarioChecksumT)
                {
                    PlayBackSingle();
                }
            } // new transmission, either during start or checksum inserted
        }
    }

    private void StartClient(string a_uiExe, string a_uiArgs)
    {
        Process clientprocess = new ();
        clientprocess.StartInfo.FileName = a_uiExe;
        clientprocess.StartInfo.Arguments = a_uiArgs;
        clientprocess.StartInfo.CreateNoWindow = false;
        clientprocess.EnableRaisingEvents = true;
        clientprocess.Exited += m_clientProcess_Exited;
        clientprocess.Start();

        Console.WriteLine("- Client Started.");

        if (m_clientProcesses == null)
        {
            m_clientProcesses = new Queue<Process>();
        }

        m_clientProcesses.Enqueue(clientprocess);
        m_clientStartedTimer.Start();
    }

    private void ClientStartedTimer(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (m_transmissionProcessing)
        {
            return;
        }

        PlayBackSingle();
    }

    private void StopAClient()
    {
        try
        {
            if (m_clientProcesses != null && m_clientProcesses.Count > 0)
            {
                m_clientProcesses.Dequeue().Kill();
            }
        }
        catch { }
    }

    private void m_clientProcess_Exited(object sender, EventArgs e)
    {
        Process p = sender as Process;
        if (p.ExitCode == 10)
        {
            Console.WriteLine("*********** Client Desynced!!! ***************");
        }
        else
        {
            Console.WriteLine("- Client exited with exit code:" + p.ExitCode);
        }
    }

    private void SendTransmission(PT.Broadcasting.Transmission a_t)
    {
        if (m_connNbr == -1)
        {
            PTBroadcaster ptBroadcaster = (PTBroadcaster)SystemController.Broadcaster;
            ptBroadcaster.CreateConnection(PT.Broadcasting.Connection.CreateDescription(), out m_connNbr, out m_connCreationTicks);
        }

        while (true)
        {
            try
            {
                if (a_t is UserBaseT)
                {
                    UserManager um;
                    using (SystemController.Sys.UsersLock.TryEnterRead(out um, 250))
                    {
                        a_t.LastTransmissionNbr = um.LastTransmissionNbr;
                    }
                }
                else if (a_t is ScenarioIdBaseT || a_t is PT.Transmissions2.IScenarioIdBaseT)
                {
                    BaseId scenarioId = a_t is ScenarioIdBaseT ? ((ScenarioIdBaseT)a_t).scenarioId : ((PT.Transmissions2.IScenarioIdBaseT)a_t).ScenarioId;
                    ScenarioManager sm;
                    using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 250))
                    {
                        Scenario s = sm.Find(scenarioId);
                        if (s != null)
                        {
                            ScenarioSummary ss;
                            using (s.AutoEnterScenarioSummary(out ss))
                            {
                                a_t.LastTransmissionNbr = ss.LastTransmissionNbr;
                            }
                        }
                    }
                }
                else if (a_t is ScenarioBaseT)
                {
                    ScenarioManager sm;
                    using (SystemController.Sys.ScenariosLock.TryEnterRead(out sm, 250))
                    {
                        a_t.LastTransmissionNbr = sm.LastTransmissionNbr;
                    }
                }

                break;
            }
            catch (AutoTryEnterException) { }
        }

        m_transmissionProcessing = true;
        Task.Run(() => { SystemController.Broadcaster.Broadcast(PT.Broadcasting.Transmission.Compress(a_t), m_connNbr); });
    }

    private void TimeoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        Console.WriteLine(" - TIMED OUT");
        PlayBackSingle();
    }

    private bool m_transmissionProcessing;
    private ulong m_lastTransmissionNbr;

    public void PlayBackSingle()
    {
        m_timeoutTimer.Stop();

        if (m_playedIdx >= m_totalNbrOfRecordings)
        {
            Console.WriteLine("- Ran all recordings");
            return;
        }

        string fullFilePath = m_recordings[m_playedIdx];
        TransmissionRecording recording;
        using (BinaryFileReader reader = new (fullFilePath))
        {
            recording = new TransmissionRecording(reader, PTSystem.TrnClassFactory);
        }

        PTTransmission t = recording.Transmission as PTTransmission;
        if (t != null)
        {
            t.SetRecording(fullFilePath, t.TransmissionNbr);
        }

        m_playedIdx++;

        if (recording.Transmission is UserErrorT)
        {
            PlayBackSingle();
            return;
        }

        if (t is ScenarioChecksumT checksumT)
        {
            Scenario s;
            ScenarioDetail sd;
            using (SystemController.Sys.AutoEnterRead(checksumT.ScenarioId, out s, out sd))
            {
                ((ScenarioChecksumT)recording.Transmission).ChecksumValues = sd.CalculateChecksums(m_lastTransmissionNbr);
            }
        }

        Console.Write(m_playedIdx + ": " + t.GetType());

        SendTransmission(recording.Transmission);

        m_timeoutTimer.Interval = TimeSpan.FromMinutes(10).TotalMilliseconds;
        m_timeoutTimer.Start();
    }

    public void Dispose()
    {
        ((PTBroadcaster)SystemController.Broadcaster).LogOff(m_connNbr, m_connCreationTicks);
        SystemController.StopAPSService("APS DesyncFinder Disposing");
    }
}