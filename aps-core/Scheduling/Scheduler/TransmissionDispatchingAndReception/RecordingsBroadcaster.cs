using PT.APSCommon;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.SystemServiceDefinitions;
using PT.Transmissions;

namespace PT.Scheduler.TransmissionDispatchingAndReception;

internal class RecordingsBroadcaster
{
    public const int c_lengthOfTransmissionsNumberMakePrivateOrInternal = 10;
    private const string c_directoryErrorMsg = "Directory \"{0}\" couldn't be deleted. Check for read-only directories or files, or file locks.";

    //private static PTBroadcaster s_broadcaster;

    private bool m_transmissionsPlayed;
    private BaseSession m_session;
    private readonly int m_maxBackups = 10;

    private bool m_loadRecordingDirectoryCached;
    private string m_loadRecordingDirectory;
    private readonly Dictionary<Guid, ChecksumValues> m_checksums = new ();
    private bool m_transmissionProcessing;
    private SortedStringList m_recordings;

    public bool HasRecordings => m_recordings != null && m_recordings.Count > 0;

    private readonly object m_lock = new ();

    //This is used when PlaybackUntilSimulation is used. This is increased as recordings are broadcast
    private int m_recordingIdx;

    private readonly WorkingDirectory m_workingDirectory;
    private readonly ServerSessionManager m_broadcaster;
    private readonly StartupVals m_constructorValues;

    /// <summary>
    /// This event is fired after each recording is broadcast
    /// </summary>
    /// <param name="a_t">Recording sent</param>
    /// <param name="a_recordingIdx">Index of current recording</param>
    /// <param name="a_recordingTotal">Total number of recordings to send</param>
    /// <param name="a_eventWillFire">Whether a transmission processed event will fire</param>
    public event Action<PTTransmission, int, int, bool> PlaybackProgressEvent;

    /// <summary>
    /// This event is fired when playback is triggered but there are no recordings to play, or when all recordings have been played.
    /// </summary>
    public event Action PlaybackEndOfTransmissionsEvent;

    internal RecordingsBroadcaster(WorkingDirectory a_workingDirectory, ServerSessionManager a_broadcaster, StartupVals a_constructorValues, long a_startDateTicks)
    {
        //s_broadcaster = a_broadcaster;
        m_workingDirectory = a_workingDirectory;
        m_broadcaster = a_broadcaster;
        m_constructorValues = a_constructorValues;
        m_workingDirectory.SetRecordingDirectoryPath(new DateTime(a_startDateTicks));
        EmptyRecordingDirectoryCleanout();
        Directory.CreateDirectory(m_workingDirectory.RecordingDirectory);
        m_maxBackups = m_constructorValues.MaxNbrSystemBackupsToStorePerSession;
    }

    internal void InitializeRecordingFiles(string[] a_recordings, long a_startingTransmissionNbr)
    {
        List<string> recordingsSubSet = new ();

        SortedStringList sortedFiles = new (a_recordings);
        if (a_startingTransmissionNbr > 0)
        {
            for (int i = 0; i < sortedFiles.Count; i++)
            {
                string filePath = sortedFiles[i];
                string file = Path.GetFileName(filePath);
                long transmissionNbr = long.Parse(file.Substring(0, c_lengthOfTransmissionsNumberMakePrivateOrInternal));
                if (transmissionNbr >= a_startingTransmissionNbr)
                {
                    recordingsSubSet.Add(filePath);
                }
            }

            m_recordings = new SortedStringList(recordingsSubSet.ToArray());
        }
        else
        {
            m_recordings = sortedFiles;
        }
    }

    internal void Init(BaseSession a_session)
    {
        m_session = a_session;
        m_nonSequenced = m_constructorValues.NonSequencedTransmissionPlayback;
    }

    /// <summary>
    /// A configuration file setting that controls whether all transmissions that are played back are made non-sequenced.
    /// </summary>
    private bool m_nonSequenced;

    private readonly TransmissionToProcess m_transmissionToProcess = new ();

    /// <summary>
    /// Send a transmission to trigger automatic Pruning.
    /// </summary>
    internal void Prune()
    {
        string[] files = Directory.GetFiles(LoadRecordingDirectory, "*.*.bin");
        SortedStringList sortedFiles = new (files);
        List<PTTransmission> transmissions = new ();

        for (int i = 0; i < sortedFiles.Count; i++)
        {
            TransmissionRecording recording;
            using (BinaryFileReader reader = new (sortedFiles[i]))
            {
                recording = new TransmissionRecording(reader, PTSystem.TrnClassFactory);
            }

            transmissions.Add((PTTransmission)recording.Transmission);
        }

        PruneScenarioT t = new (transmissions);
        m_broadcaster.TransmissionReceived(t, m_session.SessionToken);
    }

    /// <summary>
    /// Start playing back recordings. This is used when running recording immediatly after
    /// system has started.
    /// </summary>
    internal void PlayBack()
    {
        try
        {
            //Events are needed to trigger recordings individually after each one is finished processing.
            RegisterPlaybackEvents();

            //Trigger the first playback
            PlayBackSingle();
        }
        catch (Exception e) { }
    }

    private bool m_playUntilLogin;
    private readonly List<Type> m_skipTransmissionList = new ();

    internal void PlayBackUntilLogin()
    {
        m_playUntilLogin = true;
        PlayBack();
    }

    private void UnregisterEvents()
    {
        if (SystemController.Sys != null)
        {
            SystemController.Sys.TransmissionProcessedEvent -= TransmissionProcessed;
        }

        using (m_broadcaster.LiveSystem.ScenariosLock.EnterRead(out ScenarioManager sm))
        {
            sm.TransmissionProcessedEvent -= new ScenarioManager.TransmissionProcessedDelegate(SM_TransmissionHandler);
            sm.ScenarioNewEvent -= SmOnScenarioNewEvent;
            sm.ScenarioBeforeDeleteEvent -= SmOnScenarioBeforeDeleteEvent;
            for (int i = 0; i < sm.LoadedScenarioCount; i++)
            {
                Scenario s = sm.GetByIndex(i);
                using (s.AutoEnterScenarioEvents(out ScenarioEvents se))
                {
                    se.TransmissionProcessedEvent -= new ScenarioEvents.TransmissionProcessedCompleteDelegate(ScenarioEventsTransmissionHandler);
                }
            }
        }

        using (m_broadcaster.LiveSystem.UserManagerEventsLock.EnterRead(out UserManagerEvents ume))
        {
            ume.TransmissionProcessedEvent -= UmTransmissionProcessed;
        }

        m_registeredPlaybackEvents = false;
    }

    internal void SkipNextLogin()
    {
        m_skipTransmissionList.Add(typeof(UserLogOnT));
        m_skipTransmissionList.Add(typeof(ScenarioTouchT));
    }

    private bool m_registeredPlaybackEvents;
    private bool m_playbackSingle;
    private bool m_recordingPlayed;

    private void RegisterPlaybackEvents()
    {
        if (m_registeredPlaybackEvents)
        {
            return;
        }

        m_registeredPlaybackEvents = true;

        if (SystemController.Sys != null)
        {
            SystemController.Sys.TransmissionProcessedEvent += TransmissionProcessed;
        }

        using (m_broadcaster.LiveSystem.ScenariosLock.EnterRead(out ScenarioManager sm))
        {
            sm.TransmissionProcessedEvent += new ScenarioManager.TransmissionProcessedDelegate(SM_TransmissionHandler);
            sm.ScenarioNewEvent += SmOnScenarioNewEvent;
            sm.ScenarioReloadEvent += SmOnScenarioNewEvent; 
            // Other parts of the reload event chain require different behavior, but in this context,
            // reload and new are the same thing so we can reuse the event handler
            
            sm.ScenarioBeforeDeleteEvent += SmOnScenarioBeforeDeleteEvent; 
            sm.ScenarioBeforeReloadEvent += UnsubscribeScenarioEventsBeforeReload;
            // BeforeReload and BeforeDelete do the same thing, just have different event signatures
            for (int i = 0; i < sm.LoadedScenarioCount; i++)
            {
                Scenario s = sm.GetByIndex(i);
                using (s.AutoEnterScenarioEvents(out ScenarioEvents se))
                {
                    se.TransmissionProcessedEvent += new ScenarioEvents.TransmissionProcessedCompleteDelegate(ScenarioEventsTransmissionHandler);
                }
            }
        }

        using (m_broadcaster.LiveSystem.UserManagerEventsLock.EnterRead(out UserManagerEvents ume))
        {
            ume.TransmissionProcessedEvent += UmTransmissionProcessed;
        }
    }

    private void UmTransmissionProcessed(Transmission a_t, TimeSpan a_processingtime, Exception a_e)
    {
        if (a_t is ImportT)
        {
            return;
        }

        if (a_t is ScenarioBaseT sbT && sbT.ReplayForUndoRedo)
        {
            return;
        }

        TransmissionProcessed(a_t, a_processingtime, null);
    }

    private void SmOnScenarioBeforeDeleteEvent(Scenario a_s, ScenarioEvents a_se, ScenarioUndoEvents a_sue, ScenarioBaseT a_t, bool a_isUnload = false)
    {
        using (a_s.AutoEnterScenarioEvents(out ScenarioEvents se))
        {
            se.TransmissionProcessedEvent -= new ScenarioEvents.TransmissionProcessedCompleteDelegate(ScenarioEventsTransmissionHandler);
        }
    }

    private void UnsubscribeScenarioEventsBeforeReload(Scenario a_s, ScenarioEvents a_se, ScenarioUndoEvents a_sue)
    {
        using (a_s.AutoEnterScenarioEvents(out ScenarioEvents se))
        {
            se.TransmissionProcessedEvent -= new ScenarioEvents.TransmissionProcessedCompleteDelegate(ScenarioEventsTransmissionHandler);
        }
    }

    private void SmOnScenarioNewEvent(BaseId a_scenarioId, ScenarioBaseT a_t)
    {
        Task.Run(new Action(() => { AttachNewListener(a_scenarioId); }));
    }

    private void AttachNewListener(BaseId a_scenarioId)
    {
        using (m_broadcaster.LiveSystem.ScenariosLock.EnterRead(out ScenarioManager sm))
        {
            Scenario scenario = sm.Find(a_scenarioId);
            using (scenario.AutoEnterScenarioEvents(out ScenarioEvents se))
            {
                se.TransmissionProcessedEvent += new ScenarioEvents.TransmissionProcessedCompleteDelegate(ScenarioEventsTransmissionHandler);
            }
        }
    }

    private void ScenarioEventsTransmissionHandler(Transmission a_t, TimeSpan a_processingTime, Exception a_e)
    {
        if (a_t is ScenarioBaseT sbT && (sbT.ReplayForUndoRedo || !sbT.Recording))
        {
            return;
        }

        Task.Run(new Action(() => QueueScenarioEventProcessed(a_t as PTTransmission, a_processingTime)));
    }

    private void QueueScenarioEventProcessed(PTTransmission a_t, TimeSpan a_processingTime)
    {
        while (true)
        {
            lock (m_lock)
            {
                if (m_transmissionToProcess.TransmissionId == a_t.OriginalTransmissionNbr)
                {
                    m_transmissionToProcess.ScenariosToProcess--;
                    if (m_transmissionToProcess.ScenariosToProcess > 0)
                    {
                        return;
                    }

                    if (m_transmissionToProcess.ScenariosToProcess < 0)
                    {
                        throw new DebugException("Desynch?");
                    }
                }
                else
                {
                    Thread.Sleep(250);
                    continue;
                }
            }

            TransmissionProcessed(a_t, a_processingTime, null);
            break;
        }
    }

    private void SM_TransmissionHandler(ScenarioBaseT a_t, TimeSpan a_processingTime, int a_scenariosProcessed)
    {
        if (a_t is ScenarioBaseT sbT && (sbT.ReplayForUndoRedo || !sbT.Recording))
        {
            return;
        }

        if (a_scenariosProcessed == 0)
        {
            TransmissionProcessed(a_t, a_processingTime, null);
            return;
        }

        lock (m_lock)
        {
            if (a_t.OriginalTransmissionNbr == 0)
            {
                throw new DebugException("Error getting original transmission ID");
            }

            m_transmissionToProcess.ScenariosToProcess = a_scenariosProcessed;
            m_transmissionToProcess.TransmissionId = a_t.OriginalTransmissionNbr;
        }
    }

    private void TransmissionProcessed(Transmission a_t, TimeSpan a_processingTime, Exception a_e)
    {
        if (a_t is PacketT)
        {
            m_processingPacketT = false;
        }

        if (m_processingPacketT)
        {
            return;
        }

        if (a_t is ScenarioBaseT sbT && (sbT.ReplayForUndoRedo || !sbT.Recording))
        {
            return;
        }

        PTTransmission ptT = a_t as PTTransmission;
        if (a_t != null)
        {
            TransmissionProcessed(ptT, a_processingTime, a_e);
        }
    }

    private void TransmissionProcessed(PTTransmission a_t, TimeSpan a_processingTime, Exception a_e)
    {
        lock (m_lock)
        {
            if (a_t is ScenarioIdBaseT scenarioIdBaseT)
            {
                if (!m_checksums.ContainsKey(a_t.TransmissionId))
                {
                    using (SystemController.Sys.AutoEnterRead(scenarioIdBaseT.ScenarioId, out Scenario _, out ScenarioDetail sd))
                    {
                        ChecksumValues csValues = sd.CalculateChecksums(a_t.TransmissionId);
                        m_checksums.Add(a_t.TransmissionId, csValues);
                    }
                }
                else
                {
                    throw new DebugException("Something disturbing has happened with CheckSums");
                }
            }

            if (a_t.Recording) // recording was played
            {
                m_recordingPlayed = true;
                m_transmissionProcessing = false;
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

    private bool m_processingPacketT;

    private void SendTransmission(Transmission a_t)
    {
        if (a_t is PacketT)
        {
            m_processingPacketT = true;
        }

        m_transmissionProcessing = true;
        Task.Run(() => { m_broadcaster.TransmissionReceived(a_t, m_session.SessionToken); });
    }

    public void PlayBackSingle()
    {
        if (m_recordings == null || m_recordings.Count == 0)
        {
            return;
        }

        if (m_recordingIdx > m_recordings.Count - 1)
        {
            return;
        }

        string fullFilePath = m_recordings[m_recordingIdx];
        TransmissionRecording recording;
        using (BinaryFileReader reader = new (fullFilePath))
        {
            recording = new TransmissionRecording(reader, PTSystem.TrnClassFactory);
        }

        PTTransmission t = recording.Transmission as PTTransmission;
        t?.SetRecording(fullFilePath, t.TransmissionNbr);


        if (m_playbackSingle && m_recordingPlayed)
        {
            m_playbackSingle = false;
            m_recordingPlayed = false;
            return;
        }

        //Peek at transmission and decide if we should continue
        if (m_playUntilLogin && t is UserLogOnT)
        {
            m_playUntilLogin = false;
            //Don't play this one.
            return;
        }

        m_recordingIdx++;

        int indexOf = m_skipTransmissionList.IndexOf(t.GetType());
        if (indexOf >= 0)
        {
            m_skipTransmissionList.RemoveAt(indexOf);
            //Transmission skipped, continue
            PlayBackSingle();
            return;
        }

        if (t is UserErrorT)
        {
            PlayBackSingle();
            return;
        }

        //Older Checksum values didn't serialize LastTransnmissionNbr. Skip this one
        //if (t is ScenarioChecksumT checksumT && checksumT.ChecksumValues.LastProcessedTransmissionNbr != 0)
        //{
        //    if (m_checksums.TryGetValue(checksumT.ChecksumValues.LastProcessedTransmissionNbr, out ChecksumValues cachedCheckSums))
        //    {
        //        //Recalculate Checksum values here
        //        using (SystemController.Sys.AutoEnterRead(checksumT.ScenarioId, out Scenario _, out ScenarioDetail sd))
        //        {
        //            ((ScenarioChecksumT)t).ChecksumValues = cachedCheckSums;
        //        }
        //    }
        //    else
        //    {
        //        throw new DebugException("Something more disturbing has happened with CheckSums");
        //    }
        //}

        SendTransmission(recording.Transmission);

        if (m_recordingIdx < m_recordings.Count)
        {
            PlaybackProgressEvent?.Invoke(recording.Transmission as PTTransmission, m_recordingIdx, m_recordings.Count, false);
        }
        else
        {
            PlaybackEndOfTransmissionsEvent?.Invoke();
        }
    }

    public string LoadRecordingDirectory
    {
        get
        {
            if (!m_loadRecordingDirectoryCached)
            {
                m_loadRecordingDirectoryCached = true;
                m_loadRecordingDirectory = PTSystem.WorkingDirectory.StartupRecordingsPath;
            }

            return m_loadRecordingDirectory;
        }
    }

    internal void SetLoadRecordingDirectory(string a_recordingDirectory)
    {
        m_loadRecordingDirectoryCached = true;
        m_loadRecordingDirectory = a_recordingDirectory;
    }

    public void Receive(TriggerRecordingPlaybackT a_pbT)
    {
        if (m_constructorValues.StartType == EStartType.RecordingClientDelayed && !m_transmissionsPlayed)
        {
            m_playbackSingle = false;

            if (a_pbT.PlayBackMode == TriggerRecordingPlaybackT.EPlayBackModes.Full)
            {
                m_transmissionsPlayed = true;
                PlayBack();
            }
            else if (a_pbT.PlayBackMode == TriggerRecordingPlaybackT.EPlayBackModes.SingleSimulation)
            {
                m_playbackSingle = true;
                m_recordingPlayed = false;
                PlayBackSingle();
            }
            else if (a_pbT.PlayBackMode == TriggerRecordingPlaybackT.EPlayBackModes.Login)
            {
                PlayBackUntilLogin();
            }
            else if (a_pbT.PlayBackMode == TriggerRecordingPlaybackT.EPlayBackModes.SkipLogin)
            {
                SkipNextLogin();
            }
        }
    }

    /// <summary>
    /// Cleanout the recording directories that don't have anything in them. This should be done first to prevent
    /// good recordings from being deleted. For instance if the system won't start, a user may keep attempting to
    /// start the service each time this will cause the an old, but useful, instance of a recording to be deleted
    /// for a recording that doesn't contain anything.
    /// This function will not delete RecordingDirectory, since it's the directory where the current recording
    /// files are being written to and might be empty at the time this function is called.
    /// Only directories that start with "20* are deleted.
    /// </summary>
    private void EmptyRecordingDirectoryCleanout()
    {
        string[] directoriesTemp = Directory.GetDirectories(PTSystem.WorkingDirectory.RecordingRootFolder, "20*");

        // Old directories use to contain dash marks to separate date fields.
        for (int directoryI = 0; directoryI < directoriesTemp.Length; ++directoryI)
        {
            string path = directoriesTemp[directoryI];
            string directoryName = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            const int DATE_LEN = 10;

            if (fileName.IndexOf('-', 0, DATE_LEN) >= 0)
            {
                string newFileName = fileName.Replace('-', '.', DATE_LEN);
                string newPath = Path.Combine(directoryName, newFileName);
                Directory.Move(path, newPath);
                directoriesTemp[directoryI] = newPath;
            }
        }

        SortedStringList directories = new (directoriesTemp);
        string directory = "";

        try
        {
            for (int directoryI = 0; directoryI < directories.Count; ++directoryI)
            {
                directory = directories[directoryI];
                if (directory != m_workingDirectory.RecordingDirectory) // This makes sure the current recording directory isn't deleted.
                {
                    string[] files = Directory.GetFiles(directory);
                    if (files.Length == 0)
                    {
                        Common.Directory.DirectoryUtils.Delete(directory);
                    }
                }
            }
        }
        catch (Exception e)
        {
            string errorMsg = string.Format(c_directoryErrorMsg, directory);
            throw new PTException(errorMsg, e);
        }
    }

    internal string CreateBackupNameInRecordingDirectory(long a_nextTransmissionNumber)
    {
        return Path.Combine(m_workingDirectory.RecordingDirectory, CreateBackupName(a_nextTransmissionNumber));
    }

    internal string CreateBackupName(long a_nextTransmissionNumber)
    {
        DateTime now = DateTime.UtcNow;
        string nextTransmissionNumberString = a_nextTransmissionNumber.ToString(WorkingDirectory.TransmissionNumberFormatStringMakePrivateOrInternal);
        return $"{nextTransmissionNumberString}._{now.Year}.{now.Month:D2}.{now.Day:D2}.{now.Hour:D2}.{now.Minute:D2}.{now.Second:D2}.scenarios.dat";
    }

    /// <summary>
    /// Limit the number of backups to maxBackups.
    /// </summary>
    internal void MaxBackupsCleanup()
    {
        // These are the transmissions.
        string[] backupTransmissionsTemp = Directory.GetFiles(m_workingDirectory.RecordingDirectory, "*Transmissions*.bin");
        // These are the backup scenarios.
        string[] backupFilesTemp = Directory.GetFiles(m_workingDirectory.RecordingDirectory, ScenarioConstants.s_SCENARIO_DAT_FILE_EXTENSION_WILDCARD_NEW);

        // Build up the sorted list.
        SortedStringList files = new (backupTransmissionsTemp);

        for (int i = 0; i < backupFilesTemp.Length; ++i)
        {
            string backupFileName = backupFilesTemp[i];
            files.Add(backupFileName);
        }

        // Once this is set to true it indicates that directories encountered should be deleted.
        bool deleteBackups = false;
        int backupCount = 0;

        for (int fileI = files.Count - 1; fileI >= 0; --fileI)
        {
            string file = files[fileI];

            if (deleteBackups)
            {
                File.Delete(file);
            }
            else
            {
                if (file.IndexOf(ScenarioConstants.s_SCENARIO_DAT_FILE_EXTENSION_NEW) > 0)
                {
                    ++backupCount;
                    if (backupCount == m_maxBackups)
                    {
                        deleteBackups = true;
                    }
                }
            }
        }
    }

    private class TransmissionToProcess
    {
        internal int ScenariosToProcess;
        internal ulong TransmissionId;
    }
}