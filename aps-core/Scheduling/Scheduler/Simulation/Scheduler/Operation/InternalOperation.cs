using PT.APSCommon;
using PT.Common.Attributes;
using PT.Common.Debugging;
using PT.Common.Extensions;
using PT.Common.Localization;
using PT.Scheduler.Schedule;
using PT.Scheduler.Schedule.Operation;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Extensions;
using PT.SchedulerDefinitions;
using System.Collections;

namespace PT.Scheduler;

/// <summary>
/// Provides the specifications of how a step in the manufacturing process will be performed within the factory.
/// </summary>
public abstract partial class InternalOperation : IEnumerator<MaterialRequirement>
{
    #region Simulation
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public override bool Scheduled
    {
        get
        {
            if (m_activities == null)
            {
                return false;
            }

            int scheduledCount = 0;

            for (int activityI = 0; activityI < m_activities.Count; ++activityI)
            {
                InternalActivity act = m_activities.GetByIndex(activityI);
                if (!act.Finished)
                {
                    if (act.Scheduled)
                    {
                        ++scheduledCount;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return scheduledCount > 0;
        }
    }

    protected int m_activitiesLeftToSchedule;

    internal void ActivityScheduled(InternalActivity a_act)
    {
        --m_activitiesLeftToSchedule;

        #if DEBUG
        if (m_activitiesLeftToSchedule < 0)
        {
            throw new Exception("__activitiesLeftToSchedule has gone less than 0.");
        }
        #endif

        ManufacturingOrder.ActivityScheduled(a_act);
    }

    private void SimScheduledInitialization()
    {
        m_activitiesLeftToSchedule = 0;

        for (int i = 0; i < Activities.Count; ++i)
        {
            InternalActivity ba = Activities.GetByIndex(i);

            if (ba.NotFinished && ba.Operation.IsNotOmitted)
            {
                ++m_activitiesLeftToSchedule;
            }
        }
    }

    private long GetLatestScheduledFinishDateOfPredecessorOperations(bool a_includeCleanout)
    {
        // If there are predecessor operations check their finish times
        // and return the latest completion time.
        long latestFinish = PTDateTime.MinDateTime.Ticks;

        for (int predecessorI = 0; predecessorI < AlternatePathNode.Predecessors.Count; ++predecessorI)
        {
            long predFinishDate;
            AlternatePath.Node predecessor = AlternatePathNode.Predecessors[predecessorI].Predecessor;

            if (predecessor.Operation.GetScheduledFinishDate(out predFinishDate, a_includeCleanout))
            {
                latestFinish = Math.Max(latestFinish, predFinishDate);
            }
        }

        return latestFinish;
    }

    #region Simulation state variables
    #region Simulation State Variables
    private BoolVector32 m_simInternalOpFlags;

    private const int TryToEnforcePredecessorsKeepSuccessorIdx = 0;
    private const int TryToEnforceKeepSuccessorIdx = 1;
    private const int UnscheduledAnchoredActivitiesHaveBeenSetupForAnchoringIdx = 2;

    /// <summary>
    /// Some preliminary checks were done to see if this is possible given the 1 to 1 pred/suc restriction.
    /// </summary>
    internal bool TryToEnforcePredecessorsKeepSuccessor
    {
        get => m_simInternalOpFlags[TryToEnforcePredecessorsKeepSuccessorIdx];
        set => m_simInternalOpFlags[TryToEnforcePredecessorsKeepSuccessorIdx] = value;
    }

    /// <summary>
    /// Some preliminary checks were done to see if this is possible given the 1 to 1 pred/suc restriction.
    /// </summary>
    internal bool TryToEnforceKeepSuccessor
    {
        get => m_simInternalOpFlags[TryToEnforceKeepSuccessorIdx];
        set => m_simInternalOpFlags[TryToEnforceKeepSuccessorIdx] = value;
    }

    /// <summary>
    /// Whether unscheduled activities that are anchored have been configured to be anchored for a simulation. This can only be used after SimulationInitialization has been performed.
    /// </summary>
    internal bool UnscheduledAnchoredActivitiesHaveBeenSetupForAnchoring
    {
        get => m_simInternalOpFlags[UnscheduledAnchoredActivitiesHaveBeenSetupForAnchoringIdx];
        set => m_simInternalOpFlags[UnscheduledAnchoredActivitiesHaveBeenSetupForAnchoringIdx] = value;
    }

    /// <summary>
    /// The simulation cached value for the length of this operation's sequence buffer
    /// </summary>
    internal long SequenceHeadStartTicks;
    #endregion

    /// <summary>
    /// Performed before any SimulationInitializations on all objects within the system.
    /// </summary>
    internal override void ResetSimulationStateVariables(ScenarioDetail a_sd)
    {
        base.ResetSimulationStateVariables(a_sd);
        m_simInternalOpFlags.Clear();

        for (int activityI = 0; activityI < m_activities.Count; ++activityI)
        {
            InternalActivity activity = m_activities.GetByIndex(activityI);
            activity.ResetSimulationStateVariables(a_sd);
        }

        m_transferQtyProfile = null;
        m_overlapResourceReleaseTimes = new ();
        AutoSplitInfo.ResetSimulationStateVariables(this);
        SequenceHeadStartTicks = TimeSpan.FromDays(SequenceHeadStartDays).Ticks;
    }

    /// <summary>
    /// Performed after RestSimulationStateVariables() on all objects in the system.
    /// </summary>
    internal override void SimulationInitialization(PlantManager a_plantManager, ProductRuleManager a_productRuleManager, ExtensionController a_extensionController, ICalculatedValueCacheManager a_cacheManager)
    {
        base.SimulationInitialization(a_plantManager, a_productRuleManager, a_extensionController, a_cacheManager);

        SimScheduledInitialization();
        SimulationInitializationOfTryToEnforceKeepSuccessors();

        for (int activityI = 0; activityI < Activities.Count; ++activityI)
        {
            InternalActivity ia = Activities.GetByIndex(activityI);
            ia.SimulationInitialization(a_plantManager, a_productRuleManager, a_extensionController, a_cacheManager);
        }

        SimulationInitializationOfActivities(Activities);

        SimulationInitializationOfMaterials(MaterialRequirements);

        ResetLatestConstraint();
    }

    internal override void PostSimulationInitialization()
    {
        base.PostSimulationInitialization();

        for (int activityI = 0; activityI < Activities.Count; ++activityI)
        {
            InternalActivity ia = Activities.GetByIndex(activityI);
            ia.PostSimulationInitialization();
        }
    }

    private void SimulationInitializationOfTryToEnforceKeepSuccessors()
    {
        if (Activities.Count == 1)
        {
            AlternatePath.AssociationCollection predecessors = Predecessors;
            foreach (AlternatePath.Association predAsn in predecessors)
            {
                InternalOperation predOp = (InternalOperation)predAsn.Predecessor.Operation;
                InternalActivity predAct = predOp.Activities.GetByIndex(0);
                //TODO: This is not compatible with helpers that are locked. At this point we don't know the primary resource
                if (!predAct.Finished && !HasDefaultResource && Locked != lockTypes.Locked) // Default Resource and Lock supersede KeepSuccessor.
                {
                    TryToEnforcePredecessorsKeepSuccessor = true;
                    predOp.TryToEnforceKeepSuccessor = true;
                }
            }
            //if (predecessors.Count == 1)
            //{
            //    InternalOperation predOp = (InternalOperation)predecessors[0].Predecessor.Operation;

            //    if (predOp.SuccessorProcessing != PT.SchedulerDefinitions.OperationDefs.successorProcessingEnumeration.NoPreference
            //        && predOp.Activities.Count == 1
            //        && predOp.IsNotOmitted)
            //    {
            //        InternalActivity predAct = predOp.Activities.GetByIndex(0);
            //        if (!predAct.Finished)
            //        {
            //            TryToEnforcePredecessorsKeepSuccessor = true;
            //            predOp.TryToEnforceKeepSuccessor = true;
            //        }
            //    }
            //}
        }
    }

    internal void SimulationInitializationOfActivities(IEnumerable<InternalActivity> a_activitiesList)
    {
        // Product quantity activity splitting
        // Divy up each products output based on the fraction of the required finish quantity each activity makes.
        // The resulting output quantity is stored in each activity.
        // Mathematical errors are added to the first largest activity.

        // Create the productQtys arrays for each activity.
        foreach (InternalActivity act in a_activitiesList)
        {
            act.m_productQtys = new decimal[Products.Count];
            act.m_mrQtys = new Dictionary<BaseId, decimal>(MaterialRequirements.Count);
        }

        // Manually keeps track of the index of the product that is being worked on.
        int prodI = -1;
        foreach (Product p in Products)
        {
            ++prodI;

            // The activity that produces the largest amount of this product.
            InternalActivity largestAct = null;
            // The quantity of this product produced by the activity that produces the largest amount of this product.
            decimal largestQty = 0;
            // This is used to keep track of the total amount of product all the activities are expected to make.
            decimal prodSumOfActivities = 0;

            if (Activities.Count == 1)
            {
                Activities.GetByIndex(0).m_productQtys[prodI] = p.RemainingOutputQty;
                continue;
            }

            //Split activities
            foreach (InternalActivity act in Activities) //Loop through all activities so the p.TotalOutputQty lines up.
            {
                decimal actIQty = ScenarioDetail.ScenarioOptions.RoundQty(act.RemainingQty * (p.TotalOutputQty / RequiredFinishQty));

                //Only process the activities provided. Other activities in the Op may already be scheduled
                if (a_activitiesList.Contains(act))
                {
                    act.m_productQtys[prodI] = actIQty;

                    if (actIQty > largestQty)
                    {
                        largestQty = actIQty;
                        largestAct = act;
                    }
                }

                prodSumOfActivities += actIQty;
            }

            // Make sure the activities make products output quantity.
            decimal diff = p.RemainingOutputQty - prodSumOfActivities;

            if (diff > 0 && largestAct != null)
            {
                // There's some slight difference between what's expected from the activities and how much of the product should be made.
                // Add the difference to the activitiy that produces the most of the product.
                largestAct.m_productQtys[prodI] += diff;
            }
        }

        foreach (MaterialRequirement mr in MaterialRequirements)
        {
            BaseId mrId = mr.Id;
            // The activity that produces the largest amount of this product.
            InternalActivity largestAct = null;
            // The quantity of this product produced by the activity that produces the largest amount of this product.
            decimal largestQty = 0;
            // This is used to keep track of the total amount of product all the activities are expected to make.
            decimal prodSumOfActivities = 0;

            if (Activities.Count == 1)
            {
                Activities.GetByIndex(0).m_mrQtys[mrId] = mr.UnIssuedQty;
                continue;
            }

            //Split activities
            foreach (InternalActivity act in Activities) //Loop through all activities so the p.TotalOutputQty lines up.
            {
                decimal actIQty = ScenarioDetail.ScenarioOptions.RoundQty(act.FractionOfStartQty * mr.TotalRequiredQty);

                //Only process the activities provided. Other activities in the Op may already be scheduled
                if (a_activitiesList.Contains(act))
                {
                    act.m_mrQtys[mrId] = actIQty;

                    if (actIQty > largestQty)
                    {
                        largestQty = actIQty;
                        largestAct = act;
                    }
                }

                prodSumOfActivities += actIQty;
            }

            // Make sure the activities make products output quantity.
            decimal diff = mr.UnIssuedQty - prodSumOfActivities;

            if (diff > 0 && largestAct != null)
            {
                // There's some slight difference between what's expected from the activities and how much of the product should be made.
                // Add the difference to the activitiy that produces the most of the product.
                largestAct.m_mrQtys[mrId] += diff;
            }
        }
    }

    internal void SimulationInitializationOfMaterials(MaterialRequirementsCollection a_mrs)
    {
        foreach (MaterialRequirement mr in a_mrs)
        {
            if (mr.AllowExpiredSupply)
            {
                mr.Item.SaveExpiredMaterial = true;
            }
        }
    }
    #endregion

    internal override void SetupWaitForAnchorDroppedFlag()
    {
        for (int i = 0; i < m_activities.Count; i++)
        {
            m_activities.GetByIndex(i).SetupWaitForAnchorSetFlag();
        }
    }

    internal void UnscheduleNonResourceUsingOperations()
    {
        for (int i = 0; i < Activities.Count; ++i)
        {
            InternalActivity ia = Activities.GetByIndex(i);
            if (ia.PostProcessingStateWithNoResourceUsage)
            {
                ia.Unschedule(true, true);
            }
        }
    }

    private TransferQtyCompletionProfile m_transferQtyProfile;

    internal TransferQtyCompletionProfile TransferQtyProfile => m_transferQtyProfile;

    internal void CalculateContainerCompletions(Resource a_primaryResource, decimal a_requiredFinishQty, CycleAdjustmentProfile a_ccp)
    {
        try
        {
            if (a_ccp == null)
            {
                return;
            }

            m_transferQtyProfile = new TransferQtyCompletionProfile();

            decimal qty = 0;
            decimal contQtySum = 0;

            for (int cycleI = 0; cycleI < a_ccp.Count; ++cycleI)
            {
                qty += a_ccp[cycleI].Qty;

                long containers = (long)Math.Floor(qty / OverlapTransferQty);

                if (containers > 0)
                {
                    for (int containersI = 0; containersI < containers; ++containersI)
                    {
                        contQtySum += OverlapTransferQty;
                        long endDate = a_ccp[cycleI].Date;
                        endDate = AddTransferTimeAndPostProcessingTime(a_primaryResource, endDate);
                        TransferQtyCompletion tqc = new (endDate, contQtySum);
                        m_transferQtyProfile.Add(tqc);
                    }
                }

                qty = qty - containers * OverlapTransferQty;
            }

            if (qty > 0)
            {
                long endDate = a_ccp[^1].Date;
                endDate = AddTransferTimeAndPostProcessingTime(a_primaryResource, endDate);
                TransferQtyCompletion tqc = new (endDate, a_requiredFinishQty);
                m_transferQtyProfile.Add(tqc);
            }
        }
        catch (Exception e)
        {
            Overlap.Throw_OverlapDebugError(e);
            m_transferQtyProfile = null;
        }
    }

    internal ProductSupplyProfile CalculateContainerCompletionIntoQtyProfile(ScenarioDetail a_sd, Resource a_primaryResource, Product a_product, int a_productIdx, InternalActivity a_reason, CycleAdjustmentProfile a_ccp, SchedulableInfo a_si)
    {
        try
        {
            // *MaterialOverlap* take post processing time into consideration.
            ProductSupplyProfile qtyProfile = new (a_product, a_reason, a_product.Inventory, a_si);

            if (a_ccp == null || a_ccp.Count == 0)
            {
                return qtyProfile;
            }

            decimal qty = 0;

            ProductionInfo resourceProductionInfo = a_reason.GetResourceProductionInfo(a_primaryResource);
            decimal transferQty = resourceProductionInfo.TransferQty;

            decimal qtyOfProductPerOPQty = a_product.TotalOutputQty / a_reason.Operation.RequiredFinishQty;
            CycleAdjustment lastCC = a_ccp[^1];
            decimal totalQtyAdded = 0;

            //The final transfer is not equal, it is only the remainder
            decimal actQty = a_reason.m_productQtys[a_productIdx];
            bool lastCycleIsRemainder = a_ccp.CalcTotalQty() * qtyOfProductPerOPQty != actQty;

            bool useTransfer = transferQty > 0m;

            for (int cycleI = 0; cycleI < a_ccp.Count; ++cycleI)
            {
                CycleAdjustment ccI = a_ccp[cycleI];
                if (cycleI == a_ccp.Count - 1 && lastCycleIsRemainder)
                {
                    //Last cycle, use the remainder to avoid rounding issues
                    qty = actQty - totalQtyAdded;
                }
                else
                {
                    qty += qtyOfProductPerOPQty * ccI.Qty;
                }

                decimal totalQtyOfContainers;
                if (useTransfer)
                {
                    //Only transfer at full transfers
                    decimal containers = Math.Floor(qty / transferQty);
                    totalQtyOfContainers = containers * transferQty;
                }
                else
                {
                    totalQtyOfContainers = a_sd.ScenarioOptions.RoundQty(qty);
                }

                if (totalQtyOfContainers > 0)
                {
                    CycleAdjustment cc = a_ccp[cycleI];

                    qtyProfile.AddToEnd(cc.Date, a_reason, a_product, totalQtyOfContainers, a_primaryResource);
                    totalQtyAdded += totalQtyOfContainers;
                    qty -= totalQtyOfContainers;
                }
            }

            if (qty > 0)
            {
                qtyProfile.AddToEndOrIncrementEnd(lastCC.Date, a_reason, a_product, qty, a_primaryResource);
                totalQtyAdded += qty;
            }

            decimal diff = actQty - totalQtyAdded;
            if (diff > 0)
            {
                qtyProfile.AddToEndOrIncrementEnd(lastCC.Date, a_reason, a_product, diff, a_primaryResource);
            }

            return qtyProfile;
        }
        catch (Exception e)
        {
            Overlap.Throw_OverlapDebugError(e);
            throw;
        }
    }

    internal void CalculateMaterialConsumptionRangeIntoQtyProfile(MaterialDemandProfile a_demandProfile, Resource a_primaryResource, CapacityUsageProfile a_ocpProfile, long a_defaultAdjustmentTime)
    {
        decimal qtyToConsume = a_demandProfile.RemainingQty;
        decimal transferQty = a_demandProfile.Item.TransferQty;
        decimal qtyTransferredSoFar = 0m;

        long ticksPerQty = (long)Math.Floor(a_ocpProfile.CapacityFound / qtyToConsume);
        long durationPerTransfer = (long)Math.Floor(ticksPerQty * transferQty);

        if (a_ocpProfile.Count == 0)
        {
            //This segment didn't have any capacity, so make all qty at the default time (end of segment)
            a_demandProfile.AddToEnd(new QtyDemandNode(a_defaultAdjustmentTime, a_demandProfile.Activity, a_demandProfile.MR, qtyToConsume));
        }
        else if (transferQty >= qtyToConsume 
                 || durationPerTransfer >= a_ocpProfile.CapacityFound 
                 || durationPerTransfer == 0) //No transfer qty
        {
            //Only a single transfer is needed, so this is the same as available at the end of the segment
            a_demandProfile.AddToEnd(new QtyDemandNode(a_ocpProfile.CapacityEndTicks, a_demandProfile.Activity, a_demandProfile.MR, qtyToConsume));
        }
        else
        {
            long remainingTransferTime = durationPerTransfer;
            decimal remainingToConsume = 0m;
            foreach (OperationCapacity capacity in a_ocpProfile)
            {
                long capacityRemainingTicks = capacity.TotalCapacityTicks;
                long lastTransferTicks = capacity.StartTicks;
                while (remainingTransferTime > 0)
                {
                    
                    if (remainingTransferTime > capacityRemainingTicks)
                    {
                        //This capacity is not enough for a transfer
                        remainingTransferTime -= capacityRemainingTicks;
                        continue;
                    }

                    if (remainingTransferTime == capacityRemainingTicks)
                    {
                        //This capacity is exactly enough for a transfer
                        remainingTransferTime = durationPerTransfer;
                        a_demandProfile.AddToEnd(new QtyDemandNode(capacity.EndTicks, a_demandProfile.Activity, a_demandProfile.MR, transferQty));
                        qtyTransferredSoFar += transferQty;
                        remainingToConsume = qtyToConsume - qtyTransferredSoFar;

                        break;
                    }

                    //Advance the calendar transfer date forward by the remaining transfer time
                    lastTransferTicks += remainingTransferTime;
                    a_demandProfile.AddToEnd(new QtyDemandNode(lastTransferTicks, a_demandProfile.Activity, a_demandProfile.MR, transferQty));

                    //Update tracking variables
                    capacityRemainingTicks -= remainingTransferTime;
                    remainingTransferTime = durationPerTransfer;
                    qtyTransferredSoFar += transferQty;
                    remainingToConsume = qtyToConsume - qtyTransferredSoFar;
                }
            }
            
            //Check for a remainder
            if (remainingToConsume > 0)
            {
                //A remainder transfer exists, put it at the end of the segment even through it doesn't align exactly with the transfer qty.
                a_demandProfile.AddToEnd(new QtyDemandNode(a_ocpProfile.CapacityEndTicks, a_demandProfile.Activity, a_demandProfile.MR, remainingToConsume));
            }
        }
    }

    internal ProductSupplyProfile CalculateMaterialProductionRangeIntoQtyProfile(Resource a_primaryResource, Product a_product, InternalActivity a_reason, CapacityUsageProfile a_ocpProfile, long a_defaultAdjustmentTime, SchedulableInfo a_si)
    {
        // *MaterialOverlap* take post processing time into consideration.
        ProductSupplyProfile qtyProfile = new (a_product, a_reason, a_product.Inventory, a_si);

        decimal qtyToMake = a_reason.m_productQtys[0];
        decimal transferQty = a_reason.GetResourceProductionInfo(a_primaryResource).TransferQty;
        if (transferQty == 0m)
        {
            //Without a transfer qty, this will be available in one transfer at the end. Use a default 1/10 transfer
            transferQty = ScenarioDetail.ScenarioOptions.RoundQtyUp(qtyToMake / 10);
        }

        decimal qtyTransferedSoFar = 0m;

        long ticksPerQty = (long)Math.Floor(a_ocpProfile.CapacityFound / qtyToMake);
        long durationPerTransfer = (long)Math.Floor(ticksPerQty * transferQty);

        if (a_ocpProfile.Count == 0)
        {
            //This segment didn't have any capacity, so make all qty at the default time (end of segment)
            qtyProfile.AddToEndOrIncrementEnd(a_defaultAdjustmentTime, a_reason, a_product, qtyToMake, a_primaryResource);
        }
        else if (transferQty >= qtyToMake
                 || durationPerTransfer >= a_ocpProfile.CapacityFound
                 || durationPerTransfer == 0) //No transfer qty
        {
            //Only a single transfer is needed, so this is the same as available at the end of the segment
            qtyProfile.AddToEndOrIncrementEnd(a_ocpProfile.CapacityEndTicks, a_reason, a_product, qtyToMake, a_primaryResource);
        }
        else
        {
            long remainingTransferTime = durationPerTransfer;
            decimal remainingToMake = 0m;
            long lastTransferTicks = a_ocpProfile[0].StartTicks;
            foreach (OperationCapacity capacity in a_ocpProfile)
            {
                long capacityRemainingTicks = capacity.TotalCapacityTicks;
                while (remainingTransferTime > 0)
                {

                    if (remainingTransferTime > capacityRemainingTicks)
                    {
                        //This capacity is not enough for a transfer
                        remainingTransferTime -= capacityRemainingTicks;
                        lastTransferTicks += capacityRemainingTicks; //Move this date along so this capacity doesn't get lost
                        break;
                    }

                    if (remainingTransferTime == capacityRemainingTicks)
                    {
                        //This capacity is exactly enough for a transfer
                        remainingTransferTime = durationPerTransfer;
                        lastTransferTicks = capacity.EndTicks;
                        qtyProfile.AddToEndOrIncrementEnd(lastTransferTicks, a_reason, a_product, transferQty, a_primaryResource);
                        qtyTransferedSoFar += transferQty;
                        remainingToMake = qtyToMake - qtyTransferedSoFar;

                        break;
                    }

                    //Advance the calendar transfer date forward by the remaining transfer time
                    lastTransferTicks += remainingTransferTime;
                    qtyProfile.AddToEndOrIncrementEnd(lastTransferTicks, a_reason, a_product, transferQty, a_primaryResource);

                    //Update tracking variables
                    capacityRemainingTicks -= remainingTransferTime;
                    remainingTransferTime = durationPerTransfer;
                    qtyTransferedSoFar += transferQty;
                    remainingToMake = qtyToMake - qtyTransferedSoFar;
                }
            }

            //Check for a remainder
            if (remainingToMake > 0)
            {
                //A remainder transfer exists, put it at the end of the segment even through it doesn't align exactly with the transfer qty.
                qtyProfile.AddToEndOrIncrementEnd(a_ocpProfile.CapacityEndTicks, a_reason, a_product, remainingToMake, a_primaryResource);
            }
        }

        return qtyProfile;
    }


    /// <summary>
    /// This is a helper for calculating container completion of products. In this case association transfer time isn't relevant.
    /// </summary>
    private long AddPostProcessingTime(InternalResource a_primaryResource, long a_endDate, Product a_product)
    {
        return a_endDate + a_primaryResource.TransferSpanTicks + ProductionInfo.MaterialPostProcessingSpanTicks + a_product.MaterialPostProcessingTicks;
    }

    private long AddTransferTimeAndPostProcessingTime(InternalResource a_primaryResource, long a_endDate)
    {
        long transferSpanTicks;
        if (AlternatePathNode.Successors.Count > 0)
        {
            AlternatePath.Association a = AlternatePathNode.Successors[0];
            transferSpanTicks = a.TransferSpanTicks;
        }
        else
        {
            transferSpanTicks = 0;
        }

        return a_endDate + a_primaryResource.TransferSpanTicks + ProductionInfo.MaterialPostProcessingSpanTicks + transferSpanTicks;
    }

    private Dictionary<Resource, long> m_overlapResourceReleaseTimes = new();

    internal Dictionary<Resource, long> OverlapResourceReleaseTimes
    {
        get => m_overlapResourceReleaseTimes;

        set => m_overlapResourceReleaseTimes = value;
    }

    public long GetOverlapReleaseTicksByResource(Resource a_res)
    {
        if (m_overlapResourceReleaseTimes.TryGetValue(a_res, out long releaseTicks))
        {
            return releaseTicks;
        }

        return long.MaxValue;
    }


    //		internal bool CanSatisfySuccessorMaterialUsages(decimal usageQtyPerCycle, long[] cycleStartTimes)
    //		{
    //			int containerI=0;
    //
    //			for(int cycleI=0; cycleI<cycleStartTimes.Length; ++cycleI)
    //			{
    //				decimal neededQty=usageQtyPerCycle*(cycleI+1);
    //				long needDate=cycleStartTimes[cycleI];
    //
    //				while(true)
    //				{
    //					if(containerI>containerCompletionTimes.Length-1)
    //					{
    //						return true;
    //					}
    //
    //					if(containerCompletionTimes[containerI]<=needDate
    //						&&containerCompletionSums[containerI]>=neededQty)
    //					{
    //						break;
    //					}
    //					else if(containerCompletionTimes[containerI]>needDate)
    //					{
    //						return false;
    //					}
    //
    //					++containerI;
    //				}
    //			}
    //
    //			return true;
    //		}

    /// <summary>
    /// This refers to eligibility for transferQty operation overlap.
    /// Currently overlap only works when there is one predecessor and one successor and each has one activity.
    /// </summary>
    internal bool EligibleForTransferQtyOverlap()
    {
        if (SingularSuccessor())
        {
            AlternatePath.Association sucAsn = Successors[0];
            if (sucAsn.OverlapType == InternalOperationDefs.overlapTypes.TransferQty)
            {
                InternalOperation sucOp = sucAsn.Successor.Operation;
                return sucOp.SingularPredecessor();
            }
        }

        return false;
    }

    /// <summary>
    /// Whether this operation is eligible for AtFirstTransfer overlap.
    /// </summary>
    /// <returns></returns>
    internal bool EligibleForAtFirstTransferOverlap()
    {
        if (SingularSuccessor())
        {
            AlternatePath.Association sucAsn = Successors[0];
            if (sucAsn.OverlapType == InternalOperationDefs.overlapTypes.AtFirstTransfer)
            {
                InternalOperation sucOp = sucAsn.Successor.Operation;
                return sucOp.SingularPredecessor();
            }
        }

        return false;
    }

    internal bool EligibleForAtFirstTransferOverlapOrTransferQtyOverlap()
    {
        if (SingularSuccessor())
        {
            AlternatePath.Association sucAsn = Successors[0];
            if (sucAsn.OverlapType is InternalOperationDefs.overlapTypes.AtFirstTransfer or InternalOperationDefs.overlapTypes.TransferQty)
            {
                InternalOperation sucOp = sucAsn.Successor.Operation;
                return sucOp.SingularPredecessor();
            }
        }

        return false;
    }

    /// <summary>
    /// Whether there is one activity and one successor.
    /// </summary>
    private bool SingularSuccessor()
    {
        return (AutoSplitInfo.OperationIsAutoSplit || Activities.Count == 1) && Successors.Count == 1;
    }

    /// <summary>
    /// Whether there is one activity and one predecessor.
    /// </summary>
    private bool SingularPredecessor()
    {
        return (AutoSplitInfo.OperationIsAutoSplit || Activities.Count == 1) && Predecessors.Count == 1;
    }

    /// <summary>
    /// Assuming the operation is scheduled or finished, among all activities the latest completion of Transfer Time is returned. This is based on
    /// completion of processing or reported finish date  + material post processing + primary resource transfer time.
    /// There are conditions in which this will return PtDateTime.MIN_DATE.
    /// </summary>
    public long CalcEndOfResourceTransferTimeTicks()
    {
        if (!Scheduled)
        {
            return PTDateTime.MaxDateTime.Ticks;
        }

        // Obtain the predecessor ready date with Resource Transfer Time taken into consideration.
        long maxScheduledFinishDateTicksWithResourceTransferTime = PTDateTime.MinDateTime.Ticks;

        for (int iaI = 0; iaI < Activities.Count; ++iaI)
        {
            InternalActivity ia = Activities.GetByIndex(iaI);
            long endOfTransTime = 0;
            if (ia.Finished)
            {
                endOfTransTime = ia.ReportedFinishDateTicks;
            }
            else
            {
                endOfTransTime = ia.Batch.PostProcessingEndTicks;
            }

            endOfTransTime += ia.ResourceTransferSpanTicks;
            maxScheduledFinishDateTicksWithResourceTransferTime = Math.Max(endOfTransTime, maxScheduledFinishDateTicksWithResourceTransferTime);
        }

        return maxScheduledFinishDateTicksWithResourceTransferTime;
    }

    public bool AllSucOpnsFinishedOrOmitted()
    {
        bool allOmittedFinished = true;
        for (int i = 0; i < AlternatePathNode.Successors.Count; ++i)
        {
            AlternatePath.Node curSuc = AlternatePathNode.Successors[i].Successor;
            if (!curSuc.Operation.IsFinishedOrOmitted)
            {
                allOmittedFinished = false;
                ;
            }
        }

        return allOmittedFinished;
    }

    /// <summary>
    /// Assuming the operation is scheduled, among all activities the latest completion of Transfer Time is returned. This is based on
    /// completion of processing + material post processing + primary resource transfer time.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public DateTime EndOfResourceTransferTimeDate => new (CalcEndOfResourceTransferTimeTicks());

    /// <summary>
    /// Tells whether one of the activities of this operation is being moved.
    /// </summary>
    internal bool BeingMoved
    {
        get
        {
            for (int actI = 0; actI < m_activities.Count; ++actI)
            {
                InternalActivity act = m_activities.GetByIndex(actI);
                if (act.BeingMoved)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Among all the blocks that are scheduled determine whether the activity is scheduled in a cell. If 1 block's resource is in a cell then that cell is returned.
    /// If another block is scheduled in a different cell then null is returned since this is just an error on the part of the user in defining their cells.
    /// </summary>
    internal override Cell Cell
    {
        get
        {
            Cell cell = null;
            for (int actI = 0; actI < m_activities.Count; ++actI)
            {
                InternalActivity act = m_activities.GetByIndex(actI);
                Cell tempCell = act.Cell;
                if (tempCell != null)
                {
                    if (cell != null && cell != tempCell)
                    {
                        return null;
                    }

                    cell = tempCell;
                }
            }

            return cell;
        }
    }

    internal Cell GetPredecessorCell()
    {
        Cell cell = null;

        for (int predI = 0; predI < AlternatePathNode.Predecessors.Count; ++predI)
        {
            Cell tempCell = AlternatePathNode.Predecessors[predI].Predecessor.Operation.Cell;
            if (tempCell != null)
            {
                if (cell != null && tempCell != cell)
                {
                    return null;
                }

                cell = tempCell;
            }
        }

        return cell;
    }

    internal bool IsCompatibilityCodeUsable()
    {
        return UseCompatibilityCode && m_compatibilityCode.Length > 0;
    }

    /// <summary>
    /// This returns the SimStage of the primary resource requirements first eligible resource across plants.
    /// </summary>
    /// <param name="a_defaultStage">
    /// If a stage can't be found a_defaultStage is returned; this may mean there aren't eligible resources able to process the operation or it's finished or omitted (in which
    /// case eligibility isn't calculated).
    /// </param>
    /// <returns>
    /// The earliest stage number. If a stage can't be found a_defaultStage is returned; this may mean there aren't eligible resources able to process the operation or it's finished or omitted (in
    /// which case eligibility isn't calculated).
    /// </returns>
    internal int GetStageToBeScheduledIn(int a_defaultStage)
    {
        InternalResource ir = ResourceRequirements.GetFirstEligiblePrimaryResource();

        if (ir != null)
        {
            return ir.SimStage;
        }

        return a_defaultStage;
    }
    #endregion Simulation

    #region Eligibility
    internal void ClearEffectiveResourceEligibilitySet()
    {
        m_resourceRequirements.ClearEffectiveResourceEligibilitySet();
    }

    /// <summary>
    /// Part of Step 2 of eligibility. Each operation determines where it can be made; plants capable of satisfying all the resource requirements.
    /// </summary>
    internal void CreateEffectiveResourceEligibilitySet(ProductRuleManager a_productRuleManager)
    {
        m_resourceRequirements.CreateEffectiveResourceEligibilitySet(a_productRuleManager);
    }
    #endregion

    #region Anchoring
    /// <summary>
    /// Anchored Activities cannot be moved (in time or resource) during Optimizations, Moves, or Expedites.
    /// </summary>
    [Display(DisplayAttribute.displayOptions.ReadOnly)]
    public anchoredTypes Anchored
    {
        get
        {
            int anchoredCount = 0;
            for (int i = 0; i < Activities.Count; i++)
            {
                if (Activities.GetByIndex(i).Anchored)
                {
                    anchoredCount++;
                }
            }

            if (anchoredCount == 0)
            {
                return anchoredTypes.Free;
            }

            if (anchoredCount == Activities.Count)
            {
                return anchoredTypes.Anchored;
            }

            return anchoredTypes.SomeActivitiesAnchored;
        }
    }

    /// <summary>
    /// Set Anchor on an activity and perform related operation processing.
    /// </summary>
    internal void SetAnchor(InternalActivity a_act, bool a_anchor, ScenarioOptions a_scenarioOptions)
    {
        a_act.SetAnchor(a_anchor);

        if (a_scenarioOptions.CommitOnAnchor)
        {
            SetCommitDates();
        }
    }

    /// <summary>
    /// Set the Anchor flag on all the activities and perform related operation processing.
    /// </summary>
    internal void SetAnchor(bool a_anchor, ScenarioOptions a_scenarioOptions)
    {
        for (int i = 0; i < Activities.Count; i++)
        {
            Activities.GetByIndex(i).SetAnchor(a_anchor);
        }

        if (a_scenarioOptions != null && a_scenarioOptions.CommitOnAnchor)
        {
            SetCommitDates();
        }
    }

    #region Reanchoring after a simulation
    /// <summary>
    /// Used during simulations to setup the activity for reanchoring at simulation end. This function only has an affect if the activity was anchored prior to being
    /// called. At simulation completion call Reanchor() to complete this process.
    /// </summary>
    internal void ReanchorSetup()
    {
        for (int i = 0; i < Activities.Count; i++)
        {
            InternalActivity ia = Activities.GetByIndex(i);
            if (!ia.Finished)
            {
                ia.ReanchorSetup();
            }
        }
    }

    /// <summary>
    /// Reanchors activities after simulation. This function call only affects activities that were anchored prior to simulation start. ReanchorSetup()
    /// must have been called on this activity before the start of the simulation.
    /// </summary>
    internal void Reanchor(ScenarioOptions a_scenarioOptions)
    {
        for (int i = 0; i < Activities.Count; i++)
        {
            Activities.GetByIndex(i).Reanchor();
        }

        if (a_scenarioOptions.CommitOnAnchor)
        {
            SetCommitDates();
        }
    }
    #endregion
    #endregion

    internal void AbsorbReportedValues(InternalOperation a_absorbOp)
    {
        Activities.AbsorbReportedValues(a_absorbOp.Activities);
    }

    /// <summary>
    /// Scan the activities for the latest activity ScheduledFinishDateTicks.
    /// The default value is PtDateTime.MIN_DATE.
    /// </summary>
    /// <param name="readyTime">The latest activity ready time is returned in this field.</param>
    /// <returns>false if any of the activities are both not finished and not scheduled.</returns>
    internal override bool GetScheduledFinishDate(out long o_scheduledFinishDate, bool a_includeCleanout)
    {
        if (IsOmitted)
        {
            if (AlternatePathNode == null)
            {
                o_scheduledFinishDate = PTDateTime.MinDateTime.Ticks;
                return false;
            }

            if (AlternatePathNode.Predecessors.Count == 0)
            {
                o_scheduledFinishDate = PTDateTime.MinDateTime.Ticks;
                return true;
            }

            o_scheduledFinishDate = GetLatestScheduledFinishDateOfPredecessorOperations(a_includeCleanout);
            return true;
        }

        long activityReadyDateTicks;
        o_scheduledFinishDate = PTDateTime.MinDateTime.Ticks;

        for (int activityI = 0; activityI < m_activities.Count; ++activityI)
        {
            InternalActivity activity = m_activities.GetByIndex(activityI);

            if (activity.ProductionStatus == InternalActivityDefs.productionStatuses.Finished)
            {
                if (a_includeCleanout)
                {
                    activityReadyDateTicks = activity.ReportedFinishDateTicks;
                }
                else
                {
                    if (activity.ReportedEndOfStorageSet)
                    {
                        activityReadyDateTicks = activity.ReportedEndOfStorageTicks;
                    }
                    else if (activity.ReportedEndOfPostProcessingSet)
                    {
                        activityReadyDateTicks = activity.ReportedEndOfPostProcessingTicks;
                    }
                    else if (activity.ReportedEndOfProcessingSet)
                    {
                        activityReadyDateTicks = activity.ReportedEndOfRunTicks;
                    }
                    else
                    {
                        //Just use reported finish
                        activityReadyDateTicks = activity.ReportedFinishDateTicks;
                    }
                }
            }
            else if (!activity.Scheduled)
            {
                o_scheduledFinishDate = PTDateTime.MinDateTime.Ticks;
                return false;
            }
            else
            {
                if (a_includeCleanout)
                {
                    activityReadyDateTicks = activity.ScheduledFinishDateTicks;
                }
                else
                {
                    activityReadyDateTicks = activity.Batch.EndOfStorageTicks;
                }
            }

            if (activityReadyDateTicks > o_scheduledFinishDate)
            {
                o_scheduledFinishDate = activityReadyDateTicks;
            }
        }

        return true;
    }

    internal long GetEarliestScheduledActivityStartDate(out InternalActivity o_activity)
    {
        o_activity = null;
        long time = PTDateTime.MaxDateTimeTicks;

        for (int activityI = 0; activityI < Activities.Count; activityI++)
        {
            InternalActivity activity = Activities.GetByIndex(activityI);

            if (activity.Scheduled)
            {
                long scheduledStartTicks = activity.GetScheduledStartTicks();
                if (scheduledStartTicks < time)
                {
                    time = scheduledStartTicks;
                    o_activity = activity;
                }
            }
        }

        return time;
    }

    #region JIT
    /// <summary>
    /// The JIT start date of successor operations must have been calculated before invoking this function on this operation.
    /// Call this version if there are no primary resources able to manufacture this operation, which is unusual and would result in an unschedulable job.
    /// </summary>
    internal override void JITCalculateStartDates(long a_clockDate, long a_shippingBuffer)
    {
        if (NotPartOfCurrentRouting())
        {
            m_needInfo.OmitFromScheduling();
        }

        if (Successors.Count == 0)
        {
            //This is the final OP
            m_needInfo.OperationNeedDate = ManufacturingOrder.NeedDate.Ticks;
            m_needInfo.OperationDbrNeedDate = m_needInfo.OperationNeedDate - a_shippingBuffer;
        }
        else
        {
            // When the operation deliverable (material, transfer, etc) needs to complete to make it to the successor on time.
            //Initialized the NeedDate
            m_needInfo.OperationNeedDate = FindEarliestNeededSuccessorJitStart(false);
            m_needInfo.OperationDbrNeedDate = FindEarliestNeededSuccessorJitStart(true);
        }

        if (m_needInfo.OperationDbrNeedDate > PTDateTime.MinDateTimeTicks)
        {
            CalculateJITs(a_clockDate);
        }
        else
        {
            //Don't calculate JIT, the dates are not valid
        }
    }

    internal static readonly long JitNotCalculableTicks = PTDateTime.MinDateTimeTicks;

    /// <summary>
    /// Calculate and set the JIT dates of the activities and the operation. The
    /// JITs of the operation are the smallest JITs of the activities.
    /// </summary>
    /// <param name="a_clockDate"></param>
    /// <param name="a_successorNeedInfo"></param>
    /// <returns></returns>
    private void CalculateJITs(long a_clockDate)
    {
        m_needInfo.Reset(this); //Reset activity dates

        PlantResourceEligibilitySet pres = ResourceRequirements.PrimaryResourceRequirement.EligibleResources;
        if (pres.Count == 0)
        {
            return;
        }

        long minJit = long.MaxValue;
        for (int actI = 0; actI < Activities.Count; ++actI)
        {
            InternalActivity act = Activities.GetByIndex(actI);
            if (act.Finished)
            {
                continue;
            }

            //Get the eligible resources that this operation can schedule on
            List<Resource> predResList = pres.GetResources();
            Resource defaultOrLockedResource = null;
            if (act.Operation.ResourceRequirements.PrimaryResourceRequirement.DefaultResource is Resource defaultResource
                && !act.Operation.ResourceRequirements.PrimaryResourceRequirement.DefaultResource_UseJITLimitTicks)
            {
                CalcActJIT(a_clockDate, act, defaultResource);
                defaultOrLockedResource = defaultResource;
            }
            else if(Locked == lockTypes.Locked)
            {
                Resource lockedResource = act.PrimaryResourceRequirementLock();
                CalcActJIT(a_clockDate, act, lockedResource);
                defaultOrLockedResource = lockedResource;
            }
            else
            {
                // Note if the job is already scheduled, the individual resources aren't actually used.
                // In this case, the resource the activity is scheduled on is used to calculate the JIT.
                foreach (Resource res in predResList)
                {
                    CalcActJIT(a_clockDate, act, res);
                }
            }

            //If act is constrained to schedule to a resource, initialize buffer info for other resource as well to prevent errors
            if (defaultOrLockedResource != null)
            {
                foreach (Resource resource in predResList)
                {
                    if (resource != defaultOrLockedResource)
                    {
                        // Init other eligible resources to 0, in case the activity is released on any of those resources (for example it's not being optimized)
                        act.BufferInfo.Add(resource.Id, ActivityResourceBufferInfo.EmptyBufferInfo);
                    }
                }
            }

            // The JIT for the operation is the minimum JIT among all the activities and eligible resources.
            //act.JITStartTicksAndNeed_Setter(actJit, a_successorNeedInfo.OperationNeed);
            act.BufferInfo.FinalizeJitCalculations();
        }

        //Cache the earliest values for easier access
        m_needInfo.FinalizeJitCalculations(this);

        if (!m_needInfo.EarliestJitBufferInfo.DbrJitDateCalculated)
        {
            throw new PTValidationException("4133", new object[] { ManufacturingOrder.Job.ExternalId, ManufacturingOrder.ExternalId, ExternalId });
        }
        else if (minJit <= PTDateTime.MinValue.ToDateTime().Ticks)
        {

        }
    }

    private long CalcBeginTransferByTicks(long a_clockDate, long a_longestResTT, long a_longestResTransitHrs, InternalActivity a_act, Resource a_resource, AlternatePath.Association a_sucAsn, bool a_dbrCalculation)
    {
        long needDateTicks = a_dbrCalculation ? m_needInfo.OperationDbrNeedDate : m_needInfo.OperationNeedDate;
        long beginTransferByTicks = needDateTicks - Math.Max(a_longestResTT, a_longestResTransitHrs) - a_act.GetResourceProductionInfo(a_resource).MaterialPostProcessingSpanTicks; //Account for Resource Connector Transit hours.
        if (a_sucAsn?.TransferSpanTicks > 0)
        {
            long startTransferBy = beginTransferByTicks;
            if (a_sucAsn.TransferDuringPredeccessorOnlineTime)
            {
                RequiredCapacity rc = new (RequiredSpanPlusClean.s_notInit, RequiredSpanPlusSetup.s_notInit, new RequiredSpan(a_sucAsn.TransferSpanTicks, false), RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                long successorJit = a_dbrCalculation ? a_sucAsn.Successor.Operation.DbrJitStartDateTicks : a_sucAsn.Successor.Operation.JitStartDateTicks;
                startTransferBy = a_act.Operation.JITBackcalculate(a_clockDate, a_resource, a_act, rc, successorJit);
            }
            else
            {
                startTransferBy -= a_sucAsn.TransferSpanTicks;
            }

            return startTransferBy;
        }

        return beginTransferByTicks;
    }

    /// <summary>
    /// Calculate the JIT of the activity.
    /// </summary>
    /// <param name="a_clockDate"></param>
    /// <param name="a_act"></param>
    /// <param name="a_res">If specified, this will be used to calculate the JIT. If null, this function will find an eligible primary resource.</param>
    private void CalcActJIT(long a_clockDate, InternalActivity a_act, Resource a_res)
    {
        //JIT does not need to use Clean to determine start date
        RequiredCapacityPlus noCleanCapacity = new RequiredCapacityPlus(new RequiredSpanPlusClean(0, false, 0), RequiredSpanPlusSetup.s_notInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, new RequiredSpanPlusClean(0, false, 0), 0, 0m);
        RequiredCapacity rc = a_res.CalculateTotalRequiredCapacity(a_clockDate, a_act, LeftNeighborSequenceValues.NullValues, false, -1, a_act.ScenarioDetail.ExtensionController, noCleanCapacity);

        // --------------------------------------------------------
        // FIRST ACTUAL JIT
        // --------------------------------------------------------
        ActivityResourceBufferInfo resourceBufferInfo = new ();

        resourceBufferInfo.JitTransferStartDate = m_needInfo.OperationNeedDate; //Init without any transfer

        //We don't calculate Release here because it depends on the optimize options used. That will be updated at the start of optimize
        long needDateTicks = CalcOverlapAndTransfer(a_clockDate, a_act, a_res, rc, false);

        if (needDateTicks != long.MaxValue)
        {
            // If overlap is possible, it's possible to start the predecessor later.
            //The predecessor may need to start earlier due to transfer delay
            resourceBufferInfo.JitTransferStartDate = needDateTicks;
        }

        //Now calculate DBR JIT start date
        resourceBufferInfo.JitStartDate = JITBackcalculate(a_clockDate, a_res, a_act, rc, resourceBufferInfo.JitTransferStartDate);

        // --------------------------------------------------------
        // NOW DBR JIT
        // --------------------------------------------------------
        resourceBufferInfo.BufferEndDate = m_needInfo.OperationDbrNeedDate; //Init without any transfer

        //We don't calculate Release here because it depends on the optimize options used. That will be updated at the start of optimize
        long bufferNeedDateTicks = CalcOverlapAndTransfer(a_clockDate, a_act, a_res, rc, true);

        if (bufferNeedDateTicks != long.MaxValue)
        {
            // If overlap is possible, it's possible to start the predecessor later.
            //The predecessor may need to start earlier due to transfer delay
            resourceBufferInfo.BufferEndDate = bufferNeedDateTicks;
        }

        //Now calculate DBR JIT start date
        long jitStartBuffer = a_res.BufferSpanTicks + resourceBufferInfo.OperationDynamicBuffer;
        jitStartBuffer = Math.Max(a_act.Operation.StandardOperationBufferTicks, jitStartBuffer);

        Resource.FindStartFromEndResult bufferStartResult = a_res.GetStartOfBufferFromEndDate(a_clockDate, resourceBufferInfo.BufferEndDate, jitStartBuffer);
        resourceBufferInfo.BufferNeedDate = bufferStartResult.StartTicks;
        resourceBufferInfo.DbrJitStartDate = JITBackcalculate(a_clockDate, a_res, a_act, rc, resourceBufferInfo.BufferNeedDate);

        a_act.BufferInfo.Add(a_res.Id, resourceBufferInfo);
    }

    private long CalcOverlapAndTransfer(long a_clockDate, InternalActivity a_act, Resource a_res, RequiredCapacity a_rc, bool a_dbrCalculation)
    {
        long minOverlapJit = long.MaxValue;
        long minTransferJit = long.MaxValue;


        if (Successors.Count > 0)
        {
            // If there are successors, it may be possible to start this operation later than the time calculated above(based on total capacity).
            // It can start later if only a fraction of total required capacity needs to elapse before the successor starts.
            for (int sucI = 0; sucI < Successors.Count; ++sucI)
            {
                //TODO: Account for when overlap is not possible. For example when the two ops have the same default resource, or share a single eligible resource

                AlternatePath.Association asn = Successors[sucI];

                //Start with calculating the transfers
                long longestConnectedResourceTransfer = GetLongestConnectedResourceTransfer(out Resource _, ScenarioDetail.ResourceConnectorManager, a_res);

                //Calculate Transfer Time
                long beginTransferByTicks = CalcBeginTransferByTicks(a_clockDate, a_res.TransferSpanTicks, longestConnectedResourceTransfer, a_act, a_res, asn, a_dbrCalculation);

                switch (asn.OverlapType)
                {
                    case InternalOperationDefs.overlapTypes.NoOverlap:
                        break;
                    case InternalOperationDefs.overlapTypes.TransferQty:
                        if (EligibleForAtFirstTransferOverlapOrTransferQtyOverlap())
                        {
                            // Back qtyPerCycle*time percycle + setup.
                            long jitTmp = JITBackcalculateByQty(a_clockDate, a_act, a_res, a_rc, OverlapTransferQty, beginTransferByTicks);
                            minOverlapJit = Math.Min(jitTmp, minOverlapJit);
                        }
                        break;
                    case InternalOperationDefs.overlapTypes.TransferSpan:
                        RequiredCapacity rcTemp = new(a_rc.CleanBeforeSpan, a_rc.SetupSpan, new RequiredSpan(asn.OverlapTransferTicks, false), RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                        long jitTmp1 = JITBackcalculate(a_clockDate, a_res, a_act, rcTemp, beginTransferByTicks);
                        minOverlapJit = Math.Min(minOverlapJit, jitTmp1);
                        break;
                    case InternalOperationDefs.overlapTypes.AtFirstTransfer:
                    {
                        if (EligibleForAtFirstTransferOverlapOrTransferQtyOverlap())
                        {
                            // Back calculate the first transfer and setup time.
                            long ticksToCompleteFirstUsageQty = a_act.CalcSpanToMakeQty(a_res, asn.UsageQtyPerCycle);
                            RequiredCapacity rcTemp1 = new(a_rc.CleanBeforeSpan, a_rc.SetupSpan, new RequiredSpan(ticksToCompleteFirstUsageQty, false), RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);

                            long jitTmp2 = JITBackcalcuateByCycle(a_clockDate, a_act, a_res, 1, rcTemp1, beginTransferByTicks);
                            minOverlapJit = Math.Min(minOverlapJit, jitTmp2);
                        }
                    }
                        break;
                    case InternalOperationDefs.overlapTypes.TransferSpanAfterSetup:
                    {
                        // subtract the setup time off the need by and use 0 for transfer plus.
                        RequiredCapacity rcTemp2 = new(a_rc.CleanBeforeSpan, a_rc.SetupSpan, new RequiredSpan(asn.OverlapTransferTicks, false), RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                        long jitTmp3 = JITBackcalculate(a_clockDate, a_res, a_act, rcTemp2, beginTransferByTicks);
                        minOverlapJit = Math.Min(minOverlapJit, jitTmp3);
                    }
                        break;
                    case InternalOperationDefs.overlapTypes.PercentComplete:
                    {
                        long finishQtySpan = a_act.CalcProcessingSpanOfRequiredFinishQty(a_res);
                        // The length of time since processing that must  complete before overlap.
                        decimal overlapspan = asn.OverlapPercentComplete * finishQtySpan;
                        long reqSpan = (long)Math.Ceiling(overlapspan);
                        RequiredCapacity rcTemp3 = new(a_rc.CleanBeforeSpan, a_rc.SetupSpan, new RequiredSpan(reqSpan, false), RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                        long jitTmp = JITBackcalculate(a_clockDate, a_res, a_act, rcTemp3, beginTransferByTicks);
                        minOverlapJit = Math.Min(jitTmp, minOverlapJit);
                    }
                        break;
                }

                if (asn.TransferEnd == OperationDefs.EOperationTransferPoint.NoTransfer || asn.TransferStart == OperationDefs.EOperationTransferPoint.NoTransfer)
                {
                    continue;
                }

                long transferEnd = long.MaxValue;
                InternalOperation successorOperation = asn.Successor.Operation;
                switch (asn.TransferEnd)
                {
                    case OperationDefs.EOperationTransferPoint.StartOfOperation:
                        transferEnd = beginTransferByTicks;
                        break;
                    case OperationDefs.EOperationTransferPoint.EndOfSetup:
                        transferEnd = beginTransferByTicks + successorOperation.SetupSpan.Ticks;
                        break;
                    case OperationDefs.EOperationTransferPoint.EndOfRun:
                        transferEnd = beginTransferByTicks + successorOperation.SetupSpan.Ticks
                                                           + successorOperation.RunSpan.Ticks
                                                           + successorOperation.PostProcessingSpan.Ticks;
                        break;
                    case OperationDefs.EOperationTransferPoint.EndOfPostProcessing:
                        transferEnd = beginTransferByTicks + successorOperation.SetupSpan.Ticks
                                                           + successorOperation.RunSpan.Ticks
                                                           + successorOperation.PostProcessingSpan.Ticks;
                        break;
                    case OperationDefs.EOperationTransferPoint.EndOfStorage:
                    case OperationDefs.EOperationTransferPoint.EndOfOperation:
                        transferEnd = beginTransferByTicks + successorOperation.SetupSpan.Ticks
                                                           + successorOperation.RunSpan.Ticks
                                                           + successorOperation.PostProcessingSpan.Ticks
                                                           + successorOperation.StorageSpan.Ticks;
                        break;
                    case OperationDefs.EOperationTransferPoint.NoTransfer:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                RequiredCapacity rcTemp4 = null;
                switch (asn.TransferStart)
                {
                    case OperationDefs.EOperationTransferPoint.StartOfOperation:
                        rcTemp4 = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, RequiredSpanPlusSetup.s_notInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                        break;
                    case OperationDefs.EOperationTransferPoint.EndOfSetup:
                        rcTemp4 = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, a_rc.SetupSpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                        break;
                    case OperationDefs.EOperationTransferPoint.EndOfRun:
                        rcTemp4 = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, a_rc.SetupSpan, a_rc.ProcessingSpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                        break;
                    case OperationDefs.EOperationTransferPoint.EndOfPostProcessing:
                        rcTemp4 = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, a_rc.SetupSpan, a_rc.ProcessingSpan, a_rc.PostProcessingSpan, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                        break;
                    case OperationDefs.EOperationTransferPoint.EndOfStorage:
                    case OperationDefs.EOperationTransferPoint.EndOfOperation:
                        rcTemp4 = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, a_rc.SetupSpan, a_rc.ProcessingSpan, a_rc.PostProcessingSpan, a_rc.StorageSpan, RequiredSpanPlusClean.s_notInit);
                        break;
                    case OperationDefs.EOperationTransferPoint.NoTransfer:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (asn.TransferDuringPredeccessorOnlineTime)
                {
                    RequiredCapacity rcTemp5 = new(RequiredSpanPlusClean.s_notInit, RequiredSpanPlusSetup.s_notInit, new RequiredSpan(asn.TransferSpanTicks, false), RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                    transferEnd = JITBackcalculate(a_clockDate, a_res, a_act, rcTemp5, transferEnd);
                }
                else
                {
                    transferEnd -= asn.TransferSpanTicks;
                }

                long jitTemp = JITBackcalculate(a_clockDate, a_res, a_act, rcTemp4, transferEnd);
                minTransferJit = long.Min(minTransferJit, jitTemp);
            }
        }
        else
        {
            //No Successors, we need to call this to account for Material post-processing
            return CalcBeginTransferByTicks(a_clockDate, 0, 0, a_act, a_res, null, a_dbrCalculation);
        }

        long needDateTicks = long.Min(minTransferJit, minOverlapJit);
        return needDateTicks;
    }

    private long JITBackcalcuateByCycle(long a_clockDate, InternalActivity a_act, Resource a_res, long a_cycles, RequiredCapacity a_setupAndPPReqCapacity, long a_backcalculateFromTicks)
    {
        long processingCapacity = a_cycles * CycleSpanTicks;
        bool procOverrun = processingCapacity <= 0 && a_act.ProductionStatus == InternalActivityDefs.productionStatuses.Running;
        RequiredCapacity rc = new (a_setupAndPPReqCapacity.CleanBeforeSpan, a_setupAndPPReqCapacity.SetupSpan, new RequiredSpan(processingCapacity, procOverrun), a_setupAndPPReqCapacity.PostProcessingSpan, a_setupAndPPReqCapacity.StorageSpan, a_setupAndPPReqCapacity.CleanAfterSpan);
        long jit = JITBackcalculate(a_clockDate, a_res, a_act, rc, a_backcalculateFromTicks);
        return jit;
    }

    private long JITBackcalculateByQty(long a_clockDate, InternalActivity a_act, Resource a_res, RequiredCapacity a_setupAndPPReqCapacity, decimal a_qty, long a_actNeedTicks)
    {
        decimal cyclesTemp = a_qty / QtyPerCycle;
        cyclesTemp = Math.Ceiling(cyclesTemp);
        long cycles = (long)cyclesTemp;
        long jit = JITBackcalcuateByCycle(a_clockDate, a_act, a_res, cycles, a_setupAndPPReqCapacity, a_actNeedTicks);
        return jit;
    }

    /// <summary>
    /// Find the successor operation that requires this association to finish the soonest.
    /// </summary>
    private long FindEarliestNeededSuccessorJitStart(bool a_dbrJitDate)
    {
        long earliestPredecessorNeed = PTDateTime.MaxDateTimeTicks; //Successor Op need dates can be after the mo need date. This used to be set to the MO need date which would result 
        //in all operations on the job being set to the MO need date rather than the successor's JIT start date.
        for (int sucI = 0; sucI < Successors.Count; ++sucI)
        {
            AlternatePath.Association asn = Successors[sucI];
            InternalOperation sucOpn = asn.Successor.Operation;

            if (sucOpn.IsNotFinishedAndNotOmitted)
            {
                long earliestDbrJitStartDate = a_dbrJitDate ? sucOpn.m_needInfo.EarliestJitBufferInfo.DbrJitStartDate : sucOpn.m_needInfo.EarliestJitBufferInfo.JitStartDate;
                if (earliestDbrJitStartDate < earliestPredecessorNeed)
                {
                    earliestPredecessorNeed = earliestDbrJitStartDate;
                }
            }
        }

        return earliestPredecessorNeed;
    }

    /*****
     *
     */

    //*****************************************************************************************
    // Scheduled:       use the scheduled times.
    // Not Scheduled:   use the expected times.
    // Finished:        no time is required.
    //*****************************************************************************************

    /// <summary>
    /// Display a warning if calcualted required capacity is larger than warning level.
    /// </summary>
    /// <param name="a_rc"></param>
    private void WarnIfRequiredCapacityIsAboveWarningLevel(RequiredCapacity a_rc)
    {
        if (ScenarioDetail.ScenarioOptions.OpLengthWarningLevelTicks > 0 &&
            a_rc.TotalRequiredCapacity() > ScenarioDetail.ScenarioOptions.OpLengthWarningLevelTicks)
        {
            ScenarioEvents se;

            using (ScenarioDetail.Scenario.AutoTryScenarioEvents(out se, AutoExiter.THREAD_TRY_WAIT_MS))
            {
                string localizedQtyString = Localizer.GetString("Quantity");
                string msg = Localizer.GetErrorString("4113", new object[] { TimeSpan.FromTicks(a_rc.TotalRequiredCapacity()).ToString(), ScenarioDetail.ScenarioOptions.OpLengthWarningLevel.ToString() });
                msg = msg + Environment.NewLine + Environment.NewLine + this + Environment.NewLine + Environment.NewLine + ProductionInfo + " " + localizedQtyString + ": " + RequiredFinishQty + ";";
                se.FirePopupMessageEvent(msg, MessageSeverity.Warning, true, new BaseId());
            }
        }
    }

    /// <summary>
    /// Whether any of the associations OverlapType==TransferSpanBeforeStartOfPredecessor.
    /// </summary>
    /// <returns></returns>
    private bool AssocatedByTransferSpanBeforeStartOfPredecessor(AlternatePath.AssociationCollection a_asns)
    {
        for (int asnI = 0; asnI < a_asns.Count; ++asnI)
        {
            AlternatePath.Association assn = a_asns[asnI];
            if (assn.OverlapType == InternalOperationDefs.overlapTypes.TransferSpanBeforeStartOfPredecessor)
            {
                return true;
            }
        }

        return false;
    }

    protected Resource FindResForJIT(InternalActivity a_ia, Resource a_defaultRes)
    {
        Resource res = a_defaultRes;
        if (a_ia != null && a_ia.Scheduled)
        {
            ResourceBlock rb = a_ia.Batch.PrimaryResourceBlock;
            if (rb != null)
            {
                res = rb.ScheduledResource;
            }
        }
        else if (Scheduled && Activities.Count == 1)
        {
            InternalActivity ia = Activities.GetByIndex(0);
            ResourceBlock rb = ia.Batch.PrimaryResourceBlock;
            if (rb != null)
            {
                res = rb.ScheduledResource;
            }
        }
        else
        {
            if (ResourceRequirements.PrimaryResourceRequirement.DefaultResource != null)
            {
                res = (Resource)ResourceRequirements.PrimaryResourceRequirement.DefaultResource;
            }
            else
            {
                res = (Resource)ResourceRequirements.GetFirstEligiblePrimaryResource();
            }
        }

        return res;
    }

    /// <summary>
    /// A helper for calculating the JIT. It searches backwards for the specified capacity.
    /// </summary>
    /// <param name="a_res">The resource whose capacity will be used to calculate the JIT start ticks.</param>
    /// <param name="a_act">The activity whose JIT is to be calculated.</param>
    /// <param name="a_rc">The required capacity of the activity.Note Processing capcity factors in more features (such as NbrOfPeople) than setup time and post processing time.</param>
    /// <param name="a_backCalculateFromTicks">The time processing needs to finish by. The JIT will be back calculated from this if there isn't enough capacity.</param>
    /// <returns>The JIT start date if enough available capacity is available on the resource. Other wise the JIT will be before the clock.</returns>
    internal long JITBackcalculate(long a_clockDate, Resource a_res, InternalActivity a_act, RequiredCapacity a_rc, long a_backCalculateFromTicks)
    {
#if DEBUG
if (a_clockDate != ManufacturingOrder.ScenarioDetail.Clock)
{
    throw new DebugException("We are expecting clock date for JITBackCalculate but we got a different value, probably SimClock");
}
#endif

        if (a_res == null)
        {
            // No resource was specified. A simple estimate of the back calculated time is returned by subtracting the total required capacity from the FromTicks.
            long estResult = a_backCalculateFromTicks - a_rc.TotalRequiredCapacity();
            return estResult;
        }

        long jitStartTicksTemp;

        DateTime firstIntervalStartDate = a_res.FindFirstOnlineIntervalStartOnOrAfterPoint(PTDateTime.MinDateTime);
        if (a_backCalculateFromTicks <= firstIntervalStartDate.Ticks || firstIntervalStartDate == PTDateTime.MaxDateTime)
        {
            // Assume 100% online capacity
            jitStartTicksTemp = a_backCalculateFromTicks - a_rc.TotalRequiredCapacity();
            return jitStartTicksTemp;
        }

        Resource.FindStartFromEndResult findJITStartDateResult = a_res.FindCapacityReverse(a_clockDate, a_backCalculateFromTicks, a_rc, null, a_act);

        if (findJITStartDateResult.Success)
        {
            jitStartTicksTemp = findJITStartDateResult.StartTicks;
        }
        else
        {
            // The resource didn't have enough capcity between the clock and a_backCalculateFromTicks(NeedDate).

            // The number of ticks that weren't available on the resource.
            long ticksNotAvailOnRes = a_rc.TotalRequiredCapacity() - (long)findJITStartDateResult.Capacity;
            
            // Assume 100% online capacity
            jitStartTicksTemp = findJITStartDateResult.StartTicks - ticksNotAvailOnRes;
        }

        return jitStartTicksTemp;
    }

    internal long GetLongestConnectedResourceTransfer(out Resource o_internalResource, ResourceConnectorManager a_rcm, Resource a_predecessorEligibleResource)
    {
        o_internalResource = null;
        long longestTransferSpan = 0;

        if (AlternatePathNode.ResReqsEligibilityNarrowedDuringSimulation.Count == 0)
        {
            return longestTransferSpan;
        }

        PlantResourceEligibilitySet pres = AlternatePathNode.ResReqsEligibilityNarrowedDuringSimulation.PrimaryEligibilitySet;
        List<Resource> thisOpEligibleResources;

        //If locked to a resource, filter list of eligible resource to only account by locked resource to gather more accurate information
        if (Locked == lockTypes.Locked)
        {
            thisOpEligibleResources = new List<Resource>();
            foreach (InternalActivity act in Activities)
            {
                thisOpEligibleResources.AddIfNew(act.PrimaryResourceRequirementLock());
            }
        }
        else
        {
            thisOpEligibleResources = pres.GetResources();
        }

        foreach (Resource eligibleResource in thisOpEligibleResources)
        {
            GetLongestConnectedResourceTransferHelper(eligibleResource, ref longestTransferSpan, ref o_internalResource, a_rcm, a_predecessorEligibleResource);
        }

        return longestTransferSpan;
    }

    private static void GetLongestConnectedResourceTransferHelper(Resource a_res, ref long r_longestSetbackTicks, ref Resource r_longestSetbackRes, ResourceConnectorManager a_rcm, Resource a_predecessorEligibleResource)
    {
        long resSetback = 0;
        //Account for transit hours if resource connector relationship is found
        long connectorSetback = 0;
        IEnumerable<ResourceConnector> connectors = a_rcm.GetConnectorsBetweenResources(a_predecessorEligibleResource, a_res);
        foreach (ResourceConnector connector in connectors)
        {
            if (connector.TransitTicks > connectorSetback)
            {
                connectorSetback = connector.TransitTicks;
            }
        }

        if (connectorSetback > r_longestSetbackTicks)
        {
            resSetback = connectorSetback;
        }

        if (resSetback > 0)
        {
            r_longestSetbackTicks = resSetback;
            r_longestSetbackRes = a_res;
        }
    }

    /// <summary>
    /// Activity's whose eligibility has already been overridden are not filtered.
    /// </summary>
    internal void AdjustedPlantResourceEligibilitySets_Filter(ResReqsPlantResourceEligibilitySets.ExcludeFromManualFilter a_doNotFilterRes)
    {
        if (IsNotFinishedAndNotOmitted)
        {
            for (int actI = 0; actI < Activities.Count; ++actI)
            {
                InternalActivity act = Activities.GetByIndex(actI);
                if (a_doNotFilterRes.ExcludedManualFiltersDictionary.TryGetValue(act.Id, out List<BaseId> resourceIds))
                {
                    act.AdjustedPlantResourceEligibilitySets_Filter(resourceIds);

                }
                else
                {
                    act.AdjustedPlantResourceEligibilitySets_Filter(new List<BaseId>());
                }
            }
        }
    }

    /// <summary>
    /// Filter this node's AdjustedPlantEligibilitySet and recursively filter its successors.
    /// </summary>
    internal void AdjustedPlantResourceEligibilitySets_FilterNodeAndSuccessors(ResReqsPlantResourceEligibilitySets.ExcludeFromManualFilter a_excludeFromManualFilter)
    {
        AlternatePathNode.AdjustedPlantResourceEligibilitySets_FilterNodeAndSuccessors(a_excludeFromManualFilter);
    }
    #endregion

    #region Max Delay
    /// <summary>
    /// Whether to enforce Max delay between the predecessor and successors.
    /// </summary>
    /// <param name="io"></param>
    /// <returns></returns>
    internal bool EnforceMaxDelay()
    {
        if (ScenarioDetail.ScenarioOptions.EnforceMaxDelay && Activities.Count == 1 && AlternatePathNode.Predecessors.Count == 1)
        {
            AlternatePath.Association predAsn = AlternatePathNode.Predecessors[0];
            if (predAsn.MaxDelaySet)
            {
                ResourceOperation predRO = (ResourceOperation)predAsn.Predecessor.Operation;
                //TODO: MaxDelay. Should max delay carry through omitted operations? For example: Op1 -> MaxDelay to Op2 -> Max Delay to Op 3. If Op2 is omitted, what is the max delay between Op1 and Op3?
                if (predRO.Activities.Count == 1 && !predRO.IsFinishedOrOmitted)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Takes the predecessor operation.
    /// This version isn't used by the simulation logic, so ScenarioOptions.EnforceMaxDelay isn't examined.
    /// </summary>
    /// <param name="io"></param>
    /// <returns></returns>
    internal bool EnforceMaxDelayPredecessorCheck()
    {
        if (Activities.Count == 1 && AlternatePathNode.Successors.Count == 1)
        {
            AlternatePath.Association asn = AlternatePathNode.Successors[0];

            if (asn.MaxDelaySet)
            {
                ResourceOperation sucRO = (ResourceOperation)asn.Successor.Operation;

                if (sucRO.Activities.Count == 1)
                {
                    return true;
                }
            }
        }

        return false;
    }
    #endregion

    #region Similarity
    internal override int SimilarityComparison(BaseOperation a_bo)
    {
        int v;
        InternalOperation io = (InternalOperation)a_bo;

        if ((v = base.SimilarityComparison(a_bo)) != 0)
        {
            return v;
        }

        if ((v = ProductionInfo.SimilarityComparison(io.ProductionInfo)) != 0)
        {
            return v;
        }

        if ((v = m_setupNumber.CompareTo(io.m_setupNumber)) != 0)
        {
            return v;
        }

        if ((v = m_setupColor.ToArgb().CompareTo(io.m_setupColor.ToArgb())) != 0)
        {
            return v;
        }

        if ((v = m_resourceRequirements.SimilarityComparison(io.m_resourceRequirements)) != 0)
        {
            return v;
        }

        if ((v = m_internalOperationFlags.CompareTo(io.m_internalOperationFlags)) != 0)
        {
            return v;
        }

        return 0;
    }

    internal override void DetermineDifferences(BaseOperation a_op, int a_differenceTypes, System.Text.StringBuilder a_warnings)
    {
        InternalOperation op = (InternalOperation)a_op;

        ProductionInfo.DetermineDifferences(op.ProductionInfo, a_differenceTypes, a_warnings);

        if ((a_differenceTypes & ManufacturingOrder.DifferenceTypes.setupCodes) > 0)
        {
            DifferencesStatics.Differences("setupNumber", m_setupNumber, op.m_setupNumber, a_warnings);
            DifferencesStatics.Differences("setupColor", m_setupColor, op.m_setupColor, a_warnings);
        }

        if ((a_differenceTypes & ManufacturingOrder.DifferenceTypes.resourceRequirements) > 0)
        {
            m_resourceRequirements.DetermineDifferences(op.m_resourceRequirements, a_warnings);
        }
    }
    #endregion

    #region statics
    /// <summary>
    /// Taking planning scrap percent into consideration, returns the quantity you must start to end up with the required quantity.
    /// </summary>
    /// <param name="requiredQty"></param>
    /// <returns></returns>
    internal static decimal PlanningScrapPercentAdjustedQty(decimal a_planningScrapPercent, decimal a_requiredQty)
    {
        return a_requiredQty / (1 - a_planningScrapPercent);
    }

    /// <summary>
    /// Given a quantity tell how much scrap to expect.
    /// </summary>
    /// <param name="qty"></param>
    /// <returns></returns>
    internal static decimal PlanningScrapQty(decimal a_planningScrapPercent, decimal a_qty)
    {
        return a_qty * a_planningScrapPercent;
    }
    #endregion

    internal long GetNbrOfActivitiesToSchedule()
    {
        long nbrOfActivites = 0;

        if (IsNotFinishedAndNotOmitted)
        {
            for (int i = 0; i < Activities.Count; ++i)
            {
                if (!Activities.GetByIndex(i).Finished)
                {
                    ++nbrOfActivites;
                }
            }
        }

        return nbrOfActivites;
    }

    /// <summary>
    /// Whether any activity has any of its blocks scheduled within its frozan span.
    /// </summary>
    /// <param name="a_spanCalc">Used to calculate the resource frozen span.</param>
    /// <returns></returns>
    internal bool AnyActivityInSpan(long a_clockDate, OptimizeSettings.ETimePoints a_spanType)
    {
        return Activities.AnyActivityInSpan(a_clockDate, a_spanType);
    }

    /// <summary>
    /// Whether an activity lies downstream from this operation.
    /// </summary>
    /// <param name="a_ia"></param>
    /// <returns></returns>
    internal bool IsDownstream(InternalActivity a_ia)
    {
        AlternatePath.Node downstreamNode = a_ia.Operation.AlternatePathNode;
        return AlternatePath.IsDownstream(AlternatePathNode, downstreamNode);
    }

    /// <summary>
    /// Whether any activities are in production.
    /// </summary>
    /// <returns></returns>
    internal bool InProduction()
    {
        for (int actI = 0; actI < Activities.Count; ++actI)
        {
            InternalActivity act = Activities.GetByIndex(actI);
            if (act.InProduction())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Whether the primary resource requirement has a default resource.
    /// </summary>
    public bool HasDefaultResource => ResourceRequirements.HasDefaultResource;

    [DoNotAuditProperty]
    public MaterialRequirement Current => throw new NotImplementedException();

    [DoNotAuditProperty]
    object IEnumerator.Current => throw new NotImplementedException();

    /// <summary>
    /// Whether scheduling the final activity of a Split Operation.
    /// </summary>
    public bool SchedulingFinalActivity => m_activitiesLeftToSchedule == 1;

    public void Dispose() { }

    public bool MoveNext()
    {
        throw new NotImplementedException();
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// </summary>
    /// <param name="a_simStartTime"></param>
    /// <returns>Returns the earliest department frozen or stable span based on the eligible resources of this operation</returns>
    internal long GetEarliestDepartmentalEndSpan(SimulationTimePoint a_simStartTime)
    {
        if (IsFinishedOrOmitted || AlternatePathNode.ResReqsEligibilityNarrowedDuringSimulation.Count == 0)
        {
            return a_simStartTime.DateTimeTicks;
        }

        PlantResourceEligibilitySet pres = AlternatePathNode.ResReqsEligibilityNarrowedDuringSimulation.PrimaryEligibilitySet;

        long earliestDepartmentalEndOfFrozenOrStableSpan = long.MaxValue;
        SortedDictionary<BaseId, EligibleResourceSet>.Enumerator resEtr = pres.GetEnumerator();
        while (resEtr.MoveNext())
        {
            EligibleResourceSet resSet = resEtr.Current.Value;
            for (int resI = 0; resI < resSet.Count; ++resI)
            {
                InternalResource res = resSet[resI];
                long releaseTicks = a_simStartTime.GetTimeForResource(res);
                // Use the earliest of the possible frozen span times. 
                earliestDepartmentalEndOfFrozenOrStableSpan = Math.Min(earliestDepartmentalEndOfFrozenOrStableSpan, releaseTicks);
            }
        }

        return earliestDepartmentalEndOfFrozenOrStableSpan;
    }

    /// <summary>
    /// A predecessor has scheduled on a resource, verify that the assigned move resources are still valid based on resource connectors
    /// </summary>
    public void VerifyResourceConnectors(Resource a_connectorRes, InternalOperation a_scheduledOperation, ResourceConnectorManager a_resourceConnectorManager)
    {
        foreach (InternalActivity activity in Activities)
        {
            //TODO: Check this with moves
            Resource moveResource = a_connectorRes; //activity.GetMoveResource(a_connectionResReqIdx);
            if (moveResource == null)
            {
                continue;
            }

            Dictionary<BaseId, Resource> downstreamConnectors = a_resourceConnectorManager.GetDownstreamSuccessorConnectorsFromResource(a_connectorRes.Id);
            if (downstreamConnectors.Count > 0 && !downstreamConnectors.ContainsKey(moveResource.Id))
            {
                //The predecessor has moved to a resource that violates the resource connectors for where this activity is planned to schedule.
                activity.ClearMoveResources();
            }
        }
    }
}