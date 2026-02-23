using System.Collections;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Localization;
using PT.ERPTransmissions;
using PT.GanttDotNet;
using PT.PackageDefinitionsUI.Controls.Move;
using PT.Scheduler;
using PT.SchedulerData;
using PT.SchedulerDefinitions;
using PT.SchedulerExtensions.Activity;
using PT.Transmissions;
using PT.UIDefinitions;

using SchedulerData.Interfaces;

using BaseResource = PT.Scheduler.BaseResource;

namespace PT.UI.ScenarioViewer;

/// <summary>
/// Contains the events that respond to the Listener class an operate on controls in ScenarioViewer.
/// </summary>
public partial class ScenarioViewer : ILocalizable, IScenarioViewerSimulationComplete
{
    #region Scenario
    public void ScenarioCleared(ScenarioDetailClearT t)
    {
        //TODO: V12
        //if (t.ClearJobs)
        //{
        //    GetJobsControl(false)?.ClearGrid();
        //    GetTemplatesControl(false)?.ClearGrid();
        //    GetMaterialsControl(false)?.ClearGrid();
        //}

        //if (t.ClearResources)
        //{
        //    GanttView.ClearResources();
        //    //TODO need to update nbr used column in Capability table.  Do later when we rework how those tables are stored.
        //    if (GetCapacityPlanningControl(false) != null)
        //    {
        //        GetCapacityPlanningControl(false).Clear();
        //    }
        //}

        //if (t.ClearDepartments)
        //{
        //    GanttView?.ClearDepartments();
        //    if (GetCapacityPlanningControl(false) != null)
        //    {
        //        GetCapacityPlanningControl(false).Clear();
        //    }
        //}

        //if (t.ClearPlants)
        //{
        //    GanttView?.Clear();
        //    if (GetCapacityPlanningControl(false) != null)
        //    {
        //        GetCapacityPlanningControl(false).Clear();
        //    }
        //}

        ////Clear capacity intervals after Resources or else references to Resoures used in setting up the Capacity Intervals in the Gantt hit null Resources.
        //if (t.ClearCapacityIntervals)
        //{
        //    GanttView?.ReloadCapacityIntervals();
        //}

        //if (t.ClearRecurringCapacityIntervals)
        //{
        //    GanttView?.ReloadCapacityIntervals();
        //}

        if (t.ClearPurchaseToStocks)
        {
            //taken care of by time adjustment
        }

        if (t.ClearInventories)
        {
            //taken care of by time adjustment
        }

        if (t.ClearItems)
        {
            //taken care of by time adjustment
        }

        if (t.ClearWarehouses)
        {
            //taken care of by time adjustment
        }
    }
    #endregion

    #region Transmission Failures
    /// <summary>
    /// Logs the exception and exit the application if fatal or non CES Exception.
    /// </summary>
    /// <param name="e"></param>
    public static void UnhandledExceptionHandler(Exception e)
    {
        m_exceptionManager.UnhandledException(e);
    }
    #endregion

    #region ScenarioDetail Objects
    public void CapacityIntervalsPurged(DateTimeOffset purgeTime)
    {
        //TODO: V12
        try
        {
            //GanttView.PurgeCapacityIntervalsFromThePast(purgeTime);
        }
        catch (Exception e)
        {
            UnhandledExceptionHandler(e);
        }
    }

    internal event ScenarioEvents.ShopViewResourceOptionsUpdatedDelegate ShopViewResourceOptionsUpdatedEvent;

    internal void ShopViewResourceOptionsUpdated(ShopViewResourceOptionsManager mgr)
    {
        try
        {
            ShopViewResourceOptionsUpdatedEvent?.Invoke(mgr);
        }
        catch (Exception e)
        {
            UnhandledExceptionHandler(e);
        }
    }

    /// <summary>
    /// Update the gantt since block data has changed.
    /// </summary>
    public async void BlocksChanged()
    {
        using (BackgroundLock asyncLock = new (m_scenarioInfo.ScenarioId))
        {
            asyncLock.KeepExclusive("BlocksChanged");
            await asyncLock.RunLockCode(this, BlocksChanged);
        }
    }

    private void BlocksChanged(Scenario a_s, ScenarioDetail a_sd, params object[] a_params)
    {
        try
        {
            if (GanttUtilities.Initialized)
            {
                GanttUtilities.GanttView.ProcessSimulationComplete(a_sd, new HashSet<BaseId>());
            }
        }
        catch (Exception e)
        {
            UnhandledExceptionHandler(e);
        }
    }

    public delegate void MoveCompleteHandler(ScenarioDetail a_sd, MoveResult a_result);

    public event MoveCompleteHandler MoveComplete;

    internal void DisplayMoveMessages(MoveResult a_result, ScenarioDetail a_sd)
    {
        string uiMessage = "";

        //Create a message from the moveResult
        using (IEnumerator<MoveBlockProblems> moveFailures = a_result.GetEnumerator())
        {
            while (moveFailures.MoveNext())
            {
                MoveBlockProblems currentBlock = moveFailures.Current;
                using (IEnumerator<MoveProblem> currentProblem = currentBlock.GetEnumerator())
                {
                    while (currentProblem.MoveNext())
                    {
                        ResourceKey resKey = a_result.ScenarioDetailMoveT.ToResourceKey;
                        if (currentProblem.Current is ActivityMoveProblem)
                        {
                            ActivityMoveProblem actProblem = (ActivityMoveProblem)currentProblem.Current;
                            using (IEnumerator<(InternalActivity, int)> activityList = actProblem.GetEnumerator())
                            {
                                while (activityList.MoveNext())
                                {
                                    (InternalActivity Act, int rrIdx) actInfo = activityList.Current;
                                    string message = string.Format("{0} could not be moved. {1}".Localize(), actInfo.Act.GetReadableActivityName(), TranslateMoveProblemEnum(actProblem.MoveProblemEnum, actInfo.Act, actInfo.rrIdx, resKey, a_sd));
                                    uiMessage += Environment.NewLine + message + Environment.NewLine;
                                }
                            }
                        }
                        else
                        {
                            uiMessage += Environment.NewLine + TranslateMoveProblemEnum(currentProblem.Current.MoveProblemEnum, null, 0, resKey, a_sd);
                        }
                    }
                }
            }
        }

        if (uiMessage != "")
        {
            m_messageProvider.ShowMessage(uiMessage);
        }
    }

    internal void DisplayMoveFailures(MoveResult a_moveResult, BaseResource a_resource)
    {
        if (SystemController.CurrentUserId != a_moveResult.ScenarioDetailMoveT.Instigator)
        {
            //Only show move failures to the user who performed the move.
            return;
        }

        MoveFailureEnum[] failureEnums = a_moveResult.GetFailureReasons();
        if (failureEnums.Length == 0)
        {
            return;
        }

        string errorMessage = "";
        string uiMessage;
        //Create a message from the failure reasons
        foreach (MoveFailureEnum reason in failureEnums)
        {
            errorMessage += TranslateMoveFailureEnum(reason);
        }

        if (a_resource != null)
        {
            uiMessage = string.Format("The move to resource '{0}' couldn't be performed. {1}".Localize(), a_resource.Name, errorMessage);
        }
        else
        {
            uiMessage = string.Format("The move couldn't be performed. {0}".Localize(), errorMessage);
        }

        if (uiMessage != "")
        {
            uiMessage = Environment.NewLine + uiMessage;
            m_messageProvider.ShowMessage(uiMessage);
        }
    }

    /// <summary>
    /// Translates a MoveProblemEnum into a reason message for UI display.
    /// </summary>
    private string TranslateMoveProblemEnum(MoveProblemEnum a_moveFailReason, InternalActivity a_act, int a_rrIdx, ResourceKey a_resKey, ScenarioDetail a_sd)
    {
        string reason = "";
        switch (a_moveFailReason)
        {
            case MoveProblemEnum.InProduction:
                reason = "It is in production.".Localize();
                break;
            case MoveProblemEnum.LockedToRes:
                reason = "It is locked on its current resource.".Localize();
                break;
            case MoveProblemEnum.FromResDisallowDragAndDrop:
                reason = "Source resource doesn't allow moves.".Localize();
                break;
            case MoveProblemEnum.NotCompatibleWithMergeBatch:
                reason = "The activity can't be merged with the batch because the from and to batches aren't compatible. For instance they have different resource requirements.".Localize();
                break;
            case MoveProblemEnum.NotEligibleForBatchResReqCyclesGT1:
                reason = "The batch resource is not eligible for the activity because they can't be scheduled within a single cycle. The activity's required finish quantity is greater than its quantity per cycle.".Localize();
                break;
            case MoveProblemEnum.NotEligibleForBatchResReqFinQtyGTBatchVolume:
                reason = "The batch resource is not eligible for the activity because they can't be scheduled within a single cycle. The activity's required finish quantity is greater than the resource's batch volume.".Localize();
                break;
            case MoveProblemEnum.NotEligibleOnTargetRes:
                //Determine eligibility failures.
                MoveValidation moveValidation = new (m_mainForm);
                Resource resource = a_sd.PlantManager.GetResource(a_resKey);
                string minMaxQty = "";
                if (!moveValidation.IsCapableBasedOnMinMaxQtys(resource, a_act.Operation as ResourceOperation))
                {
                    minMaxQty = "MinMaxQty is not eligible on target resource".Localize();
                }

                string attributes = moveValidation.IsCapableBasedOnAttributeNumberRange(resource, a_act.Operation as ResourceOperation);
                string capabilities = moveValidation.DetermineInvalidResourceString(a_act, a_rrIdx, resource);

                if (minMaxQty != "" || attributes != "" || capabilities != "")
                {
                    if (minMaxQty != null)
                    {
                        reason = minMaxQty + Environment.NewLine;
                    }

                    if (attributes != "")
                    {
                        reason += attributes + Environment.NewLine;
                    }

                    if (capabilities != "")
                    {
                        reason += "Missing Capabilities:".Localize() + " " + capabilities + Environment.NewLine;
                    }

                    return reason;
                }

                //Else
                reason = "It is not eligible on target resource.".Localize();
                break;
            case MoveProblemEnum.AlternatePathMovesMustHave1LeadActivity:
                reason = "Alternate Path Moves only support a single lead activity. Manufacturing Orders with split activities cannot have their Alternate Path changed.".Localize();
                break;
            case MoveProblemEnum.AlternatePathNotFound:
                reason = "An alternate path with the specified external id couldn't be found.".Localize();
                break;
            case MoveProblemEnum.AlternatePathMustDifferFromCurrentPath:
                reason = "The AlternatePath move was to the same path. When an AlternatePath move is performed, the current path and new AlternatePath must differ. The behavior of a regular move differs significantly from an AlternatePath move. In an AlternatePath move, the entire MO is rescheduled.".Localize();
                break;
            default:
                reason = "The reason is not known".Localize();
                break;
        }

        return reason;
    }

    /// <summary>
    /// Translates a MoveFailureEnum into a reason message for UI display.
    /// </summary>
    private static string TranslateMoveFailureEnum(MoveFailureEnum a_moveFailReason)
    {
        string reason;
        switch (a_moveFailReason)
        {
            case MoveFailureEnum.JobsFailedToSchedule:
                reason = "Jobs failed to schedule after the move. It is recommended to perform an undo so the schedule integrity is not compromised.";
                break;
            case MoveFailureEnum.ToResDisallowsDragAndDrop:
                reason = "The target resource does not allow drag and drop.";
                break;
            case MoveFailureEnum.ToResNotActive:
                reason = "The target resource is not active.";
                break;
            case MoveFailureEnum.MoveToResUsedByOtherReq:
                reason = "The move couldn't be performed because the resource the block was dropped on was already being used for a different resource requirement of the same batch.";
                break;
            default:
                //Most reasons are not useful to the user. It means there is a bug causing a failed move.
                reason = "No Activities moved, there was at least one error.";
                break;
        }

        return Localizer.GetString(reason);
    }

    public async void MoveFinished(MoveResult a_result)
    {
        using (BackgroundLock asyncLock = new (m_scenarioInfo.ScenarioId))
        {
            await asyncLock.RunLockCode(this, MoveFinished, a_result);
        }
    }

    private void MoveFinished(Scenario a_s, ScenarioDetail a_sd, params object[] a_params)
    {
        MoveResult result = (MoveResult)a_params[0];
        MoveComplete?.Invoke(a_sd, result);

        if (SystemController.CurrentUserId != result.ScenarioDetailMoveT.Instigator)
        {
            return;
        }

        BaseResource res = a_sd.PlantManager.GetResource(result.ScenarioDetailMoveT.ToResourceKey.Resource);

        if (result.Failed)
        {
            try
            {
                BlockKeyList keyList = new ();
                using (IEnumerator<MoveBlockKeyData> iterator = result.ScenarioDetailMoveT.GetEnumerator())
                {
                    while (iterator.MoveNext())
                    {
                        keyList.Add(iterator.Current.BlockKey);
                    }
                }

                //TODO: V12
                //GanttView.ProcessSimulateComplete(a_sd, keyList);
            }
            catch (Exception e)
            {
                UnhandledExceptionHandler(e);
            }
            finally
            {
                MultiLevelHourglass.TurnOff();
            }
        }
    }

    public void SimulationFailed(ScenarioDetail.SimulationValidationException se, ScenarioBaseT t)
    {
        try
        {
            MultiLevelHourglass.TurnOn();
            m_messageProvider.ShowMessage(string.Format("Simulation Failed.  {0}".Localize(), se.Message));
        }
        catch (Exception e)
        {
            UnhandledExceptionHandler(e);
        }
        finally
        {
            MultiLevelHourglass.TurnOff();
        }
    }
    #endregion

    #region Dock Scheduling
    public void PurchaseToStockMoved(PurchaseToStock pts, PurchaseToStockMoveT t)
    {
        try
        {
            //if (!this.ultraDockManager1.PaneFromControl(this.DockSchedulingPanel).Closed && GetDocksGanttControl(false) != null)
            //{
            //    GetDocksGanttControl(false).PurchaseToStockMoved(pts, t);
            //}
        }
        catch (Exception e)
        {
            m_exceptionManager.UnhandledException(e);
        }
    }

    public void PurchaseToStockMoveFailed(PurchaseToStock pts, PurchaseToStockMoveT t)
    {
        try
        {
            //If this user initiated the move then we need to undo it.
            if (SystemController.CurrentUserId == t.Instigator)
            {
                //if (!this.ultraDockManager1.PaneFromControl(this.DockSchedulingPanel).Closed && GetDocksGanttControl(false) != null)
                //{
                //    GetDocksGanttControl(false).PurchaseToStockMoveFailed(pts, t);
                //}
            }
        }
        catch (Exception e)
        {
            m_exceptionManager.UnhandledException(e);
        }
    }

    internal void WarehouseChanges(ArrayList added, ArrayList updated, ArrayList deleted)
    {
        try
        {
            //if (!this.ultraDockManager1.PaneFromControl(this.DockSchedulingPanel).Closed && GetDocksGanttControl(false) != null)
            //{
            //    GetDocksGanttControl(false).WarehouseChanges(added, updated, deleted);
            //}
        }
        catch (Exception e)
        {
            m_exceptionManager.UnhandledException(e);
        }
    }
    #endregion Dock Scheduling

    public event SimulationCompleteHandler SimulationComplete;

    public void ScenarioIsolated(ScenarioIsolateT a_t)
    {
        bool? isolateImport = null;
        if (a_t.IsolateImportSet)
        {
            isolateImport = a_t.IsolateImport;
        }

        bool? isolateClock = null;
        if (a_t.IsolateClockAdvanceSet)
        {
            isolateClock = a_t.IsolateClockAdvance;
        }

        if (a_t.IsolateImportSet) { }
    }

    /// <summary>
    /// Update the scenario label to indicate this scenario has processed the import.
    /// </summary>
    public void ImportComplete(ImportT a_t)
    {
        try { }
        catch (Exception err)
        {
            m_exceptionManager.UnhandledException(err);
        }
    }
}