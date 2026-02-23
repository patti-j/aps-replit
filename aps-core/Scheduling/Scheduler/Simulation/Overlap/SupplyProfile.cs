using PT.APSCommon;
using PT.Common.Debugging;
using PT.Common.Extensions;
using PT.Common.PTMath;
using PT.Scheduler.Demand;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Events;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// A generic supply profile that combines inventory or is used to merge nodes. It is not produced directly by any source
/// </summary>
internal class BaseSupplyProfile : QtyProfile<QtySupplyNode>
{
    internal long CalculateAvailableDateForDemand(BaseDemandProfile a_materialDemandProfile, ScenarioDetail a_sd)
    {
        long availableDate = PTDateTime.InvalidDateTime.Ticks;
        if (!HasNodes)
        {
            return availableDate;
        }

        ResetForAllocation();
        a_materialDemandProfile.ResetForAllocation();

        //Now calculate qty based on usable nodes (they are all eligible).
        IEnumerator<QtySupplyNode> supplyEnumerator = GetEnumerator();

        QtySupplyNode supplyNode = supplyEnumerator.Current;

        using IEnumerator<QtyNode> demandEnumerator = a_materialDemandProfile.GetEnumerator();

        while (demandEnumerator.MoveNext())
        {
            QtyNode demandNode = demandEnumerator.Current;
            while (demandNode.UnallocatedQty > 0m)
            {
                decimal requiredQty = demandNode.UnallocatedQty;

                if (a_materialDemandProfile.IsSupplySourceEligible(supplyNode, a_sd))
                {
                    //We can use this node
                    decimal unallocatedSupply = supplyNode.UnallocatedQty;

                    if (unallocatedSupply >= requiredQty)
                    {
                        supplyNode.TestAllocate(requiredQty);
                        demandNode.TestAllocate(requiredQty);
                        availableDate = supplyNode.AvailableDate;
                        continue; //We satisfied the demand node, and can use more on the next demand node
                    }

                    if (unallocatedSupply > 0)
                    {
                        //We can't supply the full required qty, so use what is remaining
                        availableDate = supplyNode.AvailableDate;
                        supplyNode.TestAllocate(unallocatedSupply);
                        demandNode.TestAllocate(unallocatedSupply);
                    }
                }

                //Move to the next supply node
                if (!supplyEnumerator.MoveNext())
                {
                    //This demand node was not satisfied, return what we were able to use
                    return PTDateTime.InvalidDateTime.Ticks;
                }

                supplyNode = supplyEnumerator.Current;
            }
        }

        return availableDate;
    }
}

/// <summary>
/// Represents a filtered profile that only contains nodes that can supply material for a specified demand
/// </summary>
internal class InventoryProfile : BaseSupplyProfile
{
    protected readonly Inventory m_inventory;

    internal InventoryProfile(Inventory a_inv)
    {
        m_inventory = a_inv;
    }

    internal Inventory Inventory => m_inventory;
}

//TODO: Rename StorageProfile and add InventoryProfile as a new base class so that BaseSupplyProfile does not need to ref inventory
internal class StorageProfile : InventoryProfile
{
    private readonly ItemStorage m_storage;

    internal StorageProfile(Inventory a_inv, ItemStorage a_storage)
        : base(a_inv)
    {
        m_storage = a_storage;
    }

    internal StorageProfile(StorageProfile a_profile)
        : base(a_profile.Inventory)
    {
        m_storage = a_profile.m_storage;

        foreach (QtySupplyNode node in a_profile)
        {
            AddToEnd(new QtySupplyNode(node));
        }
    }

    internal ItemStorage Storage => m_storage;

    internal override QtySupplyNode CreateQtyNode(QtySupplyNode a_node)
    {
        return new QtySupplyNode(a_node);
    }

    internal override QtyProfile<QtySupplyNode> CopyProfile()
    {
        return new StorageProfile(this);
    }

    internal void ConsumeNode(QtySupplyNode a_supplyNode, QtyDemandNode a_demandNode, decimal a_qty, bool a_reserveOnly)
    {
        if (a_reserveOnly)
        {
            a_supplyNode.ReserveAllocation(a_qty);
        }
        else
        {
            a_supplyNode.Consume(a_qty, true, a_demandNode.Date);

        }

        if (a_supplyNode.SourceLot is Lot supplyLot)
        {
            Adjustment newAdjustment = a_demandNode.DemandSource.GenerateAdjustment(a_demandNode.Date, -a_qty, supplyLot, m_storage.StorageArea, a_supplyNode.AvailableDate);
            supplyLot.AddAdjustment(newAdjustment);
        }
    }

    internal decimal CalculateProjectedQty(long a_materialDemandDate)
    {
        //TODO: Cache this with some sort of 'Sorted' type function that keeps track of when the profile is pruned.
        decimal currentQty = this.Sum(n => n.m_currentQty);
        return currentQty;
    }
}

internal class SupplyProfile : InventoryProfile
{
    private readonly ILotGeneratorObject m_lotSource;

    internal ILotGeneratorObject LotSource => m_lotSource;

    internal readonly bool RequireEmptyStorageArea;

    /// <summary>
    /// Gets a value indicating whether items from the same lot are allowed to be placed in a storage area that is not
    /// empty.
    /// </summary>
    internal bool AllowSameLotInNonEmptyStorageArea { get; private set; }
    internal readonly StorageArea RequiredStorageArea;

    internal SupplyProfile(ISupplySource a_supplySource, Inventory a_inv, ILotGeneratorObject a_lotSource, bool a_requireEmptyStorageArea, StorageArea a_requiredStorageArea)
        : base(a_inv)
    {
        m_lotSource = a_lotSource;
        m_supplySource = a_supplySource;
        RequireEmptyStorageArea = a_requireEmptyStorageArea;
        RequiredStorageArea = a_requiredStorageArea;
    }

    internal SupplyProfile(SupplyProfile a_profile)
        : base(a_profile.Inventory)
    {
        m_lotSource = a_profile.LotSource;
        m_supplySource = a_profile.m_supplySource;

        foreach (QtySupplyNode node in a_profile)
        {
            AddToEnd(new QtySupplyNode(node));
        }
    }

    protected void SetAllowSameLotInNonEmptyStorageArea()
    {
        AllowSameLotInNonEmptyStorageArea = true;
    }

    private readonly ISupplySource m_supplySource;

    internal ISupplySource Source => m_supplySource;

    internal override QtySupplyNode CreateQtyNode(QtySupplyNode a_node)
    {
        return new QtySupplyNode(a_node);
    }

    /// <summary>
    /// The date must be greater than any other element in the profile.
    /// </summary>
    internal QtySupplyNode AddToEnd(long a_time, InternalActivity a_reason, Product a_product, decimal a_qty, Resource a_primaryResource)
    {
        long materialPostProcessing = a_product.MaterialPostProcessingTicks;
        if (a_reason.Operation.Products.PrimaryProduct == a_product)
        {
            materialPostProcessing = a_reason.GetResourceProductionInfo(a_primaryResource).MaterialPostProcessingSpanTicks;
        }
        
        QtySupplyNode node = new QtySupplyNode(a_time, a_time + materialPostProcessing, a_reason, a_product, a_qty);
        AddToEnd(node);
        return node;
    }

    /// <summary>
    /// Add a node for a purchase to stock to the end of this profile.
    /// </summary>
    /// <param name="a_time">The date must be greater than the last node in this profile.</param>
    /// <param name="a_reason">A purchase to stock</param>
    /// <param name="a_qty">The quantity being added.</param>
    /// <returns></returns>
    internal QtySupplyNode AddToEnd(long a_time, PurchaseToStock a_reason, decimal a_qty)
    {
        TestStructure();
        QtySupplyNode node = new QtySupplyNode(a_time, a_reason.AvailableDateTicks, a_reason, a_reason, a_qty);
        AddToEnd(node);
        TestStructure();
        return node;
    }

    /// <summary>
    /// Add a node for a purchase to stock to the end of this profile.
    /// </summary>
    /// <param name="a_time">The date must be greater than the last node in this profile.</param>
    /// <param name="a_reason">A purchase to stock</param>
    /// <param name="a_qty">The quantity being added.</param>
    /// <returns></returns>
    internal QtySupplyNode AddToEnd(long a_time, TransferOrderDistribution a_reason, decimal a_qty)
    {
        TestStructure();
        QtySupplyNode node = new QtySupplyNode(a_time, a_reason.ScheduledReceiveDateTicks, a_reason, a_reason, a_qty);
        AddToEnd(node);
        TestStructure();
        return node;
    }

    /// <summary>
    /// If the time is the same as the last node, increment it's quantity, otherwise add a new node to the end of the profile.
    /// </summary>
    /// <param name="a_time"></param>
    /// <param name="a_supplySource"></param>
    /// <param name="a_qty"></param>
    internal void AddToEndOrIncrementEnd(long a_time, InternalActivity a_act, Product a_product, decimal a_qty, Resource a_primaryResource)
    {
        TestStructure();
        if (HasNodes && Last.Date == a_time)
        {
            Last.UpdateQty(a_qty);
        }
        else
        {
            long materialPostProcessing = a_product.MaterialPostProcessingTicks;
            if (a_act.Operation.Products.PrimaryProduct == a_product)
            {
                materialPostProcessing = a_act.GetResourceProductionInfo(a_primaryResource).MaterialPostProcessingSpanTicks;
            }
            QtySupplyNode node = new QtySupplyNode(a_time, a_time + materialPostProcessing, a_act, a_product, a_qty);
            AddToEnd(node);
        }

        TestStructure();
    }

    internal void SetLot(Lot a_lot)
    {
        foreach (QtySupplyNode node in this)
        {
            node.SetLot(a_lot);
        }
    }

    /// <summary>
    /// Reduce the profile by the specified quantity. This will be discarded and never allocated.
    /// </summary>
    /// <param name="a_simClock"></param>
    /// <param name="a_excess"></param>
    internal void DiscardExcess(long a_simClock, ISupplySource a_reason, decimal a_excess)
    {
        QtyProfileReverseEnumerator<QtySupplyNode> reverseEnumerator = new (this);

        while (a_excess > 0m & reverseEnumerator.MoveNext())
        {
            QtyNode node = reverseEnumerator.Current;
            if (a_excess >= node.UsedQty)
            {
                node.Consume(node.UsedQty, true, PTDateTime.InvalidDateTimeTicks);
                a_excess -= node.UsedQty;
            }
            else
            {
                node.Consume(a_excess, true, PTDateTime.InvalidDateTimeTicks);
                break;
            }
        }

        Adjustment adjustment = a_reason.GenerateDiscardAdjustment(a_simClock, -a_excess);
        m_inventory.Adjustments.Add(adjustment);
    }

    HashSet<ItemStorage> m_allocatedStorage = new ();
    internal void AllocateSupply(decimal a_qty, QtySupplyNode a_qtyNode, ItemStorage a_itemStorage)
    {
        a_qtyNode.AllocateStorage(a_qty, a_itemStorage);
        RemainingQty -= a_qty;
        m_allocatedStorage.AddIfNew(a_itemStorage);
        AllocateStorageAreaStorage(a_qtyNode.Date, a_itemStorage.StorageArea);
    }

    internal void AllocateFinalSupply()
    {
        LastAllocatedStorageArea?.AllocateStorage(GetUsageRange().EndTicks);
    }

    private StorageArea LastAllocatedStorageArea;
    private long m_lastStorageAllocationTime; //store as the start of usage if we continue storing in multiple storage areas
    private void AllocateStorageAreaStorage(long a_saTimePoint, StorageArea a_sa)
    {
        if (LastAllocatedStorageArea == null)
        {
            //Allocate the start of the supply, not the first transfer
            a_sa.AllocateStorage(GetUsageRange().StartTicks);
        }
        else if (LastAllocatedStorageArea != a_sa)
        {
            a_sa.AllocateStorage(m_lastStorageAllocationTime);
        }

        m_lastStorageAllocationTime = a_saTimePoint;
        a_sa.AllocateStorage(a_saTimePoint);
        LastAllocatedStorageArea = a_sa;
    }

    internal override void ResetForAllocation()
    {
        LastAllocatedStorageArea = null;
        foreach (ItemStorage itemStorage in m_allocatedStorage)
        {
            itemStorage.StorageArea.ResetAllocation();
        }
        base.ResetForAllocation();
    }

    //Any allocations that cause the disposal of qty from storage
    private (decimal, ItemStorage) m_disposalAllocations = (0m, null);

    /// <summary>
    /// Allocate the qty that must be disposed before we can store supply
    /// </summary>
    internal void AllocateDisposal(decimal a_qty, ItemStorage a_itemStorage)
    {
        m_disposalAllocations = (-a_qty, a_itemStorage);
    }

    private (IInterval, ItemStorage) m_cleanoutInterval = (null, null);

    /// <summary>
    /// Allocate the qty that must be disposed before we can store supply
    /// </summary>
    internal void AllocateCleanout(IInterval a_cleanoutInterval, ItemStorage a_itemStorage)
    {
        m_cleanoutInterval = (a_cleanoutInterval, a_itemStorage);
    }

    /// <summary>
    /// Create adjustments and consume allocations that did not result in storage. For example discard
    /// </summary>
    /// <param name="a_act"></param>
    internal void ConsumeNonStorageAllocations(InternalActivity a_act)
    {
        foreach (QtySupplyNode node in this)
        {
            node.ConsumeNonStorageAllocations(a_act);
        }
    }

    internal void StoreProduct(InternalActivity a_act, long a_simClock, Product a_product, Lot a_newProductionLot, List<EventBase> a_usageEvents)
    {
        ItemStorage storage = m_disposalAllocations.Item2;
        storage?.DisposeAll(a_act, a_newProductionLot);

        storage = m_cleanoutInterval.Item2;
        if (storage != null)
        {
            storage.GenerateCleanoutAdjustment(m_cleanoutInterval.Item1);
        }

        foreach (QtySupplyNode node in this)
        {
            node.StoreAllocatedQty(a_act, a_product, a_newProductionLot);
        }
        
        //Allocate usage
        foreach (ItemStorage itemStorage in m_allocatedStorage)
        {
            long usageEnd = itemStorage.StorageArea.ScheduleStorage();
            if (usageEnd > 0)
            {
                a_usageEvents.Add(new StorageAreaUsageEvent(usageEnd, itemStorage.StorageArea, true));
            }
        }
        
        //If a connector was required, schedule it's allocated usage
        long? scheduleUsage = Connector?.FlowRangeConstraint.ScheduleUsage();
        if (scheduleUsage is > 0)
        {
            a_usageEvents.Add(new StorageAreaUsageEvent(scheduleUsage.Value, Connector));
        }
    }

    internal decimal TotalRequiredQty;
    internal decimal RemainingQty;

    internal bool SupplyAllocated => RemainingQty == 0m;

    internal bool HasStorageAllocation => this.Any(node => node.HasStorageAllocation);

    internal virtual IInterval GetUsageRange() => new Interval(First.Date, Last.Date);

    internal StorageAreaConnector Connector { get; set; }

    internal virtual BaseId GetSupplyId()
    {
        return Inventory.Id;
    }
}

internal class ProductSupplyProfile : SupplyProfile
{
    private readonly Product m_product;
    private readonly InternalActivity m_act;
    private readonly SchedulableInfo m_si;

    internal ProductSupplyProfile(Product a_product, InternalActivity a_activity, Inventory a_inv, SchedulableInfo o_si)
        : base(a_product, a_inv, a_activity, a_product.RequireEmptyStorageArea, a_product.StorageArea)
    {
        m_product = a_product;
        m_act = a_activity;
        m_si = o_si;
        Init();
        if (a_activity.Operation.AllowSameLotInNonEmptyStorageArea && a_activity.Operation.Split)
        {
            SetAllowSameLotInNonEmptyStorageArea();
        }
    }

    private void Init()
    {
        TotalRequiredQty = m_product.RemainingOutputQty;
        RemainingQty = TotalRequiredQty;
    }

    internal ProductSupplyProfile(ProductSupplyProfile a_profile)
        : base(a_profile)
    {

    }

    internal override void ResetForAllocation()
    {
        Init();
        base.ResetForAllocation();
    }

    internal void AdjustForMoResize(decimal a_adjustedQty)
    {
        TotalRequiredQty = a_adjustedQty;
        RemainingQty = TotalRequiredQty;
    }

    internal override QtyProfile<QtySupplyNode> CopyProfile()
    {
        return new ProductSupplyProfile(this);
    }

    internal void AllocateAllToDiscard()
    {
        foreach (QtySupplyNode node in this)
        {
            node.AllocateAllToDiscard(Inventory);
        }
    }

    private IInterval m_usageDuration;
    internal override IInterval GetUsageRange()
    {
        if (m_usageDuration != null)
        {
            return m_usageDuration;
        }

        //Calculate based on product availability
        if (m_product.InventoryAvailableTiming == ProductDefs.EInventoryAvailableTimings.ByProductionCycle)
        {
            if (m_si.m_productionFinishDate - m_si.m_setupFinishDate > 0)
            {
                m_usageDuration = new Interval(m_si.m_setupFinishDate, m_si.m_productionFinishDate);
            }
        }
        else if (m_product.InventoryAvailableTiming == ProductDefs.EInventoryAvailableTimings.DuringPostProcessing)
        {
            if (m_si.m_postProcessingFinishDate - m_si.m_productionFinishDate > 0)
            {
                m_usageDuration = new Interval(m_si.m_productionFinishDate, m_si.m_postProcessingFinishDate);
            }
        }
        else if (m_product.InventoryAvailableTiming == ProductDefs.EInventoryAvailableTimings.DuringStorage)
        {
            if (m_si.m_storageFinishDate - m_si.m_postProcessingFinishDate > 0)
            {
                m_usageDuration = new Interval(m_si.m_postProcessingFinishDate, m_si.m_storageFinishDate);
            }
        }

        if (m_usageDuration == null)
        {
            //We didn't set it from an above usage, use the default range
            m_usageDuration = base.GetUsageRange();
        }

        return m_usageDuration;
    }

    internal ProductDefs.EInventoryAvailableTimings MaterialTransferUsage => m_product.InventoryAvailableTiming;

    internal RequiredCapacity GetBackCalculateCapacity(SchedulableInfo a_si)
    {
        RequiredCapacity capacity = null;
        switch (MaterialTransferUsage)
        {
            case ProductDefs.EInventoryAvailableTimings.AtOperationRunStart:
            case ProductDefs.EInventoryAvailableTimings.ByProductionCycle:
                capacity = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, a_si.m_requiredSetupSpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                break;
            case ProductDefs.EInventoryAvailableTimings.AtOperationRunEnd:
            case ProductDefs.EInventoryAvailableTimings.DuringPostProcessing:
                capacity = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, a_si.m_requiredSetupSpan, a_si.m_requiredProcessingSpan, RequiredSpan.NotInit, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                break;
            case ProductDefs.EInventoryAvailableTimings.AtOperationResourcePostProcessingEnd:
            case ProductDefs.EInventoryAvailableTimings.DuringStorage:
                capacity = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, a_si.m_requiredSetupSpan, a_si.m_requiredProcessingSpan, a_si.m_requiredPostProcessingSpan, RequiredSpan.NotInit, RequiredSpanPlusClean.s_notInit);
                break;
            case ProductDefs.EInventoryAvailableTimings.AtStorageEnd:
                capacity = new RequiredCapacity(RequiredSpanPlusClean.s_notInit, a_si.m_requiredSetupSpan, a_si.m_requiredProcessingSpan, a_si.m_requiredPostProcessingSpan, a_si.m_requiredStorageSpan, RequiredSpanPlusClean.s_notInit);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return capacity;
    }

    internal override BaseId GetSupplyId()
    {
        return m_product.Id;
    }
}

//TODO: This could be combined with Product profile if Product and PO have some interface for required properties.
internal class PurchaseOrderSupplyProfile : SupplyProfile
{
    private readonly PurchaseToStock m_po;


    internal PurchaseOrderSupplyProfile(PurchaseToStock a_po, Inventory a_inv)
        : base(a_po, a_inv, a_po, a_po.RequireEmptyStorageArea, a_po.StorageArea)
    {
        m_po = a_po;
        Init();
    }

    private void Init()
    {
        TotalRequiredQty = m_po.QtyRemaining;
        RemainingQty = TotalRequiredQty;
    }

    internal PurchaseOrderSupplyProfile(PurchaseOrderSupplyProfile a_profile)
        : base(a_profile)
    {
        m_po = a_profile.m_po;
    }

    internal override void ResetForAllocation()
    {
        Init();
        base.ResetForAllocation();
    }

    internal override QtyProfile<QtySupplyNode> CopyProfile()
    {
        return new PurchaseOrderSupplyProfile(this);
    }
}

internal class TransferOrderSupplyProfile : SupplyProfile
{
    private readonly TransferOrderDistribution m_to;
    private readonly decimal m_qty;

    internal TransferOrderSupplyProfile(TransferOrderDistribution a_to, decimal a_qty, Inventory a_inv)
        : base(a_to, a_inv, a_to, a_to.PreferEmptyStorageArea, a_to.ToStorageArea)
    {
        m_to = a_to;
        m_qty = a_qty;
        Init();
    }

    private void Init()
    {
        TotalRequiredQty = m_qty; //TODO: Why are we not using m_to.QtyOpenToShip?
        RemainingQty = TotalRequiredQty;
    }

    internal TransferOrderSupplyProfile(TransferOrderSupplyProfile a_profile)
        : base(a_profile)
    {
        m_to = a_profile.m_to;
        m_qty = a_profile.m_qty;
    }

    internal override void ResetForAllocation()
    {
        Init();
        base.ResetForAllocation();
    }

    internal override QtyProfile<QtySupplyNode> CopyProfile()
    {
        return new TransferOrderSupplyProfile(this);
    }
}