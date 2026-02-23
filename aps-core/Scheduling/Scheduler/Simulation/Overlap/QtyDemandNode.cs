using PT.Common.Debugging;
using PT.Scheduler.Demand;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Events;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

internal class QtyDemandNode : QtyNode
{
    internal QtyDemandNode(QtyDemandNode a_node)
        : base(a_node.Date, a_node.Reason, a_node.m_currentQty)
    {
        m_allocations = new();
        //TODO: SA
        //m_demandSource = new DemandSource(a_node.Reason, a_node.DemandSource);
    }

    /// <summary>
    /// Create a breakoff node that is a copy with a lesser quantity
    /// </summary>
    /// <param name="a_node"></param>
    /// <param name="a_qty"></param>
    internal QtyDemandNode(QtyDemandNode a_node, decimal a_qty)
        : base(a_node.Date, a_node.Reason, a_node.m_currentQty - a_qty)
    {
        m_allocations = new();
        m_demandSource = a_node.DemandSource;
        //TODO: SA
        //m_demandSource = new DemandSource(a_node.Reason, a_node.DemandSource);
    }

    internal QtyDemandNode(long a_date, InternalActivity a_reason, MaterialRequirement a_mrReason, decimal a_qty)
        : base(a_date, a_reason, a_mrReason, a_qty)
    {
        m_allocations = new ();
        m_demandSource = new MaterialRequirementDemandSource(a_reason, a_mrReason);
    }

    internal QtyDemandNode(long a_date, SalesOrderLineDistribution a_reason, decimal a_qty)
        : base(a_date, a_reason, a_qty)
    {
        m_allocations = new();
        m_demandSource = new SalesOrderDemandSource(a_reason); //TODO: Pass this variable in
    }

    internal QtyDemandNode(long a_date, TransferOrderDistribution a_reason, decimal a_qty)
        : base(a_date, a_reason, a_qty)
    {
        m_allocations = new();
        m_demandSource = new TransfersOrderDemandSource(a_reason); //TODO: Pass this variable in
    }

    private List<(QtySupplyNode Node, decimal Qty)> m_allocations;
    internal bool HasSourcedAllocations => m_allocations.Count > 0;
    private (Inventory Inventory, decimal Qty) m_leadTimeAllocation;
    private (Inventory Inventory, decimal Qty, bool PastPlanningHorizon) m_shortageInventory;

    internal IEnumerable<Inventory> AllocatedInventories
    {
        get
        {
            foreach ((QtySupplyNode Node, decimal Qty) allocation in m_allocations)
            {
                if (allocation.Node.SupplyProfile is InventoryProfile profile)
                {
                    yield return profile.Inventory;
                }
            }
        }
    }

    private long m_latestAllocationDate;

    internal long LatestAllocationDate => m_latestAllocationDate;
    
    internal void AllocateSupply(decimal a_qty, QtySupplyNode a_supplyNode)
    {
        m_allocations.Add((a_supplyNode, a_qty));
        if (a_supplyNode.AvailableDate > m_latestAllocationDate)
        {
            m_latestAllocationDate = a_supplyNode.Date;
        }
    }    
    
    /// <summary>
    /// All unallocated qty is converted to a lead time allocation
    /// </summary>
    internal void AllocateRemainingSupplyFromLeadTime(Inventory a_inv)
    {
#if DEBUG
if (UnallocatedQty <= 0m)
{
    throw new DebugException("Demand Node attempted lead time allocation but it was not required");
}
#endif

        m_leadTimeAllocation = new (a_inv, UnallocatedQty);
        Allocate(UnallocatedQty, Date);
    }

    internal void AllocateShortage(Inventory a_inventoryForShortage, bool a_pastPlanningHorizon)
    {
        m_shortageInventory = (a_inventoryForShortage, UnallocatedQty, a_pastPlanningHorizon);
        Allocate(UnallocatedQty, Date);
    }

    internal void ConsumeAllocatedQty(long a_simClock, List<EventBase> a_futureConsumptionEvents, InternalActivity a_demandAct)
    {
        for (int i = 0; i < m_allocations.Count; i++)
        {
            (QtySupplyNode supplyNode, decimal qty) = m_allocations[i];
            StorageProfile supplyProfile = supplyNode.SupplyProfile as StorageProfile;
            //Create a future consumption event
            a_futureConsumptionEvents.Add(new QtyConsumeEvent(Date, supplyNode, qty, supplyProfile.Storage, DemandSource));
            supplyProfile.ConsumeNode(supplyNode, this, qty, true);

            //Add subcomponent relation, except for Tools. Tools model a limited resource, not a subcomponent.
            if (supplyProfile.Inventory.Item.ItemType != ItemDefs.itemTypes.Tool && supplyNode.Reason is InternalActivity iaReason)
            {
                a_demandAct.ManufacturingOrder.UsedAsSubComponent = true;
                a_demandAct.ManufacturingOrder.AddSubComponentSupply(iaReason, a_demandAct, (Product)supplyNode.SupplySource);
            }
        }

        if (DemandSource is MaterialRequirementDemandSource demandSource)
        {
            if (m_leadTimeAllocation.Qty > 0m)
            {
                Consume(m_leadTimeAllocation.Qty, true, Date);
                Adjustment leadTimeAdjustment = demandSource.GenerateLeadTimeAdjustment(m_leadTimeAllocation.Inventory, this, m_leadTimeAllocation.Qty);
                m_leadTimeAllocation.Inventory.AddSimulationAdjustment(leadTimeAdjustment);
            }

            if (m_shortageInventory.Inventory != null)
            {
                Consume(m_shortageInventory.Qty, true, Date);
                Adjustment shortageAdjustment = demandSource.GenerateShortageAdjustment(m_shortageInventory.Inventory, this, m_shortageInventory.Qty, m_shortageInventory.PastPlanningHorizon);
                m_shortageInventory.Inventory.AddSimulationAdjustment(shortageAdjustment);
            }
        }
    }

    /// <summary>
    /// A simpler consumption function for non-job demands
    /// </summary>
    /// <param name="a_simClock"></param>
    /// <param name="a_futureConsumptionEvents"></param>
    internal void ConsumeAllocatedQty(long a_simClock, List<EventBase> a_futureConsumptionEvents)
    {
        for (int i = 0; i < m_allocations.Count; i++)
        {
            (QtySupplyNode supplyNode, decimal qty) = m_allocations[i];
            StorageProfile supplyProfile = supplyNode.SupplyProfile as StorageProfile;
            //Create a future consumption event
            a_futureConsumptionEvents.Add(new QtyConsumeEvent(Date, supplyNode, qty, supplyProfile.Storage, DemandSource));
            supplyProfile.ConsumeNode(supplyNode, this, qty, true);
        }
    }

    private readonly DemandSource m_demandSource;
    internal DemandSource DemandSource => m_demandSource;

    private BaseDemandProfile m_profile;

    internal override void SetProfile(IQtyProfile a_profile)
    {
        m_profile = a_profile as BaseDemandProfile;
    }

    internal QtyDemandNode BreakOff(decimal a_qtyToBreak)
    {
        QtyDemandNode breakOff = new QtyDemandNode(this, a_qtyToBreak);
        m_currentQty -= a_qtyToBreak;
        return breakOff;
    }
}

internal class QtySupplyNode : QtyNode
{
    internal QtySupplyNode(long a_date, long a_availableDate, BaseIdObject a_reason, ISupplySource a_supplySource, decimal a_qty)
        : base(a_date, a_reason, a_qty)
    {
        m_allocations = new ();
        m_availableDate = a_availableDate;
        m_supplySource = a_supplySource;
    }

    internal QtySupplyNode(QtySupplyNode a_node)
        : base(a_node.Date, a_node.Reason, a_node.m_currentQty)
    {
        m_allocations = new();
        m_availableDate = a_node.m_availableDate;
        SourceLot = a_node.SourceLot;
        m_supplyProfile = a_node.m_supplyProfile;
        m_supplySource = a_node.m_supplySource;
    }

    /// <summary>
    /// Create a breakoff node that is a copy with a lesser quantity
    /// </summary>
    /// <param name="a_node"></param>
    /// <param name="a_qty"></param>
    internal QtySupplyNode(QtySupplyNode a_node, decimal a_qty)
        : base(a_node.Date, a_node.Reason, a_node.m_currentQty - a_qty)
    {
        m_allocations = new();
        m_availableDate = a_node.m_availableDate;
        SourceLot = a_node.SourceLot;
        m_supplyProfile = a_node.m_supplyProfile;
        m_supplySource = a_node.m_supplySource;
    }

    //Any allocations that fit into a storage area
    private readonly List<(decimal, ItemStorage)> m_allocations;

    //Any allocations that were discarded because they did not fit into storage
    private (decimal Qty, Inventory Inventory) m_discardAllocation;

    /// <summary>
    /// Whether any storage was allocated. If false, it may not require storage, or it was discarded
    /// </summary>
    internal bool HasStorageAllocation => m_allocations.Count > 0;

    /// <summary>
    /// Allocate some of this node's required storage to a StorageArea
    /// </summary>
    /// <param name="a_qty"></param>
    /// <param name="a_itemStorage"></param>
    internal void AllocateStorage(decimal a_qty, ItemStorage a_itemStorage)
    {
        m_allocations.Add((a_qty, a_itemStorage));
        Allocate(a_qty, Date);
    }

    internal void StoreAllocatedQty(InternalActivity a_act, Product a_product, Lot a_newProductionLot)
    {
        foreach ((decimal qty, ItemStorage itemStorage) in m_allocations)
        {
            itemStorage.StoreProduct(a_act, this, qty, a_product);
            //ResetForAllocation(); Why would we do this. It was here from the beginning, but doesn't make sense.
        }

        ConsumeNonStorageAllocations(a_act);
    }

    internal void ConsumeNonStorageAllocations(InternalActivity a_act)
    {
        if (m_discardAllocation.Inventory != null)
        {
            ProductionDiscardAdjustment discard = new ProductionDiscardAdjustment(m_discardAllocation.Inventory, Date, m_discardAllocation.Qty, a_act);
            m_discardAllocation.Inventory.AddSimulationAdjustment(discard);
        }
    }

    internal override void ResetForAllocation()
    {
        m_discardAllocation.Inventory = null;
        
        foreach ((decimal _, ItemStorage itemStorage) in m_allocations)
        {
            itemStorage.StorageArea.ResetAllocation();
        }
        
        base.ResetForAllocation();
    }

    private readonly ISupplySource m_supplySource;
    internal ISupplySource SupplySource => m_supplySource;

    private IQtyProfile m_supplyProfile;

    internal IQtyProfile SupplyProfile => m_supplyProfile;

    internal void SetLot(Lot a_lot)
    {
        SourceLot = a_lot;
    }

    /// <summary>
    /// The lot that was created from this node's Profile when stored in ItemStorage
    /// It will be null until the profile is stored and will always be null if this node is not in a SupplyProfile
    /// </summary>    
    internal Lot SourceLot;

    internal override void SetProfile(IQtyProfile a_profile)
    {
        m_supplyProfile = a_profile as InventoryProfile;
    }

    internal void AllocateAllToDiscard(Inventory a_inv)
    {
        m_discardAllocation = (UnallocatedQty, a_inv);
    }
    
    internal long ProductionDate => Date;

    private readonly long m_availableDate;
    /// <summary>
    /// The date in which the material is available to be used. It will differ from the ProductionDate (Date) if there is material post-processing
    /// </summary>
    internal long AvailableDate => m_availableDate;

    internal bool Expired => SupplySource.SupplyLot.ShelfLifeData.Expirable && SupplySource.SupplyLot.ShelfLifeData.ExpirationTicks < Date;

    internal QtySupplyNode BreakOff(decimal a_qtyToBreak)
    {
        if (a_qtyToBreak == m_currentQty)
        {
            return this;
        }

        QtySupplyNode breakOff = new QtySupplyNode(this, a_qtyToBreak);
        m_currentQty -= a_qtyToBreak;
        return breakOff;
    }
}

