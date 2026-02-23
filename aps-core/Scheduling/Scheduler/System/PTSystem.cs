using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.Common.Localization;
using PT.Common.Threading;
using PT.ERPTransmissions;
using PT.PackageDefinitions;
using PT.Scheduler.ErrorReporting;
using PT.Scheduler.PackageDefs;
using PT.Scheduler.Sessions;
using PT.SchedulerDefinitions;
using PT.ServerManagerAPIProxy.APIClients;
using PT.ServerManagerSharedLib.Data;
using PT.ServerManagerSharedLib.Definitions;
using PT.ServerManagerSharedLib.DTOs.Entities;
using PT.ServerManagerSharedLib.Helpers;
using PT.SystemDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.SystemServiceDefinitions;
using PT.Transmissions;
using PT.Transmissions.Interfaces;
using PT.Transmissions.User;

namespace PT.Scheduler;

/// <summary>
/// Outside of the broadcaster this contains all the data in the system.
/// </summary>
public class 
    
    
    PTSystem : IDeserializationInit, IPTSerializable
{
    #region IPTSerializable Members
    private readonly bool m_hasReadVersionNumber;
    private readonly int m_readVersionNumber = -1;

    /// <summary>
    /// Whether the system is an existing system that was read from disk.
    /// </summary>
    internal bool HadReadVersionNumber => m_hasReadVersionNumber;

    /// <summary>
    /// The version number of this system as it was written on disk before the system was started.
    /// </summary>
    internal int ReadVersionNumber => m_readVersionNumber;

    public static bool IsNewVersion()
    {
        Common.SoftwareVersion currentVersion = Common.AssemblyVersionChecker.GetAssemblyVersion();
        // s_scenarioSerializedFromVersion being null should mean that we started with a blank scenario
        // so the current version is the version it'll be serialized in.
        int compareNum = currentVersion.CompareTo(new Common.SoftwareVersion(s_scenarioSerializedFromVersion ?? currentVersion.ToString()));
        return compareNum == 1;
    }

    private static string s_scenarioSerializedFromVersion;
    #if DEBUG
    public static Common.SoftwareVersion ScenarioSerializedFromVersion => new (s_scenarioSerializedFromVersion);
    #endif
    /// <summary>
    /// Flag to indicate this instance of PTSystem was created for temporal purposes and its disposal shouldn't override files on disk
    /// </summary>
    internal bool IsTempSystem { get; }

    public PTSystem(IReader a_reader, long a_startDate, bool a_startup_onFirstLoadAllowResequencingOfIds_reloadToFixupIdsDontResequence, int a_maxNumberOfScenarios, int a_checksumDiagnosticsFrequency, (string serialCode, string token, EnvironmentType environmentType) a_licenseSessionInfo)
        : this(a_reader, a_startup_onFirstLoadAllowResequencingOfIds_reloadToFixupIdsDontResequence, a_maxNumberOfScenarios, a_checksumDiagnosticsFrequency, a_licenseSessionInfo,false)
    {
        m_startDate = a_startDate;
    }

    private PTSystem(IReader a_reader, bool a_startup_onFirstLoadAllowResequencingOfIds_reloadToFixupIdsDontResequence, int a_maxNumberOfScenarios, int a_checksumDiagnosticsFrequency, (string serialCode, string token, EnvironmentType environmentType) a_licenseSessionInfo, bool a_isTempSystem)
    {
        DeadTransmissionInit();
        IsTempSystem = a_isTempSystem;

        m_hasReadVersionNumber = true;
        m_readVersionNumber = a_reader.VersionNumber;

        //Versions older than 11.50 (serialization 739) are not supported. They need to upgrade to 11.50 or newer first, then to 12
        if (a_reader.VersionNumber < 739)
        {
            // Localizer has not been loaded at this point so the localizing the string here does nothing
            throw new PTException("This scenario data model is not compatible with version 12. The data should first be upgraded using an instance of version 11.50 or newer.");
        }

        SetUserManagerEvents(new UserManagerEvents());
        PackageManager packageManager = new (new PackageAssemblyLoader(WorkingDirectory.PackagesPath, ServerSessionManager.PackageErrorsLogPath));

        if (a_reader.VersionNumber >= 12511)
        {
            a_reader.Read(out s_scenarioSerializedFromVersion);
            a_reader.Read(out m_startDate);
            a_reader.Read(out m_lastTransmissionNbr);
            a_reader.Read(out s_readOnly);
            m_systemSettings = new GenericSettingSaver(a_reader);
            SetPackageManager(packageManager); // for versions before 12000.

            m_usersBaseIdGenerator = new BaseIdGenerator(a_reader);
            UserManager userManager = SystemController.WebAppActionsClient != null ?
                new WebAppUserManager(SystemController.WebAppActionsClient, a_reader, m_usersBaseIdGenerator) :
                new UserManager(a_reader, m_usersBaseIdGenerator);

            SetUsers(userManager);
            ScenarioManager scenarioManager = new ScenarioManager(a_reader, this, packageManager.GetLicenseValidationModules());
            SetScenarios(scenarioManager);

            m_scenarioManagerDispatcher = new Dispatcher(a_reader, EDispatcherOwner.ScenarioManager);


            using (m_scenariosLock.EnterWrite())
            {
                m_scenarioManager.RestoreReferences(a_reader.VersionNumber, this, m_users, s_systemLogger, a_startup_onFirstLoadAllowResequencingOfIds_reloadToFixupIdsDontResequence, packageManager, a_maxNumberOfScenarios, a_checksumDiagnosticsFrequency, a_licenseSessionInfo);
            }

            m_users.RestoreReferences(s_systemLogger, WorkingDirectoryInstance.WorkspacesPath, m_packageManager, scenarioManager.UserFieldDefinitionManager);
        }
        else
        {
            #region 12100
            if (a_reader.VersionNumber >= 12210)
            {
                a_reader.Read(out m_startDate);
                a_reader.Read(out m_lastTransmissionNbr);
                a_reader.Read(out s_readOnly);
                m_systemSettings = new GenericSettingSaver(a_reader);
                SetPackageManager(packageManager); // for versions before 12000.

                m_usersBaseIdGenerator = new BaseIdGenerator(a_reader);
                UserManager userManager = SystemController.WebAppActionsClient != null ?
                    new WebAppUserManager(SystemController.WebAppActionsClient, a_reader, m_usersBaseIdGenerator) :
                    new UserManager(a_reader, m_usersBaseIdGenerator);

                SetUsers(userManager);
                ScenarioManager scenarioManager = new ScenarioManager(a_reader, this, packageManager.GetLicenseValidationModules());
                SetScenarios(scenarioManager);

                m_scenarioManagerDispatcher = new Dispatcher(a_reader, EDispatcherOwner.ScenarioManager);

                a_reader.Read(out s_scenarioSerializedFromVersion);

                using (m_scenariosLock.EnterWrite())
                {
                    m_scenarioManager.RestoreReferences(a_reader.VersionNumber, this, m_users, s_systemLogger, a_startup_onFirstLoadAllowResequencingOfIds_reloadToFixupIdsDontResequence, packageManager, a_maxNumberOfScenarios, a_checksumDiagnosticsFrequency, a_licenseSessionInfo);
                }

                m_users.RestoreReferences(s_systemLogger, WorkingDirectoryInstance.WorkspacesPath, m_packageManager, scenarioManager.UserFieldDefinitionManager);
            }

            else if (a_reader.VersionNumber >= 12100)
            {
                a_reader.Read(out m_startDate);
                a_reader.Read(out m_lastTransmissionNbr);
                a_reader.Read(out s_readOnly);
                m_systemSettings = new GenericSettingSaver(a_reader);
                SetPackageManager(packageManager); // for versions before 12000.

                m_usersBaseIdGenerator = new BaseIdGenerator(a_reader);

                UserManager userManager = SystemController.WebAppActionsClient != null ? new WebAppUserManager(SystemController.WebAppActionsClient, a_reader, m_usersBaseIdGenerator) : new UserManager(a_reader, m_usersBaseIdGenerator);

                SetUsers(userManager);

                ScenarioManager scenarioManager = new ScenarioManager(a_reader, this, packageManager.GetLicenseValidationModules());
                SetScenarios(scenarioManager);

                m_scenarioManagerDispatcher = new Dispatcher(a_reader, EDispatcherOwner.ScenarioManager);
                new Dispatcher(a_reader, EDispatcherOwner.NotSet);
                a_reader.Read(out s_scenarioSerializedFromVersion);

                using (m_scenariosLock.EnterWrite())
                {
                    m_scenarioManager.RestoreReferences(a_reader.VersionNumber, this, m_users, s_systemLogger, a_startup_onFirstLoadAllowResequencingOfIds_reloadToFixupIdsDontResequence, packageManager, a_maxNumberOfScenarios, a_checksumDiagnosticsFrequency, a_licenseSessionInfo);
                }

                m_users.RestoreReferences(s_systemLogger, WorkingDirectoryInstance.WorkspacesPath, m_packageManager, scenarioManager.UserFieldDefinitionManager);
            }
            #endregion

            #region 12000
            else if (a_reader.VersionNumber >= 12000)
            {
                a_reader.Read(out m_startDate);
                a_reader.Read(out m_lastTransmissionNbr);
                a_reader.Read(out s_readOnly);
                //Removing this section. Key is loaded differently now and this is no longer needed

                // load key so serialization doesn't break, but only assign if this is not server and Key hasn't already been loaded from key
                //                LicenseKey licenseKey = new LicenseKey(a_reader);
                //                if (!PTSystem.Server)
                //                {
                //#if DEBUG
                //                    if (s_licenseKey.LoadedFromKey)
                //                    {
                //                        DebugException.ThrowInDebug("LoadedFromKey validation error. This should not have occurred.");
                //                    }
                //#endif
                //                    s_licenseKey = licenseKey;
                //                }

                m_systemSettings = new GenericSettingSaver(a_reader);
                SetPackageManager(packageManager); // for versions before 12000.

                m_usersBaseIdGenerator = new BaseIdGenerator(a_reader);

                UserManager userManager = SystemController.WebAppActionsClient != null ? new WebAppUserManager(SystemController.WebAppActionsClient, a_reader, m_usersBaseIdGenerator) : new UserManager(a_reader, m_usersBaseIdGenerator);

                SetUsers(userManager);
                ScenarioManager scenarioManager = new ScenarioManager(a_reader, this, packageManager.GetLicenseValidationModules());
                SetScenarios(scenarioManager);

                m_scenarioManagerDispatcher = new Dispatcher(a_reader, EDispatcherOwner.ScenarioManager);
                new Dispatcher(a_reader, EDispatcherOwner.NotSet);

                new SystemStartupOptions(a_reader);
                a_reader.Read(out s_scenarioSerializedFromVersion);

                using (m_scenariosLock.EnterWrite())
                {
                    m_scenarioManager.RestoreReferences(a_reader.VersionNumber, this, m_users, s_systemLogger, a_startup_onFirstLoadAllowResequencingOfIds_reloadToFixupIdsDontResequence, packageManager, a_maxNumberOfScenarios, a_checksumDiagnosticsFrequency, a_licenseSessionInfo);
                }

                m_users.RestoreReferences(s_systemLogger, WorkingDirectoryInstance.WorkspacesPath, m_packageManager, scenarioManager.UserFieldDefinitionManager);
            }
            #endregion

            else
            {
                m_systemSettings = new GenericSettingSaver();
                SetPackageManager(packageManager); // for versions before 12000.

                #region 734
                if (a_reader.VersionNumber >= 734)
                {
                    a_reader.Read(out m_startDate);
                    a_reader.Read(out m_lastTransmissionNbr);
                    a_reader.Read(out s_readOnly);

                    // load key so serialization doesn't break, but only assign if this is not server and Key hasn't already been loaded from key
                    LicenseKey licenseKey = new (a_reader);
                    if (!Server)
                    {
                        #if DEBUG
                        if (s_licenseKey.LoadedFromKey)
                        {
                            DebugException.ThrowInDebug("LoadedFromKey validation error. This should not have occurred.");
                        }
                        #endif
                        s_licenseKey = licenseKey;
                    }

                    m_usersBaseIdGenerator = new BaseIdGenerator(a_reader);

                    UserManager userManager = SystemController.WebAppActionsClient != null ? new WebAppUserManager(SystemController.WebAppActionsClient, a_reader, m_usersBaseIdGenerator) : new UserManager(a_reader, m_usersBaseIdGenerator);

                    SetUsers(userManager);
                    ScenarioManager scenarioManager = new ScenarioManager(a_reader, this, packageManager.GetLicenseValidationModules());
                    SetScenarios(scenarioManager);

                    m_scenarioManagerDispatcher = new Dispatcher(a_reader, EDispatcherOwner.ScenarioManager);
                    new Dispatcher(a_reader, EDispatcherOwner.NotSet);

                    new SystemStartupOptions(a_reader);
                    //VersionNumber helpful when debugging customer issues
                    a_reader.Read(out s_scenarioSerializedFromVersion);

                    using (m_scenariosLock.EnterWrite())
                    {
                        if (Server)
                        {
                            m_scenarioManager.PurgeBlackBoxScenarios(s_systemLogger);
                        }

                        m_scenarioManager.RestoreReferences(a_reader.VersionNumber, this, m_users, s_systemLogger, a_startup_onFirstLoadAllowResequencingOfIds_reloadToFixupIdsDontResequence, packageManager, a_maxNumberOfScenarios, a_checksumDiagnosticsFrequency, a_licenseSessionInfo);
                    }

                    m_users.RestoreReferences(s_systemLogger, WorkingDirectoryInstance.WorkspacesPath, m_packageManager, scenarioManager.UserFieldDefinitionManager);
                }
                #endregion
            }

            using (m_scenariosLock.EnterWrite())
            {
                foreach (Scenario scenario in m_scenarioManager.Scenarios)
                {
                    scenario.RestoreReferences(m_scenarioManager.UserFieldDefinitionManager);
                }
            }
        }

        if (Server)
        {
            m_transmissionLogger.SetTransmissionLoggingVals(GetStartupVals().LogDbConnectionString, GetStartupVals().InstanceName, GetStartupVals().SoftwareVersion);
        }

        ValidateScenarioLimit(a_maxNumberOfScenarios);
        if (!a_isTempSystem)
        {
            StartBackgroundProcessesAndReceiving();
        }
    }

    private void ValidateScenarioLimit(int a_maxNumberOfScenarios)
    {
        if (m_scenarioManager.LoadedScenarioCount > a_maxNumberOfScenarios)
        {
            throw new PTValidationException("3021", new object[] { m_scenarioManager.LoadedScenarioCount, a_maxNumberOfScenarios });
        }
    }

    internal void TouchScenarios()
    {
        using (ScenariosLock.EnterWrite(out ScenarioManager sm))
        {
            sm.InitPermissionValidationModules(m_packageManager.GetPermissionValidationModules());
        }
    }

    private bool m_serializeForClient;

    public void Serialize(IWriter a_writer, params string[] a_params)
    {
        if (a_params[0] == Scenario.SerializeForClient)
        {
            m_serializeForClient = true;
        }
        Serialize(a_writer);

        m_serializeForClient = false;
    }
    public void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        a_writer.Write(Common.AssemblyVersionChecker.GetAssemblyVersion().ToString());
        a_writer.Write(m_startDate);
        a_writer.Write(m_lastTransmissionNbr);
        a_writer.Write(s_readOnly);

        //We no longer serialize the key
        //s_licenseKey.Serialize(a_writer);

        m_systemSettings.Serialize(a_writer);

        m_usersBaseIdGenerator.Serialize(a_writer);

        using (ObjectAccess<UserManagerEvents> um = UserManagerEventsLock.EnterRead())
        {
            m_users.Serialize(a_writer);
        }

        using (ObjectAccess<ScenarioManager> sm = ScenariosLock.EnterRead())
        {
            if (m_serializeForClient)
            {
                sm.Instance.Serialize(a_writer, Scenario.SerializeForClient);
            }
            else
            {
                sm.Instance.Serialize(a_writer);
            }
        }

        m_scenarioManagerDispatcher.Serialize(a_writer);

    }

    public void SerializeForClient(IWriter a_writer, BaseId a_userId, HashSet<BaseId> a_previousSessionLoadedScenarioIds, out IEnumerable<BaseId> a_recentLoadedScenarioIds)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        a_writer.Write(Common.AssemblyVersionChecker.GetAssemblyVersion().ToString());
        a_writer.Write(m_startDate);
        a_writer.Write(m_lastTransmissionNbr);
        a_writer.Write(s_readOnly);

        //We no longer serialize the key
        //s_licenseKey.Serialize(a_writer);

        m_systemSettings.Serialize(a_writer);

        m_usersBaseIdGenerator.Serialize(a_writer);

        using (ObjectAccess<UserManagerEvents> um = UserManagerEventsLock.EnterRead())
        {
            m_users.Serialize(a_writer);
        }

        using (ObjectAccess<ScenarioManager> sm = ScenariosLock.EnterRead())
        {
            sm.Instance.SerializeForClient(a_writer, a_userId, a_previousSessionLoadedScenarioIds, out a_recentLoadedScenarioIds);
        }

        m_scenarioManagerDispatcher.Serialize(a_writer);
    }

    public void SerializeAllUnloadedScenarioData(IWriter a_writer, BaseId a_userId, HashSet<BaseId> a_loadedScenarioIds)
    {
        using (ObjectAccess<ScenarioManager> sm = ScenariosLock.EnterRead())
        {
            sm.Instance.SerializeAllUnloadedScenarioData(a_writer, a_userId, a_loadedScenarioIds);
        }
    }

    public const int UNIQUE_ID = 415;

    public int UniqueId => UNIQUE_ID;
    #endregion

    #region Static Create System helpers
    /// <summary>
    /// Create a system from a set of compressed system bytes.
    /// </summary>
    /// <param name="a_compressedSystemBytes">The compressed system bytes.</param>
    /// <param name="a_symmetricKey"></param>
    /// <param name="a_maxNumberOfScenarios"></param>
    /// <param name="a_checksumDiagnosticsFrequency"></param>
    /// <param name="a_licenseSessionInfo"></param>
    /// <param name="a_webAppsClient"></param>
    /// <param name="a_tempSystem"></param>
    /// <returns>The PTSystem created from the compressed bytes.</returns>
    public static PTSystem CreateSystem(byte[] a_compressedSystemBytes, byte[] a_symmetricKey, int a_maxNumberOfScenarios, int a_checksumDiagnosticsFrequency, (string serialCode, string token, EnvironmentType environmentType) a_licenseSessionInfo, bool a_tempSystem = false)
    {
        PTSystem sys;
        using (BinaryMemoryReader reader = new (a_symmetricKey, a_compressedSystemBytes))
        {
            sys = new PTSystem(reader, false, a_maxNumberOfScenarios, a_checksumDiagnosticsFrequency, a_licenseSessionInfo, a_tempSystem);
        }

        //TODO: Verify scheduling packages have been loaded
        using (sys.ScenariosLock.EnterRead(out ScenarioManager sm))
        {
            using (sys.PackageManagerLock.EnterRead(out IPackageManager packageManager))
            {
                sm.InitDataLicensing(packageManager.GetLicenseValidationModules());
                sm.InitPermissionValidationModules(packageManager.GetPermissionValidationModules());
            }
        }

        return sys;
    }

    //public Dictionary<int, PackageInfoExtended> GetLoadedPackages()
    //{
    //    return m_packageManager.LoadedPackages;
    //}
    #endregion

    #region Disposal
    public void Dispose()
    {
        Dispose(false);
    }

    private bool m_disposed;

    private void Dispose(bool finalizing)
    {
        if (!m_disposed)
        {
            m_disposed = true;
            using (m_scenariosLock.EnterWrite())
            {
                m_scenarioManager.Dispose();
            }
        }
    }
    #endregion

    #region Error handling object accessors. Static loggers that log to the main log.
    internal enum SchedulingErrorType { System, Key }

    internal static ISystemLogger SystemLogger => s_systemLogger;

    public ISystemLogger SystemLoggerInstance
    {
        get => s_systemLogger;
        set => s_systemLogger = value;
    }

    private static ISystemLogger s_systemLogger;
    #endregion

    #region Variables
    /// <summary>
    /// This is a workaround for a problem Microsoft. Inconsistently Scenario files don't get written when the service is shut down.
    /// </summary>
    private readonly long m_startDate;

    public static long GetStoredStartDate(IReader reader)
    {
        long startDate = 0;
        if (reader.VersionNumber >= 83)
        {
            reader.Read(out startDate);
        }

        return startDate;
    }

    private IClientSession m_clientSession;

    internal void SetClientSession(IClientSession a_clientSession)
    {
        m_clientSession = a_clientSession;
    }

    /// <summary>
    /// A global cancellation token that can be used to abort threads
    /// </summary>
    public static CancellationToken CancelToken;

    private static TransmissionClassFactory s_trnClassFactory;

    public static TransmissionClassFactory TrnClassFactory
    {
        get
        {
            if (s_trnClassFactory == null)
            {
                s_trnClassFactory = new TransmissionClassFactory();
            }

            return s_trnClassFactory;
        }
    }

    private static LicenseKey s_licenseKey = new ();
    public static LicenseKey LicenseKey => s_licenseKey;

    #region dispatchers
    private readonly Dispatcher m_scenarioManagerDispatcher;

    /// <summary>
    /// Wait for all the transmissions in the dispatcher to play.
    /// </summary>
    internal void FlushDispatcher()
    {
        using (ScenariosLock.EnterRead(out ScenarioManager sm))
        {
            for (int i = 0; i < sm.LoadedScenarioCount; i++)
            {
                sm.GetByIndex(i).FlushDispatcher();
            }
        }

        m_scenarioManagerDispatcher.Flush();
    }

    private void DispatchScenarioManager(PTTransmission a_t, int a_remainingQueuedTransmissions = 0)
    {
        try
        {
            switch (a_t)
            {
                case IDataChangesDependentT<PTTransmission> dcdT:
                    m_scenarioManager.Receive(dcdT);
                    break;
                case ScenarioBaseT sbT:
                    m_scenarioManager.Receive(sbT);
                    break;
                default:
                    DebugException.ThrowInDebug("Unhandled transmission type passed to ScenarioManager.");
                    break;
            }
        }
        catch (Exception e)
        {
            s_systemLogger.LogException(e, null);
        }
    }
    #endregion

    private ulong m_lastTransmissionNbr;

    internal ulong LastTransmissionNbr => m_lastTransmissionNbr;

    internal void UpdateLastTransmissionNbr(ulong a_transmissionNbr)
    {
        if (a_transmissionNbr > m_lastTransmissionNbr)
        {
            m_lastTransmissionNbr = a_transmissionNbr;
        }
    }
    #endregion

    #region Construction
    public PTSystem()
    {
        DeadTransmissionInit();
        m_usersBaseIdGenerator = new BaseIdGenerator();

        UserManager userManager = SystemController.WebAppActionsClient != null ?
            new WebAppUserManager(SystemController.WebAppActionsClient, s_systemLogger, m_usersBaseIdGenerator, WorkingDirectory.WorkspacesPath) :
            new UserManager(s_systemLogger, m_usersBaseIdGenerator, WorkingDirectory.WorkspacesPath);

        SetUsers(userManager);
        SetUserManagerEvents(new UserManagerEvents());
        m_systemSettings = new GenericSettingSaver();
        if (Server)
        {
            m_transmissionLogger.SetTransmissionLoggingVals(GetStartupVals().LogDbConnectionString, GetStartupVals().InstanceName, GetStartupVals().SoftwareVersion);
        }

        SetPackageManager(new PackageManager(new PackageAssemblyLoader(WorkingDirectory.PackagesPath, ServerSessionManager.PackageErrorsLogPath)));
        m_users.InitNewUserManager(m_packageManager);
        SetScenarios(new ScenarioManager(this, s_systemLogger, m_packageManager, m_users.GetAdministrator(), m_users, GetStartupVals().ScenarioLimit, GetStartupVals().EnableChecksumDiagnostics));

        m_scenarioManagerDispatcher = new Dispatcher(EDispatcherOwner.ScenarioManager);

        StartBackgroundProcessesAndReceiving();
    }

    private void StartBackgroundProcessesAndReceiving()
    {
        m_scenarioManagerDispatcher.TransmissionReceived += DispatchScenarioManager;

        PostLoadDataInit();
        //After settings and package manager is loaded, start the background processes.
        StartAllBackgroundProcesses();
    }
    #endregion

    #region Safe objects and multi-threading
    private UserManagerEvents m_userManagerEvents;
    private AutoReaderWriterLock<UserManagerEvents> m_userManagerEventsLock;

    public AutoReaderWriterLock<UserManagerEvents> UserManagerEventsLock => m_userManagerEventsLock;

    private void SetUserManagerEvents(UserManagerEvents a_userManagerEvents)
    {
        m_userManagerEvents = a_userManagerEvents;
        m_userManagerEventsLock = new AutoReaderWriterLock<UserManagerEvents>(m_userManagerEvents);
    }

    private readonly BaseIdGenerator m_usersBaseIdGenerator;
    private UserManager m_users;
    private AutoReaderWriterLock<UserManager> m_usersLock;

    public AutoReaderWriterLock<UserManager> UsersLock => m_usersLock;

    private void SetUsers(UserManager a_users)
    {
        m_users = a_users;
        m_usersLock = new AutoReaderWriterLock<UserManager>(m_users);
    }

    private ScenarioManager m_scenarioManager;
    private AutoReaderWriterLock<ScenarioManager> m_scenariosLock;

    public AutoReaderWriterLock<ScenarioManager> ScenariosLock => m_scenariosLock;

    private void SetScenarios(ScenarioManager a_scenarios)
    {
        m_scenarioManager = a_scenarios;
        m_scenariosLock = new AutoReaderWriterLock<ScenarioManager>(m_scenarioManager);
    }

    private const decimal c_autoLockTimeoutFactor = 1.06m;
    private const int c_autoLockTimeoutMax = 2250;

    /// <param name="a_id">Id of the Scenario</param>
    /// <param name="o_scenario">Scenario with matching Id or null</param>
    /// <returns>AutoDisposer that releases the lock on ScenarioManager.</returns>
    public AutoDisposer AutoEnterRead(BaseId a_id, out Scenario o_scenario)
    {
        int timeout = 250;

        while (true)
        {
            try
            {
                AutoDisposer autoDisposer = m_scenariosLock.TryEnterRead(out ScenarioManager sm, timeout);
                o_scenario = sm.Find(a_id);
                return autoDisposer;
            }
            catch (AutoTryEnterException)
            {
                timeout = Math.Min((int)(timeout * c_autoLockTimeoutFactor), c_autoLockTimeoutMax);
            }
        }
    }

    /// <param name="a_id">Id of the Scenario</param>
    /// <param name="o_scenario">Scenario with matching Id or null</param>
    /// <returns>AutoDisposer that releases the lock on ScenarioManager.</returns>
    public AutoDisposer AutoEnterWrite(BaseId a_id, out Scenario o_scenario)
    {
        int timeout = 100;

        while (true)
        {
            try
            {
                AutoDisposer autoDisposer = m_scenariosLock.TryEnterWrite(out ScenarioManager sm, timeout);
                o_scenario = sm.Find(a_id);
                return autoDisposer;
            }
            catch (AutoTryEnterException)
            {
                timeout = Math.Min((int)(timeout * c_autoLockTimeoutFactor), c_autoLockTimeoutMax);
            }
        }
    }

    /// <param name="a_id">Id of the Scenario</param>
    /// <param name="o_scenario">Scenario with matching Id or null</param>
    /// <param name="o_sd">ScenarioDetail if Scenario was found</param>
    /// <returns>AutoDisposer that releases ScenarioManager and ScenarioDetail Locks.</returns>
    public AutoDisposer AutoEnterRead(BaseId a_id, out Scenario o_scenario, out ScenarioDetail o_sd)
    {
        int timeout = 100;
        AutoDisposer smDisposer = null;

        while (true)
        {
            try
            {
                smDisposer = m_scenariosLock.TryEnterRead(out ScenarioManager sm, timeout);
                o_scenario = sm.Find(a_id);
                if (o_scenario == null)
                {
                    o_sd = null;
                    return smDisposer;
                }

                AutoDisposer sdDisposer = o_scenario.ScenarioDetailLock.TryEnterRead(out o_sd, timeout);
                return new AutoDisposer(new Action(() =>
                {
                    smDisposer.Dispose();
                    sdDisposer.Dispose();
                }));
            }
            catch (AutoTryEnterException)
            {
                if (smDisposer != null)
                {
                    smDisposer.Dispose();
                }

                timeout = Math.Min((int)(timeout * c_autoLockTimeoutFactor), c_autoLockTimeoutMax);
            }
        }
    }

    /// <param name="a_id">Id of the Scenario</param>
    /// <param name="o_scenario">Scenario with matching Id or null</param>
    /// <param name="o_sd">ScenarioDetail if Scenario was found</param>
    /// <returns>AutoDisposer that releases ScenarioManager and ScenarioDetail Locks.</returns>
    public AutoDisposer AutoEnterWrite(BaseId a_id, out Scenario o_scenario, out ScenarioDetail o_sd)
    {
        int timeout = 100;
        AutoDisposer smDisposer = null;

        while (true)
        {
            try
            {
                smDisposer = m_scenariosLock.TryEnterRead(out ScenarioManager sm, timeout);
                o_scenario = sm.Find(a_id);
                if (o_scenario == null)
                {
                    o_sd = null;
                    return smDisposer;
                }

                AutoDisposer sdDisposer = o_scenario.ScenarioDetailLock.TryEnterWrite(out o_sd, timeout);
                return new AutoDisposer(new Action(() =>
                {
                    smDisposer.Dispose();
                    sdDisposer.Dispose();
                }));
            }
            catch (AutoTryEnterException)
            {
                if (smDisposer != null)
                {
                    smDisposer.Dispose();
                }

                timeout = Math.Min((int)(timeout * c_autoLockTimeoutFactor), c_autoLockTimeoutMax);
            }
        }
    }

    /// <param name="a_id">Id of the Scenario</param>
    /// <param name="o_sd">ScenarioDetail if Scenario was found</param>
    /// <returns>AutoDisposer that releases ScenarioManager and ScenarioDetail Locks.</returns>
    public AutoDisposer AutoEnterRead(BaseId a_id, out ScenarioDetail o_sd)
    {
        int timeout = 100;
        AutoDisposer smDisposer = null;

        while (true)
        {
            try
            {
                smDisposer = m_scenariosLock.TryEnterRead(out ScenarioManager sm, timeout);
                Scenario scenario = sm.Find(a_id);
                if (scenario == null)
                {
                    o_sd = null;
                    return smDisposer;
                }

                AutoDisposer sdDisposer = scenario.ScenarioDetailLock.TryEnterRead(out o_sd, timeout);
                return new AutoDisposer(new Action(() =>
                {
                    smDisposer.Dispose();
                    sdDisposer.Dispose();
                }));
            }
            catch (AutoTryEnterException)
            {
                if (smDisposer != null)
                {
                    smDisposer.Dispose();
                }

                timeout = Math.Min((int)(timeout * c_autoLockTimeoutFactor), c_autoLockTimeoutMax);
            }
        }
    }

    /// <param name="a_id">Id of the Scenario</param>
    /// <param name="o_sd">ScenarioDetail if Scenario was found</param>
    /// <returns>AutoDisposer that releases ScenarioManager and ScenarioDetail Locks.</returns>
    public AutoDisposer AutoEnterWrite(BaseId a_id, out ScenarioDetail o_sd)
    {
        int timeout = 100;
        AutoDisposer smDisposer = null;

        while (true)
        {
            try
            {
                smDisposer = m_scenariosLock.TryEnterRead(out ScenarioManager sm, timeout);
                Scenario scenario = sm.Find(a_id);
                if (scenario == null)
                {
                    o_sd = null;
                    return smDisposer;
                }

                AutoDisposer sdDisposer = scenario.ScenarioDetailLock.TryEnterWrite(out o_sd, timeout);
                return new AutoDisposer(new Action(() =>
                {
                    smDisposer.Dispose();
                    sdDisposer.Dispose();
                }));
            }
            catch (AutoTryEnterException)
            {
                if (smDisposer != null)
                {
                    smDisposer.Dispose();
                }

                timeout = Math.Min((int)(timeout * c_autoLockTimeoutFactor), c_autoLockTimeoutMax);
            }
        }
    }

    /// <param name="a_id">Id of the User</param>
    /// <param name="o_sd">User if found, null otherwise</param>
    /// <returns>AutoDisposer that releases UserManagerLock.</returns>
    public AutoDisposer AutoEnterRead(BaseId a_id, out User o_u)
    {
        int timeout = 100;
        AutoDisposer umDisposer = null;

        while (true)
        {
            try
            {
                umDisposer = m_usersLock.TryEnterRead(out UserManager um, timeout);
                o_u = um.GetById(a_id);
                return umDisposer;
            }
            catch (AutoTryEnterException)
            {
                timeout = Math.Min((int)(timeout * c_autoLockTimeoutFactor), c_autoLockTimeoutMax);
            }
        }
    }

    /// <summary>
    /// If you're obtaining a lock on both UserManager and ScenarioManager then use this function
    /// since attempting to lock them in different order can cause a dead lock (one thread has
    /// a lock on um and is waiting for sm while another as a lock on sm waiting for um).
    /// </summary>
    /// <param name="o_um"></param>
    /// <param name="o_sm"></param>
    /// <returns></returns>
    public AutoDisposer AutoEnterRead(out UserManager o_um, out ScenarioManager o_sm)
    {
        int timeout = 100;
        AutoDisposer smDisposer = null;

        while (true)
        {
            try
            {
                smDisposer = ScenariosLock.TryEnterRead(out o_sm, timeout);
                AutoDisposer umDisposer = UsersLock.TryEnterRead(out o_um, timeout);
                return new AutoDisposer(new Action(() =>
                {
                    smDisposer.Dispose();
                    umDisposer.Dispose();
                }));
            }
            catch (AutoTryEnterException)
            {
                if (smDisposer != null)
                {
                    smDisposer.Dispose();
                }

                timeout = Math.Min((int)(timeout * c_autoLockTimeoutFactor), c_autoLockTimeoutMax);
            }
        }
    }
    #endregion

    #region Reception
    public ClientSession.ReceiveDelegate GetReceiveDelegate()
    {
        return new ClientSession.ReceiveDelegate(Receive);
    }

    public delegate void TransmissionProcessedCompleteDelegate(Transmission a_t, TimeSpan a_processingTime, Exception a_e);

    public event TransmissionProcessedCompleteDelegate TransmissionProcessedEvent;

    internal void FireTransmissionProcessedEvent(Transmission a_t, TimeSpan a_processingTime, Exception a_e)
    {
        TransmissionProcessedEvent?.Invoke(a_t, a_processingTime, a_e);
    }

    public delegate void TransmissionDelegate(Transmission t);

    public event TransmissionDelegate TransmissionEvent;
    private HashSet<int> m_deadTransmissions;

    private void DeadTransmissionInit()
    {
        List<int> temp = new ();
        temp.Add(SystemStartupOptionsT.UNIQUE_ID);
        m_deadTransmissions = new HashSet<int>(temp);
    }

    public void Receive(Transmission a_t)
    {
        try
        {
            RecordingHandler(a_t);

            ProcessT(a_t);
        }
        catch (PTHandleableException e)
        {
            s_systemLogger.LogException(e, null);
        }
        catch (Exception e)
        {
            s_systemLogger.LogException(e, null, true);
            throw;
        }
        finally
        {
        }
    }

    private void ProcessT(Transmission a_t)
    {
        bool processedAtSysLevel = false;
        DateTime start = DateTime.UtcNow;
        Exception e = new ();

        try
        {
            if (!m_deadTransmissions.Contains(a_t.UniqueId))
            {
                TransmissionEvent?.Invoke(a_t);

                if (a_t is PacketT validPacketT)
                {
                    foreach (PTTransmission ptTransmission in validPacketT)
                    {
                        ProcessT(ptTransmission);
                    }

                    processedAtSysLevel = true;
                }
                else if (a_t is SystemSettingDataT systemSettingsT)
                {
                    m_systemSettings.SaveSetting(systemSettingsT.SettingData, true);
                }
                else if (a_t is UserBaseT or UserT)
                {
                    // Pass downstream to eventually be delegated to UserManager from ScenarioManager
                    m_scenarioManagerDispatcher.Receive((PTTransmission)a_t);
                }
                else if (a_t is PerformImportBaseT performImportBaseT)
                {
                    PerformImportHandler(performImportBaseT);
                    m_scenarioManagerDispatcher.Receive(performImportBaseT);

                    if (a_t is PerformImportCompletedT completedT && completedT.Exceptions.Count > 0)
                    {
                        s_systemLogger.LogException(completedT.Exceptions, completedT, new ScenarioExceptionInfo(), ELogClassification.PtInterface, false);
                    }
                }
                else if (a_t is SystemMessageT systemMessageT)
                {
                    processedAtSysLevel = true;
                    FireSystemMessageEvent(systemMessageT);
                }
                else if (a_t is ScenarioBaseT or ERPTransmission)
                {
                    m_scenarioManagerDispatcher.Receive((PTTransmission)a_t);
                }
                else if (a_t is SystemStateSwitchT switchT)
                {
                    processedAtSysLevel = true;
                    SwitchSystemState(switchT);
                }
                else if (a_t is DebuggingBaseT)
                {
                    processedAtSysLevel = true;
                    if (a_t is TriggerRecordingPlaybackT triggerRecordingPlaybackT)
                    {
                        if (Server)
                        {
                            SystemController.ServerSessionManager.Receive(triggerRecordingPlaybackT);
                        }
                    }
                }
                else if (a_t is WorkspaceSharedDeleteT workspaceSharedDeleteT)
                {
                    processedAtSysLevel = true;
                    if (Server)
                    {
                        WorkspaceDeleteShared(workspaceSharedDeleteT);
                    }

                    FireWorkspaceSharedDeleteEvent(workspaceSharedDeleteT);
                }
                else if (a_t is WorkspaceTemplateUpdateT workspaceTemplateUpdateT)
                {
                    processedAtSysLevel = true;
                    if (Server)
                    {
                        WorkspaceUpdateTemplate(workspaceTemplateUpdateT);
                    }

                    FireWorkspaceSharedUpdateEvent(workspaceTemplateUpdateT);
                }
                else if (a_t is LicenseKeyT licKeyT)
                {
                    LicenseKey.Update(licKeyT.LicenseKeyObj);
                    processedAtSysLevel = true;
                }
                else if (a_t is SendSupportEmailT sendSupportEmailT)
                {
                    SendSupportEmailT(sendSupportEmailT);
                }
                else if (a_t is PackageStateT packageStateT)
                {
                    processedAtSysLevel = true;
                    using (PackageManagerLock.EnterWrite())
                    {
                        //TODO: AssemblyLoader accept this transmission instead of package manager
                        m_packageManager.UpdatePackageStates(packageStateT);
                    }
                }
                else if (a_t is ClientUserRestartT clientRestartT)
                {
                    processedAtSysLevel = true;
                    FireRestartRequiredEvent(clientRestartT);
                }
                else if (a_t is PackageUpdateT packageUpdateT)
                {
                    using (PackageManagerLock.EnterWrite())
                    {
                        processedAtSysLevel = true;
                        //TODO: Sync to disk
                        //m_packageManager.UpdateLoadedPackges(packageUpdateT, m_systemSettings);
                        FireRestartServiceRequiredEvent(packageUpdateT);
                    }
                }
                else if (a_t is InstanceMessageT instanceMessageT)
                {
                    processedAtSysLevel = true;
                    FireInstanceMessageReceivedEvent(instanceMessageT);
                }
                else if (a_t is DataActivationWarningT dataWarningT)
                {
                    if (Server)
                    {
                        s_systemLogger.LogException(new PTException("The system data has changed significantly from the one validated for the license.  A warning to the users has been issued."), null, false, ELogClassification.Key);
                    }

                    processedAtSysLevel = true;
                    FireDataActivationWarningReceivedEvent(dataWarningT);
                }
                else
                {
                    PTValidationException ptValidationException = new ("2288");
                    e = ptValidationException;
                    throw ptValidationException;
                }
            }
        }
        finally
        {
            if (processedAtSysLevel && TransmissionProcessedEvent != null)
            {
                TransmissionProcessedEvent?.Invoke(a_t, DateTime.UtcNow - start, e);
            }
        }
    }

    /// <summary>
    /// Sends an email to LogEmailAddresses specified in the Instance. If email or log email settings are not setup, function returns with no message logged.
    /// </summary>
    /// <param name="a_messageBody"></param>
    internal void SendLogEmail(Exception a_err, PTTransmission a_t = null, string a_suggestion = "")
    {
        if (RunningMassRecordings || !Server || a_err is PTHandleableException)
        {
            return;
        }

        try
        {
            // TODO: The v12 Server Manager doesn't seem to provide the ability to set these SMTP settings. Do we still want this functionality?
            IInstanceSettingsManager instanceSettingsManager = InstanceSettingManagerFactory.CreateInstanceSettingsManagerForInstance(m_serverStartupVals.InstanceDatabaseConnectionString, 
                Environment.MachineName, m_serverStartupVals.InstanceId, m_serverStartupVals.ApiKey, m_serverStartupVals.WebAppEnv);
            InstanceSettingsEntity instanceSettingsEntity = instanceSettingsManager.GetInstanceSettingsEntity(m_serverStartupVals.InstanceName, m_serverStartupVals.SoftwareVersion);

            if (string.IsNullOrEmpty(instanceSettingsEntity.SmtpServerAddress) || instanceSettingsEntity.SmtpServerPort == 0 || string.IsNullOrEmpty(instanceSettingsEntity.SmtpFromEmail) || string.IsNullOrEmpty(instanceSettingsEntity.LogEmailToAddresses))
            {
                return;
            }

            string headerText = FileLogger.GetTransmissionExceptionHeader(a_t, a_suggestion);
            ExceptionDescriptionInfo edi = new (a_err);
            string messageBody = SimpleExceptionLogger.CreateErrorMsgString(edi, new ScenarioExceptionInfo(), headerText);

            using (Emailer emailer = new (instanceSettingsEntity.SmtpServerAddress, instanceSettingsEntity.SmtpServerPort, instanceSettingsEntity.SmtpEnableSsl, instanceSettingsEntity.SmtpUserName, PtEncryptor.Decrypt(instanceSettingsEntity.SmtpPassword)))
            {
                emailer.NewEmail(GetLogEmailSubject(instanceSettingsEntity), messageBody, instanceSettingsEntity.LogEmailToAddresses.Split(',').ToList(), instanceSettingsEntity.SmtpFromEmail);
                emailer.Send();
            }
        }
        catch (Exception err)
        {
            s_systemLogger.LogException(new EmailException("2925", err), a_t,ELogClassification.Fatal, false);
        }
    }

    private string GetLogEmailSubject(InstanceSettingsEntity a_InstanceSettingsEntity)
    {
        string subject = "";
        if (!string.IsNullOrEmpty(a_InstanceSettingsEntity.LogEmailSubjectPrefix))
        {
            subject = $"{a_InstanceSettingsEntity.LogEmailSubjectPrefix}: ";
        }

        subject = subject + string.Format("New APS Log Entry for Instance '{0}' Version '{1}' SerialCode '{2}'".Localize(), a_InstanceSettingsEntity.Name, a_InstanceSettingsEntity.Version, LicenseKey?.SerialCode);
        return subject;
    }

    /// <summary>
    /// Sends an email to SupportEmailToAddresses set on the instance.
    /// </summary>
    /// <param name="a_t"></param>
    private void SendSupportEmailT(SendSupportEmailT a_t)
    {
        if (RunningMassRecordings || !Server)
        {
            return;
        }

        try
        {
            IInstanceSettingsManager instanceSettingsManager = InstanceSettingManagerFactory.CreateInstanceSettingsManagerForInstance(m_serverStartupVals.InstanceDatabaseConnectionString,
                Environment.MachineName, m_serverStartupVals.InstanceId, m_serverStartupVals.ApiKey, m_serverStartupVals.WebAppEnv);
            InstanceSettingsEntity instanceSettingsEntity = instanceSettingsManager.GetInstanceSettingsEntity(m_serverStartupVals.InstanceName, m_serverStartupVals.SoftwareVersion);

            if (string.IsNullOrEmpty(instanceSettingsEntity.SmtpServerAddress) || instanceSettingsEntity.SmtpServerPort == 0)
            {
                s_systemLogger.LogException(new EmailException("2925"), a_t, ELogClassification.Fatal, false);
                return;
            }

            if (string.IsNullOrEmpty(instanceSettingsEntity.SmtpFromEmail) || string.IsNullOrEmpty(instanceSettingsEntity.SupportEmailToAddresses))
            {
                s_systemLogger.LogException(new EmailException("2925"), a_t, ELogClassification.Fatal, false);
                return;
            }

            using (Emailer emailer = new (instanceSettingsEntity.SmtpServerAddress, instanceSettingsEntity.SmtpServerPort, instanceSettingsEntity.SmtpEnableSsl, instanceSettingsEntity.SmtpUserName, PtEncryptor.Decrypt(instanceSettingsEntity.SmtpPassword)))
            {
                emailer.NewEmail(GetSupportEmailSubject(instanceSettingsEntity), a_t.MessageBody, instanceSettingsEntity.SupportEmailToAddresses.Split(',').ToList(), instanceSettingsEntity.SmtpFromEmail);
                emailer.Send();
            }
        }
        catch (Exception err)
        {
            s_systemLogger.LogException(new EmailException("2925", err), a_t, ELogClassification.Fatal, false);
        }
    }

    private static string GetSupportEmailSubject(InstanceSettingsEntity a_InstanceSettingsEntity)
    {
        string subject = "";
        if (!string.IsNullOrEmpty(a_InstanceSettingsEntity.SupportEmailSubjectPrefix))
        {
            subject = $"{a_InstanceSettingsEntity.SupportEmailSubjectPrefix}: ";
        }

        subject = subject + string.Format("Support Email for Instance '{0}' Version '{1}' SerialCode '{2}'".Localize(), a_InstanceSettingsEntity.Name, a_InstanceSettingsEntity.Version, LicenseKey?.SerialCode);
        return subject;
    }

    /// <summary>
    /// Save user settings file that were sent from the client.
    /// These files will be saved to the Client Updater.
    /// </summary>
    private void WorkspaceUpdateTemplate(WorkspaceTemplateUpdateT a_workspaceTemplateUpdateT)
    {
        try
        {
            string targetDir = Path.Combine(GetLocalGridLayoutsFilePath(), "Template", "Workspace", a_workspaceTemplateUpdateT.WorkspaceName);
            string tempFile = Path.Combine(targetDir, "template.bin");
            //Remove whole directory to synch files. 
            if (Directory.Exists(targetDir))
            {
                FileUtils.DeleteDirectoryRecursivelyWithRetry(targetDir);
            }

            Directory.CreateDirectory(targetDir);
            //Create a temp zip file, extract it, and then delete it.
            File.WriteAllBytes(tempFile, a_workspaceTemplateUpdateT.ZipFileBytes);
        }
        catch (Exception e)
        {
            throw new PTException("4119", e);
        }
    }

    /// <summary>
    /// Delete shared workspace files
    /// </summary>
    private static void WorkspaceDeleteShared(WorkspaceSharedDeleteT a_workspaceSharedDeleteT)
    {
        try
        {
            string targetDir = Path.Combine(GetLocalGridLayoutsFilePath(), "Shared", "Workspace", a_workspaceSharedDeleteT.SharedWorkspaceToDelete);
            //Remove whole directory to synch files. 
            if (Directory.Exists(targetDir))
            {
                FileUtils.DeleteDirectoryRecursivelyWithRetry(targetDir);
            }
        }
        catch (Exception e)
        {
            throw new PTException("4119", e);
        }
    }

    /// <summary>
    /// Get file path in the current instance ClientUpdater directory.
    /// </summary>
    public static string GetLocalGridLayoutsFilePath()
    {
        if (!RunningMassRecordings)
        {
            return WorkingDirectory.ClientUpdaterUserSettings;
        }

        return WorkingDirectory.UnitTest;
    }

    /// <summary>
    /// switches system state by setting readyOnly.
    /// </summary>
    private void SwitchSystemState(SystemStateSwitchT a_t)
    {
        if (ReadOnly != a_t.MakeReadOnly)
        {
            const string msg = "System State changed from {0} to {1}";
            if (ReadOnly)
            {
                s_systemLogger.LogException(new PTException(string.Format(msg.Localize(), "Read-only".Localize(), "active".Localize())), null, false, ELogClassification.Key);
            }
            else
            {
                s_systemLogger.LogException(new PTException(string.Format(msg.Localize(), "active".Localize(), "Read-only".Localize())), null, false, ELogClassification.Key);
            }
        }

        ReadOnly = a_t.MakeReadOnly;
        LicenseKey.LicenseStatus = a_t.LicenseStatus;

        FireSystemStateSwitchEvent(a_t);
    }

    private void PerformImportHandler(PerformImportBaseT t)
    {
        if (t is PerformImportStartedT)
        {
            if (EnableDiagnostics)
            {
                SystemLogger.Log("Import Logging", "Running PerformImportStartedHandler");
            }
            PerformImportStartedHandler((PerformImportStartedT)t);
            if (EnableDiagnostics)
            {
                SystemLogger.Log("Import Logging", "Finished PerformImportStartedHandler");
            }
        }
        else if (t is PerformImportCompletedT)
        {
            if (EnableDiagnostics)
            {
                SystemLogger.Log("Import Logging", "Running PerformImportCompletedHandler");
            }
            PerformImportCompletedHandler((PerformImportCompletedT)t);
            if (EnableDiagnostics)
            {
                SystemLogger.Log("Import Logging", "Finished PerformImportCompletedHandler");
            }
        }
        else if (Server && !t.Recording && t is TriggerImportT triggerImportT)
        {
            if (EnableDiagnostics)
            {
                SystemLogger.Log("Import Logging", "Running ERP Import");
            }

            RefreshScenarioDataHelper.RunERPImport(SystemController.ImportingService, triggerImportT.TargetScenarioId, t.Instigator, triggerImportT.TargetConfigId);

            if (EnableDiagnostics)
            {
                SystemLogger.Log("Import Logging", "Finished ERP Import");
            }
        }
    }

    // stores transmission while scenarios process ImportT
    private PerformImportCompletedT m_inProcessImportCompleteT;

    // number of scenarios that are still processing ImportT
    private int m_scenariosRunningImport;

    /// <summary>
    /// setup import progress tracking and Fire Import Started event
    /// </summary>
    /// <param name="a_t"></param>
    private void PerformImportStartedHandler(PerformImportStartedT a_t)
    {
        m_inProcessImportCompleteT = null;
        m_scenariosRunningImport = m_scenarioManager.LoadedScenarioCount;

        FirePerformImportStartedEvent(a_t);
    }

    /// <summary>
    /// store import completed transmission. Call ImportProgressUpdated
    /// in case scenarios process ImportT too quickly.
    /// </summary>
    /// <param name="a_t"></param>
    private void PerformImportCompletedHandler(PerformImportCompletedT a_t)
    {
        m_inProcessImportCompleteT = a_t;
        ImportProgressUpdated();
    }

    /// <summary>
    /// Scenario should call this when it has processed ImportT.
    /// </summary>
    internal void ReportScenarioImportComplete()
    {
        m_scenariosRunningImport--;
        ImportProgressUpdated();
    }

    /// <summary>
    /// if all Scenarios have completed processing and PerformImportCompletedT
    /// has been received, FirePerformImportCompletedEvent.
    /// </summary>
    private void ImportProgressUpdated()
    {
        if (m_scenariosRunningImport == 0 && m_inProcessImportCompleteT != null)
        {
            FirePerformImportCompletedEvent(m_inProcessImportCompleteT);
            m_inProcessImportCompleteT = null;
        }
    }
    #endregion

    #region Transmission Recording
    /// <summary>
    /// Indicates whether any problems were detected while attempting to record transmissions.
    /// If an error occurs this variable is used to prevent further transmissions from being recorded.
    /// </summary>
    private readonly TransmissionLogger m_transmissionLogger = new (s_systemLogger);

    private bool m_goodRecording = true;

    /// <summary>
    /// If recordings are enabled this function will record each transmission that enters the system.
    /// In the event a problem occurs while a transmission is being recorded, transmission recording will
    /// cease and an error will be written within the transmission recording folder. If an error occurs
    /// while trying to record this error an error message is written with the static function
    /// PT.Common.File.SimpleExceptionLogger.LogExceptionToDesktop.
    /// </summary>
    /// <param name="t"></param>
    private void RecordingHandler(Transmission t)
    {
        if (Server) //Only the server logs recording actions
        {
            try
            {
                if (m_goodRecording)
                {
                    if (m_serverStartupVals.Record)
                    {
                        TransmissionRecording tr = new (t);
                        Type type = t.GetType();
                        //TODO: Use working directory for all of this
                        string path = WorkingDirectory.GetRecordingFilePath(GetNextRecordNumber(), type.FullName);
                        using (BinaryFileWriter writer = new (path, Common.Compression.ECompressionType.Normal))
                        {
                            tr.Serialize(writer);
                        }
                    }

                    //Always log action to DB if configured, even if not storing the actual recordings.
                    Task.Run(new Action(() => m_transmissionLogger.LogTransmissionToSQL((PTTransmission)t)));
                }
            }
            catch (Exception e)
            {
                LogRecordingError(e, "An exception was caught while trying to write a transmission recording.".Localize());
            }
        }
    }

    /// <summary>
    /// Log a recording erorr.
    /// </summary>
    /// <param name="e">The exception. This can be null.</param>
    /// <param name="message">Any extra text that you would like to include in this error message.</param>
    private void LogRecordingError(Exception e, string message)
    {
        m_goodRecording = false;

        string recordingErrorMessage = string.Format("{0} The recording is not good. {1}", message, WorkingDirectory.RecordingDirectory);

        try
        {
            string fileErrorPath = Path.Combine(WorkingDirectory.RecordingDirectory, "_PT_RECORDING_ERRORS.LOG");
            SimpleExceptionLogger.LogException(fileErrorPath, e, recordingErrorMessage);
        }
        catch (Exception internalException)
        {
            SimpleExceptionLogger.PTDefaultLog(internalException, recordingErrorMessage);
        }
    }

    /// <summary>
    /// Stores the schedule on the server in a special alerts folder. You can compare the two 
    /// schedules to determine the difference between what's on the server and the client.
    /// </summary>
    /// <param name="a_serverChecksum"></param>
    /// <param name="a_clientChecksum"></param>
    /// <param name="a_user"></param>
    /// <param name="a_scenarioName"></param>
    public static void LogDesyncScenarioDumps(ChecksumValues a_serverChecksum, ChecksumValues a_clientChecksum, string a_userId, string a_userName, string a_scenarioName, StringArrayList a_userLogContent)
    {
        string directoryForUser = Path.Combine(WorkingDirectory.CheckSumDifferences, a_userId);
        string subdirectoryServer = Path.Combine(directoryForUser, "Server");
        string subdirectoryClient = Path.Combine(directoryForUser, "Client");

        Directory.CreateDirectory(WorkingDirectory.CheckSumDifferences);
        Directory.CreateDirectory(directoryForUser);
        Directory.CreateDirectory(subdirectoryServer);
        Directory.CreateDirectory(subdirectoryClient);

        // Writing server-side logs
        string a_desyncDescriptionHeader = $"Desync Scenario Description File " +
                                           $"| User: {a_userName} (Id: {a_userId}) " +
                                           $"| Scenario: {a_scenarioName} (ID: {a_serverChecksum.ScenarioId}) " +
                                           $"| Transmission: {a_serverChecksum.TransmissionId}" +
                                           $"| BlockCount: {a_serverChecksum.BlockCount}" +
                                           $"| ResourceJobOperationCombos: {a_serverChecksum.ResourceJobOperationCombos}" +
                                           $"| StartAndEndSums: {a_serverChecksum.StartAndEndSums}" +
                                           $"| UserCultureInfo: {a_serverChecksum.ChecksumDiagnostics?.UserCultureInfoName ?? string.Empty}" +
                                           $"| RecordingIndex: {a_serverChecksum.ChecksumDiagnostics?.RecordingIndex}" +
                                           $"| LocalMachineTimeZoneId: {a_serverChecksum.ChecksumDiagnostics?.LocalMachineTimeZoneId ?? string.Empty}" +
                                           $"| UserPreferenceTimeZoneId: {a_serverChecksum.ChecksumDiagnostics?.UserPreferenceTimeZoneId ?? string.Empty}";

        TextFile tfServer = new TextFile();
        tfServer.AppendText(a_desyncDescriptionHeader);
        AppendTransmissionInfo(tfServer, a_serverChecksum);
        tfServer.AppendText(a_serverChecksum.ScheduleDescription);
        tfServer.WriteFile(Path.Combine(subdirectoryServer, $"{a_serverChecksum.TransmissionId}.txt"));

        // Writing client-side checksums
        a_desyncDescriptionHeader = $"Desync Scenario Description File " +
                                    $"| User: {a_userName} (Id: {a_userId}) " +
                                    $"| Scenario: {a_scenarioName} (ID: {a_clientChecksum.ScenarioId}) " +
                                    $"| Transmission: {a_clientChecksum.TransmissionId}" +
                                    $"| BlockCount: {a_clientChecksum.BlockCount}" +
                                    $"| ResourceJobOperationCombos: {a_clientChecksum.ResourceJobOperationCombos}" +
                                    $"| StartAndEndSums: {a_clientChecksum.StartAndEndSums}" +
                                    $"| UserCultureInfo: {a_clientChecksum.ChecksumDiagnostics?.UserCultureInfoName ?? string.Empty}" +
                                    $"| RecordingIndex: {a_clientChecksum.ChecksumDiagnostics?.RecordingIndex}" +
                                    $"| LocalMachineTimeZoneId: {a_clientChecksum.ChecksumDiagnostics?.LocalMachineTimeZoneId ?? string.Empty}" +
                                    $"| UserPreferenceTimeZoneId: {a_clientChecksum.ChecksumDiagnostics?.UserPreferenceTimeZoneId ?? string.Empty}";
        TextFile tfClient = new TextFile();
        tfClient.AppendText(a_desyncDescriptionHeader);
        AppendTransmissionInfo(tfClient, a_clientChecksum);
        tfClient.AppendText(a_clientChecksum.ScheduleDescription);
        tfClient.WriteFile(Path.Combine(subdirectoryClient, $"{a_clientChecksum.TransmissionId}.txt"));

        // Writing the client-end UI logs
        tfClient.ClearText();
        foreach (string log in a_userLogContent)
        {
            tfClient.AppendText(log);
        }
        tfClient.WriteFile(Path.Combine(subdirectoryClient, "ClientLogs.txt"));
    }

    private static void AppendTransmissionInfo(TextFile a_file, ChecksumValues a_checksum)
    {
        if (a_file == null || a_checksum == null)
        {
            return;
        }

        if (a_checksum.ChecksumDiagnostics == null)
        {
            return;
        }

        //Processed transmissions
        a_file.AppendText($"There are {a_checksum.ChecksumDiagnostics.RecentTransmissionsProcessed.Count} transmissions in the UndoSets. Here is their information: ");
        foreach ((ulong transmissionNbr, int transmissionUniqueId, BaseId instigatorId, DateTimeOffset timeStamp) in a_checksum.ChecksumDiagnostics.RecentTransmissionsProcessed)
        {
            string transmissionInfo = $"TransmissionNumber: {transmissionNbr} | TransmissionUniqueId: {transmissionUniqueId} | InstigatorId: {instigatorId} | TimeStamp {timeStamp.ToString()}";
            a_file.AppendText(transmissionInfo);
        }

        a_file.AppendText(Environment.NewLine);

        //Received transmissions
        a_file.AppendText($"There are {a_checksum.ChecksumDiagnostics.RecentTransmissionsReceived.Count} received transmissions. Here is their information: ");
        foreach ((ulong transmissionNbr, int transmissionUniqueId, BaseId instigatorId, DateTimeOffset timeStamp) in a_checksum.ChecksumDiagnostics.RecentTransmissionsReceived)
        {
            string transmissionInfo = $"TransmissionNumber: {transmissionNbr} | TransmissionUniqueId: {transmissionUniqueId} | InstigatorId: {instigatorId} | TimeStamp {timeStamp.ToString()}";
            a_file.AppendText(transmissionInfo);
        }
    }

    /// <summary>
    /// Removes old desync scenario description files from disk.
    /// These files are intended to be written and used only when checksum diagnostics are enabled. Once turned off, this method is used to clear space.
    /// </summary>
    /// <param name="a_constructorValues"></param>
    public static void CleanUnneededFiles(StartupValsAdapter a_constructorValues)
    {
        if ((ChecksumFrequencyType)a_constructorValues.EnableChecksumDiagnostics == SchedulerDefinitions.ChecksumFrequencyType.Regular && 
            Directory.Exists(WorkingDirectory.CheckSumDifferences))
        {
            FileUtils.DeleteDirectoryRecursivelyWithRetry(WorkingDirectory.CheckSumDifferences);
        }
    }

    long m_nextRecordNumber;
    long GetNextRecordNumber()
    {
        return m_nextRecordNumber++;
    }

    /// <summary>
    /// Returns the next transmission recording record number.
    /// </summary>
    /// <returns></returns>
    public long PeekNextTransmissionRecordingRecordNumber()
    {
        return m_nextRecordNumber;
    }

    private long GetLastRecordingNumber()
    {
        return m_nextRecordNumber - 1;
    }
    #endregion

    #region Static code for indicating whether this is the server.
    static PTSystem() { }

    private static bool m_server;

    /// <summary>
    /// Whether this process is the server. This value isn't serialized.
    /// </summary>
    public static bool Server
    {
        get => m_server;
        internal set => m_server = value;
    }

    private static bool s_exporterService;

    /// <summary>
    /// Whether this process is the Exporter Service. This value isn't serialized.
    /// After the exporter service starts PlanetTogether it needs to set this value to true.
    /// </summary>
    public static bool ExporterService
    {
        get => s_exporterService;
        set => s_exporterService = value;
    }
    #endregion

    #region Undo Checkpoint
    internal delegate void CheckpointDelegate();

    internal event CheckpointDelegate UndoCheckpointEvent;

    internal void FireCheckpoint()
    {
        UndoCheckpointEvent?.Invoke();
    }

    public delegate void SystemStateSwitchDelegate(SystemStateSwitchT a_t);

    public event SystemStateSwitchDelegate SystemStateSwitchEvent;

    internal void FireSystemStateSwitchEvent(SystemStateSwitchT a_t)
    {
        SystemStateSwitchEvent?.Invoke(a_t);
    }
    #endregion

    #region Startup Initialization
    void IDeserializationInit.DeserializationInit()
    {
        using (ObjectAccess<ScenarioManager> oa = ScenariosLock.EnterWrite())
        {
            IDeserializationInit di = oa.Instance;
            di.DeserializationInit();
        }
    }

    public static ISystemLogger StartupInit(StartupValsAdapter a_startupVals)
    {
        return StartupInit(new WorkingDirectoryPaths(a_startupVals), a_startupVals);
    }

    public static ISystemLogger StartupInit(WorkingDirectoryPaths a_workingDirectoryStructure, StartupValsAdapter a_startupVals)
    {
        WorkingDirectory = new WorkingDirectory(a_workingDirectoryStructure);
        ServerTimeZone = a_startupVals.GetServerTimeZone();

        //Checking null here for when we run internally
        if (s_systemLogger == null)
        {
            if (Server)
            {
                IErrorLogger sqlLogger = string.IsNullOrEmpty(a_startupVals.LogDbConnectionString) ? null : new SQLErrorLogger(a_startupVals.InstanceName, a_startupVals.SoftwareVersion, a_startupVals.LogDbConnectionString);
                s_systemLogger = new InstanceLogger(SystemController.WebAppActionsClient, a_options =>
                {
                    a_options.Dsn = a_startupVals.SentryDsn ?? String.Empty;
                    a_options.Debug = true;
                    a_options.AutoSessionTracking = true;
                    //TODO: SET THIS:
                    // a_options.Environment to Instance Environment (dev, prod, QA)
                    // Instance ID
                    // If we split the message out from the exception text logged to Sentry, also include Instance Name, Version
                }, sqlLogger, a_startupVals.WebApiTimeoutSeconds);
            }
            else
            {
                s_systemLogger = new ClientLogger(SystemController.CurrentUserId, WorkingDirectory.LogFolder);
            }
        }

        Localizer.ExceptionLoggingDirectory = WorkingDirectory.LogFolder;
        m_serverStartupVals = a_startupVals;
        return s_systemLogger;
    }

    [Obsolete("Temp for package startup from the UI. This should be removed")]
    public static ISystemLogger StartupInit(WorkingDirectoryPaths a_workingDirectoryStructure)
    {
        WorkingDirectory = new WorkingDirectory(a_workingDirectoryStructure);

        //Checking null here for when we run internally
        if (s_systemLogger == null)
        {
            if (Server)
            {
                IErrorLogger sqlLogger = string.IsNullOrEmpty(m_serverStartupVals.LogDbConnectionString) ? null : new SQLErrorLogger(m_serverStartupVals.InstanceName, m_serverStartupVals.SoftwareVersion, m_serverStartupVals.LogDbConnectionString);

                s_systemLogger = new InstanceLogger(SystemController.WebAppActionsClient, options =>
                {
                    options.Dsn = m_serverStartupVals.SentryDsn ?? String.Empty;
                    options.Debug = true;
                    options.AutoSessionTracking = true;
                }, sqlLogger, m_serverStartupVals.WebApiTimeoutSeconds);
            }
            else
            {
                s_systemLogger = new ClientLogger(SystemController.CurrentUserId, WorkingDirectory.LogFolder);
            }
        }

        Localizer.ExceptionLoggingDirectory = WorkingDirectory.LogFolder;
        return s_systemLogger;
    }
    /// <summary>
    /// Flag to indicate logging to sentry is set up and enabled
    /// </summary>
    private static StartupValsAdapter m_serverStartupVals;

    /// <summary>
    /// Copilot settings usable only by the server
    /// </summary>
    internal CoPilotSettings CopilotSettings => GetStartupVals().CoPilotSettings;

    public static bool EnableDiagnostics => false;//GetStartupVals().EnableDiagnostics;

    public bool Record
    {
        get
        {
            if (Server)
            {
                return GetStartupVals().Record;
            }

            return false;
        }
    }

    public static bool RunningFromRecordings
    {
        get
        {
            StartupVals startupVals = GetStartupVals();
            return startupVals.StartType == EStartType.RecordingClientDelayed || startupVals.StartType == EStartType.Recording;
        }
    }

    public static string InstanceId => GetStartupVals().InstanceId;
    public static string ApiKey => GetStartupVals().ApiKey;

    private static StartupVals GetStartupVals()
    {
        if (!Server)
        {
            throw new DebugException("Only the server should be accessing startup vals");
        }

        return m_serverStartupVals;
    }

    public TimeSpan AutoLogoffTimeout => TimeSpan.FromTicks(GetStartupVals().AutoLogoffTimeout);

    private static TimeZoneInfo s_serverTimeZone = TimeZoneInfo.Utc;
    /// <summary>
    /// Returns the Instance configured timezone
    /// </summary>
    public static TimeZoneInfo ServerTimeZone
    {
        get => s_serverTimeZone;
        private set => s_serverTimeZone = value;
    }

    public static WorkingDirectory WorkingDirectory;

    internal WorkingDirectory WorkingDirectoryInstance => WorkingDirectory;
    #endregion

    private static bool s_readOnly;

    /// <summary>
    /// If true transmissions aren't processed and thus no changes can be applied to the system (used when there's no valid license).
    /// </summary>
    public static bool ReadOnly
    {
        get => s_readOnly;
        internal set => s_readOnly = value;
    }

    /// <summary>
    /// Set when running MassRecordings. Can be used to run test code.
    /// </summary>
    private static bool s_runningMassRecordings;

    public static bool RunningMassRecordings
    {
        get => s_runningMassRecordings;
        set => s_runningMassRecordings = value;
    }

    /// <summary>
    /// Set when running MassRecordings. Can be used to run test code.
    /// </summary>
    private static string s_massRecordingsDatabase;

    public static string MassRecordingsDatabase
    {
        get => s_massRecordingsDatabase;
        set => s_massRecordingsDatabase = value;
    }

    /// <summary>
    /// Set when running MassRecordings. Used to associate database entries with the MR player.
    /// </summary>
    private static long s_massRecordingsSessionId;

    public static long MassRecordingsSessionId
    {
        get => s_massRecordingsSessionId;
        set => s_massRecordingsSessionId = value;
    }

    /// <summary>
    /// Set when running MassRecordings. Used to associate database entries with the MR player.
    /// </summary>
    private static string s_massRecordingsPlayerPath = "";


    public static string MassRecordingsPlayerPath
    {
        get => s_massRecordingsPlayerPath;
        set => s_massRecordingsPlayerPath = value;
    }

    #region Packages
    private PackageManager m_packageManager;
    private AutoReaderWriterLock<IPackageManager> m_packageManagerLock;
    public AutoReaderWriterLock<IPackageManager> PackageManagerLock => m_packageManagerLock;

    private void SetPackageManager(PackageManager a_packageManager)
    {
        m_packageManager = a_packageManager;
        m_packageManagerLock = new AutoReaderWriterLock<IPackageManager>(m_packageManager);
        using (m_packageManagerLock.EnterWrite())
        {
            m_packageManager.SetSettingsManager(m_systemSettings);
            m_packageManager.LoadPackagesFromDisk();

            if (Server)
            {
                LogPackagesLoaded();
            }
        }
    }

    private void LogPackagesLoaded()
    {
        AssemblyPackageInfo[] packageVersionsOnServer = GetPackageVersionsOnServer();
        PackageLogger packageLogger = new PackageLogger(s_systemLogger);
        packageLogger.SetPackageLoggingVals(GetStartupVals().LogDbConnectionString, GetStartupVals().InstanceName, GetStartupVals().SoftwareVersion);
        packageLogger.LogPackageToSQL(packageVersionsOnServer);
    }

    /// <summary>
    /// Called by client to request a package binary
    /// </summary>
    /// <returns></returns>
    public PackedAssembly GetPackedPackageAssembly(AssemblyPackageInfo a_AssemblyPackageInfo)
    {
        using (m_packageManagerLock.EnterWrite())
        {
            return m_packageManager.GetAssembly(a_AssemblyPackageInfo);
        }
    }

    /// <summary>
    /// Called by client to get a list of packages available
    /// </summary>
    /// <returns></returns>
    public AssemblyPackageInfo[] GetPackageVersionsOnServer()
    {
        List<AssemblyPackageInfo> packageVersionList = new ();
        using (m_packageManagerLock.EnterWrite())
        {
            foreach (AssemblyPackageInfo packageInfo in m_packageManager.GetValidatedPackageAssemblyInfos())
            {
                packageVersionList.Add(packageInfo);
            }
        }

        return packageVersionList.ToArray();
    }
    #endregion

    #region System Events
    public delegate void PerformImportStartedDelegate(PerformImportStartedT t);

    public event PerformImportStartedDelegate PerformImportStartedEvent;

    private void FirePerformImportStartedEvent(PerformImportStartedT t)
    {
        PerformImportStartedEvent?.Invoke(t);
    }

    public delegate void PerformImportCompletedDelegate(PerformImportCompletedT t);

    public event PerformImportCompletedDelegate PerformImportCompletedEvent;

    private void FirePerformImportCompletedEvent(PerformImportCompletedT t)
    {
        PerformImportCompletedEvent?.Invoke(t);
    }

    public delegate void SystemMessageDelegate(SystemMessageT a_t);

    public event SystemMessageDelegate SystemMessageEvent;

    private void FireSystemMessageEvent(SystemMessageT a_t)
    {
        SystemMessageEvent?.Invoke(a_t);
    }

    public delegate void WorkspaceSharedDeleteDelegate(WorkspaceSharedDeleteT a_t);

    public event WorkspaceSharedDeleteDelegate WorkspaceSharedDeleteEvent;

    private void FireWorkspaceSharedDeleteEvent(WorkspaceSharedDeleteT a_t)
    {
        WorkspaceSharedDeleteEvent?.Invoke(a_t);
    }

    public delegate void WorkspaceSharedUpdateDelegate(WorkspaceTemplateUpdateT a_t);

    public event WorkspaceSharedUpdateDelegate WorkspaceSharedUpdateEvent;

    private void FireWorkspaceSharedUpdateEvent(WorkspaceTemplateUpdateT a_t)
    {
        WorkspaceSharedUpdateEvent?.Invoke(a_t);
    }

    public delegate void RestartRequiredDelegate(ClientUserRestartT a_t);

    public event RestartRequiredDelegate RestartRequiredEvent;

    private void FireRestartRequiredEvent(ClientUserRestartT a_t)
    {
        RestartRequiredEvent?.Invoke(a_t);
    }

    public delegate void RestartServiceRequiredDelegate(PackageUpdateT a_t);

    public event RestartServiceRequiredDelegate RestartServiceRequiredEvent;

    private void FireRestartServiceRequiredEvent(PackageUpdateT a_t)
    {
        RestartServiceRequiredEvent?.Invoke(a_t);
    }

    public delegate void InstanceMessageReceivedDelegate(InstanceMessageT a_t);

    public event InstanceMessageReceivedDelegate InstanceMessageReceivedEvent;

    private void FireInstanceMessageReceivedEvent(InstanceMessageT a_t)
    {
        InstanceMessageReceivedEvent?.Invoke(a_t);
    }

    public delegate void DataActivationWarningReceivedDelegate(DataActivationWarningT a_t);

    public event DataActivationWarningReceivedDelegate DataActivationWarningEvent;

    private void FireDataActivationWarningReceivedEvent(DataActivationWarningT a_t)
    {
        DataActivationWarningEvent?.Invoke(a_t);
    }
    #endregion

    #region DataModelActivation
    private Timer m_DataModelActivationTimer;
    private bool m_warningAlreadySent;

    internal void InitiateDataModelActivationTimer()
    {
        if (Server && !string.IsNullOrEmpty(LicenseKey.DataModelActivationKey))
        {
            TimeSpan m_DataModelActivationTimespan = TimeSpan.FromMinutes(30);
            m_DataModelActivationTimer = new Timer(DataModelActivationTimerTick, m_DataModelActivationTimespan, 0, (int)m_DataModelActivationTimespan.TotalMilliseconds);
        }
    }

    private void DataModelActivationTimerTick(object a_o)
    {
        m_DataModelActivationTimer.Change(Timeout.Infinite, Timeout.Infinite);
        ValidateDataModelKey(LicenseKey.DataModelActivationKey);
        double timeout = ((TimeSpan)a_o).TotalMilliseconds;
        m_DataModelActivationTimer.Change((int)timeout, Timeout.Infinite);
    }

    /// <summary>
    /// Tests if the LicenseKey.DataModelActivationKey is valid.  If not, a warning is displayed.  If it fails an exception is thrown and the user should be sent into readonly
    /// </summary>
    private bool ValidateDataModelKey(string a_key)
    {
        bool showDangerMessage = false;
        try
        {
            DataModelActivation activationModel = new (a_key);

            if (activationModel == null)
            {
                return false;
            }

            using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
            {
                for (int i = 0; i < sm.LoadedScenarioCount; i++)
                {
                    Scenario s = sm.GetByIndex(i);
                    using (s.ScenarioDetailLock.EnterRead(out ScenarioDetail sd))
                    {
                        if (!activationModel.CompareTo(sd))
                        {
                            showDangerMessage = true;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            SetSystemReadOnly(true);
            return false;
        }

        if (showDangerMessage)
        {
            BroadcastWarningMessage();
            m_warningAlreadySent = true;
        }
        else
        {
            m_warningAlreadySent = false;
        }

        SetSystemReadOnly(false);
        return true;
    }

    private void BroadcastWarningMessage()
    {
        if (m_warningAlreadySent)
        {
            return;
        }

        DataActivationWarningT t = new ();
        m_clientSession.SendClientAction(t);
    }

    private void SetSystemReadOnly(bool a_setReadOnly)
    {
        if (a_setReadOnly && !ReadOnly)
        {
            SystemStateSwitchT t = new (true, LicenseKeyObject.ELicenseStatus.DataModelDivergence);
            m_clientSession.SendClientAction(t);

            s_systemLogger.Log("System is in Read-only mode".Localize(), PTDateTime.UtcNow.ToDateTime());
        }
        else if (!a_setReadOnly && ReadOnly)
        {
            SystemStateSwitchT t = new (false, LicenseKeyObject.ELicenseStatus.GoodStatus);
            m_clientSession.SendClientAction(t);
        }
    }

    public static string CreateActivationKey(ScenarioDetail a_sd)
    {
        DataModelActivation dma = new (a_sd);
        return dma.Key;
    }

    public bool ValidateKey(string a_key)
    {
        return ValidateDataModelKey(a_key);
    }
    #endregion

    private readonly GenericSettingSaver m_systemSettings;

    public ISettingsManager SystemSettings => m_systemSettings;

    internal void PostLoadDataInit()
    {
        foreach (IBackgroundProcessModule backgroundProcessModule in m_packageManager.GetBackgroundProcessModules())
        {
            foreach (IStartupElement startupElement in backgroundProcessModule.GetStartupElements(Server))
            {
                startupElement.PostDataLoadInit(this);
            }
        }
    }

    #region Long Running Processes
    private List<IBackgroundProcessElement> m_backgroundProcessElements;

    internal void StartAllBackgroundProcesses()
    {
        m_backgroundProcessElements = new List<IBackgroundProcessElement>();
        List<IBackgroundProcessModule> backgroundProcessModules = m_packageManager.GetBackgroundProcessModules();
        foreach (IBackgroundProcessModule bpm in backgroundProcessModules)
        {
            //TODO: Try Catch with package exception
            m_backgroundProcessElements.AddRange(bpm.GetBackgroundProcessElements(Server));
        }

        foreach (IBackgroundProcessElement serverLongRunningProcess in m_backgroundProcessElements)
        {
            serverLongRunningProcess.Start();
        }
    }

    internal void StopAllBackgroundProcesses()
    {
        foreach (IBackgroundProcessElement serverLongRunningProcess in m_backgroundProcessElements)
        {
            serverLongRunningProcess.Stop();
        }

        m_backgroundProcessElements.Clear();
    }
    #endregion

    public void InitDataLicensing()
    {
        using (ScenariosLock.EnterWrite(out ScenarioManager sm))
        {
            sm.InitDataLicensing(m_packageManager.GetLicenseValidationModules());
        }
    }

    /// <summary>
    /// Fires the RestartRequired Event using a ClientRestartT which is handled on MainForm.
    /// </summary>
    public void OnClientRestartEvent(string a_restartMessage)
    {
        //This transmission is never processed, it is just required by the FireRestartRequiredEvent.
        ClientUserRestartT restartT = new ClientUserRestartT(new List<BaseId>() { SystemController.CurrentUserId })
        {
            RestartClient = true,
            RestartMessage = a_restartMessage,
        };

        FireRestartRequiredEvent(restartT);
    }
}
