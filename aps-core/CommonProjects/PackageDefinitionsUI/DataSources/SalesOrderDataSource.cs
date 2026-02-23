using PT.APSCommon;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.Scheduler;
using PT.Scheduler.Demand;
using PT.SchedulerData.ObjectKeys;
using PT.SchedulerDefinitions;

namespace PT.PackageDefinitionsUI.DataSources;

internal class SalesOrderDataSource : ISalesOrderDataSource
{
    private readonly SalesOrderObjectDataSource m_salesOrderDataSource;
    private readonly SalesOrderLineObjectDataSource m_salesOrderLineDataSource;
    private readonly SalesOrderLineDistDataSource m_salesOrderLineDistDataSource;
    private readonly List<IDataSourceExtensionElement> m_extensionModules = new ();

    internal SalesOrderDataSource(IScenarioInfo a_scenarioInfo, BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_objectProperties, IPackageManagerUI a_packageManager, IMainForm a_mainForm)
    {
        m_salesOrderDataSource = new SalesOrderObjectDataSource(a_scenarioId, a_sdCache, a_objectProperties);
        m_salesOrderLineDataSource = new SalesOrderLineObjectDataSource(a_scenarioId, a_sdCache, a_objectProperties);
        m_salesOrderLineDistDataSource = new SalesOrderLineDistDataSource(a_scenarioId, a_sdCache, a_objectProperties);

        //Load dataSource extensions
        DataSourceUtilities.LoadDataSourceExtensions(a_packageManager, a_mainForm, a_scenarioInfo, ref m_extensionModules);
    }

    public LookUpValueStruct GetOverrideValue(string a_property, object propValue)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(GetPropertyFromColumnKey(a_property), propValue, m_extensionModules);
    }

    public LookUpValueStruct GetSalesOrderValue(string a_property, SalesOrderKey a_id)
    {
        return m_salesOrderDataSource.GetValue(a_id, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverrideSalesOrderValue(string a_property, SalesOrderKey a_id)
    {
        return GetOverrideValue(a_property, GetSalesOrderValue(a_property, a_id));
    }

    public List<LookUpValueStruct> GetSalesOrderValueBlock(string a_property, List<SalesOrderKey> a_ids)
    {
        return m_salesOrderDataSource.GetValueBlock(a_ids, a_property);
    }

    public LookUpValueStruct GetSalesOrderLineValue(string a_property, SalesOrderKey a_id)
    {
        return m_salesOrderLineDataSource.GetValue(a_id, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverrideSalesOrderLineValue(string a_property, SalesOrderKey a_id)
    {
        return GetOverrideValue(a_property, GetSalesOrderLineValue(a_property, a_id));
    }

    public List<LookUpValueStruct> GetSalesOrderLineValueBlock(string a_property, List<SalesOrderKey> a_ids)
    {
        return m_salesOrderLineDataSource.GetValueBlock(a_ids, a_property);
    }

    public LookUpValueStruct GetSalesOrderDistValue(string a_property, SalesOrderKey a_id)
    {
        return m_salesOrderLineDistDataSource.GetValue(a_id, GridConstants.RemovePropPrefix(a_property));
    }

    public LookUpValueStruct GetOverrideSalesOrderDistValue(string a_property, SalesOrderKey a_id)
    {
        return GetOverrideValue(a_property, GetSalesOrderDistValue(a_property, a_id));
    }

    public LookUpValueStruct GetValue(ISoLineProperty a_property, SalesOrderKey a_id)
    {
        return GetSalesOrderLineValue(a_property.PropertyName, a_id);
    }

    public LookUpValueStruct GetOverrideValue(ISoLineProperty a_property, SalesOrderKey a_id)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetValue(a_property, a_id), m_extensionModules);
    }

    public List<LookUpValueStruct> GetSalesOrderDistValueBlock(string a_property, List<SalesOrderKey> a_ids)
    {
        return m_salesOrderLineDistDataSource.GetValueBlock(a_ids, a_property);
    }

    public LookUpValueStruct GetValue(ISoLineDistProperty a_property, SalesOrderKey a_id)
    {
        return GetSalesOrderDistValue(a_property.PropertyName, a_id);
    }

    public LookUpValueStruct GetOverrideValue(ISoLineDistProperty a_property, SalesOrderKey a_id)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetValue(a_property, a_id), m_extensionModules);
    }

    public List<LookUpValueStruct> GetValueBlock(ISoProperty a_property, List<SalesOrderKey> a_ids)
    {
        return GetSalesOrderValueBlock(a_property.PropertyName, a_ids);
    }

    public List<LookUpValueStruct> GetOverrideValueBlock(ISoProperty a_property, List<SalesOrderKey> a_ids)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetValueBlock(a_property, a_ids), m_extensionModules);
    }

    public List<LookUpValueStruct> GetValueBlock(ISoLineProperty a_property, List<SalesOrderKey> a_ids)
    {
        return GetSalesOrderLineValueBlock(a_property.PropertyName, a_ids);
    }

    public List<LookUpValueStruct> GetOverrideValueBlock(ISoLineProperty a_property, List<SalesOrderKey> a_ids)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetValueBlock(a_property, a_ids), m_extensionModules);
    }

    public List<LookUpValueStruct> GetValueBlock(ISoLineDistProperty a_property, List<SalesOrderKey> a_ids)
    {
        return GetSalesOrderDistValueBlock(a_property.PropertyName, a_ids);
    }

    public List<LookUpValueStruct> GetOverrideValueBlock(ISoLineDistProperty a_property, List<SalesOrderKey> a_ids)
    {
        return (List<LookUpValueStruct>)DataSourceUtilities.GetOverrideValue(a_property, GetValueBlock(a_property, a_ids), m_extensionModules);
    }

    public LookUpValueStruct GetValue(ISoProperty a_property, SalesOrderKey a_id)
    {
        return GetSalesOrderValue(a_property.PropertyName, a_id);
    }

    public LookUpValueStruct GetOverrideValue(ISoProperty a_property, SalesOrderKey a_id)
    {
        return (LookUpValueStruct)DataSourceUtilities.GetOverrideValue(a_property, GetValue(a_property, a_id), m_extensionModules);
    }

    public void AddScenarioDetailReference(ScenarioDetailCache a_cache)
    {
        m_salesOrderDataSource.AddScenarioDetailReference(a_cache);
        m_salesOrderLineDataSource.AddScenarioDetailReference(a_cache);
        m_salesOrderLineDistDataSource.AddScenarioDetailReference(a_cache);
    }

    public void DeleteScenarioDetailReference(ScenarioDetailCache a_cache)
    {
        m_salesOrderDataSource.DeleteScenarioDetailReference(a_cache);
        m_salesOrderLineDataSource.DeleteScenarioDetailReference(a_cache);
        m_salesOrderLineDistDataSource.DeleteScenarioDetailReference(a_cache);
    }

    public IObjectProperty GetPropertyFromColumnKey(string a_key)
    {
        string propName = GridConstants.RemovePropPrefix(a_key);

        switch (GridConstants.GetPrefix(a_key))
        {
            case GridConstants.SoPropPrefix:
                if (m_salesOrderDataSource.GetPropLookup().TryGetValue(propName, out ISoProperty soProperty))
                {
                    return soProperty;
                }

                break;
            case GridConstants.SoLinePropPrefix:
                if (m_salesOrderLineDataSource.GetPropLookup().TryGetValue(propName, out ISoLineProperty lineProperty))
                {
                    return lineProperty;
                }

                break;
            case GridConstants.SoLineDistPropPrefix:
                if (m_salesOrderLineDistDataSource.GetPropLookup().TryGetValue(propName, out ISoLineDistProperty distProperty))
                {
                    return distProperty;
                }

                break;
        }

        return null;
    }

    public void SignalSimulationStarted()
    {
        m_salesOrderDataSource.SignalSimulationStarted();
        m_salesOrderLineDataSource.SignalSimulationStarted();
        m_salesOrderLineDistDataSource.SignalSimulationStarted();
    }

    public void SignalSimulationCompleted()
    {
        m_salesOrderDataSource.SignalSimulationCompleted();
        m_salesOrderLineDataSource.SignalSimulationCompleted();
        m_salesOrderLineDistDataSource.SignalSimulationCompleted();
    }

    public void SignalScenarioActivated()
    {
        m_salesOrderDataSource.SignalScenarioActivated();
        m_salesOrderLineDataSource.SignalScenarioActivated();
        m_salesOrderLineDistDataSource.SignalScenarioActivated();
    }

    public void SignalDataChanged(IScenarioDataChanges a_dataChanges)
    {
        if (a_dataChanges.HasUnprocessedChanges && a_dataChanges.SalesOrderChanges.HasChanges)
        {
            m_salesOrderDataSource.Clear();
            m_salesOrderLineDataSource.Clear();
            m_salesOrderLineDistDataSource.Clear();
        }
    }
}

internal class SalesOrderObjectDataSource : BaseObjectDataSource<SalesOrderKey, ISoProperty>
{
    public Dictionary<string, ISoProperty> GetPropLookup()
    {
        return m_propLookup;
    }

    public void Clear()
    {
        ClearData();
    }

    internal SalesOrderObjectDataSource(BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_objectProperties) : base(a_scenarioId, a_sdCache)
    {
        foreach (IObjectProperty objectProperty in a_objectProperties)
        {
            if (objectProperty is ISoProperty property)
            {
                m_propLookup.Add(property.PropertyName, property);
            }
        }
    }

    public override object GetValue(string a_property, SalesOrderKey a_id, ScenarioDetail a_sd)
    {
        if (m_propLookup.TryGetValue(a_property, out ISoProperty property))
        {
            SalesOrder salesOrder = a_sd.SalesOrderManager.GetById(a_id.SalesOrderId);
            return property.GetValue(salesOrder, a_sd);
        }

        return null;
    }
}

internal class SalesOrderLineObjectDataSource : BaseObjectDataSource<SalesOrderKey, ISoLineProperty>
{
    public Dictionary<string, ISoLineProperty> GetPropLookup()
    {
        return m_propLookup;
    }

    public void Clear()
    {
        ClearData();
    }

    internal SalesOrderLineObjectDataSource(BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_objectProperties) : base(a_scenarioId, a_sdCache)
    {
        foreach (IObjectProperty objectProperty in a_objectProperties)
        {
            if (objectProperty is ISoLineProperty property)
            {
                m_propLookup.Add(property.PropertyName, property);
            }
        }
    }

    public override object GetValue(string a_property, SalesOrderKey a_id, ScenarioDetail a_sd)
    {
        if (m_propLookup.TryGetValue(a_property, out ISoLineProperty property))
        {
            SalesOrder salesOrder = a_sd.SalesOrderManager.GetById(a_id.SalesOrderId);
            SalesOrderLine line = salesOrder.FindSalesOrderLine(a_id.SoLineId);
            return property.GetValue(line, a_sd);
        }

        return null;
    }
}

internal class SalesOrderLineDistDataSource : BaseObjectDataSource<SalesOrderKey, ISoLineDistProperty>
{
    public Dictionary<string, ISoLineDistProperty> GetPropLookup()
    {
        return m_propLookup;
    }

    public void Clear()
    {
        ClearData();
    }

    internal SalesOrderLineDistDataSource(BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_objectProperties) : base(a_scenarioId, a_sdCache)
    {
        foreach (IObjectProperty objectProperty in a_objectProperties)
        {
            if (objectProperty is ISoLineDistProperty property)
            {
                m_propLookup.Add(property.PropertyName, property);
            }
        }
    }

    public override object GetValue(string a_property, SalesOrderKey a_id, ScenarioDetail a_sd)
    {
        if (m_propLookup.TryGetValue(a_property, out ISoLineDistProperty property))
        {
            SalesOrder salesOrder = a_sd.SalesOrderManager.GetById(a_id.SalesOrderId);
            SalesOrderLine line = salesOrder.FindSalesOrderLine(a_id.SoLineId);
            SalesOrderLineDistribution distribution = line.FindDistribution(a_id.SoLineDistributionId);
            return property.GetValue(distribution, a_sd);
        }

        return null;
    }
}