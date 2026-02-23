using PT.APSCommon;
using PT.ERPTransmissions;
using PT.Scheduler.TransmissionDispatchingAndReception;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Summary description for ScenarioEvents.
/// </summary>
public class ScenarioEvents
{
    #region Scenario Change Event
    public delegate void ChangeDelegate(ScenarioSummary ss, ScenarioChangeT t, Scenario s);

    public event ChangeDelegate ChangeEvent;

    internal void FireChangeEvent(ScenarioSummary ss, ScenarioChangeT t, Scenario s)
    {
        ChangeEvent?.Invoke(ss, t, s);
    }

    public delegate void ScenarioOptionsChangedDelegate(SystemOptionsT a_systemOptionsT, ScenarioDetail a_sd);

    public event ScenarioOptionsChangedDelegate ScenarioOptionsChangedEvent;

    internal void FireScenarioOptionsChangedEvent(SystemOptionsT a_systemOptionsT, ScenarioDetail a_sd) //TODO V12 CN: This seems like it could be handled with ScenarioDataChanges but SystemOptions doesn't inherit 
    {
        ScenarioOptionsChangedEvent?.Invoke(a_systemOptionsT, a_sd);
    }
    #endregion

    #region Scenario Detail events
    public delegate void ScenarioDetailClearDelegate(ScenarioDetailClearT t);

    public event ScenarioDetailClearDelegate ScenarioDetailClearEvent;

    internal void FireScenarioDetailClearEvent(ScenarioDetailClearT t, ScenarioDetail a_sd)
    {
        ScenarioDetailClearEvent?.Invoke(t);
    }

    public event Action OptimizeSettingsChanged;

    internal void FireOptimizeSettingsChangedEvent()
    {
        OptimizeSettingsChanged?.Invoke();
    }
    #endregion

    #region VesselType Events
    //Todo: Do we need this? 04/14/2021
    public delegate void VesselTypeDefaultDelegate(VesselType c, VesselTypeDefaultT t);

    public event VesselTypeDefaultDelegate VesselTypeDefaultEvent;

    internal void FireVesselTypeDefaultEvent(VesselType c, VesselTypeDefaultT t)
    {
        VesselTypeDefaultEvent?.Invoke(c, t);
    }

    public delegate void VesselTypeCopyDelegate(VesselType c, VesselTypeCopyT t);

    public event VesselTypeCopyDelegate VesselTypeCopyEvent;

    internal void FireVesselTypeCopyEvent(VesselType c, VesselTypeCopyT t)
    {
        VesselTypeCopyEvent?.Invoke(c, t);
    }

    public delegate void VesselTypeDeleteDelegate(VesselType c, VesselTypeDeleteT t);

    public event VesselTypeDeleteDelegate VesselTypeDeleteEvent;

    internal void FireVesselTypeDeleteEvent(VesselType c, VesselTypeDeleteT t)
    {
        VesselTypeDeleteEvent?.Invoke(c, t);
    }
    #endregion

    #region Job Events
    public delegate void JobTSucceededDelegate(JobT t);

    public event JobTSucceededDelegate JobTSucceededEvent;

    internal void FireJobTSucceededEvent(JobT t)
    {
        JobTSucceededEvent?.Invoke(t);
    }

    public delegate void NewJobExternalIdDelegate(string newExternalId, JobRequestNewExternalIdT t, ScenarioEvents se);

    public event NewJobExternalIdDelegate NewJobExternalIdEvent;

    internal void FireNewJobExternalIdEvent(string newExternalId, JobRequestNewExternalIdT t)
    {
        NewJobExternalIdEvent?.Invoke(newExternalId, t, this);
    }

    public delegate void JobResourceEligibilityRecomputedDelegate();

    public event JobResourceEligibilityRecomputedDelegate JobResourceEligibilityRecomputedEvent;

    internal void FireJobResourceEligibilityRecomputedEvent(ScenarioDetail sd) //TODO CN: Should this be handled? This event isn't handled anywhere it appears. 
    {
        JobResourceEligibilityRecomputedEvent?.Invoke();
    }
    #endregion

    #region PurchaseToStock Events
    public delegate void PurchaseToStockMovedHandler(PurchaseToStock pts, PurchaseToStockMoveT t);

    public event PurchaseToStockMovedHandler PurchaseToStockMovedEvent;

    internal void FirePurchaseToStockMovedEvent(PurchaseToStock pts, PurchaseToStockMoveT t, ScenarioDetail a_sd)
    {
        PurchaseToStockMovedEvent?.Invoke(pts, t);
    }

    public delegate void PurchaseToStockMoveFailedHandler(PurchaseToStock pts, PurchaseToStockMoveT t);

    public event PurchaseToStockMoveFailedHandler PurchaseToStockMoveFailedEvent;

    internal void FirePurchaseToStockMoveFailedEvent(PurchaseToStock pts, PurchaseToStockMoveT t, ScenarioDetail a_sd)
    {
        PurchaseToStockMoveFailedEvent?.Invoke(pts, t);
    }
    #endregion

    #region Checksum
    public delegate void DesynchronizedScenarioDelegate(Guid a_transmissionId, string a_description, BaseId a_scenarioId);

    public event DesynchronizedScenarioDelegate DesynchronizedScenarioEvent;

    /// <summary>
    /// This event is fired when the local copy of the schedule is found to be out of sync with the schedule on the server.
    /// </summary>
    /// <param name="t"></param>
    public void FireDesynchronizedScenarioEvent(Guid a_transmissionId, string a_description, BaseId a_scenarioId)
    {
        DesynchronizedScenarioEvent?.Invoke(a_transmissionId, a_description, a_scenarioId);
    }

    public delegate void AutoUpdateKeyDelegate(string a_desc, ScenarioDetail a_sd);

    public event AutoUpdateKeyDelegate AutoUpdateKeyEvent;

    /// <summary>
    /// This is fired after a Key Update has been performed and Client needs to restart to update it's values.
    /// </summary>
    /// <param name="a_desc"></param>
    /// <param name="a_sd"></param>
    public void FireAutoUpdateKeyEvent(string a_desc, ScenarioDetail a_sd)
    {
        AutoUpdateKeyEvent?.Invoke(a_desc, a_sd);
    }
    #endregion

    #region Scenario History Events
    public delegate void ScenarioHistoryNewDelegate();

    public event ScenarioHistoryNewDelegate ScenarioHistoryNewEvent;

    internal void FireScenarioHistoryEvent()
    {
        ScenarioHistoryNewEvent?.Invoke();
    }
    #endregion

    #region ERP Transmission Handling
    public delegate void TransmissionReceivedDelegate(PTTransmission a_t, List<QueuedTransmissionData> a_queuedTransmissionDescriptions);

    public event TransmissionReceivedDelegate TransmissionReceivedEvent;

    internal void FireTransmissionReceivedEvent(PTTransmission a_t, List<QueuedTransmissionData> a_queuedTransmissionDescriptions)
    {
        TransmissionReceivedEvent?.Invoke(a_t, a_queuedTransmissionDescriptions);
    }

    public delegate void TransmissionProcessedCompleteDelegate(PTTransmission a_t, TimeSpan a_processingTime, Exception a_e);

    public event TransmissionProcessedCompleteDelegate TransmissionProcessedEvent;

    internal void FireTransmissionProcessedEvent(PTTransmission a_t, TimeSpan a_processingTime, Exception a_e)
    {
        TransmissionProcessedEvent?.Invoke(a_t, a_processingTime, a_e);
    }

    public delegate void PublishStatusDelegate(PublishStatusMessageT a_t);

    public event PublishStatusDelegate PublishStatusEvent;

    // TODO: All listeners of this event should move to polling PublishController's GetPublishStatus
    // TODO: (If they were listening for their entire lifetimes, they should instead listen for the start of publish event (ExportScenarioEvent/ ScenarioDetailExportT?) and begin polling then until publish done)
    internal void FirePublishStatusEvent(PublishStatusMessageT a_t)
    {
        PublishStatusEvent?.Invoke(a_t);
    }
    #endregion

    //TODO V12 CN: No handlers are attached to these GanttLayout events. What can we do with these?

    #region Gantt Layout
    //Adds
    public delegate void GanttViewSetAddDelegate(GanttViewerPane pane, GanttViewSet ganttViewSet);

    public event GanttViewSetAddDelegate GanttViewSetAddEvent;

    internal void FireGanttViewSetAddEvent(GanttViewerPane pane, GanttViewSet ganttViewSet)
    {
        GanttViewSetAddEvent?.Invoke(pane, ganttViewSet);
    }

    public delegate void GanttViewAddDelegate(GanttViewerPane pane, GanttViewSet ganttViewSet, GanttView ganttView);

    public event GanttViewAddDelegate GanttViewAddEvent;

    internal void FireGanttViewAddEvent(GanttViewerPane pane, GanttViewSet ganttViewSet, GanttView ganttView)
    {
        GanttViewAddEvent?.Invoke(pane, ganttViewSet, ganttView);
    }

    public delegate void GanttViewRowAddDelegate(GanttViewerPane pane, GanttViewSet ganttViewSet, GanttView ganttView, GanttViewRow row);

    public event GanttViewRowAddDelegate GanttViewRowAddEvent;

    internal void FireGanttViewRowAddEvent(GanttViewerPane pane, GanttViewSet ganttViewSet, GanttView ganttView, GanttViewRow row)
    {
        GanttViewRowAddEvent?.Invoke(pane, ganttViewSet, ganttView, row);
    }

    //Removes
    public delegate void GanttViewSetRemoveDelegate(GanttViewerPane pane, GanttViewSet ganttViewSet);

    public event GanttViewSetRemoveDelegate GanttViewSetRemoveEvent;

    internal void FireGanttViewSetRemoveEvent(GanttViewerPane pane, GanttViewSet ganttViewSet)
    {
        GanttViewSetRemoveEvent?.Invoke(pane, ganttViewSet);
    }

    public delegate void GanttViewRemoveDelegate(GanttViewerPane pane, GanttViewSet ganttViewSet, GanttView ganttView);

    public event GanttViewRemoveDelegate GanttViewRemoveEvent;

    internal void FireGanttViewRemoveEvent(GanttViewerPane pane, GanttViewSet ganttViewSet, GanttView ganttView)
    {
        GanttViewRemoveEvent?.Invoke(pane, ganttViewSet, ganttView);
    }

    public delegate void GanttViewRowRemoveDelegate(GanttViewerPane pane, GanttViewSet ganttViewSet, GanttView ganttView, GanttViewRow row);

    public event GanttViewRowRemoveDelegate GanttViewRowRemoveEvent;

    internal void FireGanttViewRowRemoveEvent(GanttViewerPane pane, GanttViewSet ganttViewSet, GanttView ganttView, GanttViewRow row)
    {
        GanttViewRowRemoveEvent?.Invoke(pane, ganttViewSet, ganttView, row);
    }

    //Changes
    public delegate void GanttViewSetChangedDelegate(GanttViewerPane pane, GanttViewSet ganttViewSet);

    public event GanttViewSetChangedDelegate GanttViewSetChangedEvent;

    internal void FireGanttViewSetChangedEvent(GanttViewerPane pane, GanttViewSet ganttViewSet)
    {
        GanttViewSetChangedEvent?.Invoke(pane, ganttViewSet);
    }

    public delegate void GanttViewChangedDelegate(GanttViewerPane pane, GanttViewSet ganttViewSet, GanttView GanttView);

    public event GanttViewChangedDelegate GanttViewChangedEvent;

    internal void FireGanttViewChangedEvent(GanttViewerPane pane, GanttViewSet ganttViewSet, GanttView GanttView)
    {
        GanttViewChangedEvent?.Invoke(pane, ganttViewSet, GanttView);
    }

    public delegate void GanttViewRowChangedDelegate(GanttViewerPane pane, GanttViewSet ganttViewSet, GanttView GanttView, GanttViewRow row);

    public event GanttViewRowChangedDelegate GanttViewRowChangedEvent;

    internal void FireGanttViewRowChangedEvent(GanttViewerPane pane, GanttViewSet ganttViewSet, GanttView GanttView, GanttViewRow row)
    {
        GanttViewRowChangedEvent?.Invoke(pane, ganttViewSet, GanttView, row);
    }
    #endregion Gantt Layout

    #region Block Change Events
    //JMC TODO May want to just send the blocks changed to speed up.
    public delegate void BlocksChangedDelegate();

    public event BlocksChangedDelegate BlocksChangedEvent;

    internal void FireBlocksChangedEvent()
    {
        BlocksChangedEvent?.Invoke();
    }
    #endregion

    #region Simulation events
    #region SimulationProgressEvent
    public delegate void SimulationProgressDelegate(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, long a_simNbr, decimal a_percentComplete, SimulationProgress.Status a_status);

    public event SimulationProgressDelegate SimulationProgressEvent;

    internal bool HasSimulationProgressListeners => SimulationProgressEvent != null;

    internal void FireSimulationProgressEvent(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, decimal a_percentComplete, SimulationProgress.Status a_status, long a_simNbr)
    {
        SimulationProgressEvent?.Invoke(a_sd, a_simType, a_t, a_simNbr, a_percentComplete, a_status);
    }

    public delegate void SimulationProgressMRPDelegate(SimulationProgress.Status a_status);

    public event SimulationProgressMRPDelegate SimulationProgressMRPEvent;

    internal void FireSimulationProgressMRPEvent(SimulationProgress.Status a_status)
    {
        SimulationProgressMRPEvent?.Invoke(a_status);
    }
    #endregion

    #region MRP Events
    public delegate void MRPStatusUpdateDelegate(SimulationProgress.Status a_statusCode, object[] a_detailParams);

    public event MRPStatusUpdateDelegate MRPStatusUpdateEvent;

    internal void FireMrpStatusUpdateEvent(SimulationProgress.Status a_statusCode, object[] a_detailParams)
    {
        MRPStatusUpdateEvent?.Invoke(a_statusCode, a_detailParams);
    }
    #endregion MRP Events

    #region CapacityIntervalsPurgedEvent
    public delegate void CapacityIntervalsPurgedDelegate(DateTime purgeTime);

    public event CapacityIntervalsPurgedDelegate CapacityIntervalsPurgedEvent;

    internal void FireCapacityIntervalsPurgedEvent(DateTime purgeTime) //TODO V12 CN: Skip this for now
    {
        CapacityIntervalsPurgedEvent?.Invoke(purgeTime);
    }

    public delegate void OfflineStatusChangedDelegate(PTTransmission t);

    public event OfflineStatusChangedDelegate OfflineStatusChangedEvent;

    internal void FireOfflineStatusChangedEvent(PTTransmission t)
    {
        OfflineStatusChangedEvent?.Invoke(t);
    }

    public delegate void PopupMessageDelegate(string message, MessageSeverity severity, bool showAllUsers, BaseId showForThisUser);

    public event PopupMessageDelegate PopupMessageEvent;

    internal void FirePopupMessageEvent(string message, MessageSeverity severity, bool showAllUsers, BaseId showForThisUser)
    {
        PopupMessageEvent?.Invoke(message, severity, showAllUsers, showForThisUser);
    }

    internal bool FirePopupMessageEventListenersRegistered => PopupMessageEvent != null;
    #endregion

    #region MoveFailedEvent
    public delegate void MoveFailedDelegate(MoveResult result, ScenarioDetail a_sd);

    public event MoveFailedDelegate MoveFailedEvent;

    internal void FireMoveFailedEvent(ScenarioDetail a_sd, MoveResult a_result)
    {
        MoveFailedEvent?.Invoke(a_result, a_sd);
    }
    #endregion

    #region MoveFinishedEvent
    public delegate void MoveFinishedDelegate(MoveResult a_result);

    public event MoveFinishedDelegate MoveFinishedEvent;

    internal void FireMoveFinishedEvent(ScenarioDetail a_sd, MoveResult a_result)
    {
        MoveFinishedEvent?.Invoke(a_result);
    }
    #endregion

    #region SimulationFailedEvent
    public delegate void SimulationValidationFailureDelegate(ScenarioDetail sd, ScenarioDetail.SimulationValidationException sve, ScenarioBaseT t);

    public event SimulationValidationFailureDelegate SimulationFailedEvent;

    internal void FireSimulationValidationFailureEvent(ScenarioDetail sd, ScenarioDetail.SimulationValidationException sve, ScenarioBaseT t)
    {
        SimulationFailedEvent?.Invoke(sd, sve, t);
    }
    #endregion

    #region SimulationCancellation
    public delegate void SimulationCancelledDelegate();
    /// <summary>
    /// Occurs when a simulation action is cancelled
    /// </summary>
    public event SimulationCancelledDelegate SimulationCancelled;
    /// <summary>
    /// Invokes the SimulationCancelled to notify all listeners that the simulation action was cancelled
    /// </summary>
    internal void FireSimulationCancelledEvent()
    {
        SimulationCancelled?.Invoke();
    }
    public delegate void SimulationCancelBeginDelegate();
    /// <summary>
    /// Occurs when a simulation action is cancelled
    /// </summary>
    public event SimulationCancelBeginDelegate SimulationBeginCancellation;
    /// <summary>
    /// Invokes the SimulationBeginCancellation to notify all listeners that the simulation cancellation process has begun 
    /// </summary>
    internal void FireSimulationCancelBeginEvent()
    {
        SimulationBeginCancellation?.Invoke();
    }
    #endregion
    #region TransmissionFailureEvent
    public delegate void TransmissionFailureDelegate(Exception e, PTTransmission t, SystemMessage message);

    public event TransmissionFailureDelegate TransmissionFailureEvent;

    internal void FireTransmissionFailureEvent(Exception e, PTTransmission t, SystemMessage message)
    {
        TransmissionFailureEvent?.Invoke(e, t, message);
    }
    #endregion
    #endregion

    #region KPI events
    public delegate void KPIChangedDelegate(KpiController kpiController, ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simType, PTTransmission a_t);

    public event KPIChangedDelegate KPIChangedEvent;

    internal void FireKPIChangedEvent(KpiController kpiController, ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simType, PTTransmission a_t)
    {
        KPIChangedEvent?.Invoke(kpiController, a_sd, a_simType, a_t);
    }
    #endregion KPI

    #region CTP
    public delegate void CTPDelegate(ScenarioBaseT a_t, Transmissions.CTP.Ctp a_ctp, Job a_job, Exception a_ctpException);

    public event CTPDelegate CTPEvent;

    internal void FireCTPEvent(ScenarioBaseT a_t, Transmissions.CTP.Ctp a_ctp, Job a_job, Exception a_ctpException)
    {
        CTPEvent?.Invoke(a_t, a_ctp, a_job, a_ctpException);
    }
    #endregion

    #region Export Events
    public delegate void ExportScenarioDelegate(ScenarioDetail sd, ScenarioDetailExportT t, ScenarioSummary ss);

    public event ExportScenarioDelegate ExportScenarioEvent;

    internal void FireExportScenarioEvent(ScenarioDetail sd, ScenarioDetailExportT t, ScenarioSummary ss)
    {
        ExportScenarioEvent?.Invoke(sd, t, ss);
    }

    public delegate void ImportScenarioCompleteDelegate(ImportT t);

    public event ImportScenarioCompleteDelegate ImportScenarioCompleteEvent;

    internal void FireImportScenarioCompleteEvent(ImportT t)
    {
        ImportScenarioCompleteEvent?.Invoke(t);
    }
    #endregion

    #region Extra Services special events
    public event Action<ScenarioBaseT> ScenarioDetailTransmissionProcessedEvent;

    internal void FireScenarioDetailTransmissionProcessedEvent(ScenarioBaseT a_t)
    {
        ScenarioDetailTransmissionProcessedEvent?.Invoke(a_t);
    }
    #endregion

    #region ShopView Events
    public delegate void ShopViewResourceOptionsUpdatedDelegate(ShopViewResourceOptionsManager mgr);

    public event ShopViewResourceOptionsUpdatedDelegate ShopViewResourceOptionsUpdatedEvent;

    internal void FireShopViewResourceOptionsUpdatedEvent(ScenarioDetail a_sd, ShopViewResourceOptionsManager mgr)
    {
        ShopViewResourceOptionsUpdatedEvent?.Invoke(mgr);
    }
    #endregion

    #region UserField events
    public event Action<UserField.EUDFObjectType> UDFDataChangedEvent;
    #endregion

    public delegate void ScenarioIsolationDelegate(ScenarioIsolateT a_t);

    public event ScenarioIsolationDelegate ScenarioIsolatedEvent;

    internal void FireScenarioIsolationEvent(ScenarioIsolateT a_t)
    {
        ScenarioIsolatedEvent?.Invoke(a_t);
    }

    /// <summary>
    /// Data objects have been changed, update listeners with the changes
    /// </summary>
    public event Action<IScenarioDataChanges> ScenarioDataChangesEvent;

    internal void FireScenarioDataChangedEvent(IScenarioDataChanges a_dataChanges)
    {
        ScenarioDataChangesEvent?.Invoke(a_dataChanges);
    }

    /// <summary>
    /// Scenario Readonly Change
    /// </summary>
    public event Action<bool> ScenarioReadonlyChangeEvent;

    internal void FireScenarioReadonlyChangeEvent(bool a_readonly)
    {
        ScenarioReadonlyChangeEvent?.Invoke(a_readonly);
    }
    public delegate void ProductionScenarioConversionDelegate(ScenarioEvents a_se, ScenarioUndoEvents a_sue, BaseId a_currentProdId, BaseId a_promotedScenarioId, bool a_autoDelete);

    public event ProductionScenarioConversionDelegate BeginPromoteScenarioEvent;

    public void FireScenarioPromotionEvent(ScenarioUndoEvents a_sue, BaseId a_currentProdId, BaseId a_promotedScenarioId, bool a_autoDelete)
    {
        BeginPromoteScenarioEvent?.Invoke(this, a_sue, a_currentProdId, a_promotedScenarioId, a_autoDelete);
    }
}