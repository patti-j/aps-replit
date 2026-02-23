using PT.APSCommon;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.Scheduler;
using PT.Scheduler.Demand;
using PT.SchedulerDefinitions;

using ForecastKey = PT.SchedulerData.ObjectKeys.ForecastKey;

namespace PT.PackageDefinitionsUI.DataSources;

internal class ForecastDataSource : IForecastDataSource
{
    private readonly ForecastObjectDataSource m_forecastOrderDataSource;

    internal ForecastDataSource(IScenarioInfo a_scenarioInfo, BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties)
    {
        m_forecastOrderDataSource = new ForecastObjectDataSource(a_scenarioId, a_sdCache, a_properties);
    }

    public List<LookUpValueStruct> GetValueBlock(IForecastProperty a_property, List<ForecastKey> a_ids)
    {
        return GetValueBlock(a_property.PropertyName, a_ids);
    }

    public List<LookUpValueStruct> GetValueBlock(string a_property, List<ForecastKey> a_ids)
    {
        return m_forecastOrderDataSource.GetValueBlock(a_ids, a_property);
    }

    public LookUpValueStruct GetValue(IForecastProperty a_property, ForecastKey a_id)
    {
        return GetValue(a_property.PropertyName, a_id);
    }

    public LookUpValueStruct GetValue(string a_property, ForecastKey a_id)
    {
        return m_forecastOrderDataSource.GetValue(a_id, a_property);
    }

    public void SignalSimulationStarted()
    {
        m_forecastOrderDataSource.SignalSimulationStarted();
    }

    public void SignalSimulationCompleted()
    {
        m_forecastOrderDataSource.SignalSimulationCompleted();
    }

    public void SignalScenarioActivated()
    {
        m_forecastOrderDataSource.SignalScenarioActivated();
    }

    public void SignalDataChanged(IScenarioDataChanges a_dataChanges)
    {
        //Nothing to do here
    }
}

internal class ForecastObjectDataSource : BaseObjectDataSource<ForecastKey, IForecastProperty>
{
    internal ForecastObjectDataSource(BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_objectProperties) : base(a_scenarioId, a_sdCache)
    {
        foreach (IObjectProperty objectProperty in a_objectProperties)
        {
            if (objectProperty is IForecastProperty property)
            {
                m_propLookup.Add(property.PropertyName, property);
            }
        }
    }

    public override object GetValue(string a_property, ForecastKey a_id, ScenarioDetail a_sd)
    {
        if (m_propLookup.TryGetValue(a_property, out IForecastProperty property))
        {
            Warehouse warehouse = a_sd.WarehouseManager.GetById(a_id.WarehouseId);
            Inventory inv = warehouse.Inventories[a_id.InvItemId];
            Forecast forecast = inv.ForecastVersionActive.GetById(a_id.ForecastId);
            ForecastShipment forecastShipment = null;

            foreach (ForecastShipment shipment in forecast.Shipments)
            {
                if (shipment.Id == a_id.ShipmentId)
                {
                    forecastShipment = shipment;
                    return property.GetValue(inv, forecast, forecastShipment);
                }
            }
        }

        return null;
    }
}