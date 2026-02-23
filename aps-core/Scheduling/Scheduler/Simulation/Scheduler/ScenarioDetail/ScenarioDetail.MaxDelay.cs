using System.Diagnostics;

using PT.APSCommon;
using PT.Common.Debugging;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Events;
using PT.SchedulerDefinitions;

/*
For the Max Delay feature to work, all the following must be true:

The system option "Enforce Max Delay" must be set.
The predecessor and successor operations are limited to a single activity.
The predecessor operation is limited to a single successor.
The successor operation is limited to a single predecessor. 2
All of the activities scheduled on the resource where the successor operations are scheduled must have a max delay relationship with a predecessor operation.
 *
 **** Some things I didn't check when I wrote this email were:
 **** Material Requirements
 **** Helper Resource Requirements
 **** Also not all features are compatible with this. There's an enhancement request to add additional feature support for Max Delay.
 *
 */
//----------------------------------------------------------------------------------------------------------------------------------------------------------------
///----------------------------------------------------------------------------------------------------------------------------------------------------------------
/// These are the possible Roll Line combinations at IL.
/// 1 3
/// 2 3
/// 1 2
///   3
/// 1 2 3
///----------------------------------------------------------------------------------------------------------------------------------------------------------------
///----------------------------------------------------------------------------------------------------------------------------------------------------------------
///
/// ResourceReservation
///    si
///    ia
///    MakeFirm - Add the reservation to the reservation list. si.resource.AddReservation(this).
///
/// ResourceReservationResult
///    ResourceReservation
///    nextPlausibleStartTicks
///
///----------------------------------------------------------------------------------------------------------------------------------------------------------------
/// What to do to complete resource reservations:
/// 1. Try adding multiple resource constraints. You don't need this at Gonnella because there operations that require multiple resources are the lead operations.
/// 2. When you schedule an operation, determine the resource based on any resource reservations it has.
/// 3. Clear out the resource reservations for resources as activities with reservations are scheduled.
/// 4. There's a possible performance enhancement when you backwards schedule.
///----------------------------------------------------------------------------------------------------------------------------------------------------------------
/// What to do to complete multiple resource requirements work.
/// 1. Change the call to DetermineNonPrimaryRR() in this file so that it uses the earliest availability of other resource requirements.
/// 2. Change Resource2.SchedulableSecondaryRRAvailableDate() so it returns the next available date of this resource (something greater than the current time).
/// 3. Change ScenarioDetail.DetermineNonPrimaryRR() so it can call the function SchedulableSecondaryRRAvailableDate() or Schedulable() and can return the earliest available secondary resource requirement time.
///----------------------------------------------------------------------------------------------------------------------------------------------------------------
/// In the next build there will be changes for a bug found with the HI-PAC files. 
/// Your max delay looks like it will work except
/// •	you have to change all the product resources to infinite; the primary resource must be finite and other resource requirements must be infinite with this feature
/// •	for max delay to work you can only move the first operation of the job so you need to set the resource’s “DoNotAllowDragAndDrops” for all resources except SR.
///
/// From the data it appears operations can span offline intervals, can you check on whether this is the case? In food production max delay is sometimes associated with food spoilage. It might be possible that operations. Let me know the results of this, some additions might need to be made to Max Delay. I’d also like to see the completed model when it’s done and before you present it as the final solution.
///----------------------------------------------------------------------------------------------------------------------------------------------------------------
namespace PT.Scheduler;

public partial class ScenarioDetail
{
    private readonly ResourceReservationManager m_resRvns = new ();

    private bool InsertContinuousSuccessor(AlternatePath.Association a_asn, SchedulableInfo a_si)
    {
        m_resRvns.ClearPlanned();

        // Determine whether the successor operation can be started.
        long startTicks = GetStartTimeForSucOp(a_asn, a_si);
        long attemptedStartTicks = startTicks;
        long mustStartByTicks = a_si.m_postProcessingFinishDate + a_asn.MaxDelayTicks;
        do
        {
            startTicks = InsertContinuousSuccessor(a_asn, startTicks, a_si, mustStartByTicks);
            if (attemptedStartTicks == startTicks)
            {
                break;
            }

            attemptedStartTicks = startTicks;
        } while (startTicks != 0 && startTicks < mustStartByTicks);


        if (startTicks != 0)
        {
            ResourceOperation ro = (ResourceOperation)a_asn.Predecessor.Operation;
            long possibleStartDate = DetermineNextPossibleStartDate(a_asn, ro, a_si, startTicks);

            if (possibleStartDate == a_si.m_scheduledStartDate)
            {
                //we are already attempting to schedule past this back-calculated date allow scheduling at this point.
                return false;
            }
            else if (possibleStartDate < a_si.m_scheduledStartDate)
            {
                DebugException.ThrowInDebug("We did something bad with the math");
                return false;
            }

            AddEvent(new AttemptToScheduleForResRvnEvent(possibleStartDate));
            return false;
        }

        //Don't firm the operations yet, this act might fail to schedule
        return true;
    }

    private long InsertContinuousSuccessor(AlternatePath.Association a_asn, long a_startTicks, SchedulableInfo a_predecessorSi, long a_mustStartByTicks)
    {
        bool tryingDifferentStartTime = false;
        int attemptNo = 0;

        do
        {
            ++attemptNo;
            ResourceOperation sucRO = (ResourceOperation)a_asn.Successor.Operation;

            // From the set of eligible resources find one that can schedule.
            //Resource schedRes = (Resource)sucRO.ResourceRequirements.PrimaryResourceRequirement.DefaultResource;
            ResourceReservationResult primaryRRR = InsertContinuousSuccessorRR(sucRO, a_asn, a_startTicks, a_predecessorSi, a_mustStartByTicks);

            if (primaryRRR == null)
            {
                continue;
            }

            if (!primaryRRR.Reservation)
            {
                return primaryRRR.NextPlausibleStartTicks;
            }

            SchedulableInfo si = primaryRRR.SchedulableResult.m_si;
            Resource primaryResource = si.m_resource;
            InternalActivity activity = primaryRRR.ResRv.m_ia;

            if (m_extensionController.RunAdvancedSchedulingExtension)
            {
                long? isSchedulableContinuousFlowCheck = m_extensionController.IsSchedulableContinuousFlowCheck(this, m_activeSimulationType, Clock, a_startTicks, primaryResource, sucRO, si);
                if (isSchedulableContinuousFlowCheck.HasValue)
                {
                    return isSchedulableContinuousFlowCheck.Value;
                }
            }

            // Add the primary resource reservation to the reservation list.
            m_resRvns.AddPlanned(primaryRRR.ResRv);

            ResourceSatiator[] satiators = new ResourceSatiator[sucRO.ResourceRequirements.Count];
            satiators[sucRO.ResourceRequirements.PrimaryResourceRequirementIndex] = new ResourceSatiator(primaryResource);
            //Add reservations for helpers. 
            for (int rrIndex = 0; rrIndex < sucRO.ResourceRequirements.Count; rrIndex++)
            {
                if (rrIndex != sucRO.ResourceRequirements.PrimaryResourceRequirementIndex)
                {
                    ResourceRequirement rr = sucRO.ResourceRequirements.GetByIndex(rrIndex);

                    if (si.ZeroCapacityRequired(rr))
                    {
                        //No capacity needed to reserve
                        continue;
                    }

                    //Determine start and end date to use the for the helper
                    long helperStart = si.GetUsageStart(rr);
                    long helperEnd = si.GetUsageEnd(rr);
                        
                    //Find the most likely/appropriate resource or the first eligible
                    Resource eligibleResource = null;
                    Resource.NonPrimarySchedulableResult result;
                    if (rr.HasDefaultResource)
                    {
                        eligibleResource = (Resource)rr.DefaultResource;
                        result = eligibleResource.NonPrimarySchedulable(this, activity, rr, satiators, si, null);
                    }
                    else if (activity.ResourceRequirementLock(rrIndex) is Resource lockedResource)
                    {
                        eligibleResource = lockedResource;
                        result = eligibleResource.NonPrimarySchedulable(this, activity, rr, satiators, si, null);
                    }
                    else
                    {
                        result = new Resource.NonPrimarySchedulableResult(SchedulableSuccessFailureEnum.NotSet);
                        //Find the first eligible
                        //Determine possible eligible helpers
                        EligibleResourceSet eligibleResourceSet = rr.GetEligibleResourcesForPlant(primaryResource.Plant.Id);
                        using IEnumerator<EligibleResourceSet.ResourceData> enumerator = eligibleResourceSet.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            //Check that this helper is allowed to be used along with the primary
                            EligibleResourceSet.ResourceData data = enumerator.Current;
                            Resource possibleHelper = (Resource)data.m_res;
                            if (primaryResource.IsAllowedHelperResource(possibleHelper))
                            {
                                //VerifyCompatibilityGroupConstraintsAndDetermineNonPrimaryRR
                                //Determine if this is a good candidate
                                result = possibleHelper.NonPrimarySchedulable(this, activity, rr, satiators, si, null);
                                if (result.Availability == SchedulableSuccessFailureEnum.Success)
                                {
                                    //Pick this one, although it is just the first, and not necessarily the best
                                    //Eligible resource found, Add reservation
                                    eligibleResource = possibleHelper;
                                    break;
                                }
                            }
                        }
                    }

                    if (result.Availability == SchedulableSuccessFailureEnum.Success)
                    {
                        //Set the resource for this rr for use in the extension
                        satiators[rrIndex] = new ResourceSatiator(eligibleResource);
                        //Check extension points
                        if (SchedulabilityCustomization_RunCustomization(activity))
                        {
                            //TODO: maybe pass in satiators?
                            long? delayedScheduleTime = m_extensionController.IsSchedulableNonPrimaryResourceRequirement(this, m_activeSimulationType, Clock, helperStart, satiators, rrIndex, activity, si);
                            if (delayedScheduleTime.HasValue)
                            {
                                if (delayedScheduleTime.Value > m_simClock)
                                {
                                    return delayedScheduleTime.Value;
                                }
                            }
                        }

                        m_resRvns.AddPlanned(new ResourceReservation(eligibleResource, activity, rrIndex, helperStart, helperEnd, primaryRRR.ResRv.ReservedResource == eligibleResource));
                    }
                    else if (result.Availability == SchedulableSuccessFailureEnum.IntersectsReservedMoveDate)
                    {
                        //TODO: It's possible that the reserved move date is due to a successor operation of this op. 
                        //Due to the complexities of the move replay actions, this cannot be easily solved.
                        //Ideally the move reservation would be added like other block reservations, instead of just an interval on the resource.
                        //In the case where it is a successor, this can actually be ignored if this op is before the op being moved
                        //For example in Op10, 20, 30: If op 20 is moved to a spot that overlaps op30, this result will be hit, because op10 fails the check for op10 -> op20 ->op30.
                        //  To solve this, we should either track move blocks the same way as other reservations, or modify the move, to move the continuous predecessor to a spot
                        //  back calculated based on the move dates of the predecessor.
                        return result.TryAgainTicks;
                    }
                    else
                    {
                        return result.TryAgainTicks;
                    }
                }
            }

            // Insert continuously into successor operations.
            if (ContinuouslyInsertSuccessor(sucRO))
            {
                AlternatePath.Association sucAsn = sucRO.AlternatePathNode.Successors[0];

                // ***LRH*** change this for AtFirstTransfer. In that case you only need to compute when the first transfer has completed. Not all transfers as is done
                // with normal overlap.
                if (sucRO.EligibleForAtFirstTransferOverlap())
                {
                    try
                    {
                        InternalActivity ia = sucRO.Activities.GetByIndex(0);
                        primaryResource.CalculateCycleCompletionTimes(ia, si, out CycleAdjustmentProfile ccp);
                        sucRO.CalculateContainerCompletions(primaryResource, si.m_requiredQty, ccp);
                    }
                    catch (Exception e)
                    {
                        Overlap.Throw_OverlapDebugError(e);
                    }
                }
                else if (sucRO.EligibleForTransferQtyOverlap())
                {
                    try
                    {
                        InternalActivity ia = sucRO.Activities.GetByIndex(0);
                        primaryResource.CalculateCycleCompletionTimes(ia, si, out CycleAdjustmentProfile ccp);
                        sucRO.CalculateContainerCompletions(primaryResource, si.m_requiredQty, ccp);
                    }
                    catch (Exception e)
                    {
                        Overlap.Throw_OverlapDebugError(e);
                    }
                }

                long nextSucStartTicks = GetStartTimeForSucOp(sucAsn, si);
                long attemptedStartTicks = nextSucStartTicks;
                long mustStartByTicksForSuc = si.m_postProcessingFinishDate + sucAsn.MaxDelayTicks;

                do
                {
                    nextSucStartTicks = InsertContinuousSuccessor(sucAsn, nextSucStartTicks, si, mustStartByTicksForSuc);
                    if (attemptedStartTicks == nextSucStartTicks)
                    {
                        break;
                    }

                    attemptedStartTicks = nextSucStartTicks;
                } while (nextSucStartTicks != 0 && nextSucStartTicks < mustStartByTicksForSuc);

                if (nextSucStartTicks != 0)
                {
                    // The successor couldn't be inserted. Remove the reservation for this association.
                    m_resRvns.RemoveByOperation(sucAsn.Successor.Operation);
                    long possibleStartDate = DetermineNextPossibleStartDate(sucAsn, sucRO, si, nextSucStartTicks);

                    if (a_asn.MaxDelayTicks > 0 && !tryingDifferentStartTime)
                    {
                        // A possible performance enhancement would be to store the deeper ResourceRequirementReservations.
                        // In this case, the successor resource requirements are being recalculated.
                        tryingDifferentStartTime = true;
                        if (a_startTicks > possibleStartDate)
                        {
                            return a_startTicks;
                        }

                        a_startTicks = possibleStartDate;
                    }
                    else
                    {
                        return possibleStartDate;
                    }
                }
            }
        } while (tryingDifferentStartTime && attemptNo == 1);

        return 0;
    }

    private ResourceReservationResult InsertContinuousSuccessorRR(ResourceOperation a_sucRO, AlternatePath.Association asn, long a_startTicks, SchedulableInfo a_predecessorSi, long a_mustStartByTicks)
    {
        Resource schedRes = null;
        ResourceReservationResult rrr = null;
        InternalActivity sucAct = a_sucRO.Activities.GetByIndex(0); // Activities under the control of Max Delay have a single activity.

        // Mirroring the sequence in ActivityReleased() and AddToAllDispatchers(); AddToAllDispatchers is only used by ActivityReleased it's just a helper of ActivityReleased()
        if (sucAct.OpOrPredOpBeingMoved && m_activeSimulationType == SimulationType.Move && sucAct.GetMoveResource(a_sucRO.ResourceRequirements.PrimaryResourceRequirementIndex, out Resource ir))
        {
            schedRes = ir;
            if (schedRes.EnforceMaxDelay) // It's possible Max Delay can't be enforced on this resource.
            {
                rrr = CreateContinuousReservationHelper(schedRes, asn, a_startTicks, a_predecessorSi, a_mustStartByTicks);
            }
        }
        else if (sucAct.BeingMoved)
        {
            if (sucAct.GetMoveResource(0, out Resource primaryRes))
            {
                schedRes = primaryRes;
                if (schedRes.EnforceMaxDelay) // It's possible Max Delay can't be enforced on this resource.
                {
                    rrr = CreateContinuousReservationHelper(schedRes, asn, a_startTicks, a_predecessorSi, a_mustStartByTicks);
                }
            }
        }
        else if (sucAct.Sequential != null && (schedRes = sucAct.Sequential[0].SequencedSimulationResource) != null)
        {
            if (schedRes.EnforceMaxDelay) // It's possible Max Delay can't be enforced on this resource.
            {
                rrr = CreateContinuousReservationHelper(schedRes, asn, a_startTicks, a_predecessorSi, a_mustStartByTicks);
            }
        }
        else if ((schedRes = sucAct.PrimaryResourceRequirementLock()) != null)
        {
            if (schedRes.EnforceMaxDelay) // It's possible Max Delay can't be enforced on this resource.
            {
                rrr = CreateContinuousReservationHelper(schedRes, asn, a_startTicks, a_predecessorSi, a_mustStartByTicks);
            }
        }
        else if ((schedRes = (Resource)a_sucRO.ResourceRequirements.PrimaryResourceRequirement.DefaultResource) != null)
        {
            if (schedRes.EnforceMaxDelay) // It's possible Max Delay can't be enforced on this resource.
            {
                rrr = CreateContinuousReservationHelper(schedRes, asn, a_startTicks, a_predecessorSi, a_mustStartByTicks);
            }
        }
        else
        {
            if (a_sucRO.AlternatePathNode.ResReqsEligibilityNarrowedDuringSimulation.Count > 0)
            {
                PlantResourceEligibilitySet pres = a_sucRO.AlternatePathNode.ResReqsEligibilityNarrowedDuringSimulation[0];

                List<Resource> resourcesToAttempt = new ();
                SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = pres.GetEnumerator();
                while (ersEtr.MoveNext())
                {
                    foreach (EligibleResourceSet.ResourceData resourceData in ersEtr.Current.Value)
                    {
                        resourcesToAttempt.Add((Resource)resourceData.m_res);
                    }
                }

                //Sort the resources based on least current reservations
                resourcesToAttempt.Sort(static (r1, r2) =>
                {
                    int r1Reservations = r1.Reservations.Count;
                    int r2Reservations = r2.Reservations.Count;

                    int cmp = r1Reservations.CompareTo(r2Reservations);
                    return cmp != 0 ? cmp : r1.Id.CompareTo(r2.Id);
                });

                if (resourcesToAttempt.Count > 1 && resourcesToAttempt.Contains(a_predecessorSi.m_resource))
                {
                    if (asn.OverlapType != InternalOperationDefs.overlapTypes.NoOverlap)
                    {
                        //Attempt the predecessor's resource last, since it couldn't overlap on the same resource
                        resourcesToAttempt.Remove(a_predecessorSi.m_resource);
                        resourcesToAttempt.Add(a_predecessorSi.m_resource);
                    }
                    else
                    {
                        //Attempt the predecessor's resource first, to keep them sequential.
                        resourcesToAttempt.Remove(a_predecessorSi.m_resource);
                        resourcesToAttempt.Insert(0, a_predecessorSi.m_resource);
                    }
                }

                // Try each plant.
                bool sameCellConstraint = false;

                if (a_predecessorSi.m_resource.Cell != null)
                {
                    foreach (Resource eligibleResource in resourcesToAttempt)
                    {
                        schedRes = eligibleResource;
                        // Try an eligible resource within a plant.
                        if (schedRes.Cell == a_predecessorSi.m_resource.Cell)
                        {
                            sameCellConstraint = true;
                            if (schedRes.EnforceMaxDelay)
                            {
                                rrr = CreateContinuousReservationHelper(schedRes, asn, a_startTicks, a_predecessorSi, a_mustStartByTicks);
                            }

                            // *** You should try to select the best possible resource.
                            // *** If selection of a resource fails to satisfy the scheduling requirements then you can try another, not just the first as done here. 
                            if (rrr.Reservation)
                            {
                                break;
                            }
                        }
                    }
                }

                if (!sameCellConstraint)
                {
                    foreach (Resource eligibleResource in resourcesToAttempt)
                    {
                        // Try an eligible resource within a plant.
                        schedRes = eligibleResource;
                        List<ResourceConnector> connectors = m_resourceConnectorManager.GetConnectorsFromResource(eligibleResource.Id).ToList();
                        if (connectors.Count > 0)
                        {
                            if (!connectors.Any(c => c.ToResources.ContainsKey(schedRes.Id)))
                            {
                                // TODO: This won't work because the value hasn't been set yet.
                                // From the previous resource you need to try to determine what's eligible.
                                // You need to carry this forward to successors under control of max delay.
                                // Batch isn't complete either.

                                continue;
                            }
                        }

                        if (schedRes.EnforceMaxDelay && a_startTicks < m_endOfPlanningHorizon) //No need to enforce MaxDelay past the planning horizon. That would impact performance.
                        {
                            rrr = CreateContinuousReservationHelper(schedRes, asn, a_startTicks, a_predecessorSi, a_mustStartByTicks);
                        }

                        // *** You should try to select the best possible resource.
                        // *** If selection of a resource fails to satisfy the scheduling requirements then you can try another, not just the first as done here.

                        // Note: It's possible or the rrr to be null when no resources are available that can be reserved.
                        // For instance, if none of the eligible resources  are able to process the able to process max delay operation (at the time of this writing, only single tasking resources
                        // could process max delay operations).
                        // In this case, rrr will be null and this function will return null. 
                        if (rrr != null && rrr.Reservation)
                        {
                            break;
                        }
                    }
                }
            }
        }


        if (rrr != null && rrr.SchedulableResult != null && rrr.SchedulableResult.m_result == SchedulableSuccessFailureEnum.IntersectsReservedMoveDate && a_sucRO.ResourceRequirements.HasMixOfUsageStarts())
        {
            AddMoveIntersector(a_startTicks, schedRes.ReservedMoveStartTicks, sucAct, a_sucRO.ResourceRequirements.PrimaryResourceRequirement, a_sucRO.ResourceRequirements.PrimaryResourceRequirementIndex, schedRes);
        }

        return rrr;
    }

    // [USAGE_CODE]: Don't create a max-delay continuous reservation if WaitingForMoveActivityToSchedule.
    /// <summary>
    /// Account for transfer between operations when calculating max delay
    /// </summary>
    /// <param name="a_schedRes">Successor Resource</param>
    /// <param name="a_asn">Path Association between the two operations</param>
    /// <param name="a_startTicks">Successor start</param>
    /// <param name="a_predecessorSi">Predecessor capacity information, including resource</param>
    /// <param name="a_mustStartByTicks"></param>
    /// <returns></returns>
    private ResourceReservationResult CreateContinuousReservationHelper(Resource a_schedRes, AlternatePath.Association a_asn, long a_startTicks, SchedulableInfo a_predecessorSi, long a_mustStartByTicks)
    {
        long adjustedStartTicks = a_startTicks;
        if (a_schedRes == a_predecessorSi.m_resource)
        {
            //The successor is scheduling on the same resource as the predecessor
            if (a_schedRes.CapacityType == InternalResourceDefs.capacityTypes.SingleTasking)
            {
                adjustedStartTicks = a_predecessorSi.m_finishDate;
            }
            else if (a_schedRes.CapacityType == InternalResourceDefs.capacityTypes.MultiTasking)
            {
                //TODO: Determine if there is enough attention
            }
        }

        return a_schedRes.CreateContinuousReservation(this, SimClock, a_asn, adjustedStartTicks, a_predecessorSi.m_resource, a_mustStartByTicks);
    }

    private long DetermineNextPossibleStartDate(AlternatePath.Association a_asn, ResourceOperation a_ro, SchedulableInfo a_si, long a_possibleStartDateOfSuccessorOp)
    {
        if (a_possibleStartDateOfSuccessorOp == long.MaxValue)
        {
            //TODO: We failed to determine a next start time, maybe this should be calculated in InsertContinuousSuccessor
            return a_si.m_finishDate; //Not good, but will need to figure out why a_possibleStartDateOfSuccessorOp fails to have a date before we try to calculate this.
        }

        // Use the next plausible start time of the successor operation as the next plausible finish date
        // of the predecessor operation. Use it to backwards calculate the next plausible start date of the 
        // predecessor operation.
        InternalActivity ia = a_ro.Activities.GetByIndex(0);

        long possibleFinishDate = a_possibleStartDateOfSuccessorOp - ia.GetResourceProductionInfo(a_si.m_resource).MaterialPostProcessingSpanTicks - Math.Max(a_asn.MaxDelayTicks, a_asn.TransferSpanTicks);
        Resource.FindStartFromEndResult findStartFromEndResult;

        //TODO: Most of this does not account for post processing. Also, why do we add OverrunRequiredSpan?? Is this a one minute buffer?
        if (a_asn.OverlapType == InternalOperationDefs.overlapTypes.TransferQty)
        {
            long tickToCompleteFirstUsageQty = ia.CalcSpanToMakeQty(a_si.m_resource, a_asn.UsageQtyPerCycle);
            RequiredCapacity rc = new (
                a_si.m_requiredAdditionalCleanBeforeSpan,
                a_si.m_requiredSetupSpan,
                new RequiredSpan(tickToCompleteFirstUsageQty, tickToCompleteFirstUsageQty <= 0),
                RequiredSpan.OverrunRequiredSpan,
                RequiredSpan.OverrunRequiredSpan,
                RequiredSpanPlusClean.s_overrun);
            findStartFromEndResult = a_si.m_resource.FindCapacityReverse(Clock, possibleFinishDate, rc, null, ia);
        }
        else if (a_asn.OverlapType == InternalOperationDefs.overlapTypes.AtFirstTransfer)
        {
            long tickToCompleteFirstUsageQty = ia.CalcSpanToMakeQty(a_si.m_resource, a_asn.UsageQtyPerCycle);
            RequiredCapacity rc = new (
                a_si.m_requiredAdditionalCleanBeforeSpan,
                a_si.m_requiredSetupSpan,
                new RequiredSpan(tickToCompleteFirstUsageQty, tickToCompleteFirstUsageQty <= 0),
                RequiredSpan.OverrunRequiredSpan,
                RequiredSpan.OverrunRequiredSpan,
                RequiredSpanPlusClean.s_overrun);
            findStartFromEndResult = a_si.m_resource.FindCapacityReverse(Clock, possibleFinishDate, rc, null, ia);
        }
        else if (a_asn.OverlapType == InternalOperationDefs.overlapTypes.TransferSpan)
        {
            possibleFinishDate -= a_asn.OverlapTransferTicks;
            RequiredCapacity rc = new (
                a_si.m_requiredAdditionalCleanBeforeSpan,
                a_si.m_requiredSetupSpan,
                RequiredSpan.OverrunRequiredSpan,
                RequiredSpan.OverrunRequiredSpan,
                RequiredSpan.OverrunRequiredSpan,
                RequiredSpanPlusClean.s_overrun);
            findStartFromEndResult = a_si.m_resource.FindCapacityReverse(Clock, possibleFinishDate, rc, null, ia);
        }
        else if (a_asn.OverlapType == InternalOperationDefs.overlapTypes.TransferSpanBeforeStartOfPredecessor)
        {
            possibleFinishDate += a_asn.OverlapTransferTicks;
            RequiredCapacity rc = new (
                a_si.m_requiredAdditionalCleanBeforeSpan,
                a_si.m_requiredSetupSpan,
                RequiredSpan.OverrunRequiredSpan,
                RequiredSpan.OverrunRequiredSpan,
                RequiredSpan.OverrunRequiredSpan,
                RequiredSpanPlusClean.s_overrun);
            findStartFromEndResult = a_si.m_resource.FindCapacityReverse(Clock, possibleFinishDate, rc, null, ia);
        }
        else if (a_asn.OverlapType == InternalOperationDefs.overlapTypes.TransferSpanAfterSetup)
        {
            possibleFinishDate -= a_asn.OverlapTransferTicks;
            RequiredCapacity rc = new (
                a_si.m_requiredAdditionalCleanBeforeSpan,
                a_si.m_requiredSetupSpan,
                RequiredSpan.OverrunRequiredSpan,
                RequiredSpan.OverrunRequiredSpan,
                RequiredSpan.OverrunRequiredSpan,
                RequiredSpanPlusClean.s_overrun);
            findStartFromEndResult = a_si.m_resource.FindCapacityReverse(Clock, possibleFinishDate, rc, null, ia);
        }
        else if (a_asn.OverlapType == InternalOperationDefs.overlapTypes.PercentComplete)
        {
            // From where the successor can start next, backcalculate when the earliest the predecessor
            // can start and release the successor in time to schedule within the max delay window.
            ResourceOperation predOp = (ResourceOperation)a_asn.Predecessor.Operation.AlternatePathNode.Operation;
            InternalActivity predAct = predOp.Activities.GetByIndex(0);
            long finishQtySpan = predAct.CalcProcessingSpanOfRequiredFinishQty(a_si.m_resource);
            // The length of time since processing that must  complete before overlap.
            decimal overlapspan = a_asn.OverlapPercentComplete * finishQtySpan;
            long reqSpan = (long)Math.Ceiling(overlapspan);
            RequiredSpan overlapReqSpan = new (reqSpan, false);
            RequiredCapacity rc = new (
                a_si.m_requiredAdditionalCleanBeforeSpan,
                a_si.m_requiredSetupSpan,
                overlapReqSpan,
                RequiredSpan.OverrunRequiredSpan,
                RequiredSpan.OverrunRequiredSpan,
                RequiredSpanPlusClean.s_overrun);
            findStartFromEndResult = a_si.m_resource.FindCapacityReverse(Clock, possibleFinishDate, rc, null, ia);
        }
        else
        {
            RequiredCapacity rc = new (
                a_si.m_requiredAdditionalCleanBeforeSpan,
                a_si.m_requiredSetupSpan,
                a_si.m_requiredProcessingSpan,
                a_si.m_requiredPostProcessingSpan,
                a_si.m_requiredStorageSpan,
                a_si.m_requiredCleanAfterSpan);
            findStartFromEndResult = a_si.m_resource.FindCapacityReverse(Clock, possibleFinishDate, rc, null, ia);
        }

        if (findStartFromEndResult.StartTicks <= 0)
        {
            findStartFromEndResult.StartTicks = Clock;
        }

        return findStartFromEndResult.StartTicks;
    }

    /// <summary>
    /// Written for use with Max Delay.
    /// Returns the earliest time the successor operation can start taking
    /// features into consideration such as overlap and transferspan.
    /// </summary>
    /// <param name="a_asn">The association between the predecessor and successor operations.</param>
    /// <param name="a_predSI"></param>
    /// <returns></returns>
    private long GetStartTimeForSucOp(AlternatePath.Association a_asn, SchedulableInfo a_predSI)
    {
        // *** LRH *** Right now you're assuming that there is only 1 activity per operation. You might need to change this 
        // to account for operations that have been broken down into multiple activities. 

        // The start time of continuous operations will depend on the release on how the predecessor and successor operation
        // are related. 
        // It could be at the completion of the predesssor, 
        // based on overlap time,
        // or based on when the first piece of material arrives from the predecessor operation.
        long successorStartTicks;
        InternalOperation predOp = a_asn.Predecessor.Operation;

        IsOperationSD2(predOp);

        if (a_asn.OverlapType == InternalOperationDefs.overlapTypes.AtFirstTransfer)
        {
            successorStartTicks = predOp.TransferQtyProfile[0].m_completionDate;
        }
        else if (a_asn.OverlapType == InternalOperationDefs.overlapTypes.TransferQty)
        {
            // ***LRH*** Change this to match the results of real overlap. Right now it's just releasing after the first transfer.
            successorStartTicks = predOp.TransferQtyProfile[0].m_completionDate;
        }
        else
        {
            if (a_asn.OverlapType == InternalOperationDefs.overlapTypes.TransferSpan)
            {
                successorStartTicks = a_asn.GetOverlapTransferSpanReleaseTicks(a_predSI.m_scheduledStartDate);
                //TODO: For successors, we shouldn't be using material transfer info.
                //startTicks += predOp.MaterialPostProcessingSpanTicks;
            }
            else if (a_asn.OverlapType == InternalOperationDefs.overlapTypes.TransferSpanBeforeStartOfPredecessor)
            {
                successorStartTicks = a_asn.CalcEarliestSucOpCanStartForOverlapByTransferSpanBeforeStartOfPredecessor(a_predSI.m_scheduledStartDate);
                //startTicks += predOp.MaterialPostProcessingSpanTicks;
            }
            else if (a_asn.OverlapType == InternalOperationDefs.overlapTypes.TransferSpanAfterSetup)
            {
                successorStartTicks = a_asn.GetOverlapTransferSpanAfterSetupReleaseTicksFromStartOfProcessing(a_predSI.ProcStartDate);
                //startTicks += predOp.MaterialPostProcessingSpanTicks;
            }
            else if (a_asn.OverlapType == InternalOperationDefs.overlapTypes.PercentComplete)
            {
                long predReleaseTicks = a_asn.GetOverlapPercentCompleteReleaseTime(a_predSI.m_scheduledStartDate, a_predSI.m_resource, a_predSI.ProcStartDate);
                successorStartTicks = predReleaseTicks;
            }

            else
            {
                // should be asn.OverlapType == InternalOperationDefs.overlapTypes.NoOverlap
                successorStartTicks = a_predSI.m_finishDate;
                //startTicks += predOp.MaterialPostProcessingSpanTicks;

            }
        }

        long totalTransferTime = 0;
        if (a_asn.TransferDuringPredeccessorOnlineTime)
        {
            //Use the predecessor activity and resource to determine the actual transfer time.
            InternalActivity predAct = predOp.Activities.GetByIndex(0); // Activities under the control of Max Delay have a single activity.
            FindCapacityResult result = a_predSI.m_resource.FindCapacity(a_predSI.m_finishDate, a_asn.TransferSpanTicks, true, null, predAct);
            totalTransferTime = result.FinishDate - a_predSI.m_finishDate;
        }
        else
        {
            totalTransferTime = a_asn.TransferSpanTicks;
        }

        //TODO: transfer not compatible with overlap. do that later. find end date based on start date.

        return successorStartTicks + totalTransferTime;
    }

    /// <summary>
    /// Max delay limitations
    /// ScenarioOptions.EnforceMaxDelay must be on.
    /// The reservation must be made within the planning horizon.
    /// On the predecessor:
    /// 1. single activity
    /// 2. single successor
    /// On the Successor
    /// 1. Single activity
    /// 2. Single resource requirement
    /// 3. Single predecessor
    /// </summary>
    /// <param name="a_predOp"></param>
    /// <returns></returns>
    private bool ContinuouslyInsertSuccessor(InternalOperation a_predOp)
    {
        if (ScenarioOptions.EnforceMaxDelay && a_predOp.Activities.Count == 1 && a_predOp.AlternatePathNode.Successors.Count == 1)
        {
            AlternatePath.Association sucAsn = a_predOp.AlternatePathNode.Successors[0];
            ResourceOperation sucRO = (ResourceOperation)sucAsn.Successor.Operation;

            //Multiple resource requirements use to prevent max delay, the restriction was removed. But max delay may be violated because the helper resources aren't reserved.
            if (EnforceMaxDelay(sucAsn) && sucRO.Activities.Count == 1 && !sucRO.IsFinishedOrOmitted)
            {
                InternalActivity sucAct = sucRO.Activities.GetByIndex(0);
                return !sucAct.ResourceReservationMade;
            }
        }

        return false;
    }

    private bool ContinuouslyInsert(InternalOperation a_io)
    {
        if (ScenarioOptions.EnforceMaxDelay && a_io.Activities.Count == 1 && a_io.AlternatePathNode.Predecessors.Count == 1)
        {
            AlternatePath.Association asn = a_io.AlternatePathNode.Predecessors[0];
            ResourceOperation predRO = (ResourceOperation)asn.Predecessor.Operation;

            if (EnforceMaxDelay(asn) && predRO.Activities.Count == 1)
            {
                InternalActivity predActivity = predRO.Activities.GetByIndex(0);
                return !predActivity.ResourceReservationMade;
            }
        }

        return false;
    }

    /// <summary>
    /// This can only be called during a simulation. It uses a simulation state variable.
    /// </summary>
    /// <param name="asn"></param>
    /// <returns></returns>
    private bool EnforceMaxDelay(AlternatePath.Association a_asn)
    {
        return EnforceMaxDelayBasicRequirement(a_asn) && WithinPlanningHorizon;
    }

    /// <summary>
    /// Max delay between the predecessor and soccessors won't be enforced by simulation unless
    /// this function returns true.
    /// </summary>
    /// <param name="a_asn"></param>
    /// <returns></returns>
    private bool EnforceMaxDelayBasicRequirement(AlternatePath.Association a_asn)
    {
        InternalOperation predOp = (InternalOperation)a_asn.Predecessor.Operation;
        InternalOperation sucOp = (InternalOperation)a_asn.Successor.Operation;
        bool bothScheduled = !predOp.IsFinishedOrOmitted && !sucOp.IsFinishedOrOmitted;
        bool singleLink = predOp.Successors.Count == 1 && sucOp.Predecessors.Count == 1;
        bool singleActs = predOp.Activities.Count == 1 && sucOp.Activities.Count == 1;
        return singleActs && singleLink && bothScheduled && a_asn.MaxDelaySet;
    }

    private void LogMaxDelayViolators()
    {
        if (ScenarioOptions.EnforceMaxDelay)
        {
            List<SchedulingWarningException> exceptions = new ();
            //bool unscheduled = false;
            foreach (Job j in JobManager)
            {
                foreach (ManufacturingOrder mo in j)
                {
                    foreach (InternalOperation op in mo.CurrentPath)
                    {
                        foreach (AlternatePath.Association sucAsn in op.AlternatePathNode.Successors)
                        {
                            if (EnforceMaxDelayBasicRequirement(sucAsn))
                            {
                                // Ignore if scheduled after ScenarioDetail.EndOfPlanningHorizon
                                InternalOperation predOp = op;
                                InternalOperation sucOp = (InternalOperation)sucAsn.Successor.Operation;

                                long endOfPredAct = predOp.ScheduledEndDate.Ticks;
                                long startOfSucAct = sucOp.ScheduledStartDate.Ticks;

                                long delay = startOfSucAct - endOfPredAct;


                                if (delay > sucAsn.MaxDelayTicks)
                                {
                                    TimeSpan ExcessDelay = new (delay);
                                    const string timespanformat = "{0:%d}d.{0:%h}:{0:%m}:{0:%s}";
                                    string span4Fmt = timespanformat.Replace('0', '4', timespanformat.Length);
                                    string span5Fmt = timespanformat.Replace('0', '5', timespanformat.Length);

                                    string fmt = "The delay between operations {0} and {1} in Job {2}, Manufacturing Order {3} exceeds the max delay of " + span4Fmt + " by " + span5Fmt;

                                    SchedulingWarningException e = new ("2984", new object[] { predOp.ExternalId, sucOp.ExternalId, j.ExternalId, mo.ExternalId, sucAsn.MaxDelay, ExcessDelay });
                                    exceptions.Add(e);
                                    //j.Unschedule();
                                    //unscheduled = true;
                                }
                            }
                        }
                    }
                }
            }

            if (exceptions.Count > 0)
            {
                m_errorReporter.LogException(new AggregateException(exceptions), null);
            }
            //if (unscheduled)
            //{
            //    Batches.RemoveDeadBatches();
            //}
        }
    }

    [ConditionalAttribute("DEBUG")]
    /// Only compiled in one of the DEBUG build configurations.
    /// Use this function during debugging to identify a specific activity.
    private void IsActivitySD2(InternalActivity a_act)
    {
        if (a_act.Id == -2147483262)
        {
            int xxx = 0;
        }
    }

    [ConditionalAttribute("DEBUG")]
    /// Only compiled in one of the DEBUG build configurations.
    /// Use this function during debugging to identify a specific activity.
    private void IsOperationSD2(InternalOperation a_op)
    {
        if (a_op.Job.Id == -2147483544 && a_op.Name.Contains("10"))
        {
            int xxx = 0;
        }
    }
}