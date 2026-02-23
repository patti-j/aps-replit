namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// </summary>
public class QtyConsumeEvent : EventBase
{
    private readonly QtySupplyNode m_supplyNode;
    private readonly decimal m_qty;
    private readonly ItemStorage m_itemStorage;
    private readonly DemandSource m_demandSource;

    /// <summary>
    /// This indicates a storage supply is being consumed by a (at the time) future allocation
    /// </summary>
    internal QtyConsumeEvent(long a_date, QtySupplyNode a_supplyNode, decimal a_qty, ItemStorage a_itemStorage, DemandSource a_demandSource) 
        : base(a_date)
    {
        m_supplyNode = a_supplyNode;
        m_qty = a_qty;
        m_itemStorage = a_itemStorage;
        m_demandSource = a_demandSource;
    }


    internal const int UNIQUE_ID = 55;

    internal override int UniqueId => UNIQUE_ID;

    internal QtySupplyNode SupplyNode => m_supplyNode;

    internal decimal Qty => m_qty;
    
    internal ItemStorage ItemStorage => m_itemStorage;

    internal DemandSource DemandSource => m_demandSource;
}