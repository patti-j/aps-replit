using PT.Scheduler.Demand;

namespace PT.SchedulerExtensions.Inventory;

public static class ForecastShipmentExtensions
{
    public static string GetConsumptionDetails(this ForecastShipment a_shipment)
    {
        string details = string.Empty;
        foreach (ForecastShipment.SalesOrderPart consumption in a_shipment.ConsumingSalesOrderParts)
        {
            if (details != string.Empty)
            {
                details += ", ";
            }

            details += $"'{consumption.SalesOrderDistribution.SalesOrderLine.SalesOrder.Name}' Line '{consumption.SalesOrderDistribution.SalesOrderLine.LineNumber}' Distribution '{consumption.SalesOrderDistribution.Id}' consumed {consumption.Qty}";
        }

        return details;
    }
}