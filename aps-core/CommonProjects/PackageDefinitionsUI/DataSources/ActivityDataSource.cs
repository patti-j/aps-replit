using PT.APSCommon;
using PT.Common.Debugging;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.Scheduler;
using PT.SchedulerDefinitions;

using ActivityKey = PT.SchedulerData.ObjectKeys.ActivityKey;

namespace PT.PackageDefinitionsUI.DataSources;

internal class ActivityDataSource : IActivityDataSource
{
    private readonly ActivityObjectDataSource m_actDataSource;
    private readonly ResourceBlockObjectDataSource m_resBlockDataSource;
    private readonly ResourceRequirementObjectDataSource m_rrDataSource;
    private readonly BatchObjectDataSource m_batchDataSource;
    private readonly List<IDataSourceExtensionElement> m_extensionModules = new ();

    internal ActivityDataSource(IScenarioInfo a_scenarioInfo, BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties, IPackageManagerUI a_packageManager, IMainForm a_mainForm)
    {
        m_actDataSource = new ActivityObjectDataSource(a_scenarioId, a_sdCache, a_properties);
        m_resBlockDataSource = new ResourceBlockObjectDataSource(a_scenarioId, a_sdCache, a_properties);
        m_rrDataSource = new ResourceRequirementObjectDataSource(a_scenarioId, a_sdCache, a_properties);
        m_batchDataSource = new BatchObjectDataSource(a_scenarioId, a_sdCache, a_properties);

        //Load dataSource extensions
        DataSourceUtilities.LoadDataSourceExtensions(a_packageManager, a_mainForm, a_scenarioInfo, ref m_extensionModules);
    }

    public LookUpValueStruct GetOverrideValue(string a_property, object propValue)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(GetPropertyFromColumnKey(a_property), propValue, m_extensionModules);
    }

    public LookUpValueStruct GetActivityValue(string a_property, ActivityKey a_id)
    {
        return m_actDataSource.GetValue(a_id, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverrideActivityValue(string a_property, ActivityKey a_id)
    {
        return GetOverrideValue(a_property, GetActivityValue(a_property, a_id));
    }

    public LookUpValueStruct GetResourceBlockValue(string a_property, ActivityKey a_id)
    {
        return m_resBlockDataSource.GetValue(a_id, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverrideResourceBlockValue(string a_property, ActivityKey a_id)
    {
        return GetOverrideValue(a_property, GetResourceBlockValue(a_property, a_id));
    }

    public LookUpValueStruct GetBatchValue(string a_property, ActivityKey a_id)
    {
        return m_batchDataSource.GetValue(a_id, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverrideBatchValue(string a_property, ActivityKey a_id)
    {
        return GetOverrideValue(a_property, GetBatchValue(a_property, a_id));
    }

    public LookUpValueStruct GetResourceRequirementValue(string a_property, ActivityKey a_id)
    {
        return m_rrDataSource.GetValue(a_id, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverrideResourceRequirementValue(string a_property, ActivityKey a_id)
    {
        return GetOverrideValue(a_property, GetResourceRequirementValue(a_property, a_id));
    }

    public LookUpValueStruct GetValue(IActivityProperty a_property, ActivityKey a_id)
    {
        return m_actDataSource.GetValue(a_id, a_property.PropertyName);
    }

    public LookUpValueStruct GetOverrideValue(IActivityProperty a_property, ActivityKey a_id)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetValue(a_property, a_id), m_extensionModules);
    }

    public LookUpValueStruct GetValue(IBatchProperty a_property, ActivityKey a_id)
    {
        return m_batchDataSource.GetValue(a_id, a_property.PropertyName);
    }

    public LookUpValueStruct GetOverrideValue(IBatchProperty a_property, ActivityKey a_id)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetValue(a_property, a_id), m_extensionModules);
    }

    public LookUpValueStruct GetValue(IResourceRequirementProperty a_property, ActivityKey a_id)
    {
        return m_rrDataSource.GetValue(a_id, a_property.PropertyName);
    }

    public LookUpValueStruct GetOverrideValue(IResourceRequirementProperty a_property, ActivityKey a_id)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetValue(a_property, a_id), m_extensionModules);
    }

    public LookUpValueStruct GetValue(IResourceBlockProperty a_property, ActivityKey a_id)
    {
        return m_resBlockDataSource.GetValue(a_id, a_property.PropertyName);
    }

    public LookUpValueStruct GetOverrideValue(IResourceBlockProperty a_property, ActivityKey a_id)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetValue(a_property, a_id), m_extensionModules);
    }

    public List<LookUpValueStruct> GetValueBlock(IActivityProperty a_property, List<ActivityKey> a_ids)
    {
        return m_actDataSource.GetValueBlock(a_ids, a_property.PropertyName);
    }

    public List<LookUpValueStruct> GetOverrideValueBlock(IActivityProperty a_property, List<ActivityKey> a_ids)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetValueBlock(a_property, a_ids), m_extensionModules);
    }

    public List<LookUpValueStruct> GetValueBlock(IBatchProperty a_property, List<ActivityKey> a_ids)
    {
        return m_batchDataSource.GetValueBlock(a_ids, a_property.PropertyName);
    }

    public List<LookUpValueStruct> GetOverrideValueBlock(IBatchProperty a_property, List<ActivityKey> a_ids)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetValueBlock(a_property, a_ids), m_extensionModules);
    }

    public List<LookUpValueStruct> GetValueBlock(IResourceRequirementProperty a_property, List<ActivityKey> a_ids)
    {
        return m_rrDataSource.GetValueBlock(a_ids, a_property.PropertyName);
    }

    public List<LookUpValueStruct> GetOverrideValueBlock(IResourceRequirementProperty a_property, List<ActivityKey> a_ids)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetValueBlock(a_property, a_ids), m_extensionModules);
    }

    public List<LookUpValueStruct> GetValueBlock(IResourceBlockProperty a_property, List<ActivityKey> a_ids)
    {
        return m_resBlockDataSource.GetValueBlock(a_ids, a_property.PropertyName);
    }

    public List<LookUpValueStruct> GetOverrideValueBlock(IResourceBlockProperty a_property, List<ActivityKey> a_ids)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetValueBlock(a_property, a_ids), m_extensionModules);
    }
    //Todo continue removal of GetOverrideValue from each datasource to ScenarioControls

    private class ActivityObjectDataSource : BaseObjectDataSource<ActivityKey, IActivityProperty>
    {
        public Dictionary<string, IActivityProperty> GetPropLookup()
        {
            return m_propLookup;
        }

        public void Clear()
        {
            ClearData();
        }

        internal ActivityObjectDataSource(BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties) : base(a_scenarioId, a_sdCache)
        {
            foreach (IObjectProperty objectProperty in a_properties)
            {
                if (objectProperty is IActivityProperty property)
                {
                    m_propLookup.Add(property.PropertyName, property);
                }
            }
        }

        public override object GetValue(string a_property, ActivityKey a_key, ScenarioDetail a_sd)
        {
            if (m_propLookup.TryGetValue(a_property, out IActivityProperty property))
            {
                Job job = a_sd.JobManager.GetById(a_key.JobId);
                ManufacturingOrder mo = job.ManufacturingOrders.GetById(a_key.MOId);
                InternalOperation operation = (InternalOperation)mo.OperationManager[a_key.OperationId];
                InternalActivity activity = operation.Activities.FindActivity(a_key.ActivityId);

                Plant plant = a_sd.PlantManager.GetById(a_key.PlantId);
                Department dept = plant.Departments.GetById(a_key.DepartmentId);
                Resource resource = dept.Resources.GetById(a_key.ResourceId);
                return property.GetValue(activity, resource, a_sd);
            }

            throw new DebugException("Data issue");
        }
    }

    private class BatchObjectDataSource : BaseObjectDataSource<ActivityKey, IBatchProperty>
    {
        public Dictionary<string, IBatchProperty> GetPropLookup()
        {
            return m_propLookup;
        }

        public void Clear()
        {
            ClearData();
        }

        internal BatchObjectDataSource(BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties) : base(a_scenarioId, a_sdCache)
        {
            foreach (IObjectProperty objectProperty in a_properties)
            {
                if (objectProperty is IBatchProperty property)
                {
                    m_propLookup.Add(property.PropertyName, property);
                }
            }
        }

        public override object GetValue(string a_property, ActivityKey a_key, ScenarioDetail a_sd)
        {
            if (m_propLookup.TryGetValue(a_property, out IBatchProperty property))
            {
                Job job = a_sd.JobManager.GetById(a_key.JobId);
                ManufacturingOrder mo = job.ManufacturingOrders.GetById(a_key.MOId);
                InternalOperation operation = (InternalOperation)mo.OperationManager[a_key.OperationId];
                InternalActivity activity = operation.Activities.FindActivity(a_key.ActivityId);
                Batch batch = activity.Batch;
                return property.GetValue(batch);
            }

            throw new DebugException("Data issue");
        }
    }

    private class ResourceBlockObjectDataSource : BaseObjectDataSource<ActivityKey, IResourceBlockProperty>
    {
        public Dictionary<string, IResourceBlockProperty> GetPropLookup()
        {
            return m_propLookup;
        }

        public void Clear()
        {
            ClearData();
        }

        internal ResourceBlockObjectDataSource(BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties) : base(a_scenarioId, a_sdCache)
        {
            foreach (IObjectProperty objectProperty in a_properties)
            {
                if (objectProperty is IResourceBlockProperty property)
                {
                    m_propLookup.Add(property.PropertyName, property);
                }
            }
        }

        public override object GetValue(string a_property, ActivityKey a_key, ScenarioDetail a_sd)
        {
            if (m_propLookup.TryGetValue(a_property, out IResourceBlockProperty property))
            {
                Job job = a_sd.JobManager.GetById(a_key.JobId);
                ManufacturingOrder mo = job.ManufacturingOrders.GetById(a_key.MOId);
                InternalOperation operation = (InternalOperation)mo.OperationManager[a_key.OperationId];
                InternalActivity activity = operation.Activities.FindActivity(a_key.ActivityId);
                ResourceBlock block = activity.GetResourceBlock(a_key.ResBlockId);
                return property.GetValue(block, a_sd);
            }

            throw new DebugException("Data issue");
        }
    }

    private class ResourceRequirementObjectDataSource : BaseObjectDataSource<ActivityKey, IResourceRequirementProperty>
    {
        public Dictionary<string, IResourceRequirementProperty> GetPropLookup()
        {
            return m_propLookup;
        }

        public void Clear()
        {
            ClearData();
        }

        internal ResourceRequirementObjectDataSource(BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties) : base(a_scenarioId, a_sdCache)
        {
            foreach (IObjectProperty objectProperty in a_properties)
            {
                if (objectProperty is IResourceRequirementProperty property)
                {
                    m_propLookup.Add(property.PropertyName, property);
                }
            }
        }

        public override object GetValue(string a_property, ActivityKey a_key, ScenarioDetail a_sd)
        {
            if (m_propLookup.TryGetValue(a_property, out IResourceRequirementProperty property))
            {
                Job job = a_sd.JobManager.GetById(a_key.JobId);
                ManufacturingOrder mo = job.ManufacturingOrders.GetById(a_key.MOId);
                InternalOperation operation = (InternalOperation)mo.OperationManager[a_key.OperationId];
                InternalActivity activity = operation.Activities.FindActivity(a_key.ActivityId);
                ResourceRequirement rr = activity.Operation.ResourceRequirements.GetByIndex(a_key.RequirementId);
                return property.GetValue(rr);
            }

            throw new DebugException("Data issue");
        }
    }

    public void AddScenarioDetailReference(ScenarioDetailCache a_cache)
    {
        m_actDataSource.AddScenarioDetailReference(a_cache);
        m_resBlockDataSource.AddScenarioDetailReference(a_cache);
        m_rrDataSource.AddScenarioDetailReference(a_cache);
        m_batchDataSource.AddScenarioDetailReference(a_cache);
    }

    public void DeleteScenarioDetailReference(ScenarioDetailCache a_cache)
    {
        m_actDataSource.DeleteScenarioDetailReference(a_cache);
        m_resBlockDataSource.DeleteScenarioDetailReference(a_cache);
        m_rrDataSource.DeleteScenarioDetailReference(a_cache);
        m_batchDataSource.DeleteScenarioDetailReference(a_cache);
    }

    public IObjectProperty GetPropertyFromColumnKey(string a_key)
    {
        string propName = GridConstants.RemovePropPrefix(a_key);

        switch (GridConstants.GetPrefix(a_key))
        {
            case GridConstants.ActivityPropPrefix:
                if (m_actDataSource.GetPropLookup().TryGetValue(propName, out IActivityProperty actProperty))
                {
                    return actProperty;
                }

                break;
            case GridConstants.ResourceBlockPropPrefix:
                if (m_resBlockDataSource.GetPropLookup().TryGetValue(propName, out IResourceBlockProperty blockProperty))
                {
                    return blockProperty;
                }

                break;
            case GridConstants.ResourceRequirementPropPrefix:
                if (m_rrDataSource.GetPropLookup().TryGetValue(propName, out IResourceRequirementProperty rrProperty))
                {
                    return rrProperty;
                }

                break;
            case GridConstants.BatchPropPrefix:
                if (m_batchDataSource.GetPropLookup().TryGetValue(propName, out IBatchProperty batchProperty))
                {
                    return batchProperty;
                }

                break;
        }

        return null;
    }

    public void SignalSimulationStarted()
    {
        m_actDataSource.SignalSimulationStarted();
        m_rrDataSource.SignalSimulationStarted();
        m_resBlockDataSource.SignalSimulationStarted();
        m_batchDataSource.SignalSimulationStarted();
    }

    public void SignalSimulationCompleted()
    {
        m_actDataSource.SignalSimulationCompleted();
        m_rrDataSource.SignalSimulationCompleted();
        m_resBlockDataSource.SignalSimulationCompleted();
        m_batchDataSource.SignalSimulationCompleted();
    }

    public void SignalScenarioActivated()
    {
        m_actDataSource.SignalScenarioActivated();
        m_rrDataSource.SignalScenarioActivated();
        m_resBlockDataSource.SignalScenarioActivated();
        m_batchDataSource.SignalScenarioActivated();
    }

    //TODO: For all of the data sources, Job, Resource, PurchaseOrder, etc, call m_dataSource.SignalDataChanged instead of checking and clearing here. Check and clear in the member data source
    public void SignalDataChanged(IScenarioDataChanges a_dataChanges)
    {
        if (a_dataChanges.HasUnprocessedChanges)
        {
            if (a_dataChanges.JobChanges.HasChanges)
            {
                m_actDataSource.Clear();
            }

            if (a_dataChanges.PlantChanges.HasChanges || a_dataChanges.DepartmentChanges.HasChanges || a_dataChanges.MachineChanges.HasChanges || a_dataChanges.CapacityIntervalChanges.HasChanges || a_dataChanges.RecurringCapacityIntervalChanges.HasChanges)
            {
                m_actDataSource.Clear();
                m_rrDataSource.Clear();
                m_resBlockDataSource.Clear();
                m_batchDataSource.Clear();
            }
        }
    }
}