using PT.APSCommon;
using PT.Common.Debugging;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.Scheduler;
using PT.SchedulerDefinitions;

using InventoryKey = PT.SchedulerData.ObjectKeys.InventoryKey;

namespace PT.PackageDefinitionsUI.DataSources;

internal class InventoryDataSource : IInventoryDataSource
{
    private readonly WarehouseObjectDataSource m_warehouseObjectDataSource;
    private readonly InventoryObjectDataSource m_inventoryObjectDataSource;
    private readonly ItemObjectDataSource m_itemObjectDataSource;
    private readonly List<IDataSourceExtensionElement> m_extensionModules = new ();

    internal InventoryDataSource(IScenarioInfo a_scenarioInfo, BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties, IPackageManagerUI a_packageManager, IMainForm a_mainForm)
    {
        m_warehouseObjectDataSource = new WarehouseObjectDataSource(a_scenarioId, a_sdCache, a_properties);
        m_inventoryObjectDataSource = new InventoryObjectDataSource(a_scenarioId, a_sdCache, a_properties);
        m_itemObjectDataSource = new ItemObjectDataSource(a_scenarioId, a_sdCache, a_properties);

        //Load dataSource extensions
        DataSourceUtilities.LoadDataSourceExtensions(a_packageManager, a_mainForm, a_scenarioInfo, ref m_extensionModules);
    }

    public LookUpValueStruct GetOverrideValue(string a_property, object propValue)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(GetPropertyFromColumnKey(a_property), propValue, m_extensionModules);
    }

    public LookUpValueStruct GetWarehouseValue(string a_property, InventoryKey a_objectId)
    {
        return m_warehouseObjectDataSource.GetValue(a_objectId, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverrideWarehouseValue(string a_property, InventoryKey a_objectId)
    {
        return GetOverrideValue(a_property, GetWarehouseValue(a_property, a_objectId));
    }

    public LookUpValueStruct GetWarehouseValue(IWarehouseProperty a_property, InventoryKey a_id)
    {
        return GetWarehouseValue(a_property.PropertyName, a_id);
    }

    public LookUpValueStruct GetOverrideWarehouseValue(IWarehouseProperty a_property, InventoryKey a_id)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetWarehouseValue(a_property, a_id), m_extensionModules);
    }

    public LookUpValueStruct GetItemValue(string a_property, InventoryKey a_id)
    {
        return m_itemObjectDataSource.GetValue(a_id, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverrideItemValue(string a_property, InventoryKey a_id)
    {
        return GetOverrideValue(a_property, GetItemValue(a_property, a_id));
    }

    public LookUpValueStruct GetItemValue(IItemProperty a_property, InventoryKey a_id)
    {
        return GetItemValue(a_property.PropertyName, a_id);
    }

    public LookUpValueStruct GetOverrideItemValue(IItemProperty a_property, InventoryKey a_id)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetItemValue(a_property, a_id), m_extensionModules);
    }

    public LookUpValueStruct GetInventoryValue(string a_property, InventoryKey a_id)
    {
        return m_inventoryObjectDataSource.GetValue(a_id, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverrideInventoryValue(string a_property, InventoryKey a_id)
    {
        return GetOverrideValue(a_property, GetInventoryValue(a_property, a_id));
    }

    public LookUpValueStruct GetInventoryValue(IInventoryProperty a_property, InventoryKey a_id)
    {
        return GetInventoryValue(a_property.PropertyName, a_id);
    }

    public LookUpValueStruct GetOverrideInventoryValue(IInventoryProperty a_property, InventoryKey a_id)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetInventoryValue(a_property, a_id), m_extensionModules);
    }

    public List<LookUpValueStruct> GetWarehouseValueBlock(string a_property, List<InventoryKey> a_ids)
    {
        return m_warehouseObjectDataSource.GetValueBlock(a_ids, a_property);
    }

    public List<LookUpValueStruct> GetWarehouseValueBlock(IWarehouseProperty a_property, List<InventoryKey> a_ids)
    {
        return GetWarehouseValueBlock(a_property.PropertyName, a_ids);
    }

    public List<LookUpValueStruct> GetOverrideWarehouseValueBlock(IWarehouseProperty a_property, List<InventoryKey> a_ids)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetWarehouseValueBlock(a_property, a_ids), m_extensionModules);
    }

    public List<LookUpValueStruct> GetItemValueBlock(string a_property, List<InventoryKey> a_ids)
    {
        return m_itemObjectDataSource.GetValueBlock(a_ids, a_property);
    }

    public List<LookUpValueStruct> GetItemValueBlock(IItemProperty a_property, List<InventoryKey> a_ids)
    {
        return GetItemValueBlock(a_property.PropertyName, a_ids);
    }

    public List<LookUpValueStruct> GetOverrideItemValueBlock(IItemProperty a_property, List<InventoryKey> a_ids)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetItemValueBlock(a_property, a_ids), m_extensionModules);
    }

    public List<LookUpValueStruct> GetInventoryValueBlock(string a_property, List<InventoryKey> a_ids)
    {
        return m_inventoryObjectDataSource.GetValueBlock(a_ids, a_property);
    }

    public List<LookUpValueStruct> GetInventoryValueBlock(IInventoryProperty a_property, List<InventoryKey> a_ids)
    {
        return GetInventoryValueBlock(a_property.PropertyName, a_ids);
    }

    public List<LookUpValueStruct> GetOverrideInventoryValueBlock(IInventoryProperty a_property, List<InventoryKey> a_ids)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetInventoryValueBlock(a_property, a_ids), m_extensionModules);
    }

    private static Warehouse GetWarehouse(InventoryKey a_id, ScenarioDetail a_sd)
    {
        Warehouse wh = a_sd.WarehouseManager.GetById(a_id.WarehouseId);

        //Could be a Tank Warehouse. Check the Resources 
        if (wh == null)
        {
            List<Resource> resources = a_sd.PlantManager.GetResourceList();
            foreach (Resource resource in resources)
            {
                if (resource.IsTank && resource.Tank.Id == a_id.WarehouseId)
                {
                    wh = resource.Tank;
                    break;
                }
            }
        }

        return wh;
    }

    private class WarehouseObjectDataSource : BaseObjectDataSource<InventoryKey, IWarehouseProperty>
    {
        public Dictionary<string, IWarehouseProperty> GetPropLookup()
        {
            return m_propLookup;
        }

        public void Clear()
        {
            ClearData();
        }

        internal WarehouseObjectDataSource(BaseId a_scenarioInfo, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties) : base(a_scenarioInfo, a_sdCache)
        {
            foreach (IObjectProperty objectProperty in a_properties)
            {
                if (objectProperty is IWarehouseProperty property)
                {
                    m_propLookup.Add(property.PropertyName, property);
                }
            }
        }

        public override object GetValue(string a_property, InventoryKey a_id, ScenarioDetail a_sd)
        {
            if (m_propLookup.TryGetValue(a_property, out IWarehouseProperty property))
            {
                Warehouse wh = GetWarehouse(a_id, a_sd);

                return property.GetValue(wh, a_sd);
            }

            throw new DebugException("Data issue");
        }
    }

    private class InventoryObjectDataSource : BaseObjectDataSource<InventoryKey, IInventoryProperty>
    {
        public Dictionary<string, IInventoryProperty> GetPropLookup()
        {
            return m_propLookup;
        }

        public void Clear()
        {
            ClearData();
        }

        internal InventoryObjectDataSource(BaseId a_scenarioInfo, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties) : base(a_scenarioInfo, a_sdCache)
        {
            foreach (IObjectProperty objectProperty in a_properties)
            {
                if (objectProperty is IInventoryProperty property)
                {
                    m_propLookup.Add(property.PropertyName, property);
                }
            }
        }

        public override object GetValue(string a_property, InventoryKey a_id, ScenarioDetail a_sd)
        {
            if (m_propLookup.TryGetValue(a_property, out IInventoryProperty property))
            {
                Warehouse wh = GetWarehouse(a_id, a_sd);
                Inventory inventory = wh.Inventories[a_id.ItemId];

                return property.GetValue(inventory, true, a_sd);
            }

            throw new DebugException("Data issue");
        }
    }

    private class ItemObjectDataSource : BaseObjectDataSource<InventoryKey, IItemProperty>
    {
        public Dictionary<string, IItemProperty> GetPropLookup()
        {
            return m_propLookup;
        }

        public void Clear()
        {
            ClearData();
        }

        internal ItemObjectDataSource(BaseId a_scenarioInfo, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties) : base(a_scenarioInfo, a_sdCache)
        {
            foreach (IObjectProperty objectProperty in a_properties)
            {
                if (objectProperty is IItemProperty property)
                {
                    m_propLookup.Add(property.PropertyName, property);
                }
            }
        }

        public override object GetValue(string a_property, InventoryKey a_id, ScenarioDetail a_sd)
        {
            if (m_propLookup.TryGetValue(a_property, out IItemProperty property))
            {
                //This can happen for buy-direct materials
                if (a_id.ItemId == BaseId.NULL_ID)
                {
                    return null;
                }

                Item item = a_sd.ItemManager.GetById(a_id.ItemId);
                return property.GetValue(item, a_sd);
            }

            throw new DebugException("Data issue");
        }
    }

    public void AddScenarioDetailReference(ScenarioDetailCache a_cache)
    {
        m_warehouseObjectDataSource.AddScenarioDetailReference(a_cache);
        m_inventoryObjectDataSource.AddScenarioDetailReference(a_cache);
        m_itemObjectDataSource.AddScenarioDetailReference(a_cache);
    }

    public void DeleteScenarioDetailReference(ScenarioDetailCache a_cache)
    {
        m_warehouseObjectDataSource.DeleteScenarioDetailReference(a_cache);
        m_inventoryObjectDataSource.DeleteScenarioDetailReference(a_cache);
        m_itemObjectDataSource.DeleteScenarioDetailReference(a_cache);
    }

    public IObjectProperty GetPropertyFromColumnKey(string a_key)
    {
        string propName = GridConstants.RemovePropPrefix(a_key);

        switch (GridConstants.GetPrefix(a_key))
        {
            case GridConstants.WarehousePropPrefix:
                if (m_warehouseObjectDataSource.GetPropLookup().TryGetValue(propName, out IWarehouseProperty whProperty))
                {
                    return whProperty;
                }

                break;
            case GridConstants.InventoryPropPrefix:
                if (m_inventoryObjectDataSource.GetPropLookup().TryGetValue(propName, out IInventoryProperty invProperty))
                {
                    return invProperty;
                }

                break;
            case GridConstants.ItemPropPrefix:
                if (m_itemObjectDataSource.GetPropLookup().TryGetValue(propName, out IItemProperty itemProperty))
                {
                    return itemProperty;
                }

                break;
        }

        return null;
    }

    public void SignalSimulationStarted()
    {
        m_warehouseObjectDataSource.SignalSimulationStarted();
        m_inventoryObjectDataSource.SignalSimulationStarted();
        m_itemObjectDataSource.SignalSimulationStarted();
    }

    public void SignalSimulationCompleted()
    {
        m_warehouseObjectDataSource.SignalSimulationCompleted();
        m_inventoryObjectDataSource.SignalSimulationCompleted();
        m_itemObjectDataSource.SignalSimulationCompleted();
    }

    public void SignalScenarioActivated()
    {
        m_warehouseObjectDataSource.SignalScenarioActivated();
        m_inventoryObjectDataSource.SignalScenarioActivated();
        m_itemObjectDataSource.SignalScenarioActivated();
    }

    public void SignalDataChanged(IScenarioDataChanges a_dataChanges)
    {
        if (a_dataChanges.HasUnprocessedChanges)
        {
            if (a_dataChanges.WarehouseChanges.HasChanges)
            {
                m_warehouseObjectDataSource.Clear();
                m_inventoryObjectDataSource.Clear();
            }

            if (a_dataChanges.InventoryChanges.HasChanges)
            {
                m_inventoryObjectDataSource.Clear();
            }

            if (a_dataChanges.ItemChanges.HasChanges || a_dataChanges.SalesOrderChanges.HasChanges)
            {
                m_itemObjectDataSource.Clear();
                m_inventoryObjectDataSource.Clear();
            }
        }
    }
}