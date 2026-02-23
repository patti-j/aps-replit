using PT.APSCommon;

using PT.Scheduler.Demand;

namespace PT.Scheduler;

//TODO: Attempt to make internal
public interface ISupplySource
{
    bool LimitMatlSrcToEligibleLots { get; }
    BaseId Id { get; }

    Lot SupplyLot { get; }

    int WearDurability { get; }

    string LotCode { get; }

    void LinkCreatedLot(Lot a_newLot);
    
    long MaterialPostProcessingTicks { get; }

    //Adjustment GenerateAdjustment(long a_time, decimal a_changeQty, Lot a_lot, StorageArea a_storageArea);
    Adjustment GenerateDiscardAdjustment(long a_simClock, decimal a_decimal);
}

public abstract class DemandSource
{
    internal BaseIdObject DemandObject { get; }

    internal DemandSource(BaseIdObject a_idObject)
    {
        DemandObject = a_idObject;
    }

    internal abstract Adjustment GenerateAdjustment(long a_time, decimal a_changeQty, Lot a_lot, StorageArea a_storageArea, long a_supplyAvailableDate);
    internal abstract Adjustment GenerateDisposalAdjustment(long a_time, decimal a_changeQty, Lot a_lot, StorageArea a_storageArea);
}

public class SalesOrderDemandSource : DemandSource
{
    internal SalesOrderDemandSource(SalesOrderLineDistribution a_sold)
        : base(a_sold)
    {
        Distribution = a_sold;
    }

    internal SalesOrderLineDistribution Distribution{ get; }

    internal override Adjustment GenerateAdjustment(long a_time, decimal a_changeQty, Lot a_lot, StorageArea a_storageArea, long a_supplyAvailableDate)
    {
        SalesOrderAdjustment adjustment = new (Distribution, a_lot.Inventory, a_time, a_changeQty, new StorageAdjustment(a_lot, a_storageArea));
        Distribution.LinkSimAdjustment(adjustment);
        return adjustment;
    }
    
    internal override Adjustment GenerateDisposalAdjustment(long a_time, decimal a_changeQty, Lot a_lot, StorageArea a_storageArea)
    {
        SalesOrderDisposalAdjustment adjustment = new (a_lot.Inventory, a_time, -a_changeQty, new StorageAdjustment(a_lot, a_storageArea), Distribution);
        Distribution.LinkSimAdjustment(adjustment);
        return adjustment;
    }
}

public class MaterialRequirementDemandSource : DemandSource
{
    internal MaterialRequirementDemandSource(InternalActivity a_activitySource, BaseIdObject a_demandSource)
        : base(a_demandSource)
    {
        Activity = a_activitySource;
    }

    internal InternalActivity Activity { get; }

    internal override Adjustment GenerateAdjustment(long a_time, decimal a_changeQty, Lot a_lot, StorageArea a_storageArea, long a_supplyAvailableDate)
    {
        if (DemandObject is MaterialRequirement mr)
        {
            MaterialRequirementAdjustment adjustment = new (a_lot.Inventory, a_time, a_changeQty, new StorageAdjustment(a_lot, a_storageArea), Activity, mr, a_supplyAvailableDate);
            mr.LinkAdjustment(adjustment);
            return adjustment;
        }
        else if (DemandObject == null && Activity != null)
        {
            return new ActivityAdjustment(a_lot.Inventory, a_time, a_changeQty, new StorageAdjustment(a_lot, a_storageArea), Activity);
        }

        throw new NotImplementedException("Demand source could not determine the required Adjustment type");
    }
    
    internal override Adjustment GenerateDisposalAdjustment(long a_time, decimal a_changeQty, Lot a_lot, StorageArea a_storageArea)
    {
        if (DemandObject is MaterialRequirement mr)
        {
            MaterialDisposalAdjustment disposalAdjustment = new (a_lot.Inventory, a_time, -a_changeQty
                , new StorageAdjustment(a_lot, a_storageArea), Activity, mr);
            //mr.LinkAdjustment(disposalAdjustment); Not linking since the MR is not using this material
            return disposalAdjustment;
        }
        else if (DemandObject == null && Activity != null)
        {
            //return new ActivityAdjustment(a_lot.Inventory, a_time, a_changeQty, new StorageAdjustment(a_lot, a_storageArea), Activity);
        }

        throw new NotImplementedException("Demand source could not determine the required Adjustment type");
    }

    internal Adjustment GenerateLeadTimeAdjustment(Inventory a_inv, QtyDemandNode a_qtyDemandNode, decimal a_leadTimeAllocation)
    {
        if (DemandObject is MaterialRequirement mr)
        {
            MaterialRequirementLeadTimeAdjustment adjustment = new (a_inv, a_qtyDemandNode.Date, -a_leadTimeAllocation, Activity, mr);
            mr.LinkAdjustment(adjustment);
            return adjustment;
        }
        else if (DemandObject == null && Activity != null)
        {
            //return new ActivityAdjustment(a_lot.Inventory, a_time, a_changeQty, new StorageAdjustment(a_lot, a_storageArea), Activity);
        }

        throw new NotImplementedException("Demand source could not determine the required Adjustment type");
    }

    internal Adjustment GenerateShortageAdjustment(Inventory a_shortageInventory, QtyDemandNode a_qtyDemandNode, decimal a_unallocatedQty, bool a_pastPlanningHorizon)
    {
        if (DemandObject is MaterialRequirement mr)
        {
            MaterialRequirementShortageAdjustment shortageAdjustment = new (a_shortageInventory, a_qtyDemandNode.Date, -a_unallocatedQty, Activity, mr, a_pastPlanningHorizon);
            mr.LinkAdjustment(shortageAdjustment);
            return shortageAdjustment;
        }
        else if (DemandObject == null && Activity != null)
        {
            //return new ActivityAdjustment(a_lot.Inventory, a_time, a_changeQty, new StorageAdjustment(a_lot, a_storageArea), Activity);
        }

        throw new NotImplementedException("Demand source could not determine the required Adjustment type");
    }
}

public class TransfersOrderDemandSource : DemandSource
{
    internal TransfersOrderDemandSource(TransferOrderDistribution a_tod)
        : base(a_tod)
    {
        Distribution = a_tod;
    }

    internal TransferOrderDistribution Distribution { get; }

    internal override Adjustment GenerateAdjustment(long a_time, decimal a_changeQty, Lot a_lot, StorageArea a_storageArea, long a_supplyAvailableDate)
    {
        TransferOrderAdjustment adjustment = new (a_lot.Inventory, a_time, a_changeQty, new StorageAdjustment(a_lot, a_storageArea), Distribution);
        Distribution.LinkSimAdjustment(adjustment);
        return adjustment;
    }

    internal override Adjustment GenerateDisposalAdjustment(long a_time, decimal a_changeQty, Lot a_lot, StorageArea a_storageArea)
    {
        TransferOrderDisposalAdjustment adjustment = new (a_lot.Inventory, a_time, -a_changeQty, new StorageAdjustment(a_lot, a_storageArea), Distribution);
        Distribution.LinkSimAdjustment(adjustment);
        return adjustment;
    }
}

