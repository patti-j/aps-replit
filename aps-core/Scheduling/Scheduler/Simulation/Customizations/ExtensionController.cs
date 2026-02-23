using PT.PackageDefinitions;
using PT.Scheduler.MRP;
using PT.Scheduler.PackageDefs;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.Scheduler.Schedule.InventoryManagement.MRP;
using PT.Scheduler.Schedule.Operation;
using PT.Scheduler.Simulation.Customizations;
using PT.Scheduler.Simulation.Customizations.MRP;
using PT.Scheduler.Simulation.Customizations.TransmissionPreprocessing;
using PT.Scheduler.Simulation.Extensions.Interfaces;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler.Simulation.Extensions;

/// <summary>
/// The purpose of this class is to interface the scheduler with the packages that implement extension points.
/// At the beginning of ever simulation, all of the elements are retrieved from package modules and stored by interface type.
/// It will handle running extensions when there is more then one module for an extension point.
/// For example it will handle multi-threading, sequencing, and consolidating return results.
/// </summary>
public class ExtensionController
{
    private readonly IPackageManager m_packageManager;
    private readonly ISystemLogger m_errorReporter;
    private readonly SchedulabilitySimulationInitializationHelper m_schedulabilityHelper = new ();
    private readonly RequiredCapacityChangeHelper m_requiredCapacityChanger = new ();
    private ScenarioDataChanges m_scenarioDataChanges = new ();

    internal ExtensionController(IPackageManager a_packageManager, ISystemLogger a_errorReporter)
    {
        m_packageManager = a_packageManager;
        m_errorReporter = a_errorReporter;
    }

    //Every extension interface stored by type
    private readonly List<ISimulationInitializationExtensionElement> m_initializationElements = new ();
    private readonly List<ISchedulingExtensionElement> m_schedulingElements = new ();
    private readonly List<IPostSimStageExtensionElement> m_postSimStageElements = new ();
    private readonly List<IMrpExtensionElement> m_mrpElements = new ();
    private readonly List<IMrpNeedDateResetExtensionElement> m_mrpNeedDateResetElements = new ();
    private readonly List<IOperationScheduledExtensionElement> m_operationScheduledElements = new ();
    private readonly List<IInventoryExtensionElement> m_inventoryElements = new ();
    private readonly List<IRequiredCapacityExtensionElement> m_requiredCapacityElements = new ();
    private readonly List<IEligibleMaterialExtensionElement> m_eligibleMaterialElements = new ();
    private readonly List<IMaterialAllocationExtensionElement> m_materialAllocationElements = new ();
    private readonly List<ILotExtensionElement> m_lotElements = new ();
    private readonly List<ISplitExtensionElement> m_splitElements = new ();
    private readonly List<IAutoJoinExtensionElement> m_autoJoinElements = new ();
    private readonly List<ICustomTableExtensionElement> m_customTableElements = new ();
    private readonly List<IPostJobTProcessingExtensionElement> m_jobPostProcessingElements = new ();
    private readonly List<ITransmissionProcessingExtensionElement> m_transmissionProcessingElements = new ();
    private readonly List<IAdvancedSchedulingExtensionElement> m_advancedSchedulingElements = new ();
    private readonly List<IAdditionalActionExtensionElement> m_additionalActionElements = new ();
    private readonly List<ITankExtensionElement> m_tankElements = new ();
    private readonly List<IScenarioExtensionsElement> m_scenarioElements = new ();
    private readonly List<IClockAdvanceExtensionElement> m_clockAdvanceExtensionElements = new();
    private readonly List<IProductionStatusExtensionElement> m_productionStatusExtensionElements = new ();
    private readonly List<IAdjustManufacturingOrderExtensionElement> m_adjustMOExtensionElements = new ();
    private readonly List<IOverrideCleanSpanExtensionElement> m_overrideCleanSpanExtensionElements = new();
    private readonly List<IFlagDelayExtensionElement> m_flagDelayExtensionElements = new();

    //Quick checks to see if a particular extension point needs to be called
    public bool RunPostSimStageExtensions => m_postSimStageElements.Count > 0;
    public bool RunJobCreationExtensions => m_mrpElements.Count > 0;
    public bool RunEndOfSimulationExtension => m_postSimStageElements.Count > 0;
    public bool RunSplitExtension => m_splitElements.Count > 0;
    public bool RunOperationScheduledExtension => m_operationScheduledElements.Count > 0;
    public bool RunEligibleMaterialExtension => m_eligibleMaterialElements.Count > 0;
    public bool RunAdjustShelfLifeExtension => m_lotElements.Count > 0;
    public bool RunTransmissionProcessingExtension => m_transmissionProcessingElements.Count > 0;
    public bool RunAutoJoinExtension => m_autoJoinElements.Count > 0;
    public bool RunAdvancedSchedulingExtension => m_advancedSchedulingElements.Count > 0;
    public bool RunSchedulingExtension => m_schedulingElements.Count > 0;
    public bool RunMrpExtension => m_mrpElements.Count > 0;
    public bool RunRequiredCapacityExtension => m_requiredCapacityElements.Count > 0;
    public bool RunTankExtension => m_tankElements.Count > 0;
    public bool RunScenarioExtension => m_scenarioElements.Count > 0;
    public bool RunClockAdvanceExtension => m_clockAdvanceExtensionElements.Count > 0;
    public bool RunProductionStatusExtension => m_productionStatusExtensionElements.Count > 0;
    public bool RunAdjustMOExtension => m_adjustMOExtensionElements.Count > 0;
    public bool RunOverrideCleanExtension => m_overrideCleanSpanExtensionElements.Count > 0;
    public bool RunFlagDelayExtension => m_flagDelayExtensionElements.Count > 0;
    public IScenarioDataChanges DataChanges => m_scenarioDataChanges;

    #region Enumerators
    //Each enumerator will return the elements in the order they should be run. The default is by Priority
    private IEnumerable<ISimulationInitializationExtensionElement> InitializationElementEnumerator
    {
        get { return m_initializationElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<ISchedulingExtensionElement> SchedulingEnumerator
    {
        get { return m_schedulingElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<IPostSimStageExtensionElement> PostSimStageEnumerator
    {
        get { return m_postSimStageElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<IMrpExtensionElement> MrpEnumerator
    {
        get { return m_mrpElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<IMrpNeedDateResetExtensionElement> MrpNeedDateResetEnumerator
    {
        get { return m_mrpNeedDateResetElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<IOperationScheduledExtensionElement> OperationScheduledEnumerator
    {
        get { return m_operationScheduledElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<IInventoryExtensionElement> InventoryEnumerator
    {
        get { return m_inventoryElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<IRequiredCapacityExtensionElement> RequiredCapacityEnumerator
    {
        get { return m_requiredCapacityElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<IEligibleMaterialExtensionElement> EligibleMaterialEnumerator
    {
        get { return m_eligibleMaterialElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<IMaterialAllocationExtensionElement> MaterialAllocationEnumerator
    {
        get { return m_materialAllocationElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<ILotExtensionElement> LotEnumerator
    {
        get { return m_lotElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<ISplitExtensionElement> SplitEnumerator
    {
        get { return m_splitElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<IAutoJoinExtensionElement> AutoJoinEnumerator
    {
        get { return m_autoJoinElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<ICustomTableExtensionElement> CustomTableElementEnumerator
    {
        get { return m_customTableElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<IPostJobTProcessingExtensionElement> PostJobTProcessingEnumerator
    {
        get { return m_jobPostProcessingElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<ITransmissionProcessingExtensionElement> TransmissionProcessingEnumerator
    {
        get { return m_transmissionProcessingElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<IAdvancedSchedulingExtensionElement> AdvancedSchedulingEnumerator
    {
        get { return m_advancedSchedulingElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<IAdditionalActionExtensionElement> AdditionalActionEnumerator
    {
        get { return m_additionalActionElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<ITankExtensionElement> TankEnumerator
    {
        get { return m_tankElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<IScenarioExtensionsElement> ScenarioExtensionsEnumerator
    {
        get { return m_scenarioElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<IClockAdvanceExtensionElement> ClockAdvanceExtensionsEnumerator
    {
        get { return m_clockAdvanceExtensionElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<IProductionStatusExtensionElement> ProductionStatusExtensionsEnumerator
    {
        get { return m_productionStatusExtensionElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<IAdjustManufacturingOrderExtensionElement> AdjustMOExtensionsEnumerator
    {
        get { return m_adjustMOExtensionElements.OrderBy(e => e.Priority); }
    }

    private IEnumerable<IOverrideCleanSpanExtensionElement> OverrideCleanSpanExtensionsEnumerator
    {
        get { return m_overrideCleanSpanExtensionElements.OrderBy(e => e.Priority); }
    } 
    
    private IEnumerable<IFlagDelayExtensionElement> FlagDelayExtensionsEnumerator
    {
        get { return m_flagDelayExtensionElements.OrderBy(e => e.Priority); }
    }
    #endregion Enumerators

    /// <summary>
    /// Retrieve all extension elements from modules and store them by type
    /// </summary>
    /// <param name="a_simType"></param>
    internal void InitializeRelevantElements()
    {
        m_initializationElements.Clear();
        m_schedulingElements.Clear();
        m_postSimStageElements.Clear();
        m_mrpElements.Clear();
        m_mrpNeedDateResetElements.Clear();
        m_operationScheduledElements.Clear();
        m_inventoryElements.Clear();
        m_requiredCapacityElements.Clear();
        m_eligibleMaterialElements.Clear();
        m_materialAllocationElements.Clear();
        m_lotElements.Clear();
        m_splitElements.Clear();
        m_autoJoinElements.Clear();
        m_customTableElements.Clear();
        m_jobPostProcessingElements.Clear();
        m_transmissionProcessingElements.Clear();
        m_advancedSchedulingElements.Clear();
        m_additionalActionElements.Clear();
        m_tankElements.Clear();
        m_scenarioElements.Clear();
        m_clockAdvanceExtensionElements.Clear();
        m_productionStatusExtensionElements.Clear();
        m_adjustMOExtensionElements.Clear();
        m_overrideCleanSpanExtensionElements.Clear();
        m_flagDelayExtensionElements.Clear();

        foreach (IExtensionModule extensionModule in m_packageManager.GetExtensionModules())
        {
            List<IExtensionElement> elements = extensionModule.GetExtensionElements(m_errorReporter);
            foreach (IExtensionElement element in elements)
            {
                if (element is ISimulationInitializationExtensionElement basicElement)
                {
                    m_initializationElements.Add(basicElement);
                }

                if (element is ISchedulingExtensionElement schedulingElement)
                {
                    m_schedulingElements.Add(schedulingElement);
                }

                if (element is IPostSimStageExtensionElement postSimStageElement)
                {
                    m_postSimStageElements.Add(postSimStageElement);
                }

                if (element is IMrpExtensionElement mrpElement)
                {
                    m_mrpElements.Add(mrpElement);
                }

                if (element is IMrpNeedDateResetExtensionElement mrpResetElement)
                {
                    m_mrpNeedDateResetElements.Add(mrpResetElement);
                }

                if (element is IOperationScheduledExtensionElement opScheduledElement)
                {
                    m_operationScheduledElements.Add(opScheduledElement);
                }

                if (element is IInventoryExtensionElement invElement)
                {
                    m_inventoryElements.Add(invElement);
                }

                if (element is IRequiredCapacityExtensionElement capacityElement)
                {
                    m_requiredCapacityElements.Add(capacityElement);
                }

                if (element is IEligibleMaterialExtensionElement eligibleMaterialElement)
                {
                    m_eligibleMaterialElements.Add(eligibleMaterialElement);
                }

                if (element is IMaterialAllocationExtensionElement matAllocElement)
                {
                    m_materialAllocationElements.Add(matAllocElement);
                }

                if (element is ILotExtensionElement lotElement)
                {
                    m_lotElements.Add(lotElement);
                }

                if (element is ISplitExtensionElement splitElement)
                {
                    m_splitElements.Add(splitElement);
                }

                if (element is IAutoJoinExtensionElement autoJoinElement)
                {
                    m_autoJoinElements.Add(autoJoinElement);
                }

                if (element is ICustomTableExtensionElement customTableElement)
                {
                    m_customTableElements.Add(customTableElement);
                }

                if (element is IPostJobTProcessingExtensionElement jobPostProcessingElement)
                {
                    m_jobPostProcessingElements.Add(jobPostProcessingElement);
                }

                if (element is ITransmissionProcessingExtensionElement transmissionProcessingElement)
                {
                    m_transmissionProcessingElements.Add(transmissionProcessingElement);
                }

                if (element is IAdvancedSchedulingExtensionElement advancedSchedulingElement)
                {
                    m_advancedSchedulingElements.Add(advancedSchedulingElement);
                }

                if (element is IAdditionalActionExtensionElement additionalActionElement)
                {
                    m_additionalActionElements.Add(additionalActionElement);
                }

                if (element is ITankExtensionElement tankElement)
                {
                    m_tankElements.Add(tankElement);
                }

                if (element is IScenarioExtensionsElement scenarioElement)
                {
                    m_scenarioElements.Add(scenarioElement);
                }

                if (element is IClockAdvanceExtensionElement clockAdvanceElement)
                {
                    m_clockAdvanceExtensionElements.Add(clockAdvanceElement);
                }

                if (element is IProductionStatusExtensionElement productionStatusElement)
                {
                    m_productionStatusExtensionElements.Add(productionStatusElement);
                }

                if (element is IAdjustManufacturingOrderExtensionElement adjustManufacturingOrderElement)
                {
                    m_adjustMOExtensionElements.Add(adjustManufacturingOrderElement);
                }

                if (element is IOverrideCleanSpanExtensionElement overrideCleanSpanExtensionElement)
                {
                    m_overrideCleanSpanExtensionElements.Add(overrideCleanSpanExtensionElement);
                }
                
                if (element is IFlagDelayExtensionElement flagDelayExtensionElement)
                {
                    m_flagDelayExtensionElements.Add(flagDelayExtensionElement);
                }
            }
        }
    }

    internal void SimulationInitialization(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simulationType, ScenarioBaseT a_transmission)
    {
        int multiThreadedCount = m_initializationElements.Count(e => e.MultiThreaded);
        if (multiThreadedCount > 1)
        {
            //There are at least two multi-threaded elements. These will run together after the single-threaded ones.
            List<Task> tasks = new ();
            foreach (ISimulationInitializationExtensionElement basicElement in InitializationElementEnumerator)
            {
                if (!basicElement.MultiThreaded)
                {
                    basicElement.SimulationInitialization(a_sd, m_schedulabilityHelper, a_simulationType, a_transmission);
                }
                else
                {
                    tasks.Add(new Task(() => basicElement.SimulationInitialization(a_sd, m_schedulabilityHelper, a_simulationType, a_transmission)));
                }
            }

            foreach (Task task in tasks)
            {
                task.Start();
            }

            Task.WaitAll(tasks.ToArray());
        }
        else
        {
            //Run them all single-threaded
            foreach (ISimulationInitializationExtensionElement basicElement in InitializationElementEnumerator)
            {
                basicElement.SimulationInitialization(a_sd, m_schedulabilityHelper, a_simulationType, a_transmission);
            }
        }

        m_schedulabilityHelper.Execute();
    }

    #region Scheduling
    internal long? IsSchedulable(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simType, long a_clock, long a_simulationClock, long a_simStartTicks, BaseResource a_res, BaseActivity a_activity)
    {
        int multiThreadedCount = m_initializationElements.Count(e => e.MultiThreaded);
        if (multiThreadedCount > 1)
        {
            List<Task<DateTime?>> tasks = new ();
            foreach (ISchedulingExtensionElement schedulingElement in SchedulingEnumerator)
            {
                if (!schedulingElement.MultiThreaded)
                {
                    DateTime? isSchedulableDelay = schedulingElement.IsSchedulable(a_sd, a_simType, a_clock, a_simulationClock, a_simStartTicks, a_res, a_activity);
                    if (isSchedulableDelay.HasValue)
                    {
                        return isSchedulableDelay.Value.Ticks;
                    }
                }
                else
                {
                    tasks.Add(new Task<DateTime?>(() => schedulingElement.IsSchedulable(a_sd, a_simType, a_clock, a_simulationClock, a_simStartTicks, a_res, a_activity)));
                }
            }

            //Now run the Multi-threaded extensions.
            CancellationTokenSource cts = new ();
            foreach (Task<DateTime?> task in tasks)
            {
                task.Start();
            }

            //Go through all tasks, as each finishes, check for false. If false, return, otherwise wait on the next finished task
            while (tasks.Count > 0)
            {
                Task<DateTime?> firstFinished = Task.WhenAny(tasks).Result;
                if (firstFinished.Result.HasValue)
                {
                    long delayedStartTime = firstFinished.Result.Value.Ticks;
                    tasks.Remove(firstFinished);
                    cts.Cancel();
                    //Wait for all tasks to cancel. Simulation gets stuck if we don't wait for these to finish.
                    Task.WaitAll(tasks.Cast<Task>().ToArray());
                    return delayedStartTime;
                }

                tasks.Remove(firstFinished);
            }
        }
        else
        {
            foreach (ISchedulingExtensionElement schedulingElement in SchedulingEnumerator)
            {
                DateTime? isSchedulableDelay = schedulingElement.IsSchedulable(a_sd, a_simType, a_clock, a_simulationClock, a_simStartTicks, a_res, a_activity);
                if (isSchedulableDelay.HasValue)
                {
                    return isSchedulableDelay.Value.Ticks;
                }
            }
        }

        //No delays
        return null;
    }

    internal long? IsSchedulable(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simType, long a_clock, long a_simulationClock, long a_simStartTicks, BaseResource a_res, BaseActivity a_activity, SchedulableInfo a_schedulableInfo)
    {
        foreach (ISchedulingExtensionElement schedulingElement in SchedulingEnumerator)
        {
            DateTime? isSchedulableDelay = schedulingElement.IsSchedulable(a_sd, a_simType, a_clock, a_simulationClock, a_simStartTicks,  a_res, a_activity, a_schedulableInfo);
            if (isSchedulableDelay.HasValue)
            {
                return isSchedulableDelay.Value.Ticks;
            }
        }

        //No delays
        return null;
    }

    internal long? IsSchedulablePostMaterialAllocation(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simType, long a_clock, long a_simulationClock, long a_simStartTicks, BaseResource a_res, BaseActivity a_activity, SchedulableInfo a_si, TransferQtyCompletionProfile a_transferQtyCompletionProfile)
    {
        foreach (ISchedulingExtensionElement schedulingElement in SchedulingEnumerator)
        {
            DateTime? isSchedulableDelay = schedulingElement.IsSchedulablePostMaterialAllocation(a_sd, a_simType, a_clock, a_simulationClock, a_simStartTicks, a_res, a_activity, a_si, a_transferQtyCompletionProfile);
            if (isSchedulableDelay.HasValue)
            {
                return isSchedulableDelay.Value.Ticks;
            }
        }

        //No delays
        return null;
    }

    internal bool? CanScheduleOnResource(ScenarioDetail a_sd, InternalResource a_resource, InternalActivity a_activity, ScenarioDetail.SimulationType a_simulationType, ScenarioBaseT a_transmission)
    {
        foreach (ISchedulingExtensionElement schedulingElement in SchedulingEnumerator)
        {
            bool? canSchedule = schedulingElement.CanScheduleOnResource(a_sd, a_resource, a_activity, a_simulationType, a_transmission);
            if (canSchedule.HasValue)
            {
                return canSchedule;
            }
        }

        return true;
    }

    /// <summary>
    /// Allows a customization to override the release date and tell the scheduler when to release the activity.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_activity"></param>
    /// <param name="a_proposedReleaseTicks">The original release date</param>
    /// <param name="a_simulationType"></param>
    /// <param name="a_transmission"></param>
    /// <returns>A new release date in ticks, or null to not modify the release</returns>
    internal long? AdjustActivityRelease(ScenarioDetail a_sd, InternalActivity a_activity, long a_proposedReleaseTicks, ScenarioDetail.SimulationType a_simulationType, ScenarioBaseT a_transmission)
    {
        long adjustedRelease = long.MaxValue;
        foreach (ISchedulingExtensionElement schedulingElement in SchedulingEnumerator)
        {
            long? modifiedRelease = schedulingElement.AdjustActivityRelease(a_sd, a_activity, a_proposedReleaseTicks, a_simulationType, a_transmission);
            if (modifiedRelease.HasValue)
            {
                adjustedRelease = Math.Min(adjustedRelease, modifiedRelease.Value);
            }
        }

        if (adjustedRelease != long.MaxValue)
        {
            return adjustedRelease;
        }

        return null;
    }

    internal InternalActivityDefs.productionStatuses? GetProductionStatus(InternalActivity a_act, InternalActivityDefs.productionStatuses a_currentProductionStatus)
    {
        foreach (IProductionStatusExtensionElement productionStatusExtensionElement in ProductionStatusExtensionsEnumerator)
        {
            return productionStatusExtensionElement.GetProductionStatus(a_act, a_currentProductionStatus);
        }

        return null;
    }

    internal RequiredSpanPlusClean OverrideCleanSpan(ScenarioDetail a_sd, InternalActivity a_previousAct, InternalActivity a_nextAct)
    {
        foreach (IOverrideCleanSpanExtensionElement overrideCleanSpanExtensionElement in OverrideCleanSpanExtensionsEnumerator)
        {
            return overrideCleanSpanExtensionElement.OverrideCleanSpan(a_sd, a_previousAct, a_nextAct);
        }

        return RequiredSpanPlusClean.s_notInit;
    }

    internal RequiredSpanPlusClean OverrideLastActCleanSpan(ScenarioDetail a_sd, InternalActivity a_lastAct, InternalResource a_primaryResource)
    {
        foreach (IOverrideCleanSpanExtensionElement overrideCleanSpanExtensionElement in OverrideCleanSpanExtensionsEnumerator)
        {
            return overrideCleanSpanExtensionElement.OverrideLastActCleanSpan(a_sd, a_lastAct, a_primaryResource);
        }

        return RequiredSpanPlusClean.s_notInit;
    }

    internal long? IsSchedulableNonPrimaryResourceRequirement(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simType, long a_clock, long a_simulationClock, ResourceSatiator[] a_resourceRequirementSatiators, int a_resourceRequirementIdx, BaseActivity a_activity, SchedulableInfo a_schedulableInfo)
    {
        foreach (IAdvancedSchedulingExtensionElement schedulingElement in AdvancedSchedulingEnumerator)
        {
            DateTime? isSchedulableDelay = schedulingElement.IsSchedulableNonPrimaryResourceRequirement(a_sd, a_simType, a_clock, a_simulationClock, a_resourceRequirementSatiators, a_resourceRequirementIdx, a_activity, a_schedulableInfo);
            if (isSchedulableDelay.HasValue)
            {
                return isSchedulableDelay.Value.Ticks;
            }
        }

        return null;
    }

    internal long? IsSchedulableContinuousFlowCheck(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simType, long a_clock, long a_simulationClock, BaseResource a_primaryResource, InternalOperation a_op, SchedulableInfo a_si)
    {
        foreach (IAdvancedSchedulingExtensionElement schedulingElement in AdvancedSchedulingEnumerator)
        {
            DateTime? isSchedulableDelay = schedulingElement.IsSchedulableContinuousFlowCheck(a_sd, a_simType, a_clock, a_simulationClock, a_primaryResource, a_op, a_si);
            if (isSchedulableDelay.HasValue)
            {
                return isSchedulableDelay.Value.Ticks;
            }
        }

        return null;
    }
    #endregion

    #region PostSimStageExtension
    private readonly PostSimStageChangeHelper m_changeHelper = new ();

    /// <summary>
    /// This is called after Simulation but before individual object's PostSimStageChange methods are called.
    /// </summary>
    /// <param name="a_simType">SimulationType</param>
    /// <param name="a_t">Transmission that initiated Sim</param>
    /// <param name="a_sd">ScenarioDetail</param>
    internal void PostSimStageSetup(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd, int a_curSimStageIdx, int a_finalSimStageIdx)
    {
        foreach (IPostSimStageExtensionElement postSimStageElement in PostSimStageEnumerator)
        {
            postSimStageElement.PostSimStageSetup(a_simType, a_t, a_sd, m_changeHelper, a_curSimStageIdx, a_finalSimStageIdx);
        }

        m_changeHelper.SchedulabilityChangeHelper.Execute();
    }

    /// <summary>
    /// Called after Simulation to change ResourceRequirement values.
    /// </summary>
    /// <param name="a_simType">SimulationType</param>
    /// <param name="a_t">Transmission that initiated Sim</param>
    /// <param name="a_sd">ScenarioDetail</param>
    /// <param name="a_mo">Manufacturing Order being changed</param>
    /// <param name="r_changable">Contains attributes that can be changed</param>
    internal void PostSimStageChangeRR(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd, InternalOperation a_op, ResourceRequirement a_rr, int a_curSimStageIdx, int a_finalSimStageIdx, out ChangableRRValues o_changable)
    {
        o_changable = null;

        foreach (IPostSimStageExtensionElement postSimStageElement in PostSimStageEnumerator)
        {
            postSimStageElement.PostSimStageChangeRR(a_simType, a_t, a_sd, a_op, a_rr, a_curSimStageIdx, a_finalSimStageIdx, out o_changable);
            if (o_changable != null)
            {
                //Only one element can set changeable values
                return;
            }
        }
    }

    /// <summary>
    /// Called after Simulation to change MO values.
    /// </summary>
    /// <param name="a_simType">SimulationType</param>
    /// <param name="a_t">Transmission that initiated Sim</param>
    /// <param name="a_sd">ScenarioDetail</param>
    /// <param name="a_mo">Manufacturing Order being changed</param>
    /// <param name="r_changable">Contains attributes that can be changed</param>
    internal void PostSimStageChangeMO(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd, ManufacturingOrder a_mo, int a_curSimStageIdx, int a_finalSimStageIdx, out ChangableMOValues o_changable)
    {
        o_changable = null;

        foreach (IPostSimStageExtensionElement postSimStageElement in PostSimStageEnumerator)
        {
            postSimStageElement.PostSimStageChangeMO(a_simType, a_t, a_sd, a_mo, a_curSimStageIdx, a_finalSimStageIdx, out o_changable);
            if (o_changable != null)
            {
                //Only one element can set changeable values
                return;
            }
        }
    }

    /// <summary>
    /// Called after Simulation to change Dept values. Also called after ChangeMO for all MOs have been called.
    /// </summary>
    /// <param name="a_simType">SimulationType</param>
    /// <param name="a_t">Transmission that initiated Sim</param>
    /// <param name="a_sd">ScenarioDetail</param>
    /// <param name="a_mo">Dept being changed</param>
    /// <param name="o_changable">Contains attributes that can be changed</param>
    internal void PostSimStageChangeDept(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd, Department a_dept, int a_curSimStageIdx, int a_finalSimStageIdx, out ChangableDeptValues o_changable)
    {
        o_changable = null;

        foreach (IPostSimStageExtensionElement postSimStageElement in PostSimStageEnumerator)
        {
            postSimStageElement.PostSimStageChangeDept(a_simType, a_t, a_sd, a_dept, a_curSimStageIdx, a_finalSimStageIdx, out o_changable);
            if (o_changable != null)
            {
                //Only one element can set changeable values
                return;
            }
        }
    }

    /// <summary>
    /// Called after Simulation and after objects' PostSimStageChange have been called.
    /// </summary>
    /// <param name="a_simType">SimulationType</param>
    /// <param name="a_t">Transmission that initiated Sim</param>
    /// <param name="a_sd">ScenarioDetail</param>
    internal void PostSimStageCleanup(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd, int a_curSimStageIdx, int a_finalSimStageIdx)
    {
        foreach (IPostSimStageExtensionElement postSimStageElement in PostSimStageEnumerator)
        {
            postSimStageElement.PostSimStageCleanup(a_simType, a_t, a_sd, a_curSimStageIdx, a_finalSimStageIdx);
        }
    }

    /// <summary>
    /// This is called after Simulation but before inidividual object's EndOfSimulationCustomization methods are called.
    /// </summary>
    /// <param name="a_simType">SimulationType</param>
    /// <param name="a_t">Transmission that initiated Sim</param>
    /// <param name="a_sd">ScenarioDetail</param>
    internal void EndOfSimulationSetup(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd)
    {
        PostSimStageSetup(a_simType, a_t, a_sd, -1, -1);
    }

    /// <summary>
    /// Called after Simulation to change ResourceRequirement values.
    /// </summary>
    /// <param name="a_simType">SimulationType</param>
    /// <param name="a_t">Transmission that initiated Sim</param>
    /// <param name="a_sd">ScenarioDetail</param>
    /// <param name="a_mo">Manufacturing Order being changed</param>
    /// <param name="r_changable">Contains attributes that can be changed</param>
    internal void EndOfSimulationChangeRR(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd, InternalOperation a_op, ResourceRequirement a_rr, out ChangableRRValues o_changable)
    {
        PostSimStageChangeRR(a_simType, a_t, a_sd, a_op, a_rr, -1, -1, out o_changable);
    }

    /// <summary>
    /// Called after Simulation to change MO values.
    /// </summary>
    /// <param name="a_simType">SimulationType</param>
    /// <param name="a_t">Transmission that initiated Sim</param>
    /// <param name="a_sd">ScenarioDetail</param>
    /// <param name="a_mo">Manufacturing Order being changed</param>
    /// <param name="r_changable">Contains attributes that can be changed</param>
    internal void EndOfSimStageChangeMO(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd, ManufacturingOrder a_mo, out ChangableMOValues o_changable)
    {
        PostSimStageChangeMO(a_simType, a_t, a_sd, a_mo, -1, -1, out o_changable);
    }

    /// <summary>
    /// Called after Simulation to change Dept values. Also called after ChangeMO for all MOs have been called.
    /// </summary>
    /// <param name="a_simType">SimulationType</param>
    /// <param name="a_t">Transmission that initiated Sim</param>
    /// <param name="a_sd">ScenarioDetail</param>
    /// <param name="a_mo">Dept being changed</param>
    /// <param name="o_changable">Contains attributes that can be changed</param>
    internal void EndOfSimulationChangeDept(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd, Department a_dept, out ChangableDeptValues o_changable)
    {
        PostSimStageChangeDept(a_simType, a_t, a_sd, a_dept, -1, -1, out o_changable);
    }

    /// <summary>
    /// Called after Simulation and after objects' EndOfSimulationChange have been called.
    /// </summary>
    /// <param name="a_simType">SimulationType</param>
    /// <param name="a_t">Transmission that initiated Sim</param>
    /// <param name="a_sd">ScenarioDetail</param>
    internal void EndOfSimulationCleanup(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd)
    {
        PostSimStageCleanup(a_simType, a_t, a_sd, -1, -1);
    }
    #endregion

    #region MRP
    internal bool? CanAllocate(ScenarioDetail a_sd, SupplyOrder a_supplyOrder, MrpDemand a_adjustment, decimal a_qty, bool a_allowPartialSupply)
    {
        foreach (IMrpExtensionElement mrpExtensionElement in MrpEnumerator)
        {
            bool? canAllocate = mrpExtensionElement.CanAllocate(a_sd, a_supplyOrder, a_adjustment, a_qty, a_allowPartialSupply);
            if (canAllocate.HasValue)
            {
                return canAllocate;
            }
        }

        return null;
    }

    internal bool? CanBatch(ScenarioDetail a_sd, SupplyOrder a_supplyOrder, MrpDemand a_adjustment, decimal a_qty, bool a_allowPartialSupply)
    {
        foreach (IMrpExtensionElement mrpExtensionElement in MrpEnumerator)
        {
            bool? canBatch = mrpExtensionElement.CanBatch(a_sd, a_supplyOrder, a_adjustment, a_qty, a_allowPartialSupply);
            if (canBatch.HasValue)
            {
                return canBatch;
            }
        }

        return null;
    }

    internal DateTime? ExistingOrderNeedDateReset(ScenarioDetail a_sd, Job a_existingJob, decimal a_supplyQty, decimal a_unallocatedQty, MrpDemand a_demandAdjustment, DateTime a_demandAdjustmentDate)
    {
        if (m_mrpNeedDateResetElements.Count > 0)
        {
            foreach (IMrpNeedDateResetExtensionElement mrpExtensionElement in MrpNeedDateResetEnumerator)
            {
                DateTime? resetDate = mrpExtensionElement.ExistingOrderNeedDateReset(a_sd, a_existingJob, a_supplyQty, a_unallocatedQty, a_demandAdjustment, a_demandAdjustmentDate);
                if (resetDate.HasValue)
                {
                    return resetDate;
                }
            }

            return null;
        }

        //Do default
        return MrpExtensionDefaults.ExistingOrderNeedDateResetDefault(a_sd, a_existingJob, a_supplyQty, a_unallocatedQty, a_demandAdjustment, a_demandAdjustmentDate);
    }

    internal bool? CanExistingSupplySatisfyDemand(ScenarioDetail a_sd, Job a_potentialSupplyJob, DateTime a_demandAdjustmentDate)
    {
        foreach (IMrpExtensionElement mrpExtensionElement in MrpEnumerator)
        {
            bool? canSupply = mrpExtensionElement.CanExistingSupplySatisfyDemand(a_sd, a_potentialSupplyJob, a_demandAdjustmentDate);
            if (canSupply.HasValue)
            {
                return canSupply;
            }
        }

        return null;
    }

    internal DateTime? PegAllocationNeedDate(ScenarioDetail a_sd, Job a_existingJob, MrpDemand a_mrpDemand)
    {
        foreach (IMrpExtensionElement mrpExtensionElement in MrpEnumerator)
        {
            DateTime? newDate = mrpExtensionElement.PegAllocationNeedDate(a_sd, a_existingJob, a_mrpDemand);
            if (newDate.HasValue)
            {
                return newDate;
            }
        }

        return null;
    }

    internal decimal? GetMaxOrderQty(ScenarioDetail a_sd, SupplyOrder a_supplyOrder, MrpDemand a_mrpDemand)
    {
        foreach (IMrpExtensionElement mrpExtensionElement in MrpEnumerator)
        {
            decimal? maxOrderQty = mrpExtensionElement.GetMaxOrderQty(a_sd, a_supplyOrder, a_mrpDemand);
            if (maxOrderQty.HasValue)
            {
                return maxOrderQty;
            }
        }

        return null;
    }

    //Job can not be nullable, if job is not notified this returns null
    internal Job CustomizeMrpJob(ScenarioDetail a_sd, SupplyOrder a_supplyOrder, Job a_newJob, ref ChangableJobValues r_changableValues, PTTransmission a_t)
    {
        Job customizedJob = a_newJob;
        foreach (IMrpExtensionElement mrpExtensionElement in MrpEnumerator)
        {
            Job changedJob = mrpExtensionElement.CustomizeMrpJob(a_sd, a_supplyOrder, customizedJob, ref r_changableValues, a_t);
            if (changedJob != null)
            {
                customizedJob = changedJob;
            }
        }

        return null;
    }

    internal PurchaseToStock CustomizeMrpPurchaseOrder(ScenarioDetail a_sd, SupplyOrder a_supplyOrder, PurchaseToStock a_pts, ref ChangeablePoValues r_changedValues)
    {
        r_changedValues = new ChangeablePoValues
        {
            NeedDate = a_pts.NeedDate.Subtract(TimeSpan.FromDays(7))
        };

        return a_pts;
    }

    internal decimal? GetUsableNonBatchOverproductionQty(ScenarioDetail a_sd, SupplyOrder a_supplyOrder, decimal a_totalDemandQty, MrpDemand a_mrpDemand)
    {
        foreach (IMrpExtensionElement mrpExtensionElement in MrpEnumerator)
        {
            decimal? usableQty = mrpExtensionElement.GetUsableNonBatchOverproductionQty(a_sd, a_supplyOrder, a_totalDemandQty, a_mrpDemand);
            if (usableQty.HasValue)
            {
                return usableQty;
            }
        }

        return null;
    }

    /// <summary>
    /// Overrides how much quantity a closed batch accept. The batch is closed because we are outside the batching window
    /// The default is production - total demand. This is the remainder produced by the current batch
    /// This can be used to match an enforced minimum production quantity.
    /// </summary>
    internal decimal? GetUsableBatchedOverproductionQty(ScenarioDetail a_sd, SupplyOrder a_supplyOrder, decimal a_producingQty, decimal a_totalDemandQty, MrpDemand a_mrpDemand)
    {
        foreach (IMrpExtensionElement mrpExtensionElement in MrpEnumerator)
        {
            decimal? usableQty = mrpExtensionElement.GetUsableBatchedOverproductionQty(a_sd, a_supplyOrder, a_producingQty, a_totalDemandQty, a_mrpDemand);
            if (usableQty.HasValue)
            {
                return usableQty;
            }
        }

        return null;
    }
    #endregion MRP

    #region OperationScheduledExtension
    //TODO: This may not be 100% threadsafe
    internal ChangeableOperationValues OperationScheduled(InternalOperation a_io, ScenarioDetail a_sd, long a_simClock)
    {
        ChangeableOperationValues changeableValues = new ();
        int multiThreadedCount = OperationScheduledEnumerator.Count(e => e.MultiThreaded);
        if (multiThreadedCount > 1)
        {
            //There are at least two multi-threaded elements. These will run together after the single-threaded ones.
            List<Task> tasks = new ();
            foreach (IOperationScheduledExtensionElement opScheduledElement in OperationScheduledEnumerator)
            {
                if (!opScheduledElement.MultiThreaded)
                {
                    opScheduledElement.OperationScheduled(a_io, changeableValues, a_sd, a_simClock);
                }
                else
                {
                    tasks.Add(new Task(() => opScheduledElement.OperationScheduled(a_io, changeableValues, a_sd, a_simClock)));
                }
            }

            foreach (Task task in tasks)
            {
                task.Start();
            }

            Task.WaitAll(tasks.ToArray());
        }
        else
        {
            //Run them all single-threaded
            foreach (IOperationScheduledExtensionElement opScheduledElement in OperationScheduledEnumerator)
            {
                opScheduledElement.OperationScheduled(a_io, changeableValues, a_sd, a_simClock);
            }
        }

        if (changeableValues.ResourceRequirementChanges != null && changeableValues.ResourceRequirementChanges.Count > 0)
        {
            return changeableValues;
        }

        return null;
    }

    internal void ActivityScheduled(InternalActivity a_act, ScenarioDetail a_sd, long a_simClock)
    {
        int multiThreadedCount = OperationScheduledEnumerator.Count(e => e.MultiThreaded);
        if (multiThreadedCount > 1)
        {
            //There are at least two multi-threaded elements. These will run together after the single-threaded ones.
            List<Task> tasks = new ();
            foreach (IOperationScheduledExtensionElement opScheduledElement in OperationScheduledEnumerator)
            {
                if (!opScheduledElement.MultiThreaded)
                {
                    opScheduledElement.ActivityScheduled(a_act, a_sd, a_simClock);
                }
                else
                {
                    tasks.Add(new Task(() => opScheduledElement.ActivityScheduled(a_act, a_sd, a_simClock)));
                }
            }

            foreach (Task task in tasks)
            {
                task.Start();
            }

            Task.WaitAll(tasks.ToArray());
        }
        else
        {
            //Run them all single-threaded
            foreach (IOperationScheduledExtensionElement opScheduledElement in OperationScheduledEnumerator)
            {
                opScheduledElement.ActivityScheduled(a_act, a_sd, a_simClock);
            }
        }
    }
    #endregion OperationScheduledExtension

    #region InventoryExtension
    /// <summary>
    /// Override to customize the lead-time of a material requirement of an operation.
    /// </summary>
    /// <param name="a_op">The operation whose lead-time to customize.</param>
    /// <param name="a_mr">The material requirement whose inventory lead-time you might want to customize.</param>
    /// <param name="a_clock">The clock time of the scenario.</param>
    /// <param name="a_normalLeadTime">The normal inventory lead-time. </param>
    /// <returns></returns>
    internal long? CustomizeLeadTime(BaseOperation a_op, MaterialRequirement a_mr, long a_clock, long a_normalLeadTime)
    {
        foreach (IInventoryExtensionElement inventoryElement in InventoryEnumerator)
        {
            long? customizeLeadTime = inventoryElement.CustomizeLeadTime(a_op, a_mr, a_clock, a_normalLeadTime);
            if (customizeLeadTime.HasValue)
            {
                return customizeLeadTime;
            }
        }

        return null;
    }
    #endregion

    #region RequiredCapacityExtension
    /// <summary>
    /// Determine the setup and processing spans of an activity or change the quantity per cycle.
    /// </summary>
    internal ProductionInfo OverrideProductionInfo(InternalActivity a_activity, Resource a_resource)
    {
        foreach (IRequiredCapacityExtensionElement capacityElement in RequiredCapacityEnumerator)
        {
            ProductionInfo productionOverride = capacityElement.OverrideProductionInfo(a_activity, a_resource);
            if (productionOverride != null)
            {
                return productionOverride;
            }
        }

        return null;
    }

    /// <summary>
    /// This function is called before the system has calculated standard setup time. If a value other null is returned, then the system will use it and
    /// skip calculating setup time a second time. Null return value signifies "pass" so system will need to calculate the setup.
    /// </summary>
    internal RequiredSpanPlusSetup? CalculateSetup(InternalActivity a_act, InternalResource a_res, long a_startTicks, LeftNeighborSequenceValues a_leftNeighborSequenceValues)
    {
        //TODO: See if we can better handle if multiple elements want to set the setup.
        foreach (IRequiredCapacityExtensionElement capacityElement in RequiredCapacityEnumerator)
        {
            RequiredSpanPlusSetup? span = capacityElement.CalculateSetup(a_act, a_res, a_startTicks, a_leftNeighborSequenceValues);
            if (span.HasValue)
            {
                return span;
            }
        }

        return null;
    }

    /// <summary>
    /// Adjusts the setup time. This is called after setup time has been calculated by standard means and the standard setup is passed to this function. The value returned by this function overrides the
    /// standard setup.
    /// </summary>
    /// <param name="a_act">The activity whose setup time will be calculated.</param>
    /// <param name="a_res">The resource to calculate the setup time on.</param>
    /// <param name="a_startTicks">The start of the setup.</param>
    /// <param name="a_setupSpan">The setup time.</param>
    /// <returns>Adjusted setup time</returns>
    internal RequiredSpanPlusSetup? AdjustSetupTime(InternalActivity a_act, InternalResource a_res, long a_startTicks, RequiredSpanPlusSetup a_setupSpan, LeftNeighborSequenceValues a_leftNeighborSequenceValues)
    {
        foreach (IRequiredCapacityExtensionElement capacityElement in RequiredCapacityEnumerator)
        {
            RequiredSpanPlusSetup? span = capacityElement.AdjustSetupTime(a_act, a_res, a_startTicks, a_setupSpan, a_leftNeighborSequenceValues);
            if (span.HasValue)
            {
                return span;
            }
        }

        return null;
    }

    internal RequiredCapacityPlus? AfterRequiredCapacityCalculation(InternalActivity a_activity, Resource a_resource, RequiredCapacityPlus a_rc, long a_startTicks)
    {
        //m_requiredCapacityChanger.RequiredCapacityChanger = a_rc;
        foreach (IRequiredCapacityExtensionElement capacityElement in RequiredCapacityEnumerator)
        {
            RequiredCapacityPlus? newCapacity = capacityElement.AfterRequiredCapacityCalculation(a_activity, a_resource, a_rc, a_startTicks);
            if (newCapacity != null)
            {
                return newCapacity;
            }
        }

        //m_requiredCapacityChanger.Execute();
        return null;
    }

    internal decimal? GetQtyPerCycle(InternalActivity a_activity, Resource a_resource)
    {
        foreach (IRequiredCapacityExtensionElement capacityElement in RequiredCapacityEnumerator)
        {
            decimal? qtyPerCycle = capacityElement.GetQtyPerCycle(a_activity, a_resource);
            if (qtyPerCycle.HasValue)
            {
                return qtyPerCycle;
            }
        }

        return null;
    }
    #endregion

    #region IsMaterialEligibleExtension
    internal bool? IsEligibleInventory(ScenarioDetail a_sd, long a_simClock, BaseActivity a_activity, Inventory a_inv)
    {
        foreach (IEligibleMaterialExtensionElement materialElement in EligibleMaterialEnumerator)
        {
            bool? eligible = materialElement.IsEligibleInventory(a_sd, a_simClock, a_activity, a_inv);
            if (eligible.HasValue)
            {
                return eligible;
            }
        }

        return null;
    }

    /// <summary>
    /// Determine whether an activity can use a provided source lot
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_firstMaterialDemandDate"></param>
    /// <param name="a_demandMr"></param>
    /// <param name="a_demandActivity"></param>
    /// <param name="a_sourceLot"></param>
    /// <returns></returns>
    internal bool? IsEligibleMaterialSource(ScenarioDetail a_sd, long a_firstMaterialDemandDate, MaterialRequirement a_demandMr, InternalActivity a_demandActivity, Lot a_sourceLot)
    {
        foreach (IEligibleMaterialExtensionElement materialElement in EligibleMaterialEnumerator)
        {
            bool? eligible = materialElement.IsEligibleMaterialSource(a_sd, a_firstMaterialDemandDate, a_demandMr, a_demandActivity, a_sourceLot);
            if (eligible.HasValue)
            {
                return eligible;
            }
        }

        return null;
    }

    /// <summary>
    /// Determine whether an activity can use a provided source lot
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_simClock"></param>
    /// <param name="a_demandMr"></param>
    /// <param name="a_demandActivity"></param>
    /// <param name="a_sourceLot"></param>
    /// <returns></returns>
    internal bool? IsEligibleMaterialSource(ScenarioDetail a_sd, long a_simClock, MaterialRequirement a_demandMr, InternalActivity a_demandActivity, InternalActivity a_supplyActivity, Lot a_sourceLot)
    {
        foreach (IEligibleMaterialExtensionElement materialElement in EligibleMaterialEnumerator)
        {
            bool? eligible = materialElement.IsEligibleMaterialSource(a_sd, a_simClock, a_demandMr, a_demandActivity, a_supplyActivity, a_sourceLot);
            if (eligible.HasValue)
            {
                return eligible;
            }
        }

        return null;
    }

    /// <summary>
    /// Determine whether an activity can use a provided source lot
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_simClock"></param>
    /// <param name="a_demandMr"></param>
    /// <param name="a_demandActivity"></param>
    /// <param name="a_sourceLot"></param>
    /// <returns></returns>
    internal bool? IsEligibleMaterialSource(ScenarioDetail a_sd, long a_simClock, MaterialRequirement a_demandMr, InternalActivity a_demandActivity, PurchaseToStock a_supplyPo, Lot a_sourceLot)
    {
        foreach (IEligibleMaterialExtensionElement materialElement in EligibleMaterialEnumerator)
        {
            bool? eligible = materialElement.IsEligibleMaterialSource(a_sd, a_simClock, a_demandMr, a_demandActivity, a_supplyPo, a_sourceLot);
            if (eligible.HasValue)
            {
                return eligible;
            }
        }

        return null;
    }

    /// <summary>
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_simClock"></param>
    /// <param name="a_mrr">The object requiring material.For instance a activity, SalesOrder, or TransferOrder</param>
    /// <param name="a_inv"></param>
    /// <param name="a_lot"></param>
    /// <returns></returns>
    internal bool? IsEligibleInventory(ScenarioDetail a_sd, long a_simClock, object a_mrr, Inventory a_inv, Lot a_lot)
    {
        foreach (IEligibleMaterialExtensionElement materialElement in EligibleMaterialEnumerator)
        {
            bool? eligible = materialElement.IsEligibleInventory(a_sd, a_simClock, a_mrr, a_inv, a_lot);
            if (eligible.HasValue)
            {
                return eligible;
            }
        }

        return null;
    }

    /// <summary>
    /// Whether to allow a source of material to fulfill a demand for material(Material Requirement, Transfer Order, Sales Order).This version of IsEligibleInventory allows you discriminate material
    /// at a finer level;down to each specific change in inventory through the QtyNode you can access fields such as .
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_simClock">The time an the material is available</param>
    /// <param name="a_mrr">The material requirer such as an activity's material requirement, a sales order, or a transfer order.</param>
    /// <param name="a_qtyNode">
    /// This might be of type QtyNodeExpirable. Of particular interest is field QtyNode.SourceQtyDataReasons, which if not null will allow you to access MinSourceQty, MaxSource Qty,
    /// and Sourcing regardless of whether MaterialRequirement, Lot, and TransferOrderDistribution.
    /// </param>
    /// <param name="a_inv"></param>
    /// <param name="a_lot"></param>
    /// <returns></returns>
    internal bool? IsEligibleInventory(ScenarioDetail a_sd, long a_simClock, object a_mrr, QtyNode a_qtyNode, Inventory a_inv, Lot a_lot)
    {
        foreach (IEligibleMaterialExtensionElement materialElement in EligibleMaterialEnumerator)
        {
            bool? eligible = materialElement.IsEligibleInventory(a_sd, a_simClock, a_mrr, a_qtyNode, a_inv, a_lot);
            if (eligible.HasValue)
            {
                return eligible;
            }
        }

        return null;
    }

    /// <summary>
    /// Override to completely redefine the standard functionality of shelf life, minimum age and expiration.
    /// </summary>
    /// <param name="a_simClock">The time the material will be used. </param>
    /// <param name="a_expirationTicks">The expiration date of the maerial.</param>
    /// <param name="a_minRemainingShelfLife"></param>
    /// <param name="a_lot">The Lot the materail is a part of. </param>
    /// <param name="a_sd"></param>
    /// <returns>true if the material can be used. False if it can't. If you return null, the standard functionality won't be overridden. </returns>
    internal bool? IsLotUsable(long a_simClock, long a_expirationTicks, long a_minRemainingShelfLife, Lot a_lot, ScenarioDetail a_sd)
    {
        foreach (IEligibleMaterialExtensionElement materialElement in EligibleMaterialEnumerator)
        {
            bool? eligible = materialElement.IsLotUsable(a_sd, a_simClock, a_expirationTicks, a_minRemainingShelfLife, a_lot);
            if (eligible.HasValue)
            {
                return eligible;
            }
        }

        return null;
    }
    #endregion

    #region MaterialAllocationExtensions
    /// <summary>
    /// Allows the sequence material is allocated in to be overridden. Specify UseOldestFirst, UseNewestFirst. To use Default functionality, return null.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_simClock"></param>
    /// <param name="a_matlAlloc"></param>
    /// <param name="a_inv"></param>
    /// <returns></returns>
    internal ItemDefs.MaterialAllocation? SequenceMaterialAllocation(ScenarioDetail a_sd, long a_simClock, IMaterialAllocation a_matlAlloc, Inventory a_inv)
    {
        foreach (IMaterialAllocationExtensionElement matAllocExtension in MaterialAllocationEnumerator)
        {
            ItemDefs.MaterialAllocation? allocation = matAllocExtension.SequenceMaterialAllocation(a_sd, a_simClock, a_matlAlloc, a_inv);
            if (allocation.HasValue)
            {
                return allocation;
            }
        }

        return null;
    }
    #endregion

    #region LotExtension
    /// <summary>
    /// This customization point allows you adjust the time the scheduling engine uses as the expiration of this
    /// part of the lot. Note, this function will be called multiple times for the same lot if the material from the lot
    /// is released during production. For instance if produced material is released after each cycle, this function will
    /// be called after each cycle since the expiration date of each cycle of material is diferent from the others.
    /// </summary>
    /// <param name="a_sd">A reference to ScenarioDetail if needed. </param>
    /// <param name="a_productionDate">The date this material became available in inventory. </param>
    /// <param name="a_expiration">The scheduler calculated expiration of the this material.</param>
    /// <param name="a_qty">The amount of material that entered inventory. </param>
    /// <param name="a_lot">The lot in question.</param>
    /// <param name="a_inv">The inventory the lot is in.</param>
    /// <returns>The value to use as the expiration of this unit of material. </returns>
    internal long? AdjustShelfLife(ScenarioDetail a_sd, long a_productionDate, long a_expiration, decimal a_qty, List<Lot> a_expirableLots, Item a_item)
    {
        long shelfLife = 0;
        foreach (ILotExtensionElement lotElement in LotEnumerator)
        {
            long? adjustedShelfLife = lotElement.AdjustShelfLife(a_sd, a_productionDate, a_expiration, a_qty, a_expirableLots, a_item);
            if (adjustedShelfLife.HasValue)
            {
                shelfLife = Math.Min(adjustedShelfLife.Value, shelfLife);
            }
        }

        if (shelfLife != 0)
        {
            return shelfLife;
        }

        return null;
    }
    #endregion

    #region SplitExtensions
    internal SplitResult Split(ScenarioDetail.SimulationType a_simType, long a_clock, long a_simClock, Resource a_primaryResource, InternalActivity a_act, SchedulableInfo a_si, CycleAdjustmentProfile a_ccp, ScenarioDetail a_sd)
    {
        foreach (ISplitExtensionElement splitElement in SplitEnumerator)
        {
            SplitResult splitResult = splitElement.Split(a_simType, a_clock, a_simClock, a_primaryResource, a_act, a_si, a_ccp, a_sd);
            if (splitResult.SplitResultEnum != SplitResult.ESplitResultEnum.NoSplit)
            {
                return splitResult;
            }
        }

        return new SplitResult(SplitResult.ESplitResultEnum.NoSplit);
    }

    internal SplitResult SplitForStorage(ScenarioDetail.SimulationType a_simType, long a_clock, long a_simClock, decimal a_totalQtyCapacityAvailable, Resource a_primaryResource, InternalActivity a_act, SchedulableInfo a_si, CycleAdjustmentProfile a_ccp, ScenarioDetail a_sd)
    {
        foreach (ISplitExtensionElement splitElement in SplitEnumerator)
        {
            SplitResult splitResult = splitElement.SplitForStorage(a_simType, a_clock, a_simClock, a_totalQtyCapacityAvailable, a_primaryResource, a_act, a_si, a_ccp, a_sd);
            if (splitResult.SplitResultEnum != SplitResult.ESplitResultEnum.NoSplit)
            {
                return splitResult;
            }
        }

        return new SplitResult(SplitResult.ESplitResultEnum.NoSplit);
    }

    internal void ProcessSplitMo(ScenarioDetail.SimulationType a_simType, long a_clock, long a_simClock, Resource a_primaryResource, ManufacturingOrder a_splitMo, SchedulableInfo a_si, CycleAdjustmentProfile a_ccp, ScenarioDetail a_sd)
    {
        foreach (ISplitExtensionElement splitElement in SplitEnumerator)
        {
            splitElement.ProcessSplitMo(a_simType, a_clock, a_simClock, a_primaryResource, a_splitMo, a_si, a_ccp, a_sd);
        }
    }
    #endregion

    #region AdjustMOExtension
    internal bool AdjustMOForStorage(ScenarioDetail.SimulationType a_simType, long a_clock, long a_simClock, decimal a_resizeQty, Resource a_primaryResource, InternalActivity a_act, SchedulableInfo a_si, CycleAdjustmentProfile a_ccp, ScenarioDetail a_sd)
    {
        foreach (IAdjustManufacturingOrderExtensionElement adjustManufacturingOrderExtensionElement in AdjustMOExtensionsEnumerator)
        {
            return adjustManufacturingOrderExtensionElement.AdjustMOForStorage(a_simType, a_clock, a_simClock, a_resizeQty, a_primaryResource, a_act, a_si, a_ccp, a_sd);
        }

        return false;
    }
    #endregion

    #region CanAutoJoinExtensions
    internal void BeforeAutoJoinProcessing(ScenarioDetail a_sd)
    {
        foreach (IAutoJoinExtensionElement autoJoinElement in AutoJoinEnumerator)
        {
            autoJoinElement.BeforeAutoJoinProcessing(a_sd);
        }
    }

    /// <summary>
    /// Called during simulation, allows custom logic to determination of autojoin availablilty of resource blocks.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_resource"></param>
    /// <param name="a_previousBlock"></param>
    /// <param name="a_nextResBlock"></param>
    /// <returns></returns>
    internal bool CanAutoJoin(ScenarioDetail a_sd, Resource a_resource, ResourceBlock a_previousBlock, ResourceBlockList.Node a_blockNode)
    {
        foreach (IAutoJoinExtensionElement autoJoinElement in AutoJoinEnumerator)
        {
            bool? autoJoin = autoJoinElement.CanAutoJoin(a_sd, a_resource, a_previousBlock, a_blockNode);
            if (autoJoin.HasValue)
            {
                return autoJoin.Value;
            }
        }

        //Otherwise run default implementation
        LinkedListNode<ResourceCapacityInterval> ciNode = a_resource.FindRCIForward(a_previousBlock.StartTicks);

        if ((!a_resource.LimitAutoJoinToSameCapacityInterval || a_blockNode.Data.EndDateTime.Ticks <= ciNode.Value.EndDate) // only check this if limiting to same shift.
            &&
            a_previousBlock.Batch.FirstActivity.Operation.ManufacturingOrder.Id != a_blockNode.Data.Batch.FirstActivity.Operation.ManufacturingOrder.Id &&
            !string.IsNullOrEmpty(a_previousBlock.Batch.FirstActivity.Operation.ManufacturingOrder.AutoJoinGroup) &&
            a_previousBlock.Batch.FirstActivity.Operation.ManufacturingOrder.AutoJoinGroup == a_blockNode.Data.Batch.FirstActivity.Operation.ManufacturingOrder.AutoJoinGroup &&
            a_previousBlock.Batch.FirstActivity.Late == a_blockNode.Data.Batch.FirstActivity.Late &&
            (a_previousBlock.Batch.FirstActivity.Late ||
             a_resource.LimitAutoJoinToSameCapacityInterval || //Allow the job to become late since they are joining by shift
             (!a_resource.LimitAutoJoinToSameCapacityInterval && !a_previousBlock.Batch.FirstActivity.Late && a_previousBlock.Batch.FirstActivity.NeedDate > a_blockNode.Data.Batch.FirstActivity.ScheduledEndDate))) // don't join if it will make it late.
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Called after AutoJoin completed, allows for custom logic updates for joined changes.
    /// </summary>
    /// <param name="a_resource"></param>
    /// <param name="a_previousOperation"></param>
    /// <param name="a_joiningOpId"></param>
    internal void AutoJoined(Resource a_resource, InternalOperation a_previousOperation, InternalOperation a_joiningOperation)
    {
        foreach (IAutoJoinExtensionElement autoJoinElement in AutoJoinEnumerator)
        {
            autoJoinElement.AutoJoined(a_resource, a_previousOperation, a_joiningOperation);
        }
    }

    internal DateTime? CalculateLatestMaterialAvailability(ScenarioDetail a_sd, ActivityAdjustment a_adjustment, InternalActivity a_ia)
    {
        foreach (IAutoJoinExtensionElement autoJoinElement in AutoJoinEnumerator)
        {
            DateTime? overrideAvailability = autoJoinElement.CalculateLatestMaterialAvailability(a_sd, a_adjustment, a_ia);
            if (overrideAvailability.HasValue)
            {
                return overrideAvailability;
            }
        }

        return null;
    }
    #endregion

    internal void FlagSchedulingDelayDueToStorageUnavailable(ScenarioDetail a_sd, InternalOperation a_operation)
    {
        foreach (IFlagDelayExtensionElement flagDelayExtensionElement in FlagDelayExtensionsEnumerator)
        {
            flagDelayExtensionElement.FlagSchedulingDelayDueToStorageUnavailable(a_sd, a_operation);
        }
    }

    internal IEnumerable<ICustomTableExtensionElement> GetCustomTableElements()
    {
        return CustomTableElementEnumerator;
    }

    internal void PostJobTProcessing(List<Job> a_addedJobs, List<Job> a_updatedJobs, List<Job> a_deletedJobs, out string a_warnings)
    {
        a_warnings = string.Empty;
        foreach (IPostJobTProcessingExtensionElement element in PostJobTProcessingEnumerator)
        {
            element.PostJobTProcessing(a_addedJobs, a_updatedJobs, a_deletedJobs, out a_warnings);
        }
    }

    #region TransmissionHandlingExtensions
    internal TransmissionHandlingResult Preprocessing(Transmission a_t, ScenarioDetail a_sd)
    {
        TransmissionHandlingResult activityUpdates = new ();
        foreach (ITransmissionProcessingExtensionElement transmissionProcessingElement in TransmissionProcessingEnumerator)
        {
            transmissionProcessingElement.Preprocessing(a_t, a_sd, activityUpdates);
        }

        if (activityUpdates.ActivityUpdateList.Count > 0)
        {
            return activityUpdates;
        }

        return null;
    }

    internal TransmissionHandlingResult Postprocessing(Transmission a_t, ScenarioDetail a_sd)
    {
        TransmissionHandlingResult activityUpdates = new ();
        foreach (ITransmissionProcessingExtensionElement transmissionProcessingElement in TransmissionProcessingEnumerator)
        {
            transmissionProcessingElement.Postprocessing(a_t, a_sd, activityUpdates);
        }

        if (activityUpdates.ActivityUpdateList.Count > 0)
        {
            return activityUpdates;
        }

        return null;
    }

    internal List<ScenarioBaseT> PreProcessingTransformation(ScenarioBaseT a_t, ScenarioDetail a_sd)
    {
        List<ScenarioBaseT> transmissions = new ();
        foreach (ITransmissionProcessingExtensionElement transmissionProcessingElement in TransmissionProcessingEnumerator)
        {
            transmissions = transmissionProcessingElement.PreProcessingTransformation(a_t, a_sd, transmissions);
        }

        if (transmissions.Count > 0)
        {
            return transmissions;
        }

        return null;
    }

    /// <summary>
    /// Allows modifying the ScenarioCopyT, for example to set IsolateFromImport or IsolateFromClockAdvance settings
    /// </summary>
    /// <param name="a_t"></param>
    /// <param name="a_originalScenario">The scenario being copied</param>
    /// <param name="a_originalScenarioDetail">The scenario detail being copied</param>
    /// <returns></returns>
    public ScenarioCopyT PreProcessingScenarioCopy(ScenarioCopyT a_t, Scenario a_originalScenario, ScenarioDetail a_originalScenarioDetail)
    {
        SortedList<int, ScenarioBaseT> actionsList = new ();
        foreach (ITransmissionProcessingExtensionElement transmissionProcessingElement in TransmissionProcessingEnumerator)
        {
            ScenarioCopyT copyT = transmissionProcessingElement.PreProcessingScenarioCopy(a_t, a_originalScenario, a_originalScenarioDetail);
            if (copyT != null)
            {
                return copyT;
            }
        }

        return null;
    }
    #endregion

    public SortedList<int, ScenarioBaseT> GetExtraTransmissionsToProcess()
    {
        SortedList<int, ScenarioBaseT> actionsList = new ();
        foreach (IAdditionalActionExtensionElement additionalActionElement in AdditionalActionEnumerator)
        {
            additionalActionElement.BuildAdditionalActions(actionsList);
        }

        return actionsList;
    }

    #region Tank Extensions
    public decimal? CalculateTankWithdrawalTiming(long a_simClock, ScenarioDetail a_scenarioDetail, Warehouse a_inventoryWarehouse, decimal a_remainingQtyInStorage, InternalActivity a_tankActivity)
    {
        foreach (ITankExtensionElement tankElement in TankEnumerator)
        {
            decimal? tankDrainage = tankElement.CalculateDrainageQty(a_simClock, a_scenarioDetail, a_inventoryWarehouse, a_remainingQtyInStorage, a_tankActivity);
            if (tankDrainage.HasValue)
            {
                return tankDrainage.Value;
            }
        }

        return null;
    }

    public DateTime? CalculateTankWithdrawalTiming(long a_simClock, ScenarioDetail a_scenarioDetail, InternalActivity a_withdrawingActivity, MaterialRequirement a_withdrawingMr, ResourceWarehouse a_tankWarehouse, decimal a_quantityWithdrawn)
    {
        foreach (ITankExtensionElement tankElement in TankEnumerator)
        {
            DateTime? tankRelease = tankElement.CalculateTankWithdrawalTiming(a_simClock, a_scenarioDetail, a_withdrawingActivity, a_withdrawingMr, a_tankWarehouse, a_quantityWithdrawn);
            if (tankRelease.HasValue)
            {
                return tankRelease.Value;
            }
        }

        return null;
    }
    #endregion

    #region Scenario Extensions
    internal void ScenarioSettingSaved(ScenarioDetail a_sd, string a_settingKey, SettingData a_settingData, ScenarioSettingDataT a_t)
    {
        foreach (IScenarioExtensionsElement scenarioElement in ScenarioExtensionsEnumerator)
        {
            scenarioElement.ScenarioSettingSaved(a_sd, a_settingKey, a_settingData, a_t);
        }
    }
    #endregion

    #region Clock Advance Extensions
    internal bool? MarkTankConsumingActivity(InternalActivity a_act, long a_newClockTicks)
    {
        foreach (IClockAdvanceExtensionElement clockAdvanceExtensionElement in ClockAdvanceExtensionsEnumerator)
        {
            return clockAdvanceExtensionElement.MarkTankConsumingActivity(a_act, a_newClockTicks);
        }

        return null;
    }
    #endregion

    public void ResetDataChanges()
    {
        m_scenarioDataChanges = new ();
    }
}