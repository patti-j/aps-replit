using System.Drawing;
using System.Windows.Forms;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.APSCommon.Windows;
using PT.Common.Debugging;
using PT.Common.Delegates;
using PT.Common.Exceptions;
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
using PT.UIDefinitions;
using PT.UIDefinitions.Interfaces;

namespace PT.UI.Scenarios;

internal class MainScenarioControl : IScenarioController, IMultiScenarioInfo
{
    private readonly IMainForm m_mainForm;
    private readonly IPackageManagerUI m_pm;
    private readonly IImpactAnalyzer m_impactAnalyzer;
    private BaseId m_activeScenarioId = BaseId.NULL_ID;
    private readonly Control m_invokeControl;
    private readonly IMessageProvider m_messageProvider;
    private readonly Dictionary<BaseId, ScenarioContextPlus> m_scenarioContexts;
    private readonly ScenarioPlanningPreferences m_scenarioPreferences = new ();

    internal MainScenarioControl(IMainForm a_mainForm, IPackageManagerUI a_pm, IImpactAnalyzer a_impactAnalyzer)
    {
        m_mainForm = a_mainForm;
        m_pm = a_pm;
        m_impactAnalyzer = a_impactAnalyzer;
        m_invokeControl = a_mainForm.GetOwnerForm();
        m_messageProvider = a_mainForm.MessageProvider;
        m_scenarioContexts = new Dictionary<BaseId, ScenarioContextPlus>();
        m_scenarioPreferences = a_mainForm.UserPreferenceInfo.LoadSetting(m_scenarioPreferences);

        a_mainForm.UiNavigationEvent += MainFormOnUiNavigationEvent;
    }

    private void MainFormOnUiNavigationEvent(UINavigationEvent a_navigationEvent)
    {
        switch (a_navigationEvent.Key)
        {
            case "ActivateScenario":
                BaseId scenarioId = (BaseId)a_navigationEvent.Data["ActivateScenario"];
                ActivateScenarioById(scenarioId);
                return;
            case "CopyScenario":
                return;
            case "DeleteScenario":
                return;
        }
    }

    public void LoadScenarioData()
    {
        m_initializing = true;
        using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm)) //Wait until a lock can be made.  Must open here.
        {
            sm.ScenarioReplacedEvent += new ScenarioManager.ScenarioReplacedDelegate(ReceiveScenarioReplaceTransmission);
            sm.ScenarioBeforeDeleteEvent += DeleteScenario;
            sm.ScenarioPromptDeleteEvent += ScenarioPromptDeleteEvent;
            sm.ScenarioDeleteFailedValidationEvent += FireScenarioDeleteFailed;
            sm.ScenarioChangedEvent += sm_ScenarioChangedEvent;
            sm.ScenarioNewEvent += NewScenario;
            sm.ScenarioConversionCompleteEvent += SmOnConvertToProductionCompleteEvent;
            sm.ScenarioNewNotLoadedEvent += NewScenarioNotLoaded;
            sm.UDFDataChangedEvent += OnUDFDataChangedEvent;
            sm.ScenarioReloadEvent += ReloadScenario;
            sm.ScenarioBeforeReloadEvent += UnsubscribeContextEvents;
            sm.ScenarioReplaceFailedEvent += ScenarioReplaceFailedNotificationEvent;

            using (SystemController.Sys.UsersLock.EnterRead(out UserManager a_um))
            {
                User user = a_um.GetById(SystemController.CurrentUserId);
                UserPermissionSet permissions = (UserPermissionSet)a_um.GetUserPermissionSetById(user.UserPermissionSetId).Clone();
                PlantPermissionSet plantPermissions = (PlantPermissionSet)a_um.GetPlantPermissionSetById(user.PlantPermissionsId).Clone();

                //Create a context for each scenario
                for (int i = 0; i < sm.LoadedScenarioCount; i++)
                {
                    Scenario s = sm.GetByIndex(i);
                    InitScenario(s);

                    using (s.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
                    {
                        //m_scenarioSelectionControl.AddScenario(newContext);
                        if (UserEligibleForScenario(permissions, ss.ScenarioSettings))
                        {
                            using (s.ScenarioDetailLock.EnterRead(out ScenarioDetail sd))
                            {
                                AddScenario(s, ss, sd, BaseId.NULL_ID);
                                UpdateUserPermissions(permissions, plantPermissions, sd);
                            }
                        }
                    }
                }
            }

            using (SystemController.Sys.UserManagerEventsLock.EnterRead(out UserManagerEvents ume))
            {
                ume.ScenarioDataChangesEvent += UmeOnScenarioDataChangesEvent;
            }
        }

        //If the user can only see the published and/or whatif scenarios and there isn't one then there will be no scenarios visible
        if (m_scenarioContexts.Count == 0)
        {
            m_messageProvider.ShowMessageBox(new PTMessage(Localizer.GetErrorString("2702"), "No visible scenarios".Localize()) { Classification = PTMessage.EMessageClassification.Information }, true);
        }
        else
        {
            BaseId scenarioIdToLoad = BaseId.NULL_ID;

            if (m_scenarioPreferences.LoadLastActiveScenario) //TODO: This always seems to be set to true
            {
                scenarioIdToLoad = m_scenarioPreferences.LastActiveScenarioId;
            }

            //Attempt to load the last active production scenario if the last active is not available
            if (scenarioIdToLoad == BaseId.NULL_ID || !m_scenarioContexts.ContainsKey(scenarioIdToLoad))
            {
                DateTimeOffset lastActiveScenarioTime = new DateTimeOffset(DateTime.MinValue, TimeSpan.Zero);
                foreach (ScenarioContextPlus scp in m_scenarioContexts.Values)
                {
                    if (scp.PlanningSettings.Production && scp.LastActionDateUtc > lastActiveScenarioTime)
                    {
                        scenarioIdToLoad = scp.ScenarioId;
                        lastActiveScenarioTime = scp.LastActionDateUtc;
                    }
                }
            }

            //There are no production scenarios???
            if (scenarioIdToLoad == BaseId.NULL_ID || !m_scenarioContexts.ContainsKey(scenarioIdToLoad))
            {
                DebugException.ThrowInDebug("There are no production scenarios!");
                ScenarioContextPlus scp = m_scenarioContexts.First().Value;
                scenarioIdToLoad = scp.ScenarioId;
            }
          
            ActivateScenarioById(scenarioIdToLoad);
        }

        m_initializing = false;
    }
    public event Action<BaseId> ScenarioPromptDelete;
    private void ScenarioPromptDeleteEvent(BaseId a_deleteScenario)
    {
        ScenarioPromptDelete?.Invoke(a_deleteScenario);
    }

    private void SmOnConvertToProductionCompleteEvent(BaseId a_scenarioId, BaseId a_instigatorId)
    {
        ScenarioConversionComplete?.Invoke(a_scenarioId, a_instigatorId);
    }

    private void ScenarioReplaceFailedNotificationEvent(BaseId a_scenarioId, ScenarioReplaceT a_scenarioReplaceT)
    {
        ScenarioReplaceFailed?.Invoke(a_scenarioId, a_scenarioReplaceT);
    }

    private async Task ReceiveScenarioReplaceTransmission(BaseId a_scenarioId, ScenarioBaseT a_t)
    {
        using (BackgroundLock asyncLock = new(a_scenarioId))
        {
            await asyncLock.RunLockCode(m_invokeControl, ReloadNewScenarioAsync);
            if (asyncLock.Status is BackgroundLock.EResultStatus.Error or BackgroundLock.EResultStatus.Canceled)
            {
                return;
            }
        }
    }

    private void NewScenarioNotLoaded(BaseId a_scenarioId, ScenarioBaseT a_transmission)
    {
        ScenarioCreated?.Invoke(a_scenarioId, a_transmission);
    }

    private void OnUDFDataChangedEvent(UserField.EUDFObjectType a_objectType)
    {
        UDFDataChangesEvent?.Invoke(a_objectType);    
    }

    private void FireScenarioDeleteFailed(BaseId a_instigatorId, BaseId a_scenarioId, string a_errorMessage)
    {
        ScenarioDeleteFailed?.Invoke(a_instigatorId, a_scenarioId, a_errorMessage);
    }

    /// <summary>
    /// Checks user permissions and adds the scenario if user has access.
    /// </summary>
    private static bool UserEligibleForScenario(UserPermissionSet a_permissions, ISettingsManager a_settingsManager)
    {
        ScenarioPermissionSettings permissionSettings = new ();
        permissionSettings = a_settingsManager.LoadSetting(permissionSettings);

        if (a_permissions.CanManageScenarios)
        {
            return true;
        }

        if (permissionSettings.CanUserView(SystemController.CurrentUserId, a_permissions.Id))
        {
            return true;
        }

        return false;
    }

    private static void InitScenario(Scenario a_scenario)
    {
        //Set the Main thread for all scenario locks to avoid threadlocks.
        a_scenario.ScenarioDetailLock.SetMainThread();
        a_scenario.ScenarioEventsLock.SetMainThread();
        a_scenario.ScenarioSummaryLock.SetMainThread();
        a_scenario.ScenarioUndoEventsLock.SetMainThread();
    }

    public IScenarioInfo GetScenarioInfo()
    {
        return this;
    }

    private async void NewScenario(BaseId a_scenarioId, ScenarioBaseT a_t)
    {
        BaseId originalId = BaseId.NULL_ID;
        if (a_t is ScenarioCopyT copyT)
        {
            originalId = copyT.OriginalId;
        }
        else if (a_t is ScenarioLoadT loadT)
        {
            originalId = loadT.ScenarioToLoadId;
        }

        using (BackgroundLock asyncLock = new (a_scenarioId))
        {
            await asyncLock.RunLockCode(m_invokeControl, LoadNewScenarioAsync, originalId).ConfigureAwait(false);
            if (asyncLock.Status is BackgroundLock.EResultStatus.Error or BackgroundLock.EResultStatus.Canceled)
            {
                return;
            }
        }

        ScenarioCreated?.Invoke(a_scenarioId, a_t);
    }

    private async void ReloadScenario(BaseId a_scenarioId, ScenarioBaseT a_scenarioReloadT)
    {
        using (BackgroundLock asyncLock = new(a_scenarioId))
        {
            await asyncLock.RunLockCode(m_invokeControl, ReloadNewScenarioAsync).ConfigureAwait(false);
            if (asyncLock.Status is BackgroundLock.EResultStatus.Error or BackgroundLock.EResultStatus.Canceled)
            {
                return;
            }
        }

        ScenarioReloaded?.Invoke(a_scenarioId, a_scenarioReloadT);
    }

    private void LoadNewScenarioAsync(Scenario a_scenario, ScenarioDetail a_sd, params object[] a_params)
    {
        BaseId originalId = (BaseId)a_params[0];
        ScenarioContextPlus scenarioContextPlus;
        using (a_scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
        {
            scenarioContextPlus = AddScenario(a_scenario, ss, a_sd, originalId);
        }

        if (originalId != BaseId.NULL_ID)
        {
            if (scenarioContextPlus.PlanningSettings.CompareScenario)
            {
                ComparableScenariosListModified?.Invoke(scenarioContextPlus.ScenarioId, true);
            }
        }
    }

    /// <summary>
    /// This function's main purpose is to re-subscribe to the various events that are unsubscribed to at
    /// the start of the reload process. 
    /// </summary>
    /// <param name="a_scenario"></param>
    /// <param name="a_sd"></param>
    /// <param name="a_params"></param>
    private void ReloadNewScenarioAsync(Scenario a_scenario, ScenarioDetail a_sd, params object[] a_params)
    {
        ScenarioContextPlus scenarioContextPlus;
        using (a_scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
        {
            scenarioContextPlus = ConnectScenarioToContext(a_scenario, ss, a_sd);
        }

        if (scenarioContextPlus.PlanningSettings.CompareScenario)
        {
            ComparableScenariosListModified?.Invoke(scenarioContextPlus.ScenarioId, true);
        }

        if (a_sd.Scenario.Id != ActiveScenarioId)
        {
            return;
        }

        ActivateScenarioById(a_sd.Scenario.Id, true);
    }

    private void DeleteScenario(Scenario a_s, ScenarioEvents a_se, ScenarioUndoEvents a_sue, ScenarioBaseT a_t, bool a_isUnload)
    {
        bool comparableScenarioDeleted = false;
        if (m_scenarioContexts.TryGetValue(a_s.Id, out ScenarioContextPlus deletedContext))
        {
            if (deletedContext.PlanningSettings.CompareScenario)
            {
                comparableScenarioDeleted = true;
            }
        }

        RemoveScenario(a_s, a_se, a_sue);
        FireCloseScenario(a_s, a_se, a_sue); // un-registers the event listeners of the ScenarioViewer

        if (a_s.Id == m_activeScenarioId)
        {
            m_activeScenarioId = BaseId.NULL_ID;
            if (m_scenarioContexts.Count > 0)
            {
                ActivateScenarioById(m_scenarioContexts.First().Key);
            }
        }

        if (comparableScenarioDeleted)
        {
            ComparableScenariosListModified?.Invoke(a_s.Id, false);
        }

        ScenarioDeleted?.Invoke(a_s, a_se, a_sue, a_t, a_isUnload);
    }

    /// <summary>
    /// This function does pretty much the same thing as the DeleteScenario event handler, except it doesn't first
    /// ComparableScenariosListModified or ScenarioDeleted. The goal is to remove the context and unsubscribe to the listeners
    /// without triggering an update on the UI so that the UI doesn't try to access the ActiveContext. This can be
    /// problematic for reloading because the ActiveContext won't exist until the scenario is finish reloading, but this
    /// portion of the code executes during the unloading portion of the reloading. 
    /// </summary>
    /// <param name="a_scenario"></param>
    /// <param name="a_scenarioEvents"></param>
    /// <param name="a_scenarioUndoEvents"></param>
    private void DetachEventListenersAndRemoveContextForScenarioReload(Scenario a_scenario, ScenarioEvents a_scenarioEvents, ScenarioUndoEvents a_scenarioUndoEvents)
    {
        UnsubscribeContextEvents(a_scenario, a_scenarioEvents, a_scenarioUndoEvents);
        FireCloseScenario(a_scenario, a_scenarioEvents, a_scenarioUndoEvents); // un-registers the event listeners of the ScenarioViewer
    }

    private void sm_ScenarioChangedEvent(ScenarioChangeT a_t)
    {
        ScenarioChanged?.Invoke(a_t);
    }

    #region Permissions
    public UserPreferences CurrentUserPreferences()
    {
        return new UserPreferences();
    }

    public UserPermissionValidator Validator => ActiveContext.Validator;

    public PlantPermissionValidator GetPlantValidator(BaseId a_plantId)
    {
        return ActiveContext.GetPlantValidator(a_plantId);
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

    #region Events
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
    public event Action<ISettingsManager, string, BaseId> ScenarioSettingChanged;
    public event Action<PTTransmission, TimeSpan, bool, Exception> TransmissionProcessed;
    public event Action<PTTransmission, List<QueuedTransmissionData>> TransmissionReceived;
    public event Action<PTTransmission> ScenarioDetailTransmissionProcessed;
    public event Action<bool, string> UndoStart;
    public event Action<bool> UndoComplete;
    public event Action<BaseId, Transmission> UndoableActionProcessed;

    /// <summary>
    /// The active scenario has been switched. Data should be reloaded using the new active scenario id
    /// </summary>
    public event Action<Scenario, ScenarioEvents> ScenarioClosed;

    public event Action<Scenario, ScenarioDetail, ScenarioEvents> ScenarioActivated;

    /// <summary>
    /// The active scenario properties have changed and should be updated.
    /// Note: No scenario detail data is changed
    /// </summary>
    public event Action<long> ScenarioPropertiesChanged;

    public event Action<UINavigationEvent> UiNavigationEvent;

    // The bool parameter, when true, means that it's an unload and not a full delete
    public event Action<Scenario, ScenarioEvents, ScenarioUndoEvents, ScenarioBaseT, bool> ScenarioDeleted;
    public event Action<BaseId, BaseId, string> ScenarioDeleteFailed;

    public event Action<ScenarioChangeT> ScenarioChanged;
    public event Action<BaseId, ScenarioBaseT> ScenarioCreated;
    public event Action ScenarioDataLocked;
    public event Action ScenarioDataUnlocked;
    public event Action<BaseId, ScenarioBaseT> ScenarioReloaded;
    public event Action<bool> ScenarioDataReadonlyChanged;
    public event Action<ScenarioBaseT, Ctp, Job, Exception> CtpEvent;
    public event Action<UserField.EUDFObjectType> UDFDataChangesEvent;
    public event Action<BaseId, ScenarioReplaceT> ScenarioReplaceFailed;

    public event Action NoAccessibleScenarios;
    public event Action UserPermissionsUpdated;
    public ScenarioDataLock DataLock => ActiveContext.DataLock;
    #endregion

    private bool m_inReadonly;
    private bool m_dataReadOnly;
    private bool m_initializing;

    internal ScenarioContextPlus ActiveContext => m_scenarioContexts[m_activeScenarioId];
    public event Action<BaseId, BaseId> BeginScenarioConversion;
    public event Action<BaseId, BaseId> ScenarioConversionComplete;
    public bool ScenarioIsInReadonly => m_inReadonly;

    public bool ScenarioIsDataLocked => m_dataReadOnly;

    public event Action<BaseId, bool> ComparableScenariosListModified;
    public event Action<BaseId> ComparableScenarioDataChanged;
    public event Action<Guid, string, BaseId> ScenarioDesynced;

    public BaseId ScenarioId => m_activeScenarioId;

    public Color ScenarioColor => ActiveContext.ScenarioColor;

    public string Name => ActiveContext.ScenarioName;

    public ScenarioPlanningSettings PlanningSettings => ActiveContext.PlanningSettings;

    public IntegrationConfigMappingSettings IntegrationSettings => ActiveContext.IntegrationConfigMappingSettings;

    public List<ScenarioContextPlus> ScenarioContexts => m_scenarioContexts.Values.ToList();

    public List<IClientScenarioData> GetComparableClientScenarioDataList()
    {
        List<IClientScenarioData> comparableClientScenarioDatas = new ();
        foreach (ScenarioContextPlus context in m_scenarioContexts.Values)
        {
            if (context.ScenarioId == m_activeScenarioId)
            {
                continue;
            }

            if (context.PlanningSettings.CompareScenario)
            {
                comparableClientScenarioDatas.Add(context.ClientScenarioData);
            }
        }

        return comparableClientScenarioDatas;
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

        if (!ScenarioPermissions.CanUserEdit(SystemController.CurrentUserId, userPermissionSetId) &&
            !ScenarioPermissions.CanUserView(SystemController.CurrentUserId, userPermissionSetId) &&
            !Validator.CanManageScenarios)
        {
            return EUserAccess.None;
        }

        if (ScenarioPermissions.IsViewOnly(SystemController.CurrentUserId, userPermissionSetId))
        {
            return EUserAccess.ViewOnly;
        }

        return EUserAccess.Edit;
    }

    public Control InvokeControl => m_invokeControl;

    public IMessageProvider MessageProvider => m_messageProvider;

    public ScenarioPermissionSettings ScenarioPermissions => ActiveContext.PermissionSettings;

    public IClientScenarioData ActiveClientScenarioData { get; private set; }

    public event VoidDelegate KpiSnapshotsUpdated;

    public ScenarioOptions SystemSettings => ActiveContext.SystemSettings;
    public event Action<ScenarioOptions> SystemSettingsChanged;

    //public event Action<ScenarioUserRights> UserPermissionsChanged;
    #endregion Permissions

    public void FireNavigationEvent(UINavigationEvent a_event)
    {
        UiNavigationEvent?.Invoke(a_event);
    }

    public ScenarioContextPlus AddScenario(Scenario a_scenario, ScenarioSummary a_ss, ScenarioDetail a_sd, BaseId a_originalId)
    {
        ScenarioContextPlus newContext = new (m_mainForm.UsersInfo, m_mainForm.ClientSession, a_scenario, a_scenario.DataReadonly, a_ss.Name, m_mainForm.CurrentBrand.ActiveTheme, m_impactAnalyzer);
        ClientScenarioData clientScenarioData = new (m_pm, m_mainForm, this, newContext.ScenarioDetailCacheLock, a_scenario.Id);
        newContext.BindClientScenarioData(clientScenarioData);

        if (a_originalId != BaseId.NEW_OBJECT_ID)
        {
            using (SystemController.Sys.UsersLock.EnterRead(out UserManager um))
            {
                User user = um.GetById(SystemController.CurrentUserId);
                UserPermissionSet permissions = (UserPermissionSet)um.GetUserPermissionSetById(user.UserPermissionSetId).Clone();
                PlantPermissionSet plantPermissions = (PlantPermissionSet)um.GetPlantPermissionSetById(user.PlantPermissionsId).Clone();

                newContext.UpdateUserPermissions(permissions);
                newContext.UpdatePlantPermissions(plantPermissions, a_sd.PlantManager.Select(p => p.Id));
            }
        }

        m_scenarioContexts.Add(a_ss.Id, newContext);
        AttachListeners(newContext);

        using (a_scenario.ScenarioEventsLock.EnterRead(out ScenarioEvents se))
        {
            newContext.InitializeListeners(se);
        }

        newContext.InitializeSettingsListeners(a_ss.ScenarioSettings);

        using (a_scenario.ScenarioUndoEventsLock.EnterRead(out ScenarioUndoEvents ue))
        {
            newContext.InitializeUndoEvents(ue);
        }

        newContext.InitializeData(a_sd);

        return newContext;
    }

    public ScenarioContextPlus ConnectScenarioToContext(Scenario a_scenario, ScenarioSummary a_scenarioSummary, ScenarioDetail a_scenarioDetail)
    {
        BaseId scenarioId = a_scenarioSummary.Id;
        if (!m_scenarioContexts.TryGetValue(scenarioId, out ScenarioContextPlus existingContext))
        {
            //TODO lite client: Add error code
            throw new PTException("Could not find a corresponding ScenarioContext to the Scenario being reloaded.");
        }
        ClientScenarioData clientScenarioData = new(m_pm, m_mainForm, this, existingContext.ScenarioDetailCacheLock, scenarioId);
        existingContext.BindClientScenarioData(clientScenarioData);

        using (SystemController.Sys.UsersLock.EnterRead(out UserManager um))
        {
            User user = um.GetById(SystemController.CurrentUserId);
            UserPermissionSet permissions = (UserPermissionSet)um.GetUserPermissionSetById(user.UserPermissionSetId).Clone();
            PlantPermissionSet plantPermissions = (PlantPermissionSet)um.GetPlantPermissionSetById(user.PlantPermissionsId).Clone();

            existingContext.UpdateUserPermissions(permissions);
            existingContext.UpdatePlantPermissions(plantPermissions, a_scenarioDetail.PlantManager.Select(p => p.Id));
        }

        AttachListeners(existingContext);
        using (a_scenario.ScenarioEventsLock.EnterRead(out ScenarioEvents se))
        {
            existingContext.InitializeListeners(se);
        }

        existingContext.InitializeSettingsListeners(a_scenarioSummary.ScenarioSettings);

        using (a_scenario.ScenarioUndoEventsLock.EnterRead(out ScenarioUndoEvents ue))
        {
            existingContext.InitializeUndoEvents(ue);
        }

        existingContext.InitializeData(a_scenarioDetail);

        return existingContext;
    }

    private void ScenarioContextOnUndoableActionProcessed(ScenarioContext a_context, ScenarioBaseT a_scenarioBaseT)
    {
        UndoableActionProcessed?.Invoke(a_context.ScenarioId, a_scenarioBaseT);
    }

    private void ScenarioContextOnUndoComplete(ScenarioContext a_context, bool a_success)
    {
        if (a_context == ActiveContext)
        {
            ScenarioDataUnlocked?.Invoke();
            m_dataReadOnly = false;
            UndoComplete?.Invoke(a_success);
        }
        else
        {
            if (a_context is ScenarioContextPlus scenarioContextPlus && scenarioContextPlus.PlanningSettings.CompareScenario)
            {
                ComparableScenarioDataChanged?.Invoke(a_context.ScenarioId);
            }
        }
    }

    private void ScenarioContextOnUndoStart(ScenarioContext a_context, bool a_isUndo, string a_description)
    {
        if (a_context == ActiveContext)
        {
            ScenarioDataLocked?.Invoke();
            m_dataReadOnly = true;
            UndoStart?.Invoke(a_isUndo, a_description);
        }
    }

    public void RemoveScenario(Scenario a_s, ScenarioEvents a_se, ScenarioUndoEvents a_sue)
    {
        ScenarioContextPlus context = m_scenarioContexts[a_s.Id];
        DetachListeners(context);
        context.RemoveScenario(a_s, a_se, a_sue);
        m_scenarioContexts.Remove(a_s.Id);
        if (m_scenarioContexts.Count == 0)
        {
            FireNoAccessibleScenarios();
        }
    }

    public void FireNoAccessibleScenarios()
    {
        NoAccessibleScenarios?.Invoke();
    }

    public void UnsubscribeContextEvents(Scenario a_s, ScenarioEvents a_se, ScenarioUndoEvents a_sue)
    {
        ScenarioContextPlus context = m_scenarioContexts[a_s.Id];
        DetachListeners(context);
        context.UnloadScenario(a_s, a_se, a_sue);
    }

    private void AttachListeners(ScenarioContextPlus a_context)
    {
        a_context.MoveComplete += ContextOnMoveComplete;
        a_context.SimulationStart += ContextOnSimulationStart;
        a_context.SimulationComplete += ContextOnSimulationCompleteAsync;
        a_context.SimulationCancelled += ContextOnSimulationCancelledAsync;
        a_context.SimulationBeginCancellation += ContextOnSimulationCancelBeginAsync;
        a_context.SimulationProgressEvent += ContextOnSimulationProgressEvent;

        a_context.APSClockChanged += ContextOnAPSClockChanged;

        a_context.PublishStart += ScenarioContextOnPublishStart;
        a_context.PublishProgress += ScenarioContextOnPublishProgress;
        a_context.PublishComplete += ScenarioContextOnPublishComplete;

        a_context.DataChanged += ContextOnDataChanged;
        a_context.ScenarioSettingChanged += ScenarioContextOnScenarioSettingChanged;
        a_context.ComparableScenariosListModified += ContextOnComparableScenariosListModified;
        a_context.TransmissionProcessed += ContextOnTransmissionProcessed;
        a_context.TransmissionReceived += ContextOnTransmissionReceived;
        a_context.ScenarioDetailTransmissionProcessedEvent += ContextOnScenarioDetailTransmissionProcessed;
        a_context.UndoStart += ScenarioContextOnUndoStart;
        a_context.UndoComplete += ScenarioContextOnUndoComplete;
        a_context.UndoableActionProcessed += ScenarioContextOnUndoableActionProcessed;
        a_context.ScenarioReadonlyChanged += ContextOnScenarioReadonlyChanged;
        a_context.ScenarioNameChanged += ContextOnScenarioNameChanged;
        a_context.SystemSettingsChanged += ContextOnSystemSettingsChanged;
        a_context.KpiSnapshotsUpdated += ContextOnKpiSnapshotsUpdated;

        a_context.CtpEvent += ContextOnCtpEvent;
        a_context.DesynchronizedScenario += ScenarioContextOnDesynchronize;
        a_context.BeginScenarioConversionEvent += BeginScenarioConversionEvent;
    }

    private void BeginScenarioConversionEvent(BaseId a_currentProdId, BaseId a_promotedWhatIfScenarioId)
    {
        BeginScenarioConversion?.Invoke(a_currentProdId, a_promotedWhatIfScenarioId);
    }

    private void ContextOnKpiSnapshotsUpdated()
    {
        KpiSnapshotsUpdated?.Invoke();
    }

    private void ContextOnCtpEvent(ScenarioContext a_context, ScenarioBaseT a_t, Ctp a_ctp, Job a_job, Exception a_exception)
    {
        if (a_context == ActiveContext)
        {
            CtpEvent?.Invoke(a_t, a_ctp, a_job, a_exception);
        }
        else
        {
            if (a_context is ScenarioContextPlus scenarioContextPlus && scenarioContextPlus.PlanningSettings.CompareScenario)
            {
                ComparableScenarioDataChanged?.Invoke(a_context.ScenarioId);
            }
        }
    }

    private void ContextOnAPSClockChanged(ScenarioContext a_context, DateTime a_clockDate)
    {
        if (a_context == ActiveContext)
        {
            ClockDateChanged?.Invoke(a_clockDate);
        }
    }

    private void ContextOnScenarioNameChanged(ScenarioContext a_context, string a_scenarioName)
    {
        ScenarioPropertiesChanged?.Invoke(a_context.ScenarioId.Value);

        if (a_context != ActiveContext)
        {
            if (a_context is ScenarioContextPlus scenarioContextPlus && scenarioContextPlus.PlanningSettings.CompareScenario)
            {
                ComparableScenarioDataChanged?.Invoke(a_context.ScenarioId);
            }
        }
    }

    private void ContextOnScenarioReadonlyChanged(ScenarioContext a_context, bool a_readonly)
    {
        if (a_context == ActiveContext)
        {
            m_inReadonly = a_readonly;
            ScenarioDataReadonlyChanged?.Invoke(a_readonly);
        }
    }

    private void ContextOnTransmissionProcessed(ScenarioContext a_context, PTTransmission a_t, TimeSpan a_processingSpan, bool a_simTransmission, Exception a_e)
    {
        if (a_context == ActiveContext)
        {
            TransmissionProcessed?.Invoke(a_t, a_processingSpan, a_simTransmission, a_e);
        }
    }

    private void ContextOnTransmissionReceived(ScenarioContext a_context, PTTransmission a_t, List<QueuedTransmissionData> a_queuedTransmissionDescriptions)
    {
        if (a_context == ActiveContext)
        {
            TransmissionReceived?.Invoke(a_t, a_queuedTransmissionDescriptions);
        }
    }

    private void ContextOnScenarioDetailTransmissionProcessed(ScenarioContext a_context, PTTransmission a_t)
    {
        if (a_context == ActiveContext)
        {
            ScenarioDetailTransmissionProcessed?.Invoke(a_t);
        }
    }

    private void ContextOnSimulationProgressEvent(ScenarioContext a_context, SimulationProgress.Status a_status, decimal a_percentComplete)
    {
        if (a_context == ActiveContext)
        {
            SimulationProgress?.Invoke(a_status, a_percentComplete);
        }
    }

    private void ContextOnDataChanged(ScenarioContext a_context, IScenarioDataChanges a_changes)
    {
        if (a_context == ActiveContext)
        {
            DataChanged?.Invoke(a_changes);
        }
        else
        {
            if (a_context is ScenarioContextPlus scenarioContextPlus && scenarioContextPlus.PlanningSettings.CompareScenario)
            {
                ComparableScenarioDataChanged?.Invoke(a_context.ScenarioId);
            }
        }
    }

    private void ScenarioContextOnScenarioSettingChanged(ScenarioContext a_scenarioContext, ISettingsManager a_scenarioSettingsManager, string a_settingKey)
    {
        if (IsSettingPropagatedToUI(a_settingKey) && ScenarioId == a_scenarioContext.ScenarioId)
        {
            if (GetCurrentUserEditAccess() == EUserAccess.None)
            {
                bool foundAccessibleScenario = false;
                BaseId userPermissionSetId = m_mainForm.UsersInfo.GetUserPermissionSetId(SystemController.CurrentUserId);
                foreach ((BaseId scenarioId, ScenarioContextPlus scenarioContextPlus) in m_scenarioContexts)
                {
                    if (scenarioId == ScenarioId)
                    {
                        //skip over the active scenario
                        continue;
                    }

                    if (scenarioContextPlus.PermissionSettings.CanUserView(SystemController.CurrentUserId, userPermissionSetId))
                    {
                        foundAccessibleScenario = true;
                        ActivateScenarioById(scenarioId);
                        break;
                    }
                }

                if (!foundAccessibleScenario)
                {
                    NoAccessibleScenarios?.Invoke();
                }
            }
        }

        ScenarioSettingChanged?.Invoke(a_scenarioSettingsManager, a_settingKey, a_scenarioContext.ScenarioId);
    }

    /// <summary>
    /// Determines whether the setting is something the UI cares about on update.
    /// Originally this was just for ScenarioPermissions, but was extended to other relevant settings.
    /// There's no general reason why other settings types can't be propagated down, but we scope here to limit the number of needless UI event invocations.
    /// </summary>
    /// <param name="a_settingKey"></param>
    /// <returns></returns>
    private bool IsSettingPropagatedToUI(string a_settingKey)
    {
        return a_settingKey == ScenarioPermissions.SettingKey ||
               a_settingKey == PlanningSettings.SettingKey ||
               a_settingKey == ScenarioPermissions.SettingKey;
    }

    private void ContextOnComparableScenariosListModified(BaseId a_scenarioId, bool a_compare)
    {
        ComparableScenariosListModified?.Invoke(a_scenarioId, a_compare);
    }

    private void ScenarioContextOnDesynchronize(Guid a_transmissionId, string a_description, BaseId a_scenarioId)
    {
        ScenarioDesynced?.Invoke(a_transmissionId, a_description, a_scenarioId);
    }

    private void DetachListeners(ScenarioContextPlus a_context)
    {
        a_context.MoveComplete -= ContextOnMoveComplete;
        a_context.SimulationStart -= ContextOnSimulationStart;
        a_context.SimulationComplete -= ContextOnSimulationCompleteAsync;
        a_context.SimulationCancelled -= ContextOnSimulationCancelledAsync;
        a_context.SimulationBeginCancellation -= ContextOnSimulationCancelBeginAsync;
        a_context.SimulationProgressEvent -= ContextOnSimulationProgressEvent;

        a_context.PublishStart -= ScenarioContextOnPublishStart;
        a_context.PublishProgress -= ScenarioContextOnPublishProgress;
        a_context.PublishComplete -= ScenarioContextOnPublishComplete;

        a_context.DataChanged -= ContextOnDataChanged;
        a_context.ScenarioSettingChanged -= ScenarioContextOnScenarioSettingChanged;
        a_context.ComparableScenariosListModified -= ContextOnComparableScenariosListModified;
        a_context.TransmissionProcessed -= ContextOnTransmissionProcessed;
        a_context.TransmissionReceived -= ContextOnTransmissionReceived;
        a_context.UndoStart -= ScenarioContextOnUndoStart;
        a_context.UndoComplete -= ScenarioContextOnUndoComplete;
        a_context.UndoableActionProcessed -= ScenarioContextOnUndoableActionProcessed;
        a_context.ScenarioReadonlyChanged -= ContextOnScenarioReadonlyChanged;
        a_context.ScenarioNameChanged -= ContextOnScenarioNameChanged;
        a_context.SystemSettingsChanged -= ContextOnSystemSettingsChanged;
        a_context.KpiSnapshotsUpdated -= KpiSnapshotsUpdated;

        a_context.DesynchronizedScenario -= ScenarioContextOnDesynchronize;
    }

    private void ContextOnSystemSettingsChanged(ScenarioContext a_context, ScenarioOptions a_settings)
    {
        if (a_context == ActiveContext)
        {
            SystemSettingsChanged?.Invoke(a_settings);
        }
    }
    private void ContextOnSimulationCancelledAsync(ScenarioContext a_context)
    {
        if (a_context == ActiveContext)
        {
            ScenarioDataUnlocked?.Invoke();
            m_dataReadOnly = false;
            SimulationCancelled?.Invoke();
        }
        else
        {
            if (a_context is ScenarioContextPlus scenarioContextPlus && scenarioContextPlus.PlanningSettings.CompareScenario)
            {
                ComparableScenarioDataChanged?.Invoke(a_context.ScenarioId);
            }
        }
    }
    private void ContextOnSimulationCancelBeginAsync(ScenarioContext a_context)
    {
        if (a_context == ActiveContext)
        {
            ScenarioDataLocked?.Invoke();
            m_dataReadOnly = true;
            SimulationBeginCancellation?.Invoke();
        }
    }
    private void ContextOnSimulationCompleteAsync(ScenarioContext a_context)
    {
        if (a_context == ActiveContext)
        {
            ScenarioDataUnlocked?.Invoke();
            m_dataReadOnly = false;
            SimulationComplete?.Invoke();
        }
        else
        {
            if (a_context is ScenarioContextPlus scenarioContextPlus && scenarioContextPlus.PlanningSettings.CompareScenario)
            {
                ComparableScenarioDataChanged?.Invoke(a_context.ScenarioId);
            }
        }
    }

    private void ContextOnSimulationStart(ScenarioContext a_context, ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, BaseId a_instigatorId, DateTime a_timeStarted)
    {
        if (a_context == ActiveContext)
        {
            ScenarioDataLocked?.Invoke();
            m_dataReadOnly = true;
            SimulationStart?.Invoke(a_simType, a_t, a_instigatorId, a_timeStarted);
        }
    }

    private void ContextOnMoveComplete(ScenarioContext a_context, object a_moveResult)
    {
        if (a_context == ActiveContext)
        {
            ScenarioDataUnlocked?.Invoke();
            m_dataReadOnly = false;
            MoveComplete?.Invoke(a_moveResult);
        }
        else
        {
            if (a_context is ScenarioContextPlus scenarioContextPlus && scenarioContextPlus.PlanningSettings.CompareScenario)
            {
                ComparableScenarioDataChanged?.Invoke(a_context.ScenarioId);
            }
        }
    }

    private void ScenarioContextOnPublishComplete(ScenarioContext a_context, DateTime a_timeStamp)
    {
        if (a_context == ActiveContext)
        {
            PublishComplete?.Invoke(a_timeStamp);
        }
    }

    private void ScenarioContextOnPublishProgress(ScenarioContext a_context, PublishStatuses.EPublishProgressStep a_progressStatus, double a_percentComplete)
    {
        if (a_context == ActiveContext)
        {
            PublishProgress?.Invoke(a_progressStatus, a_percentComplete);
        }
    }

    private void ScenarioContextOnPublishStart(ScenarioContext a_context, ScenarioBaseT a_t, BaseId a_instigator, DateTime a_timeStamp)
    {
        if (a_context == ActiveContext)
        {
            PublishStart?.Invoke(a_t, a_instigator, a_timeStamp);
        }
    }

    public void UpdateUserPermissions(UserPermissionSet a_permissions, PlantPermissionSet a_plantPermissions, ScenarioDetail a_sd)
    {
        BaseId scenarioId = a_sd.Scenario.Id;
        m_scenarioContexts[scenarioId].UpdateUserPermissions(a_permissions);
        m_scenarioContexts[scenarioId].UpdatePlantPermissions(a_plantPermissions, a_sd.PlantManager.Select(p => p.Id));

        if (!m_initializing && scenarioId == ActiveContext.ScenarioId)
        {
            UserPermissionsUpdated?.Invoke();
        }
    }

    /// <summary>
    /// Detatch all events
    /// </summary>
    public void UnloadAllScenarios()
    {
        foreach (ScenarioContextPlus contextPlus in m_scenarioContexts.Values)
        {
            DetachListeners(contextPlus);
        }

        //Also unload UserManager events
        using (SystemController.Sys.UserManagerEventsLock.EnterRead(out UserManagerEvents ume))
        {
            ume.ScenarioDataChangesEvent -= UmeOnScenarioDataChangesEvent;
        }
    }

    public async void ActivateScenarioById(BaseId a_scenarioId, bool a_activeScenarioReplaced = false)
    {
        using (new MultiLevelHourglass())
        {
            if (m_scenarioContexts.TryGetValue(a_scenarioId, out ScenarioContextPlus context))
            {
                if (m_activeScenarioId != BaseId.NULL_ID) //When first activating, there is no current active scenario
                {
                    using (BackgroundLock asyncLock = new (m_activeScenarioId))
                    {
                        await asyncLock.RunLockCode(m_invokeControl, FireCloseScenario);
                        if (asyncLock.Status is BackgroundLock.EResultStatus.Error or BackgroundLock.EResultStatus.Canceled)
                        {
                            return;
                        }
                    }
                }

                if (m_activeScenarioId == a_scenarioId && !a_activeScenarioReplaced)
                {
                    return;
                }

                m_activeScenarioId = a_scenarioId;
                m_scenarioPreferences.LastActiveScenarioId = a_scenarioId;
                m_mainForm.UserPreferenceInfo.SaveSetting(m_scenarioPreferences);

                m_inReadonly = ActiveContext.ScenarioIsInReadonly;

                await ActiveContext.DataLock.CreateNewLock().RunLockCode(FireActivateScenario, a_activeScenarioReplaced);
            }
            else
            {
                DebugException.ThrowInDebug("Scenario does not exist to activate");
            }
        }
    }

    public ScenarioDetailCacheLock ScenarioDetailCacheLock => ActiveContext.ScenarioDetailCacheLock;

    public void FireActivateScenario(Scenario a_scenario, ScenarioDetail a_sd, ScenarioEvents a_se, params object[] a_params)
    {
        ActiveClientScenarioData = ActiveContext.ClientScenarioData;
        ActiveContext.SignalScenarioActivated();

        //if scenario is being replaced, there is no need to really notify the ui to act on scenario activated
        bool scenarioReplace = (bool)a_params[0];
        if (!scenarioReplace)
        {
            ScenarioActivated?.Invoke(a_scenario, a_sd, a_se);
        }
    }

    // TODO: Remove ScenarioUndoEvents? It's not used in this function
    public void FireCloseScenario(Scenario a_scenario, ScenarioEvents a_se, ScenarioUndoEvents a_sue)
    {
        ScenarioClosed?.Invoke(a_scenario, a_se);
    }

    private void UmeOnScenarioDataChangesEvent(IScenarioDataChanges a_changes)
    {
        DataChanged?.Invoke(a_changes);
    }

    public BaseId ActiveScenarioId => m_activeScenarioId;

    public bool IsScenarioLoaded(BaseId a_scenarioId)
    {
        using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager scenarioManager))
        {
            return scenarioManager.IsScenarioLoaded(a_scenarioId);
        }
    }
}