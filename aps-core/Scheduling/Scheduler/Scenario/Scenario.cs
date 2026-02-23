using System.Timers;

using PT.APIDefinitions.RequestsAndResponses;
using PT.APSCommon;
using PT.APSCommon.Exceptions;
using PT.APSCommon.Extensions;
using PT.Common.Collections;
using PT.Common.Compression;
using PT.Common.Debugging;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.Common.Http;
using PT.Common.Threading;
using PT.ERPTransmissions;
using PT.PackageDefinitions;
using PT.PackageDefinitions.PackageInterfaces;
using PT.PackageDefinitions.Settings;
using PT.PackageDefinitions.Settings.PublishOptions;
using PT.Scheduler.ErrorReporting;
using PT.Scheduler.PackageDefs;
using PT.Scheduler.Simulation.Extensions;
using PT.Scheduler.Simulation.UndoReceive;
using PT.Scheduler.Simulation.UndoReceive.Move;
using PT.Scheduler.TransmissionDispatchingAndReception;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using PT.Transmissions2;

namespace PT.Scheduler;

/// <summary>
/// </summary>
public class Scenario : IDeserializationInit, IPTSerializable
{
    public const string SerializeForClient = "Client";
    public const int UNIQUE_ID = 344;

    #region IPTSerializable Members
    #region Deserialize
    //************************************************************************
    // Read the server calculated checksum values.
    // You can clean delete these. They're no longer used.
    // The only time they're sent is when a client starts up.
    // The true parameter passes up from PTBroadcaster.
    // You might use this for debugging why checksums sometimes differ when sending to the client.
    //************************************************************************

    public Scenario(IReader reader)
    {
        SetScenarioUndoEvents(new ScenarioUndoEvents());
        m_licenseManager = new ScenarioLicenseManager();
        m_scenarioPermissionValidationManager = new ScenarioPermissionValidationManager();

        #region 125xx
        if (reader.VersionNumber >= 12514)
        {
            m_bools = new BoolVector32(reader);
            reader.Read(out m_dispatchTime);

            SetScenarioDetail(new ScenarioDetail(reader));
            SetScenarioSummary(new ScenarioSummary(reader));
            m_dispatcher = new Dispatcher(reader, EDispatcherOwner.Scenario);
            m_kpiController = new KpiController(reader);
            SetScenarioEvents(new ScenarioEvents());
            
            reader.Read(out bool isForClient);
            if (!isForClient)
            {
                m_nextUndoNbr = new Id(reader);
                SetUndoSets(new UndoSets(reader));
                DeserializeCheckpoints(reader);
            }
            else
            {
                m_nextUndoNbr = new Id();
                SetUndoSets(new UndoSets(this));
            }

            reader.Read(out LastReceivedTransmissionId);
            reader.Read(out LastReceivedTransmissionTimeTicks);
        }
        #endregion
        #region 12512
        else if (reader.VersionNumber >= 12512)
        {
            m_bools = new BoolVector32(reader);
            reader.Read(out m_dispatchTime);

            SetScenarioDetail(new ScenarioDetail(reader));
            SetScenarioSummary(new ScenarioSummary(reader));
            m_dispatcher = new Dispatcher(reader, EDispatcherOwner.Scenario);
            m_kpiController = new KpiController(reader);
            SetScenarioEvents(new ScenarioEvents());

            m_nextUndoNbr = new Id(reader);
            SetUndoSets(new UndoSets(reader));
            DeserializeCheckpoints(reader);

            reader.Read(out LastReceivedTransmissionId); 
        }
        #endregion
        #region 12407
        else if (reader.VersionNumber >= 12407)
        {
            m_bools = new BoolVector32(reader);
            reader.Read(out m_dispatchTime);

            SetScenarioDetail(new ScenarioDetail(reader));
            SetScenarioSummary(new ScenarioSummary(reader));
            m_dispatcher = new Dispatcher(reader, EDispatcherOwner.Scenario);
            m_kpiController = new KpiController(reader);
            SetScenarioEvents(new ScenarioEvents());

            m_nextUndoNbr = new Id(reader);
            SetUndoSets(new UndoSets(reader));
            DeserializeCheckpoints(reader);
        }
        #endregion
        #region 12309
        else
        {
            ScenarioPublishOptions publishOptions = new ScenarioPublishOptions();
            if (reader.VersionNumber >= 12309)
            {
                m_bools = new BoolVector32(reader);
                reader.Read(out m_dispatchTime);

                    SetScenarioDetail(new ScenarioDetail(reader, out publishOptions));
                SetScenarioSummary(new ScenarioSummary(reader));
                m_dispatcher = new Dispatcher(reader, EDispatcherOwner.Scenario);
                m_kpiController = new KpiController(reader);
                SetScenarioEvents(new ScenarioEvents());
                
                m_nextUndoNbr = new Id(reader);
                SetUndoSets(new UndoSets(reader));
                DeserializeCheckpoints(reader);
            }
            #endregion 12309
            #region 658
            else if (reader.VersionNumber >= 658)
            {
                m_bools = new BoolVector32(reader);
                reader.Read(out m_dispatchTime);

                m_nextUndoNbr = new Id(reader);

                SetScenarioDetail(new ScenarioDetail(reader, out publishOptions));
                SetScenarioSummary(new ScenarioSummary(reader));
                m_dispatcher = new Dispatcher(reader, EDispatcherOwner.Scenario);
                m_kpiController = new KpiController(reader);
                SetScenarioEvents(new ScenarioEvents());
                SetUndoSets(new UndoSets(reader));
                DeserializeCheckpoints(reader);
            }

            #endregion 658
            using (ScenarioSummaryLock.EnterWrite(out ScenarioSummary scenarioSummary))
            {
                SetPublishOptionsBackwardCompatibility(scenarioSummary, publishOptions);
            }

        }

        m_dispatcher.TransmissionReceived += Receive;
        StartListeningToEvents();
    }

    private void DeserializeCheckpoints(IReader a_reader)
    {
        if (!Directory.Exists(PTSystem.WorkingDirectory.Checkpoints))
        {
            Directory.CreateDirectory(PTSystem.WorkingDirectory.Checkpoints);
        }

        using (UndoSetsLock.EnterRead(out UndoSets uss))
        {
            for (int i = 0; i < uss.Count; ++i)
            {
                UndoSet us = uss[i];
                string fullPath = SerializationPath(us.m_undoNbr);

                a_reader.Read(out byte[] checkpointCompressedBytes);

                FileUtils.SaveBinaryFile(fullPath, checkpointCompressedBytes);
            }
        }
    }
    #endregion

    //Even when the dispatcher is flushed, the last action will be playing. If the system is writing data down while the action is playing,
    //the undo sets may get out of sync with the rest of the data
    private object m_serializationLock = new object();

    #region Serialization
    public void Serialize(IWriter writer)
    {
        lock (m_serializationLock)
        {
            m_bools.Serialize(writer);
            writer.Write(m_dispatchTime);

            using (ObjectAccess<ScenarioDetail> sd = ScenarioDetailLock.EnterRead())
            {
                sd.Instance.Serialize(writer);
            }

            using (ObjectAccess<ScenarioSummary> ss = ScenarioSummaryLock.EnterRead())
            {
                ss.Instance.Serialize(writer);
            }

            m_dispatcher.Serialize(writer);
            m_kpiController.Serialize(writer);

            writer.Write(m_isClient);
            if (!m_isClient)
            {
                using ObjectAccess<UndoSets> us = UndoSetsLock.EnterRead();
                m_nextUndoNbr.Serialize(writer);
                us.Instance.Serialize(writer);
                SerializeCheckpoints(writer, us.Instance);
            }

            writer.Write(LastReceivedTransmissionId);
            writer.Write(LastReceivedTransmissionTimeTicks);
        }
    }

    public void Serialize(IWriter a_writer, params string[] a_params)
    {
        if (a_params[0] == SerializeForClient)
        {
            m_isClient = true;
        }
        Serialize(a_writer);

        m_isClient = false;
    }

    public int UniqueId => UNIQUE_ID;

    void SerializeCheckpoints(IWriter writer, UndoSets a_uss)
    {
        for (int i = 0; i < a_uss.Count; ++i)
        {
            UndoSet us = a_uss[i];
            byte[] bytes = File.ReadAllBytes(SerializationPath(us.m_undoNbr));
            writer.Write(bytes);
        }
    }
    #endregion

    private IUserManager m_userManager;
    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        using (ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
        {
            sd.RestoreReferences(a_udfManager);
        }
    }
    internal void RestoreReferences(int a_serializationVersionNumber, ISystemLogger a_eRM, bool a_startup_onFirstLoadAllowResequencingOfIds_reloadToFixupIdsDontResequence, IPackageManager a_packageManager, IUserManager a_userManager)
    {
        m_errorReporter = a_eRM;
        m_packageManager = a_packageManager;
        m_userManager = a_userManager;

        m_extensionController = new ExtensionController(a_packageManager, a_eRM);
        m_extensionController.InitializeRelevantElements();

        using (ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
        {
            sd.RestoreReferences(a_serializationVersionNumber, this, a_startup_onFirstLoadAllowResequencingOfIds_reloadToFixupIdsDontResequence, a_packageManager, a_eRM, m_extensionController);

            KpiController.InitCalculatorList(a_packageManager.GetKpiCalculatorModules());
        }

        using (ScenarioSummaryLock.EnterWrite(out ScenarioSummary scenarioSummary))
        {
            scenarioSummary.RestoreReferences(a_serializationVersionNumber);
        }

        using (UndoSetsLock.EnterWrite(out UndoSets uss))
        {
            uss.RestoreReferences(this);

            if (PTSystem.Server)
            {
                if (a_serializationVersionNumber < 12500)
                {
                    //Undo sets must be cleared for version 11 versions to avoid serialization errors.
                    uss.Clear(out bool changes);
                    CreateUndoSet(uss);
                }
                else
                {
                    if (PTSystem.IsNewVersion() && !PTSystem.RunningFromRecordings)
                    {
                        //Create a new undo set so undoing of new actions won't need to load undo sets from previous versions. This helps avoid any incompatibilities.
                        CreateUndoSet(uss);
                    }
                }
            }
        }
    }
    #endregion

    private ISystemLogger m_errorReporter;
    //private ISystemLogger m_errorLogger;

    private ExtensionController m_extensionController;

    internal ExtensionController ExtensionController => m_extensionController;

    private IPackageManager m_packageManager;

    #region Dispatcher
    private Dispatcher m_dispatcher;
    internal Dispatcher Dispatcher
    {
        set { m_dispatcher = value; }
        get { return m_dispatcher; }
    }

    private bool m_isTempScenario;
    /// <summary>
    /// Flags a scenario as a temporary scenario created by a temporary <see cref="PTSystem"/>.
    /// <para></para>
    /// <remarks>Temporary scenarios do not alter files on disk when disposed</remarks>
    /// </summary>
    internal void FlagTempScenario()
    {
        m_isTempScenario = true;
    }
    /// <summary>
    /// Wait for all the transmissions in the dispatcher to play.
    /// </summary>
    internal void FlushDispatcher()
    {
        m_dispatcher.Flush(); //This will return as the last transmission is playing
    }
    #endregion

    #region Performance measurements
    private KpiController m_kpiController;

    public KpiController KpiController => m_kpiController;

    /// <summary>
    /// Add a new value to each of the KPIs based on the new schedule.
    /// </summary>
    internal void CalculateKPIs(ScenarioDetail a_sd, ScenarioDetail.SimulationType simulationType, PTTransmission transmission)
    {
        try
        {
            KpiController.CalculateKPIs(this, a_sd, simulationType, transmission, m_errorReporter);
        }
        catch (KpiController.KPIException e)
        {
            ScenarioEvents se;
            using (ScenarioEventsLock.EnterRead(out se))
            {
                SystemMessage systemMessage = new (transmission.Instigator, e.Message);
                se.FireTransmissionFailureEvent(e, transmission, systemMessage);
            }
        }
    }
    #endregion Performance measurements

    #region Construction
    /// <summary>
    /// Create a new ScenarioContainer for a blank scenario
    /// </summary>
    /// <param name="a_id"></param>
    /// <param name="a_scenarioName"></param>
    /// <param name="a_initialSettings"></param>
    /// <param name="erm"></param>
    /// <param name="a_scenarioType"></param>
    /// <param name="a_packageManager"></param>
    internal Scenario(BaseId a_id, string a_scenarioName, List<ISettingData> a_initialSettings, ISystemLogger erm, ScenarioTypes a_scenarioType, IPackageManager a_packageManager, BaseId a_adminId, IUserManager a_userManager, UserFieldDefinitionManager a_udfManager)
    {
        m_packageManager = a_packageManager;
        m_errorReporter = erm;
        m_userManager = a_userManager;
        // This constructor is used when creating a ScenarioManager, which I'm assuming means this scenario is the first scenario.
        // I think that means it needs to be a Production Scenario which is why I'm passing in true for a_isProductionScenario.
        ScenarioSummary scenarioSummary = new (a_id, a_initialSettings, a_scenarioType, true, a_adminId, a_userManager);
        scenarioSummary.Name = a_scenarioName;
        SetScenarioSummary(scenarioSummary);
        SetScenarioDetail(new ScenarioDetail(a_packageManager, erm));
        Init();

        m_scenarioDetail.RestoreReferencesForNewScenarioDetail(this, erm, m_extensionController);
        InitializeUndoSet();

        m_kpiController.InitCalculatorList(a_packageManager.GetKpiCalculatorModules());
    }

    /// <summary>
    /// Create a copy of a ScenarioContainer.
    /// </summary>
    internal Scenario(BaseId a_id, Scenario a_s, ScenarioDetail a_sd, ScenarioSummary a_ss, IPackageManager a_packageManager, IUserManager a_userManager) : this(a_id, a_s, a_sd, a_ss, a_packageManager)
    {
        m_userManager = a_userManager;
    }

    /// <summary>
    /// Create a copy of a ScenarioContainer.
    /// </summary>
    internal Scenario(BaseId a_id, Scenario a_s, ScenarioDetail a_sd, ScenarioSummary a_ss, IPackageManager a_packageManager)
    {
        m_packageManager = a_packageManager;

        SetScenarioDetail(a_sd);
        SetScenarioSummary(a_ss);

        //This can be null if the copy is being performed for a RuleSeek simulation
        if (a_s == null)
        {
            m_errorReporter = PTSystem.SystemLogger;
        }
        else
        {
            m_errorReporter = a_s.m_errorReporter;
        }

        m_scenarioSummary.Id = a_id;
        ((IScenarioRef)m_scenarioDetail).SetReferences(this, null);
        Init();

        m_scenarioDetail.RestoreReferences(Serialization.VersionNumber, this, false, a_packageManager, m_errorReporter, m_extensionController);

        m_kpiController.InitCalculatorList(a_packageManager.GetKpiCalculatorModules());
    }

    /// <summary>
    /// Used to recheck checksums in case the client finished before the server, and the server checksum was not ready
    /// </summary>
    private System.Timers.Timer m_checksumValidationTimer;

    private void Init()
    {
        m_dispatcher = new Dispatcher(EDispatcherOwner.Scenario);
        m_dispatcher.TransmissionReceived += Receive;
        SetScenarioEvents(new ScenarioEvents());
        m_kpiController = new KpiController();
        SetUndoSets(new UndoSets(this));
        SetScenarioUndoEvents(new ScenarioUndoEvents());
        StartListeningToEvents();
        m_licenseManager = new ScenarioLicenseManager();
        m_scenarioPermissionValidationManager = new ScenarioPermissionValidationManager();
        m_extensionController = new ExtensionController(m_packageManager, m_errorReporter);
        m_extensionController.InitializeRelevantElements();
    }

    private void ChecksumValidationTimerOnElapsed(object a_sender, ElapsedEventArgs a_e)
    {
        ClientValidateChecksums();
    }

    internal void InitializeUndoSet()
    {
        //If the id is null the scenario is being used for system only simulations and should not have checkpoints.
        if (Id != BaseId.NULL_ID)
        {
            using (ObjectAccess<UndoSets> uss = UndoSetsLock.EnterWrite())
            {
                CreateUndoSet(uss.Instance);
            }
        }
    }
    #endregion

    #region Disposal
    ~Scenario()
    {
        Dispose(true);
    }

    public void Dispose()
    {
        Dispose(false);
    }

    private bool m_disposed;

    private void Dispose(bool a_finalizing)
    {
        if (!m_disposed)
        {
            m_disposed = true;
            m_checksumValidationTimer?.Dispose();
            FlushDispatcher();

            //It is possible that an error occured before the undo sets were created. If so, then the lock and the set 
            //  are both null and there is nothing to dispose. Skip this.
            if (UndoSetsLock != null)
            {
                using (UndoSetsLock.EnterWrite(out UndoSets uss))
                {
                    uss.Dispose();
                }
            }
            
            m_dispatcher.TransmissionReceived -= Receive;
        }
    }
    #endregion

    #region Scenario detail
    private ScenarioDetail m_scenarioDetail;
    private AutoReaderWriterLock<ScenarioDetail> m_scenarioDetailLock;

    public AutoReaderWriterLock<ScenarioDetail> ScenarioDetailLock => m_scenarioDetailLock;

    /// <summary>
    /// Change the ScenarioDetail of this Scenario.
    /// This was created to make it possible to undo a transmission.
    /// </summary>
    /// <param name="a_sd"></param>
    internal void SetScenarioDetail(ScenarioDetail a_sd)
    {
        m_scenarioDetail = a_sd;

        if (m_scenarioDetailLock == null)
        {
            m_scenarioDetailLock = new AutoReaderWriterLock<ScenarioDetail>(m_scenarioDetail);
        }
        else
        {
            m_scenarioDetailLock.ChangeLockedObject(a_sd);
        }
    }
    #endregion

    #region ScenarioSummary
    private ScenarioSummary m_scenarioSummary;
    private AutoReaderWriterLock<ScenarioSummary> m_scenarioSummaryLock;

    public AutoReaderWriterLock<ScenarioSummary> ScenarioSummaryLock => m_scenarioSummaryLock;

    internal void SetScenarioSummary(ScenarioSummary a_ss)
    {
        if (m_scenarioSummary != null)
        {
            // This value of ScenarioSummary must not be changed.
            // It is maintained by ScenarioManager.
            // When an Undo is performed the current ScenarioSummary is replaced with a newly loaded ScenarioSummary,
            // so that may end up overwritting this value.
            a_ss.SetType(m_scenarioSummary.Type);
        }

        m_scenarioSummary = a_ss;

        if (m_scenarioSummaryLock == null)
        {
            m_scenarioSummaryLock = new AutoReaderWriterLock<ScenarioSummary>(a_ss);
        }
        else
        {
            m_scenarioSummaryLock.ChangeLockedObject(a_ss);
        }
    }

    public AutoDisposer AutoEnterScenarioSummary(out ScenarioSummary o_ss)
    {
        return m_scenarioSummaryLock.EnterRead(out o_ss);
    }
    #endregion

    #region ScenarioEvents
    private AutoReaderWriterLock<ScenarioEvents> m_scenarioEventsLock;

    public AutoReaderWriterLock<ScenarioEvents> ScenarioEventsLock => m_scenarioEventsLock;

    private void SetScenarioEvents(ScenarioEvents a_se)
    {
        m_scenarioEventsLock = new AutoReaderWriterLock<ScenarioEvents>(a_se);
    }

    public AutoDisposer AutoEnterScenarioEvents(out ScenarioEvents o_se)
    {
        return m_scenarioEventsLock.EnterRead(out o_se);
    }

    public AutoDisposer AutoTryScenarioEvents(out ScenarioEvents o_se, int a_ms)
    {
        return m_scenarioEventsLock.TryEnterRead(out o_se, a_ms);
    }
    #endregion

    #region ScenarioUndoEvents
    private AutoReaderWriterLock<ScenarioUndoEvents> m_scenarioUndoEventsLock;

    public AutoReaderWriterLock<ScenarioUndoEvents> ScenarioUndoEventsLock => m_scenarioUndoEventsLock;

    private void SetScenarioUndoEvents(ScenarioUndoEvents a_su)
    {
        m_scenarioUndoEventsLock = new AutoReaderWriterLock<ScenarioUndoEvents>(a_su);
    }
    #endregion

    #region ScenarioBaseT reception functionality.
    /// <summary>
    /// This is called possibly on a different Thread.
    /// </summary>
    /// <param name="a_t"></param>
    private void Receive(PTTransmissionBase a_t, int a_remainingQueuedTransmissions = 0)
    {
        try
        {
            ScenarioBaseT scenarioBaseT = (ScenarioBaseT)a_t;
            TimeSpan procTime;
            
            lock (m_serializationLock) //Don't allow any object changes during serialization
            {
                procTime = Receive(scenarioBaseT, a_remainingQueuedTransmissions);
            }

            if (PTSystem.Server)
            {
                ServerProcessing(scenarioBaseT, procTime);
                CalculateAndStoreChecksum(scenarioBaseT);
                //m_performingUndoOrRedo
            }
            else
            {
                //We don't really need to calculate and validate checksums on the client we would definitely desync
                //because as part of the client's process of handling undoes, the client's checksums are cleared and
                //waits for a new scenario while the server actually processes the undo and broadcasts a transmission
                //with the new scenario bytes
                if (a_t is ScenarioStartUndoT || a_t is ScenarioUndoT)
                {
                    return;
                }

                CalculateAndStoreChecksum(scenarioBaseT);
                ClientValidateChecksums();
            }
        }
        catch (PTSimCancelledException)
        {
            //Only send the undo from the client to ensure the server does not process multiple undos from the Simulation cancellation
            if (!PTSystem.Server)
            {
                GetUndoIdxByTransmissionNbrRequest nbrRequest = new GetUndoIdxByTransmissionNbrRequest();
                nbrRequest.ScenarioId = Id.Value;
                nbrRequest.TransmissionNbr = a_t.TransmissionNbr;
                GetUndoIdxByTransmissionNbrResponse undoIdxByTransmissionNbr = SystemController.ClientSession.MakePostRequest<GetUndoIdxByTransmissionNbrResponse>("UndoIdByTransmissionNbr", nbrRequest, "api/SystemService");

                ScenarioUndoT undoLastAction = new ScenarioUndoT(Id, undoIdxByTransmissionNbr.UndoIdx);
                undoLastAction.UndoIds.Add(a_t.TransmissionId);
                undoLastAction.CancellingSimulation = true;

                SystemController.ClientSession.SendClientAction(undoLastAction);
            }
        }

        catch (PTException e)
        {
            
            PTTransmission tTempt = a_t as PTTransmission;
            ScenarioExceptionInfo sei = new ();
            sei.Create(this);
            m_errorReporter.LogException(e, tTempt, sei, ELogClassification.PtSystem, e.LogToSentry);
        }
        catch (Exception e)
        {
            
            PTTransmission tTempt = a_t as PTTransmission;
            ScenarioExceptionInfo sei = new ();
            sei.Create(this);
            m_errorReporter.LogException(e, tTempt, sei, ELogClassification.Fatal, true);
        }
        finally
        {
            // Put this logging here instead of at the top so if we ever run into issues
            // with logging the transmissions, we don't just drop the transmission and not send it to the scenario
            ScenarioBaseT scenarioBaseT = (ScenarioBaseT)a_t;
            if (ShouldTransmissionBeLoggedForDesyncDiagnostics(scenarioBaseT))
            {
                //Records the received transmission info before it is processed.
                RecordReceivedTransmissionForChecksum(scenarioBaseT);
            }
        }
    }

    private ScenarioPermissionValidationManager m_scenarioPermissionValidationManager;
    private ScenarioLicenseManager m_licenseManager;
    internal ScenarioLicenseManager LicenseManager => m_licenseManager;
    public bool DataReadonly => m_licenseManager.DataReadOnly;

    /// <summary>
    /// Process transmission.
    /// </summary>
    /// <param name="a_t"></param>
    /// <returns></returns>
    internal TimeSpan Receive(ScenarioBaseT a_t, int a_remainingQueuedTransmissions = 0)
    {
        GetQueuedTransmissionDescriptions(a_t);

        ScenarioEvents se;
        DateTime start = DateTime.UtcNow;
        TimeSpan span;
        Exception exception = null;
        bool simulationCancelled = false;
        ScenarioDataChanges dataChanges = new();
        try
        {
            LastReceivedTransmissionId = a_t.TransmissionId;
            LastReceivedTransmissionTimeTicks = a_t.TimeStamp.Ticks;
         
            if (m_licenseManager.DataReadOnly)
            {
                //Scenario is read-only, verify if this transmission is allowed
                bool allowed = m_licenseManager.VerifyReadonlyTransmission(a_t);
                if (!allowed)
                {
                    throw new PTValidationException("Scenario is is readonly".Localize());
                }
            }
            else
            {
                //Check if this transmission would result in read-only
                bool allowed = m_licenseManager.VerifyTransmission(a_t);
                if (!allowed)
                {
                    throw new PTValidationException("Action was prevented due to licensing".Localize());
                }
            }

            if (!m_scenarioPermissionValidationManager.VerifyTransmission(a_t.Instigator, this))
            {
                throw new PTValidationException("Action was prevented. User does not own edit privileges for this scenario.".Localize());
            }

            //Handle undo checkpoints
            if (!a_t.ReplayForUndoRedo && a_t.PerformUndoCheckpoint(Id))
            {
                using (ObjectAccess<UndoSets> uss = UndoSetsLock.EnterWrite())
                {
                    CreateUndoSet(uss.Instance);
                }
            }
            
            if (a_t is ScenarioReplaceT || (!PTSystem.Server && a_t.ClientWillWaitForScenarioResult))
            {
                //This is passed to scenario so the sequence numbers can stay in sync. It should not be processed by the client.

                // TODO lite client:
                // Should we worry about this? I don't think we'll dispatch this transmission to the Scenario at all
                // given the current flow we have in mind for Undo and Reload
            }
            else if (a_t is ScenarioClearUndoSetsT && !a_t.ReplayForUndoRedo)
            {
                bool notifyChanges;
                using (ObjectAccess<UndoSets> uss = UndoSetsLock.EnterWrite())
                {
                    uss.Instance.Clear(out notifyChanges);
                    CreateUndoSet(uss.Instance);
                }

                if (notifyChanges)
                {
                    FireUndoSetChangedEvent(a_t);
                }
            }
            else if (a_t is ScenarioChangeT scenarioChangeT)
            {
                Change(scenarioChangeT, dataChanges);
            }
            else if (a_t is ScenarioUndoT undoT)
            {
                if (PTSystem.Server)
                {
                    if (!a_t.Recording)
                    {
                        ScenarioStartUndoT startUndo = new ScenarioStartUndoT(undoT.ScenarioId);
                        startUndo.Count = undoT.UndoIds.Count;
                        startUndo.IsCancellingSimulation = undoT.CancellingSimulation;
                        startUndo.IsUndo = !undoT.Redo;
                        SystemController.ClientSession.SendClientAction(startUndo);
                    }

                    Undo((ScenarioUndoT)a_t);
                }
                else
                {
                    //if an undo is triggered from a recording fire Cancellation Begin from here to notify clients and update UI according
                    if (a_t.Recording && undoT.CancellingSimulation)
                    {
                        using (ScenarioEventsLock.EnterRead(out se))
                        {
                            //This is typically fired when the Cancel simulation exception is caught but since that exception isn't thrown for recordings 
                            //we trigger it here
                            se.FireSimulationCancelBeginEvent();
                        }

                    }
                    HandleClientUndo(undoT);
                }
            }
            else if (a_t is ScenarioStartUndoT startUndoT)
            {
                if (!PTSystem.Server)
                {
                    HandleClientUndo(startUndoT);
                }
            }
            else if (a_t is ScenarioNewT)
            {
                NewScenario();
            }
            else if (a_t is PublishStatusMessageT publishStatusT)
            {
                // TODO: listeners of this event need to switch to polling
                using (ScenarioEventsLock.EnterRead(out se))
                {
                    se.FirePublishStatusEvent(publishStatusT);
                }
            }
            else if (a_t is ScenarioKpiOptionsUpdateT)
            {
                KpiController.UpdateOptions((ScenarioKpiOptionsUpdateT)a_t);
            }
            else if (a_t is ScenarioKpiSnapshotT)
            {
                using (ScenarioDetailLock.EnterRead(out ScenarioDetail sd))
                {
                    KpiController.CalculateKPIsAtUserRequest(this, sd, (ScenarioKpiSnapshotT)a_t, m_errorReporter);
                }
            }
            else if (a_t is KpiSnapshotOfLiveScenarioT)
            {
                using (ScenarioDetailLock.EnterRead(out ScenarioDetail sd))
                {
                    KpiController.CalculateKPIsForSchedulingAgent(this, sd, (KpiSnapshotOfLiveScenarioT)a_t, m_errorReporter);
                }
            }
            else if (a_t is ScenarioKpiVisibilityT kpiVisibilityT)
            {
                KpiController.UpdateKpiVisibility(kpiVisibilityT);
                dataChanges.KPIChanges = true;
                using (ScenarioEventsLock.EnterRead(out se))
                {
                    se.FireScenarioDataChangedEvent(dataChanges);
                }
            }
            else if (a_t is ScenarioDetailClearPastShortTermT)
            {
                using (ScenarioSummaryLock.EnterWrite(out ScenarioSummary ss))
                {
                    string prefix = "ShortTerm".Localize();
                    if (!ss.Name.Contains(prefix))
                    {
                        ss.Name = prefix + " : " + ss.Name;
                    }
                }

                using (ObjectAccess<ScenarioDetail> sd = ScenarioDetailLock.EnterWrite())
                {
                    sd.Instance.Receive(a_t, dataChanges);
                }
            }
            else if (a_t is ScenarioIsolateT)
            {
                Isolate((ScenarioIsolateT)a_t);
            }
            else if (a_t is ImportT importT)
            {
                using (ScenarioSummaryLock.EnterWrite(out ScenarioSummary ss))
                {
                    ss.LastRefreshDate = importT.TimeStamp.ToDateTime();
                }

                using (ObjectAccess<ScenarioDetail> sd = ScenarioDetailLock.EnterWrite())
                {
                    sd.Instance.Receive(a_t, dataChanges);
                }
            }
            else if (a_t is ScenarioClockAdvanceT)
            {
                using (ObjectAccess<ScenarioDetail> sd = ScenarioDetailLock.EnterWrite())
                {
                    sd.Instance.Receive(a_t, dataChanges);
                }
            }
            else if (a_t is ScenarioSettingDataT settingsT)
            {
                using (ScenarioSummaryLock.EnterWrite(out ScenarioSummary ss))
                {
                    ss.ScenarioSettings.SaveSetting(settingsT.SettingData, dataChanges, true);
                }

                using (ScenarioDetailLock.EnterRead(out ScenarioDetail sd))
                {
                    ExtensionController.ScenarioSettingSaved(sd, settingsT.SettingData.Key, settingsT.SettingData, settingsT);
                }

                CheckForRequiredAdditionalSimulation();
            }
            else if (a_t is ScenarioDetailHoldSettingsT holdT)
            {
                //TODO: Use ScenarioSettingDataT instead
                using (ScenarioSummaryLock.EnterWrite(out ScenarioSummary ss))
                {
                    ss.ScenarioSettings.SaveSetting(new SettingData(holdT.HoldSettings), dataChanges, true);
                }
            }
            //To prevent UserFieldDefinitionT from being received by ScenarioDetail, override the handling of the transmission here.
            else if (a_t is UserFieldDefinitionT) { }
            else if (a_t is ScenarioDetailMoveT moveT)
            {
                if (m_sdLastSimDataCopy == null)
                {
                    //This is the first action performed before loading the scenario
                    CopySDToForUndoingTheNextAction();
                }
                //Special handling for when a MoveT needs to 'undo' and retry again with slightly different parameters
                using (ObjectAccess<ScenarioDetail> oa = ScenarioDetailLock.EnterWrite())
                {
                    ScenarioDetail sd = oa.Instance;
                    UndoReceive undoReReceive = new UndoReceiveMove();

                    sd.Receive(moveT, dataChanges, undoReReceive);
                    while (undoReReceive.ReReceiveTransmission)
                    {
                        // [USAGE_CODE] Receive: Undo/re-receive code.
                        // Check undo for additional details on restoring scenarios.
                        //if (undoReReceive.UndoReceivedTransmission)
                        // Undo the changes to the scenario file.
                        ScenarioDetail unchangedSD;
                        lock (sdLastSimDataCopyLock)
                        {
                            BinaryMemoryReader reader = new (m_sdLastSimDataCopy);
                            unchangedSD = new (reader);
                            unchangedSD.RestoreReferences(reader.VersionNumber, this, false, m_packageManager, m_errorReporter, m_extensionController);
                            SetScenarioDetail(unchangedSD);
                        }

                        // [USAGE_CODE] Receive: Re-receive the transmission with additional data from the previous simulation that will allow the next simulation to schedule blocks better.
                        UndoReceiveMove moveResult = (UndoReceiveMove)undoReReceive;
                        moveResult.IncrementReapplies();
                        long moveTicks = moveResult.HasReReceiveMoveTicks ? moveResult.ReReceiveMoveTicks : moveT.StartTicks;

                        if (moveT is ScenarioDetailAlternatePathMoveT apMoveT)
                        {
                            moveT = new ScenarioDetailAlternatePathMoveT(apMoveT, apMoveT.ActivityAltPaths);
                        }
                        else
                        {
                            moveT = new ScenarioDetailMoveT(moveT, moveT.ToResourceKey, moveTicks, moveT.ExpediteSuccessors, moveT.LockMove, moveT.AnchorMove, moveT.Exact, moveT.PrependSetupToMoveBlocksRightNeighbors);
                        }

                        //Resend the transmissions with original sender data so UI knows original source
                        moveT.Instigator = moveT.Instigator;
                        moveT.SetTimeStamp(moveT.TimeStamp);

                        unchangedSD.Receive(moveT, dataChanges, undoReReceive);
                    }
                }
            }
            else
            { 
                //Default send to SD
                using (ObjectAccess<ScenarioDetail> oa = ScenarioDetailLock.EnterWrite())
                {
                    ScenarioDetail sd = oa.Instance;
                    sd.Receive(a_t, dataChanges);
                }
            }

            //After all processing is done, send a scenario result if needed
            if (PTSystem.Server && a_t.ClientWillWaitForScenarioResult)
            {
                if (a_t is ScenarioIdBaseT)
                {
                    byte[] bs;
                    using (BinaryMemoryWriter writer = new (ECompressionType.Fast))
                    {
                        Serialize(writer);
                        bs = writer.GetBuffer();
                    }

                    ScenarioReplaceT replaceT = new (Id, bs);
                    SystemController.ClientSession.SendClientAction(replaceT); //preserve the sender
                }
                else if (a_t is ERPTransmission)
                {
                    //TODO
                }
            }
        }
        catch (PTSimCancelledException)
        {
            using (ScenarioEventsLock.EnterRead(out se))
            {
                se.FireSimulationCancelBeginEvent();
            }

            throw new PTSimCancelledException();
        }
        catch (PTValidationException validationException)
        {
            exception = validationException;
            throw;
        }
        catch (PTHandleableException handleableException)
        {
            exception = handleableException;
            throw;
        }
        catch (PTException ptException)
        {
            exception = ptException;
            throw;
        }
        catch (Exception err)
        {
            exception = err;
            throw new PTException("2894", err);
        }
        finally
        {
            //Transmission has been processed in Scenario and was not sent to SD
            DateTime processingEnd = DateTime.UtcNow;
            TimeSpan processingTime = processingEnd - start;
            if (!a_t.ReplayForUndoRedo)
            {
                using (ScenarioEventsLock.EnterRead(out se))
                {
                    se.FireTransmissionProcessedEvent(a_t, processingTime, exception);
                }
            }

            DateTime end = DateTime.UtcNow;
            span = end - start;
            bool fireUndoSetChangedEvent = false;

            if (!a_t.ReplayForUndoRedo && Id != BaseId.NULL_ID) // Id = BaseId.NULL_ID means that this is a background scenario (co-pilot)  and has no undo sets. See Scenario.InitializeUndoSet()
            {
                if (WasLastTransmissionUndoT)
                {
                    if (a_t.Instigator != BaseId.ERP_ID && a_t is not (ScenarioUndoT or ScenarioStartUndoT))
                    {
                        PTTransmission cesT = a_t;
                        CleanUndoTransmissions(cesT.Instigator, out fireUndoSetChangedEvent);
                    }
                }

                if (a_t is not (ScenarioUndoT or ScenarioClearUndoSetsT or ScenarioStartUndoT))
                {
                    using (UndoSetsLock.EnterWrite(out UndoSets uss))
                    {
                        if (uss.Count == 0) //TODO: This was added for InsertJobs only
                        {
                            CreateUndoSet(uss);
                        }

                        uss.AddTransmissionToCurrentSet(a_t, span);

                        //TODO: I don't know why this is here, but it has always been
                        if (uss.Count == 0)
                        {
                            CreateUndoSet(uss);
                        }
                    }

                    fireUndoSetChangedEvent = true;
                    // This can also get set in the CleanUndoTransmissions call above
                }

                if (fireUndoSetChangedEvent)
                {
                    FireUndoSetChangedEvent(a_t);
                }

                WasLastTransmissionUndoT = a_t is (ScenarioUndoT or ScenarioStartUndoT);
            }

            VerifyDataLicensing();
            
            CopySDToForUndoingTheNextAction();


            if (dataChanges.HasChanges && !a_t.ReplayForUndoRedo)
            {
                using (ScenarioEventsLock.EnterRead(out ScenarioEvents seChanges))
                {
                    seChanges.FireScenarioDataChangedEvent(dataChanges);
                }
            }

            if (dataChanges.AuditEntries.Any())
            {
                SystemController.Sys.SystemLoggerInstance.LogAudit(Id, a_t.Instigator, dataChanges);
            }
        }

        return span;
    }

    private void HandleClientUndo(ScenarioIdBaseT a_transmissionBaseT)
    {
        UndoChecksumProcessing(a_transmissionBaseT);

        m_dispatcher.CancelTransmissionsDispatching();

        using (SystemController.Sys.ScenariosLock.TryEnterRead(out ScenarioManager sm, AutoExiter.THREAD_TRY_WAIT_MS))
        {
            sm.CacheScenarioDispatcher(Id, m_dispatcher);
        }

        bool undoRedo = false;
        string description = String.Empty;
        bool cancellingSimulation = false;
        
        if (a_transmissionBaseT is ScenarioStartUndoT undoStartT)
        {
            undoRedo = undoStartT.IsUndo;
            description = undoStartT.Description;
            cancellingSimulation = undoStartT.IsCancellingSimulation;
        }
        else if (a_transmissionBaseT is ScenarioUndoT undoT && undoT.UndoIds.Count > 0 )
        {
            undoRedo = !undoT.Redo;
            cancellingSimulation = undoT.CancellingSimulation;
            description = undoT.Description;
        }

        if (cancellingSimulation)
        {
            return;
        }

        using (ScenarioUndoEventsLock.EnterRead(out ScenarioUndoEvents sue))
        {
            sue.FireUndoBeginEvent(Id, undoRedo, description);
        }
    }

    private void GetQueuedTransmissionDescriptions(ScenarioBaseT a_t)
    {
        if(!PTSystem.Server)
        {
            //Get the list of QueuedTransmissionData objects from the dispatcher. We need ScenarioDetail in order to build more descriptive action descriptions for moves and expedites. 
            List<QueuedTransmissionData> queueDescription = null;
            using (ScenarioDetailLock.EnterRead(out ScenarioDetail sd))
            {
                try
                {
                    queueDescription = m_dispatcher.GetQueueDescriptions(sd);
                }
                catch (Exception e)
                {
                    ScenarioExceptionInfo sei = new();
                    sei.Create(this);
                    m_errorReporter.LogException(new PTHandleableException("2957", new object[] { a_t.TransmissionId }), sei, false);
                }
            }

            if (queueDescription != null)
            {
                using (ScenarioEventsLock.EnterRead(out ScenarioEvents se))
                {
                    //Fire the transmission received event which is currently listened to on the QueuedTransmissionsNotificationElement.
                    se.FireTransmissionReceivedEvent(a_t, queueDescription);
                }
            }
        }
    }

    private void VerifyDataLicensing()
    {
        bool currentReadonly = m_licenseManager.DataReadOnly;
        using (ObjectAccess<ScenarioDetail> sd = ScenarioDetailLock.EnterRead())
        {
            m_licenseManager.ValidateData(this, sd.Instance);
        }

        if (currentReadonly != m_licenseManager.DataReadOnly)
        {
            //Readonly state changed
            using (ScenarioEventsLock.EnterRead(out ScenarioEvents se))
            {
                se.FireScenarioReadonlyChangeEvent(m_licenseManager.DataReadOnly);
            }
        }
    }

    private object sdLastSimDataCopyLock = new ();
    private byte[] m_sdLastSimDataCopy;

    /// <summary>
    /// A copy of ScenarioDetail made after the last action performed or after
    /// deserialization. It's purpose is to restore SD if  a simulation needs to be undone
    /// and replayed.
    /// </summary>
    private void CopySDToForUndoingTheNextAction()
    {
        using (ObjectAccess<ScenarioDetail> oa = ScenarioDetailLock.EnterRead())
        {
            ScenarioDetail sd = oa.Instance;

            lock (sdLastSimDataCopyLock)
            {
                using (BinaryMemoryWriter sdWriter = new BinaryMemoryWriter(ECompressionType.Fast))
                {
                    sd.Serialize(sdWriter);
                    m_sdLastSimDataCopy = sdWriter.GetBuffer();
                }
            }
        }
    }

    /// <summary>
    /// Apply scenario isolation settings
    /// </summary>
    /// <param name="a_scenarioIsolateT"></param>
    private void Isolate(ScenarioIsolateT a_scenarioIsolateT)
    {
        if (a_scenarioIsolateT.IsolateImportSet)
        {
            m_bools[c_isolatedFromImportIdx] = a_scenarioIsolateT.IsolateImport;
        }

        if (a_scenarioIsolateT.IsolateClockAdvanceSet)
        {
            m_bools[c_isolatedFromClockAdvanceIdx] = a_scenarioIsolateT.IsolateClockAdvance;
        }

        FireIsolationChangedEvent(a_scenarioIsolateT);
    }

    #region Checksums
    private bool m_calculateChecksum; // this is set to true after each simulation. If true checksum is broadcasted if ChecksumFrequency is ScheduleAdjustment.

    private const ulong c_checksumFrequencyCount = 5; // every this many transmissions a checksum is broadcasted if ChecksumFrequency is Regular.

    //The client checksums. We store a list because the client could be ahead of the server. So we keep the full list
    //Also the collection of checksums that the server stores.
    //It's possible that if the server is significantly faster than the client, the server may overwrite the checksums the client is on
    private readonly CircularQueueDictionary<Guid, ChecksumValues> m_checksums = new (50);
    private readonly object m_checksumLock = new ();

    private CircularQueue<(ulong TransmissionNbr, int TransmissionUniqueId, BaseId InstigatorId, DateTimeOffset TimeStamp)> m_receivedTransmissionInfos;
    private readonly object m_receivedTransmissionInfosLock = new();

    /// <summary>
    /// determines whether checksum should be calculated based on the scenario settings
    /// and type and nbr of last processed transmission.
    /// </summary>
    /// <param name="a_t">last processed transmission</param>
    /// <returns></returns>
    private bool ShouldChecksumBeCalculated(ScenarioBaseT a_t)
    {
        if (a_t is ScenarioUndoCheckpointT || a_t is ScenarioClearUndoSetsT || a_t.ReplayForUndoRedo)
        {
            return false;
        }

        if (a_t.Recording || (ChecksumFrequency == ChecksumFrequencyType.Regular && a_t.TransmissionNbr % c_checksumFrequencyCount == 0) 
                          || (ChecksumFrequency == ChecksumFrequencyType.ScheduleAdjustment && m_calculateChecksum) 
                          || a_t is ScenarioUndoT
                          || a_t is ScenarioStartUndoT)
        {
            m_calculateChecksum = false;
            return true;
        }

        return false;
    }

    private static bool ShouldTransmissionBeLoggedForDesyncDiagnostics(ScenarioBaseT a_t)
    {
        if (ChecksumFrequency != ChecksumFrequencyType.ScheduleAdjustment)
        {
            return false;
        }

        if (a_t is ScenarioUndoCheckpointT 
            || a_t is ScenarioClearUndoSetsT 
            || a_t.ReplayForUndoRedo)
        {
            return false;
        }

        return true;
    }

    private void RecordReceivedTransmissionForChecksum(ScenarioBaseT a_receivedTransmission)
    {
        lock (m_receivedTransmissionInfosLock)
        {
            if (m_receivedTransmissionInfos == null)
            {
                m_receivedTransmissionInfos = new (1000);
            }

            (ulong TransmissionNbr, int TransmissionUniqueId, BaseId InstigatorId, DateTimeOffset TimeStamp) transmissionInfo = new ();
            transmissionInfo.InstigatorId = a_receivedTransmission.Instigator;
            transmissionInfo.TransmissionNbr = a_receivedTransmission.TransmissionNbr;
            transmissionInfo.TransmissionUniqueId = a_receivedTransmission.UniqueId;
            transmissionInfo.TimeStamp = a_receivedTransmission.TimeStamp;

            m_receivedTransmissionInfos.Enqueue(transmissionInfo);
        }
    }

    /// <summary>
    /// Calculate and store checksums so when a checksum transmission is received we can validate it.
    /// We need to store them in case the server is slower than client and the checkusm transmission is deleayed.
    /// Call this function on the client.
    /// </summary>
    /// <param name="a_lastProcessedTransmission"></param>
    private void CalculateAndStoreChecksum(ScenarioBaseT a_lastProcessedTransmission)
    {
        lock (m_checksumLock)
        {
            if (!ShouldChecksumBeCalculated(a_lastProcessedTransmission))
            {
                return;
            }

            ChecksumValues checksumValues;
            using (ObjectAccess<ScenarioDetail> oa = ScenarioDetailLock.EnterRead())
            {
                checksumValues = oa.Instance.CalculateChecksums(a_lastProcessedTransmission.TransmissionId);
            }

            if (m_checksums.Contains(checksumValues.TransmissionId))
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(this);
                m_errorReporter.LogException(new PTHandleableException("2957", new object[] { a_lastProcessedTransmission.TransmissionId }), sei, false);
            }

            if (ChecksumFrequency == ChecksumFrequencyType.ScheduleAdjustment)
            {
                
                List<(ulong TransmissionNbr, int TransmissionUniqueId, BaseId InstigatorId, DateTimeOffset TimeStamp)> processedTransmissionInfos = new();
                using (UndoSetsLock.EnterRead(out UndoSets undoSets))
                {
                    // Just arbitrary choice of UndoSets to iterate through
                    for (int i = 0; i < undoSets.Count; i++)
                    {
                        UndoSet undoSet = undoSets[i];
                        for (int j = 0; j < undoSet.Count; j++)
                        {
                            TransmissionJar transmissionJar = undoSet[j];
                            (ulong TransmissionNbr, int TransmissionUniqueId, BaseId InstigatorId, DateTimeOffset TimeStamp) transmissionInfo = (transmissionJar.TransmissionInfo.TransmissionNbr, transmissionJar.TransmissionInfo.TransmissionUniqueId, transmissionJar.TransmissionInfo.Instigator, transmissionJar.TransmissionInfo.TimeStamp);
                            processedTransmissionInfos.Add(transmissionInfo);
                        }
                    }
                }

                ChecksumValues.ChecksumDiagnosticsValues checksumDiagnostics = new ChecksumValues.ChecksumDiagnosticsValues(SystemController.Sys.PeekNextTransmissionRecordingRecordNumber(), TimeZoneAdjuster.CurrentTimeZoneInfo.Id, processedTransmissionInfos);
                checksumValues.SetDiagnostics(checksumDiagnostics);

                //Add the received transmission infos to the checksum values
                lock (m_receivedTransmissionInfosLock)
                {
                    checksumValues.ChecksumDiagnostics.RecentTransmissionsReceived.Clear();

                    (ulong TransmissionNbr, int TransmissionUniqueId, BaseId InstigatorId, DateTimeOffset TimeStamp)[] receivedTransmissionInfos = m_receivedTransmissionInfos.GetQueueCopy();
                    foreach ((ulong TransmissionNbr, int TransmissionUniqueId, BaseId InstigatorId, DateTimeOffset TimeStamp) receivedTransmissionInfo in receivedTransmissionInfos)
                    {
                        checksumValues.ChecksumDiagnostics.RecentTransmissionsReceived.Add(receivedTransmissionInfo);
                    }
                }
            }

            m_checksums.Enqueue(checksumValues.TransmissionId, checksumValues);
        }
    }

    /// <summary>
    /// Validates two checksum values and throws exception if they don't match.
    /// </summary>
    /// <param name="a_checksumT"></param>
    internal void ClientValidateChecksums()
    {
        try
        {
            m_checksumValidationTimer?.Stop();
            lock (m_checksumLock)
            {
                if (m_checksums.Count == 0)
                {
                    //No checksums calculated yet
                    return;
                }

                ChecksumValues clientChecksum = m_checksums.Peek();

                //Get the server checksum

                ByteResponse serverChecksumBytes = null;
                try
                {
                    //If not found, we are either ahead or way behind the server.
                    serverChecksumBytes = SystemController.ClientSession.MakeGetRequest<ByteResponse>("GetScenarioChecksum", null, new GetParam { Name = "a_scenarioId", Value = Id.ToString() }, new GetParam { Name = "a_transmissionId", Value = clientChecksum.TransmissionId.ToString() });
                }
                catch (ApiException e)
                {
                    if (e.StatusCode == (int)System.Net.HttpStatusCode.NotFound)
                    {
                        clientChecksum.ValidationCount++;
                        if (clientChecksum.ValidationCount > 40)
                        {
                            // The client has failed to get this checksum from the server an unreasonable number of times.
                            // It's likely that the checksum will never be enqueued serverside, being somehow lost -
                            // log this and remove it from the queue of checksums to validate.
                            m_checksums.Dequeue();
                            UserErrorT errorT = new(SystemController.CurrentUserId, new PTDesyncException("4478", 
                                new object[] { clientChecksum.TransmissionId }), string.Empty);
                            SystemController.ClientSession.SendClientAction(errorT);

                        }
                        else if (clientChecksum.ValidationCount % 10 == 0)
                        {
                            // Rotate the queue, so other checksums are validated first
                            ChecksumValues dequeuedValue = m_checksums.Dequeue();
                            m_checksums.Enqueue(dequeuedValue.TransmissionId, dequeuedValue);
                        }
                        StartChecksumTimer();
                    }

                    //We can't validate, we didn't get the server checksum
                    return;
                }


                ChecksumValues serverChecksum;
                using (BinaryMemoryReader reader = new (serverChecksumBytes.Content))
                {
                    serverChecksum = new ChecksumValues(reader);
                }

                if (!clientChecksum.Equals(serverChecksum))
                {
                    //If the checksum diagnostics setting is enabled
                    if (ChecksumFrequency == ChecksumFrequencyType.ScheduleAdjustment)
                    {
                        // Send the client log and checksum data are sent to the server to be included with the ChecksumDiagnostics logs
                        // We cannot mix byte[] with other types when making a post request so the information had to be combined into a single byte[].
                        // It'd also be possible to create a parallel class for the ChecksumValues to use in the request, but 
                        // it'd make modifying the Checksums really annoying in the future.

                        byte[] concatBytes;
                        using (BinaryMemoryWriter writer = new BinaryMemoryWriter())
                        {
                            clientChecksum.Serialize(writer);
                            new StringArrayList(GetRelevantLogs()).Serialize(writer);
                            concatBytes = writer.GetBuffer();
                        }

                        //TODO: Add some handling around this post request in case it times out or something else bad happens
                        SystemController.ClientSession.MakePostRequest<BoolResponse>("HandleChecksumDiscrepancy", concatBytes, "api/SystemService/");
                    }
                    
                    string description = serverChecksum.GetComparisonDescription(clientChecksum, Id.Value);
                    using (ScenarioEventsLock.EnterRead(out ScenarioEvents se))
                    {
                        se.FireDesynchronizedScenarioEvent(clientChecksum.TransmissionId, description, Id);
                    }

                }

                m_checksums.Dequeue();

                if (m_checksums.Count != 0)
                {
                    StartChecksumTimer();
                }
            }
        }
        catch (Exception e)
        {
            //TODO: log
        }
    }

    private IEnumerable<string> GetRelevantLogs()
    {
        List<string> logs = new ();
        logs.Add("PTSystem Logs:" + m_errorReporter.GetLogContents(ELogClassification.PtSystem));
        logs.Add("Fatal Logs:" + m_errorReporter.GetLogContents(ELogClassification.Fatal));
        logs.Add("PTUser Logs:" + m_errorReporter.GetLogContents(ELogClassification.PtUser));
        logs.Add("Misc Logs:" + m_errorReporter.GetLogContents(ELogClassification.Misc));
        return logs; 
    }

    /// <summary>
    /// Start the checksum timer to recheck a checksum from the server.
    /// Initializes the timer if needed.
    /// </summary>
    internal void StartChecksumTimer()
    {
        //Initialize and start a timer to recheck the checksum
        if (m_checksumValidationTimer == null)
        {
            double milliseconds = TimeSpan.FromSeconds(10).TotalMilliseconds;
            m_checksumValidationTimer = new System.Timers.Timer(milliseconds);
            m_checksumValidationTimer.Elapsed += ChecksumValidationTimerOnElapsed;
            m_checksumValidationTimer.AutoReset = false;
        }

        m_checksumValidationTimer.Start();
    }

    public ChecksumValues GetChecksumById(Guid a_transmissionId)
    {
        lock (m_checksumLock)
        {
            try
            {
                if (m_checksums.Peek(a_transmissionId, out ChecksumValues foundChecksum))
                {
                    return foundChecksum;
                }
            }
            catch (DebugException de)
            {
                // This is expected when the client is running ahead of the server (a common occurence).
                // Client will handle the returned null.
            }
        }

        return null;
    }

    // call this after undo to remove any old values.
    private void ClearAllChecksumValues()
    {
        lock (m_checksumLock)
        {
            m_checksums.Clear();
        }
    }

    /// <summary>
    /// call this after undo transmission has been processed.
    /// </summary>
    /// <param name="a_t"></param>
    private void UndoChecksumProcessing(ScenarioBaseT a_t)
    {
        if (!PTSystem.Server)
        {
            ClearAllChecksumValues(); // clean up old undo ChecksumValues on client so new values are not compared with old ones.
        }
    }
    #endregion

    /// <summary>
    /// Processes the transmission by calling Receive on a seperate thread
    /// </summary>
    /// <param name="a_t"></param>
    internal void Dispatch(PTTransmission a_t)
    {
        if (a_t is ScenarioBaseT or ERPTransmission)
        {
            m_dispatcher.Receive(a_t);
        }
        else
        {
            throw new TransmissionValidationException(a_t, "2660", new object[] { a_t.GetType().FullName });
        }
    }

    internal void DispatchCTP(Transmissions.CTP.CtpT a_t, Scenario a_sourceScenario, IScenarioDataChanges a_dataChanges)
    {
        using (ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
        {
            ScenarioEvents se;

            //Get the list of QueuedTransmissionData objects from the dispatcher. We need ScenarioDetail in order to build more descriptive action descriptions for moves and expedites. 
            if (!PTSystem.Server)
            {
                List<QueuedTransmissionData> queueDescriptions = new ();
                using (ScenarioEventsLock.EnterRead(out se))
                {
                    queueDescriptions = m_dispatcher.GetQueueDescriptions(sd);
                }

                se.FireTransmissionReceivedEvent(a_t, queueDescriptions);
            }
            
            DateTime start = DateTime.UtcNow;             
            sd.ReceiveCtp(a_t, a_sourceScenario, a_dataChanges);
            DateTime end = DateTime.UtcNow;

            using (ScenarioEventsLock.EnterRead(out se))
            {
                TimeSpan ts = end - start;
                se.FireTransmissionProcessedEvent(a_t, ts, null);
            }
        }
    }
    #endregion

    #region PTTransmissionBase creation, validation, and reception functionality
    private void Change(ScenarioChangeT a_t, IScenarioDataChanges a_dataChanges)
    {
        using (ScenarioSummaryLock.EnterWrite(out ScenarioSummary summary))
        {
            if (a_t.ScenarioName != summary.Name)
            {
                summary.Name = a_t.ScenarioName;
            }

            //TODO: Add some validation here to make sure no more than 1 Production scenario exists
            // There's code that blocks the user from changing the scenario into a Production scenario
            // if one already exists.
            foreach (SettingData settingData in a_t.ScenarioSettings)
            {
                summary.ScenarioSettings.SaveSetting(settingData, a_dataChanges, true);
            }

            using (ScenarioEventsLock.EnterRead(out ScenarioEvents events))
            {
                events.FireChangeEvent(summary, a_t, this);
            }
        }
    }
    #endregion

    #region Undo/Redo
    private AutoReaderWriterLock<UndoSets> m_undoSetsLock;

    public AutoReaderWriterLock<UndoSets> UndoSetsLock => m_undoSetsLock;

    private void SetUndoSets(UndoSets a_undoSets)
    {
        m_undoSetsLock = new AutoReaderWriterLock<UndoSets>(a_undoSets);
    }

    public class UndoValidationException : PTValidationException
    {
        public UndoValidationException(string a_s)
            : base(a_s, null, true) { }
    }

    #region configurable settings.
    /// <summary>
    /// The number of seconds worth of server CPU processing time that separates undo sets.
    /// </summary>
    private static int s_undoThreshold = 5;

    internal static int UndoThreshold
    {
        get => s_undoThreshold;

        set => s_undoThreshold = value;
    }

    /// <summary>
    /// The maximum memory to use for undo sets
    /// </summary>
    private static decimal s_maxUndoMemoryUsage = 40;

    internal static decimal MaxUndoMemoryUsage
    {
        get => s_maxUndoMemoryUsage;

        set => s_maxUndoMemoryUsage = value;
    }

    private static ChecksumFrequencyType m_checksumFrequency;

    internal static ChecksumFrequencyType ChecksumFrequency
    {
        get => m_checksumFrequency;
        set => m_checksumFrequency = value;
    }

    private BoolVector32 m_bools;

    private const short c_isolatedFromImportIdx = 0; //Obsolete, replaced with ScenarioPlanningSettings in V12
    private const short c_isolatedFromClockAdvanceIdx = 1; //Obsolete, replaced with ScenarioPlanningSettings in V12
    private const short c_wasLastTransmissionUndoTIdx = 2;
    #endregion

    /// <summary>
    /// Used to assign unique ids to the undo sets.
    /// </summary>
    private Id m_nextUndoNbr = new (1);

    /// <summary>
    /// The amount of time it has taken to dispatch all the transmissions in this undo set.
    /// </summary>
    private TimeSpan m_dispatchTime;

    internal bool WasLastTransmissionUndoT
    {
        get => m_bools[c_wasLastTransmissionUndoTIdx];
        private set => m_bools[c_wasLastTransmissionUndoTIdx] = value;
    }

    /// <summary>
    /// Used to save the scenario. The detail and summary are put into it and it is serialized.
    /// </summary>
    private class UndoCheckpoint : IPTSerializable
    {
        public const int UNIQUE_ID = 414;

        #region IPTSerializable Members
        public UndoCheckpoint(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                m_sd = new ScenarioDetail(reader);
                m_ss = new ScenarioSummary(reader);
            }
        }

        public void Serialize(IWriter writer)
        {
            #if DEBUG
            writer.DuplicateErrorCheck(this);
            #endif
            m_sd.Serialize(writer);
            m_ss.Serialize(writer);
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public UndoCheckpoint() { }

        public ScenarioDetail m_sd;
        public ScenarioSummary m_ss;
    }

    public class TransmissionJar : IPTSerializable
    {
        #region IPTSerializable Members
        public TransmissionJar(IReader a_reader, IClassFactory a_classFactory)
        {
            if (a_reader.VersionNumber >= 657)
            {
                a_reader.Read(out m_play);
                a_reader.Read(out m_processingTime);
                a_reader.Read(out m_transmissionBytes);
                m_transmissionInfo = new TransmissionInfo(a_reader);
            }
            else if (a_reader.VersionNumber >= 1)
            {
                a_reader.Read(out m_play);
                a_reader.Read(out m_processingTime);
                Init((ScenarioBaseT)a_classFactory.Deserialize(a_reader));
            }
        }

        public void Serialize(IWriter a_writer)
        {
            #if DEBUG
            a_writer.DuplicateErrorCheck(this);
            #endif
            a_writer.Write(m_play);
            a_writer.Write(m_processingTime);
            a_writer.Write(m_transmissionBytes);
            m_transmissionInfo.Serialize(a_writer);
        }

        public const int UNIQUE_ID = 426;
        public int UniqueId => UNIQUE_ID;
        #endregion

        /// <summary>
        /// The amount of time it took to perform this transmission.
        /// </summary>
        private readonly TimeSpan m_processingTime;

        public TimeSpan ProcessingTime => m_processingTime;

        private bool m_play = true;

        public bool Play
        {
            get => m_play;
            internal set => m_play = value;
        }

        private byte[] m_transmissionBytes;
        private TransmissionInfo m_transmissionInfo;

        public TransmissionInfo TransmissionInfo => m_transmissionInfo;

        private static TransmissionClassFactory s_classFactory;

        public TransmissionJar(ScenarioBaseT a_t, TimeSpan a_timespan)
        {
            Init(a_t);
            m_processingTime = a_timespan;
        }

        private void Init(ScenarioBaseT a_t)
        {
            m_transmissionBytes = Transmission.Compress(a_t);
            m_transmissionInfo = new TransmissionInfo(a_t);
        }

        /// <summary>
        /// Creates transmission from compressed bytes. This should only be used when performing
        /// an undo. To get information about transmission in this Jar, use TransmissionInfo class.
        /// </summary>
        /// <returns></returns>
        public ScenarioBaseT GetTransmission()
        {
            if (s_classFactory == null)
            {
                s_classFactory = new TransmissionClassFactory();
            }

            return (ScenarioBaseT)Transmission.Decompress(m_transmissionBytes, s_classFactory);
        }

        public int TransmissionSize => m_transmissionBytes.Length;

        public override string ToString()
        {
            string description = m_transmissionInfo == null ? "No TransmissionInfo".Localize() : m_transmissionInfo.UndoChangeString;
            return $"{description}; {ProcessingTime:T}";
        }

        //Whether the description has been overriden from the serialized values (for example with more accurate info)
        public bool ActionDescriptionOverrideSet { get; private set; }

        private string m_actionDescriptionOverride;

        public string ActionDescriptionOverride
        {
            set
            {
                ActionDescriptionOverrideSet = true;
                m_actionDescriptionOverride = value;
            }
        }
    }

    /// <summary>
    /// Stores information about the transmissions in the undoset, so the transmission
    /// doesn't need to be deserialized unless it is being played back. For example,
    /// UI uses this info to show the list of transmission that can be undone.
    /// </summary>
    public class TransmissionInfo : IPTSerializable
    {
        private readonly ulong m_transmissionNbr;
        public ulong TransmissionNbr => m_transmissionNbr;
        private readonly ulong m_originalTransmissionNbr;
        public ulong OriginalTransmissionNbr => m_originalTransmissionNbr;
        private readonly BaseId m_instigator;
        public BaseId Instigator => m_instigator; 
        private readonly DateTimeOffset m_timeStamp; 
        public DateTimeOffset TimeStamp => m_timeStamp;
        private readonly bool m_isInternal;
        public bool IsInternal => m_isInternal;
        private readonly string m_undoChangeString;
        public string UndoChangeString => m_undoChangeString;
        private readonly int m_transmissionUniqueId;
        public int TransmissionUniqueId => m_transmissionUniqueId;
        private readonly Guid m_transmissionId = Guid.Empty;
        public Guid TransmissionId => m_transmissionId;

        public const int UNIQUE_ID = 825;
        public int UniqueId => UNIQUE_ID;

        public TransmissionInfo(IReader a_reader)
        {
            if (a_reader.VersionNumber >= 12520)
            {
                a_reader.Read(out m_transmissionNbr);
                m_instigator = new BaseId(a_reader);
                a_reader.ReadDateTimeToOffsetConversion(out m_timeStamp);
                a_reader.Read(out m_isInternal);
                a_reader.Read(out m_undoChangeString);
                a_reader.Read(out m_originalTransmissionNbr);
                a_reader.Read(out m_transmissionUniqueId);
                a_reader.Read(out m_transmissionId);
            }
            else if (a_reader.VersionNumber >= 12511)
            {
                a_reader.Read(out m_transmissionNbr);
                m_instigator = new BaseId(a_reader);
                a_reader.Read(out m_timeStamp);
                a_reader.Read(out m_isInternal);
                a_reader.Read(out m_undoChangeString);
                a_reader.Read(out m_originalTransmissionNbr);
                a_reader.Read(out m_transmissionUniqueId);
                a_reader.Read(out m_transmissionId);
            }
            else if (a_reader.VersionNumber >= 12500) //Same as 758 reader from v11. It is here to since multiple versions of Hydrogen were released with this serialization
            {
                a_reader.Read(out m_transmissionNbr);
                m_instigator = new BaseId(a_reader);
                a_reader.ReadDateTimeToOffsetConversion(out m_timeStamp);
                a_reader.Read(out m_isInternal);
                a_reader.Read(out m_undoChangeString);
                a_reader.Read(out m_originalTransmissionNbr);
                a_reader.Read(out m_transmissionUniqueId);
            }
            else if (a_reader.VersionNumber >= 12430) // Neptune backwards compatibility
            {
                a_reader.Read(out m_transmissionNbr);
                m_instigator = new BaseId(a_reader);
                a_reader.ReadDateTimeToOffsetConversion(out m_timeStamp);
                a_reader.Read(out m_isInternal);
                a_reader.Read(out m_undoChangeString);
                a_reader.Read(out m_originalTransmissionNbr);
                a_reader.Read(out m_transmissionUniqueId);
                a_reader.Read(out m_transmissionId);
            }
            
            else if (a_reader.VersionNumber >= 12108) //Same as 758 reader from v11
            {
                a_reader.Read(out m_transmissionNbr);
                m_instigator = new BaseId(a_reader);
                a_reader.ReadDateTimeToOffsetConversion(out m_timeStamp);
                a_reader.Read(out m_isInternal);
                a_reader.Read(out m_undoChangeString);
                a_reader.Read(out m_originalTransmissionNbr);
                a_reader.Read(out m_transmissionUniqueId);
            }
            else if (a_reader.VersionNumber >= 12000) //Save as 738 for V12 backwards compatibility
            {
                a_reader.Read(out m_transmissionNbr);
                m_instigator = new BaseId(a_reader);
                a_reader.ReadDateTimeToOffsetConversion(out m_timeStamp);
                a_reader.Read(out m_isInternal);
                a_reader.Read(out m_undoChangeString);
                a_reader.Read(out m_originalTransmissionNbr);
            }
            else if (a_reader.VersionNumber >= 758)
            {
                a_reader.Read(out m_transmissionNbr);
                m_instigator = new BaseId(a_reader);
                a_reader.ReadDateTimeToOffsetConversion(out m_timeStamp);
                a_reader.Read(out m_isInternal);
                a_reader.Read(out m_undoChangeString);
                a_reader.Read(out m_originalTransmissionNbr);
                a_reader.Read(out m_transmissionUniqueId);
            }
            else if (a_reader.VersionNumber >= 738)
            {
                a_reader.Read(out m_transmissionNbr);
                m_instigator = new BaseId(a_reader);
                a_reader.ReadDateTimeToOffsetConversion(out m_timeStamp);
                a_reader.Read(out m_isInternal);
                a_reader.Read(out m_undoChangeString);
                a_reader.Read(out m_originalTransmissionNbr);
            }
            else if (a_reader.VersionNumber >= 657)
            {
                a_reader.Read(out m_transmissionNbr);
                m_instigator = new BaseId(a_reader);
                a_reader.ReadDateTimeToOffsetConversion(out m_timeStamp);
                a_reader.Read(out m_isInternal);
                a_reader.Read(out m_undoChangeString);
            }
        }

        public void Serialize(IWriter a_writer)
        {
            a_writer.Write(m_transmissionNbr);
            m_instigator.Serialize(a_writer);
            a_writer.Write(m_timeStamp);
            a_writer.Write(m_isInternal);
            a_writer.Write(m_undoChangeString);
            a_writer.Write(m_originalTransmissionNbr);
            a_writer.Write(m_transmissionUniqueId);
            a_writer.Write(m_transmissionId);
        }

        internal TransmissionInfo(ScenarioBaseT a_t)
        {
            m_transmissionNbr = a_t.TransmissionNbr;
            m_originalTransmissionNbr = a_t.OriginalTransmissionNbr;
            m_instigator = a_t.Instigator;
            m_timeStamp = a_t.TimeStamp;
            m_isInternal = IsInternalTransmission(a_t);
            m_undoChangeString = MakeUndoChangeString(a_t);
            m_transmissionUniqueId = a_t.UniqueId;
            m_transmissionId = a_t.TransmissionId;
        }

        /// <summary>
        /// No need to display these in the list
        /// </summary>
        /// <param name="a_transmission"></param>
        /// <returns></returns>
        internal static bool IsInternalTransmission(PTTransmission a_t)
        {
            return a_t is ScenarioChecksumT or ScenarioUndoT or ScenarioStartUndoT or ScenarioUndoCheckpointT or SystemMessageT or UserErrorT or UserLogOnT or ScenarioTouchT or ScenarioDetailExportT;
        }

        private static string MakeUndoChangeString(Transmission t)
        {
            return t.Description.Localize();
        }
    }

    public class UndoSet : IPTSerializable
    {
        public const int UNIQUE_ID = 427;

        #region IPTSerializable Members
        public UndoSet(IReader reader)
        {
            IClassFactory classFactory = new TransmissionClassFactory();

            if (reader.VersionNumber >= 659)
            {
                m_undoNbr = new Id(reader);
                reader.Read(out m_checkPointSize);
                reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    TransmissionJar jar = new (reader, classFactory);
                    Add(jar);
                }
            }
            else if (reader.VersionNumber >= 1)
            {
                m_undoNbr = new Id(reader);
                reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    TransmissionJar jar = new (reader, classFactory);
                    Add(jar);
                }
            }
        }

        public void Serialize(IWriter writer)
        {
            #if DEBUG
            writer.DuplicateErrorCheck(this);
            #endif
            m_undoNbr.Serialize(writer);
            writer.Write(m_checkPointSize);
            writer.Write(Count);
            for (int i = 0; i < Count; i++)
            {
                this[i].Serialize(writer);
            }
        }

        public int UniqueId => UNIQUE_ID;
        #endregion

        public readonly Id m_undoNbr;
        private readonly long m_checkPointSize;
        private readonly List<TransmissionJar> m_jars = new ();

        public UndoSet(Id a_undoNbr, long a_fileSize)
        {
            m_undoNbr = a_undoNbr;
            m_checkPointSize = a_fileSize;
        }

        public long CheckPointSize => m_checkPointSize;

        /// <summary>
        /// Removes transmissions are preserved for redo functionality, specified by TransmissionJar.Play = false.
        /// This is called before a new transmission (that is not undoT) is added to UndoSet.
        /// </summary>
        internal void RemoveRedoTransmissions()
        {
            for (int i = Count - 1; i >= 0; i--)
            {
                TransmissionJar tj = this[i];
                if (!tj.Play)
                {
                    RemoveAt(i);
                }
            }
        }

        internal TransmissionJar GetTJByTransmissionNbr(ulong a_tNbr, bool a_recording)
        {
            foreach (TransmissionJar transmissionJar in m_jars)
            {
                ulong jarTransmissionNbr;
                if (a_recording)
                {
                    jarTransmissionNbr = transmissionJar.TransmissionInfo.OriginalTransmissionNbr == 0 ? transmissionJar.TransmissionInfo.TransmissionNbr : transmissionJar.TransmissionInfo.OriginalTransmissionNbr;
                }
                else
                {
                    jarTransmissionNbr = transmissionJar.TransmissionInfo.TransmissionNbr;
                }

                if (jarTransmissionNbr == a_tNbr)
                {
                    return transmissionJar;
                }
            }

            return null;
        }

        public void Add(ScenarioBaseT a_t, TimeSpan a_aTS)
        {
            TransmissionJar tj = new (a_t, a_aTS);
            Add(tj);
        }

        private void Add(TransmissionJar a_tj)
        {
            m_jars.Add(a_tj);
        }

        internal void RemoveAt(int a_idx)
        {
            m_jars.RemoveAt(a_idx);
        }

        public int Count => m_jars.Count;

        public TransmissionJar this[int a_i] => m_jars[a_i];

        #region IDeserializationInit Members
        #endregion

        /// <summary>
        /// Return the length of time it took to play all the tranmissions in this set.
        /// </summary>
        /// <returns></returns>
        internal TimeSpan TotalTimeSpan()
        {
            TimeSpan total = new ();

            for (int tjI = 0; tjI < Count; ++tjI)
            {
                TransmissionJar tj = this[tjI];
                total += tj.ProcessingTime;
            }

            return total;
        }

        public override string ToString()
        {
            return string.Format("Contains a total of {0} Transmissions. TotalTimeSpan to replay={1:T}".Localize(), Count, TotalTimeSpan());
        }
    }

    public class UndoSets : IPTSerializable
    {
        public const int UNIQUE_ID = 428;

        #region IPTSerializable Members
        public UndoSets(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                reader.Read(out int count);
                for (int i = 0; i < count; i++)
                {
                    UndoSet us = new (reader);
                    m_undoSets.Add(us);
                }
            }
        }

        public void Serialize(IWriter writer)
        {
            #if DEBUG
            writer.DuplicateErrorCheck(this);
            #endif
            writer.Write(Count);
            for (int i = 0; i < Count; i++)
            {
                this[i].Serialize(writer);
            }
        }

        public int UniqueId => UNIQUE_ID;

        internal void RestoreReferences(Scenario a_scenario)
        {
            m_scenario = a_scenario;
        }
        #endregion

        private readonly List<UndoSet> m_undoSets = new ();
        private Scenario m_scenario;

        internal UndoSets(Scenario scenario)
        {
            m_scenario = scenario;
        }

        #region Disposal
        ~UndoSets()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(false);
        }

        private bool m_disposed;

        private void Dispose(bool a_finalizing)
        {
            if (!m_disposed)
            {
                m_disposed = true;
                
                for (int i = 0; i < m_undoSets.Count; ++i)
                {
                    // Delete the undo checkpoint file.
                    UndoSet deleteUs = m_undoSets[i];

                    //Do not remove undo checkpoint file if scenario was flagged as a temporary scenario
                    if (m_scenario != null && !m_scenario.m_isTempScenario) // If the system needs to be destroyed before it's completely made this can be null. This happens in a ScenarioTouchT.
                    {
                        string path = m_scenario.SerializationPath(deleteUs.m_undoNbr);
                        File.Delete(path);
                    }
                }
            }
        }
        #endregion

        internal TransmissionJar GetTransmissionJarByTransmissionNbr(ulong a_transmissionNbr, bool a_recording)
        {
            foreach (UndoSet undoSet in m_undoSets)
            {
                TransmissionJar jar = undoSet.GetTJByTransmissionNbr(a_transmissionNbr, a_recording);
                if (jar != null)
                {
                    return jar;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a new UndoSet. New transmissions will be added to this "Current" UndoSet.
        /// </summary>
        /// <param name="a_undoNbr"></param>
        internal void AddNewCurrentUndoSet(Id a_undoNbr, long a_fileSize)
        {
            UndoSet us = new (a_undoNbr, a_fileSize);
            #if DEBUG
            if (m_undoSets.Any(x => x.m_undoNbr == a_undoNbr))
            {
                DebugException.ThrowInDebug("An undo set with the same undo number already exists.");
            }
            #endif
            m_undoSets.Add(us);
        }

        /// <summary>
        /// Clear the undo sets.
        /// </summary>
        /// <param name="o_notifyClientsOfUndoChange">Whether NotifyClientsOfUndoChange() needs to be called after all locks have been cleared.</param>
        internal void Clear(out bool o_notifyClientsOfUndoChange)
        {
            o_notifyClientsOfUndoChange = false;

            while (m_undoSets.Count > 0)
            {
                RemoveFirstUndoSet();
                o_notifyClientsOfUndoChange = true;
            }
        }
        /// <summary>
        /// Temporarily clears undo sets of the scenario from memory without removing them from the scenario checkpoints on disk
        /// </summary>
        internal void TempClearUndoSets()
        {
            while (m_undoSets.Count > 0)
            {
                m_undoSets.RemoveAt(0);
            }
        }
        private void RemoveFirstUndoSet()
        {
            // Delete the undo checkpoint file.
            UndoSet deleteUs = m_undoSets[0];
            string path = m_scenario.SerializationPath(deleteUs.m_undoNbr);
            File.Delete(path);

            // Remove the undo checkpoint from the undocheckpoints set.
            m_undoSets.RemoveAt(0);
        }
       
        public int Count => m_undoSets.Count;

        public UndoSet this[int a_i]
        {
            get
            {
                if (a_i > m_undoSets.Count - 1)
                {
                    throw new PTException("2742", new object[] { a_i });
                }

                return m_undoSets[a_i];
            }
        }

        private UndoSet Current
        {
            get
            {
                if (m_undoSets.Count == 0)
                {
                    throw new PTException("2893");
                }

                return m_undoSets[m_undoSets.Count - 1];
            }
        }

        internal int CurrentUndoSetTransmissionCount
        {
            get
            {
                if (m_undoSets.Count == 0)
                {
                    return 0;
                }

                return Current.Count;
            }
        }

        internal decimal CurrentMemoryUsageMB
        {
            get
            {
                decimal totalSize = 0;
                foreach (UndoSet undoSet in m_undoSets)
                {
                    for (int i = 0; i < undoSet.Count; i++)
                    {
                        TransmissionJar transmissionJar = undoSet[i];
                        totalSize += transmissionJar.TransmissionSize;
                    }

                    totalSize += undoSet.CheckPointSize;
                }

                return totalSize / 1000000;
            }
        }

        internal void AddTransmissionToCurrentSet(ScenarioBaseT a_t, TimeSpan a_aTS)
        {
            if (!PTSystem.Server)
            {
                return;
            }

            if (!TransmissionInfo.IsInternalTransmission(a_t))
            {
                RemoveRedoTransmissions();
            }

            Current.Add(a_t, a_aTS);
            if (Current.Count == 1)
            {
                // The new set has had its first entry added to it. Get rid of any unneeded UndoSets.
                ReduceNbrOfUndoSetsToTheMaximumNbrOfUndoSets();
            }
        }

        private void ReduceNbrOfUndoSetsToTheMaximumNbrOfUndoSets()
        {
            // If the settings have changed, it's possible more than 1 UndoSet needs to be removed.
            while (CurrentMemoryUsageMB > MaxUndoMemoryUsage)
            {
                RemoveFirstUndoSet();
            }
        }

        public int FindUndoSetByTransmissionNbr(ulong a_transmissionNbr, bool a_recording)
        {
            for (int i = 0; i < Count; ++i)
            {
                UndoSet us = this[i];

                if (us.GetTJByTransmissionNbr(a_transmissionNbr, a_recording) != null)
                {
                    return i;
                }
            }

            return -1;
        }

        public UndoSet FindUndoSet(Id a_id)
        {
            for (int i = 0; i < Count; ++i)
            {
                UndoSet us = this[i];

                if (us.m_undoNbr == a_id)
                {
                    return us;
                }
            }

            return null;
        }

        public int FindUndoSetIdx(Id a_id)
        {
            for (int i = 0; i < Count; ++i)
            {
                UndoSet us = this[i];

                if (us.m_undoNbr == a_id)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Remove redo transmissions in all UndoSets
        /// </summary>
        internal void RemoveRedoTransmissions()
        {
            foreach (UndoSet us in m_undoSets)
            {
                us.RemoveRedoTransmissions();
            }
        }

        public override string ToString()
        {
            int totalTransmissions = 0;
            TimeSpan totalTimeSpan = new ();

            for (int usI = 0; usI < Count; ++usI)
            {
                UndoSet us = this[usI];
                totalTransmissions += us.Count;
                totalTimeSpan += us.TotalTimeSpan();
            }

            return string.Format("Contains a total of {0} undo sets, {3} transmissions. TotalTimeSpan to replay={1:T}. Total memory used: {2}".Localize(), Count, totalTimeSpan, CurrentMemoryUsageMB, totalTransmissions);
        }
    }

    /// <summary>
    /// Get the path and file name that the undo set should be saved as.
    /// </summary>
    /// <param name="undoNbr">A unique incremental number that uniquely identifies the undo set within this scenario.</param>
    /// <returns>The path and file name to save the redo set as.</returns>
    internal string SerializationPath(Id a_undoNbr)
    {
        return Path.Combine(PTSystem.WorkingDirectory.Checkpoints, SerializationFileName(a_undoNbr));
    }

    internal static string GetSerializationPath(string a_checkpoint)
    {
        return Path.Combine(PTSystem.WorkingDirectory.Checkpoints, a_checkpoint);
    }

    internal string SerializationFileName(Id a_undoNbr)
    {
        string fileName;
        using (ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
        {
            fileName = string.Format("scenario.{0}.copy.{1}.bin", ss.Id.ToString(), a_undoNbr);
        }

        return fileName;
    }

    /// <summary>
    /// Save the current undo set and starts the next.
    /// Make sure nothing is locked prior to calling this function. It fire an event.
    /// </summary>
    internal void CreateUndoSet(UndoSets a_uss)
    {
        if (!PTSystem.Server)
        {
            return;
        }
        UndoCheckpoint c = new ();
        string serializationPath = SerializationPath(m_nextUndoNbr);
        using (ScenarioDetailLock.EnterRead(out c.m_sd))
        {
            using (ScenarioSummaryLock.EnterRead(out c.m_ss))
            {
                using (BinaryFileWriter writer = new (serializationPath, Common.Compression.ECompressionType.Fast))
                {
                    c.Serialize(writer);
                }
            }
        }

        long fileSize = new FileInfo(serializationPath).Length;

        m_dispatchTime = TimeSpan.Zero;
        a_uss.AddNewCurrentUndoSet(m_nextUndoNbr, fileSize);
        ++m_nextUndoNbr;
    }

    /// <summary>
    /// Locks ScenarioUndoEvents and Fires ScenarioUndoEvents.FireUndoSetChangedEvent()
    /// </summary>
    internal void FireUndoSetChangedEvent(ScenarioBaseT a_t)
    {
        using (ScenarioUndoEventsLock.EnterRead(out ScenarioUndoEvents sue))
        {
            sue.FireUndoSetChangedEvent(Id, a_t);
        }
    }

    //This is set for the duration of an Undo. It us used to suppress ChecksumTs created during transmission processing.
    private bool m_performingUndoOrRedo;

    internal void SwapProductionData(ScenarioDetail a_sd)
    {
        using (ScenarioDetailLock.EnterWrite(out ScenarioDetail _))
        {
            SetScenarioDetail(a_sd);
            m_scenarioDetail.RestoreReferences(Serialization.VersionNumber, this, false, m_packageManager, m_errorReporter, m_extensionController);
            KpiController.InitCalculatorList(m_packageManager.GetKpiCalculatorModules());
        }
    }

    /// <summary>
    /// Load a scenario and replay messages up to and including the end point transmission.
    /// Make sure nothing is locked prior to calling this function. It fire events.
    /// </summary>
    /// <param name="undoNbr">The number of the undo set that contains the scenario you want.</param>
    /// <param name="endPoint">The last transmission in the undo set to replay.</param>
    private void Undo(ScenarioUndoT a_undoT)
    {
        m_performingUndoOrRedo = true;
        if (a_undoT.UndoIds.Count == 0)
        {
            throw new UndoValidationException("2295");
        }

        using (ScenarioUndoEventsLock.EnterRead(out ScenarioUndoEvents sue))
        {
            sue.FireUndoBeginEvent(Id, !a_undoT.Redo, a_undoT.Description);
        }

        byte[] scenarioBytes = [];
        try
        {
            using (UndoSetsLock.EnterRead(out UndoSets uss))
            {
                Id minUSNbr = a_undoT.InitialUndoSetId;

                string path = SerializationPath(minUSNbr);
                int verNbr;
                UndoCheckpoint checkpoint;
                using (BinaryFileReader reader = new (path))
                {
                    verNbr = reader.VersionNumber;
                    checkpoint = new UndoCheckpoint(reader);
                }

                ScenarioSummary ss;
                using (ScenarioDetailLock.EnterWrite(out ScenarioDetail _))
                {
                    using (ScenarioSummaryLock.EnterWrite(out ss))
                    {
                        SetScenarioDetail(checkpoint.m_sd);
                        SetScenarioSummary(checkpoint.m_ss);
                        m_scenarioDetail.RestoreReferences(verNbr, this, false, m_packageManager, m_errorReporter, m_extensionController);
                        KpiController.InitCalculatorList(m_packageManager.GetKpiCalculatorModules());
                    }
                }

                int minUSIdx = uss.FindUndoSetIdx(minUSNbr);
                
                for (int i = minUSIdx; i < uss.Count; ++i)
                {
                    UndoSet us = uss[i];
                    for (int j = 0; j < us.Count; ++j)
                    {
                        TransmissionJar tj = us[j];
                        ScenarioBaseT t = tj.GetTransmission();

                        //This action is being modified by the transmission
                        if (a_undoT.UndoIds.Contains(t.TransmissionId))
                        {
                            tj.Play = a_undoT.Redo;
                        }

                        if (tj.Play) //Play
                        {
                            t.ReplayForUndoRedo = true;
                            try
                            {
                                Receive(t);
                            }
                            catch (Exception)
                            {
                                //This would have occured previously and been logged.
                            }
                        }
                    }

                    UndoCheckpoint uc = new ();
                    using (ScenarioDetailLock.EnterRead(out uc.m_sd))
                    {
                        using (ScenarioSummaryLock.EnterRead(out uc.m_ss))
                        {
                            Id id = us.m_undoNbr;
                            ++id;

                            using (BinaryFileWriter writer = new (SerializationPath(id), Common.Compression.ECompressionType.Fast))
                            {
                                uc.Serialize(writer);
                            }
                        }
                    }
                }
            }

            m_performingUndoOrRedo = false;

            //Send new scenario back to client
            scenarioBytes = GetClientScenarioBytes();
        }
        finally
        {
            m_performingUndoOrRedo = false;
            FireUndoSetChangedEvent(a_undoT);

            if (!a_undoT.Recording)
            {
                ScenarioReplaceT replaceT = new (Id, scenarioBytes ?? []);
                replaceT.ClientWillWaitForScenarioResult = true;
                replaceT.Instigator = a_undoT.Instigator;
                replaceT.CancellingSimulation = a_undoT.CancellingSimulation;
                replaceT.InstigatorTransmissionId = a_undoT.UniqueId;
                SystemController.ClientSession.SendClientAction(replaceT);
            }
        }
    }
    /// <summary>
    /// Serializes and returns the bytes array of this scenario optimize specifically for the client
    /// </summary>
    /// <returns></returns>
    internal byte[] GetClientScenarioBytes()
    {
        byte[] scenarioBytes;
        using (BinaryMemoryWriter writer = new(ECompressionType.Normal))
        {
            Serialize(writer, SerializeForClient);
            scenarioBytes = writer.GetBuffer();
        }

        return scenarioBytes;
    }

    /// <summary>
    /// This function takes an UndoSets, gets all the transmissions from them, and sends them
    /// over to the scenario to be Received. 
    /// </summary>
    /// <param name="a_undoSets"></param>
    /// This function is not used right now because there are some issues with playing back
    /// undos or when playing back incomplete history. This function is kept here because
    /// the goal is now to restrict the Merge Scenario data functionality to only be used
    /// by copilot in specific situations so the two issues discussed above are not encountered. 
    internal void PlayTransmissionsInUndoSets(UndoSets a_undoSets)
    {
        for (int i = 0; i < a_undoSets.Count; i++)
        {
            UndoSet undoSet = a_undoSets[i];
            for (int j = 0; j < undoSet.Count; j++)
            {
                TransmissionJar transmissionJar = undoSet[j];

                ScenarioBaseT transmission = transmissionJar.GetTransmission();
                
                if (transmission is ScenarioChangeT
                    || transmission is ConvertToProductionScenarioT)
                {
                    // There might be other transmissions that need to be blocked
                    continue;
                }

                if (transmissionJar.Play)
                {
                    transmission.ReplayForUndoRedo = false;
                    Receive(transmission);
                }

            }
        }
    }

    /// <summary>
    /// This form of the dispatch function is only called on the server.
    /// It keeps track of the amount of time spent dispatching transmissions within the current undo set.
    /// </summary>
    /// <param name="t">The transmission to dispatch.</param>
    private void ServerProcessing(ScenarioBaseT a_t, TimeSpan a_span)
    {
        if (a_t.Recording)
        {
            // When a recording is being replayed we don't send undocheckpoints because they are already part of the recordings.
            // Checksums cannot be sent because transmissions are played before any actions occur and there is no checksum data to calculate.
            return;
        }
        
        SystemController.ServerSessionManager.CheckRequestedChecksum(Id,a_t.TransmissionId);

        if (!(a_t is ScenarioUndoT || a_t is ScenarioUndoCheckpointT))
        {
            m_dispatchTime += a_span;
        }

        // When the undo threshold is passed a message is sent which indicates its time to 
        // start a new undo set.
        decimal memoryUsage;
        using (ObjectAccess<UndoSets> uss = UndoSetsLock.EnterRead())
        {
            memoryUsage = uss.Instance.CurrentMemoryUsageMB;
        }

        if (m_dispatchTime.TotalSeconds > UndoThreshold || memoryUsage >= MaxUndoMemoryUsage)
        {
            ScenarioUndoCheckpointT ut;
            using (ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
            {
                if (a_t is ScenarioIdBaseT)
                {
                    //For a specific scenario only
                    ut = new ScenarioUndoCheckpointT(ss.Id);
                }
                else
                {
                    //This is heading to all scenarios
                    ut = new ScenarioUndoCheckpointT(ss.Id);
                }
            }

            ut.TransmissionSender = PTTransmissionBase.TransmissionSenderType.PTSystem;
            SystemController.ClientSession.SendClientAction(ut);
        }
    }

    internal void StopListeningToEvents()
    {
        using (ObjectAccess<ScenarioEvents> se = ScenarioEventsLock.EnterWrite())
        {
            se.Instance.SimulationProgressEvent -= new ScenarioEvents.SimulationProgressDelegate(HandleSimulationProgressEvent);
        }
    }

    private void StartListeningToEvents()
    {
        using (ObjectAccess<ScenarioEvents> se = ScenarioEventsLock.EnterWrite())
        {
            se.Instance.SimulationProgressEvent += new ScenarioEvents.SimulationProgressDelegate(HandleSimulationProgressEvent);
        }
    }

    private void HandleSimulationProgressEvent(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, long a_simNbr, decimal a_percentComplete, SimulationProgress.Status a_status)
    {
        if (Id == BaseId.NULL_ID)
        {
            //Some CoPilot transmission may be sent to reproduce a scenario. ChecksumTs from this event will be out of sequence.
            return;
        }

        if (a_status == SimulationProgress.Status.PostSimulationWorkComplete)
        {
            CalculateKPIs(a_sd, a_simType, a_t);
            m_calculateChecksum = true;
        }
    }

    private void CleanUndoTransmissions(BaseId a_userId, out bool o_fireUndoSetChangedEvent)
    {
        using (UndoSetsLock.EnterWrite(out UndoSets uss))
        {
            for (int i = 0; i < uss.Count; ++i)
            {
                UndoSet us = uss[i];
                for (int j = us.Count - 1; j >= 0; --j)
                {
                    TransmissionJar jar = us[j];

                    if (jar.TransmissionInfo.Instigator != BaseId.ERP_ID)
                    {
                        if (jar.TransmissionInfo.Instigator == a_userId && !jar.Play)
                        {
                            us.RemoveAt(j);
                        }
                    }
                }
            }

            o_fireUndoSetChangedEvent = true;
        }
    }


    internal class CheckPointPath
    {
        public CheckPointPath(string a_fileName, string a_path)
        {
            m_fullPath = a_path;
            m_fileName = a_fileName;
        }

        public string m_fullPath;
        public string m_fileName;
    }

    internal List<CheckPointPath> GetUndoCheckpointPaths()
    {
        List<CheckPointPath> undoCPPaths = new ();
        using (UndoSetsLock.EnterRead(out UndoSets uss))
        {
            for (int undoSetIdx = 0; undoSetIdx < uss.Count; ++undoSetIdx)
            {
                UndoSet us;

                us = uss[undoSetIdx];
                string path = SerializationPath(us.m_undoNbr);
                string fileName = SerializationFileName(us.m_undoNbr);

                undoCPPaths.Add(new CheckPointPath(fileName, path));
            }
        }

        return undoCPPaths;
    }
    #endregion

    void IDeserializationInit.DeserializationInit()
    {
        using (ScenarioDetailLock.EnterWrite(out ScenarioDetail sd))
        {
            IScenarioRef sr = sd;
            sr.SetReferences(this, null);
        }

        using (UndoSetsLock.EnterWrite(out UndoSets uss))
        {
            IDeserializationInit di = (IDeserializationInit)uss;
            di.DeserializationInit();
        }
    }

    internal long CopyInMemory(out ScenarioDetail o_sd, out ScenarioSummary o_ss)
    {
        long totalSize;
        using (ScenarioDetailLock.EnterRead(out o_sd))
        {
            using (ScenarioSummaryLock.EnterRead(out o_ss))
            {
                o_sd = CopyInMemory(o_sd, out long sizeOfCopy);
                totalSize = sizeOfCopy;
                o_ss = CopyInMemory(o_ss, out sizeOfCopy);
                return totalSize += sizeOfCopy;
            }
        }
    }

    /// <summary>
    /// Creates a copy of a ScenarioDetail and ScenarioSummary that have been previously serialized into byte[] containers.
    /// </summary>
    /// <param name="o_sd">New ScenarioDetail</param>
    /// <param name="o_ss">New ScenarioSummary</param>
    /// <param name="a_sdWriter">ScenarioDetail writer</param>
    /// <param name="a_ssWriter">ScenarioSummary writer</param>
    internal static void CopyFromSerializedByteArray(out ScenarioDetail o_sd, out ScenarioSummary o_ss, byte[] a_sdWriter, byte[] a_ssWriter)
    {
        o_sd = CopySDFromSerializedByteArray(a_sdWriter);
        o_ss = CopySSFromSerializedByteArray(a_ssWriter);
    }

    /// <summary>
    /// Copies the ScenarioDetail and ScenarioSummary and serializes them into byte[] containers.
    /// </summary>
    /// <param name="o_sdByteArray">New ScenarioDetail writer</param>
    /// <param name="o_ssByteArray">New ScenarioSummary writer</param>
    internal void CopyAndStoreInByteArrays(out byte[] o_sdByteArray, out byte[] o_ssByteArray)
    {
        using (ScenarioDetailLock.EnterRead(out ScenarioDetail sd))
        {
            using (ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
            {
                o_sdByteArray = CopyAndStoreInByteArray(sd);
                o_ssByteArray = CopyAndStoreInByteArray(ss);
            }
        }
    }

    internal long CopyWithFile(out ScenarioDetail o_sd, out ScenarioSummary o_ss)
    {
        long totalSize;
        using (ScenarioDetailLock.EnterWrite(out o_sd))
        {
            using (ScenarioSummaryLock.EnterWrite(out o_ss))
            {
                o_sd = CopyWithFile(o_sd, out long sizeOfCopy);
                totalSize = sizeOfCopy;
                o_ss = CopyInMemory(o_ss, out sizeOfCopy);
                return totalSize += sizeOfCopy;
            }
        }
    }

    internal static byte[] CopyAndStoreInByteArray(ScenarioDetail a_sd)
    {
        return Serialization.CopyAndStoreInByteArray(a_sd);
    }

    internal static byte[] CopyAndStoreInByteArray(ScenarioSummary a_ss)
    {
        return Serialization.CopyAndStoreInByteArray(a_ss);
    }

    internal static ScenarioDetail CopySDFromSerializedByteArray(byte[] a_writer)
    {
        return (ScenarioDetail)Serialization.CopyFromSerializedByteArray(a_writer, new Serialization.CopyCreatorDelegate(SDCreator));
    }

    internal static ScenarioSummary CopySSFromSerializedByteArray(byte[] a_writer)
    {
        return (ScenarioSummary)Serialization.CopyFromSerializedByteArray(a_writer, new Serialization.CopyCreatorDelegate(SSCreator));
    }

    internal static ScenarioDetail CopyInMemory(ScenarioDetail a_sD, out long a_sizeOfScenarioDetail)
    {
        return (ScenarioDetail)Serialization.CopyInMemory(a_sD, new Serialization.CopyCreatorDelegate(SDCreator), out a_sizeOfScenarioDetail);
    }

    internal static ScenarioDetail CopyWithFile(ScenarioDetail a_sD, out long a_sizeOfScenarioDetail)
    {
        return (ScenarioDetail)Serialization.CopyWithFile(a_sD, new Serialization.CopyCreatorDelegate(SDCreator), out a_sizeOfScenarioDetail);
    }

    internal static ScenarioSummary CopyInMemory(ScenarioSummary a_sS, out long a_sizeOfScenarioSummary)
    {
        return (ScenarioSummary)Serialization.CopyInMemory(a_sS, new Serialization.CopyCreatorDelegate(SSCreator), out a_sizeOfScenarioSummary);
    }

    /// <summary>
    /// Create a new Blank Scenario.
    /// </summary>
    private void NewScenario()
    {
        //TODO
    }

    private static object SDCreator(IReader reader)
    {
        ScenarioDetail newSD = new (reader);
        return newSD;
    }

    private static object SSCreator(IReader reader)
    {
        ScenarioSummary newSS = new (reader);
        return newSS;
    }

    internal static void SaveCheckpointAtStartup(string a_ileName, byte[] a_ytes)
    {
        FileUtils.SaveBinaryFile(GetSerializationPath(a_ileName), a_ytes);
    }

    /// <summary>
    /// The identifier of this scenario.
    /// </summary>
    public BaseId Id
    {
        get
        {
            using (ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
            {
                return ss.Id;
            }
        }
    }

    /// <summary>
    /// Type of this scenario.
    /// </summary>
    public ScenarioTypes Type
    {
        get
        {
            ScenarioSummary ss;
            using (ScenarioSummaryLock.EnterRead(out ss))
            {
                return ss.Type;
            }
        }
    }

    /// <summary>
    /// Name of this scenario.
    /// </summary>
    public string Name
    {
        get
        {
            using (ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
            {
                return ss.Name;
            }
        }
    }

    /// <summary>
    /// Locks ScenarioUndoEvents and Fires ScenarioUndoEvents.FireIsolationChanged()
    /// </summary>
    private void FireIsolationChangedEvent(ScenarioIsolateT a_t)
    {
        using (ScenarioEventsLock.EnterRead(out ScenarioEvents se))
        {
            se.FireScenarioIsolationEvent(a_t);
        }
    }

    public void InitDataLicensing(IEnumerable<ILicenseValidationModule> a_modules)
    {
        m_licenseManager.RegisterLicenseElements(a_modules);
        VerifyDataLicensing();
    }

    public void InitPermissionModules(List<IPermissionValidationModule> a_modules)
    {
        m_scenarioPermissionValidationManager.InitializeValidationElements(a_modules);
    }

    public bool ValidateScenarioCanEditPermissions(BaseId a_userId, ScenarioPermissionSettings a_scenarioPermissionSettings)
    {
        return m_userManager.ValidateScenarioEditPermissions(a_userId, a_scenarioPermissionSettings);
    }

    /// <summary>
    /// Check all of the loaded customizations for additinal simulation flags. A customization may need another simulation performed.
    /// </summary>
    private void CheckForRequiredAdditionalSimulation()
    {
        ScenarioDataChanges dataChanges = new ();
        SortedList<int, ScenarioBaseT> transmissions = m_extensionController.GetExtraTransmissionsToProcess();
        using (ScenarioDetailLock.EnterRead(out ScenarioDetail sd))
        {
            foreach (ScenarioBaseT t in transmissions.Values)
            {
                //Change the instigator because many transmissions will default to ERP, which can have unintended side effects.
                t.Instigator = BaseId.NULL_ID; //Possibly allow customizations to set this
                if (t is ScenarioDetailMoveT)
                {
                    sd.Receive(t, dataChanges, new UndoReceiveMove());
                }
                else
                {
                    sd.Receive(t, dataChanges);
                }
            }
        }

        if (dataChanges.HasChanges)
        {
            using (ScenarioEventsLock.EnterRead(out ScenarioEvents se))
            {
                se.FireScenarioDataChangedEvent(dataChanges);
            }
        }
    }

    private void SetPublishOptionsBackwardCompatibility(ScenarioSummary a_summary, ScenarioPublishOptions a_scenarioPublishOptions)
    { 
        ScenarioPublishDestinations scenarioPublishDestinations = new ScenarioPublishDestinations(a_scenarioPublishOptions.PublishAllActivitesForMO, a_scenarioPublishOptions.PublishToCustomDll, a_scenarioPublishOptions.PublishToSQL, a_scenarioPublishOptions.PublishToXML);
        a_summary.ScenarioSettings.SaveSetting(new SettingData(scenarioPublishDestinations), false);

       ScenarioPublishHistory scenarioPublishHistory = new ScenarioPublishHistory(a_scenarioPublishOptions.EnableHistory, a_scenarioPublishOptions.HistoryHorizonSpan, a_scenarioPublishOptions.HistoryMaxAge, a_scenarioPublishOptions.WhatIfHistoryMaxAge);
       a_summary.ScenarioSettings.SaveSetting(new SettingData(scenarioPublishHistory), false);
       
       ScenarioPublishDataLimits scenarioPublishDataLimits = new ScenarioPublishDataLimits(a_scenarioPublishOptions.PublishInventory, a_scenarioPublishOptions.PublishBlocks, a_scenarioPublishOptions.PublishBlockIntervals, a_scenarioPublishOptions.KeepHistoryInventory, a_scenarioPublishOptions.KeepHistoryBlocks,
            a_scenarioPublishOptions.KeepHistoryBlockIntervals, a_scenarioPublishOptions.PublishCapacityIntervals, a_scenarioPublishOptions.KeepCapacityIntervals, a_scenarioPublishOptions.PublishProductRules, a_scenarioPublishOptions.KeepHistoryProductRules, a_scenarioPublishOptions.PublishJobs,
            a_scenarioPublishOptions.KeepHistoryJobs, a_scenarioPublishOptions.PublishTemplates, a_scenarioPublishOptions.KeepHistoryTemplates, a_scenarioPublishOptions.PublishManufacturingOrders, a_scenarioPublishOptions.KeepHistoryManufacturingOrders, a_scenarioPublishOptions.PublishOperations,
            a_scenarioPublishOptions.KeepHistoryOperations, a_scenarioPublishOptions.PublishActivities, a_scenarioPublishOptions.KeepHistoryActivities);
        a_summary.ScenarioSettings.SaveSetting(new SettingData(scenarioPublishDataLimits), false);
       
        ScenarioPublishNetChange scenarioPublishNetChange = new ScenarioPublishNetChange(a_scenarioPublishOptions.RunStoredProcedureAfterNetChangePublish, a_scenarioPublishOptions.NetChangePublishingEnabled, a_scenarioPublishOptions.NetChangeStoredProcedureName);
        a_summary.ScenarioSettings.SaveSetting(new SettingData(scenarioPublishNetChange), false);

        ScenarioPublishReportSecurity scenarioPublishReportSecurity = new ScenarioPublishReportSecurity(a_scenarioPublishOptions.ReportsUseIntegratedSecurity, a_scenarioPublishOptions.ReportSecurityUserName, a_scenarioPublishOptions.ReportSecurityPassword);
        a_summary.ScenarioSettings.SaveSetting(new SettingData(scenarioPublishReportSecurity), false);

        ScenarioPublishPostPublish scenarioPublishPostPublish = new ScenarioPublishPostPublish(a_scenarioPublishOptions.RunMicrosoftProjectStoredProcedureAfterPublish, a_scenarioPublishOptions.RunProgramAfterPublish, a_scenarioPublishOptions.RunStoredProcedureAfterPublish, a_scenarioPublishOptions.PostPublishStoredProcedureName, a_scenarioPublishOptions.RunProgramPath, a_scenarioPublishOptions.RunProgramCommandLine);
        a_summary.ScenarioSettings.SaveSetting(new SettingData(scenarioPublishPostPublish), false);

        ScenarioPublishAnalytics scenarioPublishAnalytics = new ScenarioPublishAnalytics(a_scenarioPublishOptions.PublishToDashboard);
        a_summary.ScenarioSettings.SaveSetting(new SettingData(scenarioPublishAnalytics), false);

        ScenarioPublishAutomaticSettings scenarioPublishAutomaticSettings = new ScenarioPublishAutomaticSettings(a_scenarioPublishOptions.AutomaticPublish,a_scenarioPublishOptions.AutomaticPublishDelay,a_scenarioPublishOptions.ExportDestination);
        a_summary.ScenarioSettings.SaveSetting(new SettingData(scenarioPublishAutomaticSettings), false);
    }

    /// <summary>
    /// This Guid is used to identify which transmission was last received/processed by this scenario.
    /// It's used when reloading scenarios after a desync to determine which transmissions
    /// in the transmission queue need to be process or discarded.
    /// It might be useful during Undo/Redo also, but it's very likely that all transmissions in the queue are processed.  
    /// </summary>
    internal Guid LastReceivedTransmissionId;

    private bool m_isClient;
    internal long LastReceivedTransmissionTimeTicks;
}