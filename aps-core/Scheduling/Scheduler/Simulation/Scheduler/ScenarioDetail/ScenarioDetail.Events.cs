using System.Collections;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.Common.PTMath;
using PT.Scheduler.Demand;
using PT.Scheduler.ErrorReporting;
using PT.Scheduler.Schedule;
using PT.Scheduler.Schedule.Demand;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Demand;
using PT.Scheduler.Simulation.Events;
using PT.Scheduler.Simulation.Scheduler.AlternatePaths;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Contains all data related to one copy
/// </summary>
public partial class ScenarioDetail
{
    /// <summary>
    /// Each new simulation event must be added to this function.
    /// Make sure each id is unique.
    /// You could automatically check these in AddEvent by
    /// saving names and ids and making sure none collide. Or you
    /// could use reflection.
    /// Look for unused ids below
    /// </summary>
    internal static void ValidateSimulationEventUniqueIds()
    {
        List<int> ids = new();

        // Set this up with a macro
        ids.Add(ActivityReleasedEvent.UNIQUE_ID);
        ids.Add(AnchorReleaseEvent.UNIQUE_ID);
        ids.Add(ClockReleaseEvent.UNIQUE_ID);
        ids.Add(InProcessReleaseEvent.UNIQUE_ID);
        ids.Add(DelayedReleaseToResourceEvent.UNIQUE_ID);
        ids.Add(MoveActivityTimeEvent.UNIQUE_ID);
        ids.Add(OptimizationReleaseEvent.UNIQUE_ID);
        ids.Add(ReleaseToResourceEvent.UNIQUE_ID);
        ids.Add(RightMovingNeighborReleaseEvent.UNIQUE_ID);
        ids.Add(ScheduledDateBeforeMoveEvent.UNIQUE_ID);
        ids.Add(ResourceCleanoutEvent.UNIQUE_ID);
        ids.Add(TransferSpanEvent.UNIQUE_ID);
        ids.Add(BlockFinishedEvent.UNIQUE_ID);
        ids.Add(SchedulabilityCustomizationEvent.UNIQUE_ID);
        ids.Add(LeadTimeEvent.UNIQUE_ID);
        ids.Add(UNUSED_Event.UNIQUE_ID);
        ids.Add(QtyToStockEvent.UNIQUE_ID);
        ids.Add(JobHoldReleasedEvent.UNIQUE_ID);
        ids.Add(ManufacturingOrderHoldReleasedEvent.UNIQUE_ID);
        ids.Add(ManufacturingOrderReleasedEvent.UNIQUE_ID);
        ids.Add(PredecessorMOAvailableEvent.UNIQUE_ID);
        ids.Add(FinishedPredecessorMOsAvailableEvent.UNIQUE_ID);
        ids.Add(HoldUntilEvent.UNIQUE_ID);
        ids.Add(MaterialAvailableEvent.UNIQUE_ID);
        ids.Add(OperationFinishedEvent.UNIQUE_ID);
        ids.Add(OperationReadyEvent.UNIQUE_ID);
        ids.Add(PredecessorOperationAvailableEvent.UNIQUE_ID);
        ids.Add(ResourceAvailableEvent.UNIQUE_ID);
        ids.Add(ResourceUnavailableEvent.UNIQUE_ID);
        ids.Add(TransferOrderShipEvent.UNIQUE_ID);
        ids.Add(PlanningHorizonEvent.UNIQUE_ID);
        ids.Add(ResourceReservationEvent.UNIQUE_ID);
        ids.Add(AttemptToScheduleForResRvnEvent.UNIQUE_ID);
        ids.Add(TransferOrderReceiveEvent.UNIQUE_ID);
        ids.Add(BatchOperationReadyEvent.UNIQUE_ID);
        ids.Add(JitCompressReleaseEvent.UNIQUE_ID);
        ids.Add(TrialEvent.UNIQUE_ID);
        ids.Add(AlternatePathReleaseEvent.UNIQUE_ID);
        ids.Add(MaterialInTankShelfLifeExpiredEvent.UNIQUE_ID);
        ids.Add(MoveTicksEvent.UNIQUE_ID);
        ids.Add(BlockReservationEvent.UNIQUE_ID);
        ids.Add(UsageEvent.UNIQUE_ID);
        ids.Add(PreventMoveIntersectionEvent.UNIQUE_ID);

        //Gap
        //ids.Add(MoveIntersectionEvent.UNIQUE_ID);
        //ids.Add(PurchaseToStockEvent.UNIQUE_ID);
        //ids.Add(ShelfLifeEvent.UNIQUE_ID);
        //ids.Add(MinAgeEvent.UNIQUE_ID);
        //ids.Add(SalesOrderLineDistributionEvent.UNIQUE_ID);
        //ids.Add(FutureMaterialAvailableEvent.UNIQUE_ID);
        //ids.Add(RetryForCleanoutEvent.UNIQUE_ID);


        if (ids[0] != 0)
        {
            throw new Exception("First index isn't set to 0.");
        }

        int indexBeforeLast = ids.Count - 1;

        for (int i = 0; i < indexBeforeLast; ++i)
        {
            int current = ids[i];
            int next = ids[i + 1];

            if (current + 1 != next)
            {
                throw new Exception("There's a gap or sequence issue with the simulation event unique ids.");
            }
        }

        // Set a breakpoint here to determine the next available id.
        int lastId = ids[ids.Count - 1];
    }

    /// Look for unused ids below
    private void ProcessEvent(EventBase a_nextEvent)
    {
        // Make sure the validation function above has been updated when this function has been adjusted.
        switch (a_nextEvent.UniqueId)
        {
            case ActivityReleasedEvent.UNIQUE_ID: // 0 
                ActivityReleasedEventReceived(a_nextEvent);
                break;

            case AnchorReleaseEvent.UNIQUE_ID: // 1 
                AnchorReleaseEventReceived(a_nextEvent);
                break;

            case ClockReleaseEvent.UNIQUE_ID: // 2 
                ClockReleaseEventReceived(a_nextEvent);
                break;

            case InProcessReleaseEvent.UNIQUE_ID: // 3 
                InProcessReleaseEventReceived(a_nextEvent);
                break;

            case DelayedReleaseToResourceEvent.UNIQUE_ID: // 4 
                BufferReleaseEventReceived(a_nextEvent);
                break;

            case MoveActivityTimeEvent.UNIQUE_ID: // 5 
                MoveActivityTimeEventReceived(a_nextEvent);
                break;

            case OptimizationReleaseEvent.UNIQUE_ID: // 6 
                OptimizationReleaseEventReceived(a_nextEvent);
                break;

            case ReleaseToResourceEvent.UNIQUE_ID: // 7 
                ReleaseToResourceEventReceived(a_nextEvent);
                break;

            case RightMovingNeighborReleaseEvent.UNIQUE_ID: // 8 
                RightMovingNeighborReleaseEventReceived(a_nextEvent);
                break;

            case ScheduledDateBeforeMoveEvent.UNIQUE_ID: // 9 
                ScheduledDateBeforeMoveEventReceived(a_nextEvent);
                break;

            case ResourceCleanoutEvent.UNIQUE_ID: // 10 
                ResourceCleanoutEventReceived(a_nextEvent);
                break;

            case TransferSpanEvent.UNIQUE_ID: // 11 
                TransferSpanEventReceived(a_nextEvent);
                break;

            case BlockFinishedEvent.UNIQUE_ID: // 12 
                BlockFinishedEventReceived(a_nextEvent);
                break;

            case SchedulabilityCustomizationEvent.UNIQUE_ID: // 13 
                SchedulabilityCustomizationEventReceived(a_nextEvent);
                break;

            case LeadTimeEvent.UNIQUE_ID: // 14 
                LeadTimeEventReceived(a_nextEvent);
                break;

            case UNUSED_Event.UNIQUE_ID: // 15 
                UNUSED_EventReceived(a_nextEvent);
                break;

            case QtyToStockEvent.UNIQUE_ID: // 16 

                QtyToStockEventReceived(a_nextEvent);
                break;

            case JobHoldReleasedEvent.UNIQUE_ID: // 17 
                JobHoldReleasedEventReceived(a_nextEvent);
                break;

            case ManufacturingOrderHoldReleasedEvent.UNIQUE_ID: // 18 
                ManufacturingOrderHoldReleasedEventReceived(a_nextEvent);
                break;

            case ManufacturingOrderReleasedEvent.UNIQUE_ID: // 19 
                ManufacturingOrderReleasedEventReceived(a_nextEvent);
                break;

            case PredecessorMOAvailableEvent.UNIQUE_ID: // 20 
                PredecessorMOAvailableEventReceived(a_nextEvent);
                break;

            case FinishedPredecessorMOsAvailableEvent.UNIQUE_ID: // 21 
                FinishedPredecessorMOsAvailableEventReceived(a_nextEvent);
                break;

            case HoldUntilEvent.UNIQUE_ID: // 22 
                HoldUntilEventReceived(a_nextEvent);
                break;

            case MaterialAvailableEvent.UNIQUE_ID: // 23 
                MaterialAvailableEventReceived(a_nextEvent);
                break;

            case OperationFinishedEvent.UNIQUE_ID: // 24 
                OperationFinishedEventReceived(a_nextEvent);
                break;

            case OperationReadyEvent.UNIQUE_ID: // 25 
                OperationReadyEventReceived(a_nextEvent);
                break;

            case PredecessorOperationAvailableEvent.UNIQUE_ID: // 26 
                PredecessorOperationAvailableEventReceived(a_nextEvent);
                break;

            case ResourceAvailableEvent.UNIQUE_ID: // 27 
                ResourceAvailableEventReceived(a_nextEvent);
                break;

            case ResourceUnavailableEvent.UNIQUE_ID: // 28 
                ResourceUnavailableEventReceived(a_nextEvent);
                break;

            case TransferOrderShipEvent.UNIQUE_ID: // 29 
                TransferOrderShipEventReceived(a_nextEvent);
                break;

            case PlanningHorizonEvent.UNIQUE_ID: // 30 
                PlanningHorizonEventReceived(a_nextEvent);
                break;

            case ResourceReservationEvent.UNIQUE_ID: // 31 
                ResourceReservationEventReceived(a_nextEvent);
                break;

            case AttemptToScheduleForResRvnEvent.UNIQUE_ID: // 32 
                AttemptToScheduleForResRvnEventReceived(a_nextEvent);
                break;

            case TransferOrderReceiveEvent.UNIQUE_ID: // 33 
                TransferOrderReceiveEventReceived(a_nextEvent);
                break;

            case BatchOperationReadyEvent.UNIQUE_ID: // 34
                BatchOperationReadyReceived(a_nextEvent);
                break;

            case JitCompressReleaseEvent.UNIQUE_ID: // 35
                JitCompressReleaseEventReceived(a_nextEvent);
                break;

            case TrialEvent.UNIQUE_ID: // 36
                // May allow activities to schedule.
                break;

            case AlternatePathReleaseEvent.UNIQUE_ID: // 37
                AlternatePathReleased(a_nextEvent);
                break;

            case MaterialInTankShelfLifeExpiredEvent.UNIQUE_ID: // 38
                MaterialInTankShelfLifeExpiredEventReceived(SimClock, a_nextEvent);
                break;

            case MoveTicksEvent.UNIQUE_ID: // 39
                MoveTicksEventReceived(a_nextEvent);
                break;

            case BlockReservationEvent.UNIQUE_ID: // 40
                BlockReservationEventReceived(a_nextEvent);
                break;

            case UsageEvent.UNIQUE_ID: // 41
                ResourceDelayedUsageEventReceived(a_nextEvent);
                break;

            case PreventMoveIntersectionEvent.UNIQUE_ID: // 42
                MoveIntersectionConstraintEventReceived(a_nextEvent);
                break;

            case MoveIntersectionEvent.UNIQUE_ID: //44
                MoveIntersectionEventReceived(a_nextEvent);
                break;

            case PurchaseToStockEvent.UNIQUE_ID: //46
                PurchaseToStockEventReceived(a_nextEvent);
                // Nothing to do other than cause activities to attempt to schedule.
                break;

            case ShelfLifeEvent.UNIQUE_ID: // 47
                ShelfLifeReceivedEvent(a_nextEvent);
                break;

            case MinAgeEvent.UNIQUE_ID: // 48
                MinAgeEventReceived(a_nextEvent);
                break;

            case SalesOrderLineDistributionEvent.UNIQUE_ID: // 49
                SalesOrderLineDistributionReceivedEvent(a_nextEvent);
                break;

            case FutureMaterialAvailableEvent.UNIQUE_ID: // 50
                FutureMaterialEventReceived(a_nextEvent);
                break;

            case RetryForCleanoutEvent.UNIQUE_ID: // 51
                RetryForCleanoutEventReceived(a_nextEvent);
                break;

            case ConnectorReleaseEvent.UNIQUE_ID: // 52
                ConnectorReleaseEventReceived(a_nextEvent);
                break;
            
            case RetryForConnectorEvent.UNIQUE_ID: // 53
                RetryForConnectorEventReceived(a_nextEvent);
                break; 
            
            case FutureStorageAvailableEvent.UNIQUE_ID: // 54
                FutureStorageEventReceived(a_nextEvent);
                break;

            case QtyConsumeEvent.UNIQUE_ID: // 55
                QtyConsumedEventReceived(a_nextEvent);
                break;            
            
            case RetryForEmptyStorageAreaEvent.UNIQUE_ID: // 56
                RetryForEmptyStorageAreaReceived(a_nextEvent);
                break;

            case AlternatePathValidityEndEvent.UNIQUE_ID: // 1122
                AlternatePathValidityEnd(a_nextEvent);
                break;
            
            case FutureStorageDisposalEvent.UNIQUE_ID: // 57
                FutureStorageDisposalEventReceived(a_nextEvent);
                break;
            
            case StorageAreaUsageEvent.UNIQUE_ID: //58
                StorageAreaUsageEventReceived(a_nextEvent);
                break;
            
            case HeadStartWindowRetryEvent.UNIQUE_ID: //59
                RetryForHeadStartEndEventReceived(a_nextEvent);
                break;
#if DEBUG
            // Make sure the validation function above has been updated when this function has been adjusted.

            default:
                throw new Exception("Unhandled simulation event");
#endif
        }
    }

    private void JitCompressReleaseEventReceived(EventBase a_nextEvent)
    {
        JitCompressReleaseEvent e = (JitCompressReleaseEvent)a_nextEvent;
#if DEBUG
        if (e.Activity.WaitForRightCompressReleaseEvent == false)
        {
            throw new Exception("RightCompressReleaseEvent is already false.");
        }
#endif
        e.Activity.WaitForRightCompressReleaseEvent = false;
        if (e.Activity.Released)
        {
            ActivityReleased(e.Activity, e.Time);
        }
    }

    private void BatchOperationReadyReceived(EventBase a_nextEvent)
    {
        BatchOperationReadyEvent e = (BatchOperationReadyEvent)a_nextEvent;
        InternalOperation io = (InternalOperation)e.Operation;
        InternalActivityManager iam = io.Activities;

        for (int i = 0; i < iam.Count; ++i)
        {
            InternalActivity ia = iam.GetByIndex(i);
            if (ia.WaitForPredBatchOpnsToBeScheduled)
            {
                ia.WaitForPredBatchOpnsToBeScheduled = false;
                if (ia.Released)
                {
                    AddActivityReleasedEvent(ia, a_nextEvent.Time);
                }
            }
        }
    }

    private void ResourceAvailableEventReceived(EventBase a_nextEvent)
    {
        ResourceAvailableEvent resourceAvailableEvent = (ResourceAvailableEvent)a_nextEvent;

        if (resourceAvailableEvent.EventState != ResourceAvailableEvent.State.Cancelled)
        {
            // Add the resource to the resource available set.
            AddToAvailableResourceEvents(resourceAvailableEvent);

            // Schedule the next unavailable event if there are any.
            Resource resource = resourceAvailableEvent.Resource;

            if (resource != null)
            {
                ResourceCapacityInterval rci = resourceAvailableEvent.Node.Value;
                if (!rci.IsPastPlanningHorizon)
                {
                    AddResourceUnavailableEvent(resource, rci.EndDate, null);
                }
            }
        }
    }

    private void AddResourceUnavailableEvent(Resource a_resource, long a_endDate, LinkedListNode<ResourceCapacityInterval> a_lastUsedCapacityIntervalNode, long a_nextAvailableResourceTimeTicks = -1)
    {
        CancelLastScheduledAddedResourceUnavailableEvent(a_resource);

        a_resource.LastScheduledResourceUnavailableEvent = new ResourceUnavailableEvent(a_endDate, a_resource, a_lastUsedCapacityIntervalNode, a_nextAvailableResourceTimeTicks);
        AddEvent(a_resource.LastScheduledResourceUnavailableEvent);
    }

    /// <summary>
    /// If an unprocessed ResourceUnavailableEvent is on the queue, but a new resource unavailable time has been determined, then the unprocessed ResourceAvailableEvent should be cancelled so its not
    /// processed.
    /// For instance. The an offline initial event may have been determined when the resource online event was hit. When something is scheduled, the scheduled end time of the block may be before or after
    /// the previously scheduled offline event, so it needs to be cancelled in exchange for the new ResourceUnavailableEvent that's about to be created.
    /// </summary>
    /// <param name="a_resource">The resource whose LastAddedResourceAvailableEvent you want to cancel.</param>
    private void CancelLastScheduledAddedResourceUnavailableEvent(InternalResource a_resource)
    {
        if (a_resource.LastScheduledResourceUnavailableEvent != null)
        {
            // An updated event time has been determined. Ignore the previously determined resource available event time.
            // For instance. The an offline initial event may have been determined when the resource online event was hit. When something is scheduled, the scheduled end time of the block may be before or after
            // the previously scheduled offline event, so it needs to be cancelled in exchange for the new ResourceUnavailableEvent that's about to be created.
            a_resource.LastScheduledResourceUnavailableEvent.Cancelled = true;
            a_resource.LastScheduledResourceUnavailableEvent = null;
        }
    }

    private void ResourceUnavailableEventReceived(EventBase a_nextEvent)
    {
        ResourceUnavailableEvent resourceUnavailableEvent = (ResourceUnavailableEvent)a_nextEvent;
        Resource resource = resourceUnavailableEvent.Resource;

        if (!resourceUnavailableEvent.Cancelled)
        {
            // The AvailablePosition was already nulled out by the Remove called in AttemptToScheduleActivity.
            m_availableResourceEventsSet.Remove(resourceUnavailableEvent.Resource);

            // Schedule the next resource available event.
            if (resource != null)
            {
                resource.LastScheduledResourceUnavailableEvent = null;
                if (resourceUnavailableEvent.NextAvailableResourceTimeTicks != -1)
                {
                    AddResourceAvailableEvent(AddResourceavailableEventType.AtSpecifiedTime, resourceUnavailableEvent.NextAvailableResourceTimeTicks, resource, null);
                }
                else
                {
                    AddResourceAvailableEvent(AddResourceavailableEventType.AtNextAvailableOnlineTime, resourceUnavailableEvent.Time, resourceUnavailableEvent.Resource, resourceUnavailableEvent.LastUsedCapacityIntervalNode);
                }

                resource.LastScheduledResourceUnavailableEvent = null;
            }
        }
    }

    private enum AddResourceavailableEventType { AtSpecifiedTime, AtNextAvailableOnlineTime }

    /// <summary>
    /// Add a ResourceAvailableEvent.
    /// </summary>
    /// <param name="a_addType">Used to specify how to use the time being passed in. If a</param>
    /// <param name="a_time"></param>
    /// <param name="a_resource"></param>
    private void AddResourceAvailableEvent(AddResourceavailableEventType a_addType, long a_time, Resource a_resource, LinkedListNode<ResourceCapacityInterval> a_lastUsedCapacityIntervalNode)
    {
        if (a_addType == AddResourceavailableEventType.AtSpecifiedTime)
        {
            // Find the next available online interval.
            LinkedListNode<ResourceCapacityInterval> nextAvailableInterval = a_resource.FindContainingCapacityIntervalNode(a_time, null);

            while (!nextAvailableInterval.Value.Active)
            {
                nextAvailableInterval = a_resource.FindContainingCapacityIntervalNode(nextAvailableInterval.Value.EndDate, nextAvailableInterval);
            }

            long nextAvailableDate;

            if (Interval.Contains(nextAvailableInterval.Value.StartDate, nextAvailableInterval.Value.EndDate, a_time))
            {
                // The time lies within an online interval.
                nextAvailableDate = a_time;
            }
            else
            {
                // Use the start of the next available interval.
                nextAvailableDate = nextAvailableInterval.Value.StartDate;
            }

            AddResourceAvailableEvent(a_resource, nextAvailableDate, nextAvailableInterval);

            CancelLastScheduledAddedResourceUnavailableEvent(a_resource);
        }
        else
        {
            AddResourceAvailableEventAtNextOnlineTime(a_resource, a_time, a_lastUsedCapacityIntervalNode);
        }
    }

    private void ResourceCleanoutEventReceived(EventBase a_nextEvent)
    {
        ResourceCleanoutEvent rme = (ResourceCleanoutEvent)a_nextEvent;
        rme.Resource.Cleanout();

        LinkedListNode<ResourceCapacityInterval> node = rme.Node.Next;

        while (node != null)
        {
            if (node.Value.ClearChangeovers)
            {
                AddEvent(new ResourceCleanoutEvent(node.Value.EndDate, rme.Resource, node));
                break;
            }

            node = node.Next;
        }
    }

    /// <summary>
    /// Determine the ship quantity (minimum of specified ship quantity and actual), deduct it from inventory, and
    /// create a transfer order receive event for the amount going to another inventory.
    /// </summary>
    /// <param name="a_nextEvent">Actual type TransferOrderShipEvent.</param>
    private void TransferOrderShipEventReceived(EventBase a_nextEvent)
    {
        // Attempt to find the specified ship quantity and allocate the material.
        TransferOrderShipEvent e = (TransferOrderShipEvent)a_nextEvent;
        TransferOrderDistribution tod = e.Distribution;
        Inventory inv = tod.FromWarehouse.Inventories[tod.Item.Id];
        decimal remainingShipQty = tod.QtyOpenToShip;

        if (remainingShipQty > 0 && inv != null)
        {
            if (e.InitialProcessingForDemand)
            {
                Adjustment demandAdjustment = tod.GenerateDemandAdjustment(inv);
                inv.AddSimulationAdjustment(demandAdjustment);
                e.InitialProcessingForDemand = false;
            }

            TransferOrderDemandProfile transferOrderDemandProfile = new TransferOrderDemandProfile(tod);
            ConsumeMaterialResult result = ConsumeMatlForDistributionEventEvent(a_nextEvent, transferOrderDemandProfile);
            if (result.ConsumedMaterial || !tod.TransferOrder.Firm) //Planned orders will not try again for material and just show a shortage if material was not found
            {
                // The material has shipped late.
                // It's possible the material wasn't available for transfer at the desired  time due to a lack of availability, so
                // the actual date of shipment may differ from the transfer order distribution's ScheduledShipDateTicks.
                // Here, I'm estimating the transfer delay by subtracting the ship date from the scheduled receive date.
                long estTT = tod.ScheduledReceiveDateTicks - tod.ScheduledShipDateTicks;
                estTT = Math.Max(estTT, 0); //If for some reason the receipt date is before the ship date
                AddTodReceivedEvent(e.Time + estTT, tod);
            }
            else
            {
                if (result.RetryEvent != null)
                {
                    //TODO: we probably don't need this as it will retry again with the material retry event
                    //Retry the material consumption
                    AddEvent(result.RetryEvent);
                }

                // Add to set of materials that aren't readily fulfillable.
                transferOrderDemandProfile.Item.AddRetryMatlEvt(a_nextEvent);
            }
        }
    }

    /// <summary>
    /// Add a TransferOrderReceiveEvent.
    /// </summary>
    /// <param name="a_recvTicks">The time when transferred material will arrive at its destination warehouse.</param>
    /// <param name="tod">The transfer order</param>
    /// <param name="remainingShipQty">The quantity being shipped.</param>
    private void AddTodReceivedEvent(long a_recvTicks, TransferOrderDistribution tod)
    {
        long tt = WarehouseManager.GetTT(tod.FromWarehouse, tod.ToWarehouse);
        long receiveTicks = a_recvTicks + tt;
        TransferOrderReceiveEvent tore = new (receiveTicks, tod);
        AddEvent(tore);
    }

    /// <summary>
    /// Increment the inventory by the receive quantity.
    /// </summary>
    /// <param name="a_nextEvent">Actual type TransferOrderReceiveEvent</param>
    private void TransferOrderReceiveEventReceived(EventBase a_nextEvent)
    {
        TransferOrderReceiveEvent e = (TransferOrderReceiveEvent)a_nextEvent;
        TransferOrderDistribution dist = e.Distribution;
        dist.ActualReceiptTicks = e.Time;
        Warehouse distToWarehouse = dist.ToWarehouse;
        Lot newLot = distToWarehouse.StoreTransferOrder(dist, e.ReceiveQty, m_simClock, ScenarioOptions);
        RetryItemMatlReqs(e.Time, dist.Item);
        
        if (newLot != null && newLot.ShelfLifeData.Expirable)
        {
            AddEvent(new ShelfLifeEvent(newLot.ShelfLifeData.ExpirationTicks, newLot));
        }
    }

    private void AddToAvailableResourceEvents(EventBase a_nextEvent)
    {
        ResourceAvailableEvent rae = (ResourceAvailableEvent)a_nextEvent;
        m_availableResourceEventsSet.Add(rae);
    }

    /// <summary>
    /// A predecessor operation have completed processing and material post-processing.
    /// Transfer-time is taken into consideration in this function.
    /// </summary>
    /// <param name="predecessorReadyEvent"></param>
    private void PredecessorOperationAvailableEventReceived(EventBase a_nextEvent)
    {
        PredecessorOperationAvailableEvent predReadyEvt = (PredecessorOperationAvailableEvent)a_nextEvent;
        BaseOperation sucBaseOp = predReadyEvt.Association.Successor.Operation;
        InternalOperation predecessorOperation = predReadyEvt.Association.Predecessor.Operation as InternalOperation;

        sucBaseOp.PredecessorOperationReady(predReadyEvt.Time, predReadyEvt.Association);
        sucBaseOp.AlternatePathNode.ReportPredecessorReleaseTicks(a_nextEvent.Time);
        SetupMaterialConstraints(sucBaseOp, predReadyEvt.Time);

        InternalOperation sucOp = sucBaseOp as InternalOperation;
        if (sucOp != null)
        {
            if (m_activeSimulationType == SimulationType.Move || m_activeSimulationType == SimulationType.MoveAndExpedite)
            {
                // Make sure the successor activities have their predecessor operation waiting flag cleared.
                for (int activityI = 0; activityI < sucOp.Activities.Count; ++activityI)
                {
                    InternalActivity act = sucOp.Activities.GetByIndex(activityI);
                    if (act.WaitForPredecessorOperationReleaseEvent)
                    {
                        // *LRH*TODO*Verify that all the predecessor operations are ready
                        act.WaitForPredecessorOperationReleaseEvent = false;
                    }
                }
            }

            if (!OperationReleaseCheckAndHandling(predReadyEvt.Time, sucOp, predReadyEvt.PredecessorReleaseTime, InternalOperation.LatestConstraintEnum.PredecessorOperation, predecessorOperation))
            {
                if (predecessorOperation != null)
                {
                    // Check for transfer time.
                    long predecessorTransferTicks;

                    if (predecessorOperation.EligibleForAtFirstTransferOverlapOrTransferQtyOverlap())
                    {
                        // This type of transfer time is handled
                        predecessorTransferTicks = -1;
                    }
                    else
                    {
                        predecessorTransferTicks = predReadyEvt.Association.TransferSpanTicks;
                    }

                    if (predReadyEvt.Association.TransferStart != OperationDefs.EOperationTransferPoint.NoTransfer && predecessorTransferTicks >= 0 && !sucOp.IsFinishedOrOmitted)
                    {
                        //Calculate the successor release. It may be this event time if the successor can overlap transfer
                        long successorReleaseTicks = CalcReleaseWithTransferTime(predReadyEvt.Time, predReadyEvt.EarliestReleaseTime, predReadyEvt.Association, out TransferInfo releaseInfo);

                        TransferSpanEvent tse = new(successorReleaseTicks, predReadyEvt.Association, releaseInfo);
                        AddEvent(tse);
                    }
                }
            }
        }
    }

    private long CalcReleaseWithTransferTime(long a_releaseTicks, long a_earliestReleaseTicks, AlternatePath.Association a_predAssociation, out TransferInfo o_transferInfo)
    {
        long releaseTicks = a_earliestReleaseTicks;

        // In the predecessor operation.
        // For each activity.
        //  Find the end of the TT on the Primary Res
        //  If it's the largest found so far
        //      Save it as the release time.
        ResourceOperation predRO = (ResourceOperation)a_predAssociation.Predecessor.Operation;
        Resource predOpPriRes = (Resource)predRO.ResourceRequirements.GetFirstEligiblePrimaryResource();

        bool splitTransferDates = false;
        Dictionary<BaseId, long> m_activityTransferEnds = null; //Only needed for auto-split associated pred-succ operations
        if (predRO.Split && predRO.Successors.Any(s => s.Successor.Operation.AutoSplitInfo.AutoSplitType is OperationDefs.EAutoSplitType.PredecessorMaterials or OperationDefs.EAutoSplitType.PredecessorQuantityRatio))
        {
            splitTransferDates = true;
            m_activityTransferEnds = new();
        }

        // Find the maximum release ticks among the predecessor acitivity's primary resources.
        for (int actI = 0; actI < predRO.Activities.Count; ++actI)
        {
            Resource res;

            long transferStartTicks;
            long transferDuration = a_predAssociation.TransferSpanTicks;

            InternalActivity act = predRO.Activities.GetByIndex(actI);
            if (act.Batch == null || act.Finished)
            {
                res = predOpPriRes;

                switch (a_predAssociation.TransferStart)
                {
                    case OperationDefs.EOperationTransferPoint.StartOfOperation:
                        transferStartTicks = act.ReportedStartDateSet ? act.ReportedStartDateTicks : Clock;
                        break;
                    case OperationDefs.EOperationTransferPoint.EndOfSetup:
                        transferStartTicks = act.ReportedProcessingStartDateSet ? act.ReportedProcessingStartTicks : Clock;
                        break;
                    case OperationDefs.EOperationTransferPoint.EndOfRun:
                        transferStartTicks = act.ReportedEndOfProcessingSet ? act.ReportedEndOfRunTicks : Clock;
                        break;
                    case OperationDefs.EOperationTransferPoint.EndOfPostProcessing:
                        transferStartTicks = act.ReportedEndOfPostProcessingSet ? act.ReportedEndOfPostProcessingTicks : Clock;
                        break;
                    case OperationDefs.EOperationTransferPoint.EndOfStorage:
                        transferStartTicks = act.ReportedEndOfStorageSet ? act.ReportedEndOfStorageTicks : Clock;
                        break;
                    case OperationDefs.EOperationTransferPoint.EndOfOperation:
                        transferStartTicks = act.ReportedFinishDateSet ? act.ReportedFinishDateTicks : Clock;
                        break;
                    default:
                        throw new NotImplementedException("Invalid Transfer start point set");
                }
            }
            else
            {
                res = act.Batch.PrimaryResource;
                switch (a_predAssociation.TransferStart)
                {
                    case OperationDefs.EOperationTransferPoint.StartOfOperation:
                        transferStartTicks = act.ProductionStatus >= InternalActivityDefs.productionStatuses.Started ? act.ReportedStartDateTicks : act.Batch.StartTicks;
                        break;
                    case OperationDefs.EOperationTransferPoint.EndOfSetup:
                        transferStartTicks = act.ProductionStatus >= InternalActivityDefs.productionStatuses.Running ? act.ReportedProcessingStartTicks : act.Batch.SetupEndTicks;
                        break;
                    case OperationDefs.EOperationTransferPoint.EndOfRun:
                        transferStartTicks = act.ProductionStatus >= InternalActivityDefs.productionStatuses.PostProcessing ? act.ReportedEndOfRunTicks : act.Batch.ProcessingEndTicks;
                        break;
                    case OperationDefs.EOperationTransferPoint.EndOfPostProcessing:
                        transferStartTicks = act.ProductionStatus >= InternalActivityDefs.productionStatuses.Storing ? act.ReportedEndOfPostProcessingTicks : act.Batch.PostProcessingEndTicks;
                        break;
                    case OperationDefs.EOperationTransferPoint.EndOfStorage:
                        transferStartTicks = act.ProductionStatus >= InternalActivityDefs.productionStatuses.Cleaning ? act.ReportedEndOfStorageTicks : act.Batch.EndOfStorageTicks;
                        break;
                    case OperationDefs.EOperationTransferPoint.EndOfOperation: 
                        transferStartTicks = act.ProductionStatus >= InternalActivityDefs.productionStatuses.Finished ? act.ReportedFinishDateTicks : act.Batch.EndTicks;
                        break;
                    default:
                        throw new NotImplementedException("Invalid Transfer start point set");
                }
            }

            if (a_predAssociation.TransferDuringPredeccessorOnlineTime && res != null)
            {
                // The release transfer time occurs while the predecesor resource is online.
                FindCapacityResult result = res.FindCapacity(transferStartTicks, a_predAssociation.TransferSpanTicks, true, null, act);
                transferDuration = result.FinishDate - transferStartTicks;
            }

            if (splitTransferDates)
            {
                m_activityTransferEnds.Add(act.Id, Math.Max(transferStartTicks + transferDuration, m_simClock));
            }
            else
            {
                releaseTicks = Math.Max(releaseTicks, transferStartTicks + transferDuration);
            }
        }

        //It's possible if an activity was finished far in the past, the transfer could have already elapsed
        // We still need to send an event so the transfer of the successor is released.
        if (splitTransferDates)
        {
            releaseTicks = m_activityTransferEnds.Values.Max();
            o_transferInfo = new TransferInfo(m_activityTransferEnds, a_predAssociation.TransferEnd);
        }
        else
        {
            releaseTicks = Math.Max(releaseTicks, m_simClock);

            o_transferInfo = new TransferInfo(releaseTicks, a_predAssociation.TransferEnd);
        }

        //Return the end of transfer based on the duration and start point.
        if (a_predAssociation.TransferEnd != OperationDefs.EOperationTransferPoint.StartOfOperation)
        {
            //If the TransferEnd is not StartOfOperation, then it will be validated in AttemptToSchedule, and we can release the successor immediately
            return a_releaseTicks;
        }

        //Constraint the start of the successor based on the transfer end
        return releaseTicks;
    }

    private void OperationReadyEventReceived(EventBase a_nextEvent)
    {
        OperationReadyEvent operationReadyEvent = (OperationReadyEvent)a_nextEvent;
        OperationReleaseCheckAndHandling(operationReadyEvent.Time, operationReadyEvent.Operation, operationReadyEvent.LatestConstraint);
    }

    private void OperationReleaseCheckAndHandling(long a_releaseTicks, BaseOperation a_readyOperation, InternalOperation.LatestConstraintEnum a_latestConstraint)
    {
        OperationReleaseCheckAndHandling(a_releaseTicks, a_readyOperation, a_releaseTicks, a_latestConstraint, null);
    }

    private void AddActivityReleasedEvent(InternalActivity a_act, long a_time)
    {
        if (!a_act.ActivityReleaseEventPosted)
        {
            if (a_act.WaitForScheduledDateBeforeMoveEvent && a_act.OriginalScheduledStartTicks > a_time)
            {
                return;
            }
            // [USAGE_CODE] AddActivityReleasedEvent: If a sequenced activity with ResourceRequirement whose UsageStart==Run, change the release time to the RR SequentialState's release time minus the SetupSpan. I don't know why this is done instead of releasing at the time passed to this function. Maybe the function is being passed a release time of the Run block, so setup needs to be subtracted to release the blocks with setup at the right time.
            long releaseTime = a_time;
            if (a_act.Sequenced)
            {
                Resource primary = (Resource)a_act.Sequential[a_act.Operation.ResourceRequirements.PrimaryResourceRequirementIndex].SequencedSimulationResource;
                SchedulableResult sr = primary.PrimarySchedulable(this, a_time, a_time, a_act, ExtensionController, false);

                if (sr.m_result == SchedulableSuccessFailureEnum.Success) // Otherwise it can still be released without the potential adjustment below.
                {
                    SchedulableInfo si = sr.m_si;

                    if (si.m_requiredSetupSpan.TimeSpanTicks > 0)
                    {
                        for (int rrI = 0; rrI < a_act.Operation.ResourceRequirements.Count; ++rrI)
                        {
                            ResourceRequirement rr = a_act.Operation.ResourceRequirements.GetByIndex(rrI);
                            if (rr.UsageStart == MainResourceDefs.usageEnum.Run)
                            {
                                InternalActivity.SequentialState ss = a_act.Sequential[rrI];
                                long adjustedRelease = ss.ReleaseTicks - si.m_requiredSetupSpan.TimeSpanTicks; // Change the release time to the Run start time minus the setup time.
                                releaseTime = Math.Max(adjustedRelease, SimClock); // Avoids going further back than the SimClock
                            }
                        }
                    }
                }
            }

            ActivityReleasedEvent activityReleasedEvent = new(releaseTime, a_act);
            AddEvent(activityReleasedEvent);
            a_act.ActivityReleaseEventPosted = true;
        }
    }

    private void AddAnchorReleaseEvent(InternalActivity a_activity, long a_time)
    {
        if (!a_activity.Operation.EnforceMaxDelay())
        {
            AnchorReleaseEvent anchorReleasedEvent = new(a_time, a_activity);
            AddEvent(anchorReleasedEvent);
        }
        else
        {
            // Anchoring is not a constraint when used with Max Delay.
            a_activity.WaitForAnchorReleaseEvent = false;
        }
    }

    /// <summary>
    /// Add an activity release event for the activity if the activity has been released.
    /// </summary>
    /// <param name="activity">The activity that might be released.</param>
    /// <param name="time">The time of release.</param>
    private void AddActivityReleaseEventIfReleased(InternalActivity a_activity, long a_time)
    {
        if (a_activity.Released)
        {
            AddActivityReleasedEvent(a_activity, a_time);
        }
    }

    /// <summary>
    /// Adds ready leaf activities of a manufacturing order to eligible dispatchers.
    /// </summary>
    /// <param name="moReleasedEvent"></param>
    private void ManufacturingOrderReleasedEventReceived(EventBase a_nextEvent)
    {
        ManufacturingOrderReleasedEvent moReleasedEvent = (ManufacturingOrderReleasedEvent)a_nextEvent;
        moReleasedEvent.ManufacturingOrder.MOReleasedDateEventOccurred = true;

        if (moReleasedEvent.ManufacturingOrder.Released)
        {
            InternalOperation.LatestConstraintEnum latestConstraintType;

            switch (moReleasedEvent.ReleaseType)
            {
                case ManufacturingOrder.EffectiveReleaseDateType.PredecessorMO:
                    latestConstraintType = InternalOperation.LatestConstraintEnum.PredecessorManufacturingOrder;
                    break;

                case ManufacturingOrder.EffectiveReleaseDateType.Unconstrained:
                    latestConstraintType = InternalOperation.LatestConstraintEnum.Clock;
                    break;

                default:
                    latestConstraintType = InternalOperation.LatestConstraintEnum.Unknown;
                    break;
            }

            ManufacturingOrderReleased(moReleasedEvent.ManufacturingOrder, moReleasedEvent.Time, latestConstraintType);
        }
    }

    // !ALTERNATE_PATH!; Definition of delegate that performs the AlternatePath releases.
    private delegate void ProcessPathReleaseDelegate(long a_time, ManufacturingOrder a_mo, AlternatePath a_path);

    private ProcessPathReleaseDelegate m_processPathReleaseDelegate;

    // !ALTERNATE_PATH!; $$$ 2.0 The path delegate is activated here.
    private void AlternatePathReleased(EventBase a_nextEvent)
    {
        AlternatePathReleaseEvent pathReleaseEvent = (AlternatePathReleaseEvent)a_nextEvent;

        if (!PathIsNoLongerEligibleBecauseAnotherPathIsAlreadyBeingScheduled(pathReleaseEvent.ManufacturingOrder, pathReleaseEvent.AlternatePath))
        {
            AlternatePathRelease(pathReleaseEvent.ManufacturingOrder, pathReleaseEvent.AlternatePath, pathReleaseEvent.Time, pathReleaseEvent.LatestConstraint);
        }
    }

    private void AlternatePathRelease(ManufacturingOrder a_mo, AlternatePath a_path, long a_pathReleaseEventTicks, InternalOperation.LatestConstraintEnum a_latestConstraint)
    {
        AddConstrainingEvents(a_path);

        if (m_processPathReleaseDelegate != null)
        {
            m_processPathReleaseDelegate(a_pathReleaseEventTicks, a_mo, a_path);
        }

        // Add release events for the operations that are released.
        IDictionaryEnumerator alternateNodesEnumerator = a_path.GetSchedulableNodeEnumerator();

        while (alternateNodesEnumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)alternateNodesEnumerator.Current;
            AlternatePath.Node node = (AlternatePath.Node)de.Value;

            //Release the node
            node.PathReleased();

            BaseOperation baseOperation = node.Operation;

            if (node.Operation.Released)
            {
                OperationReleaseCheckAndHandling(a_pathReleaseEventTicks, baseOperation, a_latestConstraint);
            }
        }

        a_path.PathReleaseEventProcessed = true;
        a_mo.PathReleaseEventProcessed = true;
    }

    private void AlternatePathValidityEnd(EventBase a_nextEvent)
    {
        AlternatePathValidityEndEvent pathEndEvent = (AlternatePathValidityEndEvent)a_nextEvent;
        ManufacturingOrder mo = pathEndEvent.ManufacturingOrder;
        AlternatePath path = pathEndEvent.AlternatePath;

        //If not path has scheduled, or the scheduled path is not this path, remove it from scheduling
        if (mo.CurrentPathNeedsToBeSetWhenFirstActivityIsScheduled || mo.CurrentPath != path || (mo.PathCount == 1 && !mo.ActivitiesScheduled)) 
        {
            RemovePathFromScheduling(path);
        }
    }

    private void RemovePathFromScheduling(AlternatePath a_alternatePath)
    {
        foreach (ResourceOperation alternateOperation in a_alternatePath.GetOperationsByLevel(true))
        {
            if (a_alternatePath.PathReleaseEventProcessed) //It's possible the path was never released, so there is nothing to remove here.
            {
                foreach (InternalActivity alternateAct in alternateOperation.Activities)
                {
                    RemoveActivityFromAllResourceDispatchers(alternateAct);
                }
            }

            //Remove lot usage tracking
            foreach (MaterialRequirement mr in alternateOperation.MaterialRequirements)
            {
                if (mr.MustUseEligLot && !mr.IssuedComplete)
                {
                    Dictionary<string, EligibleLot>.Enumerator elEtr = mr.GetEligibleLotsEnumerator();
                    while (elEtr.MoveNext())
                    {
                        UsedAsEligibleLotsLotCodes.Remove(elEtr.Current.Key, mr);
                    }
                }
            }
        }

        a_alternatePath.RemoveFromScheduling();
        a_alternatePath.ExcludedBySchedulingDueToValidity = true;
    }


    /// <summary>
    /// Release the tank if the material hasn't been reelased after a long period of time, such as the shelf life.
    /// </summary>
    /// <param name="a_nextEvent"></param>
    private void MaterialInTankShelfLifeExpiredEventReceived(long a_simClock, EventBase a_nextEvent)
    {
        MaterialInTankShelfLifeExpiredEvent shelfLifeExpiredEvent = (MaterialInTankShelfLifeExpiredEvent)a_nextEvent;

        //if (shelfLifeExpiredEvent.Batch is TankBatch tankBatch && !tankBatch.TankEmptied)
        //{
        //    // [TANK_CODE]: Release the tank if the material hasn't been released after a long period of time, such as the shelf life.
        //    ReleaseTankResources(a_simClock, shelfLifeExpiredEvent.TankResource.Tank, tankBatch, shelfLifeExpiredEvent.Time);
        //    //TODO: Storage
        //    //shelfLifeExpiredEvent.Inventory.ZeroInventory(shelfLifeExpiredEvent.Time, this, shelfLifeExpiredEvent.Inventory);
        //}
    }

    private void PurchaseToStockEventReceived(EventBase a_e)
    {
        PurchaseToStockEvent ptse = (PurchaseToStockEvent)a_e;
        PurchaseToStock pts = ptse.PurchaseToStock;

        Lot newLot = pts.Warehouse.StorePurchaseOrder(pts, ptse.Time, ScenarioOptions);
        if (newLot != null && newLot.ShelfLifeData.Expirable)
        {
            AddEvent(new ShelfLifeEvent(newLot.ShelfLifeData.ExpirationTicks, newLot));
        }

        //This could be the first source of this lot code, release waiting demands
        if (!string.IsNullOrEmpty(pts.LotCode))
        {
            ReleaseLotCode(pts.LotCode, ptse.Time, pts.Inventory.Item);
        }

        RetryItemMatlReqs(ptse.Time, pts.Inventory.Item);
    }

    private void ShelfLifeReceivedEvent(EventBase a_nextEvent)
    {
        ShelfLifeEvent sle = (ShelfLifeEvent)a_nextEvent;

        WarehouseManager.ExpireLots(sle.Time, sle.ExpirableLots, sle.Item);

        //TODO: If we had a way to track which SAs the lot was in, this could be improved to Retry in just those SAs
        //Note that technically we can check the Lot adjustments to get the storage Areas, but that will make the simulation reliant on Adjustments
        RetryStorageAreaEmptyEvents(sle.Time);
    }

    private void MinAgeEventReceived(EventBase a_nextEvent)
    {
        // The event triggers attempts to schedule at the time the material is old enough.
        if (a_nextEvent is MinAgeEvent minAgeEvent)
        {
            RetryItemMatlReqs(minAgeEvent.Time, minAgeEvent.Item);
        }
    }

    /// <summary>
    /// Attempt to fulfill a sales order line distribution.
    /// </summary>
    /// <param name="a_nextEvent"></param>
    /// <returns></returns>
    private void SalesOrderLineDistributionReceivedEvent(EventBase a_nextEvent)
    {
        SalesOrderLineDistributionEvent slde = (SalesOrderLineDistributionEvent)a_nextEvent;

        SalesOrderLineDistribution dist = slde.SalesOrderLineDistribution;
        if (slde.InitialProcessingForDemand)
        {
            if (dist.UseMustSupplyFromWarehouse)
            {
                Inventory inv = dist.MustSupplyFromWarehouse.Inventories[dist.Item.Id];
                Adjustment demandAdjustment = dist.GenerateDemandAdjustment(inv);
                inv.AddSimulationAdjustment(demandAdjustment);
            }
            else
            {
                foreach (Warehouse wh in WarehouseManager)
                {
                    if (wh.Inventories[dist.Item.Id] is Inventory inv)
                    {
                        //TODO: this isn't quite right. We need a way to select which warehouse to use to satisfy the demand and not pick the first random one.
                        Adjustment demandAdjustment = dist.GenerateDemandAdjustment(inv);
                        inv.AddSimulationAdjustment(demandAdjustment);
                        break;
                    }
                }
            }

            slde.InitialProcessingForDemand = false;
        }

        if (dist.RemainingDemandQty <= 0)
        {
            //The SOD has already been allocated and consumed demand, there is nothing more required.
            return;
        }

        SalesOrderDemandProfile salesOrderDemandProfile = new SalesOrderDemandProfile(dist);
        ConsumeMaterialResult result = ConsumeMatlForDistributionEventEvent(a_nextEvent, salesOrderDemandProfile);
        if (!result.ConsumedMaterial && dist.SalesOrderLine.SalesOrder.Firm)
        {
            if (result.RetryEvent != null)
            {
                //TODO: this is for MinAge that still needs to be implemented
                //Retry 
                AddEvent(result.RetryEvent);
            }

            salesOrderDemandProfile.Item.AddRetryMatlEvt(a_nextEvent);
        }
    }

    internal struct ConsumeMaterialResult
    {
        internal bool ConsumedMaterial;
        internal EventBase RetryEvent;
    }

    /// <summary>
    /// Attempt to consume material for a demands such as salesOrderLineDistributions, and TransferOrderDistributions.
    /// One of 3 things will happen:
    /// 1. If the material is available, it will be consumed and the ActualAvailableTicks will be set.
    /// 2. If the material isn't available, but it's known when it will be, the event will be readded to the event queue so an attempt to consume the material can be made then.
    /// 3. If neither 1 or 2 above, the event is added to a set in the item for events to try to fullfill when inventory is added.
    /// </summary>
    /// <param name="a_nextEvent"></param>
    /// <returns>The date the material became or will become available or -1.</returns>
    private ConsumeMaterialResult ConsumeMatlForDistributionEventEvent(EventBase a_nextEvent, WarehouseDemandProfile a_demandProfile)
    {
        a_demandProfile.GenerateProfile(this);

        bool allocated = ConsumeQtyReq(a_demandProfile, out long matlAvailTicks);
        ConsumeMaterialResult result = new ConsumeMaterialResult() { ConsumedMaterial = false, RetryEvent = null };
        if (!allocated)
        {
            if (matlAvailTicks > a_nextEvent.Time)
            {
                // We know when the required material might be available.

                EventBase retryEvent = (EventBase)a_nextEvent.Clone();
                retryEvent.Time = matlAvailTicks;

                // Recreate the event to try again when the material might be available.
                // Note the material might end up being used by something else.
                //a_nextEvent.Time = matlAvailTicks;
                result = new ConsumeMaterialResult()
                {
                    ConsumedMaterial = false,
                    RetryEvent = retryEvent
                };

            }
            
            //Track First Shortage Date on Inventory
            if (a_demandProfile.MustSupplyFromWarehouse)
            {
                // TODO: Do a proper validation on inventory instead of just the null check
                Inventory inv = a_demandProfile.SupplyWarehouse.Inventories[a_demandProfile.Item.Id];
                inv?.TrackFirstShortageDate(a_nextEvent.Time);
            }
            else
            {
                foreach (Warehouse w in WarehouseManager)
                {
                    Inventory inv = w.Inventories[a_demandProfile.Item.Id];
                    //check null in case inventory is not in this warehouse
                    inv?.TrackFirstShortageDate(a_nextEvent.Time);
                }
            }
        }
        else
        {
            // The demand was fullfilled.
            //TODO: Set this when the material consumed
            a_demandProfile.UpdateActualAvailableTicks(matlAvailTicks);

            result = new ConsumeMaterialResult() { ConsumedMaterial = true, RetryEvent = null };
        }

        return result;
    }

    /// <summary>
    /// Attempt to consume material for a quantity requirement.
    /// </summary>
    /// <param name="a_simClock"></param>
    /// <param name="a_qtyReq">A quantity requirement such as a sales order line distribution or a transfer order distribution.</param>
    /// <returns></returns>
    private bool ConsumeQtyReq(WarehouseDemandProfile a_demandProfile, out long o_matlAvailableTicks)
    {
        MaterialAllocationPlan unconstrainedPlan = new ();
        if (a_demandProfile.MustSupplyFromWarehouse)
        {
            AddPlanForStorageArea(a_demandProfile.SupplyWarehouse, a_demandProfile.Item, ref unconstrainedPlan);
        }
        else
        {
            foreach (Warehouse wh in WarehouseManager)
            {
                AddPlanForStorageArea(wh, a_demandProfile.Item, ref unconstrainedPlan);
            }
        }

        // Filter out storage areas that don't have any available qty
        unconstrainedPlan.OrderStorageAreas(a_demandProfile, this);

        if (unconstrainedPlan.MaxAvailableQty >= a_demandProfile.RemainingQty)
        {
            a_demandProfile.ResetForAllocation();
            unconstrainedPlan.AllocatePlan(a_demandProfile, this);

            //All the allocated demand nodes will now consume their supply.
            List<EventBase> futureConsumptionEvents = new();
            a_demandProfile.ConsumeAllocations(m_simClock, unconstrainedPlan.AvailableStorage, futureConsumptionEvents);
            foreach (EventBase futureConsumptionEvent in futureConsumptionEvents)
            {
                AddEvent(futureConsumptionEvent);
            }

            o_matlAvailableTicks = 0;
            return true;
        }

        //Not available
        o_matlAvailableTicks = unconstrainedPlan.CalculateAvailableDate(a_demandProfile, this, out BaseSupplyProfile _);
        return false;
    }

    private void AddPlanForStorageArea(Warehouse a_wh, Item a_item, ref MaterialAllocationPlan a_unconstrainedPlan)
    {
        //Not constrained by Connectors
        foreach (StorageArea sa in a_wh.StorageAreas)
        {
            if (sa.CanStoreItem(a_item.Id))
            {
                a_unconstrainedPlan.AddStorageArea(sa);
            }
        }
    }

    /// <summary>
    /// Attempt to fullfill Quantity requirements for requirements such as sales order line distributions and
    /// </summary>
    /// <param name="a_simClock"></param>
    private long ConsumeQtyReq(long a_simClock, IMaterialAllocation a_matlAlloc, IQtyRequirement a_qtyReq, Inventory inv)
    {
        //TODO: Storage
        QtyNode earliestAvailableQtyNode;
        QtyNode earliestAvailableFutureQtyNode;
        decimal eligOnHandSimQty;

        //long availTicks = inv.FindDateUnAllocatedQtyWillBeAvailable(a_simClock, this, a_qtyReq, MaterialRequirementOverlapOptions.OverlapAll, a_matlAlloc, a_qtyReq, a_qtyReq, null, a_qtyReq.QtyOpenToShip, out earliestAvailableQtyNode, out earliestAvailableFutureQtyNode, out eligOnHandSimQty);
        //if (availTicks == a_simClock)
        //{
        //    inv.ResetAllocationsForOverlap(a_simClock, this, a_qtyReq, inv, MaterialRequirementOverlapOptions.OverlapAll, a_matlAlloc, a_qtyReq, a_qtyReq, null);

        //    decimal qtyAllocatedFromOnHand;
        //    decimal qtyAllocatedFromExpected;
        //    MaterialRequirementDefs.SupplySource source = new();

        //    inv.Allocate(a_matlAlloc, a_qtyReq.QtyOpenToShip, out qtyAllocatedFromOnHand, out qtyAllocatedFromExpected, ref source);
        //    InventoryAllocation alloc = new(null, inv, 0, qtyAllocatedFromOnHand, qtyAllocatedFromExpected, 0, 0, 0, 0, source, a_simClock, earliestAvailableQtyNode);

        //    //Consume it.
        //    BaseIdObject qtyReqBaseIdObj = (BaseIdObject)a_qtyReq;
        //    inv.Consume(a_simClock, this, qtyReqBaseIdObj, a_matlAlloc, a_qtyReq, a_qtyReq, null, MaterialRequirementOverlapOptions.OverlapAll, alloc);

        //    //Set the actual date.
        //    a_qtyReq.ActualAvailableTicks = a_simClock;
        //    UsedAsEligibleLotsLotCodes.Remove(a_qtyReq);
        //}

        return -1; // availTicks;
    }

    /// <summary>
    /// Signals that the "move to" time has been reached.
    /// Used to make sure the "move to" resources are released in the event the moved activities couldn't be released at the "move to" time.
    /// </summary>
    /// <param name="a_nextEvent"></param>
    private void MoveTicksEventReceived(EventBase a_nextEvent)
    {
        MoveTicksEvent moveTicksEvent = (MoveTicksEvent)a_nextEvent;
        m_moveTicksState = MoveReleaseState.CanBeReleased;
        AddActivityReleaseEventIfReleased(moveTicksEvent.Activity, moveTicksEvent.Time);
    }

    /// <summary>
    /// Create the blocks for a block reservation.
    /// </summary>
    /// <param name="a_nextEvent"></param>
    private void BlockReservationEventReceived(EventBase a_nextEvent)
    {
        // Block reservation events are stored and handled at the same time as activities are scheduled.
        BlockReservationEvent e = (BlockReservationEvent)a_nextEvent;
        m_blockReservations.Add(e);
    }

    private void ResourceDelayedUsageEventReceived(EventBase a_nextEvent)
    {
        // [USAGE_CODE] ResourceDelayedUsageEventReceived(): The activity's WaitForUsageEvent flag is set to false; though it's possible it was never set to true. It's only set to true under certain conditions.
        UsageEvent e = (UsageEvent)a_nextEvent;

        e.Activity.WaitForUsageEvent = false;
    }

    // [USAGE_CODE] MoveIntersectionConstraintEventReceived: Release PreventMoveIntersectionEvent constraints.
    /// <summary>
    /// Release the move constraint.
    /// </summary>
    /// <param name="a_nextEvent"></param>
    private void MoveIntersectionConstraintEventReceived(EventBase a_nextEvent)
    {
        PreventMoveIntersectionEvent e = (PreventMoveIntersectionEvent)a_nextEvent;
        e.Activity.PreventMoveIntersectionEvent = false;
        AddActivityReleaseEventIfReleased(e.Activity, e.Time);
    }

    private void MoveIntersectionEventReceived(EventBase a_nextEvent)
    {
        MoveIntersectionEvent e = (MoveIntersectionEvent)a_nextEvent;

        // Infinite resources: not relevant
        // Multi tasking resources: It can happen if there's enough attention available. The reservation also needs to include attention percent.

        // Intersector: an attempt to schedule a block in the future (UsageStart==Run) that intersects a move reservation on a resource.

        // Save up intersector end times as they're encountered.

        // Once the move block fails to schedule at the move time.
        //  If anything intersected, 
        //      bail out of the simulation, using the earliest intersector end time as the basis for the new move date, and reapply the move 
        //  else
        //      Release the Move lock and allow normal scheduling until the block schedules.


        // Mark the next activity to be moved as MoveIntersectionReached
        // When an attempt is made to schedule this activity
        // If the attempt fails,
        //  Cancel the simulation
        //  Save the IntersectionActivity in the MoveT (perhaps the Intersecting RR data plus the intersector)
        //  When the simulation is reapplied, constrain the move activity until the intersecting activity is scheduled.
    }

    private void ActivityReleasedEventReceived(EventBase a_nextEvent)
    {
        ActivityReleasedEvent activityReleasedEvent = (ActivityReleasedEvent)a_nextEvent;

        ActivityReleased(activityReleasedEvent.Activity, activityReleasedEvent.Time);
    }

    /// <summary>
    /// Process ClockReleaseEvent.
    /// </summary>
    /// <param name="nextEvent"></param>
    private void ClockReleaseEventReceived(EventBase a_nextEvent)
    {
        ClockReleaseEvent nextEvent = (ClockReleaseEvent)a_nextEvent;
        nextEvent.Activity.WaitForClockAdjustmentRelease = false;
        AddActivityReleaseEventIfReleased(nextEvent.Activity, nextEvent.Time);
    }

    private void RightMovingNeighborReleaseEventReceived(EventBase a_nextEvent)
    {
        RightMovingNeighborReleaseEvent nextEvent = (RightMovingNeighborReleaseEvent)a_nextEvent;
        nextEvent.Activity.WaitForRightMovingNeighborReleaseEvent = false;
        MoveReleaseProcessing(nextEvent.Time, nextEvent.Activity);
    }

    private void ScheduledDateBeforeMoveEventReceived(EventBase a_nextEvent)
    {
        ScheduledDateBeforeMoveEvent nextEvent = (ScheduledDateBeforeMoveEvent)a_nextEvent;
        nextEvent.Activity.WaitForScheduledDateBeforeMoveEvent = false;
        MoveReleaseProcessing(nextEvent.Time, nextEvent.Activity);
    }

    private void MoveActivityTimeEventReceived(EventBase a_nextEvent)
    {
        MoveActivityTimeEvent nextEvent = (MoveActivityTimeEvent)a_nextEvent;
        InternalActivity activity = nextEvent.Activity;
        ReserveMoveResources(activity, SimClock, nextEvent.Time);

        activity.WaitForActivityMovedEvent = false;
        if (activity.Released)
        {
            ActivityReleased(activity, nextEvent.Time);
        }
    }

    /// <summary>
    /// Indicates the availability of material for an operation.
    /// </summary>
    /// <param name="matlEvent"></param>
    private void MaterialAvailableEventReceived(EventBase a_nextEvent)
    {
        MaterialAvailableEvent matlEvent = (MaterialAvailableEvent)a_nextEvent;
        if (matlEvent.Operation is InternalOperation)
        {
            InternalOperation operation = (InternalOperation)matlEvent.Operation;

            operation.MaterialRequirements.WaitingOnMaterial = false;
            if (operation.Released)
            {
                OperationReleaseCheckAndHandling(matlEvent.Time, operation, matlEvent.Time, InternalOperation.LatestConstraintEnum.MaterialRequirement, matlEvent.m_latestMR);
            }
        }
    }

    /// <summary>
    /// Handle the completion of a predecessor MO.
    /// The effect of processing this event is that the successor may become eligible for scheduling.
    /// </summary>
    /// <param name="predMOAvailEvent"></param>
    private void PredecessorMOAvailableEventReceived(EventBase a_nextEvent)
    {
        PredecessorMOAvailableEvent predMOAvailEvent = (PredecessorMOAvailableEvent)a_nextEvent;
        BaseOperation sucOp = predMOAvailEvent.SuccessorOperation;

        if (sucOp != null)
        {
            sucOp.NotifyOfPredecessorMOConstraintSatisfaction(a_nextEvent.Time);

            //Check that a path has been released first, otherwise the constraints have not be set for the operations.
            //For example operation Hold constraints and events may not be set which makes the operation's Released property meaningless
            if (sucOp.ManufacturingOrder.PathReleaseEventProcessed && sucOp.Released)
            {
                OperationReleaseCheckAndHandling(predMOAvailEvent.Time, sucOp, InternalOperation.LatestConstraintEnum.PredecessorManufacturingOrder);
            }
        }
        else if (predMOAvailEvent.SuccessorMO != null)
        {
            predMOAvailEvent.SuccessorMO.NotifyOfPredecessorMOConstraintSatisfaction(a_nextEvent.Time);

            if (predMOAvailEvent.SuccessorMO.Released)
            {
                ManufacturingOrderReleased(predMOAvailEvent.SuccessorMO, predMOAvailEvent.Time, InternalOperation.LatestConstraintEnum.PredecessorManufacturingOrder);
            }
        }
    }

    /// <summary>
    /// Controls the earliest start time of activities that cannot begin before the optimization time.
    /// This event is only used in combination with optimization.
    /// </summary>
    /// <param name="nextEvent"></param>
    private void OptimizationReleaseEventReceived(EventBase a_nextEvent)
    {
        OptimizationReleaseEvent nextEvent = (OptimizationReleaseEvent)a_nextEvent;
        BaseActivity ba = nextEvent.Activity;

        ba.WaitForOptimizationReleaseEvent = false;
        if (ba.Released)
        {
            ActivityReleased(ba, nextEvent.Time);
        }
    }

    private void HoldUntilEventReceived(EventBase a_nextEvent)
    {
        HoldUntilEvent holdEvent = (HoldUntilEvent)a_nextEvent;
        holdEvent.Operation.WaitForHoldUntilEvent = false;

        InternalOperation.LatestConstraintEnum latestConstraint;
        switch (holdEvent.HoldType)
        {
            case HoldEnum.Job:
                latestConstraint = InternalOperation.LatestConstraintEnum.JobHoldDate;
                break;

            case HoldEnum.MO:
                latestConstraint = InternalOperation.LatestConstraintEnum.ManufacturingOrderHoldDate;
                break;

            case HoldEnum.Operation:
                latestConstraint = InternalOperation.LatestConstraintEnum.OperationHoldDate;
                break;

            default:
                latestConstraint = InternalOperation.LatestConstraintEnum.Unknown;
                break;
        }

        OperationReleaseCheckAndHandling(holdEvent.Time, holdEvent.Operation, latestConstraint);
    }

    /// <summary>
    /// Indicates that the anchor release date has occurred.
    /// </summary>
    /// <param name="nextEvent"></param>
    private void AnchorReleaseEventReceived(EventBase a_nextEvent)
    {
        AnchorReleaseEvent nextEvent = (AnchorReleaseEvent)a_nextEvent;
        nextEvent.Activity.WaitForAnchorReleaseEvent = false;
        nextEvent.Activity.Operation.SetLatestConstraint(nextEvent.Time, InternalOperation.LatestConstraintEnum.AnchorDate);
        if (nextEvent.Activity.Released)
        {
            AddActivityReleasedEvent(nextEvent.Activity, nextEvent.Time);
        }
    }

    /// <summary>
    /// Release the event at the clock time.
    /// </summary>
    /// <param name="nextEvent"></param>
    private void InProcessReleaseEventReceived(EventBase a_nextEvent)
    {
        InProcessReleaseEvent nextEvent = (InProcessReleaseEvent)a_nextEvent;

        AddActivityReleasedEvent(nextEvent.Activity, nextEvent.Time);
    }

    private void TransferSpanEventReceived(EventBase a_nextEvent)
    {
        TransferSpanEvent nextEvent = (TransferSpanEvent)a_nextEvent;
        AlternatePath.Association association = nextEvent.TransferSpanAssociation;

        association.TransferSpanReleased = true;

        InternalOperation successorOperation = association.Successor.Operation;
        successorOperation.SetOrUpdateTransferInfo(nextEvent.TransferInfo);

        //AddBatchOperationReadyEventForSucOpsNoLongerWaitingForPredBatchOps((InternalOperation)association.Predecessor.Operation, (InternalOperation)successorOperation, nextEvent.Time);

        if (successorOperation.Released)
        {
            if (association.Predecessor != null) //not sure if this is possible, just in case.
            {
                OperationReleaseCheckAndHandling(nextEvent.Time, successorOperation, nextEvent.Time, InternalOperation.LatestConstraintEnum.TransferSpan, association.Predecessor.Operation);
            }
            else
            {
                OperationReleaseCheckAndHandling(nextEvent.Time, successorOperation, InternalOperation.LatestConstraintEnum.TransferSpan);
            }
        }
    }

    /// <summary>
    /// Handle release of predecessor MOs on an operation.
    /// </summary>
    /// <param name="predMOsAvailableEvent"></param>
    private void FinishedPredecessorMOsAvailableEventReceived(EventBase a_nextEvent)
    {
        FinishedPredecessorMOsAvailableEvent predMOsAvailableEvent = (FinishedPredecessorMOsAvailableEvent)a_nextEvent;
        predMOsAvailableEvent.Operation.WaitForFinishedPredecessorMOsAvailableEvent = false;
        if (predMOsAvailableEvent.Operation.Released)
        {
            InternalOperation io = predMOsAvailableEvent.Operation as InternalOperation;
            if (io != null)
            {
                OperationReleaseCheckAndHandling(predMOsAvailableEvent.Time, io, InternalOperation.LatestConstraintEnum.PredecessorManufacturingOrder);
            }
        }
    }

    /// <summary>
    /// Handle the release of an operation that's finished. This is only important when the operation's
    /// reported finish date is greater than the current clock time.
    /// </summary>
    /// <param name="nextEvent"></param>
    private void OperationFinishedEventReceived(EventBase a_nextEvent)
    {
        OperationFinishedEvent nextEvent = (OperationFinishedEvent)a_nextEvent;
        nextEvent.Operation.WaitForOperationFinishedEvent = false;
        OperationReleaseCheckAndHandling(nextEvent.Time, nextEvent.Operation, nextEvent.OperationReadyTicks, InternalOperation.LatestConstraintEnum.FinishTime, null);
    }

    /// <summary>
    /// Events for all inventory lead times are created during initialization.
    /// This is a trigger for any MRs that are waiting for material, it is now available by leadtime.
    /// </summary>
    private void LeadTimeEventReceived(EventBase a_nextEvent)
    {
        LeadTimeEvent nextEvent = (LeadTimeEvent)a_nextEvent;
        if (nextEvent.MaterialRequirement != null)
        {
            //Let the MR know that lead time has occurred. This could release the constraint
            nextEvent.MaterialRequirement.LeadTimeEvent();

            if (nextEvent.CustomizedLeadTime)
            {
                //This is a specific retry for this MR that had an inventory release event different from the standard inventory event
                RetryItemMatlReqs(nextEvent.Time, nextEvent.Item);
            }
        }
        else
        {
            //This is a general event for inventory lead time
            RetryItemMatlReqs(nextEvent.Time, nextEvent.Item);
        }
    }

    /// <summary>
    /// The products produced by this operation go to Inventory.
    /// </summary>
    /// <param name="nextEvent"></param>
    private void UNUSED_EventReceived(EventBase a_nextEvent)
    {
        UNUSED_Event nextEvent = (UNUSED_Event)a_nextEvent;
    }

    private void JobHoldReleasedEventReceived(EventBase a_nextEvent)
    {
        JobHoldReleasedEvent nextEvent = (JobHoldReleasedEvent)a_nextEvent;
        nextEvent.Job.WaitingForJobReleaseEvent = false;
        Job job = nextEvent.Job;

        for (int i = 0; i < job.ManufacturingOrders.Count; ++i)
        {
            ManufacturingOrder mo = job.ManufacturingOrders[i];
            if (mo.Released)
            {
                ManufacturingOrderReleased(mo, nextEvent.Time, InternalOperation.LatestConstraintEnum.JobHoldDate);
            }
        }
    }

    private void ManufacturingOrderHoldReleasedEventReceived(EventBase a_nextEvent)
    {
        ManufacturingOrderHoldReleasedEvent nextEvent = (ManufacturingOrderHoldReleasedEvent)a_nextEvent;
        nextEvent.ManufacturingOrder.WaitingOnHoldReleasedEvent = false;
        if (nextEvent.ManufacturingOrder.Released)
        {
            ManufacturingOrderReleased(nextEvent.ManufacturingOrder, nextEvent.Time, InternalOperation.LatestConstraintEnum.ManufacturingOrderHoldDate);
        }
    }

    private void BlockFinishedEventReceived(EventBase a_nextEvent)
    {
        // Do nothing. This event is used to trigger scheduling of ready resources. In particular
        // it is neccesary for MultiTasking resources. They need to indicate when some percentage
        // of their capacity is free.
    }

    private void SchedulabilityCustomizationEventReceived(EventBase a_nextEvent)
    {
        // Do nothing. This event is used to trigger scheduling of ready resources. For example
        // in the case of Mozzarella Fresca, certain activities aren't eligible for scheduling
        // on the first two moulders during the *18*th hour cleanout of the canals. This makes sure
        // resource scheduling is picked up again at the end of the cleanout.
    }

    private void QtyToStockEventReceived(EventBase a_nextEvent)
    {
        // Release MaterialRequirements that were waiting on the availability of the lot code that has just become available.
#if tfstask10688Disable
            return;
#endif
        // This is being used to disable the code changes for 10688.
        // To reenable them, delete this conditional to enable this code.
        //Note there are 2 conditionals like this in this project. The other being in MatertialRequirement.WaitingOnEligibleLots

        //ConsumeQtyReqs(a_nextEvent.Time);
        QtyToStockEvent qtse = (QtyToStockEvent)a_nextEvent;
        if (qtse.Product?.LotCode != null)
        {
            ReleaseLotCode(qtse.Product.LotCode, qtse.Time, qtse.Item);
        }

        RetryItemMatlReqs(qtse.Time, qtse.Item);
    }

    /// <summary>
    /// Release all of the  material requirements waiting on eligible lots to become available.
    /// This should be called at the end of the planning horizon to allow jobs without material to schedule.
    /// </summary>
    private void PlanningHorizonEventReceived(EventBase a_nextEvent)
    {
        // This event is used to trigger scheduling. When the attmpt occurs if someone
        // tries to use the inventory in this event Inventory.FindQty() will end cause the quantity profile
        // to update what's currently available at the simulation time. That is some quantity may
        // available immediately.
        ReleaseAllEligibleLots();

        //Release all activities that were removed due to material or storage constraints
        foreach (Item item in ItemManager)
        {
            RetryItemMatlReqs(a_nextEvent.Time, item);
            
            RetryItemStorageReqs(a_nextEvent.Time, item);
        }
        
        RetryStorageAreaEmptyEvents(a_nextEvent.Time);
        RetryItemStorageEvents(a_nextEvent.Time);

        //Temporarily replace the global factor min score and append a final Cleanout to the last block on all resources
        foreach (Resource res in PlantManager.ResourcesEnumerator)
        {
            if (res.ActiveDispatcher is BalancedCompositeDispatcher bcd)
            {
                //This will allow activities that couldn't meet the global min score to schedule at the end of the planning horizon
                bcd.BalancedCompositeDispatcherDefinition.SetTempMinScoreForEndOfPlanningHorizon();
            }

            if (res.Blocks.Count > 0)
            {
                res.AppendHorizonCleanout(this);
            }
        }

        m_withinPlanningHorizon = false;
    }

    private void ResourceReservationEventReceived(EventBase a_nextEvent)
    {
        ResourceReservationEvent resRvn = (ResourceReservationEvent)a_nextEvent;
        /// Add the activity to the event queue of the resource.
        /// A simulation will occur and the activity will be scheduled. 
        /// Because the activity is continuous the scheduling of the predecessor operation will not have
        /// caused the release of this successor.
    }

    private void AttemptToScheduleForResRvnEventReceived(EventBase a_nextEvent)
    {
        // Do nothing.
    }

    /// <summary>
    /// Retry scheduling activities that were delayed due to material requirements
    /// </summary>
    /// <param name="a_nextEvent">FutureMaterialAvailableEvent</param>
    /// <returns></returns>
    private void FutureMaterialEventReceived(EventBase a_nextEvent)
    {
        FutureMaterialAvailableEvent materialEvent = (FutureMaterialAvailableEvent)a_nextEvent;
        if (materialEvent.MaterialRetryInfo == null)
        {
            //TODO: What can we use these events for?
            return;
        }

        if (materialEvent.Activity.SimScheduled)
        {
            return; //Already added from another event. This can occur for storage events (since they are tracked by SA) or events at the planning horizon
        }

        foreach (InternalResource previousDispatcher in materialEvent.MaterialRetryInfo.PreviousDispatchers)
        {
            //if(materialEvent.Activity.RemovedFromDispatcherPendingRetry.conta)
            AddToDispatcherHelper(SimClock, (Resource)previousDispatcher, materialEvent.Activity);
            //materialEvent.Activity.RemovedFromDispatcherPendingRetry.Remove(previousDispatcher.Id);
        }

        //A future material source could be available for this activity. For example a PO material is available since the last attempted schedule
        //if (m_materialConstrainedActivities.TryGetValue(materialEvent.Activity, out Plant.FindMaterialResult materialResult))
        //{

        //}
    }
    
    /// <summary>
    /// Retry scheduling activities that were delayed due to material requirements
    /// </summary>
    /// <param name="a_nextEvent">FutureMaterialAvailableEvent</param>
    /// <returns></returns>
    private void FutureStorageEventReceived(EventBase a_nextEvent)
    {
        FutureStorageAvailableEvent storageEvent = (FutureStorageAvailableEvent)a_nextEvent;

        if (storageEvent.Activity.SimScheduled/* || storageEvent.Activity.RemovedFromDispatcherPendingRetry*/)
        {
            return; //Already added from another event. This can occur for storage events (since they are tracked by SA) or events at the planning horizon
        }

        //storageEvent.Activity.RemovedFromDispatcherPendingRetry = true;

        foreach (InternalResource previousDispatcher in storageEvent.PreviousDispatchers)
        {
            AddToDispatcherHelper(SimClock, (Resource)previousDispatcher, storageEvent.Activity);
        }

        //A future material source could be available for this activity. For example a PO material is available since the last attempted schedule
        //if (m_materialConstrainedActivities.TryGetValue(materialEvent.Activity, out Plant.FindMaterialResult materialResult))
        //{

        //}
    }    

    private static void RetryForCleanoutEventReceived(EventBase a_nextEvent) { }

    private static void RetryForConnectorEventReceived(EventBase a_nextEvent) { }

    private static void RetryForHeadStartEndEventReceived(EventBase a_nextEvent) { }

    private void AddConnectorReleaseEvent(ResourceConnector a_connector, long a_eventTime)
    {
        ConnectorReleaseEvent connectorEvent = new (a_eventTime, a_connector);
        if (a_connector.AllocatedInventory != null)
        {
            connectorEvent.AllocatedInventory = a_connector.AllocatedInventory;
        }
        AddEvent(connectorEvent);
    }

    /// <summary>
    /// A Connector is no longer in use
    /// </summary>
    private void ConnectorReleaseEventReceived(EventBase a_nextEvent)
    {
        ConnectorReleaseEvent nextEvent = (ConnectorReleaseEvent)a_nextEvent;
        nextEvent.Connector.ReleaseConnector(a_nextEvent.Time);

        //If this connector was transferring material, other activities may have failed due to the constraint.
        //Retry any failed to schedule activities for this item now that the connector is available.
        if (nextEvent.AllocatedInventory != null)
        {
            RetryItemMatlReqs(nextEvent.Time, nextEvent.AllocatedInventory.Item);
        }
    }

    /// <summary>
    /// Retry scheduling activities that were delayed due to material requirements
    /// </summary>
    /// <param name="a_nextEvent">FutureMaterialAvailableEvent</param>
    /// <returns></returns>
    private void QtyConsumedEventReceived(EventBase a_nextEvent)
    {
        QtyConsumeEvent consumeEvent = (QtyConsumeEvent)a_nextEvent;

        consumeEvent.SupplyNode.Consume(consumeEvent.Qty, false, consumeEvent.Time);

        RetryItemStorageReqs(a_nextEvent.Time, consumeEvent.SupplyNode.SourceLot.Item);
        RetryItemStorageEvents(a_nextEvent.Time, consumeEvent.ItemStorage);

        //Storage Area level check
        if (consumeEvent.SupplyNode.m_currentQty == 0m)
        {
            int currentStoredItemCount = consumeEvent.ItemStorage.StorageArea.UpdateStoredItemsCache();
            if (currentStoredItemCount == 0)
            {
                // Retry operations that depend on empty storage areas
                RetryStorageAreaEmptyEvents(a_nextEvent.Time, consumeEvent.ItemStorage.StorageArea);
            }
        }
    }
    
    /// <summary>
    /// Retry scheduling activities that were delayed due to material requirements
    /// </summary>
    /// <param name="a_nextEvent">FutureMaterialAvailableEvent</param>
    /// <returns></returns>
    private void RetryForEmptyStorageAreaReceived(EventBase a_nextEvent)
    {
        RetryForEmptyStorageAreaEvent storageEvent = (RetryForEmptyStorageAreaEvent)a_nextEvent;

        if (storageEvent.Activity.SimScheduled/* || storageEvent.Activity.RemovedFromDispatcherPendingRetry*/)
        {
            bool found = false;
            foreach (Resource resource in PlantManager.GetResourceArrayList())
            {
                if (resource.ActiveDispatcher.Contains(storageEvent.Activity))
                {
                    found = true;
                }
            }

            if (!found && !storageEvent.Activity.SimScheduled)
            {
                //skipped but not added
            }

            return; //Already added from another event. This can occur for storage events (since they are tracked by SA) or events at the planning horizon
        }

        //storageEvent.Activity.RemovedFromDispatcherPendingRetry = true;

        foreach (InternalResource previousDispatcher in storageEvent.PreviousDispatchers)
        {
            AddToDispatcherHelper(SimClock, (Resource)previousDispatcher, storageEvent.Activity);
        }
    }

    private void RetryStorageAreaEmptyEvents(long a_simClock)
    {
        foreach (Warehouse warehouse in WarehouseManager)
        {
            foreach (StorageArea sa in warehouse.StorageAreas)
            {
                RetryStorageAreaEmptyEvents(a_simClock, sa);
            }
        }
    }

    private void RetryStorageAreaEmptyEvents(long a_simClock, StorageArea a_sa)
    {
        foreach (EventBase eventBase in a_sa.GetRetryStorageEmptyEventSet())
        {
            eventBase.Time = a_simClock;
            AddEvent(eventBase);
        }

        a_sa.ClearRetryStorageEmptyEventSet();
    }
    
    private void RetryItemStorageEvents(long a_simClock)
    {
        foreach (Warehouse warehouse in WarehouseManager)
        {
            foreach (StorageArea sa in warehouse.StorageAreas)
            {
                foreach (ItemStorage itemStorage in sa.Storage)
                {
                    RetryItemStorageEvents(a_simClock, itemStorage);
                }
            }
        }
    }

    private void RetryItemStorageEvents(long a_simClock, ItemStorage a_itemStorage)
    {
        foreach (EventBase eventBase in a_itemStorage.GetRetryStorageEventSet())
        {
            eventBase.Time = a_simClock;
            AddEvent(eventBase);
        }

        a_itemStorage.ClearRetryStorageEventSet();
    }
    
    private void FutureStorageDisposalEventReceived(EventBase a_event)
    {
        FutureStorageDisposalEvent disposalEvent = (FutureStorageDisposalEvent)a_event;
        RetryItemStorageEvents(a_event.Time, disposalEvent.ItemStorage);

        //This is the last adjustment using this supply node, check for disposal. We don't check earlier to prevent disposal before planned adjustments are accounted for.
        //TODO: Track disposal on the node so it's more efficient to return this back
        List<(decimal, QtyNode)> disposedNodes = disposalEvent.ItemStorage.CheckForImmediateDisposal(a_event.Time);
        if (disposedNodes != null)
        {
            foreach ((decimal disposedQty, QtyNode node) in disposedNodes)
            {
                //Create disposal adjustment since we already consumed the material
                Adjustment disposalAdjustment = disposalEvent.DemandSource.GenerateDisposalAdjustment(a_event.Time, disposedQty, ((QtySupplyNode)node).SourceLot, disposalEvent.ItemStorage.StorageArea);
                disposalEvent.ItemStorage.Inventory.AddSimulationAdjustment(disposalAdjustment);
            }

            int currentStoredItemCount = disposalEvent.ItemStorage.StorageArea.UpdateStoredItemsCache();
            if (currentStoredItemCount == 0)
            {
                // Retry operations that depend on empty storage areas
                RetryStorageAreaEmptyEvents(a_event.Time, disposalEvent.ItemStorage.StorageArea);
            }
        }
    }

    private void StorageAreaUsageEventReceived(EventBase a_event)
    {
        StorageAreaUsageEvent usageEvent = (StorageAreaUsageEvent)a_event;
        if (usageEvent.StorageArea != null)
        {
            //Storage Area
            usageEvent.StorageArea.PurgeUsage(usageEvent.Time, usageEvent.Inflow);
        }
        else
        {
            //Connector
            usageEvent.StorageAreaConnector.FlowRangeConstraint.Purge(a_event.Time);
        }
    }
}
