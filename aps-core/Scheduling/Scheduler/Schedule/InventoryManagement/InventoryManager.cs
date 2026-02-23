using System.Collections;
using System.ComponentModel;

using PT.APSCommon;
using PT.APSCommon.Collections;
using PT.Common.File;
using PT.ERPTransmissions;
using PT.Scheduler.Demand;
using PT.Scheduler.ErrorReporting;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Keeps track of all the Inventories for a specific Warehouse.
/// </summary>
public partial class InventoryManager : IEnumerable<Inventory>, IPTSerializable, AfterRestoreReferences.IAfterRestoreReferences
{
    public const int UNIQUE_ID = 533;

    #region IPTSerializable Members
    public InventoryManager(IReader a_reader, BaseIdGenerator a_idGen)
    {
        m_idGen = a_idGen;
        if (a_reader.VersionNumber >= 404)
        {
            m_inventories = new InventoryList(a_reader, a_idGen);
        }
    }

    private ISystemLogger m_errorReporter;

    internal void RestoreReferences(ScenarioDetail a_sd, CustomerManager a_cm, ItemManager a_itemManager, Warehouse a_wh, ISystemLogger a_errorReporter)
    {
        m_errorReporter = a_errorReporter;

        Dictionary<BaseId, ItemStorage> itemStorageCollection = new (); //Used to cache ItemStorage lookups across lots
        foreach (Inventory inventory in m_inventories)
        {
            inventory.RestoreReferences(a_cm, a_itemManager, a_wh, a_errorReporter, ref itemStorageCollection);
        }
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        foreach (Inventory inventory in this)
        {
            inventory.RestoreReferences(a_udfManager);
        }
    }
    
    internal void RestoreTemplateReferences(JobManager a_aJobManager)
    {
        foreach (Inventory inventory in this)
        {
            inventory.RestoreTemplateMoReference(a_aJobManager);
        }
    }

    internal void RestoreAdjustmentReferences(ScenarioDetail a_sd)
    {
        foreach (Inventory inventory in this)
        {
            inventory.RestoreAdjustmentReferences(a_sd);
        }
    }    
    
    internal void AfterRestoreAdjustmentReferences()
    {
        foreach (Inventory inventory in this)
        {
            inventory.AfterRestoreAdjustmentReferences();
        }
    }

    internal void SetImportedTemplateMoReferences(JobManager a_jobManager, PTTransmission a_t, ScenarioExceptionInfo a_sei, ISystemLogger a_errorReporter)
    {
        bool jobManagerFastLookupInited = false;
        try
        {
            m_errorReporter = a_errorReporter;
            ApplicationExceptionList errList = new();

            foreach (Inventory inventory in this)
            {
                try
                {
                    if (inventory.TemplateMoImportRef != null && !string.IsNullOrEmpty(inventory.TemplateMoImportRef.m_templateJobExternalId)) //Have a Template MO so set the reference.
                    {
                        if (!jobManagerFastLookupInited) // at least one inventory has a template. most likely there are others. Init fast lookup.
                        {
                            a_jobManager.InitFastLookupByExternalId();
                            jobManagerFastLookupInited = true;
                        }

                        string templateJobExternalId = inventory.TemplateMoImportRef.m_templateJobExternalId;
                        Job job = a_jobManager.GetByExternalId(templateJobExternalId);
                        if (job != null) //may not be here yet
                        {
                            foreach (ManufacturingOrder mo in job.ManufacturingOrders) // get the first mo that makes this inventory
                            {
                                Product p = mo.GetPrimaryProduct();
                                if (p != null && p.Inventory.Id == inventory.Id)
                                {
                                    inventory.SetTemplateMoReference(mo);
                                    break;
                                }
                            }

                            if (inventory.TemplateManufacturingOrder == null)
                            {
                                inventory.ClearImportTemplateMoReferenceDueToError();
                                throw new PTValidationException("2900", new object[] { inventory.Item.Name, inventory.Warehouse.Name, templateJobExternalId });
                            }
                        }
                        else
                        {
                            inventory.ClearImportTemplateMoReferenceDueToError();
                            throw new PTValidationException("2901", new object[] { inventory.Item.Name, inventory.Warehouse.Name, templateJobExternalId });
                        }
                    }
                }
                catch (Exception err)
                {
                    errList.Add(err);
                }
            }

            if (errList.Count > 0)
            {
                m_errorReporter.LogException(errList, a_t, a_sei, ELogClassification.PtInterface, false);
            }
        }
        finally
        {
            if (jobManagerFastLookupInited)
            {
                a_jobManager.DeInitFastLookupByExternalId();
            }
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_inventories.Serialize(a_writer);
    }

    [Browsable(false)]
    public int UniqueId => UNIQUE_ID;
    #endregion

    private readonly BaseIdGenerator m_idGen;

    #region IAfterRestoreReferences
    public void AfterRestoreReferences_1(int a_serializationVersionNbr, HashSet<object> a_processedAfterRestoreReferences1, HashSet<object> a_processedAfterRestoreReferences2)
    {
        if (!a_processedAfterRestoreReferences1.Contains(this))
        {
            a_processedAfterRestoreReferences1.Add(this);
            ReinsertObjects(true);
            OnContentsCallAfterRestoreReferences_1(a_serializationVersionNbr, a_processedAfterRestoreReferences1, a_processedAfterRestoreReferences2);
            AfterRestoreReferences.Helpers.CallObjMembers_AfterRestoreReferences_1(a_serializationVersionNbr, this, a_processedAfterRestoreReferences1, a_processedAfterRestoreReferences2);
        }
    }

    public void AfterRestoreReferences_2(int a_serializationVersionNbr, HashSet<object> a_processedAfterRestoreReferences1, HashSet<object> a_processedAfterRestoreReferences2)
    {
        if (!a_processedAfterRestoreReferences2.Contains(this))
        {
            a_processedAfterRestoreReferences2.Add(this);
            ReinsertObjects(false);
            OnContentsCallAfterRestoreReferences_2(a_serializationVersionNbr, a_processedAfterRestoreReferences1, a_processedAfterRestoreReferences2);
            AfterRestoreReferences.Helpers.CallObjMembers_AfterRestoreReferences_2(a_serializationVersionNbr, this, a_processedAfterRestoreReferences1, a_processedAfterRestoreReferences2);
        }
    }

    internal void ReinsertObjects(bool a_resetIds)
    {
        if (a_resetIds)
        {
            for (int i = 0; i < m_inventories.Count; ++i)
            {
                Inventory itemInv = m_inventories.GetByIndex(i);
                BaseIdObject o = itemInv;

                if (a_resetIds)
                {
                    o.Id = m_idGen.NextID();
                }
            }
        }
    }

    protected void OnContentsCallAfterRestoreReferences_1(int a_serializationVersionNbr, HashSet<object> a_processedAfterRestoreReferences1, HashSet<object> a_processedAfterRestoreReferences2)
    {
        foreach (Inventory itemInv in m_inventories.ReadOnlyList)
        {
            Inventory inv = itemInv;
            inv.AfterRestoreReferences_1(a_serializationVersionNbr, a_processedAfterRestoreReferences1, a_processedAfterRestoreReferences2);
        }
    }

    protected void OnContentsCallAfterRestoreReferences_2(int a_serializationVersionNbr, HashSet<object> a_processedAfterRestoreReferences1, HashSet<object> a_processedAfterRestoreReferences2)
    {
        foreach (Inventory itemInv in m_inventories.ReadOnlyList)
        {
            Inventory inv = itemInv;
            inv.AfterRestoreReferences_2(a_serializationVersionNbr, a_processedAfterRestoreReferences1, a_processedAfterRestoreReferences2);
        }
    }
    #endregion

    public InventoryManager(BaseIdGenerator a_idGen)
    {
        m_idGen = a_idGen;
        m_inventories = new InventoryList();
    }

    #region Transmission Handling
    /// <summary>
    /// Add new Inventories, update Inventories that already exist and delete those omitted.
    /// </summary>
    internal void UpdateInventories(WarehouseT.Warehouse a_warehouseT, UserFieldDefinitionManager a_udfManager, Warehouse a_warehouse, bool a_inventoryAutoDelete, Warehouse a_parentWarehouse, ApplicationExceptionList a_errList, PTTransmission a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges, List<PostProcessingAction> a_inventoryAutoDeleteActions)
    {
        HashSet<BaseId> affectedInventories = new();
        for (int i = 0; i < a_warehouseT.InventoryCount; i++)
        {
            try
            {
                WarehouseT.Inventory inventoryNode = a_warehouseT.GetInventoryByIndex(i);
                Item item = a_sd.ItemManager.GetByExternalId(inventoryNode.ItemExternalId);
                if (item == null)
                {
                    a_errList.Add(new PTValidationException("2194", new object[] { inventoryNode.ItemExternalId, a_warehouse.ExternalId }));
                    continue;
                }

                if (Contains(item.Id))
                {
                    //Update Inventory
                    Inventory inv = this[item.Id];
                    if (!affectedInventories.Contains(inv.Item.Id))
                    {
                        affectedInventories.Add(inv.Item.Id);
                        AuditEntry invAuditEntry = new AuditEntry(inv.Id, inv.Warehouse.Id, inv);
                        inv.Update(inventoryNode, a_sd, a_udfManager);
                        a_dataChanges.InventoryChanges.UpdatedObject(inv.Id);
                        a_dataChanges.AuditEntry(invAuditEntry);
                    }
                    else
                    {
                        throw new PTValidationException("2880", new object[] { a_warehouse.Name, inv.Item.Name });
                    }
                }
                else
                {
                    //Add new Inventory					
                    Inventory inv = new Inventory(m_idGen.NextID(), a_warehouse, a_sd, item, inventoryNode, m_idGen, a_udfManager);
                    m_inventories.Add(inv);
                    if (!affectedInventories.Contains(inv.Item.Id))
                    {
                        affectedInventories.Add(inv.Item.Id);
                    }

                    a_dataChanges.InventoryChanges.AddedObject(inv.Id);
                    a_dataChanges.AuditEntry(new AuditEntry(inv.Id, inv.Warehouse.Id, inv), true);
                }
            }
            catch (PTValidationException err)
            {
                a_errList.Add(err);
            }
        }

        if (a_inventoryAutoDelete)
        {
            InventoryDeleteProfile deleteProfile = new();

            //Remove any Inventories not updated
            foreach (Inventory inventory in m_inventories)
            {
                if (!affectedInventories.Contains(inventory.Item.Id))
                {
                    deleteProfile.Add(inventory);
                }
            }

            //Now perform the deletes
            if (!deleteProfile.Empty)
            {
                a_inventoryAutoDeleteActions.Add(new PostProcessingAction(a_t, false, () =>
                {
                    ApplicationExceptionList delErrList = new();

                    try
                    {
                        DeleteInventory(a_sd, deleteProfile, a_sd.JobManager, a_sd.PurchaseToStockManager, a_sd.TransferOrderManager, a_warehouse, a_dataChanges);
                    }
                    catch (PTValidationException delErr)
                    {
                        delErrList.Add(delErr);
                    }

                    foreach (PTValidationException validationException in deleteProfile.ValidationExceptions)
                    {
                        delErrList.Add(validationException);
                    }

                    if (delErrList.Count > 0)
                    {
                        ScenarioExceptionInfo sei = new();
                        sei.Create(a_sd);
                        m_errorReporter.LogException(delErrList, a_t, sei, ELogClassification.PtInterface, false);
                    }
                }));
            }
        }
    }

    public void UpdateLots(ScenarioDetail a_sd, WarehouseT.Warehouse a_warehouseT, UserFieldDefinitionManager a_udfManager, bool a_autoDeleteLots, bool a_autoDeleteItemStorageLots, ApplicationExceptionList a_errList, IScenarioDataChanges a_dataChanges)
    {
        for (int i = 0; i < a_warehouseT.InventoryCount; i++)
        {
            try
            {
                WarehouseT.Inventory inventoryNode = a_warehouseT.GetInventoryByIndex(i);
                Item item = a_sd.ItemManager.GetByExternalId(inventoryNode.ItemExternalId);

                if (item == null)
                {
                    throw new PTValidationException("2194", new object[] { inventoryNode.ItemExternalId, a_warehouseT.ExternalId });
                }

                Inventory inventory = this[item.Id];
                if (inventory == null)
                {
                    throw new PTValidationException("3115", new object[] { inventoryNode.ItemExternalId, a_warehouseT.ExternalId });
                }

                inventory.UpdateLots(inventoryNode, a_udfManager, a_autoDeleteLots, a_autoDeleteItemStorageLots, a_dataChanges);
            }
            catch (PTValidationException err)
            {
                a_errList.Add(err);
            }
        }
    }
    #endregion

    #region Deleting
    /// <summary>
    /// Remove any inventories for this Item.
    /// </summary>
    /// <param name="item"></param>
    internal void ValidateAndDeleteItems(ScenarioDetail a_sd, ItemDeleteProfile a_items, JobManager a_jobs, PurchaseToStockManager a_purchases, TransferOrderManager a_transferOrderManager, Warehouse a_warehouse, IScenarioDataChanges a_dataChanges)
    {
        InventoryDeleteProfile deleteProfile = new();

        foreach (Item item in a_items)
        {
            Inventory inv = this[item.Id];
            if (inv != null)
            {
                deleteProfile.Add(inv);
            }
        }

        DeleteInventory(a_sd, deleteProfile, a_jobs, a_purchases, a_transferOrderManager, a_warehouse, a_dataChanges);

        //Update the item profile in case it will be used in other warehouses
        foreach (Inventory withError in deleteProfile.InventoriesWithErrors())
        {
            a_items.AddInventoryInUse(withError);
        }
    }

    /// <summary>
    /// Deleting warehouse so remove references to all Inventories.
    /// </summary>
    internal void Deleting(ScenarioDetail a_sd)
    {
        foreach (Inventory inventory in this)
        {
            inventory.Deleting();
            inventory.ForecastVersionsClear(a_sd, null);
        }
    }

    public void Receive(ScenarioDetailClearT a_t, ScenarioDetail a_sd, Warehouse a_wh, IScenarioDataChanges a_dataChanges)
    {
        if (a_t.ClearInventories || a_t.ClearForecasts)
        {
            foreach (Inventory inventory in this)
            {
                inventory.ForecastVersionsClear(a_sd, a_t);
            }
        }

        foreach (Inventory inventory in this)
        {
            if (a_t.ClearLots)
            {
                inventory.ClearLots(a_sd.JobManager, a_sd.PurchaseToStockManager, a_dataChanges);
            }
        }

        if (a_t.ClearInventories)
        {
            Clear(a_t, a_sd, a_wh, a_dataChanges);
        }
    }

    public void Clear(ScenarioDetailClearT a_t, ScenarioDetail a_sd, Warehouse a_wh, IScenarioDataChanges a_dataChanges)
    {
        InventoryDeleteProfile deleteProfile = new(a_t.ClearJobs, a_t.ClearPurchaseToStocks, a_t.ClearTransferOrders, a_t.ClearItemStorages);
        foreach (Inventory inventory in this)
        {
            deleteProfile.Add(inventory);
        }

        DeleteInventory(a_sd, deleteProfile, a_sd.JobManager, a_sd.PurchaseToStockManager, a_sd.TransferOrderManager, a_wh, a_dataChanges);
    }

    private void DeleteInventory(ScenarioDetail a_sd, InventoryDeleteProfile a_deleteProfile, JobManager a_jobs, PurchaseToStockManager a_purchases, TransferOrderManager a_transferOrderManager, Warehouse a_wh, IScenarioDataChanges a_dataChanges)
    {
        //Prepare caches for fast delete
        a_jobs.ValidateInventoryDelete(a_deleteProfile);
        a_purchases.ValidateInventoryDelete(a_deleteProfile);
        a_transferOrderManager.ValidateInventoryDelete(a_deleteProfile);

        foreach (Inventory inventory in a_deleteProfile.InventoriesSafeToDelete())
        {
            inventory.Deleting();
            inventory.ForecastVersionsClear(a_sd, null);
            inventory.ClearLots(a_sd.JobManager, a_sd.PurchaseToStockManager, a_dataChanges);
            a_dataChanges.AuditEntry(new AuditEntry(inventory.Id, inventory.Warehouse.Id, inventory), false, true);
            a_dataChanges.InventoryChanges.DeletedObject(inventory.Id);
            m_inventories.RemoveByKey(inventory.Item.Id);
        }
    }

    /// <summary>
    /// Throws an exception if this Inventory Manager can't be deleted due to its Inventories being referenced.
    /// </summary>
    internal void ValidateDelete(JobManager a_jobs, PurchaseToStockManager a_purchases, TransferOrderManager a_transferOrderManager)
    {
        InventoryDeleteProfile deleteProfile = new();
        foreach (Inventory inventory in this)
        {
            deleteProfile.Add(inventory);
        }

        a_jobs.ValidateInventoryDelete(deleteProfile);
        a_purchases.ValidateInventoryDelete(deleteProfile);
        a_transferOrderManager.ValidateInventoryDelete(deleteProfile);
    }
    #endregion

    #region List Maintenance
    private readonly InventoryList m_inventories;

    internal void Add(Inventory a_inventory)
    {
        m_inventories.Add(a_inventory);
    }

    public bool Contains(BaseId a_itemId)
    {
        return m_inventories.GetValue(a_itemId) != null;
    }

    /// <summary>
    /// Returns the On Hand Qty for the specified Item.
    /// If the Item has no Inventory in this Inventory Manager then -1 is returned.
    /// </summary>
    /// <param name="a_item"></param>
    /// <returns></returns>
    public decimal GetOnHandQty(Item a_item)
    {
        if (Contains(a_item.Id))
        {
            return this[a_item.Id].OnHandQty;
        }

        return -1;
    }

    /// <summary>
    /// Returns the Inventory tracking storage of the Item or null if the item isn't in this manager.
    /// </summary>
    public Inventory this[BaseId a_itemId]
    {
        get
        {
            Inventory itemInv = m_inventories.GetValue(a_itemId);
            return itemInv;
        }
    }

    public Inventory GetByIndex(int a_index)
    {
        Inventory itemInv = m_inventories.GetByIndex(a_index);
        return itemInv;
    }

    public IEnumerator<Inventory> GetEnumerator()
    {
        return m_inventories.GetEnumerator();
    }

    public int Count => m_inventories.Count;
    #endregion

    #region Import Database
    public void PopulateImportDataSet(PtImportDataSet a_ds, PtImportDataSet.WarehousesRow a_warehouseRow, PtImportDataSet.ItemsDataTable a_itemsTable, PtImportDataSet.LotsDataTable a_lotTable)
    {
        foreach (Inventory inventory in this)
        {
            //Need to find the corresponding Item row
            PtImportDataSet.ItemsRow itemRow = a_itemsTable.FindByExternalId(inventory.Item.ExternalId);
            inventory.PopulateImportDataSet(a_ds, a_warehouseRow, itemRow, a_lotTable);
        }
    }
    #endregion Import Database

    internal void ConsumeForecasts(ScenarioDetail a_sd, bool a_consumeForecasts)
    {
        Parallel.ForEach(m_inventories.ReadOnlyList, a_itemInv => { a_itemInv.ConsumeForecasts(a_sd, a_consumeForecasts); });
    }

    IEnumerator<Inventory> IEnumerable<Inventory>.GetEnumerator()
    {
        return m_inventories.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
