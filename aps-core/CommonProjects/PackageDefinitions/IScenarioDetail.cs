using System.Collections;

using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.PackageDefinitions;

/// <summary>
/// Demo interface for ScenarioDetail. Implementing this allows more separation of objects in the Scheduler project.
/// </summary>
public interface IScenarioDetail
{
    DateTime ClockDate { get; }
    DateTime GetEndOfShortTerm();
    DateTime GetPlanningHorizonEnd();

    ScenarioOptions ScenarioOptions { get; }

    IJobManager JobManagerInterface { get; }
    IPlantManager PlantManagerInterface { get; }
    IWarehouseManager WarehouseManagerInterface { get; }
    ISalesOrderManager SalesOrderManagerInterface { get; }
    IPurchaseOrderManager PurchaseOrderManagerInterface { get; }
}

public interface IObjectManager : IEnumerable
{
    int Count { get; }
}

public interface IJobManager : IObjectManager
{
    object GetJobById(BaseId a_id);
    object GetJobByIndex(int a_index);
    object this[int a_index] { get; }
}

public interface IPlantManager : IObjectManager
{
    object GetPlantById(BaseId a_id);
    object GetPlantByIndex(int a_index);
    object GetPlantResource(BaseId a_resId);
    object GetPlantDepartments();
    object this[int a_index] { get; }
}

public interface IWarehouseManager : IObjectManager
{
    object GetWarehouseById(BaseId a_id);
    object GetWarehouseByIndex(int a_index);
    object this[int a_index] { get; }
}

public interface ISalesOrderManager : IObjectManager
{
    object GetSalesOrderById(BaseId a_id);
    object GetSalesOrderByIndex(int a_index);
    object this[int a_index] { get; }
}

public interface IPurchaseOrderManager : IObjectManager
{
    object GetPurchaseOrderById(BaseId a_id);
    object GetPurchaseOrderByIndex(int a_index);
    object this[int a_index] { get; }
}