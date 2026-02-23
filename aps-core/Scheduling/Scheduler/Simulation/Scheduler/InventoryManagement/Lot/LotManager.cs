using PT.Scheduler.Simulation.Events;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

public partial class LotManager
{
    internal void ResetSimulationStateVariables()
    {
        for (int lotI = Count - 1; lotI >= 0; --lotI)
        {
            Lot lot = this[lotI];
            // Preserve existing inventory and partial production lots
            if (lot.LotSource is ItemDefs.ELotSource.Production or ItemDefs.ELotSource.PurchaseToStock)
            {
                Remove(lot);
            }
            else
            {
                lot.ResetSimulationStateVariables();
            }
        }
    }

    internal void SimulationInitialization(long a_clockDate, ref List<QtyToStockEvent> r_retryEvents)
    {
        foreach (Lot lot in this)
        {
            lot.SimulationInitialization(a_clockDate, ref r_retryEvents);
        }
    }
}