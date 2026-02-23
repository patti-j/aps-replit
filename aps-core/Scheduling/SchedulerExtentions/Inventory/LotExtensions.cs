using PT.APSCommon;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.SchedulerExtensions.Inventory;

public static class LotExtensions
{
    public static Lot FindLotProduced(this InternalOperation a_op, Product a_product)
    {
        if (a_product.Warehouse != null)
        {
            Scheduler.Inventory inventory = a_product.Warehouse.Inventories[a_product.Item.Id];
            List<BaseId> actIds = a_op.Activities.Select(x => x.Id).ToList();

            foreach (Lot lot in inventory.Lots)
            {
                if (lot.LotSource == ItemDefs.ELotSource.Production)
                {
                    if (actIds.Contains(lot.SupplySourceId))
                    {
                        return lot;
                    }
                }
            }
        }

        return null;
    }

    public static IEnumerable<Tuple<Lot, DateTime>> FindLotsProducedWithExpiration(this InternalActivity a_act, ScenarioDetail a_sd)
    {
        foreach (Product product in a_act.Operation.Products)
        {
            foreach (Warehouse warehouse in a_sd.WarehouseManager)
            {
                Scheduler.Inventory inventory = warehouse.Inventories[product.Item.Id];
                if (inventory != null)
                {
                    foreach (Lot lot in inventory.Lots)
                    {
                        if (lot.Source is Product prodSource && prodSource.Id == product.Id)
                        {
                            DateTime expirationDate = lot.ShelfLifeData.Expirable ? lot.ShelfLifeData.ExpirationDate : DateTime.MaxValue;
                            yield return new Tuple<Lot, DateTime>(lot, expirationDate);
                        }
                    }
                }
            }
        }
    }

    public static IEnumerable<Tuple<Lot, decimal>> FindExpirableLotsConsumed(this InternalActivity a_act)
    {
        List<Tuple<Lot, decimal>> supplyingLots = new ();
        foreach (MaterialRequirement mr in a_act.Operation.MaterialRequirements)
        {
            if (!mr.BuyDirect)
            {
                foreach (Tuple<Lot, decimal> supplyingLot in mr.SupplyingLots)
                {
                    if (supplyingLot.Item1.ShelfLifeData.Expirable && supplyingLot.Item1.ShelfLifeData.ExpirationDate < a_act.ScheduledStartDate)
                    {
                        supplyingLots.Add(supplyingLot);
                    }
                }
            }
        }

        return supplyingLots;
    }

    public static decimal FindExpirableLotsProduced(this InternalActivity a_act)
    {
        decimal supplyingLots = 0;
        foreach (Product product in a_act.Operation.Products)
        {
            supplyingLots += CalcLotExpirationsOnOrBeforeDateTime(product.Inventory, product.Inventory.Item.ShelfLife.Ticks);
        }
        return supplyingLots;
    }

    public static IEnumerable<Tuple<Lot, decimal>> FindExpirableLotsConsumed(this MaterialRequirement a_mr, InternalActivity a_act)
    {
        List<Tuple<Lot, decimal>> supplyingLots = new ();
        if (!a_mr.BuyDirect)
        {
            foreach (Tuple<Lot, decimal> supplyingLot in a_mr.SupplyingLots)
            {
                if (supplyingLot.Item1.ShelfLifeData.Expirable && supplyingLot.Item1.ShelfLifeData.ExpirationDate < a_act.ScheduledStartDate)
                {
                    supplyingLots.Add(supplyingLot);
                }
            }
        }

        return supplyingLots;
    }

    public static DateTime EarliestExpiredUsage(this InternalActivity a_act)
    {
        DateTime earliestExpiration = DateTime.MaxValue;
        foreach (MaterialRequirement mr in a_act.Operation.MaterialRequirements)
        {
            if (!mr.BuyDirect)
            {
                foreach (Tuple<Lot, decimal> supplyingLot in mr.SupplyingLots)
                {
                    if (supplyingLot.Item1.ShelfLifeData.Expirable)
                    {
                        earliestExpiration = PTDateTime.Min(earliestExpiration, supplyingLot.Item1.ShelfLifeData.ExpirationDate);
                    }
                }
            }
        }

        return earliestExpiration;
    }

    public static DateTime EarliestExpiredUsage(this MaterialRequirement a_mr, InternalActivity a_act)
    {
        DateTime earliestExpiration = DateTime.MaxValue;
        if (!a_mr.BuyDirect)
        {
            foreach (Tuple<Lot, decimal> supplyingLot in a_mr.SupplyingLots)
            {
                if (supplyingLot.Item1.ShelfLifeData.Expirable && supplyingLot.Item1.ShelfLifeData.ExpirationDate < a_act.ScheduledStartDate)
                {
                    earliestExpiration = PTDateTime.Min(earliestExpiration, supplyingLot.Item1.ShelfLifeData.ExpirationDate);
                }
            }
        }

        return earliestExpiration;
    }

    /// <summary>
    /// Calculates the quantity of material that has expired before the specified limit
    /// </summary>
    public static decimal CalcLotExpirationsOnOrBeforeDateTime(this Scheduler.Inventory a_inv, long a_limitTicks)
    {
        decimal expiredQty = 0;

        if (a_inv.Item.ShelfLife > TimeSpan.Zero)
        {
            foreach (Lot lot in a_inv.Lots)
            {
                //Find expirations of ShelfLifeLots
                if (lot.ShelfLifeData.Expirable)
                {
                    if (lot.ShelfLifeData.ExpirationDate.Ticks < a_limitTicks)
                    {
                        //TODO: SA
                        //expiredQty += lot.UnallocatedQty;
                    }
                }
            }

            AdjustmentArray array = a_inv.GetAdjustmentArray();
            for (int i = 0; i < array.Count; i++)
            {
                Adjustment adjustment = array[i];
                if (adjustment is ActivityAdjustment actAdj && adjustment.Storage is StorageAdjustment lotAdjustment)
                {
                    if (!lotAdjustment.Lot.ShelfLifeData.Expirable)
                    {
                        if (lotAdjustment.Lot.ProductionDate.Add(a_inv.Item.ShelfLife) < actAdj.Activity.ScheduledStartDate)
                        {
                            expiredQty += -adjustment.ChangeQty;
                        }
                    }
                }
            }
        }

        return expiredQty;
    }
}