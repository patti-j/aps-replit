using PT.Common.Exceptions;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Customizations.Material;

namespace PT.Scheduler;

/// <summary>
/// Represents when a quantity of material from an Activity, PO, or Inventory becomes available.
/// </summary>
public abstract class QtyNode
{
    /// <summary>
    /// The constructor for activity must include the product.
    /// </summary>
    /// <param name="a_date"></param>
    /// <param name="a_reason"></param>
    /// <param name="a_reasonDetail"></param>
    /// <param name="a_qty"></param>
    internal QtyNode(long a_date, BaseIdObject a_reason, decimal a_qty)
    {
        Date = a_date;
        Reason = a_reason;
        m_originalQty = a_qty;
        m_currentQty = a_qty;
        m_remainingUnAllocatedQty = a_qty;
        m_lastConsumptionDateTicks = a_date;
    }

    internal QtyNode(long a_date, InternalActivity a_act, MaterialRequirement a_mrReason, decimal a_qty)
    {
        Date = a_date;
        Reason = a_act;
        m_originalQty = a_qty;
        m_currentQty = a_qty;
        m_remainingUnAllocatedQty = a_qty;
        m_lastConsumptionDateTicks = a_date;
    }

    internal QtyNode(long a_date, Inventory a_invReason, decimal a_qty)
    {
        Date = a_date;
        Reason = a_invReason;
        m_originalQty = a_qty;
        m_currentQty = a_qty;
        m_lastConsumptionDateTicks = a_date;
        Type = SupplyTypeEnum.Inventory;
        //Init();
    }

    internal QtyNode(long a_date, Demand.TransferOrderDistribution a_toReason, decimal a_qty)
    {
        Date = a_date;
        Reason = a_toReason;
        m_originalQty = a_qty;
        m_currentQty = a_qty;
        Type = SupplyTypeEnum.TransferOrder;
        SourceQtyDataReason = a_toReason;
        m_lastConsumptionDateTicks = a_date;
        //Init();
    }

    /// <summary>
    /// Create a deep copy of the QtyNode.
    /// </summary>
    /// <param name="a_sourceQtyNode"></param>
    internal QtyNode(QtyNode a_sourceQtyNode)
    {
        Date = a_sourceQtyNode.Date;
        Reason = a_sourceQtyNode.Reason;
        m_originalQty = a_sourceQtyNode.m_originalQty;
        m_currentQty = a_sourceQtyNode.m_currentQty;
        Type = a_sourceQtyNode.Type;
        SourceQtyDataReason = a_sourceQtyNode.SourceQtyDataReason;
        m_lastConsumptionDateTicks = a_sourceQtyNode.Date;
        //Init();
    }

    /// <summary>
    /// The time the PO arrived, container of an activity's material is delivered, or inventory was produced.
    /// </summary>
    private long m_date;

    public long Date
    {
        get => m_date;
        set
        {
            if (m_date != 0 && m_date != value)
            {
                throw new Exception("Ouch.QtyNode.Date shouldn't be changed after initially being set.");
            }

            m_date = value;
        }
    }

    /// <summary>
    /// Null if the Reason doesn't implement ISourceQtyData
    /// </summary>
    public ISourceQtyData SourceQtyDataReason { get; private set; }

    /// <summary>
    /// Where the quantity comes from. This can be either a PO, an activity, or inventory.
    /// </summary>
    internal BaseIdObject Reason { get; private set; }

    /// <summary>
    /// Whether the reason object is a PO, Activity, or Inventory object.
    /// </summary>
    internal SupplyTypeEnum Type { get; private set; }

    /// <summary>
    /// The amount of material available in the node.
    /// As allocations are consumed, this quantity will decrease.
    /// </summary>
    internal decimal m_currentQty;

    /// <summary>
    /// The original supply qty for this node before any allocations and consumptions
    /// </summary>
    internal decimal m_originalQty;

    /// <summary>
    /// The Qty that is remaining to allocate to future demand. This will differ from the Original qty by the amount that has been reserved.
    /// It will differ from the Current Qty by the amount that has been consumed.
    /// </summary>
    internal decimal m_remainingUnAllocatedQty;

    private decimal m_unallocatedQty;

    /// <summary>
    /// The qty that can be used for new allocations. It is reset to the remaining UnAllocatedQty when the QtyNode is reset for allocation.
    /// </summary>
    internal decimal UnallocatedQty => m_unallocatedQty;

    /// <summary>
    /// The date ticks of the last consumption on this qty node.
    /// </summary>
    private long m_lastConsumptionDateTicks;

    internal long LastConsumptionDate => m_lastConsumptionDateTicks;

    #if DEBUG
    protected bool d_allocated;
    protected bool d_testAllocated;
    #endif
    /// <summary>
    /// Allocate the specified quantity. It's presumed you already know that there is enough quantity available.
    /// </summary>
    /// <param name="a_allocationQty">The quantity to allocate.</param>
    /// <param name="a_date">The date the allocation is to occur.</param>
    /// <param name="a_connectedResource"></param>
    internal void Allocate(decimal a_allocationQty, long a_date)
    {
        #if DEBUG
        d_allocated = true;

        if (d_testAllocated)
        {
            throw new PTException("There can't be allocations and test allocations.");
        }
        #endif

        m_unallocatedQty -= a_allocationQty;

        #if DEBUG
        if (m_unallocatedQty < 0 && !MathStatics.IsZero(m_unallocatedQty))
        {
            throw new PTException("The remaining unallocated quantity is less than 0");
        }
        #endif
    }

    /// <summary>
    /// Allocate all the of unallocated quantity.
    /// </summary>
    /// <param name="a_simClock">The date the allocation is to occur.</param>
    /// <param name="a_connectedResource"></param>
    internal void AllocateAll(long a_simClock, Inventory a_inventory)
    {
        #if DEBUG
        d_allocated = true;

        if (d_testAllocated)
        {
            throw new PTException("There can't be allocations and test allocations.");
        }
        #endif
        m_unallocatedQty = 0;
    }

    internal void ReserveAllocation(decimal a_qty)
    {
        //TODO: Track the reservation time. Add a new function to get available qty at date.
        m_remainingUnAllocatedQty -= a_qty;
    }

    internal void Consume(decimal a_qty, bool a_consumeReservation, long a_consumeDate)
    {
        if (SourceQtyDataReason != null)
        {
            SourceQtyDataReason.SourceQtyData.QtyConsumed = true;
            SourceQtyDataReason.SourceQtyData.AvailableQty = m_unallocatedQty;
        }

        //if (Lot != null)
        //{
        //    Lot.UnallocatedQty = m_unallocatedQty;
        //}

        if (a_consumeDate != PTDateTime.InvalidDateTimeTicks)
        {
            m_lastConsumptionDateTicks = a_consumeDate;
        }

        m_currentQty -= a_qty;
        if (a_consumeReservation)
        {
            ReserveAllocation(a_qty);
        }

#if DEBUG
        if (m_currentQty < 0m)
        {
            //throw new PTException("Invalid qty node consumption. Consumed more than remained");
        }
#endif
    }

    /// <summary>
    /// Reduces the the unallocated quantity. But doesn't perform all actions necessary for a full allocation.
    /// </summary>
    /// <param name="a_allocationQty">The quantity to allocate.</param>
    internal void TestAllocate(decimal a_allocationQty)
    {
        #if DEBUG
        d_testAllocated = true;

        if (d_allocated)
        {
            //throw new PTException("There can't be test allocations and allocations.");
        }
        #endif

        m_unallocatedQty -= a_allocationQty;
    }

    /// <summary>
    /// The quantity that has been allocated: qty-unallocatedQty. During an allocation you can use this value to determine how much material is planned to be used by the activity that is in the process of
    /// being scheduled.
    /// </summary>
    internal decimal UsedQty => m_currentQty - m_unallocatedQty;

    #region Allocation date and times.
   

    /// <summary>
    /// Adjusts the input parameters based on the allocations. See the description of each parameter for more information.
    /// </summary>
    /// <param name="a_mrSupply">Adds the source of supply and quantity.</param>
    /// <param name="a_adjArray">For each allocation, an adjustment is added.</param>
    /// <param name="a_reason">The activity's MO configured as a sub component of this </param>
    //TODO: Storage: pass in storage area
    internal void AddAdjustments(MRSupply a_mrSupply, AdjustmentArray a_adjArray, BaseIdObject a_reason, bool a_expired, StorageArea a_storeArea = null)
    {
        //int count = AllocationCount;
        //decimal qty = 0;

        ////TODO: [Expiration] add expiration to the adjustments
        //for (int i = 0; i < count; ++i)
        //{
        //    qty += m_allocationQtyList[i];
        //    Adjustment adj;
        //    if (MaterialRequirementDefs.SupplySource.SupplyLot == null)
        //    {
        //        //adj = new Adjustment(m_allocationDateList[i], a_reason, -m_allocationQtyList[i], Reason, a_storeArea);
        //    }
        //    else
        //    {
        //        //adj = new StorageAdjustment(m_allocationDateList[i], a_reason, SupplySource.SupplyLot, -m_allocationQtyList[i], Reason, null, a_storeArea);
        //    }

        //    //a_adjArray.Add(adj);
        //}

        //if (a_mrSupply != null)
        //{
        //    a_mrSupply.Add(Reason, this, qty, a_expired, a_reason);
        //}
    }
    #endregion
    
    internal virtual void ResetForAllocation()
    {
        m_unallocatedQty = m_remainingUnAllocatedQty;
        #if DEBUG
        d_allocated = false;
        d_testAllocated = false;
        #endif
    }
    
    internal static int SortComparer(QtyNode a_x, QtyNode a_y)
    {
        return Comparer<long>.Default.Compare(a_x.Date, a_y.Date);
    }

    #region DEBUG
    public override string ToString()
    {
        return de;
    }

    internal string de
    {
        get
        {
            string lotCode = "";
            return string.Format("{0} @ {1}; UnallocatedQty={2}; LotCode={3}", m_currentQty, Date, m_unallocatedQty, lotCode);
        }
    }
    #endregion

    private BoolVector32 m_bools;
    private const int c_removedIdx = 0;

    /// <summary>
    /// Set to true if the node has been removed from a quantity profile.
    /// </summary>
    internal bool Removed
    {
        get => m_bools[c_removedIdx];
        set
        {
            m_bools[c_removedIdx] = value;
        }
    }

    internal abstract void SetProfile(IQtyProfile a_profile);

    //TODO: Covarient overloads don't work here for some reason on client computers. It can't load the type, so don't inherit for now
    //internal abstract QtyNode BreakOff(decimal a_qtyToBreak);

    /// <summary>
    /// Update the node quantity, for example instead of creating another node at the same time
    /// </summary>
    /// <param name="a_additionalQty"></param>
    internal void UpdateQty(decimal a_additionalQty)
    {
        m_currentQty += a_additionalQty;
        m_remainingUnAllocatedQty += a_additionalQty;
    }
}

internal interface IQtyNode
{
    long Date { get; }

    //QtyProfile Profile { get; set; }
}