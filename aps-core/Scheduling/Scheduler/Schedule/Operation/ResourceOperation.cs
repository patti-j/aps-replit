using System.ComponentModel;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Extensions;
using PT.ERPTransmissions;
using PT.Scheduler.Schedule;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// An Operation that is performed on a Machine.
/// </summary>
public partial class ResourceOperation : InternalOperation, ICloneable, IPTDeserializableIdGen
{
    #region IPTSerializable Members
    public new static readonly int UNIQUE_ID = 306;

    public ResourceOperation(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_reader, a_idGen) { }

    //Not needed until there is something to serialize
    //public override void Serialize(PT.Common.IWriter writer)
    //{
    //    base.Serialize(writer);
    //}

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    internal ResourceOperation(BaseId a_id, ManufacturingOrder a_manufacturingOrder, JobT.ResourceOperation a_jobTOperation, CapabilityManager a_resourceCapabilities, ScenarioDetail a_sd, bool a_isErpUpdate, IScenarioDataChanges a_dataChanges, ISystemLogger a_errorReporter, bool a_createDefaultActivity, UserFieldDefinitionManager a_udfManager, bool a_autoDeleteOperationAttributes)
        : base(a_id, a_manufacturingOrder, a_jobTOperation, a_resourceCapabilities, a_sd, a_isErpUpdate, a_dataChanges, a_errorReporter, a_createDefaultActivity, a_udfManager, a_autoDeleteOperationAttributes) { }

    /// <summary>
    /// Create a Resource Operation from an existing Resource Operation.  Used for BreakOffs.
    /// </summary>
    internal ResourceOperation(BaseId a_id, InternalOperation a_sourceOp, ManufacturingOrder a_parentMo, IScenarioDataChanges a_dataChanges, BaseIdGenerator a_idGen)
        : base(a_id, a_sourceOp, a_parentMo, a_idGen) { }

    /// <summary>
    /// Set the quantities for the Operations based on the new and old mo qties.
    /// </summary>
    internal static void AdjustQuantitiesForBreakOff(ResourceOperation a_sourceOp, ResourceOperation a_newOp, decimal a_sourceMoReqdQty, decimal a_newMoReqdQty)
    {
        decimal breakOffFraction = a_newMoReqdQty / a_sourceMoReqdQty;
        decimal newOpNewQty = breakOffFraction * a_sourceOp.RequiredFinishQty;

        if (newOpNewQty <= 0)
        {
            throw new PTValidationException("2240", new object[] { a_newOp.Name, newOpNewQty });
        }

        decimal sourceOpNewQty = a_sourceOp.RequiredFinishQty - newOpNewQty;

        //Update Activity quantities
        a_sourceOp.UpdateRequiredFinishQuantity(sourceOpNewQty);
        a_newOp.UpdateRequiredFinishQuantity(newOpNewQty);
    }

    /// <summary>
    /// Set the quantities for the Operations based on the new and old mo qties.
    /// The ratio between op qty and mo qty is maintained.
    /// </summary>
    internal static void AdjustQuantitiesForTemplateCopy(ResourceOperation a_sourceOp, ResourceOperation a_newOp, decimal a_sourceMoReqdQty, decimal a_newMoReqdQty, Product a_primaryProduct, ScenarioOptions a_scenarioOptions)
    {
        decimal newOpNewQty = a_sourceOp.RequiredFinishQty * a_newMoReqdQty / a_sourceMoReqdQty;
        decimal ratio = a_newMoReqdQty / a_sourceMoReqdQty;

        if (newOpNewQty <= 0)
        {
            throw new PTValidationException("2241", new object[] { a_newOp.Name, newOpNewQty });
        }

        //Update Activity quantities
        a_newOp.UpdateRequiredFinishQuantity(newOpNewQty);
        a_newOp.Products.AdjustOutputQtys(ratio, a_newMoReqdQty, a_scenarioOptions, a_primaryProduct);
        a_newOp.MaterialRequirements.AdjustQtys(ratio, a_newMoReqdQty, a_scenarioOptions);
    }
    #endregion

    #region Properties
    /// <summary>
    /// Special summary or troubleshooting information.
    /// </summary>
    [Browsable(false)]
    public override string Analysis
    {
        get
        {
            System.Text.StringBuilder analysis = new();
            if (OnHold)
            {
                analysis.Append(string.Format("{2}\t\tOn-Hold until {0}   {1}".Localize(), HoldUntil.ToDisplayTime().ToLongDateString(), HoldReason, Environment.NewLine));
            }

            if (DbrJitStartDateTicks == JitNotCalculableTicks) // this means the jit could not be calculated, most likely due to lack of eligible resources.
            {
                analysis.Append("JIT Start Date coult not be calculated. One possible reason is that there are no eligible resources that this Operation can schedule on.".Localize());
            }

            if (Scheduled)
            {
                if (Activities.Count == 1) //Don't include activity analysis for each activity -- TNG for example has hundres of splits.
                {
                    analysis.Append(string.Format("{1}{1}\t\t--Activity--{0}".Localize(), Activities.GetByIndex(0).Analysis, Environment.NewLine));
                }
                else
                {
                    analysis.Append(string.Format("Split into {0} Activities.".Localize(), Activities.Count));
                }
            }

            if (!Scheduled && ResourceRequirements.Count > 1)
            {
                analysis.Append("Since there are multiple Resource Requirements please verify that there are enough different Resources with the needed Capabilities to satisfy all of them together.".Localize());
            }

            for (int rrI = 0; rrI < ResourceRequirements.Count; rrI++)
            {
                analysis.Append(Environment.NewLine + "\t\t" + ResourceRequirements.GetByIndex(rrI).Analysis);
            }

            for (int mrI = 0; mrI < MaterialRequirements.Count; mrI++)
            {
                analysis.Append(Environment.NewLine + "\t\t" + ((MaterialRequirement)MaterialRequirements.GetRow(mrI)).Analysis);
            }

            return analysis.ToString();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Returns the amount of estimated standard cycle time remaining to perform the operation.
    /// The actual value may differ from this value when scheduled. Product Rules and Batch resources
    /// can have an effect on the actual time.
    /// </summary>
    public TimeSpan GetRemainingRunSpan()
    {
        decimal remainingStartQty = PlanningScrapPercentAdjustedQty(PlanningScrapPercent, RemainingFinishQty);
        return new TimeSpan((long)(remainingStartQty / QtyPerCycle * CycleSpanTicks));
    }

    /// <summary>
    /// This is the sum of the Activity durations times there resources' costs, if scheduled.
    /// </summary>
    /// <returns></returns>
    public override decimal GetResourceCost()
    {
        decimal resourceCost = 0;
        for (int activityI = 0; activityI < Activities.Count; activityI++)
        {
            InternalActivity activity = Activities.GetByIndex(activityI);
            resourceCost += activity.GetResourceCost();
        }

        return resourceCost;
    }

    /// <summary>
    /// This is the sum of the Activity durations times their resources' costs, if scheduled.
    /// </summary>
    /// <returns></returns>
    public override decimal GetResourceCarryingCost()
    {
        decimal carryingCost = 0;
        for (int activityI = 0; activityI < Activities.Count; activityI++)
        {
            InternalActivity activity = Activities.GetByIndex(activityI);
            carryingCost += activity.GetResourceCarryingCost();
        }

        return carryingCost;
    }

    /// <summary>
    /// Returns a list of all Resources that are eligible for any Resource Requirement for this Operation.
    /// </summary>
    /// <returns></returns>
    public List<Resource> GetEligibleResourcesForAllPlants()
    {
        List<Resource> eligResources = new();
        HashSet<BaseId> addedResourceIdsHash = new();

        for (int rrI = 0; rrI < ResourceRequirements.Count; rrI++)
        {
            ResourceRequirement rr = ResourceRequirements.GetByIndex(rrI);
            List<Resource> rrResources = rr.GetEligibleResources();
            for (int eligResI = 0; eligResI < rrResources.Count; eligResI++)
            {
                Resource resource = rrResources[eligResI];
                if (!addedResourceIdsHash.Contains(resource.Id))
                {
                    eligResources.Add(resource);
                    addedResourceIdsHash.Add(resource.Id);
                }
            }
        }

        return eligResources;
    }

    public List<Resource> GetEligibleResourcesForPlant(BaseId a_plantId)
    {
        List<Resource> eligResources = new();
        HashSet<BaseId> addedResourceIdsHash = new();

        for (int rrI = 0; rrI < ResourceRequirements.Count; rrI++)
        {
            ResourceRequirement rr = ResourceRequirements.GetByIndex(rrI);
            List<Resource> rrResources = rr.GetEligibleResources();
            for (int eligResI = 0; eligResI < rrResources.Count; eligResI++)
            {
                Resource resource = rrResources[eligResI];
                if (resource.PlantId == a_plantId && !addedResourceIdsHash.Contains(resource.Id))
                {
                    eligResources.Add(resource);
                    addedResourceIdsHash.Add(resource.Id);
                }
            }
        }

        return eligResources;
    }
    #endregion

    #region Transmission functionality
    internal override void Receive(OperationIdBaseT a_t, ProductRuleManager a_productRuleManager, IScenarioDataChanges a_dataChanges)
    {
        if (a_t is SplitOperationT)
        {
            SplitValidation();

            bool scheduled = ManufacturingOrder.Scheduled;
            long scheduledStartDate = ManufacturingOrder.GetScheduledStartDate().Ticks;

            SplitOperation((SplitOperationT)a_t, a_dataChanges);
            Job.ComputeEligibility(a_productRuleManager); //We don't really want to unschedule here because it would make splitting and moving activities infeasible.
            ManufacturingOrder.Job.CalculateJitTimes(ManufacturingOrder.ScenarioDetail.Clock, true); // Move this call back into the Job.

            //Expedite the MO if specified in the transmission, otherwise just a TimeAdjustment
            if (((SplitOperationT)a_t).splitSettings.ExpediteAfterSplit && scheduled)
            {
                ManufacturingOrder.ScenarioDetail.ExpediteMO(a_t, scheduledStartDate, ManufacturingOrder, false, false, a_dataChanges); // Move call to ScenarioDetail.
            }
            else
            {
                ManufacturingOrder.ScenarioDetail.TimeAdjustment(a_t);
            }
        }
        else if (a_t is UnSplitOperationT)
        {
            SplitValidation();

            bool scheduled = ManufacturingOrder.Scheduled;
            long scheduledStartDate = ManufacturingOrder.GetScheduledStartDate().Ticks;

            Unsplit(a_dataChanges);
            ManufacturingOrder.Job.CalculateJitTimes(ManufacturingOrder.ScenarioDetail.Clock, true); // Move this call back into the Job.

            //Expedite the MO if specified in the transmission, otherwise just a TimeAdjustment
            if (((UnSplitOperationT)a_t).splitSettings.ExpediteAfterSplit && scheduled)
            {
                ManufacturingOrder.ScenarioDetail.ExpediteMO(a_t, scheduledStartDate, ManufacturingOrder, false, false, a_dataChanges); // Move call to ScenarioDetail.
            }
            else
            {
                ManufacturingOrder.ScenarioDetail.TimeAdjustment(a_t);
            }
        }
        else if (a_t is ActivityIdBaseT) //JMC TODO call base here and move this to InternalOperation.
        {
            ActivityIdBaseT activityT = (ActivityIdBaseT)a_t;
            InternalActivity activity = Activities[activityT.activityId];
            activity.Receive(activityT, a_dataChanges);
        }
    }

    private void SplitValidation()
    {
        if (!ScenarioDetail.ScenarioOptions.SplitOperationEnabled)
        {
            throw new ValidationException("2242");
        }
    }

    private const int c_maxAllowedSplits = 1000; //Avoid huge numbers likely to be accidents and cause problems.

    private void SplitOperation(SplitOperationT a_t, IScenarioDataChanges a_dataChanges)
    {
        ProductionInfo.OnlyAllowManualUpdatesToSplitOperation = true;
        switch (a_t.splitSettings.HowToSplit)
        {
            case SplitSettings.howToSplitTypes.EligibleResourceCount:
            case SplitSettings.howToSplitTypes.ActivityCount:
                SplitByCount(a_t.newActivityCount, a_t.splitSettings.IntegerSplits || WholeNumberSplits, a_dataChanges);
                break;
            case SplitSettings.howToSplitTypes.PercentOfQty:
                SplitByPercent(a_t.splitPercent, a_t.splitSettings.IntegerSplits || WholeNumberSplits, a_dataChanges);
                break;
            case SplitSettings.howToSplitTypes.MaxActivityDuration:
                SplitByMaxDuration(a_t.maxActivityRunSpan, a_t.splitSettings.IntegerSplits || WholeNumberSplits, a_dataChanges);
                break;
            case SplitSettings.howToSplitTypes.ListOfSplitQties:
                SplitByQtyList(a_t.QtiesToSplit, a_t.splitSettings.IntegerSplits || WholeNumberSplits, a_dataChanges);
                break;
        }
    }

    /// <summary>
    /// Split the Operation into the specified number of Activities.
    /// </summary>
    /// <param name="a_newActivityCount"></param>
    public void SplitByCount(int a_newActivityCount, bool a_integerSplits, IScenarioDataChanges a_dataChanges)
    {
        SplitSetup(a_dataChanges);

        //Get the original Activity --  there must be one.
        InternalActivity originalActivity = Activities.GetByIndex(0);
        decimal remainingQty = originalActivity.RemainingQty;

        //Validate parameters
        if (a_integerSplits && a_newActivityCount > remainingQty)
        {
            throw new ValidationException("2243", new object[] { a_newActivityCount, remainingQty });
        }

        if (a_newActivityCount < 2)
        {
            return; //nothing to do
        }

        if (a_newActivityCount > c_maxAllowedSplits)
        {
            throw new ValidationException("2244", new object[] { a_newActivityCount, c_maxAllowedSplits });
        }

        //Divide the remainingQty among the Activites
        decimal newActivityQty = remainingQty / a_newActivityCount;
        if (a_integerSplits)
        {
            newActivityQty = Math.Floor(newActivityQty);
        }
        else
        {
            newActivityQty = ScenarioDetail.ScenarioOptions.RoundQty(newActivityQty);
        }

        for (int i = 1; i < a_newActivityCount; i++)
        {
            InternalActivity newActivity = new InternalActivity(Activities.Count + 1, Activities.IdGen.NextID(), originalActivity.Operation, originalActivity);
            newActivity.RequiredFinishQty = originalActivity.RequiredFinishQty - newActivityQty;
            newActivity.ProductionStatus = originalActivity.ProductionStatus;
            originalActivity.RequiredFinishQty = newActivityQty;
            Activities.Add(newActivity);

            //Split setup
            switch (SetupSplitType)
            {
                case OperationDefs.ESetupSplitType.None:
                    //Nothing to split
                    break;
                case OperationDefs.ESetupSplitType.FirstActivity:
                    newActivity.SplitSetup(0);
                    break;
                case OperationDefs.ESetupSplitType.SplitByQty:
                    //Update both the new and old activity based on the new split ratio
                    newActivity.SplitSetup((long)ScenarioDetail.ScenarioOptions.RoundQty(newActivity.SplitRatio * newActivity.Operation.SetupSpanTicks));
                    originalActivity.SplitSetup((long)ScenarioDetail.ScenarioOptions.RoundQty(originalActivity.SplitRatio * originalActivity.Operation.SetupSpanTicks));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ScheduleSplitActivity(originalActivity, newActivity);
            originalActivity = newActivity;
        }
    }

    /// <summary>
    /// Split off the specified qty from the Operation.
    /// </summary>
    /// <param name="newActivityCount"></param>
    private void SplitByQty(decimal a_newActivityQty, bool a_integerSplits, IScenarioDataChanges a_dataChanges)
    {
        SplitSetup(a_dataChanges);

        //Get the original Activity --  there must be one.
        InternalActivity originalActivity = Activities.GetByIndex(0);
        decimal remainingQty = originalActivity.RemainingQty;

        //Round the quantity down if doing integer splits
        if (a_integerSplits)
        {
            a_newActivityQty = Math.Floor(a_newActivityQty);
        }
        else
        {
            a_newActivityQty = ScenarioDetail.ScenarioOptions.RoundQty(a_newActivityQty);
        }

        //Validate parameters
        if (a_newActivityQty <= 0)
        {
            return; //nothing to do
        }

        if (a_newActivityQty > remainingQty)
        {
            throw new ValidationException("2245", new object[] { a_newActivityQty, UOM, remainingQty });
        }

        SplitOffAnotherQty(a_newActivityQty);
    }

    public void SplitByQtyList(List<decimal> a_dblList, bool a_integerSplits, IScenarioDataChanges a_dataChanges)
    {
        SplitSetup(a_dataChanges);

        InternalActivity originalActivity = Activities.GetByIndex(0);
        //Validate
        if (a_dblList.Count == 0)
        {
            throw new ValidationException("2246");
        }

        decimal totalQty = 0;
        for (int i = 0; i < a_dblList.Count; i++)
        {
            totalQty += a_dblList[i];
        }

        if (totalQty >= originalActivity.RemainingQty)
        {
            throw new ValidationException("2247", new object[] { totalQty, UOM, originalActivity.RemainingQty });
        }

        //Split off the n-1st qties.  The UI adds the remaining qty to the end of the list so that will be left as the original.
        for (int i = 0; i < a_dblList.Count; i++)
        {
            if (a_integerSplits)
            {
                SplitOffAnotherQty(Math.Floor(a_dblList[i]));
            }
            else
            {
                decimal nextSplitQty = a_dblList[i];
                nextSplitQty = ScenarioDetail.ScenarioOptions.RoundQty(nextSplitQty);
                SplitOffAnotherQty(nextSplitQty);
            }
        }
    }

    /// <summary>
    /// Splits off the specified qty from the original (zero index) Activity.
    /// No validation is done.
    /// </summary>
    /// <param name="newOpQty"></param>
    private void SplitOffAnotherQty(decimal a_newActivityQty)
    {
        InternalActivity originalActivity = Activities.GetByIndex(0);

        InternalActivity newActivity = new InternalActivity(Activities.Count + 1, Activities.IdGen.NextID(), originalActivity.Operation, originalActivity);
        newActivity.RequiredFinishQty = a_newActivityQty;
        newActivity.ProductionStatus = originalActivity.ProductionStatus;
        originalActivity.RequiredFinishQty -= a_newActivityQty;
        //Set the status if the operation has no more remaining quantity
        if (originalActivity.RemainingQty <= 0)
        {
            //If its running and has post processing set to post processing. If not running, set to finished.
            if (originalActivity.ScheduledOrDefaultProductionInfo.PostProcessingSpanTicks > 0)
            {
                if (originalActivity.ProductionStatus == InternalActivityDefs.productionStatuses.Running)
                {
                    originalActivity.ProductionStatus = InternalActivityDefs.productionStatuses.PostProcessing;
                }
            }
            else
            {
                originalActivity.ProductionStatus = InternalActivityDefs.productionStatuses.Finished;
            }
        }

        Activities.Add(newActivity);

        //Split setup
        switch (SetupSplitType)
        {
            case OperationDefs.ESetupSplitType.None:
                //Nothing to split
                break;
            case OperationDefs.ESetupSplitType.FirstActivity:
                newActivity.SplitSetup(0);
                break;
            case OperationDefs.ESetupSplitType.SplitByQty:
                //Update both the new and old activity based on the new split ratio
                newActivity.SplitSetup((long)ScenarioDetail.ScenarioOptions.RoundQty(newActivity.SplitRatio * newActivity.Operation.SetupSpanTicks));
                originalActivity.SplitSetup((long)ScenarioDetail.ScenarioOptions.RoundQty(originalActivity.SplitRatio * originalActivity.Operation.SetupSpanTicks));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        //Update the new actvities scheduled status
        ScheduleSplitActivity(originalActivity, newActivity);
    }

    /// <summary>
    /// Schedules an activity that was created from splitting. Schedules next to the activity it was split off of.
    /// </summary>
    /// <param name="a_sourceAct">Source Activity</param>
    /// <param name="a_copyAct">Split Activity to schedule</param>
    internal void ScheduleSplitActivity(InternalActivity a_sourceAct, InternalActivity a_copyAct)
    {
        if (a_sourceAct.Scheduled)
        {
            Batch sourceBatch = a_sourceAct.Batch;
            Batch newBatch = ScenarioDetail.Batch_CreateBatch(a_sourceAct.PrimaryResourceRequirementBlock.ScheduledResource, a_copyAct, sourceBatch.m_si, sourceBatch);

            for (int reqI = 0; reqI < a_sourceAct.ResourceRequirementBlockCount; ++reqI)
            {
                ResourceBlock sourceRB = a_sourceAct.GetResourceRequirementBlock(reqI);
                ResourceBlock copyRB = null;

                if (sourceRB != null)
                {
                    // [TANK_DEBUG]: test changes to splitting. The new ResourceBlock below use to take a parameter with the source batches endTicks +1 as the start ticks.
                    sourceRB.Batch.EndTicks = sourceRB.StartTicks + 1;
                    newBatch.StartTicks = sourceRB.StartTicks + 1;

                    //TODO: Test splitting with the new OCP
                    copyRB = new ResourceBlock(sourceRB.Id, newBatch, ResourceRequirements.GetByIndex(sourceRB.ResourceRequirementIndex), sourceRB.ScheduledResource, sourceBatch.m_si.m_ocp);

                    ResourceBlockList.Node node = sourceRB.ScheduledResource.Blocks.FindByRef(sourceRB);
                    copyRB.MachineBlockListNode = sourceRB.ScheduledResource.Blocks.Add(copyRB, node);
                    newBatch.m_rrBlockNodes[reqI] = copyRB.MachineBlockListNode;
                }

                a_copyAct.Schedule(copyRB);
            }
        }
    }

    /// <summary>
    /// Split off the specified percent of the remaining qty from the Operation.
    /// </summary>
    /// <param name="newActivityCount"></param>
    private void SplitByPercent(decimal a_newActivityPercent, bool a_integerSplits, IScenarioDataChanges a_dataChanges)
    {
        SplitSetup(a_dataChanges);

        //Get the original Activity --  there must be one.
        InternalActivity originalActivity = Activities.GetByIndex(0);
        decimal remainingQty = originalActivity.RemainingQty;

        decimal newActivityQty = a_newActivityPercent * remainingQty;
        if (newActivityQty < 1 && a_integerSplits)
        {
            throw new ValidationException("2248");
        }

        SplitByQty(newActivityQty, a_integerSplits, a_dataChanges);
    }

    /// <summary>
    /// Split the operation into the number of Activities required to reduce the standard run time of all Activities below the
    /// specified maximum timespan.
    /// </summary>
    /// <param name="newActivityCount"></param>
    private void SplitByMaxDuration(TimeSpan a_maxActivityDuration, bool a_integerSplits, IScenarioDataChanges a_dataChanges)
    {
        SplitSetup(a_dataChanges);

        if (a_maxActivityDuration.Ticks < CycleSpanTicks)
        {
            throw new ValidationException("2249");
        }

        //Get the original Activity --  there must be one.
        InternalActivity originalActivity = Activities.GetByIndex(0);
        
        decimal remainingQty = originalActivity.RemainingQty;

        decimal remainingCyclesDbl = remainingQty / QtyPerCycle;
        if (remainingCyclesDbl < 2)
        {
            //There are not enough Cycles to Split so do nothing.
            return;
        }

        int cyclesPerSplit = (int)Math.Floor(a_maxActivityDuration.TotalSeconds / CycleSpan.TotalSeconds);
        decimal qtyPerSplit = cyclesPerSplit * QtyPerCycle;

        while (true) //can split off another one
        {
            //Calculate the next split quantity. 
            decimal nextQtyToSplit = Math.Min(originalActivity.RemainingQty - qtyPerSplit, qtyPerSplit);
            if (nextQtyToSplit <= 0)
            {
                break;
            }

            InternalActivity newActivity = new InternalActivity(Activities.Count + 1, Activities.IdGen.NextID(), originalActivity.Operation, originalActivity);
            newActivity.RequiredFinishQty = nextQtyToSplit;
            newActivity.ProductionStatus = originalActivity.ProductionStatus;
            originalActivity.RequiredFinishQty -= nextQtyToSplit;

            //Split setup
            switch (SetupSplitType)
            {
                case OperationDefs.ESetupSplitType.None:
                    //Nothing to split
                    break;
                case OperationDefs.ESetupSplitType.FirstActivity:
                    newActivity.SplitSetup(0);
                    break;
                case OperationDefs.ESetupSplitType.SplitByQty:
                    //Update both the new and old activity based on the new split ratio
                    newActivity.SplitSetup((long)ScenarioDetail.ScenarioOptions.RoundQty(newActivity.SplitRatio * newActivity.Operation.SetupSpanTicks));
                    originalActivity.SplitSetup((long)ScenarioDetail.ScenarioOptions.RoundQty(originalActivity.SplitRatio * originalActivity.Operation.SetupSpanTicks));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Activities.Add(newActivity);
            ScheduleSplitActivity(originalActivity, newActivity);
        }
    }

    private void SplitSetup(IScenarioDataChanges a_dataChanges)
    {
        if (Split)
        {
            Unsplit(a_dataChanges);
        }
    }

    //2014.9.12: Splitting was changed to keep all reported values on one activity.
    //           This was no longer needed.
    //void DividProgressFromFirstActivityAcrossSplitsEvenly()
    //{
    //    int activityCount = Activities.Count;
    //    if (activityCount > 1)
    //    {
    //        InternalActivity fstActivity = Activities.GetByIndex(0);
    //        //decimal reportedSetupHrsPerActivity = fstActivity.ReportedSetupSpan.TotalHours / (decimal)activityCount; Doesn't make sense for Setup. Each split will have to setup too.
    //        decimal reportedRunHrsPerActivity = fstActivity.ReportedRunSpan.TotalHours / (decimal)activityCount;
    //        decimal reportedPostProcessingHrsPerActivity = fstActivity.ReportedPostProcessingSpan.TotalHours / (decimal)activityCount;

    //        decimal reportedGoodQtyPerActivity;
    //        decimal reportedScrapQtyPerActivity;
    //        if (WholeNumberSplits)
    //        {
    //            reportedGoodQtyPerActivity = Math.Round(fstActivity.ReportedGoodQty / (decimal)activityCount, MidpointRounding.AwayFromZero);
    //            reportedScrapQtyPerActivity = Math.Round(fstActivity.ReportedScrapQty / (decimal)activityCount, MidpointRounding.AwayFromZero);
    //        }
    //        else
    //        {
    //            reportedGoodQtyPerActivity = fstActivity.ReportedGoodQty / (decimal)activityCount;
    //            reportedScrapQtyPerActivity = fstActivity.ReportedScrapQty / (decimal)activityCount;
    //        }
    //        for (int i = 1; i < activityCount; i++)
    //        {
    //            InternalActivity splitActivity = Activities.GetByIndex(i);
    //            splitActivity.ReportedRunSpan = TimeSpan.FromHours(reportedRunHrsPerActivity);
    //            splitActivity.ReportedPostProcessingSpan = TimeSpan.FromHours(reportedPostProcessingHrsPerActivity);
    //            splitActivity.ReportedGoodQty = reportedGoodQtyPerActivity;
    //            splitActivity.ReportedScrapQty = reportedScrapQtyPerActivity;
    //            //splitActivity.ReportedSetupSpan = TimeSpan.FromHours(reportedSetupHrsPerActivity);  not splitting setup

    //            //Subtract from first activity too

    //            fstActivity.ReportedRunSpan = fstActivity.ReportedRunSpan.Subtract(splitActivity.ReportedRunSpan);
    //            fstActivity.ReportedPostProcessingSpan = fstActivity.ReportedPostProcessingSpan.Subtract(splitActivity.ReportedPostProcessingSpan);
    //            fstActivity.ReportedGoodQty -= splitActivity.ReportedGoodQty;
    //            fstActivity.ReportedScrapQty -= splitActivity.ReportedScrapQty;
    //            //fstActivity.ReportedSetupSpan=fstActivity.ReportedSetupSpan.Subtract(splitActivity.ReportedSetupSpan); not splitting setup
    //        }
    //    }
    //}

    /// <summary>
    /// Get rid of all but one Activity. Move Reported hours and quantities from all Activities into the one that remains.
    /// </summary>
    /// <param name="a_dataChanges"></param>
    /// <param name="a_autoSplit">Flag to indicate this method was called by an auto-split logic</param>
    /// <exception cref="ValidationException"></exception>
    internal void Unsplit(IScenarioDataChanges a_dataChanges, bool a_autoSplit = false)
    {
        //Validate
        if (Activities.Count == 0)
        {
            throw new ValidationException("2250");
        }

        if (Activities.Count == 1)
        {
            return; // nothing to do.
        }

        ProductionInfo.OnlyAllowManualUpdatesToSplitOperation = true;

        //We'll get rid of all but the zero Activity and store all Reported Qties and Times in Activity zero.
        InternalActivity activity0 = Activities.GetByIndex(0);
        for (int i = Activities.Count - 1; i > 0; i--)
        {
            InternalActivity nextActivity = Activities.GetByIndex(i);
            activity0.RequiredFinishQty += nextActivity.RequiredFinishQty;
            //Unfinish activity zero if it's finished so that it schedules.
            if (activity0.ProductionStatus == InternalActivityDefs.productionStatuses.Finished)
            {
                activity0.ProductionStatus = InternalActivityDefs.productionStatuses.Ready;
            }

            activity0.ReportedGoodQty += nextActivity.ReportedGoodQty;
            activity0.ReportedScrapQty += nextActivity.ReportedScrapQty;
            activity0.ReportedPostProcessingSpan = activity0.ReportedPostProcessingSpan.Add(nextActivity.ReportedPostProcessingSpan);
            activity0.ReportedRunSpan = activity0.ReportedRunSpan.Add(nextActivity.ReportedRunSpan);
            //activity0.ReportedSetupSpan=activity0.ReportedSetupSpan.Add(nextActivity.ReportedSetupSpan); doesn't make sense for Setup.
            if (SetupSplitType == OperationDefs.ESetupSplitType.SplitByQty)
            {
                //Rejoin setup. It is possible that setup was manually changed. In that case the new setup will be rolled up to the original activity
                activity0.SplitSetup(activity0.ScheduledOrDefaultProductionInfo.SetupSpanTicks + nextActivity.ScheduledOrDefaultProductionInfo.SetupSpanTicks, a_autoSplit, true);
            }

            nextActivity.Unschedule(true, true);
            nextActivity.Deleting(ScenarioDetail.PlantManager, a_dataChanges);
            Activities.Remove(nextActivity);
            m_activitiesLeftToSchedule--;
        }

        //Unsplit overrides previous autosplits
        AutoSplitInfo.Unsplit();
        activity0.InitializeProductionInfoForResources(ScenarioDetail.PlantManager, ScenarioDetail.ProductRuleManager, ScenarioDetail.ExtensionController);
    }
    #endregion

    /// <summary>
    /// Gets the ManualUpdate flag if it's a Tank, otherwise false.
    /// </summary>
    internal override void PopulateJobDataSet(JobDataSet.ManufacturingOrderRow a_moRow, JobManager a_jobs, ref JobDataSet r_dataSet)
    {
        GetReportedStartDate(out long reportedStartTicks);
        GetReportedFinishDate(out long reportedFinishedTicks);

        DateTime scheduledEndDate = Scheduled ? ScheduledEndDate : PTDateTime.MinDateTime;

        r_dataSet.ResourceOperation.AddResourceOperationRow(
            ManufacturingOrder.Job.ExternalId,
            a_moRow.ExternalId,
            Id.ToBaseType(),
            ExternalId,
            Name,
            Description,
            PercentFinished,
            Finished,
            Scheduled,
            RequiredFinishQty,
            RemainingFinishQty,
            StartDateTime.ToDisplayTime().ToDateTime(),
            scheduledEndDate.ToDisplayTime().ToDateTime(),
            new DateTime(reportedStartTicks).ToDisplayTime().ToDateTime(),
            new DateTime(reportedFinishedTicks).ToDisplayTime().ToDateTime(),
            ResourcesUsed,
            JitStartDate.ToDisplayTime().ToDateTime(),
            NeedDate.ToDisplayTime().ToDateTime(),
            SetupSpan.TotalHours,
            ProductionSetupCost,
            CycleSpan.TotalHours,
            QtyPerCycle,
            PostProcessingSpan.TotalHours,
            CleanSpan.TotalHours,
            CleanoutGrade,
            CleanoutCost,
            StorageSpan.TotalHours,
            BatchCode,
            ColorUtils.ConvertColorToHexString(SetupColor),
            SetupNumber,
            Late,
            LatestConstraint,
            LatestConstraintDate.ToDisplayTime().ToDateTime(),
            ExpectedFinishQty,
            UseExpectedFinishQty,
            PlanningScrapPercent,
            DeductScrapFromRequired,
            ExpectedScrapQty,
            RequiredStartQty,
            ReportedGoodQty,
            WorkContent,
            StandardRunSpan.TotalHours,
            AutoSplit,
            WholeNumberSplits,
            CanPause,
            CanSubcontract,
            CarryingCost,
            CompatibilityCode,
            UseCompatibilityCode,
            (int)Locked,
            OnHold,
            HoldReason,
            HoldUntil.ToDisplayTime().ToDateTime(),
            (int)Anchored,
            CommitStartDate.ToDisplayTime().ToDateTime(),
            CommitEndDate.ToDisplayTime().ToDateTime(),
            IsRework,
            SuccessorProcessing.ToString(),
            KeepSuccessorsTimeLimit.TotalHours,
            Omitted.ToString(),
            Split,
            TimeBasedReporting,
            UOM,
            Notes,
            OutputName,
            Bottleneck,
            AutoReportProgress,
            OverlapTransferQty,
            StandardSetupSpan.TotalHours,
            AutoFinish,
            -1,
            "",
            BaseOperationDefs.autoCreateRequirementsType.None.ToString(),
            InternalOperationDefs.splitUpdateModes.UpdateSplitsIndividually.ToString(),
            Started,
            ProductionInfo.OnlyAllowManualUpdatesToCycleSpan,
            ProductionInfo.OnlyAllowManualUpdatesToQtyPerCycle,
            ProductionInfo.OnlyAllowManualUpdatesToSetupSpan,
            ProductionInfo.OnlyAllowManualUpdatesToPostProcessingSpan,
            ProductionInfo.OnlyAllowManualUpdatesToCleanSpan,
            ProductionInfo.OnlyAllowManualUpdatesToPlanningScrapPercent,
            ProductionInfoBaseOperation.OnlyAllowManualupdates.Materials,
            ProductionInfoBaseOperation.OnlyAllowManualupdates.Products,
            ProductionInfo.OnlyAllowManualUpdatesToResourceRequirements,
            CanResize,
            UserFields == null ? "" : UserFields.GetUserFieldImportString(),
            // default value for ManualOnlyEndOfPostProcessing. Will be set by TankOperation
            false,
            Convert.ToDouble(StandardOperationBufferDays),
            PlannedScrapQty,
            ProductCode,
            AutoSplitInfo.AutoSplitType.ToString(),
            AutoSplitInfo.MinAutoSplitAmount,
            AutoSplitInfo.MaxAutoSplitAmount,
            SetupSplitType.ToString(),
            AutoSplitInfo.KeepSplitsOnSameResource,
            PreventSplitsFromIncurringSetup,
            PreventSplitsFromIncurringClean,
            CampaignCode,
            TotalSetupCost,
            SequenceHeadStartDays,
            AllowSameLotInNonEmptyStorageArea,
            DbrJitStartDate.ToDisplayTime().ToDateTime()
        );

        // populate any child class fields.
        PopulateJobDataSetHelper(ref r_dataSet);

        //Add Resource Requirements
        ResourceRequirements.PopulateJobDataSet(ref r_dataSet);

        //Add Activities
        Activities.PopulateJobDataSet(ref r_dataSet);

        //Add Materials
        MaterialRequirements.PopulateJobDataSet(ref r_dataSet, this);

        //Add Products
        Products.PopulateJobDataSet(ref r_dataSet, this);

        //Add Attributes
        for (int attI = 0; attI < Attributes.Count; attI++)
        {
            OperationAttribute a = Attributes[attI];
            r_dataSet.ResourceOperationAttributes.AddResourceOperationAttributesRow(
                a_moRow.JobExternalId,
                a.PTAttribute.ExternalId,
                a.Code,
                a.CodeManualUpdateOnly,
                a.Number,
                a.NumberManualUpdateOnly,
                a.Duration.TotalHours,
                a.DurationOverride,
                a.DurationManualUpdateOnly,
                a.Cost,
                a.CostOverride,
                a.CostManualUpdateOnly,
                ColorUtils.ConvertColorToHexString(a.ColorCode),
                a.ColorOverride,
                a.ColorCodeManualUpdateOnly,
                ManufacturingOrder.ExternalId,
                ExternalId,
                a.ShowInGantt,
                a.ShowInGanttManualUpdateOnly,
                a.ShowInGanttOverride
            );
        }

        //Add Subassemblies that are used in this operation (Manufacturing Orders that have this Operation as their Successor)
        for (int i = 0; i < a_jobs.Count; i++)
        {
            Job job = a_jobs[i];
            for (int moI = 0; moI < job.ManufacturingOrders.Count; moI++)
            {
                ManufacturingOrder mo = job.ManufacturingOrders[moI];
                ManufacturingOrder.SubassemblyType subassyType = mo.IsSubassembly(this);
                if (subassyType == ManufacturingOrder.SubassemblyType.SubassemblyOfOperation || (subassyType == ManufacturingOrder.SubassemblyType.SubassemblyOfMo && PartOfCurrentRouting() && !HasPredecessors()))

                {
                    r_dataSet._Subassembly.Add_SubassemblyRow(
                        Job.ExternalId,
                        ManufacturingOrder.ExternalId,
                        ExternalId,
                        mo.ProductName,
                        mo.RequiredQty,
                        mo.Job.ExternalId,
                        mo.ExternalId,
                        mo.GetScheduledEndDate().ToDisplayTime().ToDateTime(),
                        mo.Finished
                    );
                }
            }
        }
    }

    /// <summary>
    /// This method is overridden in child classes to update
    /// necessary fields before Activities, ResourceRequirements, etc are populated.
    /// </summary>
    protected virtual void PopulateJobDataSetHelper(ref JobDataSet r_dataSet) { }

    /// <summary>
    /// Returns the WorkCenterExternalId of the first scheduled Activity's Primary Resource Requirement.
    /// This is to facilitate update of ERP systems with the scheduled WorkCenter.
    /// </summary>
    /// <returns></returns>
    public string GetScheduledPrimaryWorkCenterExternalId()
    {
        InternalActivity act = GetLeadActivity();
        if (act?.Batch?.PrimaryResourceBlock is ResourceBlock block)
        {
            return block.ScheduledResource.WorkcenterExternalId;
        }

        return "";
    }

    /// <summary>
    /// Returns the Resource of the first scheduled Activity's Primary Resource Requirement.
    /// If none exists, returns null.
    /// </summary>
    /// <returns></returns>
    public Resource GetScheduledPrimaryResource()
    {
        InternalActivity act = GetLeadActivity();
        if (act?.Batch?.PrimaryResourceBlock is ResourceBlock block)
        {
            return block.ScheduledResource;
        }

        return null;
    }

    #region Cloning
    public virtual object Clone()
    {
        ResourceOperation clone = this.CopyInMemory(ScenarioDetail.IdGen, InternalClone);
        clone.AlternatePathNode = AlternatePathNode;
        return clone;
    }

    private static ResourceOperation InternalClone(IReader a_reader, BaseIdGenerator a_idGen)
    {
        return new ResourceOperation(a_reader, a_idGen);
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion

    /// <summary>
    /// The value of this field is based on the most constraining finished predecessor operation.
    /// </summary>
    public override decimal ExpectedFinishQty
    {
        get
        {
            if (AlternatePathNode == null)
            {
                return RequiredFinishQty;
            }

            decimal expectedFinishQty = RequiredFinishQty;

            if (UseExpectedFinishQty)
            {
                AlternatePath.AssociationCollection predecessors = AlternatePathNode.Predecessors;
                for (int predecessorI = 0; predecessorI < predecessors.Count; ++predecessorI)
                {
                    AlternatePath.Association association = predecessors[predecessorI];
                    decimal predecessorActualQty = association.Predecessor.Operation.Finished && association.Predecessor.Operation.ReportedGoodQty > 0 ? association.Predecessor.Operation.ReportedGoodQty : association.Predecessor.Operation.ExpectedFinishQty;
                    decimal ratio = predecessorActualQty / association.Predecessor.Operation.RequiredFinishQty;
                    decimal qty = RequiredFinishQty * ratio;
                    if (WholeNumberSplits)
                    {
                        qty = Math.Floor(qty);
                    }
                    else
                    {
                        qty = ScenarioDetail.ScenarioOptions.RoundQty(qty);
                    }

                    if (qty < expectedFinishQty)
                    {
                        expectedFinishQty = qty;
                    }
                }
            }

            return expectedFinishQty;
        }
    }

    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    /// <summary>
    /// The estimated number of cycles left. When scheduled the actual number of cycles may vary. Things that can affect the actual number of cycles include Product Rules and Batch Resources.
    /// </summary>
    public long Cycles => (long)Math.Ceiling(RequiredFinishQty / QtyPerCycle);

    /// <summary>
    /// Total time -- Number of Cycles times Time Per Cycle.
    /// </summary>
    public override TimeSpan RunSpan => new(Cycles * CycleSpanTicks);

    #region Cost
    public new decimal MaterialCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < MaterialRequirements.Count; i++)
            {
                cost += MaterialRequirements[i].TotalCost;
            }

            return cost;
        }
    }

    public override decimal LaborCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < Activities.Count; i++)
            {
                cost += Activities.GetByIndex(i).LaborCost;
            }

            return cost;
        }
    }

    public override decimal MachineCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < Activities.Count; i++)
            {
                cost += Activities.GetByIndex(i).MachineCost;
            }

            return cost;
        }
    }

    public override decimal SubcontractCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < Activities.Count; i++)
            {
                cost += Activities.GetByIndex(i).SubcontractCost;
            }

            return cost;
        }
    }

    public decimal TotalSetupCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < Activities.Count; i++)
            {
                cost += Activities.GetByIndex(i).TotalSetupCost;
            }

            return cost;
        }
    }
    public decimal TotalCleanoutCost
    {
        get
        {
            decimal cost = 0;
            for (int i = 0; i < Activities.Count; i++)
            {
                cost += Activities.GetByIndex(i).TotalCleanCost;
            }

            return cost;
        }
    }
    #endregion Cost

    #region Buffers
  /// <summary>
    /// Based on Operation Buffer Penetration Percent OK if less than 33%. Warning if between 33% and 66%. Critical if 66% or higher.
    /// </summary>
    public BufferDefs.WarningLevels CurrentBufferWarningLevel => BufferDefs.GetBufferWarningFromPercent(CurrentBufferPenetrationPercent);

    /// <summary>
    /// Based on Operation Buffer Penetration Percent OK if less than 33%. Warning if between 33% and 66%. Critical if 66% or higher.
    /// </summary>
    public BufferDefs.WarningLevels ScheduledBufferWarningLevel => BufferDefs.GetBufferWarningFromPercent(ScheduledBufferPenetrationPercent);


    public decimal CurrentBufferPenetrationPercent => GetBufferPenetrationPercent(true);

    public decimal ScheduledBufferPenetrationPercent => GetBufferPenetrationPercent(false);

    private decimal GetBufferPenetrationPercent(bool a_current)
    {
        if (!Scheduled)
        {
            return 0;
        }

        InternalActivity lastAct = GetLatestEndingScheduledActivity();
        InternalResource lastSchedRes = lastAct.Batch.PrimaryResource;

        long endTicks = a_current ? ScenarioDetail.Clock : lastAct.Batch.EndTicks;

        //TODO: How to handle Split Operations? For now, only use latest activity values
        long jitStart = lastAct.BufferInfo.GetResourceInfo(lastAct.Batch.PrimaryResource.Id).DbrJitStartDate;
        long bufferStart = lastAct.BufferInfo.GetResourceInfo(lastAct.Batch.PrimaryResource.Id).BufferNeedDate;
        long bufferEnd = lastAct.BufferInfo.GetResourceInfo(lastAct.Batch.PrimaryResource.Id).BufferEndDate;


        if (endTicks == jitStart)
        {
            //Op is ending exactly at the start of JIT --0% penetration
            return 0m;
        }

        long bufferOnlineCapacityTicks = lastSchedRes.FindOnlineCapacityBetweenTwoDates(bufferStart, bufferEnd);
        if (bufferOnlineCapacityTicks == 0)
        {
            //No buffer, no penetration
            return 0m;
        }

        //1. EndDate is before the JIt --early case, negative penetration
        if (endTicks < jitStart)
        {
            long onlineCapacityBeforeJIT = lastSchedRes.FindOnlineCapacityBetweenTwoDates(endTicks, jitStart);
            return (-onlineCapacityBeforeJIT / (decimal)bufferOnlineCapacityTicks) * 100m;
        }

        //2. EndDate is within or after the JIT --penetration is positive
        long overlappingCapacity = lastSchedRes.FindOnlineCapacityBetweenTwoDates(bufferStart, endTicks);
        return (overlappingCapacity / (decimal)bufferOnlineCapacityTicks) * 100m;
    }
    #endregion
}