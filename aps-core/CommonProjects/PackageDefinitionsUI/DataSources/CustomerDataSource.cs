using PT.APSCommon;
using PT.Common.Debugging;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.PackageDefinitionsUI.DataSources;

internal class CustomerDataSource : ICustomerDataSource
{
    private readonly CustomerObjectDataSource m_customerDataSource;
    private readonly List<IDataSourceExtensionElement> m_extensionModules = new ();

    internal CustomerDataSource(IScenarioInfo a_scenarioInfo, BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties, IPackageManagerUI a_packageManager, IMainForm a_mainForm)
    {
        m_customerDataSource = new CustomerObjectDataSource(a_scenarioId, a_sdCache, a_properties);

        //Load dataSource extensions
        DataSourceUtilities.LoadDataSourceExtensions(a_packageManager, a_mainForm, a_scenarioInfo, ref m_extensionModules);
    }

    public LookUpValueStruct GetOverrideValue(string a_property, object propValue)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(GetPropertyFromColumnKey(a_property), propValue, m_extensionModules);
    }

    public LookUpValueStruct GetValue(ICustomerProperty a_property, BaseId a_id)
    {
        return GetValue(a_property.PropertyName, a_id);
    }

    public LookUpValueStruct GetOverrideValue(ICustomerProperty a_property, BaseId a_id)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetValue(a_property, a_id), m_extensionModules);
    }

    public List<LookUpValueStruct> GetValueBlock(ICustomerProperty a_property, List<BaseId> a_ids)
    {
        return GetValueBlock(a_property.PropertyName, a_ids);
    }

    public List<LookUpValueStruct> GetOverrideValueBlock(ICustomerProperty a_property, List<BaseId> a_ids)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetValueBlock(a_property, a_ids), m_extensionModules);
    }

    public List<LookUpValueStruct> GetValueBlock(string a_property, List<BaseId> a_ids)
    {
        return m_customerDataSource.GetValueBlock(a_ids, a_property);
    }

    public LookUpValueStruct GetValue(string a_property, BaseId a_id)
    {
        return m_customerDataSource.GetValue(a_id, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverrideValue(string a_property, BaseId a_id)
    {
        return GetOverrideValue(a_property, GetValue(a_property, a_id));
    }

    private class CustomerObjectDataSource : BaseObjectDataSource<BaseId, ICustomerProperty>
    {
        public Dictionary<string, ICustomerProperty> GetPropLookup()
        {
            return m_propLookup;
        }

        public void Clear()
        {
            ClearData();
        }

        internal CustomerObjectDataSource(BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_objectProperties) : base(a_scenarioId, a_sdCache)
        {
            foreach (IObjectProperty objectProperty in a_objectProperties)
            {
                if (objectProperty is ICustomerProperty property)
                {
                    m_propLookup.Add(property.PropertyName, property);
                }
            }
        }

        public override object GetValue(string a_property, BaseId a_id, ScenarioDetail a_sd)
        {
            if (m_propLookup.TryGetValue(a_property, out ICustomerProperty property))
            {
                Customer customer = a_sd.CustomerManager.GetById(a_id);
                return property.GetValue(a_sd, customer);
            }

            throw new DebugException("Data issue");
        }
    }

    public void AddScenarioDetailReference(ScenarioDetailCache a_cache)
    {
        m_customerDataSource.AddScenarioDetailReference(a_cache);
    }

    public void DeleteScenarioDetailReference(ScenarioDetailCache a_cache)
    {
        m_customerDataSource.DeleteScenarioDetailReference(a_cache);
    }

    public IObjectProperty GetPropertyFromColumnKey(string a_key)
    {
        string propName = GridConstants.RemovePropPrefix(a_key);

        switch (GridConstants.GetPrefix(a_key))
        {
            case GridConstants.CustomerPropPrefix:
                if (m_customerDataSource.GetPropLookup().TryGetValue(propName, out ICustomerProperty customerProperty))
                {
                    return customerProperty;
                }

                break;
        }

        return null;
    }

    public void SignalSimulationStarted()
    {
        m_customerDataSource.SignalSimulationStarted();
    }

    public void SignalSimulationCompleted()
    {
        m_customerDataSource.SignalSimulationCompleted();
    }

    public void SignalScenarioActivated()
    {
        m_customerDataSource.SignalScenarioActivated();
    }

    public void SignalDataChanged(IScenarioDataChanges a_dataChanges)
    {
        if (a_dataChanges.HasUnprocessedChanges && a_dataChanges.CustomerChanges.HasChanges)
        {
            m_customerDataSource.Clear();
        }
    }
}