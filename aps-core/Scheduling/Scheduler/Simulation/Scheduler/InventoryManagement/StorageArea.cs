using PT.APSCommon;
using PT.Common.Attributes;
using PT.Common.Debugging;
using PT.Common.PTMath;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Events;

namespace PT.Scheduler;

public partial class StorageArea
{
    //Keeps track of which items are stored during the simulation
    private Dictionary<BaseId, ItemStorage> m_storedItems;

    private FlowRangeConstraint m_inflowRanges;
    private FlowRangeConstraint m_outflowRanges;
    
    /// <summary>
    /// The last item that was stored in the SA for sequence tracking
    /// </summary>
    private ItemStorage m_lastStorage;
    [DoNotAuditProperty]
    internal ItemStorage LastStorage => m_lastStorage;


    internal decimal CalculateAvailableQtyForDemand(BaseDemandProfile a_materialDemandProfile, ScenarioDetail a_sd)
    {
        ItemStorage itemStorage = m_storage[a_materialDemandProfile.Item.Id];
        return itemStorage.CalculateAvailableQtyForDemand(a_materialDemandProfile, a_sd);
    }
    
    internal void ResetSimulationStateVariables()
    {
        Storage.ResetSimulationStateVariables();
        m_storedItems = new Dictionary<BaseId, ItemStorage>();
        m_retryStorageEmptyEvents.Clear();

        m_inflowRanges = null;
        m_outflowRanges = null;
        if (ConstrainInFlow)
        {
            m_inflowRanges = new FlowRangeConstraint(m_storageInFlowLimit);
        }
        else if(ConstrainCounterFlow)
        {
            //We aren't tracking flow limit, but we still need to count individual usages for CounterFlow
            m_inflowRanges = new FlowRangeConstraint(10000);
        }

        if (ConstrainOutFlow)
        {
            m_outflowRanges = new FlowRangeConstraint(m_storageOutFlowLimit);
        }
        else if (ConstrainCounterFlow)
        {
            //We aren't tracking flow limit, but we still need to count individual usages for CounterFlow
            m_outflowRanges = new FlowRangeConstraint(10000);
        }

        m_lastStorage = null;
    }

    internal void FlagStorage(ItemStorage a_itemStorage)
    {
        m_storedItems.TryAdd(a_itemStorage.Id, a_itemStorage);
        m_lastStorage = a_itemStorage;
        #if DEBUG
        if (SingleItemStorage && m_storedItems.Count > 1)
        {
            //throw new DebugException("Single item storage tracking is off track");
        }

        #endif
    }

    internal void FlagStorageRemoval(ItemStorage a_itemStorage)
    {
        m_storedItems.Remove(a_itemStorage.Id);
    }

    /// <summary>
    /// Returns the single item stored for SA with SingleItemStorage
    /// </summary>
    /// <returns>Null if nothing is stored</returns>
    internal ItemStorage? GetSingleStoredItem()
    {
        return Empty ? null : m_storedItems.Values.First();
    }

    internal IEnumerable<ItemStorage> GetStoredItems()
    {
        foreach (ItemStorage itemStorage in m_storedItems.Values)
        {
            yield return itemStorage;
        }
    }


/// <summary>
    /// Whether any items are stored in this storage area
    /// </summary>
    internal bool Empty => m_storedItems.Count == 0;

    /// <summary>
    /// Store material from an on-hand lot
    /// </summary>
    /// <param name="a_lot"></param>
    /// <param name="a_onHandQty"></param>
    internal void StoreOnHandLot(Lot a_lot, ItemStorage a_storage, decimal a_onHandQty)
    {
        //TODO: We need to track the date to keep the latest, or sort the storage ahead of time
        //if (SingleItemStorage && !Empty)
        //{
        //    m_storedItems.
        //}

        a_storage.StoreOnHandLot(a_lot, a_onHandQty);
    }

    /// <summary>
    /// The activities that failed to schedule due to non-empty storage constraints
    /// </summary>
    private readonly List<EventBase> m_retryStorageEmptyEvents = new ();
    
    internal void AddRetryStorageEmptyEvent(EventBase a_retryEvent)
    {
        m_retryStorageEmptyEvents.Add(a_retryEvent);
    }

    /// <summary>
    /// Clear the retry set. You should do this after you've added the events back to the event queue.
    /// </summary>
    internal void ClearRetryStorageEmptyEventSet()
    {
        m_retryStorageEmptyEvents.Clear();
    }

    /// <summary>
    /// Get an enumerator to the retry events.
    /// </summary>
    /// <returns></returns>
    internal IEnumerable<EventBase> GetRetryStorageEmptyEventSet()
    {
        return m_retryStorageEmptyEvents;
    }


    internal bool ValidateInFlow(IInterval a_newStorageRange, out long o_retryDate)
    {
        if (m_inflowRanges == null)
        {
            //Not tracking flow
            o_retryDate = 0;
            return true;
        }
        
        if (!m_inflowRanges.VerifyAllocationRange(a_newStorageRange, out o_retryDate))
        {
            return false;
        }

        bool outFlow = m_outflowRanges.Count > 0;
        if (outFlow)
        {
            if (ConstrainCounterFlow)
            {
                if (m_inflowRanges.Count + m_outflowRanges.Count >= m_counterFlowLimit)
                {
                    List<IInterval> sortedOutflowUsages = m_outflowRanges.GetSortedUsages();
                    if (m_inflowRanges.Count == 0)
                    {
                        int count = m_counterFlowLimit + 1;
                        foreach (IInterval outflowUsage in sortedOutflowUsages)
                        {
                            o_retryDate = outflowUsage.EndTicks;
                            count--;
                            if (count == 0)
                            {
                                break;
                            }
                        }

                        return false;
                    }

                    List<IInterval> sortedInflowUsages = m_inflowRanges.GetSortedUsages();
                    IInterval earliestInFlowUsage = sortedInflowUsages[0];
                    IInterval earliestOutFlowUsage = sortedOutflowUsages[0];
                    if (earliestInFlowUsage.EndTicks < earliestOutFlowUsage.EndTicks)
                    {
                        o_retryDate = earliestInFlowUsage.EndTicks;
                    }
                    else
                    {
                        o_retryDate = earliestOutFlowUsage.EndTicks;
                    }

                    return false;
                }
            }
            else
            {
                //There is outflow, so we can't have inflow
                return false;
            }
        }

        return true;
    }

    internal bool ValidateOutFlow(IInterval a_newStorageRange, out long o_retryDate)
    {
        if (m_outflowRanges == null)
        {
            //Not tracking flow
            o_retryDate = 0;
            return true;
        }
        
        if (!m_outflowRanges.VerifyAllocationRange(a_newStorageRange, out o_retryDate))
        {
            return false;
        }

        bool infFlow = m_inflowRanges.Count > 0;
        if (infFlow)
        {
            if (ConstrainCounterFlow)
            {
                if (m_inflowRanges.Count + m_outflowRanges.Count >= m_counterFlowLimit)
                {
                    List<IInterval> sortedInflowUsages = m_inflowRanges.GetSortedUsages();
                    if (m_outflowRanges.Count == 0)
                    {
                        int count = m_counterFlowLimit + 1;
                        foreach (IInterval inFlowUsages in sortedInflowUsages)
                        {
                            o_retryDate = inFlowUsages.EndTicks;
                            count--;
                            if (count == 0)
                            {
                                break;
                            }
                        }

                        return false;
                    }

                    List<IInterval> sortedOutflowUsages = m_inflowRanges.GetSortedUsages();
                    IInterval earliestOutFlowUsage = sortedOutflowUsages[0];
                    IInterval earliestInFlowUsage = sortedInflowUsages[0];
                    if (earliestInFlowUsage.EndTicks < earliestOutFlowUsage.EndTicks)
                    {
                        o_retryDate = earliestInFlowUsage.EndTicks;
                    }
                    else
                    {
                        o_retryDate = earliestOutFlowUsage.EndTicks;
                    }

                    return false;
                }
            }
            else
            {
                //There is inflow, so we can't have outflow
                return false;
            }
        }

        return true;
    }
    

    internal void ResetAllocation()
    {
        m_inflowRanges?.ResetAllocation();
        m_outflowRanges?.ResetAllocation();
    }

    /// <summary>
    /// Allocate a time point in which this SA is in use. Future Allocations will extend the end date of the usage range
    /// </summary>
    internal void AllocateUsage(long a_usage)
    {
        m_outflowRanges?.AllocateUsage(a_usage);
    }

    /// <summary>
    /// Schedule allocated outflow usage
    /// </summary>
    /// <param name="a_simClock"></param>
    internal long ScheduleUsage()
    {
        if (m_outflowRanges == null)
        {
            return 0;
        }
        
        return m_outflowRanges.ScheduleUsage();
    }

    /// <summary>
    /// Allocate a time point in which this SA is in use. Future Allocations will extend the end date of the usage range
    /// </summary>
    internal void AllocateStorage(long a_usage)
    {
        m_inflowRanges?.AllocateUsage(a_usage);
    }

    /// <summary>
    /// Schedule allocated inflow usage
    /// </summary>
    /// <param name="a_simClock"></param>
    internal long ScheduleStorage()
    {
        if (m_inflowRanges == null)
        {
            return 0;
        }
        
        return m_inflowRanges.ScheduleUsage();
    }

    internal void PurgeUsage(long a_usageEventTime, bool a_usageEventInflow)
    {
        if (a_usageEventInflow)
        {
            m_inflowRanges?.Purge(a_usageEventTime);
        }
        else
        {
            m_outflowRanges?.Purge(a_usageEventTime);
        }
    }

    internal void UnlinkResource(BaseId a_resourceId)
    {
        if (m_resource?.Id == a_resourceId)
        {
            m_resource = null;
        }
    }
}

