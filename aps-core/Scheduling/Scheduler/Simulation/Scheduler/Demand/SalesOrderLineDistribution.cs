using PT.Scheduler.Schedule.Demand;
using System.Collections.Generic;

namespace PT.Scheduler.Demand;

public partial class SalesOrderLineDistribution : Schedule.IQtyRequirement, IComparable<SalesOrderLineDistribution>
{
    public Item Item => SalesOrderLine.Item;

    internal void ResetSimulationStateVariables()
    {
        SupplyingWarehouse = null;
        QtyAllocatedFromOnHandInventory = 0;
        QtyAllocatedFromProjectedInventory = 0;
        m_demandAdjustments.Clear();
        m_simAdjustments.Clear();
    }

    /// <summary>
    /// Sales order line distributions check for shelflife usability.
    /// </summary>
    /// <returns></returns>
    public bool IsUsable(Lot a_lot, object a_args)
    {
        //if (a_lot.Usability is ShelfLifeLotData shelfLife && a_args is ShelfLifeRequirement.IsUsableArgs sdReference)
        //{
        //    // Check if the extension wants to override this check
        //    if (sdReference.ScenarioDetail.ExtensionController.RunEligibleMaterialExtension)
        //    {
        //        bool? ret = sdReference.ScenarioDetail.ExtensionController.IsLotUsable(sdReference.ScenarioDetail.SimClock, shelfLife.ExpirationTicks, -1, a_lot, sdReference.ScenarioDetail);
        //        if (ret.HasValue)
        //        {
        //            return ret.Value;
        //        }
        //    }

        //    bool expirationTest = shelfLife.ExpirationTicks >= sdReference.ScenarioDetail.SimClock;

        //    return expirationTest;
        //}

        return true;
    }

    internal void AddEligibleLotCodes(EligibleLots a_eligibleLotCodes)
    {
        EligibleLots.AddEligibleLotCodes(a_eligibleLotCodes);
    }

    public override string ToString()
    {
        return string.Format("Sales Order Dist: Qty{0}; Item:{1}; eligibleLots:{2}", QtyOpenToShip, Item, EligibleLots);
    }

    /// <summary>
    /// Whether a lot code is contained in the set of lots eligible to fulfill this object.
    /// </summary>
    /// <param name="a_lotCode"></param>
    /// <returns></returns>
    public bool ContainsEligibleLot(string a_lotCode)
    {
        return m_eligibleLots.Contains(a_lotCode);
    }

    public int CompareTo(SalesOrderLineDistribution a_other)
    {
        if (a_other is null)
        {
            return 1;
        }
     
        //This should not happen, if so, it the other values will be the same anyways and it will return 0
        //if (Id == a_other.Id)
        //{
        //    return 0;
        //}
        
        if (Priority < a_other.Priority)
        {
            return -1;
        }

        if (Priority > a_other.Priority)
        {
            return 1;
        }

        if (RequiredAvailableDateTicks < a_other.RequiredAvailableDateTicks)
        {
            return -1;
        }

        if (RequiredAvailableDateTicks > a_other.RequiredAvailableDateTicks)
        {
            return 1;
        }

        return 0;
    }
    
    private readonly AdjustmentArray m_demandAdjustments; //Adjustments that don't affect the schedule and are stored on this distribution
    private readonly AdjustmentArray m_simAdjustments; //A collection of linked adjustments that are stored on another source (Lot).
    
    public AdjustmentArray Adjustments
    {
        get
        {
            AdjustmentArray combinedArray = new ();
            combinedArray.Add(m_demandAdjustments);
            combinedArray.Add(m_simAdjustments);
            return combinedArray;
        }
    }

    internal void LinkSimAdjustment(SalesOrderAdjustment a_adjustment)
    {
        m_simAdjustments.Add(a_adjustment);
    }

    internal Adjustment GenerateDemandAdjustment(Inventory a_inv)
    {
        SalesOrderMrpDemandAdjustment demandAdjustment = new (this, a_inv, -QtyOpenToShip);
        m_demandAdjustments.Add(demandAdjustment);
        return demandAdjustment;
    }

    internal decimal RemainingDemandQty => QtyOpenToShip - m_simAdjustments.Sum(a => a.Qty);

    internal void SimulationActionComplete()
    {
        //Sort adjustments now so when accessed they don't sort and get collection modified exceptions
        m_demandAdjustments.Sort();
        m_simAdjustments.Sort();
    }
}