using PT.APSCommon;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.PackageDefinitionsUI.DataSources;

internal class UserDataSource : IUserDataSource
{
    private readonly UserObjectDataSource m_userDataSource;

    internal UserDataSource(IScenarioInfo a_scenarioInfo, BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_properties)
    {
        m_userDataSource = new UserObjectDataSource(a_scenarioId, a_sdCache, a_properties);
    }

    public List<LookUpValueStruct> GetValueBlock(IUserProperty a_property, List<BaseId> a_ids)
    {
        return GetValueBlock(a_property.PropertyName, a_ids);
    }

    public List<LookUpValueStruct> GetValueBlock(string a_property, List<BaseId> a_ids)
    {
        return m_userDataSource.GetValueBlock(a_ids, a_property);
    }

    public LookUpValueStruct GetValue(IUserProperty a_property, BaseId a_id)
    {
        return GetValue(a_property.PropertyName, a_id);
    }

    public LookUpValueStruct GetValue(string a_property, BaseId a_id)
    {
        return m_userDataSource.GetValue(a_id, a_property);
    }

    public void SignalSimulationStarted()
    {
        m_userDataSource.SignalSimulationStarted();
    }

    public void SignalSimulationCompleted()
    {
        m_userDataSource.SignalSimulationCompleted();
    }

    public void SignalScenarioActivated()
    {
        m_userDataSource.SignalScenarioActivated();
    }

    public void SignalDataChanged(IScenarioDataChanges a_dataChanges)
    {
        m_userDataSource.SignalDataChanged(a_dataChanges);
    }
}

internal class UserObjectDataSource : BaseObjectDataSource<BaseId, IUserProperty>
{
    internal UserObjectDataSource(BaseId a_scenarioId, ScenarioDetailCacheLock a_sdCache, List<IObjectProperty> a_objectProperties) : base(a_scenarioId, a_sdCache)
    {
        foreach (IObjectProperty objectProperty in a_objectProperties)
        {
            if (objectProperty is IUserProperty property)
            {
                m_propLookup.Add(property.PropertyName, property);
            }
        }
    }

    public override object GetValue(string a_property, BaseId a_id, ScenarioDetail a_sd)
    {
        if (m_propLookup.TryGetValue(a_property, out IUserProperty property))
        {
            using (SystemController.Sys.UsersLock.EnterRead(out UserManager um))
            {
                User user = um.GetById(a_id);
                if (user == null)
                {
                    return null;
                }

                if (m_userDetailPropsToInitialize.TryGetValue(a_property, out IUserDetailProperty sdProp))
                {
                    sdProp.Reload(um);
                    m_userDetailPropsToInitialize.Remove(a_property);
                }

                return property.GetValue(um, user);
            }
        }

        return null;
    }

    public override void SignalDataChanged(IScenarioDataChanges a_dataChanges)
    {
        if (a_dataChanges.UserChanges.HasChanges)
        {
            ClearData();
        }
    }
}