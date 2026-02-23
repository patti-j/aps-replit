using System.Data;
using System.Diagnostics;

using PT.APIDefinitions;
using PT.APIDefinitions.RequestsAndResponses.Webapp;
using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;

namespace PT.ImportDefintions;

public static class IntegrationConstants
{
    public const string CoreCategory = "Tables";
    public const string AdvancedTablesCategory = "Advanced Tables";
    public const string FeatureCategory = "Features";
    public static int GetCategoryOrder(string a_categoryName)
    {
        switch (a_categoryName)
        {
            case (IntegrationConstants.CoreCategory):
                return 1;
            case (IntegrationConstants.AdvancedTablesCategory):
                return 2;
            case (IntegrationConstants.FeatureCategory):
                return 3;
            default:
                return int.MaxValue;
        }
    }
}

public enum ObjectType
{
    Resources,
    Items,
    Jobs,
    Attributes,
    Cleanouts,
    Compatibilities
}

public class IntegrationFeatureBase
{
    private List<IntegrationProperty> m_properties;

    public List<IntegrationProperty> Properties
    {
        get => m_properties;
        set => m_properties = value.OrderByDescending(prop => prop.RequiredForFeature)
                                   .ThenBy(prop => prop.TableName)
                                   .ThenBy(prop => prop.ColumnName).ToList();
    }

    public virtual string Category { get; set; }

    public virtual string Group { get; set; }
    
    public virtual string FeatureName { get; set; }
    public virtual string FeatureCaption => FeatureName.Localize();

    public virtual string FeatureHelpTopic { get; set; }
    
    public virtual List<ObjectType> ObjectTypes { get; set; }


    public virtual int Step { get; set; } = UInt16.MaxValue;

    public virtual bool IsBaseTableFeature => Category == IntegrationConstants.CoreCategory || Category == IntegrationConstants.AdvancedTablesCategory;
    
    //1-31-2025 All Current AutoDelete settings organized by the Import Step they are used
    //
    //Plants: PlantSettings
    // Departments: DepartmentSettings
    // Customer: CustomerSettings
    // UDF: UserFieldSettings
    // Capabilities: CapabilitySettings
    // Cells: CellsSettings
    // Resources: MachineSettings
    // 	    CapabilityAssociations: CapabilityAssignmentSettings
    // 	    AllowedHelpers: AllowedHelperResourcesSettings
    // ResourceConnectors: ResourceConnectorsSettings
    // 	    Connections: ResourceConnectionSettings
    // ItemsOnly: ItemSettings
    // Warehouses: WarehouseSettings
    // 	    Inventories: InventorySettings
    // 	    Lots: LotsSettings
    // 	    Items: ItemSettings
    // 	    StorageAreas: StorageAreaSettings
    // Jobs: JobSettings
    // CapacityIntervals: CapacityIntervalSettings
    // 	    ResourceAssociations: CapacityIntervalResourceSettings
    // RecurringCapacityIntervals: RecurringCapacityIntervalSettings
    // 	    ResourceAssociations: CapacityIntervalResourceSettings
    // ProductRules: ProductRulesSettings
    // Attributes: AttributeSettings
    // AttributesSetupTable: SetupTableAttSettings
    // AttributeCodeTable: AttributeCodeTableSetting
    // CompatibilityCodeTable: CompatibilityCodeTablesSettings
    // PurchaseToStock: PurchaseToStockSettings
    // SalesOrders: SalesOrderSettings
    // Forecasts: ForecastSettings
    // TransferOrder: TransferOrderSettings
    public virtual bool CanSetAutoDelete { get; set; } = false;

    // Add common validation here, and have subclasses call base.ValidateRow in addition to defining any additional logic and passing in a dictionary to add to
    // TODO: The precedent here is to add dictionary values keyed by violating property name. That won't work if the same prop is invalid on multiple dimensions, so we should handle this better
    public virtual Result<object, Dictionary<string, string>> ValidateRow(DataRow a_dataRow, Dictionary<string, string> a_invalidValues = null)
    {
        a_invalidValues ??= new Dictionary<string, string>();
        
        ValidateRequiredProperties(a_dataRow, a_invalidValues);

        if (a_invalidValues.Count > 0)
        {
            return new(a_invalidValues);
        }

        return new("");
    }

    private void ValidateRequiredProperties(DataRow a_dataRow, Dictionary<string, string> dict)
    {
        foreach (IntegrationProperty requiredProp in Properties.Where(prop => prop.RequiredForFeature))
        {
            string requiredPropName = requiredProp.ColumnName;
            if (a_dataRow.Table.Columns.Contains(requiredPropName) && 
                string.IsNullOrEmpty((string)a_dataRow[requiredPropName]))
            {
                dict.Add(requiredPropName, string.Format("{0} cannot be null or empty".Localize(), requiredPropName));
            }
        }
    }

    /// <summary>
    /// Properties that are *always* non-nullable in every table they show up in.
    /// If any tables don't use this prop in the same way, it should instead be handled in subclasses.
    /// </summary>
    private List<string> m_commonRequiredTableProps => new List<string>()
    {
        "ExternalId", "Name"
    };


    public bool Enabled { get; set; } = false;
    public int WebAppId { get; set; }

    /// <summary>
    /// Gets the Core Table Feature name for a particular (canonical) table.
    /// </summary>
    /// <param name="a_tableName"></param>
    /// <returns></returns>
    internal static string GetFeatureNameForTable(string a_tableName)
    {
        // Currently, we store both Feature and Table name as the entity name, singular. Keeping this method here in case that changes.
        return a_tableName;
    }

    public void Update(FeatureDTO a_featureDto, Dictionary<PropertyDTO.PropertyLookupKey, PropertyDTO> a_propertyDtoLookup)
    {
        Enabled = a_featureDto.Enabled;

        UpdateProperties(a_propertyDtoLookup);
    }

    private void UpdateProperties(Dictionary<PropertyDTO.PropertyLookupKey, PropertyDTO> a_propertyDtoLookup)
    {
        foreach (IntegrationProperty featureProperty in Properties)
        {
            PropertyDTO.PropertyLookupKey propKey = new PropertyDTO.PropertyLookupKey(featureProperty.TableName, featureProperty.ColumnName);
            if (a_propertyDtoLookup.TryGetValue(propKey, out PropertyDTO propertyDto))
            {
                featureProperty.Update(propertyDto);
            }
            else
            {
                // If the DTO feature doesn't contain this property, it is defined in the current software version but not the one this was loaded from.
                // We can leave the current property at its defaults.
                // TODO: Notifiy the user that new defaults were added for this prop

                // Note conversely that properties in the DTO but not in the software version's feature will also be ignored -
                // this is fine for the use of the config, since the software would have nothing to do with the prop, but we probably want to capture it so that it can be saved and stored for other versions?
            }
        }
    }
}

#region Feature Subclasses
public class AllowedHelpersFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "AllowedHelpers";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Allowed_Helpers";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Resources };
    public override int Step => 2800;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "AllowedHelpers";
    public AllowedHelpersFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("AllowedHelpers", "AllowedHelperDepartmentExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("AllowedHelpers", "AllowedHelperPlantExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("AllowedHelpers", "AllowedHelperResourceExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("AllowedHelpers", "ResourceDepartmentExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("AllowedHelpers", "ResourceExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("AllowedHelpers", "ResourcePlantExternalId", EPropertyDataType.String, true),
        };
    }
}
public class CapabilitiesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Capabilities";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Capability";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Resources };
    public override int Step => 1300;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public override string Group => "Capabilities";
    public CapabilitiesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Capabilities", "Description", EPropertyDataType.String, false),
            new IntegrationProperty("Capabilities", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Capabilities", "Name", EPropertyDataType.String, true),
        };
    }
}

public class CapacityIntervalResourcesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "CapacityIntervalResources";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Capacity_Interval_Resource";
    public override int Step => 1700;
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Resources };
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public override string Group => "CapacityIntervalResources";
    public CapacityIntervalResourcesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("CapacityIntervalResources", "CapacityIntervalExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("CapacityIntervalResources", "PlantExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("CapacityIntervalResources", "ResourceExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("CapacityIntervalResources", "DepartmentExternalId", EPropertyDataType.String, true),
        };
    }
}
public class CapacityIntervalsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "CapacityIntervals";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Capacity_Interval";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Resources };
    public override int Step => 1500;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public override string Group => "CapacityIntervals";
    public CapacityIntervalsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("CapacityIntervals", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("CapacityIntervals", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("CapacityIntervals", "StartDateTime", EPropertyDataType.DateTime, true),
            new IntegrationProperty("CapacityIntervals", "EndDateTime", EPropertyDataType.DateTime, true),
            new IntegrationProperty("CapacityIntervals", "IntervalType", EPropertyDataType.String, true),
            new IntegrationProperty("CapacityIntervals", "CanStartActivity", EPropertyDataType.Boolean, false),
            new IntegrationProperty("CapacityIntervals", "CapacityCode", EPropertyDataType.String, false),
            new IntegrationProperty("CapacityIntervals", "ResetAttributeChangeovers", EPropertyDataType.Boolean, false),
            new IntegrationProperty("CapacityIntervals", "Color", EPropertyDataType.String, false),
            new IntegrationProperty("CapacityIntervals", "Description", EPropertyDataType.String, false),
            new IntegrationProperty("CapacityIntervals", "IntervalPreset", EPropertyDataType.String, false),
            new IntegrationProperty("CapacityIntervals", "Overtime", EPropertyDataType.Boolean, false),
            new IntegrationProperty("CapacityIntervals", "PreventOperationsFromSpanning", EPropertyDataType.Boolean, false),
            new IntegrationProperty("CapacityIntervals", "UsedForClean", EPropertyDataType.Boolean, false),
            new IntegrationProperty("CapacityIntervals", "UsedForPostProcessing", EPropertyDataType.Boolean, false),
            new IntegrationProperty("CapacityIntervals", "UsedForRun", EPropertyDataType.Boolean, false),
            new IntegrationProperty("CapacityIntervals", "UsedForSetup", EPropertyDataType.Boolean, false),
            new IntegrationProperty("CapacityIntervals", "UsedForStoragePostProcessing", EPropertyDataType.Boolean, false),
            new IntegrationProperty("CapacityIntervals", "UseOnlyWhenLate", EPropertyDataType.Boolean, false),
            new IntegrationProperty("CapacityIntervals", "CanBeDraggedAndResized", EPropertyDataType.Boolean, false),
            new IntegrationProperty("CapacityIntervals", "CanBeDeleted", EPropertyDataType.Boolean, false),
        };
    }
}
public class CellsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Cells";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Cell";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Resources };
    public override int Step => 4000;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "Cells";
    public CellsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Cells", "Description", EPropertyDataType.String, false),
            new IntegrationProperty("Cells", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Cells", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("Cells", "UserFields", EPropertyDataType.String, false),
        };
    }
}
public class CleanoutTriggerTableOpCountFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "CleanoutTriggerTableOpCount";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Operation_Count_Cleanout_Triggers_Table";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Cleanouts };
    public override int Step => 4400;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "CleanoutTriggerTableOpCount";
    public CleanoutTriggerTableOpCountFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("CleanoutTriggerTableOpCount", "TableName", EPropertyDataType.String, true),
            new IntegrationProperty("CleanoutTriggerTableOpCount", "CleanCost", EPropertyDataType.Decimal, true),
            new IntegrationProperty("CleanoutTriggerTableOpCount", "DurationHours", EPropertyDataType.Double, true),
            new IntegrationProperty("CleanoutTriggerTableOpCount", "TriggerValue", EPropertyDataType.Int, true),
            new IntegrationProperty("CleanoutTriggerTableOpCount", "CleanoutGrade", EPropertyDataType.Int, false),
        };
    }
}
public class CleanoutTriggerTableProdUnitsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "CleanoutTriggerTableProdUnits";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Production_Unit_Cleanout_Triggers_Table";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Cleanouts };
    public override int Step => 4500;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "CleanoutTriggerTableProdUnits";
    public CleanoutTriggerTableProdUnitsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("CleanoutTriggerTableProdUnits", "TableName", EPropertyDataType.String, true),
            new IntegrationProperty("CleanoutTriggerTableProdUnits", "ProductionUnit", EPropertyDataType.String, true),
            new IntegrationProperty("CleanoutTriggerTableProdUnits", "TriggerValue", EPropertyDataType.Decimal, true),
            new IntegrationProperty("CleanoutTriggerTableProdUnits", "DurationHours", EPropertyDataType.Double, true),
            new IntegrationProperty("CleanoutTriggerTableProdUnits", "CleanCost", EPropertyDataType.Decimal, false),
            new IntegrationProperty("CleanoutTriggerTableProdUnits", "CleanoutGrade", EPropertyDataType.Int, false),
        };
    }
}
public class CleanoutTriggerTableResourcesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "CleanoutTriggerTableResources";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Cleanouts };
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Cleanout_Trigger_Tables_Assigned_Resources";
    public override int Step => 4200; 
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "CleanoutTriggerTableResources";
    public CleanoutTriggerTableResourcesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("CleanoutTriggerTableResources", "DepartmentExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("CleanoutTriggerTableResources", "PlantExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("CleanoutTriggerTableResources", "ResourceExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("CleanoutTriggerTableResources", "TableName", EPropertyDataType.String, true),
        };
    }
}
public class CleanoutTriggerTablesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "CleanoutTriggerTables";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Cleanout_Trigger_Tables";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Cleanouts };
    public override int Step => 4100;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "CleanoutTriggerTables";
    public CleanoutTriggerTablesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("CleanoutTriggerTables", "TableName", EPropertyDataType.String, true),
            new IntegrationProperty("CleanoutTriggerTables", "Description", EPropertyDataType.String, false),
        };
    }
}
public class CleanoutTriggerTableTimeFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "CleanoutTriggerTableTime";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Time_Cleanout_Triggers_Table";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Cleanouts };
    public override int Step => 4300;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "CleanoutTriggerTableTime";
    public CleanoutTriggerTableTimeFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("CleanoutTriggerTableTime", "TableName", EPropertyDataType.String, true),
            new IntegrationProperty("CleanoutTriggerTableTime", "TriggerValueHours", EPropertyDataType.Double, true),
            new IntegrationProperty("CleanoutTriggerTableTime", "DurationHours", EPropertyDataType.Double, true),
            new IntegrationProperty("CleanoutTriggerTableTime", "CleanCost", EPropertyDataType.Decimal, false),
            new IntegrationProperty("CleanoutTriggerTableTime", "CleanoutGrade", EPropertyDataType.Int, false),
            new IntegrationProperty("CleanoutTriggerTableTime", "TriggerAtEnd", EPropertyDataType.Boolean, false),
            new IntegrationProperty("CleanoutTriggerTableTime", "UsePostProcessingTime", EPropertyDataType.Boolean, false),
            new IntegrationProperty("CleanoutTriggerTableTime", "UseProcessingTime", EPropertyDataType.Boolean, false),
        };
    }
}
public class CompatibilityCodesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "CompatibilityCodes";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Compatibility_Code_Tables";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Compatibilities };
    public override int Step => 4600;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "CompatibilityCodes";
    public CompatibilityCodesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("CompatibilityCodes", "TableName", EPropertyDataType.String, true),
            new IntegrationProperty("CompatibilityCodes", "CompatibilityCode", EPropertyDataType.String, true),
        };
    }
}
public class CompatibilityCodeTableResourcesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "CompatibilityCodeTableResources";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Compatibility_Code_Tables_Assigned_Resources";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Compatibilities };
    public override int Step => 4700;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "CompatibilityCodeTableResources";
    public CompatibilityCodeTableResourcesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("CompatibilityCodeTableResources", "DepartmentExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("CompatibilityCodeTableResources", "PlantExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("CompatibilityCodeTableResources", "ResourceExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("CompatibilityCodeTableResources", "TableName", EPropertyDataType.String, true),
        };
    }
}
public class CompatibilityCodeTablesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "CompatibilityCodeTables";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Compatibility_Code_Tables";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Compatibilities };
    public override int Step => 4800;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "CompatibilityCodeTables";
    public CompatibilityCodeTablesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("CompatibilityCodeTables", "TableName", EPropertyDataType.String, true),
            new IntegrationProperty("CompatibilityCodeTables", "AllowedList", EPropertyDataType.Boolean, true),
            new IntegrationProperty("CompatibilityCodeTables", "Description", EPropertyDataType.String, false),
        };
    }
}
public class CustomersFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Customers";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Customer";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Resources };
    public override int Step => 4900;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "Customers";
    public CustomersFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Customers", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Customers", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("Customers", "AbcCode", EPropertyDataType.String, false),
            new IntegrationProperty("Customers", "ColorCode", EPropertyDataType.String, false),
            new IntegrationProperty("Customers", "CustomerType", EPropertyDataType.String, false),
            new IntegrationProperty("Customers", "Description", EPropertyDataType.String, false),
            new IntegrationProperty("Customers", "GroupCode", EPropertyDataType.String, false),
            new IntegrationProperty("Customers", "Priority", EPropertyDataType.Int, false),
            new IntegrationProperty("Customers", "Region", EPropertyDataType.String, false),
            new IntegrationProperty("Customers", "UserFields", EPropertyDataType.String, false),
        };
    }
}
public class CustomerConnectionsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "CustomerConnections";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_CustomerConnection";
    public override List<ObjectType> ObjectTypes => new() { ObjectType.Resources };
    public override int Step => 4910;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "CustomerConnections";
    public CustomerConnectionsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("CustomerConnections", "CustomerExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("CustomerConnections", "JobExternalId", EPropertyDataType.String, false),

        };
    }
}

public class DepartmentsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Departments";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Department";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Resources };
    public override int Step => 1100;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public override string Group => "Departments";
    public DepartmentsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Departments", "DepartmentFrozenSpanHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("Departments", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Departments", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("Departments", "PlantExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Departments", "Description", EPropertyDataType.String, false),
        };
    }
}
public class ForecastsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Forecasts";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Forecast";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Items };
    public override int Step => 5100;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "Forecasts";
    public ForecastsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Forecasts", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Forecasts", "ItemExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Forecasts", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("Forecasts", "WarehouseExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Forecasts", "ForecastVersion", EPropertyDataType.String, true),
            new IntegrationProperty("Forecasts", "Description", EPropertyDataType.String, false),
            new IntegrationProperty("Forecasts", "CustomerExternalId", EPropertyDataType.String, false),
        };
    }
}
public class ForecastShipmentsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "ForecastShipments";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Forecast_Shipment";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Items };
    public override int Step => 5200;
    public override bool IsBaseTableFeature => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override bool CanSetAutoDelete => true;
    public override string Group => "ForecastShipments";
    public ForecastShipmentsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("ForecastShipments", "ForecastExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ForecastShipments", "RequiredDate", EPropertyDataType.DateTime, true),
            new IntegrationProperty("ForecastShipments", "RequiredQty", EPropertyDataType.Decimal, true),
            new IntegrationProperty("ForecastShipments", "WarehouseExternalId", EPropertyDataType.String, false),
        };
    }
}
public class InventoriesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Inventories";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Inventory";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Items };
    public override int Step => 2100;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public override string Group => "Inventories";
    public InventoriesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Inventories", "ItemExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Inventories", "WarehouseExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Inventories", "StorageAreaExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("Inventories", "LeadTimeDays", EPropertyDataType.Double, false),
            new IntegrationProperty("Inventories", "ForecastConsumptionWindowDays", EPropertyDataType.Double, false),
            new IntegrationProperty("Inventories", "ForecastInterval", EPropertyDataType.String, false),
            new IntegrationProperty("Inventories", "MaterialAllocation", EPropertyDataType.String, false),
            new IntegrationProperty("Inventories", "MaxInventory", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Inventories", "MrpExcessQuantityAllocation", EPropertyDataType.String, false),
            new IntegrationProperty("Inventories", "NumberOfIntervalsToForecast", EPropertyDataType.Int, false),
            new IntegrationProperty("Inventories", "PlannerExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("Inventories", "PreventSharedBatchOverflow", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Inventories", "SafetyStock", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Inventories", "SafetyStockWarningLevel", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Inventories", "StorageCapacity", EPropertyDataType.Decimal, false),
        };
    }
}
public class ItemsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Items";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Item";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Items };
    public override int Step => 1800;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public override string Group => "Items";
    public ItemsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Items", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Items", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("Items", "ItemType", EPropertyDataType.String, false),
            new IntegrationProperty("Items", "DefaultLeadTimeDays", EPropertyDataType.Double, false),
            new IntegrationProperty("Items", "Description", EPropertyDataType.String, false),
            new IntegrationProperty("Items", "ItemGroup", EPropertyDataType.String, false),
            new IntegrationProperty("Items", "Source", EPropertyDataType.String, false),
            new IntegrationProperty("Items", "MaxOrderQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Items", "PlanInventory", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Items", "RollupAttributesToParent", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Items", "UnitVolume", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Items", "TransferQty", EPropertyDataType.Decimal, false),
        };
    }
}
public class JobActivitiesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "JobActivities";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Internal_Activity";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Jobs };
    public override int Step => 5300;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "JobActivities";
    public JobActivitiesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("JobActivities", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobActivities", "JobExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobActivities", "MoExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobActivities", "OpExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobActivities", "RequiredFinishQty", EPropertyDataType.Decimal, true),
            new IntegrationProperty("JobActivities", "ProductionStatus", EPropertyDataType.String, false),
            new IntegrationProperty("JobActivities", "SetupHrs", EPropertyDataType.Double, true),
            new IntegrationProperty("JobActivities", "ActualResourcesUsed", EPropertyDataType.String, false),
            new IntegrationProperty("JobActivities", "Anchor", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobActivities", "AnchorStartDate", EPropertyDataType.DateTime, false),
            new IntegrationProperty("JobActivities", "BatchAmount", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobActivities", "CleanHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobActivities", "CleanOutGrade", EPropertyDataType.Int, false),
            new IntegrationProperty("JobActivities", "CleanTimeManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobActivities", "CleanTimeOverride", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobActivities", "Comments", EPropertyDataType.String, false),
            new IntegrationProperty("JobActivities", "Comments2", EPropertyDataType.String, false),
            new IntegrationProperty("JobActivities", "CycleHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobActivities", "CycleSpanManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobActivities", "CycleSpanOverride", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobActivities", "Paused", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobActivities", "PostProcessingHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobActivities", "PostProcessManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobActivities", "PostProcessOverride", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobActivities", "QtyPerCycle", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobActivities", "QtyPerCycleManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobActivities", "QtyPerCycleOverride", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobActivities", "ReportedCleanHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobActivities", "ReportedCleanoutGrade", EPropertyDataType.Int, false),
            new IntegrationProperty("JobActivities", "ReportedStorageHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobActivities", "ReportedEndOfPostProcessingHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobActivities", "ReportedEndOfStorageHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobActivities", "ScrapPercentOverride", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobActivities", "SetupTimeOverride", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobActivities", "StorageHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobActivities", "StorageManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobActivities", "StorageHrsOverride", EPropertyDataType.Boolean, false),
        };
    }
}
public class JobMaterialsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "JobMaterials";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Material_Requirement";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Jobs };
    public override int Step => 5400;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "JobMaterials";
    public JobMaterialsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("JobMaterials", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobMaterials", "ItemExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobMaterials", "JobExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobMaterials", "MoExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobMaterials", "OpExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobMaterials", "WarehouseExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobMaterials", "TotalRequiredQty", EPropertyDataType.Decimal, true),
            new IntegrationProperty("JobMaterials", "IssuedQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobMaterials", "ConstraintType", EPropertyDataType.String, false),
            new IntegrationProperty("JobMaterials", "AllowedLotCodes", EPropertyDataType.String, false),
            new IntegrationProperty("JobMaterials", "AllowPartialSupply", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobMaterials", "Available", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobMaterials", "FixedQty", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobMaterials", "LatestSourceDateTime", EPropertyDataType.DateTime, false),
            new IntegrationProperty("JobMaterials", "LeadTimeHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobMaterials", "MaterialAllocation", EPropertyDataType.String, false),
            new IntegrationProperty("JobMaterials", "MaterialDescription", EPropertyDataType.String, false),
            new IntegrationProperty("JobMaterials", "MaterialName", EPropertyDataType.String, false),
            new IntegrationProperty("JobMaterials", "MaterialSourcing", EPropertyDataType.String, false),
            new IntegrationProperty("JobMaterials", "MaxSourceQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobMaterials", "MinAgeHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobMaterials", "MinSourceQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobMaterials", "MultipleWarehouseSupplyAllowed", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobMaterials", "PlannedScrapQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobMaterials", "ProductRelease", EPropertyDataType.String, false),
            new IntegrationProperty("JobMaterials", "RequirementType", EPropertyDataType.String, false),
            new IntegrationProperty("JobMaterials", "Source", EPropertyDataType.String, false),
            new IntegrationProperty("JobMaterials", "UOM", EPropertyDataType.String, false),
            new IntegrationProperty("JobMaterials", "StorageAreaExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("JobMaterials", "MaterialUsedTiming", EPropertyDataType.String, false),
            new IntegrationProperty("JobMaterials", "MultipleStorageAreaSupplyAllowed", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobMaterials", "RequireFirstTransferAtSetup", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobMaterials", "AllowExpiredSupply", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobMaterials", "ShelfLifeWarning", EPropertyDataType.Boolean, false),
        };
    }
}
public class JobOperationAttributesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "JobOperationAttributes";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Operation_Attribute";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Jobs };
    public override int Step => 5500;
    public override bool IsBaseTableFeature => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override bool CanSetAutoDelete => true;
    public override string Group => "JobOperationAttributes";
    public JobOperationAttributesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("JobOperationAttributes", "JobExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobOperationAttributes", "MoExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobOperationAttributes", "OpExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobOperationAttributes", "AttributeExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobOperationAttributes", "CodeManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperationAttributes", "ColorCodeManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperationAttributes", "ColorOverride", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperationAttributes", "Cost", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobOperationAttributes", "CostManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperationAttributes", "CostOverride", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperationAttributes", "DurationHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobOperationAttributes", "DurationHrsManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperationAttributes", "DurationOverride", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperationAttributes", "NumberManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperationAttributes", "ShowInGantt", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperationAttributes", "ShowInGanttOverride", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperationAttributes", "ShowInGanttManualUpdateOnly", EPropertyDataType.Boolean, false),
        };
    }
}
public class JobOperationsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "JobOperations";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Resource_Operation";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Jobs };
    public override int Step => 2400;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public override string Group => "JobOperations";
    public JobOperationsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("JobOperations", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobOperations", "JobExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobOperations", "MoExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobOperations", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("JobOperations", "RequiredFinishQty", EPropertyDataType.Decimal, true),
            new IntegrationProperty("JobOperations", "CycleHrs", EPropertyDataType.Double, true),
            new IntegrationProperty("JobOperations", "QtyPerCycle", EPropertyDataType.Decimal, true),
            new IntegrationProperty("JobOperations", "AutoCreatedCapabilityExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("JobOperations", "AutoCreateRequirements", EPropertyDataType.String, false),
            new IntegrationProperty("JobOperations", "AutoFinish", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "AutoReportProgress", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "AutoSplitType", EPropertyDataType.String, false),
            new IntegrationProperty("JobOperations", "CanPause", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "CanResize", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "CanSubcontract", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "CleanHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobOperations", "CleanOutGrade", EPropertyDataType.Int, false),
            new IntegrationProperty("JobOperations", "CleanTimeManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "CycleSpanManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "Description", EPropertyDataType.String, false),
            new IntegrationProperty("JobOperations", "HoldReason", EPropertyDataType.String, false),
            new IntegrationProperty("JobOperations", "HoldUntilDateTime", EPropertyDataType.DateTime, false),
            new IntegrationProperty("JobOperations", "MaterialsManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "MaxAutoSplitAmount", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobOperations", "MinAutoSplitAmount", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobOperations", "Omitted", EPropertyDataType.String, false),
            new IntegrationProperty("JobOperations", "OnHold", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "OperationSequence", EPropertyDataType.Long, false),
            new IntegrationProperty("JobOperations", "OutputName", EPropertyDataType.String, false),
            new IntegrationProperty("JobOperations", "PlannedScrapQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobOperations", "PostProcessingHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobOperations", "PostProcessManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "PreventSplitsFromIncurringClean", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "PreventSplitsFromIncurringSetup", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "ProductCode", EPropertyDataType.String, false),
            new IntegrationProperty("JobOperations", "ProductsManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "QtyPerCycleManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "ResourceRequirementsManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "SetupColor", EPropertyDataType.String, false),
            new IntegrationProperty("JobOperations", "SetupHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobOperations", "SetupSplitType", EPropertyDataType.String, false),
            new IntegrationProperty("JobOperations", "TimeBasedReporting", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "UOM", EPropertyDataType.String, false),
            new IntegrationProperty("JobOperations", "UseExpectedFinishQty", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "StorageHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobOperations", "StorageManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "CampaignCode", EPropertyDataType.String, false),
        };
    }
}

public class JobPathsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "JobPaths";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Alternate_Path";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Jobs };
    public override int Step => 5600;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "JobPaths";
    public JobPathsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("JobPaths", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobPaths", "JobExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobPaths", "MoExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobPaths", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("JobPaths", "ValidityEndDate", EPropertyDataType.DateTime, false),
            new IntegrationProperty("JobPaths", "ValidityStartDate", EPropertyDataType.DateTime, false),
        };
    }
}

public class JobPathNodesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "JobPathNodes";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Alternate_Path";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Jobs };
    public override int Step => 5700;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "JobPathNodes";
    public JobPathNodesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("JobPathNodes", "JobExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobPathNodes", "MoExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobPathNodes", "PathExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobPathNodes", "PredecessorOperationExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobPathNodes", "SuccessorOperationExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("JobPathNodes", "TransferDuringPredeccessorOnlineTime", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobPathNodes", "TransferEnd", EPropertyDataType.String, false),
            new IntegrationProperty("JobPathNodes", "TransferStart", EPropertyDataType.String, false),
        };
    }
}
public class JobProductsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "JobProducts";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Product";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Jobs };
    public override int Step => 2700;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public override string Group => "JobProducts";
    public JobProductsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("JobProducts", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobProducts", "ItemExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobProducts", "JobExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobProducts", "MoExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobProducts", "OpExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobProducts", "WarehouseExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobProducts", "TotalOutputQty", EPropertyDataType.Decimal, true),
            new IntegrationProperty("JobProducts", "CompletedQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobProducts", "FixedQty", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobProducts", "InventoryAvailableTiming", EPropertyDataType.String, false),
            new IntegrationProperty("JobProducts", "LimitMatlSrcToEligibleLots", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobProducts", "LotCode", EPropertyDataType.String, false),
            new IntegrationProperty("JobProducts", "MaterialPostProcessingHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobProducts", "UnitVolumeOverride", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobProducts", "UseLimitMatlSrcToEligibleLots", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobProducts", "StorageAreaExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("JobProducts", "RequireEmptyStorageArea", EPropertyDataType.Boolean, false),
        };
    }
}
public class JobResourceCapabilitiesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "JobResourceCapabilities";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Required_Capabilities";
    public override List<ObjectType> ObjectTypes => new() { ObjectType.Jobs };
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override int Step => 2600;
    public override string Category => IntegrationConstants.CoreCategory;
    public override string Group => "JobResourceCapabilities";
    public JobResourceCapabilitiesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("JobResourceCapabilities", "JobExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobResourceCapabilities", "MoExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobResourceCapabilities", "OpExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobResourceCapabilities", "CapabilityExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobResourceCapabilities", "ResourceRequirementExternalId", EPropertyDataType.String, true),
        };
    }
}
public class JobResourcesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "JobResources";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Resource_Requirement";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Jobs };
    public override int Step => 2500;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public override string Group => "JobResources";
    public JobResourcesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("JobResources", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobResources", "JobExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobResources", "MoExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobResources", "OpExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobResources", "DefaultResourceDepartmentExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("JobResources", "DefaultResourceExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("JobResources", "DefaultResourcePlantExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("JobResources", "CapacityCode", EPropertyDataType.String, false),
            new IntegrationProperty("JobResources", "Description", EPropertyDataType.String, false),
            new IntegrationProperty("JobResources", "UsageEnd", EPropertyDataType.String, false),
            new IntegrationProperty("JobResources", "UsageStart", EPropertyDataType.String, false),
            new IntegrationProperty("JobResources", "UseDefaultResourceJITLimit", EPropertyDataType.Boolean, false),
        };
    }
}
public class JobsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Jobs";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Job";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Jobs };
    public override int Step => 2200;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public override string Group => "Jobs";
    public JobsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Jobs", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Jobs", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("Jobs", "AlmostLateDays", EPropertyDataType.Double, false),
            new IntegrationProperty("Jobs", "Description", EPropertyDataType.String, false),
            new IntegrationProperty("Jobs", "Hot", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Jobs", "MaxEarlyDeliveryDays", EPropertyDataType.Double, false),
            new IntegrationProperty("Jobs", "NeedDateTime", EPropertyDataType.DateTime, false),
            new IntegrationProperty("Jobs", "OrderNumber", EPropertyDataType.String, false),
            new IntegrationProperty("Jobs", "AgentEmail", EPropertyDataType.String, false),
            new IntegrationProperty("Jobs", "Cancelled", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Jobs", "Classification", EPropertyDataType.String, false),
            new IntegrationProperty("Jobs", "ColorCode", EPropertyDataType.String, false),
            new IntegrationProperty("Jobs", "Commitment", EPropertyDataType.String, false),
            new IntegrationProperty("Jobs", "CustomerEmail", EPropertyDataType.String, false),
            new IntegrationProperty("Jobs", "Destination", EPropertyDataType.String, false),
            new IntegrationProperty("Jobs", "DoNotDelete", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Jobs", "DoNotSchedule", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Jobs", "Hold", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Jobs", "HoldReason", EPropertyDataType.String, false),
            new IntegrationProperty("Jobs", "HoldUntilDate", EPropertyDataType.DateTime, false),
            new IntegrationProperty("Jobs", "HotReason", EPropertyDataType.String, false),
            new IntegrationProperty("Jobs", "Importance", EPropertyDataType.Int, false),
            new IntegrationProperty("Jobs", "Invoiced", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Jobs", "MarkForDeletion", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Jobs", "OldExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("Jobs", "Printed", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Jobs", "Reviewed", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Jobs", "Shipped", EPropertyDataType.Int, false),
            new IntegrationProperty("Jobs", "Type", EPropertyDataType.String, false),
        };
    }
}
public class LotsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Lots";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Lots";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Items };
    public override int Step => 6000;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "Lots";
    public LotsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Lots", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Lots", "WarehouseExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Lots", "ItemExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Lots", "Code", EPropertyDataType.String, false),
            new IntegrationProperty("Lots", "LimitMatlSrcToEligibleLots", EPropertyDataType.Boolean, false),
        };
    }
}
public class ManufacturingOrdersFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "ManufacturingOrders";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Manufacturing_Order";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Jobs };
    public override int Step => 2300;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public override string Group => "ManufacturingOrders";
    public ManufacturingOrdersFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("ManufacturingOrders", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ManufacturingOrders", "JobExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ManufacturingOrders", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("ManufacturingOrders", "RequiredQty", EPropertyDataType.Decimal, true),
            new IntegrationProperty("ManufacturingOrders", "ProductDescription", EPropertyDataType.String, false),
            new IntegrationProperty("ManufacturingOrders", "ProductName", EPropertyDataType.String, false),
            new IntegrationProperty("ManufacturingOrders", "DefaultPathExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("ManufacturingOrders", "Description", EPropertyDataType.String, false),
            new IntegrationProperty("ManufacturingOrders", "ExpectedFinishQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("ManufacturingOrders", "Family", EPropertyDataType.String, false),
            new IntegrationProperty("ManufacturingOrders", "Hold", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ManufacturingOrders", "HoldReason", EPropertyDataType.String, false),
            new IntegrationProperty("ManufacturingOrders", "HoldUntilDate", EPropertyDataType.DateTime, false),
            new IntegrationProperty("ManufacturingOrders", "IsReleased", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ManufacturingOrders", "MoNeedDate", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ManufacturingOrders", "NeedDate", EPropertyDataType.DateTime, false),
            new IntegrationProperty("ManufacturingOrders", "PreserveRequiredQty", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ManufacturingOrders", "ProductColor", EPropertyDataType.String, false),
            new IntegrationProperty("ManufacturingOrders", "ReleaseDateTime", EPropertyDataType.DateTime, false),
            new IntegrationProperty("ManufacturingOrders", "UOM", EPropertyDataType.String, false),
            new IntegrationProperty("ManufacturingOrders", "ResizeForStorage", EPropertyDataType.Boolean, false),
        };
    }
}
public class PlantsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Plants";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Plant";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Resources };
    public override int Step => 1000;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public override string Group => "Plants";
    public PlantsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Plants", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Plants", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("Plants", "AnnualPercentageRate", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Plants", "Description", EPropertyDataType.String, false),
            new IntegrationProperty("Plants", "HeavyLoadThreshold", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Plants", "StableSpanHrs", EPropertyDataType.Double, false),
        };
    }
}
public class PlantWarehousesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "PlantWarehouses";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Plant_Warehouse";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Items };
    public override int Step => 2000;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public override string Group => "PlantWarehouses";
    public PlantWarehousesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("PlantWarehouses", "PlantExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("PlantWarehouses", "WarehouseExternalId", EPropertyDataType.String, true),
        };
    }
}
public class ProductRulesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "ProductRules";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Product_Rule";
    public override int Step => 6100;
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Resources };
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "ProductRules";
    public ProductRulesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("ProductRules", "DepartmentExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ProductRules", "PlantExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ProductRules", "ProductItemExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ProductRules", "ResourceExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ProductRules", "CleanHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("ProductRules", "CleanoutUnitsRatio", EPropertyDataType.Decimal, false),
            new IntegrationProperty("ProductRules", "MaterialPostProcessingHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("ProductRules", "MaxVolume", EPropertyDataType.Decimal, false),
            new IntegrationProperty("ProductRules", "MinQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("ProductRules", "MinVolume", EPropertyDataType.Decimal, false),
            new IntegrationProperty("ProductRules", "ProductCode", EPropertyDataType.String, false),
            new IntegrationProperty("ProductRules", "Priority", EPropertyDataType.Int, false),
            new IntegrationProperty("ProductRules", "TransferQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("ProductRules", "UseCleanHrs", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ProductRules", "UseCleanoutUnits", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ProductRules", "UseMaterialPostProcessingSpan", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ProductRules", "UseMaxQty", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ProductRules", "UseMaxVolume", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ProductRules", "UseMinQty", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ProductRules", "UseMinVolume", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ProductRules", "UsePriority", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ProductRules", "UserFields", EPropertyDataType.String, false),
            new IntegrationProperty("ProductRules", "UseTransferQty", EPropertyDataType.Boolean, false),
        };
    }
}
public class PurchasesToStockFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "PurchasesToStock";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Purchase_To_Stock";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Items };
    public override int Step => 6200;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "PurchasesToStock";
    public PurchasesToStockFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("PurchasesToStock", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("PurchasesToStock", "ItemExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("PurchasesToStock", "WarehouseExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("PurchasesToStock", "StorageAreaExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("PurchasesToStock", "VendorExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("PurchasesToStock", "QtyOrdered", EPropertyDataType.Decimal, true),
            new IntegrationProperty("PurchasesToStock", "ScheduledReceiptDate", EPropertyDataType.DateTime, true),
            new IntegrationProperty("PurchasesToStock", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("PurchasesToStock", "Description", EPropertyDataType.String, false),
            new IntegrationProperty("PurchasesToStock", "ActualReceiptDate", EPropertyDataType.DateTime, false),
            new IntegrationProperty("PurchasesToStock", "BuyerExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("PurchasesToStock", "LimitMatlSrcToEligibleLots", EPropertyDataType.Boolean, false),
            new IntegrationProperty("PurchasesToStock", "LotCode", EPropertyDataType.String, false),
            new IntegrationProperty("PurchasesToStock", "QtyReceived", EPropertyDataType.Decimal, false),
            new IntegrationProperty("PurchasesToStock", "TransferHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("PurchasesToStock", "UnloadHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("PurchasesToStock", "UseLimitMatlSrcToEligibleLots", EPropertyDataType.Boolean, false),
            new IntegrationProperty("PurchasesToStock", "OverrideStorageConstraint", EPropertyDataType.Boolean, false),
            new IntegrationProperty("PurchasesToStock", "RequireEmptyStorageArea", EPropertyDataType.Boolean, false),
        };
    }
}
public class RecurringCapacityIntervalsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "RecurringCapacityIntervals";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Recurring_Capacity_Interval";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Resources };
    public override int Step => 1600;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public override string Group => "RecurringCapacityIntervals";
    public RecurringCapacityIntervalsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("RecurringCapacityIntervals", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("RecurringCapacityIntervals", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("RecurringCapacityIntervals", "IntervalType", EPropertyDataType.String, true),
            new IntegrationProperty("RecurringCapacityIntervals", "Recurrence", EPropertyDataType.String, true),
            new IntegrationProperty("RecurringCapacityIntervals", "RecurrenceEndType", EPropertyDataType.String, true),
            new IntegrationProperty("RecurringCapacityIntervals", "EndDateTime", EPropertyDataType.DateTime, true),
            new IntegrationProperty("RecurringCapacityIntervals", "CanStartActivity", EPropertyDataType.Boolean, false),
            new IntegrationProperty("RecurringCapacityIntervals", "CapacityCode", EPropertyDataType.String, false),
            new IntegrationProperty("RecurringCapacityIntervals", "ResetAttributeChangeovers", EPropertyDataType.Boolean, false),
            new IntegrationProperty("RecurringCapacityIntervals", "Color", EPropertyDataType.String, false),
            new IntegrationProperty("RecurringCapacityIntervals", "Description", EPropertyDataType.String, false),
            new IntegrationProperty("RecurringCapacityIntervals", "Friday", EPropertyDataType.Boolean, false),
            new IntegrationProperty("RecurringCapacityIntervals", "IntervalPreset", EPropertyDataType.String, false),
            new IntegrationProperty("RecurringCapacityIntervals", "MaxNbrRecurrences", EPropertyDataType.Int, false),
            new IntegrationProperty("RecurringCapacityIntervals", "Monday", EPropertyDataType.Boolean, false),
            new IntegrationProperty("RecurringCapacityIntervals", "MonthlyDayNumber", EPropertyDataType.Int, false),
            new IntegrationProperty("RecurringCapacityIntervals", "NbrIntervalsToOverride", EPropertyDataType.Int, false),
            new IntegrationProperty("RecurringCapacityIntervals", "NbrOfPeople", EPropertyDataType.Decimal, false),
            new IntegrationProperty("RecurringCapacityIntervals", "NbrOfPeopleOverride", EPropertyDataType.Decimal, false),
            new IntegrationProperty("RecurringCapacityIntervals", "Overtime", EPropertyDataType.Boolean, false),
            new IntegrationProperty("RecurringCapacityIntervals", "PreventOperationsFromSpanning", EPropertyDataType.Boolean, false),
            new IntegrationProperty("RecurringCapacityIntervals", "RecurrenceEndDateTime", EPropertyDataType.DateTime, false),
            new IntegrationProperty("RecurringCapacityIntervals", "Saturday", EPropertyDataType.Boolean, false),
            new IntegrationProperty("RecurringCapacityIntervals", "SkipFrequency", EPropertyDataType.Int, false),
            new IntegrationProperty("RecurringCapacityIntervals", "StartDateTime", EPropertyDataType.DateTime, true),
            new IntegrationProperty("RecurringCapacityIntervals", "Sunday", EPropertyDataType.Boolean, false),
            new IntegrationProperty("RecurringCapacityIntervals", "Thursday", EPropertyDataType.Boolean, false),
            new IntegrationProperty("RecurringCapacityIntervals", "Tuesday", EPropertyDataType.Boolean, false),
            new IntegrationProperty("RecurringCapacityIntervals", "UsedForClean", EPropertyDataType.Boolean, false),
            new IntegrationProperty("RecurringCapacityIntervals", "UsedForPostProcessing", EPropertyDataType.Boolean, false),
            new IntegrationProperty("RecurringCapacityIntervals", "UsedForRun", EPropertyDataType.Boolean, false),
            new IntegrationProperty("RecurringCapacityIntervals", "UsedForSetup", EPropertyDataType.Boolean, false),
            new IntegrationProperty("RecurringCapacityIntervals", "UsedForStoragePostProcessing", EPropertyDataType.Boolean, false),
            new IntegrationProperty("RecurringCapacityIntervals", "UseOnlyWhenLate", EPropertyDataType.Boolean, false),
            new IntegrationProperty("RecurringCapacityIntervals", "Wednesday", EPropertyDataType.Boolean, false),
            new IntegrationProperty("RecurringCapacityIntervals", "CanBeDraggedAndResized", EPropertyDataType.Boolean, false),
            new IntegrationProperty("RecurringCapacityIntervals", "CanBeDeleted", EPropertyDataType.Boolean, false),
        };
    }
}

public class ResourceCapabilitiesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "ResourceCapabilities";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Required_Capabilities";
    public override List<ObjectType> ObjectTypes => new() { ObjectType.Resources };
    public override int Step => 1400;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public override string Group => "ResourceCapabilities";
    public ResourceCapabilitiesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("ResourceCapabilities", "CapabilityExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ResourceCapabilities", "DepartmentExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ResourceCapabilities", "PlantExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ResourceCapabilities", "ResourceExternalId", EPropertyDataType.String, true),
        };
    }
}

public class ResourceConnectionsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "ResourceConnections";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_ResourceConnections";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Resources };
    public override int Step => 6300;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "ResourceConnections";
    public ResourceConnectionsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("ResourceConnections", "DepartmentExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ResourceConnections", "PlantExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ResourceConnections", "ResourceConnectorExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ResourceConnections", "ResourceExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ResourceConnections", "ConnectionDirection", EPropertyDataType.String, true),
        };
    }
}
public class ResourceConnectorsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "ResourceConnectors";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_ResourceConnectors";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Resources };
    public override int Step => 6400; 
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "ResourceConnectors";
    public ResourceConnectorsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("ResourceConnectors", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ResourceConnectors", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("ResourceConnectors", "Description", EPropertyDataType.String, false),
            new IntegrationProperty("ResourceConnectors", "TransitHours", EPropertyDataType.Double, false),
            new IntegrationProperty("ResourceConnectors", "UserFields", EPropertyDataType.String, false),
        };
    }
}
public class ResourcesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Resources";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Resource";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Resources };
    public override int Step => 1200;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public override string Group => "Resources";
    public ResourcesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Resources", "DepartmentExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Resources", "Description", EPropertyDataType.String, false),
            new IntegrationProperty("Resources", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Resources", "HeadStartHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("Resources", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("Resources", "PlantExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Resources", "ResourceType", EPropertyDataType.String, false),
            new IntegrationProperty("Resources", "Workcenter", EPropertyDataType.String, false),
            new IntegrationProperty("Resources", "Active", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Resources", "ActivitySetupEfficiencyMultiplier", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Resources", "CellExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("Resources", "ChangeoverSetupEfficiencyMultiplier", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Resources", "DisallowDragAndDrops", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Resources", "ExcludeFromGantts", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Resources", "ExperimentalSequencingPlan", EPropertyDataType.String, false),
            new IntegrationProperty("Resources", "ExperimentalSequencingPlanFour", EPropertyDataType.String, false),
            new IntegrationProperty("Resources", "ExperimentalSequencingPlanThree", EPropertyDataType.String, false),
            new IntegrationProperty("Resources", "ExperimentalSequencingPlanTwo", EPropertyDataType.String, false),
            new IntegrationProperty("Resources", "GanttRowHeightFactor", EPropertyDataType.Int, false),
            new IntegrationProperty("Resources", "ImageFileName", EPropertyDataType.String, false),
            new IntegrationProperty("Resources", "ManualAssignmentOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Resources", "MaxVolume", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Resources", "MinVolume", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Resources", "NoDefaultRecurringCapacityInterval", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Resources", "NormalSequencingPlan", EPropertyDataType.String, false),
            new IntegrationProperty("Resources", "Priority", EPropertyDataType.Int, false),
            new IntegrationProperty("Resources", "ScheduledRunSpanAlgorithm", EPropertyDataType.String, false),
            new IntegrationProperty("Resources", "ScheduledTransferSpanAlgorithm", EPropertyDataType.String, false),
            new IntegrationProperty("Resources", "Sequential", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Resources", "StandardCleanHours", EPropertyDataType.Double, false),
            new IntegrationProperty("Resources", "StandardCleanoutGrade", EPropertyDataType.Int, false),
            new IntegrationProperty("Resources", "UseAttributeCleanouts", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Resources", "UseOperationCleanout", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Resources", "WorkcenterExternalId", EPropertyDataType.String, false),
        };
    }
}
public class SalesOrderLineDistributionsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "SalesOrderLineDistributions";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Sales_Order_Line_Distribution";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Items };
    public override int Step => 6600;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "SalesOrderLineDistributions";
    public SalesOrderLineDistributionsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("SalesOrderLineDistributions", "SalesOrderExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("SalesOrderLineDistributions", "LineNumber", EPropertyDataType.String, true),
            new IntegrationProperty("SalesOrderLineDistributions", "QtyOrdered", EPropertyDataType.Decimal, true),
            new IntegrationProperty("SalesOrderLineDistributions", "RequiredAvailableDate", EPropertyDataType.DateTime, true),
            new IntegrationProperty("SalesOrderLineDistributions", "AllowedLotCodes", EPropertyDataType.String, false),
            new IntegrationProperty("SalesOrderLineDistributions", "MaterialAllocation", EPropertyDataType.String, false),
            new IntegrationProperty("SalesOrderLineDistributions", "MaterialSourcing", EPropertyDataType.String, false),
            new IntegrationProperty("SalesOrderLineDistributions", "MaxSourceQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("SalesOrderLineDistributions", "MinSourceQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("SalesOrderLineDistributions", "UseMustSupplyFromWarehouseExternalId", EPropertyDataType.Boolean, false),
        };
    }
}
public class SalesOrderLinesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "SalesOrderLines";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Sales_Order_Line";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Items };
    public override int Step => 6700;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "SalesOrderLines";
    public SalesOrderLinesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("SalesOrderLines", "SalesOrderExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("SalesOrderLines", "ItemExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("SalesOrderLines", "Description", EPropertyDataType.String, true),
        };
    }
}
public class SalesOrdersFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "SalesOrders";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Sales_Order";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Items };
    public override int Step => 6500;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "SalesOrders";
    public SalesOrdersFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("SalesOrders", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("SalesOrders", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("SalesOrders", "Description", EPropertyDataType.String, false),
            new IntegrationProperty("SalesOrders", "CustomerExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("SalesOrders", "Notes", EPropertyDataType.String, false),
        };
    }
}
public class TransferOrderDistributionsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "TransferOrderDistributions";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Transfer_Order_Distribution";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Items };
    public override int Step => 6900;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "TransferOrderDistributions";
    public TransferOrderDistributionsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("TransferOrderDistributions", "ExternalID", EPropertyDataType.String, true),
            new IntegrationProperty("TransferOrderDistributions", "FromWarehouseExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("TransferOrderDistributions", "ItemExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("TransferOrderDistributions", "QtyOrdered", EPropertyDataType.Decimal, true),
            new IntegrationProperty("TransferOrderDistributions", "QtyReceived", EPropertyDataType.Decimal, true),
            new IntegrationProperty("TransferOrderDistributions", "QtyShipped", EPropertyDataType.Decimal, true),
            new IntegrationProperty("TransferOrderDistributions", "ScheduledReceiveDate", EPropertyDataType.DateTime, true),
            new IntegrationProperty("TransferOrderDistributions", "ScheduledShipDate", EPropertyDataType.DateTime, true),
            new IntegrationProperty("TransferOrderDistributions", "ToWarehouseExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("TransferOrderDistributions", "TransferOrderExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("TransferOrderDistributions", "Closed", EPropertyDataType.Boolean, true),
            new IntegrationProperty("TransferOrderDistributions", "MaterialAllocation", EPropertyDataType.String, false),
            new IntegrationProperty("TransferOrderDistributions", "MaterialSourcing", EPropertyDataType.String, false),
            new IntegrationProperty("TransferOrderDistributions", "MaxSourceQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("TransferOrderDistributions", "MinSourceQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("TransferOrderDistributions", "PreferEmptyStorageArea", EPropertyDataType.Boolean, false),
            new IntegrationProperty("TransferOrderDistributions", "OverrideStorageConstraint", EPropertyDataType.Boolean, false),
            new IntegrationProperty("TransferOrderDistributions", "AllowPartialAllocations", EPropertyDataType.Boolean, false),
        };
    }
}
public class UserFieldDefinitionsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "UserFieldDefinitions";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_UserFields";
    public override List<ObjectType> ObjectTypes => new();
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override string Group => "UserFieldDefinitions";
    public UserFieldDefinitionsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("UserFieldDefinitions", "DefaultValue", EPropertyDataType.String, false),
            new IntegrationProperty("UserFieldDefinitions", "Description", EPropertyDataType.String, false),
            new IntegrationProperty("UserFieldDefinitions", "DisplayInUI", EPropertyDataType.Boolean, false),
            new IntegrationProperty("UserFieldDefinitions", "ExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("UserFieldDefinitions", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("UserFieldDefinitions", "ObjectType", EPropertyDataType.String, false),
            new IntegrationProperty("UserFieldDefinitions", "Publish", EPropertyDataType.Boolean, false),
            new IntegrationProperty("UserFieldDefinitions", "UDFType", EPropertyDataType.String, false),
        };
    }
}
public class WarehousesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Warehouses";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Warehouse";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Items };
    public override int Step => 1900;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public override string Group => "Warehouses";
    public WarehousesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Warehouses", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Warehouses", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("Warehouses", "Description", EPropertyDataType.String, false),
        };
    }
}
public class ActivityBatchingFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Activity Batching";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public ActivityBatchingFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("JobOperations", "BatchCode", EPropertyDataType.String, true),
            new IntegrationProperty("Resources", "BatchType", EPropertyDataType.String, true),
            new IntegrationProperty("Resources", "BatchVolume", EPropertyDataType.Decimal, true),
        };
    }
}

public class AttributesAdvancedFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "AttributesAdvanced";
    public override string FeatureName => s_FeatureName;
    public override string FeatureCaption => "Attributes Advanced";
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public AttributesAdvancedFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("AttributeCodeTableAttrCodes", "CleanoutGrade", EPropertyDataType.Int, false),
            new IntegrationProperty("AttributeCodeTableAttrCodes", "Cost", EPropertyDataType.Decimal, false),
            new IntegrationProperty("AttributeCodeTables", "PreviousPrecedence", EPropertyDataType.Boolean, false),
            new IntegrationProperty("AttributeCodeTables", "Wildcard", EPropertyDataType.String, false),
            new IntegrationProperty("AttributeRangeTableAttrNames", "EligibilityConstraint", EPropertyDataType.Boolean, false),
            new IntegrationProperty("AttributeRangeTableTo", "SetupMinutes", EPropertyDataType.Double, true),
            new IntegrationProperty("Attributes", "AttributeTrigger", EPropertyDataType.String, false),
            new IntegrationProperty("Attributes", "CleanoutGrade", EPropertyDataType.Int, false),
            new IntegrationProperty("Attributes", "ColorCode", EPropertyDataType.String, false),
            new IntegrationProperty("Attributes", "ConsecutiveSetup", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Attributes", "DefaultCost", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Attributes", "DefaultDurationHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("Attributes", "HideInGrids", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Attributes", "ShowInGantt", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Attributes", "IncurResourceSetup", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Attributes", "UseInSequencing", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperationAttributes", "Code", EPropertyDataType.String, false),        
            new IntegrationProperty("JobOperationAttributes", "ColorCode", EPropertyDataType.String, false),
            new IntegrationProperty("JobOperationAttributes", "Number", EPropertyDataType.Decimal, false),
        };
    }
}

public class AttributesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Attributes";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Attribute";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Attributes };
    public override int Step => 2900;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public AttributesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Attributes", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("Attributes", "AttributeType", EPropertyDataType.String, false),
            new IntegrationProperty("Attributes", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("Attributes", "Description", EPropertyDataType.String, false),
        };
    }
}

public class AttributeRangeTablesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "AttributeRangeTables";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Lookup_Attribute_Range_Table";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Attributes };
    public override int Step => 3000;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public AttributeRangeTablesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("AttributeRangeTables", "TableId", EPropertyDataType.Long, true),
            new IntegrationProperty("AttributeRangeTables", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("AttributeRangeTables", "Description", EPropertyDataType.String, true),
        };
    }
}

public class AttributeRangeTableAttributeNameFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "AttributeRangeTableAttrNames";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Lookup_Attribute_Range_Table";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Attributes };
    public override int Step => 3100;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public AttributeRangeTableAttributeNameFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("AttributeRangeTableAttrNames", "AttributeExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("AttributeRangeTableAttrNames", "TableId", EPropertyDataType.Long, true),
            new IntegrationProperty("AttributeRangeTableAttrNames", "Description", EPropertyDataType.String, false),
        };
    }
}

public class AttributeRangeTableFromFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "AttributeRangeTableFrom";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Lookup_Attribute_Range_Table";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Attributes };
    public override int Step => 3200;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public AttributeRangeTableFromFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("AttributeRangeTableFrom", "AttributeExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("AttributeRangeTableFrom", "FromId", EPropertyDataType.Long, true),
            new IntegrationProperty("AttributeRangeTableFrom", "FromRangeEnd", EPropertyDataType.Decimal, true),
            new IntegrationProperty("AttributeRangeTableFrom", "FromRangeStart", EPropertyDataType.Decimal, true),
            new IntegrationProperty("AttributeRangeTableFrom", "TableId", EPropertyDataType.Long, true),
        };
    }
}

public class AttributeRangeTableToFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "AttributeRangeTableTo";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Lookup_Attribute_Range_Table";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Attributes };
    public override int Step => 3300;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public AttributeRangeTableToFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("AttributeRangeTableTo", "AttributeExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("AttributeRangeTableTo", "TableId", EPropertyDataType.Long, true),
            new IntegrationProperty("AttributeRangeTableTo", "FromId", EPropertyDataType.Long, true),
            new IntegrationProperty("AttributeRangeTableTo", "ToRangeStart", EPropertyDataType.Decimal, true),
            new IntegrationProperty("AttributeRangeTableTo", "ToRangeEnd", EPropertyDataType.Decimal, true),
        };
    }
}

public class AttributeRangeTableResourceFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "AttributeRangeTableResources";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Lookup_Attribute_Range_Table";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Attributes };
    public override int Step => 3400;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public AttributeRangeTableResourceFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("AttributeRangeTableResources", "DepartmentExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("AttributeRangeTableResources", "PlantExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("AttributeRangeTableResources", "ResourceExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("AttributeRangeTableResources", "TableId", EPropertyDataType.Long, true),
        };
    }
}

public class AttributeCodeTablesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "AttributeCodeTables";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Attribute_Code_Table";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Attributes };
    public override int Step => 3500;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public AttributeCodeTablesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("AttributeCodeTables", "TableName", EPropertyDataType.String, true),
            new IntegrationProperty("AttributeCodeTables", "Description", EPropertyDataType.String, false),
        };
    }
}

public class AttributeCodeTableAttributeNameFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "AttributeCodeTableAttrNames";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Attribute_Code_Table";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Attributes };
    public override int Step => 3600;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public AttributeCodeTableAttributeNameFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("AttributeCodeTableAttrNames", "AttributeExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("AttributeCodeTableAttrNames", "TableName", EPropertyDataType.String, true),
        };
    }
}

public class AttributeCodeTableAttributeCodeFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "AttributeCodeTableAttrCodes";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Attribute_Code_Table";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Attributes };
    public override int Step => 3700;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public AttributeCodeTableAttributeCodeFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("AttributeCodeTableAttrCodes", "AttributeExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("AttributeCodeTableAttrCodes", "TableName", EPropertyDataType.String, true),
            new IntegrationProperty("AttributeCodeTableAttrCodes", "DurationHours", EPropertyDataType.Double, true),
            new IntegrationProperty("AttributeCodeTableAttrCodes", "NextOpAttributeCode", EPropertyDataType.String, true),
            new IntegrationProperty("AttributeCodeTableAttrCodes", "PreviousOpAttributeCode", EPropertyDataType.String, true),

        };
    }
}

public class AttributeCodeTableAssignedResourcesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "AttributeCodeTableResources";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Attribute_Code_Table";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Attributes };
    public override int Step => 3800;
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public AttributeCodeTableAssignedResourcesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("AttributeCodeTableResources", "DepartmentExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("AttributeCodeTableResources", "PlantExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("AttributeCodeTableResources", "ResourceExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("AttributeCodeTableResources", "TableName", EPropertyDataType.String, true),
        };
    }
}

public class CellsAdvancedFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "CellsAdvanced";
    public override string FeatureName => s_FeatureName;
    public override string FeatureCaption => "Cells Advanced";
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public CellsAdvancedFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Resources", "DiscontinueSameCellScheduling", EPropertyDataType.Boolean, false),
        };
    }
}
public class CompatibilityGroupFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Compatibility Group";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public CompatibilityGroupFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("JobOperations", "CompatibilityCode", EPropertyDataType.String, true),
            new IntegrationProperty("JobOperations", "UseCompatibilityCode", EPropertyDataType.Boolean, true),
        };
    }
}
public class DBRFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "DBR";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public DBRFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Resources", "Drum", EPropertyDataType.Boolean, true),
            new IntegrationProperty("Plants", "BottleneckThreshold", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Resources", "BufferSpanHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("Inventories", "BufferStock", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Inventories", "DbrReceivingBufferDays", EPropertyDataType.Double, false),
            new IntegrationProperty("Inventories", "DbrShippingBufferDays", EPropertyDataType.Double, false),
            new IntegrationProperty("ManufacturingOrders", "DBRShippingBufferOverrideDays", EPropertyDataType.Double, false),
            new IntegrationProperty("JobOperations", "JITStartDateBufferDays", EPropertyDataType.Double, false),
        };
    }
}
public class FinancialFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Financial";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public FinancialFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Warehouses", "AnnualPercentageRate", EPropertyDataType.Decimal, true),
            new IntegrationProperty("JobOperations", "CarryingCost", EPropertyDataType.Decimal, true),
            new IntegrationProperty("Items", "Cost", EPropertyDataType.Decimal, true),
            new IntegrationProperty("Plants", "DailyOperatingExpense", EPropertyDataType.Decimal, true),
            new IntegrationProperty("Plants", "InvestedCapital", EPropertyDataType.Decimal, true),
            new IntegrationProperty("Jobs", "LatePenaltyCost", EPropertyDataType.Decimal, true),
            new IntegrationProperty("Resources", "OvertimeHourlyCost", EPropertyDataType.Decimal, true),
            new IntegrationProperty("Jobs", "Revenue", EPropertyDataType.Decimal, true),
            new IntegrationProperty("Jobs", "ShippingCost", EPropertyDataType.Decimal, true),
            new IntegrationProperty("Resources", "StandardHourlyCost", EPropertyDataType.Decimal, true),
            new IntegrationProperty("JobMaterials", "TotalCost", EPropertyDataType.Decimal, true),
            new IntegrationProperty("AttributeRangeTableTo", "SetupCost", EPropertyDataType.Decimal, false),
        };
    }
}
public class ForecastsAdvancedFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "ForecastsAdvanced";
    public override string FeatureName => s_FeatureName;
    public override string FeatureCaption => "Forecasts Advanced";
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public ForecastsAdvancedFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Inventories", "ForecastConsumption", EPropertyDataType.String, true),
            new IntegrationProperty("Inventories", "AutoGenerateForecasts", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Forecasts", "Planner", EPropertyDataType.String, false),
            new IntegrationProperty("Forecasts", "SalesOffice", EPropertyDataType.String, false),
            new IntegrationProperty("Forecasts", "SalesPerson", EPropertyDataType.String, false),
        };
    }
}
public class LotsAdvancedFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "LotsAdvanced";
    public override string FeatureName => s_FeatureName;
    public override string FeatureCaption => "Lots Advanced";
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public LotsAdvancedFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Lots", "LotProductionDate", EPropertyDataType.DateTime, true),
            new IntegrationProperty("Lots", "ExpirationDate", EPropertyDataType.DateTime, false),
            new IntegrationProperty("JobMaterials", "MaxEligibleWearAmount", EPropertyDataType.Int, false),
            new IntegrationProperty("JobMaterials", "MinRemainingShelfLifeHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("Items", "ShelfLifeHrs", EPropertyDataType.Double, false),
        };
    }
}
public class MaxDelayFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Max Delay";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public MaxDelayFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("JobPathNodes", "MaxDelayHrs", EPropertyDataType.Double, true),
        };
    }
}
public class MOBatchingFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "MOBatching";
    public override string FeatureName => s_FeatureName;
    public override string FeatureCaption => "MO Batching";
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public MOBatchingFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("ManufacturingOrders", "BatchDefinitionName", EPropertyDataType.String, true),
            new IntegrationProperty("ManufacturingOrders", "BatchGroupName", EPropertyDataType.String, true),
        };
    }
}
public class MRPAdvancedFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "MRP Advanced";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public MRPAdvancedFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("SalesOrderLineDistributions", "AllowPartialAllocations", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobActivities", "PlanningScrapPercent", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobOperations", "PlanningScrapPercent", EPropertyDataType.Decimal, false),
        };
    }
}
public class MRPBatchingFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "MRP Batching";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public MRPBatchingFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Items", "BatchSize", EPropertyDataType.Decimal, true),
            new IntegrationProperty("Items", "BatchWindowHrs", EPropertyDataType.Double, true),
            new IntegrationProperty("Items", "JobAutoSplitQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Items", "MinOrderQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Items", "MinOrderQtyRoundupLimit", EPropertyDataType.Decimal, false),
        };
    }
}
public class MRPFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "MRP";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public MRPFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Inventories", "MrpProcessing", EPropertyDataType.String, true),
            new IntegrationProperty("JobProducts", "SetWarehouseDuringMRP", EPropertyDataType.Boolean, false),
        };
    }
}
public class MultiPlantFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Multi Plant";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public MultiPlantFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("ManufacturingOrders", "CanSpanPlants", EPropertyDataType.Boolean, true),
            new IntegrationProperty("Jobs", "CanSpanPlants", EPropertyDataType.Boolean, true),
            new IntegrationProperty("ManufacturingOrders", "LockedPlantExternalId", EPropertyDataType.String, false),
        };
    }
}
public class OverlapFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Overlap";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public OverlapFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("JobPathNodes", "OverlapType", EPropertyDataType.String, true),
            new IntegrationProperty("JobPathNodes", "OverlapPercentComplete", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobPathNodes", "OverlapTransferHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobOperations", "OverlapTransferQty", EPropertyDataType.Decimal, false),
        };
    }
}
public class PlanningFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Planning";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public PlanningFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("SalesOrders", "ExpirationDate", EPropertyDataType.DateTime, false),
        };
    }
}
public class PriorityFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Priority";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public PriorityFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Jobs", "Priority", EPropertyDataType.Int, true),
            new IntegrationProperty("Forecasts", "Priority", EPropertyDataType.Int, false),
            new IntegrationProperty("Inventories", "SafetyStockJobPriority", EPropertyDataType.Int, false),
        };
    }
}
public class ProductRulesAdvancedFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "ProductRulesAdvanced";
    public override string FeatureName => s_FeatureName;
    public override string FeatureCaption => "Product Rules Advanced";
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public ProductRulesAdvancedFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("ProductRules", "CycleHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("ProductRules", "HeadStartHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("ProductRules", "PlanningScrapPercent", EPropertyDataType.Double, false),
            new IntegrationProperty("ProductRules", "PostProcessingHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("ProductRules", "QtyPerCycle", EPropertyDataType.Double, false),
            new IntegrationProperty("ProductRules", "SetupHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("ProductRules", "UseCycleHrs", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ProductRules", "UseHeadStartSpan", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ProductRules", "UsePlanningScrapPercent", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ProductRules", "UsePostProcessingHrs", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ProductRules", "UseQtyPerCycle", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ProductRules", "UseSetupHrs", EPropertyDataType.Boolean, false),
        };
    }
}
public class PurchaseOrdersFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Purchase Orders";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public PurchaseOrdersFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("PurchasesToStock", "Closed", EPropertyDataType.Boolean, false),
            new IntegrationProperty("PurchasesToStock", "Firm", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobMaterials", "UseOverlapPurchases", EPropertyDataType.Boolean, false),
        };
    }
}
public class QuantityConstraintsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Quantity Constraints";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public QuantityConstraintsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Resources", "MaxQtyPerCycle", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Resources", "MinQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Resources", "MaxQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Resources", "MinQtyPerCycle", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Warehouses", "StorageCapacity", EPropertyDataType.Decimal, false),
        };
    }
}
public class ReportedValuesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Reported Values";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public ReportedValuesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("JobActivities", "ReportedEndOfRunDate", EPropertyDataType.DateTime, false),
            new IntegrationProperty("JobActivities", "ReportedFinishDate", EPropertyDataType.DateTime, false),
            new IntegrationProperty("JobActivities", "ReportedGoodQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobActivities", "ReportedPostProcessingHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobActivities", "ReportedRunHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobActivities", "ReportedScrapQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobActivities", "ReportedSetupHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobActivities", "ReportedStartDate", EPropertyDataType.DateTime, false),
            new IntegrationProperty("JobActivities", "ReportedStartOfProcessingDate", EPropertyDataType.DateTime, false),
        };
    }
}
public class ResourceCapacityFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Resource Capacity";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public ResourceCapacityFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Resources", "CapacityType", EPropertyDataType.String, true),
            new IntegrationProperty("JobActivities", "PeopleUsage", EPropertyDataType.String, true),
            new IntegrationProperty("JobResources", "PrimaryRequirement", EPropertyDataType.Boolean, true),
            new IntegrationProperty("JobResources", "AttentionPercent", EPropertyDataType.Int, false),
            new IntegrationProperty("JobResources", "BlockFillImageFile", EPropertyDataType.String, false),
            new IntegrationProperty("JobResources", "BlockFillPattern", EPropertyDataType.String, false),
            new IntegrationProperty("JobResources", "BlockFillType", EPropertyDataType.String, false),
            new IntegrationProperty("JobResources", "CopyMaterialsToCapabilities", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Resources", "CycleEfficiencyMultiplier", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobResources", "DefaultResourceJITLimitHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("Resources", "MinNbrOfPeople", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobActivities", "NbrOfPeople", EPropertyDataType.Decimal, false),
            new IntegrationProperty("CapacityIntervals", "NbrOfPeople", EPropertyDataType.Double, false),
            new IntegrationProperty("JobResources", "NumberOfResourcesRequired", EPropertyDataType.Int, false),
        };
    }
}
public class ResourceStagingFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Resource Staging";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public ResourceStagingFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Resources", "Stage", EPropertyDataType.Int, false),
        };
    }
}
public class RoutingsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Routings";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public RoutingsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("JobPathNodes", "AllowManualConnectorViolation", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ManufacturingOrders", "AlternatePathSelection", EPropertyDataType.String, false),
            new IntegrationProperty("JobPaths", "AutoBuildLinearPath", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobPathNodes", "AutoFinishPredecessor", EPropertyDataType.String, false),
            new IntegrationProperty("JobPaths", "AutoUse", EPropertyDataType.String, false),
            new IntegrationProperty("JobPaths", "AutoUseReleaseOffsetDays", EPropertyDataType.Double, false),
            new IntegrationProperty("ManufacturingOrders", "CopyRoutingFromTemplate", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobPathNodes", "IgnoreInvalidSuccessorOperationExternalIds", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "KeepSuccessorsTimeLimitHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("ManufacturingOrders", "LockToCurrentAlternatePath", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobPaths", "Preference", EPropertyDataType.Int, false),
            new IntegrationProperty("JobOperations", "SuccessorProcessing", EPropertyDataType.String, false),
            new IntegrationProperty("Jobs", "Template", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Inventories", "TemplateJobExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("JobPathNodes", "TransferHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobPathNodes", "UsageQtyPerCycle", EPropertyDataType.Decimal, false),
        };
    }
}
public class SalesOrdersAdvancedFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "SalesOrdersAdvanced";
    public override string FeatureName => s_FeatureName;
    public override string FeatureCaption => "Sales Orders Advanced";
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public SalesOrdersAdvancedFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("SalesOrderLineDistributions", "Closed", EPropertyDataType.Boolean, true),
            new IntegrationProperty("SalesOrderLines", "LineNumber", EPropertyDataType.String, true),
            new IntegrationProperty("SalesOrderLineDistributions", "Priority", EPropertyDataType.Int, true),
            new IntegrationProperty("SalesOrderLineDistributions", "QtyShipped", EPropertyDataType.Decimal, true),
            new IntegrationProperty("SalesOrders", "SalesAmount", EPropertyDataType.Decimal, true),
            new IntegrationProperty("SalesOrderLines", "UnitPrice", EPropertyDataType.Decimal, true),
            new IntegrationProperty("SalesOrders", "CancelAtExpirationDate", EPropertyDataType.Boolean, false),
            new IntegrationProperty("SalesOrders", "Cancelled", EPropertyDataType.Boolean, false),
            new IntegrationProperty("SalesOrderLineDistributions", "Hold", EPropertyDataType.Boolean, false),
            new IntegrationProperty("SalesOrderLineDistributions", "HoldReason", EPropertyDataType.String, false),
            new IntegrationProperty("SalesOrderLineDistributions", "MaximumLatenessDays", EPropertyDataType.Double, false),
            new IntegrationProperty("SalesOrderLineDistributions", "MinAllocationQty", EPropertyDataType.Decimal, false),
            new IntegrationProperty("SalesOrderLineDistributions", "MustSupplyFromWarehouseExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("SalesOrders", "Planner", EPropertyDataType.String, false),
            new IntegrationProperty("SalesOrders", "Project", EPropertyDataType.String, false),
            new IntegrationProperty("SalesOrders", "SalesOffice", EPropertyDataType.String, false),
            new IntegrationProperty("SalesOrders", "SalesPerson", EPropertyDataType.String, false),
            new IntegrationProperty("SalesOrderLineDistributions", "SalesRegion", EPropertyDataType.String, false),
            new IntegrationProperty("SalesOrderLineDistributions", "ShipToZone", EPropertyDataType.String, false),
            new IntegrationProperty("SalesOrderLineDistributions", "StockShortageRule", EPropertyDataType.String, false),
            new IntegrationProperty("SalesOrders", "Estimate", EPropertyDataType.Boolean, false),
        };
    }
}
public class ScheduleAdheranceFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Schedule Adherance";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public ScheduleAdheranceFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("JobOperations", "CommitEndDate", EPropertyDataType.DateTime, false),
            new IntegrationProperty("JobOperations", "CommitStartDate", EPropertyDataType.DateTime, false),
        };
    }
}
public class ScrapFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Scrap";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public ScrapFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("JobOperations", "DeductScrapFromRequired", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "IsRework", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobActivities", "ScrapPercentManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "ScrapPercentManualUpdateOnly", EPropertyDataType.Boolean, false),
        };
    }
}
public class SetupAdvancedFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Setup Advanced";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public SetupAdvancedFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Resources", "ConsecutiveSetupTimes", EPropertyDataType.Boolean, true),
            new IntegrationProperty("Resources", "SetupIncluded", EPropertyDataType.String, true),
            new IntegrationProperty("SetupCodeTableSetupCode", "", EPropertyDataType.Double, false),
            new IntegrationProperty("Resources", "OmitSetupOnFirstActivity", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Resources", "OmitSetupOnFirstActivityInShift", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Resources", "ScheduledSetupSpanAlgorithm", EPropertyDataType.String, false),
            new IntegrationProperty("Resources", "SetupHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobOperations", "SetupNumber", EPropertyDataType.Decimal, false),
            new IntegrationProperty("JobActivities", "SetupTimeManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("JobOperations", "SetupTimeManualUpdateOnly", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Resources", "UseOperationSetupTime", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Resources", "UseResourceSetupTime", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Resources", "UseSequencedSetupTime", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Resources", "UseResourceCleanout", EPropertyDataType.Boolean, false),
            new IntegrationProperty("Resources", "ResourceSetupCost", EPropertyDataType.Decimal, false),
            new IntegrationProperty("Resources", "ResourceCleanoutCost", EPropertyDataType.Decimal, false),
        };
    }
}
public class SplittingFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Splitting";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public SplittingFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("ManufacturingOrders", "AutoJoinGroup", EPropertyDataType.String, false),
            new IntegrationProperty("Resources", "AutoJoinHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("Resources", "AutoSplitHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("ManufacturingOrders", "SplitUpdateMode", EPropertyDataType.String, false),
            new IntegrationProperty("JobOperations", "SplitUpdateMode", EPropertyDataType.String, false),
            new IntegrationProperty("JobOperations", "WholeNumberSplits", EPropertyDataType.Boolean, false),
        };
    }
}
public class SuccessorMOsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "JobSuccessorManufacturingOrders";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Successor_MO";
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override int Step => 5800;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override List<ObjectType> ObjectTypes => new() { ObjectType.Jobs };
    public SuccessorMOsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("JobSuccessorManufacturingOrders", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobSuccessorManufacturingOrders", "JobExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobSuccessorManufacturingOrders", "MoExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobSuccessorManufacturingOrders", "SuccessorJobExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobSuccessorManufacturingOrders", "SuccessorManufacturingOrderExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("JobSuccessorManufacturingOrders", "SuccessorOperationExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("JobSuccessorManufacturingOrders", "SuccessorPathExternalId", EPropertyDataType.String, false),
            new IntegrationProperty("JobSuccessorManufacturingOrders", "TransferHrs", EPropertyDataType.Double, false),
            new IntegrationProperty("JobSuccessorManufacturingOrders", "UsageQtyPerCycle", EPropertyDataType.Decimal, false),
        };
    }
}
public class TankFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Tank";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public TankFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Resources", "IsTank", EPropertyDataType.Boolean, true),
            new IntegrationProperty("JobMaterials", "TankStorageReleaseTiming", EPropertyDataType.String, true),
            new IntegrationProperty("Warehouses", "TankWarehouse", EPropertyDataType.Boolean, false),
        };
    }
}
public class TransferOrderFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "TransferOrders";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_Transfer_Order";
    public override List<ObjectType> ObjectTypes => new () { ObjectType.Items };
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override int Step => 6800;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public TransferOrderFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("TransferOrders", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("TransferOrders", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("TransferOrders", "Closed", EPropertyDataType.Boolean, true),
            new IntegrationProperty("TransferOrders", "Description", EPropertyDataType.String, false),
            new IntegrationProperty("TransferOrders", "Priority", EPropertyDataType.Int, false),
        };
    }
}

public class TransferSpanFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Transfer Span";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public TransferSpanFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Resources", "TransferHrs", EPropertyDataType.Double, true),
        };
    }
}

public class NotesFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "Notes";
    public override string FeatureName => s_FeatureName;
    //public override List<ObjectType> ObjectTypes => new () { ObjectType.Resources, ObjectType.Items, ObjectType.Jobs };
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public NotesFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Capabilities", "Notes", EPropertyDataType.String, false),
            new IntegrationProperty("CapacityIntervals", "Notes", EPropertyDataType.String, false),
            new IntegrationProperty("Cells", "Notes", EPropertyDataType.String, false),
            new IntegrationProperty("Customers", "Notes", EPropertyDataType.String, false),
            new IntegrationProperty("Departments", "Notes", EPropertyDataType.String, false),
            new IntegrationProperty("Items", "Notes", EPropertyDataType.String, false),
            new IntegrationProperty("JobOperations", "Notes", EPropertyDataType.String, false),
            new IntegrationProperty("Jobs", "Notes", EPropertyDataType.String, false),
            new IntegrationProperty("ManufacturingOrders", "Notes", EPropertyDataType.String, false),
            new IntegrationProperty("Plants", "Notes", EPropertyDataType.String, false),
            new IntegrationProperty("PurchasesToStock", "Notes", EPropertyDataType.String, false),
            new IntegrationProperty("RecurringCapacityIntervals", "Notes", EPropertyDataType.String, false),
            new IntegrationProperty("ResourceConnectors", "Notes", EPropertyDataType.String, false),
            new IntegrationProperty("Resources", "Notes", EPropertyDataType.String, false),
            new IntegrationProperty("UserFieldDefinitions", "Notes", EPropertyDataType.String, false),
            //new IntegrationProperty("Users", "Notes", EPropertyDataType.String, false),
            new IntegrationProperty("Warehouses", "Notes", EPropertyDataType.String, false),
            new IntegrationProperty("Forecasts", "Notes", EPropertyDataType.String, false),
            new IntegrationProperty("TransferOrders", "Notes", EPropertyDataType.String, false),
            new IntegrationProperty("StorageAreaConnectors", "Notes", EPropertyDataType.String),
        };
    }
}
public class UDFFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "UDF";
    public override string FeatureName => s_FeatureName;
    public override bool IsBaseTableFeature => false;
    public override string Category => IntegrationConstants.FeatureCategory;
    public UDFFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("Forecasts", "UserFields", EPropertyDataType.String, false),
            new IntegrationProperty("TransferOrders", "UserFields", EPropertyDataType.String, false),
            new IntegrationProperty("Items", "UserFields", EPropertyDataType.String, false),
            new IntegrationProperty("PurchasesToStock", "UserFields", EPropertyDataType.String, false),
            new IntegrationProperty("SalesOrders", "UserFields", EPropertyDataType.String, false),
            new IntegrationProperty("Warehouses", "UserFields", EPropertyDataType.String, false),
            new IntegrationProperty("ManufacturingOrders", "UserFields", EPropertyDataType.String, false),
            new IntegrationProperty("JobOperations", "UserFields", EPropertyDataType.String, false),
            new IntegrationProperty("Jobs", "UserFields", EPropertyDataType.String, false),
            new IntegrationProperty("Capabilities", "UserFields", EPropertyDataType.String, false),
            new IntegrationProperty("CapacityIntervals", "UserFields", EPropertyDataType.String, false),
            new IntegrationProperty("Departments", "UserFields", EPropertyDataType.String, false),
            new IntegrationProperty("Plants", "UserFields", EPropertyDataType.String, false),
            new IntegrationProperty("Resources", "UserFields", EPropertyDataType.String, false),
            new IntegrationProperty("Lots", "UserFields", EPropertyDataType.String, false),
            new IntegrationProperty("StorageAreas", "UserFields", EPropertyDataType.String, false),
            new IntegrationProperty("StorageAreaConnectors", "UserFields", EPropertyDataType.String, false),
            new IntegrationProperty("ItemStorage", "UserFields", EPropertyDataType.String, false),
        };
    }
}

public class StorageAreaFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "StorageAreas";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_StorageArea";
    public override string FeatureCaption => "StorageAreas";
    public override int Step => 2040;
    public override List<ObjectType> ObjectTypes => new() { ObjectType.Items };
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public StorageAreaFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("StorageAreas", "ExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("StorageAreas", "Name", EPropertyDataType.String, true),
            new IntegrationProperty("StorageAreas", "Description", EPropertyDataType.String),
            new IntegrationProperty("StorageAreas", "Notes", EPropertyDataType.String),
            new IntegrationProperty("StorageAreas", "WarehouseExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("StorageAreas", "SingleItemStorage", EPropertyDataType.Boolean),
        };
    }
}

public class StorageAreaConnectorsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "StorageAreaConnectors";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_StorageAreaConnector";
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override List<ObjectType> ObjectTypes => new() { ObjectType.Items };
    public override int Step => 6750;
    public StorageAreaConnectorsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("StorageAreaConnectors", "ExternalId", EPropertyDataType.String),
            new IntegrationProperty("StorageAreaConnectors", "Description", EPropertyDataType.String),
            new IntegrationProperty("StorageAreaConnectors", "Name", EPropertyDataType.String),
            new IntegrationProperty("StorageAreaConnectors", "WarehouseExternalId", EPropertyDataType.String),
            new IntegrationProperty("StorageAreaConnectors", "CounterFlow", EPropertyDataType.Boolean),
            new IntegrationProperty("StorageAreaConnectors", "StorageInFlowLimit", EPropertyDataType.Int),
            new IntegrationProperty("StorageAreaConnectors", "StorageOutFlowLimit", EPropertyDataType.Int),
            new IntegrationProperty("StorageAreaConnectors", "CounterFlowLimit", EPropertyDataType.Int),
            
        };
    }
}

public class StorageAreaConnectorsInFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "StorageAreaConnectorsIn";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_StorageAreaConnectorIn";
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override List<ObjectType> ObjectTypes => new() { ObjectType.Items };
    public override int Step => 6755;
    public StorageAreaConnectorsInFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("StorageAreaConnectorsIn", "StorageAreaConnectorExternalId", EPropertyDataType.String),
            new IntegrationProperty("StorageAreaConnectorsIn", "StorageAreaExternalId", EPropertyDataType.String),
            new IntegrationProperty("StorageAreaConnectorsIn", "WarehouseExternalId", EPropertyDataType.String),
        };
    }
}
public class StorageAreaConnectorsOutFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "StorageAreaConnectorsOut";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_StorageAreaConnectorOut";
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override List<ObjectType> ObjectTypes => new() { ObjectType.Items };
    public override int Step => 6760;
    public StorageAreaConnectorsOutFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("StorageAreaConnectorsOut", "StorageAreaConnectorExternalId", EPropertyDataType.String),
            new IntegrationProperty("StorageAreaConnectorsOut", "StorageAreaExternalId", EPropertyDataType.String),
            new IntegrationProperty("StorageAreaConnectorsOut", "WarehouseExternalId", EPropertyDataType.String),
        };
    }
}

public class ResourceStorageAreaConnectorsInFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "ResourceStorageAreaConnectorsIn";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_ResourceStorageAreaConnectorIn";
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override List<ObjectType> ObjectTypes => new() { ObjectType.Items };
    public override int Step => 6450;
    public ResourceStorageAreaConnectorsInFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("ResourceStorageAreaConnectorsIn", "StorageAreaConnectorExternalId", EPropertyDataType.String),
            new IntegrationProperty("ResourceStorageAreaConnectorsIn", "PlantExternalId", EPropertyDataType.String),
            new IntegrationProperty("ResourceStorageAreaConnectorsIn", "DepartmentExternalId", EPropertyDataType.String),
            new IntegrationProperty("ResourceStorageAreaConnectorsIn", "ResourceExternalId", EPropertyDataType.String),
            new IntegrationProperty("ResourceStorageAreaConnectorsIn", "WarehouseExternalId", EPropertyDataType.String),
        };
    }
}

public class ResourceStorageAreaConnectorsOutFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "ResourceStorageAreaConnectorsOut";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_ResourceStorageAreaConnectorOut";
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override List<ObjectType> ObjectTypes => new() { ObjectType.Items };
    public override int Step => 6460;
    public ResourceStorageAreaConnectorsOutFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("ResourceStorageAreaConnectorsOut", "StorageAreaConnectorExternalId", EPropertyDataType.String),
            new IntegrationProperty("ResourceStorageAreaConnectorsOut", "PlantExternalId", EPropertyDataType.String),
            new IntegrationProperty("ResourceStorageAreaConnectorsOut", "DepartmentExternalId", EPropertyDataType.String),
            new IntegrationProperty("ResourceStorageAreaConnectorsOut", "ResourceExternalId", EPropertyDataType.String),
            new IntegrationProperty("ResourceStorageAreaConnectorsOut", "WarehouseExternalId", EPropertyDataType.String),
        };
    }
}

public class ItemStorageFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "ItemStorage";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_ItemStorage";
    public override int Step => 2060;
    public override List<ObjectType> ObjectTypes => new() { ObjectType.Items };
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.CoreCategory;
    public ItemStorageFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("ItemStorage", "StorageAreaExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ItemStorage", "ItemExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ItemStorage", "WarehouseExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ItemStorage", "MaxQty", EPropertyDataType.Decimal, true),
        };
    }
}
public class ItemStorageLotsFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "ItemStorageLots";
    public override string FeatureName => s_FeatureName;
    public override string FeatureHelpTopic => "Wizard_ItemStorageLot";
    public override bool IsBaseTableFeature => true;
    public override bool CanSetAutoDelete => true;
    public override string Category => IntegrationConstants.AdvancedTablesCategory;
    public override List<ObjectType> ObjectTypes => new() { ObjectType.Items };
    public override int Step => 5250;
    public ItemStorageLotsFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("ItemStorageLots", "StorageAreaExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ItemStorageLots", "ItemExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ItemStorageLots", "WarehouseExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ItemStorageLots", "LotExternalId", EPropertyDataType.String, true),
            new IntegrationProperty("ItemStorageLots", "Qty", EPropertyDataType.Decimal, true),
            new IntegrationProperty("ItemStorageLots", "Code", EPropertyDataType.String, false),
            new IntegrationProperty("ItemStorageLots", "ExpirationDate", EPropertyDataType.DateTime, false),
            new IntegrationProperty("ItemStorageLots", "LimitMatlSrcToEligibleLots", EPropertyDataType.Boolean, false),
            new IntegrationProperty("ItemStorageLots", "LotProductionDate", EPropertyDataType.DateTime, false),
        };
    }
}

public class ItemStorageDisposalFeature : IntegrationFeatureBase
{
    public static string s_FeatureName => "ItemStorageDisposal";
    public override string FeatureName => s_FeatureName;
    public override string Category => IntegrationConstants.FeatureCategory;
    public ItemStorageDisposalFeature()
    {
        Properties = new List<IntegrationProperty>()
        {
            new IntegrationProperty("ItemStorage", "DisposalQty", EPropertyDataType.Decimal, true),
            new IntegrationProperty("ItemStorage", "DisposeImmediately", EPropertyDataType.Boolean, false),
        };
    }
}
#endregion


public static class PropertySourceOptionsExtensions
{
    // TODO: We'll need to consider Localization at some point (here and in the board in general) - especially since these might be shared across instances
    public static string ToFString(this EPropertySourceOption a_sourceOption) //does not override ToString even if named ToString, something about boxing
    {
        switch (a_sourceOption)
        {
            case EPropertySourceOption.FromTable:
                return "From Table";
            case EPropertySourceOption.FixedValue:
                return "Fixed Value";
            case EPropertySourceOption.KeepValue:
                return "Keep Value";
            case EPropertySourceOption.ClearValue:
                return "Clear Value";
            default:
                throw new UnreachableException();
        }
    }

    public static EPropertySourceOption FromFString(string a_sourceOption)
    {
        switch (a_sourceOption)
        {
            case "From Table":
                return EPropertySourceOption.FromTable;
            case "Fixed Value":
                return EPropertySourceOption.FixedValue;
            case "Keep Value":
                return EPropertySourceOption.KeepValue;
            case "Clear Value":
                return EPropertySourceOption.ClearValue;
            default:
                throw new UnreachableException();
        }
    }
}

public class IntegrationProperty
{
    public int WebAppId { get; set; }
    public string TableName { get; set; }
    public string ColumnName { get; set; }
    public bool RequiredForFeature { get; set; }
    public EPropertyDataType DataType { get; set; }
    public EPropertySourceOption SourceOption { get; set; }
    public string FixedValue { get; set; }
    public List<string> FeatureDependencies { get; set; }

    /// <summary>
    /// Uses the currently set SourceOption to determine whether the property is being used in the current integration.
    /// TODO: We may want integrator feedback on whether this is an appropriate check (e.g., maybe KeepValue shouldn't be considered?)
    /// </summary>
    public bool IsEnabledForImport => SourceOption == EPropertySourceOption.FromTable ||
                                      SourceOption == EPropertySourceOption.KeepValue ||
                                      (SourceOption == EPropertySourceOption.FixedValue && !string.IsNullOrWhiteSpace(FixedValue));

    public IntegrationProperty() { }

    // TODO: Need to population all properties with whether they are required for feature
    /// <summary>
    /// Creates a Property, which corresponds to a particular field in a dataset. <see cref="IntegrationFeatureBase"/> objects will contain one or more of these.
    /// </summary>
    /// <param name="a_tableName"></param>
    /// <param name="a_columnName"></param>
    /// <param name="a_isRequiredForFeature"></param>
    /// <param name="a_additionalFeatureDependencies">Additional Features that are required for this Property to be used. The Feature corresponding to the property's table is automatically included.</param>
    public IntegrationProperty(string a_tableName, string a_columnName, EPropertyDataType a_dataType, bool a_isRequiredForFeature = false, List<string> a_additionalFeatureDependencies = null, EPropertySourceOption a_sourceOption = EPropertySourceOption.FromTable, string a_fixedValue = "")
    {
        TableName = a_tableName;
        ColumnName = a_columnName;
        DataType = a_dataType;
        RequiredForFeature = a_isRequiredForFeature;
        FeatureDependencies = a_additionalFeatureDependencies ?? new List<string>();
        SourceOption = a_sourceOption;
        FixedValue = a_fixedValue;
        
        // A property always requires its parent table, so we can shorthand it in here
        string featureNameForTable = IntegrationFeatureBase.GetFeatureNameForTable(TableName);
        if (!FeatureDependencies.Any(featureDependency => featureDependency.Equals(featureNameForTable)))
        {
            FeatureDependencies.Add(featureNameForTable);
        }
    }

    /// <summary>
    /// Update changeable fields from incoming DTO.
    /// </summary>
    /// <param name="a_dto"></param>
    public void Update(PropertyDTO a_dto)
    {
        WebAppId = a_dto.Id;
        FixedValue = a_dto.FixedValue;
        SourceOption = a_dto.SourceOption;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        var other = (IntegrationProperty)obj;
        return ColumnName == other.ColumnName && TableName == other.TableName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ColumnName, TableName);
    }
}