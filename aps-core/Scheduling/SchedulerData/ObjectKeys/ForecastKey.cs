using PT.APSCommon;
using PT.Common.ObjectHelpers;
using PT.Scheduler;
using PT.Scheduler.Demand;

namespace PT.SchedulerData.ObjectKeys;

public readonly struct ForecastKey : IEquatable<ForecastKey>
{
    public readonly BaseId WarehouseId;
    public readonly BaseId InvItemId;
    public readonly BaseId ForecastId;
    public readonly BaseId ShipmentId;

    public ForecastKey(BaseId a_whId, BaseId a_invItemId, BaseId a_forecastId, BaseId a_shipmentId)
    {
        WarehouseId = a_whId;
        InvItemId = a_invItemId;
        ForecastId = a_forecastId;
        ShipmentId = a_shipmentId;
    }

    public ForecastKey(InventoryKey a_inventoryKey, BaseId a_forecastId, BaseId a_shipmentId)
    {
        WarehouseId = a_inventoryKey.WarehouseId;
        InvItemId = a_inventoryKey.ItemId;
        ForecastId = a_forecastId;
        ShipmentId = a_shipmentId;
    }

    public Forecast GetForecast(ScenarioDetail a_sd)
    {
        Warehouse warehouse = a_sd.WarehouseManager.GetById(WarehouseId);
        Inventory inventory = warehouse.Inventories[InvItemId];
        return inventory.ForecastVersionActive.GetById(ForecastId);
    }

    public ForecastShipment GetForecastShipment(ScenarioDetail a_sd)
    {
        Forecast forecast = GetForecast(a_sd);
        foreach (ForecastShipment shipment in forecast.Shipments)
        {
            if (shipment.Id == ShipmentId)
            {
                return shipment;
            }
        }

        return null;
    }

    public bool Equals(ForecastKey a_other)
    {
        return ShipmentId.Value == a_other.ShipmentId.Value && InvItemId.Value == a_other.InvItemId.Value && ForecastId.Value == a_other.ForecastId.Value && WarehouseId.Value == a_other.WarehouseId.Value;
    }

    public override bool Equals(object a_other)
    {
        if (a_other is ForecastKey key)
        {
            return Equals(key);
        }

        return false;
    }

    public override int GetHashCode()
    {
        //Don't have to worry much about activities and other properties since these ids are unique
        return HashCodeHelper.GetHashCode(WarehouseId, InvItemId, ForecastId);
    }
}