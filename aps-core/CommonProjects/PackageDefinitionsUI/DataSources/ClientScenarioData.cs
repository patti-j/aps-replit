using PT.APSCommon;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.SchedulerDefinitions;

namespace PT.PackageDefinitionsUI.DataSources;

public class ClientScenarioData : IClientScenarioData
{
    public ClientScenarioData(IPackageManagerUI a_pm, IMainForm a_mainForm, IScenarioInfo a_scenarioInfo, ScenarioDetailCacheLock a_sdCache, BaseId a_scenarioId)
    {
        m_dataSources = new List<IScenarioDataChangeSignalInfo>(8);

        List<IObjectProperty> objectProperties = a_pm.GetObjectProperties();

        ScenarioId = a_scenarioId;
        JobData = new JobDataSource(a_scenarioInfo, a_scenarioId, a_sdCache, objectProperties, a_pm, a_mainForm);
        ResourceData = new ResourceDataSource(a_scenarioInfo, a_scenarioId, a_sdCache, objectProperties, a_pm, a_mainForm);
        PurchaseOrderData = new PurchaseOrderDataSource(a_scenarioInfo, a_scenarioId, a_sdCache, objectProperties, a_pm, a_mainForm);
        SalesOrderData = new SalesOrderDataSource(a_scenarioInfo, a_scenarioId, a_sdCache, objectProperties, a_pm, a_mainForm);
        UserData = new UserDataSource(a_scenarioInfo, a_scenarioId, a_sdCache, objectProperties);
        InventoryData = new InventoryDataSource(a_scenarioInfo, a_scenarioId, a_sdCache, objectProperties, a_pm, a_mainForm);
        ForecastData = new ForecastDataSource(a_scenarioInfo, a_scenarioId, a_sdCache, objectProperties);
        ActivityData = new ActivityDataSource(a_scenarioInfo, a_scenarioId, a_sdCache, objectProperties, a_pm, a_mainForm);
        CustomerData = new CustomerDataSource(a_scenarioInfo, a_scenarioId, a_sdCache, objectProperties, a_pm, a_mainForm);
        StorageAreaData = new StorageAreaDataSource(a_scenarioInfo, a_scenarioId, a_sdCache, objectProperties, a_pm, a_mainForm);

        m_dataSources.Add(JobData);
        m_dataSources.Add(ResourceData);
        m_dataSources.Add(PurchaseOrderData);
        m_dataSources.Add(SalesOrderData);
        m_dataSources.Add(UserData);
        m_dataSources.Add(InventoryData);
        m_dataSources.Add(ForecastData);
        m_dataSources.Add(ActivityData);
        m_dataSources.Add(CustomerData);
        m_dataSources.Add(StorageAreaData);
    }

    private readonly List<IScenarioDataChangeSignalInfo> m_dataSources;

    public BaseId ScenarioId { get; }
    public IActivityDataSource ActivityData { get; }
    public IJobDataSource JobData { get; }
    public IResourceDataSource ResourceData { get; }
    public IPurchaseOrderDataSource PurchaseOrderData { get; }
    public IInventoryDataSource InventoryData { get; }
    public IStorageAreaDataSource StorageAreaData { get; }
    public ISalesOrderDataSource SalesOrderData { get; }
    public IUserDataSource UserData { get; }
    public IForecastDataSource ForecastData { get; }
    public ICustomerDataSource CustomerData { get; }

    public void SignalSimulationStarted()
    {
        foreach (IScenarioDataChangeSignalInfo dataSource in m_dataSources)
        {
            dataSource.SignalSimulationStarted();
        }
    }

    public void SignalSimulationCompleted()
    {
        foreach (IScenarioDataChangeSignalInfo dataSource in m_dataSources)
        {
            dataSource.SignalSimulationCompleted();
        }
    }

    public void SignalScenarioActivated()
    {
        foreach (IScenarioDataChangeSignalInfo dataSource in m_dataSources)
        {
            dataSource.SignalScenarioActivated();
        }
    }

    public void SignalDataChanges(IScenarioDataChanges a_dataChanges)
    {
        foreach (IScenarioDataChangeSignalInfo dataSource in m_dataSources)
        {
            dataSource.SignalDataChanged(a_dataChanges);
        }
    }
}