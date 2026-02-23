using PT.APSCommon;
using PT.Scheduler.Schedule;
using PT.Scheduler.Schedule.Operation;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Extensions;
using PT.SchedulerDefinitions;
using PT.Transmissions;
using System.Collections;

using PT.Common.Exceptions;

namespace PT.Scheduler;

/// <summary>
/// A collection of Manufacturing Orders that are being produced usually for a specific customer.
/// </summary>
public partial class Job : IEnumerable<ManufacturingOrder>
{
    #region Eligibility
    /// <summary>
    /// Part of Step 1, 2 & 3 of eligibility.
    /// In setp 1:
    /// For each individual resource requirement, the resources that are able to satisfy them are determined. ManufacturingOrder.EligiblePlants is taken into consideration at this point.
    /// In step 2:
    /// Each operation determines where it can be made, satisfying all resource requirements.
    /// That is determine the plants capable of satisfying all resource requirements and the resources that can be used to satisfy each resource requirement.
    /// In step 3:
    /// Take other path constraints into consideration, for instance Resource Connectors.
    /// In step 4:
    /// Job.CanSpanPlants and ManufacturingOrder.CanSpanPlants are taken into consideration.
    /// </summary>
    internal void ComputeEligibility(ProductRuleManager a_productRuleManager, bool a_recalculateJIT = true)
    {
        try
        {
            if (Template)
            {
                return; // Will compute Eligibility when copied for actual Jobs since Qty may change.
            }

            BaseIdList cantSpanPlantsEligiblePlants = null;
            Dictionary<BaseId, ResReqsPlantResourceEligibilitySets>[] mosCurrentPathConstrainedEligibility = new Dictionary<BaseId, ResReqsPlantResourceEligibilitySets>[ManufacturingOrders.Count];

            bool eligibilityChanged = false;
            /// Part of Step 2 of eligibility. Each operation determines where it can be made.
            /// That is the plants capable of satisfying all resource requirements, plus the eligible resources.
            for (int moI = 0; moI < ManufacturingOrders.Count; ++moI)
            {
                ManufacturingOrder mo = ManufacturingOrders[moI];

                if (!mo.Finished)
                {
                    for (int alternatePathI = 0; alternatePathI < mo.AlternatePaths.Count; alternatePathI++)
                    {
                        AlternatePath ap = mo.AlternatePaths[alternatePathI];
                        ap.CreateEffectiveResourceEligibilitySets(a_productRuleManager);

                        //**************************************************************************************************
                        // Step 3: Take path level constraints into consideration.
                        // Simple predecessor and successor 2 operations in routing
                        // foreach alternate path node
                        //     foreach resource requirement
                        //         foreach eligible resource that has connections
                        //             foreach successor
                        //                 foreach eligible resource
                        //                     ++is there a connection between the predcessr and successor
                        //                     
                        // If there end up being no connections between the predecessor and any success, then the predecessor resource isn't eligible.
                        // 
                        // 3 or more operations in routing.
                        // In the first phase above, determine the number of eligible successor operations. In the case where there are no resource connectors, set the value to 1.
                        // Moving from operations in the last level to the first level update the counts by subtracting for each successor==0.
                        // Remove all resources that end up at 0.
                        // 
                        // mo.CanSpanPlants & job.CanSpanPlants
                        // If you do this as step 2.5 it should determine this 
                        //
                        // 
                        //**************************************************************************************************

                        Dictionary<BaseId, ResReqsPlantResourceEligibilitySets> pathConstrainedEligibilityTemp;
                        pathConstrainedEligibilityTemp = new Dictionary<BaseId, ResReqsPlantResourceEligibilitySets>(); //Skip all of this if there are no Resource connectors. Just use the ro.ResourceRequirements.__EligibleRsources without creating the deep copy.

                        List<Pair<AlternatePath.Node, int>> ops = ap.GetNodesByLevel(true);
                        for (int nodeI = 0; nodeI < ops.Count; ++nodeI)
                        {
                            ResourceOperation ro = ops[nodeI].value1.Operation;
                            ResReqsPlantResourceEligibilitySets tempSet;
                            if (ro.IsFinishedOrOmitted)
                            {
                                tempSet = new ResReqsPlantResourceEligibilitySets();
                            }
                            else
                            {
                                tempSet = new ResReqsPlantResourceEligibilitySets(ro.ResourceRequirements.EligibleResources);
                            }

                            pathConstrainedEligibilityTemp.Add(ro.Id, tempSet);
                        }

                        for (int nodeI = 0; nodeI < ops.Count; ++nodeI)
                        {
                            ResourceOperation op = ops[nodeI].value1.Operation;
                            if (op.IsFinishedOrOmitted)
                            {
                                continue;
                            }

                            ResReqsPlantResourceEligibilitySets opLevelEligibleResources = pathConstrainedEligibilityTemp[op.Id];
                            PlantResourceEligibilitySet pres = opLevelEligibleResources.PrimaryEligibilitySet;

                            SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = pres.GetEnumerator();
                            while (ersEtr.MoveNext())
                            {
                                EligibleResourceSet ers = ersEtr.Current.Value;
                                for (int resI = 0; resI < ers.Count; ++resI)
                                {
                                    EligibleResourceSet.ResourceData rd = ers.GetResourceData(resI);

                                    Dictionary<BaseId, Resource> downstreamConnectors = ScenarioDetail.ResourceConnectorManager.GetDownstreamSuccessorConnectorsFromResource(rd.m_res.Id);
                                    if (!downstreamConnectors.Any())
                                    {
                                        rd.m_successorResourceConnectionCount = int.MaxValue;
                                    }
                                    else
                                    {
                                        AlternatePath.Node predNode = ops[nodeI].value1;

                                        if (predNode.Successors.Count == 0)
                                        {
                                            rd.m_successorResourceConnectionCount = int.MaxValue;
                                        }
                                        else
                                        {
                                            for (int sucI = 0; sucI < predNode.Successors.Count; ++sucI)
                                            {
                                                ResourceOperation sucOp = predNode.Successors[sucI].Successor.Operation;
                                                //Op is not going to schedule, don't include resource connector calculations
                                                if (sucOp.IsFinishedOrOmitted)
                                                {
                                                    continue;
                                                }

                                                ResReqsPlantResourceEligibilitySets sucPresAL = pathConstrainedEligibilityTemp[sucOp.Id];
                                                PlantResourceEligibilitySet sucPres = sucPresAL.PrimaryEligibilitySet;

                                                SortedDictionary<BaseId, EligibleResourceSet>.Enumerator sucErsEtr = sucPres.GetEnumerator();
                                                while (sucErsEtr.MoveNext())
                                                {
                                                    EligibleResourceSet sucERS = sucErsEtr.Current.Value;
                                                    for (int eligibleResourceI = 0; eligibleResourceI < sucERS.Count; ++eligibleResourceI)
                                                    {
                                                        EligibleResourceSet.ResourceData sucRD = sucERS.GetResourceData(eligibleResourceI);
                                                        if (downstreamConnectors.ContainsKey(sucRD.m_res.Id))
                                                        {
                                                            ++rd.m_successorResourceConnectionCount;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        for (int nodeI = 0; nodeI < ops.Count; ++nodeI)
                        {
                            ResourceOperation op = ops[nodeI].value1.Operation;
                            if (op.IsFinishedOrOmitted)
                            {
                                continue;
                            }

                            ResReqsPlantResourceEligibilitySets opLevelEligibleResources = pathConstrainedEligibilityTemp[op.Id];
                            PlantResourceEligibilitySet pres = opLevelEligibleResources.PrimaryEligibilitySet;
                            int ersDeleteCount = 0;

                            SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = pres.GetEnumerator();
                            while (ersEtr.MoveNext())
                            {
                                EligibleResourceSet ers = ersEtr.Current.Value;
                                int rdDeleteCount = 0;

                                for (int resI = 0; resI < ers.Count; ++resI)
                                {
                                    EligibleResourceSet.ResourceData rd = ers.GetResourceData(resI);

                                    Dictionary<BaseId, Resource> downstreamConnectors = ScenarioDetail.ResourceConnectorManager.GetDownstreamSuccessorConnectorsFromResource(rd.m_res.Id);

                                    if (downstreamConnectors.Count > 0)
                                    {
                                        AlternatePath.Node predNode = ops[nodeI].value1;
                                        for (int sucI = 0; sucI < predNode.Successors.Count; ++sucI)
                                        {
                                            ResourceOperation sucOp = predNode.Successors[sucI].Successor.Operation;
                                            //Op is not going to schedule, don't include resource connector calculations
                                            if (sucOp.IsFinishedOrOmitted)
                                            {
                                                continue;
                                            }

                                            ResReqsPlantResourceEligibilitySets sucPresAL = pathConstrainedEligibilityTemp[sucOp.Id];
                                            PlantResourceEligibilitySet sucPres = sucPresAL.PrimaryEligibilitySet;

                                            SortedDictionary<BaseId, EligibleResourceSet>.Enumerator sucErsEtr = sucPres.GetEnumerator();
                                            while (sucErsEtr.MoveNext())
                                            {
                                                EligibleResourceSet sucERS = sucErsEtr.Current.Value;
                                                for (int eligibleResourceI = 0; eligibleResourceI < sucERS.Count; ++eligibleResourceI)
                                                {
                                                    EligibleResourceSet.ResourceData sucRD = sucERS.GetResourceData(eligibleResourceI);
                                                    if (downstreamConnectors.ContainsKey(sucRD.m_res.Id))
                                                    {
                                                        if (sucRD.m_successorResourceConnectionCount == 0)
                                                        {
                                                            --rd.m_successorResourceConnectionCount;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (rd.m_successorResourceConnectionCount == 0)
                                    {
                                        rd.m_delete = true;
                                        ++rdDeleteCount;
                                    }
                                }

                                if (rdDeleteCount == ers.Count)
                                {
                                    ers.m_delete = true;
                                    ++ersDeleteCount;
                                }
                            }

                            if (ersDeleteCount == pres.Count)
                            {
                                for (int rrI = 0; rrI < opLevelEligibleResources.Count; ++rrI)
                                {
                                    opLevelEligibleResources[rrI].m_delete = true;
                                }
                            }
                        }

                        Dictionary<BaseId, ResReqsPlantResourceEligibilitySets> pathConstrainedEligibility;
                        pathConstrainedEligibility = new Dictionary<BaseId, ResReqsPlantResourceEligibilitySets>(); //Skip all of this if there are no Resource connectors. Just use the ro.ResourceRequirements.__EligibleRsources without creating the deep copy.

                        for (int nodeI = 0; nodeI < ops.Count; ++nodeI)
                        {
                            ResourceOperation ro = ops[nodeI].value1.Operation;
                            ResReqsPlantResourceEligibilitySets tempSet = new (pathConstrainedEligibilityTemp[ro.Id]);
                            pathConstrainedEligibility.Add(ro.Id, tempSet);
                        }

                        //**************************************************************************************************
                        // Step 4 of eligibility.
                        // 
                        // Narrow down eligibility based on the Job and Manufacturing order CanSpanPlants setting.
                        // The result is stored in totalEligibility.
                        // Invariants:
                        // 1. The corresponding operation's ResourceRequirementManager must have computed plant/resource eligibility.
                        // 2. The corresponding operation must be a type of InternalOperation.
                        //**************************************************************************************************
                        //
                        // Now within the node you may narrow down the plants that are eligible based on can span plants within the job and the 
                        // manufacturing order.
                        //
                        //	The possibilities:
                        //		Job.CanSpanPlants=true
                        //			ManufacturingOrder.CanSpanPlants=false : You can narrow down selection to only those plants that are capable of processing every operation in the manufacturing order.
                        //			ManufacturingOrder.CanSpanPlants=true  : Everything is always acceptable.
                        //
                        //		Job.CanSpanPlants=false
                        //			ManufacturingOrder.CanSpanPlants=false : You can narrow down selection to only those plants that are capable of processing every operation of every manufacturing order.
                        //			ManufacturingOrder.CanSpanPlants=true  : Same as above.
                        //
                        //	The end result for each node is a set of resources for each eligible plant for each resource requirement.
                        //
                        //**************************************************************************************************
                        if (CanSpanPlants)
                        {
                            if (mo.CanSpanPlants && mo.LockedPlant == null)
                            {
                                //------------------------------------------------------------------------------------------------------------
                                // Job.CanSpanPlants==true;
                                // ManufacturingOrder.CanSpanPlants==true;
                                // ManufacturingOrder.LockedPlant==null
                                //------------------------------------------------------------------------------------------------------------

                                // Everything is acceptable so you can just make a copy of the eligibility computed by the RequirementManager.
                                // Since totalPlantResourceEligibilitySets is a copy of the EffectiveResourceRequirment
                                // eligibility set there is nothing that needs to be done in this block.
                                ap.AllPlantsAreEligible(pathConstrainedEligibility);
                            }
                            else // ManufacturingOrder.CanSpanPlants==false
                            {
                                //------------------------------------------------------------------------------------------------------------
                                // Job.CanSpanPlants==true;
                                // ManufacturingOrder.CanSpanPlants==false;
                                // ManufacturingOrder.LockedPlant is either null or set to a plant.
                                //------------------------------------------------------------------------------------------------------------

                                // Narrow down eligibility of this MO  to plants that are capable of manufacturing the entire MO.
                                ap.MOCantSpanPlants(pathConstrainedEligibility, out bool changed);
                                eligibilityChanged |= changed;
                            }
                        }
                        else // Job.CanSpanPlants==false
                        {
                            //------------------------------------------------------------------------------------------------------------
                            // Job.CanSpanPlants==false;
                            // ManufacturingOrder.CanSpanPlants is irrelevant. The affect of Job's setting is that CanSpanPlants for the MOs reverts to false.
                            //------------------------------------------------------------------------------------------------------------

                            // Narrow down eligibility of every MO to plants that are capable of manufacturing the entire MO.
                            BaseIdList moPlants = ap.MOCantSpanPlants(pathConstrainedEligibility, out bool changed);
                            eligibilityChanged |= changed;

                            // Find the intersection among all eligible MO plants.
                            // The entire job must be scheduled within the same plant.
                            if (mo.CurrentPath == ap)
                            {
                                if (cantSpanPlantsEligiblePlants == null)
                                {
                                    cantSpanPlantsEligiblePlants = moPlants;
                                }
                                else
                                {
                                    cantSpanPlantsEligiblePlants.Intersection(moPlants);
                                }
                            }
                        }

                        if (mo.CurrentPath == ap)
                        {
                            mosCurrentPathConstrainedEligibility[moI] = pathConstrainedEligibility;
                        }
                    }
                }
            }

            if (!CanSpanPlants)
            {
                if (cantSpanPlantsEligiblePlants != null)
                {
                    for (int manufacturingOrderI = 0; manufacturingOrderI < ManufacturingOrders.Count; ++manufacturingOrderI)
                    {
                        ManufacturingOrder mo = ManufacturingOrders[manufacturingOrderI];
                        if (!mo.Finished)
                        {
                            eligibilityChanged |= mo.CurrentPath.NarrowDownEligibilityToThesePlants(cantSpanPlantsEligiblePlants, mosCurrentPathConstrainedEligibility[manufacturingOrderI]);
                        }
                    }
                }
            }

            //Now that eligibility changed? 
            //We need to update JIT to make sure new eligible resources have JIT data.
            if (eligibilityChanged && a_recalculateJIT)
            {
                CalculateJitTimes(ScenarioDetail.Clock, false);
            }
        }
        catch (PTException)
        {
            throw;
        }
        catch (Exception err)
        {
            throw new PTException("2892", err, new object[] { Name });
        }
    }

    /// <summary>
    /// After recomputing eligibility, you can verify whether a scheduled job is still eligible on the resource's that it's scheduled on. If not, unschedule the job.
    /// </summary>
    /// <returns></returns>
    internal bool VerifyEligibilityOfScheduledResourceRequirements()
    {
        if (m_scheduledStatus == JobDefs.scheduledStatuses.Scheduled ||
            m_scheduledStatus == JobDefs.scheduledStatuses.PartiallyScheduled)
        {
            for (int mI = 0; mI < ManufacturingOrders.Count; ++mI)
            {
                ManufacturingOrder mo = ManufacturingOrders[mI];
                AlternatePath currentPath = mo.CurrentPath;
                for (int nodeI = 0; nodeI < currentPath.NodeCount; ++nodeI)
                {
                    AlternatePath.Node node = currentPath.GetNodeByIndex(nodeI);
                    ResourceOperation ro = (ResourceOperation)node.Operation;
                    for (int actI = 0; actI < ro.Activities.Count; ++actI)
                    {
                        InternalActivity ia = ro.Activities.GetByIndex(actI);
                        for (int blockResReqI = 0; blockResReqI < ia.ResourceRequirementBlockCount; ++blockResReqI)
                        {
                            ResourceBlock rb = ia.GetResourceRequirementBlock(blockResReqI);
                            // Check whether this is in the set of eligible resources for this resource requirement. If not return false and unschedule the job somewhere.
                            if (rb != null && rb.ScheduledResource != null)
                            {
                                if (node.ResReqsMasterEligibilitySet.Count == 0 || !node.ResReqsMasterEligibilitySet[blockResReqI].Contains(rb.ScheduledResource))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    /// When an adjustment is made to the plant list (adds/deletes), you need to call this function so the jobs can update their
    /// MO plant eligibility.
    /// </summary>
    /// <param name="t"></param>
    internal void AdjustEligiblePlants(ScenarioBaseT a_t)
    {
        m_moManager.AdjustEligiblePlants(a_t);
    }

    /// <summary>
    /// Filter AdjustedPlantResourceEligibilitySets down to a single plant. This might lead to the job being unschedulable.
    /// If needed, this should be done prior to the optimize to it can be excluded if it's no longer schedulable.
    /// </summary>
    /// <param name="a_plantId"></param>
    internal void AdjustedPlantResourceEligibilitySets_FilterDownToSpecificPlant(BaseId a_plantId)
    {
        for (int moI = 0; moI < ManufacturingOrders.Count; ++moI)
        {
            ManufacturingOrders[moI].AdjustedPlantResourceEligibilitySets_FilterDownToSpecificPlant(a_plantId);
        }
    }

    /// <summary>
    /// Activity's whose eligibility has already been overridden are not filtered.
    /// </summary>
    internal void AdjustedPlantResourceEligibilitySets_Filter(ResReqsPlantResourceEligibilitySets.ExcludeFromManualFilter a_excludeFromManualFilter)
    {
        for (int moI = 0; moI < ManufacturingOrders.Count; ++moI)
        {
            ManufacturingOrders[moI].AdjustedPlantResourceEligibilitySets_Filter(a_excludeFromManualFilter);
        }
    }

    /// <summary>
    /// Determine whether all the non-finished MOs within the Job can be scheduled based on resource requirements, CanSpanPlants settings, etc...
    /// The return value is either of:
    /// AllPathsSatisfiable: all paths in all the MOs are satisfiable.
    /// SomePathsNotSatisfiable: at least one MO has a paths that are satisfiable and paths that aren't satisfiable.
    /// </summary>
    internal ManufacturingOrder.AlternatePathSatisfiability AdjustedPlantResourceEligibilitySets_IsSatisfiable(ScenarioDetail.SimulationType a_simulationType)
    {
        ManufacturingOrder.AlternatePathSatisfiability satisfiability = ManufacturingOrder.AlternatePathSatisfiability.AllPathsSatisfiable;

        for (int moI = 0; moI < ManufacturingOrders.Count; moI++)
        {
            ManufacturingOrder mo = ManufacturingOrders[moI];
            ManufacturingOrder.AlternatePathSatisfiability moSatisfiability;

            if (!mo.Finished)
            {
                moSatisfiability = mo.AdjustedPlantResourceEligibilitySets_IsSatisfiable(a_simulationType);

                if (satisfiability == ManufacturingOrder.AlternatePathSatisfiability.AllPathsSatisfiable)
                {
                    // Satisfiability can go from all down to anything.
                    satisfiability = moSatisfiability;
                }
                else if (satisfiability == ManufacturingOrder.AlternatePathSatisfiability.SomePathsNotSatisfiable && moSatisfiability == ManufacturingOrder.AlternatePathSatisfiability.NoPathsSatisfiable)
                {
                    // Satisfiability can go from some to nothing.
                    satisfiability = moSatisfiability;
                }
                // else it must already be set to nothing
            }
        }

        return satisfiability;
    }

    /// <summary>
    /// Computes eligibility. If the job becomes ineligible on the resources it's scheduled on, it will be unscheduled.
    /// </summary>
    /// <returns>true if the job was unscheduled.</returns>
    internal bool ComputeEligibilityAndUnscheduleIfIneligible(ProductRuleManager a_productRuleManager, bool a_recalculateJIT = true)
    {
        bool unscheduled = false;

        ComputeEligibility(a_productRuleManager, a_recalculateJIT);
        for (int moI = 0; moI < ManufacturingOrders.Count; ++moI)
        {
            ManufacturingOrder mo = ManufacturingOrders[moI];
            for (int pathI = 0; pathI < mo.AlternatePaths.Count; ++pathI)
            {
                AlternatePath path = mo.AlternatePaths[pathI];
                if (path.IsAnchored() && !path.AdjustedPlantResourceEligibilitySets_IsSatisfiable())
                {
                    for (int nodeI = 0; nodeI < path.NodeCount; ++nodeI)
                    {
                        AlternatePath.Node node = path[nodeI];
                        InternalOperation nodeOp = (InternalOperation)node.Operation;
                        nodeOp.SetAnchor(false, null);
                    }
                }
            }
        }

        if (ScheduledStatus != JobDefs.scheduledStatuses.Finished)
        {
            if (!VerifyEligibilityOfScheduledResourceRequirements())
            {
                Unschedule();
                unscheduled = true;
            }
        }

        return unscheduled;
    }
    #endregion

    #region JIT
    /// <summary>
    /// Calculate the JIT start date of all the MOs in the job.
    /// </summary>
    /// <param name="a_limitToCurrentPaths">Whether to limit calculating to the current path.</param>
    /// <param name="a_alreadyCalculatedMoIds">Set of ManufacturingOrder ids that have already had their JIT calculated. This is used to avoid calculating Jit multiple times for the same MO.</param>
    internal void CalculateJitTimes(long a_simClock, bool a_limitToCurrentPaths, HashSet<BaseId> a_alreadyCalculatedMoIds = null)
    {
        if (a_alreadyCalculatedMoIds == null)
        {
            a_alreadyCalculatedMoIds = new HashSet<BaseId>();
        }

        for (int i = ManufacturingOrders.Count - 1; i >= 0; i--)
        {
            CalculateJitTimes(a_simClock, ManufacturingOrders[i], a_limitToCurrentPaths, a_alreadyCalculatedMoIds);
        }
    }

    /// <summary>
    /// Calculates Jit for Successor Manufacturing Orders first and then the Manufacturing Order that is passed in.
    /// This is necessary because Jit Start date of Successor Manufacturing Orders affects the Jit calculation of
    /// predecessors.
    /// </summary>
    /// <param name="a_simClock"></param>
    /// <param name="a_mo">ManufacturingOrder whose Jit should be calculated</param>
    /// <param name="a_limitToCurrentPaths">whether to calculate Jit for current path only</param>
    /// <param name="a_alreadyCalculatedMoIds">A list of manufacturing order ids whose jit has already been calculated</param>
    internal void CalculateJitTimes(long a_simClock, ManufacturingOrder a_mo, bool a_limitToCurrentPaths, HashSet<BaseId> a_alreadyCalculatedMoIds)
    {
        if (a_alreadyCalculatedMoIds.Contains(a_mo.Id))
        {
            // It's possible that multiple jobs have this MO as a successor, so it's possible that it's
            // JIT start time has already been determined.
            return;
        }

        Stack<ManufacturingOrder> moStack = new ();
        moStack.Push(a_mo);

        while (moStack.Count > 0)
        {
            ManufacturingOrder mo = moStack.Pop();
            if (a_alreadyCalculatedMoIds.Contains(mo.Id))
            {
                continue;
            }

            mo.CalculateJitTimes(a_simClock, a_limitToCurrentPaths);
            a_alreadyCalculatedMoIds.Add(mo.Id);

            if (mo.SuccessorMOs != null)
            {
                for (int i = 0; i < mo.SuccessorMOs.Count; i++)
                {
                    SuccessorMO successorMO = mo.SuccessorMOs[i];
                    if (successorMO.SuccessorManufacturingOrder != null)
                    {
                        moStack.Push(successorMO.SuccessorManufacturingOrder);
                    }
                }
            }
        }
    }
    #endregion

    #region Successor MO
    internal void LinkSuccessorMOs()
    {
        ManufacturingOrders.LinkSuccessorMOs();
    }
    #endregion

    #region Simulation
    #region BOOLS
    private BoolVector32 m_bools; //DO NOT USE FOR PROPERTIES TO PRESERVE -- CLEARED DURING SIMULATIONS.
    private const int BOOLS_WAITING_FOR_JOB_RELEASE_EVENT_IDX = 0;
    private const int BOOLS_JOB_SCHEDULED_STATUS_UPDATED_IDX = 1;
    private const int BOOLS_SUB_COMPONENT_PRIORITY_SET_IDX = 2;
    private const int BOOLS_SUB_COMPONENT_HOT_SET_IDX = 3;
    private const int BOOLS_SUB_COMPONENT_NEED_DATE_SET_IDX = 4;
    private const int BOOLS_DO_NOT_SCHEDULE_PLANT_NOT_INCLUDED_IN_OPTIMIZE = 5;
    private const int CAN_SPAN_PLANTS_HANDLED_IDX = 6;
    private const int BOOLS_DO_NOT_SCHEDULE_NOT_SATISFIABLE = 7;
    private const int SPLIT_DURING_OPTIMIZE_TO_BE_SCHEDULED_IDX = 8;
    private const int BOOLS_TO_BE_SCHEDULED_IDX = 9;

    internal bool WaitingForJobReleaseEvent
    {
        get => m_bools[BOOLS_WAITING_FOR_JOB_RELEASE_EVENT_IDX];
        set => m_bools[BOOLS_WAITING_FOR_JOB_RELEASE_EVENT_IDX] = value;
    }

    /// <summary>
    /// Originally created for Expedite
    /// </summary>
    internal bool JobScheduledStatusUpdated
    {
        get => m_bools[BOOLS_JOB_SCHEDULED_STATUS_UPDATED_IDX];
        set => m_bools[BOOLS_JOB_SCHEDULED_STATUS_UPDATED_IDX] = value;
    }

    /// <summary>
    /// Sim only. If this is a sub-component. Whether Priority has been adjusted based on the orders it's pegged to.
    /// </summary>
    internal bool SubComponentPrioritySet
    {
        get => m_bools[BOOLS_SUB_COMPONENT_PRIORITY_SET_IDX];
        set => m_bools[BOOLS_SUB_COMPONENT_PRIORITY_SET_IDX] = value;
    }

    /// <summary>
    /// Sim only. If this is a sub-component. Whether Hot has been adjusted based on the orders it's pegged to.
    /// </summary>
    internal bool SubComponentHotSet
    {
        get => m_bools[BOOLS_SUB_COMPONENT_HOT_SET_IDX];
        set => m_bools[BOOLS_SUB_COMPONENT_HOT_SET_IDX] = value;
    }

    /// <summary>
    /// Sim only. If this is a sub-component. Whether priority has been adjusted based on the orders it's pegged to.
    /// </summary>
    internal bool SubComponentNeedDateSet
    {
        get => m_bools[BOOLS_SUB_COMPONENT_NEED_DATE_SET_IDX];
        set => m_bools[BOOLS_SUB_COMPONENT_NEED_DATE_SET_IDX] = value;
    }

    internal bool DoNotSchedule_PlantNotIncludedInOptimize
    {
        get => m_bools[BOOLS_DO_NOT_SCHEDULE_PLANT_NOT_INCLUDED_IN_OPTIMIZE];
        private set => m_bools[BOOLS_DO_NOT_SCHEDULE_PLANT_NOT_INCLUDED_IN_OPTIMIZE] = value;
    }

    /// <summary>
    /// Marks the reason the job isn't schedulable and sets the MOs schedulable flags to false.
    /// </summary>
    internal void DoNotSchedule_SetPlantNotIncludedInOptimize()
    {
        DoNotSchedule_PlantNotIncludedInOptimize = true;
        SetMOSchedulable(false);
    }

    private bool CanSpanPlantsHandled
    {
        get => m_bools[CAN_SPAN_PLANTS_HANDLED_IDX];
        set => m_bools[CAN_SPAN_PLANTS_HANDLED_IDX] = value;
    }

    /// <summary>
    /// Marks the reason the job isn't schedulable and sets the MOs schedulable flags to false.
    /// </summary>
    internal bool DoNotSchedule_NotSatisfiable
    {
        get => m_bools[BOOLS_DO_NOT_SCHEDULE_NOT_SATISFIABLE];
        private set => m_bools[BOOLS_DO_NOT_SCHEDULE_NOT_SATISFIABLE] = value;
    }

    /// <summary>
    /// true if the job was split off from another job during the optimize and is to be scheduled during the optimize.
    /// </summary>
    internal bool SplitDuringOptimizeAndToBeScheduled
    {
        get => m_bools[SPLIT_DURING_OPTIMIZE_TO_BE_SCHEDULED_IDX];
        set => m_bools[SPLIT_DURING_OPTIMIZE_TO_BE_SCHEDULED_IDX] = value;
    }

    /// <summary>
    /// Set to true during a simulation if the job has MOs that are to be scheduled.
    /// This can't be merged with the corresponding MO property since it's possible that not all of the MOs within a job
    /// will be scheduled (some can be finished).
    /// </summary>
    internal bool ToBeScheduled
    {
        get => m_bools[BOOLS_TO_BE_SCHEDULED_IDX];
        set => m_bools[BOOLS_TO_BE_SCHEDULED_IDX] = value;
    }
    #endregion

    internal void DoNotSchedule_SetNotSatisfiable()
    {
        DoNotSchedule_NotSatisfiable = true;
        SetMOSchedulable(false);
    }

    /// <summary>
    /// Set the Schedulable flag of all the MOs in the job
    /// </summary>
    /// <param name="a_schedulable"></param>
    private void SetMOSchedulable(bool a_schedulable)
    {
        for (int moI = 0; moI < ManufacturingOrders.Count; ++moI)
        {
            ManufacturingOrders[moI].Schedulable = a_schedulable;
        }
    }

    internal bool WaitingForSimRelease
    {
        get
        {
            if (WaitingForJobReleaseEvent)
            {
                return false;
            }

            return true;
        }
    }
    /// <summary>
    /// Retrieves the scheduled resource if the operation is scheduled or the locked resource if the
    /// operation is locked onto one or the set default resource otherwise the first eligible resource is returned.
    /// </summary>
    /// <param name="a_operation"></param>
    /// <returns></returns>
    private static Resource GetResource(InternalOperation a_operation)
    {
        if (a_operation.Scheduled)
        {
            return ((ResourceOperation)a_operation).GetScheduledPrimaryResource();
        }

        Resource lockedResource = a_operation.GetFirstLockedResource();

        if (lockedResource != null)
        {
            return lockedResource;
        }

        foreach (ResourceRequirement resourceRequirementsRequirement in a_operation.ResourceRequirements.Requirements)
        {
            if (!resourceRequirementsRequirement.HasDefaultResource)
            {
                continue;
            }
            BaseResource defaultResource = resourceRequirementsRequirement.DefaultResource;

            return (Resource)defaultResource;
        }

        Resource firstEligiblePrimaryResource = (Resource)a_operation.ResourceRequirements.GetFirstEligiblePrimaryResource();

        return firstEligiblePrimaryResource;
    }

    internal void UpdateSubJobSettings(long a_simClock, InternalActivity a_dest, bool a_setSubJobHotFlags, ScenarioOptions.ESubJobNeedDateResetPoint a_setSubJobNeedDatePoint, bool a_setSubJobPriorities, out bool o_updatedSubJobNeedDates, ScenarioOptions a_scenarioOptions, IScenarioDataChanges a_dataChanges)
    {
        List<(InternalActivity, Product)> suppliers = new List<(InternalActivity, Product)>();
        foreach (MaterialRequirement materialRequirement in a_dest.Operation.MaterialRequirements)
        {
            if (materialRequirement.BuyDirect)
            {
                continue;
            }

            if (materialRequirement.NonConstraint && materialRequirement.MustUseEligLot)
            {
                //Find all MO primary products with that lot code the reset its need date
                foreach (Job job in ScenarioDetail.JobManager.JobEnumerator)
                {
                    if (job == this)
                    {
                        continue;
                    }

                    foreach (ManufacturingOrder jobManufacturingOrder in job.ManufacturingOrders)
                    {
                        for (int i = 0; i < jobManufacturingOrder.OperationCount; i++)
                        {
                            InternalOperation op = (InternalOperation)jobManufacturingOrder.OperationManager.GetByIndex(i);
                            foreach (Product product in op.Products)
                            {
                                if (string.IsNullOrEmpty(product.LotCode))
                                {
                                    continue;
                                }
                                if (materialRequirement.ContainsEligibleLot(product.LotCode))
                                {
                                    InternalActivity internalActivity = op.GetLeadActivity() ?? op.Activities.GetByIndex(0);
                                    suppliers.Add((internalActivity, product));
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (materialRequirement.SupplyingActivityAdjustments.Any())
                {
                    HashSet<BaseId> sourceIds = new HashSet<BaseId>();
                    foreach ((InternalActivity internalActivity, decimal qty, bool expired) in materialRequirement.SupplyingActivityAdjustments)
                    {

                        if (sourceIds.Contains(internalActivity.Operation.Id))
                        {
                            continue;
                        }
                        Product linkedProduct = internalActivity.Operation.Products.GetByItemId(materialRequirement.Item.Id);
                        sourceIds.Add(internalActivity.Operation.Id);
                        suppliers.Add((internalActivity, linkedProduct));
                    }
                }

            }
        }

        UpdateSubJobSettings(a_simClock, a_dest, a_setSubJobHotFlags, a_setSubJobNeedDatePoint, a_setSubJobPriorities, out o_updatedSubJobNeedDates, a_scenarioOptions, a_dataChanges, suppliers);
    }

    internal void UpdateSubJobSettings(long a_simClock, InternalActivity a_dest, bool a_setSubJobHotFlags, ScenarioOptions.ESubJobNeedDateResetPoint a_setSubJobNeedDatePoint, bool a_setSubJobPriorities, out bool o_updatedSubJobNeedDates, ScenarioOptions a_scenarioOptions, IScenarioDataChanges a_dataChanges, List<(InternalActivity, Product)> a_suppliers)
    {
        o_updatedSubJobNeedDates = false;

        HashSet<BaseId> adjustedSuppliers = new HashSet<BaseId>();

        foreach ((InternalActivity supplyingAct, Product primaryProduct) in a_suppliers)
        {
            Job supplyingJob = supplyingAct.Operation.Job;

            if (adjustedSuppliers.Contains(supplyingJob.Id))
            {
                continue;
            }

            if (a_setSubJobHotFlags)
            {
                if (supplyingJob.SubComponentHotSet)
                {
                    if (m_hot)
                    {
                        supplyingJob.Hot = m_hot;
                    }
                }
                else
                {
                    supplyingJob.Hot = m_hot;
                    supplyingJob.SubComponentHotSet = true;
                }
            }

            if (a_setSubJobNeedDatePoint != ScenarioOptions.ESubJobNeedDateResetPoint.None)
            {
                long newNeedDateTicks = 0;
                Resource sourceResource = GetResource(supplyingAct.Operation);
                ProductionInfo sourceProdInfo = supplyingAct.GetResourceProductionInfo(sourceResource);

                //Determine the material post processing based on primary product or linked product
                long materialPostProcessingTicks = primaryProduct.MaterialPostProcessingTicks;
                if (primaryProduct == supplyingAct.Operation.Products.PrimaryProduct)
                {
                    materialPostProcessingTicks = sourceProdInfo.MaterialPostProcessingSpanTicks;
                }


                if (a_dest.Operation.Scheduled)
                {
                    CapacityUsageProfile destCapacityProfile = a_dest.Batch.PrimaryResourceBlock.CapacityProfile.ProductionProfile;

                    switch (a_setSubJobNeedDatePoint)
                    {
                        case ScenarioOptions.ESubJobNeedDateResetPoint.EarlierOfJitAndScheduledStart:
                            newNeedDateTicks = Math.Min(a_dest.DbrJitStartTicks, a_dest.Operation.StartDateTime.Ticks);
                            break;
                        case ScenarioOptions.ESubJobNeedDateResetPoint.OperationJitStart:
                            newNeedDateTicks = a_dest.DbrJitStartTicks;
                            break;
                        case ScenarioOptions.ESubJobNeedDateResetPoint.OperationScheduledStart:
                            newNeedDateTicks = a_dest.Operation.StartDateTime.Ticks;
                            break;
                        case ScenarioOptions.ESubJobNeedDateResetPoint.BottleneckedOperationScheduledStart:
                            List<BaseOperation> bottleneckOperations = a_dest.Operation.ManufacturingOrder.GetBottleneckOperations();
                            newNeedDateTicks = DateTime.MaxValue.Ticks;
                            long jobRunTimeBuffer = 0;
                            foreach (BaseOperation bottleneckOperation in bottleneckOperations)
                            {
                                if (bottleneckOperation.Scheduled)
                                {
                                    if (bottleneckOperation.StartDateTime.Ticks < newNeedDateTicks && bottleneckOperation.StartDateTime > a_dest.Operation.StartDateTime)
                                    {
                                        newNeedDateTicks = bottleneckOperation.StartDateTime.Ticks;
                                        jobRunTimeBuffer = bottleneckOperation.DbrJitStartDateTicks - a_dest.Operation.DbrJitStartDateTicks;
                                    }
                                }
                            }

                            if (newNeedDateTicks == DateTime.MaxValue.Ticks)
                            {
                                //No bottlenecks, just use first op start
                                newNeedDateTicks = a_dest.Operation.StartDateTime.Ticks;
                            }
                            else
                            {
                                //Account for the time it takes to run from a_dest operation to the bottlenecked operation.
                                newNeedDateTicks -= jobRunTimeBuffer;
                            }
                            break;
                        case ScenarioOptions.ESubJobNeedDateResetPoint.ProductionOverlap:
                            foreach (Product product in supplyingAct.Operation.Products)
                            {
                                //Check for the pegged MR.
                                foreach (MaterialRequirement mr in a_dest.Operation.MaterialRequirements.StockMaterials)
                                {
                                    if (mr.Item == product.Item)
                                    {
                                        //Get the total required capacity of the supplying activity.

                                        ProductionInfo destProdInfo = a_dest.GetResourceProductionInfo(a_dest.Batch.PrimaryResource);
                                        decimal transferQty = sourceProdInfo.TransferQty; //Since we are transfering to and from, use the max transfer qty

                                        long materialNeedTicks;
                                        switch (mr.MaterialUsedTiming)
                                        {
                                            case MaterialRequirementDefs.EMaterialUsedTiming.SetupStart:
                                                materialNeedTicks = a_dest.Batch.StartTicks;
                                                break;
                                            case MaterialRequirementDefs.EMaterialUsedTiming.DuringSetup:
                                                materialNeedTicks = a_dest.Batch.SetupEndTicks; //TODO: Techinically we could overlap by transferqty averaged over setup duration
                                                break;
                                            case MaterialRequirementDefs.EMaterialUsedTiming.ProductionStart:
                                                materialNeedTicks = a_dest.Batch.SetupEndTicks;
                                                break;
                                            case MaterialRequirementDefs.EMaterialUsedTiming.ByProductionCycle:
                                                //We have to account for transfer quantities here since material may be needed earlier than the final cycle
                                                if (transferQty > 0)
                                                {
                                                    //We need to calculate the first cycle that needs the material from the last transfer
                                                    decimal transfers = Math.Floor(mr.TotalRequiredQty / transferQty);

                                                    long cycleSpan = (long)(destProdInfo.CycleSpanTicks * a_dest.Batch.PrimaryResource.CycleEfficiencyMultiplier);
                                                    materialNeedTicks = a_dest.Batch.SetupEndTicks;
                                                    if (destCapacityProfile.CapacityFound >= cycleSpan)
                                                    {
                                                        decimal cycleToMatRatio = mr.TotalRequiredQty / a_dest.Operation.RequiredFinishQty;
                                                        decimal qtyRemainingForFinalTransfer = transfers * transferQty;
                                                        decimal qtyPerCycle = destProdInfo.QtyPerCycle * cycleToMatRatio;
                                                        long remainingCycleSpan = cycleSpan;
                                                        long lastProdCycleStart = 0;
                                                        //Find the start of the destination block's final cycle
                                                        for (int i = 0; i < destCapacityProfile.Count; i++)
                                                        {
                                                            OperationCapacity capacity = destCapacityProfile[i];
                                                            if (capacity.TotalCapacityTicks >= remainingCycleSpan)
                                                            {
                                                                long remainingCapacityTicks = capacity.TotalCapacityTicks;
                                                                while (remainingCapacityTicks > 0)
                                                                {
                                                                    //Subtract cycles off of this capacity segment
                                                                    if (remainingCapacityTicks > remainingCycleSpan)
                                                                    {
                                                                        long start = Math.Max(lastProdCycleStart, capacity.StartTicks);
                                                                        lastProdCycleStart = start + (long)(remainingCycleSpan * capacity.CapacityRatio);
                                                                        remainingCapacityTicks -= remainingCycleSpan;
                                                                        remainingCycleSpan = cycleSpan;
                                                                        qtyRemainingForFinalTransfer -= qtyPerCycle;
                                                                        if (qtyRemainingForFinalTransfer <= 0)
                                                                        {
                                                                            break;
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        remainingCycleSpan -= remainingCapacityTicks;
                                                                        remainingCapacityTicks = 0;
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                remainingCycleSpan -= capacity.TotalCapacityTicks;
                                                            }

                                                            if (qtyRemainingForFinalTransfer <= 0)
                                                            {
                                                                break;
                                                            }
                                                        }

                                                        materialNeedTicks = lastProdCycleStart;
                                                    }
                                                }
                                                else
                                                {
                                                    //Do the LastCycle calculation
                                                    //TODO: Extract to method
                                                    long remainingFinalCycleCapacity2 = (long)(destProdInfo.CycleSpanTicks * a_dest.Batch.PrimaryResource.CycleEfficiencyMultiplier);
                                                    if (destCapacityProfile.CapacityFound < remainingFinalCycleCapacity2)
                                                    {
                                                        materialNeedTicks = a_dest.Batch.SetupEndTicks;
                                                    }
                                                    else
                                                    {
                                                        long lastProdCycleStart = 0;
                                                        //Find the start of the destination block's final cycle
                                                        for (int i = destCapacityProfile.Count - 1; i >= 0; i--)
                                                        {
                                                            OperationCapacity capacity = destCapacityProfile[i];
                                                            if (capacity.TotalCapacityTicks >= remainingFinalCycleCapacity2)
                                                            {
                                                                lastProdCycleStart = capacity.EndTicks - (long)(remainingFinalCycleCapacity2 * capacity.CapacityRatio);
                                                                break;
                                                            }
                                                            else
                                                            {
                                                                remainingFinalCycleCapacity2 -= capacity.TotalCapacityTicks;
                                                            }
                                                        }

                                                        materialNeedTicks = lastProdCycleStart;
                                                    }
                                                }

                                                break;
                                            case MaterialRequirementDefs.EMaterialUsedTiming.FirstAndLastProductionCycle: //TODO: For performance this could be calculated separately
                                            case MaterialRequirementDefs.EMaterialUsedTiming.LastProductionCycle:
                                                //We don't use transfer Qty here since this is the last material consumption. We don't need material any earlier
                                                long remainingFinalCycleCapacity = (long)(destProdInfo.CycleSpanTicks * a_dest.Batch.PrimaryResource.CycleEfficiencyMultiplier);
                                                if (destCapacityProfile.CapacityFound < remainingFinalCycleCapacity)
                                                {
                                                    materialNeedTicks = a_dest.Batch.SetupEndTicks;
                                                }
                                                else
                                                {
                                                    long lastProdCycleStart = 0;
                                                    //Find the start of the destination block's final cycle
                                                    for (int i = destCapacityProfile.Count - 1; i >= 0; i--)
                                                    {
                                                        OperationCapacity capacity = destCapacityProfile[i];
                                                        if (capacity.TotalCapacityTicks >= remainingFinalCycleCapacity)
                                                        {
                                                            lastProdCycleStart = capacity.EndTicks - (long)(remainingFinalCycleCapacity * capacity.CapacityRatio);
                                                            break;
                                                        }
                                                        else
                                                        {
                                                            remainingFinalCycleCapacity -= capacity.TotalCapacityTicks;
                                                        }
                                                    }

                                                    materialNeedTicks = lastProdCycleStart;
                                                }

                                                break;
                                            case MaterialRequirementDefs.EMaterialUsedTiming.PostProcessingStart:
                                                materialNeedTicks = a_dest.Batch.ProcessingEndTicks;
                                                break;
                                            case MaterialRequirementDefs.EMaterialUsedTiming.PostProcessingEnd:
                                                materialNeedTicks = a_dest.Batch.PostProcessingEndTicks;
                                                break;
                                            default:
                                                throw new ArgumentOutOfRangeException();
                                        }

                                        //Take into account DRB buffers //TODO: Test this out
                                        long bufferDuration = a_dest.GetLongestEligibleBufferTime();
                                        Resource.FindStartFromEndResult bufferOffsetStart = sourceResource.FindUnconstrainedCapacityReverse(a_simClock, materialNeedTicks, bufferDuration);
                                        //TODO: Should we use clock if it fails?
                                        if (bufferOffsetStart.Success)
                                        {
                                            //We have a valid buffer start date
                                            materialNeedTicks = bufferOffsetStart.StartTicks;
                                        }

                                        //Now we need to find the material available ticks

                                        //Calculate when the source op needs to start
                                        //TODO: This assumes all qty is going to the dest op. Update this to calculate based on qty supplying
                                        RequiredCapacity rc;

                                        RequiredSpanPlusSetup supplySetup;
                                        RequiredSpan supplyProcessingCapacitySpan;
                                        RequiredSpan supplyPostProcessingSpan;
                                        RequiredSpan supplyStorageSpan;
                                        if (!supplyingAct.Scheduled)
                                        {
                                            RequiredCapacityPlus requiredCapacity = sourceResource.CalculateTotalRequiredCapacity(a_simClock, supplyingAct, LeftNeighborSequenceValues.NullValues, false, -1, supplyingAct.ScenarioDetail.ExtensionController, RequiredCapacityPlus.s_notInit);
                                            supplySetup = requiredCapacity.SetupSpan;
                                            supplyPostProcessingSpan = requiredCapacity.PostProcessingSpan;
                                            supplyProcessingCapacitySpan = requiredCapacity.ProcessingSpan;
                                            supplyStorageSpan = requiredCapacity.StorageSpan;
                                        }
                                        else
                                        {
                                            supplySetup = supplyingAct.Batch.SetupCapacitySpan;
                                            supplyProcessingCapacitySpan = supplyingAct.Batch.ProcessingCapacitySpan;
                                            supplyPostProcessingSpan = supplyingAct.Batch.PostProcessingSpan;
                                            supplyStorageSpan = supplyingAct.Batch.StorageSpan;
                                        }


                                        switch (product.InventoryAvailableTiming)
                                        {
                                            case ProductDefs.EInventoryAvailableTimings.AtOperationRunStart:
                                                //Backcalculate using existing setup assuming that the sequence will stay the same
                                                rc = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, supplySetup, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                                                break;
                                            case ProductDefs.EInventoryAvailableTimings.AtOperationResourcePostProcessingEnd:
                                            case ProductDefs.EInventoryAvailableTimings.DuringPostProcessing:
                                                decimal supplyQty3;

                                                if (supplyingAct.Operation.Scheduled)
                                                {
                                                    supplyQty3 = product.GetSupplyQtyForMr(mr);
                                                }
                                                else
                                                {
                                                    supplyQty3 = Math.Max(mr.TotalRequiredQty, product.TotalOutputQty);
                                                }
                                                decimal ratioOfRequiredMaterial3 = supplyQty3 / product.TotalOutputQty;
                                                long postProcessingTimeSpanTicks = (long)(supplyPostProcessingSpan.TimeSpanTicks * ratioOfRequiredMaterial3);
                                                rc = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, supplySetup, supplyProcessingCapacitySpan, new RequiredSpan(postProcessingTimeSpanTicks, false), RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                                                break;
                                            case ProductDefs.EInventoryAvailableTimings.AtOperationRunEnd:
                                                rc = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, supplySetup, supplyProcessingCapacitySpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                                                break;
                                            case ProductDefs.EInventoryAvailableTimings.ByProductionCycle:
                                                //Determine capacity of cyles to make required qty.
                                                decimal supplyQty;

                                                if (supplyingAct.Operation.Scheduled)
                                                {
                                                    supplyQty = product.GetSupplyQtyForMr(mr);
                                                }
                                                else
                                                {
                                                    supplyQty = Math.Max(mr.TotalRequiredQty, product.TotalOutputQty);
                                                }

                                                if (transferQty > 0)
                                                {
                                                    if (supplyQty < transferQty)
                                                    {
                                                        supplyQty = transferQty;
                                                    }
                                                    else
                                                    {
                                                        decimal nbrOfTransfers = Math.Ceiling(supplyQty / transferQty);
                                                        supplyQty = nbrOfTransfers * transferQty;
                                                    }
                                                }

                                                decimal cycleToMatRatio = product.TotalOutputQty / supplyingAct.Operation.RequiredFinishQty;
                                                long numberOfCycles = (long)Math.Ceiling(supplyQty / (sourceProdInfo.QtyPerCycle * cycleToMatRatio));
                                                long requiredCycleCapacity = numberOfCycles * sourceProdInfo.CycleSpanTicks;

                                                rc = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, supplySetup, new RequiredSpan(requiredCycleCapacity, false), RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                                                break;
                                            case ProductDefs.EInventoryAvailableTimings.AtStorageEnd:
                                                rc = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, supplySetup, supplyProcessingCapacitySpan, supplyPostProcessingSpan, supplyStorageSpan, RequiredSpanPlusClean.s_notInit);
                                                break;
                                            case ProductDefs.EInventoryAvailableTimings.DuringStorage:
                                                decimal supplyQty2;

                                                if (supplyingAct.Operation.Scheduled)
                                                {
                                                    supplyQty2 = product.GetSupplyQtyForMr(mr);
                                                }
                                                else
                                                {
                                                    supplyQty2 = Math.Max(mr.TotalRequiredQty, product.TotalOutputQty);
                                                }
                                                decimal ratioOfRequiredMaterial = supplyQty2 / product.TotalOutputQty;
                                                long storageSpanTimeSpanTicks = (long)(supplyStorageSpan.TimeSpanTicks * ratioOfRequiredMaterial);
                                                rc = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, supplySetup, supplyProcessingCapacitySpan, supplyPostProcessingSpan, new RequiredSpan(storageSpanTimeSpanTicks, false), RequiredSpanPlusClean.s_notInit);
                                                break;
                                            default:
                                                throw new ArgumentOutOfRangeException();
                                        }

                                        Resource.FindStartFromEndResult findStartResult = sourceResource.FindCapacityReverse(a_simClock, materialNeedTicks - materialPostProcessingTicks, rc, null, supplyingAct);

                                        if (findStartResult.Success)
                                        {
                                            bool resetStartEarlier = false;
                                            long sourceStartTicks = findStartResult.StartTicks;
                                            if (sourceStartTicks > a_dest.Batch.StartTicks)
                                            {
                                                sourceStartTicks = a_dest.Batch.StartTicks;
                                                resetStartEarlier = true;
                                            }

                                            // If overlapping completely by cycles, we have one more check to do
                                            //TODO: We may also have to do this for the 'during' production stages as well
                                            if (mr.MaterialUsedTiming is MaterialRequirementDefs.EMaterialUsedTiming.ByProductionCycle or MaterialRequirementDefs.EMaterialUsedTiming.FirstAndLastProductionCycle
                                                && product.InventoryAvailableTiming == ProductDefs.EInventoryAvailableTimings.ByProductionCycle)
                                            {
                                                //We need to verify that the first production cycle is completed before the dest production starts
                                                //This calculates the need date for the first cycle/transfer to complete at the start of the dest run
                                                decimal demandMaterialPerCycleQty = a_scenarioOptions.RoundQty(mr.TotalRequiredQty / (a_dest.Operation.RequiredFinishQty / destProdInfo.QtyPerCycle));
                                                if (transferQty > demandMaterialPerCycleQty)
                                                {
                                                    demandMaterialPerCycleQty = transferQty;
                                                }

                                                //This is how much the demand operation needs on its first cycle
                                                decimal firstCycleQty = Math.Min(mr.UnIssuedQty, demandMaterialPerCycleQty);

                                                //Figure out how many cycles the supply source takes to make the required first cycle qty
                                                decimal qtyPerCycle = sourceProdInfo.QtyPerCycle;
                                                long nbrOfCycles = 1;
                                                if (qtyPerCycle < firstCycleQty)
                                                {
                                                    nbrOfCycles = (long)Math.Ceiling(firstCycleQty / qtyPerCycle);
                                                }
                                                long totalCycleTicks = nbrOfCycles * sourceProdInfo.CycleSpanTicks;

                                                long firstCycleStart;
                                                if (mr.RequireFirstTransferAtSetup)
                                                {
                                                    firstCycleStart = a_dest.Batch.StartTicks;
                                                }
                                                else
                                                {
                                                    firstCycleStart = a_dest.Batch.SetupEndTicks;
                                                }

                                                //We need to verify that the first cycle can complete before the dest starts 
                                                RequiredCapacity oneCycleCapacity = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, supplySetup, new RequiredSpan(totalCycleTicks, false), RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                                                Resource.FindStartFromEndResult startToFinishFirstCycleResult = sourceResource.FindCapacityReverse(a_simClock, firstCycleStart - materialPostProcessingTicks, oneCycleCapacity, null, supplyingAct);
                                                if (startToFinishFirstCycleResult.Success && startToFinishFirstCycleResult.StartTicks < sourceStartTicks)
                                                {
                                                    sourceStartTicks = startToFinishFirstCycleResult.StartTicks;
                                                }
                                            }

                                            RequiredCapacity fullSourceRc = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, supplySetup, supplyProcessingCapacitySpan, supplyPostProcessingSpan, supplyStorageSpan, RequiredSpanPlusClean.s_notInit);
                                            FindCapacityResult sourceEndDateResult = sourceResource.FindFullCapacity(sourceStartTicks, fullSourceRc, supplyingAct.Operation.CanPause, null, supplyingAct, false);

                                            if (sourceEndDateResult.ResultStatus == SchedulableSuccessFailureEnum.Success)
                                            {
                                                if (newNeedDateTicks == 0 || sourceEndDateResult.FinishDate < newNeedDateTicks)
                                                {
                                                    newNeedDateTicks = sourceEndDateResult.FinishDate;
                                                }
                                            }

                                            //if (resetStartEarlier)
                                            {
                                                // We moved the source start date earlier because it can't start after the dest.
                                                // However this date will be moved back again by the material post processing, so see if we can add that post processing to the need date
                                                newNeedDateTicks += materialPostProcessingTicks;
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                }

                if (newNeedDateTicks == 0)
                {
                    //Use a default
                    newNeedDateTicks = a_dest.DbrJitStartTicks;
                }

                bool recalcJit = false;
                if (supplyingJob.SubComponentNeedDateSet)
                {
                    if (newNeedDateTicks < supplyingJob.NeedDateTicks)
                    {
                        supplyingJob.NeedDateTicks = newNeedDateTicks;
                        recalcJit = true;
                        a_dataChanges.FlagVisualChanges(supplyingJob.Id);
                    }
                }
                else
                {
                    supplyingJob.SubComponentNeedDateSet = true;
                    if (supplyingJob.NeedDateTicks != newNeedDateTicks)
                    {
                        supplyingJob.NeedDateTicks = newNeedDateTicks;
                        recalcJit = true;
                        a_dataChanges.FlagVisualChanges(supplyingJob.Id);
                    }
                }

                //Also update the MO's need date since buffers are based on MO need date
                recalcJit |= supplyingAct.Operation.ManufacturingOrder.UpdateSubMoNeedDate(newNeedDateTicks);

                if (recalcJit)
                {
                    o_updatedSubJobNeedDates = true;
                    supplyingJob.CalculateJitTimes(a_simClock, false);
                    adjustedSuppliers.Add(supplyingJob.Id);
                }
            }

            if (a_setSubJobPriorities)
            {
                if (supplyingJob.SubComponentPrioritySet)
                {
                    if (Priority < supplyingJob.Priority)
                    {
                        supplyingJob.Priority = Priority;
                        a_dataChanges.FlagVisualChanges(supplyingJob.Id);
                    }
                }
                else
                {
                    supplyingJob.Priority = Priority;
                    supplyingJob.SubComponentPrioritySet = true;
                    a_dataChanges.FlagVisualChanges(supplyingJob.Id);
                }
            }
        }
    }

    /// <summary>
    /// Setup state variables for simulation.
    /// </summary>
    internal void SimulationInitialization(PlantManager a_plantManager, ProductRuleManager a_productRuleManager, ExtensionController a_extensionController, ICalculatedValueCacheManager a_cacheManager)
    {
        m_bools.Clear();
        m_moManager.SimulationInitialization(a_plantManager, a_productRuleManager, a_extensionController, a_cacheManager);
    }

    internal void PostSimulationInitialization()
    {
        m_moManager.PostSimulationInitialization();
    }

    /// <summary>
    /// More simulation initialization that occurs after simulation state variable initialization.
    /// </summary>
    internal void ResetSimulationStateVariables(ScenarioDetail a_sd)
    {
        a_sd.InitializeCache(ref m_cachedLatestActivity);
        a_sd.InitializeCache(ref m_scheduledEndDateCache);

        for (int moIdx = 0; moIdx < m_moManager.Count; moIdx++)
        {
            ManufacturingOrder mo = m_moManager.GetByIndex(moIdx);
            mo.ResetSimulationStateVariables(a_sd);
        }
    }

    /// <summary>
    /// More simulation initialization that occurs after simulation state variable initialization.
    /// </summary>
    internal void ResetSimulationStateVariables2()
    {
        for (int moIdx = 0; moIdx < m_moManager.Count; moIdx++)
        {
            ManufacturingOrder mo = m_moManager.GetByIndex(moIdx);
            mo.ResetSimulationStateVariables2();
        }
    }

    /// <summary>
    /// Returns the stage of the first operation.
    /// This assumes the routing is linear.
    /// </summary>
    /// <param name="a_defaultStage">
    /// If a stage can't be found a_defaultStage is returned; this may mean there aren't eligible resources able to process the operation or it's finished or omitted (in which
    /// case eligibility isn't calculated).
    /// </param>
    /// <returns>
    /// The earliest stage number. If a stage can't be found a_defaultStage is returned; this may mean there aren't eligible resources able to process the operation or it's finished or omitted (in
    /// which case eligibility isn't calculated).
    /// </returns>
    internal int GetFirstStageToScheduleIn(int a_defaultStage)
    {
        int minStage = a_defaultStage;
        for (int i = 0; i < ManufacturingOrders.Count; ++i)
        {
            int stage = ManufacturingOrders[i].GetFirstStageToScheduleIn(a_defaultStage);
            minStage = Math.Min(minStage, stage);
        }

        return minStage;
    }

    /// <summary>
    /// This is called after Simulation only if a PostSimStageCust is loaded.
    /// </summary>
    /// <param name="a_simType"></param>
    /// <param name="a_t"></param>
    /// <param name="a_sd"></param>
    internal ManufacturingOrder.PostSimStageChangeTypes PostSimStageCust(long a_simClock, ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd, int a_curSimStageIdx, int a_finalSimStageIdx)
    {
        ManufacturingOrder.PostSimStageChangeTypes moChangeTypes = ManufacturingOrder.PostSimStageChangeTypes.None;
        bool computeEligibility = false;
        for (int moIdx = 0; moIdx < m_moManager.Count; moIdx++)
        {
            ManufacturingOrder.PostSimStageChangeTypes moChanges = ManufacturingOrder.PostSimStageChangeTypes.None;
            ManufacturingOrder mo = m_moManager.GetByIndex(moIdx);
            bool computeEligibilityTemp;
            moChanges = mo.PostSimStageCust(a_simClock, a_simType, a_t, a_sd, a_curSimStageIdx, a_finalSimStageIdx, out computeEligibilityTemp);
            computeEligibility = computeEligibility || computeEligibilityTemp;
            if (moChanges != ManufacturingOrder.PostSimStageChangeTypes.None)
            {
                moChangeTypes |= moChangeTypes;
            }
        }

        if (computeEligibility)
        {
            ComputeEligibility(a_sd.ProductRuleManager);
        }

        return moChangeTypes;
    }

    /// <summary>
    /// This is called after Simulation only if a PostSimStageCust is loaded.
    /// </summary>
    /// <param name="a_simType"></param>
    /// <param name="a_t"></param>
    /// <param name="a_sd"></param>
    internal ManufacturingOrder.PostSimStageChangeTypes EndOfSimulationCustExecute(long a_simClock, ScenarioDetail.SimulationType a_simType, ScenarioBaseT a_t, ScenarioDetail a_sd)
    {
        ManufacturingOrder.PostSimStageChangeTypes moChangeTypes = ManufacturingOrder.PostSimStageChangeTypes.None;
        bool computeEligibility = false;
        for (int moIdx = 0; moIdx < m_moManager.Count; moIdx++)
        {
            ManufacturingOrder.PostSimStageChangeTypes moChanges = ManufacturingOrder.PostSimStageChangeTypes.None;
            ManufacturingOrder mo = m_moManager.GetByIndex(moIdx);
            moChanges = mo.EndOfSimulationCustExecute(a_simClock, a_simType, a_t, a_sd, out bool computeEligibilityTemp);
            computeEligibility = computeEligibility || computeEligibilityTemp;
            if (moChanges != ManufacturingOrder.PostSimStageChangeTypes.None)
            {
                moChangeTypes |= moChanges;
            }
        }

        if (computeEligibility)
        {
            ComputeEligibility(a_sd.ProductRuleManager);
        }

        return moChangeTypes;
    }

    //internal long GetNbrOfActivitiesToSchedule()
    //{
    //    long nbrOfActivities = 0;

    //    for (int i = 0; i < ManufacturingOrders.Count; ++i)
    //    {
    //        nbrOfActivities+=ManufacturingOrders[i].GetNbrOfActivitiesToSchedule();
    //    }

    //    return nbrOfActivities;
    //}
    #endregion

    #region General Scheduling: Unschedule, scheduled status, etc...
    /// <summary>
    /// Don't use this version if the Job is being deleted.
    /// </summary>
    internal void Unschedule()
    {
        Unschedule(false);
    }

    internal void Exclude()
    {
        Unschedule();
        ScheduledStatus_set = JobDefs.scheduledStatuses.Excluded;
    }

    /// <summary>
    /// Unschedule every MO in this Job.
    /// </summary>
    /// <param name="deleteing">Indicate whether the job is being deleted.</param>
    public void Unschedule(bool a_deleting, UnscheduleHelper a_removalList = null)
    {
        for (int i = 0; i < ManufacturingOrders.Count; ++i)
        {
            ManufacturingOrder mo = ManufacturingOrders[i];

            ManufacturingOrder.UnscheduleType unscheduleType;
            if (a_deleting)
            {
                unscheduleType = ManufacturingOrder.UnscheduleType.Deletion;
            }
            else
            {
                unscheduleType = ManufacturingOrder.UnscheduleType.Normal;
            }

            mo.Unschedule(unscheduleType, true, a_removalList);
        }

        if (ScheduledStatus != JobDefs.scheduledStatuses.New)
        {
            ScheduledStatus_set = JobDefs.scheduledStatuses.Unscheduled;
        }
    }

    /// <summary>
    /// Notify a job that one of its MOs has been unscheduled.
    /// </summary>
    /// <param name="mo"></param>
    internal void UnscheduledMONotification(ManufacturingOrder a_mo)
    {
        UpdateScheduledStatus();
    }

    /// <summary>
    /// Update the scheduledStatus field. The update is based on counting the number of scheduled MOs.
    /// </summary>
    internal void UpdateScheduledStatus()
    {
        if (ScheduledStatus == JobDefs.scheduledStatuses.Template)
        {
            // Do nothing.
        }
        else
        {
            int finishedCount = 0;
            int scheduledCount = 0;
            int unscheduledCount = 0;

            for (int i = 0; i < ManufacturingOrders.Count; ++i)
            {
                ManufacturingOrder mo = ManufacturingOrders[i];
                if (mo.Finished)
                {
                    ++finishedCount;
                }
                else if (mo.Scheduled)
                {
                    ++scheduledCount;
                }
                else
                {
                    ++unscheduledCount;
                }
            }

            int unfinishedMOsCount = ManufacturingOrders.Count - finishedCount;

            if (ScheduledStatus == JobDefs.scheduledStatuses.New && unscheduledCount == ManufacturingOrders.Count)
            {
                // Do nothing the Job is still new.
            }
            else if (finishedCount == ManufacturingOrders.Count)
            {
                ScheduledStatus_set = JobDefs.scheduledStatuses.Finished;
                ExcluedReasons_SetNotExcluded();
            }
            else if (unscheduledCount == unfinishedMOsCount)
            {
                ScheduledStatus_set = JobDefs.scheduledStatuses.Unscheduled;
            }
            else if (scheduledCount == unfinishedMOsCount)
            {
                ScheduledStatus_set = JobDefs.scheduledStatuses.Scheduled;
                ExcluedReasons_SetNotExcluded();
            }
            else if (scheduledCount < unfinishedMOsCount)
            {
                ScheduledStatus_set = JobDefs.scheduledStatuses.PartiallyScheduled;
                ExcluedReasons_SetNotExcluded();
            }
        }
    }

    /// <summary>
    /// Whether the Job is currently scheduled.  ONLY Use for Simulation. Only means it doesn't need to be scheduled.  If all operations are Finished this is TRUE.
    /// </summary>
    internal bool Scheduled
    {
        get
        {
            int scheduledCount = 0;

            for (int i = 0; i < ManufacturingOrders.Count; ++i)
            {
                ManufacturingOrder mo = ManufacturingOrders[i];
                bool scheduled = mo.Scheduled;
                if (scheduled)
                {
                    ++scheduledCount;
                }

                if (!scheduled && !mo.IsFinishedOrOmitted)
                {
                    return false;
                }
            }

            return scheduledCount > 0;
        }
    }
    #endregion

    /// <summary>
    /// Returns false if any ManufacturingOrder's Schedulable flag is false.
    /// </summary>
    /// <returns></returns>
    internal bool IsSchedulable()
    {
        for (int i = 0; i < ManufacturingOrders.Count; ++i)
        {
            ManufacturingOrder mo = ManufacturingOrders[i];
            if (!mo.Finished && !mo.Schedulable)
            {
                return false;
            }
        }

        return true;
    }

    #region Debugging
    public override string ToString()
    {
        string str = string.Format("Job: Name '{0}';  ExternalId '{1}';", Name, ExternalId);
#if DEBUG
        str = str + string.Format("  Id={0};", Id);
#endif
        return str;
    }
    #endregion

    #region Job Copying
    /// <summary>
    /// After you create a copy of a job you will need to call this function to complete its initialization.
    /// Call this function after you have completed making any adjustments to the copy. It performs initializations
    /// such as eligibility computation and JIT time calculations.
    /// </summary>
    internal void InitGeneratedJob(long a_simClock, ProductRuleManager a_productRuleManager)
    {
        ComputeEligibility(a_productRuleManager);
        CalculateJitTimes(a_simClock, false);
    }
    #endregion

    #region Quantity Adjustments
    /// <summary>
    /// Add a new MO to this job by splitting off a specified ratio of the source Manufacturing Order.
    /// </summary>
    /// <param name="a_moId">This must exist in the Job or an error will occur.</param>
    /// <param name="a_splitOffRatio">Create a new MO by splitting off the specified ratio of the source MO.</param>
    internal ManufacturingOrder SplitOffFractionIntoAnotherMOWithinTheSameJob(long a_simClock, BaseId a_moId, decimal a_splitOffRatio, ProductRuleManager a_productRuleManager)
    {
        ManufacturingOrder newMo = m_moManager.SplitOffRatioInNewMO(a_moId, a_splitOffRatio, a_productRuleManager);
        InitGeneratedJob(a_simClock, a_productRuleManager);
        return newMo;
    }

    /// <summary>
    /// Add a new MO to this job by splitting off a specified ratio of the source Manufacturing Order. This one will not unschedule operations ineligible operations after the split.
    /// </summary>
    /// <param name="a_sourceMO">The source MO whose quantity will be reduced.</param>
    /// <param name="a_splitOffRatio">Create a new MO by splitting off the specified ratio of the source MO.</param>
    /// <returns></returns>
    internal ManufacturingOrder SplitOffFractionIntoAnotherMOWithinTheSameJobForExtension(long a_simClock, ManufacturingOrder a_sourceMO, decimal a_splitOffRatio, ProductRuleManager a_productRuleManager)
    {
        ManufacturingOrder newMo = m_moManager.SplitOffRatioInNewMO(a_sourceMO, a_splitOffRatio, false, a_productRuleManager);
        InitGeneratedJob(a_simClock, a_productRuleManager);
        return newMo;
    }

    /// <summary>
    /// Set the Required Quantity and adjust other quantities proportinally to this adjustment.
    /// </summary>
    /// <param name="a_mo">The ManufacturingOrder must be a part of this job.</param>
    /// <param name="a_newRequiredQty">What the new Required Quantity should be set to.</param>
    internal void AdjustRequiredQty(long a_simClock, ManufacturingOrder a_mo, decimal a_newRequiredQty, ProductRuleManager a_productRuleManager)
    {
        decimal changeRatio = ScenarioDetail.ScenarioOptions.RoundQty(a_newRequiredQty / a_mo.RequiredQty);
        a_mo.AdjustRequiredQtyByRatioHelper(changeRatio, a_productRuleManager);
        InitGeneratedJob(a_simClock, a_productRuleManager);
    }

    internal void HandleScenarioDetailChangeMOQtyT(ScenarioDetailChangeMOQtyT a_t, ProductRuleManager a_productRuleManager)
    {
        m_moManager.HandleScenarioDetailChangeMOQtyT(a_t, a_productRuleManager);
    }
    #endregion

    internal void ActivityScheduled(InternalActivity a_act)
    {
        if (!CanSpanPlants && !CanSpanPlantsHandled)
        {
            CanSpanPlantsHandled = true;

            for (int moI = 0; moI < ManufacturingOrders.Count; ++moI)
            {
                ManufacturingOrders[moI].FilterForCanSpanPlants(a_act);
            }
        }
    }

    /// <summary>
    /// Lock and/or Anchor activities that start before a time.
    /// </summary>
    internal void LockAndAnchorBefore(OptimizeSettings.ETimePoints a_startSpan, ScenarioOptions a_scenarioOptions)
    {
        for (int i = 0; i < ManufacturingOrders.Count; i++)
        {
            ManufacturingOrders[i].LockAndAnchorBefore(a_startSpan, a_scenarioOptions);
        }
    }

    public IEnumerator<ManufacturingOrder> GetEnumerator()
    {
        return ((IEnumerable<ManufacturingOrder>)ManufacturingOrders).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable<ManufacturingOrder>)ManufacturingOrders).GetEnumerator();
    }
}