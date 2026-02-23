using System.Data;

using Microsoft.Data.SqlClient;

using PT.APSCommon;
using PT.APSCommon.Exceptions;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.Common.Sql.Exceptions;
using PT.Common.Sql.SqlServer;
using PT.Database;
using PT.PackageDefinitions.Settings;
using PT.PackageDefinitions.Settings.PublishOptions;
using PT.Scheduler;
using PT.Scheduler.Simulation.Extensions.Interfaces;
using PT.SchedulerData.Capacity;
using PT.SchedulerData.InventoryManagement;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.SchedulerData;

/// <summary>
/// Used to keep the PT Database up to date.
/// </summary>
public class PtDatabaseUpdater : IDisposable
{
    public PtDatabaseUpdater(ScenarioDetail a_sd, BaseId a_scenarioId, string a_instanceId, string a_instanceName, string a_instanceVersion, string a_instanceEnvType)
    {
        m_sd = a_sd;
        m_scenarioId = a_scenarioId;
        m_instanceId = a_instanceId;
        m_instanceName = a_instanceName;
        m_instanceVersion = a_instanceVersion;
        m_instanceEnvType = a_instanceEnvType;
    }

    public void Dispose()
    {
        m_sd = null;
    }

    private ScenarioDetail m_sd;
    private readonly BaseId m_scenarioId;
    private readonly string m_instanceId;
    private readonly string m_instanceName;
    private readonly string m_instanceVersion;
    private readonly string m_instanceEnvType;
    public BaseId Instigator = BaseId.NULL_ID;
    private double m_currentProgressPoints;
    private double m_totalProgressPoints;
    private double m_allowedProgressPoints;

    #region Events
    public delegate void PublishStatusDelegate(BaseId a_scenarioId, BaseId a_instigator, PublishStatuses.EPublishProgressStep a_currentStep, double a_currentProgress);

    public event PublishStatusDelegate PublishStatusEvent;

    private void FireStatusEvent(PublishStatuses.EPublishProgressStep a_step)
    {
        PublishStatusEvent?.Invoke(m_scenarioId, Instigator, a_step, m_currentProgressPoints / m_totalProgressPoints * 100);
    }

    private void FireStatusEvent(PublishStatuses.EPublishProgressStep a_step, double a_progressPercent)
    {
        PublishStatusEvent?.Invoke(m_scenarioId, Instigator, a_step, a_progressPercent);
    }
    #endregion

    #region Public Methods
    public void WriteToXML(string filename, ScenarioDetailExportT t)
    {
        PtDbDataSet ds = PopulateDataSet(t.Instigator, false, false, null, t.LimitToSpecifiedResources, t.ResourcesToExport, null, out DateTime publishDate);
        UpdateProgressPercent(50);
        FireStatusEvent(PublishStatuses.EPublishProgressStep.WritingData);
        ds.WriteXml(filename, XmlWriteMode.WriteSchema);
        ds.WriteXmlSchema(filename + ".xsd");
    }

    /// <summary>
    /// Clears all PT Database tables and populates them with current data.
    /// </summary>
    public void PublishScenarioToSql(ScenarioDetailExportT a_t, string a_SqlServerConnectionString, int a_timeout, out DateTime a_publishDate)
    {
        BaseId instigator = a_t.Instigator;

        using (SqlConnection conn = new (a_SqlServerConnectionString))
        {
            FireStatusEvent(PublishStatuses.EPublishProgressStep.Connecting, 1);
            try
            {
                conn.Open();
            }
            catch (SqlException e)
            {
                throw new PTException("4074", new object[] { e.Server, "Publish", e.Message });
            }

            PtDbDataSet ds = PopulateDataSet(instigator, false, false, null, a_t.LimitToSpecifiedResources, a_t.ResourcesToExport, null, out a_publishDate);
            FireStatusEvent(PublishStatuses.EPublishProgressStep.ClearingOldHistory);
            UpdateProgressPercent(5);
            PrepareTargetDatabase(ds, conn, m_sd);
            RemoveOldHistoryData(conn, m_sd);
            AppendScheduleToDB(ds, conn, m_sd, a_timeout, false);
            ExportCustomTables(conn, a_publishDate, m_instanceId);
            SqlConnection.ClearAllPools();
            conn.Close();
        }
    }

    /// <summary>
    /// Exports any custom tables loaded through CustomTableCustomization.
    /// </summary>
    /// <param name="a_sqlConn"></param>
    /// <param name="a_publishDate"></param>
    private void ExportCustomTables(SqlConnection a_sqlConn, DateTime a_publishDate, string a_instanceId)
    {
        List<Exception> errors = new ();

        foreach (ICustomTableExtensionElement customTableElement in m_sd.GetCustomTableExtensionElements())
        {
            try
            {
                SystemDataTable systemData = new (a_sqlConn);
                List<DataTable> customTables = customTableElement.GetCustomTables(m_sd, a_publishDate, a_instanceId);
                if (customTables == null)
                {
                    continue;
                }

                foreach (DataTable table in customTables)
                {
                    try
                    {
                        try
                        {
                            if (systemData.ClearCustomTables)
                            {
                                SqlTableCreator.DeleteSqlTable(a_sqlConn, table);
                            }
                        }
                        catch { } // in case the table doesn't exist.

                        try
                        {
                            if (systemData.ClearCustomTables)
                            {
                                SqlTableCreator.CreateSqlTable(a_sqlConn, table);
                            }

                            SqlTableCreator.InsertToSql(a_sqlConn, table);
                        }
                        catch (Exception err)
                        {
                            throw new PTDatabaseException("2943", err, new object[] { table.TableName, err.GetExceptionFullMessage() });
                        }
                    }
                    catch (Exception err)
                    {
                        errors.Add(err);
                    }
                }
            }
            catch (Exception err)
            {
                errors.Add(err);
            }
        }

        if (errors.Count > 0)
        {
            throw new AggregateException(errors);
        }
    }

    public void PublishNetChangedJobToSql(BaseId instigator, ScenarioSummary ss, string aSqlServerConnectionString, int a_timeout, out DateTime o_publishDate)
    {
        PtDbDataSet ds = new ();
        ds.EnforceConstraints = false;
        ScenarioPublishHistory scenarioPublishHistory = new ScenarioPublishHistory();
        scenarioPublishHistory = ss.ScenarioSettings.LoadSetting(scenarioPublishHistory);
        

        using (SqlConnection conn = new (aSqlServerConnectionString))
        {
            try
            {
                FireStatusEvent(PublishStatuses.EPublishProgressStep.Connecting, 1);
                conn.Open();
            }
            catch (SqlException e)
            {
                throw new PTException("4074", new object[] { e.Server, e.Message });
            }

            ds.BeginLoadData(); //Improves performance when loading lots of rows

            DateTime maxPublishDate = m_sd.ClockDate.Add(scenarioPublishHistory.PublishHorizonSpan);
            PTDatabaseHelper dbHelper = new (PTSystem.ServerTimeZone, false, maxPublishDate, PTSystem.LicenseKey.IncludeDetailedScheduling);
            PtDbDataSet.SchedulesRow schedulesRow = PopulateSchedulesTable(ss, instigator, dbHelper, true, ref ds, out o_publishDate);

            //Add the jobs
            m_sd.PlantManager.PreProcessWorkForPtDbPopulate();

            bool includeInventory = false; //Item table not populated here so can't add child records to materials/products tables since they won't have matching parent table records //sd.ScenarioPublishOptions.PublishInventory || includeAllData;
            List<Job> jobs = Scheduler.Schedule.Analysis.NetChange.ActivityStateManager.GetChangedJobs(m_sd.NetChangeList);
            for (int i = 0; i < jobs.Count; i++)
            {
                Job job = jobs[i];
                job.PtDbPopulate(m_sd, ref ds, schedulesRow, includeInventory, false, null, dbHelper);
            }

            ds.EndLoadData();

            PrepareTargetDatabase(ds, conn, m_sd);
            AppendScheduleToDB(ds, conn, m_sd, a_timeout, true);
            conn.Close();
        }
    }

    private void PrepareTargetDatabase(PtDbDataSet ds, SqlConnection conn, ScenarioDetail a_sd)
    {
        SoftwareVersion currentVersion = AssemblyVersionChecker.GetAssemblyVersion();

        try
        {
            SystemDataTable systemData = new (conn);

            if (systemData.PrepareData)
            {
                FireStatusEvent(PublishStatuses.EPublishProgressStep.FormattingData);
                PrepDataForSQLServerPublish(ds, a_sd);
            }

            UpdateProgressPercent(5);

            if (systemData.ValidateDb || systemData.Version != currentVersion.ToString())
            {
                FireStatusEvent(PublishStatuses.EPublishProgressStep.FormattingTables);
                DatabaseSynchronizer.AlterDbStructureToMatchDataSet(conn, ds);
                systemData.UpdateVersion(conn, currentVersion.ToString());
            }

            UpdateProgressPercent(5);
        }
        catch (Exception err)
        {
            throw new PTHandleableException("2125", err);
        }
    }

    public void AppendScheduleToDB(PtDbDataSet ds, SqlConnection conn, ScenarioDetail a_sd, int a_timeout, bool a_netChange)
    {
        UpdateSchedulesTableFlags(conn, a_sd, a_netChange);

        string currentTableName = "";
        try
        {
            //Save the data from the DataSet down to the Database.  Do it in order of decreasing hierarchy.

            // ** NOTE WHEN ADDING NEW TABLES 
            //        Need to add above, below and to ClearDatabaseHelper() and RemoveOldHistoryData() too.
            //        In the SQL Database (not the DataSet) need to set the relations to cascade updates and deletes.
            //        The dataset column names must match the database columns exactly (case sensitive).
            List<string> tableNameList = RetrieveOrderedDataTablesToInsert(a_sd);

            //Use the BulkCopy class to insert the data. This class performs much faster than data adapter
            SqlBulkCopy bulkInsert = new (conn, SqlBulkCopyOptions.TableLock, null);
            bulkInsert.BulkCopyTimeout = a_timeout;

            FireStatusEvent(PublishStatuses.EPublishProgressStep.UploadingData);
            for (int i = 0; i < tableNameList.Count; i++)
            {
                currentTableName = tableNameList[i];
                bulkInsert.DestinationTableName = currentTableName;
                DataTable dt = ds.Tables[currentTableName];
                bulkInsert.BatchSize = dt.Rows.Count;

                //Create Mapping List. Although the dataset columns are the same in the Database, they are not always in the same order.
                // A mapping is needed so that BulkCopy can match the columns by name.
                //DEV: If you run into errors below about columns that are unable to be mapped, make sure you have the 'SystemData.ValidateDb' field set to true in your SQL publish db.
                List<SqlBulkCopyColumnMapping> mappingList = new ();
                foreach (DataColumn c in dt.Columns)
                {
                    mappingList.Add(new SqlBulkCopyColumnMapping(c.ColumnName, c.ColumnName));
                }

                //Clear the old mappings if needed.
                bulkInsert.ColumnMappings.Clear();
                foreach (SqlBulkCopyColumnMapping mapping in mappingList)
                {
                    bulkInsert.ColumnMappings.Add(mapping);
                }

                //Send the data.
                bulkInsert.WriteToServer(dt);
            }

            //This is probably not needed anymore. SqlAdapater and SqlBulkCopy have similar event handlers tha can be used for 
            // diagnosing publish error. 

            #region ROW BY ROW
            //Create a DataAdapter for each Table  

            //currentTableName = "Schedules";
            //SqlDataAdapter schedulesDataAdapter = new SqlDataAdapter("Select * FROM Schedules", conn);
            //SetDataAdapter(schedulesDataAdapter, true);
            //schedulesDataAdapter.Update(ds, currentTableName);

            //currentTableName = "Plants";
            //SqlDataAdapter plantsDataAdapter = new SqlDataAdapter("Select * FROM Plants", conn);
            //SetDataAdapter(plantsDataAdapter, true);
            //plantsDataAdapter.Update(ds, currentTableName);

            //currentTableName = "Departments";
            //SqlDataAdapter departmentsDataAdapter = new SqlDataAdapter("Select * FROM Departments", conn);
            //SetDataAdapter(departmentsDataAdapter, true);
            //departmentsDataAdapter.Update(ds, currentTableName);

            //currentTableName = "Resources";
            //SqlDataAdapter resourcesDataAdapter = new SqlDataAdapter("Select * FROM Resources", conn);
            //SetDataAdapter(resourcesDataAdapter, true);
            //resourcesDataAdapter.Update(ds, currentTableName);

            //currentTableName = "Capabilities";
            //SqlDataAdapter capabilitiesDataAdapter = new SqlDataAdapter("Select * FROM Capabilities", conn);
            //SetDataAdapter(capabilitiesDataAdapter, true);
            //capabilitiesDataAdapter.Update(ds, currentTableName);

            //currentTableName = "ResourceCapabilities";
            //SqlDataAdapter resourceCapabilitiesDataAdapter = new SqlDataAdapter("Select * FROM ResourceCapabilities", conn);
            //SetDataAdapter(resourceCapabilitiesDataAdapter, true);
            //resourceCapabilitiesDataAdapter.Update(ds, currentTableName);

            //if (aSD.ScenarioPublishOptions.PublishInventory) //Publish before Jobs since Jobs reference these.
            //{
            //    //Inventory Management            
            //    currentTableName = "Warehouses";
            //    SqlDataAdapter warehousesDataAdapter = new SqlDataAdapter("Select * FROM Warehouses", conn);
            //    SetDataAdapter(warehousesDataAdapter, true);
            //    warehousesDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "PlantWarehouses";
            //    SqlDataAdapter plantWarehousesDataAdapter = new SqlDataAdapter("Select * FROM PlantWarehouses", conn);
            //    SetDataAdapter(plantWarehousesDataAdapter, true);
            //    plantWarehousesDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "Items";
            //    SqlDataAdapter itemsDataAdapter = new SqlDataAdapter("Select * FROM Items", conn);
            //    SetDataAdapter(itemsDataAdapter, true);
            //    itemsDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "Inventories";
            //    SqlDataAdapter inventoriesDataAdapter = new SqlDataAdapter("Select * FROM Inventories", conn);
            //    SetDataAdapter(inventoriesDataAdapter, true);
            //    inventoriesDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "PurchasesToStock";
            //    SqlDataAdapter purchasesToStockDataAdapter = new SqlDataAdapter("Select * FROM PurchasesToStock", conn);
            //    SetDataAdapter(purchasesToStockDataAdapter, true);
            //    purchasesToStockDataAdapter.Update(ds, currentTableName);
            //}

            ////Jobs

            //currentTableName = "Jobs";
            //SqlDataAdapter jobsDataAdapter = new SqlDataAdapter("Select * FROM Jobs", conn);
            //SetDataAdapter(jobsDataAdapter, true);
            //jobsDataAdapter.Update(ds, currentTableName);

            //currentTableName = "ManufacturingOrders";
            //SqlDataAdapter manufacturingOrdersDataAdapter = new SqlDataAdapter("Select * FROM ManufacturingOrders", conn);
            //SetDataAdapter(manufacturingOrdersDataAdapter, true);
            //manufacturingOrdersDataAdapter.Update(ds, currentTableName);

            //currentTableName = "JobSuccessorManufacturingOrders";
            //SqlDataAdapter successorManufacturingOrdersDataAdapter = new SqlDataAdapter("Select * FROM JobSuccessorManufacturingOrders", conn);
            //SetDataAdapter(successorManufacturingOrdersDataAdapter, false);
            //successorManufacturingOrdersDataAdapter.Update(ds, currentTableName);

            //currentTableName = "JobOperationsSelect";
            //SqlDataAdapter jobOperationsDataAdapter = new SqlDataAdapter("Select * FROM JobOperations", conn);
            //currentTableName = "JobOperationsSet";
            //SetDataAdapter(jobOperationsDataAdapter, true);
            //currentTableName = "JobOperationsUpdate";
            //jobOperationsDataAdapter.Update(ds, "JobOperations");

            //currentTableName = "JobResources";
            //SqlDataAdapter jobResourcesDataAdapter = new SqlDataAdapter("Select * FROM JobResources", conn);
            //SetDataAdapter(jobResourcesDataAdapter, true);
            //jobResourcesDataAdapter.Update(ds, currentTableName);

            //currentTableName = "JobResourceCapabilities";
            //SqlDataAdapter jobResourceCapabilitiesDataAdapter = new SqlDataAdapter("Select * FROM JobResourceCapabilities", conn);
            //SetDataAdapter(jobResourceCapabilitiesDataAdapter, true);
            //jobResourceCapabilitiesDataAdapter.Update(ds, currentTableName);

            //currentTableName = "JobMaterials";
            //SqlDataAdapter jobMaterialsDataAdapter = new SqlDataAdapter("Select * FROM JobMaterials", conn);
            //SetDataAdapter(jobMaterialsDataAdapter, true);
            //jobMaterialsDataAdapter.Update(ds, currentTableName);

            //currentTableName = "JobProducts";
            //SqlDataAdapter jobProductsDataAdapter = new SqlDataAdapter("Select * FROM JobProducts", conn);
            //SetDataAdapter(jobProductsDataAdapter, true);
            //jobProductsDataAdapter.Update(ds, currentTableName);

            //currentTableName = "JobActivities";
            //SqlDataAdapter jobActivitiesDataAdapter = new SqlDataAdapter("Select * FROM JobActivities", conn);
            //SetDataAdapter(jobActivitiesDataAdapter, true);
            //jobActivitiesDataAdapter.Update(ds, currentTableName);

            //currentTableName = "JobResourceBlocks";
            //SqlDataAdapter jobResourceBlocksDataAdapter = new SqlDataAdapter("Select * FROM JobResourceBlocks", conn);
            //SetDataAdapter(jobResourceBlocksDataAdapter, true);
            //jobResourceBlocksDataAdapter.Update(ds, currentTableName);

            //currentTableName = "JobResourceBlockIntervals";
            //SqlDataAdapter jobResourceBlockIntervalsDataAdapter = new SqlDataAdapter("Select * FROM JobResourceBlockIntervals", conn);
            //SetDataAdapter(jobResourceBlockIntervalsDataAdapter, true);
            //jobResourceBlockIntervalsDataAdapter.Update(ds, currentTableName);

            //currentTableName = "JobMaterialSupplyingActivities";
            //SqlDataAdapter jobMaterialSupplyingActivitiesDataAdapter = new SqlDataAdapter("Select * FROM JobMaterialSupplyingActivities", conn);
            //SetDataAdapter(jobMaterialSupplyingActivitiesDataAdapter, false);
            //jobMaterialSupplyingActivitiesDataAdapter.Update(ds, currentTableName);

            //currentTableName = "JobOperationAttributes";
            //SqlDataAdapter jobOperationAttributesDataAdapter = new SqlDataAdapter("Select * FROM JobOperationAttributes", conn);
            //SetDataAdapter(jobOperationAttributesDataAdapter, true);
            //jobOperationAttributesDataAdapter.Update(ds, currentTableName);

            //currentTableName = "JobPaths";
            //SqlDataAdapter jobPathsDataAdapter = new SqlDataAdapter("Select * FROM JobPaths", conn);
            //SetDataAdapter(jobPathsDataAdapter, true);
            //jobPathsDataAdapter.Update(ds, currentTableName);

            //currentTableName = "JobPathNodes";
            //SqlDataAdapter jobPathNodesDataAdapter = new SqlDataAdapter("Select * FROM JobPathNodes", conn);
            //SetDataAdapter(jobPathNodesDataAdapter, false);
            //jobPathNodesDataAdapter.Update(ds, currentTableName);


            //if (aSD.ScenarioPublishOptions.PublishInventory) 
            //{
            //    currentTableName = "Forecasts";
            //    SqlDataAdapter forecastsDataAdapter = new SqlDataAdapter("Select * FROM Forecasts", conn);
            //    SetDataAdapter(forecastsDataAdapter, true);
            //    forecastsDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "ForecastShipments";
            //    SqlDataAdapter forecastShipmentsDataAdapter = new SqlDataAdapter("Select * FROM ForecastShipments", conn);
            //    SetDataAdapter(forecastShipmentsDataAdapter, true);
            //    forecastShipmentsDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "SalesOrders";
            //    SqlDataAdapter salesOrdersDataAdapter = new SqlDataAdapter("Select * FROM SalesOrders", conn);
            //    SetDataAdapter(salesOrdersDataAdapter, true);
            //    salesOrdersDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "SalesOrderLines";
            //    SqlDataAdapter salesOrderLinesDataAdapter = new SqlDataAdapter("Select * FROM SalesOrderLines", conn);
            //    SetDataAdapter(salesOrderLinesDataAdapter, true);
            //    salesOrderLinesDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "SalesOrderLineDistributions";
            //    SqlDataAdapter salesOrderLineDistributionsDataAdapter = new SqlDataAdapter("Select * FROM SalesOrderLineDistributions", conn);
            //    SetDataAdapter(salesOrderLineDistributionsDataAdapter, true);
            //    salesOrderLineDistributionsDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "TransferOrders";
            //    SqlDataAdapter transferOrdersDataAdapter = new SqlDataAdapter("Select * FROM TransferOrders", conn);
            //    SetDataAdapter(transferOrdersDataAdapter, true);
            //    transferOrdersDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "TransferOrderDistributions";
            //    SqlDataAdapter transferOrderDistributionsDataAdapter = new SqlDataAdapter("Select * FROM TransferOrderDistributions", conn);
            //    SetDataAdapter(transferOrderDistributionsDataAdapter, true);
            //    transferOrderDistributionsDataAdapter.Update(ds, currentTableName);

            //    //JobProduct Demands
            //    currentTableName = "JobProductSalesOrderDemands";
            //    SqlDataAdapter JobProductSalesOrderDemandsDataAdapter = new SqlDataAdapter("Select * FROM JobProductSalesOrderDemands", conn);
            //    SetDataAdapter(JobProductSalesOrderDemandsDataAdapter, true);
            //    JobProductSalesOrderDemandsDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "JobProductForecastDemands";
            //    SqlDataAdapter JobProductForecastDemandsDataAdapter = new SqlDataAdapter("Select * FROM JobProductForecastDemands", conn);
            //    SetDataAdapter(JobProductForecastDemandsDataAdapter, true);
            //    JobProductForecastDemandsDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "JobProductSafetyStockDemands";
            //    SqlDataAdapter JobProductSafetyStockDemandsDataAdapter = new SqlDataAdapter("Select * FROM JobProductSafetyStockDemands", conn);
            //    SetDataAdapter(JobProductSafetyStockDemandsDataAdapter, true);
            //    JobProductSafetyStockDemandsDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "JobProductTransferOrderDemands";
            //    SqlDataAdapter JobProductTransferOrderDemandsDataAdapter = new SqlDataAdapter("Select * FROM JobProductTransferOrderDemands", conn);
            //    SetDataAdapter(JobProductTransferOrderDemandsDataAdapter, true);
            //    JobProductTransferOrderDemandsDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "JobProductDeletedDemands";
            //    SqlDataAdapter JobProductDeletedDemandsDataAdapter = new SqlDataAdapter("Select * FROM JobProductDeletedDemands", conn);
            //    SetDataAdapter(JobProductDeletedDemandsDataAdapter, true);
            //    JobProductDeletedDemandsDataAdapter.Update(ds, currentTableName);

            //    //PTS Demands
            //    currentTableName = "PurchaseToStockSalesOrderDemands";
            //    SqlDataAdapter PurchaseToStockSalesOrderDemandsDataAdapter = new SqlDataAdapter("Select * FROM PurchaseToStockSalesOrderDemands", conn);
            //    SetDataAdapter(PurchaseToStockSalesOrderDemandsDataAdapter, true);
            //    PurchaseToStockSalesOrderDemandsDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "PurchaseToStockForecastDemands";
            //    SqlDataAdapter PurchaseToStockForecastDemandsDataAdapter = new SqlDataAdapter("Select * FROM PurchaseToStockForecastDemands", conn);
            //    SetDataAdapter(PurchaseToStockForecastDemandsDataAdapter, true);
            //    PurchaseToStockForecastDemandsDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "PurchaseToStockSafetyStockDemands";
            //    SqlDataAdapter PurchaseToStockSafetyStockDemandsDataAdapter = new SqlDataAdapter("Select * FROM PurchaseToStockSafetyStockDemands", conn);
            //    SetDataAdapter(PurchaseToStockSafetyStockDemandsDataAdapter, true);
            //    PurchaseToStockSafetyStockDemandsDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "PurchaseToStockTransferOrderDemands";
            //    SqlDataAdapter PurchaseToStockTransferOrderDemandsDataAdapter = new SqlDataAdapter("Select * FROM PurchaseToStockTransferOrderDemands", conn);
            //    SetDataAdapter(PurchaseToStockTransferOrderDemandsDataAdapter, true);
            //    PurchaseToStockTransferOrderDemandsDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "PurchaseToStockDeletedDemands";
            //    SqlDataAdapter PurchaseToStockDeletedDemandsDataAdapter = new SqlDataAdapter("Select * FROM PurchaseToStockDeletedDemands", conn);
            //    SetDataAdapter(PurchaseToStockDeletedDemandsDataAdapter, true);
            //    PurchaseToStockDeletedDemandsDataAdapter.Update(ds, currentTableName);

            //    //Inventory Adjustments
            //    currentTableName = "JobActivityInventoryAdjustments";
            //    SqlDataAdapter JobActivityInventoryAdjustmentsDataAdapter = new SqlDataAdapter("Select * FROM JobActivityInventoryAdjustments", conn);
            //    SetDataAdapter(JobActivityInventoryAdjustmentsDataAdapter, false);
            //    JobActivityInventoryAdjustmentsDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "SalesOrderDistributionInventoryAdjustments";
            //    SqlDataAdapter SalesOrderDistributionInventoryAdjustmentsDataAdapter = new SqlDataAdapter("Select * FROM SalesOrderDistributionInventoryAdjustments", conn);
            //    SetDataAdapter(SalesOrderDistributionInventoryAdjustmentsDataAdapter, true);
            //    SalesOrderDistributionInventoryAdjustmentsDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "ForecastShipmentInventoryAdjustments";
            //    SqlDataAdapter ForecastShipmentInventoryAdjustmentsDataAdapter = new SqlDataAdapter("Select * FROM ForecastShipmentInventoryAdjustments", conn);
            //    SetDataAdapter(ForecastShipmentInventoryAdjustmentsDataAdapter, true);
            //    ForecastShipmentInventoryAdjustmentsDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "PurchaseToStockInventoryAdjustments";
            //    SqlDataAdapter PurchaseToStockInventoryAdjustmentsDataAdapter = new SqlDataAdapter("Select * FROM PurchaseToStockInventoryAdjustments", conn);
            //    SetDataAdapter(PurchaseToStockInventoryAdjustmentsDataAdapter, true);
            //    PurchaseToStockInventoryAdjustmentsDataAdapter.Update(ds, currentTableName);

            //    currentTableName = "TransferOrderDistributionInventoryAdjustments";
            //    SqlDataAdapter TransferOrderDistributionInventoryAdjustmentsDataAdapter = new SqlDataAdapter("Select * FROM TransferOrderDistributionInventoryAdjustments", conn);
            //    SetDataAdapter(TransferOrderDistributionInventoryAdjustmentsDataAdapter, true);
            //    TransferOrderDistributionInventoryAdjustmentsDataAdapter.Update(ds, currentTableName);
            //}

            //if (aSD.ScenarioPublishOptions.PublishCapacityIntervals) //tables added later; this allow for them to not be published thus avoiding an errror when publishing to old databases.
            //{
            //    //Capacity Intervals
            //    currentTableName = "CapacityIntervals";
            //    SqlDataAdapter capacityIntervalsDataAdapter = new SqlDataAdapter("Select * FROM CapacityIntervals", conn);
            //    SetDataAdapter(capacityIntervalsDataAdapter, true);
            //    capacityIntervalsDataAdapter.Update(ds, "CapacityIntervals");

            //    currentTableName = "CapacityIntervalResourceAssignments";
            //    SqlDataAdapter capacityIntervalResourceAssignmentsDataAdapter = new SqlDataAdapter("Select * FROM CapacityIntervalResourceAssignments", conn);
            //    SetDataAdapter(capacityIntervalResourceAssignmentsDataAdapter, true);
            //    capacityIntervalResourceAssignmentsDataAdapter.Update(ds, "CapacityIntervalResourceAssignments");

            //    currentTableName = "RecurringCapacityIntervals";
            //    SqlDataAdapter recurringCapacityIntervalsDataAdapter = new SqlDataAdapter("Select * FROM RecurringCapacityIntervals", conn);
            //    SetDataAdapter(recurringCapacityIntervalsDataAdapter, true);
            //    recurringCapacityIntervalsDataAdapter.Update(ds, "RecurringCapacityIntervals");

            //    currentTableName = "RecurringCapacityIntervalResourceAssignments";
            //    SqlDataAdapter recurringCapacityIntervalResourceAssignmentsDataAdapter = new SqlDataAdapter("Select * FROM RecurringCapacityIntervalResourceAssignments", conn);
            //    SetDataAdapter(recurringCapacityIntervalResourceAssignmentsDataAdapter, true);
            //    recurringCapacityIntervalResourceAssignmentsDataAdapter.Update(ds, "RecurringCapacityIntervalResourceAssignments");

            //    currentTableName = "RecurringCapacityIntervalRecurrences";
            //    SqlDataAdapter recurringCapacityIntervalRecurrencesDataAdapter = new SqlDataAdapter("Select * FROM RecurringCapacityIntervalRecurrences", conn);
            //    SetDataAdapter(recurringCapacityIntervalRecurrencesDataAdapter, true);
            //    recurringCapacityIntervalRecurrencesDataAdapter.Update(ds, "RecurringCapacityIntervalRecurrences");
            //}

            //currentTableName = "KPIs";
            //SqlDataAdapter KPIsDataAdapter = new SqlDataAdapter("Select * FROM KPIs", conn);
            //SetDataAdapter(KPIsDataAdapter, false);
            //KPIsDataAdapter.Update(ds, currentTableName);
            #endregion
        }
        catch (Exception e)
        {
            throw new PTDatabaseException("2740", e, new object[] { currentTableName, e.GetExceptionFullMessage() });
        }
    }

    /// <summary>
    /// Make sure all dates in all tables are valid for sql server.
    /// </summary>
    private void PrepDataForSQLServerPublish(PtDbDataSet ds, ScenarioDetail a_sd)
    {
        for (int tableI = 0; tableI < ds.Tables.Count; tableI++)
        {
            DataTable table = ds.Tables[tableI];

            for (int rowI = 0; rowI < table.Rows.Count; rowI++)
            {
                DataRow row = table.Rows[rowI];
                row.BeginEdit();
                for (int colI = 0; colI < table.Columns.Count; colI++)
                {
                    DataColumn col = table.Columns[colI];

                    if (col.DataType == typeof(DateTime))
                    {
                        try
                        {
                            if (row[colI] == DBNull.Value)
                            {
                                row[colI] = SQLServerConversions.GetValidDateTime(PTDateTime.MinDateTime);
                            }

                            row[colI] = SQLServerConversions.GetValidDateTime((DateTime)row[colI]);
                        }
                        catch (Exception err)
                        {
                            throw new PTException("4076", new object[] { tableI, rowI, colI, err.Message });
                        }
                    }

                    if (row[colI] == DBNull.Value)
                    {
                        //Value is NULL, nothing to round.
                        continue;
                    }

                    if (col.DataType == typeof(double))
                    {
                        try
                        {
                            row[colI] = a_sd.ScenarioOptions.RoundQty((double)row[colI]);
                        }
                        catch (Exception err)
                        {
                            throw new PTException("4077", new object[] { tableI, rowI, colI, err.Message });
                        }
                    }
                    else if (col.DataType == typeof(decimal))
                    {
                        try
                        {
                            row[colI] = a_sd.ScenarioOptions.RoundQty((decimal)row[colI]);
                        }
                        catch (Exception err)
                        {
                            throw new PTException("4078", new object[] { tableI, rowI, colI, err.Message });
                        }
                    }
                }

                row.EndEdit();
            }
        }
    }

    /// <summary>
    /// Clear and Update flags like LastLiveSchedulePublished or LastPublishForScenario in Schedules Table.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_conn"></param>
    private void UpdateSchedulesTableFlags(SqlConnection a_conn, ScenarioDetail a_sd, bool a_netChange)
    {
        string currentTaskDescr = "";
        ScenarioSummary ss;

        try
        {
            if (a_netChange)
            {
                using (a_sd.Scenario.ScenarioSummaryLock.EnterRead(out ss))
                {
                    //Update the fields that specify the last schedule of each type
                    if (ss.ScenarioSettings.LoadSetting(new ScenarioPlanningSettings()).Production)
                    {
                        currentTaskDescr = "Schedules Clear LastNetChangeProductionSchedulePublished Field";
                        SqlCommand cmd = new ("UPDATE Schedules SET LastNetChangeProductionSchedulePublished = 0", a_conn);
                        cmd.CommandTimeout = 0;
                        cmd.ExecuteNonQuery();
                    }
                    //else if (ss.Type == ScenarioTypes.Published)
                    //{
                    //    currentTaskDescr = "Schedules Clear LastNetChangePublishedSchedulePublished Field";
                    //    SqlCommand cmd = new SqlCommand("UPDATE Schedules SET LastNetChangePublishedSchedulePublished = 0", a_conn);
                    //    cmd.CommandTimeout = 0;
                    //    cmd.ExecuteNonQuery();
                    //}
                }
            }
            else
            {
                //Update the fields that specify the last schedule of each type
                currentTaskDescr = "Schedules Clear LastSchedulePublished Field";
                SqlCommand cmd = new ("UPDATE Schedules SET LastSchedulePublished = 0", a_conn);
                cmd.CommandTimeout = 0; //Fix for Maximus.  Was timing out on export.  0 means wait until done.
                cmd.ExecuteNonQuery();

                currentTaskDescr = "Schedules Clear LastSchedulePublishedType Field";
                using (a_sd.Scenario.ScenarioSummaryLock.EnterRead(out ss))
                {
                    if (ss.ScenarioSettings.LoadSetting(new ScenarioPlanningSettings()).Production)
                    {
                        cmd = new SqlCommand("UPDATE Schedules SET LastProductionSchedulePublished = 0", a_conn);
                        cmd.ExecuteNonQuery();
                    }
                    else
                    {
                        cmd = new SqlCommand("UPDATE Schedules SET LastWhatIfSchedulePublished = 0", a_conn);
                        cmd.ExecuteNonQuery();
                    }

                    // Set LastPublishForScenario = 0 for publishes with same ScenarioId
                    currentTaskDescr = "Schedules Clear LastPublishForScenario Field";
                    cmd = new SqlCommand(string.Format("UPDATE Schedules SET LastPublishForScenario = 0 where ScenarioId = {0}", m_scenarioId.ToBaseType()), a_conn);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception e)
        {
            throw new PTDatabaseException("2740", e, new object[] { currentTaskDescr, e.GetExceptionFullMessage() });
        }
    }

    /// <summary>
    /// Add table names in the order that the data will be inserted into the database.
    /// </summary>
    /// <param name="aSD"></param>
    /// <returns></returns>
    private List<string> RetrieveOrderedDataTablesToInsert(ScenarioDetail aSD)
    {
        ScenarioPublishDataLimits scenarioPublishDataLimits = new ScenarioPublishDataLimits();
        using (aSD.Scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
        {
            scenarioPublishDataLimits = ss.ScenarioSettings.LoadSetting(scenarioPublishDataLimits);
        }
        List<string> tableNameList = new ();
        tableNameList.Add("Schedules");
        tableNameList.Add("Plants");
        tableNameList.Add("Departments");
        tableNameList.Add("Resources");
        tableNameList.Add("Capabilities");
        tableNameList.Add("ResourceCapabilities");
        
        if (scenarioPublishDataLimits.PublishInventory) //Publish before Jobs since Jobs reference these.
        {
            tableNameList.Add("Warehouses");
            tableNameList.Add("PlantWarehouses");
            tableNameList.Add("Items");
            tableNameList.Add("Inventories");
            tableNameList.Add("Lots");
            tableNameList.Add("PurchasesToStock");
            tableNameList.Add("StorageAreas");
            tableNameList.Add("StorageAreaConnector");
            tableNameList.Add("StorageAreaConnectorIn");
            tableNameList.Add("StorageAreaConnectorOut");
            tableNameList.Add("ResourceStorageAreaConnectorIn");
            tableNameList.Add("ResourceStorageAreaConnectorOut");
        }

        tableNameList.Add("PTAttributes");

        tableNameList.Add("Jobs");
        tableNameList.Add("ManufacturingOrders");
        tableNameList.Add("JobSuccessorManufacturingOrders");
        tableNameList.Add("JobOperations");
        tableNameList.Add("JobResources");
        tableNameList.Add("JobResourceCapabilities");
        tableNameList.Add("JobMaterials");
        tableNameList.Add("JobProducts");
        tableNameList.Add("JobActivities");
        tableNameList.Add("JobResourceBlocks");
        tableNameList.Add("JobResourceBlockIntervals");
        tableNameList.Add("JobMaterialSupplyingActivities");
        tableNameList.Add("JobOperationAttributes");
        tableNameList.Add("JobPaths");
        tableNameList.Add("JobPathNodes");
        tableNameList.Add("ReportBlocks");

        if (scenarioPublishDataLimits.PublishInventory)
        {
            tableNameList.Add("Forecasts");
            tableNameList.Add("ForecastShipments");
            tableNameList.Add("SalesOrders");
            tableNameList.Add("SalesOrderLines");
            tableNameList.Add("SalesOrderLineDistributions");
            tableNameList.Add("TransferOrders");
            tableNameList.Add("TransferOrderDistributions");
            tableNameList.Add("JobProductSalesOrderDemands");
            tableNameList.Add("JobProductForecastDemands");
            tableNameList.Add("JobProductSafetyStockDemands");
            tableNameList.Add("JobProductTransferOrderDemands");
            tableNameList.Add("JobProductDeletedDemands");
            tableNameList.Add("PurchaseToStockSalesOrderDemands");
            tableNameList.Add("PurchaseToStockForecastDemands");
            tableNameList.Add("PurchaseToStockSafetyStockDemands");
            tableNameList.Add("PurchaseToStockTransferOrderDemands");
            tableNameList.Add("PurchaseToStockDeletedDemands");
            tableNameList.Add("JobActivityInventoryAdjustments");
            tableNameList.Add("SalesOrderDistributionInventoryAdjustments");
            tableNameList.Add("ForecastShipmentInventoryAdjustments");
            tableNameList.Add("PurchaseToStockInventoryAdjustments");
            tableNameList.Add("TransferOrderDistributionInventoryAdjustments");
        }

        if (scenarioPublishDataLimits.PublishProductRules)
        {
            tableNameList.Add("ProductRules");
        }

        if (scenarioPublishDataLimits.PublishCapacityIntervals) //tables added later; this allow for them to not be published thus avoiding an errror when publishing to old databases.
        {
            tableNameList.Add("CapacityIntervals");
            tableNameList.Add("CapacityIntervalResourceAssignments");
            tableNameList.Add("RecurringCapacityIntervals");
            tableNameList.Add("RecurringCapacityIntervalResourceAssignments");
            tableNameList.Add("RecurringCapacityIntervalRecurrences");
        }

        tableNameList.Add("KPIs");
        tableNameList.Add("Metrics");
        tableNameList.Add("Customers");
        return tableNameList;
    }

    private void RemoveOldHistoryData(SqlConnection conn, ScenarioDetail sd)
    {
        //Clear the database completely if not using History
        try
        {
            ScenarioPublishDataLimits scenarioPublishDataLimits = new ScenarioPublishDataLimits();
            ScenarioPublishHistory history = new ScenarioPublishHistory();
            using (sd.Scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
            {
                scenarioPublishDataLimits = ss.ScenarioSettings.LoadSetting(scenarioPublishDataLimits);
                history = ss.ScenarioSettings.LoadSetting(history);
            }

            if (!history.EnableHistory)
            {
                using (SqlCommand cmd = new ("ClearAllHistoryData", conn))
                {
                    cmd.CommandTimeout = 0;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@PublishInventory", SqlDbType.Bit));
                    cmd.Parameters["@PublishInventory"].Value = scenarioPublishDataLimits.PublishInventory;
                    cmd.Parameters.Add(new SqlParameter("@PublishCapacityIntervals", SqlDbType.Bit));
                    cmd.Parameters["@PublishCapacityIntervals"].Value = scenarioPublishDataLimits.PublishCapacityIntervals;
                    cmd.ExecuteNonQuery();
                }
            }
            else
            {
                using (SqlCommand cmd = new ("ClearOldHistoryData", conn))
                {
                    DateTime maxScheduleDisplayDate = DateTime.Now.Subtract(history.HistoryMaxAge);
                    DateTime maxScheduleWhatifDisplayDate = DateTime.Now.Subtract(history.WhatIfHistoryMaxAge);

                    cmd.CommandTimeout = 0;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@MaxScheduleDisplayDate", SqlDbType.DateTime));
                    cmd.Parameters["@MaxScheduleDisplayDate"].Value = maxScheduleDisplayDate;
                    cmd.Parameters.Add(new SqlParameter("@MaxWhatIfDisplayDate", SqlDbType.DateTime));
                    cmd.Parameters["@MaxWhatIfDisplayDate"].Value = maxScheduleWhatifDisplayDate;
                    cmd.Parameters.Add(new SqlParameter("@ScenarioType", SqlDbType.NVarChar));
                    cmd.Parameters["@ScenarioType"].Value = SCENARIO_TYPE_WHATIF;

                    cmd.Parameters.Add(new SqlParameter("@ClearInventory", SqlDbType.Bit));
                    cmd.Parameters["@ClearInventory"].Value = !scenarioPublishDataLimits.KeepHistoryInventory;
                    cmd.Parameters.Add(new SqlParameter("@ClearProductRules", SqlDbType.Bit));
                    cmd.Parameters["@ClearProductRules"].Value = !scenarioPublishDataLimits.KeepHistoryProductRules;
                    cmd.Parameters.Add(new SqlParameter("@ClearCapacityIntervals", SqlDbType.Bit));
                    cmd.Parameters["@ClearCapacityIntervals"].Value = !scenarioPublishDataLimits.KeepCapacityIntervals;
                    cmd.Parameters.Add(new SqlParameter("@MaxHorizonDays", SqlDbType.Int));
                    cmd.Parameters["@MaxHorizonDays"].Value = history.HistoryHorizonSpan.TotalDays;

                    cmd.Parameters.Add(new SqlParameter("@ClearJobs", SqlDbType.Bit));
                    cmd.Parameters["@ClearJobs"].Value = !scenarioPublishDataLimits.KeepHistoryJobs;

                    cmd.Parameters.Add(new SqlParameter("@ClearTemplates", SqlDbType.Bit));
                    cmd.Parameters["@ClearTemplates"].Value = !scenarioPublishDataLimits.KeepHistoryTemplates;

                    cmd.Parameters.Add(new SqlParameter("@ClearManufacturingOrders", SqlDbType.Bit));
                    cmd.Parameters["@ClearManufacturingOrders"].Value = !scenarioPublishDataLimits.KeepHistoryManufacturingOrders;

                    cmd.Parameters.Add(new SqlParameter("@ClearOperations", SqlDbType.Bit));
                    cmd.Parameters["@ClearOperations"].Value = !scenarioPublishDataLimits.KeepHistoryOperations;

                    cmd.Parameters.Add(new SqlParameter("@ClearActivities", SqlDbType.Bit));
                    cmd.Parameters["@ClearActivities"].Value = !scenarioPublishDataLimits.KeepHistoryActivities;

                    cmd.Parameters.Add(new SqlParameter("@ClearBlocks", SqlDbType.Bit));
                    cmd.Parameters["@ClearBlocks"].Value = !scenarioPublishDataLimits.KeepHistoryBlocks;

                    cmd.Parameters.Add(new SqlParameter("@ClearBlockIntervals", SqlDbType.Bit));
                    cmd.Parameters["@ClearBlockIntervals"].Value = !scenarioPublishDataLimits.KeepHistoryBlockIntervals;

                    cmd.ExecuteNonQuery();
                }
            }
        }
        catch (Exception e)
        {
            //   time it's logged so the root error doesn't get logged.
            throw new PTDatabaseException("2739", e, new object[] { e.Message });
        }
    }

    private const string SCENARIO_TYPE_PRODUCTION = "Production";
    private const string SCENARIO_TYPE_WHATIF = "What-If";

    /// <summary>
    /// populate Schedules table and return it. This also sets o_publishDate.
    /// </summary>
    /// <param name="a_ss"></param>
    /// <param name="a_sd"></param>
    /// <param name="a_instigator"></param>
    /// <param name="a_helper"></param>
    /// <param name="a_netChange"></param>
    /// <param name="r_ds"></param>
    /// <param name="o_publishDate"></param>
    /// <returns></returns>
    private PtDbDataSet.SchedulesRow PopulateSchedulesTable(ScenarioSummary a_ss, BaseId a_instigator, PTDatabaseHelper a_helper, bool a_netChange, ref PtDbDataSet r_ds, out DateTime o_publishDate)
    {
        // TODO: ScenarioType could be made a production bool in future, but for now certain history stored procedures are expecting a string column with this name
        bool isProduction = a_ss.ScenarioSettings.LoadSetting(new ScenarioPlanningSettings()).Production;
        string scenarioType = isProduction ? SCENARIO_TYPE_PRODUCTION : SCENARIO_TYPE_WHATIF;

        bool lastProductionSchedulePublished = !a_netChange && isProduction;
        bool lastWhatIfSchedulePublished = !a_netChange && !isProduction;
        bool lastNetChangeProductionSchedulePublished = a_netChange && isProduction;
        bool lastPublishForScenario = !a_netChange;

        o_publishDate = a_helper.AdjustPublishTime(PTDateTime.UtcNow).ToDateTime();
        PtDbDataSet.SchedulesRow schedulesRow = r_ds.Schedules.AddSchedulesRow(
                o_publishDate,
                m_instanceId,
                m_instanceName,
                m_instanceVersion,
                m_scenarioId.ToBaseType(),
                a_ss.Name,
                a_ss.Description,
                a_instigator.ToBaseType(),
                false,
                a_helper.AdjustPublishTime(m_sd.ClockDate),
                a_helper.AdjustPublishTime(a_helper.MaxPublishDate),
                scenarioType,
                true,
                lastProductionSchedulePublished,
                lastWhatIfSchedulePublished,
                lastNetChangeProductionSchedulePublished,
                lastPublishForScenario,
                a_helper.AdjustPublishTime(m_sd.GetPlanningHorizonEnd()),
                a_helper.GetTimeZoneId(),
                m_instanceEnvType);
        // Should we be using PTDateTime.UserDateTimeNow to determine the offset?

        return schedulesRow;
    }

    private DateTime GetPublishHorizonEnd(ScenarioDetail a_sd, bool a_includeAllData)
    {
        //Limit what's included based on the Publish options unless the option to include all data is set (for internal viewing)
        if (!a_includeAllData)
        {
            ScenarioPublishHistory history = new ScenarioPublishHistory();
            using (a_sd.Scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
            {
                history = ss.ScenarioSettings.LoadSetting(history);
            }
            return a_sd.ClockDate.Add(history.PublishHorizonSpan);
        }

        return PTDateTime.MaxDateTime.ToUniversalTime();
    }

    private PtDbDataSet PopulateDataSet(BaseId instigator, bool includeAllData, bool limitJobsToList, HashSet<BaseId> jobIdList, bool limitToResourceList, HashSet<BaseId> resourceIds, HashSet<BaseId> a_itemsToInclude, out DateTime o_publishDate)
    {
        PtDbDataSet ds = new();

        try
        {
            ds.EnforceConstraints = false;
            ds.BeginLoadData();
            
            DateTime maxPublishDate = GetPublishHorizonEnd(m_sd, includeAllData);
            TimeZoneInfo publishTimeZone = PTSystem.ServerTimeZone;
            PTDatabaseHelper dbHelper = new (publishTimeZone, includeAllData, maxPublishDate, PTSystem.LicenseKey.IncludeDetailedScheduling);
            PtDbDataSet.SchedulesRow schedulesRow;
            ScenarioPublishDataLimits scenarioPublishDataLimits = new ScenarioPublishDataLimits();
            Dictionary<string, IUserFieldDefinition> udfDefinitions;
            using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager scenarioManager))
            {
                dbHelper.UserFieldDefinitions = scenarioManager.UserFieldDefinitionManager.GenerateFastLookupByExternalId();
            }

            //Convert to interface so it can be used in base classes. //TODO: Maybe there is a better way to do this?
            

            using (m_sd.Scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
            {
                schedulesRow = PopulateSchedulesTable(ss, instigator, dbHelper, false, ref ds, out o_publishDate);
                scenarioPublishDataLimits = ss.ScenarioSettings.LoadSetting(scenarioPublishDataLimits);
            }

            bool includeInventory = scenarioPublishDataLimits.PublishInventory || includeAllData;
            bool includeProductRules = scenarioPublishDataLimits.PublishProductRules || includeAllData;
            bool includeJobs = scenarioPublishDataLimits.PublishJobs || includeAllData;
            UpdateProgressPercent(1);

            //Call top level ScenarioDetail objects to populate the DataSet with current values.
            FireStatusEvent(PublishStatuses.EPublishProgressStep.PreparingPlants);
            m_sd.PlantManager.PtDbPopulate(ref ds, schedulesRow, limitToResourceList, resourceIds, m_sd, dbHelper);
            UpdateProgressPercent(4);
            if (includeInventory)
            {
                FireStatusEvent(PublishStatuses.EPublishProgressStep.PreparingItems);
                m_sd.ItemManager.PtDbPopulate(ref ds, schedulesRow, m_sd, dbHelper);
            }

            UpdateProgressPercent(10);
            if (includeInventory)
            {
                FireStatusEvent(PublishStatuses.EPublishProgressStep.PreparingInventory);
                m_sd.WarehouseManager.PtDbPopulate(m_sd, ref ds, ds.Items, schedulesRow, a_itemsToInclude, dbHelper); //must populate Items first
                AddPlantWarehouses(ds, m_sd.PlantManager, schedulesRow);
            }

            UpdateProgressPercent(10);
            FireStatusEvent(PublishStatuses.EPublishProgressStep.PreparingCapabilities);
            m_sd.CapabilityManager.PtDbPopulate(ref ds, schedulesRow, m_sd, dbHelper);
            UpdateProgressPercent(5);

            FireStatusEvent(PublishStatuses.EPublishProgressStep.PreparingAttributes);
            m_sd.AttributeManager.PtDbPopulate(
                ref ds,
                dbHelper,
                schedulesRow
            );
            UpdateProgressPercent(5);

            if (includeJobs)
            {
                FireStatusEvent(PublishStatuses.EPublishProgressStep.PreparingJobs);
                m_sd.JobManager.PtDbPopulate(m_sd,
                    ref ds,
                    m_sd.PlantManager,
                    schedulesRow,
                    includeInventory,
                    limitJobsToList,
                    jobIdList,
                    limitToResourceList,
                    resourceIds,
                    dbHelper);
            }

            UpdateProgressPercent(15);

            if (includeInventory)
            {
                FireStatusEvent(PublishStatuses.EPublishProgressStep.PreparingDemands);
                m_sd.PurchaseToStockManager.PtDbPopulate(ref ds, schedulesRow, m_sd, dbHelper);
                m_sd.SalesOrderManager.PtDbPopulate(ref ds, schedulesRow, dbHelper, m_sd);
                m_sd.TransferOrderManager.PtDbPopulate(ref ds, schedulesRow, dbHelper);
            }

            UpdateProgressPercent(10);

            if (includeProductRules)
            {
                FireStatusEvent(PublishStatuses.EPublishProgressStep.PreparingProductRules);
                PtDbPopulateProductRules(ref ds, dbHelper, m_sd, schedulesRow);
            }

            UpdateProgressPercent(5);

            if (scenarioPublishDataLimits.PublishCapacityIntervals)
            {
                FireStatusEvent(PublishStatuses.EPublishProgressStep.PreparingCapacity);
                m_sd.CapacityIntervalManager.PtDbPopulate(ref ds, schedulesRow, limitToResourceList, resourceIds, dbHelper);
                m_sd.RecurringCapacityIntervalManager.PtDbPopulate(ref ds, schedulesRow, limitToResourceList, resourceIds, dbHelper);
            }

            UpdateProgressPercent(5);

            //KPIs
            FireStatusEvent(PublishStatuses.EPublishProgressStep.PreparingKPIs);
            m_sd.Scenario.KpiController.PtDbPopulate(ref ds, schedulesRow);
            UpdateProgressPercent(4);

            //Metrics
            FireStatusEvent(PublishStatuses.EPublishProgressStep.PreparingMetrics);
            Metric metricSet = new (m_sd);
            metricSet.PtDbPopulate(ref ds, dbHelper, schedulesRow);
            UpdateProgressPercent(3);

            //Customers
            FireStatusEvent(PublishStatuses.EPublishProgressStep.PreparingCustomers);
            m_sd.CustomerManager.PtDbPopulate(ref ds, dbHelper, schedulesRow);
            UpdateProgressPercent(3);
            
            ds.EndLoadData();
        }
        catch (Exception e)
        {
            throw new PTDatabaseException("2741", e, new object[] { e.Message, e.StackTrace });
        }

        return ds;
    }

    private PtDbDataSet PopulateJobDataSet(ScenarioDetail a_sd, BaseId a_instigator, HashSet<BaseId> a_jobIdList, bool a_publishUsingLocalTimeOverride)
    {
        PtDbDataSet ds = new();
        try
        {
            ds.EnforceConstraints = false;
            ds.BeginLoadData();
            DateTime maxPublishDate = GetPublishHorizonEnd(a_sd, false);
            TimeZoneInfo publishTimeZone = a_publishUsingLocalTimeOverride ? TimeZoneAdjuster.CurrentTimeZoneInfo : PTSystem.ServerTimeZone;
            PTDatabaseHelper dbHelper = new (publishTimeZone, false, maxPublishDate, PTSystem.LicenseKey.IncludeDetailedScheduling);
            PtDbDataSet.SchedulesRow schedulesRow;
            
            using (a_sd.Scenario.ScenarioSummaryLock.EnterRead(out ScenarioSummary ss))
            {
                schedulesRow = PopulateSchedulesTable(ss, a_instigator, dbHelper, false, ref ds, out DateTime publishDate);
            }
            
            using (SystemController.Sys.ScenariosLock.EnterRead(out ScenarioManager scenarioManager))
            {
                dbHelper.UserFieldDefinitions = scenarioManager.UserFieldDefinitionManager.GenerateFastLookupByExternalId();
            }
            
            UpdateProgressPercent(50);
            FireStatusEvent(PublishStatuses.EPublishProgressStep.PreparingJobs);
            a_sd.JobManager.PtDbPopulate(a_sd,
                ref ds,
                a_sd.PlantManager,
                schedulesRow,
                false,
                true,
                a_jobIdList,
                false,
                null,
                dbHelper);
            UpdateProgressPercent(50);
            
            ds.EndLoadData();
        }
        catch (Exception e)
        {
            throw new PTDatabaseException("2741", e, new object[] { e.Message, e.StackTrace });
        }

        return ds;
    }

    private void UpdateProgressPercent(double a_percentIncrease)
    {
        m_currentProgressPoints += m_allowedProgressPoints * (a_percentIncrease / 100);
    }

    /// <summary>
    /// Populates ProductRules table in PtDbDataSet.
    /// </summary>
    /// <param name="a_ds">ref dataset to populate</param>
    /// <param name="a_plants"></param>
    /// <param name="a_scheduleRow">schedule row containing publish date</param>
    /// <param name="a_sd"></param>
    private void PtDbPopulateProductRules(ref PtDbDataSet a_ds, PTDatabaseHelper a_dbHelper, ScenarioDetail a_sd, PtDbDataSet.SchedulesRow a_scheduleRow)
    {
        PlantManager plants = a_sd.PlantManager;
        for (int plantI = 0; plantI < plants.Count; plantI++)
        {
            Plant plant = plants[plantI];

            for (int deptI = 0; deptI < plant.DepartmentCount; deptI++)
            {
                Department dept = plant.Departments.GetByIndex(deptI);

                for (int resI = 0; resI < dept.ResourceCount; resI++)
                {
                    Resource res = dept.Resources.GetByIndex(resI);

                    foreach(ProductRule rule in a_sd.ProductRuleManager.EnumerateForResource(res.Id))
                    {
                        PtDbDataSet.ProductRulesRow ruleRow = a_ds.ProductRules.AddProductRulesRow(
                            a_scheduleRow,
                            m_instanceId,
                            rule.ItemId.ToBaseType(),
                            plant.Id.ToBaseType(),
                            dept.Id.ToBaseType(),
                            res.Id.ToBaseType(),
                            rule.ProductCode,
                            rule.SetupSpan.TotalHours,
                            rule.UseSetupSpan,
                            rule.ProductionSetupCost,
                            rule.UseProductionSetupCost,
                            rule.CycleSpan.TotalHours,
                            rule.UseCycleSpan,
                            rule.QtyPerCycle,
                            rule.UseQtyPerCycle,
                            rule.MaterialPostProcessingSpan.TotalHours,
                            rule.UseMaterialPostProcessingSpan,
                            rule.PostProcessingSpan.TotalHours,
                            rule.UsePostProcessingSpan,
                            Convert.ToDouble(rule.PlanningScrapPercent),
                            rule.UsePlanningScrapPercent,
                            rule.HeadStartSpan.TotalHours,
                            rule.UseHeadStartSpan,
                            rule.MinVolume,
                            rule.UseMinVolume,
                            rule.MaxVolume,
                            rule.UseMaxVolume,
                            rule.UseCleanSpan,
                            rule.CleanSpan.TotalHours,
                            rule.CleanoutCost,
                            rule.UseCleanoutCost,
                            rule.CleanoutUnitsRatio,
                            rule.UseCleanoutUnits,
                            rule.TransferQty,
                            rule.UseTransferQty,
                            rule.Priority,
                            rule.UsePriority
                        );

                        rule.PopulateUserFields(ruleRow, a_dbHelper.UserFieldDefinitions, a_sd);
                    }
                }
            }
        }
    }

    private void AddPlantWarehouses(PtDbDataSet ds, PlantManager plants, PtDbDataSet.SchedulesRow schedulesRow)
    {
        for (int plantI = 0; plantI < plants.Count; plantI++)
        {
            Plant plant = plants[plantI];
            PtDbDataSet.PlantsRow plantRow = ds.Plants.FindByPublishDatePlantIdInstanceId(schedulesRow.PublishDate, plant.Id.ToBaseType(), m_instanceId);
            for (int wI = 0; wI < plant.WarehouseCount; wI++)
            {
                Warehouse w = plant.GetWarehouseAtIndex(wI);
                PtDbDataSet.WarehousesRow warehouseRow = ds.Warehouses.FindByPublishDateWarehouseIdInstanceId(schedulesRow.PublishDate, w.Id.ToBaseType(), m_instanceId);
                ds.PlantWarehouses.AddPlantWarehousesRow(schedulesRow.PublishDate, m_instanceId, plantRow.PlantId, warehouseRow.WarehouseId);
            }
        }
    }

    private void SetDataAdapter(SqlDataAdapter da, bool hasKeyColumns)
    {
        try
        {
            SqlCommandBuilder cb = new (da);
            da.InsertCommand = cb.GetInsertCommand();
            if (hasKeyColumns) //can't generate delete code if there are no key columns
            {
                da.DeleteCommand = cb.GetDeleteCommand();
                da.UpdateCommand = cb.GetUpdateCommand();
            }

            da.InsertCommand.UpdatedRowSource = UpdateRowSource.OutputParameters;

            da.ContinueUpdateOnError = false; //to fail silently set to true
        }
        catch (Exception e)
        {
            if (da.SelectCommand != null)
            {
                throw new PTException("4080", new object[] { da.SelectCommand.CommandText, e.Message });
            }

            throw new PTException("4079", new object[] { e.Message });
        }
    }

    public PtDbDataSet GetPtDataSet(ScenarioDetailExportT t, bool includeAllData)
    {
        return PopulateDataSet(t.Instigator, includeAllData, false, null, t.LimitToSpecifiedResources, t.ResourcesToExport, null, out DateTime publishDate);
    }

    public PtDbDataSet GetPtDataSet(BaseId instigator, bool includeAllData)
    {
        return PopulateDataSet(instigator, includeAllData, false, null, false, null, null, out DateTime publishDate);
    }

    public PtDbDataSet GetPtDataSetForReport(BaseId instigator, bool includeAllData, HashSet<BaseId> jobIdsToInclude, HashSet<BaseId> a_itemsToInclude, bool a_limitJobsToList)
    {
        return PopulateDataSet(instigator, includeAllData, a_limitJobsToList, jobIdsToInclude, false, null, a_itemsToInclude, out DateTime publishDate);
    }

    public PtDbDataSet GetPtDataSetForJobsReport(BaseId a_instigator, HashSet<BaseId> a_jobIdsToInclude)
    {
        return PopulateJobDataSet(m_sd, a_instigator, a_jobIdsToInclude, true);
    }

    public void CancelRunStoredProcedure(SqlCommand a_cmd)
    {
        a_cmd.Cancel();
    }

    /// <summary>
    /// Run a some SQL command.
    /// </summary>
    /// <param name="a_connStr"></param>
    /// <param name="a_sqlToRun"></param>
    public void RunSqlCommand(string a_connStr, string a_sqlToRun, int a_timeout = 30)
    {
        try
        {
            using (SqlConnection conn = new (a_connStr))
            {
                SqlCommand cmd = new (a_sqlToRun, conn);
                cmd.CommandTimeout = a_timeout;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        catch (Exception err)
        {
            throw new PTDatabaseException("Error running SQL Command", err);
        }
    }

    /// <summary>
    /// Run a stored procedure
    /// </summary>
    public async Task<bool> RunStoredProcedureAsync(string a_connStr, string a_storedProcedureName, ScenarioDetailExportT a_t, CancellationToken a_cancelToken)
    {
        try
        {
            using (SqlConnection conn = new (a_connStr))
            {
                SqlCommand cmd = new (a_storedProcedureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 0;
                conn.Open();

                if (a_cancelToken == CancellationToken.None)
                {
                    await cmd.ExecuteNonQueryAsync();
                }
                else
                {
                    a_cancelToken.Register(() =>
                    {
                        cmd.Cancel();
                        cmd.Dispose();
                    });

                    Task<int> result = cmd.ExecuteNonQueryAsync(a_cancelToken);
                    await result;
                    if (result.Exception != null)
                    {
                        throw result.Exception;
                    }
                }

                return true;
            }
        }
        catch (Exception e)
        {
            if (a_cancelToken.IsCancellationRequested == false)
            {
                throw new PTDatabaseException("4117", e, new object[] { a_storedProcedureName });
            }

            return false; // indicate that this was canceled;
        }
    }

    /// <summary>
    /// Run the Stored Procedure that can be specified in the ExtraServices app.config file.
    /// Anything after the first space in the storedProcedureName is passed as the 'UserDefinedParameter' string.
    /// </summary>
    public void RunStoredProcedure(string storedProcedureDbSQLConnectionString, string storedProcedureName, DateTime publishDate, out string returnMessage, out bool undo)
    {
        try
        {
            using (SqlConnection conn = new (storedProcedureDbSQLConnectionString))
            {
                conn.Open();


                //Check for user parameters -- anything after the first embedded character
                storedProcedureName = storedProcedureName.Trim();
                string[] segments = storedProcedureName.Split(" ".ToCharArray(), 2);


                string userParamValue = "";
                if (segments.Length == 2)
                {
                    storedProcedureName = segments.GetValue(0).ToString(); //first part is stored proc name
                    userParamValue = segments.GetValue(1).ToString(); //second part is param
                }

                SqlCommand cmd = new (storedProcedureName, conn);
                cmd.CommandType = CommandType.StoredProcedure; //allow running querries and sending parameters, etc.


                SqlParameter userDefinedParameter = new ("@UserDefinedParameter", SqlDbType.NVarChar, 8000);
                userDefinedParameter.IsNullable = false;
                userDefinedParameter.Direction = ParameterDirection.Input; //sending a value
                userDefinedParameter.Value = userParamValue;
                cmd.Parameters.Add(userDefinedParameter);

                //Hard-coded Parameters
                SqlParameter publishDateParam = new ("@PublishDate", SqlDbType.DateTime);
                publishDateParam.IsNullable = false;
                publishDateParam.Direction = ParameterDirection.Input; //sending a value
                publishDateParam.Value = publishDate;
                cmd.Parameters.Add(publishDateParam);

                SqlParameter messageParam = new ("@Message", SqlDbType.NVarChar, 8000);
                messageParam.IsNullable = true;
                messageParam.Direction = ParameterDirection.Output; //getting value from stored procedure for display
                cmd.Parameters.Add(messageParam);

                SqlParameter undoParam = new ("@Undo", SqlDbType.Bit);
                undoParam.IsNullable = true;
                undoParam.Direction = ParameterDirection.Output; //getting value from stored procedure for display
                cmd.Parameters.Add(undoParam);

                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();

                object rMessage = cmd.Parameters[messageParam.ParameterName].Value;
                if (rMessage != null)
                {
                    returnMessage = rMessage.ToString();
                }
                else
                {
                    returnMessage = "";
                }

                object rUndo = cmd.Parameters[undoParam.ParameterName].Value;
                if (rUndo == null || rUndo is DBNull)
                {
                    undo = false;
                }
                else
                {
                    undo = (bool)rUndo;
                }


                SqlConnection.ClearAllPools();
                conn.Close();
            }
        }
        catch (Exception e)
        {
            string errMsg = string.Format("Error while running stored procedure (in the Import database specified in Config Manager) '{0}':  {1}", storedProcedureName, e.Message);
            returnMessage = errMsg;
            throw new PTDatabaseException(errMsg, e);
        }
    }
    #endregion Public Methods

    private void da_RowUpdated(object sender, SqlRowUpdatedEventArgs e)
    {
        if (e.Status == UpdateStatus.Continue && e.StatementType == StatementType.Insert)
        {
            int new_id = (int)e.Command.Parameters[0].Value; //ID must be first column in table!
            e.Row[0] = new_id;
            e.Row.AcceptChanges();
        }
    }

    public void SetInitialProgressPoints(double a_currentProgressPoints, double a_totalPoints, double a_allowedProgressPoints)
    {
        m_currentProgressPoints = a_currentProgressPoints;
        m_totalProgressPoints = a_totalPoints;
        m_allowedProgressPoints = a_allowedProgressPoints;
    }
}