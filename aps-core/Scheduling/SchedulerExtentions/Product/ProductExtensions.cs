using PT.Scheduler;

namespace PT.SchedulerExtensions.DataObjects;

public static class ProductExtensions
{
    /// <summary>
    /// Get the earliest date this product is supplied to another demand
    /// </summary>
    /// <param name="a_product"></param>
    /// <param name="a_act"></param>
    /// <param name="a_maxEndDateTicks"></param>
    /// <returns></returns>
    public static long GetEarliestProductSupplyDate(this Product a_product, InternalActivity a_act, long a_maxEndDateTicks)
    {
        long earliestSupplyDate = a_maxEndDateTicks;
        foreach (Scheduler.Inventory warehouseInventory in a_product.Warehouse.Inventories)
        {
            if (warehouseInventory.Item.Id == a_product.Item.Id)
            {
                //Get adjustments
                AdjustmentArray adjustments = warehouseInventory.GetAdjustmentArray();

                //Get all negative adjustments up to the currently calculated max end date
                AdjustmentArray negativeAdjustments = adjustments.GetNegativeAdjustments(new DateTime(earliestSupplyDate));

                for (int i = 0; i < negativeAdjustments.Count; i++)
                {
                    Adjustment negativeAdjustment = negativeAdjustments[i];
                    if (negativeAdjustment.AdjDate < a_act.ScheduledStartDate)
                    {
                        continue;
                    }

                    if (negativeAdjustment.AdjDate.Ticks < earliestSupplyDate + a_product.MaterialPostProcessingTicks)
                    {
                        if (negativeAdjustment is ActivityAdjustment actAdj && actAdj.Activity.Id == a_act.Id)
                        {
                            earliestSupplyDate = negativeAdjustment.AdjDate.Ticks;
                            earliestSupplyDate -= a_product.MaterialPostProcessingTicks;
                            break;
                        }
                    }
                }
            }
        }

        return earliestSupplyDate;
    }
}