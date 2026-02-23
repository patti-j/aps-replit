using PT.Scheduler.Demand;
using PT.Scheduler.Simulation.Events;

namespace PT.Scheduler;

/// <summary>
/// Keeps track of all of the Inventories for a specific Warehouse.
/// </summary>
public partial class InventoryManager
{
    internal void ResetSimulationStateVariables(long a_clock, ScenarioDetail a_sd)
    {
        IEnumerator<Inventory> dEnumerator = GetEnumerator();
        while (dEnumerator.MoveNext())
        {
            Inventory inv = dEnumerator.Current;
            inv.ResetSimulationStateVariables(a_clock, a_sd);
        }
    }

    internal void AllocateForecasts()
    {
        IEnumerator<Inventory> dEnumerator = GetEnumerator();
        while (dEnumerator.MoveNext())
        {
            Inventory inv = dEnumerator.Current;
            inv.ForecastsAddToAdjustmentProfile();
        }
    }

    /// <summary>
    /// This function should be called after all changes to demands have been completed by a transmission.
    /// DaysOnHand:Synchronization:4]
    /// </summary>
    /// <param name="a_sm"></param>
    internal void ProcessDemandChanges(SalesOrderManager a_sm)
    {
        foreach (Inventory inv in this)
        {
            inv.CalcDemandTotals(a_sm);
        }
    }

    internal void SimulationInitialization(long a_clockDate, ref List<QtyToStockEvent> r_retryEvents)
    {
        foreach (Inventory inventory in this)
        {
            inventory.SimulationInitialization(a_clockDate, ref r_retryEvents);
        }
    }
}