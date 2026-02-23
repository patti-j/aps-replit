using PT.APIDefinitions;

namespace PT.ImportDefintions;

/// <summary>
/// Collection of all typed Features in PT. These can be configured as needed.
/// </summary>
public class ImportFeaturesSettings
{
    public IntegrationFeatureBase AttributesFeature => AllFeatures["Attributes"];
    public IntegrationFeatureBase AttributeRangeTablesFeature => AllFeatures["AttributeRangeTables"];
    public IntegrationFeatureBase AttributeRangeTableAttrNamesFeature => AllFeatures["AttributeRangeTableAttrNames"];
    public IntegrationFeatureBase AttributeRangeTableFromFeature => AllFeatures["AttributeRangeTableFrom"];
    public IntegrationFeatureBase AttributeRangeTableToFeature => AllFeatures["AttributeRangeTableTo"];
    public IntegrationFeatureBase AttributeRangeTableResourceFeature => AllFeatures["AttributeRangeTableResources"];
    public IntegrationFeatureBase AttributeCodeTablesFeature => AllFeatures["AttributeCodeTables"];
    public IntegrationFeatureBase AttributeCodeTableAttrNamesFeature => AllFeatures["AttributeCodeTableAttrNames"];
    public IntegrationFeatureBase AttributeCodeTableAttrCodesFeature => AllFeatures["AttributeCodeTableAttrCodes"];
    public IntegrationFeatureBase AttributeCodeTableAssignedResourcesFeature => AllFeatures["AttributeCodeTableResources"];
    public IntegrationFeatureBase CapabilitiesFeature => AllFeatures["Capabilities"];
    public IntegrationFeatureBase CapacityIntervalResourcesFeature => AllFeatures["CapacityIntervalResources"];
    public IntegrationFeatureBase CapacityIntervalsFeature => AllFeatures["CapacityIntervals"];
    public IntegrationFeatureBase CellsFeature => AllFeatures["Cells"];
    public IntegrationFeatureBase CleanoutTriggerTableOpCountFeature => AllFeatures["CleanoutTriggerTableOpCount"];
    public IntegrationFeatureBase CleanoutTriggerTableProdUnitsFeature => AllFeatures["CleanoutTriggerTableProdUnits"];
    public IntegrationFeatureBase CleanoutTriggerTableResourcesFeature => AllFeatures["CleanoutTriggerTableResources"];
    public IntegrationFeatureBase CleanoutTriggerTablesFeature => AllFeatures["CleanoutTriggerTables"];
    public IntegrationFeatureBase CleanoutTriggerTableTimeFeature => AllFeatures["CleanoutTriggerTableTime"];
    public IntegrationFeatureBase CompatibilityCodesFeature => AllFeatures["CompatibilityCodes"];
    public IntegrationFeatureBase CompatibilityCodeTableResourcesFeature => AllFeatures["CompatibilityCodeTableResources"];
    public IntegrationFeatureBase CompatibilityCodeTablesFeature => AllFeatures["CompatibilityCodeTables"];
    public IntegrationFeatureBase CustomersFeature => AllFeatures["Customers"];
    public IntegrationFeatureBase CustomerConnectionsFeature => AllFeatures["CustomerConnections"];
    public IntegrationFeatureBase DepartmentsFeature => AllFeatures["Departments"];
    public IntegrationFeatureBase ForecastsFeature => AllFeatures["Forecasts"];
    public IntegrationFeatureBase ForecastShipmentsFeature => AllFeatures["ForecastShipments"];
    public IntegrationFeatureBase InventoriesFeature => AllFeatures["Inventories"];
    public IntegrationFeatureBase ItemsFeature => AllFeatures["Items"];
    public IntegrationFeatureBase JobActivitiesFeature => AllFeatures["JobActivities"];
    public IntegrationFeatureBase JobMaterialsFeature => AllFeatures["JobMaterials"];
    public IntegrationFeatureBase JobOperationAttributesFeature => AllFeatures["JobOperationAttributes"];
    public IntegrationFeatureBase JobOperationsFeature => AllFeatures["JobOperations"];
    public IntegrationFeatureBase JobPathsFeature => AllFeatures["JobPaths"];
    public IntegrationFeatureBase JobPathNodesFeature => AllFeatures["JobPathNodes"];
    public IntegrationFeatureBase JobProductsFeature => AllFeatures["JobProducts"];
    public IntegrationFeatureBase JobResourceCapabilitiesFeature => AllFeatures["JobResourceCapabilities"];
    public IntegrationFeatureBase JobResourcesFeature => AllFeatures["JobResources"];
    public IntegrationFeatureBase JobsFeature => AllFeatures["Jobs"];
    public IntegrationFeatureBase LotsFeature => AllFeatures["Lots"];
    public IntegrationFeatureBase ManufacturingOrdersFeature => AllFeatures["ManufacturingOrders"];
    public IntegrationFeatureBase PlantsFeature => AllFeatures["Plants"];
    public IntegrationFeatureBase PlantWarehousesFeature => AllFeatures["PlantWarehouses"];
    public IntegrationFeatureBase ProductRulesFeature => AllFeatures["ProductRules"];
    public IntegrationFeatureBase PurchasesToStockFeature => AllFeatures["PurchasesToStock"];
    public IntegrationFeatureBase RecurringCapacityIntervalsFeature => AllFeatures["RecurringCapacityIntervals"];
    public IntegrationFeatureBase ResourceCapabilitiesFeature => AllFeatures["ResourceCapabilities"];

    public IntegrationFeatureBase ResourceConnectionsFeature => AllFeatures["ResourceConnections"];
    public IntegrationFeatureBase ResourceConnectorsFeature => AllFeatures["ResourceConnectors"];
    public IntegrationFeatureBase ResourcesFeature => AllFeatures["Resources"];
    public IntegrationFeatureBase SalesOrderLineDistributionsFeature => AllFeatures["SalesOrderLineDistributions"];
    public IntegrationFeatureBase SalesOrderLinesFeature => AllFeatures["SalesOrderLines"];
    public IntegrationFeatureBase SalesOrdersFeature => AllFeatures["SalesOrders"];
    public IntegrationFeatureBase TransferOrderDistributionsFeature => AllFeatures["TransferOrderDistributions"];
    public IntegrationFeatureBase UserFieldDefinitionsFeature => AllFeatures["UserFieldDefinitions"];
    public IntegrationFeatureBase WarehousesFeature => AllFeatures["Warehouses"];
    public IntegrationFeatureBase ActivityBatchingFeature => AllFeatures["Activity Batching"];
    public IntegrationFeatureBase AttributesAdvancedFeature => AllFeatures["AttributesAdvanced"];
    public IntegrationFeatureBase CellsAdvancedFeature => AllFeatures["CellsAdvanced"];
    public IntegrationFeatureBase CompatibilityGroupFeature => AllFeatures["Compatibility Group"];
    public IntegrationFeatureBase DBRFeature => AllFeatures["DBR"];
    public IntegrationFeatureBase FinancialFeature => AllFeatures["Financial"];
    public IntegrationFeatureBase ForecastsAdvancedFeature => AllFeatures["ForecastsAdvanced"];
    public IntegrationFeatureBase LotsAdvancedFeature => AllFeatures["LotsAdvanced"];
    public IntegrationFeatureBase MaxDelayFeature => AllFeatures["Max Delay"];
    public IntegrationFeatureBase MOBatchingFeature => AllFeatures["MOBatching"];
    public IntegrationFeatureBase MRPAdvancedFeature => AllFeatures["MRP Advanced"];
    public IntegrationFeatureBase MRPBatchingFeature => AllFeatures["MRP Batching"];
    public IntegrationFeatureBase MRPFeature => AllFeatures["MRP"];
    public IntegrationFeatureBase MultiPlantFeature => AllFeatures["Multi Plant"];
    public IntegrationFeatureBase OverlapFeature => AllFeatures["Overlap"];
    public IntegrationFeatureBase PlanningFeature => AllFeatures["Planning"];
    public IntegrationFeatureBase PriorityFeature => AllFeatures["Priority"];
    public IntegrationFeatureBase ProductRulesAdvancedFeature => AllFeatures["ProductRulesAdvanced"];
    public IntegrationFeatureBase PurchaseOrdersFeature => AllFeatures["Purchase Orders"];
    public IntegrationFeatureBase QuantityConstraintsFeature => AllFeatures["Quantity Constraints"];
    public IntegrationFeatureBase ReportedValuesFeature => AllFeatures["Reported Values"];
    public IntegrationFeatureBase ResourceCapacityFeature => AllFeatures["Resource Capacity"];
    public IntegrationFeatureBase ResourceStagingFeature => AllFeatures["Resource Staging"];
    public IntegrationFeatureBase RoutingsFeature => AllFeatures["Routings"];
    public IntegrationFeatureBase SalesOrdersAdvancedFeature => AllFeatures["SalesOrdersAdvanced"];
    public IntegrationFeatureBase ScheduleAdheranceFeature => AllFeatures["Schedule Adherance"];
    public IntegrationFeatureBase ScrapFeature => AllFeatures["Scrap"];
    public IntegrationFeatureBase SetupAdvancedFeature => AllFeatures["Setup Advanced"];
    public IntegrationFeatureBase SplittingFeature => AllFeatures["Splitting"];
    public IntegrationFeatureBase SuccessorMOsFeature => AllFeatures["JobSuccessorManufacturingOrders"];
    public IntegrationFeatureBase TankFeature => AllFeatures["Tank"];
    public IntegrationFeatureBase TransferOrderFeature => AllFeatures["TransferOrders"];
    public IntegrationFeatureBase TransferSpanFeature => AllFeatures["Transfer Span"];
    public IntegrationFeatureBase UDFFeature => AllFeatures["UserFieldDefinitions"];
    public IntegrationFeatureBase StorageAreaFeature => AllFeatures["StorageAreas"];
    public IntegrationFeatureBase StorageAreaConnectorsFeature => AllFeatures["StorageAreaConnectors"];
    public IntegrationFeatureBase StorageAreaConnectorsInFeature => AllFeatures["StorageAreaConnectorsIn"];
    public IntegrationFeatureBase StorageAreaConnectorsOutFeature => AllFeatures["StorageAreaConnectorsOut"];
    public IntegrationFeatureBase ResourceStorageAreaConnectorsInFeature => AllFeatures["ResourceStorageAreaConnectorsIn"];
    public IntegrationFeatureBase ResourceStorageAreaConnectorsOutFeature => AllFeatures["ResourceStorageAreaConnectorsOut"];
    public IntegrationFeatureBase ItemStorageFeature => AllFeatures["ItemStorage"];
    public IntegrationFeatureBase ItemStorageLotsFeature => AllFeatures["ItemStorageLots"];
    public Dictionary<string, IntegrationFeatureBase> AllFeatures { get; set; } = new()
    {
            {"Attributes", new AttributesFeature() },
            {"AttributeRangeTables", new AttributeRangeTablesFeature()},
            {"AttributeRangeTableAttrNames", new AttributeRangeTableAttributeNameFeature()},
            {"AttributeRangeTableFrom", new AttributeRangeTableFromFeature()},
            {"AttributeRangeTableTo", new AttributeRangeTableToFeature()},
            {"AttributeRangeTableResources", new AttributeRangeTableResourceFeature()},
            {"AttributeCodeTables", new AttributeCodeTablesFeature()},
            {"AttributeCodeTableAttrNames", new AttributeCodeTableAttributeNameFeature()},
            {"AttributeCodeTableAttrCodes", new AttributeCodeTableAttributeCodeFeature()},
            {"AttributeCodeTableResources", new AttributeCodeTableAssignedResourcesFeature()},
            {"AllowedHelpers", new AllowedHelpersFeature()},
            {"Capabilities", new CapabilitiesFeature()},
            {"CapacityIntervalResources", new CapacityIntervalResourcesFeature()},
            {"CapacityIntervals", new CapacityIntervalsFeature()},
            {"Cells", new CellsFeature()},
            {"CleanoutTriggerTableOpCount", new CleanoutTriggerTableOpCountFeature()},
            {"CleanoutTriggerTableProdUnits", new CleanoutTriggerTableProdUnitsFeature()},
            {"CleanoutTriggerTableResources", new CleanoutTriggerTableResourcesFeature()},
            {"CleanoutTriggerTables", new CleanoutTriggerTablesFeature()},
            {"CleanoutTriggerTableTime", new CleanoutTriggerTableTimeFeature()},
            {"CompatibilityCodes", new CompatibilityCodesFeature()},
            {"CompatibilityCodeTableResources", new CompatibilityCodeTableResourcesFeature()},
            {"CompatibilityCodeTables", new CompatibilityCodeTablesFeature()},
            {"Customers", new CustomersFeature()},
            {"CustomerConnections", new CustomerConnectionsFeature()},
            {"Departments", new DepartmentsFeature()},
            {"Forecasts", new ForecastsFeature()},
            {"ForecastShipments", new ForecastShipmentsFeature()},
            {"Inventories", new InventoriesFeature()},
            {"Items", new ItemsFeature()},
            {"JobActivities", new JobActivitiesFeature()},
            {"JobMaterials", new JobMaterialsFeature()},
            {"JobOperationAttributes", new JobOperationAttributesFeature()},
            {"JobOperations", new JobOperationsFeature()},
            {"JobPaths", new JobPathsFeature()},
            {"JobPathNodes", new JobPathNodesFeature()},
            {"JobProducts", new JobProductsFeature()},
            {"JobResourceCapabilities", new JobResourceCapabilitiesFeature()},
            {"JobResources", new JobResourcesFeature()},
            {"Jobs", new JobsFeature()},
            {"Lots", new LotsFeature()},
            {"ManufacturingOrders", new ManufacturingOrdersFeature()},
            {"Plants", new PlantsFeature()},
            {"PlantWarehouses", new PlantWarehousesFeature()},
            {"ProductRules", new ProductRulesFeature()},
            {"PurchasesToStock", new PurchasesToStockFeature()},
            {"RecurringCapacityIntervals", new RecurringCapacityIntervalsFeature()},
            {"ResourceCapabilities", new ResourceCapabilitiesFeature()},
            {"ResourceConnections", new ResourceConnectionsFeature()},
            {"ResourceConnectors", new ResourceConnectorsFeature()},
            {"Resources", new ResourcesFeature()},
            {"SalesOrderLineDistributions", new SalesOrderLineDistributionsFeature()},
            {"SalesOrderLines", new SalesOrderLinesFeature()},
            {"SalesOrders", new SalesOrdersFeature()},
            {"TransferOrderDistributions", new TransferOrderDistributionsFeature()},
            {"UserFieldDefinitions", new UserFieldDefinitionsFeature()},
            {"Warehouses", new WarehousesFeature()},
            {"Activity Batching", new ActivityBatchingFeature()},
            {"AttributesAdvanced", new AttributesAdvancedFeature()},
            {"CellsAdvanced", new CellsAdvancedFeature()},
            {"Compatibility Group", new CompatibilityGroupFeature()},
            {"DBR", new DBRFeature()},
            {"Financial", new FinancialFeature()},
            {"ForecastsAdvanced", new ForecastsAdvancedFeature()},
            {"LotsAdvanced", new LotsAdvancedFeature()},
            {"Max Delay", new MaxDelayFeature()},
            {"MOBatching", new MOBatchingFeature()},
            {"MRP Advanced", new MRPAdvancedFeature()},
            {"MRP Batching", new MRPBatchingFeature()},
            {"MRP", new MRPFeature()},
            {"Multi Plant", new MultiPlantFeature()},
            {"Overlap", new OverlapFeature()},
            {"Planning", new PlanningFeature()},
            {"Priority", new PriorityFeature()},
            {"ProductRulesAdvanced", new ProductRulesAdvancedFeature()},
            {"Purchase Orders", new PurchaseOrdersFeature()},
            {"Quantity Constraints", new QuantityConstraintsFeature()},
            {"Reported Values", new ReportedValuesFeature()},
            {"Resource Capacity", new ResourceCapacityFeature()},
            {"Resource Staging", new ResourceStagingFeature()},
            {"Routings", new RoutingsFeature()},
            {"SalesOrdersAdvanced", new SalesOrdersAdvancedFeature()},
            {"Schedule Adherance", new ScheduleAdheranceFeature()},
            {"Scrap", new ScrapFeature()},
            {"Setup Advanced", new SetupAdvancedFeature()},
            {"Splitting", new SplittingFeature()},
            {"JobSuccessorManufacturingOrders", new SuccessorMOsFeature()},
            {"Tank", new TankFeature()},
            {"TransferOrders", new TransferOrderFeature()},
            {"Transfer Span", new TransferSpanFeature()},
            {"Notes", new NotesFeature()},
            {"UDF", new UDFFeature()},
            {"StorageAreas", new StorageAreaFeature()},
            {"StorageAreaConnectors", new StorageAreaConnectorsFeature()},
            {"StorageAreaConnectorsIn", new StorageAreaConnectorsInFeature()},
            {"StorageAreaConnectorsOut", new StorageAreaConnectorsOutFeature()},
            {"ResourceStorageAreaConnectorsIn", new ResourceStorageAreaConnectorsInFeature()},
            {"ResourceStorageAreaConnectorsOut", new ResourceStorageAreaConnectorsOutFeature()},
            {"ItemStorage", new ItemStorageFeature()},
            {"ItemStorageLots", new ItemStorageLotsFeature()},
            {"ItemStorageDisposal", new ItemStorageDisposalFeature()},
    };
    public IntegrationFeatureBase GetFeatureByName(string a_featureName)
    {
        if (AllFeatures.TryGetValue(a_featureName, out IntegrationFeatureBase feature))
        {
            return feature;
        }

        return null;
    }

    public IEnumerable<IntegrationProperty> GetPropertiesByNameAndTableName(string a_propertyName, string a_tableName)
    {
        return AllFeatures
               .Select(f => f.Value.Properties)
               .Aggregate((a, b) 
                   => a.Concat(b).ToList())
               .Where( p => p.ColumnName == a_propertyName && p.TableName == a_tableName);
    }

    /// <summary>
    /// Checks all features for whether the property provided has its dependencies satisfied. If not, returns the missing ones.
    /// </summary>
    /// <param name="a_propertyToMatch">The property being checked against all features</param>
    /// <param name="a_featurePropIsMappedTo">The feature this property is being checked from. Provided to avoid the property seeming dependent on itself.</param>
    /// <returns></returns>
    public bool IsPropertyMissingDependencies(IntegrationProperty a_propertyToMatch, IntegrationFeatureBase a_featurePropIsMappedTo, out List<IntegrationFeatureBase> missingDependencies)
    {
        missingDependencies = new List<IntegrationFeatureBase>();

        foreach (string dep in a_propertyToMatch.FeatureDependencies)
        {
            IntegrationFeatureBase feature = GetFeatureByName(dep);
            if (feature != null &&
                !a_propertyToMatch.TableName.Equals(a_featurePropIsMappedTo.FeatureName) && // Don't add dependency on self (ie - if checking properties on Core Table Feature).
                !feature.Enabled)
            {
                missingDependencies.Add(feature);
            }
        }

        return missingDependencies.Any();
    }

    /// <summary>
    /// Checks all features for invalid properties (current based on any missing dependencies for *required* properties)
    /// </summary>
    /// <param name="o_invalidFeatures"></param>
    /// <returns></returns>
    public bool AreAllFeaturesValid(out List<(IntegrationFeatureBase Feature, List<IntegrationProperty> InvalidProperties)> o_invalidFeatures)
    {
        o_invalidFeatures = new();

        foreach (IntegrationFeatureBase feature in AllFeatures.Values.Where(feature => feature.Enabled))
        {
            IsFeatureValid(o_invalidFeatures, feature);
        }

        return !o_invalidFeatures.Any();
    }

    /// <summary>
    /// TODO: use this as the basis for treelist-level validation
    /// </summary>
    /// <param name="o_invalidFeatures"></param>
    /// <param name="feature"></param>
    public void IsFeatureValid(List<(IntegrationFeatureBase Feature, List<IntegrationProperty> InvalidProperties)> o_invalidFeatures, IntegrationFeatureBase feature)
    {
        List <IntegrationProperty> invalidProperties = new List<IntegrationProperty>();
        foreach (IntegrationProperty property in feature.Properties)
        {
            if (//TODO: Once we add different ways to import settings, we need an "ignore" option that is only valid for non-required props. We can skip those here.
                IsPropertyMissingDependencies(property, feature, out List<IntegrationFeatureBase> missingDependencies))
            {
                invalidProperties.Add(property);
            }
        }

        if (invalidProperties.Any())
        {
            o_invalidFeatures.Add(new ValueTuple<IntegrationFeatureBase, List<IntegrationProperty>>()
            {
                Item1 = feature,
                Item2 = invalidProperties
            });
        }
    }

    public IEnumerable<IntegrationFeatureBase> GetFeaturesWithProperty(IEnumerable<IntegrationProperty> a_invalidProperties, bool a_includeDisabled)
    {
        return AllFeatures.Values.Where(feature => feature.Properties
                                                          .Any(prop => a_invalidProperties.Contains(prop)))
                                                          .Where(feature => a_includeDisabled || feature.Enabled);
    }

    public IEnumerable<IntegrationFeatureBase> GetFeaturesUsingTable(string a_tableName, bool a_includeDisabled)
    {
        return AllFeatures.Values.Where(feature => feature.Properties
                                                          .Any(prop => prop.TableName.Equals(a_tableName, StringComparison.OrdinalIgnoreCase)))
                          .Where(feature => a_includeDisabled || feature.Enabled);
    }

    public IEnumerable<IntegrationProperty> GetPropertiesUsingTable(string a_tableName, bool a_includeDisabled, Func<IntegrationProperty, bool> a_predicate)
    {
        return GetFeaturesUsingTable(a_tableName, a_includeDisabled).SelectMany(f => f.Properties).Where(p => p.TableName == a_tableName && a_predicate.Invoke(p));
    }

    public IEnumerable<IntegrationProperty> GetClearValuePropertiesUsingTable(string a_tableName)
    {
        return GetPropertiesUsingTable(a_tableName, false, (p) => p.SourceOption == EPropertySourceOption.ClearValue);
    }

    /// <summary>
    /// Finds the feature that represents the baseline features for that table (ie the one that contains its required properties).
    /// </summary>
    /// <param name="a_atableTableName"></param>
    /// <returns></returns>
    public IntegrationFeatureBase GetBaselineFeatureForTable(string a_atableTableName)
    {
        // TODO: Confirm this is correct once we've named all features
        return AllFeatures[a_atableTableName];
    }
}