namespace PT.Scheduler.Simulation;

public class MRSupplyNode
{
    internal MRSupplyNode(BaseIdObject a_supply, QtyNode a_qtyNode, decimal a_qty, bool a_expired, BaseIdObject a_demand)
    {
        m_supply = a_supply;
        m_demand = a_demand;
        QtyNode = a_qtyNode;
        Qty = a_qty;
        Expired = a_expired;
    }

    private readonly BaseIdObject m_supply;
    private readonly BaseIdObject m_demand;

    /// <summary>
    /// This can be an InternalActivity,PurchaseToStock, or Inventory object.
    /// </summary>
    public BaseIdObject Supply => m_supply;
    public BaseIdObject Demand => m_demand;

    internal QtyNode QtyNode { get; private set; }

    private decimal m_qty;

    public decimal Qty
    {
        get => m_qty;
        private set => m_qty = value;
    }

    private bool m_expired;

    public bool Expired
    {
        get => m_expired;
        private set => m_expired = value;
    }

    internal void AddQty(decimal a_qty)
    {
        Qty += a_qty;
    }

    public override string ToString()
    {
        return m_supply + "; Qty=" + Qty;
    }
}