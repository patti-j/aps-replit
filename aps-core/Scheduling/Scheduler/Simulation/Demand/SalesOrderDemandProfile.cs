using PT.Scheduler.Demand;
using PT.Scheduler.Simulation;
using PT.Scheduler.Simulation.Events;

namespace PT.Scheduler;

internal class SalesOrderDemandProfile : WarehouseDemandProfile
{
    private readonly SalesOrderLineDistribution m_sod;

    internal SalesOrderDemandProfile(SalesOrderLineDistribution a_sod) 
        : base(a_sod.Item, a_sod.UseMustSupplyFromWarehouse, a_sod.MustSupplyFromWarehouse, new SalesOrderDemandSource(a_sod))
    {
        m_sod = a_sod;
        AllowPartialSupply = a_sod.AllowPartialAllocations;
        AllowMultiStorageAreaSupply = true; //Always pull from multiple. //TODO: We could add a field on SO for this.
        m_materialAllocation = a_sod.MaterialAllocation;
        RemainingQty = a_sod.QtyOpenToShip;
    }

    internal override void GenerateProfile(ScenarioDetail a_sd)
    {
        QtyDemandNode qtyNode = new (a_sd.SimClock, m_sod, RemainingQty);
        AddToEnd(qtyNode);
    }

    internal override void UpdateActualAvailableTicks(long a_matlAvailTicks)
    {
        m_sod.ActualAvailableTicks = a_matlAvailTicks;
    }

    internal override bool IsSupplySourceEligible(QtySupplyNode a_node, ScenarioDetail a_sd)
    {
        if (a_node.SupplySource == null)
        {
            //TODO: This shouldn't happen. Fix this
            return true;
        }

        if (a_sd.ExtensionController.RunEligibleMaterialExtension) // a_act might be null when finding qty for thing such as TransferOrders.
        {
            //TODO: SO extensions
        }

        //if (!AllowExpiredSupply && a_node.Expired)
        //{
        //    return false;
        //}

        //Check lot tracking:
        if (!m_sod.IsLotElig(a_node.SupplySource.SupplyLot, a_sd.UsedAsEligibleLotsLotCodes))
        {
            return false;
        }

        //if (!m_mr.ShelfLifeRequirement.IsUsable(a_node.SupplySource.SupplyLot, a_node.Date))
        //{
        //    return false;
        //}

        //if (m_mr.WearRequirement.MaxWearAmount < a_node.SupplySource.SupplyLot.WearData.WearAmount)
        //{
        //    return false;
        //}

        //if (a_matlAllocation != null)
        //{
        //    IMaterialAllocation ima = a_matlAllocation;
        //    if (ima != null && SourceQtyDataReason != null)
        //    {
        //        SourceQtyData sqd = SourceQtyDataReason.SourceQtyData;
        //        if (ima.MaterialSourcing == SchedulerDefinitions.ItemDefs.MaterialSourcing.Exclusive && sqd.QtyConsumed)
        //        {
        //            eligibleMatl = false;
        //        }

        //        if (ima.MinSourceQty > 0 && sqd.AvailableQty < ima.MinSourceQty)
        //        {
        //            // The material source is ineligible because it has less than the required minimum amount of material the requirement allows its sources of material to have. 
        //            eligibleMatl = false;
        //        }

        //        if (ima.MaxSourceQty > 0 && sqd.AvailableQty > ima.MaxSourceQty)
        //        {
        //            eligibleMatl = false;
        //        }
        //    }
        //}

        return true;
    }
}

