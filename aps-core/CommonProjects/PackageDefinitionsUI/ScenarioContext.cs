using PT.APIDefinitions.RequestsAndResponses;
using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Delegates;
using PT.Common.Http;
using PT.PackageDefinitions;
using PT.PackageDefinitions.Settings;
using PT.PackageDefinitionsUI.DataSources;
using PT.Scheduler;
using PT.Scheduler.TransmissionDispatchingAndReception;
using PT.SchedulerData;
using PT.SchedulerDefinitions;
using PT.SchedulerDefinitions.PermissionTemplates;
using PT.SchedulerDefinitions.UserSettingTemplates;
using PT.Transmissions;
using PT.Transmissions.CTP;
using PT.Transmissions.Interfaces;

namespace PT.PackageDefinitionsUI;

public class ScenarioContext
{
    protected readonly BaseId m_scenarioId;
    protected readonly IUsersInfo m_usersInfo;
    private readonly IClientSession m_clientSession;
    private readonly object m_validatorLock;
    private bool m_undoing;
    private bool m_updatePlantValidator;
    private readonly Dictionary<BaseId, PlantPermissionValidator> m_plantPermissionValidators = new();
    private bool m_readOnly;
    private string m_scenarioName;
    private string m_lastAction;
    private bool m_noActions;
    private DateTimeOffset m_lastActionDateUtc;
    private readonly IImpactAnalyzer m_impactAnalyzer;
    private readonly ScenarioDetailCacheLock m_scenarioDetailCacheLock;
    private readonly ScenarioDataLock m_scenarioDataLock;
    private Guid m_lastUndoableTransmissionGuidClient;
    private Guid m_lastUndoableTransmissionGuidServer;


    public ScenarioContext(IUsersInfo a_usersInfo, IClientSession a_clientSession, Scenario a_s, bool a_isReadOnly, string a_scenarioName, IImpactAnalyzer a_impactAnalyzer)
    {
        m_usersInfo = a_usersInfo;
        m_clientSession = a_clientSession;
        m_scenarioId = a_s.Id;
        m_readOnly = a_isReadOnly;
        m_scenarioName = a_scenarioName;
        m_validatorLock = new object();
        m_impactAnalyzer = a_impactAnalyzer;
        m_scenarioDataLock = new ScenarioDataLock(m_scenarioId);
        m_scenarioDetailCacheLock = new ScenarioDetailCacheLock(m_scenarioDataLock);

        Task.Run(RetrieveLastActionInfo);
    }

    public BaseId ScenarioId => m_scenarioId;
    public string ScenarioName => m_scenarioName;
    public bool ScenarioIsInReadonly => m_readOnly;
    public string LastAction => m_lastAction;
    public DateTimeOffset LastActionDateUtc => m_lastActionDateUtc;
    public bool NoActions => m_noActions;
    public ClientScenarioData ClientScenarioData;
    public ScenarioDataLock DataLock => m_scenarioDataLock;

    public ScenarioDetailCacheLock ScenarioDetailCacheLock
    {
        get
        {
            m_scenarioDetailCacheLock.InitCache();
            return m_scenarioDetailCacheLock;
        }
    }

    public event Action<ScenarioContext, ScenarioDetail.SimulationType, ScenarioBaseT, BaseId, DateTime> SimulationStart;
    public event Action<ScenarioContext> SimulationComplete;
    public event Action<ScenarioContext> SimulationBeginCancellation;
    public event Action<ScenarioContext> SimulationCancelled;
    public event Action<ScenarioContext, SimulationProgress.Status, decimal> SimulationProgressEvent;

    public event Action<ScenarioContext, DateTime> APSClockChanged;

    public event Action<ScenarioContext, ScenarioBaseT, BaseId, DateTime> PublishStart;
    public event Action<ScenarioContext, PublishStatuses.EPublishProgressStep, double> PublishProgress;
    public event Action<ScenarioContext, DateTime> PublishComplete;

    public event Action<ScenarioContext, object> MoveComplete;
    public event Action<ScenarioContext, IScenarioDataChanges> DataChanged;
    public event Action<ScenarioContext, ISettingsManager, string> ScenarioSettingChanged;
    public event Action<ScenarioContext, string> ScenarioNameChanged;

    public event Action<ScenarioContext, PTTransmission, TimeSpan, bool, Exception> TransmissionProcessed;
    public event Action<ScenarioContext, PTTransmission, List<QueuedTransmissionData>> TransmissionReceived;
    public event Action<ScenarioContext, PTTransmission> ScenarioDetailTransmissionProcessedEvent;
    public event Action<ScenarioContext, bool, string> UndoStart;
    public event Action<ScenarioContext, bool> UndoComplete;
    public event Action<ScenarioContext, ScenarioBaseT> UndoableActionProcessed;
    public event Action<ScenarioContext, bool> ScenarioReadonlyChanged;

    public event Action<ScenarioContext, ScenarioBaseT, Ctp, Job, Exception> CtpEvent;

    public event Action<ScenarioContext, UserField.EUDFObjectType> UDFDataChangedEvent;

    /// <summary>
    /// Adds event listeners for a specified Scenario.  This is done each time a Scenario is opened so it is kept up to date in the UI.
    /// </summary>
    public void InitializeListeners(ScenarioEvents a_se)
    {
        a_se.SimulationProgressEvent += new ScenarioEvents.SimulationProgressDelegate(se_SimulationProgressEvent);
        a_se.SimulationCancelled += SeOnSimulationCancelled;
        a_se.SimulationBeginCancellation += SeOnSimulationBeginCancellation;
        a_se.PublishStatusEvent += SeOnPublishStatusEvent; // TODO: Start polling on ExportScenarioEvent? Also, should DeInitializeListeners remove this and other tagged events?
        a_se.MRPStatusUpdateEvent += new ScenarioEvents.MRPStatusUpdateDelegate(se_MRPStatusUpdateEvent);
        a_se.MoveFinishedEvent += new ScenarioEvents.MoveFinishedDelegate(se_MoveFinishedEvent);
        a_se.MoveFailedEvent += new ScenarioEvents.MoveFailedDelegate(se_MoveFailedEvent);
        a_se.DesynchronizedScenarioEvent += SeOnDesynchronizedScenarioEvent;
        a_se.BeginPromoteScenarioEvent += SeOnBeginPromoteScenarioEvent;
        a_se.ScenarioDataChangesEvent += SeOnScenarioDataChangesEvent; // not in DeInit
        a_se.TransmissionReceivedEvent += SeOnTransmissionReceivedEvent;
        a_se.TransmissionProcessedEvent += SeOnTransmissionProcessedEvent;
        a_se.ScenarioOptionsChangedEvent += SeOnScenarioOptionsChangedEvent; // not in DeInit
        a_se.ScenarioReadonlyChangeEvent += SeOnScenarioReadonlyChangeEvent;
        a_se.ChangeEvent += SeOnChangeEvent;
        a_se.CTPEvent += SeOnCTPEvent;
        a_se.ScenarioDetailTransmissionProcessedEvent += SeOnScenarioDetailTransmissionProcessedEvent;
        a_se.KPIChangedEvent += SeOnKPIChangedEvent;
        a_se.UDFDataChangedEvent += SeOnUDFDataChangedEvent;
        m_usersInfo.UserDataChanged += UsersInfoOnUserDataChanged;
    }

    private void SeOnBeginPromoteScenarioEvent(ScenarioEvents a_se, ScenarioUndoEvents a_sue, BaseId a_currentProdId, BaseId a_prodId, bool a_autoDelete)
    {
        FireScenarioPromotionEvent(a_currentProdId, a_prodId);

        if (a_autoDelete && ScenarioId == a_prodId)
        {
            DeInitializeListeners(a_se, a_sue);
        }
    }

    private void SeOnSimulationBeginCancellation()
    {
        SimulationBeginCancellation?.Invoke(this);
    }

    private void SeOnUDFDataChangedEvent(UserField.EUDFObjectType a_objectType)
    {
        UDFDataChangedEvent?.Invoke(this, a_objectType);
    }

    private void SeOnSimulationCancelled()
    {
        SimulationCancelled?.Invoke(this);
    }

    private void SeOnKPIChangedEvent(KpiController a_kpicontroller, ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simtype, PTTransmission a_t)
    {
        KpiSnapshotsUpdated?.Invoke();
    }

    private void SeOnScenarioDetailTransmissionProcessedEvent(ScenarioBaseT a_t)
    {
        ScenarioDetailTransmissionProcessedEvent?.Invoke(this, a_t);
    }

    /// <summary>
    /// Removes ScenarioEvent listeners when a scenario is being unloaded or deleted.
    /// </summary>
    private void DeInitializeListeners(ScenarioEvents a_se, ScenarioUndoEvents a_sue)
    {
        a_se.SimulationCancelled -= SeOnSimulationCancelled;
        a_se.SimulationBeginCancellation -= SeOnSimulationBeginCancellation;
        a_se.SimulationProgressEvent -= new ScenarioEvents.SimulationProgressDelegate(se_SimulationProgressEvent);
        a_se.MRPStatusUpdateEvent -= new ScenarioEvents.MRPStatusUpdateDelegate(se_MRPStatusUpdateEvent);
        a_se.MoveFinishedEvent -= new ScenarioEvents.MoveFinishedDelegate(se_MoveFinishedEvent);
        a_se.MoveFailedEvent -= new ScenarioEvents.MoveFailedDelegate(se_MoveFailedEvent);
        a_se.DesynchronizedScenarioEvent -= SeOnDesynchronizedScenarioEvent;
        a_se.TransmissionReceivedEvent -= SeOnTransmissionReceivedEvent;
        a_se.TransmissionProcessedEvent -= SeOnTransmissionProcessedEvent;
        a_se.ScenarioReadonlyChangeEvent -= SeOnScenarioReadonlyChangeEvent;
        a_se.ChangeEvent -= SeOnChangeEvent;
        a_se.CTPEvent -= SeOnCTPEvent;
        a_se.ScenarioDetailTransmissionProcessedEvent -= SeOnScenarioDetailTransmissionProcessedEvent;
        a_se.KPIChangedEvent -= SeOnKPIChangedEvent;
        a_se.UDFDataChangedEvent -= SeOnUDFDataChangedEvent;
        m_usersInfo.UserDataChanged -= UsersInfoOnUserDataChanged;

        a_sue.UndoBeginEvent -= UeOnUndoBeginEvent;
        a_sue.UndoEndEvent -= UeOnUndoEndEvent;
        a_sue.UndoSetChangedEvent -= UeOnUndoSetChangedEvent;
    }

    private void UsersInfoOnUserDataChanged(IScenarioDataChanges a_dataChanges)
    {
        ClientScenarioData.UserData.SignalDataChanged(a_dataChanges);
    }

    private void SeOnCTPEvent(ScenarioBaseT a_t, Ctp a_ctp, Job a_job, Exception a_ctpException)
    {
        if (m_undoing)
        {
            return;
        }

        CtpEvent?.Invoke(this, a_t, a_ctp, a_job, a_ctpException);
    }

    private void FireSimulationComplete()
    {
        if (m_undoing)
        {
            return;
        }

        SimulationComplete?.Invoke(this);
    }

    private void SeOnChangeEvent(ScenarioSummary a_ss, ScenarioChangeT a_t, Scenario a_s)
    {
        if (a_ss.Name != ScenarioName)
        {
            m_scenarioName = a_ss.Name;
            ScenarioNameChanged?.Invoke(this, ScenarioName);
        }
    }

    private void SeOnScenarioReadonlyChangeEvent(bool a_readonly)
    {
        m_readOnly = a_readonly;
        ScenarioReadonlyChanged?.Invoke(this, a_readonly);
    }

    public void InitializeData(ScenarioDetail a_sd)
    {
        SystemSettings = a_sd.ScenarioOptions;
        m_impactAnalyzer.InitializeScenario(a_sd);
    }

    private void SeOnScenarioOptionsChangedEvent(SystemOptionsT a_systemOptionsT, ScenarioDetail a_sd)
    {
        SystemSettings = a_sd.ScenarioOptions;
        SystemSettingsChanged?.Invoke(this, SystemSettings);
    }

    public ScenarioOptions SystemSettings;
    public event Action<ScenarioContext, ScenarioOptions> SystemSettingsChanged;

    public void InitializeUndoEvents(ScenarioUndoEvents a_ue)
    {
        a_ue.UndoBeginEvent += UeOnUndoBeginEvent;
        a_ue.UndoEndEvent += UeOnUndoEndEvent;
        a_ue.UndoSetChangedEvent += UeOnUndoSetChangedEvent;
    }

    private void UeOnUndoSetChangedEvent(BaseId a_scenarioId, ScenarioBaseT a_t)
    {
        m_lastUndoableTransmissionGuidClient = a_t.TransmissionId;
        Task.Run(RetrieveLastActionInfo);
        UndoableActionProcessed?.Invoke(this, a_t);
    }

    private void UeOnUndoEndEvent(ScenarioDetail a_sd, UserManager a_userManager, bool a_success)
    {
        m_undoing = false;
        //Update data that must be up to date before other UI elements access them
        m_impactAnalyzer.SimulationComplete(a_sd);
        User user = a_userManager.GetById(SystemController.CurrentUserId);
        UserPermissionSet permissions = (UserPermissionSet)a_userManager.GetUserPermissionSetById(user.UserPermissionSetId).Clone();
        PlantPermissionSet plantPermissions = (PlantPermissionSet)a_userManager.GetPlantPermissionSetById(user.PlantPermissionsId).Clone();
        UpdateUserPermissions(permissions);
        UpdatePlantPermissions(plantPermissions, a_sd.PlantManager.Select(p => p.Id));

        using (a_sd.Scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary scenarioSummary))
        {
            m_scenarioName = scenarioSummary.Name;
        }
        //Clears cached ClientScenario before notifying listeners of undo complete

        UndoComplete?.Invoke(this, a_success);
        APSClockChanged?.Invoke(this, a_sd.ClockDate);
    }

    private void UeOnUndoBeginEvent(BaseId a_scenarioId,bool a_isUndo, string a_description)
    {
        m_undoing = true;
        UndoStart?.Invoke(this, a_isUndo, a_description);
        m_impactAnalyzer.ResetData(a_scenarioId);
    }

    private void SeOnTransmissionProcessedEvent(PTTransmission a_t, TimeSpan a_processingtime, Exception a_e)
    {
        if (a_t is ScenarioClockAdvanceT clockAdvanceT)
        {
            APSClockChanged?.Invoke(this, clockAdvanceT.time);
        }

        if (m_simTransmissionNbr == a_t.TransmissionNbr)
        {
            FireSimulationComplete();
            SignalSimulationCompleted();
        }

        TransmissionProcessed?.Invoke(this, a_t, a_processingtime, m_simTransmissionNbrInitial == a_t.TransmissionNbr, a_e);
    }

    private void SeOnTransmissionReceivedEvent(PTTransmission a_t, List<QueuedTransmissionData> a_queuedTransmissionDescriptions)
    {
        TransmissionReceived?.Invoke(this, a_t, a_queuedTransmissionDescriptions);
    }

    public ScenarioPermissionSettings PermissionSettings = new();

    public virtual void InitializeSettingsListeners(ISettingsManager a_settingsManager)
    {
        PermissionSettings = a_settingsManager.LoadSetting(PermissionSettings);
        a_settingsManager.SettingSavedEvent += ScenarioSettingsOnSettingSavedEvent;
    }

    public void FireScenarioSettingsChangedEvent(ISettingsManager a_settingsManager, string a_settingKey)
    {
        ScenarioSettingChanged?.Invoke(this, a_settingsManager, a_settingKey);
    }

    protected virtual async void ScenarioSettingsOnSettingSavedEvent(ISettingsManager a_settingsManager, string a_settingKey)
    {
        if (a_settingKey == PermissionSettings.SettingKey)
        {
            using (BackgroundLock asyncLock = new(m_scenarioId))
            {
                await asyncLock.RunLockCodeBackground(ReloadScenarioPermissions);
                if (asyncLock.Status is BackgroundLock.EResultStatus.Error or BackgroundLock.EResultStatus.Canceled) { }
            }

        }

        FireScenarioSettingsChangedEvent(a_settingsManager, a_settingKey);
    }

    private async void SeOnScenarioDataChangesEvent(IScenarioDataChanges a_changes)
    {
        if (m_undoing)
        {
            return;
        }

        if (a_changes.PlantChanges.TotalAddedObjects > 0 || a_changes.PlantChanges.TotalDeletedObjects > 0)
        {
            using (BackgroundLock bg = new(ScenarioId))
            {
                await bg.RunLockCodeBackground(AddNewPlantValidators);
            }
        }

        ClientScenarioData.SignalDataChanges(a_changes);
        DataChanged?.Invoke(this, a_changes);
    }

    private void AddNewPlantValidators(Scenario a_s, ScenarioDetail a_sd, UserManager a_um, params object[] a_params)
    {
        lock (m_validatorLock)
        {
            if (m_updatePlantValidator)
            {
                return;
            }

            m_updatePlantValidator = true;
        }

        User user = a_um.GetById(SystemController.CurrentUserId);
        PlantPermissionSet plantPermissions = (PlantPermissionSet)a_um.GetPlantPermissionSetById(user.PlantPermissionsId).Clone();
        UpdatePlantPermissions(plantPermissions, a_sd.PlantManager.Select(p => p.Id));

        lock (m_validatorLock)
        {
            m_updatePlantValidator = false;
        }
    }

    public delegate void DesynchronizedScenarioDelegate(Guid a_transmissionId, string a_description, BaseId a_scenarioId);

    public event DesynchronizedScenarioDelegate DesynchronizedScenario;

    private void SeOnDesynchronizedScenarioEvent(Guid a_transmissionId, string a_description, BaseId a_scenarioid)
    {
        DesynchronizedScenario?.Invoke(a_transmissionId, a_description, a_scenarioid);
    }

    public delegate void ProductionScenarioConversionDelegate(BaseId a_currentProdId, BaseId a_prodId);

    public event ProductionScenarioConversionDelegate BeginScenarioConversionEvent;

    public void FireScenarioPromotionEvent(BaseId a_currentProdId, BaseId a_promotedProdScenarioId)
    {
        BeginScenarioConversionEvent?.Invoke(a_currentProdId, a_promotedProdScenarioId);
    }

    /// <summary>
    /// Unsubscribes the various event handlers to the ScenarioEvents and the ScenarioUndoEvents,
    /// and also disposes the scenarioDetailCacheLock
    /// </summary>
    /// <param name="a_s"></param>
    /// <param name="a_se"></param>
    /// <param name="a_sue"></param>
    public void RemoveScenario(Scenario a_s, ScenarioEvents a_se, ScenarioUndoEvents a_sue)
    {
        DeInitializeListeners(a_se, a_sue);
        m_scenarioDetailCacheLock.Dispose();
    }

    /// <summary>
    /// Unsubscribes the various event handlers to the ScenarioEvents and the ScenarioUndoEvents,
    /// without disposing the scenarioDetailCacheLock
    /// </summary>
    /// <param name="a_s"></param>
    /// <param name="a_se"></param>
    /// <param name="a_sue"></param>
    public void UnloadScenario(Scenario a_s, ScenarioEvents a_se, ScenarioUndoEvents a_sue)
    {
        DeInitializeListeners(a_se, a_sue);
    }

    private void se_MoveFinishedEvent(MoveResult a_result)
    {
        if (m_undoing)
        {
            return;
        }

        MoveComplete?.Invoke(this, a_result);
    }

    private void se_MoveFailedEvent(MoveResult a_result, ScenarioDetail a_sd)
    {
        if (m_undoing)
        {
            return;
        }

        MoveComplete?.Invoke(this, a_result);
    }

    #region SimulationProgress
    // this is set to true right before the last MRP Optimize. 
    private bool m_finalMRPOptimize;

    // this is true between MRP_Start and MRP_Complete or MRP_Exception
    private bool m_runningMRP;
    private bool m_mrpStarting;
    private ulong m_simTransmissionNbr;

    private void se_SimulationProgressEvent(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, long a_simNbr, decimal a_percentComplete, SimulationProgress.Status a_status)
    {
        if (m_undoing)
        {
            return;
        }

        if (a_status == SimulationProgress.Status.Initializing)
        {
            if (m_mrpStarting)
            {
                m_mrpStarting = false;
            }
            else if (m_runningMRP)
            {
                return;
            }

            m_simTransmissionNbrInitial = m_simTransmissionNbr = a_t.TransmissionNbr;
            SimulationStart?.Invoke(this, a_simType, a_t, a_t.Instigator, a_t.TimeStamp.ToDateTime());
            SignalSimulationStarted();
        }
        else if (a_status == SimulationProgress.Status.PostSimulationWorkComplete)
        {
            // if running MRP, skip these unless it's the last Optimize and status is done
            if (m_runningMRP && !m_finalMRPOptimize)
            {
                return;
            }

            m_simTransmissionNbr = 0;
            m_impactAnalyzer.SimulationComplete(a_sd);
            FireSimulationComplete();
            SignalSimulationCompleted();
        }
        else
        {
            SimulationProgressEvent?.Invoke(this, a_status, a_percentComplete);
        }
    }

    private void SeOnPublishStatusEvent(PublishStatusMessageT a_t)
    {
        if (m_undoing)
        {
            return;
        }

        if (a_t.ProgressStep == PublishStatuses.EPublishProgressStep.Started)
        {
            //Transmissions from the server should always be UTC since server time is UTC, so convert to display time
            PublishStart?.Invoke(this, a_t, a_t.Instigator, a_t.TimeStamp.ToDisplayTime().ToDateTime());
        }
        else
        {
            PublishProgress?.Invoke(this, a_t.ProgressStep, a_t.ProgressPercent);
            if (a_t.ProgressStep == PublishStatuses.EPublishProgressStep.Complete)
            {
                PublishComplete?.Invoke(this, PTDateTime.UtcNow.ToDateTime());
                // TODO: Update/Check all the references to this event to make sure 
                // that their event handlers expect the times to come in as UTC
            }
        }
    }

    private void se_MRPStatusUpdateEvent(SimulationProgress.Status a_statusCode, object[] a_detailParams)
    {
        switch (a_statusCode)
        {
            case SimulationProgress.Status.MRP_Start:
                m_mrpStarting = true;
                m_runningMRP = true;
                break;
            case SimulationProgress.Status.MRP_StartOptimizeToSetNeedDates:
                m_finalMRPOptimize = false;
                break;
            case SimulationProgress.Status.MRP_Complete:
                m_runningMRP = false;
                m_finalMRPOptimize = false;
                return;
            case SimulationProgress.Status.MRP_StartFinalOptimize:
                m_finalMRPOptimize = true;
                break;
            case SimulationProgress.Status.Exception:
            case SimulationProgress.Status.MRP_FinishedWithException:
                m_runningMRP = false;
                m_finalMRPOptimize = false;
                break;
        }
    }
    #endregion SimulationProgress

    #region Permissions
    private UserPermissionValidator m_permissionValidator;
    private ulong m_simTransmissionNbrInitial;
    private static readonly PlantPermissionValidator s_alwaysGrantPlantPermissionsValidator = new PlantPermissionValidator(new HashSet<string>(), true);  

    public UserPermissionValidator Validator => m_permissionValidator;

    public event VoidDelegate KpiSnapshotsUpdated;

    public void UpdateUserPermissions(UserPermissionSet a_permissions)
    {
        m_permissionValidator = a_permissions.GetPermissionsValidator();
    }

    private void ReloadScenarioPermissions(ScenarioSummary a_ss, params object[] a_params)
    {
        PermissionSettings = a_ss.ScenarioSettings.LoadSetting(PermissionSettings);
    }

    public PlantPermissionValidator GetPlantValidator(BaseId a_plantId)
    {
        lock (m_validatorLock)
        {
            if (m_plantPermissionValidators.TryGetValue(a_plantId, out PlantPermissionValidator ppv))
            {
                return ppv;
            }
        }

        return s_alwaysGrantPlantPermissionsValidator;
    }

    public void UpdatePlantPermissions(PlantPermissionSet a_plantPermissions, IEnumerable<BaseId> a_plantIds)
    {
        m_plantPermissionValidators.Clear();

        foreach (BaseId plantId in a_plantIds)
        {
            m_plantPermissionValidators.Add(plantId, a_plantPermissions.GetPlantPermissionsValidator(plantId));
        }
    }
    #endregion Permissions
    /// <summary>
    /// The server and the client may not process the same transmission at the same time, (i.e. one may be ahead of other). This flag is to indicate that the last processed transmission information maybe outdated
    /// and <see cref="RetrieveLastActionInfo"/> need to be called to retrieve the latest information on the last action from the server.
    /// </summary>
    public bool IsFetchRequired => m_lastUndoableTransmissionGuidClient != m_lastUndoableTransmissionGuidServer;
    /// <summary>
    /// Retrieves the last action performed in the scenario from the server
    /// </summary>
    public void RetrieveLastActionInfo()
    {
        try
        {
            ApsWebServiceScenarioRequest request = new ApsWebServiceScenarioRequest();
            request.ScenarioId = m_scenarioId.Value;

            GetScenarioLastActionInfoResponse response = m_clientSession.MakeGetRequest<GetScenarioLastActionInfoResponse>("GetLastScenarioActions", "api/ScenarioActions", new GetParam { Name = "a_scenarioId", Value = m_scenarioId.ToString() });

            m_lastAction = response.LastActionInfo.Localize();
            m_lastActionDateUtc = new DateTime(response.LastActionTicks).ToDateTimeOffsetUtc();
            m_noActions = response.HasLastActions;
            m_lastUndoableTransmissionGuidServer = response.LastActionTransmissionGuid;

            if (m_lastUndoableTransmissionGuidClient == Guid.Empty)
            {
                //Only true if this is the first time m_lastUndoableTransmissionGuidClient since we logged in
                m_lastUndoableTransmissionGuidClient = m_lastUndoableTransmissionGuidServer;
            }
        }
        catch
        {
            // ignored
        }
    }

    public void BindClientScenarioData(ClientScenarioData a_clientScenarioData)
    {
        ClientScenarioData = a_clientScenarioData;
    }

    public void SignalSimulationStarted()
    {
        ClientScenarioData.SignalSimulationStarted();
    }

    public void SignalSimulationCompleted()
    {
        ClientScenarioData.SignalSimulationCompleted();
    }

    public void SignalScenarioActivated()
    {
        ClientScenarioData.SignalScenarioActivated();
    }
}