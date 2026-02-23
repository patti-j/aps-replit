using PT.Scheduler.Simulation.Events;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

public partial class Lot
{
    internal void ResetSimulationStateVariables()
    {
        if (m_partialProduction > 0m)
        {
            //If this lot has partial production, we need to keep that on-hand. Remove the Production from the last simulation
            m_partialProduction = 0m;
        }

        m_adjustments.Clear();
    }

    internal void SimulationInitialization(long a_clockDate, ref List<QtyToStockEvent> r_retryEvents)
    {
        if (LotSource == ItemDefs.ELotSource.Inventory && ProductionTicks > a_clockDate)
        {
            ProductionTicks = a_clockDate;
            //TODO: Log this automatic adjustment
        }
        else
        {
            if (ProductionTicks > a_clockDate)
            {
                r_retryEvents.Add(new QtyToStockEvent(ProductionTicks, Inventory.Warehouse, null, Inventory.Item));
            }
        }
        
        m_storageProfile.SimulationInitialization(this, a_clockDate);
    }

    public override string ToString()
    {
        return $"Qty={Qty}; Code={m_code}; ProdDate={ProductionDate}";
    }

    internal void AddAdjustment(Adjustment a_productAdjustment)
    {
        m_adjustments.Add(a_productAdjustment);
    }

    internal void GenerateExpirationAdjustment(decimal a_qty, StorageArea a_storageArea)
    {
        m_adjustments.Add(new LotExpirationAdjustment(this, a_qty, Item.SaveExpiredMaterial, new StorageAdjustment(this, a_storageArea)));
    }
    
    internal void GenerateExpirationDisposalAdjustment(decimal a_qty, StorageArea a_storageArea)
    {
        m_adjustments.Add(new LotExpirationDisposalAdjustment(this, a_qty, new StorageAdjustment(this, a_storageArea)));
    }
}