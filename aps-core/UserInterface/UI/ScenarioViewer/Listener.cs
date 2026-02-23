using System.Collections;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.PackageDefinitionsUI;
using PT.Scheduler;
using PT.Transmissions;
using PT.Transmissions.Extensions;

namespace PT.UI.ScenarioViewer;

/// <summary>
/// Listens to events that effect the ScenarioViewer controls.
/// </summary>
public class ScenarioViewerListener
{
    private readonly IMainForm m_mainForm;
    private readonly ScenarioViewer m_scenarioViewer; //Needed for invoking delegates on the UI thread.

    #region Construction
    public ScenarioViewerListener(ScenarioViewer aScenarioViewer, IMainForm a_mainForm)
    {
        m_mainForm = a_mainForm;
        m_scenarioViewer = aScenarioViewer;
    }
    #endregion

    #region Delegate Declarations
    //THESE ARE NOT NEEDED IF THEY ARE THE SAME AS THEIR ORIGINAL DEFINITION IN SCENARIO EVENTS
    //User
//		public delegate void UserDefaultDelegate(User u, UserManager um, UserDefaultT t);
//		public delegate void UserCopyDelegate(User u, UserManager um, UserCopyT t);
//		public delegate void UserChangeDelegate(User u, UserChangeT t);
//		public delegate void UserDeleteDelegate(User u, UserDeleteT t);
    //ScenarioEvents
    public delegate void ScenarioHistoryNewDelegate(ArrayList newHistories);

    public delegate void UndoLoadedDelegate(ScenarioUndoT t);

    public delegate void UndoBeginDelegate(ScenarioUndoT t);

    public delegate void UndoEndDelegate(ScenarioUndoT t);

    public delegate void UndoSetChangedDelegate(BaseId a_scenarioBaseId, ScenarioBaseT a_T);

    public delegate void CapacityIntervalsPurgedDelegate(DateTimeOffset a_purgeTime);

    //System
    public delegate void MoveFailedDelegate(MoveResult result);

    public delegate void MoveFinishedDelegate(MoveResult result);

    public delegate void SimulationFailedDelegate(ScenarioDetail.SimulationValidationException e, ScenarioBaseT t);

    public delegate void ScenarioOptionsChangedDelegate(SystemOptionsT t);

    //Scenario Details
    public delegate void ObjectNewDelegate(BaseIdObject b, int connection);

    public delegate void ObjectDeleteDelegate(BaseIdObject b, int connection);

    public delegate void CapabilityDeleteDelegate(Capability c, int connection, ArrayList resources);

    public delegate void ResourceDeleteDelegate(Resource r, int connection, ArrayList capabilities);

    public delegate void JobsDeleteDelegate(ArrayList j);

    public delegate void JobsUpdateDelegate(List<Job> addedObjects, List<Job> updatedObjects, List<Job> deletedObjects, JobManager jobManager);

    public delegate void ObjectChangesDelegate1(ArrayList addedObjects, ArrayList updatedObjects, ArrayList deletedObjects);

    public delegate void ObjectChangesDelegate2(ArrayList addedObjects, ArrayList updatedObjects, ArrayList deletedObjects, ArrayList affectedObjects);

    public delegate void ObjectChangesDelegate3(ArrayList addedObjects, ArrayList updatedObjects, ArrayList deletedObjects, ArrayList affectedObjects, ArrayList affectedObjects2);

    public delegate void CapacityIntervalsChangedDelegate(CalendarResourcesCollection calendarResources);

    public delegate void CapacityIntervalsChangedDelegate2(CalendarResourcesCollection calendarResources, ArrayList deletedCIs, ArrayList updatedCIs, ArrayList addedCIs);

    public delegate void JobResourceEligibilityRecomputeDelegate();

    public delegate void TriggerSimulationCompleteDelegate(ScenarioBaseT a_t, long a_simId);

    //GanttViewLayout		
    public delegate void GanttViewSetAddDelegate(GanttViewerPane pane, GanttViewSet ganttViewSet);

    public delegate void GanttViewAddDelegate(GanttViewerPane pane, GanttViewSet ganttViewSet, GanttView ganttView);

    public delegate void GanttViewRowAddDelegate(GanttViewerPane pane, GanttViewSet ganttViewSet, GanttView ganttView, GanttViewRow row);

    public delegate void GanttViewSetRemoveDelegate(GanttViewerPane pane, GanttViewSet ganttViewSet);

    public delegate void GanttViewRemoveDelegate(GanttViewerPane pane, GanttViewSet ganttViewSet, GanttView ganttView);

    public delegate void GanttViewRowRemoveDelegate(GanttViewerPane pane, GanttViewSet ganttViewSet, GanttView ganttView, GanttViewRow row);

    public delegate void GanttViewSetChangedDelegate(GanttViewerPane pane, GanttViewSet ganttViewSet);

    public delegate void GanttViewChangedDelegate(GanttViewerPane pane, GanttViewSet ganttViewSet, GanttView ganttView);

    public delegate void GanttViewRowChangedDelegate(GanttViewerPane pane, GanttViewSet ganttViewSet, GanttView ganttView, GanttViewRow row);

    //Simulation
    public delegate void OptimizeCompleteDelegate(ScenarioDetail sd, ScenarioDetailOptimizeT t, bool thisUserInstigated);

    public delegate void SimulationStartDelegate(ScenarioBaseT t, ScenarioDetail.SimulationType simulationType);

    public delegate void SimulationProgressDelegate(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, double a_percentComplete, SimulationProgress.Status a_status, bool a_thisUserInstigated, long a_simId);

    public delegate void SimulationProgressMRPDelegate(SimulationProgress.Status a_status);

    public delegate void BlocksChangedDelegate();

    //KPI
    public delegate void KPIChangedDelegate(KpiController kpiController, ScenarioDetail.SimulationType a_simType, PTTransmission a_t);

    //Status bar
    public delegate void StatusBarDetailsUpdateDelegate(string aMessage);
    #endregion

    #region Add/Remove Listeners
    internal void AddScenarioEventListeners(ScenarioEvents a_se)
    {
        //System
        a_se.ScenarioDetailClearEvent += new ScenarioEvents.ScenarioDetailClearDelegate(se_ScenarioDetailClearEvent);

        //Simulation
        a_se.MoveFailedEvent += new PT.Scheduler.ScenarioEvents.MoveFailedDelegate(se_MoveFailedEvent);
        a_se.MoveFinishedEvent += new ScenarioEvents.MoveFinishedDelegate(se_MoveFinishedEvent);
        a_se.SimulationFailedEvent += new ScenarioEvents.SimulationValidationFailureDelegate(se_SimulationFailedEvent);
        a_se.CapacityIntervalsPurgedEvent += new PT.Scheduler.ScenarioEvents.CapacityIntervalsPurgedDelegate(se_CapacityIntervalsPurgedEvent);

        //Blocks
        a_se.BlocksChangedEvent += new PT.Scheduler.ScenarioEvents.BlocksChangedDelegate(se_BlocksChangedEvent);

        //PurchaseToStocks
        a_se.PurchaseToStockMovedEvent += new ScenarioEvents.PurchaseToStockMovedHandler(se_PurchaseToStockMovedEvent);
        a_se.PurchaseToStockMoveFailedEvent += new ScenarioEvents.PurchaseToStockMoveFailedHandler(se_PurchaseToStockMoveFailedEvent);

        //Import
        a_se.ImportScenarioCompleteEvent += new ScenarioEvents.ImportScenarioCompleteDelegate(se_ImportComplete);
    }

    /// <summary>
    /// Stop listening to the specified scenario for events since it's not being displayed.
    /// </summary>
    /// <param name="scenarioId"></param>
    internal void RemoveScenarioEventListeners(ScenarioEvents se)
    {
        se.ScenarioDetailClearEvent -= new ScenarioEvents.ScenarioDetailClearDelegate(se_ScenarioDetailClearEvent);

        //Simulation			
        se.MoveFailedEvent -= new ScenarioEvents.MoveFailedDelegate(se_MoveFailedEvent);
        se.MoveFinishedEvent -= new ScenarioEvents.MoveFinishedDelegate(se_MoveFinishedEvent);
        se.SimulationFailedEvent -= new ScenarioEvents.SimulationValidationFailureDelegate(se_SimulationFailedEvent);
        se.CapacityIntervalsPurgedEvent -= new PT.Scheduler.ScenarioEvents.CapacityIntervalsPurgedDelegate(se_CapacityIntervalsPurgedEvent);

        //Blocks
        se.BlocksChangedEvent -= new ScenarioEvents.BlocksChangedDelegate(se_BlocksChangedEvent);


        //PurchaseToStocks
        se.PurchaseToStockMovedEvent -= new ScenarioEvents.PurchaseToStockMovedHandler(se_PurchaseToStockMovedEvent);
        se.PurchaseToStockMoveFailedEvent -= new ScenarioEvents.PurchaseToStockMoveFailedHandler(se_PurchaseToStockMoveFailedEvent);

        //Import
        se.ImportScenarioCompleteEvent -= new ScenarioEvents.ImportScenarioCompleteDelegate(se_ImportComplete);
    }
    #endregion

    #region Ignore events code.
    private long ignoreEventsCount;

    private bool IgnoreEvents
    {
        get
        {
            if (ignoreEventsCount < 0)
            {
                throw new PTException("Ignore events variable out of sync.");
            }

            return ignoreEventsCount > 0 || m_mainForm.ClosingSoIgnoreAllEvents || !m_scenarioViewer.IsHandleCreated;
        }
    }

    public class IgnoreEventsDisposer : IDisposable
    {
        public IgnoreEventsDisposer(ScenarioViewerListener listener)
        {
            this.listener = listener;
            ++listener.ignoreEventsCount;
        }

        private readonly ScenarioViewerListener listener;

        #region IDisposable Members
        public void Dispose()
        {
            --listener.ignoreEventsCount;
        }
        #endregion
    }

    public IgnoreEventsDisposer GetIgnoreEventsDisposer()
    {
        return new IgnoreEventsDisposer(this);
    }
    #endregion

    private void se_MoveFailedEvent(MoveResult a_result, ScenarioDetail a_sd)
    {
        if (IgnoreEvents) // || t.ConnectionNbr!=SystemController.ConnectionNumber)
        {
            return;
        }

        m_scenarioViewer.BeginInvoke(new MoveFinishedDelegate(m_scenarioViewer.MoveFinished), a_result);
    }

    private void se_MoveFinishedEvent(MoveResult a_result)
    {
        if (IgnoreEvents) // || t.ConnectionNbr!=SystemController.ConnectionNumber)
        {
            return;
        }

        //TODO: Updating move finished should be done immediately. However the current locking process does not allow Invoke, because the UI thread is likely to be busy waitin on SD.
        m_scenarioViewer.BeginInvoke(new MoveFinishedDelegate(m_scenarioViewer.MoveFinished), a_result);
    }

    private void se_SimulationFailedEvent(ScenarioDetail sd, ScenarioDetail.SimulationValidationException e, ScenarioBaseT t)
    {
        if (IgnoreEvents || !t.SentByActiveUser())
        {
            return;
        }

        //TODO: Updating simulation failed should be done immediately. However the current locking process does not allow Invoke, because the UI thread is likely to be busy waitin on SD.
        m_scenarioViewer.BeginInvoke(new SimulationFailedDelegate(m_scenarioViewer.SimulationFailed), e, t);
    }

    private void se_CapacityIntervalsPurgedEvent(DateTime purgeTime)
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_scenarioViewer.BeginInvoke(new CapacityIntervalsPurgedDelegate(m_scenarioViewer.CapacityIntervalsPurged), purgeTime.ToDisplayTime());
    }

    private void se_ScenarioDetailClearEvent(ScenarioDetailClearT t)
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_scenarioViewer.BeginInvoke(new ScenarioEvents.ScenarioDetailClearDelegate(m_scenarioViewer.ScenarioCleared), t);
    }

    private void se_BlocksChangedEvent()
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_scenarioViewer.BeginInvoke(new BlocksChangedDelegate(m_scenarioViewer.BlocksChanged));
    }

    #region Purhase To Stocks
    private void se_PurchaseToStockMovedEvent(PurchaseToStock pts, PurchaseToStockMoveT t)
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_scenarioViewer.BeginInvoke(new ScenarioEvents.PurchaseToStockMovedHandler(m_scenarioViewer.PurchaseToStockMoved), pts, t);
    }

    private void se_PurchaseToStockMoveFailedEvent(PurchaseToStock pts, PurchaseToStockMoveT t)
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_scenarioViewer.BeginInvoke(new ScenarioEvents.PurchaseToStockMoveFailedHandler(m_scenarioViewer.PurchaseToStockMoveFailed), pts, t);
    }
    #endregion

    private void se_ScenarioIsolated(ScenarioIsolateT a_t)
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_scenarioViewer.BeginInvoke(new ScenarioEvents.ScenarioIsolationDelegate(m_scenarioViewer.ScenarioIsolated), a_t);
    }

    private void se_ImportComplete(ImportT a_t)
    {
        if (IgnoreEvents)
        {
            return;
        }

        m_scenarioViewer.BeginInvoke(new ScenarioEvents.ImportScenarioCompleteDelegate(m_scenarioViewer.ImportComplete), a_t);
    }
}