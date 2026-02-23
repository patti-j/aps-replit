using System.Data;

using PT.PackageDefinitions;
using PT.Scheduler.MRP;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.Scheduler.Schedule.Operation;
using PT.Scheduler.Simulation.Customizations;
using PT.Scheduler.Simulation.Customizations.MRP;
using PT.Scheduler.Simulation.Customizations.TransmissionPreprocessing;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler.Simulation.Extensions.Interfaces;

public interface ISimulationInitializationExtensionElement : IExtensionElement
{
    void SimulationInitialization(ScenarioDetail a_sd, SchedulabilitySimulationInitializationHelper a_schedulabilityHelper, ScenarioDetail.SimulationType a_simulationType, ScenarioBaseT a_transmission);
}

public interface ISchedulingExtensionElement : IExtensionElement
{
    DateTime? IsSchedulable(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simType, long a_clock, long a_simulationClock, long a_simStartTicks, BaseResource a_res, BaseActivity a_activity)
    {
        return null;
    }

    DateTime? IsSchedulable(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simType, long a_clock, long a_simulationClock, long a_simStartTicks, BaseResource a_res, BaseActivity a_activity, SchedulableInfo a_schedulableInfo)
    {
        return null;
    }

    DateTime? IsSchedulablePostMaterialAllocation(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simType, long a_clock, long a_simulationClock, long a_simStartTicks, BaseResource a_res, BaseActivity a_activity, SchedulableInfo a_si, TransferQtyCompletionProfile a_transferQtyProfile)
    {
        return null;
    }

    bool? CanScheduleOnResource(ScenarioDetail a_sd, InternalResource a_resource, InternalActivity a_activity, ScenarioDetail.SimulationType a_simulationType, ScenarioBaseT a_transmission)
    {
        return null;
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
    long? AdjustActivityRelease(ScenarioDetail a_sd, InternalActivity a_activity, long a_proposedReleaseTicks, ScenarioDetail.SimulationType a_simulationType, ScenarioBaseT a_transmission)
    {
        return null;
    }
}

public interface IAdvancedSchedulingExtensionElement : IExtensionElement
{
    DateTime? IsSchedulableNonPrimaryResourceRequirement(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simType, long a_clock, long a_simulationClock, ResourceSatiator[] a_resourceRequirementSatiators, int a_resourceRequirementIdx, BaseActivity a_activity, SchedulableInfo a_schedulableInfo);
    DateTime? IsSchedulableContinuousFlowCheck(ScenarioDetail a_sd, ScenarioDetail.SimulationType a_simType, long a_clock, long a_simulationClock, BaseResource a_primaryResource, InternalOperation a_op, SchedulableInfo a_si);
}

public interface IPostSimStageExtensionElement : IExtensionElement
{
    void PostSimStageSetup(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd, PostSimStageChangeHelper a_changeHelper, int a_curSimStageIdx, int a_finalSimStageIdx) { }

    void PostSimStageChangeRR(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd, InternalOperation a_op, ResourceRequirement a_rr, int a_curSimStageIdx, int a_finalSimStageIdx, out ChangableRRValues o_changable)
    {
        o_changable = null;
    }

    void PostSimStageChangeMO(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd, ManufacturingOrder a_mo, int a_curSimStageIdx, int a_finalSimStageIdx, out ChangableMOValues o_changable)
    {
        o_changable = null;
    }

    void PostSimStageChangeDept(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd, Department a_dept, int a_curSimStageIdx, int a_finalSimStageIdx, out ChangableDeptValues o_changable)
    {
        o_changable = null;
    }

    void PostSimStageCleanup(ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd, int a_curSimStageIdx, int a_finalSimStageIdx) { }
}

public interface IOperationScheduledExtensionElement : IExtensionElement
{
    void OperationScheduled(InternalOperation a_io, ChangeableOperationValues a_changableOperationValues, ScenarioDetail a_sd, long a_simClock);
    void ActivityScheduled(InternalActivity a_act, ScenarioDetail a_sd, long a_simClock);
}

public interface IClockAdvanceExtensionElement : IExtensionElement
{
    /// <summary>
    /// Add a UDF to the Activity if auto-reporting progress on it while the supplying Activity is a Tank Activity still in Storage.
    /// </summary>
    /// <param name="a_act"></param>
    /// <param name="a_newClockTicks"></param>
    /// <returns></returns>
    bool? MarkTankConsumingActivity(InternalActivity a_act, long a_newClockTicks);
}

public interface IRequiredCapacityExtensionElement : IExtensionElement
{
    /// <summary>
    /// Determine the setup and processing spans of an activity or change the quantity per cycle.
    /// </summary>
    ProductionInfo OverrideProductionInfo(InternalActivity a_activity, Resource a_resource)
    {
        return null;
    }

    /// <summary>
    /// This function is called before the system has calculated standard setup time. If a value other null is returned, then the system will use it and
    /// skip calculating setup time a second time. Null return value signifies "pass" so system will need to calculate the setup.
    /// </summary>
    RequiredSpanPlusSetup? CalculateSetup(InternalActivity a_act, InternalResource a_res, long a_startTicks, LeftNeighborSequenceValues a_leftNeighborSequenceValues)
    {
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
    RequiredSpanPlusSetup? AdjustSetupTime(InternalActivity a_act, InternalResource a_res, long a_startTicks, RequiredSpanPlusSetup a_setupSpan, LeftNeighborSequenceValues a_leftNeighborSequenceValues)
    {
        return null;
    }

    RequiredCapacityPlus? AfterRequiredCapacityCalculation(InternalActivity a_activity, Resource a_resource, RequiredCapacityPlus a_rc, long a_startTicks);

    decimal? GetQtyPerCycle(InternalActivity a_activity, Resource a_resource)
    {
        return null;
    }
}

public interface IEligibleMaterialExtensionElement : IExtensionElement
{
    bool? IsEligibleInventory(ScenarioDetail a_sd, long a_simClock, BaseActivity a_activity, Inventory a_inv)
    {
        return null;
    }

    /// <summary>
    /// Determine whether an activity can use a provided source lot
    /// </summary>
    bool? IsEligibleMaterialSource(ScenarioDetail a_sd, long a_firstMaterialDemandDate, MaterialRequirement a_demandMr, InternalActivity a_demandActivity, Lot a_sourceLot)
    {
        return null;
    }

    /// <summary>
    /// Determine whether an activity can use a provided source lot
    /// </summary>
    bool? IsEligibleMaterialSource(ScenarioDetail a_sd, long a_firstMaterialDemandDate, MaterialRequirement a_demandMr, InternalActivity a_demandActivity, InternalActivity a_supplyActivity, Lot a_sourceLot)
    {
        return null;
    }

    /// <summary>
    /// Determine whether an activity can use a provided source lot
    /// </summary>
    bool? IsEligibleMaterialSource(ScenarioDetail a_sd, long a_firstMaterialDemandDate, MaterialRequirement a_demandMr, InternalActivity a_demandActivity, PurchaseToStock a_supplyPo, Lot a_sourceLot)
    {
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
    bool? IsEligibleInventory(ScenarioDetail a_sd, long a_simClock, object a_mrr, Inventory a_inv, Lot a_lot)
    {
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
    bool? IsEligibleInventory(ScenarioDetail a_sd, long a_simClock, object a_mrr, QtyNode a_qtyNode, Inventory a_inv, Lot a_lot)
    {
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
    bool? IsLotUsable(ScenarioDetail a_sd, long a_simClock, long a_expirationTicks, long a_minRemainingShelfLife, Lot a_lot)
    {
        return null;
    }
}

public interface IInventoryExtensionElement : IExtensionElement
{
    /// <summary>
    /// Override to customize the lead-time of a material requirement of an operation.
    /// </summary>
    /// <param name="a_op">The operation whose lead-time to customize.</param>
    /// <param name="a_mr">The material requirement whose inventory lead-time you might want to customize.</param>
    /// <param name="a_clock">The clock time of the scenario.</param>
    /// <param name="a_normalLeadTime">The normal inventory lead-time. </param>
    /// <returns></returns>
    long? CustomizeLeadTime(BaseOperation a_op, MaterialRequirement a_mr, long a_clock, long a_normalLeadTime)
    {
        return null;
    }
}

public interface IMaterialAllocationExtensionElement : IExtensionElement
{
    /// <summary>
    /// Allows the sequence material is allocated in to be overridden. Specify UseOldestFirst, UseNewestFirst. To use Default functionality, return null.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_simClock"></param>
    /// <param name="a_matlAlloc"></param>
    /// <param name="a_inv"></param>
    /// <returns></returns>
    SchedulerDefinitions.ItemDefs.MaterialAllocation? SequenceMaterialAllocation(ScenarioDetail a_sd, long a_simClock, IMaterialAllocation a_matlAlloc, Inventory a_inv)
    {
        return null;
    }
}

public interface ILotExtensionElement : IExtensionElement
{
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
    long? AdjustShelfLife(ScenarioDetail a_sd, long a_productionDate, long a_expiration, decimal a_qty, List<Lot> a_lot, Item a_item)
    {
        return null;
    }
}

public interface ISplitExtensionElement : IExtensionElement
{
    SplitResult Split(ScenarioDetail.SimulationType a_simType, long a_clock, long a_simClock, Resource a_primaryResource, InternalActivity a_act, SchedulableInfo a_si, CycleAdjustmentProfile a_ccp, ScenarioDetail a_sd);
    SplitResult SplitForStorage(ScenarioDetail.SimulationType a_simType, long a_clock, long a_simClock, decimal a_totalQtyCapacityAvailable, Resource a_primaryResource, InternalActivity a_act, SchedulableInfo a_si, CycleAdjustmentProfile a_ccp, ScenarioDetail a_sd);
    void ProcessSplitMo(ScenarioDetail.SimulationType a_simType, long a_clock, long a_simClock, Resource a_primaryResource, ManufacturingOrder a_splitMo, SchedulableInfo a_si, CycleAdjustmentProfile a_ccp, ScenarioDetail a_sd);
}

public interface IAdjustManufacturingOrderExtensionElement : IExtensionElement
{
    bool AdjustMOForStorage(ScenarioDetail.SimulationType a_simType, long a_clock, long a_simClock, decimal a_resizeQty, Resource a_primaryResource, InternalActivity a_act, SchedulableInfo a_si, CycleAdjustmentProfile a_ccp, ScenarioDetail a_sd);
}

public interface IAutoJoinExtensionElement : IExtensionElement
{
    void BeforeAutoJoinProcessing(ScenarioDetail a_sd);

    /// <summary>
    /// Called during simulation, allows custom logic to determination of autojoin availablilty of resource blocks.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_resource"></param>
    /// <param name="a_previousBlock"></param>
    /// <param name="a_nextResBlock"></param>
    /// <returns></returns>
    bool? CanAutoJoin(ScenarioDetail a_sd, Resource a_resource, ResourceBlock a_previousBlock, ResourceBlockList.Node a_blockNode)
    {
        return null;
    }

    /// <summary>
    /// Called after AutoJoin completed, allows for custom logic updates for joined changes.
    /// </summary>
    /// <param name="a_resource"></param>
    /// <param name="a_previousOperation"></param>
    /// <param name="a_joiningOpId"></param>
    void AutoJoined(Resource a_resource, InternalOperation a_previousOperation, InternalOperation a_joiningOperation);

    DateTime? CalculateLatestMaterialAvailability(ScenarioDetail a_sd, ActivityAdjustment a_adjustment, InternalActivity a_ia)
    {
        return null;
    }
}

public interface ICustomTableExtensionElement : IExtensionElement
{
    List<DataTable> GetCustomTables(ScenarioDetail a_sd, DateTime a_publishDate, string a_instanceId);
}

public interface IPostJobTProcessingExtensionElement : IExtensionElement
{
    void PostJobTProcessing(List<Job> a_addedJobs, List<Job> a_updatedJobs, List<Job> a_deletedJobs, out string a_warnings);
}

public interface ITransmissionProcessingExtensionElement : IExtensionElement
{
    List<TransmissionHandlingResult.ActivityChange> Preprocessing(Transmission a_t, ScenarioDetail a_sd, TransmissionHandlingResult a_activityUpdates)
    {
        return null;
    }

    List<TransmissionHandlingResult.ActivityChange> Postprocessing(Transmission a_t, ScenarioDetail a_sd, TransmissionHandlingResult a_activityUpdates)
    {
        return null;
    }

    List<ScenarioBaseT> PreProcessingTransformation(ScenarioBaseT a_t, ScenarioDetail a_sd, List<ScenarioBaseT> a_newTransmissionsLIst)
    {
        return new List<ScenarioBaseT>();
    }

    ScenarioCopyT PreProcessingScenarioCopy(ScenarioCopyT a_t, Scenario a_originalScenario, ScenarioDetail a_originalScenarioDetail)
    {
        return null;
    }
}

public interface IMrpNeedDateResetExtensionElement : IExtensionElement
{
    DateTime? ExistingOrderNeedDateReset(ScenarioDetail a_sd, Job a_existingJob, decimal a_supplyQty, decimal a_unallocatedQty, MrpDemand a_mrpDemand, DateTime a_demandAdjustmentDate)
    {
        return null;
    }
}

public interface IMrpExtensionElement : IExtensionElement
{
    bool? CanAllocate(ScenarioDetail a_sd, Schedule.InventoryManagement.MRP.SupplyOrder a_supplyOrder, MrpDemand a_mrpDemand, decimal a_qty, bool a_allowPartialSupply)
    {
        return null;
    }

    bool? CanBatch(ScenarioDetail a_sd, Schedule.InventoryManagement.MRP.SupplyOrder a_supplyOrder, MrpDemand a_mrpDemand, decimal a_qty, bool a_allowPartialSupply)
    {
        return null;
    }

    bool? CanExistingSupplySatisfyDemand(ScenarioDetail a_sd, Job a_potentialSupplyJob, DateTime a_demandAdjustmentDate)
    {
        return null;
    }

    DateTime? PegAllocationNeedDate(ScenarioDetail a_sd, Job a_existingJob, MrpDemand a_mrpDemand)
    {
        return null;
    }

    decimal? GetMaxOrderQty(ScenarioDetail a_sd, Schedule.InventoryManagement.MRP.SupplyOrder a_supplyOrder, MrpDemand a_mrpDemand)
    {
        return null;
    }

    //Job can not be nullable, if job is not notified this returns null
    Job CustomizeMrpJob(ScenarioDetail a_sd, Schedule.InventoryManagement.MRP.SupplyOrder a_supplyOrder, Job a_newJob, ref ChangableJobValues r_changableValues, PTTransmission a_t)
    {
        return null;
    }

    /// <summary>
    /// Overrides how much quantity a non batched order can accept.
    /// The default is 0 because we are not batching.
    /// This can be used to match an enforced minimum production quantity.
    /// </summary>
    decimal? GetUsableNonBatchOverproductionQty(ScenarioDetail a_sd, Schedule.InventoryManagement.MRP.SupplyOrder a_supplyOrder, decimal a_totalDemandQty, MrpDemand a_mrpDemand)
    {
        return null;
    }

    /// <summary>
    /// Overrides how much quantity a closed batch accept. The batch is closed because we are outside the batching window
    /// The default is production - total demand. This is the remainder produced by the current batch
    /// This can be used to match an enforced minimum production quantity.
    /// </summary>
    decimal? GetUsableBatchedOverproductionQty(ScenarioDetail a_sd, Schedule.InventoryManagement.MRP.SupplyOrder a_supplyOrder, decimal a_producingQty, decimal a_totalDemandQty, MrpDemand a_mrpDemand)
    {
        return null;
    }
}

public interface IAdditionalActionExtensionElement : IExtensionElement
{
    void BuildAdditionalActions(SortedList<int, ScenarioBaseT> a_actionsList);
}

public interface ITankExtensionElement : IExtensionElement
{
    /// <summary>
    /// Allows specifying a drainage qty for tanks. Any value currently in the tank less than or equal to this value will cause the tank to be emptied and the remainder in storage discarded
    /// </summary>
    /// <param name="a_simClock">Current simulation time</param>
    /// <param name="a_scenarioDetail">Scenario Model</param>
    /// <param name="a_inventoryWarehouse">The tank warehouse that is being withdrawn</param>
    /// <param name="a_remainingQtyInStorage">The current qty in the tank after the last withdraw</param>
    /// <param name="a_tankActivity">The tank activity that created the quantity stored in the tank</param>
    /// <returns>Minimum quantity to drain from the tank</returns>
    decimal? CalculateDrainageQty(long a_simClock, ScenarioDetail a_scenarioDetail, Warehouse a_inventoryWarehouse, decimal a_remainingQtyInStorage, InternalActivity a_tankActivity);

    /// <summary>
    /// Allows overriding the standard tank withdrawal timing.
    /// </summary>
    /// <param name="a_simClock">Current simulation time</param>
    /// <param name="a_scenarioDetail">Scenario Model</param>
    /// <param name="a_withdrawingActivity">The activity consuming the material</param>
    /// <param name="a_withdrawingMr">The operation's Material Requirement the material is sourced to</param>
    /// <param name="a_tankWarehouse">The tank warehouse, which has a reference to the tank resource</param>
    /// <param name="a_quantityWithdrawn">The quantity being withdrawn from the tank</param>
    /// <returns>The end date of the withdrawal. If the tank is emptied from this withdrawal, the tank storage will end at this time</returns>
    DateTime? CalculateTankWithdrawalTiming(long a_simClock, ScenarioDetail a_scenarioDetail, InternalActivity a_withdrawingActivity, MaterialRequirement a_withdrawingMr, ResourceWarehouse a_tankWarehouse, decimal a_quantityWithdrawn);
}

public interface IScenarioExtensionsElement : IExtensionElement
{
    void ScenarioSettingSaved(ScenarioDetail a_sd, string a_settingKey, SettingData a_settingData, ScenarioSettingDataT a_t);
}

public interface IProductionStatusExtensionElement : IExtensionElement
{
    InternalActivityDefs.productionStatuses? GetProductionStatus(InternalActivity a_act, InternalActivityDefs.productionStatuses a_currentProductionStatus);
}

public interface IOverrideCleanSpanExtensionElement : IExtensionElement
{
    RequiredSpanPlusClean OverrideCleanSpan(ScenarioDetail a_sd, InternalActivity a_previousAct, InternalActivity a_nextAct);
    RequiredSpanPlusClean OverrideLastActCleanSpan(ScenarioDetail a_sd, InternalActivity a_lastAct, InternalResource a_primaryResource);
}

public interface IFlagDelayExtensionElement : IExtensionElement
{
    void FlagSchedulingDelayDueToStorageUnavailable(ScenarioDetail a_sd, InternalOperation a_operation);
}