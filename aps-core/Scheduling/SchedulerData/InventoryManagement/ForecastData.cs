using PT.APSCommon.Extensions;
using PT.Database;
using PT.Scheduler.Demand;
using PT.SchedulerDefinitions;
using PT.SchedulerExtensions.Inventory;

namespace PT.SchedulerData.InventoryManagement;

public static class ForecastData
{
    public static void PtDbPopulate(this ForecastVersions a_forecastVersions, ref PtDbDataSet dataSet, PtDbDataSet.InventoriesRow parentInventoryRow, PTDatabaseHelper a_dbHelper)
    {
        for (int i = 0; i < a_forecastVersions.Versions.Count; i++)
        {
            a_forecastVersions.Versions[i].PtDbPopulate(ref dataSet, parentInventoryRow, a_dbHelper);
        }
    }

    public static void PtDbPopulate(this ForecastVersion a_forecastVersion, ref PtDbDataSet r_dataSet, PtDbDataSet.InventoriesRow parentInventoryRow, PTDatabaseHelper a_dbHelper)
    {
        for (int i = 0; i < a_forecastVersion.Forecasts.Count; i++)
        {
            a_forecastVersion.Forecasts[i].PtDbPopulate(ref r_dataSet, parentInventoryRow, a_forecastVersion.Version, a_dbHelper);
        }
    }

    public static void PtDbPopulate(this Forecast a_forecast, ref PtDbDataSet dataSet, PtDbDataSet.InventoriesRow parentInventoryRow, string forecastVersion, PTDatabaseHelper a_dbHelper)
    {
        PtDbDataSet.ForecastsRow forecastRow = dataSet.Forecasts.AddForecastsRow(
            parentInventoryRow.PublishDate,
            parentInventoryRow.InstanceId,
            parentInventoryRow.InventoryId,
            forecastVersion,
            a_forecast.Id.ToBaseType(),
            a_forecast.Name,
            a_forecast.Description,
            a_forecast.ExternalId,
            a_forecast.Notes,
            a_forecast.Customer != null ? a_forecast.Customer.ExternalId : "None".Localize(),
            a_forecast.Planner,
            a_forecast.Priority,
            a_forecast.SalesOffice,
            a_forecast.SalesPerson);

        IEnumerator<ForecastShipment> shipmentsEnumerator = a_forecast.Shipments.GetEnumerator();
        while (shipmentsEnumerator.MoveNext())
        {
            ForecastShipment shipment = shipmentsEnumerator.Current;
            if (shipment.RequiredDate <= a_dbHelper.MaxPublishDate)
            {
                shipment.PtDbPopulate(ref dataSet, forecastRow, a_dbHelper);
            }
        }
    }

    public static void PtDbPopulate(this ForecastShipment a_shipment, ref PtDbDataSet dataSet, PtDbDataSet.ForecastsRow parentForecastRow, PTDatabaseHelper a_dbHelper)
    {
        dataSet.ForecastShipments.AddForecastShipmentsRow(
            parentForecastRow.PublishDate,
            parentForecastRow.InstanceId,
            parentForecastRow.ForecastId,
            a_shipment.Id.ToBaseType(),
            a_dbHelper.AdjustPublishTime(a_shipment.RequiredDate),
            a_shipment.RequiredQty,
            a_shipment.GetConsumedQty(),
            a_shipment.GetConsumptionDetails());
    }
}