using PT.APSCommon;
using PT.Scheduler.Schedule;
using PT.Scheduler.Simulation.Extensions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of ManufacturingOrder objects.
/// </summary>
public partial class ManufacturingOrderManager
{
    #region Eligibility
    internal void AdjustEligiblePlants(ScenarioBaseT a_t)
    {
        for (int i = 0; i < Count; ++i)
        {
            GetByIndex(i).AdjustEligiblePlants(a_t);
        }
    }
    #endregion

    #region Successor MOs
    internal void LinkSuccessorMOs()
    {
        for (int moI = 0; moI < Count; ++moI)
        {
            ManufacturingOrder mo = this[moI];
            mo.LinkSuccessorMOs();
        }
    }
    #endregion

    #region Simulation
    /// <summary>
    /// Initialize non-finished MOs for simulation.
    /// </summary>
    internal void SimulationInitialization(PlantManager a_plantManager, ProductRuleManager a_productRuleManager, ExtensionController a_extensionController, ICalculatedValueCacheManager a_cacheManager)
    {
        for (int i = 0; i < Count; ++i)
        {
            ManufacturingOrder mo = this[i];

            if (!mo.Finished)
            {
                mo.SimulationInitialization(a_plantManager, a_productRuleManager, a_extensionController, a_cacheManager);
            }
        }
    }

    internal void PostSimulationInitialization()
    {
        for (int i = 0; i < Count; ++i)
        {
            ManufacturingOrder mo = this[i];

            if (!mo.Finished)
            {
                mo.PostSimulationInitialization();
            }
        }
    }
    #endregion

    #region Quantity Adjustments
    #region Split
    /// <summary>
    /// Create a new MO by copying the specified MO and setting the new MO's qty to the specified fraction of the existing MO.
    /// If the source MO is already scheduled, its blocks will be split in place between the source and new MO.
    /// </summary>
    /// <param name="moId">This must exist within this manager or an error will occur.</param>
    /// <param name="qty"></param>
    internal ManufacturingOrder SplitOffRatioInNewMO(BaseId a_moId, decimal a_splitOffRatio, ProductRuleManager a_productRuleManager)
    {
        ManufacturingOrder sourceMO = GetById(a_moId);
        return SplitOffRatioInNewMO(sourceMO, a_splitOffRatio, true, a_productRuleManager);
    }

    /// <summary>
    /// Create a new MO by copying the specified MO and setting the new MO's qty to the specified fraction of the existing MO.
    /// If the source MO is already scheduled, its blocks will be split in place between the source and new MO.
    /// </summary>
    internal ManufacturingOrder SplitOffRatioInNewMO(ManufacturingOrder a_sourceMO, decimal a_splitOffRatio, bool a_unscheduleIneligibleJob, ProductRuleManager a_productRuleManager)
    {
        Simulation.Scheduler.Job.SplitHelpers.MOSplittableValidation(a_sourceMO);

        //We need to store the currently scheduled resources by operation to check eligibility after split
        Dictionary<string, Resource> opScheduledResource = new ();
        foreach (ResourceOperation op in a_sourceMO.CurrentPath.GetOperationsByLevel(false))
        {
            //TODO check all ResourceRequirements, not just lead act
            opScheduledResource[op.ExternalId] = op.GetScheduledPrimaryResource();
        }

        // Add Copy
        ManufacturingOrder copyMO = CreateInitializedCopyOfMO(a_sourceMO);
        copyMO.Split = true;
        copyMO.SplitFromManufacturingOrderId = a_sourceMO.Id;
        bool needsToUnscheduleJob = a_sourceMO.AdjustRequiredQtyByRatio(copyMO, a_splitOffRatio, a_productRuleManager);
        if (!a_unscheduleIneligibleJob)
        {
            needsToUnscheduleJob = false;
        }
        else if (!needsToUnscheduleJob)
        {
            //We need to check the new job's eligibility based on the previous job's scheduled operations
            foreach (ResourceOperation newOp in copyMO.CurrentPath.GetOperationsByLevel(false))
            {
                //Find the scheduled resource of the equivalent op in the source job
                Resource scheduledResource = opScheduledResource[newOp.ExternalId];
                if (!newOp.ResourceRequirements.PrimaryResourceRequirement.IsEligible(scheduledResource, a_productRuleManager))
                {
                    needsToUnscheduleJob = true;
                    break;
                }
            }
        }

        //Need to compute eligiblity because other resources may now be capable 
        a_sourceMO.Job.ComputeEligibility(a_productRuleManager);

        Add(copyMO);

        if (needsToUnscheduleJob)
        {
            a_sourceMO.Job.Unschedule();
        }

        return copyMO;
    }

    internal ManufacturingOrder AddManufacturingOrderCopy(ManufacturingOrder a_sourceMO)
    {
        ManufacturingOrder copyMO = CreateInitializedCopyOfMO(a_sourceMO);

        copyMO.Split = true;
        copyMO.SplitFromManufacturingOrderId = a_sourceMO.Id;
        Add(copyMO);
        return copyMO;
    }

    private ManufacturingOrder CreateInitializedCopyOfMO(ManufacturingOrder a_sourceMO)
    {
        ManufacturingOrder copyMO = a_sourceMO.CreateUnitializedCopy();

        BaseId nextId = NextID();
        copyMO.Id = nextId;

        ResetExternalIdAndName(a_sourceMO, copyMO);

        AfterRestoreReferences.Helpers.ResetContainedIds(Serialization.VersionNumber, copyMO);
        copyMO.RestoreReferences(m_job, m_scenarioDetail.PlantManager, m_scenarioDetail.CapabilityManager, m_scenarioDetail, m_scenarioDetail.WarehouseManager, m_scenarioDetail.ItemManager);
        copyMO.RestoreReferences2();

        copyMO.Unschedule();

        return copyMO;
    }

    private void ResetExternalIdAndName(ManufacturingOrder a_sourceMO, ManufacturingOrder a_mo)
    {
        a_mo.ExternalId = MakeDefaultName(a_sourceMO.ExternalId.TrimEnd(null), Simulation.Scheduler.Job.SplitHelpers.SPLIT_MO_SEPERATOR_CHARS, a_mo.Id.Value);
        a_mo.Name = MakeDefaultName(a_sourceMO.Name.TrimEnd(null), Simulation.Scheduler.Job.SplitHelpers.SPLIT_MO_SEPERATOR_CHARS, a_mo.Id.Value);
    }

    internal void ResetAllMOExternalIdAndNames()
    {
        for (int i = 0; i < Count; ++i)
        {
            ManufacturingOrder mo = this[i];
            ResetExternalIdAndName(mo, mo);
        }
    }
    #endregion

    #region Unsplit
    internal void Unsplit(ManufacturingOrder a_leftMO, ManufacturingOrder a_rightMO, ProductRuleManager a_productRuleManager, IScenarioDataChanges a_scenarioDataChanges)
    {
        bool needsToUnschedule = a_leftMO.Join(a_rightMO, a_productRuleManager);
        if (needsToUnschedule)
        {
            a_leftMO.Job.Unschedule();
        }

        //Need to compute eligiblity because other resources may now be capable 
        a_leftMO.Job.ComputeEligibility(a_productRuleManager);
        Delete(a_rightMO, a_scenarioDataChanges);
    }
    #endregion

    #region Cycle Increment and Decrement
    internal void HandleScenarioDetailChangeMOQtyT(ScenarioDetailChangeMOQtyT a_t, ProductRuleManager a_productRuleManager)
    {
        if (a_t.Qty <= 0)
        {
            throw new PTValidationException("2284");
        }

        ManufacturingOrder sourceMO = GetById(a_t.ActivityKey.MOId);

        if (sourceMO == null)
        {
            throw new PTValidationException("2285");
        }

        ResourceOperation ro = (ResourceOperation)sourceMO.FindOperation(a_t.ActivityKey.OperationId);

        if (ro == null)
        {
            throw new PTValidationException("2286");
        }

        InternalActivity ia = ro.Activities.FindActivity(a_t.ActivityKey.ActivityId);

        if (ia == null)
        {
            throw new PTValidationException("2287");
        }

        sourceMO.ChangeQty(a_t.Qty, ro, ia, a_productRuleManager);
    }

    //internal void ChangeQty_______(long a_startTicks, long a_finishTicks, InternalActivity a_op)
    //{
    //    decimal newReqFinishQty = FindQuantityThatCanBeProcessedBetweenStartAndFinish(a_startTicks, a_finishTicks, a_op);
    //    decimal ratio = newReqFinishQty / a_op.RequiredFinishQty;
    //}
    #endregion
    #endregion

    #region Properties
    public int OperationCount
    {
        get
        {
            int opCount = 0;
            for (int i = 0; i < Count; i++)
            {
                opCount += this[i].OperationCount;
            }

            return opCount;
        }
    }

    public int FinishedOperationCount
    {
        get
        {
            int opCount = 0;
            for (int i = 0; i < Count; i++)
            {
                opCount += this[i].FinishedOperationCount;
            }

            return opCount;
        }
    }
    #endregion Properties

    /// <summary>
    /// Whether any activity has any of its blocks scheduled within its frozan span.
    /// </summary>
    /// <param name="a_spanCalc">Used to calculate the resource frozen span.</param>
    /// <returns></returns>
    internal bool AnyActivityInStableSpan()
    {
        for (int i = 0; i < Count; ++i)
        {
            ManufacturingOrder mo = GetByIndex(i);
            if (mo.AnyActivityInStableSpan())
            {
                return true;
            }
        }

        return false;
    }
}