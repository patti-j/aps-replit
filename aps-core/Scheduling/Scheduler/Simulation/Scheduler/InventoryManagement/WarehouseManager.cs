using PT.APSCommon;
using PT.Scheduler.Demand;
using PT.Scheduler.Simulation.Events;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of Warehouse objects.
/// </summary>
public partial class WarehouseManager
{
    internal void ResetSimulationStateVariables(long a_clock, ScenarioDetail a_sd)
    {
        for (int whI = 0; whI < Count; ++whI)
        {
            Warehouse wh = GetByIndex(whI);
            wh.ResetSimulationStateVariables(a_clock, a_sd);
        }

        whTTInitialization();
    }

    internal void SimulationInitialization(long a_clockDate, ref List<QtyToStockEvent> r_retryEvents)
    {
        foreach (Warehouse warehouse in this)
        {
            warehouse.SimulationInitialization(a_clockDate, ref r_retryEvents);
        }
    }

    internal void SimulationComplete()
    {
        for (int i = 0; i < Count; ++i)
        {
            Warehouse w = GetByIndex(i);
            w.SimulationComplete();
        }

        AddForecastInventoryAdjustments();
    }

    /// <summary>
    /// Add adjustments for forecasts.
    /// </summary>
    internal void AddForecastInventoryAdjustments()
    {
        for (int whI = 0; whI < Count; ++whI)
        {
            Warehouse wh = GetByIndex(whI);
            wh.AllocateForecasts();
        }
    }

    /// <summary>
    /// Initialize the warehouse to warehouse TTs.
    /// </summary>
    internal void whTTInitialization()
    {
        //Assign warehouses transfer time maxtrix index arrays
        for (int whI = 0; whI < Count; ++whI)
        {
            Warehouse wh = this[whI];
            GetByIndex(whI);
            wh.TTIdx = whI;
        }

        m_whTTs = new long[Count, Count];

        // Initially initialize values to long.MaxValue. This allows differentiation between specification of zero TT and an
        // unitialized TT.
        long UNITIALIZED = long.MaxValue;
        for (int x = 0; x < Count; ++x)
        {
            for (int y = 0; y < Count; ++y)
            {
                m_whTTs[x, y] = UNITIALIZED;
            }
        }

        // Initialize the TTs.
        List<WarehouseTT>.Enumerator wttEtr = m_whTTList.GetEnumerator();
        while (wttEtr.MoveNext())
        {
            Warehouse fromWH = GetByExternalId(wttEtr.Current.FromWHExtId);
            Warehouse toWH = GetByExternalId(wttEtr.Current.ToWHExtId);

            // Set the TT  going from the "from" warehouse to the "to" warehouse. In this direction it's always set and won't be overwritten in the opposite direction below.
            m_whTTs[fromWH.TTIdx, toWH.TTIdx] = wttEtr.Current.TT;

            // Initialize going in the reverse direction only if the reverse direction hasn't already been specified; that is don't overwrite the value if it has already been specified.
            if (m_whTTs[toWH.TTIdx, fromWH.TTIdx] == long.MaxValue)
            {
                m_whTTs[toWH.TTIdx, fromWH.TTIdx] = wttEtr.Current.TT;
            }
        }

        // Set uninitialized TTs to 0.
        for (int x = 0; x < Count; ++x)
        {
            for (int y = 0; y < Count; ++y)
            {
                if (m_whTTs[x, y] == UNITIALIZED)
                {
                    m_whTTs[x, y] = 0;
                }
            }
        }
    }

    /// <summary>
    /// Warehouse to Warehouse transfer time matrix.
    /// The matrixes size is equal to the number of warehouses squared.
    /// To find a transfer time like this:
    /// m_whTTs[fromWH.TTIdx, toWH. TTIdx].
    /// </summary>
    private long[,] m_whTTs;

    /// <summary>
    /// This function should be called after all changes to demands have been completed by a transmission.
    /// [DaysOnHand:Synchronization:2]
    /// </summary>
    /// <param name="a_sm"></param>
    internal void ProcessDemandChanges(SalesOrderManager a_sm)
    {
        foreach (Warehouse wh in this)
        {
            wh.ProcessDemandChanges(a_sm);
        }
    }

    /// <summary>
    /// An expirable event has been received for Lots.
    /// Remove the Lots from Inventories
    /// Remove qtyNodes from profiles
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    internal void ExpireLots(long a_simClock, List<Lot> a_expirableLots, Item a_item)
    {
        HashSet<BaseId> lotIds = new(a_expirableLots.Count);

        foreach (Lot lot in a_expirableLots)
        {
            lotIds.Add(lot.Id);
        }

        foreach (Warehouse warehouse in this)
        {
            foreach (StorageArea storageArea in warehouse.StorageAreas)
            {
                storageArea.Storage[a_item.Id]?.ExpireLots(a_simClock, lotIds);
            }
        }
    }
}