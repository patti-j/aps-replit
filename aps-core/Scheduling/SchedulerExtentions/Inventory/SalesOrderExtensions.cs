using PT.Scheduler;
using PT.Scheduler.Demand;
using PT.SchedulerDefinitions;
using PT.Scheduler.Simulation;

namespace PT.SchedulerExtensions.Inventory;

public static class SalesOrderExtensions
{
    public static int DistributionsCount(this SalesOrderManager a_salesOrderManager)
    {
        int distributions = 0;
        for (int i = 0; i < a_salesOrderManager.Count; i++)
        {
            distributions += a_salesOrderManager[i].DistributionsCount();
        }

        return distributions;
    }

    public static int DistributionsCount(this SalesOrder a_so)
    {
        int distributions = 0;
        for (int i = 0; i < a_so.SalesOrderLines.Count; i++)
        {
            distributions += a_so.SalesOrderLines[i].DistributionsCount();
        }

        return distributions;
    }

    public static int DistributionsCount(this SalesOrderLine a_line)
    {
        return a_line.LineDistributions.Count;
    }

    /// <summary>
    /// </summary>
    /// <param name="a_so"></param>
    /// <param name="a_wm"></param>
    /// <param name="a_clockDate"></param>
    /// <returns>If a distribution's material availability is after it is needed</returns>
    public static bool Late(this SalesOrder a_so, WarehouseManager a_wm, DateTime a_clockDate)
    {
        if (a_so.Cancelled)
        {
            return false;
        }

        foreach (SalesOrderLine line in a_so.SalesOrderLines)
        {
            foreach (SalesOrderLineDistribution distribution in line.LineDistributions)
            {
                if (distribution.Closed || distribution.QtyOpenToShip == 0)
                {
                    //Distribution is finished
                    continue;
                }

                DateTime materialSourceDate = distribution.GetMaterialSourceDate(a_wm, a_clockDate);
                if (materialSourceDate > distribution.RequiredAvailableDate)
                {
                    //It's late
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// </summary>
    /// <param name="a_so"></param>
    /// <param name="a_wm"></param>
    /// <param name="a_clockDate"></param>
    /// <returns>How late a material's availability is after it's needed.</returns>
    public static TimeSpan Lateness(this SalesOrder a_so, ScenarioDetail a_sd)
    {
        TimeSpan highestLateness = TimeSpan.Zero;
        if (a_so.Cancelled)
        {
            return highestLateness;
        }

        foreach (SalesOrderLine line in a_so.SalesOrderLines)
        {
            foreach (SalesOrderLineDistribution distribution in line.LineDistributions)
            {
                if (distribution.Closed || distribution.QtyOpenToShip == 0)
                {
                    //Distribution is finished
                    continue;
                }

                DateTime materialSourceDate = distribution.GetMaterialSourceDate(a_sd.WarehouseManager, a_sd.ClockDate);

                if (materialSourceDate > distribution.RequiredAvailableDate)
                {
                    highestLateness = materialSourceDate - distribution.RequiredAvailableDate > highestLateness ? materialSourceDate - distribution.RequiredAvailableDate : highestLateness;
                }
            }
        }

        return highestLateness;
    }


    /// <summary>
    /// </summary>
    /// <param name="a_so"></param>
    /// <param name="a_wm"></param>
    /// <param name="a_clockDate"></param>
    /// <returns>
    /// If a distribution's material availability date. If sourced from multiple warehouses, the date that all material is sourced. If Sales Order is cancelled or all
    /// distributions are finished PTDateTime.MinValue is returned
    /// </returns>
    public static DateTime GetLatestMaterialSourceDate(this SalesOrder a_so, WarehouseManager a_wm, DateTime a_clockDate)
    {
        if (a_so.Cancelled)
        {
            return PTDateTime.MinValue.ToDateTime();
        }

        foreach (SalesOrderLine line in a_so.SalesOrderLines)
        {
            foreach (SalesOrderLineDistribution distribution in line.LineDistributions)
            {
                if (distribution.Closed || distribution.QtyOpenToShip == 0)
                {
                    //Distribution is finished
                    continue;
                }

                DateTime latestMaterialSourceDate = distribution.GetMaterialSourceDate(a_wm, a_clockDate);
                return latestMaterialSourceDate;
            }
        }

        return PTDateTime.MinValue.ToDateTime();
    }

    /// <summary>
    /// Get the material source date
    /// </summary>
    /// <param name="a_dist"></param>
    /// <param name="a_wm"></param>
    /// <param name="a_clockDate"></param>
    /// <returns>The material source date or PTDateTime.MinValue if no source adjustment or lead time is found</returns>
    public static DateTime GetMaterialSourceDate(this SalesOrderLineDistribution a_dist, WarehouseManager a_wm, DateTime a_clockDate)
    {
        decimal totalSourcedQty = 0;
        DateTime maxSourceDate = PTDateTime.MinValue.ToDateTime();
        DateTime maxLeadTimeDateTime = PTDateTime.MinValue.ToDateTime();

        foreach (Adjustment adjustment in a_dist.Adjustments)
        {
            if (adjustment.AdjustmentType is InventoryDefs.EAdjustmentType.SalesOrderDistribution)
            {
                if (adjustment.AdjDate > maxSourceDate)
                {
                    maxSourceDate = adjustment.AdjDate;
                }

                totalSourcedQty += adjustment.ChangeQty;
            }
            else if (adjustment.AdjustmentType is InventoryDefs.EAdjustmentType.SalesOrderDemand)
            {
                DateTime invLeadTimeDateTime = a_clockDate.Add(adjustment.Inventory.LeadTime);
                if (invLeadTimeDateTime > maxLeadTimeDateTime)
                {
                    maxLeadTimeDateTime = invLeadTimeDateTime;
                }
            }
        }

        bool fullySourced = Math.Abs(a_dist.QtyOpenToShip) == Math.Abs(totalSourcedQty);
        //If not fully source we need to calculate the max between the given source date and the lead time
        maxSourceDate = fullySourced ? maxSourceDate : PTDateTime.Max(maxSourceDate, maxLeadTimeDateTime);

        return maxSourceDate;
    }

    /// <summary>
    /// </summary>
    /// <param name="a_so"></param>
    /// <param name="a_wm"></param>
    /// <param name="a_limit">Should be DateTime.UtcNow</param>
    /// <returns>If a distribution is required before a_limit and the material is available after a_limit</returns>
    public static bool Overdue(this SalesOrder a_so, WarehouseManager a_wm, DateTime a_limit, DateTime a_clockDate)
    {
        if (a_so.Cancelled)
        {
            return false;
        }

        foreach (SalesOrderLine line in a_so.SalesOrderLines)
        {
            foreach (SalesOrderLineDistribution distribution in line.LineDistributions)
            {
                if (distribution.RequiredAvailableDate >= a_limit || distribution.Closed || distribution.QtyOpenToShip == 0)
                {
                    //Distribution is finished
                    continue;
                }

                DateTime materialSourceDate = distribution.GetMaterialSourceDate(a_wm, a_clockDate);
                if (materialSourceDate > a_limit)
                {
                    //It's overdue
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Get list of Supplying Warehouses
    /// </summary>
    /// <param name="a_dist"></param>
    /// <param name="a_wm"></param>
    /// <returns></returns>
    private static List<Warehouse> GetWarehouseList(SalesOrderLineDistribution a_dist, WarehouseManager a_wm)
    {
        List<Warehouse> warehouseList;
        if (a_dist.MustSupplyFromWarehouse == null)
        {
            warehouseList = new List<Warehouse>(a_wm.Count);
            warehouseList.AddRange(a_wm);
        }
        else
        {
            warehouseList = new List<Warehouse>(1) { a_dist.MustSupplyFromWarehouse };
        }

        return warehouseList;
    }
}