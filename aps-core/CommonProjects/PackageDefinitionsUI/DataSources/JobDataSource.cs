using PT.APSCommon;
using PT.Common.Debugging;
using PT.Common.Extensions;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.Scheduler;
using PT.SchedulerData.ObjectKeys;
using PT.SchedulerDefinitions;

namespace PT.PackageDefinitionsUI.DataSources;

internal class JobDataSource : IJobDataSource
{
    private readonly JobObjectDataSource m_jobDataSource;
    private readonly MoObjectDataSource m_moObjectDataSource;
    private readonly OperationObjectDataSource m_opDataSource;
    private readonly MaterialRequirementObjectDataSource m_materialRequirementObjectDataSource;
    private readonly List<IDataSourceExtensionElement> m_extensionModules = new ();

    internal JobDataSource(IScenarioInfo a_scenarioInfo, BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties, IPackageManagerUI a_packageManager, IMainForm a_mainForm)
    {
        m_jobDataSource = new JobObjectDataSource(a_scenarioId, a_sdCache, a_properties);
        m_moObjectDataSource = new MoObjectDataSource(a_scenarioId, a_sdCache, a_properties);
        m_opDataSource = new OperationObjectDataSource(a_scenarioId, a_sdCache, a_properties);
        m_materialRequirementObjectDataSource = new MaterialRequirementObjectDataSource(a_scenarioId, a_sdCache, a_properties);

        //Load dataSource extensions
        DataSourceUtilities.LoadDataSourceExtensions(a_packageManager, a_mainForm, a_scenarioInfo, ref m_extensionModules);
    }

    public LookUpValueStruct GetOverrideValue(string a_property, object propValue)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(GetPropertyFromColumnKey(a_property), propValue, m_extensionModules);
    }

    public LookUpValueStruct GetJobValue(string a_property, JobKey a_objectId)
    {
        return m_jobDataSource.GetValue(a_objectId, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverrideJobValue(string a_property, JobKey a_objectId)
    {
        return GetOverrideValue(a_property, GetJobValue(a_property, a_objectId));
    }

    public LookUpValueStruct GetValue(IJobBaseProperty a_property, JobKey a_objectId)
    {
        return GetJobValue(a_property.PropertyName, a_objectId);
    }

    public LookUpValueStruct GetOverrideValue(IJobBaseProperty a_property, JobKey a_objectId)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetValue(a_property, a_objectId), m_extensionModules);
    }

    public LookUpValueStruct GetValue(IMoProperty a_property, JobKey a_objectId)
    {
        return m_moObjectDataSource.GetValue(a_objectId, a_property.PropertyName);
    }

    public LookUpValueStruct GetOverrideValue(IMoProperty a_property, JobKey a_objectId)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetValue(a_property, a_objectId), m_extensionModules);
    }

    public LookUpValueStruct GetValue(IOperationProperty a_property, JobKey a_objectId)
    {
        return m_opDataSource.GetValue(a_objectId, a_property.PropertyName);
    }

    public LookUpValueStruct GetOverrideValue(IOperationProperty a_property, JobKey a_objectId)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetValue(a_property, a_objectId), m_extensionModules);
    }

    public LookUpValueStruct GetValue(IMaterialsProperty a_property, JobKey a_objectId)
    {
        return m_materialRequirementObjectDataSource.GetValue(a_objectId, a_property.PropertyName);
    }

    public LookUpValueStruct GetOverrideValue(IMaterialsProperty a_property, JobKey a_objectId)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetValue(a_property, a_objectId), m_extensionModules);
    }

    public List<LookUpValueStruct> GetJobValueBlock(string a_property, List<JobKey> a_id)
    {
        return m_jobDataSource.GetValueBlock(a_id, a_property);
    }

    public List<LookUpValueStruct> GetValueBlock(IJobBaseProperty a_property, List<JobKey> a_id)
    {
        return GetJobValueBlock(a_property.PropertyName, a_id);
    }

    public List<LookUpValueStruct> GetOverrideValueBlock(IJobBaseProperty a_property, List<JobKey> a_id)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetValueBlock(a_property, a_id), m_extensionModules);
    }

    public List<LookUpValueStruct> GetValueBlock(IMoProperty a_property, List<JobKey> a_ids)
    {
        return m_moObjectDataSource.GetValueBlock(a_ids, a_property.PropertyName);
    }

    public List<LookUpValueStruct> GetOverrideValueBlock(IMoProperty a_property, List<JobKey> a_ids)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetValueBlock(a_property, a_ids), m_extensionModules);
    }

    public List<LookUpValueStruct> GetValueBlock(IOperationProperty a_property, List<JobKey> a_ids)
    {
        return m_opDataSource.GetValueBlock(a_ids, a_property.PropertyName);
    }

    public List<LookUpValueStruct> GetOverrideValueBlock(IOperationProperty a_property, List<JobKey> a_ids)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetValueBlock(a_property, a_ids), m_extensionModules);
    }

    public List<LookUpValueStruct> GetValueBlock(IMaterialsProperty a_property, List<JobKey> a_ids)
    {
        return m_materialRequirementObjectDataSource.GetValueBlock(a_ids, a_property.PropertyName);
    }

    public List<LookUpValueStruct> GetOverrideValueBlock(IMaterialsProperty a_property, List<JobKey> a_ids)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetValueBlock(a_property, a_ids), m_extensionModules);
    }

    public LookUpValueStruct GetOperationValue(string a_property, JobKey a_objectId)
    {
        return m_opDataSource.GetValue(a_objectId, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverrideOperationValue(string a_property, JobKey a_objectId)
    {
        return GetOverrideValue(a_property, GetOperationValue(a_property, a_objectId));
    }

    public LookUpValueStruct GetMoValue(string a_property, JobKey a_objectId)
    {
        return m_moObjectDataSource.GetValue(a_objectId, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverrideMoValue(string a_property, JobKey a_objectId)
    {
        return GetOverrideValue(a_property, GetMoValue(a_property, a_objectId));
    }

    public object GetMaterialValue(string a_property, JobKey a_objectId)
    {
        return m_materialRequirementObjectDataSource.GetValue(a_objectId, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverrideMaterialValue(string a_property, JobKey a_objectId)
    {
        return GetOverrideValue(a_property, GetMaterialValue(a_property, a_objectId));
    }

    private class JobObjectDataSource : BaseObjectDataSource<JobKey, IJobBaseProperty>
    {
        public Dictionary<string, IJobBaseProperty> GetPropLookup()
        {
            return m_propLookup;
        }

        public void Clear()
        {
            ClearData();
        }

        internal JobObjectDataSource(BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties) : base(a_scenarioId, a_sdCache)
        {
            foreach (IObjectProperty objectProperty in a_properties)
            {
                if (objectProperty is IJobBaseProperty jobProperty)
                {
                    m_propLookup.Add(jobProperty.PropertyName, jobProperty);
                }
            }
        }

        public override object GetValue(string a_property, JobKey a_id, ScenarioDetail a_sd)
        {
            if (m_propLookup.TryGetValue(a_property, out IJobBaseProperty jobProp))
            {
                Job job = a_sd.JobManager.GetById(a_id.JobId);
                return job == null ? null : jobProp.GetValue(job, a_sd);
            }

            // TODO Do something when we cannot find data
            throw new DebugException("Data issue");
        }
    }

    private class MoObjectDataSource : BaseObjectDataSource<JobKey, IMoProperty>
    {
        public Dictionary<string, IMoProperty> GetPropLookup()
        {
            return m_propLookup;
        }

        public void Clear()
        {
            ClearData();
        }

        internal MoObjectDataSource(BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties) : base(a_scenarioId, a_sdCache)
        {
            foreach (IObjectProperty objectProperty in a_properties)
            {
                if (objectProperty is IMoProperty property)
                {
                    m_propLookup.Add(property.PropertyName, property);
                }
            }
        }

        public override object GetValue(string a_property, JobKey a_id, ScenarioDetail a_sd)
        {
            if (m_propLookup.TryGetValue(a_property, out IMoProperty property))
            {
                ManufacturingOrder mo = a_sd.JobManager.GetById(a_id.JobId)?.ManufacturingOrders.GetById(a_id.MOId);
                return mo == null ? null : property.GetValue(mo, a_sd);
            }

            throw new DebugException("Data issue");
        }
    }

    private class OperationObjectDataSource : BaseObjectDataSource<JobKey, IOperationProperty>
    {
        public Dictionary<string, IOperationProperty> GetPropLookup()
        {
            return m_propLookup;
        }

        public void Clear()
        {
            ClearData();
        }

        internal OperationObjectDataSource(BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties) : base(a_scenarioId, a_sdCache)
        {
            foreach (IObjectProperty objectProperty in a_properties)
            {
                if (objectProperty is IOperationProperty property)
                {
                    m_propLookup.AddIfNew(property.PropertyName, property);
                }
            }
        }

        public override object GetValue(string a_property, JobKey a_id, ScenarioDetail a_sd)
        {
            if (m_propLookup.TryGetValue(a_property, out IOperationProperty property))
            {
                InternalOperation op = (InternalOperation)a_sd.JobManager.GetById(a_id.JobId)?.ManufacturingOrders.GetById(a_id.MOId)?.OperationManager[a_id.OperationId];
                return op == null ? null : property.GetValue(op, a_sd);
            }

            throw new DebugException("Data issue");
        }
    }

    private class MaterialRequirementObjectDataSource : BaseObjectDataSource<JobKey, IMaterialsProperty>
    {
        public Dictionary<string, IMaterialsProperty> GetPropLookup()
        {
            return m_propLookup;
        }

        public void Clear()
        {
            ClearData();
        }

        internal MaterialRequirementObjectDataSource(BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties) : base(a_scenarioId, a_sdCache)
        {
            foreach (IObjectProperty objectProperty in a_properties)
            {
                if (objectProperty is IMaterialsProperty property)
                {
                    m_propLookup.Add(property.PropertyName, property);
                }
            }
        }

        public override object GetValue(string a_property, JobKey a_id, ScenarioDetail a_sd)
        {
            if (m_propLookup.TryGetValue(a_property, out IMaterialsProperty property))
            {
                Job job = a_sd.JobManager.GetById(a_id.JobId);
                ManufacturingOrder mo = job?.ManufacturingOrders.GetById(a_id.MOId);
                InternalOperation op = (InternalOperation)mo?.OperationManager[a_id.OperationId];
                InternalActivity act = op?.Activities.FindActivity(a_id.ActivityId);
                if (act == null)
                {
                    return null;
                }

                MaterialRequirement mr = op.MaterialRequirements.FindByBaseId(a_id.MaterialRequirementId);
                if (mr == null)
                {
                    return null;
                }

                return property.GetValue(mr, op, act, a_sd);
            }

            throw new DebugException("Data issue");
        }
    }

    public void AddScenarioDetailReference(ScenarioDetailCache a_cache)
    {
        m_jobDataSource.AddScenarioDetailReference(a_cache);
        m_moObjectDataSource.AddScenarioDetailReference(a_cache);
        m_opDataSource.AddScenarioDetailReference(a_cache);
        m_materialRequirementObjectDataSource.AddScenarioDetailReference(a_cache);
    }

    public void DeleteScenarioDetailReference(ScenarioDetailCache a_cache)
    {
        m_jobDataSource.DeleteScenarioDetailReference(a_cache);
        m_moObjectDataSource.DeleteScenarioDetailReference(a_cache);
        m_opDataSource.DeleteScenarioDetailReference(a_cache);
        m_materialRequirementObjectDataSource.DeleteScenarioDetailReference(a_cache);
    }

    public IObjectProperty GetPropertyFromColumnKey(string a_key)
    {
        string propName = GridConstants.RemovePropPrefix(a_key);

        switch (GridConstants.GetPrefix(a_key))
        {
            case GridConstants.JobPropPrefix:
                if (m_jobDataSource.GetPropLookup().TryGetValue(propName, out IJobBaseProperty jobProperty))
                {
                    return jobProperty;
                }

                break;
            case GridConstants.MoPropPrefix:
                if (m_moObjectDataSource.GetPropLookup().TryGetValue(propName, out IMoProperty moProperty))
                {
                    return moProperty;
                }

                break;
            case GridConstants.OperationPropPrefix:
                if (m_opDataSource.GetPropLookup().TryGetValue(propName, out IOperationProperty opProperty))
                {
                    return opProperty;
                }

                break;
            case GridConstants.MaterialsPropPrefix:
                if (m_materialRequirementObjectDataSource.GetPropLookup().TryGetValue(propName, out IMaterialsProperty materialProperty))
                {
                    return materialProperty;
                }

                break;
        }

        return null;
    }

    public void SignalSimulationStarted()
    {
        m_jobDataSource.SignalSimulationStarted();
        m_materialRequirementObjectDataSource.SignalSimulationStarted();
        m_moObjectDataSource.SignalSimulationStarted();
        m_opDataSource.SignalSimulationStarted();
    }

    public void SignalSimulationCompleted()
    {
        m_jobDataSource.SignalSimulationCompleted();
        m_materialRequirementObjectDataSource.SignalSimulationCompleted();
        m_moObjectDataSource.SignalSimulationCompleted();
        m_opDataSource.SignalSimulationCompleted();
    }

    public void SignalScenarioActivated()
    {
        m_jobDataSource.SignalScenarioActivated();
        m_materialRequirementObjectDataSource.SignalScenarioActivated();
        m_moObjectDataSource.SignalScenarioActivated();
        m_opDataSource.SignalScenarioActivated();
    }

    public void SignalDataChanged(IScenarioDataChanges a_dataChanges)
    {
        if (a_dataChanges.HasUnprocessedChanges && (a_dataChanges.JobChanges.HasChanges || a_dataChanges.TemplateChanges.HasChanges))
        {
            m_jobDataSource.Clear();
            m_moObjectDataSource.Clear();
            m_opDataSource.Clear();
            m_materialRequirementObjectDataSource.Clear();
        }
    }
}