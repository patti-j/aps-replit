using System.Text;

namespace PT.Scheduler;

/// <summary>
/// When trying to determine if material is available this class is used to describe where to obtain some
/// and in what quantity.
/// </summary>
internal class InventoryAllocation
{
    internal InventoryAllocation(MaterialRequirement a_mr,
                                 Inventory a_inventory,
                                 decimal a_qtyFromIssued,
                                 decimal a_qtyAllocatedFromOnHand,
                                 decimal a_qtyAllocatedFromExpected,
                                 decimal a_qtyAllocatedFromOverlap,
                                 decimal a_nonAllocatedQty,
                                 decimal a_nonallocatedQtySourced,
                                 decimal a_qtyFromLeadTime,
                                 long a_matlReleaseDate,
                                 QtyNode a_earliestAvailableQtyNode)
    {
        m_mr = a_mr;
        m_inventory = a_inventory;

        m_qtyFromIssued = a_qtyFromIssued;
        m_qtyAllocatedFromOnHand = a_qtyAllocatedFromOnHand;
        m_qtyAllocatedFromExpected = a_qtyAllocatedFromExpected;
        m_qtyAllocatedFromOverlap = a_qtyAllocatedFromOverlap;
        m_nonAllocatedQty = a_nonAllocatedQty;
        NonallocatedQtySourced = a_nonallocatedQtySourced;
        QtyFromLeadTime = a_qtyFromLeadTime;

        m_materialReleaseDate = a_matlReleaseDate;

        m_earliestAvailableQtyNode = a_earliestAvailableQtyNode;
    }

    private readonly MaterialRequirement m_mr;

    internal MaterialRequirement MR => m_mr;

    private readonly Inventory m_inventory;

    internal Inventory Inventory => m_inventory;

    private readonly decimal m_qtyFromIssued;

    internal decimal QtyFromIssued => m_qtyFromIssued;

    private readonly decimal m_qtyAllocatedFromOnHand;

    /// <summary>
    /// On hand quantity. Available from the start of the schedule.
    /// </summary>
    internal decimal QtyAllocatedFromOnHand => m_qtyAllocatedFromOnHand;

    private readonly decimal m_qtyAllocatedFromExpected;

    /// <summary>
    /// Quantity that's not available at the clock, but will be available in the future at or before the time processing begins.
    /// </summary>
    internal decimal QtyAllocatedFromExpected => m_qtyAllocatedFromExpected;

    private readonly decimal m_qtyAllocatedFromOverlap;

    internal decimal QtyAllocatedFromOverlap => m_qtyAllocatedFromOverlap;

    private readonly decimal m_nonAllocatedQty;

    /// <summary>
    /// Quantity accounted for by combinations of factors including the material requests constraint type, the inventory's lead time,
    /// the part's lead time, and the end of planning horizon.
    /// </summary>
    internal decimal NonAllocatedQty => m_nonAllocatedQty;

    internal readonly decimal NonallocatedQtySourced;

    /// <summary>
    /// The quantity expected to come from lead-time.
    /// </summary>
    internal readonly decimal QtyFromLeadTime;

    private readonly long m_materialReleaseDate;

    /// <summary>
    /// Approximately how early the material becomes available.
    /// </summary>
    internal long MaterialReleaseDate => m_materialReleaseDate;

    private readonly QtyNode m_earliestAvailableQtyNode;

    /// <summary>
    /// If the quantity is available in inventory, this is set to the first node in the QuanityProfile that has enough material.
    /// </summary>
    internal QtyNode EarliestAvailableQtyNode => m_earliestAvailableQtyNode;

    /// <summary>
    /// The total quantity from available and expected inventory.
    /// </summary>
    /// <returns></returns>
    internal decimal QtyFromAvailAndExpected()
    {
        decimal ttl = m_qtyAllocatedFromExpected + m_qtyAllocatedFromOnHand;
        return ttl;
    }

    public override string ToString()
    {
        StringBuilder sb = new ();

        if (Inventory != null)
        {
            sb.AppendFormat("{0}; ", Inventory);
        }

        sb.AppendFormat("ReleaseDate:{0}", DateTimeHelper.ToLocalTimeFromUTCTicks(MaterialReleaseDate));
        if (QtyFromIssued > 0)
        {
            sb.AppendFormat("; Issued={0}", QtyFromIssued);
        }

        if (QtyAllocatedFromOnHand > 0)
        {
            sb.AppendFormat("; OhHand={0}", QtyAllocatedFromOnHand);
        }

        if (QtyAllocatedFromExpected > 0)
        {
            sb.AppendFormat("; Expected={0}", QtyAllocatedFromExpected);
        }

        if (NonAllocatedQty > 0)
        {
            sb.AppendFormat("; NonalloctedQty={0}", NonAllocatedQty);
        }

        if (NonallocatedQtySourced > 0)
        {
            sb.AppendFormat("; NonallocatedQtySources={0}", NonallocatedQtySourced);
        }

        return sb.ToString();
    }
}