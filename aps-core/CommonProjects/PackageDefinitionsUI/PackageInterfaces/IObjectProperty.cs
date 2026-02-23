using System.Windows.Forms;

using DevExpress.Utils;
using DevExpress.XtraEditors.Repository;

using PT.ERPTransmissions;
using PT.PackageDefinitions;
using PT.Scheduler;
using PT.Scheduler.Demand;
using PT.SchedulerDefinitions;
using PT.Transmissions;

using BaseResource = PT.Scheduler.BaseResource;

namespace PT.PackageDefinitionsUI.PackageInterfaces;

/// <summary>
/// A calculated field that will be displayed based on interface type
/// </summary>
public interface IObjectProperty : IPackageElement
{
    /// <summary>
    /// Display text
    /// </summary>
    string PropertyName { get; }

    /// <summary>
    /// Type used for dataset and display
    /// </summary>
    Type PropertyType { get; }
}

/// <summary>
/// TODO: Update this summary when the recommended changes are made
/// This is just being used as a flag to determine if the column displaying the associated
/// DateTime-typed property should be set to DateOnly.
/// </summary>
public interface IFormatColumnAsDateOnly
{
    /*
     * TODO:
     * Having interfaces work as flags or switches can get confusing after awhile so
     * we'd like to make this interface more useful in the future.
     * It'd be nice if it contained information so that the program could
     * format the associated column differently from the standards already available.
     */

    // Suggested member
    //ENonDefaultFormats ColumnFormat { get; }
    // Note: The enum should be declared elsewhere, maybe in UnboundDataSourceModule.cs
    // Basically, when a column is generated using an IObjectProperty, then
    // we should check if it also implements this interface and use the ColumnFormat 
    // to register a different RepositoryItemEdit with formatting that we set. 
}

/// <summary>
/// An interface that should be implemented if the property needs non-standard format type
/// or custom format string. This interface is specifically meant for uneditable properties
/// because the IGridEditProperty already has a repository item, and the formatting of
/// the property can be set on the repository item.
/// </summary>
public interface IHasCustomFormattingProperty
{
    /// <summary>
    /// The property's FormatType
    /// </summary>
    FormatType PropertyFormatType { get; }

    /// <summary>
    /// The property's FormatString
    /// </summary>
    string PropertyFormatString { get; }
}

/// <summary>
/// This is an interface for properties that are meant to be editable in a grid,
/// and it is meant to be used in conjunction with any of the PT properties that implement
/// IObjectProperty (such as IActivityProperty).
/// Its default implementation is done in BasePropertyGridEditItem.cs, and the default
/// repository item it generates is GenericPropertyGridEditControl.cs.
/// </summary>
public interface IGridEditProperty
{
    /// <summary>
    /// A function that generates a repository item for the column generated
    /// according to the specifications of its associated property. The repository
    /// item is a control associated with a column that the user can interact with
    /// when they are trying to edit a property value. It determines formatting, masks,
    /// and other characteristics/behavior of the column when a user is editing the property.
    /// The default implementation in BasePropertyGridEditItem should be override
    /// if a specific property need specialized formatting, masks, and etc.
    /// </summary>
    void GenerateRepositoryItem();

    /// <summary>
    /// A reference to the repository item generated
    /// </summary>
    RepositoryItem RepositoryItemInstance { get; }
}

public interface IObjectPropertyEditControl
{
    /// <summary>
    /// The feature this property relates to. It is used for UI categorization
    /// </summary>
    string FeatureCategory { get; }

    /// <summary>
    /// Whether the current value is the default value for this property
    /// </summary>
    bool IsDefaultValue();

    IObjectProperty ObjectProperty { get; }

    Control ControlInstance { get; }
}

public interface IJobPropertyEditControl : IObjectPropertyEditControl
{
    /// <summary>
    /// Provides a resource update object.
    /// </summary>
    void Update(JobDataSet a_jobDataSet, JobDataSet.JobRow a_jobRow);

    /// <summary>
    /// Creates and initializes the property edit control
    /// </summary>
    void InitControl();

    /// <summary>
    /// Initializes the value for the property edit control
    /// </summary>
    void InitValue(Job a_job, ScenarioDetail a_sd);

    /// <summary>
    /// Fires when the edit value has been changed
    /// </summary>
    event Action<IJobPropertyEditControl> EditValueChanged;
}

public interface IActivityPropertyEditControl : IObjectPropertyEditControl
{
    /// <summary>
    /// Provides a resource update object.
    /// </summary>
    void Update(JobDataSet a_jobDataSet, JobDataSet.ActivityRow a_actRow);

    /// <summary>
    /// Creates and initializes the property edit control
    /// </summary>
    void InitControl();

    /// <summary>
    /// Initializes the value for the property edit control
    /// </summary>
    void InitValue(InternalActivity a_act, Resource a_res, Block a_block, ScenarioDetail a_sd);

    /// <summary>
    /// Fires when the edit value has been changed
    /// </summary>
    event Action<IActivityPropertyEditControl> EditValueChanged;
}

public interface IWarehousePropertyEditControl : IObjectPropertyEditControl
{
    /// <summary>
    /// Provides a warehouse update object.
    /// </summary>
    void Update(WarehouseT.Warehouse a_warehouse);

    /// <summary>
    /// Creates and initializes the property edit control
    /// </summary>
    void InitControl();

    /// <summary>
    /// Initializes the value for the property edit control
    /// </summary>
    void InitValue(Warehouse a_warehouse, ScenarioDetail a_sd);

    /// <summary>
    /// Fires when the edit value has been changed
    /// </summary>
    event Action<IWarehousePropertyEditControl> EditValueChanged;
}

public interface IInventoryPropertyEditControl : IObjectPropertyEditControl
{
    /// <summary>
    /// Provides a warehouse and an inventory update object.
    /// </summary>
    void Update(WarehouseT.Warehouse a_warehouse, WarehouseT.Inventory a_inventory);

    /// <summary>
    /// Creates and initializes the property edit control
    /// </summary>
    void InitControl();

    /// <summary>
    /// Initializes the value for the property edit control
    /// </summary>
    void InitValue(Inventory a_inventory, bool a_includeForecastInNetInv, ScenarioDetail a_sd);

    /// <summary>
    /// Fires when the edit value has been changed
    /// </summary>
    event Action<IInventoryPropertyEditControl> EditValueChanged;
}
public interface IStorageAreaPropertyEditControl : IObjectPropertyEditControl
{
    /// <summary>
    /// Provides a warehouse and a storage area update object.
    /// </summary>
    void Update(WarehouseT.Warehouse a_warehouse, WarehouseT.StorageArea a_storageArea);

    /// <summary>
    /// Creates and initializes the property edit control
    /// </summary>
    void InitControl();

    /// <summary>
    /// Initializes the value for the property edit control
    /// </summary>
    void InitValue(StorageArea a_storageArea, ScenarioDetail a_sd);

    /// <summary>
    /// Fires when the edit value has been changed
    /// </summary>
    event Action<IStorageAreaPropertyEditControl> EditValueChanged;
}

public interface IForecastPropertyEditControl : IObjectPropertyEditControl
{
    /// <summary>
    /// Provides a warehouse and an inventory update object.
    /// </summary>
    void Update(ForecastT.Forecast a_forecast, ForecastT.ForecastShipment a_forecastShipment);

    /// <summary>
    /// Creates and initializes the property edit control
    /// </summary>
    void InitControl();

    void InitValue(Inventory a_inventory, Forecast a_forecast, ForecastShipment a_shipment, ScenarioDetail a_sd);

    /// <summary>
    /// Fires when the edit value has been changed
    /// </summary>
    event Action<IForecastPropertyEditControl> EditValueChanged;
}

public interface IItemPropertyEditControl : IObjectPropertyEditControl
{
    /// <summary>
    /// Provides an item update object.
    /// </summary>
    void Update(WarehouseT.Item a_item);

    /// <summary>
    /// Creates and initializes the property edit control
    /// </summary>
    void InitControl();

    /// <summary>
    /// Initializes the value for the property edit control
    /// </summary>
    void InitValue(Item a_item, ScenarioDetail a_sd);

    /// <summary>
    /// Fires when the edit value has been changed
    /// </summary>
    event Action<IItemPropertyEditControl> EditValueChanged;
}

public interface IMaterialPropertyEditControl : IObjectPropertyEditControl
{
    /// <summary>
    /// Provides an item update object.
    /// </summary>
    void Update(JobDataSet a_dataSet, JobDataSet.MaterialRequirementRow a_row);

    /// <summary>
    /// Creates and initializes the property edit control
    /// </summary>
    void InitControl();

    /// <summary>
    /// Initializes the value for the property edit control
    /// </summary>
    void InitValue(MaterialRequirement a_mr, InternalOperation a_op, InternalActivity a_act, ScenarioDetail a_sd);

    /// <summary>
    /// Fires when the edit value has been changed
    /// </summary>
    event Action<IMaterialPropertyEditControl> EditValueChanged;
}

public interface IPoPropertyEditControl : IObjectPropertyEditControl
{
    /// <summary>
    /// Provides a purchase order update object.
    /// </summary>
    void Update(PurchaseToStockT.PurchaseToStock a_po);

    /// <summary>
    /// Creates and initializes the property edit control
    /// </summary>
    void InitControl();

    /// <summary>
    /// Initializes the value for the property edit control
    /// </summary>
    void InitValue(PurchaseToStock a_po, ScenarioDetail a_sd);

    /// <summary>
    /// Fires when the edit value has been changed
    /// </summary>
    event Action<IPoPropertyEditControl> EditValueChanged;
}

public interface IPoGridEditProperty : IPoProperty, IGridEditProperty
{
    /// <summary>
    /// Provides a purchase order update object.
    /// </summary>
    void Update(PurchaseToStockEdit a_po, object a_cellValue);
}

public interface IPlantGridEditProperty : IPlantProperty, IGridEditProperty
{
    /// <summary>
    /// Provides a Plant update object.
    /// </summary>
    void Update(PlantEdit a_plant, object a_cellValue);
}

public interface IDepartmentGridEditProperty : IDepartmentProperty, IGridEditProperty
{
    /// <summary>
    /// Provides a Department update object.
    /// </summary>
    void Update(DepartmentEdit a_department, object a_cellValue);
}

public interface IResourceGridEditProperty : IResourceProperty, IGridEditProperty
{
    /// <summary>
    /// Provides a Resource update object.
    /// </summary>
    void Update(ResourceEdit a_resource, object a_cellValue);
}

public interface ICustomerGridEditProperty : ICustomerProperty, IGridEditProperty
{
    /// <summary>
    /// Provides a purchase order update object.
    /// </summary>
    void Update(CustomerEdit a_ce, object a_cellValue);
}

public interface ISoGridEditProperty : ISoProperty, IGridEditProperty
{
    void Update(SalesOrderEdit a_so, object a_cellValue);
}

public interface ISoLineGridEditProperty : ISoLineProperty, IGridEditProperty
{
    void Update(ScenarioDetail a_sd, SalesOrderLineEdit a_edit, object a_cellValue);
}

public interface ISoLineDistributionGridEditProperty : ISoLineDistProperty, IGridEditProperty
{
    void Update(ScenarioDetail a_sd, SalesOrderLineDistributionEdit a_soLineDistribution, object a_cellValue);
}

public interface IMaterialsGridEditProperty : IMaterialsProperty, IGridEditProperty
{
    void Update(MaterialEdit a_materialEdit, MaterialRequirement a_mr, object a_cellValue);
}

public interface IMaterialsGridEditReadOnlyProperty : IMaterialsProperty, IGridEditProperty
{
    void Update(MaterialEdit a_materialEdit, MaterialRequirement a_mr, object a_cellValue);
}

public interface IMoGridEditProperty : IMoProperty, IGridEditProperty
{
    void Update(ManufacturingOrderEdit a_edit, object a_cellValue);
}

public interface IOperationGridEditProperty : IOperationProperty, IGridEditProperty
{
    void Update(OperationEdit a_operationEdit, object a_cellValue);
}

public interface IMaterialOperationGridEditProperty : IOperationProperty, IGridEditProperty
{
    void Update(OperationEdit a_operationEdit, object a_cellValue);
}

public interface IActivityGridEditProperty : IActivityProperty, IGridEditProperty
{
    void Update(ActivityEdit a_edit, object a_cellValue);
}

public interface ISoPropertyEditControl : IObjectPropertyEditControl
{
    /// <summary>
    /// Provides a sales order update object.
    /// </summary>
    void Update(SalesOrderTDataSet.SalesOrderRow a_so, bool displayOnly);

    /// <summary>
    /// Creates and initializes the property edit control
    /// </summary>
    void InitControl();

    /// <summary>
    /// Initializes the value for the property edit control
    /// </summary>
    void InitValue(SalesOrder a_so, ScenarioDetail a_sd);
}

public interface ISoLinePropertyEditControl : IObjectPropertyEditControl
{
    /// <summary>
    /// Provides a sales order line update object.
    /// </summary>
    object Update(SalesOrderTDataSet.SalesOrderLineRow a_line, bool displayOnly);

    /// <summary>
    /// Creates and initializes the property edit control
    /// </summary>
    void InitControl();

    /// <summary>
    /// Initializes the value for the property edit control
    /// </summary>
    void InitValue(SalesOrderLine a_line, ScenarioDetail a_sd);
}

public interface ISoLineDistPropertyEditControl : IObjectPropertyEditControl
{
    /// <summary>
    /// Provides a sales order line distribution update object.
    /// </summary>
    object Update(SalesOrderTDataSet.SalesOrderLineDistRow a_dist, bool displayOnly);

    /// <summary>
    /// Creates and initializes the property edit control
    /// </summary>
    void InitControl();

    /// <summary>
    /// Initializes the value for the property edit control
    /// </summary>
    void InitValue(SalesOrderLineDistribution a_dist, ScenarioDetail a_sd);
}

public interface IMoPropertyEditControl : IObjectPropertyEditControl
{
    /// <summary>
    /// Provides a manufacturing order update object.
    /// </summary>
    void Update(JobDataSet a_jobDataSet, JobDataSet.ManufacturingOrderRow a_moRow);

    /// <summary>
    /// Creates and initializes the property edit control
    /// </summary>
    void InitControl();

    /// <summary>
    /// Initializes the value for the property edit control
    /// </summary>
    void InitValue(ManufacturingOrder a_mo, ScenarioDetail a_sd);

    /// <summary>
    /// Fires when the edit value has been changed
    /// </summary>
    event Action<IMoPropertyEditControl> EditValueChanged;
}

public interface ICustomerPropertyEditControl : IObjectPropertyEditControl
{
    /// <summary>
    /// Provides an item update object.
    /// </summary>
    void Update(CustomerT a_customerT);

    /// <summary>
    /// Creates and initializes the property edit control
    /// </summary>
    void InitControl();

    /// <summary>
    /// Initializes the value for the property edit control
    /// </summary>
    void InitValue(Customer a_customer, ScenarioDetail a_sd);

    /// <summary>
    /// Fires when the edit value has been changed
    /// </summary>
    event Action<ICustomerPropertyEditControl> EditValueChanged;
}

/// <summary>
/// A job property that reflects scheduled values
/// </summary>
public interface IJobProperty : IUnscheduledJobProperty { }

public interface IJobImpactProperty : IImpactProperty, IPackageElement
{
    IComparable CalculateValue(Job a_job, ScenarioDetail a_sd);
}

/// <summary>
/// A template property
/// </summary>
public interface ITemplateProperty : IJobBaseProperty { }

public interface ITemplateImpactProperty : IImpactProperty, IPackageElement
{
    IComparable CalculateValue(Job a_job, ScenarioDetail a_sd);
}

/// <summary>
/// An property of a job that can be scheduled
/// </summary>
public interface IUnscheduledJobProperty : IJobBaseProperty { }

/// <summary>
/// A shared job property
/// </summary>
public interface IJobBaseProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(Job a_job, ScenarioDetail a_sd);
}

public interface IJobGridEditProperty : IJobProperty, IGridEditProperty
{
    void Update(JobEdit a_edit, object a_cellValue);
}

public interface IJobBaseGridEditProperty : IJobBaseProperty, IGridEditProperty
{
    void Update(JobEdit a_edit, object a_cellValue);
}

public interface IBaseUDFProperty
{
    public bool Display { get; }
}

/// <summary>
/// A shared job UDF property
/// </summary>
public interface IJobUDFProperty : IJobGridEditProperty, IBaseUDFProperty { }

/// <summary>
/// A shared job Attribute property
/// </summary>
public interface IJobAttributeProperty : IJobEditProperty { }

public interface IJobEditProperty : IJobBaseProperty
{
    IJobPropertyEditControl GenerateEditControl();
}

/// <summary>
/// A resource property
/// </summary>
public interface IResourceProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(Resource a_res, ScenarioDetail a_sd);
}

public interface IResourceImpactProperty : IImpactProperty, IPackageElement
{
    IComparable CalculateValue(Resource a_res, ScenarioDetail a_sd);
}

/// <summary>
/// A shared Resource UDF property
/// </summary>
public interface IResourceUDFProperty : IBaseUDFProperty, IResourceGridEditProperty { }

/// <summary>
/// An activities scenario data locked property
/// </summary>
public interface IActivityProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(InternalActivity a_act, BaseResource a_resource, ScenarioDetail a_sd);
}

public interface IActivityImpactProperty : IImpactProperty, IPackageElement
{
    IComparable CalculateValue(InternalActivity a_act, BaseResource a_resource, Block a_block, ScenarioDetail a_sd);
}

public interface IActivityEditProperty : IActivityProperty
{
    IActivityPropertyEditControl GenerateEditControl();
}

/// <summary>
/// An operation scenario data locked property
/// </summary>
public interface IOperationProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(InternalOperation a_iOp, ScenarioDetail a_sd);
}

public interface IOperationImpactProperty : IImpactProperty, IPackageElement
{
    IComparable CalculateValue(InternalOperation a_iOp, ScenarioDetail a_sd);
}

/// <summary>
/// A shared Operation UDF property
/// </summary>
public interface IOperationUDFProperty : IOperationGridEditProperty, IBaseUDFProperty { }

/// <summary>
/// A shared Operation Attribute property
/// </summary>
public interface IOperationAttributeProperty : IOperationGridEditProperty
{
    public bool HideInGrid { get; }
}

/// <summary>
/// A Plant property
/// </summary>
public interface IPlantProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(Plant a_plant, ScenarioDetail a_sd);
}

/// <summary>
/// A shared Plant UDF property
/// </summary>
public interface IPlantUDFProperty : IBaseUDFProperty, IPlantGridEditProperty { }

/// <summary>
/// A Department property
/// </summary>
public interface IDepartmentProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(Department a_dept, ScenarioDetail a_sd);
}

/// <summary>
/// A shared Department UDF property
/// </summary>
public interface IDepartmentUDFProperty : IBaseUDFProperty, IDepartmentGridEditProperty { }

/// <summary>
/// A Warehouse property
/// </summary>
public interface IWarehouseProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(Warehouse a_warehouse, ScenarioDetail a_sd);
}

public interface IWarehouseImpactProperty : IImpactProperty, IPackageElement
{
    IComparable CalculateValue(Warehouse a_warehouse, ScenarioDetail a_sd);
}

public interface IWarehouseEditProperty : IWarehouseProperty
{
    IWarehousePropertyEditControl GenerateEditControl();
}

public interface IWarehouseGridEditProperty : IGridEditProperty, IWarehouseProperty
{
    void Update(WarehouseEdit a_edit, object a_cellValue);
}

/// <summary>
/// A shared Warehouse UDF property
/// </summary>
public interface IWarehouseUDFProperty : IWarehouseGridEditProperty, IBaseUDFProperty { }

/// <summary>
/// A shared Warehouse Attribute property
/// </summary>
public interface IWarehouseAttributeProperty : IWarehouseEditProperty { }

/// <summary>
/// A manufacturing order property
/// </summary>
public interface IMoProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(ManufacturingOrder a_mo, ScenarioDetail a_sd);
}

public interface IMoImpactProperty : IImpactProperty, IPackageElement
{
    IComparable CalculateValue(ManufacturingOrder a_mo, ScenarioDetail a_sd);
}

public interface IMoEditProperty : IMoProperty
{
    IMoPropertyEditControl GenerateEditControl();
}

/// <summary>
/// A shared Mo UDF property
/// </summary>
public interface IMoUDFProperty : IMoGridEditProperty, IBaseUDFProperty { }

/// <summary>
/// A shared Mo Attribute property
/// </summary>
public interface IMoAttributeProperty : IMoEditProperty { }

/// <summary>
/// A Materials property
/// </summary>
public interface IMaterialsProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(MaterialRequirement a_materialRequirement, InternalOperation a_iOp, InternalActivity a_activity, ScenarioDetail a_sd);
}

public interface IMaterialImpactProperty : IImpactProperty, IPackageElement
{
    IComparable CalculateValue(MaterialRequirement a_materialRequirement, InternalOperation a_iOp, InternalActivity a_activity, ScenarioDetail a_sd);
}

public interface IMaterialEditProperty : IMaterialsProperty
{
    IMaterialPropertyEditControl GenerateEditControl();
}

/// <summary>
/// An Inventory property
/// </summary>
public interface IInventoryProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(Inventory a_inv, bool a_includeForecastInNetInv, ScenarioDetail a_sd);
}

public interface IInventoryGridEditProperty : IInventoryProperty, IGridEditProperty
{
    void Update(InventoryEdit a_edit, object a_cellValue);
}

public interface IInventoryImpactProperty : IImpactProperty, IPackageElement
{
    IComparable CalculateValue(Inventory a_inv, bool a_includeForecastInNetInv, ScenarioDetail a_sd);
}

public interface IInventoryEditProperty : IInventoryProperty
{
    IInventoryPropertyEditControl GenerateEditControl();
}
/// <summary>
/// An StorageArea property
/// </summary>
public interface IStorageAreaProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(StorageArea a_storageArea, ScenarioDetail a_sd);
}
public interface IStorageAreaEditProperty : IStorageAreaProperty
{
    IStorageAreaPropertyEditControl GenerateEditControl();
}
public interface IStorageAreaGridEditProperty : IGridEditProperty, IStorageAreaProperty
{
    void Update(StorageAreaEdit a_edit, object a_cellValue);
}
public interface IStorageAreaImpactProperty : IImpactProperty, IPackageElement
{
    IComparable CalculateValue(StorageArea a_storageArea, ScenarioDetail a_sd);
}

/// <summary>
/// An inventory forecast property
/// </summary>
public interface IForecastProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(Inventory a_inv, Forecast a_forecast, ForecastShipment a_shipment);
}

public interface IForecastImpactProperty : IImpactProperty, IPackageElement
{
    IComparable CalculateValue(Inventory a_inv, Forecast a_forecast, ForecastShipment a_shipment);
}

public interface IForecastEditProperty : IForecastProperty
{
    IForecastPropertyEditControl GenerateEditControl();
}

/// <summary>
/// A shared Forecast UDF property
/// </summary>
public interface IForecastUDFProperty : IForecastEditProperty, IBaseUDFProperty { }

/// <summary>
/// An Item property
/// </summary>
public interface IItemProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(Item a_item, ScenarioDetail a_sd);
}

public interface IItemImpactProperty : IImpactProperty, IPackageElement
{
    IComparable CalculateValue(Item a_item, ScenarioDetail a_sd);
}

/// <summary>
/// A shared Item UDF property
/// </summary>
public interface IItemUDFProperty : IItemGridEditProperty, IBaseUDFProperty { }

/// <summary>
/// A shared Item Attribute property
/// </summary>
public interface IItemAttributeProperty : IItemEditProperty { }

public interface IItemEditProperty : IItemProperty
{
    IItemPropertyEditControl GenerateEditControl();
}

public interface IItemGridEditProperty : IItemProperty, IGridEditProperty
{
    void Update(ItemEdit a_edit, object a_cellValue);
}

/// <summary>
/// A purchase order property
/// </summary>
public interface IPoProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(PurchaseToStock a_po, ScenarioDetail a_sd);
}

public interface IPoImpactProperty : IImpactProperty, IPackageElement
{
    IComparable CalculateValue(PurchaseToStock a_po, ScenarioDetail a_sd);
}

/// <summary>
/// A shared Po UDF property
/// </summary>
public interface IPoUDFProperty : IPoGridEditProperty, IBaseUDFProperty { }

/// <summary>
/// A Resource Block property
/// </summary>
public interface IResourceBlockProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(ResourceBlock a_block, ScenarioDetail a_sd);
}

public interface IResourceBlockImpactProperty : IImpactProperty, IPackageElement
{
    IComparable CalculateValue(ResourceBlock a_block, ScenarioDetail a_sd);
}

/// <summary>
/// A sales order property
/// </summary>
public interface ISoProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(SalesOrder a_so, ScenarioDetail a_sd);
}

public interface ISoImpactProperty : IImpactProperty, IPackageElement
{
    IComparable CalculateValue(SalesOrder a_so, ScenarioDetail a_sd);
}

/// <summary>
/// A shared So UDF property
/// </summary>
public interface ISoUDFProperty : ISoGridEditProperty, IBaseUDFProperty { }

/// <summary>
/// A shared So Attribute property
/// </summary>
public interface ISoAttributeProperty : ISoEditProperty { }

/// <summary>
/// A sales order line property
/// </summary>
public interface ISoLineProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(SalesOrderLine a_line, ScenarioDetail a_sd);
}

public interface ISoLineImpactProperty : IImpactProperty, IPackageElement
{
    IComparable CalculateValue(SalesOrderLine a_line, ScenarioDetail a_sd);
}

/// <summary>
/// A sales order line distribution property
/// </summary>
public interface ISoLineDistProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(SalesOrderLineDistribution a_dist, ScenarioDetail a_sd);
}

public interface ISoLineDistImpactProperty : IImpactProperty, IPackageElement
{
    IComparable CalculateValue(SalesOrderLineDistribution a_dist, ScenarioDetail a_sd);
}

public interface ISoEditProperty : ISoProperty
{
    ISoPropertyEditControl GenerateEditControl();
}

public interface ISoLineEditProperty : ISoLineProperty
{
    ISoLinePropertyEditControl GenerateEditControl();
}

public interface ISoLineDistEditProperty : ISoLineDistProperty
{
    ISoLineDistPropertyEditControl GenerateEditControl();
}

public interface ISalesOrder { }

public interface ISalesOrderLine { }

public interface ISalesOrderLineDistribution { }

public interface IBatchProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(Batch a_batch);
}

public interface IResourceRequirementProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(ResourceRequirement a_resourceRequirement);
}

/// <summary>
/// A User property
/// </summary>
public interface IUserProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(UserManager a_um, User a_user);
}

public interface IUserGridEditProperty : IUserProperty, IGridEditProperty
{
    /// <summary>
    /// Provides a purchase order update object.
    /// </summary>
    void Update(UserEdit a_userEdit, object a_cellValue);
}

/// <summary>
/// A shared User UDF property
/// </summary>
public interface IUserUDFProperty : IBaseUDFProperty, IUserGridEditProperty { }

/// <summary>
/// A shared Customer UDF property
/// </summary>
public interface ICustomerUDFProperty : IBaseUDFProperty, ICustomerGridEditProperty { }

/// <summary>
/// A shared Storage Area UDF property
/// </summary>
public interface IStorageAreaUDFProperty : IBaseUDFProperty, IStorageAreaGridEditProperty { }

/// <summary>
/// A property that should be displayed in a different manor than it's base type
/// </summary>
public interface IPercentProperty
{
    RepositoryItemProgressBar GetCellEditor(int a_percentValue);
}

public interface IImageProperty
{
    RepositoryItemImageComboBox GetColumnEditor();
}

public interface ICurrencyProperty { }

public enum ETimeSpanDisplayUnits { Minutes, Hours, Days }

/// <summary>
/// This property requires ScenarioDetail to be provided to cache data for better calculations
/// </summary>
public interface IScenarioDetailProperty : IObjectProperty
{
    /// <summary>
    /// This is called before a datasource attempts to get the properties value for the first time after SD changes
    /// </summary>
    /// <param name="a_sd">Data Model </param>
    void Reload(ScenarioDetail a_sd);
}

/// <summary>
/// This property requires UserManager to be provided to cache data for better calculations
/// </summary>
public interface IUserDetailProperty : IObjectProperty
{
    /// <summary>
    /// This is called before a datasource attempts to get the properties value for the first time after User Manager changes
    /// </summary>
    /// <param name="a_userManager">UJser Data </param>
    void Reload(UserManager a_userManager);
}

public interface ITimeSpanProperty
{
    ETimeSpanDisplayUnits DefaultDisplayUnits { get; }
}

/// <summary>
/// A product rule property
/// </summary>
public interface IProductRuleProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(ScenarioDetail a_sd, PT.ERPTransmissions.ProductRulesT.ProductRule a_prTRule, Item a_item);
}

/// <summary>
/// A Materials property
/// </summary>
public interface ICustomerProperty : IObjectProperty
{
    /// <summary>
    /// Return a calculated value. Must be of IObjectProperty.Type type
    /// </summary>
    object GetValue(ScenarioDetail a_sd, Customer a_customer);
}

public interface ICustomerImpactProperty : IImpactProperty, IPackageElement
{
    IComparable CalculateValue(ScenarioDetail a_sd, Customer a_customer);
}

public interface ICustomerEditProperty : ICustomerProperty
{
    ICustomerPropertyEditControl GenerateEditControl();
}