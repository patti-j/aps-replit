using PT.APSCommon;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.Scheduler;
using PT.SchedulerDefinitions;

using ResourceKey = PT.SchedulerData.ObjectKeys.ResourceKey;

namespace PT.PackageDefinitionsUI.DataSources;

internal class ResourceDataSource : IResourceDataSource
{
    private readonly ResourceObjectDataSource m_resourceDataSource;
    private readonly DepartmentObjectDataSource m_departmentDataSource;
    private readonly PlantObjectDataSource m_plantDataSource;
    private readonly List<IDataSourceExtensionElement> m_extensionModules = new ();

    internal ResourceDataSource(IScenarioInfo a_scenarioInfo, BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_objectProperties, IPackageManagerUI a_packageManager, IMainForm a_mainForm)
    {
        m_resourceDataSource = new ResourceObjectDataSource(a_scenarioId, a_sdCache, a_objectProperties);
        m_departmentDataSource = new DepartmentObjectDataSource(a_scenarioId, a_sdCache, a_objectProperties);
        m_plantDataSource = new PlantObjectDataSource(a_scenarioId, a_sdCache, a_objectProperties);

        //Load dataSource extensions
        DataSourceUtilities.LoadDataSourceExtensions(a_packageManager, a_mainForm, a_scenarioInfo, ref m_extensionModules);
    }

    public LookUpValueStruct GetOverrideValue(string a_property, object propValue)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(GetPropertyFromColumnKey(a_property), propValue, m_extensionModules);
    }

    public LookUpValueStruct GetPlantValue(string a_property, ResourceKey a_id)
    {
        return m_plantDataSource.GetValue(a_id, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverridePlantValue(string a_property, ResourceKey a_id)
    {
        return GetOverrideValue(a_property, GetPlantValue(a_property, a_id));
    }

    public List<LookUpValueStruct> GetPlantValueBlock(string a_property, List<ResourceKey> a_id)
    {
        return m_plantDataSource.GetValueBlock(a_id, a_property);
    }

    public List<LookUpValueStruct> GetValueBlock(IPlantProperty a_property, List<ResourceKey> a_id)
    {
        return GetPlantValueBlock(a_property.PropertyName, a_id);
    }

    public List<LookUpValueStruct> GetOverrideValueBlock(IPlantProperty a_property, List<ResourceKey> a_id)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetValueBlock(a_property, a_id), m_extensionModules);
    }

    public LookUpValueStruct GetValue(IPlantProperty a_property, ResourceKey a_id)
    {
        return GetPlantValue(a_property.PropertyName, a_id);
    }

    public LookUpValueStruct GetOverrideValue(IPlantProperty a_property, ResourceKey a_id)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetValue(a_property, a_id), m_extensionModules);
    }

    public LookUpValueStruct GetDepartmentValue(string a_property, ResourceKey a_id)
    {
        return m_departmentDataSource.GetValue(a_id, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverrideDepartmentValue(string a_property, ResourceKey a_id)
    {
        return GetOverrideValue(a_property, GetDepartmentValue(a_property, a_id));
    }

    public List<LookUpValueStruct> GetDepartmentValueBlock(string a_property, List<ResourceKey> a_id)
    {
        return m_departmentDataSource.GetValueBlock(a_id, a_property);
    }

    public List<LookUpValueStruct> GetValueBlock(IDepartmentProperty a_property, List<ResourceKey> a_id)
    {
        return GetDepartmentValueBlock(a_property.PropertyName, a_id);
    }

    public List<LookUpValueStruct> GetOverrideValueBlock(IDepartmentProperty a_property, List<ResourceKey> a_id)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetValueBlock(a_property, a_id), m_extensionModules);
    }

    public LookUpValueStruct GetValue(IDepartmentProperty a_property, ResourceKey a_id)
    {
        return GetDepartmentValue(a_property.PropertyName, a_id);
    }

    public LookUpValueStruct GetOverrideValue(IDepartmentProperty a_property, ResourceKey a_id)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetValue(a_property, a_id), m_extensionModules);
    }

    public LookUpValueStruct GetResourceValue(string a_property, ResourceKey a_id)
    {
        return m_resourceDataSource.GetValue(a_id, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverrideResourceValue(string a_property, ResourceKey a_id)
    {
        return GetOverrideValue(a_property, GetResourceValue(a_property, a_id));
    }

    public List<LookUpValueStruct> GetResourceValueBlock(string a_property, List<ResourceKey> a_id)
    {
        return m_resourceDataSource.GetValueBlock(a_id, a_property);
    }

    public List<LookUpValueStruct> GetValueBlock(IResourceProperty a_property, List<ResourceKey> a_id)
    {
        return GetResourceValueBlock(a_property.PropertyName, a_id);
    }

    public List<LookUpValueStruct> GetOverrideValueBlock(IResourceProperty a_property, List<ResourceKey> a_id)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetValueBlock(a_property, a_id), m_extensionModules);
    }

    public LookUpValueStruct GetValue(IResourceProperty a_property, ResourceKey a_id)
    {
        return GetResourceValue(a_property.PropertyName, a_id);
    }

    public LookUpValueStruct GetOverrideValue(IResourceProperty a_property, ResourceKey a_id)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetValue(a_property, a_id), m_extensionModules);
    }

    public void AddScenarioDetailReference(ScenarioDetailCache a_cache)
    {
        m_resourceDataSource.AddScenarioDetailReference(a_cache);
        m_departmentDataSource.AddScenarioDetailReference(a_cache);
        m_plantDataSource.AddScenarioDetailReference(a_cache);
    }

    public void DeleteScenarioDetailReference(ScenarioDetailCache a_cache)
    {
        m_resourceDataSource.DeleteScenarioDetailReference(a_cache);
        m_departmentDataSource.DeleteScenarioDetailReference(a_cache);
        m_plantDataSource.DeleteScenarioDetailReference(a_cache);
    }

    public IObjectProperty GetPropertyFromColumnKey(string a_key)
    {
        string propName = GridConstants.RemovePropPrefix(a_key);

        switch (GridConstants.GetPrefix(a_key))
        {
            case GridConstants.ResourcePropPrefix:
                if (m_resourceDataSource.GetPropLookup().TryGetValue(propName, out IResourceProperty resProperty))
                {
                    return resProperty;
                }

                break;
            case GridConstants.DepartmentPropPrefix:
                if (m_departmentDataSource.GetPropLookup().TryGetValue(propName, out IDepartmentProperty deptProperty))
                {
                    return deptProperty;
                }

                break;
            case GridConstants.PlantPropPrefix:
                if (m_plantDataSource.GetPropLookup().TryGetValue(propName, out IPlantProperty plantProperty))
                {
                    return plantProperty;
                }

                break;
        }

        return null;
    }

    public void SignalSimulationStarted()
    {
        m_plantDataSource.SignalSimulationStarted();
        m_departmentDataSource.SignalSimulationStarted();
        m_resourceDataSource.SignalSimulationStarted();
    }

    public void SignalSimulationCompleted()
    {
        m_plantDataSource.SignalSimulationCompleted();
        m_departmentDataSource.SignalSimulationCompleted();
        m_resourceDataSource.SignalSimulationCompleted();
    }

    public void SignalScenarioActivated()
    {
        m_plantDataSource.SignalScenarioActivated();
        m_departmentDataSource.SignalScenarioActivated();
        m_resourceDataSource.SignalScenarioActivated();
    }

    public void SignalDataChanged(IScenarioDataChanges a_dataChanges)
    {
        if (a_dataChanges.HasUnprocessedChanges)
        {
            if (a_dataChanges.PlantChanges.HasChanges)
            {
                m_plantDataSource.Clear();
            }

            if (a_dataChanges.DepartmentChanges.HasChanges)
            {
                m_departmentDataSource.Clear();
            }

            if (a_dataChanges.MachineChanges.HasChanges)
            {
                m_resourceDataSource.Clear();
            }
        }
    }
}

internal class ResourceObjectDataSource : BaseObjectDataSource<ResourceKey, IResourceProperty>
{
    public Dictionary<string, IResourceProperty> GetPropLookup()
    {
        return m_propLookup;
    }

    public void Clear()
    {
        ClearData();
    }

    internal ResourceObjectDataSource(BaseId a_scenarioInfo, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_objectProperties) : base(a_scenarioInfo, a_sdCache)
    {
        foreach (IObjectProperty objectProperty in a_objectProperties)
        {
            if (objectProperty is IResourceProperty resProperty)
            {
                m_propLookup.Add(resProperty.PropertyName, resProperty);
            }
        }
    }

    public override object GetValue(string a_property, ResourceKey a_id, ScenarioDetail a_sd)
    {
        if (m_propLookup.TryGetValue(a_property, out IResourceProperty property))
        {
            Plant plant = a_sd.PlantManager.GetById(a_id.PlantId);
            Department dept = plant.Departments.GetById(a_id.DepartmentId);
            Resource res = dept.Resources.GetById(a_id.ResourceId);
            return property.GetValue(res, a_sd);
        }

        return null;
    }
}

internal class DepartmentObjectDataSource : BaseObjectDataSource<ResourceKey, IDepartmentProperty>
{
    public Dictionary<string, IDepartmentProperty> GetPropLookup()
    {
        return m_propLookup;
    }

    public void Clear()
    {
        ClearData();
    }

    internal DepartmentObjectDataSource(BaseId a_scenarioInfo, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_objectProperties) : base(a_scenarioInfo, a_sdCache)
    {
        foreach (IObjectProperty objectProperty in a_objectProperties)
        {
            if (objectProperty is IDepartmentProperty property)
            {
                m_propLookup.Add(property.PropertyName, property);
            }
        }
    }

    public override object GetValue(string a_property, ResourceKey a_id, ScenarioDetail a_sd)
    {
        if (m_propLookup.TryGetValue(a_property, out IDepartmentProperty property))
        {
            Plant plant = a_sd.PlantManager.GetById(a_id.PlantId);
            Department dept = plant.Departments.GetById(a_id.DepartmentId);
            return property.GetValue(dept, a_sd);
        }

        return null;
    }
}

internal class PlantObjectDataSource : BaseObjectDataSource<ResourceKey, IPlantProperty>
{
    public Dictionary<string, IPlantProperty> GetPropLookup()
    {
        return m_propLookup;
    }

    public void Clear()
    {
        ClearData();
    }

    internal PlantObjectDataSource(BaseId a_scenarioInfo, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_objectProperties) : base(a_scenarioInfo, a_sdCache)
    {
        foreach (IObjectProperty objectProperty in a_objectProperties)
        {
            if (objectProperty is IPlantProperty property)
            {
                m_propLookup.Add(property.PropertyName, property);
            }
        }
    }

    public override object GetValue(string a_property, ResourceKey a_id, ScenarioDetail a_sd)
    {
        if (m_propLookup.TryGetValue(a_property, out IPlantProperty property))
        {
            Plant plant = a_sd.PlantManager.GetById(a_id.PlantId);
            return property.GetValue(plant, a_sd);
        }

        return null;
    }
}