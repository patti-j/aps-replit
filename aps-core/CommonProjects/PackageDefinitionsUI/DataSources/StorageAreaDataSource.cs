using PT.APSCommon;
using PT.Common.Debugging;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.Scheduler;
using PT.SchedulerData.ObjectKeys;
using PT.SchedulerDefinitions;

namespace PT.PackageDefinitionsUI.DataSources;

internal class StorageAreaDataSource : IStorageAreaDataSource
{
    private readonly WarehouseObjectDataSource m_warehouseObjectDataSource;
    private readonly StorageAreaObjectDataSource m_storageAreaObjectDataSource;
    private readonly List<IDataSourceExtensionElement> m_extensionModules = new ();

    internal StorageAreaDataSource(IScenarioInfo a_scenarioInfo, BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties, IPackageManagerUI a_packageManager, IMainForm a_mainForm)
    {
        m_warehouseObjectDataSource = new WarehouseObjectDataSource(a_scenarioId, a_sdCache, a_properties);
        m_storageAreaObjectDataSource = new StorageAreaObjectDataSource(a_scenarioId, a_sdCache, a_properties);

        //Load dataSource extensions
        DataSourceUtilities.LoadDataSourceExtensions(a_packageManager, a_mainForm, a_scenarioInfo, ref m_extensionModules);
    }

    public LookUpValueStruct GetStorageAreaValue(string a_property, StorageAreaKey a_id)
    {
        return m_storageAreaObjectDataSource.GetValue(a_id, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetStorageAreaValue(IStorageAreaProperty a_property, StorageAreaKey a_id)
    {
        return GetStorageAreaValue(a_property.PropertyName, a_id);
    }

    public List<LookUpValueStruct> GetStorageAreaValueBlock(string a_property, List<StorageAreaKey> a_ids)
    {
        return m_storageAreaObjectDataSource.GetValueBlock(a_ids, a_property);
    }

    public List<LookUpValueStruct> GetStorageAreaValueBlock(IStorageAreaProperty a_property, List<StorageAreaKey> a_ids)
    {
        return GetStorageAreaValueBlock(a_property.PropertyName, a_ids);
    }

    public LookUpValueStruct GetOverrideStorageAreaValue(string a_property, StorageAreaKey a_id)
    {
        return GetOverrideValue(a_property, GetStorageAreaValue(a_property, a_id));
    }

    public LookUpValueStruct GetOverrideStorageAreaValue(IStorageAreaProperty a_property, StorageAreaKey a_id)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetStorageAreaValue(a_property, a_id), m_extensionModules);
    }
    public List<LookUpValueStruct> GetOverrideStorageAreaValueBlock(IStorageAreaProperty a_property, List<StorageAreaKey> a_ids)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetStorageAreaValueBlock(a_property, a_ids), m_extensionModules);
    }

    public LookUpValueStruct GetOverrideValue(string a_property, object propValue)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(GetPropertyFromColumnKey(a_property), propValue, m_extensionModules);
    }

    public LookUpValueStruct GetWarehouseValue(string a_property, StorageAreaKey a_objectId)
    {
        return m_warehouseObjectDataSource.GetValue(a_objectId, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverrideWarehouseValue(string a_property, StorageAreaKey a_objectId)
    {
        return GetOverrideValue(a_property, GetWarehouseValue(a_property, a_objectId));
    }

    public LookUpValueStruct GetWarehouseValue(IWarehouseProperty a_property, StorageAreaKey a_id)
    {
        return GetWarehouseValue(a_property.PropertyName, a_id);
    }

    public LookUpValueStruct GetOverrideWarehouseValue(IWarehouseProperty a_property, StorageAreaKey a_id)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetWarehouseValue(a_property, a_id), m_extensionModules);
    }
    public List<LookUpValueStruct> GetWarehouseValueBlock(string a_property, List<StorageAreaKey> a_ids)
    {
        return m_warehouseObjectDataSource.GetValueBlock(a_ids, a_property);
    }

    public List<LookUpValueStruct> GetWarehouseValueBlock(IWarehouseProperty a_property, List<StorageAreaKey> a_ids)
    {
        return GetWarehouseValueBlock(a_property.PropertyName, a_ids);
    }

    public List<LookUpValueStruct> GetOverrideWarehouseValueBlock(IWarehouseProperty a_property, List<StorageAreaKey> a_ids)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetWarehouseValueBlock(a_property, a_ids), m_extensionModules);
    }
    private static Warehouse GetWarehouse(StorageAreaKey a_id, ScenarioDetail a_sd)
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

    private class WarehouseObjectDataSource : BaseObjectDataSource<StorageAreaKey, IWarehouseProperty>
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

        public override object GetValue(string a_property, StorageAreaKey a_id, ScenarioDetail a_sd)
        {
            if (m_propLookup.TryGetValue(a_property, out IWarehouseProperty property))
            {
                Warehouse wh = GetWarehouse(a_id, a_sd);

                return property.GetValue(wh, a_sd);
            }

            throw new DebugException("Data issue");
        }
    }

    private class StorageAreaObjectDataSource : BaseObjectDataSource<StorageAreaKey, IStorageAreaProperty>
    {
        public Dictionary<string, IStorageAreaProperty> GetPropLookup()
        {
            return m_propLookup;
        }

        public void Clear()
        {
            ClearData();
        }

        internal StorageAreaObjectDataSource(BaseId a_scenarioInfo, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties) : base(a_scenarioInfo, a_sdCache)
        {
            foreach (IObjectProperty objectProperty in a_properties)
            {
                if (objectProperty is IStorageAreaProperty property)
                {
                    m_propLookup.Add(property.PropertyName, property);
                }
            }
        }

        public override object GetValue(string a_property, StorageAreaKey a_id, ScenarioDetail a_sd)
        {
            if (m_propLookup.TryGetValue(a_property, out IStorageAreaProperty property))
            {
                Warehouse wh = GetWarehouse(a_id, a_sd);
                StorageArea storageArea = wh.StorageAreas.Find(a_id.StorageAreaId);

                return property.GetValue(storageArea, a_sd);
            }

            throw new DebugException("Data issue");
        }
    }

    public void AddScenarioDetailReference(ScenarioDetailCache a_cache)
    {
        m_warehouseObjectDataSource.AddScenarioDetailReference(a_cache);
        m_storageAreaObjectDataSource.AddScenarioDetailReference(a_cache);
    }

    public void DeleteScenarioDetailReference(ScenarioDetailCache a_cache)
    {
        m_warehouseObjectDataSource.DeleteScenarioDetailReference(a_cache);
        m_storageAreaObjectDataSource.DeleteScenarioDetailReference(a_cache);
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
            case GridConstants.StorageAreaPropPrefix:
                if (m_storageAreaObjectDataSource.GetPropLookup().TryGetValue(propName, out IStorageAreaProperty invProperty))
                {
                    return invProperty;
                }

                break;
        }

        return null;
    }

    public void SignalSimulationStarted()
    {
        m_warehouseObjectDataSource.SignalSimulationStarted();
        m_storageAreaObjectDataSource.SignalSimulationStarted();
    }

    public void SignalSimulationCompleted()
    {
        m_warehouseObjectDataSource.SignalSimulationCompleted();
        m_storageAreaObjectDataSource.SignalSimulationCompleted();
    }

    public void SignalScenarioActivated()
    {
        m_warehouseObjectDataSource.SignalScenarioActivated();
        m_storageAreaObjectDataSource.SignalScenarioActivated();
    }

    public void SignalDataChanged(IScenarioDataChanges a_dataChanges)
    {
        if (a_dataChanges.HasUnprocessedChanges)
        {
            if (a_dataChanges.WarehouseChanges.HasChanges)
            {
                m_warehouseObjectDataSource.Clear();
                m_storageAreaObjectDataSource.Clear();
            }

            if (a_dataChanges.StorageAreaChanges.HasChanges)
            {
                m_storageAreaObjectDataSource.Clear();
            }
        }
    }
}