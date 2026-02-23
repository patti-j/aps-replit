using System.Timers;

using PT.APSCommon;
using PT.PackageDefinitionsUI.PackageInterfaces;
using PT.Scheduler;
using PT.SchedulerData;
using PT.SchedulerData.ObjectKeys;
using PT.SchedulerDefinitions;

using ActivityKey = PT.SchedulerData.ObjectKeys.ActivityKey;
using ForecastKey = PT.SchedulerData.ObjectKeys.ForecastKey;
using InventoryKey = PT.SchedulerData.ObjectKeys.InventoryKey;
using ResourceKey = PT.SchedulerData.ObjectKeys.ResourceKey;
using Timer = System.Timers.Timer;

namespace PT.PackageDefinitionsUI;

public interface IScenarioDataChangeSignalInfo
{
    void SignalSimulationStarted();
    void SignalSimulationCompleted();
    void SignalScenarioActivated();
    void SignalDataChanged(IScenarioDataChanges a_dataChanges);
}

public interface IScenarioDetailDataSource : IScenarioDataChangeSignalInfo
{
    void AddScenarioDetailReference(ScenarioDetailCache a_cache);
    void DeleteScenarioDetailReference(ScenarioDetailCache a_cache);
    IObjectProperty GetPropertyFromColumnKey(string a_key);
}

public interface IBaseObjectDataSource<TKeyType> : IScenarioDetailDataSource
{
    List<LookUpValueStruct> GetValueBlock(List<TKeyType> a_objectIds, string a_property);

    LookUpValueStruct GetValue(TKeyType a_object, string a_property, ScenarioDetail a_sd = null);
}

public class ScenarioDetailCacheLock : IDisposable
{
    private readonly ScenarioDataLock m_dataLock;
    private ScenarioDetail m_sd;
    private static readonly object s_cacheLock = new ();
    private int m_useCounter;
    private DateTime m_cachedDateTime;
    private bool m_locking;

    private readonly Timer m_timer = new (250);

    public ScenarioDetailCacheLock(ScenarioDataLock a_dataLock)
    {
        m_dataLock = a_dataLock;
        m_timer.Elapsed += TimerOnElapsed;
    }

    public void Dispose()
    {
        m_sd = null;
        m_timer.Elapsed -= TimerOnElapsed;
        m_timer.Dispose();
    }

    public void InitCache()
    {
        lock (s_cacheLock)
        {
            if (m_locking)
            {
                return;
            }

            m_locking = true;
        }

        Task.Factory.StartNew(() =>
            {
                m_dataLock.CreateNewLock().RunLockCodeBackground(AddScenarioDetailCache).Wait();

                lock (s_cacheLock)
                {
                    m_locking = false;
                }
            },
            TaskCreationOptions.LongRunning);
    }

    public void AddScenarioDetailCache(ScenarioManager a_sm, Scenario a_s, ScenarioDetail a_sd, params object[] a_params)
    {
        lock (s_cacheLock)
        {
            m_cachedDateTime = DateTime.UtcNow;
            m_sd = a_sd;
            m_timer.Start();
        }

        while (m_sd != null)
        {
            Thread.Sleep(50);
        }
    }

    public ScenarioDetailCacheValue GetScenarioDetailCache()
    {
        lock (s_cacheLock)
        {
            if (m_sd != null)
            {
                m_useCounter++;
                return new ScenarioDetailCacheValue { CacheFound = true, ScenarioDetail = m_sd };
            }

            return NotFoundResult;
        }
    }

    public void ReturnScenarioDetailCache()
    {
        lock (s_cacheLock)
        {
            m_useCounter--;

            if (m_useCounter == 0 && DateTime.UtcNow - m_cachedDateTime > TimeSpan.FromMilliseconds(250))
            {
                m_sd = null;
            }
        }
    }

    private void TimerOnElapsed(object a_sender, ElapsedEventArgs a_e)
    {
        lock (s_cacheLock)
        {
            if (m_useCounter == 0)
            {
                m_sd = null;
                m_timer.Stop();
            }
        }
    }

    public ScenarioDetailCacheValue NotFoundResult => new ();

    public struct ScenarioDetailCacheValue
    {
        public bool CacheFound;
        public ScenarioDetail ScenarioDetail;
    }

    public ScenarioDetail ScenarioDetail => m_sd;
}

public class ScenarioDetailCache : IDisposable
{
    private readonly ScenarioDetail m_sd;
    private readonly IScenarioDetailDataSource m_dataSource;

    public ScenarioDetailCache(ScenarioDetail a_sd, IScenarioDetailDataSource a_dataSource)
    {
        m_sd = a_sd;
        m_dataSource = a_dataSource;
        m_dataSource.AddScenarioDetailReference(this);
    }

    public void Dispose()
    {
        m_dataSource.DeleteScenarioDetailReference(this);
    }

    public ScenarioDetail ScenarioDetail => m_sd;
}

public interface IJobExtensionDataSource : IScenarioDetailDataSource
{
    LookUpValueStruct GetOverrideJobValue(string a_property, JobKey a_id);
    LookUpValueStruct GetOverrideValue(IJobBaseProperty a_property, JobKey a_id);
    LookUpValueStruct GetOverrideValue(IMoProperty a_property, JobKey a_id);
    LookUpValueStruct GetOverrideValue(IOperationProperty a_property, JobKey a_id);
    LookUpValueStruct GetOverrideValue(IMaterialsProperty a_property, JobKey a_id);
    List<LookUpValueStruct> GetOverrideValueBlock(IJobBaseProperty a_property, List<JobKey> a_ids);
    List<LookUpValueStruct> GetOverrideValueBlock(IMoProperty a_property, List<JobKey> a_ids);
    List<LookUpValueStruct> GetOverrideValueBlock(IOperationProperty a_property, List<JobKey> a_ids);
    List<LookUpValueStruct> GetOverrideValueBlock(IMaterialsProperty a_property, List<JobKey> a_ids);
    LookUpValueStruct GetOverrideOperationValue(string a_property, JobKey a_id);
    LookUpValueStruct GetOverrideMoValue(string a_property, JobKey a_id);
    LookUpValueStruct GetOverrideMaterialValue(string a_property, JobKey a_id);
    LookUpValueStruct GetOverrideValue(string a_property, object propValue);
}

public interface IJobDataSource : IJobExtensionDataSource
{
    LookUpValueStruct GetJobValue(string a_property, JobKey a_id);
    LookUpValueStruct GetValue(IJobBaseProperty a_property, JobKey a_id);
    LookUpValueStruct GetValue(IMoProperty a_property, JobKey a_id);
    LookUpValueStruct GetValue(IOperationProperty a_property, JobKey a_id);
    LookUpValueStruct GetValue(IMaterialsProperty a_property, JobKey a_id);
    List<LookUpValueStruct> GetJobValueBlock(string a_property, List<JobKey> a_ids);
    List<LookUpValueStruct> GetValueBlock(IJobBaseProperty a_property, List<JobKey> a_ids);
    List<LookUpValueStruct> GetValueBlock(IMoProperty a_property, List<JobKey> a_ids);
    List<LookUpValueStruct> GetValueBlock(IOperationProperty a_property, List<JobKey> a_ids);
    List<LookUpValueStruct> GetValueBlock(IMaterialsProperty a_property, List<JobKey> a_ids);
    LookUpValueStruct GetOperationValue(string a_property, JobKey a_id);
    LookUpValueStruct GetMoValue(string a_property, JobKey a_id);
}

public interface IResourceExtensionDataSource : IScenarioDetailDataSource
{
    LookUpValueStruct GetOverridePlantValue(string a_property, ResourceKey a_id);
    List<LookUpValueStruct> GetOverrideValueBlock(IPlantProperty a_property, List<ResourceKey> a_ids);
    LookUpValueStruct GetOverrideValue(IPlantProperty a_property, ResourceKey a_id);

    LookUpValueStruct GetOverrideDepartmentValue(string a_property, ResourceKey a_id);
    List<LookUpValueStruct> GetOverrideValueBlock(IDepartmentProperty a_property, List<ResourceKey> a_ids);
    LookUpValueStruct GetOverrideValue(IDepartmentProperty a_property, ResourceKey a_id);
    LookUpValueStruct GetOverrideResourceValue(string a_property, ResourceKey a_id);
    List<LookUpValueStruct> GetOverrideValueBlock(IResourceProperty a_property, List<ResourceKey> a_ids);
    LookUpValueStruct GetOverrideValue(IResourceProperty a_property, ResourceKey a_id);
    LookUpValueStruct GetOverrideValue(string a_property, object propValue);
}

public interface IResourceDataSource : IResourceExtensionDataSource
{
    LookUpValueStruct GetPlantValue(string a_property, ResourceKey a_id);
    List<LookUpValueStruct> GetPlantValueBlock(string a_property, List<ResourceKey> a_ids);
    List<LookUpValueStruct> GetValueBlock(IPlantProperty a_property, List<ResourceKey> a_ids);
    LookUpValueStruct GetValue(IPlantProperty a_property, ResourceKey a_id);
    LookUpValueStruct GetDepartmentValue(string a_property, ResourceKey a_id);
    List<LookUpValueStruct> GetDepartmentValueBlock(string a_property, List<ResourceKey> a_ids);
    List<LookUpValueStruct> GetValueBlock(IDepartmentProperty a_property, List<ResourceKey> a_ids);
    LookUpValueStruct GetValue(IDepartmentProperty a_property, ResourceKey a_id);
    LookUpValueStruct GetResourceValue(string a_property, ResourceKey a_id);
    List<LookUpValueStruct> GetResourceValueBlock(string a_property, List<ResourceKey> a_ids);
    List<LookUpValueStruct> GetValueBlock(IResourceProperty a_property, List<ResourceKey> a_ids);
    LookUpValueStruct GetValue(IResourceProperty a_property, ResourceKey a_id);
}

public interface ISalesOrderExtensionDataSource : IScenarioDetailDataSource
{
    LookUpValueStruct GetOverrideSalesOrderValue(string a_property, SalesOrderKey a_id);
    LookUpValueStruct GetOverrideSalesOrderLineValue(string a_property, SalesOrderKey a_id);
    LookUpValueStruct GetOverrideSalesOrderDistValue(string a_property, SalesOrderKey a_id);
    LookUpValueStruct GetOverrideValue(ISoProperty a_property, SalesOrderKey a_id);
    LookUpValueStruct GetOverrideValue(ISoLineProperty a_property, SalesOrderKey a_id);
    LookUpValueStruct GetOverrideValue(ISoLineDistProperty a_property, SalesOrderKey a_id);
    List<LookUpValueStruct> GetOverrideValueBlock(ISoProperty a_property, List<SalesOrderKey> a_ids);
    List<LookUpValueStruct> GetOverrideValueBlock(ISoLineProperty a_property, List<SalesOrderKey> a_ids);
    List<LookUpValueStruct> GetOverrideValueBlock(ISoLineDistProperty a_property, List<SalesOrderKey> a_ids);
    LookUpValueStruct GetOverrideValue(string a_property, object propValue);
}

public interface ISalesOrderDataSource : ISalesOrderExtensionDataSource
{
    LookUpValueStruct GetSalesOrderValue(string a_property, SalesOrderKey a_id);
    List<LookUpValueStruct> GetSalesOrderValueBlock(string a_property, List<SalesOrderKey> a_ids);
    LookUpValueStruct GetSalesOrderLineValue(string a_property, SalesOrderKey a_id);
    List<LookUpValueStruct> GetSalesOrderLineValueBlock(string a_property, List<SalesOrderKey> a_ids);
    LookUpValueStruct GetSalesOrderDistValue(string a_property, SalesOrderKey a_id);
    LookUpValueStruct GetValue(ISoLineProperty a_property, SalesOrderKey a_id);
    List<LookUpValueStruct> GetSalesOrderDistValueBlock(string a_property, List<SalesOrderKey> a_ids);
    LookUpValueStruct GetValue(ISoLineDistProperty a_property, SalesOrderKey a_id);
    List<LookUpValueStruct> GetValueBlock(ISoProperty a_property, List<SalesOrderKey> a_ids);
    List<LookUpValueStruct> GetValueBlock(ISoLineProperty a_property, List<SalesOrderKey> a_ids);
    List<LookUpValueStruct> GetValueBlock(ISoLineDistProperty a_property, List<SalesOrderKey> a_ids);
    LookUpValueStruct GetValue(ISoProperty a_property, SalesOrderKey a_id);
}

public interface IActivityExtensionDataSource : IScenarioDetailDataSource
{
    LookUpValueStruct GetOverrideActivityValue(string a_property, ActivityKey a_id);
    LookUpValueStruct GetOverrideResourceBlockValue(string a_property, ActivityKey a_id);
    LookUpValueStruct GetOverrideBatchValue(string a_property, ActivityKey a_id);
    LookUpValueStruct GetOverrideResourceRequirementValue(string a_property, ActivityKey a_id);
    LookUpValueStruct GetOverrideValue(IActivityProperty a_property, ActivityKey a_id);
    LookUpValueStruct GetOverrideValue(IResourceBlockProperty a_property, ActivityKey a_id);
    LookUpValueStruct GetOverrideValue(IBatchProperty a_property, ActivityKey a_id);
    LookUpValueStruct GetOverrideValue(IResourceRequirementProperty a_property, ActivityKey a_id);
    List<LookUpValueStruct> GetOverrideValueBlock(IActivityProperty a_property, List<ActivityKey> a_ids);
    List<LookUpValueStruct> GetOverrideValueBlock(IResourceBlockProperty a_property, List<ActivityKey> a_ids);
    List<LookUpValueStruct> GetOverrideValueBlock(IBatchProperty a_property, List<ActivityKey> a_ids);
    List<LookUpValueStruct> GetOverrideValueBlock(IResourceRequirementProperty a_property, List<ActivityKey> a_ids);
    LookUpValueStruct GetOverrideValue(string a_property, object propValue);
}

public interface IActivityDataSource : IActivityExtensionDataSource
{
    LookUpValueStruct GetActivityValue(string a_property, ActivityKey a_id);
    LookUpValueStruct GetResourceBlockValue(string a_property, ActivityKey a_id);
    LookUpValueStruct GetBatchValue(string a_property, ActivityKey a_id);
    LookUpValueStruct GetResourceRequirementValue(string a_property, ActivityKey a_id);
    LookUpValueStruct GetValue(IActivityProperty a_property, ActivityKey a_id);
    LookUpValueStruct GetValue(IResourceBlockProperty a_property, ActivityKey a_id);
    LookUpValueStruct GetValue(IBatchProperty a_property, ActivityKey a_id);
    LookUpValueStruct GetValue(IResourceRequirementProperty a_property, ActivityKey a_id);
    List<LookUpValueStruct> GetValueBlock(IActivityProperty a_property, List<ActivityKey> a_ids);
    List<LookUpValueStruct> GetValueBlock(IResourceBlockProperty a_property, List<ActivityKey> a_ids);
    List<LookUpValueStruct> GetValueBlock(IBatchProperty a_property, List<ActivityKey> a_ids);
    List<LookUpValueStruct> GetValueBlock(IResourceRequirementProperty a_property, List<ActivityKey> a_ids);
}

public interface IPurchaseOrderExtensionDataSource : IScenarioDetailDataSource
{
    LookUpValueStruct GetOverrideValue(IPoProperty a_property, BaseId a_baseId);
    List<LookUpValueStruct> GetOverrideValueBlock(IPoProperty a_property, List<BaseId> a_ids);
    LookUpValueStruct GetOverrideValue(string a_property, BaseId a_id);
    LookUpValueStruct GetOverrideValue(string a_property, object propValue);
}

public interface IPurchaseOrderDataSource : IPurchaseOrderExtensionDataSource
{
    LookUpValueStruct GetValue(IPoProperty a_property, BaseId a_baseId);
    List<LookUpValueStruct> GetValueBlock(IPoProperty a_property, List<BaseId> a_ids);
    List<LookUpValueStruct> GetValueBlock(string a_property, List<BaseId> a_ids);
    LookUpValueStruct GetValue(string a_property, BaseId a_id);
}

public interface IInventoryExtensionDataSource : IScenarioDetailDataSource
{
    LookUpValueStruct GetOverrideWarehouseValue(string a_property, InventoryKey a_id);
    LookUpValueStruct GetOverrideWarehouseValue(IWarehouseProperty a_property, InventoryKey a_id);
    LookUpValueStruct GetOverrideItemValue(string a_property, InventoryKey a_id);
    LookUpValueStruct GetOverrideItemValue(IItemProperty a_property, InventoryKey a_id);
    LookUpValueStruct GetOverrideInventoryValue(string a_property, InventoryKey a_id);
    LookUpValueStruct GetOverrideInventoryValue(IInventoryProperty a_property, InventoryKey a_id);
    List<LookUpValueStruct> GetOverrideWarehouseValueBlock(IWarehouseProperty a_property, List<InventoryKey> a_ids);
    List<LookUpValueStruct> GetOverrideItemValueBlock(IItemProperty a_property, List<InventoryKey> a_ids);
    List<LookUpValueStruct> GetOverrideInventoryValueBlock(IInventoryProperty a_property, List<InventoryKey> a_ids);
    LookUpValueStruct GetOverrideValue(string a_property, object propValue);
}

public interface IInventoryDataSource : IInventoryExtensionDataSource
{
    LookUpValueStruct GetWarehouseValue(string a_property, InventoryKey a_id);
    LookUpValueStruct GetWarehouseValue(IWarehouseProperty a_property, InventoryKey a_id);
    LookUpValueStruct GetItemValue(string a_property, InventoryKey a_id);
    LookUpValueStruct GetItemValue(IItemProperty a_property, InventoryKey a_id);
    LookUpValueStruct GetInventoryValue(string a_property, InventoryKey a_id);
    LookUpValueStruct GetInventoryValue(IInventoryProperty a_property, InventoryKey a_id);
    List<LookUpValueStruct> GetWarehouseValueBlock(string a_property, List<InventoryKey> a_ids);
    List<LookUpValueStruct> GetWarehouseValueBlock(IWarehouseProperty a_property, List<InventoryKey> a_ids);
    List<LookUpValueStruct> GetItemValueBlock(string a_property, List<InventoryKey> a_ids);
    List<LookUpValueStruct> GetItemValueBlock(IItemProperty a_property, List<InventoryKey> a_ids);
    List<LookUpValueStruct> GetInventoryValueBlock(string a_property, List<InventoryKey> a_ids);
    List<LookUpValueStruct> GetInventoryValueBlock(IInventoryProperty a_property, List<InventoryKey> a_ids);
}
public interface IStorageAreaExtensionDataSource : IScenarioDetailDataSource
{
    LookUpValueStruct GetOverrideWarehouseValue(string a_property, StorageAreaKey a_id);
    LookUpValueStruct GetOverrideWarehouseValue(IWarehouseProperty a_property, StorageAreaKey a_id);
    LookUpValueStruct GetOverrideStorageAreaValue(string a_property, StorageAreaKey a_id);
    LookUpValueStruct GetOverrideStorageAreaValue(IStorageAreaProperty a_property, StorageAreaKey a_id);
    List<LookUpValueStruct> GetOverrideWarehouseValueBlock(IWarehouseProperty a_property, List<StorageAreaKey> a_ids);
    List<LookUpValueStruct> GetOverrideStorageAreaValueBlock(IStorageAreaProperty a_property, List<StorageAreaKey> a_ids);
    LookUpValueStruct GetOverrideValue(string a_property, object a_propValue);
}

public interface IStorageAreaDataSource : IStorageAreaExtensionDataSource
{
    LookUpValueStruct GetWarehouseValue(string a_property, StorageAreaKey a_id);
    LookUpValueStruct GetWarehouseValue(IWarehouseProperty a_property, StorageAreaKey a_id);
    LookUpValueStruct GetStorageAreaValue(string a_property, StorageAreaKey a_id);
    LookUpValueStruct GetStorageAreaValue(IStorageAreaProperty a_property, StorageAreaKey a_id);
    List<LookUpValueStruct> GetWarehouseValueBlock(string a_property, List<StorageAreaKey> a_ids);
    List<LookUpValueStruct> GetWarehouseValueBlock(IWarehouseProperty a_property, List<StorageAreaKey> a_ids);
    List<LookUpValueStruct> GetStorageAreaValueBlock(string a_property, List<StorageAreaKey> a_ids);
    List<LookUpValueStruct> GetStorageAreaValueBlock(IStorageAreaProperty a_property, List<StorageAreaKey> a_ids);
}

public interface IUserDataSource : IScenarioDataChangeSignalInfo
{
    List<LookUpValueStruct> GetValueBlock(IUserProperty a_property, List<BaseId> a_ids);
    List<LookUpValueStruct> GetValueBlock(string a_property, List<BaseId> a_ids);
    LookUpValueStruct GetValue(IUserProperty a_property, BaseId a_id);
    LookUpValueStruct GetValue(string a_property, BaseId a_id);
}

public interface IForecastDataSource : IScenarioDataChangeSignalInfo
{
    List<LookUpValueStruct> GetValueBlock(IForecastProperty a_property, List<ForecastKey> a_ids);
    List<LookUpValueStruct> GetValueBlock(string a_property, List<ForecastKey> a_ids);
    LookUpValueStruct GetValue(IForecastProperty a_property, ForecastKey a_id);
    LookUpValueStruct GetValue(string a_property, ForecastKey a_id);
}

public interface ICustomerDataSource : ICustomerExtensionDataSource
{
    List<LookUpValueStruct> GetValueBlock(ICustomerProperty a_property, List<BaseId> a_ids);
    List<LookUpValueStruct> GetValueBlock(string a_property, List<BaseId> a_ids);
    LookUpValueStruct GetValue(ICustomerProperty a_property, BaseId a_id);
    LookUpValueStruct GetValue(string a_property, BaseId a_id);
}

public interface ICustomerExtensionDataSource : IScenarioDetailDataSource
{
    LookUpValueStruct GetOverrideValue(ICustomerProperty a_property, BaseId a_baseId);
    List<LookUpValueStruct> GetOverrideValueBlock(ICustomerProperty a_property, List<BaseId> a_ids);
    LookUpValueStruct GetOverrideValue(string a_property, BaseId a_id);
    LookUpValueStruct GetOverrideValue(string a_property, object propValue);
}