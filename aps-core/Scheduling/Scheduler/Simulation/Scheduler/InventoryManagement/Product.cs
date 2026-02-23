using PT.APSCommon;
using PT.Scheduler.Simulation.Customizations.Material;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

public partial class Product : ISourceQtyData, ISupplySource
{
    /// <summary>
    /// For example a ratio of 3/2 and quantity of 10 would result in a quantity of 15.
    /// </summary>
    internal void AdjustOutputQty(decimal a_ratio, decimal a_newRequiredMOQty, ScenarioOptions a_so, bool a_ignoreFixedQty)
    {
        if (FixedQty && !a_ignoreFixedQty)
        {
            return; // This Product is protected against adjustments
        }

        TotalOutputQty = a_so.RoundQtyWithZeroCheck(a_ratio * TotalOutputQty, a_newRequiredMOQty);
    }

    private readonly SourceQtyData m_sourceQtyData = new ();

    /// <summary>
    /// A simulation only value used to keep track of quantity for some of the IMaterialAllocation functionality.
    /// </summary>
    public SourceQtyData SourceQtyData => m_sourceQtyData;

    internal void ResetSimulationStateVariables()
    {
        m_sourceQtyData.ResetSimulationStateVariables();
        if (m_producedLot != null && m_producedLot.LotSource == ItemDefs.ELotSource.PartialProduction)
        {
            PartialProductionLotId = m_producedLot.Id;
        }
        else
        {
            PartialProductionLotId = BaseId.NULL_ID;
        }
        m_producedLot = null;
    }

    public override string ToString()
    {
        return string.Format("Product for {0}; Qty={1}; AvailableTiming={2}", Inventory == null ? "" : Inventory.ToString(), TotalOutputQty, InventoryAvailableTiming);
    }

    private Lot m_producedLot;
    public Lot SupplyLot => m_producedLot;

    internal BaseId PartialProductionLotId = BaseId.NULL_ID;

    public void LinkCreatedLot(Lot a_newLot)
    {
        m_producedLot = a_newLot;
    }

    //Products don't discard
    public Adjustment GenerateDiscardAdjustment(long a_simClock, decimal a_decimal)
    {
        throw new NotImplementedException();
    }
}