using System.Text;

using PT.APSCommon;
using PT.Common.Extensions;
using PT.Scheduler;
using PT.Scheduler.Schedule.Demand;
using PT.SchedulerDefinitions;

namespace PT.SchedulerData.InventoryManagement;

public static class MaterialRequirementData
{
    /// <summary>
    /// Returns the the earliest time at which the inventory is available for the required qty, or the max datetime.
    /// If warehouse is not set, uses MR.AvailableDateTime which is scheduled start date, or imported date if BuyDirect.
    /// </summary>
    public static DateTime CalculateAvailableDate(this MaterialRequirement a_mr, DateTime a_minTime, DateTime a_maxTime, ScenarioDetail a_sd)
    {
        bool temp;
        return CalculateAvailableDate(a_mr, a_minTime, a_maxTime, a_sd, out temp);
    }

    /// <summary>
    /// Returns the the earliest time at which the inventory is available for the required qty, or the max datetime.
    /// If warehouse is not set or the inventory is not set, uses MR.LatestSourceDate which is scheduled start date, or imported date if BuyDirect.
    /// [bool] out variable indicates whether this value was calculated (false) or
    /// is the MaterialRequirement's LatestSourceDate in the case of no inventory or no warehouse references (true).
    /// </summary>
    public static DateTime CalculateAvailableDate(this MaterialRequirement a_mr, DateTime a_minTime, DateTime a_maxTime, ScenarioDetail a_sd, out bool a_usingBaseAvailableDate)
    {
        a_usingBaseAvailableDate = false;

        if (a_mr.Item != null && a_mr.Warehouse != null)
        {
            DateTime availableDate = a_maxTime;
            Inventory inv = a_mr.Warehouse.Inventories[a_mr.Item.Id];

            foreach (Adjustment adj in a_mr.MRSupply)
            {
                if (adj.AdjustmentType == InventoryDefs.EAdjustmentType.LeadTime || adj.AdjustmentType == InventoryDefs.EAdjustmentType.Shortage)
                {
                    availableDate = PTDateTime.Min(a_maxTime, a_sd.ClockDate.Add(inv.LeadTime));
                }
            }

            availableDate = PTDateTime.Min(availableDate, inv.GetAvailableDate(a_mr.UnIssuedQty, a_minTime, a_maxTime, a_mr.GetAllowedLotCodesHash(), a_sd));

            availableDate = PTDateTime.Max(a_minTime, availableDate);

            return availableDate;
        }

        a_usingBaseAvailableDate = true;
        return a_mr.LatestSourceDateTime;

        //TODO: use the MaterialRequirementAdjustment supply source dates if possible
        //a_usingBaseAvailableDate = false;

        //if (a_mr.BuyDirect)
        //{
        //    return a_mr.LatestSourceDateTime;
        //}

        //DateTime availableDate = PTDateTime.InvalidDateTime;
        //HashSet<Inventory> inventoriesToCheck = new ();
        //foreach (Adjustment adj in a_mr.MRSupply)
        //{
        //    if (adj is MaterialRequirementAdjustment mrAdjustment)
        //    {
        //        if (mrAdjustment.SupplyAvailableDate == PTDateTime.InvalidDateTimeTicks)
        //        {
        //            return a_maxTime; //Not fully sourced
        //        }

        //        availableDate = PTDateTime.Max(availableDate, new DateTime(mrAdjustment.SupplyAvailableDate));
        //    }

        //    if (adj.Inventory != null)
        //    {
        //        inventoriesToCheck.AddIfNew(adj.Inventory);
        //    }
        //}

        //if (inventoriesToCheck.Count == 0 && a_mr.Warehouse != null && a_mr.Warehouse.Inventories[a_mr.Item.Id] is Inventory defaultInv)
        //{
        //    inventoriesToCheck.Add(defaultInv);
        //}
        
        ////TODO: This only works if a supply can source the entire MR. This needs to be broken down by MRSupplyAdjustment to see when the latest demand node can be satisfied.
        //foreach (Inventory inventory in inventoriesToCheck)
        //{
        //    availableDate = PTDateTime.Min(availableDate, inventory.GetAvailableDate(a_mr.UnIssuedQty, a_minTime, a_maxTime, a_mr.GetAllowedLotCodesHash(), a_sd));
        //}

        //if (availableDate != PTDateTime.InvalidDateTime)
        //{
        //    return availableDate;
        //}

        //a_usingBaseAvailableDate = true;
        //return a_mr.LatestSourceDateTime;
    }

    /// <summary>
    /// Returns a comma separated string of lot codes
    /// </summary>
    public static string GetAllowedLotCodes(this MaterialRequirement a_mr)
    {
        StringBuilder codeString = new();
        foreach (string lotCode in a_mr.EligibleLotCodesEnumerator)
        {
            codeString.Append(lotCode);
            codeString.Append(',');
        }

        return codeString.ToString().TrimEnd(',');
    }

    /// <summary>
    /// Returns a comma separated string of lot codes
    /// </summary>
    public static HashSet<string> GetAllowedLotCodesHash(this MaterialRequirement a_mr)
    {
        HashSet<string> allowedCodes = new ();
        foreach (string lotCode in a_mr.EligibleLotCodesEnumerator)
        {
            allowedCodes.Add(lotCode);
        }

        return allowedCodes;
    }
}