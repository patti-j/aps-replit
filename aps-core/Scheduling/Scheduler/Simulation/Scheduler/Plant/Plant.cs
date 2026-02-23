using PT.APSCommon;
using PT.Common.Collections;
using PT.Common.Debugging;
using PT.Common.Extensions;
using PT.Common.PTMath;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Demand;
using PT.SchedulerDefinitions;

using static PT.SchedulerDefinitions.MainResourceDefs;

namespace PT.Scheduler;

/// <summary>
/// A plant defines a set of Resources.  Many viewing and scheduling
/// options relate to whether Resources are in the same Plant, such
/// as whether Jobs are permitted to span Plants.
/// </summary>
public partial class Plant
{
    internal void ResetSimulationStateVariables(long a_clock, ScenarioOptions a_so)
    {
        for (int departmentI = 0; departmentI < Departments.Count; departmentI++)
        {
            Departments[departmentI].ResetSimulationStateVariables(a_clock, a_so);
        }

        SetupCompatibleResources();
        AllowedHelperManager.ResetSimulationStateVariables();
    }

    /// <summary>
    /// This is called after Simulation only if a PostSimStageCust is loaded.
    /// </summary>
    /// <param name="a_simType"></param>
    /// <param name="a_t"></param>
    /// <param name="a_sd"></param>
    internal void PostSimStageCustExecute(ScenarioDetail.SimulationType a_simType, Transmissions.ScenarioBaseT a_t, ScenarioDetail a_sd, int a_curSimStageIdx, int a_finalSimStageIdx)
    {
        for (int departmentI = 0; departmentI < Departments.Count; departmentI++)
        {
            Departments[departmentI].PostSimStageCustExecute(a_simType, a_t, a_sd, a_curSimStageIdx, a_finalSimStageIdx);
        }
    }

    /// <summary>
    /// This is called after Simulation only if a EndOfSimulationCustomization is loaded.
    /// </summary>
    /// <param name="a_simType"></param>
    /// <param name="a_t"></param>
    /// <param name="a_sd"></param>
    internal void EndofSimulationCustExecute(ScenarioDetail.SimulationType a_simType, Transmissions.ScenarioBaseT a_t, ScenarioDetail a_sd)
    {
        for (int departmentI = 0; departmentI < Departments.Count; departmentI++)
        {
            Departments[departmentI].EndOfSimulationCustExecute(a_simType, a_t, a_sd);
        }
    }

    /// <summary>
    /// For each Inventory MR verify that some combination of the warehouses are able to satisfy the remaining quantity requirement.
    /// </summary>
    /// <param name="a_act">The activity that may need material.</param>
    /// <param name="a_primaryResource"></param>
    /// <param name="a_si"></param>
    /// <param name="a_simClock"></param>
    /// <param name="a_clock"></param>
    /// <param name="a_sd"></param>
    /// <returns>true if the Inventory MRs can be satisfied by the warehouses eligible to supply this plant.</returns>
    internal FindMaterialResult FindAvailableMaterial(InternalActivity a_act,
                                                      Resource a_primaryResource,
                                                      SchedulableInfo a_si,
                                                      long a_simClock,
                                                      long a_clock,
                                                      ScenarioDetail a_sd,
                                                      bool a_originatesFromImport)
    {
        a_act.Operation.MaterialRequirements.IssuedQtyAllocationInitialization();

        FindMaterialResult successResult = new ();

        if (a_si.m_requiredProcessingSpan.Overrun || a_si.m_requiredNumberOfCycles <= 0 || a_si.m_requiredQty <= 0)
        {
            // Since the activity is in process or has been processed presume the material was available and consumed.
            // It should also be IssuedComplete, but there's nothing enforcing this, so it's possible this should be changed 
            // to attempt to allocate material, but if it can't the activity must still appear at the start of the schedule since
            // it's already running or in post processing.
            return successResult;
        }

        bool inPlanningHorizon = a_simClock < a_sd.GetPlanningHorizonEndTicks();

        foreach (MaterialRequirement mr in a_act.Operation.MaterialRequirements)
        {
            if (mr.BuyDirect)
            {
                // No inventory planning is done for BuyDirect requirements.
                continue;
            }

            Resource.AllocationResult allocResult = null;

            //******************************************************
            // Create the demand profile and checks basic properties
            // Verify issued qty
            // Create the profile nodes
            //*****************************************************
            MaterialDemandProfile materialDemand = new MaterialDemandProfile(mr, a_act, a_sd);

            if (materialDemand.Satisfied)
            {
                successResult.Add(materialDemand);
                continue;
            }

            //Create the demand profile for this material requirement
            materialDemand.GenerateProfile(a_sd, a_si, a_primaryResource);

            BaseId mrItemId = mr.Item.Id;

            //******************************************************
            // Find potential storage areas based on MR requirements
            // and StorageAreaConnectors
            //*****************************************************
            List<MaterialAllocationPlan> potentialAllocationPlans = new ();
            MaterialAllocationPlan unconstrainedPlan = new ();

            long flowRetryTime = -1;
            if (mr.Warehouse != null)
            {
                Inventory inv = mr.Warehouse.Inventories[mrItemId];

                if (inv == null)
                {
                    #if DEBUG
                    throw new DebugException("Material Requirement specifies a warehouse, but that warehouse cannot store the item. This should have been caught during validation");
                    #endif
                    continue;
                }

                if (mr.StorageArea != null)
                {
                    ItemStorage itemStorage = mr.StorageArea.Storage[mrItemId];
                    if (itemStorage == null)
                    {
                        //The specified storage area cannot store this item. This is bad data
                        #if DEBUG
                        throw new DebugException("Material Requirement specifies a storage area, but that storage area cannot store the item. This should have been caught during validation");
                        #endif
                        continue;
                    }

                    AddPlanForStorageArea(a_sd.Clock, a_act, mr.Warehouse, mr.Item, mr.StorageArea, a_primaryResource, materialDemand, a_si, ref potentialAllocationPlans, ref unconstrainedPlan, out long saFlowRetry);
                    if (saFlowRetry != long.MaxValue)
                    {
                        flowRetryTime = saFlowRetry;
                    }
                }
                else
                {
                    AddPlanForStorageArea(a_sd.Clock, a_act, mr.Warehouse, mr.Item, null, a_primaryResource, materialDemand, a_si, ref potentialAllocationPlans, ref unconstrainedPlan, out long saFlowRetry);
                    if (saFlowRetry != long.MaxValue)
                    {
                        flowRetryTime = saFlowRetry;
                    }
                }
            }
            else
            {
                // Create a single plan for all SAs
                foreach (Warehouse warehouse in Warehouses)
                {
                    AddPlanForStorageArea(a_sd.Clock, a_act, warehouse, mr.Item, null, a_primaryResource, materialDemand, a_si, ref potentialAllocationPlans, ref unconstrainedPlan, out long saFlowRetry);
                    if (saFlowRetry != long.MaxValue)
                    {
                        flowRetryTime = saFlowRetry;
                    }

                    if (!materialDemand.AllowMultiWarehouseSupply && !unconstrainedPlan.Empty)
                    {
                        // We can't use all warehouses at the same time, so add the current SAs and reset for the next warehouse
                        potentialAllocationPlans.Add(unconstrainedPlan);
                        unconstrainedPlan = new MaterialAllocationPlan();
                    }
                }
            }

            potentialAllocationPlans.Add(unconstrainedPlan);

            //******************************************************
            // The potential storage areas now contain all usable storage
            // We need to determine the order to use to satisfy the demand 
            // 1. Filter out storage areas that cannot supply qty
            // 2. Sort the SAs based on material allocation direction and qty node available timing
            // 3. Once sorted, merge the SAs profile one at a time until there is enough supply
            // TODO: determine algorithm as there may be multiple combinations, which is best?
            // TODO: Track lead time by storage area
            // TODO: Return success result vs allocations
            //*****************************************************

            // Verify and remove any invalid plans
            // If all plans are invalid, use the first available date to try again
            List<MaterialAllocationPlan> availablePlans = potentialAllocationPlans.Where(p => p.RetryDate == 0).ToList();
            if (availablePlans.Count != potentialAllocationPlans.Count)
            {
                if (potentialAllocationPlans.Where(p => p.RetryDate > 0).MinBy(p => p.RetryDate) is MaterialAllocationPlan retryPlan)
                {
                    flowRetryTime = flowRetryTime > 0 ? Math.Min(flowRetryTime, retryPlan.RetryDate) : retryPlan.RetryDate;
                }

                if (availablePlans.Count == 0)
                {
                    //All invalid
                    return new FindMaterialResult(mr, flowRetryTime);
                }
            }

            // Filter out storage areas that don't have any available qty
            foreach (MaterialAllocationPlan plan in availablePlans)
            {
                plan.OrderStorageAreas(materialDemand, a_sd);
            }

            if (materialDemand.CanUseLeadTime)
            {
                if (availablePlans.Count == 0)
                {
                    //Create an empty plan that doesn't have any SAs, but can use leadtime
                    availablePlans.Add(new MaterialAllocationPlan());
                }
            }
            else if(!a_originatesFromImport)
            {
                if (inPlanningHorizon && availablePlans.Max(p => p.MaxAvailableQty) < materialDemand.RemainingQty)
                {
                    //There is no way to find enough material
                    return CalculatePotentialMaterialAvailableDate(a_sd, flowRetryTime, availablePlans, materialDemand, a_si, a_primaryResource, a_act);
                }
            }

            //Order plans by most available qty
            availablePlans = availablePlans.Where(p => !p.Empty).OrderByDescending(p => p.HasExactSupplySource).ThenByDescending(p => p.MaxAvailableQty).ToList();

            //Find the first available Plan that can satisfy the requirement
            MaterialAllocationPlan successfulPlan = null;
            foreach (MaterialAllocationPlan plan in availablePlans)
            {
                //Now that we will determine actual allocation, reset the profile.
                materialDemand.ResetForAllocation();
                materialDemand.CacheItemStorageSources(plan.AvailableStorage);
                successfulPlan = plan;

                plan.AllocatePlan(materialDemand, a_sd);

                //Check if this plan was sufficient
                if (materialDemand.Satisfied)
                {
                    break;
                }

                //We have no way to source enough material
                if (!materialDemand.CanUseLeadTime)
                {
                    continue; //Try another plan
                }

                //There is not enough on-hand and expected sources. Now check lead-time to satisfy the remainder
                if (mr.Warehouse != null)
                {
                    Inventory inv = mr.Warehouse.Inventories[mrItemId];
                    //TODO: Extensions. Add a lead time extension point here to customize leadtime by mr/item/res
                    //Find the first unallocated node
                    materialDemand.AllocateRemainingDemandFromLeadTime(inv, a_sd.Clock + inv.LeadTimeTicks);

                    if (!materialDemand.Satisfied)
                    {
                        materialDemand.AllocateRemainingDemandFromPastPlanningHorizonIfPossible(inv, a_sd.EndOfPlanningHorizon);
                    }
                }
                else
                {
                    List<Inventory> orderedLeadTimeSources = new();
                    //Order resources by leadtime availability starting with warehouses that have already been used for allocation
                    foreach (Inventory inv in materialDemand.AllocatedInventories)
                    {
                        orderedLeadTimeSources.Add(inv);
                    }

                    orderedLeadTimeSources.Sort((i1, i2) => i1.LeadTimeTicks.CompareTo(i2.LeadTimeTicks));

                    Inventory pastPlanningHorizonSource = null;
                    foreach (Inventory invSource in orderedLeadTimeSources)
                    {
                        pastPlanningHorizonSource = invSource;
                        materialDemand.AllocateRemainingDemandFromLeadTime(invSource, a_sd.Clock + invSource.LeadTimeTicks);
                        if (materialDemand.Satisfied)
                        {
                            //These warehouses were enough
                            break;
                        }
                    }

                    if (materialDemand.Satisfied)
                    {
                        //Lead time was enough
                        break;
                    }

                    //Existing sources (if any) did not have lead time early enough to satisfy the requirement
                    //Use all available warehouses
                    List<Inventory> unsourcedSources = new(Warehouses.Count);
                    foreach (Warehouse warehouse in Warehouses)
                    {
                        Inventory inv = warehouse.Inventories[mrItemId];
                        if (inv != null)
                        {
                            if (!orderedLeadTimeSources.Contains(inv))
                            {
                                unsourcedSources.Add(inv);
                            }
                        }
                    }

                    unsourcedSources.Sort((i1, i2) => i1.LeadTimeTicks.CompareTo(i2.LeadTimeTicks));

                    foreach (Inventory invSource in unsourcedSources)
                    {
                        if (pastPlanningHorizonSource == null)
                        {
                            pastPlanningHorizonSource = invSource;
                        }
                        materialDemand.AllocateRemainingDemandFromLeadTime(invSource, a_sd.Clock + invSource.LeadTimeTicks);
                        if (materialDemand.Satisfied)
                        {
                            //These warehouses were enough
                            break;
                        }
                    }

                    if (!materialDemand.Satisfied && materialDemand.Last.Date >= a_sd.EndOfPlanningHorizon)
                    {
                        materialDemand.AllocateRemainingDemandFromPastPlanningHorizonIfPossible(pastPlanningHorizonSource, a_sd.EndOfPlanningHorizon);
                    }
                }

                if (materialDemand.Satisfied)
                {
                    //Lead time was enough
                    break;
                }
                

                //TODO: If non-constraint we always use the first plan. Should we try another plan in case it has a different leadtime?
                //Check the result of lead time allocation
                if (materialDemand.NonConstraint || a_sd.ScenarioOptions.RestoreMaterialConstraintsOnOptimizedActivities)
                {
                    //All sources exhausted
                    materialDemand.AllocateShortage(Warehouses);

                    //If this is coming from an import, we want to set the Constraint types of any constrained MR to Ignored. This is undone in InitJobForOptimize.
                    //This might be unnecessary here.
                    if (a_originatesFromImport)
                    {
                        if (materialDemand.MR.ConstraintType == MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate)
                        {
                            materialDemand.MR.ConstraintType = MaterialRequirementDefs.constraintTypes.IgnoredConstrainedByAvailableDate;
                        }
                        else if (materialDemand.MR.ConstraintType == MaterialRequirementDefs.constraintTypes.ConstrainedByEarlierOfLeadTimeOrAvailableDate)
                        {
                            materialDemand.MR.ConstraintType = MaterialRequirementDefs.constraintTypes.IgnoredConstrainedByEarlierOfLeadTimeOrAvailableDate;
                        }
                    }

                    break;
                }
            }

            //Final check, return response
            if (materialDemand.Satisfied || a_originatesFromImport)
            {
                successResult.Add(materialDemand);
                successfulPlan.Connector?.FlowRangeConstraint.AllocateUsage(materialDemand.GetUsageRange(a_si));
                materialDemand.Connector = successfulPlan.Connector;
                materialDemand.AllocateLastStorageAreaNode(); //This will complete the usage on the last SA.

                //If this is coming from an import, we want to set the Constraint types of any constrained MR to Ignored. This is undone in InitJobForOptimize.
                if (!materialDemand.Satisfied && a_originatesFromImport)
                {
                    if (materialDemand.MR.ConstraintType == MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate)
                    {
                        materialDemand.MR.ConstraintType = MaterialRequirementDefs.constraintTypes.IgnoredConstrainedByAvailableDate;
                    }
                    else if (materialDemand.MR.ConstraintType == MaterialRequirementDefs.constraintTypes.ConstrainedByEarlierOfLeadTimeOrAvailableDate)
                    {
                        materialDemand.MR.ConstraintType = MaterialRequirementDefs.constraintTypes.IgnoredConstrainedByEarlierOfLeadTimeOrAvailableDate;
                    }

                    materialDemand.AllocateShortage(Warehouses);
                }
            }
            else
            {
                if (!inPlanningHorizon)
                {
                    //No sources
                    materialDemand.AllocateShortage(Warehouses);
                    break;
                }
                
                FindMaterialResult calculatePotentialMaterialAvailableDate = CalculatePotentialMaterialAvailableDate(a_sd, flowRetryTime, availablePlans, materialDemand, a_si, a_primaryResource, a_act);
                return calculatePotentialMaterialAvailableDate;
            }
        }

        return successResult;
    }

    /// <summary>
    /// Calculates a retry date for this activity based on when the material is available and when the material is needed
    /// </summary>
    private static FindMaterialResult CalculatePotentialMaterialAvailableDate(ScenarioDetail a_sd, long a_connectorRetryDate, List<MaterialAllocationPlan> a_availablePlans, MaterialDemandProfile a_materialDemand, SchedulableInfo a_si, Resource a_primaryResource, InternalActivity a_act)
    {
        //Try to calculate when the material will be available and backcalculate from the first demand date
        long availableTicks = PTDateTime.MaxDateTimeTicks;
        BaseSupplyProfile bestSupplyProfile = null;
        foreach (MaterialAllocationPlan availablePlan in a_availablePlans)
        {
            long planAvailableDate = availablePlan.CalculateAvailableDate(a_materialDemand, a_sd, out BaseSupplyProfile supplyProfile);
            if (planAvailableDate != PTDateTime.InvalidDateTime.Ticks)
            {
                if (planAvailableDate < availableTicks)
                {
                    bestSupplyProfile = supplyProfile;
                    availableTicks = planAvailableDate;
                }
            }
        }

        if (availableTicks == PTDateTime.MaxDateTimeTicks)
        {
            //Retry at the connector date (-1 will be the default if the retry date is not set)
            return new FindMaterialResult(a_materialDemand.MR, a_connectorRetryDate);
        }

        //Now backcalculate to find the start date for the operation
        RequiredCapacity rc = null;
        switch (a_materialDemand.MR.MaterialUsedTiming)
        {
            case MaterialRequirementDefs.EMaterialUsedTiming.SetupStart:
                rc = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, RequiredSpanPlusSetup.s_notInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                break;
            case MaterialRequirementDefs.EMaterialUsedTiming.DuringSetup:
                // TODO: We need to back calculate some percentage of the production                                         
                a_materialDemand.ResetForAllocation();
                bestSupplyProfile.ResetForAllocation();
                availableTicks = EarliestDemandStart(bestSupplyProfile, a_materialDemand, availableTicks);

                rc = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, RequiredSpanPlusSetup.s_notInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                break;
            case MaterialRequirementDefs.EMaterialUsedTiming.ProductionStart:
                rc = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, a_si.m_requiredSetupSpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                break;
            case MaterialRequirementDefs.EMaterialUsedTiming.ByProductionCycle:
                // TODO: We need to back calculate some percentage of the production                                         
                a_materialDemand.ResetForAllocation();
                bestSupplyProfile.ResetForAllocation();
                availableTicks = EarliestDemandStart(bestSupplyProfile, a_materialDemand, availableTicks);

                rc = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, a_si.m_requiredSetupSpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                break;
            case MaterialRequirementDefs.EMaterialUsedTiming.FirstAndLastProductionCycle: //TODO: For performance this could be calculated separately
            case MaterialRequirementDefs.EMaterialUsedTiming.LastProductionCycle:
                a_materialDemand.ResetForAllocation();
                bestSupplyProfile.ResetForAllocation();
                if (a_materialDemand.MR.MaterialUsedTiming == MaterialRequirementDefs.EMaterialUsedTiming.FirstAndLastProductionCycle)
                {
                    //Also check the first cycle. Find when the first cycle can start
                    decimal materialPerCycle = a_sd.ScenarioOptions.RoundQty(a_materialDemand.RemainingQty / a_act.Operation.RequiredFinishQty);
                    decimal qtyAvailable = 0m;
                    foreach (QtySupplyNode node in bestSupplyProfile)
                    {
                        qtyAvailable += node.UnallocatedQty;
                        if (qtyAvailable >= materialPerCycle)
                        {
                            //We found enough
                            if (node.AvailableDate > a_sd.SimClock)
                            {
                                availableTicks = node.AvailableDate;
                                rc = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, a_si.m_requiredSetupSpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                            }
                            break;
                        }
                    }
                }

                if (rc == null) //first cycle was not a constraint
                {
                    //A special one for overlap. We need to find the capacity to backcalculate by subtracting 1 cycle from the end of the processing span
                    long cycleSpanTicks = a_act.GetResourceProductionInfo(a_primaryResource).CycleSpanTicks;
                    RequiredSpan firstCyclesSpan = new(a_si.m_requiredProcessingSpan.TimeSpanTicks - cycleSpanTicks, a_si.m_requiredProcessingSpan.Overrun);
                    rc = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, a_si.m_requiredSetupSpan, firstCyclesSpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                }

                break;
            case MaterialRequirementDefs.EMaterialUsedTiming.PostProcessingStart:
                rc = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, a_si.m_requiredSetupSpan, a_si.m_requiredProcessingSpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                break;
            case MaterialRequirementDefs.EMaterialUsedTiming.PostProcessingEnd:
                rc = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, a_si.m_requiredSetupSpan, a_si.m_requiredProcessingSpan, a_si.m_requiredPostProcessingSpan, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Resource.FindStartFromEndResult startDateResult = a_primaryResource.FindCapacityReverse(a_sd.Clock, availableTicks, rc, null, a_act);
        if (startDateResult.Success)
        {
            if (a_connectorRetryDate > -1 && a_connectorRetryDate < startDateResult.StartTicks)
            {
                //There is a connector available earlier
                return new FindMaterialResult(a_materialDemand.MR, a_connectorRetryDate);
            }

            //Try again at the expected material availability
            return new FindMaterialResult(a_materialDemand.MR, startDateResult.StartTicks);
        }

        //No material availability, try again at the first connector, if one has been set
        if (a_connectorRetryDate > -1)
        {
            //There is a connector available earlier
            return new FindMaterialResult(a_materialDemand.MR, a_connectorRetryDate);
        }

        // No material sources found
        return new FindMaterialResult(a_materialDemand.MR, -1);
    }

    /// <summary>
    /// Returns the minimal start date so that cumulative supply is never less than cumulative demand at any time.
    ///
    /// Streaming O(n+m)
    /// Throws InvalidOperationException if total supply is insufficient.
    /// </summary>
    internal static long EarliestDemandStart(BaseSupplyProfile a_supplyProfile, MaterialDemandProfile a_demandProfile, long a_defaultAvailableDate)
    {
        using var sEnum = a_supplyProfile.GetEnumerator();
        bool haveSupply = sEnum.MoveNext();

        decimal supplyTotal = 0;                  // cumulative supply seen so far
        decimal demandTotal = 0;                  // cumulative demand seen so far
        long lastSupplyTime = haveSupply ? sEnum.Current.AvailableDate : 0;
        long requiredShift = long.MinValue;
        bool sawAnyDemand = false;

        foreach (QtyDemandNode demandNode in a_demandProfile)
        {
            sawAnyDemand = true;
            demandTotal += demandNode.UnallocatedQty;

            // Advance supply until it covers this cumulative demand level
            while (haveSupply && supplyTotal < demandTotal)
            {
                supplyTotal += sEnum.Current.UnallocatedQty;
                lastSupplyTime = sEnum.Current.AvailableDate;
                haveSupply = sEnum.MoveNext();
            }

            if (supplyTotal < demandTotal)
            {
                throw new InvalidOperationException("Insufficient total supply to satisfy all demand for any start time.");
            }

            // For this prefix, S must be at least (time_supply_reaches_prefix - time_demand_reaches_prefix)
            long candidateShift = lastSupplyTime - demandNode.Date;
            if (candidateShift > requiredShift) requiredShift = candidateShift;
        }

        if (!sawAnyDemand || requiredShift < 0)
        {
            return a_defaultAvailableDate; // no demand => no shift needed
        }

        return a_demandProfile.First.Date + requiredShift;
    }


    private void AddPlanForStorageArea(long a_clockDate, InternalActivity a_act, Warehouse a_wh, Item a_item, StorageArea a_sa, Resource a_primaryResource, MaterialDemandProfile a_demandProfile, SchedulableInfo a_si, ref List<MaterialAllocationPlan> a_plans, ref MaterialAllocationPlan a_unconstrainedPlan, out long o_saFlowRetryTime)
    {
        o_saFlowRetryTime = long.MaxValue;
        StorageAreaConnector[] storageAreaConnectors = a_wh.StorageAreaConnectors.GetStorageConnectorsForResourceOut(a_primaryResource).ToArray();
        IInterval usageRange = a_demandProfile.GetUsageRange(a_si);
        if (storageAreaConnectors.Length > 0)
        {
            foreach (StorageAreaConnector connector in storageAreaConnectors)
            {
                if (a_sa != null && !connector.StorageAreaOutList.Contains(a_sa.Id))
                {
                    //Only the allowed SA is allowed in this case
                    continue;
                }

                //check if the connector is available
                //TODO: Why are we backcalculating retry date here but not for other out retry dates????????
                if (!connector.FlowRangeConstraint.VerifyAllocationRange(usageRange, out long retryDate))
                {
                    //This connector is allocated for another storage transfer and cannot be used for this one.
                    RequiredCapacity rc = a_demandProfile.GetRequiredCapacity(a_si);

                    //Backcalculate from the intersectionEndTicks
                    Resource.FindStartFromEndResult findResult = a_primaryResource.FindCapacityReverse(a_clockDate, retryDate, rc, null, a_act);
                    if (findResult.Success)
                    {
                        a_plans.Add(new MaterialAllocationPlan(connector, findResult.StartTicks)); //We will check later if this plan is feasible
                        continue;
                    }
                }

                MaterialAllocationPlan newPlan = new (connector);
                if (a_sa != null)
                {
                    //pegged SA
                    if (CheckStorageConstraints(a_sa, usageRange, ref o_saFlowRetryTime))
                    {
                        newPlan.AddStorageArea(a_sa);
                    }
                }
                else
                {
                    foreach (StorageArea storageArea in connector.StorageAreasOut)
                    {
                        if (storageArea.CanStoreItem(a_item.Id))
                        {
                            if (CheckStorageConstraints(storageArea, usageRange, ref o_saFlowRetryTime))
                            {
                                newPlan.AddStorageArea(storageArea);
                            }
                        }
                    }
                }

                if (!newPlan.Empty)
                {
                    a_plans.Add(newPlan);
                }
            }
        }
        else
        {
            //Not constrained by Connectors
            if (a_sa != null)
            {
                if (CheckStorageConstraints(a_sa, usageRange, ref o_saFlowRetryTime))
                {
                    a_unconstrainedPlan.AddStorageArea(a_sa);
                }
            }
            else
            {
                foreach (StorageArea sa in a_wh.StorageAreas)
                {
                    if (sa.CanStoreItem(a_item.Id) && CheckStorageConstraints(sa, usageRange, ref o_saFlowRetryTime))
                    {
                        a_unconstrainedPlan.AddStorageArea(sa);
                    }
                }
            }
        }
    }

    private bool CheckStorageConstraints(StorageArea a_sa, IInterval a_usageRange, ref long r_retryDate)
    {
        if (!a_sa.ValidateOutFlow(a_usageRange, out long saRetryDate))
        {
            if (saRetryDate > 0)
            {
                r_retryDate = Math.Min(saRetryDate, r_retryDate);
            }
            
            return false;
        }

        if (a_sa.Resource != null)
        {
            long storageIntervalDuration = Math.Max(1, a_usageRange.Duration);
            FindCapacityResult findCapacityResult = a_sa.Resource.FindCapacity(a_usageRange.StartTicks, storageIntervalDuration, false, null, usageEnum.Unspecified, true, false, string.Empty, false, InternalActivityDefs.peopleUsages.UseSpecifiedNbr, 1m, out bool _);
            if (findCapacityResult.ResultStatus != SchedulableSuccessFailureEnum.Success)
            {
                if (findCapacityResult.NextStartTime > 0)
                {
                    r_retryDate = Math.Min(findCapacityResult.NextStartTime, r_retryDate);
                    return false;
                }
            }
        }

        return true;
    }

    // Each element in this list is a ResourceArrayList
    private DictionaryCollection<BaseId, Resource> m_compatibleResourcesCachedCollection;

    /// <summary>
    /// Creates a cache of compatible resources by the compatibility code tables they have in common
    /// </summary>
    private void SetupCompatibleResources()
    {
        Dictionary<BaseId, HashSet<BaseId>> resourceTableCollectionCache = new ();
        List<Resource> activeResources = ActiveResourceEnumerator.ToList();

        foreach (Resource activeResource in activeResources)
        {
            HashSet<BaseId> tableIds = new ();
            foreach (CompatibilityCodeTable table in activeResource.CompatibilityTables)
            {
                tableIds.Add(table.Id);
            }

            resourceTableCollectionCache.Add(activeResource.Id, tableIds);
        }

        m_compatibleResourcesCachedCollection = new DictionaryCollection<BaseId, Resource>();
        foreach (Resource activeResource in activeResources)
        {
            foreach (CompatibilityCodeTable codeTable in activeResource.CompatibilityTables)
            {
                //Cache all other resources that are also using this table
                foreach (Resource resource in activeResources)
                {
                    if (resourceTableCollectionCache.TryGetValue(resource.Id, out HashSet<BaseId> tableIds))
                    {
                        if (tableIds.Contains(codeTable.Id))
                        {
                            m_compatibleResourcesCachedCollection.Add(activeResource.Id, resource);
                        }
                    }
                }

            }

        }
    }

    internal Resource FindResource(BaseId a_resId)
    {
        return m_departments.GetResource(a_resId);
    }

    #region outside simulation setup
    internal void AddResources(List<InternalResource> a_resources)
    {
        for (int dI = 0; dI < m_departments.Count; ++dI)
        {
            m_departments[dI].AddResources(a_resources);
        }
    }
    #endregion

    #region Diagnostics
    internal void PrintResultantCapacity()
    {
        for (int i = 0; i < DepartmentCount; ++i)
        {
            Department d = Departments[i];
            d.PrintResultantCapacity();
        }
    }
    #endregion

    internal class FindMaterialResult
    {
        private readonly long m_retryConnectorDate;
        internal FindMaterialResult() { }

        internal FindMaterialResult(MaterialRequirement a_failedSourceMr, long a_retryConnectorDate)
        {
            m_retryConnectorDate = a_retryConnectorDate;
            FirstFailedSourceItem = a_failedSourceMr.Item;
            RequiredWarehouse = a_failedSourceMr.Warehouse;
        }

        internal void SetPreviousDispatchers(IEnumerable<InternalResource> a_previousDispatchers)
        {
            PreviousDispatchers = a_previousDispatchers;
        }

        internal bool MaterialSourcesFound => FirstFailedSourceItem == null;

        internal Item? FirstFailedSourceItem;
        internal Warehouse? RequiredWarehouse;
        internal IEnumerable<InternalResource> PreviousDispatchers;

        internal List<MaterialDemandProfile> MaterialDemandProfiles;
        internal long RetryDate => m_retryConnectorDate;

        internal void Add(MaterialDemandProfile a_demandProfile)
        {
            if (MaterialDemandProfiles == null)
            {
                MaterialDemandProfiles = new ();
            }

            MaterialDemandProfiles.Add(a_demandProfile);
        }
    }
}