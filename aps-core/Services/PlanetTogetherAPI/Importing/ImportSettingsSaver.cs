using System.ComponentModel;
using System.Xml;

using PT.Common.Localization;
using PT.ImportDefintions;

namespace PT.PlanetTogetherAPI.Importing;

public class ImportSettingsSaver
{
    private const string c_generalSettings = "GeneralSettings";
    private const string c_connections = "Connections";
    private const string c_mappingsNode = "Mappings";
    private const string c_property = "Property";
    private const string c_userSettings = "UserSettings";
    private const string c_plantSettings = "PlantSettings";
    private const string c_deptSettings = "DeptSettings";
    private const string c_machineSettings = "ResourceSettings";
    private const string c_cellSettings = "CellSettings";
    private const string c_machineCapabilityAssignmentSettings = "CapabilityAssignmentSettings";
    private const string c_machineCapabilitySettings = "CapabilitySettings";
    private const string c_allowedHelperSettings = "AllowedHelperSettings";
    private const string c_capacityIntervalSettings = "CapacityIntervalSettings";
    private const string c_capacityIntervalResourceSettings = "CapacityIntervalResourceSettings";
    private const string c_recurringCapacityIntervalSettings = "RecurringCapacityIntervalSettings";
    private const string c_productRulesSettings = "ProductRulesSettings";
    private const string c_attributeSettings = "AttributeSettings";
    private const string c_stepAttributeSetupTableSettings = "AttributeSetupTableSettings";
    private const string c_stepAttributeSetupTableAttSettings = "AttributeSetupTableAttributeNameSettings";
    private const string c_stepAttributeSetupTableFromSettings = "AttributeSetupTableFromRangeSettings";
    private const string c_stepAttributeSetupTableToSettings = "AttributeSetupTableToRangeSettings";
    private const string c_stepAttributeSetupTableResourceSettings = "AttributeSetupTableResourceSettings";
    private const string c_attributeCodeTableSettings = "AttributeCodeTableSetting";
    private const string c_attributeCodeTableAttributeNameSettings = "AttributeCodeTableAttributeNameSetting";
    private const string c_attributeCodeTableAttributeCodesSettings = "AttributeCodeTableAttributeCodesSetting";
    private const string c_attributeCodeTableAssignedResourcesSettings = "AttributeCodeTableAssignedResourcesSetting";
    
    private const string c_compatibilityCodeTablesSettings = "CompatibilityCodeTablesSetting";
    private const string c_compatibilityCodeTablesAssignedResourcesSettings = "CompatibilityCodeTablesAssignedResourcesSetting";
    private const string c_compatibilityCodesSettings = "CompatibilityCodesSetting";

    private const string c_itemSettings = "ItemSettings";
    private const string c_storageAreaSettings = "StorageAreaSettings";
    private const string c_storageAreaConnectorSettings = "StorageAreaConnectorsSettings";
    private const string c_storageAreaConnectorsInSettings = "StorageAreaConnectorsInSettings";
    private const string c_storageAreaConnectorsOutSettings = "StorageAreaConnectorsOutSettings";
    private const string c_resourceStorageAreaConnectorsInSettings = "ResourceStorageAreaConnectorsInSettings";
    private const string c_resourceStorageAreaConnectorsOutSettings = "ResourceStorageAreaConnectorsOutSettings";
    private const string c_itemStorageSettings = "ItemStorageSettings";
    private const string c_itemStorageLotsSettings = "ItemStorageLotsSettings";
    private const string c_warehouseSettings = "WarehouseSettings";
    private const string c_plantWarehouseSettings = "PlantWarehouseSettings";
    private const string c_inventorySettings = "InventorySettings";
    private const string c_lotsSettings = "LotsSettings";
    private const string c_customerSettings = "CustomerSettings";
    private const string c_purchaseToStockSettings = "PurchaseToStockSettings";
    private const string c_salesOrderSettings = "SalesOrderSettings";
    private const string c_salesOrderLineSettings = "SalesOrderLineSettings";
    private const string c_salesOrderLineDistSettings = "SalesOrderLineDistSettings";
    private const string c_forecastSettings = "ForecastSettings";
    private const string c_forecastShipmentSettings = "ForecastShipmentSettings";
    private const string c_transferOrderSettings = "TransferOrderSettings";
    private const string c_transferOrderDistSettings = "TransferOrderDistributionSettings";

    private const string c_jobSettings = "JobSettings";
    private const string c_moSettings = "MoSettings";
    private const string c_resourceOperationSettings = "ResourceOperationSettings";
    private const string c_internalActivitySettings = "InternalActivitySettings";
    private const string c_resourceRequirementSettings = "ResourceRequirementSettings";
    private const string c_requiredCapabilitySettings = "RequiredCapabilitySettings";
    private const string c_materialSettings = "MaterialSettings";
    private const string c_productSettings = "ProductSettings";
    private const string c_opAttributeSettings = "OpAttributeSettings";
    private const string c_successorMOSettings = "SuccessorMoSettings";
    private const string c_pathSettings = "PathSettings";
    private const string c_pathNodeSettings = "PathNodeSettings";
    private const string c_customerConnectionSettings = "CustomerConnectionSettings";
    private const string c_cleanoutTriggerSettings = "CleanoutTriggerSettings";
    private const string c_cleanoutTriggerAssignedResourcesSettings = "CleanoutTriggerAssignedResourcesSettings";
    private const string c_operationCountCleanoutTriggersSettings = "OperationCountCleanoutTriggersSettings";
    private const string c_productionUnitCleanoutTriggersSettings = "ProductionUnitCleanoutTriggersSettings";
    private const string c_timeCleanoutTriggersSettings = "TimeCleanoutTriggersSettings";
    private const string c_itemCleanoutTriggersTableMappings = "ItemCleanoutTriggerTableMappings";

    private const string c_stepStorageAreaItemCleanoutTriggersTableMappings = "StorageAreaItemCleanoutsSettings";
    private const string c_stepStorageAreaCleanTableListTableMappings = "StorageAreaCleanTableListSettings";
    private const string c_stepStorageAreaAssignedResTableMappings = "StorageAreaCleanAssignmentsTaSettings";

    private const string c_resourceConnectorsSettings = "ResourceConnectorsSettings";
    private const string c_resourceConnectionSettings = "ResourceConnectionSettings";
    private const string c_lastRunActivitiesSettings = "LastRunActivitiesSettings";
    private const string c_rootElementId = "Settings";
    private const string c_userFieldSettings = "UserFieldSettings";

    private readonly ImportUtilities m_utility;

    public string SettingsFileWithPath { get; set; }

    private const int c_currentVersion = 4; //Increment this if this file's format changes as necessary.

    public ImportSettingsSaver(string a_settingsFilePath, ImportUtilities a_utility)
    {
        SettingsFileWithPath = a_settingsFilePath;
        m_utility = a_utility;
    }

    public void SaveImportSettings(ImportSettings a_settings)
    {
        XmlDocument xmlDoc = NewSettingsXmlDoc();

        a_settings.VERSION = c_currentVersion;

        //Save each object separately to a separate xml node
        SaveNode(xmlDoc, a_settings, c_generalSettings);
        SaveNode(xmlDoc, a_settings.UserSettings, c_userSettings);
        SaveNode(xmlDoc, a_settings.PlantSettings, c_plantSettings);
        SaveNode(xmlDoc, a_settings.DepartmentSettings, c_deptSettings);
        SaveNode(xmlDoc, a_settings.MachineSettings, c_machineSettings);
        SaveNode(xmlDoc, a_settings.CellsSettings, c_cellSettings);
        SaveNode(xmlDoc, a_settings.CapabilityAssignmentSettings, c_machineCapabilityAssignmentSettings);
        SaveNode(xmlDoc, a_settings.CapabilitySettings, c_machineCapabilitySettings);
        SaveNode(xmlDoc, a_settings.AllowedHelperResourcesSettings, c_allowedHelperSettings);
        SaveNode(xmlDoc, a_settings.CapacityIntervalSettings, c_capacityIntervalSettings);
        SaveNode(xmlDoc, a_settings.CapacityIntervalResourceSettings, c_capacityIntervalResourceSettings);
        SaveNode(xmlDoc, a_settings.RecurringCapacityIntervalSettings, c_recurringCapacityIntervalSettings);
        SaveNode(xmlDoc, a_settings.ProductRulesSettings, c_productRulesSettings);
        SaveNode(xmlDoc, a_settings.AttributeSettings, c_attributeSettings);
        SaveNode(xmlDoc, a_settings.SetupTableAttSettings, c_stepAttributeSetupTableSettings);
        SaveNode(xmlDoc, a_settings.SetupTableAttNameSettings, c_stepAttributeSetupTableAttSettings);
        SaveNode(xmlDoc, a_settings.SetupTableAttFromSettings, c_stepAttributeSetupTableFromSettings);
        SaveNode(xmlDoc, a_settings.SetupTableAttToSettings, c_stepAttributeSetupTableToSettings);
        SaveNode(xmlDoc, a_settings.SetupTableAttResourceSettings, c_stepAttributeSetupTableResourceSettings);
        
        SaveNode(xmlDoc, a_settings.CompatibilityCodeTablesSettings, c_compatibilityCodeTablesSettings);
        SaveNode(xmlDoc, a_settings.CompatibilityCodeTablesAssignedResourcesSettings, c_compatibilityCodeTablesAssignedResourcesSettings);
        SaveNode(xmlDoc, a_settings.CompatibilityCodesSettings, c_compatibilityCodesSettings);

        SaveNode(xmlDoc, a_settings.AttributeCodeTableSetting, c_attributeCodeTableSettings);
        SaveNode(xmlDoc, a_settings.AttributeCodeTableAttributeExternalIdSetting, c_attributeCodeTableAttributeNameSettings);
        SaveNode(xmlDoc, a_settings.AttributeCodeTableAttributeCodesSetting, c_attributeCodeTableAttributeCodesSettings);
        SaveNode(xmlDoc, a_settings.AttributeCodeTableAssignedResourcesSetting, c_attributeCodeTableAssignedResourcesSettings);

        SaveNode(xmlDoc, a_settings.WarehouseSettings, c_warehouseSettings);
        SaveNode(xmlDoc, a_settings.PlantWarehouseSettings, c_plantWarehouseSettings);
        SaveNode(xmlDoc, a_settings.InventorySettings, c_inventorySettings);
        SaveNode(xmlDoc, a_settings.LotsSettings, c_lotsSettings);
        SaveNode(xmlDoc, a_settings.ItemSettings, c_itemSettings);
        SaveNode(xmlDoc, a_settings.StorageAreaSettings, c_storageAreaSettings);
        SaveNode(xmlDoc, a_settings.ItemStorageSettings, c_itemStorageSettings);
        SaveNode(xmlDoc, a_settings.ItemStorageLotsSettings, c_itemStorageLotsSettings);

        SaveNode(xmlDoc, a_settings.PurchaseToStockSettings, c_purchaseToStockSettings);
        SaveNode(xmlDoc, a_settings.CustomerSettings, c_customerSettings);
        SaveNode(xmlDoc, a_settings.UserFieldSettings, c_userFieldSettings);

        SaveNode(xmlDoc, a_settings.SalesOrderSettings, c_salesOrderSettings);
        SaveNode(xmlDoc, a_settings.SalesOrderLineSettings, c_salesOrderLineSettings);
        SaveNode(xmlDoc, a_settings.SalesOrderLineDistSettings, c_salesOrderLineDistSettings);
        SaveNode(xmlDoc, a_settings.ForecastSettings, c_forecastSettings);
        SaveNode(xmlDoc, a_settings.ForecastShipmentSettings, c_forecastShipmentSettings);
        SaveNode(xmlDoc, a_settings.TransferOrderSettings, c_transferOrderSettings);
        SaveNode(xmlDoc, a_settings.TransferOrderDistributionSettings, c_transferOrderDistSettings);

        SaveNode(xmlDoc, a_settings.JobSettings, c_jobSettings);
        SaveNode(xmlDoc, a_settings.MoSettings, c_moSettings);
        SaveNode(xmlDoc, a_settings.ResourceOperationSettings, c_resourceOperationSettings);
        SaveNode(xmlDoc, a_settings.ResourceRequirementSettings, c_resourceRequirementSettings);
        SaveNode(xmlDoc, a_settings.RequiredCapabilitySettings, c_requiredCapabilitySettings);
        SaveNode(xmlDoc, a_settings.InternalActivitySettings, c_internalActivitySettings);
        SaveNode(xmlDoc, a_settings.MaterialSettings, c_materialSettings);
        SaveNode(xmlDoc, a_settings.ProductSettings, c_productSettings);
        SaveNode(xmlDoc, a_settings.OpAttributeSettings, c_opAttributeSettings);
        SaveNode(xmlDoc, a_settings.SuccessorMoSettings, c_successorMOSettings);
        SaveNode(xmlDoc, a_settings.PathSettings, c_pathSettings);
        SaveNode(xmlDoc, a_settings.PathNodeSettings, c_pathNodeSettings);
        SaveNode(xmlDoc, a_settings.CustomerConnectionSettings, c_customerConnectionSettings);
        SaveNode(xmlDoc, a_settings.CleanoutTriggerTablesSettings, c_cleanoutTriggerSettings);
        SaveNode(xmlDoc, a_settings.CleanoutTriggerTablesAssignedResourcesSettings, c_cleanoutTriggerAssignedResourcesSettings);
        SaveNode(xmlDoc, a_settings.StorageAreaItemCleanoutTablesSettings, c_itemCleanoutTriggersTableMappings);
        SaveNode(xmlDoc, a_settings.OperationCountCleanoutTriggersSettings, c_operationCountCleanoutTriggersSettings);
        SaveNode(xmlDoc, a_settings.ProductionUnitCleanoutTriggersSettings, c_productionUnitCleanoutTriggersSettings);
        SaveNode(xmlDoc, a_settings.TimeCleanoutTriggersSettings, c_timeCleanoutTriggersSettings);
        
        SaveNode(xmlDoc, a_settings.StorageAreaItemCleanoutTablesSettings, c_stepStorageAreaCleanTableListTableMappings);
        SaveNode(xmlDoc, a_settings.StorageAreaCleanAssignmentSettings, c_stepStorageAreaAssignedResTableMappings);
        SaveNode(xmlDoc, a_settings.StorageAreaItemCleanoutsSettings, c_stepStorageAreaItemCleanoutTriggersTableMappings);
        
        
        SaveNode(xmlDoc, a_settings.ResourceConnectorsSettings, c_resourceConnectorsSettings);
        SaveNode(xmlDoc, a_settings.ResourceConnectionSettings, c_resourceConnectionSettings);
        SaveNode(xmlDoc, a_settings.UserFieldSettings, c_userFieldSettings);

        SaveNode(xmlDoc, a_settings.StorageAreaSettings, c_storageAreaSettings);
        SaveNode(xmlDoc, a_settings.StorageAreaConnectorSettings, c_storageAreaConnectorSettings);
        SaveNode(xmlDoc, a_settings.StorageAreaConnectorInSettings, c_storageAreaConnectorsInSettings);
        SaveNode(xmlDoc, a_settings.StorageAreaConnectorOutSettings, c_storageAreaConnectorsOutSettings);
        SaveNode(xmlDoc, a_settings.ResourceStorageAreaConnectorInSettings, c_resourceStorageAreaConnectorsInSettings);
        SaveNode(xmlDoc, a_settings.ResourceStorageAreaConnectorOutSettings, c_resourceStorageAreaConnectorsOutSettings);

        SaveXmlDocument(xmlDoc);
    }

    /// <summary>
    /// Saves the properties of the specified object to a new node with the specified name.
    /// If the object is a MapStepSettings object then its subvalues are stored too.
    /// </summary>
    private void SaveNode(XmlDocument a_xmlDoc, object a_o, string a_nodeName)
    {
        XmlNode newNode = a_xmlDoc.CreateNode(XmlNodeType.Element, a_nodeName, "");
        XmlNode root = a_xmlDoc.DocumentElement;
        root.AppendChild(newNode);
        SaveProperties(a_xmlDoc, newNode, a_o);
        if (a_o is MapStepSettings)
        {
            SaveMapStepSettings(a_xmlDoc, newNode, (MapStepSettings)a_o);
        }
    }

    /// <summary>
    /// Returns the loaded settings or a new InterfaceSettings object if the Settings file is not found.
    /// </summary>
    /// <returns></returns>
    public ImportSettings LoadSettings()
    {
        ImportSettings loadedSettings = new ();
        loadedSettings.InitializeSettings();

        XmlDocument xmlDoc = OpenSettingsXmlDoc();

        LoadNode(xmlDoc, loadedSettings, c_generalSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.UserSettings, c_userSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.PlantSettings, c_plantSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.DepartmentSettings, c_deptSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.MachineSettings, c_machineSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.CellsSettings, c_cellSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.CapabilityAssignmentSettings, c_machineCapabilityAssignmentSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.CapabilitySettings, c_machineCapabilitySettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.AllowedHelperResourcesSettings, c_allowedHelperSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.CapacityIntervalSettings, c_capacityIntervalSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.CapacityIntervalResourceSettings, c_capacityIntervalResourceSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.RecurringCapacityIntervalSettings, c_recurringCapacityIntervalSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.WarehouseSettings, c_warehouseSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.PlantWarehouseSettings, c_plantWarehouseSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.InventorySettings, c_inventorySettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.LotsSettings, c_lotsSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.SalesOrderSettings, c_salesOrderSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.SalesOrderLineSettings, c_salesOrderLineSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.SalesOrderLineDistSettings, c_salesOrderLineDistSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.ForecastSettings, c_forecastSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.ForecastShipmentSettings, c_forecastShipmentSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.TransferOrderSettings, c_transferOrderSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.TransferOrderDistributionSettings, c_transferOrderDistSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.ItemSettings, c_itemSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.StorageAreaSettings, c_storageAreaSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.ItemStorageSettings, c_itemStorageSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.ItemStorageLotsSettings, c_itemStorageLotsSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.PurchaseToStockSettings, c_purchaseToStockSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.CustomerSettings, c_customerSettings, loadedSettings.VERSION);

        LoadNode(xmlDoc, loadedSettings.ProductRulesSettings, c_productRulesSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.AttributeSettings, c_attributeSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.SetupTableAttSettings, c_stepAttributeSetupTableSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.SetupTableAttNameSettings, c_stepAttributeSetupTableAttSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.SetupTableAttFromSettings, c_stepAttributeSetupTableFromSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.SetupTableAttToSettings, c_stepAttributeSetupTableToSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.SetupTableAttResourceSettings, c_stepAttributeSetupTableResourceSettings, loadedSettings.VERSION);

        LoadNode(xmlDoc, loadedSettings.AttributeCodeTableSetting, c_attributeCodeTableSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.AttributeCodeTableAttributeExternalIdSetting, c_attributeCodeTableAttributeNameSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.AttributeCodeTableAttributeCodesSetting, c_attributeCodeTableAttributeCodesSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.AttributeCodeTableAssignedResourcesSetting, c_attributeCodeTableAssignedResourcesSettings, loadedSettings.VERSION);

        LoadNode(xmlDoc, loadedSettings.CompatibilityCodeTablesSettings, c_compatibilityCodeTablesSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.CompatibilityCodeTablesAssignedResourcesSettings, c_compatibilityCodeTablesAssignedResourcesSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.CompatibilityCodesSettings, c_compatibilityCodesSettings, loadedSettings.VERSION);

        LoadNode(xmlDoc, loadedSettings.JobSettings, c_jobSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.MoSettings, c_moSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.ResourceOperationSettings, c_resourceOperationSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.ResourceRequirementSettings, c_resourceRequirementSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.RequiredCapabilitySettings, c_requiredCapabilitySettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.InternalActivitySettings, c_internalActivitySettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.MaterialSettings, c_materialSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.ProductSettings, c_productSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.OpAttributeSettings, c_opAttributeSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.SuccessorMoSettings, c_successorMOSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.PathSettings, c_pathSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.PathNodeSettings, c_pathNodeSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.CustomerConnectionSettings, c_customerConnectionSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.CleanoutTriggerTablesSettings, c_cleanoutTriggerSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.CleanoutTriggerTablesAssignedResourcesSettings, c_cleanoutTriggerAssignedResourcesSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.StorageAreaItemCleanoutTablesSettings, c_itemCleanoutTriggersTableMappings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.OperationCountCleanoutTriggersSettings, c_operationCountCleanoutTriggersSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.ProductionUnitCleanoutTriggersSettings, c_productionUnitCleanoutTriggersSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.TimeCleanoutTriggersSettings, c_timeCleanoutTriggersSettings, loadedSettings.VERSION);
        
        LoadNode(xmlDoc, loadedSettings.StorageAreaItemCleanoutTablesSettings, c_stepStorageAreaCleanTableListTableMappings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.StorageAreaCleanAssignmentSettings, c_stepStorageAreaAssignedResTableMappings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.StorageAreaItemCleanoutsSettings, c_stepStorageAreaItemCleanoutTriggersTableMappings, loadedSettings.VERSION);

        LoadNode(xmlDoc, loadedSettings.ResourceConnectorsSettings, c_resourceConnectorsSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.ResourceConnectionSettings, c_resourceConnectionSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.UserFieldSettings, c_userFieldSettings, loadedSettings.VERSION);

        LoadNode(xmlDoc, loadedSettings.StorageAreaSettings, c_storageAreaSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.StorageAreaConnectorSettings, c_storageAreaConnectorSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.StorageAreaConnectorInSettings, c_storageAreaConnectorsInSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.StorageAreaConnectorOutSettings, c_storageAreaConnectorsOutSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.ResourceStorageAreaConnectorInSettings, c_resourceStorageAreaConnectorsInSettings, loadedSettings.VERSION);
        LoadNode(xmlDoc, loadedSettings.ResourceStorageAreaConnectorOutSettings, c_resourceStorageAreaConnectorsOutSettings, loadedSettings.VERSION);

        return loadedSettings;
    }

    /// <summary>
    /// Loads the settings for the specified Node into the specified object.
    /// Returns true if found and loaded, else returns false.
    /// </summary>
    private bool LoadNode(XmlDocument a_xmlDoc, object a_o, string a_nodeName, int a_version)
    {
        XmlNodeList nodeList = a_xmlDoc.GetElementsByTagName(a_nodeName);
        XmlNode node;
        if (nodeList != null && nodeList.Count > 0)
        {
            node = nodeList[0];
            LoadPropertiesForNode(node, a_o, a_version, a_nodeName);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Writes the (non-ready-only, non MapStepSettings) settings for the object to the node in the xmldoc.
    /// </summary>
    public void SaveProperties(XmlDocument a_xmlDoc, XmlNode a_newNode, object a_o)
    {
        Type type = a_o.GetType();
        int propCount = TypeDescriptor.GetProperties(type).Count;

        //Store each property's settting to the new node.
        for (int i = 0; i < propCount; i++)
        {
            PropertyDescriptor pd;
            pd = TypeDescriptor.GetProperties(type)[i];
            //Don't want to store the actual mappings here.  That's done separately.
            if (pd.PropertyType != typeof(MapStepSettings) && !pd.IsReadOnly && pd.PropertyType != typeof(string[]))
            {
                object curValue = pd.GetValue(a_o);
                if (curValue != null)
                {
                    SaveSetting(a_xmlDoc, a_newNode, pd.Name, curValue.ToString());
                }
            }
        }
    }

    public void LoadPropertiesForNode(XmlNode a_node, object a_o, int a_version, string a_nodeName)
    {
        Type type = a_o.GetType();
        int propCount = TypeDescriptor.GetProperties(type).Count;

        //Get each property's settings from the xml and place it in the object.
        for (int i = 0; i < propCount; i++)
        {
            PropertyDescriptor pd;
            pd = TypeDescriptor.GetProperties(type)[i];
            //Don't want to get the actual mappings here.  That's done separately.
            if (pd.PropertyType != typeof(MapStepSettings) && !pd.IsReadOnly && pd.PropertyType != typeof(string[]))
            {
                XmlElement element = a_node[pd.Name];
                if (element != null)
                {
                    if (pd.PropertyType == typeof(bool))
                    {
                        if (element.InnerText.ToUpperInvariant() == "TRUE")
                        {
                            pd.SetValue(a_o, true);
                        }
                        else
                        {
                            pd.SetValue(a_o, false);
                        }
                    }
                    else if (pd.PropertyType == typeof(string))
                    {
                        pd.SetValue(a_o, element.InnerText);
                    }
                    else if (pd.PropertyType == typeof(long))
                    {
                        pd.SetValue(a_o, Convert.ToInt64(element.InnerText));
                    }
                    else if (pd.PropertyType == typeof(int))
                    {
                        pd.SetValue(a_o, Convert.ToInt32(element.InnerText));
                    }
                    else if (pd.PropertyType == typeof(double))
                    {
                        pd.SetValue(a_o, Convert.ToDouble(element.InnerText));
                    }
                    else if (pd.PropertyType.BaseType == typeof(Enum))
                    {
                        pd.SetValue(a_o, Enum.Parse(pd.PropertyType, element.InnerText, true));
                    }
                }
            }
        }

        if (a_o is MapStepSettings)
        {
            LoadMapStepSettings(a_node, (MapStepSettings)a_o, a_version, a_nodeName);
        }
    }

    /// <summary>
    /// Saves the field mappings for a MapStep.
    /// </summary>
    private void SaveMapStepSettings(XmlDocument a_xmlDoc, XmlNode a_parentNode, MapStepSettings a_map)
    {
        XmlNode saNode = a_xmlDoc.CreateNode(XmlNodeType.Element, c_mappingsNode, "");
        a_parentNode.AppendChild(saNode);

        for (int i = 0; i < a_map.MapInfos.Count; i++)
        {
            SaveSetting(a_xmlDoc, saNode, a_map.MapInfos[i].PropertyName, a_map.MapInfos[i].SourceExpression);
        }
    }

    /// <summary>
    /// Load field maps from the specified node into the MapStepSettings.
    /// </summary>
    private void LoadMapStepSettings(XmlNode a_node, MapStepSettings a_map, int a_version, string a_nodeName)
    {
        System.Text.StringBuilder errBuilder = new ();

        //Get the Mappings subnode
        XmlNode mappingsNode = a_node[c_mappingsNode];
        if (mappingsNode != null)
        {
            //Iterate through the properties maps, loading the value from the xml node if it exists.
            for (int i = 0; i < a_map.MapInfos.Count; i++)
            {
                string propertyName = a_map.MapInfos[i].PropertyName;
                XmlElement element = mappingsNode[propertyName];
                bool found = false;
                if (element != null)
                {
                    a_map.MapInfos[i].SourceExpression = element.InnerText;
                    found = true;
                }
                else //value was null -- not found in the file
                {
                    if (a_version < 3) //This is when we finished the switch from using the multiple readers to the dataset filling.  The fields ending with "Span" now end with "Hrs".
                    {
                        if (propertyName == "MaxDelayHrs")
                        {
                            string newPropName = "MaxDelay"; //renamed.  This is the old name.
                            element = mappingsNode[newPropName];
                            if (element != null)
                            {
                                a_map.MapInfos[i].SourceExpression = element.InnerText;
                                found = true;
                            }
                        }
                        else if (propertyName == "LeadTime")
                        {
                            string newPropName = "LeadTimeDays"; //renamed.  This is the old name.
                            element = mappingsNode[newPropName];
                            if (element != null)
                            {
                                a_map.MapInfos[i].SourceExpression = element.InnerText;
                                found = true;
                            }
                        }
                        else if (propertyName == "KeepSuccessorsTimeLimitHrs")
                        {
                            string newPropName = "KeepSuccessorsTimeLimit"; //renamed.  This is the old name.
                            element = mappingsNode[newPropName];
                            if (element != null)
                            {
                                a_map.MapInfos[i].SourceExpression = element.InnerText;
                                found = true;
                            }
                        }
                        else if (propertyName.EndsWith("Hrs")) //see if there is a property with the same name but ending with "Span".  If so we'll use that instead.
                        {
                            string newPropName = string.Format("{0}Span", propertyName.Substring(0, propertyName.Length - "Hrs".Length));
                            element = mappingsNode[newPropName];
                            if (element != null)
                            {
                                a_map.MapInfos[i].SourceExpression = element.InnerText;
                                found = true;
                            }
                        }
                        else if (propertyName.EndsWith("Days")) //see if there is a property with the same name but ending with "Span".  If so we'll use that instead.
                        {
                            string newPropName = string.Format("{0}Span", propertyName.Substring(0, propertyName.Length - "Days".Length));
                            element = mappingsNode[newPropName];
                            if (element != null)
                            {
                                a_map.MapInfos[i].SourceExpression = element.InnerText;
                                found = true;
                            }
                        }
                    }
                }

                if (!found) //log it
                {
                    errBuilder.Append(string.Format("\t{0}\r\n", propertyName));
                }
            }

            if (errBuilder.Length > 0)
            {
                m_utility.LogSettingsError(Localizer.GetErrorString("2722", new object[] { a_nodeName, errBuilder.ToString() }));
            }
        }
    }

    /// <summary>
    /// Updates or creates the specified setting to the specified XmlNode.
    /// </summary>
    private void SaveSetting(XmlDocument a_xmlDoc, XmlNode a_entryNode, string a_settingId, string a_settingValue)
    {
        XmlElement element = a_xmlDoc.GetElementById(a_settingId);
        if (element == null)
        {
            element = a_xmlDoc.CreateElement(a_settingId);
        }

        element.InnerText = a_settingValue;
        a_entryNode.AppendChild(element);
    }

    /// <summary>
    /// Returns the settings file or a new xml document if the file is not found or has a problem with its structure.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    private XmlDocument OpenSettingsXmlDoc()
    {
        XmlDocument xmlDoc = new ();
        //If the file does not exist then create it.
        if (!File.Exists(SettingsFileWithPath))
        {
            //Write a warning to the startup log and then create a blank file
            CommonException pte = new APSCommon.PTValidationException("2033", new object[] { SettingsFileWithPath });
            m_utility.LogSettingsError(pte, "");

            return NewSettingsXmlDoc();
        }

        try
        {
            using (FileStream stream = File.Open(SettingsFileWithPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                XmlTextReader reader = new (stream);
                reader.Read();
                xmlDoc.Load(reader);
            }
        }
        catch (Exception e)
        {
            //Problem with the format of the file
            m_utility.LogSettingsError(e, Localizer.GetErrorString("2034", new object[] { SettingsFileWithPath }));

            throw;
        }

        return xmlDoc;
    }

    private XmlDocument NewSettingsXmlDoc()
    {
        XmlDocument xmlDoc = new ();
        //Label with xml version
        xmlDoc.AppendChild(xmlDoc.CreateNode(XmlNodeType.XmlDeclaration, "", ""));
        //Add the root node
        xmlDoc.AppendChild(xmlDoc.CreateNode(XmlNodeType.Element, c_rootElementId, ""));
        return xmlDoc;
    }

    private void SaveXmlDocument(XmlDocument a_settings)
    {
        try
        {
            FileStream stream = File.Open(SettingsFileWithPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            a_settings.Save(stream);
            stream.Close();
        }
        catch (Exception e)
        {
            throw new SettingsSaveException(SettingsFileWithPath, e.Message);
        }
    }
}