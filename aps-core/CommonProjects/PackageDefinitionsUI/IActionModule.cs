using System.Windows.Forms;

using DevExpress.Utils.Menu;
using DevExpress.Utils.Svg;

using PT.APSCommon;
using PT.PackageDefinitions;
using PT.Scheduler;
using PT.Scheduler.Demand;
using PT.SchedulerDefinitions;
using PT.Transmissions.Interfaces;

namespace PT.PackageDefinitionsUI;

public interface IObjectActionModule
{
    /// <summary>
    /// Provide the system with a collection of elements that should be visible to the user based on a provided object
    /// </summary>
    /// <param name="a_scenarioInfo">ScenarioInfo reference</param
    /// <param name="a_sourceKey">The source object's key. This can be used to determine if the elements should be loaded for that object</param>
    /// <returns></returns>
    List<IObjectActionElement> GetObjectActionElements(IScenarioInfo a_scenarioInfo, string a_sourceKey);
}

public interface IJobActionElement : IObjectActionElement
{
    void PrepareAction(IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<Job> a_jobs);

    void PerformAction(IClientSession a_clientSession, IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<Job> a_jobs);
}

/// <summary>
/// This is not a normal action element, it exists to add the buttons to the ui.
/// The actual logic for these actions are handled on the grid.
///
/// If you wish to use this for a normal action element you will need to do some reworking to the logic handling,
/// these elements in various places in the app.
/// </summary>
public interface ISequenceFactorActionElement : IObjectActionElement
{
    void PrepareAction(IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd);

    void PerformAction(IClientSession a_clientSession, IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd);
}

public interface IPurchaseOrderActionElement : IObjectActionElement
{
    void PrepareAction(IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<PurchaseToStock> a_pos);

    void PerformAction(IClientSession a_clientSession, IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<PurchaseToStock> a_pos);
}

public interface ISalesOrderActionElement : IObjectActionElement
{
    void PrepareAction(IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<SalesOrder> a_salesOrders, IEnumerable<SalesOrderLineDistribution> a_lineDistributions);

    void PerformAction(IClientSession a_clientSession, IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<SalesOrder> a_salesOrders, IEnumerable<SalesOrderLineDistribution> a_lineDistributions);
}

public interface IActivityActionElement : IObjectActionElement
{
    void PrepareAction(IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<InternalActivity> a_activities);

    void PerformAction(IClientSession a_clientSession, IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<InternalActivity> a_activities);
}

public interface IMaterialActionElement : IObjectActionElement
{
    void PrepareAction(IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<(InternalOperation, MaterialRequirement)> a_opMaterials);

    void PerformAction(IClientSession a_clientSession, IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<(InternalOperation, MaterialRequirement)> a_opMaterials);
}

public interface IResourceActionElement : IObjectActionElement
{
    void PrepareAction(IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<Resource> a_resources);

    void PerformAction(IClientSession a_clientSession, IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<Resource> a_resources);
}

public interface IDepartmentActionElement : IObjectActionElement
{
    void PrepareAction(IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<Department> a_departments);

    void PerformAction(IClientSession a_clientSession, IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<Department> a_departments);
}

public interface IPlantActionElement : IObjectActionElement
{
    void PrepareAction(IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<Plant> a_plants);

    void PerformAction(IClientSession a_clientSession, IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<Plant> a_plants);
}

public interface ICustomerActionElement : IObjectActionElement
{
    void PrepareAction(IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<BaseId> a_customers);

    void PerformAction(IClientSession a_clientSession, IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<BaseId> a_customers);
}

public interface IStorageAreaActionElement : IObjectActionElement
{
    void PrepareAction(IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<StorageArea> a_storageAreas);

    void PerformAction(IClientSession a_clientSession, IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, IEnumerable<StorageArea> a_storageAreas);
}

public interface ICapacityIntervalActionElement : IObjectActionElement
{
    void PrepareAction(IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, CapacityInterval a_capacityInterval, ResourceKey a_resourceKey);

    void PerformAction(IClientSession a_clientSession, IScenarioInfo a_scenarioInfo, ScenarioDetail a_sd, CapacityInterval a_capacityInterval, ResourceKey a_resourceKey, DateTimeOffset a_startDate, DateTimeOffset a_endDate);
}

/// <summary>
/// A base action element
/// </summary>
public interface IObjectActionElement : IPackageElement, IPriorityElement
{
    /// <summary>
    /// The image to show in the UI for this action. This image will only be loaded once per UI representation
    /// </summary>
    SvgImage Image { get; }

    /// <summary>
    /// The display text to show the user. This can be changed during PrepareAction
    /// </summary>
    string Caption { get; }

    /// <summary>
    /// The display text to show the user for extra details. This can be changed during PrepareAction
    /// </summary>
    string Description { get; }

    /// <summary>
    /// A separation group for elements that are related by concept.
    /// For example a separation group on a right click menu or separate group box for an action tile
    /// </summary>
    string Classification { get; }

    /// <summary>
    /// 0 or more keyboard keys for use as a shortcut
    /// For example [Keys.Control, Keys.L]
    /// </summary>
    Keys[] ShortcutKeys => null;

    /// <summary>
    /// Whether to show this element as enabled. This can be changed during PrepareAction
    /// </summary>
    bool Enabled { get; }

    /// <summary>
    /// Whether to show this element. This can be changed during PrepareAction
    /// </summary>
    bool Visible { get; }

    /// <summary>
    /// Whether to show this element in a action tile in the board
    /// </summary>
    bool ShowOnBoard { get; }

    /// <summary>
    /// Whether the action of this element should run on the main thread. We would need to run on the main
    /// thread if we need to modify or create a user control.
    /// </summary>
    bool RunOnMainThread { get; }

    /// <summary>
    /// If the action associated with the element modifies scenario data, then this should be set to true.
    /// This should be the case for most actions, which is why it defaults to true. This property is used
    /// in conjunction with permissions to determine what action elements a user can see and interact with.
    /// </summary>
    bool ModifiesScenario => true;

    /// <summary>
    /// The permissions required to use this action. The user will need all required permissions to perform the action.
    /// </summary>
    IEnumerable<string> RequiredPermissions => Enumerable.Empty<string>();
}

/// <summary>
/// An extension to an action element for showing on a popup menu
/// </summary>
public interface IGridMenuActionElement : IObjectActionElement
{
    /// <summary>
    /// Return the menu item to show in the popup menu
    /// </summary>
    /// <returns></returns>
    DXMenuItem GetMenuItem()
    {
        DXMenuItem menuItem = new (Caption);
        menuItem.ImageOptions.SvgImage = Image;
        return menuItem;
    }

    /// <summary>
    /// A collection group for elements that are related by function.
    /// For example a sub menu on a right click menu
    /// </summary>
    string SubGroup => "";
}