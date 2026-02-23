using PT.APSCommon;

namespace PT.PackageDefinitionsUI.DataSources;

public interface IClientScenarioData
{
    BaseId ScenarioId { get; }
    IActivityDataSource ActivityData { get; }
    IJobDataSource JobData { get; }
    IResourceDataSource ResourceData { get; }
    IPurchaseOrderDataSource PurchaseOrderData { get; }
    IInventoryDataSource InventoryData { get; }
    IStorageAreaDataSource StorageAreaData{ get; }
    ISalesOrderDataSource SalesOrderData { get; }
    IUserDataSource UserData { get; }
    IForecastDataSource ForecastData { get; }
    ICustomerDataSource CustomerData { get; }
}