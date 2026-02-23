using System.Windows.Forms;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.APSCommon.Windows;
using PT.Common.Delegates;
using PT.Common.Localization;
using PT.PackageDefinitions;
using PT.PackageDefinitions.PackageInterfaces;
using PT.PackageDefinitions.Settings;
using PT.PackageDefinitionsUI;
using PT.PackageDefinitionsUI.DataSources;
using PT.Scheduler;
using PT.Scheduler.TransmissionDispatchingAndReception;
using PT.SchedulerData;
using PT.SchedulerDefinitions;
using PT.SchedulerDefinitions.PermissionTemplates;
using PT.SchedulerDefinitions.UserSettingTemplates;
using PT.Transmissions;
using PT.Transmissions.CTP;
using PT.UIDefinitions.Interfaces;

namespace PT.UI.Scenarios;

public class SingleScenarioController : IScenarioController, IScenarioInfo
{
    private readonly IMainForm m_mainForm;
    private readonly IPackageManagerUI m_pm;
    private readonly IImpactAnalyzer m_impactAnalyzer;
    private readonly Control m_invokeControl;
    private readonly IMessageProvider m_messageProvider;
    private ScenarioContext m_scenarioContext;

    public SingleScenarioController(IMainForm a_mainForm, IPackageManagerUI a_pm, IImpactAnalyzer a_impactAnalyzer)
    {
        m_mainForm = a_mainForm;
        m_pm = a_pm;
        m_impactAnalyzer = a_impactAnalyzer;
        m_invokeControl = a_mainForm.GetOwnerForm();
        m_messageProvider = a_mainForm.MessageProvider;
    }

    public async void LoadScenarioData()
    {
        m_initializing = true;
        using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm)) //Wait until a lock can be made.  Must open here.
        {
            //sm.ScenarioReplacedEvent += new ScenarioManager.ScenarioReplacedDelegate(ReceiveScenarioReplaceTransmission);
            sm.ScenarioConversionCompleteEvent += SmScenarioConversionCompleteEvent; 
           
            using (SystemController.Sys.UsersLock.EnterRead(out UserManager um))
            {
                User user = um.GetById(SystemController.CurrentUserId);
                UserPermissionSet permissions = (UserPermissionSet)um.GetUserPermissionSetById(user.UserPermissionSetId).Clone();
                PlantPermissionSet plantPermissions = (PlantPermissionSet)um.GetPlantPermissionSetById(user.PlantPermissionsId).Clone();

                Scenario s = sm.GetFirstProductionScenario();
                InitScenario(s);

                using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                {
                    m_inReadonly = s.DataReadonly;
                    m_scenarioContext = new ScenarioContext(m_mainForm.UsersInfo, m_mainForm.ClientSession, s, s.DataReadonly, ss.Name,  m_impactAnalyzer);
                    ClientScenarioData dataSource = new (m_pm, m_mainForm, this, m_scenarioContext.ScenarioDetailCacheLock, s.Id);
                    m_scenarioContext.BindClientScenarioData(dataSource);
                    m_scenarioContext.InitializeSettingsListeners(ss.ScenarioSettings);
                }

                m_scenarioContext.MoveComplete += ContextOnMoveComplete;
                m_scenarioContext.SimulationStart += ContextOnSimulationStart;
                m_scenarioContext.SimulationComplete += ContextOnSimulationCompleteAsync;
                m_scenarioContext.SimulationBeginCancellation += ContextOnSimulationCancelBeginAsync;
                m_scenarioContext.SimulationCancelled += ContextOnSimulationCancelledAsync;
                m_scenarioContext.SimulationProgressEvent += ScenarioContextOnSimulationProgressEvent;

                m_scenarioContext.APSClockChanged += ScenarioContextOnAPSClockChanged;

                m_scenarioContext.PublishStart += ScenarioContextOnPublishStart;
                m_scenarioContext.PublishProgress += ScenarioContextOnPublishProgress;
                m_scenarioContext.PublishComplete += ScenarioContextOnPublishComplete;

                m_scenarioContext.DataChanged += ScenarioContextOnDataChanged;
                m_scenarioContext.ScenarioSettingChanged += ScenarioContextOnScenarioSettingChanged;
                m_scenarioContext.TransmissionProcessed += ScenarioContextOnTransmissionProcessed;
                m_scenarioContext.TransmissionReceived += ScenarioContextOnTransmissionReceived;
                m_scenarioContext.ScenarioDetailTransmissionProcessedEvent += ScenarioContextOnScenarioDetailTransmissionProcessedEvent;
                m_scenarioContext.UndoStart += ScenarioContextOnUndoStart;
                m_scenarioContext.UndoComplete += ScenarioContextOnUndoComplete;
                m_scenarioContext.UndoableActionProcessed += ScenarioContextOnUndoableActionProcessed;
                m_scenarioContext.ScenarioReadonlyChanged += ScenarioContextOnScenarioReadonlyChanged;
                m_scenarioContext.SystemSettingsChanged += ScenarioContextOnSystemSettingsChanged;
                m_scenarioContext.UDFDataChangedEvent += ScenarioContextOnUDFDataChangedEvent;

                m_scenarioContext.CtpEvent += ScenarioContextOnCtpEvent;

                m_scenarioContext.KpiSnapshotsUpdated += ScenarioContextOnKpiSnapshotsUpdated;

                m_scenarioContext.DesynchronizedScenario += ScenarioContextOnDesynchronize;
                m_scenarioContext.BeginScenarioConversionEvent += BeginScenarioConversionEvent;

                using (s.ScenarioEventsLock.EnterRead(out ScenarioEvents se))
                {
                    m_scenarioContext.InitializeListeners(se);
                }

                using (s.ScenarioUndoEventsLock.EnterRead(out ScenarioUndoEvents sue))
                {
                    m_scenarioContext.InitializeUndoEvents(sue);
                }

                using (s.ScenarioDetailLock.EnterRead(out ScenarioDetail sd))
                {
                    m_scenarioContext.InitializeData(sd);
                    UpdateUserPermissions(permissions, plantPermissions, sd);
                }

                using (SystemController.Sys.UserManagerEventsLock.EnterRead(out UserManagerEvents ume))
                {
                    ume.ScenarioDataChangesEvent += UmeOnScenarioDataChangesEvent;
                }
            }

            m_initializing = false;
        }

        //If the user can only see the published and/or whatif scenarios and there isn't one then there will be no scenarios visible
        if (m_scenarioContext == null)
        {
            m_messageProvider.ShowMessageBox(new PTMessage(Localizer.GetErrorString("2702"), "No visible scenarios".Localize()) { Classification = PTMessage.EMessageClassification.Information }, true);
        }
        else
        {
            using (BackgroundLock asyncLock = new (ScenarioId))
            {
                await asyncLock.RunLockCode(m_invokeControl, FireScenarioActivated);
                if (asyncLock.Status is BackgroundLock.EResultStatus.Error or BackgroundLock.EResultStatus.Canceled) { }
            }
        }
    }

    private void SmScenarioConversionCompleteEvent(BaseId a_scenarioId, BaseId a_instigator)
    {
        ScenarioConversionComplete?.Invoke(a_scenarioId, a_instigator);
    }

    private void BeginScenarioConversionEvent(BaseId a_currentProdId, BaseId a_promotedWhatIfScenarioId)
    {
        BeginScenarioConversion?.Invoke(a_currentProdId, a_promotedWhatIfScenarioId);   
    }

    private void ScenarioContextOnUDFDataChangedEvent(ScenarioContext a_sc, UserField.EUDFObjectType a_objectType)
    {
        UDFDataChangesEvent?.Invoke(a_objectType);
    }

    private void ScenarioContextOnKpiSnapshotsUpdated()
    {
        KpiSnapshotsUpdated?.Invoke();
    }

    private void ScenarioContextOnScenarioDetailTransmissionProcessedEvent(ScenarioContext a_context, PTTransmission a_t)
    {
        ScenarioDetailTransmissionProcessed?.Invoke(a_t);
    }

    private void ScenarioContextOnCtpEvent(ScenarioContext a_context, ScenarioBaseT a_t, Ctp a_ctp, Job a_job, Exception a_exception)
    {
        CtpEvent?.Invoke(a_t, a_ctp, a_job, a_exception);
    }

    private void ScenarioContextOnAPSClockChanged(ScenarioContext a_arg1, DateTime a_arg2)
    {
        ClockDateChanged?.Invoke(a_arg2);
    }

    private void ScenarioContextOnScenarioReadonlyChanged(ScenarioContext a_context, bool a_readonly)
    {
        m_inReadonly = a_readonly;
        ScenarioDataReadonlyChanged?.Invoke(a_readonly);
    }

    private void UmeOnScenarioDataChangesEvent(IScenarioDataChanges a_changes)
    {
        DataChanged?.Invoke(a_changes);
    }

    private void ScenarioContextOnUndoableActionProcessed(ScenarioContext a_context, ScenarioBaseT a_scenarioBaseT)
    {
        UndoableActionProcessed?.Invoke(a_context.ScenarioId, a_scenarioBaseT);
    }

    private void ScenarioContextOnUndoComplete(ScenarioContext a_context, bool a_success)
    {
        ScenarioDataUnlocked?.Invoke();
        m_dataReadOnly = false;
        UndoComplete?.Invoke(a_success);
    }

    private void ScenarioContextOnUndoStart(ScenarioContext a_context,bool a_isUndo, string a_description)
    {
        ScenarioDataLocked?.Invoke();
        m_dataReadOnly = true;
        UndoStart?.Invoke(a_isUndo, a_description);
    }

    private void ScenarioContextOnTransmissionProcessed(ScenarioContext a_arg1, PTTransmission a_t, TimeSpan a_processingSpan, bool a_simTransmission, Exception a_e)
    {
        TransmissionProcessed?.Invoke(a_t, a_processingSpan, a_simTransmission, a_e);
    }

    private void ScenarioContextOnTransmissionReceived(ScenarioContext a_context, PTTransmission a_t, List<QueuedTransmissionData> a_queuedTransmissionDescriptions)
    {
        TransmissionReceived?.Invoke(a_t, a_queuedTransmissionDescriptions);
    }

    private void FireScenarioActivated(Scenario a_s, ScenarioDetail a_sd, ScenarioEvents a_se, params object[] a_params)
    {
        ScenarioActivated?.Invoke(a_s, a_sd, a_se);
    }

    private void ScenarioContextOnScenarioSettingChanged(ScenarioContext a_sc, ISettingsManager a_scenarioSettingsManager, string a_settingKey)
    {
        ScenarioSettingChanged?.Invoke(a_scenarioSettingsManager, a_settingKey, a_sc.ScenarioId);
    }

    private void ScenarioContextOnSimulationProgressEvent(ScenarioContext a_context, SimulationProgress.Status a_status, decimal a_progressPercent)
    {
        SimulationProgress?.Invoke(a_status, a_progressPercent);
    }

    private void ScenarioContextOnDataChanged(ScenarioContext a_context, IScenarioDataChanges a_changes)
    {
        DataChanged?.Invoke(a_changes);
    }

    private void ScenarioContextOnDesynchronize(Guid a_transmissionId, string a_description, BaseId a_scenarioId)
    {
        ScenarioDesynced?.Invoke(a_transmissionId, a_description, a_scenarioId);
    } 
    private static void InitScenario(Scenario a_scenario)
    {
        //Set the Main thread for all scenario locks to avoid threadlocks.
        a_scenario.ScenarioDetailLock.SetMainThread();
        a_scenario.ScenarioEventsLock.SetMainThread();
        a_scenario.ScenarioSummaryLock.SetMainThread();
        a_scenario.ScenarioUndoEventsLock.SetMainThread();
    }
    private void ContextOnSimulationCancelledAsync(ScenarioContext a_context)
    {
        ScenarioDataUnlocked?.Invoke();
        m_dataReadOnly = false;
        SimulationCancelled?.Invoke();
    }
    private void ContextOnSimulationCancelBeginAsync(ScenarioContext a_context)
    {
        ScenarioDataLocked?.Invoke();
        m_dataReadOnly = true;
        SimulationBeginCancellation?.Invoke();
    }

    private void ContextOnSimulationCompleteAsync(ScenarioContext a_context)
    {
        ScenarioDataUnlocked?.Invoke();
        m_dataReadOnly = false;
        SimulationComplete?.Invoke();
    }

    private void ContextOnSimulationStart(ScenarioContext a_context, ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, BaseId a_instigatorId, DateTime a_timeStarted)
    {
        ScenarioDataLocked?.Invoke();
        m_dataReadOnly = true;
        SimulationStart?.Invoke(a_simType, a_t, a_instigatorId, a_timeStarted);
    }

    private void ContextOnMoveComplete(ScenarioContext a_context, object a_moveResult)
    {
        ScenarioDataUnlocked?.Invoke();
        m_dataReadOnly = false;
        MoveComplete?.Invoke(a_moveResult);
    }

    private void ScenarioContextOnPublishComplete(ScenarioContext a_context, DateTime a_timeStamp)
    {
        PublishComplete?.Invoke(a_timeStamp);
    }

    private void ScenarioContextOnPublishProgress(ScenarioContext a_context, PublishStatuses.EPublishProgressStep a_progressStatus, double a_percentComplete)
    {
        PublishProgress?.Invoke(a_progressStatus, a_percentComplete);
    }

    private void ScenarioContextOnPublishStart(ScenarioContext a_context, ScenarioBaseT a_t, BaseId a_instigator, DateTime a_timeStamp)
    {
        PublishStart?.Invoke(a_t, a_instigator, a_timeStamp);
    }

    public IScenarioInfo GetScenarioInfo()
    {
        return this;
    }

    public void UpdateUserPermissions(UserPermissionSet a_permissions, PlantPermissionSet a_plantPermissions, ScenarioDetail a_sd)
    {
        m_scenarioContext.UpdateUserPermissions(a_permissions);
        m_scenarioContext.UpdatePlantPermissions(a_plantPermissions, a_sd.PlantManager.Select(p => p.Id));

        if (!m_initializing)
        {
            UserPermissionsUpdated?.Invoke();
        }
    }

    public void UnloadAllScenarios()
    {
        m_scenarioContext.MoveComplete -= ContextOnMoveComplete;
        m_scenarioContext.SimulationStart -= ContextOnSimulationStart;
        m_scenarioContext.SimulationComplete -= ContextOnSimulationCompleteAsync;
        m_scenarioContext.SimulationCancelled -= ContextOnSimulationCancelledAsync;
        m_scenarioContext.SimulationBeginCancellation -= ContextOnSimulationCancelBeginAsync;
        m_scenarioContext.SimulationProgressEvent -= ScenarioContextOnSimulationProgressEvent;

        m_scenarioContext.APSClockChanged -= ScenarioContextOnAPSClockChanged;

        m_scenarioContext.PublishStart -= ScenarioContextOnPublishStart;
        m_scenarioContext.PublishProgress -= ScenarioContextOnPublishProgress;
        m_scenarioContext.PublishComplete -= ScenarioContextOnPublishComplete;

        m_scenarioContext.DataChanged -= ScenarioContextOnDataChanged;
        m_scenarioContext.ScenarioSettingChanged -= ScenarioContextOnScenarioSettingChanged;
        m_scenarioContext.TransmissionProcessed -= ScenarioContextOnTransmissionProcessed;
        m_scenarioContext.TransmissionReceived -= ScenarioContextOnTransmissionReceived;
        m_scenarioContext.UndoStart -= ScenarioContextOnUndoStart;
        m_scenarioContext.UndoComplete -= ScenarioContextOnUndoComplete;
        m_scenarioContext.UndoableActionProcessed -= ScenarioContextOnUndoableActionProcessed;
        m_scenarioContext.ScenarioReadonlyChanged -= ScenarioContextOnScenarioReadonlyChanged;
        m_scenarioContext.SystemSettingsChanged -= ScenarioContextOnSystemSettingsChanged;
        m_scenarioContext.UDFDataChangedEvent -= ScenarioContextOnUDFDataChangedEvent;

        m_scenarioContext.CtpEvent -= ScenarioContextOnCtpEvent;

        m_scenarioContext.KpiSnapshotsUpdated -= KpiSnapshotsUpdated;
        m_scenarioContext.DesynchronizedScenario += ScenarioContextOnDesynchronize;


        using (SystemController.Sys.UserManagerEventsLock.EnterRead(out UserManagerEvents ume))
        {
            ume.ScenarioDataChangesEvent -= UmeOnScenarioDataChangesEvent;
        }
        //TODO: Possibly dispose scenario context
    }

    private void ScenarioContextOnSystemSettingsChanged(ScenarioContext a_context, ScenarioOptions a_systemSettings)
    {
        SystemSettingsChanged?.Invoke(a_systemSettings);
    }

    #region Permissions
    public UserPermissionValidator Validator => m_scenarioContext.Validator;

    public PlantPermissionValidator GetPlantValidator(BaseId a_plantId)
    {
        return m_scenarioContext.GetPlantValidator(a_plantId);
    }

    public bool PlantPermissionAutoGrantProperty
    {
        get
        {
            using (SystemController.Sys.UsersLock.EnterRead(out UserManager a_um))
            {
                User user = a_um.GetById(SystemController.CurrentUserId);

                PlantPermissionSet plantPermissions = (PlantPermissionSet)a_um.GetPlantPermissionSetById(user.PlantPermissionsId).Clone();
                return plantPermissions.AutoGrantNewPermissions;
            }
        }
    }
    #endregion

    public event Action<ScenarioDetail.SimulationType, ScenarioBaseT, BaseId, DateTime> SimulationStart;
    public event Action SimulationComplete;
    public event Action SimulationBeginCancellation;
    public event Action SimulationCancelled;
    public event Action<SimulationProgress.Status, decimal> SimulationProgress;
    public event Action<DateTime> ClockDateChanged;

    public event Action<ScenarioBaseT, BaseId, DateTime> PublishStart;
    public event Action<PublishStatuses.EPublishProgressStep, double> PublishProgress;
    public event Action<DateTime> PublishComplete;

    public event Action<object> MoveComplete;
    public event Action<IScenarioDataChanges> DataChanged;
    public event Action<Scenario, ScenarioEvents> ScenarioClosed;
    public event Action<Scenario, ScenarioDetail, ScenarioEvents> ScenarioActivated;
    public event Action<ISettingsManager, string, BaseId> ScenarioSettingChanged;
    public event Action<PTTransmission, TimeSpan, bool, Exception> TransmissionProcessed;
    public event Action<PTTransmission, List<QueuedTransmissionData>> TransmissionReceived;
    public event Action<PTTransmission> ScenarioDetailTransmissionProcessed;
    public event Action<bool, string> UndoStart;
    public event Action<bool> UndoComplete;
    public event Action<BaseId, Transmission> UndoableActionProcessed;
    public event Action ScenarioDataLocked;
    public event Action ScenarioDataUnlocked;
    public event Action<bool> ScenarioDataReadonlyChanged;
    public event Action<ScenarioBaseT, Ctp, Job, Exception> CtpEvent;
    public event Action<UserField.EUDFObjectType> UDFDataChangesEvent;
    public event Action<BaseId, bool> ComparableScenariosListModified;

    public event Action<BaseId> ComparableScenarioDataChanged;

    // TODO: Implement using the event below
    public event Action NoAccessibleScenarios;
    public event Action UserPermissionsUpdated;
    public ScenarioDataLock DataLock => m_scenarioContext.DataLock;

    public event Action<Guid, string, BaseId> ScenarioDesynced;
    public event Action<BaseId, BaseId> BeginScenarioConversion;
    public event Action<BaseId, BaseId> ScenarioConversionComplete;

    private bool m_inReadonly;
    private bool m_dataReadOnly;
    private bool m_initializing;
    public bool ScenarioIsInReadonly => m_inReadonly;

    public bool ScenarioIsDataLocked => m_dataReadOnly;

    public BaseId ScenarioId => m_scenarioContext.ScenarioId;

    public Control InvokeControl => m_invokeControl;

    public IMessageProvider MessageProvider => m_messageProvider;

    public ScenarioPermissionSettings ScenarioPermissions => m_scenarioContext.PermissionSettings;

    public IClientScenarioData ActiveClientScenarioData => m_scenarioContext.ClientScenarioData;

    public event VoidDelegate KpiSnapshotsUpdated;

    public List<IClientScenarioData> GetComparableClientScenarioDataList()
    {
        return new List<IClientScenarioData>();
    }

    public ScenarioDetailCacheLock ScenarioDetailCacheLock
    {
        get
        {
            m_scenarioContext.ScenarioDetailCacheLock.InitCache();
            return m_scenarioContext.ScenarioDetailCacheLock;
        }
    }

    /// <summary>
    /// Determine the current user's access level based on user permissions and the Scenario's View only/Edit state
    /// </summary>
    /// <param name="a_userPermissions">The required user permissions</param>
    public EUserAccess GetCurrentUserEditAccess(params string[] a_userPermissions)
    {
        BaseId userPermissionSetId = m_mainForm.UsersInfo.GetUserPermissionSetId(SystemController.CurrentUserId);
        foreach (string permission in a_userPermissions)
        {
            if (!Validator.ValidatePermission(permission))
            {
                return EUserAccess.None;
            }
        }

        if (!ScenarioPermissions.CanUserEdit(SystemController.CurrentUserId, userPermissionSetId) && !ScenarioPermissions.CanUserView(SystemController.CurrentUserId, userPermissionSetId))
        {
            return EUserAccess.None;
        }

        if (ScenarioPermissions.IsViewOnly(SystemController.CurrentUserId, userPermissionSetId))
        {
            return EUserAccess.ViewOnly;
        }

        return EUserAccess.Edit;
    }

    public ScenarioOptions SystemSettings => m_scenarioContext.SystemSettings;
    public event Action<ScenarioOptions> SystemSettingsChanged;
    public event Action<BaseId> ScenarioInfoScenarioDesynced;

    public UserPreferences CurrentUserPreferences()
    {
        throw new NotImplementedException();
    }
}