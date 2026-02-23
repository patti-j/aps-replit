using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Attributes;
using PT.Common.Collections;
using PT.Common.Debugging;
using PT.PackageDefinitions.Settings.PublishOptions;
using PT.Scheduler.Demand;
using PT.Scheduler.Extensions;
using PT.Scheduler.PackageDefs;
using PT.Scheduler.Schedule.Demand;
using PT.Scheduler.Schedule.Resource.LookupTables;
using PT.Scheduler.Simulation.Extensions;
using PT.Scheduler.Simulation.Extensions.Interfaces;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using Assembly = System.Reflection.Assembly;

namespace PT.Scheduler;

/// <summary>
/// Contains all data related to one copy
/// </summary>
public partial class ScenarioDetail : IScenarioRef, IPTSerializable
{
    private ISystemLogger m_errorReporter;

    #region IPTSerializable Members
    public ScenarioDetail(IReader a_reader) 
    {
        if (a_reader.VersionNumber >= 13005) //Removed m_mobDefManager
        {
            a_reader.Read(out m_clock);
            m_idGen = new BaseIdGenerator(a_reader);
            m_scenarioHistoryManager = new ScenarioHistoryManager(a_reader);

            m_capacityIntervalManager = new CapacityIntervalManager(a_reader, m_idGen);
            m_recurringCapacityIntervalManager = new RecurringCapacityIntervalManager(a_reader, m_idGen);

            m_itemManager = new ItemManager(a_reader, m_idGen);
            m_warehouseManager = new WarehouseManager(a_reader, m_idGen);
            m_purchaseToStockManager = new PurchaseToStockManager(a_reader, m_idGen);

            m_plantManager = new PlantManager(a_reader, m_idGen);
            m_cellManager = new CellManager(a_reader, m_idGen);
            m_customerManager = new CustomerManager(a_reader, m_idGen);

            m_capabilityManager = new CapabilityManager(a_reader, m_idGen);
            m_vesselTypeManager = new VesselTypeManager(a_reader, m_idGen);
            m_dispatcherDefinitionManager = new DispatcherDefinitionManager(a_reader, m_idGen);

            m_jobManager = new JobManager(a_reader, m_idGen);
            m_scenarioOptions = new ScenarioOptions(a_reader);

            m_shopViewResourceOptionsManager = new ShopViewResourceOptionsManager(a_reader);
            m_optimizeSettings = new OptimizeSettings(a_reader);

            m_compressSettings = new OptimizeSettings(a_reader);
            if (a_reader.VersionNumber < 12204)
            {
                //StartTime for compress used to be the end time
                m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();
            }

            m_attributeCodeTableManager = new AttributeCodeTableManager(a_reader);
            m_fromRangeSetManager = new RangeLookup.FromRangeSetsManager(a_reader, m_idGen);

            m_salesOrderManager = new SalesOrderManager(a_reader, m_idGen);
            m_transferOrderManager = new TransferOrderManager(a_reader, m_idGen);

            m_batchManager = new BatchManager(a_reader);
            m_inventoryTransferRuleManager = new Schedule.InventoryManagement.InventoryTransferRuleManager(a_reader);
            m_attributeManager = new AttributeManager(a_reader, m_idGen);

            a_reader.Read(out m_lastScenarioDetailExportTDateTicks);
            a_reader.Read(out m_lastSimulationDateTicks);
            a_reader.Read(out m_nbrOfSimulations);
            m_customizationEnablingList = new Schedule.Customizations.CustomizationEnablingList(a_reader);
            m_onlineMode = new OnlineMode(a_reader);

            m_cleanoutTableManager = new CleanoutTriggerTableManager(a_reader, m_idGen);

            m_resourceConnectorManager = new ResourceConnectorManager(a_reader, m_idGen);

            m_compatibilityCodeTableManager = new CompatibilityCodeTableManager(a_reader, m_idGen);

            m_productRuleManager = new ProductRuleManager(a_reader);

            m_itemCleanoutTableManager = new ItemCleanoutTableManager(a_reader, m_idGen);
        } 
        else if (a_reader.VersionNumber >= 13000)
        {
            a_reader.Read(out m_clock);
            m_idGen = new BaseIdGenerator(a_reader);
            m_scenarioHistoryManager = new ScenarioHistoryManager(a_reader);

            m_capacityIntervalManager = new CapacityIntervalManager(a_reader, m_idGen);
            m_recurringCapacityIntervalManager = new RecurringCapacityIntervalManager(a_reader, m_idGen);

            m_itemManager = new ItemManager(a_reader, m_idGen);
            m_warehouseManager = new WarehouseManager(a_reader, m_idGen);
            m_purchaseToStockManager = new PurchaseToStockManager(a_reader, m_idGen);

            m_plantManager = new PlantManager(a_reader, m_idGen);
            m_cellManager = new CellManager(a_reader, m_idGen);
            m_customerManager = new CustomerManager(a_reader, m_idGen);

            m_capabilityManager = new CapabilityManager(a_reader, m_idGen);
            m_vesselTypeManager = new VesselTypeManager(a_reader, m_idGen);
            m_dispatcherDefinitionManager = new DispatcherDefinitionManager(a_reader, m_idGen);

            m_jobManager = new JobManager(a_reader, m_idGen);
            m_scenarioOptions = new ScenarioOptions(a_reader);

            m_shopViewResourceOptionsManager = new ShopViewResourceOptionsManager(a_reader);
            m_optimizeSettings = new OptimizeSettings(a_reader);

            m_compressSettings = new OptimizeSettings(a_reader);
            if (a_reader.VersionNumber < 12204)
            {
                //StartTime for compress used to be the end time
                m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();
            }

            m_attributeCodeTableManager = new AttributeCodeTableManager(a_reader);
            m_fromRangeSetManager = new RangeLookup.FromRangeSetsManager(a_reader, m_idGen);

            m_salesOrderManager = new SalesOrderManager(a_reader, m_idGen);
            m_transferOrderManager = new TransferOrderManager(a_reader, m_idGen);

            _ = new Simulation.ManufacturingOrderBatchDefinitionManager(a_reader, m_idGen);
            m_batchManager = new BatchManager(a_reader);
            m_inventoryTransferRuleManager = new Schedule.InventoryManagement.InventoryTransferRuleManager(a_reader);
            m_attributeManager = new AttributeManager(a_reader, m_idGen);

            a_reader.Read(out m_lastScenarioDetailExportTDateTicks);
            a_reader.Read(out m_lastSimulationDateTicks);
            a_reader.Read(out m_nbrOfSimulations);
            m_customizationEnablingList = new Schedule.Customizations.CustomizationEnablingList(a_reader);
            m_onlineMode = new OnlineMode(a_reader);

            m_cleanoutTableManager = new CleanoutTriggerTableManager(a_reader, m_idGen);

            m_resourceConnectorManager = new ResourceConnectorManager(a_reader, m_idGen);

            m_compatibilityCodeTableManager = new CompatibilityCodeTableManager(a_reader, m_idGen);

            m_productRuleManager = new ProductRuleManager(a_reader);

            m_itemCleanoutTableManager = new ItemCleanoutTableManager(a_reader, m_idGen);
        }
        else
        {
            if (a_reader.VersionNumber >= 12502)
            {
                a_reader.Read(out m_clock);
                m_idGen = new BaseIdGenerator(a_reader);
                m_scenarioHistoryManager = new ScenarioHistoryManager(a_reader);

                m_capacityIntervalManager = new CapacityIntervalManager(a_reader, m_idGen);
                m_recurringCapacityIntervalManager = new RecurringCapacityIntervalManager(a_reader, m_idGen);

                m_itemManager = new ItemManager(a_reader, m_idGen);
                m_warehouseManager = new WarehouseManager(a_reader, m_idGen);
                m_purchaseToStockManager = new PurchaseToStockManager(a_reader, m_idGen);

                m_plantManager = new PlantManager(a_reader, m_idGen);
                m_cellManager = new CellManager(a_reader, m_idGen);
                m_customerManager = new CustomerManager(a_reader, m_idGen);

                m_capabilityManager = new CapabilityManager(a_reader, m_idGen);
                m_vesselTypeManager = new VesselTypeManager(a_reader, m_idGen);
                m_dispatcherDefinitionManager = new DispatcherDefinitionManager(a_reader, m_idGen);

                m_jobManager = new JobManager(a_reader, m_idGen);
                m_scenarioOptions = new ScenarioOptions(a_reader);

                m_shopViewResourceOptionsManager = new ShopViewResourceOptionsManager(a_reader);
                m_optimizeSettings = new OptimizeSettings(a_reader);

                m_compressSettings = new OptimizeSettings(a_reader);
                if (a_reader.VersionNumber < 12204)
                {
                    //StartTime for compress used to be the end time
                    m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();
                }

                m_attributeCodeTableManager = new AttributeCodeTableManager(a_reader);
                m_fromRangeSetManager = new RangeLookup.FromRangeSetsManager(a_reader, m_idGen);

                m_salesOrderManager = new SalesOrderManager(a_reader, m_idGen);
                m_transferOrderManager = new TransferOrderManager(a_reader, m_idGen);

                _ = new Simulation.ManufacturingOrderBatchDefinitionManager(a_reader, m_idGen);
                m_batchManager = new BatchManager(a_reader);
                m_inventoryTransferRuleManager = new Schedule.InventoryManagement.InventoryTransferRuleManager(a_reader);
                m_attributeManager = new AttributeManager(a_reader, m_idGen);

                a_reader.Read(out m_lastScenarioDetailExportTDateTicks);
                a_reader.Read(out m_lastSimulationDateTicks);
                a_reader.Read(out m_nbrOfSimulations);
                m_customizationEnablingList = new Schedule.Customizations.CustomizationEnablingList(a_reader);
                m_onlineMode = new OnlineMode(a_reader);

                m_cleanoutTableManager = new CleanoutTriggerTableManager(a_reader, m_idGen);

                m_resourceConnectorManager = new ResourceConnectorManager(a_reader, m_idGen);

                m_compatibilityCodeTableManager = new CompatibilityCodeTableManager(a_reader, m_idGen);

                m_productRuleManager = new ProductRuleManager(a_reader);
            }
            else
            {
                if (a_reader.VersionNumber >= 12408)
                {
                    a_reader.Read(out m_clock);
                    m_idGen = new BaseIdGenerator(a_reader);
                    m_scenarioHistoryManager = new ScenarioHistoryManager(a_reader);

                    m_capacityIntervalManager = new CapacityIntervalManager(a_reader, m_idGen);
                    m_recurringCapacityIntervalManager = new RecurringCapacityIntervalManager(a_reader, m_idGen);

                    m_itemManager = new ItemManager(a_reader, m_idGen);
                    m_warehouseManager = new WarehouseManager(a_reader, m_idGen);
                    m_purchaseToStockManager = new PurchaseToStockManager(a_reader, m_idGen);

                    m_plantManager = new PlantManager(a_reader, m_idGen);
                    m_cellManager = new CellManager(a_reader, m_idGen);
                    m_customerManager = new CustomerManager(a_reader, m_idGen);

                    m_capabilityManager = new CapabilityManager(a_reader, m_idGen);
                    m_vesselTypeManager = new VesselTypeManager(a_reader, m_idGen);
                    m_dispatcherDefinitionManager = new DispatcherDefinitionManager(a_reader, m_idGen);

                    m_jobManager = new JobManager(a_reader, m_idGen);
                    m_scenarioOptions = new ScenarioOptions(a_reader);

                    m_shopViewResourceOptionsManager = new ShopViewResourceOptionsManager(a_reader);
                    m_optimizeSettings = new OptimizeSettings(a_reader);

                    m_compressSettings = new OptimizeSettings(a_reader);
                    if (a_reader.VersionNumber < 12204)
                    {
                        //StartTime for compress used to be the end time
                        m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();
                    }

                    m_setupCodeTableManager = new SetupCodeTableManager(a_reader);
                    m_attributeCodeTableManager = new AttributeCodeTableManager(a_reader);
                    m_fromRangeSetManager = new RangeLookup.FromRangeSetsManager(a_reader, m_idGen);

                    m_salesOrderManager = new SalesOrderManager(a_reader, m_idGen);
                    m_transferOrderManager = new TransferOrderManager(a_reader, m_idGen);

                    _ = new Simulation.ManufacturingOrderBatchDefinitionManager(a_reader, m_idGen);
                    m_batchManager = new BatchManager(a_reader);
                    m_inventoryTransferRuleManager = new Schedule.InventoryManagement.InventoryTransferRuleManager(a_reader);
                    m_attributeManager = new AttributeManager(a_reader, m_idGen);

                    a_reader.Read(out m_lastScenarioDetailExportTDateTicks);
                    a_reader.Read(out m_lastSimulationDateTicks);
                    a_reader.Read(out m_nbrOfSimulations);
                    m_customizationEnablingList = new Schedule.Customizations.CustomizationEnablingList(a_reader);
                    m_onlineMode = new OnlineMode(a_reader);

                    m_cleanoutTableManager = new CleanoutTriggerTableManager(a_reader, m_idGen);

                    m_resourceConnectorManager = new ResourceConnectorManager(a_reader, m_idGen);

                    m_compatibilityCodeTableManager = new CompatibilityCodeTableManager(a_reader, m_idGen);

                    m_productRuleManager = new ProductRuleManager(a_reader);
                }
                else
                {
                    m_productRuleManager = new ProductRuleManager();

                    if (a_reader.VersionNumber >= 12407)
                    {
                        a_reader.Read(out m_clock);
                        m_idGen = new BaseIdGenerator(a_reader);
                        m_scenarioHistoryManager = new ScenarioHistoryManager(a_reader);

                        m_capacityIntervalManager = new CapacityIntervalManager(a_reader, m_idGen);
                        m_recurringCapacityIntervalManager = new RecurringCapacityIntervalManager(a_reader, m_idGen);

                        m_itemManager = new ItemManager(a_reader, m_idGen);
                        m_warehouseManager = new WarehouseManager(a_reader, m_idGen);
                        m_purchaseToStockManager = new PurchaseToStockManager(a_reader, m_idGen);

                        m_plantManager = new PlantManager(a_reader, m_idGen);
                        m_cellManager = new CellManager(a_reader, m_idGen);
                        m_customerManager = new CustomerManager(a_reader, m_idGen);

                        m_capabilityManager = new CapabilityManager(a_reader, m_idGen);
                        m_vesselTypeManager = new VesselTypeManager(a_reader, m_idGen);
                        m_dispatcherDefinitionManager = new DispatcherDefinitionManager(a_reader, m_idGen);

                        m_jobManager = new JobManager(a_reader, m_idGen);
                        m_scenarioOptions = new ScenarioOptions(a_reader);

                        m_shopViewResourceOptionsManager = new ShopViewResourceOptionsManager(a_reader);
                        m_optimizeSettings = new OptimizeSettings(a_reader);

                        m_compressSettings = new OptimizeSettings(a_reader);
                        if (a_reader.VersionNumber < 12204)
                        {
                            //StartTime for compress used to be the end time
                            m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();
                        }

                        m_setupCodeTableManager = new SetupCodeTableManager(a_reader);
                        m_attributeCodeTableManager = new AttributeCodeTableManager(a_reader);
                        m_fromRangeSetManager = new RangeLookup.FromRangeSetsManager(a_reader, m_idGen);

                        m_salesOrderManager = new SalesOrderManager(a_reader, m_idGen);
                        m_transferOrderManager = new TransferOrderManager(a_reader, m_idGen);

                        _ = new Simulation.ManufacturingOrderBatchDefinitionManager(a_reader, m_idGen);
                        m_batchManager = new BatchManager(a_reader);
                        m_inventoryTransferRuleManager = new Schedule.InventoryManagement.InventoryTransferRuleManager(a_reader);
                        m_attributeManager = new AttributeManager(a_reader, m_idGen);

                        a_reader.Read(out m_lastScenarioDetailExportTDateTicks);
                        a_reader.Read(out m_lastSimulationDateTicks);
                        a_reader.Read(out m_nbrOfSimulations);
                        m_customizationEnablingList = new Schedule.Customizations.CustomizationEnablingList(a_reader);
                        m_onlineMode = new OnlineMode(a_reader);

                        m_cleanoutTableManager = new CleanoutTriggerTableManager(a_reader, m_idGen);

                        m_resourceConnectorManager = new ResourceConnectorManager(a_reader, m_idGen);

                        m_compatibilityCodeTableManager = new CompatibilityCodeTableManager(a_reader, m_idGen);
                    }
                    else if (a_reader.VersionNumber >= 12320) //Added Cleanout table manager
                    {
                        a_reader.Read(out m_clock);
                        m_idGen = new BaseIdGenerator(a_reader);
                        m_scenarioHistoryManager = new ScenarioHistoryManager(a_reader);

                        m_capacityIntervalManager = new CapacityIntervalManager(a_reader, m_idGen);
                        m_recurringCapacityIntervalManager = new RecurringCapacityIntervalManager(a_reader, m_idGen);

                        m_itemManager = new ItemManager(a_reader, m_idGen);
                        m_warehouseManager = new WarehouseManager(a_reader, m_idGen);
                        m_purchaseToStockManager = new PurchaseToStockManager(a_reader, m_idGen);

                        m_plantManager = new PlantManager(a_reader, m_idGen);
                        m_cellManager = new CellManager(a_reader, m_idGen);
                        m_customerManager = new CustomerManager(a_reader, m_idGen);

                        m_capabilityManager = new CapabilityManager(a_reader, m_idGen);
                        m_vesselTypeManager = new VesselTypeManager(a_reader, m_idGen);
                        m_dispatcherDefinitionManager = new DispatcherDefinitionManager(a_reader, m_idGen);

                        m_jobManager = new JobManager(a_reader, m_idGen);
                        m_scenarioOptions = new ScenarioOptions(a_reader);
                        m_scenarioPublishOptions = new ScenarioPublishOptions(a_reader);

                        m_shopViewResourceOptionsManager = new ShopViewResourceOptionsManager(a_reader);
                        m_optimizeSettings = new OptimizeSettings(a_reader);

                        m_compressSettings = new OptimizeSettings(a_reader);
                        if (a_reader.VersionNumber < 12204)
                        {
                            //StartTime for compress used to be the end time
                            m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();
                        }

                        m_setupCodeTableManager = new SetupCodeTableManager(a_reader);
                        m_attributeCodeTableManager = new AttributeCodeTableManager(a_reader);
                        m_fromRangeSetManager = new RangeLookup.FromRangeSetsManager(a_reader, m_idGen);

                        m_salesOrderManager = new SalesOrderManager(a_reader, m_idGen);
                        m_transferOrderManager = new TransferOrderManager(a_reader, m_idGen);

                        _ = new Simulation.ManufacturingOrderBatchDefinitionManager(a_reader, m_idGen);
                        m_batchManager = new BatchManager(a_reader);
                        m_inventoryTransferRuleManager = new Schedule.InventoryManagement.InventoryTransferRuleManager(a_reader);
                        m_attributeManager = new AttributeManager(a_reader, m_idGen);

                        a_reader.Read(out m_lastScenarioDetailExportTDateTicks);
                        a_reader.Read(out m_lastSimulationDateTicks);
                        a_reader.Read(out m_nbrOfSimulations);
                        m_customizationEnablingList = new Schedule.Customizations.CustomizationEnablingList(a_reader);
                        m_onlineMode = new OnlineMode(a_reader);

                        m_cleanoutTableManager = new CleanoutTriggerTableManager(a_reader, m_idGen);

                        m_resourceConnectorManager = new ResourceConnectorManager(a_reader, m_idGen);

                        m_compatibilityCodeTableManager = new CompatibilityCodeTableManager(a_reader, m_idGen);
                    }
                    else
                    {
                        if (a_reader.VersionNumber >= 12313) //Added Cleanout table manager
                        {
                            a_reader.Read(out m_clock);
                            m_idGen = new BaseIdGenerator(a_reader);
                            m_scenarioHistoryManager = new ScenarioHistoryManager(a_reader);

                            m_capacityIntervalManager = new CapacityIntervalManager(a_reader, m_idGen);
                            m_recurringCapacityIntervalManager = new RecurringCapacityIntervalManager(a_reader, m_idGen);

                            m_itemManager = new ItemManager(a_reader, m_idGen);
                            m_warehouseManager = new WarehouseManager(a_reader, m_idGen);
                            m_purchaseToStockManager = new PurchaseToStockManager(a_reader, m_idGen);

                            m_plantManager = new PlantManager(a_reader, m_idGen);
                            m_cellManager = new CellManager(a_reader, m_idGen);
                            m_customerManager = new CustomerManager(a_reader, m_idGen);

                            m_capabilityManager = new CapabilityManager(a_reader, m_idGen);
                            m_vesselTypeManager = new VesselTypeManager(a_reader, m_idGen);
                            m_dispatcherDefinitionManager = new DispatcherDefinitionManager(a_reader, m_idGen);

                            m_jobManager = new JobManager(a_reader, m_idGen);
                            m_scenarioOptions = new ScenarioOptions(a_reader);
                            m_scenarioPublishOptions = new ScenarioPublishOptions(a_reader);

                            m_shopViewResourceOptionsManager = new ShopViewResourceOptionsManager(a_reader);
                            m_optimizeSettings = new OptimizeSettings(a_reader);

                            m_compressSettings = new OptimizeSettings(a_reader);
                            if (a_reader.VersionNumber < 12204)
                            {
                                //StartTime for compress used to be the end time
                                m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();
                            }

                            m_setupCodeTableManager = new SetupCodeTableManager(a_reader);
                            m_attributeCodeTableManager = new AttributeCodeTableManager(a_reader);
                            m_fromRangeSetManager = new RangeLookup.FromRangeSetsManager(a_reader, m_idGen);

                            m_salesOrderManager = new SalesOrderManager(a_reader, m_idGen);
                            m_transferOrderManager = new TransferOrderManager(a_reader, m_idGen);

                            _ = new Simulation.ManufacturingOrderBatchDefinitionManager(a_reader, m_idGen);
                            m_batchManager = new BatchManager(a_reader);
                            m_inventoryTransferRuleManager = new Schedule.InventoryManagement.InventoryTransferRuleManager(a_reader);
                            m_attributeManager = new AttributeManager(a_reader, m_idGen);

                            a_reader.Read(out m_lastScenarioDetailExportTDateTicks);
                            a_reader.Read(out m_lastSimulationDateTicks);
                            a_reader.Read(out m_nbrOfSimulations);
                            m_customizationEnablingList = new Schedule.Customizations.CustomizationEnablingList(a_reader);
                            m_onlineMode = new OnlineMode(a_reader);

                            m_cleanoutTableManager = new CleanoutTriggerTableManager(a_reader, m_idGen);

                            m_resourceConnectorManager = new ResourceConnectorManager(a_reader, m_idGen);
                        }
                        else
                        {
                            if (a_reader.VersionNumber >= 12305) //Added Cleanout table manager
                            {
                                a_reader.Read(out m_clock);
                                m_idGen = new BaseIdGenerator(a_reader);
                                m_scenarioHistoryManager = new ScenarioHistoryManager(a_reader);

                                m_capacityIntervalManager = new CapacityIntervalManager(a_reader, m_idGen);
                                m_recurringCapacityIntervalManager = new RecurringCapacityIntervalManager(a_reader, m_idGen);

                                m_itemManager = new ItemManager(a_reader, m_idGen);
                                m_warehouseManager = new WarehouseManager(a_reader, m_idGen);
                                m_purchaseToStockManager = new PurchaseToStockManager(a_reader, m_idGen);

                                m_plantManager = new PlantManager(a_reader, m_idGen);
                                m_cellManager = new CellManager(a_reader, m_idGen);
                                m_customerManager = new CustomerManager(a_reader, m_idGen);

                                m_capabilityManager = new CapabilityManager(a_reader, m_idGen);
                                m_vesselTypeManager = new VesselTypeManager(a_reader, m_idGen);
                                m_dispatcherDefinitionManager = new DispatcherDefinitionManager(a_reader, m_idGen);

                                m_jobManager = new JobManager(a_reader, m_idGen);
                                m_scenarioOptions = new ScenarioOptions(a_reader);
                                m_scenarioPublishOptions = new ScenarioPublishOptions(a_reader);

                                m_shopViewResourceOptionsManager = new ShopViewResourceOptionsManager(a_reader);
                                m_optimizeSettings = new OptimizeSettings(a_reader);

                                m_compressSettings = new OptimizeSettings(a_reader);
                                if (a_reader.VersionNumber < 12204)
                                {
                                    //StartTime for compress used to be the end time
                                    m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();
                                }

                                m_setupCodeTableManager = new SetupCodeTableManager(a_reader);
                                m_attributeCodeTableManager = new AttributeCodeTableManager(a_reader);
                                m_fromRangeSetManager = new RangeLookup.FromRangeSetsManager(a_reader, m_idGen);

                                m_salesOrderManager = new SalesOrderManager(a_reader, m_idGen);
                                m_transferOrderManager = new TransferOrderManager(a_reader, m_idGen);

                                _ = new Simulation.ManufacturingOrderBatchDefinitionManager(a_reader, m_idGen);
                                m_batchManager = new BatchManager(a_reader);
                                m_inventoryTransferRuleManager = new Schedule.InventoryManagement.InventoryTransferRuleManager(a_reader);
                                m_attributeManager = new AttributeManager(a_reader, m_idGen);

                                a_reader.Read(out m_lastScenarioDetailExportTDateTicks);
                                a_reader.Read(out m_lastSimulationDateTicks);
                                a_reader.Read(out m_nbrOfSimulations);
                                m_customizationEnablingList = new Schedule.Customizations.CustomizationEnablingList(a_reader);
                                m_onlineMode = new OnlineMode(a_reader);

                                m_cleanoutTableManager = new CleanoutTriggerTableManager(a_reader, m_idGen);
                            }
                            else
                            {
                                if (a_reader.VersionNumber >= 12303) //Added Attribute manager
                                {
                                    a_reader.Read(out m_clock);
                                    m_idGen = new BaseIdGenerator(a_reader);
                                    m_scenarioHistoryManager = new ScenarioHistoryManager(a_reader);

                                    m_capacityIntervalManager = new CapacityIntervalManager(a_reader, m_idGen);
                                    m_recurringCapacityIntervalManager = new RecurringCapacityIntervalManager(a_reader, m_idGen);

                                    m_itemManager = new ItemManager(a_reader, m_idGen);
                                    m_warehouseManager = new WarehouseManager(a_reader, m_idGen);
                                    m_purchaseToStockManager = new PurchaseToStockManager(a_reader, m_idGen);

                                    m_plantManager = new PlantManager(a_reader, m_idGen);
                                    m_cellManager = new CellManager(a_reader, m_idGen);
                                    m_customerManager = new CustomerManager(a_reader, m_idGen);

                                    m_capabilityManager = new CapabilityManager(a_reader, m_idGen);
                                    m_vesselTypeManager = new VesselTypeManager(a_reader, m_idGen);
                                    m_dispatcherDefinitionManager = new DispatcherDefinitionManager(a_reader, m_idGen);

                                    m_jobManager = new JobManager(a_reader, m_idGen);
                                    m_scenarioOptions = new ScenarioOptions(a_reader);
                                    m_scenarioPublishOptions = new ScenarioPublishOptions(a_reader);

                                    m_shopViewResourceOptionsManager = new ShopViewResourceOptionsManager(a_reader);
                                    m_optimizeSettings = new OptimizeSettings(a_reader);

                                    m_compressSettings = new OptimizeSettings(a_reader);
                                    if (a_reader.VersionNumber < 12204)
                                    {
                                        //StartTime for compress used to be the end time
                                        m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();
                                    }

                                    m_setupCodeTableManager = new SetupCodeTableManager(a_reader);
                                    m_attributeCodeTableManager = new AttributeCodeTableManager(a_reader);
                                    m_fromRangeSetManager = new RangeLookup.FromRangeSetsManager(a_reader, m_idGen);

                                    m_salesOrderManager = new SalesOrderManager(a_reader, m_idGen);
                                    m_transferOrderManager = new TransferOrderManager(a_reader, m_idGen);

                                   _ = new Simulation.ManufacturingOrderBatchDefinitionManager(a_reader, m_idGen);
                                    m_batchManager = new BatchManager(a_reader);
                                    m_inventoryTransferRuleManager = new Schedule.InventoryManagement.InventoryTransferRuleManager(a_reader);
                                    m_attributeManager = new AttributeManager(a_reader, m_idGen);

                                    a_reader.Read(out m_lastScenarioDetailExportTDateTicks);
                                    a_reader.Read(out m_lastSimulationDateTicks);
                                    a_reader.Read(out m_nbrOfSimulations);
                                    m_customizationEnablingList = new Schedule.Customizations.CustomizationEnablingList(a_reader);
                                    m_onlineMode = new OnlineMode(a_reader);
                                }
                                else if (a_reader.VersionNumber >= 12025) //Removed Alerts manager
                                {
                                    a_reader.Read(out m_clock);
                                    m_idGen = new BaseIdGenerator(a_reader);
                                    m_scenarioHistoryManager = new ScenarioHistoryManager(a_reader);

                                    m_capacityIntervalManager = new CapacityIntervalManager(a_reader, m_idGen);
                                    m_recurringCapacityIntervalManager = new RecurringCapacityIntervalManager(a_reader, m_idGen);

                                    m_itemManager = new ItemManager(a_reader, m_idGen);
                                    m_warehouseManager = new WarehouseManager(a_reader, m_idGen);
                                    m_purchaseToStockManager = new PurchaseToStockManager(a_reader, m_idGen);

                                    m_plantManager = new PlantManager(a_reader, m_idGen);
                                    m_cellManager = new CellManager(a_reader, m_idGen);
                                    m_customerManager = new CustomerManager(a_reader, m_idGen);

                                    m_capabilityManager = new CapabilityManager(a_reader, m_idGen);
                                    m_vesselTypeManager = new VesselTypeManager(a_reader, m_idGen);
                                    m_dispatcherDefinitionManager = new DispatcherDefinitionManager(a_reader, m_idGen);

                                    m_jobManager = new JobManager(a_reader, m_idGen);
                                    m_scenarioOptions = new ScenarioOptions(a_reader);
                                    m_scenarioPublishOptions = new ScenarioPublishOptions(a_reader);

                                    m_shopViewResourceOptionsManager = new ShopViewResourceOptionsManager(a_reader);
                                    m_optimizeSettings = new OptimizeSettings(a_reader);

                                    m_compressSettings = new OptimizeSettings(a_reader);
                                    if (a_reader.VersionNumber < 12204)
                                    {
                                        //StartTime for compress used to be the end time
                                        m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();
                                    }

                                    m_setupCodeTableManager = new SetupCodeTableManager(a_reader);
                                    m_attributeCodeTableManager = new AttributeCodeTableManager(a_reader);
                                    m_fromRangeSetManager = new RangeLookup.FromRangeSetsManager(a_reader, m_idGen);

                                    m_salesOrderManager = new SalesOrderManager(a_reader, m_idGen);
                                    m_transferOrderManager = new TransferOrderManager(a_reader, m_idGen);

                                    a_reader.Read(out int moBatchCountThatShouldAlwaysBeZero);
                                    if (moBatchCountThatShouldAlwaysBeZero != 0)
                                    {
                                        //This is just a side effect, the real serialization error happened above.
                                        DebugException.ThrowInDebug("Serialization Error");
                                    }
                                    //m_mobDefManager = new Simulation.ManufacturingOrderBatchDefinitionManager(a_reader, m_idGen);
                                    m_batchManager = new BatchManager(a_reader);
                                    m_inventoryTransferRuleManager = new Schedule.InventoryManagement.InventoryTransferRuleManager(a_reader);

                                    a_reader.Read(out m_lastScenarioDetailExportTDateTicks);
                                    a_reader.Read(out m_lastSimulationDateTicks);
                                    a_reader.Read(out m_nbrOfSimulations);
                                    m_customizationEnablingList = new Schedule.Customizations.CustomizationEnablingList(a_reader);
                                    m_onlineMode = new OnlineMode(a_reader);
                                }
                                else if (a_reader.VersionNumber >= 737)
                                {
                                    a_reader.Read(out m_clock);
                                    m_idGen = new BaseIdGenerator(a_reader);
                                    m_scenarioHistoryManager = new ScenarioHistoryManager(a_reader);

                                    m_capacityIntervalManager = new CapacityIntervalManager(a_reader, m_idGen);
                                    m_recurringCapacityIntervalManager = new RecurringCapacityIntervalManager(a_reader, m_idGen);

                                    m_itemManager = new ItemManager(a_reader, m_idGen);
                                    m_warehouseManager = new WarehouseManager(a_reader, m_idGen);
                                    m_purchaseToStockManager = new PurchaseToStockManager(a_reader, m_idGen);

                                    m_plantManager = new PlantManager(a_reader, m_idGen);
                                    m_cellManager = new CellManager(a_reader, m_idGen);
                                    m_customerManager = new CustomerManager(a_reader, m_idGen);

                                    m_capabilityManager = new CapabilityManager(a_reader, m_idGen);
                                    m_vesselTypeManager = new VesselTypeManager(a_reader, m_idGen);
                                    m_dispatcherDefinitionManager = new DispatcherDefinitionManager(a_reader, m_idGen);

                                    m_jobManager = new JobManager(a_reader, m_idGen);
                                    m_scenarioOptions = new ScenarioOptions(a_reader);
                                    m_scenarioPublishOptions = new ScenarioPublishOptions(a_reader);

                                    m_shopViewResourceOptionsManager = new ShopViewResourceOptionsManager(a_reader);
                                    m_optimizeSettings = new OptimizeSettings(a_reader);

                                    m_compressSettings = new OptimizeSettings(a_reader);
                                    //StartTime for compress used to be the end time
                                    m_compressSettings.SetBackwardsCompatibilityForStartEndTimes();

                                    m_setupCodeTableManager = new SetupCodeTableManager(a_reader);
                                    m_attributeCodeTableManager = new AttributeCodeTableManager(a_reader);
                                    m_fromRangeSetManager = new RangeLookup.FromRangeSetsManager(a_reader, m_idGen);

                                    m_salesOrderManager = new SalesOrderManager(a_reader, m_idGen);
                                    m_transferOrderManager = new TransferOrderManager(a_reader, m_idGen);

                                    _ = new Simulation.ManufacturingOrderBatchDefinitionManager(a_reader, m_idGen);
                                    m_batchManager = new BatchManager(a_reader);
                                    m_inventoryTransferRuleManager = new Schedule.InventoryManagement.InventoryTransferRuleManager(a_reader);

                                    a_reader.Read(out m_lastScenarioDetailExportTDateTicks);
                                    a_reader.Read(out m_lastSimulationDateTicks);
                                    a_reader.Read(out m_nbrOfSimulations);
                                    m_customizationEnablingList = new Schedule.Customizations.CustomizationEnablingList(a_reader);
                                    m_onlineMode = new OnlineMode(a_reader);
                                }

                                m_attributeManager = new AttributeManager(m_idGen);
                                m_cleanoutTableManager = new CleanoutTriggerTableManager(m_idGen);
                            }

                            m_resourceConnectorManager = new ResourceConnectorManager(m_idGen);
                        }

                        m_compatibilityCodeTableManager = new CompatibilityCodeTableManager(m_idGen);
                    }
                }

                ConvertSetupCodeTablesToAttributeCodeTables();
            }

            m_itemCleanoutTableManager = new(m_idGen);
        }

        m_optimizeSettings.VerifyLicenseConstraintsForOptimizeSettings();

#if TEST
                InitNonSerializedSimulationMembers();

                mt_desyncTest = new DesyncDebuggingResult(reader);
                CompareDesyncResults();
#endif
    }

    private void ConvertSetupCodeTablesToAttributeCodeTables()
    {
        if (m_setupCodeTableManager.Count == 0)
        {
            return;
        }

        for (var i = 0; i < m_setupCodeTableManager.Count; i++)
        {
            m_attributeCodeTableManager.ConvertSetupCodeTableForBackwardsCompatibility(m_setupCodeTableManager[i], m_attributeManager);
        }
    }


    public ScenarioDetail(IReader a_reader, out ScenarioPublishOptions o_scenarioPublishOptions) : this(a_reader)
    {
        o_scenarioPublishOptions = m_scenarioPublishOptions;
    }

    public void Serialize(IWriter a_writer)
    {
#if DEBUG
        a_writer.DuplicateErrorCheck(this);
#endif
        a_writer.Write(m_clock);
        m_idGen.Serialize(a_writer);
        m_scenarioHistoryManager.Serialize(a_writer);

        m_capacityIntervalManager.Serialize(a_writer);
        m_recurringCapacityIntervalManager.Serialize(a_writer);

        m_itemManager.Serialize(a_writer);
        m_warehouseManager.Serialize(a_writer);
        m_purchaseToStockManager.Serialize(a_writer);

        m_plantManager.Serialize(a_writer);
        m_cellManager.Serialize(a_writer);
        m_customerManager.Serialize(a_writer);

        m_capabilityManager.Serialize(a_writer);
        m_vesselTypeManager.Serialize(a_writer);
        m_dispatcherDefinitionManager.Serialize(a_writer);

        m_jobManager.Serialize(a_writer);
        m_scenarioOptions.Serialize(a_writer);

        m_shopViewResourceOptionsManager.Serialize(a_writer);
        m_optimizeSettings.Serialize(a_writer);

        m_compressSettings.Serialize(a_writer);
        m_attributeCodeTableManager.Serialize(a_writer);

        m_fromRangeSetManager.Serialize(a_writer);
        m_salesOrderManager.Serialize(a_writer);
        m_transferOrderManager.Serialize(a_writer);

        m_batchManager.Serialize(a_writer);
        InventoryTransferRuleManager.Serialize(a_writer);
        m_attributeManager.Serialize(a_writer);

        a_writer.Write(m_lastScenarioDetailExportTDateTicks);
        a_writer.Write(m_lastSimulationDateTicks);
        a_writer.Write(m_nbrOfSimulations);
        m_customizationEnablingList.Serialize(a_writer);
        m_onlineMode.Serialize(a_writer);

        m_cleanoutTableManager.Serialize(a_writer);

        m_resourceConnectorManager.Serialize(a_writer);

        m_compatibilityCodeTableManager.Serialize(a_writer);

        m_productRuleManager.Serialize(a_writer);

        m_itemCleanoutTableManager.Serialize(a_writer);

        #if TEST
            DesyncDebuggingResult deleteMe = DesyncDebugging.CalcPrimitiveNumericSum(this);
            deleteMe.Serialize(writer);
        #endif
    }

    public const int UNIQUE_ID = 410;

    public int UniqueId => UNIQUE_ID;

    internal void RestoreReferences(int serializationVersionNumber, Scenario a_scenario, bool aStartup, IPackageManager a_packageManager, ISystemLogger a_errorReporter, ExtensionController a_extensionController)
    {
        m_errorReporter = a_errorReporter;
        m_extensionController = a_extensionController;

        // Customization references
        SetEnablingForCustomizations();

        _scenario = a_scenario;
        m_scenarioHistoryManager.RestoreReferences(this);

        //Restore Scenario and ScenarioDetail
        m_capabilityManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_capacityIntervalManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_cellManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_customerManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_dispatcherDefinitionManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_itemManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_jobManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_plantManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_salesOrderManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_transferOrderManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_recurringCapacityIntervalManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_warehouseManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_vesselTypeManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_attributeCodeTableManager.RestoreReferences(a_errorReporter);
        m_cleanoutTableManager.RestoreReferences(a_errorReporter);
        m_compatibilityCodeTableManager.RestoreReferences(a_errorReporter);
        m_itemCleanoutTableManager.RestoreReferences(a_errorReporter);

        if (serializationVersionNumber < 12050)
        {
            // For backwards compatibility. We changed this from a flag enum to an int enum
            m_plantManager.SetDepartmentFrozenSpanForLegacyScenarios(m_scenarioOptions.LegacyFrozenSpan);
        }

        if (serializationVersionNumber < 12047)
        {
            // For backwards compatibility. We changed this from a flag enum to an int enum
            m_scenarioPublishOptions.ExportDestination = EExportDestinations.BasedOnSystemOptions;
        }

        if (serializationVersionNumber < 13013)
        {
            //For backwards compatibility. This is needed after the MRP changes.
            //Updated to 13013 so older scenarios with JIT issues will be fixed now that JIT is recalculated after any eligibility change.
            m_jobManager.RecalculateJitForBackwardsCompatibility = true;
        }

        //Restore additional references with extra objects
        m_warehouseManager.RestoreReferences(this, CustomerManager, m_itemManager);
        m_salesOrderManager.RestoreReferences(this, m_warehouseManager, m_itemManager, m_jobManager, m_customerManager);
        m_transferOrderManager.RestoreReferences(m_warehouseManager, m_itemManager);
        m_purchaseToStockManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_plantManager.RestoreReferences(CustomerManager, m_capabilityManager, m_capacityIntervalManager, m_recurringCapacityIntervalManager, m_dispatcherDefinitionManager, m_warehouseManager, m_itemManager, this);
        m_jobManager.RestoreReferences(m_customerManager, m_plantManager, m_capabilityManager, this, m_warehouseManager, m_itemManager, a_errorReporter);
        m_warehouseManager.RestoreTemplateReferences(m_jobManager); //must do AFTER restoring Job Manager.
        m_shopViewResourceOptionsManager.RestoreReferences(m_plantManager);

        m_batchManager.RestoreReferences(m_jobManager, m_plantManager);

        InventoryTransferRuleManager.RestoreReferences(this);
        m_attributeManager.RestoreReferences(_scenario, this, m_errorReporter);

        m_resourceConnectorManager.RestoreReferences(m_plantManager, this, m_errorReporter);

        if (m_jobManager.ComputeEligibility(false))
        {
            m_batchManager.RemoveDeadBatches();
        }

        m_warehouseManager.RestoreAdjustmentReferences(this); //must do AFTER restoring all other managers.
        
        
        m_warehouseManager.AfterRestoreAdjustmentReferences();
        m_salesOrderManager.AfterRestoreAdjustmentReferences();
        m_transferOrderManager.AfterRestoreAdjustmentReferences();
        m_jobManager.AfterRestoreAdjustmentReferences();

        if (serializationVersionNumber < 13007)
        {
            //For backwards compatibility. Operations did not use to store MR quantities.
            foreach (Job job in m_jobManager)
            {
                foreach (InternalOperation op in job.GetOperations())
                {
                    op.SimulationInitializationOfActivities(op.Activities);
                }
            }
        }

        // A copy of the schedule is written right after the scenario has finished loading. This allows us to verify that the 
        // results of loading the scenario are identical between versions.
        UnitTestHandling();

        Init();

#if TEST
            if (!aStartup)
            {
                InitTestSequence();
            }
#endif
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        m_plantManager.RestoreReferences(a_udfManager);
        m_cellManager.RestoreReferences(a_udfManager);
        m_capacityIntervalManager.RestoreReferences(a_udfManager);
        m_productRuleManager.RestoreReferences(a_udfManager);
        m_resourceConnectorManager.RestoreReferences(a_udfManager);
        m_itemManager.RestoreReferences(a_udfManager);
        m_warehouseManager.RestoreReferences(a_udfManager);
        m_salesOrderManager.RestoreReferences(a_udfManager);
        m_purchaseToStockManager.RestoreReferences(a_udfManager);
        m_transferOrderManager.RestoreReferences(a_udfManager);
        m_jobManager.RestoreReferences(a_udfManager);
        m_customerManager.RestoreReferences(a_udfManager);
    }

    private void SetEnablingForCustomizations()
    {
        //TODO: V12 extension, are there any settings to save? I think this can be deleted and all extension settings would be saved in scenario settings.
    }

    private void AddTestRanges()
    {
        string testName = "DesignPosition";

        if (FromRangeSetManager.Find(testName) != null)
        {
            return;
        }

        decimal[,] ranges =
        {
            { 45401, 45409 },
            { 45410, 45418 },
            { 45510, 45518 },
            { 47201, 47204 },
            { 47205, 47208 },
            { 47209, 47212 },
            { 47213, 47218 },
            { 36810, 36818 },
            { 32001, 32008 },
            { 32009, 32018 }
        };
        int rangeCount = ranges.GetLength(0);

        int[,] setupTimes =
        {
            { 0, 120, 120, 300, 300, 300, 300, 300, 720, 720 },
            { 120, 0, 120, 300, 300, 300, 300, 300, 720, 720 },
            { 120, 120, 0, 300, 300, 300, 300, 300, 720, 720 },
            { 300, 300, 300, 0, 180, 180, 180, 180, 720, 720 },
            { 300, 300, 300, 180, 0, 180, 180, 180, 720, 720 },
            { 300, 300, 300, 180, 180, 0, 180, 180, 720, 720 },
            { 300, 300, 300, 180, 180, 180, 0, 180, 720, 720 },
            { 300, 300, 300, 180, 180, 180, 180, 0, 720, 720 },
            { 720, 720, 720, 720, 720, 720, 720, 720, 0, 120 },
            { 720, 720, 720, 720, 720, 720, 720, 720, 120, 0 }
        };

        RangeLookup.FromRangeSets multiAttFromRangeSet = FromRangeSetManager.Add(testName, "Design Position change over times");

        RangeLookup.FromRangeSet attFromRangeSet = multiAttFromRangeSet.Add(testName, "Design Position", false);

        for (int fromRangeI = 0; fromRangeI < rangeCount; ++fromRangeI)
        {
            decimal fromRangeStart = ranges[fromRangeI, 0];
            decimal fromRangeEnd = ranges[fromRangeI, 1];

            RangeLookup.FromRange attFromRange = new (fromRangeStart, fromRangeEnd);

            for (int toRangeI = 0; toRangeI < rangeCount; ++toRangeI)
            {
                decimal toRangeStart = ranges[toRangeI, 0];
                decimal toRangeEnd = ranges[toRangeI, 1];
                int setupMinutes = setupTimes[fromRangeI, toRangeI];

                RangeLookup.ToRange toRange = new (toRangeStart, toRangeEnd, 0, TimeSpan.FromMinutes(setupMinutes).Ticks);
                attFromRange.Add(toRange);
            }

            attFromRange.DataChangesCompleted();
            attFromRangeSet.Add(attFromRange);
        }

        attFromRangeSet.DataChangesCompleted();
        multiAttFromRangeSet.DataChangesCompleted();

        for (int pI = 0; pI < PlantManager.Count; ++pI)
        {
            Plant p = PlantManager.GetByIndex(pI);
            for (int dI = 0; dI < p.DepartmentCount; ++dI)
            {
                Department d = p.Departments[dI];
                for (int rI = 0; rI < d.Resources.Count; ++rI)
                {
                    Resource res = d.Resources[rI];
                    res.FromToRanges = multiAttFromRangeSet;
                }
            }
        }
    }
    #endregion

    #region Variables
    #region Serialized. Sorted in the order of serialization.
    private long m_clock;
    private readonly BaseIdGenerator m_idGen;
    private readonly ScenarioHistoryManager m_scenarioHistoryManager;

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly CapacityIntervalManager m_capacityIntervalManager;

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly RecurringCapacityIntervalManager m_recurringCapacityIntervalManager;

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly ItemManager m_itemManager;

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly WarehouseManager m_warehouseManager;

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly PurchaseToStockManager m_purchaseToStockManager;

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly PlantManager m_plantManager;

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly CellManager m_cellManager;

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly CustomerManager m_customerManager;

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly CapabilityManager m_capabilityManager;

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly VesselTypeManager m_vesselTypeManager;

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly DispatcherDefinitionManager m_dispatcherDefinitionManager;

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly JobManager m_jobManager;

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly AttributeManager m_attributeManager;

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly ResourceConnectorManager m_resourceConnectorManager;

    private ScenarioOptions m_scenarioOptions;
    [Obsolete("Left here for backward compatibility do not use")]
    private ScenarioPublishOptions m_scenarioPublishOptions;

    private readonly ShopViewResourceOptionsManager m_shopViewResourceOptionsManager;
    private readonly OptimizeSettings m_optimizeSettings;

    private readonly OptimizeSettings m_compressSettings;

    private readonly SetupCodeTableManager m_setupCodeTableManager;
    private readonly AttributeCodeTableManager m_attributeCodeTableManager;
    private readonly ItemCleanoutTableManager m_itemCleanoutTableManager;
    private readonly CleanoutTriggerTableManager m_cleanoutTableManager;
    private readonly RangeLookup.FromRangeSetsManager m_fromRangeSetManager;
    private readonly CompatibilityCodeTableManager m_compatibilityCodeTableManager;
    private readonly ProductRuleManager m_productRuleManager;

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly SalesOrderManager m_salesOrderManager;

    [AfterRestoreReferences.MasterCopyManagerAttribute]
    private readonly TransferOrderManager m_transferOrderManager;

    //[AfterRestoreReferences.MasterCopyManagerAttribute]
    //private readonly Simulation.ManufacturingOrderBatchDefinitionManager m_mobDefManager;

    //public Simulation.ManufacturingOrderBatchDefinitionManager MOBatchDefinitionManager => m_mobDefManager;

    private BatchManager m_batchManager;

    public BatchManager Batches => m_batchManager;

    //LotAllocationRuleManager m_lotAllocationRules;
    //public LotAllocationRuleManager LotAllocationRules
    //{
    //    get { return m_lotAllocationRules; }
    //    private set { m_lotAllocationRules = value; }
    //}

    private readonly Schedule.InventoryManagement.InventoryTransferRuleManager m_inventoryTransferRuleManager = new ();

    public Schedule.InventoryManagement.InventoryTransferRuleManager InventoryTransferRuleManager => m_inventoryTransferRuleManager;
    #endregion

    internal Scenario _scenario;

    public Scenario Scenario => _scenario;

    void IScenarioRef.SetReferences(Scenario scenario, ScenarioDetail scenarioDetail)
    {
        if (_scenario == null)
        {
            _scenario = scenario;
            ScenarioRef.SetRef(this, scenario, this);
        }
    }
    #endregion

    #region Construction
    //TODO: Continue to pass the grant manager down the chain
    // Right now, plants 100% will be licensed in some way, but there's the possibly that other PT Objects will require licensing
    internal ScenarioDetail(IPackageManager a_packageManager, ISystemLogger a_errorReporter)
    {
        m_errorReporter = a_errorReporter;
        m_clock = PTDateTime.UtcNow.RemoveSeconds().Ticks;
        m_idGen = new BaseIdGenerator();

        m_idGen.InitNextId(0);

        m_scenarioHistoryManager = new ScenarioHistoryManager(this);

        m_capacityIntervalManager = new CapacityIntervalManager(m_idGen);
        m_recurringCapacityIntervalManager = new RecurringCapacityIntervalManager(m_idGen);

        m_itemManager = new ItemManager(m_idGen);
        m_warehouseManager = new WarehouseManager(m_idGen);
        m_purchaseToStockManager = new PurchaseToStockManager(m_idGen);

        m_plantManager = new PlantManager(m_idGen);
        m_cellManager = new CellManager(m_idGen);
        m_customerManager = new CustomerManager(m_idGen);

        m_capabilityManager = new CapabilityManager(m_idGen);
        m_vesselTypeManager = new VesselTypeManager(m_idGen);
        m_dispatcherDefinitionManager = new DispatcherDefinitionManager(m_idGen);

        m_jobManager = new JobManager(this, m_idGen);
        m_scenarioOptions = new ScenarioOptions();
        m_scenarioPublishOptions = new ScenarioPublishOptions();

        m_shopViewResourceOptionsManager = new ShopViewResourceOptionsManager();
        m_optimizeSettings = new OptimizeSettings();

        m_compressSettings = new OptimizeSettings();
        m_attributeCodeTableManager = new AttributeCodeTableManager();

        m_fromRangeSetManager = new RangeLookup.FromRangeSetsManager(m_idGen);
        m_salesOrderManager = new SalesOrderManager(m_idGen);
        m_transferOrderManager = new TransferOrderManager(m_idGen);

        //m_mobDefManager = new Simulation.ManufacturingOrderBatchDefinitionManager(m_idGen);
        m_batchManager = new BatchManager();
        m_inventoryTransferRuleManager = new Schedule.InventoryManagement.InventoryTransferRuleManager();
        m_attributeManager = new AttributeManager(m_idGen);

        m_cleanoutTableManager = new CleanoutTriggerTableManager(m_idGen);

        m_resourceConnectorManager = new ResourceConnectorManager(m_idGen);

        m_compatibilityCodeTableManager = new CompatibilityCodeTableManager(m_idGen);

        m_productRuleManager = new ProductRuleManager();

        m_itemCleanoutTableManager = new ItemCleanoutTableManager(m_idGen);

        InitNonSerializedSimulationMembers();
    }

    /// <summary>
    /// If you Create a ScenarioDetail using the parameterless constructor you must call this function after Scenario has completed initialization.
    /// </summary>
    /// <param name="a_scenario"></param>
    /// <param name="a_errorReporter"></param>
    /// <param name="a_extensionController"></param>
    internal void RestoreReferencesForNewScenarioDetail(Scenario a_scenario, ISystemLogger a_errorReporter, ExtensionController a_extensionController)
    {
        m_errorReporter = a_errorReporter;
        ((IScenarioRef)this).SetReferences(a_scenario, null);
        m_extensionController = a_extensionController;

        //Restore Scenario and ScenarioDetail
        m_capabilityManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_capacityIntervalManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_cellManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_customerManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_dispatcherDefinitionManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_itemManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_jobManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_plantManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_purchaseToStockManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_salesOrderManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_transferOrderManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_recurringCapacityIntervalManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_warehouseManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_vesselTypeManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_attributeCodeTableManager.RestoreReferences(a_errorReporter);
        m_attributeManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_resourceConnectorManager.RestoreReferences(_scenario, this, a_errorReporter);
        m_cleanoutTableManager.RestoreReferences(a_errorReporter);
        m_itemCleanoutTableManager.RestoreReferences(a_errorReporter);

        //Restore additional references with extra objects
        m_warehouseManager.RestoreReferences(this, CustomerManager, m_itemManager);
        m_salesOrderManager.RestoreReferences(this, m_warehouseManager, m_itemManager, m_jobManager, m_customerManager);
        m_transferOrderManager.RestoreReferences(m_warehouseManager, m_itemManager);
        m_plantManager.RestoreReferences(CustomerManager, m_capabilityManager, m_capacityIntervalManager, m_recurringCapacityIntervalManager, m_dispatcherDefinitionManager, m_warehouseManager, m_itemManager, this);
        m_jobManager.RestoreReferences(m_customerManager, m_plantManager, m_capabilityManager, this, m_warehouseManager, m_itemManager, a_errorReporter);
        m_warehouseManager.RestoreTemplateReferences(m_jobManager); //must do AFTER restoring Job Manager.
        m_shopViewResourceOptionsManager.RestoreReferences(m_plantManager);

        InventoryTransferRuleManager.RestoreReferences(this);

        if (m_jobManager.ComputeEligibility())
        {
            m_batchManager.RemoveDeadBatches();
        }

        SetEnablingForCustomizations();

        Init();

#if TEST
            InitTestSequence();
#endif
    }

    /// <summary>
    /// Whenever ScenarioDetail is constructed, including through serialization, this function is called after the main construction work has completed.
    /// </summary>
    private void Init()
    {
        NetChangeInit();
    }
    #endregion

    #region Net change
    private Schedule.Analysis.NetChange.ActivityStateManager _activityStateManager;

    private List<Schedule.Analysis.NetChange.ActivityState> _netChangeList;

    public List<Schedule.Analysis.NetChange.ActivityState> NetChangeList => _netChangeList;

    /// <summary>
    /// Call when then scenario is created.
    /// </summary>
    private void NetChangeInit()
    {
        ScenarioPublishNetChange netChangeSettings = new ScenarioPublishNetChange();
        using (_scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary scenarioSummary))
        {
            netChangeSettings = scenarioSummary.ScenarioSettings.LoadSetting(netChangeSettings);
        }

        if (PTSystem.ExporterService && netChangeSettings.NetChangePublishingEnabled)
        {
            _activityStateManager = new Schedule.Analysis.NetChange.ActivityStateManager();

            ScenarioEvents se;
            using (_scenario.ScenarioEventsLock.EnterRead(out se))
            {
                // Create event to listen for the start of a simulation.
                se.SimulationProgressEvent += new ScenarioEvents.SimulationProgressDelegate(se_SimulationProgressEvent);
            }
        }
    }

    private void se_SimulationProgressEvent(ScenarioDetail a_sd, SimulationType a_simType, ScenarioBaseT a_t, long a_simNbr, decimal a_percentComplete, SimulationProgress.Status a_status)
    {
        if (a_status == SimulationProgress.Status.Initializing)
        {
            if (_activityStateManager != null)
            {
                _activityStateManager.GetStartStates(this);
            }
        }
        else if (a_status == SimulationProgress.Status.Complete)
        {
            if (_activityStateManager != null)
            {
                _activityStateManager.GetFinishStates(this);
                _netChangeList = _activityStateManager.DetermineNetChange();
                _activityStateManager.Clear();
            }
        }
    }
    #endregion

    #region Sub-object Accessor Properties
    public PlantManager PlantManager => m_plantManager;

    public WarehouseManager WarehouseManager => m_warehouseManager;

    public ItemManager ItemManager => m_itemManager;

    public PurchaseToStockManager PurchaseToStockManager => m_purchaseToStockManager;

    public CellManager CellManager => m_cellManager;

    public CustomerManager CustomerManager => m_customerManager;

    public CapacityIntervalManager CapacityIntervalManager => m_capacityIntervalManager;

    public RecurringCapacityIntervalManager RecurringCapacityIntervalManager => m_recurringCapacityIntervalManager;

    public CapabilityManager CapabilityManager => m_capabilityManager;

    public VesselTypeManager VesselTypeManager => m_vesselTypeManager;

    public DispatcherDefinitionManager DispatcherDefinitionManager => m_dispatcherDefinitionManager;

    public JobManager JobManager => m_jobManager;

    public ShopViewResourceOptionsManager ShopViewResourceOptionsManager => m_shopViewResourceOptionsManager;

    public ScenarioHistoryManager ScenarioHistoryManager => m_scenarioHistoryManager;

    public AttributeManager AttributeManager => m_attributeManager;

    public ResourceConnectorManager ResourceConnectorManager => m_resourceConnectorManager;


    public DateTime GetPlanningHorizonEnd()
    {
        return new DateTime(Clock).Add(ScenarioOptions.PlanningHorizon);
    }

    internal long GetPlanningHorizonEndTicks()
    {
        return Clock + ScenarioOptions.PlanningHorizon.Ticks;
    }

    public OptimizeSettings OptimizeSettings => m_optimizeSettings;

    public OptimizeSettings CompressSettings => m_compressSettings;

    public SetupCodeTableManager SetupCodeTableManager => m_setupCodeTableManager;

    public CleanoutTriggerTableManager CleanoutTriggerTableManager => m_cleanoutTableManager;

    public AttributeCodeTableManager AttributeCodeTableManager => m_attributeCodeTableManager;
    public ItemCleanoutTableManager ItemCleanoutTableManager => m_itemCleanoutTableManager;

    public RangeLookup.FromRangeSetsManager FromRangeSetManager => m_fromRangeSetManager;
    public CompatibilityCodeTableManager CompatibilityCodeTableManager => m_compatibilityCodeTableManager;
    public ProductRuleManager ProductRuleManager => m_productRuleManager;

    public SalesOrderManager SalesOrderManager => m_salesOrderManager;

    public TransferOrderManager TransferOrderManager => m_transferOrderManager;

    //public Simulation.ManufacturingOrderBatchSetsByDefAndGroup ManufacturingOrderBatchSetsByGroup => m_mobsByGroup;

    /// <summary>
    /// Material requirers that must be fulfilled by specific lots
    /// prevent other requiremnts from consuming material in the lot
    /// until they've all ben fulfilled.
    /// This is used by the simulation to keep track of which lots
    /// are needed as suppliers of some material requirers such
    /// as SalesOrders, TrnasferOrders, and MaterialRequirements.
    /// At the start of the simulation, this is filled with
    /// all the lots that are required by material requirers.
    /// As requirements are fulfilled, the requirements are
    /// removed from this set.
    /// This allows tracking of whether a lot has left over material
    /// that can be used by material requirers that don't specifically
    /// require use of specific lots.
    /// </summary>
    private readonly EligibleLots m_usedAsEligibleLotLotCodes = new ();

    /// <summary>
    /// This is the set of lot codes that are used as eligible lots by
    /// SalesOrders, TransferOrders, or MaterialRequirements.
    /// </summary>
    internal EligibleLots UsedAsEligibleLotsLotCodes => m_usedAsEligibleLotLotCodes;

    /// <summary>
    /// These are existing preserved batches. All of their orders had some part that was before the optimize start date.
    /// </summary>
    //public Simulation.ManufacturingOrderBatchSetsByDefAndGroup ManufacturingOrderBatchSetsByGroupBeforeOptimizeStartDate => m_mobsByGroupBeforeOptimizeStartDate;

    public BaseIdGenerator IdGen => m_idGen;
    #endregion

    #region System and Scenario Options
    internal long Clock => m_clock;
    
    public DateTime ClockDate => new (Clock, DateTimeKind.Utc);

    /// <summary>
    /// Stores a set of options that are copied into each ScenarioDetail and must be kept in sync across Scenarios.
    /// </summary>
    public ScenarioOptions ScenarioOptions
    {
        get => m_scenarioOptions;
        set => m_scenarioOptions = value;
    }


    #endregion

    #region Shared Transmission/ERP Transmission Handling
    /// <summary>
    /// Sets the references between the Resource and its Capabilities.  This replaces any previous references.
    /// Returns an ArrayList of AffectedCapabilties.
    /// </summary>
    /// <param name="m">Resource whose Capabilities list should be updated.</param>
    /// <param name="a_capabilities">Array List of Capabilities.</param>
    private void SetMachineCapabilities(Resource m, List<Capability> a_capabilities, bool autoDeleteAssociations, out ArrayList o_affectedCapabilities, out bool o_jobsAffected)
    {
        o_jobsAffected = false;
        o_affectedCapabilities = new ArrayList();

        if (autoDeleteAssociations)
        {
            //Remove Machine associations from Capabilities that are no longer in the ResourceT's list of Capabilities.
            for (int c = m.CapabilityCount - 1; c >= 0; --c)
            {
                Capability curCapability = m.GetCapabilityByIndex(c);
                if (!a_capabilities.Contains(curCapability))
                {
                    curCapability.RemoveResourceAssociation(m);
                    if (m.DisassociateCapability(curCapability, m_productRuleManager))
                    {
                        o_jobsAffected = true;
                    }

                    o_affectedCapabilities.Add(curCapability);
                }
            }
        }

        //If a Capability from the transmission is new to the Machine then add it to the Machine's list and add a Machine reference to the Capability.
        foreach (Capability newCap in a_capabilities)
        {
            if (m.GetCapabilityById(newCap.Id) == null)
            {
                m.AddCapability(newCap);
                newCap.AddResourceAssociation(m);
                o_affectedCapabilities.Add(newCap);
            }
        }
    }

    /// <summary>
    /// Sets the references between the Capability and its Machines.  This replaces any previous references.
    /// Returns an ArrayList of AffectedMachines.
    /// </summary>
    /// <param name="c">Capabilty whose Machines list should be updated.</param>
    /// <param name="machines">List of machines that should have this Capability.</param>
    private void SetCapabilityMachines(Capability c, ResourceKeyList machines, out ArrayList o_affectedMachines, out bool o_jobsAffected)
    {
        o_jobsAffected = false;
        o_affectedMachines = new ArrayList();

        //Remove Capability associations from Machines that are no longer in the list of Machines.
        for (int m = c.Resources.Count - 1; m >= 0; m--)
        {
            Resource curMachine = (Resource)c.Resources.GetByIndex(m);
            ResourceKeyList.Node node = machines.First;
            bool contained = false;
            while (node != null)
            {
                if (node.Data.Plant == curMachine.Department.Plant.Id && node.Data.Department == curMachine.Department.Id && node.Data.Resource == curMachine.Id)
                {
                    contained = true;
                }

                node = node.Next;
            }

            if (!contained) //Machine should no longer be referenced by this Capability
            {
                c.RemoveResourceAssociation(curMachine);
                if (curMachine.DisassociateCapability(c, m_productRuleManager))
                {
                    o_jobsAffected = true;
                }

                o_affectedMachines.Add(curMachine);
            }
        }

        //If a Machine from the transmission is new to the Capability then add it to the Capability's list and add a Capability reference to the Machine.
        ResourceKeyList.Node mNode = machines.First;
        while (mNode != null)
        {
            if (!c.Resources.Contains(mNode.Data))
            {
                Resource newMachine = FindResource(mNode.Data);
                newMachine.AddCapability(c);
                c.AddResourceAssociation(newMachine);
                o_affectedMachines.Add(newMachine);
            }

            mNode = mNode.Next;
        }
    }
    #endregion

    #region Transmission Handling
    private const int c_maxNbrOfTransmissionTimingEntries = 100;

    [DebugLogging(EDebugLoggingType.None)] private readonly CircularQueue<Common.Testing.Timing> m_transmissionTimingQueue = new (c_maxNbrOfTransmissionTimingEntries);

    public CircularQueue<Common.Testing.Timing> GetCopyOfTransmissionTimingQueue()
    {
        return new CircularQueue<Common.Testing.Timing>(m_transmissionTimingQueue);
    }

#if DEBUG
    [DebugLogging(EDebugLoggingType.None)] private static int __s_transmissionReceivedCount = 0;

    [DebugLogging(EDebugLoggingType.None)] private int __transmissionReceivedCount;
#endif

    private void UpdateAttributeFromRangeSetListReferences(RangeLookup.FromRangeSets list, ResourceKeyList rkl)
    {
        // Clear the resource from all the tables.
        Dictionary<string, Resource> dictionary = PlantManager.GetResourceHash();
        ClearResourceFromToRanges(dictionary, list.Id);

        ResourceKeyList.Node node = rkl.First;

        while (node != null)
        {
            string key = BaseResource.CreateResourceKey(node.Data.Plant, node.Data.Department, node.Data.Resource);

            if (dictionary.ContainsKey(key))
            {
                Resource res = dictionary[key];
                res.FromToRanges = list;
            }

            node = node.Next;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="ht"></param>
    /// <param name="tableId">The id is passed in instead of the reference because the table is recreated and it's id is copied into the new object.</param>
    internal void ClearResourceFromToRanges(Dictionary<string, Resource> dictionary, BaseId tableId)
    {
        Dictionary<string, Resource>.Enumerator resEnum = dictionary.GetEnumerator();

        while (resEnum.MoveNext())
        {
            Resource r = resEnum.Current.Value;
            if (r.FromToRanges != null)
            {
                if (r.FromToRanges.Id == tableId)
                {
                    r.FromToRanges = null;
                }
            }
        }
    }

    internal void ClearResourceFromToRanges(List<Resource> list)
    {
        for (int i = 0; i < list.Count; ++i)
        {
            Resource r = list[i];
            r.FromToRanges = null;
        }
    }

    private void DeleteAllLookupTables(IScenarioDataChanges a_dataChanges)
    {
        FromRangeSetManager.DeleteAll(a_dataChanges);
        List<Resource> resources = PlantManager.GetResourceList();
        ClearResourceFromToRanges(resources);

        AttributeCodeTableManager.DeleteAll(this, a_dataChanges);

        CompatibilityCodeTableManager.DeleteAll(this,a_dataChanges);
        CleanoutTriggerTableManager.DeleteAll(this,a_dataChanges);
    }

    internal void RecurringCapacityIntervalConvertHandler(UserFieldDefinitionManager a_udfManager, RecurringCapacityIntervalConvertT a_t, IScenarioDataChanges a_dataChanges)
    {
        //Get the original CapacityInterval
        CapacityInterval ci = CapacityIntervalManager.GetById(a_t.originalCapacityIntervalId);
        if (ci == null)
        {
            throw new TransmissionValidationException(a_t, "2572", new object[] { a_t.originalCapacityIntervalId.ToString() });
        }

        //Create a new RecurringCapacityInterval by copying the values from the original Capacity Interval and hook it to the same Resources as the CapacityInterval referenced.
        RecurringCapacityInterval rci = RecurringCapacityIntervalManager.AddConvert(a_udfManager, a_t, ci, a_dataChanges);

        //Delete the original CapacityInterval
        CapacityIntervalManager.Receive(a_t, a_dataChanges);

        //NOTE: A constraint change will be run since capacity changed
        a_dataChanges.FlagConstraintChanges(BaseId.NULL_ID);

        //TODO: Do we need to add a resource change here?
        a_dataChanges.CapacityIntervalChanges.DeletedObject(ci.Id);
        a_dataChanges.RecurringCapacityIntervalChanges.AddedObject(rci.Id);
    }

    internal void CapacityIntervalConvertHandler(UserFieldDefinitionManager a_udfManager, CapacityIntervalConvertT a_t, IScenarioDataChanges a_dataChanges)
    {
        //Get the original RecurringCapacityInterval
        RecurringCapacityInterval rci = RecurringCapacityIntervalManager.GetById(a_t.originalRecurringCapacityIntervalId);
        if (rci == null)
        {
            throw new TransmissionValidationException(a_t, "2613", new object[] { a_t.originalRecurringCapacityIntervalId.ToString() });
        }

        //Create a new CapacityInterval by copying the values from the transmission and hook it to the same Resources as the RecurringCapacityInterval referenced.
        CapacityInterval ci = CapacityIntervalManager.AddConvert(a_udfManager, a_t, rci, a_dataChanges);

        //Delete the original RecurringCapacityInterval
        RecurringCapacityIntervalManager.Receive(a_t, a_dataChanges);

        //NOTE: A constraint change will be run since capacity changed
        a_dataChanges.FlagConstraintChanges(BaseId.NULL_ID);
        //TODO: Do we need to add a resource change here?
        a_dataChanges.CapacityIntervalChanges.AddedObject(ci.Id);
        a_dataChanges.RecurringCapacityIntervalChanges.DeletedObject(rci.Id);
    }

    /// <summary>
    /// Set the Capabilities for a specific Machine.
    /// </summary>
    /// <param name="t"></param>
    private void SetMachineCapabilitiesTransmission(ScenarioDetailSetCapabilitiesT t, IScenarioDataChanges a_dataChanges)
    {
        Plant plant = PlantManager.GetById(t.plantId);
        if (plant == null)
        {
            throw new TransmissionValidationException(t, "2615", new object[] { t.plantId.ToString() });
        }

        Department dept = plant.Departments.GetById(t.departmentId);
        if (dept == null)
        {
            throw new TransmissionValidationException(t, "2614", new object[] { t.departmentId.ToString(), plant.Name });
        }

        Resource m = dept.Resources.GetById(t.machineId);
        if (m == null)
        {
            throw new TransmissionValidationException(t, "2616", new object[] { t.machineId.ToString(), dept.Name, plant.Name });
        }

        //Build an ArrayList of the Machine's Capabilities
        List<Capability> capabilities = new ();
        CapabilityKeyList.Node node = t.CapabilityIds.First;
        while (node != null)
        {
            Capability mCap = CapabilityManager.GetById(node.Data);
            if (mCap == null)
            {
                throw new TransmissionValidationException(t, "2617", new object[] { node.Data, m.Name });
            }

            capabilities.Add(mCap);
            node = node.Next;
        }

        //Build ArrayLists to use for updating the UI with changes.
        SetMachineCapabilities(m, capabilities, true, out ArrayList affectedCapabilities, out bool jobsAffected); //Sets the references.
        if (jobsAffected)
        {
            //Need to run constraint change
            a_dataChanges.FlagConstraintChanges(BaseId.NULL_ID);
        }

        a_dataChanges.MachineChanges.UpdatedObject(m.Id);

        if (a_dataChanges.CapabilityChanges.TotalUpdatedObjects > 0)
        {
            m_signals[SignalCriticalResourceUpdateIdx] = true;
        }
    }

    /// <summary>
    /// Set the Machines for a specific Capability.
    /// </summary>
    /// <param name="t"></param>
    private void SetCapabilityMachinesTransmission(ScenarioDetailSetCapabilityResourcesT t, IScenarioDataChanges a_dataChanges)
    {
        Capability c = CapabilityManager.GetById(t.capabilityId);
        if (c == null)
        {
            throw new TransmissionValidationException(t, "2618", new object[] { t.capabilityId.ToString() });
        }

        //Send data changes for affected resources. //TODO: Not sure this is necessary
        ResourceKeyList.Node node = t.machines.First;
        while (node != null)
        {
            a_dataChanges.MachineChanges.UpdatedObject(node.Data.Resource);
            node = node.Next;
        }

        //Build ArrayLists to use for updating the UI with changes.
        SetCapabilityMachines(c, t.machines, out ArrayList affectedMachines, out bool jobsAffected); //Sets the references.
        if (jobsAffected)
        {
            //Need to run constraint change
            a_dataChanges.FlagConstraintChanges(BaseId.NULL_ID);
        }

        a_dataChanges.CapabilityChanges.UpdatedObject(c.Id);

        if (a_dataChanges.MachineChanges.TotalUpdatedObjects > 0)
        {
            m_signals[SignalCriticalResourceUpdateIdx] = true;
        }
    }

    /// <summary>
    /// Throws TransmissionException if the resource isn't found.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    internal Resource FindResource(ResourceKey key)
    {
        Plant plant = PlantManager.GetById(key.Plant);
        if (plant == null)
        {
            throw new TransmissionException("2573", new object[] { key.Plant.ToString() });
        }

        Department dept = plant.Departments.GetById(key.Department);
        if (dept == null)
        {
            throw new TransmissionException("2574", new object[] { key.Department.ToString(), plant.Name });
        }

        Resource m = dept.Resources.GetById(key.Resource);
        if (m == null)
        {
            throw new TransmissionException("2575", new object[] { key.Resource.ToString(), dept.Name, plant.Name });
        }

        return m;
    }

    /// <summary>
    /// Remove the specified types of objects from the scenario.
    /// </summary>
    /// <param name="t"></param>
    private void ClearScenarioData(ScenarioDetailClearT t, IScenarioDataChanges a_dataChanges)
    {
        //Work from the child items up
        if (t.ClearJobs)
        {
            JobManager.Clear(a_dataChanges);
        }

        if (t.ClearCapacityIntervals)
        {
            CapacityIntervalManager.Clear(a_dataChanges);
        }

        if (t.ClearRecurringCapacityIntervals)
        {
            RecurringCapacityIntervalManager.Clear(a_dataChanges);
        }

        if (t.ClearBalancedCompositeRules)
        {
            DispatcherDefinitionManager.Clear(this, a_dataChanges);
        }

        if (t.ClearCells)
        {
            CellManager.Clear(a_dataChanges);
        }

        if (t.ClearCapabilities)
        {
            CapabilityManager.Clear(m_productRuleManager, a_dataChanges);
        }

        if (t.ClearPurchaseToStocks)
        {
            PurchaseToStockManager.Clear(a_dataChanges);
        }

        if (t.ClearSalesOrders)
        {
            SalesOrderManager.Clear(this, t, a_dataChanges);
        }

        if (t.ClearTransferOrders)
        {
            TransferOrderManager.Clear(this, t, a_dataChanges);
        }

        PlantManager.Receive(t, this, a_dataChanges);
        
        WarehouseManager.Receive(t, this, a_dataChanges);

        if (t.ClearItems)
        {
            ItemManager.Clear(this, a_dataChanges);
        }

        if (t.ClearAttributeCodeTables)
        {
            m_attributeCodeTableManager.DeleteAll(this, a_dataChanges);
        }

        if (t.ClearAttributeNumberRangeTables)
        {
            m_fromRangeSetManager.DeleteAll(a_dataChanges);
        }

        if (t.ClearCustomers)
        {
            CustomerManager.ClearCustomers(a_dataChanges);
        }

        using (_scenario.ScenarioEventsLock.EnterRead(out ScenarioEvents se))
        {
            //TODO: DataChanges
            se.FireScenarioDetailClearEvent(t, this);
        }

        if (t.ClearAttributes)
        {
            AttributeManager.ClearPTAttributes(this, t, a_dataChanges);
        }

        if (t.ClearCleanOutTables)
        {
            m_cleanoutTableManager.DeleteAll(this, a_dataChanges);
        }

        if (t.ClearResourceConnectors)
        {
            m_resourceConnectorManager.Clear(a_dataChanges);
        }

        if (t.ClearCompatibilityCodeTables)
        {
            m_compatibilityCodeTableManager.DeleteAll(this, a_dataChanges);
        }

        if (t.ClearProductRules)
        {
            m_productRuleManager.Clear();
        }

        if (t.ClearUserFields)
        {
            using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager sm))
            {
                sm.UserFieldDefinitionManager.Receive(sm, this, t);
            }
        }

        //Fire the ScenarioDetailClearEvent first to give the ui a chance to remove resources prior to TimeAdjustment which may try to update the schedule on resouces that are gone.
        if (t.ClearJobs ||
            t.ClearCapacityIntervals ||
            t.ClearDepartments ||
            t.ClearInventories ||
            t.ClearItems ||
            t.ClearPlants ||
            t.ClearPurchaseToStocks ||
            t.ClearRecurringCapacityIntervals ||
            t.ClearResources ||
            t.ClearWarehouses ||
            t.ClearProductRules ||
            t.ClearAttributeCodeTables ||
            t.ClearAttributeNumberRangeTables ||
            t.ClearResourceConnectors ||
            t.ClearAllowedHelpers ||
            t.ClearForecasts ||
            t.ClearSalesOrders ||
            t.ClearTransferOrders ||
            t.ClearCustomers ||
            t.ClearCompatibilityCodeTables)
        {
            TimeAdjustment(t);
        }
    }

    /// <summary>
    /// Remove the specified types of objects from the scenario.
    /// </summary>
    /// <param name="t"></param>
    private void GeneralizeScenarioData(ScenarioDetailClearT t, IScenarioDataChanges a_dataChanges)
    {
        //Work from the child items up

        Dictionary<string, string> jobRemapDict = new ();
        Dictionary<string, string> capabilityRemapDict = new ();
        Dictionary<string, string> resRemapDict = new ();
        Dictionary<string, string> deptRemapDict = new ();
        Dictionary<string, string> plantRemapDict = new ();
        Dictionary<string, string> itemRemapDict = new ();
        Dictionary<string, string> customerRemapDict = new ();
        Dictionary<string, string> remapDict = new ();

        if (t.ClearJobs)
        {
            foreach (Job job in JobManager)
            {
                if (jobRemapDict.TryGetValue(job.Name, out string mappedName))
                {
                    job.Name = mappedName;
                }
                else
                {
                    string newJobName = string.Format("{0} {1}", "Job".Localize(), jobRemapDict.Count + 1);
                    job.Name = newJobName;
                    jobRemapDict.Add(job.Name, newJobName);
                }

                if (job.Template)
                {
                    a_dataChanges.TemplateChanges.AddedObject(job.Id);
                }
                else
                {
                    a_dataChanges.JobChanges.AddedObject(job.Id);
                }

                job.Description = "";
                job.Notes = "";
            }
        }

        if (t.ClearResources)
        {
            foreach (Resource res in PlantManager.GetResourceList())
            {
                if (resRemapDict.TryGetValue(res.Name, out string mappedName))
                {
                    res.Name = mappedName;
                }
                else
                {
                    string newJobName = string.Format("{0} {1}", "Resource".Localize(), resRemapDict.Count + 1);
                    res.Name = newJobName;
                    resRemapDict.Add(res.Name, newJobName);
                }

                a_dataChanges.MachineChanges.UpdatedObject(res.Id);
                res.Description = "";
                res.Notes = "";
            }
        }

        if (t.ClearDepartments)
        {
            foreach (Department dept in PlantManager.GetDepartments())
            {
                if (deptRemapDict.TryGetValue(dept.Name, out string mappedName))
                {
                    dept.Name = mappedName;
                }
                else
                {
                    string newName = string.Format("{0} {1}", "Department".Localize(), deptRemapDict.Count + 1);
                    dept.Name = newName;
                    deptRemapDict.Add(dept.Name, newName);
                }

                a_dataChanges.DepartmentChanges.UpdatedObject(dept.Id);
                dept.Description = "";
                dept.Notes = "";
            }
        }

        if (t.ClearPlants)
        {
            foreach (Plant plant in PlantManager)
            {
                if (plantRemapDict.TryGetValue(plant.Name, out string mappedName))
                {
                    plant.Name = mappedName;
                }
                else
                {
                    string newName = string.Format("{0} {1}", "Plant".Localize(), plantRemapDict.Count + 1);
                    plant.Name = newName;
                    plantRemapDict.Add(plant.Name, newName);
                }

                a_dataChanges.PlantChanges.UpdatedObject(plant.Id);
                plant.Description = "";
                plant.Notes = "";
            }
        }

        if (t.ClearCapacityIntervals)
        {
            foreach (CapacityInterval capacityInterval in CapacityIntervalManager)
            {
                if (remapDict.TryGetValue(capacityInterval.Name, out string mappedName))
                {
                    capacityInterval.Name = mappedName;
                }
                else
                {
                    string newCapacityIntervalName = string.Format("{0} {1}", "Capacity Interval".Localize(), remapDict.Count + 1);
                    capacityInterval.Name = newCapacityIntervalName;
                    remapDict.Add(capacityInterval.Name, newCapacityIntervalName);
                }

                a_dataChanges.CapacityIntervalChanges.UpdatedObject(capacityInterval.Id);
                capacityInterval.Description = "";
                capacityInterval.Notes = "";
            }

            remapDict.Clear();
        }

        if (t.ClearRecurringCapacityIntervals)
        {
            foreach (RecurringCapacityInterval recurringCapacityInterval in RecurringCapacityIntervalManager)
            {
                if (remapDict.TryGetValue(recurringCapacityInterval.Name, out string mappedName))
                {
                    recurringCapacityInterval.Name = mappedName;
                }
                else
                {
                    string newRecurringCapacityIntervalName = string.Format("{0} {1}", "Recurring Capacity Interval".Localize(), remapDict.Count + 1);
                    recurringCapacityInterval.Name = newRecurringCapacityIntervalName;
                    remapDict.Add(recurringCapacityInterval.Name, newRecurringCapacityIntervalName);
                }

                a_dataChanges.RecurringCapacityIntervalChanges.UpdatedObject(recurringCapacityInterval.Id);
                recurringCapacityInterval.Description = "";
                recurringCapacityInterval.Notes = "";
            }

            remapDict.Clear();
        }

        if (t.ClearBalancedCompositeRules)
        {
            foreach (DispatcherDefinition dispatcherDefinition in DispatcherDefinitionManager)
            {
                if (remapDict.TryGetValue(dispatcherDefinition.Name, out string mappedName))
                {
                    dispatcherDefinition.Name = mappedName;
                }
                else
                {
                    string newDispatcherDefName = string.Format("{0} {1}", "Dispatcher Definition".Localize(), remapDict.Count + 1);
                    dispatcherDefinition.Name = newDispatcherDefName;
                    remapDict.Add(dispatcherDefinition.Name, newDispatcherDefName);
                }

                a_dataChanges.BalancedCompositeDispatcherDefinitionChanges.UpdatedObject(dispatcherDefinition.Id);
            }

            remapDict.Clear();
        }

        if (t.ClearCells)
        {
            foreach (Cell cell in CellManager)
            {
                if (remapDict.TryGetValue(cell.Name, out string mappedName))
                {
                    cell.Name = mappedName;
                }
                else
                {
                    string newCellName = string.Format("{0} {1}", "Cell".Localize(), remapDict.Count + 1);
                    cell.Name = newCellName;
                    remapDict.Add(cell.Name, newCellName);
                }

                a_dataChanges.CellChanges.UpdatedObject(cell.Id);
                cell.Description = "";
                cell.Notes = "";
            }

            remapDict.Clear();
        }

        if (t.ClearCapabilities)
        {
            foreach (Capability c in CapabilityManager)
            {
                if (capabilityRemapDict.TryGetValue(c.Name, out string mappedName))
                {
                    c.Name = mappedName;
                }
                else
                {
                    string newName = string.Format("{0} {1}", "Capability".Localize(), capabilityRemapDict.Count + 1);
                    c.Name = newName;
                    capabilityRemapDict.Add(c.Name, newName);
                }

                a_dataChanges.CapabilityChanges.UpdatedObject(c.Id);
            }
        }

        if (t.ClearPurchaseToStocks)
        {
            foreach (PurchaseToStock purchaseToStock in PurchaseToStockManager)
            {
                if (remapDict.TryGetValue(purchaseToStock.Name, out string mappedName))
                {
                    purchaseToStock.Name = mappedName;
                }
                else
                {
                    string newPurchaseToStockName = string.Format("{0} {1}", "Purchase To Stock".Localize(), remapDict.Count + 1);
                    purchaseToStock.Name = newPurchaseToStockName;
                    remapDict.Add(purchaseToStock.Name, newPurchaseToStockName);
                }

                a_dataChanges.PurchaseToStockChanges.UpdatedObject(purchaseToStock.Id);
                purchaseToStock.Description = "";
                purchaseToStock.Notes = "";
            }

            remapDict.Clear();
        }

        if (t.ClearSalesOrders)
        {
            foreach (SalesOrder salesOrder in SalesOrderManager)
            {
                string generalizedSalesOrderName;
                if (remapDict.TryGetValue(salesOrder.Name, out generalizedSalesOrderName))
                {
                    salesOrder.Name = generalizedSalesOrderName;
                }
                else
                {
                    generalizedSalesOrderName = string.Format("{0} {1}", "Sales Order".Localize(), remapDict.Count + 1);
                    salesOrder.Name = generalizedSalesOrderName;
                    remapDict.Add(salesOrder.Name, generalizedSalesOrderName);
                }

                salesOrder.Description = "";
                salesOrder.Notes = "";
                a_dataChanges.SalesOrderChanges.UpdatedObject(salesOrder.Id);

                int counter = 1;
                foreach (SalesOrderLine salesOrderLine in salesOrder.SalesOrderLines)
                {
                    salesOrderLine.LineNumber = generalizedSalesOrderName + "_" + "Line" + counter;
                    salesOrderLine.Description = "";
                    a_dataChanges.SalesOrderChanges.UpdatedObject(salesOrderLine.Id);
                    counter++;
                }
            }

            remapDict.Clear();
        }

        if (t.ClearTransferOrders)
        {
            foreach (TransferOrder transferOrder in TransferOrderManager)
            {
                if (remapDict.TryGetValue(transferOrder.Name, out string mappedName))
                {
                    transferOrder.Name = mappedName;
                }
                else
                {
                    string newName = string.Format("{0} {1}", "Transfer Order".Localize(), remapDict.Count + 1);
                    transferOrder.Name = newName;
                    remapDict.Add(transferOrder.Name, newName);
                }

                a_dataChanges.TransferOrderChanges.UpdatedObject(transferOrder.Id);
                transferOrder.Description = "";
                transferOrder.Notes = "";
            }

            remapDict.Clear();
        }

        if (t.ClearWarehouses)
        {
            foreach (Warehouse warehouse in WarehouseManager)
            {
                if (remapDict.TryGetValue(warehouse.Name, out string mappedName))
                {
                    warehouse.Name = mappedName;
                }
                else
                {
                    string newWarehouseName = string.Format("{0} {1}", "Warehouse".Localize(), remapDict.Count + 1);
                    warehouse.Name = newWarehouseName;
                    remapDict.Add(warehouse.Name, newWarehouseName);
                }

                a_dataChanges.WarehouseChanges.UpdatedObject(warehouse.Id);
                warehouse.Description = "";
                warehouse.Notes = "";
            }

            remapDict.Clear();
        }

        if (t.ClearItems)
        {
            foreach (Item item in ItemManager)
            {
                if (itemRemapDict.TryGetValue(item.Name, out string mappedName))
                {
                    item.Name = mappedName;
                }
                else
                {
                    string newName = string.Format("{0} {1}", "Item".Localize(), itemRemapDict.Count + 1);
                    item.Name = newName;
                    itemRemapDict.Add(item.Name, newName);
                }

                a_dataChanges.ItemChanges.UpdatedObject(item.Id);
                item.Description = "";
                item.Notes = "";
            }
        }

        if (t.ClearCustomers)
        {
            foreach (Customer customer in CustomerManager)
            {
                if (customerRemapDict.TryGetValue(customer.Name, out string mappedName))
                {
                    customer.Name = mappedName;
                }
                else
                {
                    string newName = string.Format("{0} {1}", "Customer".Localize(), customerRemapDict.Count + 1);
                    customer.Name = newName;
                    customerRemapDict.Add(customer.Name, newName);
                }

                a_dataChanges.CustomerChanges.UpdatedObject(customer.Id);
                customer.Description = "";
                customer.Notes = "";
            }
        }

        // Seems like these aren't in the GeneralizeData tile, but maybe keep this here as 
        // potential placeholder.
        //if (t.ClearSetupCodeTables)
        //{
        //    m_setupCodeTableManager.DeleteAll(Scenario, this, a_dataChanges);
        //}
        //if (t.ClearAttributeCodeTables)
        //{
        //    m_attributeCodeTableManager.DeleteAll(Scenario, this, a_dataChanges);
        //}
        //if (t.ClearAttributeNumberRangeTables)
        //{
        //    m_fromRangeSetManager.DeleteAll(this);   
        //}

        //if (t.ClearProductRules || t.ClearItems || t.ClearResources)
        //{
        //    ProductRules_UpdateProductRulesLookupArrays_SetResIndexes_and_AssociateAllJobs();
        //}

        using (_scenario.ScenarioEventsLock.EnterRead(out ScenarioEvents se))
        {
            se.FireScenarioDetailClearEvent(t, this);
        }

        //Fire the ScenarioDetailClearEvent first to give the ui a chance to remove resources prior to TimeAdjustment which may try to update the schedule on resouces that are gone.
        if (t.ClearJobs ||
            t.ClearCapacityIntervals ||
            t.ClearDepartments ||
            t.ClearInventories ||
            t.ClearItems ||
            t.ClearPlants ||
            t.ClearPurchaseToStocks ||
            t.ClearRecurringCapacityIntervals ||
            t.ClearResources ||
            t.ClearWarehouses ||
            t.ClearProductRules ||
            t.ClearAttributeCodeTables ||
            t.ClearAttributeNumberRangeTables ||
            t.ClearResourceConnectors ||
            t.ClearAllowedHelpers ||
            t.ClearForecasts ||
            t.ClearSalesOrders ||
            t.ClearTransferOrders ||
            t.ClearCustomers ||
            t.ClearAttributes ||
            t.ClearCleanOutTables)
        {
            TimeAdjustment(t);
        }
    }

    #region AddInEnabling
    private void UpdateSimulationCustomizationEnabling(AddInControlUpdateT t)
    {
        //TODO: V12 extensions. This may not be needed
    }

    private readonly Schedule.Customizations.CustomizationEnablingList m_customizationEnablingList = new ();

    /// <summary>
    /// List that tracks the enabling of Scheduler Customizations.
    /// </summary>
    public Schedule.Customizations.CustomizationEnablingList CustomizationEnablingList => m_customizationEnablingList;
    #endregion AddInEnabling
    #endregion

    #region Ctp
    private void Ctp(Transmissions.CTP.CtpT t, Scenario scenarioToSendResultsTo, IScenarioDataChanges a_dataChanges)
    {
        CtpCreator.CreateCTP(t, scenarioToSendResultsTo, this, a_dataChanges, m_errorReporter);
    }
    #endregion

    #region Export
    private long m_lastScenarioDetailExportTDateTicks;

    public DateTime LastScenarioDetailExportTDate => new (m_lastScenarioDetailExportTDateTicks);

    private void ExportScenario(ScenarioDetailExportT t)
    {
        m_lastScenarioDetailExportTDateTicks = t.TimeStamp.Ticks;

        ScenarioEvents se;
        using (_scenario.ScenarioEventsLock.EnterRead(out se))
        {
            ScenarioSummary ss;
            using (_scenario.ScenarioSummaryLock.EnterRead(out ss))
            {
                se.FireExportScenarioEvent(this, t, ss);
            }
        }
    }
    #endregion

    #region Server CheckPoints for scenario
    public ChecksumValues CalculateChecksums(Guid a_transmissionId)
    {
        decimal startAndEndSums;
        decimal resourceJobOperationCombos;
        int blockCount;
        string scheduleDescription;

        CalculateChecksums(Scenario.ChecksumFrequency == ChecksumFrequencyType.ScheduleAdjustment, out startAndEndSums, out resourceJobOperationCombos, out blockCount, out scheduleDescription);

        return new ChecksumValues(Scenario.Id, startAndEndSums, resourceJobOperationCombos, blockCount, scheduleDescription, a_transmissionId);
    }

    /// <summary>
    /// Some function that ChatGPT blessed us with to convert a collection of guids into a hash string to
    /// be used when checking for desyncs. This hash string is added to be part of the ChecksumValues (which we compare for desyncs).
    /// The collection of Guids passed in should correspond to the guid of the first and last transmission of the UndoSet
    /// </summary>
    /// <param name="a_guids"></param>
    /// <returns></returns>
    /// This function was used in Neptune, but since Hydrogen (and onward) doesn't keep UndoSets on the scenario,
    /// there's no reason to include UndoSets in the checksum calculations. 
    //public static string ComputeHash(IList<Guid> a_guids)
    //{
    //    if (a_guids.Count == 0)
    //    {
    //        return string.Empty;
    //    }

    //    // Convert to bytes (or strings) and then to a single byte array.
    //    using (var sha256 = SHA256.Create())
    //    {
    //        // ChatGPT told us to sort a_guid if we wanted order independence, but order should matter for us in the context
    //        // of the undo sets since the transmissions of the undo sets are played in order.
    //        List<byte[]> guidBytesList = new List<byte[]>();
    //        int totalGuidBytesCount = 0;
    //        int bytesWrittenSoFar = 0;
    //        foreach (Guid guid in a_guids)
    //        {
    //            byte[] guidBytes = guid.ToByteArray();
    //            guidBytesList.Add(guidBytes);
    //            totalGuidBytesCount += guidBytes.Length;
    //        }

    //        byte[] combinedBytes = new byte[totalGuidBytesCount];
    //        foreach (byte[] guidBytes in guidBytesList)
    //        {
    //            sha256.TransformBlock(guidBytes, 0, guidBytes.Length, combinedBytes, bytesWrittenSoFar);
    //            bytesWrittenSoFar += guidBytes.Length;
    //        }

    //        // Complete the hash computation
    //        sha256.TransformFinalBlock(combinedBytes, 0, totalGuidBytesCount);

    //        // Get the final hash result
    //        byte[] hashBytes = sha256.Hash;

    //        // Convert to a readable hex string (or base64, if you prefer)
    //        StringBuilder sb = new StringBuilder();
    //        foreach (var b in hashBytes)
    //            sb.Append(b.ToString("x2"));

    //        return sb.ToString();
    //    }
    //}
    /// <summary>
    /// Calculate various checksums. These are used to determine whether the copy of the local schedule matches what's on the server.
    /// </summary>
    /// <param name="aStartAndEndSums">The sum of all the block start and finish times.</param>
    /// <param name="aResourceJobOperationCombos">The sum of all block Resource*Job*Operation. Where Id is used as the object's values.</param>
    /// <param name="aBlockCount">The total number of blocks in the schedule.</param>
    public void CalculateChecksums(bool a_checksumDiagnosticsEnabled, out decimal o_startAndEndSums, out decimal o_resourceJobOperationCombos, out int o_blockCount, out string o_description)
    {
        o_startAndEndSums = long.MinValue;
        o_resourceJobOperationCombos = long.MinValue;
        o_blockCount = 0;
        StringBuilder sbDescription = new ();

        for (int plantI = 0; plantI < m_plantManager.Count; ++plantI)
        {
            DepartmentManager dm = m_plantManager[plantI].Departments;
            for (int departmentI = 0; departmentI < dm.Count; ++departmentI)
            {
                ResourceManager rm = dm[departmentI].Resources;
                for (int resourceI = 0; resourceI < rm.Count; ++resourceI)
                {
                    Resource resource = rm[resourceI];
                    long startAndEndSums;
                    long resourceJobOperationCombos;
                    int blockCount;
                    resource.CalculateChecksums(a_checksumDiagnosticsEnabled, out startAndEndSums, out resourceJobOperationCombos, out blockCount, sbDescription);
                    o_startAndEndSums += startAndEndSums;
                    o_resourceJobOperationCombos += resourceJobOperationCombos;
                    o_blockCount += blockCount;
                }
            }
        }

        o_description = sbDescription.ToString();
    }

#if TEST
        CheckSumSDSet _csSDs;
        int _appliedSimulates;
        bool _csPrimarySD = true;

        class CheckSumSDSet
        {
            internal CheckSumSDSet(ScenarioDetail aOriginalSD)
            {
                _originalSD = aOriginalSD;
                _csSDs = new List<ChecksumSD>();
                _receivedTransmissions = new List<string>();
                _exceptionNames = new List<string>();
                _exceptionText = new List<string>();
            }

            List<ChecksumSD> _csSDs;
            ScenarioDetail _originalSD;

            List<string> _receivedTransmissions;
            List<string> _exceptionNames;
            List<string> _exceptionText;

            internal void Receive(ScenarioBaseT t)
            {
                try
                {
                    for (int i = _csSDs.Count - 1; i >= 0; --i)
                    {
                        bool b;
                        try
                        {
                            b = _csSDs[i].Receive(t);
                            ValidateChecksums(i, b, t);
                        }
                        catch (Exception e)
                        {
                            _exceptionNames.Add(e.GetType().Name);
                            _exceptionText.Add(e.ToString());
                        }
                    }
                }
            }

            internal void Receive(CtpT ctpT, Scenario scenarioToSendResultTo)
            {
                try
                {
                    for (int i = _csSDs.Count - 1; i >= 0; --i)
                    {
                        bool b = false;
                        try
                        {
                            b = _csSDs[i].Receive(ctpT, scenarioToSendResultTo);
                        }
                        ValidateChecksums(i, b, ctpT);
                    }
                }
            }

            internal void Receive(ScenarioClockAdvanceT scenarioClockAdvanceT)
            {
                try
                {
                    for (int i = _csSDs.Count - 1; i >= 0; --i)
                    {
                        bool b = _csSDs[i].Receive(scenarioClockAdvanceT);
                        ValidateChecksums(i, b, scenarioClockAdvanceT);
                    }
                }
            }

            internal void Receive(ScenarioTouchT touchT)
            {
                try
                {
                    for (int i = _csSDs.Count - 1; i >= 0; --i)
                    {
                        bool b = _csSDs[i].Receive(touchT);
                        ValidateChecksums(i, b, touchT);
                    }
                }
            }

            internal void ValidateChecksums(int sdIdx, bool recreate, Broadcasting.Transmission t)
            {
                if (t != null)
                {
                    _receivedTransmissions.Add(t.GetType().Name);
                }

                // Validate
                ChecksumValues originalCSV = _originalSD.CalculateChecksums();
                ChecksumValues testCSV = _csSDs[sdIdx]._sd.CalculateChecksums();

                if (!originalCSV.Equals(testCSV))
                {
                    _originalSD.WriteUnitTestFile("C:\\_Tmp_Orig", SimulationType.None);
                    _csSDs[sdIdx]._sd.WriteUnitTestFile("C:\\_Tmp_Test", SimulationType.None);
                    throw new Exception("The simulated client checksum validation code has found a problem.");
                }
                else
                {
                }

                if (recreate)
                {
                    int maxSimulates = _csSDs[sdIdx]._maxSimulates;
                    _csSDs.RemoveAt(sdIdx);
                    Add(maxSimulates);
                }
            }

            internal void Add(int aMaxSimulates)
            {
                _csSDs.Add(new ChecksumSD(_originalSD, aMaxSimulates));
                ValidateChecksums(_csSDs.Count - 1, false, null); // Perform touch?
            }
        }

        class ChecksumSD
        {
            internal ChecksumSD(ScenarioDetail aSD, int aMaxSimulates)
            {
                _maxSimulates = aMaxSimulates;

                _sd = Scenario.Copy(aSD);
                _sd._csPrimarySD = false;
                ((IScenarioRef)_sd).SetReferences(aSD.Scenario, null);
                _sd.RestoreReferences(PT.ConstantDefinitions.SerializationVersionNumber.NoVersion, aSD.Scenario, aSD._schedulerCustomizations, aSD._operationScheduledCustomizations, aSD._schedulabilityCustomization, false);
            }

            internal ScenarioDetail _sd;
            internal int _maxSimulates;
            internal int _simulates;

            int _before;
            void Before()
            {
                _before = _sd._appliedSimulates;
            }

            bool After()
            {
                return (_simulates += _sd._appliedSimulates - _before) >= _maxSimulates;
            }

            internal bool Receive(ScenarioBaseT t)
            {
                Before();
                _sd.Receive(t);
                return After();
            }

            internal bool Receive(CtpT ctpT, Scenario scenarioToSendResultTo)
            {
                Before();
                _sd.Receive(ctpT, null);
                return After();
            }

            internal bool Receive(ScenarioClockAdvanceT scenarioClockAdvanceT)
            {
                Before();
                _sd.Receive(scenarioClockAdvanceT);
                return After();
            }

            internal bool Receive(ScenarioTouchT touchT)
            {
                Before();
                _sd.Receive(touchT);
                return After();
            }
        }

        void InitTestSequence()
        {
            if (_csPrimarySD)
            {
                _csSDs = new CheckSumSDSet(this);
                _csSDs.Add(1);
                //_csSDs.Add(4);
                //_csSDs.Add(32);
                //_csSDs.Add(200);
                //_csSDs.Add(400);
                _csSDs.Add(1000000);
            }
        }

#endif
    #endregion

    #region Spans
    public DateTime GetEndOfShortTerm()
    {
        return new DateTime(m_clock + ScenarioOptions.ShortTermSpan.Ticks);
    }
    #endregion

    #region Offline Mode
    private readonly OnlineMode m_onlineMode = new ();

    public OnlineMode ScenarioOnlineMode => m_onlineMode;

    private void SetOnline(ScenarioDetailOnlineT a_t)
    {
        if (!m_onlineMode.Online)
        {
            m_onlineMode.SetOnline(a_t);
            FireOfflineStatusChangedEvent(a_t);
            TimeAdjustment(a_t);
        }
    }

    private void SetOffline(ScenarioDetailOfflineT a_t)
    {
        if (m_onlineMode.Online)
        {
            m_onlineMode.SetOffline(a_t);
            FireOfflineStatusChangedEvent(a_t);
        }
    }

    private void SetOffline()
    {
        if (m_onlineMode.Online)
        {
            m_onlineMode.SetOffline();
        }
    }

    private void SetOnline(ScenarioBaseT a_t)
    {
        if (!m_onlineMode.Online)
        {
            m_onlineMode.SetOnline();
            Touch(a_t);
        }
    }

    private void FireOfflineStatusChangedEvent(PTTransmission t)
    {
        ScenarioEvents se;
        using (_scenario.ScenarioEventsLock.EnterRead(out se))
        {
            se.FireOfflineStatusChangedEvent(t);
        }
    }
    #endregion Offline Mode

    #region Customizations
    private ExtensionController m_extensionController;

    internal ExtensionController ExtensionController => m_extensionController;

    public IEnumerable<ICustomTableExtensionElement> GetCustomTableExtensionElements()
    {
        return m_extensionController.GetCustomTableElements();
    }

    /// <summary>
    /// Check all of the loaded customizations for additinal simulation flags. A customization may need another simulation performed.
    /// </summary>
    private void CheckForRequiredAdditionalSimulation(IScenarioDataChanges a_dataChanges)
    {
        SortedList<int, ScenarioBaseT> transmissions = m_extensionController.GetExtraTransmissionsToProcess();

        foreach (ScenarioBaseT t in transmissions.Values)
        {
            //Change the instigator because many transmissions will default to ERP, which can have unintended side effects.
            if (t is ScenarioDetailMoveT)
            {
                Receive(t, a_dataChanges, new Simulation.UndoReceive.Move.UndoReceiveMove());
            }
            else
            {
                Receive(t, a_dataChanges);
            }
        }
    }
    #endregion

#if TEST
        DesyncDebuggingResult mt_desyncTest;
        internal void CompareDesyncResults()
        {
            if (!PTSystem.Server)
            {
                if (DesyncDebugging.s_lastType != this.GetType())
                {
                    DesyncDebugging.s_lastType = this.GetType();
                }
                DesyncDebuggingResult newDesyncTestResult;
                List<string> differentFields;
                if (DesyncDebuggingResult.Different(this, mt_desyncTest, out newDesyncTestResult, out differentFields))
                {
                    int x = 0; // Set breakpoint here to examine the differences.
                }
            }
        }
#endif
    /// <summary>
    /// [DesyncDebugging]
    /// This function will iterate through all alternate path nodes on each job. It logs keys, operations, predecessors, and successors.
    /// It is used to find differences between the server and client.
    /// </summary>
    [Conditional("DEBUG")]
    internal void LogPathInformation(string a_logPath)
    {
        File.WriteAllText(a_logPath, "");
        StringBuilder sb = new ();
        for (int jobI = 0; jobI < JobManager.Count; jobI++)
        {
            Job job = JobManager[jobI];
            sb.AppendLine(job.ToString());
            for (int moI = 0; moI < job.ManufacturingOrders.Count; moI++)
            {
                ManufacturingOrder mo = job.ManufacturingOrders[moI];
                sb.AppendLine(mo.Name + " | " + mo.Description);
                for (int pathI = 0; pathI < mo.AlternatePaths.Count; pathI++)
                {
                    IEnumerator<KeyValuePair<string, AlternatePath.Node>> enumerator = mo.AlternatePaths[pathI].AlternateNodeSortedList.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        sb.AppendLine("Key | " + enumerator.Current.Key);
                        AlternatePath.Node node = enumerator.Current.Value;
                        sb.AppendLine("node | " + node.Id);
                        sb.AppendLine("Op | " + node.Operation);
                        sb.AppendLine("ResReqsMasterEligibilitySet | " + node.ResReqsMasterEligibilitySet.Count);
                        sb.AppendLine("EligibilityNarrowedDuringSim | " + node.ResReqsEligibilityNarrowedDuringSimulation.Count);
                        sb.AppendLine("Predecessors | " + node.Predecessors);
                        for (int predI = 0; predI < node.Predecessors.Count; predI++)
                        {
                            sb.AppendLine("Pred " + predI + " | " + node.Predecessors[predI]);
                        }

                        sb.AppendLine("Successors | " + node.Successors);
                        for (int sucI = 0; sucI < node.Successors.Count; sucI++)
                        {
                            sb.AppendLine("Suc " + sucI + " | " + node.Successors[sucI]);
                        }
                    }
                }
            }
        }

        File.WriteAllText(a_logPath, sb.ToString());
    }

    /// <summary>
    /// [DesyncDebugging]
    /// This function will recursively iterate through all fields in ScenarioDetail that are within the Scheduler, APSCommon, or Common Assemblies.
    /// It will output each filed type and value. Sub Items are written with a '|' character for each level below the root object.
    /// If a filename is provided, the log will be written to the specified file.
    /// </summary>
    [Conditional("DEBUG")]
    internal void RecursiveReflectionLogger(string a_fileName = null)
    {
        DateTime start = DateTime.Now;
        bool logToFile = !string.IsNullOrEmpty(a_fileName);
        try
        {
            StringBuilder sb = new ();
            if (logToFile)
            {
                File.WriteAllText(a_fileName, sb.ToString());
            }

            HashSet<object> loggedObjects = new ();
            Dictionary<Type, int> lookups = new ();
            LogFieldValues(this, "", ref sb, ref loggedObjects, ref lookups, 0, logToFile);
            if (logToFile)
            {
                using (StreamWriter writer = new (a_fileName, false))
                {
                    const int c_chunkLength = 30000;
                    while (sb.Length > c_chunkLength)
                    {
                        writer.Write(sb.ToString(0, c_chunkLength));
                        sb.Remove(0, c_chunkLength);
                    }

                    writer.Write(sb);
                    //TimeSpan runTime = DateTime.Now - start;
                    //System.IO.File.AppendAllText(a_fileName, runTime.ToString());
                }
            }
        }
        catch (IOException)
        {
            //Couldn't log the file
        }
        catch (Exception e)
        {
            //May be an internal error. Try and log to file
            try
            {
                TimeSpan runTime = DateTime.Now - start;
                File.AppendAllText(a_fileName, runTime.ToString());
                File.AppendAllText(a_fileName, e.Message);
            }
            catch (Exception)
            {
                //Couldn't log to file
            }
        }
    }

    [Conditional("DEBUG")]
    private void LogObject(StringBuilder a_sb, string a_message, int a_indentCount)
    {
        const char c_indent = '|';
        string indent = new (c_indent, a_indentCount);
        indent += "(" + a_indentCount + ")";
        a_sb.AppendLine(indent + a_message);
    }

    [Conditional("DEBUG")]
    private void LogFieldValues(object a_obj, object a_parentObject, ref StringBuilder a_sb, ref HashSet<object> a_loggedObjects, ref Dictionary<Type, int> a_lookups, int a_indentCount, bool a_logToFile)
    {
        if (a_obj == null)
        {
            return;
        }

        //Validate if the object has been already processed. If so, keep track of its type and return
        Type t;
        if (a_loggedObjects.Contains(a_obj))
        {
            //int count;
            //t = a_obj.GetType();
            //if (a_lookups.TryGetValue(t, out count))
            //{
            //    count++;
            //    a_lookups[t] = count;
            //    if (count > 1000)
            //    {
            //        bool b = false;
            //    }
            //}
            //else
            //{
            //    a_lookups.Add(t, 1);
            //}
            return;
        }

        //Exclude specific types from being logged or processed
        if (a_obj is BaseId or ResourceBlockList or ResourceBlockList.Node[] or Scheduler.Scenario)
        {
            return;
        }

        a_loggedObjects.Add(a_obj);
        t = a_obj.GetType();

        //Ignore certain types by name instead of specific type since the type is not accessible or is this is a wildcard filter
        //if (t.FullName.EndsWith("+ReferenceInfo"))
        //{
        //    return;
        //}

        //Get all fields and recursively log them and their fields
        FieldInfo[] fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        foreach (FieldInfo fieldInfo in fields)
        {
            LogFieldInfo(t, fieldInfo, a_obj, a_parentObject, ref a_sb, ref a_loggedObjects, ref a_lookups, a_indentCount, a_logToFile);
        }

        if (t.BaseType != null)
        {
            FieldInfo[] baseFields = t.BaseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (FieldInfo fieldInfo in baseFields)
            {
                LogFieldInfo(t.BaseType, fieldInfo, a_obj, a_parentObject, ref a_sb, ref a_loggedObjects, ref a_lookups, a_indentCount, a_logToFile);
            }
        }

        //Check if the type has a count property. If it does, enumerate the index values
        PropertyInfo[] properties = t.GetProperties();

        if (t == typeof(SortedList<DateIntKey, ResourceCapacityInterval>))
        {
            SortedList<DateIntKey, ResourceCapacityInterval> sortedList = (SortedList<DateIntKey, ResourceCapacityInterval>)a_obj;
            for (int i = 0; i < sortedList.Values.Count; i++)
            {
                ResourceCapacityInterval resCI = sortedList.Values[i];
                if (a_logToFile)
                {
                    LogObject(a_sb, a_parentObject + "<:>" + resCI.GetType().FullName + "[" + i + "] " + resCI, a_indentCount);
                }

                LogFieldValues(resCI, a_obj, ref a_sb, ref a_loggedObjects, ref a_lookups, a_indentCount + 1, a_logToFile);
            }
        }
        else if (t == typeof(InternalActivityManager))
        {
            InternalActivityManager internalActivityManager = (InternalActivityManager)a_obj;
            for (int i = 0; i < internalActivityManager.Count; i++)
            {
                InternalActivity act = internalActivityManager.GetByIndex(i);
                if (a_logToFile)
                {
                    LogObject(a_sb, a_parentObject + "<:>" + act.GetType().FullName + "[" + i + "] " + act, a_indentCount);
                }

                LogFieldValues(act, a_obj, ref a_sb, ref a_loggedObjects, ref a_lookups, a_indentCount + 1, a_logToFile);
            }
        }
        else if (t == typeof(SortedList))
        {
            SortedList list = (SortedList)a_obj;
            for (int i = 0; i < list.Count; i++)
            {
                object obj = list.GetByIndex(i);
                if (a_logToFile)
                {
                    LogObject(a_sb, a_parentObject + "<:>" + obj.GetType().FullName + "[" + i + "] " + obj, a_indentCount);
                }

                LogFieldValues(obj, a_obj, ref a_sb, ref a_loggedObjects, ref a_lookups, a_indentCount + 1, a_logToFile);
            }
        }
        else if (t == typeof(InternalResourceList))
        {
            InternalResourceList internalResourceList = (InternalResourceList)a_obj;
            for (int i = 0; i < internalResourceList.Count; i++)
            {
                InternalResource obj = internalResourceList.GetByIndex(i);
                if (a_logToFile)
                {
                    LogObject(a_sb, a_parentObject + "<:>" + obj.GetType().FullName + "[" + i + "] " + obj, a_indentCount);
                }

                LogFieldValues(obj, a_obj, ref a_sb, ref a_loggedObjects, ref a_lookups, a_indentCount + 1, a_logToFile);
            }
        }
        else if (t == typeof(SortedList<BaseId, ResourceRequirement>))
        {
            SortedList<BaseId, ResourceRequirement> resReqList = (SortedList<BaseId, ResourceRequirement>)a_obj;
            for (int i = 0; i < resReqList.Values.Count; i++)
            {
                ResourceRequirement obj = resReqList.Values[i];
                if (a_logToFile)
                {
                    LogObject(a_sb, a_parentObject + "<:>" + obj.GetType().FullName + "[" + i + "] " + obj, a_indentCount);
                }

                LogFieldValues(obj, a_obj, ref a_sb, ref a_loggedObjects, ref a_lookups, a_indentCount + 1, a_logToFile);
            }
        }
        else
        {
            foreach (PropertyInfo property in properties)
            {
                object propValue = null;
                //We must loop through all properties instead of looking up the Item property directly because there may be an ambigous reference exception if there is more than one "Item" property.
                if (property.Name == "Item" && !(property.PropertyType.FullName.StartsWith("PT") && property.PropertyType.FullName.EndsWith(".Item")))
                {
                    int count = -1;
                    //Look for a Count property to determine the max index. if there is no count, then ignore this property
                    PropertyInfo countProp = t.GetProperty("Count");
                    if (countProp == null)
                    {
                        continue;
                    }

                    count = (int)countProp.GetValue(a_obj, null);
                    if (count > 0)
                    {
                        object[] index = new object[count];
                        for (int i = 0; i < count; i++)
                        {
                            try
                            {
                                object val = property.GetValue(a_obj, new object[] { i });
                                if (val == null)
                                {
                                    continue;
                                }

                                Assembly a = val.GetType().Assembly;
                                if (a == t.Assembly || a.FullName.StartsWith("PT.Common") || a.FullName.StartsWith("PT.SchedulerDefinitions") || a.FullName.StartsWith("PT.APSCommon"))
                                {
                                    if (a_logToFile)
                                    {
                                        LogObject(a_sb, a_parentObject + "<:>" + val.GetType().FullName + "[" + i + "] " + val, a_indentCount);
                                    }

                                    LogFieldValues(val, a_obj, ref a_sb, ref a_loggedObjects, ref a_lookups, a_indentCount + 1, a_logToFile);
                                }
                                else
                                {
                                    a_sb.AppendLine(property.Name + " | " + val);
                                }
                            }
                            catch (TargetParameterCountException)
                            {
                                a_sb.AppendLine($"[DEBUG LOGGER MISSING TYPE] : Type {t.FullName}, Property {property.Name}, a_obj {a_obj.GetType().FullName}, a_parentObject {a_parentObject.GetType().FullName}");
                                break;
                            }
                            catch (ArgumentException)
                            {
                                //todo: this type should be handled
                                a_sb.AppendLine($"[DEBUG LOGGER MISSING TYPE] : Type {t.FullName}, Property {property.Name}, a_obj {a_obj.GetType().FullName}, a_parentObject {a_parentObject.GetType().FullName}");
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private void LogFieldInfo(Type a_t, FieldInfo a_fieldInfo, object a_obj, object a_parentObject, ref StringBuilder a_sb, ref HashSet<object> a_loggedObjects, ref Dictionary<Type, int> a_lookups, int a_indentCount, bool a_logToFile)
    {
        //Ignore compiler added fields
        if (a_fieldInfo.Name.StartsWith("CS$") || a_fieldInfo.FieldType.FullName.Contains("AVLTree"))
        {
            return;
        }

        //For some reason this event does not keep the DebugLogging attribute.
        if (a_fieldInfo.Name == "ResourceAddedEvent")
        {
            return;
        }

        //Ignore delegate pointers. Could potentiall ignore all IntPtr types
        if (a_fieldInfo.Name == "_methodPtr" || a_fieldInfo.Name == "_ptr" || a_fieldInfo.Name == "_ptrType")
        {
            return;
        }

        //Look for the debug logging attribute. If it exists, save its logtype parameter.
        EDebugLoggingType logType = EDebugLoggingType.Undefined;
        foreach (CustomAttributeData attribute in a_fieldInfo.CustomAttributes)
        {
            if (attribute.AttributeType == typeof(DebugLogging))
            {
                foreach (CustomAttributeTypedArgument constructorArgument in attribute.ConstructorArguments)
                {
                    if (constructorArgument.ArgumentType == typeof(EDebugLoggingType))
                    {
                        logType = (EDebugLoggingType)(int)constructorArgument.Value;
                        break;
                    }
                }
            }
        }

        //This field has been set to not log
        if (logType == EDebugLoggingType.None || logType == EDebugLoggingType.IterateOnly)
        {
            return;
        }

        //Ignore constants and public readonly static values
        if (a_fieldInfo.IsLiteral || (a_fieldInfo.IsInitOnly && a_fieldInfo.IsStatic && a_fieldInfo.IsPublic))
        {
            //Constants and readonly statics can be written if they have an detailed or verbose attribute present.
            if (logType == EDebugLoggingType.Undefined)
            {
                return;
            }
        }

        //Attempt to avoid iterating through linked lists.
        if (a_fieldInfo.Name.EndsWith("next") || a_fieldInfo.Name.EndsWith("previous"))
        {
            if (logType != EDebugLoggingType.IterateOnly && logType != EDebugLoggingType.Verbose)
            {
                return;
            }
        }

        object currentObject = a_fieldInfo.GetValue(a_obj);
        if (currentObject == null)
        {
            return;
        }

        if (a_parentObject is Batch && currentObject is BaseId)
        {
            return;
        }

        if (a_logToFile)
        {
            LogObject(a_sb, a_parentObject + "<:>" + a_fieldInfo + " | " + currentObject, a_indentCount);
        }

        LogFieldValues(currentObject, a_obj, ref a_sb, ref a_loggedObjects, ref a_lookups, a_indentCount + 1, a_logToFile);
    }
}