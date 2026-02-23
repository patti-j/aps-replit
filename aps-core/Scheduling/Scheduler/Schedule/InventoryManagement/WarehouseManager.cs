using System.Collections;
using System.ComponentModel;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.ERPTransmissions;
using PT.Scheduler.ErrorReporting;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of Warehouse objects.
/// </summary>
public partial class WarehouseManager : ScenarioBaseObjectManager<Warehouse>, IPTSerializable
{
    #region IPTSerializable Members
    internal WarehouseManager(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_idGen)
    {
        if (a_reader.VersionNumber >= 692)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Warehouse warehouse = new (a_reader, a_idGen);
                Add(warehouse);
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; ++i)
            {
                WarehouseTT tt = new (a_reader);
                m_whTTList.Add(tt);
            }
        }
    }

    internal void RestoreReferences(ScenarioDetail a_sd, CustomerManager a_cm, ItemManager itemManager)
    {
        foreach (Warehouse wh in this)
        {
            wh.RestoreReferences(a_sd, a_cm, itemManager, m_errorReporter);
        }
    }

    internal void RestoreTemplateReferences(JobManager aJobManager)
    {
        foreach (Warehouse wh in this)
        {
            wh.RestoreTemplateReferences(aJobManager);
        }
    }

    internal void RestoreAdjustmentReferences(ScenarioDetail a_sd)
    {
        foreach (Warehouse wh in this)
        {
            wh.RestoreAdjustmentReferences(a_sd);
        }
    }    
    
    internal void AfterRestoreAdjustmentReferences()
    {
        foreach (Warehouse wh in this)
        {
            wh.AfterRestoreAdjustmentReferences();
        }
    }

    /// <summary>
    /// This should be called whenever the Inventory or Job lists are updated in order to set references between Inventories and their MO Templates.
    /// For better performance, call this function first: jobs.InitFastGetByExternalId();
    /// </summary>
    /// <param name="a_jobManager"></param>
    internal void SetImportedTemplateMoReferences(JobManager a_jobManager, PTTransmission a_t, ScenarioExceptionInfo a_sei)
    {
        foreach (Warehouse wh in this)
        {
            wh.SetImportedTemplateMoReferences(a_jobManager, a_t, a_sei, m_errorReporter);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        
        writer.Write(m_whTTList.Count);
        for (int i = 0; i < m_whTTList.Count; ++i)
        {
            m_whTTList[i].Serialize(writer);
        }
    }

    public new const int UNIQUE_ID = 531;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    internal class WarehouseManagerException : PTException
    {
        internal WarehouseManagerException(string message) : base(message) { }
    }

    internal class ValidationException : PTValidationException
    {
        internal ValidationException() { }

        internal ValidationException(string a_message, object[] a_stringParameters = null, bool a_appendHelpUrl = false)
            : base(a_message, a_stringParameters, a_appendHelpUrl) { }

        internal ValidationException(string a_message, Exception a_innerException, bool a_appendHelpUrl = false, object[] a_stringParameters = null)
            : base(a_message, a_innerException, a_appendHelpUrl, a_stringParameters) { }
    }
    #endregion

    #region Construction
    public WarehouseManager(BaseIdGenerator idGen) : base(idGen) { }
    #endregion

    #region Warehouse Edit Functions
    private void Delete(Warehouse warehouse, JobManager jobs, PlantManager plants, PurchaseToStockManager purchases, Demand.SalesOrderManager salesOrderManager, Demand.TransferOrderManager a_transferOrderManager, IScenarioDataChanges a_dataChanges)
    {
        a_dataChanges?.AuditEntry(new AuditEntry(warehouse.Id, warehouse), false, true);
        warehouse.ValidateDelete(jobs, plants, purchases, salesOrderManager, a_transferOrderManager);
        warehouse.Deleting(m_scenarioDetail);
        Remove(warehouse.Id);
    }
    #endregion

    #region Internal Transmissions
    internal void Receive(ScenarioDetailClearT t, ScenarioDetail sd, IScenarioDataChanges a_dataChanges)
    {
        for (int i = 0; i < Count; i++)
        {
            Warehouse w = GetByIndex(i);
            w.Receive(t, sd, a_dataChanges);
        }

        if (t.ClearWarehouses)
        {
            for (int w = Count - 1; w >= 0; w--)
            {
                Delete(GetByIndex(w), sd.JobManager, sd.PlantManager, sd.PurchaseToStockManager, sd.SalesOrderManager, sd.TransferOrderManager, a_dataChanges);
            }
        }
    }

    internal void Receive(ScenarioDetail a_sd, InventoryEditT a_t, IScenarioDataChanges a_dataChanges)
    {
        foreach (InventoryEdit invEdit in a_t.InventoryEdits)
        {
            Warehouse warehouse = GetById(invEdit.WarehouseId);
            if (warehouse != null)
            {
                Inventory inv = warehouse.Inventories[invEdit.ItemId];

                if (inv != null)
                {
                    AuditEntry invAuditEntry = new AuditEntry(inv.Id, inv.Warehouse.Id, inv);
                    inv.Edit(a_t, invEdit, a_sd.JobManager, a_sd.SalesOrderManager, m_errorReporter);
                    a_dataChanges.InventoryChanges.UpdatedObject(invEdit.InventoryId);
                    a_dataChanges.AuditEntry(invAuditEntry);
                }
            }
        }

        foreach (ItemEdit itemEdit in a_t.ItemEdits)
        {
            Warehouse warehouse = GetById(itemEdit.WarehouseId);

            if (warehouse != null)
            {
                Inventory inv = warehouse.Inventories[itemEdit.ItemId];

                if (inv != null && inv.Item != null)
                {
                    AuditEntry itemAuditEntry = new AuditEntry(inv.Item.Id, inv.Warehouse.Id, inv.Item);
                    inv.Item.Edit(a_dataChanges, itemEdit, a_sd.JobManager);
                    a_dataChanges.ItemChanges.UpdatedObject(itemEdit.ItemId);
                    a_dataChanges.AuditEntry(itemAuditEntry);
                }
            }
        }

        foreach (WarehouseEdit warehouseEdit in a_t.WarehouseEdits)
        {
            Warehouse warehouse = GetById(warehouseEdit.WarehouseId);
            if (warehouse != null)
            {
                AuditEntry whAuditEntry = new AuditEntry(warehouse.Id, warehouse);
                warehouse.Edit(warehouseEdit);
                a_dataChanges.WarehouseChanges.UpdatedObject(warehouseEdit.WarehouseId);
                a_dataChanges.AuditEntry(whAuditEntry);
            }
        }
    }

    internal void Receive(ScenarioDetail a_sd, StorageAreaEditT a_t, IScenarioDataChanges a_dataChanges)
    {
        foreach (StorageAreaEdit storageAreaEdit in a_t.StorageAreaEdits)
        {
            Warehouse warehouse = GetById(storageAreaEdit.WarehouseId);
            if (warehouse != null)
            {
                StorageArea storageArea = warehouse.StorageAreas.GetValue(storageAreaEdit.StorageAreaId);

                if (storageArea != null)
                {
                    AuditEntry storageAreaAuditEntry = new AuditEntry(storageArea.Id, storageArea.Warehouse.Id, storageArea);
                    storageArea.Edit(a_sd, a_t, storageAreaEdit);
                    a_dataChanges.StorageAreaChanges.UpdatedObject(storageAreaEdit.Id);
                    a_dataChanges.AuditEntry(storageAreaAuditEntry);
                }
            }
        }

        foreach (WarehouseEdit warehouseEdit in a_t.WarehouseEdits)
        {
            Warehouse warehouse = GetById(warehouseEdit.WarehouseId);
            if (warehouse != null)
            {
                warehouse.Edit(warehouseEdit);
                a_dataChanges.WarehouseChanges.UpdatedObject(warehouseEdit.WarehouseId);
            }
        }
    }

    internal void IssueMaterial(MaterialIdBaseT a_t, MaterialRequirement a_mr, IScenarioDataChanges a_dataChanges)
    {
        if (a_t is ScenarioDetailMaterialUpdateT updateT)
        {
            IssueMaterial(a_mr, updateT.IssuedQty, a_dataChanges);
        }
    }

    internal void IssueMaterial(JobManager a_jobs, MaterialEdit a_materialEdit, IScenarioDataChanges a_dataChanges)
    {
        if (a_materialEdit.IssuedQtySet)
        {
            if (a_jobs.FindOperation(a_materialEdit.JobId, a_materialEdit.MOId, a_materialEdit.OperationId) is BaseOperation op)
            {
                if (op.MaterialRequirements.FindByBaseId(a_materialEdit.RequirementId) is MaterialRequirement mr)
                {
                    IssueMaterial(mr, a_materialEdit.IssuedQty, a_dataChanges);
                }
            }
        }
    }

    private void IssueMaterial(MaterialRequirement a_mr, decimal a_qty, IScenarioDataChanges a_dataChanges)
    {
        if (!CanIssueMaterial(a_mr, a_qty))
        {
            throw new PTValidationException("3034");
        }

        if (a_mr.Warehouse != null)
        {
            Inventory inventory = a_mr.Warehouse.Inventories[a_mr.Item.Id];
            inventory.SubtractOnHandQty(a_qty);
            a_dataChanges.InventoryChanges.UpdatedObject(inventory.Id);
        }
        else
        {
            decimal remainingQty = a_qty;
            foreach (Warehouse warehouse in this)
            {
                Inventory inventory = warehouse.Inventories[a_mr.Item.Id];
                if (inventory != null)
                {
                    decimal qtyToIssue = Math.Max(remainingQty, inventory.OnHandQty);
                    remainingQty -= qtyToIssue;
                    inventory.SubtractOnHandQty(qtyToIssue);
                    a_dataChanges.InventoryChanges.UpdatedObject(inventory.Id);
                    if (remainingQty <= 0)
                    {
                        break;
                    }
                }
            }
        }

        a_mr.IssuedQty += a_qty;
    }

    /// <summary>
    /// Determine whether there is enough on-hand material to issue to a material requirement
    /// </summary>
    /// <param name="a_mr">The material Requirement to issue quantity to</param>
    /// <param name="a_qty">THe quantity to issue</param>
    /// <returns>Whether there is enough on-hand material to issue to a material requirement</returns>
    public bool CanIssueMaterial(MaterialRequirement a_mr, decimal a_qty)
    {
        if (a_mr.Warehouse != null)
        {
            Inventory inventory = a_mr.Warehouse.Inventories[a_mr.Item.Id];
            return inventory.OnHandQty >= a_qty;
        }

        if (!a_mr.MultipleWarehouseSupplyAllowed)
        {
            return false;
        }

        decimal availableQty = 0;
        foreach (Warehouse warehouse in this)
        {
            Inventory inventory = warehouse.Inventories[a_mr.Item.Id];
            if (inventory != null)
            {
                availableQty += inventory.OnHandQty;
                if (availableQty >= a_qty)
                {
                    return true;
                }
            }
        }

        return false;
    }

    internal void Receive(Transmissions.Forecast.ForecastBaseT a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        if (a_t is Transmissions.Forecast.ForecastIntervalQtyChangeT)
        {
            UpdateForecastsInInterval((Transmissions.Forecast.ForecastIntervalQtyChangeT)a_t, a_sd);
        }
        else if (a_t is Transmissions.Forecast.ForecastShipmentDeleteT)
        {
            ProcessForecastDeleteT((Transmissions.Forecast.ForecastShipmentDeleteT)a_t, a_sd);
        }
        else if (a_t is Transmissions.Forecast.ForecastShipmentAdjustmentT)
        {
            ProcessForecastShipmentAdjustmentT((Transmissions.Forecast.ForecastShipmentAdjustmentT)a_t, a_sd);
        }
        else if (a_t is Transmissions.Forecast.ForecastShipmentGenerateT)
        {
            ProcessForecastShipmentGenerateT((Transmissions.Forecast.ForecastShipmentGenerateT)a_t, a_sd);
        }
        else if (a_t is Transmissions.Forecast.MultiForecastShipmentGenerateT)
        {
            ProcessMultiForecastShipmentGenerateT((Transmissions.Forecast.MultiForecastShipmentGenerateT)a_t, a_sd);
        }
    }

    /// <summary>
    /// Modify the Forecasts for the specified Item/Warehouse.
    /// </summary>
    private void UpdateForecastsInInterval(Transmissions.Forecast.ForecastIntervalQtyChangeT t, ScenarioDetail a_sd)
    {
        Warehouse warehouse = GetById(t.WarehouseId);
        if (warehouse == null)
        {
            throw new PTValidationException("2192");
        }

        if (warehouse.Inventories.Contains(t.ItemId))
        {
            Inventory inv = warehouse.Inventories[t.ItemId];
            inv.UpdateForecastsInInterval(t, IdGen, a_sd);
        }
        else
        {
            throw new PTValidationException("2193", new object[] { warehouse.Name });
        }
    }

    private void ProcessForecastDeleteT(Transmissions.Forecast.ForecastShipmentDeleteT a_t, ScenarioDetail a_sd)
    {
        ApplicationExceptionList errList = new ();

        try
        {
            foreach (KeyValuePair<InventoryKey, List<ForecastShipmentKey>> invShipments in a_t.ShipmentsToDelete)
            {
                Inventory inv = GetInventory(invShipments.Key);
                if (inv != null)
                {
                    inv.ForecastVersionsReceive(a_t, invShipments.Value, a_sd);
                }
            }
        }
        catch (PTHandleableException err)
        {
            errList.Add(err);
        }
        finally
        {
            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(a_sd);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
            }
        }
    }

    private void ProcessForecastShipmentAdjustmentT(Transmissions.Forecast.ForecastShipmentAdjustmentT a_t, ScenarioDetail a_sd)
    {
        ApplicationExceptionList errList = new ();

        try
        {
            foreach (KeyValuePair<InventoryKey, Dictionary<ForecastShipmentKey, decimal>> invShipments in a_t.m_shipmentAdjustments)
            {
                Inventory inv = GetInventory(invShipments.Key);
                if (inv != null)
                {
                    inv.ForecastVersionsReceive(a_t, invShipments.Value, a_sd);
                }
            }
        }
        catch (PTHandleableException err)
        {
            errList.Add(err);
        }
        finally
        {
            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(a_sd);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
            }
        }
    }

    private void ProcessForecastShipmentGenerateT(Transmissions.Forecast.ForecastShipmentGenerateT a_t, ScenarioDetail a_sd)
    {
        ApplicationExceptionList errList = new ();

        try
        {
            CreateForecastFromRequirement(a_t.Requirement, a_t.AutoGenerated, a_sd, a_t);
        }
        catch (PTHandleableException err)
        {
            errList.Add(err);
        }
        finally
        {
            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(a_sd);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
            }
        }
    }

    private void ProcessMultiForecastShipmentGenerateT(Transmissions.Forecast.MultiForecastShipmentGenerateT a_t, ScenarioDetail a_sd)
    {
        ApplicationExceptionList errList = new ();

        try
        {
            foreach (Transmissions.Forecast.ForecastRequirement req in a_t.Requirements)
            {
                CreateForecastFromRequirement(req, a_t.AutoGenerated, a_sd, a_t);
            }
        }
        catch (PTHandleableException err)
        {
            errList.Add(err);
        }
        finally
        {
            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(a_sd);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
            }
        }
    }

    private void CreateForecastFromRequirement(Transmissions.Forecast.ForecastRequirement a_requirements, bool a_autoGenerated, ScenarioDetail a_sd, PTTransmission a_t)
    {
        foreach (InventoryKey invKey in a_requirements.InventoryKeys)
        {
            Inventory inv = GetInventory(invKey);
            if (inv != null)
            {
                inv.CreateForecasts(a_requirements.ReqDateAndQties, a_autoGenerated, a_sd, IdGen, a_t);
            }
        }
    }

    private Inventory GetInventory(InventoryKey a_invKey)
    {
        Warehouse w = GetById(new BaseId(a_invKey.WarehouseId));
        if (w != null)
        {
            return w.Inventories[new BaseId(a_invKey.ItemId)];
        }

        return null;
    }

    #endregion

    #region ATP
    /// <summary>
    /// Returns the total of all ATP quantities for the Item across all Warehouses as of the date specified.
    /// </summary>
    public decimal GetAtpForItem(Item item, DateTime asOf, DateTime a_clockDate)
    {
        decimal totalAtp = 0;
        for (int w = 0; w < Count; w++)
        {
            Warehouse warehouse = GetByIndex(w);
            Inventory inv = warehouse.Inventories[item.Id];
            if (inv != null)
            {
                totalAtp += inv.GetAtpQty(asOf, a_clockDate);
            }
        }

        return totalAtp;
    }

    /// <summary>
    /// Find the shortest lead time by searching all Warehouses.
    /// If no Warehouse stocks the Item then foundASupplyingWarehouse will be false and aInventory will be null.
    /// </summary>
    public void GetShortestLeadTime(Item aItem, out bool foundASupplyingWarehouse, out Inventory aInventory)
    {
        Inventory bestInventory = null;

        for (int w = 0; w < Count; w++)
        {
            Warehouse warehouse = GetByIndex(w);
            Inventory inv = warehouse.Inventories[aItem.Id];

            if (inv != null)
            {
                if (bestInventory == null)
                {
                    bestInventory = inv;
                }
                else
                {
                    if (inv.LeadTimeTicks < bestInventory.LeadTimeTicks)
                    {
                        bestInventory = inv;
                    }
                }
            }
        }

        foundASupplyingWarehouse = bestInventory != null;
        aInventory = bestInventory;
    }

    /// <summary>
    /// Returns the total of all final quantities for the Item across all Warehouses.
    /// </summary>
    public decimal GetFinalOnHandQtyForItem(Item item)
    {
        decimal finalQty = 0;
        for (int w = 0; w < Count; w++)
        {
            Warehouse warehouse = GetByIndex(w);
            Inventory inv = warehouse.Inventories[item.Id];
            if (inv != null)
            {
                finalQty += inv.GetFinalOnHandQty();
            }
        }

        return finalQty;
    }
    #endregion

    #region ERP Transmissions
    internal void Receive(WarehouseT a_t, UserFieldDefinitionManager a_udfManager, PlantManager plants, ItemManager items, JobManager jobs, PurchaseToStockManager purchases, Demand.SalesOrderManager a_salesOrderManager, Demand.TransferOrderManager a_transferOrderManager, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges, ScenarioExceptionInfo a_sei)
    {
        if (a_t == null)
        {
            return;
        }

        ApplicationExceptionList errList = new ();
        List<PostProcessingAction> actions = new ();

        try
        {
            //TODO: Return to Parallel.Invoke
            //Temp solution until V12 datachages is in place to reduce number of threads
            Thread t1 = new (() => { InitFastLookupByExternalId(); });
            t1.Start();

            Thread t2 = new (() => { a_sd.ItemManager.InitFastLookupByExternalId(); });
            t2.Start();

            Thread t3 = new (() => { a_sd.PlantManager.InitFastLookupByExternalId(); });
            t3.Start();

            Thread t4 = new (() => { a_sd.JobManager.InitFastLookupByExternalId(); });
            t4.Start();

            Thread t5 = new (() => { a_sd.PurchaseToStockManager.InitFastLookupByExternalId(); });
            t5.Start();

            t1.Join();
            t2.Join();
            t3.Join();
            t4.Join();
            t5.Join();

            HashSet<BaseId> affectedWarehouses = new ();

            for (int i = 0; i < a_t.Count; ++i)
            {
                try
                {
                    WarehouseT.Warehouse warehouseNode = a_t[i];

                    Warehouse warehouse = GetByExternalId(warehouseNode.ExternalId);

                    if (warehouse == null)
                    {
                        warehouse = new Warehouse(NextID(), warehouseNode, IdGen);
                        warehouse.Update(warehouseNode, false, false, false, false,  false, false, false, false,false, false,false, a_udfManager, a_t, a_sd, m_errorReporter, a_dataChanges, true, actions, a_sei);
                        Add(warehouse);

                        a_dataChanges.WarehouseChanges.AddedObject(warehouse.Id);
                        a_dataChanges.AuditEntry(new AuditEntry(warehouse.Id, warehouse), true);
                    }
                    else
                    {
                        AuditEntry warehouseAuditEntry = new AuditEntry(warehouse.Id, warehouse);
                        warehouse.Update(warehouseNode
                            , a_t.AutoDeleteInventories
                            , a_t.AutoDeleteStorageAreas
                            , a_t.AutoDeleteItemStorage
                            , a_t.AutoDeleteLots
                            , a_t.AutoDeleteItemStorageLots
                            , a_t.AutoDeleteMode //Plant Associations
                            , a_t.AutoDeleteStorageAreaConnectors
                            , a_t.AutoDeleteStorageAreaConnectorsIn
                            , a_t.AutoDeleteStorageAreaConnectorsOut
                            , a_t.AutoDeleteResourceStorageAreaConnectorIn
                            , a_t.AutoDeleteResourceStorageAreaConnectorOut
                            , a_udfManager, a_t, m_scenarioDetail, m_errorReporter, a_dataChanges, false, actions, a_sei);
                        a_dataChanges.AuditEntry(warehouseAuditEntry);
                    }

                    if (!affectedWarehouses.Contains(warehouse.Id))
                    {
                        affectedWarehouses.Add(warehouse.Id);
                    }
                }
                catch (PTHandleableException err)
                {
                    errList.Add(err);
                }
            }

            if (a_t.AutoDeleteMode)
            {
                List<Warehouse> warehousesToDelete = new ();
                for (int i = Count - 1; i >= 0; i--)
                {
                    Warehouse warehouse = GetByIndex(i);
                    if (!affectedWarehouses.Contains(warehouse.Id))
                    {
                        warehousesToDelete.Add(warehouse);
                    }
                }

                if (warehousesToDelete.Count > 0)
                {
                    actions.Add(new PostProcessingAction(a_t, false, () =>
                        {
                            ApplicationExceptionList delErrList = new ();

                            for (int i = 0; i < warehousesToDelete.Count; i++)
                            {
                                try
                                {
                                    Warehouse deletedWh = warehousesToDelete[i];
                                    Delete(deletedWh, jobs, plants, purchases, a_salesOrderManager, a_transferOrderManager, a_dataChanges);
                                    a_dataChanges.WarehouseChanges.DeletedObject(deletedWh.Id);
                                }
                                catch (PTHandleableException err)
                                {
                                    delErrList.Add(err);
                                }
                            }

                            if (delErrList.Count > 0)
                            {
                                ScenarioExceptionInfo sei = new ();
                                sei.Create(a_sd);
                                m_errorReporter.LogException(delErrList, a_t, sei, ELogClassification.PtInterface, false);
                            }
                        }));
                }
            }
        }
        catch (PTHandleableException err)
        {
            errList.Add(err);
        }
        finally
        {
            actions.Add(new PostProcessingAction(a_t, true, () =>
                {
                    DeInitFastLookupByExternalId();
                    items.DeInitFastLookupByExternalId();
                    plants.DeInitFastLookupByExternalId();
                    jobs.DeInitFastLookupByExternalId();
                    purchases.DeInitFastLookupByExternalId();
                }));
            m_scenarioDetail.AddProcessingAction(actions);

            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(a_sd);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
            }
        }

        List<Tuple<string, string, long>>.Enumerator wttEtr = a_t.GetWarehouseTTEnumerator();
        while (wttEtr.MoveNext())
        {
            WarehouseTT tt = new (wttEtr.Current.Item1, wttEtr.Current.Item2, wttEtr.Current.Item3);
            m_whTTList.Add(tt);
        }
    }

    private readonly List<WarehouseTT> m_whTTList = new ();

    internal long GetTT(Warehouse a_fromWH, Warehouse a_toWH)
    {
        return m_whTTs[a_fromWH.TTIdx, a_toWH.TTIdx];
    }

    // Call this prior to processing the Items, check for the definition of the item ItemManager, if it's not there it might be a new 
    // Item so then check the WarehouseT.Items to see if it's a new Item.
    // Then receive WarehouseT.Items and finally receive WarehouseT.Warehouses
    internal void Validate(WarehouseT t, ItemManager items, PlantManager plants)
    {
        //Make sure the Item Ids are valid
        for (int w = 0; w < t.Count; w++)
        {
            WarehouseT.Warehouse warehouse = t[w];

            for (int storeAreaIdx = 0; storeAreaIdx < warehouse.StorageAreaCount; storeAreaIdx++)
            {
                WarehouseT.StorageArea storageArea = warehouse.GetStorageAreaByIndex(storeAreaIdx);
                for (int invIdx = 0; invIdx < storageArea.ItemStorageCount; invIdx++)
                {
                    WarehouseT.ItemStorage itemStorage = storageArea.GetItemStorageByIndex(invIdx);

                    Item item = items.GetByExternalId(itemStorage.ItemExternalId);
                    if (item == null)
                    {
                        //check to see if the item is in the transmission
                        if (!t.ItemsExternalIdSet.Contains(itemStorage.ItemExternalId))
                        {
                            //The item is not in the transmission either
                            throw new PTValidationException("2194", new object[] { itemStorage.ItemExternalId, warehouse.ExternalId });
                        }
                    }
                }
            }


            //Make sure the Plant ids are valid.
            for (int p = 0; p < warehouse.SuppliedPlantsCount; p++)
            {
                string plantExternalId = warehouse.GetSuppliedPlantExternalIdFromIndex(p);

                Plant plant = plants.GetByExternalId(plantExternalId);
                if (plant == null)
                {
                    throw new PTValidationException("2195", new object[] { plantExternalId, warehouse.ExternalId });
                }
            }
        }
    }

    internal void ProcessForecastT(UserFieldDefinitionManager a_udfManager, ForecastT a_t, ItemManager aItemManager, ScenarioDetail a_sd)
    {
        ApplicationExceptionList errList = new ();
        List<PostProcessingAction> actions = new ();

        try
        {
            Inventory[] invs;
            Validate(a_sd, a_t, aItemManager, out invs);
            HashSet<Inventory> updatedInvs = new ();

            for (int fvsI = 0; fvsI < a_t.InventoryForecasts.Count; ++fvsI)
            {
                try
                {
                    ForecastT.ForecastVersions fvs = a_t.InventoryForecasts[fvsI];
                    Inventory inv = invs[fvsI];
                    updatedInvs.Add(inv);
                    inv.ForecastVersionsReceive(a_udfManager, a_t, fvs, IdGen, a_sd);
                }
                catch (PTHandleableException err)
                {
                    errList.Add(err);
                }
            }

            if (a_t.AutoDeleteMode)
            {
                actions.Add(new PostProcessingAction(a_t, false, () =>
                    {
                        for (int whI = 0; whI < Count; ++whI)
                        {
                            Warehouse wh = GetByIndex(whI);
                            IEnumerator<Inventory> invEtr = wh.Inventories.GetEnumerator();

                            while (invEtr.MoveNext())
                            {
                                try
                                {
                                    Inventory inv = invEtr.Current;
                                    if (!updatedInvs.Contains(inv))
                                    {
                                        inv.ForecastVersionsClear(a_sd, a_t);
                                    }
                                }
                                catch (PTHandleableException err)
                                {
                                    errList.Add(err);
                                }
                            }
                        }
                    }));
            }
        }
        catch (PTHandleableException err)
        {
            errList.Add(err);
        }
        finally
        {
            a_sd.AddProcessingAction(actions);
            if (errList.Count > 0)
            {
                ScenarioExceptionInfo sei = new ();
                sei.Create(a_sd);
                m_errorReporter.LogException(errList, a_t, sei, ELogClassification.PtInterface, false);
            }
        }
    }

    internal void RegenerateForecasts(ScenarioDetail a_sd, PTTransmission a_t)
    {
        try
        {
            if (!PTSystem.Server)
            {
                return;
            }

            ForecastT forecastT = new ();
            forecastT.Instigator = a_t.Instigator;

            foreach (Warehouse w in this)
            {
                Parallel.For(0,
                    w.Inventories.Count,
                    new Action<int>(i =>
                    {
                        Inventory inv = w.Inventories.GetByIndex(i);
                        if (inv.AutoGenerateForecasts && inv.SalesOrdersChangedSinceLastForecast)
                        {
                            ForecastT.ForecastVersions versions = inv.GenerateForecastVersionsT(a_sd);
                            if (versions != null)
                            {
                                forecastT.InventoryForecasts.Add(versions);
                            }
                        }
                    }));
            }

            if (forecastT.Count > 0)
            {
                //TODO: Remove static reference
                SystemController.ClientSession.SendClientAction(forecastT);
            }
        }
        catch (Exception err)
        {
            ScenarioExceptionInfo sei = new ();
            sei.Create(a_sd);
            m_errorReporter.LogException(err, a_t, sei, ELogClassification.PtInterface, false, "Error automatically regenerating forecasts.".Localize());
        }
    }

    void Validate(ScenarioDetail a_sd, ForecastT a_t, ItemManager a_itemManager, out Inventory[] a_inventories)
    {
        a_inventories = new Inventory[a_t.InventoryForecasts.Count];

        for (int fvsI = 0; fvsI < a_t.InventoryForecasts.Count; ++fvsI)
        {
            ForecastT.ForecastVersions fvs = a_t.InventoryForecasts[fvsI];

            foreach (ForecastT.ForecastVersion forecastVersion in fvs.Versions)
            {
                foreach (ForecastT.Forecast forecast in forecastVersion.Forecasts.Values)
                {
                    //Validate that the customer exists
                    if (forecast.CustomerSet)
                    {
                        a_sd.CustomerManager.InitFastLookupByExternalId();

                        if (a_sd.CustomerManager.GetByExternalId(forecast.Customer) == null)
                        {
                            throw new PTValidationException(GetForecastVersionErrorMessage(fvs) + string.Format("Customer '{0}' doesn't exist.".Localize(), forecast.Customer));
                        }

                        a_sd.CustomerManager.DeInitFastLookupByExternalId();
                    }
                }
            }

            Warehouse wh = GetByExternalId(fvs.WarehouseExternalId);

            if (wh == null)
            {
                throw new PT.APSCommon.PTValidationException(GetForecastVersionErrorMessage(fvs) + "The Warehouse doesn't exist.".Localize());
            }

            BaseObject bo = a_itemManager.GetByExternalId(fvs.ItemExternalId);

            if (bo == null)
            {
                throw new PT.APSCommon.PTValidationException(GetForecastVersionErrorMessage(fvs) + "The Item doesn't exist.".Localize());
            }

            Inventory inv = wh.Inventories[bo.Id];

            if (inv == null)
            {
                throw new PT.APSCommon.PTValidationException(GetForecastVersionErrorMessage(fvs) + "The Inventory doesn't exist.".Localize());
            }

            a_inventories[fvsI] = inv;
        }
    }

    private string GetForecastVersionErrorMessage(ForecastT.ForecastVersions fvs)
    {
        return "Forecast validation error for WarehouseExternalId=".Localize() + fvs.WarehouseExternalId + ", ItemExternalId=".Localize() + fvs.ItemExternalId + ". ";
    }
    #endregion

    #region ICopyTable
    public override Type ElementType => typeof(Warehouse);
    #endregion

    #region Deleting
    internal void ValidateAndDeleteItems(ScenarioDetail a_sd, ItemDeleteProfile a_items, JobManager jobs, PurchaseToStockManager purchases, Demand.TransferOrderManager a_transferOrderManager, IScenarioDataChanges a_dataChanges)
    {
        //Need to remove inventories for the Item from the Warehouses
        for (int i = 0; i < Count; i++)
        {
            this[i].ValidateAndDeleteItems(a_sd, a_items, jobs, purchases, a_transferOrderManager, a_dataChanges);
        }
    }

    internal void DeletingResources(Resource a_resource)
    {
        foreach (Warehouse warehouse in this)
        {
            foreach (StorageArea sa in warehouse.StorageAreas)
            {
                sa.UnlinkResource(a_resource.Id);
            }
        }

        for (int i = 0; i < Count; i++)
        {
            this[i].StorageAreaConnectors.Delete(a_resource);
        }
    }
    #endregion

    #region Import Database
    /// <summary>
    /// Fills dataset.
    /// </summary>
    /// <param name="ds"></param>
    /// <param name="itemsTable">MUST BE POPULATED ALREADY.</param>
    public void PopulateImportDataSet(PtImportDataSet ds, PtImportDataSet.ItemsDataTable itemsTable, ScenarioDetail sd)
    {
        //Create a hashtable of plant rows to be used in the populating of the individual warehouses.
        Hashtable plantRowsHash = new ();
        for (int plantI = 0; plantI < ds.Plants.Count; plantI++)
        {
            PtImportDataSet.PlantsRow plantRow = ds.Plants[plantI];
            plantRowsHash.Add(plantRow.ExternalId, plantRow);
        }

        for (int i = 0; i < Count; i++)
        {
            GetByIndex(i).PopulateImportDataSet(ds, itemsTable, sd, plantRowsHash);
        }
    }
    #endregion Import Database

    #region MRP
    /// <summary>
    /// A Dictionary of Job Templates by Item and Warehouse.
    /// </summary>
    internal class TemplatesDictionary
    {
        private readonly Dictionary<Item, ItemInventories> m_templates = new ();

        internal void Add(Inventory aInv, WarehouseManager aWarehouseManager)
        {
            ItemInventories itemInventories;
            if (m_templates.ContainsKey(aInv.Item))
            {
                itemInventories = m_templates[aInv.Item];
            }
            else
            {
                itemInventories = new ItemInventories(aInv.Item);
                m_templates.Add(aInv.Item, itemInventories);
            }

            BOM newBom = itemInventories.Add(aInv);

            //Keep a list of all Inventories that are in BOMs or could be used by MRs if their warehouse is null.
            for (int mI = 0; mI < newBom.MaterialsCount; mI++)
            {
                MaterialRequirement mr = newBom.GetMaterialAtIndex(mI);
                List<Inventory> inventoriesThatMaySatisfyMR = aWarehouseManager.GetInventoriesThatMaySupplyMR(mr);
                for (int invI = 0; invI < inventoriesThatMaySatisfyMR.Count; invI++)
                {
                    Inventory mrInv = inventoriesThatMaySatisfyMR[invI];
                    if (!m_InventoriesUsedAsMaterials.ContainsKey(mrInv))
                    {
                        m_InventoriesUsedAsMaterials.Add(mrInv, mrInv);
                    }
                }
            }
        }

        internal bool Contains(Item aItem)
        {
            return m_templates.ContainsKey(aItem);
        }

        internal ItemInventories this[Item aItem] => m_templates[aItem];

        internal int Count => m_templates.Count;

        private readonly Dictionary<Inventory, Inventory> m_InventoriesUsedAsMaterials = new ();

        /// <summary>
        /// Checks whether the Inventory is refered to in an MaterialRequirement for a Template (either directly or indirectly by an MR that can come from any warehouse.
        /// </summary>
        internal bool InventoryIsUsedAsMaterial(Inventory aInv)
        {
            return m_InventoriesUsedAsMaterials.ContainsKey(aInv);
        }

        /// <summary>
        /// Get a list of BOMs for Items that are not used as materials in any other BOMs in the Template list.
        /// </summary>
        /// <returns></returns>
        internal List<BOM> GetFinishedGoodBOMS()
        {
            List<BOM> m_fgBoms = new ();

            Dictionary<Item, ItemInventories>.Enumerator templatesEnumerator = m_templates.GetEnumerator();
            while (templatesEnumerator.MoveNext())
            {
                ItemInventories itemInv = templatesEnumerator.Current.Value;
                Dictionary<Inventory, BOM>.Enumerator itemInvEnuerator = itemInv.GetEnumerator();
                while (itemInvEnuerator.MoveNext())
                {
                    BOM bom = itemInvEnuerator.Current.Value;
                    if (!InventoryIsUsedAsMaterial(bom.Inventory))
                    {
                        m_fgBoms.Add(bom);
                    }
                }
            }

            return m_fgBoms;
        }
    }

    /// <summary>
    /// Returns a list of Inventories that may supply the MR based on whether its Warehouse is set.
    /// </summary>
    internal List<Inventory> GetInventoriesThatMaySupplyMR(MaterialRequirement aMR)
    {
        List<Inventory> inventoriesList = new ();

        if (aMR.Warehouse != null)
        {
            if (aMR.Warehouse.Inventories.Contains(aMR.Item.Id))
            {
                Inventory mrInv = aMR.Warehouse.Inventories[aMR.Item.Id];
                inventoriesList.Add(mrInv);
            }
        }
        else //no warehouse is specified so add all Inventories for this Item
        {
            for (int wI = 0; wI < Count; wI++)
            {
                Warehouse warehouse = GetByIndex(wI);
                if (warehouse.Inventories.Contains(aMR.Item.Id))
                {
                    Inventory mrInv = warehouse.Inventories[aMR.Item.Id];
                    inventoriesList.Add(mrInv);
                }
            }
        }

        return inventoriesList;
    }

    /// <summary>
    /// A dictionary of Inventories for one Item.
    /// </summary>
    internal class ItemInventories
    {
        internal ItemInventories(Item aItem)
        {
            m_item = aItem;
        }

        private readonly Item m_item;

        internal Item Item => m_item;

        private readonly Dictionary<Inventory, BOM> m_inventories = new ();

        internal bool Contains(Inventory aInventory)
        {
            return m_inventories.ContainsKey(aInventory);
        }

        internal BOM GetBomForInventory(Inventory aInventory)
        {
            return m_inventories[aInventory];
        }

        internal BOM Add(Inventory aInventory)
        {
            BOM newBom = new (aInventory);
            m_inventories.Add(aInventory, newBom);
            return newBom;
        }

        internal Dictionary<Inventory, BOM>.Enumerator GetEnumerator()
        {
            return m_inventories.GetEnumerator();
        }
    }

    /// <summary>
    /// A Job Template and a list of its Materials in all MOs and all Operations.
    /// </summary>
    internal class BOM
    {
        internal BOM(Inventory aInv)
        {
            m_Job = aInv.TemplateManufacturingOrder.Job;
            m_inv = aInv;

            for (int moI = 0; moI < Job.ManufacturingOrders.Count; moI++)
            {
                ManufacturingOrder mo = Job.ManufacturingOrders[moI];
                for (int opI = 0; opI < mo.OperationManager.Count; opI++)
                {
                    BaseOperation op = mo.OperationManager.GetByIndex(opI);
                    for (int mrI = 0; mrI < op.MaterialRequirements.Count; mrI++)
                    {
                        MaterialRequirement mr = op.MaterialRequirements[mrI];
                        if (mr.Item != null) //only care about Stock MRs.
                        {
                            m_materials.Add(mr);
                        }
                    }
                }
            }
        }

        private readonly Job m_Job;

        internal Job Job => m_Job;

        private readonly Inventory m_inv;

        internal Item ParentItem => m_inv.Item;

        internal Inventory Inventory => m_inv;

        private readonly List<MaterialRequirement> m_materials = new ();

        internal int MaterialsCount => m_materials.Count;

        internal MaterialRequirement GetMaterialAtIndex(int index)
        {
            return m_materials[index];
        }
    }

    /// <summary>
    /// Returns a dictionary of all Templates organized by Item and Inventory.
    /// </summary>
    internal TemplatesDictionary GetTemplatesByProduct()
    {
        TemplatesDictionary productTemplates = new ();

        for (int wI = 0; wI < Count; wI++)
        {
            Warehouse warehouse = GetByIndex(wI);
            IEnumerator<Inventory> invEnumerator = warehouse.Inventories.GetEnumerator();
            while (invEnumerator.MoveNext())
            {
                Inventory inv = invEnumerator.Current;
                if (inv.TemplateManufacturingOrder != null)
                {
                    productTemplates.Add(inv, this);
                }
            }
        }

        return productTemplates;
    }

    internal void SetMrpNetChangeFlagForAllInventoriesToFalse()
    {
        for (int wI = 0; wI < Count; wI++)
        {
            Warehouse warehouse = GetByIndex(wI);
            IEnumerator<Inventory> invEnumerator = warehouse.Inventories.GetEnumerator();
            while (invEnumerator.MoveNext())
            {
                Inventory inv = invEnumerator.Current;
                inv.IncludeInNetChangeMRP = false;
            }
        }
    }
    #endregion MRP

    #region Restore References
    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        foreach (Warehouse warehouse in this)
        {
            warehouse.RestoreReferences(a_udfManager);
        }
    }
    #endregion

    internal void ConsumeForecasts(ScenarioDetail a_sd, bool a_consumeForecasts)
    {
        Parallel.ForEach(this, w => { w.ConsumeForecasts(a_sd, a_consumeForecasts); });
    }
}
