using PT.Scheduler.Demand;

namespace PT.Scheduler.MRP;
public class MrpDemand
{
    internal decimal DemandQty;
    internal DateTime DemandDate;
    internal bool LotControlled;

    internal MrpDemand(DateTime a_demandDate, decimal a_demandQty)
    {
        DemandDate = a_demandDate;
        DemandQty = a_demandQty;
    }
    
    //internal string LotCode;
    internal virtual int Priority => 0;

    internal virtual string ReasonDescription => "Demand Object";
}

internal class ActivityMrpDemand : MrpDemand
{
    internal InternalActivity Activity;
    internal MaterialRequirement MR;

    internal ActivityMrpDemand(InternalActivity a_activity, MaterialRequirement a_mr, DateTime a_demandDate, decimal a_demandQty)
        : base(a_demandDate, a_demandQty)
    {
        Activity = a_activity;
        MR = a_mr;
        LotControlled = a_mr.MustUseEligLot;
    }

    internal override int Priority => Activity.Job.Priority;
    internal override string ReasonDescription => $"Job: '{Activity.Job.Name}' | Material: '{MR.Item.Name}'";
}

internal class SalesOrderMrpDemand : MrpDemand
{
    internal SalesOrder SalesOrder;
    internal SalesOrderLineDistribution Distribution;

    internal SalesOrderMrpDemand(SalesOrderLineDistribution a_dist, DateTime a_demandDate, decimal a_demandQty)
        : base(a_demandDate, a_demandQty)
    {
        SalesOrder = a_dist.SalesOrderLine.SalesOrder;
        Distribution = a_dist;
        LotControlled = a_dist.MustUseEligLot;
    }

    internal override int Priority => Distribution.Priority;
    internal override string ReasonDescription => $"SO: '{SalesOrder.Name}' | Line: '{Distribution.SalesOrderLine.LineNumber}'";
}

internal class TransferOrderMrpDemand : MrpDemand
{
    internal TransferOrder TransferOrder;
    internal TransferOrderDistribution Distribution;

    internal TransferOrderMrpDemand(TransferOrderDistribution a_dist, DateTime a_demandDate, decimal a_demandQty)
        : base(a_demandDate, a_demandQty)
    {
        TransferOrder = a_dist.TransferOrder;
        Distribution = a_dist;
        LotControlled = a_dist.MustUseEligLot;
    }

    internal override int Priority => TransferOrder.Priority;
    internal override string ReasonDescription => $"TO: '{TransferOrder.Name}' | WH: '{Distribution.FromWarehouse.Name}'";
}

internal class SafetyStockMrpDemand : MrpDemand
{
    internal Inventory Inventory;

    internal SafetyStockMrpDemand(Inventory a_inv, DateTime a_demandDate, decimal a_demandQty)
        : base(a_demandDate, a_demandQty)
    {
        Inventory = a_inv;
    }

    internal override int Priority => Inventory.SafetyStockJobPriority;
}

internal class ForecastMrpDemand : MrpDemand
{
    internal ForecastShipment Shipment;

    internal ForecastMrpDemand(ForecastShipment a_forecastShipment, DateTime a_demandDate, decimal a_demandQty)
        : base(a_demandDate, a_demandQty)
    {
        Shipment = a_forecastShipment;
    }

    internal override int Priority => Shipment.Forecast.Priority;
}


