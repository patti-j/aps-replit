using System.Diagnostics;
using System.Net;
using System.Timers;

using PT.APIDefinitions;
using PT.APIDefinitions.RequestsAndResponses;
using PT.APIDefinitions.RequestsAndResponses.Webapp;
using PT.APSCommon;
using PT.APSCommon.Exceptions;
using PT.APSCommon.Extensions;
using PT.APSCommon.Interfaces;
using PT.Common.Compression;
using PT.Common.Encryption;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.Common.File;
using PT.Common.Http;
using PT.Common.Localization;
using PT.Common.Testing;
using PT.Common.Threading;
using PT.PackageDefinitions;
using PT.PackageDefinitions.DTOs;
using PT.PackageDefinitions.PackageInterfaces;
using PT.PackageDefinitions.Settings;
using PT.Scheduler.PackageDefs;
using PT.Scheduler.Sessions;
using PT.Scheduler.TransmissionDispatchingAndReception;
using PT.SchedulerDefinitions;
using PT.SchedulerDefinitions.PermissionTemplates;
using PT.SchedulerDefinitions.Session;
using PT.SchedulerDefinitions.UserSettingTemplates;
using PT.ServerManagerAPIProxy.APIClients;
using PT.ServerManagerSharedLib.Data;
using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.ServerManagerSharedLib.DTOs.Requests;
using PT.ServerManagerSharedLib.DTOs.Responses;
using PT.ServerManagerSharedLib.Helpers;
using PT.SystemDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.SystemServiceDefinitions;
using PT.Transmissions;

using ECompressionType = PT.Common.Compression.ECompressionType;
using SoftwareVersion = PT.Common.SoftwareVersion;

namespace PT.Scheduler;

/// <summary>
/// Create sessions and broadcast transmissions.
/// </summary>
public class ServerSessionManager : ServerSession, IDisposable, IAuthorizer
{
    #region Constants
    private const string c_scenariosFileName = "scenarios.dat";
    private const int c_lengthOfTransmissionsNumber = 10;
    private const string c_transmissionNumberFormatString = "D10";
    private const string c_couldntFindScenarioFileForRecording = "The recording couldn't be loaded because the recording folder didn't contain a scenarios.dat file.";
    public static string SystemDataFileName => c_scenariosFileName;
    #endregion

    #region Static Properties
    public static string SystemDataFileFullPath => Path.Combine(PTSystem.WorkingDirectory.Scenario, c_scenariosFileName);

    public PTSystem LiveSystem => m_liveSystem;

    //static ServerSessionManager s_broadcaster;
    //public static ServerSessionManager BroadcasterInstance
    //{
    //    get { return s_broadcaster; }
    //}
    #endregion

    #region Variables
    /// <summary>
    /// This event is fired when playback is triggered but there are no recordings to play, or when all recordings have been played.
    /// </summary>
    public event Action PlaybackEndOfTransmissionsEvent;

    private RecordingsBroadcaster m_recordingsBroadcaster;

    // The system that receives messages.
    private PTSystem m_liveSystem;
    private ISystemLogger m_errorReporter;

    /// <summary>
    /// Don't serialize this value.
    /// </summary>
    private SoftwareVersion m_productVersion;

    public SoftwareVersion ProductVersion => m_productVersion;

    /// <summary>
    /// License validation helper
    /// </summary>
    private LicenseKeyValidationHelper m_licenseValidator;
    #endregion

    #region Construction and disposal
    private void ConstructionException(Exception a_e, StartupValsAdapter a_constructorValues)
    {
        try
        {
            WriteToWindowsEventLog(a_e);
        }
        catch
        {
            // Don't let this stop the attempts to log the problem.
        }

        ISystemLogger errorReporter;
        if (m_liveSystem?.WorkingDirectoryInstance == null || PTSystem.WorkingDirectory == null)
        {
            // I create an ErrorReporter here because the PTSystem may not exist.

            errorReporter = PTSystem.StartupInit(a_constructorValues);
            Localizer.ExceptionLoggingDirectory = PTSystem.WorkingDirectory.LogFolder;
        }
        else
        {
            errorReporter = m_liveSystem.SystemLoggerInstance;
        }

        errorReporter.LogException(a_e, null);
    }

    private StartupValsAdapter m_constructorValues;

    public StartupValsAdapter ConstructorValues => m_constructorValues;

    private long m_startDate { get; set; }

    /// <summary>
    /// </summary>
    /// <param name="a_startDate"></param>
    /// <param name="a_constructorValues"></param>
    /// <param name="a_errorReporter"></param>
    /// <param name="a_liveSystem"></param>
    /// <param name="aStartDate">For workaround purposes. This is used to id the system when trying to recover from intermittent write failures that rarely happen on some machines.</param>
    public ServerSessionManager(long a_startDate, StartupValsAdapter a_constructorValues, ISystemLogger a_errorReporter, ref PTSystem r_liveSystem)
    {
        m_classFactory = PTSystem.TrnClassFactory;
        m_startDate = a_startDate;
        m_dropConnectionsTimer = new System.Threading.Timer(new TimerCallback(CleanupSessions), null, 0, c_cleanUpDeadConnectionsMilliSeconds);
        SetSessionIsLoggedIn();

        try
        {
            InitWindowsEventLog();
            LoadScenarioAndStart(a_startDate, a_constructorValues, a_errorReporter, ref r_liveSystem);
            using (r_liveSystem.ScenariosLock.EnterRead(out ScenarioManager scenarioManager))
            {
                //TODO lite client: All scenarios are considered opened on the server right now. We want to change this at some point, so this HashSet should change accordingly
                LoadedScenarioIds = new HashSet<BaseId>(scenarioManager.Scenarios.Select(a_scenario => a_scenario.Id));
            }
            InitEventListeners(true);
        }
        catch (Exception e)
        {
            ConstructionException(e, a_constructorValues);
            throw;
        }
    }

    ~ServerSessionManager()
    {
        Dispose(true);
    }
    #endregion

    #region Windows Event Log
    private const string c_startupErrorsSource = "APS - Construction Error";
    private const string c_startupErrorsLogName = "APS - Advanced Planning and Scheduling";

    private static void InitWindowsEventLog()
    {
        try
        {
            if (!EventLog.SourceExists(c_startupErrorsSource))
            {
                // This event type won't work the first time this application runs on a computer. Subsequent runs will work.
                // That is if this block is entered and an attempt to log a message is made it won't work.
                // The next time the program runs, the event source will have completely registered with Windows and log attempts will work. 
                EventLog.CreateEventSource(c_startupErrorsSource, c_startupErrorsLogName);
            }
        }
        catch { }
    }

    private static void WriteToWindowsEventLog(Exception a_exception)
    {
        try
        {
            string msg;

            try
            {
                ExceptionDescriptionInfo edi = new (a_exception);
                msg = SimpleExceptionLogger.CreateErrorMsgString(edi, new ScenarioExceptionInfo(), "Construction Error");
            }
            catch
            {
                msg = a_exception.Message;
            }

            EventLog myLog = new ();
            myLog.Source = c_startupErrorsSource;
            myLog.WriteEntry(msg);
        }
        catch { }
    }
    #endregion

    private UserSession GenerateNewSession(string a_name, byte[] a_passwordHash, ELoginMethod a_loginType, string a_description)
    {
        try
        {
            User user = UserLicenseCheck(a_name, a_passwordHash, a_loginType, true);
            HashSet<BaseId> scenariosToLoad = new HashSet<BaseId>();
            if (user.LoadedScenarioIds.Count == 0)
            {
                // New user or user from previous older versions that didn't have lite client
                using (m_liveSystem.ScenariosLock.EnterRead(out ScenarioManager sm))
                {
                    scenariosToLoad.Add(sm.GetFirstProductionScenario().Id);
                }
            }
            else
            {
                scenariosToLoad.AddRangeIfNew(user.LoadedScenarioIds);
            }

            PreCreateConnectionProcessing();

            UserSession newSession = new ServerSession();
            newSession.Encrypt(m_symmetricKey);
            newSession.SetCompression(user.CompressionType);
            newSession.SetUser(user.Id);
            newSession.LoadedScenarioIds = scenariosToLoad; 
            // Need to instantiate a new HashSet, otherwise, the assignment above will make the collection on the User and the UserSession
            // reference the instance
            if (user.AppUser)
            {
                //Find all other app sessions this user has and drop them
                //This is done since app sessions don't timeout
                CleanupSessions(user.Id);

                //Don't timeout this session
                newSession.ConnectionTimeout = TimeSpan.MaxValue;
            }
            else
            {
                newSession.ConnectionTimeout = TimeSpan.FromSeconds(BiDirectionalConnectionTimeout);
            }

            lock (m_connectionLock)
            {
                AddSession(newSession);
            }

            return newSession;
        }
        catch (Exception e)
        {
            SimpleExceptionLogger.LogException(BroadcastingAlerts.CreateConnectionExceptionLogFilePath, e, "An exception occurred while trying to create a connection.");
            throw;
        }
    }

    public bool ValidateAuthorization(string a_sessionToken)
    {
        if (SessionToken == a_sessionToken)
        {
            return true;
        }

        foreach (BaseSession session in m_activeSessions)
        {
            if (session.SessionToken == a_sessionToken)
            {
                return true;
            }
        }

        return false;
    }

    public IUserPermissionSet GetUserPermissions(string a_sessionToken)
    {
        BaseSession validateSession = ValidateSession(a_sessionToken);
        if (validateSession is UserSession userSession)
        {
            using (SystemController.Sys.UsersLock.EnterRead(out UserManager um))
            {
                UserPermissionSet permissionSet = um.GetUserPermissionSetById(userSession.UserId);
                return permissionSet;
            }
        }

        return null;
    }

    public bool IsAppConnection(string a_sessionToken)
    {
        if (GetSession(a_sessionToken) is UserSession userSession)
        {
            //TODO: Store app user setting on the session. We can't use UserManager here because this gets called on every action.
            return userSession.ConnectionTimeout == TimeSpan.MaxValue;
        }

        return false;
    }

    /// <summary>
    /// Uses a random token created by ServerManager to validate requests coming from the local server
    /// </summary>
    /// <param name="a_serverToken">Authentication Token</param>
    /// <returns></returns>
    public bool ValidateServerAuthorization(string a_serverToken)
    {
        return a_serverToken == ConstructorValues.SecurityToken;
    }

    public BaseSession ValidateSession(string a_sessionToken)
    {
        if (SessionToken == a_sessionToken)
        {
            return this;
        }

        foreach (BaseSession session in m_activeSessions)
        {
            if (session.SessionToken == a_sessionToken)
            {
                return session;
            }
        }

        //TODO: Throw errors
        return null;
    }

    #region UndoSets
    public int GetUndoIdByTransmissionNbr(BaseId a_scenarioId, ulong a_transmissionNbr)
    {
        using (m_liveSystem.ScenariosLock.TryEnterRead(out ScenarioManager sm, AutoExiter.THREAD_TRY_WAIT_MS))
        {
            Scenario scenario = sm.Find(a_scenarioId);
            using (scenario.UndoSetsLock.EnterRead(out Scenario.UndoSets uss))
            {
                return uss.FindUndoSetByTransmissionNbr(a_transmissionNbr, false);
            }
        }
    }
    public List<UndoAction> GetScenarioUndoSets(BaseId a_scenarioId, string a_userToken)
    {
        TransmissionSession transmissionSession = GetSession(a_userToken);
        BaseId requestingUserId = BaseId.NULL_ID;
        if (transmissionSession is UserSession userSession)
        {
            requestingUserId = userSession.UserId;
        }
        else
        {
            return null;
        }

        UserPermissionSet userPermissionSet;
        bool canUndoERPActions = false;
        using (m_liveSystem.UsersLock.TryEnterRead(out UserManager um, AutoExiter.THREAD_TRY_WAIT_MS))
        {
            BaseId findUserPermissionSetIdUsingUserId = um.FindUserPermissionSetIdUsingUserId(requestingUserId);
            userPermissionSet = um.GetUserPermissionSetById(findUserPermissionSetIdUsingUserId);
            canUndoERPActions = userPermissionSet.GetPermissionsValidator().ValidatePermission(UserPermissionKeys.UndoErpActions);
        }

        List<UndoAction> scenarioUndoSets = new ();
        using (m_liveSystem.ScenariosLock.TryEnterRead(out ScenarioManager sm, AutoExiter.THREAD_TRY_WAIT_MS))
        {
            Scenario scenario = sm.Find(a_scenarioId);
            bool canEditScenario = false;

            using (scenario.ScenarioSummaryLock.TryEnterRead(out ScenarioSummary ss, AutoExiter.THREAD_TRY_WAIT_MS))
            {
                ScenarioPermissionSettings permission = ss.ScenarioSettings.LoadSetting<ScenarioPermissionSettings>(ScenarioPermissionSettings.Key);

              canEditScenario =  scenario.ValidateScenarioCanEditPermissions(requestingUserId, permission);
            }

            using (ObjectAccess<Scenario.UndoSets> und = scenario.UndoSetsLock.TryEnterRead(AutoExiter.THREAD_TRY_WAIT_MS))
            {
                for (int i = 0; i < und.Instance.Count; i++)
                {
                    Scenario.UndoSet undoSet = und.Instance[i];


                    for (int j = 0; j < undoSet.Count; j++)
                    {
                        Scenario.TransmissionJar jar = undoSet[j];
                        BaseId instigator = jar.TransmissionInfo.Instigator;
                        // System Transmissions, don't display these
                        if (jar.TransmissionInfo.IsInternal)
                        {
                            continue;
                        }

                        string actionDescription = jar.TransmissionInfo.UndoChangeString;
                        bool canUndoSingle = false;
                        int transmissionId = jar.TransmissionInfo.TransmissionUniqueId;
                        //Disable the row based on the user's rights
                        bool canUndo = true;
                        if (transmissionId is ScenarioDetailExpediteBaseT.UNIQUE_ID or ScenarioDetailMoveT.UNIQUE_ID)
                        {
                            canUndoSingle = true;
                            UpdateTransmissionDescriptions(scenario, jar, ref actionDescription);
                        }
                        else if (transmissionId is ScenarioClearUndoSetsT.UNIQUE_ID)
                        {
                            canUndo = false;
                        }

                        bool userCanOverrideOthersActions = userPermissionSet.AllowChangesThatOverrideOtherUserActions;
                        
                        if (instigator == BaseId.ERP_ID &&
                           (!canUndoERPActions || !canEditScenario) &&
                            !userCanOverrideOthersActions)
                        {
                            canUndo = false;
                        }
                        else if (instigator != BaseId.ERP_ID &&
                                 instigator != requestingUserId &&
                                 !userPermissionSet.GetPermissionsValidator().AllowActionsThatAffectUsers &&
                                 !userCanOverrideOthersActions)
                        {
                            canUndo = false;
                            canUndoSingle = false;
                        }

                        if (instigator == BaseId.NULL_ID)
                        {
                            canUndo = false;
                        }

                        ulong undoSetUndoNbr = undoSet.m_undoNbr.ToBaseType();
                        scenarioUndoSets.Add(new (undoSetUndoNbr, actionDescription, instigator.Value, jar.TransmissionInfo.TimeStamp.ToDateTime(), jar.ProcessingTime, jar.TransmissionInfo.TransmissionNbr, canUndo, canUndoSingle, jar.Play, (ulong)jar.TransmissionInfo.TransmissionUniqueId, jar.TransmissionInfo.TransmissionId.ToString()));
                    }
                }
            }
        }

        return scenarioUndoSets;
    }
   
    private static void UpdateTransmissionDescriptions(Scenario a_scenario, Scenario.TransmissionJar jar, ref string actionDescription)
    {
        if (!jar.ActionDescriptionOverrideSet)
        {
            try
            {
                using (a_scenario.ScenarioDetailLock.TryEnterRead(out ScenarioDetail sd, 100))
                {
                    if (jar.TransmissionInfo.TransmissionUniqueId == ScenarioDetailMoveT.UNIQUE_ID)
                    {
                        ScenarioDetailMoveT moveT = (ScenarioDetailMoveT)jar.GetTransmission();
                        int count = moveT.Count();

                        Resource resource = sd.PlantManager.GetResource(moveT.ToResourceKey);
                        if (resource == null)
                        {
                            return;
                        }

                        if (count > 1)
                        {
                            actionDescription = string.Format("{0} activities moved to {1}".Localize(), count, resource.Name);
                        }

                        foreach (MoveBlockKeyData data in moveT)
                        {
                            //Only 1 block
                            BlockKey blockKey = data.BlockKey;
                            if (sd.JobManager.GetById(blockKey.JobId) is Job job)
                            {
                                if (job.ManufacturingOrders.GetById(blockKey.MOId) is ManufacturingOrder mo)
                                {
                                    if (mo.OperationManager[blockKey.OperationId] is BaseOperation op)
                                    {
                                        actionDescription = string.Format("Job {0} Op {1} moved to {2}".Localize(), job.Name, op.Name, resource.Name);
                                    }
                                }
                            }
                        }
                    }
                    else if (jar.TransmissionInfo.TransmissionUniqueId == ScenarioDetailExpediteMOsT.UNIQUE_ID)
                    {
                        ScenarioDetailExpediteMOsT expediteT = (ScenarioDetailExpediteMOsT)jar.GetTransmission();
                        if (expediteT.MOs.Count > 1)
                        {
                            actionDescription = string.Format("{0} jobs expedited".Localize(), expediteT.MOs.Count);
                        }
                        else
                        {
                            //Single expedite
                            MOKeyList.Node moNode = expediteT.MOs.First;
                            Job job = sd.JobManager.GetById(moNode.Data.JobId);
                            if (job == null)
                            {
                                return;
                            }

                            if (job.ManufacturingOrders.Count == 1)
                            {
                                actionDescription = string.Format("{0} expedited".Localize(), job.Name);
                            }
                            else
                            {
                                ManufacturingOrder manufacturingOrder = job.ManufacturingOrders.GetById(moNode.Data.MOId);
                                if (manufacturingOrder != null)
                                {
                                    actionDescription = string.Format("Job {0} MO {1} expedited".Localize(), job.Name, manufacturingOrder.Name);
                                }
                            }
                        }
                    }
                    else if (jar.TransmissionInfo.TransmissionUniqueId == ScenarioDetailExpediteJobsT.UNIQUE_ID)
                    {
                        ScenarioDetailExpediteJobsT expediteJobsT = (ScenarioDetailExpediteJobsT)jar.GetTransmission();
                        if (expediteJobsT.Jobs.Count > 1)
                        {
                            actionDescription = string.Format("{0} jobs expedited".Localize(), expediteJobsT.Jobs.Count);
                        }
                        else
                        {
                            //Single expedite
                            Job job = sd.JobManager.GetById(expediteJobsT.Jobs.GetFirst());
                            if (job != null)
                            {
                                actionDescription = string.Format("{0} expedited".Localize(), job.Name);
                            }
                        }
                    }
                }
            }
            catch (AutoTryEnterException)
            {
                //We can try and load it again next time
            }
        }
    }
    #endregion
    #region Perform action on the whole system. Such as write the scenario to disk or write the scenario to a compressed array of bytes.
    private string GetTransmissionFilePath(string a_workingDirectory, string a_systemPrefix)
    {
        string transmissionFileName = string.Format("{0}.transmissions.bin", a_systemPrefix);
        string transmissionFilePath = Path.Combine(a_workingDirectory, transmissionFileName);
        return transmissionFilePath;
    }

    // I'm just using this for synchronization purposes.
    // I always call lock against this variable before attempting to write the
    // system data file.
    private readonly object m_systemDataFileLock = new ();

    #region WritePTSystemToDisk
    /// <summary>
    /// Serialize the PTSystem to disk as a single binary file.
    /// </summary>
    /// <param name="a_writeDirectory">Where the </param>
    /// <param name="a_fileName">For instance "scenario.dat"</param>
    /// <param name="a_restartReceiver">Whether to restart the system's receiver after the write has completed.</param>
    /// <param name="a_shutDown">Whether the system is being shut down.</param>
    /// <param name="a_touchScenarios"></param>
    private void WritePTSystemToDisk(string a_writeDirectory, string a_fileName, bool a_restartReceiver)
    {
        WritePTSystemToDiskParameters performActionOnSystemDataParameters = new (a_writeDirectory, a_fileName, null, ECompressionType.Normal);
        PerformActionOnSystemData(a_restartReceiver, WriteSystemDataToDiskHelper, performActionOnSystemDataParameters);
    }

    private void WritePTSystemToMemory(bool a_restartReceiver, TransmissionSession a_loggingInSession, ECompressionType a_compressionType)
    {
        WritePTSystemToMemoryParameters performActionOnSystemDataParameters = new (a_loggingInSession, a_compressionType);
        PerformActionOnSystemData(a_restartReceiver, WriteSystemDataToMemoryHelper, performActionOnSystemDataParameters);
    }

    private void WriteAllUnloadedScenarioDataToMemory(string a_writeDirectory, bool a_restartReceiver, TransmissionSession a_loggingInSession, ECompressionType a_compressionType)
    {
        // This currently locks the entire system. Is there a way to do it where we just lock the ScenarioManager?
        WriteUnloadedScenarioDataToMemoryParameters performActionOnSystemDataParameters = new(a_writeDirectory, a_loggingInSession, a_compressionType);
        PerformActionOnSystemData(a_restartReceiver, WriteUnloadedScenarioDataToMemoryHelper, performActionOnSystemDataParameters);
    }

    /// <summary>
    /// A helper class of WritePTSystemToDisk used to pass values through a delagate to PerformActionOnSystemData.
    /// </summary>
    private class WritePTSystemToDiskParameters
    {
        internal WritePTSystemToDiskParameters(string a_workingDirectory, string a_fileName, TransmissionSession a_loggingInSession, ECompressionType a_compressionType = ECompressionType.Normal)
        {
            WorkingDirectory = a_workingDirectory;
            FileName = a_fileName;
            CompressionType = a_compressionType;
            LoggingInSession = a_loggingInSession;
        }

        internal string WorkingDirectory { get; private set; }
        internal string FileName { get; private set; }
        internal ECompressionType CompressionType { get; private set; }
        public TransmissionSession LoggingInSession { get; private set; }
    }
    /// <summary>
    /// A helper class of WritePTSystemToMemory used to pass values through a delegate to PerformActionOnSystemData.
    /// </summary>
    private class WritePTSystemToMemoryParameters
    {
        internal WritePTSystemToMemoryParameters(TransmissionSession a_loggingInSession, ECompressionType a_compressionType = ECompressionType.Normal)
        {
            CompressionType = a_compressionType;
            LoggingInSession = a_loggingInSession;

            if (a_loggingInSession is IUserSession userSession)
            {
                UserId = userSession.UserId;
                PreviousSessionLoadedScenarioIds = userSession.LoadedScenarioIds;
            }
        }

        internal ECompressionType CompressionType { get; private set; }
        public TransmissionSession LoggingInSession { get; private set; }
        internal BaseId UserId { get; private set; }
        internal HashSet<BaseId> PreviousSessionLoadedScenarioIds { get; private set; }
    }

    private class WriteUnloadedScenarioDataToMemoryParameters
    {
        internal WriteUnloadedScenarioDataToMemoryParameters(string a_workingDirectory, TransmissionSession a_loggingInSession, ECompressionType a_compressionType = ECompressionType.Normal)
        {
            WorkingDirectory = a_workingDirectory;
            CompressionType = a_compressionType;
            LoggingInSession = a_loggingInSession;

            if (a_loggingInSession is IUserSession userSession)
            {
                UserId = userSession.UserId;
                PreviousSessionLoadedScenarioIds = userSession.LoadedScenarioIds;
            }
        }

        internal string WorkingDirectory { get; private set; }
        internal ECompressionType CompressionType { get; private set; }
        public TransmissionSession LoggingInSession { get; private set; }
        internal BaseId UserId { get; private set; }
        internal HashSet<BaseId> PreviousSessionLoadedScenarioIds { get; private set; }
    }

    /// <summary>
    /// Passed as a parameter to PerformActionOnSystemData. This function takes the locked system and serializes it to disk.
    /// </summary>
    /// <param name="a_lockedPTSystem"></param>
    /// <param name="a_parameters">The type must be WritePTSystemToDiskParameters.</param>
    private void WriteSystemDataToDiskHelper(PTSystem a_lockedPTSystem, object a_parameters)
    {
        WritePTSystemToDiskParameters parameters = (WritePTSystemToDiskParameters)a_parameters;
        string path = Path.Combine(parameters.WorkingDirectory, parameters.FileName);
        using (BinaryFileWriter writer = new (path, parameters.CompressionType))
        {
            m_liveSystem.Serialize(writer);
        }
    }

    private void WriteSystemDataToMemoryHelper(PTSystem a_lockedPTSystem, object a_parameters)
    {
        WritePTSystemToMemoryParameters parameters = (WritePTSystemToMemoryParameters)a_parameters;
        using (BinaryMemoryWriter writer = new (m_symmetricKey, parameters.CompressionType))
        {
            m_liveSystem.SerializeForClient(writer, parameters.UserId, parameters.PreviousSessionLoadedScenarioIds, out IEnumerable<BaseId> recentLoadedScenarioIds);
            m_systemBytes = writer.GetBuffer();

            UpdateUserSession(parameters.LoggingInSession, recentLoadedScenarioIds);
            SyncTransmissionsOfLoggingInSession();
        }
    }
    /// <summary>
    /// Updates the Transmission Session which a collection of Base Ids of the most recent scenarios sent to the client
    /// </summary>
    /// <param name="a_session"></param>
    /// <param name="a_loadedScenarios"></param>
    private static void UpdateUserSession(TransmissionSession a_session, IEnumerable<BaseId> a_loadedScenarios)
    {
        if (a_session is UserSession userSession)
        {
            userSession.LoadedScenarioIds.Clear();
            foreach (BaseId scenario in a_loadedScenarios)
            {
                userSession.LoadedScenarioIds.Add(scenario);
            }
        }
    }

    private void SyncTransmissionsOfLoggingInSession()
    {
        lock (m_connectionLock)
        {
            foreach (BaseSession session in m_activeSessions)
            {
                if (session is TransmissionSession transmissionSession &&
                    !transmissionSession.SessionIsLoggedIn)
                {
                    SyncTransmissions(transmissionSession);
                }
            }
        }
    }

    private void WriteUnloadedScenarioDataToMemoryHelper(PTSystem a_lockedPTSystem, object a_parameters)
    {
        WriteUnloadedScenarioDataToMemoryParameters parameters = (WriteUnloadedScenarioDataToMemoryParameters)a_parameters;
        using (BinaryMemoryWriter writer = new ())
        {
            m_liveSystem.SerializeAllUnloadedScenarioData(writer, parameters.UserId, parameters.PreviousSessionLoadedScenarioIds);
            m_unloadedScenarioDataBytes = writer.GetBuffer();
        }
    }
    #endregion

    #region WritePTSystemToCompressedBytes
    /// <summary>
    /// Get the system as an array of bytes that were serialized by PT.Common.ISerializable and compressed by the Deflate algorithm using the format used by System.IO.Compression.DeflateStream.
    /// </summary>
    /// <returns>The PTSystem as a compressed array of bytes.</returns>
    internal byte[] WritePTSystemToCompressedBytes()
    {
        byte[] compressedPTSystem;
        {
            WritePTSystemToBytesParameters results = new ();
            PerformActionOnSystemData(true, WritePTSystemToBytes, results); // ? first Parameter, ? second parameter
            compressedPTSystem = Optimal.Compress(results.PTSystemBytes);
        }
        return compressedPTSystem;
    }

    /// <summary>
    /// Used as a helper class of WritePTSystemToBytes. It serializes the PTSystem as an array of bytes.
    /// </summary>
    private class WritePTSystemToBytesParameters
    {
        /// <summary>
        /// The PTSystem as a serialized array of bytes.
        /// </summary>
        internal byte[] PTSystemBytes { get; set; }
    }

    /// <summary>
    /// Serializes the PTSystem as a compressed array of bytes.
    /// </summary>
    /// <param name="a_lockedPTSystem">The PTSystem must be thread locked and all transmissions must have been flushed.</param>
    /// <param name="a_parameters">
    /// The type is set to object to conform to PerformActionOnLockedSystemDataDelegate through which this is called. The real expected type is WritePTSystemToBytesParameters where
    /// the bytes will be stored.
    /// </param>
    private void WritePTSystemToBytes(PTSystem a_lockedPTSystem, object a_parameters)
    {
        byte[] ptSystemBytes;
        using (BinaryMemoryWriter writer = new (ECompressionType.Normal))
        {
            a_lockedPTSystem.Serialize(writer);
            ptSystemBytes = writer.GetBuffer();
        }

        WritePTSystemToBytesParameters results = (WritePTSystemToBytesParameters)a_parameters;
        results.PTSystemBytes = ptSystemBytes;
    }
    #endregion

    /// <summary>
    /// After the PTSystem has been locked by PerformActionOnSystemData, a function that conforms to this deleagte is called to perform some processing.
    /// </summary>
    /// <param name="a_lockedPTSystem"></param>
    /// <param name="a_parameters"></param>
    private delegate void PerformActionOnLockedSystemDataDelegate(PTSystem a_lockedPTSystem, object a_parameters);

    /// <summary>
    /// Lock the PTSystem and call a delegate to perform some action.
    /// </summary>
    /// <param name="workingDirectory"></param>
    /// <param name="fileName">The name you want to serialize the system data as.</param>
    /// <param name="restartReceiver">Whether to restart the receiver after the data has been serialzed.</param>
    private void PerformActionOnSystemData(bool a_restartReceiver, PerformActionOnLockedSystemDataDelegate a_performActionOnSystemDelegate, object a_performActionOnSystemDelegateParameters)
    {
        lock (m_systemDataFileLock)
        {
            try
            {
                //Stop processing new transmission while we copy the system.
                Task.WaitAll(SystemController.StopReceiving());

                // Wait for the dispatch of all the messages at the system level.
                m_liveSystem.FlushDispatcher();

                a_performActionOnSystemDelegate(m_liveSystem, a_performActionOnSystemDelegateParameters);
            }
            catch (Exception e)
            {
                m_errorReporter.LogException(e, null);
                Environment.Exit(-1);
            }
            finally
            {
                if (a_restartReceiver)
                {
                    SystemController.StartReceiving();
                }
            }
        }
    }

    #region Load PTSystem
    private void LoadSystemAndPackages(string a_fullFilePath, bool a_copyCheckpointsToo, long a_startDate, Timing a_deserializeTiming, StartupValsAdapter a_constructorValues, ref PTSystem r_liveSystem)
    {
        //TODO: Maybe call init workspace on System
        LoadScenario(a_fullFilePath, a_startDate, a_deserializeTiming, a_constructorValues, ref r_liveSystem);

        m_liveSystem.TouchScenarios();


        BiDirectionalConnectionTimeout = a_constructorValues.ClientTimeoutSeconds;

        TransmissionNbr = m_liveSystem.LastTransmissionNbr;
    }

    private void LoadPackages()
    {
        using (ObjectAccess<IPackageManager> pmoa = m_liveSystem.PackageManagerLock.EnterWrite())
        {
            try
            {
                PackageManager packageManager = (PackageManager)pmoa.Instance;
                packageManager.LoadPackagesFromDisk();
            }
            catch (Exception err)
            {
                SimpleExceptionLogger.LogPackageException(Path.Combine(PackageErrorsLogPath), err.GetExceptionFullMessage());
            }
        }
    }

    internal static bool _s_primeLoadComplete;

    private void LoadScenario(string a_fullFilePath, long a_startDate, Timing a_deserializeTiming, StartupValsAdapter a_startupVals, ref PTSystem r_liveSystem)
    {
        int version = LoadScenarioHelper(a_fullFilePath, a_startDate, a_deserializeTiming, true, a_startupVals, ref r_liveSystem);
        _s_primeLoadComplete = true;

        m_recordingsBroadcaster.Init(this);
    }

    private int LoadScenarioHelper(string a_fullFilePath, long a_startDate, Timing a_deserializeTiming, bool a_startup_onFirstLoadAllowResequencingOfIds_reloadToFixupIdsDontResequence, StartupValsAdapter a_startupVals, ref PTSystem r_liveSystem)
    {
        int version;

        if (File.Exists(a_fullFilePath))
        {
            FileUtils.RemoveReadOnlyAttribute(a_fullFilePath);
        }

        using (BinaryFileReader reader = new (a_fullFilePath))
        {
            version = reader.VersionNumber;

            if (version > Serialization.VersionNumber)
            {
                //TODO: See TODO in GetErrorString. Cloud Architecture may need to rework this later now that we've got exceptions before language modules are loaded.
                throw new PTException($"The scenario file was written with a newer version of APS. To run this scenario you'll need to upgrade your version. Written with serialization #{version}; this executable's serialization #{Serialization.VersionNumber}.");
            }

            a_deserializeTiming.Start();
            (string serialCode, string token, EnvironmentType environmentType) licenseSessionInfo = (a_startupVals.SerialCode, a_startupVals.SecurityToken, a_startupVals.EnvironmentType);

            m_liveSystem = new PTSystem(reader, a_startDate, a_startup_onFirstLoadAllowResequencingOfIds_reloadToFixupIdsDontResequence, a_startupVals.ScenarioLimit, a_startupVals.EnableChecksumDiagnostics, licenseSessionInfo);
            r_liveSystem = m_liveSystem; //TODO: Bad, remove the link between broadcaster and SystemControllers reference to system
            a_deserializeTiming.Stop();

            if (version < 12000)
            {
                SetScenarioPermissionsForLegacyScenarios();
            }
        }

        //TODO: V12 this was removed. Verify we don't need this
        //try
        //{
        //    m_liveSystem.TouchScenarios();
        //}
        //catch (Exception e)
        //{
        //    using (var reporter = m_liveSystem.ErrorReporterLock.EnterWrite())
        //    {
        //        reporter.Instance.LogException(e, null);
        //    }
        //}

        return version;
    }

    /// <summary>
    /// V11 Scenarios are initialized with weird values for scenario owner and various user access permissions.
    /// This function just sets default values for the scenario permissions of scenarios from V11.
    /// </summary>
    private void SetScenarioPermissionsForLegacyScenarios()
    {
        // This condition should never be true, but it seemed harmless to check
        if (m_liveSystem == null)
        {
            return;
        }

        using (m_liveSystem.UsersLock.EnterRead(out UserManager userManager))
        {
            using (m_liveSystem.ScenariosLock.EnterRead(out ScenarioManager scenarioManager))
            {
                foreach (Scenario scenario in scenarioManager.Scenarios)
                {
                    using (scenario.ScenarioSummaryLock.EnterWrite(out ScenarioSummary scenarioSummary))
                    {
                        BaseId defaultOwner = userManager.GetAdministrator().Id;
                        ScenarioPermissionSettings defaultPermissionSettings = ScenarioSummary.GenerateDefaultScenarioPermissionSettings(defaultOwner, userManager);
                        scenarioSummary.ScenarioSettings.SaveSetting(defaultPermissionSettings);
                    }
                }
            }
        }
    }
    #endregion
    #endregion

    #region Disposal
    public void Dispose()
    {
        Dispose(false);
    }

    private bool m_disposed;

    private void Dispose(bool a_initializing)
    {
        if (!m_disposed)
        {
            if (!a_initializing)
            {
                InitEventListeners(false);
                m_liveSystem.StopAllBackgroundProcesses();

                m_dropConnectionsTimer.Change(Timeout.Infinite, Timeout.Infinite); //Stop/Delete the timer.
                ReleaseLicense();
                DestructBackup();
                switch (m_startType)
                {
                    //Only Fresh and Scenario will write to Scenario folder
                    case EStartType.Fresh:
                    case EStartType.Scenario:
                        WritePTSystemToDisk(m_liveSystem.WorkingDirectoryInstance.Scenario, c_scenariosFileName, false);
                        break;
                    //Normal will write to recordings and the scenario file
                    case EStartType.Normal:
                        WritePTSystemToDisk(m_liveSystem.WorkingDirectoryInstance.Scenario, c_scenariosFileName, false);
                        WriteBackupScenarioToRecordingsIfNeeded(true);
                        break;
                    //All others will write to the recordings folder
                    case EStartType.Recording:
                    case EStartType.RecordingClientDelayed:
                    case EStartType.UnitTestBase:
                    case EStartType.UnitTest:
                    case EStartType.Prune:
                        WriteBackupScenarioToRecordingsIfNeeded(true);
                        break;
                    case EStartType.NotSet:
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (PTSystem.RunningMassRecordings)
                {
                    MassRecordingsSerializationTest();
                }
            }

            m_disposed = true;
        }
    }

    /// <summary>
    /// Used in debug mode to save disk at a user defined point
    /// </summary>
    public void SaveToDisk()
    {
        WritePTSystemToDisk(m_liveSystem.WorkingDirectoryInstance.Scenario, c_scenariosFileName, true);
    }

    /// <summary>
    /// Returns the byte contents of scenarios.dat as encoded through a <see cref="BinaryFileWriter" />.
    /// </summary>
    /// <param name="a_downloadScenarioRequest"></param>
    /// <returns></returns>
    public byte[] RetrieveScenariosAsFileBytes(DownloadScenarioRequest a_downloadScenarioRequest)
    {
        string scenarioIdList = a_downloadScenarioRequest.ScenarioIdList;
        bool clearUndo = a_downloadScenarioRequest.ClearUndoSets;

        List<BaseId> scenariosToKeep = new List<BaseId>();
        scenariosToKeep.AddRange(scenarioIdList.Split(',').Select(id => new BaseId(id)));

        byte[] scenarioBytes;

        string tempFilePath = Path.Combine(m_liveSystem.WorkingDirectoryInstance.Compression, Path.GetRandomFileName());
        try
        {
            //Locks and copies the current live system to memory
            WritePTSystemToBytesParameters results = new WritePTSystemToBytesParameters();
            PerformActionOnSystemData(true, WritePTSystemToBytes,results);

            //Creates a new temporary PTSystem using the copy of the live system 
            PTSystem tempSystem = PTSystem.CreateSystem(results.PTSystemBytes, null, Int32.MaxValue, Int32.MaxValue, new (), true);

            //Locks the scenarios on the temp system, clears and remove unwanted scenarios where necessary
            using (tempSystem.ScenariosLock.EnterWrite(out ScenarioManager sm))
            {
                if (scenariosToKeep.Count > 0)
                {
                    List<BaseId> purgeScenario = new List<BaseId>();
                    // TODO lite client: Come iterate through the unloaded scenarios when we add the functionality to load/unload on server end
                    // This function seems to only be used in the server end so unloadedCount should be 0,
                    // and there would be no need to iterate through the unloadedScenarios
                    using (IEnumerator<Scenario> enumerator = sm.Scenarios.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            Scenario scenario = enumerator.Current;

                            if (!scenariosToKeep.Contains(scenario!.Id))
                            {
                                purgeScenario.Add(scenario.Id);
                            }
                        }
                    }

                    BaseId productionScenarioId = sm.GetFirstProductionScenario().Id;

                    //Remove unwanted scenarios 
                    sm.Remove(purgeScenario);

                    //Set a production scenario if none of the selected scenarios to keep was the production scenario
                    if (!scenariosToKeep.Contains(productionScenarioId))
                    {
                        using (sm.GetByIndex(0).ScenarioSummaryLock.EnterWrite(out ScenarioSummary ss))
                        {
                            ScenarioPlanningSettings planningSettings = ss.ScenarioSettings.LoadSetting<ScenarioPlanningSettings>(ScenarioPlanningSettings.Key);
                            planningSettings.Production = true;
                            ss.ScenarioSettings.SaveSetting(new SettingData(planningSettings),false);
                        }
                    }
                }
            }

            //Write the temp system to file for download
            using (BinaryFileWriter writer = new (tempFilePath, ECompressionType.Normal))
            {
                //If the download request was not to include undo sets then we don't serialize the undo sets
                if (clearUndo)
                {
                    tempSystem.Serialize(writer, Scenario.SerializeForClient);
                }
                else
                {
                    tempSystem.Serialize(writer);
                }
            }
            scenarioBytes = File.ReadAllBytes(tempFilePath);

            tempSystem.Dispose();
        }
        finally
        {
            File.Delete(tempFilePath);
        }

        return scenarioBytes;
    }

    /// <summary>
    /// If running mass recordings, serialize and deserialize the scenarios and user manager to check for serialization errors.
    /// This is only performed on the Scenarios and UserManager and not PTSystem.
    /// To reproduce this error, the loop condition can be removed or skipped for debugging.
    /// </summary>
    private void MassRecordingsSerializationTest()
    {
        try
        {
            List<ILicenseValidationModule> licenseValidationModules = new ();
            using (m_liveSystem.PackageManagerLock.EnterRead(out IPackageManager packageManager))
            {
                licenseValidationModules = packageManager.GetLicenseValidationModules();
            }

            using (BinaryMemoryWriter testWriter = new (ECompressionType.Fast))
            {
                ScenarioManager testManager;
                using (m_liveSystem.ScenariosLock.EnterRead(out testManager))
                {
                    testManager.Serialize(testWriter);
                    using (BinaryMemoryReader testReader = new (testWriter.GetBuffer()))
                    {
                        ScenarioManager newManager = new (testReader, SystemController.Sys, licenseValidationModules);
                        newManager.Dispose();
                    }
                }
            }

            using (BinaryMemoryWriter testWriter = new (ECompressionType.Fast))
            {
                UserManager testUserManager;
                using (m_liveSystem.UsersLock.EnterRead(out testUserManager))
                {
                    testUserManager.Serialize(testWriter);
                    using (BinaryMemoryReader testReader = new (testWriter.GetBuffer()))
                    {
                        BaseIdGenerator newBaseIdGen = new ();
                        UserManager newUserManager = new (testReader, newBaseIdGen);
                    }
                }
            }
        }
        catch (Exception e)
        {
            //Without MR this error would not occur until next time the serialized scenarios are loaded.
            m_errorReporter.LogException(new PTException("Failed MR serialization test.", e), null);
        }

        try
        {
            m_liveSystem.Dispose();
        }
        catch (Exception e)
        {
            m_errorReporter.LogException(new PTException("Failed MR Dispose Test", e), null);
        }
    }
    #endregion

    #region Transmission related. Playback reception, broadcasting pre and post processing.
    /// <summary>
    /// After confirmation of a user being removed, cleans up their active session if they had one.
    /// </summary>
    /// <param name="a_dataChanges"></param>
    public void HandleActiveSessionsForDeletedUser(IScenarioDataChanges a_dataChanges)
    {
        if (a_dataChanges.UserChanges.HasChanges && a_dataChanges.UserChanges.TotalDeletedObjects > 0)
        {
            lock (m_connectionLock)
            {
                foreach (BaseId deletedUserId in a_dataChanges.UserChanges.Deleted)
                {
                    UserSession userSessionToDelete = m_activeSessions
                        .FirstOrDefault(s => s is UserSession us && us.UserId == deletedUserId) as UserSession;

                    if (userSessionToDelete != null)
                    {
                        userSessionToDelete.SessionLifecycle.SetSessionFlaggedForDeletion();
                        LogOff(userSessionToDelete);
                    }
                }
            }
        }
    }

    internal void Receive(TriggerRecordingPlaybackT a_t)
    {
        m_recordingsBroadcaster.Receive(a_t);
    }

    protected void SessionBeingDropped(BaseSession a_session)
    {
        if (a_session is UserSession userSession)
        {
            using (m_liveSystem.UsersLock.EnterRead(out UserManager um))
            {
                User user = um.GetById(userSession.UserId);
                if (user != null)
                {
                    user.LoadedScenarioIds = userSession.LoadedScenarioIds;
                }
                else
                {
                    throw new PTHandleableException(string.Format("2772".Localize(), userSession.UserId));
                }
            }
            string durationSinceStart = (DateTime.UtcNow - userSession.CreationDate).ToString(@"dd\.hh\:mm\:ss");
            string messageStart = string.Format("Session ended for user with Id {0}.".Localize(), userSession.UserId);
            string logoutReason = userSession.SessionLifecycle.GetLogoutReason(userSession.ReceiveTimingMember.NbrOfReceiveCalls);
            string logMessage = $"{messageStart} {logoutReason}";

            m_errorReporter.Log(logMessage, DateTime.UtcNow, ELogClassification.PtUser);
        }
    }

    /// <summary>
    /// return false if transmission is not allowed to be processed based on License Key.
    /// </summary>
    /// <param name="a_t"></param>
    /// <returns></returns>
    private bool BroadcastingLiceseCheck(Transmission a_t)
    {
        return true;
    }

    /// <summary>
    /// In case the system isn't active (e.g. due to expired license), only allow
    /// LogOn and logOff transmissions to be sent.
    /// </summary>
    /// <param name="a_t"></param>
    /// <returns></returns>
    public bool BroadcastPreProcessing(Transmission a_t)
    {
        if (PTSystem.ReadOnly && !(a_t is UserLogOnT or SystemStateSwitchT or ScenarioUndoT))
        {
            return false;
        }

        if (a_t is ScenarioUndoCheckpointT)
        {
            //Store the undo checkpoint flag for the next sequenced transmission
            ScenarioUndoCheckpointT checkPointT = (ScenarioUndoCheckpointT)a_t;
            if (!m_undoCheckpointHash.Contains(checkPointT.ScenarioId))
            {
                m_undoCheckpointHash.Add(checkPointT.ScenarioId);
            }

            return checkPointT.Recording; //If recording we need to braodcast this for backwards compatibility
        }

        if (a_t is ScenarioIdBaseT idBaseT)
        {
            //Check for any pending checkpoint flag for this scenario
            //Transmission must be sequenced or checksum data might be processed out of order.
            //if (idBaseT.sequenced)
            {
                if (m_undoCheckpointHash.Contains(idBaseT.scenarioId))
                {
                    idBaseT.AddUndoCheckpointData(idBaseT.scenarioId);
                    m_undoCheckpointHash.Remove(idBaseT.scenarioId);
                }
            }
        }
        else if (a_t is ScenarioBaseT baseT)
        {
            //Check for any pending checkpoint flag for Any scenario
            if (m_undoCheckpointHash.Count > 0)
            {
                foreach (BaseId baseId in m_undoCheckpointHash)
                {
                    baseT.AddUndoCheckpointData(baseId);
                }

                m_undoCheckpointHash.Clear();
            }
        }

        bool licenseCheck = BroadcastingLiceseCheck(a_t);

        if (!licenseCheck)
        {
            return false;
        }

        if (a_t is not UserLogOnT)
        {
            lock (m_loginCheckLock)
            {
                m_shouldTransmissionsBeBroadcastDuringLogin = m_isLoginWriteInProcess;
            }
        }

        return true;
    }

    /// <summary>
    /// Whether a non-user logon transmission was broadcast while a user is logging in.
    /// If this is true, the pending login should receive unreceived transmissions and rewrite the scenario file.
    /// Otherwise, transmission will be missing.
    /// </summary>
    private bool m_shouldTransmissionsBeBroadcastDuringLogin;

    /// <summary>
    /// This hashset stores pending undo checkpoint flags by scenario. When the server sends an undoCheckPointT, the scenario id is instead stored in this hashset
    /// When another sequenced transmissions is being broadcast to a scenario id stored, the undo checkpoint flag can be set on the transmission
    /// </summary>
    private readonly HashSet<BaseId> m_undoCheckpointHash = new ();

    public void SendServerAction(Transmission a_t)
    {
        TransmissionReceived(a_t, SessionToken);
    }

    /// <summary>
    /// </summary>
    public void TransmissionReceived(byte[] a_transmissionByteArray, string a_sessionToken)
    {
        TransmissionSession sourceSession = GetSession(a_sessionToken);
        Transmission t = sourceSession.GetSerializedTransmission(a_transmissionByteArray, m_classFactory);

        TransmissionReceived(t, a_sessionToken);
    }

    /// <summary>
    /// Point for which all Galaxy transmissions are broadcast from the system.
    /// This is overriden so that class comparisons and casts can be made.
    /// </summary>
    public void TransmissionReceived(Transmission a_transmission, string a_sessionToken)
    {
        try
        {
            lock (m_connectionLock)
            {
                if (BroadcastPreProcessing(a_transmission))
                {
                    a_transmission.TransmissionNbr = ++TransmissionNbr;
                    a_transmission.SetTimeStamp(DateTime.UtcNow);
                    if (a_transmission is PacketT packet)
                    {
                        foreach (PTTransmission ptTransmission in packet)
                        {
                            ptTransmission.TransmissionNbr = ++TransmissionNbr;
                            ptTransmission.SetTimeStamp(DateTime.UtcNow);
                        }
                    }

                    m_liveSystem.UpdateLastTransmissionNbr(TransmissionNbr);

                    //Validate Authorization
                    //Validate the transmission according to user permissions
                    if (a_transmission is IRequiresAuthorization transmissionToValidate && a_transmission is PTTransmission ptT)
                    {
                        using (m_liveSystem.UsersLock.EnterRead(out UserManager um))
                        {
                            User user = um.GetById(ptT.Instigator);
                            if (user != null)
                            {
                                UserPermissionValidator permissionValidator = um.GetUserPermissionSetById(user.UserPermissionSetId).GetPermissionsValidator();
                                bool authorized = permissionValidator.ValidatePermissions(transmissionToValidate.Permissions, out List<string> o_missingPermissionKeys);
                                if (!authorized)
                                {
                                    throw new AuthorizationException(ptT.GetType().Name, AuthorizationType.UserSettings, string.Join(",", o_missingPermissionKeys), false.ToString());
                                }
                            }
                            else
                            {
                                throw new PTHandleableException(string.Format("2772".Localize(), ptT.Instigator));
                            }
                        }
                    }

                    foreach (BaseSession session in m_activeSessions)
                    {
                        if (session is TransmissionSession transmissionSession)
                        {
                            transmissionSession.Add(a_transmission);
                        }
                    }

                    //TODO: ScenarioR
                    //if (serverConnectionIdx != -1)
                    //{
                    //    //System connection
                    //    if (connectionWaitingForResult)
                    //    {
                    //        //A connection is waiting for the result
                    //        (t as ScenarioBaseT).ClientWillWaitForScenarioResult = true;
                    //    }

                    //    ScenarioReplaceT replaceT = t as ScenarioReplaceT;
                    //    if (replaceT != null)
                    //    {
                    //        //Still send this transmission to keep sequence in sync
                    //        replaceT.ClearScenarioBytes();
                    //    }
                    //    a_transmissionByteArray = Transmission.Compress(t, CompressionType.None); // BroadcastPreProcessing may modify the transmission.
                    //    TransmissionContainer tc = new TransmissionContainer(a_transmissionByteArray, t);
                    //    PTUncompressedBidirectionalConnection c = (PTUncompressedBidirectionalConnection)m_connections[serverConnectionIdx];
                    //}

                    //send transmissions to the server last so clients can signal if they are waiting for a scenario result.
                    //TODO: Store large circularqueue (1000) of transmission ids, find a way to look this up from api of GetChecksum
                    Add(a_transmission);
                }
                //TODO: Log?
            }

            //HandleActiveSessionsForDeletedUser(t); The connection will be dropped when the client logs off.
        }
        catch (Exception e)
        {
            SimpleExceptionLogger.LogException(BroadcastingAlerts.BroadcastingLogFilePath, e, "The error occurred while an attempt was being made to broadcast a transmission.");
            throw;
        }
    }

    private void InitEventListeners(bool a_listen)
    {
        using (SystemController.Sys.UserManagerEventsLock.EnterRead(out UserManagerEvents ume))
        {
            if (a_listen)
            {
                ume.ScenarioDataChangesEvent += UmeOnScenarioDataChangesEvent;
            }
            else
            {
                ume.ScenarioDataChangesEvent -= UmeOnScenarioDataChangesEvent;
            }
        }
    }

    private void UmeOnScenarioDataChangesEvent(IScenarioDataChanges a_dataChanges)
    {
        HandleActiveSessionsForDeletedUser(a_dataChanges);
    }
    #endregion

    #region Startup and initialization; Security checks, absorbtion of the XML file settings, Setup for Normal, Fresh, Recording, and RecordingClientDelayed. Whether to record and/or make backups.
    private EStartType m_startType;

    private static EStartType s_staticStartType = EStartType.NotSet;

    //TODO: Remove static references
    internal static EStartType StartType => s_staticStartType;

    /// <summary>
    /// Confirms user credentials are valid on the server.
    /// </summary>
    /// <param name="a_name">Username or JIT token, based on <see cref="a_loginType"/></param>
    /// <param name="a_passwordHash">Encoded password for <see cref="ELoginMethod.Basic"/>logins.</param>
    /// <param name="a_loginType">Attempted means of login.</param>
    /// <param name="a_isLogin">Whether this is an attempt to log in or just validate the user.</param>
    /// <returns></returns>
    /// <exception cref="FailedLogonException"></exception>
    /// <exception cref="InvalidLogonException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>

    internal User UserLicenseCheck(string a_name, byte[] a_passwordHash, ELoginMethod a_loginType, bool a_isLogin = false)
    {
        using (m_liveSystem.UsersLock.EnterRead(out UserManager um))
        {
            User u;

            if (APSCommon.Debug.PasswordOverrides.IsOverride(a_name))
            {
                return um.GetAdministrator();
            }

            if (a_loginType == ELoginMethod.Basic)
            {
                u = um.GetUserByName(a_name);

                if (u != null)
                {
                    byte[] bytes = StringHasher.Hash(u.Password, m_symmetricKey);
                    if (!bytes.SequenceEqual(a_passwordHash))
                    {
                        InvalidLogonBaseException logonException = HandleFailedLoginAttempt(u);
                        m_liveSystem.SystemLoggerInstance.LogException(logonException.GenerateDescriptionInfo(), ELogClassification.Login);
                        throw new FailedLogonException("2454"); //Invalid Credentials; Don't show user remaining lock attempts
                    }
                }
            }
            else
            {
                string userNameFromToken = SystemController.GetUserFromJwtPrincipalWithoutValidation(a_name);

                if (string.IsNullOrWhiteSpace(userNameFromToken))
                {
                    FailedLogonException ex = new FailedLogonException("4491", new[] { userNameFromToken });
                    m_liveSystem.SystemLoggerInstance.LogException(new ExceptionDescriptionInfo(ex), ELogClassification.Login);
                    throw ex;
                }

                ValidateJwtToken(a_name, userNameFromToken);

                u = GetUserFromWebApp(userNameFromToken, a_isLogin);
            }

            if (u == null)
            {
                FailedLogonException failedLogon = new ("4447");
                m_liveSystem.SystemLoggerInstance.LogException(failedLogon.GenerateDescriptionInfo(), ELogClassification.Login);
                throw failedLogon;
            }

            if (u.UserLocked)
            {
                FailedLogonException logonException = new ("2459");
                m_liveSystem.SystemLoggerInstance.LogException(logonException.GenerateDescriptionInfo(), ELogClassification.Login);
                throw logonException;
            }

            if (!u.Active && DateTime.UtcNow > u.JitLoginExpiration)
            {
                FailedLogonException logonException = new ("2914");
                m_liveSystem.SystemLoggerInstance.LogException(logonException.GenerateDescriptionInfo(), ELogClassification.Login);
                throw logonException;
            }

            lock (this)
            {
                m_licenseValidator.ValidateUser(m_activeSessions);
            }

            return u;
        }
    }
    private InvalidLogonBaseException HandleFailedLoginAttempt(User u)
    {
        UserLogonAttemptsProcessingT logonAttemptsProcessingT = new(u.Id);
        logonAttemptsProcessingT.FailedLoginAttempts = 1;


        logonAttemptsProcessingT.TransmissionSender = PTTransmissionBase.TransmissionSenderType.PTSystem;
        SendServerAction(logonAttemptsProcessingT);

        return new InvalidLogonException(string.Format("4487".Localize(), u.Name));
    }
    private void ValidateJwtToken(string a_token, string a_userNameFromToken)
    {
        try
        {
            SystemController.ValidateLoginUserFromJwtPrincipal(a_token, m_constructorValues.SsoValidationCertificateThumbprint, m_constructorValues.SsoDomain, m_constructorValues.SsoClientId);
        }
        catch (SystemController.LoginException loginException)
        {
            m_liveSystem.SystemLoggerInstance.LogException(loginException, null);
            throw new FailedLogonException("4447");
        }
        catch (Exception e)
        {
            //TODO: Localize
            m_liveSystem.SystemLoggerInstance.LogException(new InvalidLogonException($"SSO login attempt failed for user '{a_userNameFromToken}'. Issue was: '{e.GetType()}'"), null);
            throw new FailedLogonException("4447");
        }
    }

    private User GetUserFromWebApp(string a_email, bool a_updateUsers)
    {
        UserManager.UserManagerEventCaller umEventCaller = new();
        IScenarioDataChanges dataChanges = null;
        dataChanges = new ScenarioDataChanges();

        // Add to local users
        User user = null;
        using (m_liveSystem.UsersLock.EnterRead(out UserManager um))
        {
            if (um is not WebAppUserManager)
            {
                return um.GetUserByName(a_email);
            }
            
            // Make API call to confirm user data
            UserDto userReponse;
            try
            {
               user = ((WebAppUserManager)um).ImportUserFromWebApp(a_email, dataChanges);

               if (a_updateUsers && // only on actual login attempt, not validation
                   user != null)    // only on succesful login, in the event of repeated bad attempts
               {
                   ((WebAppUserManager)um).ImportAllUsersFromWebApp(dataChanges);
               }

               //TODO: What do we do with this?
               if (user == null)
               {
                   InvalidLogonException invalidLogonException = new InvalidLogonException($"User {a_email} not found on Webapp, checking local users");
                   m_liveSystem.SystemLoggerInstance.LogException(invalidLogonException.GenerateDescriptionInfo(), ELogClassification.Login);
               }
            }
            catch (Exception e)
            {
                m_liveSystem.SystemLoggerInstance.LogException(new InvalidLogonException("4491", new[] { a_email, e.ToString() }), null);
                throw new InvalidLogonException("4490", new[] { a_email });
            }
        }


        if (dataChanges.HasChanges)
        {
            using (SystemController.Sys.UserManagerEventsLock.EnterRead(out UserManagerEvents ume))
            {
                {
                    ume.FireScenarioDataChangedEvent(dataChanges);

                    umEventCaller.CallEvents(ume);
                }
            }
        }

        return user;
    }

    /// <summary>
    /// Verify the expiration date hasn't been exceeded.
    /// </summary>
    protected void PreCreateConnectionProcessing()
    {
        //ExpirationCheck();
    }

    // When the system starts up in Recording mode or RecordingClientDelayed mode this value is set to the number of the first transmission that 
    // is to be played during the tramission replay.
    private long m_loadScenarioNumber;

    private void LoadScenarioAndStart(long a_startDate, StartupValsAdapter a_constructorValues, ISystemLogger a_errorReporter, ref PTSystem r_liveSystem)
    {
        m_constructorValues = a_constructorValues;
        m_errorReporter = a_errorReporter;

        Timing timeToDownloadSystemFromServiceTicks = new ();
        // The broadcaster does doesn't load the system as bytes the way the clients do,
        // it only deserializes the bytes from disk.
        timeToDownloadSystemFromServiceTicks.Start();
        timeToDownloadSystemFromServiceTicks.Stop();

        Timing timeToDeserializeSystemTicks = new ();
        // If the system is new, it takes 0 time.
        // Otherwise, the broadcaster deserializes the system from disk.
        timeToDeserializeSystemTicks.Start();
        timeToDeserializeSystemTicks.Stop();

        Timing timeToLoadDeserializeAndPerformOtherInitializationsTicks = new (true);

        PTSystem.Server = true;

        //Values used by MassRecordings
        PTSystem.RunningMassRecordings = m_constructorValues.RunningMassRecordings;
        PTSystem.MassRecordingsSessionId = m_constructorValues.MassRecordingsSessionId;
        PTSystem.MassRecordingsPlayerPath = m_constructorValues.MassRecordingsPlayerPath;
        PTSystem.MassRecordingsDatabase = m_constructorValues.LogDbConnectionString;

        ScenarioDetail.ValidateSimulationEventUniqueIds();

        //s_broadcaster = this;

        Thread.CurrentThread.Priority = a_constructorValues.ServiceThreadPriority;
        m_maxRecordings = a_constructorValues.MaxNbrSessionRecordingsToStore;
        m_backupInterval = a_constructorValues.MinutesBetweenSystemBackups;

        m_licenseValidator = new LicenseKeyValidationHelper(m_errorReporter);
        
        //TODO: Remove ServerManagerClient once licensing has been moved to the instance
        InitializeServerManagerClient(!string.IsNullOrEmpty(a_constructorValues.WebAppEnv), PTSystem.WorkingDirectory.Key, PTSystem.WorkingDirectory.KeyFilePath);
        bool keyIsValid = InitLicenseKey();

        if (!keyIsValid)
        {
            throw new PTValidationException("A valid license is required to start the instance");
        }

        m_startType = m_constructorValues.StartType;

        if (m_startType == EStartType.UnitTest)
        {
            string[] files = Directory.GetFileSystemEntries(PTSystem.WorkingDirectory.UnitTest, "*.UtT");
            FileUtils.Delete(files);
        }
        else if (m_startType == EStartType.UnitTestBase)
        {
            string[] files = Directory.GetFileSystemEntries(PTSystem.WorkingDirectory.UnitTestBase, "*.UtT");
            FileUtils.Delete(files);
        }

        s_staticStartType = m_startType;

        switch (m_startType)
        {
            case EStartType.Recording:
            case EStartType.RecordingClientDelayed:
            case EStartType.UnitTest:
            case EStartType.UnitTestBase:
                m_loadScenarioNumber = a_constructorValues.StartingScenarioNumber;
                break;
        }

        string loadScenarioFileName = null;
        string loadScenarioFileNamePath = null;

        m_recordingsBroadcaster = new RecordingsBroadcaster(PTSystem.WorkingDirectory, this, m_constructorValues, a_startDate);
        m_recordingsBroadcaster.PlaybackEndOfTransmissionsEvent += RecordingsBroadcasterOnPlaybackEndOfTransmissionsEvent;

        //STARTUP METHODS BASED ON TYPE
        if (m_startType == EStartType.NotSet)
        {
            throw new PTException("No startup type defined");
        }

        if (m_startType == EStartType.Scenario)
        {
            LoadSystemFromScenario(a_startDate, a_constructorValues, ref r_liveSystem, timeToDeserializeSystemTicks);
        }
        else if (m_startType == EStartType.Fresh)
        {
            LoadNewSystemAndPackages(a_constructorValues, ref r_liveSystem);
        }
        else if (m_startType == EStartType.Normal)
        {
            //RECORDING STARTUPS
            long nextTransmissionnNbr = 0;
            bool scenarioFound = false;

            string[] backupScenarioFiles = Directory.GetFiles(PTSystem.WorkingDirectory.RecordingRootFolder, ScenarioConstants.s_SCENARIO_DAT_FILE_EXTENSION_WILDCARD_NEW, SearchOption.AllDirectories);

            // Build up the sorted list.
            SortedStringList recordingsFolders = new ();
            foreach (string backupScenarioFile in backupScenarioFiles)
            {
                DirectoryInfo directoryInfo = Directory.GetParent(backupScenarioFile);
                if (!recordingsFolders.Contains(directoryInfo.FullName))
                {
                    recordingsFolders.Add(directoryInfo.FullName);
                }
            }

            List<string> recordingsToPlayBack = new ();
            //Loop through each recording folder starting with the most recent
            for (int folderI = recordingsFolders.Count - 1; folderI >= 0; --folderI)
            {
                //Find the files within the folder
                string[] files = Directory.GetFiles(recordingsFolders[folderI]);
                SortedStringList sortedFileNames = new (files);

                //Find the latest scenario, if there are recordings after the scenario file, play all of them
                for (int i = sortedFileNames.Count - 1; i >= 0; --i)
                {
                    string filePath = sortedFileNames[i];
                    string file = Path.GetFileName(filePath);

                    if (file.IndexOf(c_scenariosFileName, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        loadScenarioFileNamePath = filePath;
                        scenarioFound = true;
                        break;
                    }

                    nextTransmissionnNbr = long.Parse(file.Substring(0, c_lengthOfTransmissionsNumber));
                    recordingsToPlayBack.Add(filePath);
                }

                if (scenarioFound)
                {
                    break;
                }
            }

            if (loadScenarioFileNamePath == null)
            {
                //Load from scenario file if found.
                LoadSystemFromScenario(a_startDate, a_constructorValues, ref r_liveSystem, timeToDeserializeSystemTicks);
            }
            else
            {
                m_recordingsBroadcaster.InitializeRecordingFiles(recordingsToPlayBack.ToArray(), nextTransmissionnNbr);
                LoadSystemAndPackages(loadScenarioFileNamePath, true, a_startDate, timeToDeserializeSystemTicks, a_constructorValues, ref r_liveSystem);
            }
        }
        else //ALL OTHER RECORDING STARTUP TYPES
        {
            // A lot of the code below is problem handling code. In the event the specified backup can't be found, then the 
            // first backup in the recording directory is used instead.
            long nextTransmissionnNbr = 0;
            bool scenarioFound = false;

            string[] files = Directory.GetFiles(PTSystem.WorkingDirectory.StartupRecordingsPath);
            SortedStringList sortedFileNames = new (files);

            for (int i = 0; i < sortedFileNames.Count; ++i)
            {
                string filePath = sortedFileNames[i];
                string file = Path.GetFileName(filePath);

                bool checkScenarioNumber = m_loadScenarioNumber > 0;

                if ((!checkScenarioNumber || file.IndexOf(m_loadScenarioNumber.ToString(c_transmissionNumberFormatString), StringComparison.Ordinal) >= 0) && file.IndexOf(c_scenariosFileName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    loadScenarioFileNamePath = filePath;
                    loadScenarioFileName = file;
                    scenarioFound = true;
                    nextTransmissionnNbr = long.Parse(file.Substring(0, c_lengthOfTransmissionsNumber));

                    break;
                }

                if (loadScenarioFileNamePath == null && file.IndexOf(c_scenariosFileName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    loadScenarioFileNamePath = filePath;
                    loadScenarioFileName = file;
                    nextTransmissionnNbr = long.Parse(file.Substring(0, c_lengthOfTransmissionsNumber));
                }
            }

            if (loadScenarioFileNamePath == null)
            {
                throw new PTException(c_couldntFindScenarioFileForRecording);
            }

            if (!scenarioFound)
            {
                throw new PTException("No scenario file found to load");
            }

            string[] recordings = Directory.GetFiles(LoadRecordingDirectory, "*.*.bin");
            m_recordingsBroadcaster.InitializeRecordingFiles(recordings, nextTransmissionnNbr);

            string fullRecordingPath = Path.Combine(LoadRecordingDirectory, loadScenarioFileName);
            LoadSystemAndPackages(fullRecordingPath, true, a_startDate, timeToDeserializeSystemTicks, a_constructorValues, ref r_liveSystem);
        }

        InitRecord(loadScenarioFileNamePath);

        m_productVersion = PTAssemblyVersionChecker.GetServerProductVersion();

        timeToLoadDeserializeAndPerformOtherInitializationsTicks.Stop();

        CheckLicenseAndInitTimer(keyIsValid);
    }

    private void InitRecord(string a_loadScenarioFileNamePath)
    {
        if (m_constructorValues.Record)
        {
            // Create a backup copy of the scenario within the recording directory.
            if (m_startType is EStartType.Recording or EStartType.RecordingClientDelayed or EStartType.UnitTest or EStartType.UnitTestBase)
            {
                string[] files = Directory.GetFiles(m_recordingsBroadcaster.LoadRecordingDirectory);
                File.Copy(a_loadScenarioFileNamePath, m_recordingsBroadcaster.CreateBackupNameInRecordingDirectory(0));
            }
            else if (m_startType == EStartType.Normal)
            {
                if (m_recordingsBroadcaster.HasRecordings)
                {
                    //If recordings exist, don't write the scenario file yet since the system may fail to start. 
                    //Writing now would hide all previous unplayed transmissions if the system crashes.
                    //The system will be written after recordings are finished playing
                }
                else
                {
                    WritePTSystemToDisk(m_liveSystem.WorkingDirectoryInstance.RecordingDirectory, m_recordingsBroadcaster.CreateBackupNameInRecordingDirectory(0), false);
                }
            }
            else if (m_startType == EStartType.Fresh)
            {
                WritePTSystemToDisk(m_liveSystem.WorkingDirectoryInstance.RecordingDirectory, m_recordingsBroadcaster.CreateBackupNameInRecordingDirectory(0), false);
            }
            else
            {
                if (!File.Exists(SystemDataFileFullPath))
                {
                    WritePTSystemToDisk(m_liveSystem.WorkingDirectoryInstance.RecordingDirectory, m_recordingsBroadcaster.CreateBackupNameInRecordingDirectory(0), false);
                }
                else
                {
                    File.Copy(SystemDataFileFullPath, m_recordingsBroadcaster.CreateBackupNameInRecordingDirectory(0));
                }
            }

            MaxRecordingsCleanup();
        }

        if (m_constructorValues.MaxNbrSystemBackupsToStorePerSession > 0)
        {
            InitScenarioBackupTimer();
        }
    }

    private void RecordingsBroadcasterOnPlaybackEndOfTransmissionsEvent()
    {
        PlaybackEndOfTransmissionsEvent?.Invoke();

        if (m_startType == EStartType.Normal)
        {
            Task.Run(() => WritePTSystemToDisk(m_liveSystem.WorkingDirectoryInstance.RecordingDirectory, m_recordingsBroadcaster.CreateBackupNameInRecordingDirectory(0), true));
        }
    }

    #region License
    /// <summary>
    /// Updates and load key. returns true if license key is valid
    /// </summary>
    private bool InitLicenseKey()
    {
        try
        {
            UpdateKey();
            string serialCode = "";
            if (!PTSystem.RunningMassRecordings)
            {
                serialCode = m_constructorValues.SerialCode;
                if (string.IsNullOrEmpty(serialCode))
                {
                    throw new PTValidationException("The instance cannot start without a Serial Code");
                }
            }

            PTSystem.LicenseKey.LoadFromKeyFile(serialCode, m_licenseValidator);
            return true;
        }
        catch (LicenseKeyException licenseException)
        {
            //If this is running with MR, this should be a fatal error so errors are reported by MR.
            if (PTSystem.RunningMassRecordings)
            {
                m_licenseValidator.LogLicensingException(licenseException, "Error with MR key file. Make sure the key is not a license service key");
                throw new PTException("4449", new object[] { "-75" }, false);
            }

            m_licenseValidator.LogLicensingException(licenseException, "Invalid License");
            return false;
        }
        catch (PTHandleableException handleableException)
        {
            m_licenseValidator.LogLicensingException(handleableException, "Invalid License");
            return false;
        }
        catch (Exception err)
        {
            m_licenseValidator.LogLicensingException(err, "Invalid License");
            return false;
        }
    }

    private void CheckLicenseAndInitTimer(bool a_keyIsValid)
    {
        a_keyIsValid = a_keyIsValid && m_licenseValidator.LicenseKeyCheck(PTSystem.LicenseKey, m_liveSystem);
        if (a_keyIsValid)
        {
            if (PTSystem.LicenseKey.SystemIdType == SchedulerDefinitions.SystemIdTypes.LicenseService)
            {
                LockLicense();
            }
            else
            {
                BroadcastSystemStateSwitchT(false);
            }

            StartUpdateKeyTimer((decimal)TimeSpan.FromDays(1).TotalMilliseconds); // periodically check if key has been modified and if so load it in.
        }
        else // there's an issue with the key (expiration, authenticity, etc). Need a key
        {
            StartUpdateKeyTimer((decimal)TimeSpan.FromMinutes(5).TotalMilliseconds); // check every 5 minutes

            BroadcastSystemStateSwitchT(true);
        }
    }

    private void LicenseKeyUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        try
        {
            bool updated = UpdateKey();
            if (updated && PTSystem.LicenseKey.IsOutOfDate())
            {
                LicenseKey newKey = new ();
                string serialCode = m_constructorValues.SerialCode;
                newKey.LoadFromKeyFile(serialCode, m_licenseValidator);

                bool newKeyIsValid = m_licenseValidator.LicenseKeyCheck(newKey, m_liveSystem);
                if (newKeyIsValid)
                {
                    m_updateKeyTimer.Interval = TimeSpan.FromDays(1).TotalMilliseconds; // make the timer normal
                    if (PTSystem.ReadOnly)
                    {
                        if (newKey.SystemIdType == SchedulerDefinitions.SystemIdTypes.LicenseService)
                        {
                            LockLicense();
                        }
                        else
                        {
                            BroadcastSystemStateSwitchT(false);
                        }
                    }

                    LicenseKeyT keyT = new (newKey);
                    SendServerAction(keyT); // update keys everywhere
                }
            }
            else // didn't update but need to check if expired
            {
                bool keyIsValid = m_licenseValidator.LicenseKeyCheck(PTSystem.LicenseKey, m_liveSystem);
                if (!keyIsValid)
                {
                    m_updateKeyTimer.Interval = TimeSpan.FromMinutes(5).TotalMilliseconds;
                    if (!PTSystem.ReadOnly)
                    {
                        ReleaseLicense();
                        BroadcastSystemStateSwitchT(true);
                    }
                }
                else
                {
                    m_updateKeyTimer.Interval = TimeSpan.FromDays(1).TotalMilliseconds; // make the timer normal
                    if (PTSystem.ReadOnly)
                    {
                        if (PTSystem.LicenseKey.SystemIdType == SchedulerDefinitions.SystemIdTypes.LicenseService)
                        {
                            LockLicense();
                        }
                        else
                        {
                            BroadcastSystemStateSwitchT(false);
                        }
                    }
                }
            }
        }
        catch (Exception err)
        {
            m_licenseValidator.LogLicensingException(err, "Could not Update License Key.");
        }
        finally
        {
            StartUpdateKeyTimer((decimal)m_updateKeyTimer.Interval);
        }
    }

    private InstanceActionsClient m_serverManagerClient;
    private const string c_serverSideSMAddress = "https://localhost:7980/api/";
    private LicenseHelper m_licenseHelper;

    [Obsolete("Move licensing from Server Agent into the instance")]
    private void InitializeServerManagerClient(bool a_usingWebApp, string a_keyFolder, string a_keyFile)
    {
        m_licenseHelper = new LicenseHelper(m_errorReporter, m_constructorValues.InstanceName, m_constructorValues.SoftwareVersion, m_constructorValues.SerialCode, a_keyFolder, a_keyFile);

        if (!a_usingWebApp)
        {
            //TODO: "Move licensing from Server Agent into the instance")
            m_serverManagerClient = new InstanceActionsClient(m_constructorValues.InstanceName, m_constructorValues.SoftwareVersion, c_serverSideSMAddress);
        }
    }

    private IInstanceSettingsManager m_instanceSettingsManager;

    private IInstanceSettingsManager InstanceSettingsManager
    {
        get
        {
            if (m_instanceSettingsManager == null)
            {
                m_instanceSettingsManager = InstanceSettingManagerFactory.CreateInstanceSettingsManagerForInstance(m_constructorValues.InstanceDatabaseConnectionString, 
                    Environment.MachineName, m_constructorValues.InstanceId, m_constructorValues.ApiKey, m_constructorValues.WebAppEnv);
            }

            return m_instanceSettingsManager;
        }
    }

    private System.Timers.Timer m_pingTimer;
    private System.Timers.Timer m_timeoutTimer;
    private System.Timers.Timer m_obtainLicenseTimer;
    private System.Timers.Timer m_updateKeyTimer;

    private void StartUpdateKeyTimer(decimal a_timerIntervalms)
    {
        if (m_updateKeyTimer == null)
        {
            m_updateKeyTimer = new System.Timers.Timer();
            m_updateKeyTimer.AutoReset = false;
            m_updateKeyTimer.Elapsed += LicenseKeyUpdateTimer_Elapsed;
        }

        m_updateKeyTimer.Interval = (double)a_timerIntervalms;
        m_updateKeyTimer.Start();
    }

    public static string PackageErrorFile => "PackageErrors.log";
    public static string PackageErrorsLogPath => Path.Combine(PTSystem.WorkingDirectory.LogFolder, PackageErrorFile);
    public static string PackageWarningLogPath => Path.Combine(PTSystem.WorkingDirectory.LogFolder, "PackageInfo.log");
    

    /// <summary>
    /// Update key. Return false if there was an exception while updating key.
    /// </summary>
    private bool UpdateKey()
    {
        try
        {
            #if DEBUG
            //m_licenseHelper.UpdateKey();
            //The key needs to be manually installed because hydrogen and Neptune typically share an instance.
            // In this case, there is no way to generate a key for both instances with the same serial code without one version being considered newer.
            return true;
#else
            m_licenseHelper.UpdateKey();
            return true;
#endif
        }
        catch (ApiException apiException)
        {
            if (apiException.StatusCode is (int)HttpStatusCode.NotFound or (int)HttpStatusCode.NoContent)
            {
                PTValidationException exception = new ("The instance cannot start without a license");
                m_licenseValidator.LogLicensingException(exception, "Could not Update License Key.");
                throw exception;
            }

            m_licenseValidator.LogLicensingException(apiException, "Could not Update License Key.");
            return false;
        }
        catch (Exception err)
        {
            m_licenseValidator.LogLicensingException(err, "Could not Update License Key.");
            return false;
        }
    }

    /// <summary>
    /// Obtains license.
    /// </summary>
    /// <param name="a_o"></param>
    /// <param name="a_e"></param>
    private void LockLicense(object a_o = null, ElapsedEventArgs a_e = null)
    {
        StopReadOnlyStateTimer();
        bool lockResponse;
        try
        {
            lockResponse = m_licenseHelper.LockLicense();
        }
        catch (Exception ex)
        {
            lockResponse = false;
            m_licenseValidator.InvalidReason = LicenseKeyObject.ELicenseStatus.UnableToLockKey;
            m_licenseValidator.LogLicensingException(ex, "License could not be locked.");
        }

        BroadcastSystemStateSwitchT(!lockResponse);

        if (!lockResponse)
        {
            StartReadOnlyStateTimer();
        }
        else
        {
            StartActiveStateTimers();
        }
    }

    /// <summary>
    /// Sends a Ping through ServerManager.
    /// </summary>
    /// <param name="a_o"></param>
    private void Ping(object a_o, ElapsedEventArgs a_e)
    {
        bool active;
        try
        {
            active = m_licenseHelper.Ping();
        }
        catch (Exception ex)
        {
            m_licenseValidator.LogLicensingException(ex, "Could not Ping License Service.");
            active = false;
        }

        // If ping was successful reset timeout timer.
        if (active && m_timeoutTimer != null)
        {
            try
            {
                m_timeoutTimer.Stop();
                m_timeoutTimer.Interval = m_licenseHelper.TimeoutInterval;
                m_timeoutTimer.Start();
            }
            catch (Exception ex)
            {
                m_licenseValidator.LogLicensingException(ex, "Could not reset license timeout timer.");
            }
        }
    }

    /// <summary>
    /// Notifies ServerManager that License is no longer in use.
    /// </summary>
    private void ReleaseLicense()
    {
        try
        {
            if (PTSystem.LicenseKey.SystemIdType == SchedulerDefinitions.SystemIdTypes.LicenseService)
            {
                StopReadOnlyStateTimer();
                StopActiveStateTimers();
                m_licenseHelper.Release();
            }
        }
        catch (Exception ex)
        {
            m_licenseValidator.LogLicensingException(ex, "Could not Release License.");
        }
    }

    /// <summary>
    /// This is called when no pings have been successfull for duration of timeout period.
    /// It changes the state to readOnly
    /// </summary>
    /// <param name="a_o"></param>
    private void LicenseTimeoutExpired(object a_o, ElapsedEventArgs a_e)
    {
        StopActiveStateTimers();
        BroadcastSystemStateSwitchT(true);
        StartReadOnlyStateTimer();
    }

    //TODO: Replace this with sending the data to the Web App.
    //TODO: In the mean time, we can still send this back to the old SM
    /// <summary>
    /// Broadcasts a SystemStateSwitch transmission to update all connection's state (active or read-only).
    /// </summary>
    /// <param name="a_readOnly">State to change to, true for changing to active.</param>
    private void BroadcastSystemStateSwitchT(bool a_readOnly)
    {
        if (PTSystem.ReadOnly != a_readOnly)
        {
            if (!PTSystem.RunningMassRecordings)
            {
                if (m_serverManagerClient == null)
                {
                    BoolResponse response = SystemController.WebAppActionsClient.NotifyLicenseStatus(a_readOnly ? ELicenseStatus.ReadOnly : ELicenseStatus.Active);
                    if (!response.Content)
                    {
                        m_licenseValidator.LogLicensingException(new PTException("7014"), string.Empty);
                    }
                }
                else
                {
                    //Otherwise tell the old SM
                    GenericResponse response = m_serverManagerClient.NotifyLicenseStatus(a_readOnly);
                    if (!response.success)
                    {
                        m_licenseValidator.LogLicensingException(new PTException(response.error), string.Empty);
                    }
                }
            }

            if (m_liveSystem != null)
            {
                SystemStateSwitchT t = new (a_readOnly, m_licenseValidator.InvalidReason);
                SendServerAction(t);

                m_liveSystem.SystemLoggerInstance.Log("System is in Read-only mode".Localize(), PTDateTime.UtcNow.ToDateTime());
            }
            else
            {
                PTSystem.ReadOnly = a_readOnly; // set it here at least
                PTSystem.SystemLogger.Log("System is in Read-only mode".Localize(), PTDateTime.UtcNow.ToDateTime());
            }
        }
    }

    /// <summary>
    /// Sets off two timers,
    /// 1. Ping Timer: for timing intervals between Pings.
    /// 2. Timout Timer: for timing the time passed since the last successful Ping.
    /// </summary>
    private void StartActiveStateTimers()
    {
        try
        {
            if (m_pingTimer == null)
            {
                m_pingTimer = new System.Timers.Timer();
                m_pingTimer.Elapsed += new ElapsedEventHandler(Ping);
                m_pingTimer.AutoReset = true;
            }

            m_pingTimer.Interval = m_licenseHelper.PingInterval;
            m_pingTimer.Start();

            if (m_timeoutTimer == null)
            {
                m_timeoutTimer = new System.Timers.Timer();
                m_timeoutTimer.Elapsed += new ElapsedEventHandler(LicenseTimeoutExpired);
                m_timeoutTimer.AutoReset = false;
            }

            m_timeoutTimer.Interval = m_licenseHelper.TimeoutInterval;
            m_timeoutTimer.Start();
        }
        catch (Exception ex)
        {
            m_licenseValidator.LogLicensingException(ex, "Could not start Active state timers.");
        }
    }

    /// <summary>
    /// disposes of active state timers.
    /// </summary>
    private void StopActiveStateTimers()
    {
        m_pingTimer?.Stop();

        m_timeoutTimer?.Stop();
    }

    /// <summary>
    /// Starts timer for trying to obtain license.
    /// </summary>
    private void StartReadOnlyStateTimer()
    {
        try
        {
            if (m_obtainLicenseTimer == null)
            {
                m_obtainLicenseTimer = new System.Timers.Timer();
                m_obtainLicenseTimer.Elapsed += new ElapsedEventHandler(LockLicense);
                m_obtainLicenseTimer.AutoReset = false;
            }

            m_obtainLicenseTimer.Interval = m_licenseHelper.PingInterval;
            m_obtainLicenseTimer.Start();
        }
        catch (Exception ex)
        {
            m_licenseValidator.LogLicensingException(ex, "Could not start read-only state timers.");
        }
    }

    /// <summary>
    /// Disposes of ReadOnlyStateTimer.
    /// </summary>
    private void StopReadOnlyStateTimer()
    {
        m_obtainLicenseTimer?.Stop();
    }
#endregion

    private void LoadNewSystemAndPackages(StartupValsAdapter a_constructorValues, ref PTSystem r_liveSystem)
    {
        // Make sure a_webAppClient is null if this System isn't connected to the WebApp
        m_liveSystem = new PTSystem();
        r_liveSystem = m_liveSystem;
        LoadPackages();

        BiDirectionalConnectionTimeout = a_constructorValues.ClientTimeoutSeconds;

        m_liveSystem.InitDataLicensing();
        m_liveSystem.TouchScenarios();
    }

    private void LoadSystemFromScenario(long a_startDate, StartupValsAdapter a_constructorValues, ref PTSystem r_liveSystem, Timing timeToDeserializeSystemTicks)
    {
        string fullPath = Path.Combine(PTSystem.WorkingDirectory.Scenario, SystemDataFileName);
        if (File.Exists(fullPath))
        {
            LoadSystemAndPackages(fullPath, false, a_startDate, timeToDeserializeSystemTicks, a_constructorValues, ref r_liveSystem);
        }
        else
        {
            LoadNewSystemAndPackages(a_constructorValues, ref r_liveSystem);
        }
    }
#endregion

    #region Startup System
    private readonly object m_loginWriteLock = new ();
    private readonly object m_loginCheckLock = new ();
    private readonly object m_currentLoginCountLock = new ();
    private bool m_isLoginWriteInProcess;
    private byte[] m_systemBytes;
    private byte[] m_unloadedScenarioDataBytes;
    private int m_currentLoginCount;

    /// <summary>
    /// Validates a login without actually signing in and creating a session
    /// </summary>
    /// <param name="a_identifier">UserName</param>
    /// <param name="a_passwordHash">Hashed Password</param>
    /// <returns></returns>
    public bool ValidateLogin(string a_identifier, byte[] a_passwordHash)
    {
        try
        {
            User user = UserLicenseCheck(a_identifier, a_passwordHash, ELoginMethod.Basic);
            if (user != null && !user.AppUser)
            {
                return true;
            }
        }
        catch (Exception e) { }

        return false;
    }

    /// <summary>
    /// Validates a login without actually signing in and creating a session
    /// </summary>
    /// <param name="a_token">Login with Token</param>
    /// <returns></returns>
    public bool ValidateLogin(string a_token)
    {
        try
        {
            User user = UserLicenseCheck(a_token, null, ELoginMethod.Token);
            if (user != null && !user.AppUser)
            {
                return true;
            }
        }
        catch (Exception e) { }

        return false;
    }

    public UserSession LoginNewUserSession(string a_identifier, byte[] a_passwordHash)
    {
        return GenerateNewSession(a_identifier, a_passwordHash, ELoginMethod.Basic, "");
    }

    public UserSession LoginNewUserSession(string a_token, ELoginMethod a_tokenLoginType)
    {
        return GenerateNewSession(a_token, null, a_tokenLoginType, "");
    }

    /// <summary>
    /// Get a copy of the system.
    /// </summary>
    /// <param name="aConnectionDescription">
    /// Information about the computer and the user of this connection.
    /// Connection.CreateDescription() should be called on the computer requesting this information.
    /// </param>
    /// <returns>The system or null if an exception is encountered.  The Exception is not serialized back which is why an out parameter is used instead.</returns>
    public byte[] GetStartupSystem(string a_sessionToken, out SoftwareVersion o_productVersion, out string o_anExceptionMessage, out long o_waitDuration)
    {
        //TODO: See is o_waitDuration is still in use, and remove it entirely if not
        o_waitDuration = 0;
        // Also remove the usages of Timing here if o_waitDuration has no use
        //Timing byteWaitTiming = new ();
        //byteWaitTiming.Start();
        lock (m_currentLoginCountLock)
        {
            m_currentLoginCount++;
        }

        TransmissionSession transmissionSession = null;
        try
        {
            transmissionSession = GetSession(a_sessionToken);

            o_anExceptionMessage = "";
            o_productVersion = m_productVersion;


            //TODO lite client: Either remove or refactor this block of commented code
            // This block of code use to be used so that if multiple login requests were made while the SystemBytes were
            // being serialized, then the same byte array could be used to serve those login requests (assuming that
            // no other transmissions came in during the serialization). This is still a possibility
            // with the lite client, but the conditions need to be re-worked accordingly. We can only use the same
            // byte array if the sessions that are requesting the SystemBytes have the same set of LoadedScenarioIds
            // as the first session that requested the SystemBytes. 
            //bool alreadySerializingScenario;
            //lock (m_loginCheckLock)
            //{
            //    alreadySerializingScenario = !m_shouldTransmissionsBeBroadcastDuringLogin && m_isLoginWriteInProcess;
            //}

            //if (alreadySerializingScenario)
            //{
            //    //Scenario is already being serialized, wait for it to finish
            //    lock (m_loginWriteLock)
            //    {

            //        if (transmissionSession is IUserSession userSession)
            //        {
            //            BroadcastUserLogOnT(userSession.UserId);
            //        }

            //        byteWaitTiming.Stop();
            //        o_waitDuration = byteWaitTiming.Length;

            //        byte[] justInCaseReturningm_systemBytesCausesAnError = new byte[m_systemBytes.Length];
            //        Array.Copy(m_systemBytes, justInCaseReturningm_systemBytesCausesAnError, m_systemBytes.Length);
            //        return justInCaseReturningm_systemBytesCausesAnError;
            //    }
            //}
            //else continue to write the scenario

            lock (m_loginWriteLock)
            {
                lock (m_loginCheckLock)
                {
                    m_isLoginWriteInProcess = true;
                }

                //Serialize and read scenario bytes (stored in m_systemBytes)
                ECompressionType compressionType = transmissionSession is ICompressedSession compressedSession ? compressedSession.CompressionType : ECompressionType.None;

                //Store the data snapshot from the server
                WritePTSystemToMemory( false, transmissionSession, compressionType);
                //a_restartReceiver for WritePTSystemToMemory is false because the Receiver is restarted in the finally block of this function

                if (transmissionSession is IUserSession userSession)
                {
                    BroadcastUserLogOnT(userSession.UserId);


                }

                lock (m_loginCheckLock)
                {
                    m_isLoginWriteInProcess = false;
                    m_shouldTransmissionsBeBroadcastDuringLogin = false;
                }

                //byteWaitTiming.Stop();
                //o_waitDuration = byteWaitTiming.Length;
                byte[] justInCaseReturningMemberSystemBytesCausesAnError = new byte[m_systemBytes.Length];
                Array.Copy(m_systemBytes, justInCaseReturningMemberSystemBytesCausesAnError, m_systemBytes.Length);
                return justInCaseReturningMemberSystemBytesCausesAnError;
            }
        }
        catch (Exception e)
        {
            o_productVersion = m_productVersion;
            o_anExceptionMessage = e.Message;

            if (e is InvalidLogonException logonException)
            {
                string exceptionLogFilePath = Path.Combine(BroadcastingAlerts.BroadcastingAlertsAndExceptionsWorkingDirectory, "StartupSystemRetrievalExpiredExceptions.log");
                SimpleExceptionLogger.LogException(exceptionLogFilePath, e);
            }
            else if (e is LicenseKeyValidationHelper.ExpiredException)
            {
                string exceptionLogFilePath = Path.Combine(BroadcastingAlerts.BroadcastingAlertsAndExceptionsWorkingDirectory, "StartupSystemRetrievalExpiredExceptions.log");
                SimpleExceptionLogger.LogException(exceptionLogFilePath, e);
            }
            else
            {
                BaseId userId = BaseId.NULL_ID;
                if (transmissionSession is UserSession userSession)
                {
                    userId = userSession.UserId;
                }

                string errMsg = string.Format("An exception occurred when a client attempted to get the startup system for user: {0}".Localize(), userId);
                string exceptionLogFilePath = Path.Combine(BroadcastingAlerts.BroadcastingAlertsAndExceptionsWorkingDirectory, "StartupSystemRetrievalExceptions.log");
                SimpleExceptionLogger.LogException(exceptionLogFilePath, e, errMsg);
            }

            //byteWaitTiming.Stop();
            //o_waitDuration = byteWaitTiming.Length;
            return null;
        }
        finally
        {
            //If no more users are logging in, don't keep the scenario bytes in memory.
            lock (m_currentLoginCountLock)
            {
                m_currentLoginCount--;
                if (m_currentLoginCount == 0)
                {
                    m_systemBytes = null;
                    SystemController.StartReceiving(); // all logins finished, start receiving again.
                }
            }
        }
    }

    public byte[] GetAllUnloadedScenarioData(string a_sessionToken)
    {
        
        TransmissionSession transmissionSession = null;
        try
        {
            transmissionSession = GetSession(a_sessionToken);

            if (transmissionSession is not IUserSession userSession)
            {
                string errMsg = string.Format("An attempt to retrieve close scenario data for a non-UserSession was made.".Localize());
                string exceptionLogFilePath = Path.Combine(BroadcastingAlerts.BroadcastingAlertsAndExceptionsWorkingDirectory, "StartupSystemRetrievalExceptions.log");
                SimpleExceptionLogger.LogException(exceptionLogFilePath, errMsg);
                return null;
            }

            BaseId userId = userSession.UserId;
            byte[] unloadedScenarioData;
            using (BinaryMemoryWriter writer = new ())
            {
                using (ObjectAccess<ScenarioManager> sm = SystemController.Sys.ScenariosLock.TryEnterWrite(AutoExiter.THREAD_TRY_WAIT_MS))
                {
                    sm.Instance.SerializeAllUnloadedScenarioData(writer, userId, userSession.LoadedScenarioIds);
                    unloadedScenarioData = writer.GetBuffer();
                }
            }

            return unloadedScenarioData;
        }
        catch (Exception e)
        {
            BaseId userId = BaseId.NULL_ID;
            if (transmissionSession is UserSession userSession)
            {
                userId = userSession.UserId;
            }

            string errMsg = string.Format("An exception occurred when a client attempted to get the closed scenario data  for user: {0}".Localize(), userId);
            string exceptionLogFilePath = Path.Combine(BroadcastingAlerts.BroadcastingAlertsAndExceptionsWorkingDirectory, "StartupSystemRetrievalExceptions.log");
            SimpleExceptionLogger.LogException(exceptionLogFilePath, e, errMsg);
            return null;
        }
        finally
        {
            // I'm mimicking some behavior from the "GetScenarios" API, but I don't think I need this here
            m_unloadedScenarioDataBytes = null;
            SystemController.StartReceiving(); // all logins finished, start receiving again.
        }
    }

    /// <summary>
    /// A helper function to the UpdateUserLoadedScenarioIds API. 
    /// </summary>
    /// <param name="a_isAddId">True to add the Id, false to remove it</param>
    /// <param name="a_scenarioId">The scenarioId to be added or removed from the LoadedScenarioIds set</param>
    /// <param name="a_sessionToken">The SessionToken used to get the UserSession</param>
    /// <returns>A bool indicating success or failure</returns>
    /// The return value isn't used right now, but I included it just because.
    /// Also, the API function wraps the usage of this in a try-catch statement so
    /// this function doesn't do any of it as a result
    public bool UpdateUserSessionLoadedScenarioIds(bool a_isAddId, BaseId a_scenarioId, string a_sessionToken)
    {
        TransmissionSession transmissionSession = GetSession(a_sessionToken);
        if (transmissionSession is UserSession userSession)
        {
            if (a_isAddId)
            {
                return userSession.LoadedScenarioIds.Add(a_scenarioId);
            }
            return userSession.LoadedScenarioIds.Remove(a_scenarioId);
        }
        // Can't update if it's not a UserSession
        return false;
    }

    /// <summary>
    /// PTBroadcaster calls this function after successfully creating a connection.
    /// </summary>
    internal void BroadcastUserLogOnT(BaseId a_userId)
    {
        UserLogOnT t = new (a_userId);
        t.TransmissionSender = PTTransmissionBase.TransmissionSenderType.PTSystem;
        SendServerAction(t);
    }

    // Might not need this anymore
    public byte[] GetScenarioBytes(long a_scenarioId)
    {
        string tmpPath = Path.GetTempFileName();
        BaseId scenarioId = new (a_scenarioId);
        ScenarioExceptionInfo sei = new ();
        try
        {
            using (LiveSystem.AutoEnterRead(scenarioId, out Scenario s))
            {
                if (s == null)
                {
                    return null;
                }
                BinaryMemoryWriter writer = new ();
                s.Serialize(writer);
                DateTime creationDateTime;
                using (s.ScenarioSummaryLock.TryEnterRead(out ScenarioSummary ss, AutoExiter.THREAD_TRY_WAIT_MS))
                {
                    creationDateTime = ss.CreationDateTime;
                }
                sei.Initialize(s.Name, s.Type.ToString(), creationDateTime);
                return writer.GetBuffer();
            }
        }
        catch (Exception err)
        {
            LiveSystem.SystemLoggerInstance.LogException(err, sei);

            return null;
        }
        finally
        {
            try
            {
                File.Delete(tmpPath);
            }
            catch { }
        }
    }

    public byte[] GetScenarioBytes(BaseId a_scenarioId)
    {
        string tmpPath = Path.GetTempFileName();
        ScenarioExceptionInfo sei = new();
        try
        {
            using (LiveSystem.AutoEnterRead(a_scenarioId, out Scenario s))
            {
                // I don't think we're flushing the dispatchers with this method. Should we be?
                // There's a lock in Scenario.Serialize so we might be okay...
                BinaryMemoryWriter writer = new ();
                s.Serialize(writer);
                DateTime creationDateTime;
                using (s.ScenarioSummaryLock.TryEnterRead(out ScenarioSummary ss, AutoExiter.THREAD_TRY_WAIT_MS))
                {
                    creationDateTime = ss.CreationDateTime;
                }
                sei.Initialize(s.Name, s.Type.ToString(), creationDateTime);
                return writer.GetBuffer();
            }
        }
        catch (Exception err)
        {
            LiveSystem.SystemLoggerInstance.LogException(err, sei);

            return null;
        }
        finally
        {
            try
            {
                File.Delete(tmpPath);
            }
            catch { }
        }
    }

    #endregion

    #region Backup
    // This is the interval in minutes in which backups of the system are saved in the
    // scenario/backups folder. System backups are stored as zip files. To use a backup
    // you need to unzip the file, remove the date prefix, and drop it in the scenario folder.
    private int m_backupInterval = 15;

    private System.Timers.Timer m_backupTimer;

    private void InitScenarioBackupTimer()
    {
        int ms = m_backupInterval * 1000 * 60; // 1000*60=1 minute worth of milliseconds.

        m_backupTimer = new System.Timers.Timer(ms);
        m_backupTimer.Elapsed += CreateBackup;
        m_backupTimer.Start();
    }

    private void DestructBackup()
    {
        m_backupTimer.Dispose();
        m_backupTimer = null;
    }

    // This is the maximum number of backup files the system will store in the backups folder.
    // Each time a new backup file is created the oldest backups beyond this limit are deleted.
    private int m_maxRecordings = 25;
    private const string DIRECTORY_ERROR_MSG = "Directory \"{0}\" couldn't be deleted. Check for read-only directories or files, or file locks.";

    /// <summary>
    /// Limit the number of recordings to maxRecordings.
    /// /// Only directories that start with "20* are deleted.
    /// </summary>
    private void MaxRecordingsCleanup()
    {
        string[] directoriesTemp = Directory.GetDirectories(m_liveSystem.WorkingDirectoryInstance.RecordingRootFolder, "20*");
        SortedStringList directories = new ();
        // Once this is set to true it indicates that directories encountered should be deleted.

        //Clear empty recording folders, it's possible the system didn't start up correctly but created the folder.
        for (int i = directoriesTemp.Length - 1; i >= 0; i--)
        {
            DirectoryInfo info = new (directoriesTemp[i]);
            if (info.FullName == m_liveSystem.WorkingDirectoryInstance.RecordingDirectory || info.GetFiles().Length > 0)
            {
                directories.Add(info.FullName);
            }
            else
            {
                Common.Directory.DirectoryUtils.Delete(info.FullName);
            }
        }

        int deleteCount = directories.Count - m_maxRecordings;

        if (deleteCount > 0)
        {
            string directory = "";

            try
            {
                for (int directoryI = 0; directoryI < deleteCount; ++directoryI)
                {
                    directory = directories[directoryI];
                    Common.Directory.DirectoryUtils.Delete(directory);
                }
            }
            catch (Exception e)
            {
                string errorMsg = string.Format(DIRECTORY_ERROR_MSG, directory);
                throw new PTException(errorMsg, e);
            }
        }
    }

    /// <summary>
    /// Create a backup of the system in the current recording folder.
    /// </summary>
    /// <param name="o"></param>
    private void CreateBackup(object a_timer, ElapsedEventArgs a_elapsedEventArgs)
    {
        WriteBackupScenarioToRecordingsIfNeeded(false);
    }

    private void WriteBackupScenarioToRecordingsIfNeeded(bool a_always)
    {
        if (BackupNeeded() || a_always) //Always check backup since it updates recording numbers
        {
            string backupName = m_recordingsBroadcaster.CreateBackupName(m_nextTransmissionRecordingRecordNumber);
            WritePTSystemToDisk(m_liveSystem.WorkingDirectoryInstance.RecordingDirectory, backupName, true);
            m_recordingsBroadcaster.MaxBackupsCleanup();
        }
    }

    private long m_nextTransmissionRecordingRecordNumber;

    internal bool BackupNeeded()
    {
        long nextTransmissionRecordingRecordNumberTemp = m_liveSystem.PeekNextTransmissionRecordingRecordNumber();

        if (nextTransmissionRecordingRecordNumberTemp == m_nextTransmissionRecordingRecordNumber)
        {
            // Nothing has changed since the last backup.
            return false;
        }

        m_nextTransmissionRecordingRecordNumber = nextTransmissionRecordingRecordNumberTemp;
        return true;
    }
    #endregion

    #region Packages
    public PackedAssembly GetPackedPackageAssembly(AssemblyPackageInfo a_AssemblyPackageInfo)
    {
        return m_liveSystem.GetPackedPackageAssembly(a_AssemblyPackageInfo);
    }

    /// <summary>
    /// Retrieves missing assembly infos for the client
    /// </summary>
    /// <param name="a_packagesOnClient"></param>
    /// <returns></returns>
    public AssemblyPackageInfo[] GetPackagesOnServer()
    {
        return m_liveSystem.GetPackageVersionsOnServer();
    }
    #endregion

    /// <summary>
    /// Only works on the Server.
    /// </summary>
    /// <returns></returns>
    public byte[] GetLoggedInInstanceData()
    {
        InstanceSettingsEntity instanceSettingsEntity = InstanceSettingsManager.GetInstanceSettingsEntity(m_constructorValues.InstanceName, m_constructorValues.SoftwareVersion);
        LoggedInInstanceInfo instanceInfo = new (instanceSettingsEntity);

        using (BinaryMemoryWriter writer = new (m_symmetricKey, ECompressionType.Normal))
        {
            instanceInfo.Serialize(writer);
            return writer.GetBuffer();
        }
    }

    internal void PlayBack()
    {
        m_recordingsBroadcaster.PlayBack();
    }

    internal void Prune()
    {
        m_recordingsBroadcaster.Prune();
    }

    public string LoadRecordingDirectory => m_recordingsBroadcaster.LoadRecordingDirectory;

    internal bool Record => m_constructorValues.Record;

    private readonly IClassFactory m_classFactory;
    private const int c_cleanUpDeadConnectionsMilliSeconds = 5000;
    private readonly DateTime m_utcDateTimeInstanceId = DateTime.UtcNow;

    public DateTime BroadcasterUtcDateTimeInstanceId => m_utcDateTimeInstanceId;

    /// <summary>
    /// A list of active sessions attached to this broadcaster.
    /// </summary>
    private readonly List<BaseSession> m_activeSessions = new ();

    /// <summary>
    /// Add a newly created connection to the set of connections in the broadcaster.
    /// Assign a connection number to the connection.
    /// </summary>
    /// <param name="a_connection"></param>
    private void AddSession(BaseSession a_session)
    {
        lock (m_connectionLock)
        {
            if (m_activeSessions.Any(s => s.SessionToken == a_session.SessionToken))
            {
                if (a_session is UserSession userSession)
                {
                    Exception exception = new ("4457".Localize());
                    m_errorReporter.LogException(exception,null);
                }

                return;
            }

            m_activeSessions.Add(a_session);

            if (a_session is UserSession a_userSession)
            {
                string logMessage = string.Format("Session created for user with Id {0}.".Localize(), a_userSession.UserId);
                m_errorReporter.Log(logMessage, DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"), ELogClassification.PtUser);
            }
        }
    }

    public GetTransmissionResponse ReturnNextTransmissionContainer(string a_sessionToken, string a_lastProcessedTransmission)
    {
        try
        {
            TransmissionSession session;
            lock (m_connectionLock)
            {
                session = GetSession(a_sessionToken);
            }

            byte[] data = session.DequeueTransmissionContainer(a_lastProcessedTransmission);

            if (data != null)
            {
                int queuedTransmissions = session.GetQueuedTransmissionsCount();
                return new GetTransmissionResponse
                {
                    RemainingTransmissions = queuedTransmissions,
                    TransmissionData = data
                };
            }

            return null;
        }
        catch (Exception e)
        {
            SimpleExceptionLogger.LogException(BroadcastingAlerts.BroadcastingLogFilePath, e, "An error occurred while trying to process a request to receive compressed transmissions.");
            throw;
        }
    }

    /// <summary>
    /// Doesn't throw any exceptions.
    /// </summary>
    /// <param name="a_connection"></param>
    //void LogLogOn(BaseSession a_connection)
    //{
    //    LogALogOnOrLogOff("LogOn", a_connection.ConnectionNbr, a_connection.CreationTicks, a_connection.Description, null, null);
    //}
    public void LogOff(string a_sessionToken)
    {
        lock (m_connectionLock)
        {
            LogOff(GetSession(a_sessionToken));
        }
    }

    public void LogOff(BaseSession a_session)
    {
        lock (m_connectionLock)
        {
            try
            {
                SessionBeingDropped(a_session);
                m_activeSessions.Remove(a_session);
            }
            catch (Exception e)
            {
                SimpleExceptionLogger.LogException(BroadcastingAlerts.MiscExceptionLogFilePath, e, "An error occurred while trying to DropConnection during log off.");
                throw;
            }
        }
    }

    //private void LogALogOnOrLogOff(string a_elementName, int a_connectionNbr, long a_creationTicks, string a_connectionDescription, string a_receiverDescription, XElement a_additionalElement)
    //{
    //    try
    //    {
    //        //TODO: session
    //    }
    //    catch (Exception e)
    //    {
    //        string msg = string.Format("An error occurred while trying to log a {0}. Not important. The only affect is there will be a missing entry in the LogOnOff.log.", a_elementName);
    //        Common.File.SimpleExceptionLogger.LogException(BroadcastingAlerts.MiscExceptionLogFilePath, e, msg);
    //    }
    //}

    private const string c_deadConnMsg = "Session '{0}' does not exist. It may have been dropped after timeout expired.";

    /// <summary>
    /// Throws an exception if the connection isn't valid. The connection number of id must go together.
    /// </summary>
    protected TransmissionSession GetSession(string a_sessionToken)
    {
        lock (m_connectionLock)
        {
            if (SessionToken == a_sessionToken)
            {
                return this;
            }

            foreach (BaseSession activeSession in m_activeSessions)
            {
                if (activeSession.SessionToken == a_sessionToken)
                {
                    return activeSession as TransmissionSession;
                }
            }

            // error thrown here on auth call
            throw new InvalidSessionException(string.Format(c_deadConnMsg, a_sessionToken));
        }
    }

    private ulong m_transmissionNbr;

    protected ulong TransmissionNbr
    {
        get => m_transmissionNbr;

        // This value is settable so that when the system starts you may set the transmission back to the value that it was before.
        set => m_transmissionNbr = value;
    }

    /// <summary>
    /// Remove previous open connections for this app user
    /// </summary>
    /// <param name="a_userAppId"></param>
    private void CleanupSessions(BaseId a_userAppId)
    {
        try
        {
            lock (m_connectionLock)
            {
                for (int i = m_activeSessions.Count - 1; i >= 0; --i)
                {
                    BaseSession activeSession = m_activeSessions[i];

                    if (activeSession is UserSession userSession && userSession.UserId == a_userAppId)
                    {
                        userSession.SessionLifecycle.SetSessionHasTimedOut();
                        LogOff(activeSession);
                    }
                }
            }
        }
        catch (Exception e)
        {
            SimpleExceptionLogger.LogException(BroadcastingAlerts.CleanupConnectionsExceptionLogFilePath, e);
            throw;
        }
    }

    private void CleanupSessions(object a_state)
    {
        try
        {
            lock (m_connectionLock)
            {
                for (int i = m_activeSessions.Count - 1; i >= 0; --i)
                {
                    BaseSession activeSession = m_activeSessions[i];

                    DateTime utcNow = DateTime.UtcNow;
                    DateTime lastPing = activeSession.LastReceiveDate;
                    TimeSpan diff = utcNow - lastPing;

                    if (diff > activeSession.ConnectionTimeout)
                    {
                        if (activeSession is UserSession userSession)
                        {
                            userSession.SessionLifecycle.SetSessionHasTimedOut();
                        }

                        LogOff(activeSession);
                    }
                }
            }
        }
        catch (Exception e)
        {
            SimpleExceptionLogger.LogException(BroadcastingAlerts.CleanupConnectionsExceptionLogFilePath, e);
            throw;
        }
    }

    /// <summary>
    /// Used to lock the broadcaster while modifying connections.
    /// </summary>
    protected object m_connectionLock = new ();

    /// <summary>
    /// A timer that goes off peridocially to check for and drop dead connections.
    /// </summary>
    private readonly System.Threading.Timer m_dropConnectionsTimer;

    /// <summary>
    /// The timeout interval used on clients. Any connection that doesn't
    /// contact the broadcaster within each interval will be dropped.
    /// </summary>
    private int m_biDirectionalConnectionTimeout = 120;

    private readonly object m_biDirectionalConnectionTimeoutLock = new ();

    // This is the minimum number of seconds that will be used to determine whether a connection has timed out.
    private const int MIN_CONNECTION_TIMEOUT = 10;

    /// <summary>
    /// The timeout interval used on clients. Any connection that doesn't
    /// contact the broadcaster within each interval will be dropped.
    /// </summary>
    protected int BiDirectionalConnectionTimeout
    {
        get
        {
            lock (m_biDirectionalConnectionTimeoutLock)
            {
                return m_biDirectionalConnectionTimeout;
            }
        }

        set
        {
            lock (m_biDirectionalConnectionTimeoutLock)
            {
                if (value < MIN_CONNECTION_TIMEOUT)
                {
                    throw new CommonException(string.Format("2309: The minimum value of BiDirectionalConnectionTimeout is {0}", MIN_CONNECTION_TIMEOUT));
                }

                m_biDirectionalConnectionTimeout = value;
            }
        }
    }

    public class InvalidSessionException : CommonException
    {
        public InvalidSessionException() { }

        public InvalidSessionException(string a_message, Exception a_inner = null)
            : base(a_message, a_inner) { }
    }

    /// <summary>
    /// Returns the number of connections per user.
    /// Dev note: running internal does not create an active user session.
    /// </summary>
    /// <returns></returns>
    public ConnectedUserData[] GetLoggedInUserData()
    {
        List<ConnectedUserData> data = new ();
        Dictionary<BaseId, short> connectionsDict = new ();

        foreach (BaseSession activeSession in m_activeSessions)
        {
            if (activeSession is UserSession userSession)
            {
                if (!connectionsDict.TryAdd(userSession.UserId, 1))
                {
                    connectionsDict[userSession.UserId] += 1;
                }
            }
        }

        using (m_liveSystem.UsersLock.EnterRead(out UserManager um))
        {
            foreach ((BaseId id, short connections) in connectionsDict)
            {
                User user = um.GetById(id);

                if (user == null)
                {
                    // User was deleted but session not yet cleaned up. We expect it to be soon handled by the standard process, and will report it as removed here.
                    m_errorReporter.Log(string.Format("A session was found without a corresponding user (Id {0}).".Localize(), id), "");
                    continue;
                }

                data.Add(new ConnectedUserData
                {
                    ReadableName = user.Name,
                    ActiveConnections = connections,
                    Id = user.Id.Value
                });
            }
        }

        return data.ToArray();
    }
    
    /// <summary>
    /// Returns basic User description info for the user of a particular session.
    /// </summary>
    /// <param name="a_sessionToken"></param>
    /// <returns></returns>
    public ConnectedUserData GetLoggedInUserData(string a_sessionToken)
    {
        BaseSession validateSession = ValidateSession(a_sessionToken);
        if (validateSession is UserSession userSession)
        {
            using (SystemController.Sys.UsersLock.EnterRead(out UserManager um))
            {
                User user = um.GetById(userSession.UserId);
                return new ConnectedUserData()
                {
                    ReadableName = user.Name,
                    Id = user.Id.Value
                };
            }
        }

        return null;
    }


    public ServiceStatus GetServiceStatus()
    {
        ServiceStatus status = new ()
        {
            UsersOnline = m_activeSessions.Count, //May have to subtract for server session
            LastActionTime = LastReceiveDate,
            LastLogon = m_activeSessions.Count != 0 ? m_activeSessions.Max(x => x.CreationDate) : PTDateTime.MinDateTime,
            StartTime = new DateTime(m_startDate)
        };

        status.ProductVersion = ProductVersion.ToSimpleVersion();
        status.InstanceName = m_constructorValues.InstanceName;

        if (status.UsersOnline == 0)
        {
            status.State = ServiceStatus.EServiceState.Started;
        }
        else if ((DateTime.UtcNow - status.LastActionTime).TotalMinutes <= 10)
        {
            status.State = ServiceStatus.EServiceState.Active;
        }
        else
        {
            status.State = ServiceStatus.EServiceState.Idle;
        }

        return status;
    }

    public Task<ChecksumValues> GetChecksum(long a_scenarioId, Guid a_transmissionId)
    {
        return Task.Run(() =>
        {
            using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
            {
                Scenario scenario = sm.Find(new BaseId(a_scenarioId));
                if (scenario == null)
                {
                    return null;
                }

                //TODO: validate params
                return scenario.GetChecksumById(a_transmissionId);
            }
        });
    }

    public void EnableJitLoginForUser(string a_token)
    {
        SystemController.ValidateLoginUserFromJwtPrincipal(a_token, m_constructorValues.SsoValidationCertificateThumbprint, m_constructorValues.SsoDomain, m_constructorValues.SsoClientId);
        //Validate token is from authorized source (like login, but without validating user)
        //Parse user name
        string userNameFromToken = SystemController.GetUserFromJwtPrincipalWithoutValidation(a_token);
        if (string.IsNullOrWhiteSpace(userNameFromToken))
        {
            throw new PTValidationException("Invalid user");
        }

        using (SystemController.Sys.UsersLock.EnterRead(out UserManager um))
        {
            um.EnableJitLoginForUser(userNameFromToken);
        }
    }

    public async Task<RuleSeekDiagnositcs> GetRuleSeekDiagnostics()
    {
        RuleSeekDiagnositcs result = null;
        await Task.Run(() =>
        {
            using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
            {
                (RuleSeekDiagnositcs, InsertJobsDiagnostics)? currentStatus = sm.CalcCoPilotDiagnosticsForUpdate();
                result = currentStatus?.Item1;
            }
        });

        return result;
    }

    public async Task<InsertJobsDiagnostics> GetInsertJobDiagnostics()
    {
        InsertJobsDiagnostics result = null;
        await Task.Run(() =>
        {
            using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
            {
                (RuleSeekDiagnositcs, InsertJobsDiagnostics)? currentStatus = sm.CalcCoPilotDiagnosticsForUpdate();
                result = currentStatus?.Item2;
            }
        });

        return result;
    }

    public void SendMessageToUser(MessageRequest a_request)
    {
        if (a_request.SelectedUsers.Count > 0)
        {
            using (SystemController.Sys.UsersLock.EnterRead(out UserManager userManager))
            {
                foreach (long userIdLong in a_request.SelectedUsers)
                {
                    BaseId userId = new (userIdLong);
                    if (userManager.GetById(userId) == null)
                    {
                        //TODO optional: Build a more advance exception that tracks which users were invalid and
                        // throw the exception at the end instead
                        string exceptionString = string.Format("The user with Id '{0}' is invalid. The message was not sent".Localize(), userId);
                        throw new PTValidationException(exceptionString);
                    }
                }
            }
        }

        InstanceMessageT t = new (a_request.Message, a_request.Shutdown, a_request.SelectedUsers, a_request.ShutdownWarning);
        t.Instigator = BaseId.NULL_ID;
        SendServerAction(t);
    }

    public PublishStatusMessage GetCurrentPublishStatus(long a_scenarioId)
    {
        using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
        {
            Scenario scenario = sm.Find(new BaseId(a_scenarioId));

            if (scenario != null)
            {
                return SystemController.PublishHelper.GetCurrentPublishStatus(new BaseId(a_scenarioId));
            }

            // Could be null if the client is ahead of the server and has already created a new scenario.
            // TODO: While existing users of this method handle null appropriately (and most won't be called on scenario creation),
            // TODO: this is not guaranteed for all client calls for server data. We need a general API strategy for handling client transmissions that can (preemptively) call apis.  
            // TODO: This may be more straightforward when we move to the light client model
            return null;
        }
    }

    public ImportStatusMessage GetCurrentImportStatus()
    {
        return SystemController.ImportingService.GetCurrentImportStatus();
    }

    public string GetLogContent(PackageEnums.ELogTypes a_logType, bool a_showDetails = true)
    {
        // TODO: We need to handle retrieve the contents from the appropriate source from the server
        return String.Empty;
        //string logContent;

        //switch (a_logType)
        //{
        //    case PackageEnums.ELogTypes.External:
        //        logContent = a_showDetails ? m_errorReporter.ExternalInterfaceLogContents : m_errorReporter.ExternalInterfaceLogContentsFiltered;
        //        break;
        //    case PackageEnums.ELogTypes.Interface:
        //        logContent = a_showDetails ? m_errorReporter.PTInterfaceLogContents : m_errorReporter.PTInterfaceLogContentsFiltered;
        //        break;
        //    case PackageEnums.ELogTypes.System:
        //        logContent = a_showDetails ? m_errorReporter.PTSystemLogContents : m_errorReporter.PTSystemLogContentsFiltered;
        //        break;
        //    case PackageEnums.ELogTypes.Fatal:
        //        logContent = a_showDetails ? m_errorReporter.FatalLogContents : m_errorReporter.FatalLogContentsFiltered;
        //        break;
        //    case PackageEnums.ELogTypes.User:
        //        logContent = a_showDetails ? m_errorReporter.PTUserLogContents : m_errorReporter.PTUserLogContentsFiltered;
        //        break;
        //    case PackageEnums.ELogTypes.Misc:
        //        logContent = a_showDetails ? m_errorReporter.MiscLogContents : m_errorReporter.MiscLogContentsFiltered;
        //        break;
        //    case PackageEnums.ELogTypes.SchedulingWarnings:
        //        logContent = a_showDetails ? m_errorReporter.SchedulingWarningsContents : m_errorReporter.SchedulingWarningContentsFiltered;
        //        break;
        //    case PackageEnums.ELogTypes.Notifications:
        //        logContent = a_showDetails ? m_errorReporter.NotificationsContents : m_errorReporter.NotificationsContentsFiltered;
        //        break;
        //    default:
        //        throw new ArgumentOutOfRangeException();
        //}

        //return logContent;
    }

    #region Scenario Permissions
    /// <summary>
    /// Returns a dictionary of all ScenarioIds the provided User BaseId has permissions to, along with the access level.
    /// </summary>
    /// <param name="a_userId"></param>
    /// <returns></returns>
    public Dictionary<long, EUserAccess> GetScenarioPermissionsForUser(BaseId a_userId)
    {
        Dictionary<long, EUserAccess> scenarioPermissions = new ();

        using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
        {
            foreach (Scenario scenario in sm.Scenarios)
            {
                using (scenario.AutoEnterScenarioSummary(out ScenarioSummary ss))
                {
                    ScenarioPermissionSettings permissionSettings = ss.ScenarioSettings.LoadSetting<ScenarioPermissionSettings>(ScenarioPermissionSettings.Key);

                    if (permissionSettings.UserIdToAccessLevel.TryGetValue(a_userId, out EUserAccess accessLevel))
                    {
                        scenarioPermissions.Add(scenario.Id.Value, accessLevel);
                    }
                }
            }
        }

        return scenarioPermissions;
    }

    /// <summary>
    /// Returns a dictionary of all ScenarioIds the provided Group BaseId has permissions to, along with the access level.
    /// </summary>
    /// <param name="a_groupId"></param>
    /// <returns></returns>
    public Dictionary<long, EUserAccess> GetScenarioPermissionsForGroup(BaseId a_groupId)
    {
        Dictionary<long, EUserAccess> scenarioPermissions = new ();

        using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
        {
            foreach (Scenario scenario in sm.Scenarios)
            {
                using (scenario.AutoEnterScenarioSummary(out ScenarioSummary ss))
                {
                    ScenarioPermissionSettings permissionSettings = ss.ScenarioSettings.LoadSetting<ScenarioPermissionSettings>(ScenarioPermissionSettings.Key);

                    if (permissionSettings.GroupIdToAccessLevel.TryGetValue(a_groupId, out EUserAccess accessLevel))
                    {
                        scenarioPermissions.Add(scenario.Id.Value, accessLevel);
                    }
                }
            }
        }

        return scenarioPermissions;
    }

    /// <summary>
    /// Returns a tree collection of scenario permissions for all groups and their constituent users, per scenario.
    /// </summary>
    /// <param name="a_groupId"></param>
    /// <returns></returns>
    public List<ScenarioPermissionSet> GetAllScenarioPermissions()
    {
        List<ScenarioPermissionSet> scenarioPermissionSets = new ();

        // Scenario permissions don't contain group memberships. We need to first get those in order to build the response below.
        using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
        {
            foreach (Scenario scenario in sm.Scenarios)
            {
                Dictionary<BaseId, GroupedScenarioPermissions> groupPermissions = GetUserInfoByGroup();

                using (scenario.AutoEnterScenarioSummary(out ScenarioSummary ss))
                {
                    ScenarioPermissionSettings permissionSettings = ss.ScenarioSettings.LoadSetting<ScenarioPermissionSettings>(ScenarioPermissionSettings.Key);


                    if (permissionSettings.Shared)
                        // Shared Scenarios have their permissions defined by values set in their User/GroupIdToAccessLevel lookups, or through default values for those missing.
                        // Note that we can't use the ScenarioPermissionSettings interface for checking permissions here (e.g. CanUserEdit) - this method is intended to get each permission in isolation,
                        // and shouldn't for instance say a user has CanEdit access if that is only in virtue of it using its Group Permission.
                    {
                        foreach (GroupedScenarioPermissions groupPermission in groupPermissions.Values)
                        {
                            AddExplicitGroupPermission(permissionSettings, groupPermission);

                            AddExplicitUserPermissions(permissionSettings, groupPermission);
                        }
                    }
                    else
                        // Unshared scenarios are accessible only to their owner and a few select others, but don't reflect this in their AccessLevel lookups.
                    {
                        foreach (GroupedScenarioPermissions groupPermission in groupPermissions.Values)
                        {
                            foreach (UserPermissionDto userInfo in groupPermission.UserPermissions.Values)
                            {
                                // Leverage the built-in logic for users here - this will check for ownership, special admin status, etc.
                                EUserAccess userPermission = EUserAccess.None;
                                if (permissionSettings.CanUserEdit(new BaseId(userInfo.UserId), new BaseId(groupPermission.GroupId)))
                                {
                                    userPermission = EUserAccess.Edit;
                                }

                                userInfo.UserAccess = userPermission;
                            }
                        }
                    }

                    // Add permissions for this scenario to the overall collection
                    scenarioPermissionSets.Add(new ScenarioPermissionSet
                    {
                        ScenarioId = scenario.Id.Value,
                        ScenarioName = scenario.Name,
                        ScenarioOwnerId = permissionSettings.OwnerId.Value,
                        ScenarioOwnerName = GetScenarioOwnerName(groupPermissions, permissionSettings),
                        Shared = permissionSettings.Shared,
                        GroupScenarioPermissions = groupPermissions.Values.OrderBy(gsp => gsp.GroupId).ToList()
                    });
                }
            }
        }

        return scenarioPermissionSets;
    }

    /// <summary>
    /// The scenario owner name is not directly stored; but we can find it as we build the permissions structure.
    /// </summary>
    /// <param name="allGroupPermissions"></param>
    /// <param name="permissionSettings"></param>
    /// <returns></returns>
    private static string GetScenarioOwnerName(Dictionary<BaseId, GroupedScenarioPermissions> allGroupPermissions, ScenarioPermissionSettings permissionSettings)
    {
        return allGroupPermissions.Values
                                  .SelectMany(group => group.UserPermissions.Values) // search flattened list of users
                                  .FirstOrDefault(user => user.UserId == permissionSettings.OwnerId.Value)
                                  ?.UserName;
    }

    /// <summary>
    /// Returns a list of user Ids/Names, grouped by their Permission Set/Group.
    /// </summary>
    /// <returns></returns>
    private static Dictionary<BaseId, GroupedScenarioPermissions> GetUserInfoByGroup()
    {
        Dictionary<BaseId, GroupedScenarioPermissions> userIdsPerGroup = new ();
        using (SystemController.Sys.UsersLock.EnterRead(out UserManager um))
        {
            // Each group should be present, even if no users are currently members.
            userIdsPerGroup = um.GetUserPermissionSets()
                                .ToDictionary(x => x.Id,
                                    x => new GroupedScenarioPermissions
                                    {
                                        GroupId = x.Id.Value,
                                        GroupName = x.Name,
                                        GroupPermission = ScenarioPermissionSettings.c_DefaultGroupAccess, // will be overwritten if a value is found later in the process
                                        UserPermissions = new Dictionary<long, UserPermissionDto>()
                                    });

            // Attach users to their corresponding group.
            foreach (User user in um)
            {
                if (user.Id != BaseId.ServerId && user.Id != BaseId.ERP_ID)
                {
                    GroupedScenarioPermissions permissionsForGroup = userIdsPerGroup[user.UserPermissionSetId];
                    permissionsForGroup.UserPermissions.Add(user.Id.Value,
                        new UserPermissionDto
                        {
                            UserName = user.Name,
                            UserId = user.Id.Value,
                            UserAccess = ScenarioPermissionSettings.c_DefaultUserAccess, // will be overwritten if a value is found later in the process
                            GroupAccess = ScenarioPermissionSettings.c_DefaultGroupAccess
                        });
                }
            }
        }

        return userIdsPerGroup;
    }

    private static void AddExplicitGroupPermission(ScenarioPermissionSettings a_permissionSettings, GroupedScenarioPermissions a_groupPermission)
    {
        if (a_permissionSettings.GroupIdToAccessLevel.TryGetValue(new BaseId(a_groupPermission.GroupId), out EUserAccess explicitGroupPermission))
        {
            a_groupPermission.GroupPermission = explicitGroupPermission;
        }

        // If not found, permission will remain as default
    }

    private static void AddExplicitUserPermissions(ScenarioPermissionSettings a_permissionSettings, GroupedScenarioPermissions a_groupPermission)
    {
        foreach (UserPermissionDto userPermission in a_groupPermission.UserPermissions.Values)
        {
            if (a_permissionSettings.UserIdToAccessLevel.TryGetValue(new BaseId(userPermission.UserId), out EUserAccess explicitPermission))
            {
                userPermission.UserAccess = explicitPermission;
            }
            // If not found, permission will remain as default

            userPermission.GroupAccess = a_groupPermission.GroupPermission;
        }
    }
    #endregion

    public void LogDesync(ChecksumValues a_serverChecksum, ChecksumValues a_clientChecksum, ConnectedUserData a_user, string a_scenarioName, long a_scenarioId, StringArrayList a_userLogContent)
    {
        string userId = a_user?.Id.ToString() ?? "Unknown";
        string userName = a_user?.ReadableName ?? "Unknown";
        string comparisonDescription = a_serverChecksum.GetComparisonDescription(a_clientChecksum, a_scenarioId);

        PTSystem.LogDesyncScenarioDumps(a_serverChecksum, a_clientChecksum, userId, userName, a_scenarioName, a_userLogContent);

        PTDesyncException dex = new PTDesyncException("3114", new object[] { userName, userId, comparisonDescription });

        System.Text.StringBuilder sb = new();
        ExceptionDescriptionInfo edi = new(dex);

        m_errorReporter.LogException(edi, ELogClassification.Desync);
    }

    private readonly Dictionary<BaseId, Dictionary<Guid, (long, string)>> m_checksumAudit = new Dictionary<BaseId, Dictionary<Guid, (long, string)>>();
    private readonly Dictionary<BaseId, Dictionary<Guid, (long, string)>> m_serverProcessedChecksumAudit = new Dictionary<BaseId, Dictionary<Guid, (long, string)>>();
    public void LogMissingChecksum(BaseId a_scenarioId, Guid a_transmissionGuid, string a_sessionToken)
    {
        if (m_checksumAudit.TryGetValue(a_scenarioId, out Dictionary<Guid, (long, string)> scenarioAudit))
        {
            if (!scenarioAudit.TryGetValue(a_transmissionGuid, out (long, string) o_audit))
            {
                scenarioAudit[a_transmissionGuid] = (PTDateTime.UtcNow.Ticks, a_sessionToken);
            }
            else if (m_serverProcessedChecksumAudit.TryGetValue(a_scenarioId, out Dictionary<Guid, (long, string)> serverAudit) && serverAudit.TryGetValue(a_transmissionGuid, out (long, string) o_serverProcessed))
            {
                string time = string.Format("Checksum with Id:'{0}' was calculated by the server at :'{1}' however the client attempt to retrieve it at : '{2}' for scenario : '{3}'",
                    a_transmissionGuid, new DateTime(o_serverProcessed.Item1), PTDateTime.UtcNow, a_scenarioId);
                m_errorReporter.Log("Client behind Server", time, ELogClassification.Desync);

                serverAudit.Remove(a_transmissionGuid);
            }
        }
        else
        {
            Dictionary<Guid, (long, string)> checksum = new Dictionary<Guid, (long, string)>();
            checksum.Add(a_transmissionGuid, (PTDateTime.UtcNow.Ticks, a_sessionToken));
            m_checksumAudit[a_scenarioId] = checksum;
        }
    }

    public void CheckRequestedChecksum(BaseId a_scenarioId, Guid a_transmissionGuid)
    {
        if (m_checksumAudit.TryGetValue(a_scenarioId, out Dictionary<Guid, (long, string)> scenarioAudit) && scenarioAudit.TryGetValue(a_transmissionGuid, out (long, string) o_auditDetails))
        {
            TransmissionSession transmissionSession = GetSession(o_auditDetails.Item2);

            if (transmissionSession is UserSession userSession)
            {
                string time = string.Format("Checksum with Id: '{0}' was requested at :'{1}' by user '{2}' however the server processed and make it available at :'{3}' for scenario: '{4}'",
                    a_transmissionGuid, new DateTime(o_auditDetails.Item1), userSession.UserId, PTDateTime.UtcNow, a_scenarioId);
                m_errorReporter.Log("Client ahead of server", time, ELogClassification.Desync);
            }

            scenarioAudit.Remove(a_transmissionGuid);
        }
        else
        {
            if (!m_serverProcessedChecksumAudit.TryGetValue(a_scenarioId, out Dictionary<Guid, (long, string)> serverAudit))
            {
                serverAudit = new Dictionary<Guid, (long, string)>();
                m_serverProcessedChecksumAudit[a_scenarioId] = serverAudit;
            }

            if (!serverAudit.ContainsKey(a_transmissionGuid))
            {
                serverAudit[a_transmissionGuid] = (PTDateTime.UtcNow.Ticks, "");
            }
        }
    }
}
