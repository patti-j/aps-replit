using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Attributes;
using PT.ERPTransmissions;
using PT.Scheduler.Demand;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.Scheduler.Schedule.Storage;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Storage area has a collection of items that can be stored.
/// </summary>

public partial class StorageArea : BaseObject, IComparable<StorageArea>, IPTSerializable
{
    private readonly BaseIdGenerator m_idGen;

    //Similar to how Inventory maps Item to Warehouse, this maps Items to a Storage Area. Only Items in this ItemStorage collection can be stored in this StorageArea
    private ItemStorageList m_storage = new ItemStorageList();

    internal StorageArea(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_reader)
    {
        m_idGen = a_idGen;
        m_referenceInfo = new ();

        if (a_reader.VersionNumber >= 13000)
        {
            m_bools = new BoolVector32(a_reader);
            m_storage = new ItemStorageList(a_reader, a_idGen);
            a_reader.Read(out m_storageInFlowLimit);
            a_reader.Read(out m_storageOutFlowLimit);
            a_reader.Read(out m_counterFlowLimit);
            m_referenceInfo = new ReferenceInfo(a_reader);
        }
        else if (a_reader.VersionNumber >= 12555)
        {
            m_bools = new BoolVector32(a_reader);
            m_storage = new ItemStorageList(a_reader, a_idGen);
            a_reader.Read(out m_storageInFlowLimit);
            a_reader.Read(out m_storageOutFlowLimit);
            a_reader.Read(out m_counterFlowLimit);
        }
        else if (a_reader.VersionNumber >= 12511)
        {
            m_bools = new BoolVector32(a_reader);
            m_storage = new ItemStorageList(a_reader, a_idGen);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_storage.Serialize(a_writer);
        a_writer.Write(m_storageInFlowLimit);
        a_writer.Write(m_storageOutFlowLimit);
        a_writer.Write(m_counterFlowLimit);

        m_referenceInfo = new (Resource);
        m_referenceInfo.Serialize(a_writer);
    }

    /// <summary>
    /// New object Constructor
    /// </summary>
    internal StorageArea(BaseIdGenerator a_idGen, PTObjectBase a_objectBase, Warehouse a_warehouse, UserFieldDefinitionManager a_udfManager)
        : base(a_idGen.NextID(), a_objectBase, a_udfManager, UserField.EUDFObjectType.StorageArea)
    {
        m_idGen = a_idGen;
        Warehouse = a_warehouse;
    }


    public override int UniqueId => 1119;

    private ReferenceInfo m_referenceInfo = new ();

    private class ReferenceInfo
    {
        private readonly BaseId m_resourceId = BaseId.NULL_ID;
        internal BaseId ResourceId => m_resourceId;

        internal ReferenceInfo(IReader a_reader)
        {
            m_resourceId = new BaseId(a_reader);
        }

        internal ReferenceInfo() { }

        internal ReferenceInfo(BaseResource a_resource)
        {
            if (a_resource != null)
            {
                m_resourceId = a_resource.Id;
            }
        }

        internal void Serialize(IWriter a_writer)
        {
            m_resourceId.Serialize(a_writer);
        }
    }

    internal void RestoreReferences(PlantManager a_plantManager, CustomerManager a_cm, ItemManager a_itemManager, Warehouse a_warehouse, ISystemLogger a_errorReporter)
    {
        if (a_plantManager.GetResource(m_referenceInfo.ResourceId) is Resource res)
        {
            m_resource = res;
        }

        Warehouse = a_warehouse;
        foreach (ItemStorage itemStorage in m_storage)
        {
            itemStorage.RestoreReferences(a_cm, a_itemManager, a_warehouse, this, a_errorReporter);
        }

        m_referenceInfo = null;
    }

    public override string DefaultNamePrefix => "Storage Area";
    
    public Warehouse Warehouse;

    private BoolVector32 m_bools;

    private const short c_storeSingleItemIdx = 0;
    private const short c_constraintInFlowIdx = 1;
    private const short c_constraintOutFlowIdx = 2;
    private const short c_constraintCounterFlowIdx = 3;

    public bool SingleItemStorage
    {
        get => m_bools[c_storeSingleItemIdx];
        private set => m_bools[c_storeSingleItemIdx] = value;
    }

    public bool ConstrainInFlow
    {
        get => m_bools[c_constraintInFlowIdx];
        private set => m_bools[c_constraintInFlowIdx] = value;
    }

    public bool ConstrainOutFlow
    {
        get => m_bools[c_constraintOutFlowIdx];
        private set => m_bools[c_constraintOutFlowIdx] = value;
    }

    public bool ConstrainCounterFlow
    {
        get => m_bools[c_constraintCounterFlowIdx];
        private set => m_bools[c_constraintCounterFlowIdx] = value;
    }

    private int m_storageInFlowLimit;
    /// <summary>
    /// Indicates how many objects can store material into storage at the same time
    /// </summary>
    public int StorageInFlowLimit
    {
        get => m_storageInFlowLimit;
        private set
        {
            if (value < 0)
            {
                throw new PTValidationException("3123", new object[] { ExternalId });
            }

            m_storageInFlowLimit = value;
        }
    }

    private int m_storageOutFlowLimit;
    /// <summary>
    /// Indicates how many objects can withdraw material from storage at the same time
    /// </summary>
    public int StorageOutFlowLimit
    {
        get => m_storageOutFlowLimit;
        private set
        {
            if (value < 0)
            {
                throw new PTValidationException("3124", new object[] { ExternalId });
            }

            m_storageOutFlowLimit = value;
        }
    }

    private int m_counterFlowLimit; // < 0 means not allowed
    /// <summary>
    /// Indicates the total limit of storing and withdrawing
    /// </summary>
    public int CounterFlowLimit
    {
        get => m_counterFlowLimit;
        private set
        {
            if (value < 0)
            {
                throw new PTValidationException("3125", new object[] { ExternalId });
            }

            m_counterFlowLimit = value;
        }
    }

    public ItemStorage GetItemStorage(Item a_item)
    {
        return m_storage[a_item];
    }
    [DoNotAuditProperty]
    internal ItemStorageList Storage => m_storage;

    private Resource m_resource;
    [DoNotAuditProperty]
    public Resource Resource => m_resource;

    public IEnumerable<Item> GetItemsStored()
    {
        foreach (ItemStorage itemStorage in Storage)
        {
            yield return itemStorage.Item;
        }
    }

    #region PT Import DB
    public void PopulateImportDataSet(PtImportDataSet a_ds, PtImportDataSet.WarehousesRow a_wRow, ScenarioDetail a_sd)
    {
        string plantExternalId = string.Empty;
        string departmentExternalId = string.Empty;
        string resourceExternalId = string.Empty;

        if (Resource != null)
        {
            plantExternalId = Resource.Department.Plant.ExternalId;
            departmentExternalId = Resource.Department.ExternalId;
            resourceExternalId = Resource.ExternalId;
        }

        PtImportDataSet.StorageAreasRow addStorageAreaRow = a_ds.StorageAreas.AddStorageAreasRow(
            ExternalId, a_wRow, Name, Description, Notes, UserFields == null ? "" : UserFields.GetUserFieldImportString(),
            SingleItemStorage, StorageInFlowLimit, StorageOutFlowLimit, CounterFlowLimit, ConstrainInFlow, ConstrainOutFlow, 
            ConstrainCounterFlow, plantExternalId, departmentExternalId, resourceExternalId
        );

        foreach (ItemStorage itemStorage in Storage)
        {
            a_ds.ItemStorage.AddItemStorageRow(itemStorage.Item.ExternalId, a_wRow.ExternalId,addStorageAreaRow, itemStorage.MaxQty, itemStorage.DisposalQty, itemStorage.DisposeImmediately, itemStorage.UserFields == null ? "" : itemStorage.UserFields.GetUserFieldImportString());
        }
    }
    #endregion

    internal void SimulationStageInitialization()
    {
        foreach (ItemStorage itemStorage in Storage)
        {
            itemStorage.SimulationStageInitialization();
        }
    }

    internal void SimulationComplete()
    {
        foreach (ItemStorage itemStorage in Storage)
        {
            itemStorage.SimulationComplete();
        }
    }

    /// <summary>
    /// Whether the current SA can store the item. For use in scheduling
    /// </summary>
    /// <param name="a_itemId"></param>
    /// <returns></returns>
    internal bool CanAddStorage(SupplyProfile a_profile)
    {
        //We need the entire profile to check for future adjustments
        if (!m_storage.TryGetValue(a_profile.Inventory.Item.Id, out ItemStorage storage))
        {
            return false;
        }

        if (SingleItemStorage)
        {
            if (m_storedItems.Count == 0)
            {
                return true;
            }

            if (m_storedItems.Count == 1)
            {
                return m_storedItems.First().Value.Item.Id == storage.Item.Id;
            }

            // > 1
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// Removes any ItemStorage that no longer has any active nodes
    /// </summary>
    internal int UpdateStoredItemsCache()
    {
        ItemStorage[] itemStorages = m_storedItems.Values.ToArray();
        foreach (ItemStorage itemStorage in itemStorages)
        {
            if (itemStorage.CheckForEmpty(long.MaxValue))
            {
                FlagStorageRemoval(itemStorage);
            }
        }

        return m_storedItems.Count;
    }

    #region ERP Transmissions
    internal void Edit(ScenarioDetail a_sd, StorageAreaEditT a_t, StorageAreaEdit a_edit)
    {
        base.Edit(a_edit);

        if (a_edit.SingleItemStorageSet)
        {
            SingleItemStorage = a_edit.SingleItemStorage;
        }

        if (a_edit.ConstrainCounterFlowIsSet)
        {
            ConstrainCounterFlow = a_edit.ConstrainCounterFlow;
        }

        if (a_edit.ConstrainInFlowIsSet)
        {
            ConstrainInFlow = a_edit.ConstrainInFlow;
        }

        if (a_edit.ConstrainOutFlowIsSet)
        {
            ConstrainOutFlow = a_edit.ConstrainOutFlow;
        }

        if (a_edit.StorageInFlowLimitIsSet)
        {
            StorageInFlowLimit = a_edit.StorageInFlowLimit;
        }

        if (a_edit.StorageOutFlowLimitIsSet)
        {
            StorageOutFlowLimit = a_edit.StorageOutFlowLimit;
        }

        if (a_edit.CounterFlowLimitIsSet)
        {
            CounterFlowLimit = a_edit.CounterFlowLimit;
        }

        if (a_edit.ResourceIdIsSet)
        {
            if (a_edit.ResourceId == BaseId.NULL_ID)
            {
                ClearStorageResourceReference();
            }
            else
            {
                Resource res = (Resource)a_sd.PlantManager.GetResource(a_edit.ResourceId);
                SetStorageResourceReference(res);
            }
        }
    }
    
    #endregion

    internal void Update(ScenarioDetail a_sd, 
                         WarehouseT.StorageArea a_storageAreaT, 
                         ItemManager a_itemManager, 
                         UserFieldDefinitionManager a_udfManager, 
                         PTTransmission a_ptTransmission, 
                         bool a_autoDeleteItemStorage,
                         IScenarioDataChanges a_dataChanges, 
                         ApplicationExceptionList a_errList, 
                         List<PostProcessingAction> a_itemStorageAutoDeleteActions)
    {
        AuditEntry storageAreaAuditEntry = new AuditEntry(Id, Warehouse.Id, this);
        Update(a_storageAreaT, a_ptTransmission, a_udfManager, UserField.EUDFObjectType.StorageArea);

        if (a_storageAreaT.SingleItemStorageIsSet && a_storageAreaT.SingleItemStorage != SingleItemStorage)
        {
            SingleItemStorage = a_storageAreaT.SingleItemStorage;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_storageAreaT.StorageInFlowLimitIsSet && a_storageAreaT.StorageInFlowLimit != StorageInFlowLimit)
        {
            StorageInFlowLimit = a_storageAreaT.StorageInFlowLimit;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_storageAreaT.StorageOutFlowLimitIsSet && a_storageAreaT.StorageOutFlowLimit != StorageOutFlowLimit)
        {
            StorageOutFlowLimit = a_storageAreaT.StorageOutFlowLimit;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_storageAreaT.CounterFlowLimitIsSet && a_storageAreaT.CounterFlowLimit != CounterFlowLimit)
        {
            CounterFlowLimit = a_storageAreaT.CounterFlowLimit;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_storageAreaT.ConstrainInFlowIsSet && a_storageAreaT.ConstrainInFlow != ConstrainInFlow)
        {
            ConstrainInFlow = a_storageAreaT.ConstrainInFlow;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_storageAreaT.ConstrainOutFlowIsSet && a_storageAreaT.ConstrainOutFlow != ConstrainOutFlow)
        {
            ConstrainOutFlow = a_storageAreaT.ConstrainOutFlow;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_storageAreaT.ConstrainCounterFlowIsSet && a_storageAreaT.ConstrainCounterFlow != ConstrainCounterFlow)
        {
            ConstrainCounterFlow = a_storageAreaT.ConstrainCounterFlow;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_storageAreaT.PlantExternalIdIsSet)
        {
            if (String.IsNullOrEmpty(a_storageAreaT.PlantExternalId))
            {
                //Clearing
                ClearStorageResourceReference();
            }
        }

        HashSet<BaseId> importedItemStorageIds = new ();
        for (int i = 0; i < a_storageAreaT.ItemStorageCount; i++)
        {
            WarehouseT.ItemStorage itemStorageT = a_storageAreaT.GetItemStorageByIndex(i);

            Item item = a_itemManager.GetByExternalId(itemStorageT.ItemExternalId);
            
            if (item == null)
            {
               throw new PTValidationException("2194", new object[] { itemStorageT.ItemExternalId, a_storageAreaT.WarehouseExternalId});
            }

            importedItemStorageIds.Add(item.Id);
            ItemStorage itemStorage = Storage.GetValue(item.Id);
            Inventory inventory = Warehouse.Inventories[item.Id];

            if (inventory == null)
            {
                throw new PTValidationException("3138", new object[] {ExternalId, itemStorageT.ItemExternalId, a_storageAreaT.WarehouseExternalId });
            }


            if (itemStorage == null)
            {
                itemStorage = new ItemStorage(m_idGen.NextID(), item, this, inventory);
                itemStorage.Update(itemStorageT, inventory?.Lots, a_udfManager);
                Storage.Add(itemStorage);
                a_dataChanges.AuditEntry(new AuditEntry(itemStorage.Id, Id, itemStorage), true);
            }
            else
            {
                AuditEntry itemStorageAuditEntry = new AuditEntry(itemStorage.Id, Id, itemStorage);
                itemStorage.Update(itemStorageT, inventory?.Lots, a_udfManager);
                a_dataChanges.AuditEntry(itemStorageAuditEntry);
            }
        }

        if (a_autoDeleteItemStorage)
        {
            List<ItemStorage> itemStorageToDelete = new List<ItemStorage>();
            //Remove any Inventories not updated
            foreach (ItemStorage itemStorage in Storage)
            {
                if (!importedItemStorageIds.Contains(itemStorage.Item.Id))
                {
                    itemStorageToDelete.Add(itemStorage);
                }
            }

            //Now perform the deletes
            if (itemStorageToDelete.Count > 0)
            {
                a_itemStorageAutoDeleteActions.Add(new PostProcessingAction(a_ptTransmission, false, () =>
                {
                    ItemStorageDeleteProfile deleteProfile = new();
                    try
                    {
                        foreach (ItemStorage itemStorage in itemStorageToDelete)
                        {
                            deleteProfile.Add(itemStorage);
                        }

                        Storage.ValidateDeleteItemStorage(a_sd.JobManager, a_sd.PurchaseToStockManager, Warehouse, deleteProfile);
                        Storage.DeleteItemStorage(Warehouse, deleteProfile);

                        foreach (ItemStorage itemStorage in deleteProfile.ItemStoragesSafeToDelete())
                        {
                            a_dataChanges.AuditEntry(new AuditEntry(itemStorage.Id, Id, itemStorage), false, true);
                        }
                    }
                    catch (PTValidationException delErr)
                    {
                        a_errList.Add(delErr);
                    }

                    foreach (PTValidationException validationException in deleteProfile.ValidationExceptions)
                    {
                        a_errList.Add(validationException);
                    }
                }));
            }
        }

        a_dataChanges.StorageAreaChanges.UpdatedObject(Id);
        a_dataChanges.AuditEntry(storageAreaAuditEntry);
    }

    internal void ClearStorageResourceReference()
    {
        if (m_resource != null)
        {
            //Importing empty string, clear the reference
            m_resource.StorageAreaResource = false;
            m_resource = null;
        }
    }

    internal void SetStorageResourceReference(Resource a_resource)
    {
        m_resource = a_resource;
        m_resource.StorageAreaResource = true;
    }

    public int CompareTo(StorageArea a_other)
    {
        return ID.CompareTo(a_other.Id);
    }

    internal void Receive(ScenarioDetailClearT a_clearT, JobManager a_jobManager, PurchaseToStockManager a_purchaseToStockManager)
    {
        if (a_clearT.ClearItemStorages)
        {
            ItemStorageDeleteProfile deleteProfile = new(a_clearT.ClearJobs, a_clearT.ClearPurchaseToStocks);
            foreach (ItemStorage itemStorage in Storage)
            {
                deleteProfile.Add(itemStorage);
            }

            if (!deleteProfile.Empty)
            {
                Storage.ValidateDeleteItemStorage(a_jobManager, a_purchaseToStockManager, Warehouse, deleteProfile);
                if (deleteProfile.CanSafelyRemoveAll)
                {
                    Storage.DeleteItemStorage(Warehouse, deleteProfile);
                }
            }
            //TODO: Log error deletions
        }
    }

    internal void ValidateDelete(JobManager a_jobManager, PurchaseToStockManager a_purchaseToStockManager, TransferOrderManager a_transferOrderManager, StorageAreasDeleteProfile a_storageAreaDeleteProfile, ItemStorageDeleteProfile a_itemStorageDelete)
    {
        a_jobManager.ValidateStorageAreaDelete(this, a_storageAreaDeleteProfile);
        a_purchaseToStockManager.ValidateStorageAreaDelete(this, a_storageAreaDeleteProfile);
        a_transferOrderManager.ValidateStorageAreaDelete(this, a_storageAreaDeleteProfile);

        if (a_storageAreaDeleteProfile.HasError(this.Id))
        {
            //Can't delete
            return;
        }

        foreach (ItemStorage itemStorage in Storage)
        {
            a_itemStorageDelete.Add(itemStorage);
        }

        if (!a_itemStorageDelete.Empty)
        {
            Storage.ValidateDeleteItemStorage(a_jobManager, a_purchaseToStockManager, Warehouse, a_itemStorageDelete);
            if (a_itemStorageDelete.CanSafelyRemoveAll)
            {
                Storage.DeleteItemStorage(Warehouse, a_itemStorageDelete);
            }
            else
            {
                PTValidationException validationException = new("3106", new object[] { this.Name, Warehouse.Name });
                a_storageAreaDeleteProfile.AddValidationException(this, validationException);
            }
        }
    }
        
    /// <summary>
    /// Total Number of items mapped as eligible to be stored in this storage area.
    /// </summary>
    public int ItemCount
    {
        get => Storage.Count;
    }
    /// <summary>
    /// Total number of adjustments made to inventory levels in this storage area over the entire schedule.
    /// </summary>
    /// <returns></returns>
    public int GetTotalAdjustmentCount()
    {
        int totalAdjustmentCount = 0;
        foreach (ItemStorage itemStorage in Storage)
        {
            var adjustments = itemStorage.Inventory?.Adjustments;
            if (adjustments == null)
            {
                continue;
            }

            foreach (Adjustment adjustment in adjustments)
            {
                if (adjustment.HasStorage && adjustment.Storage.StorageArea.Equals(Id))
                {
                    totalAdjustmentCount++;
                }
            }
        }
        return totalAdjustmentCount;
    }
    /// <summary>
    /// Sum of all demands requiring material from this storage area (including jobs, sales orders, and forecasts).
    /// </summary>
    /// <returns></returns>
    public decimal GetTotalDemand()
    {
        decimal totalDemand = 0;
        foreach (ItemStorage itemStorage in Storage)
        {
            var adjustments = itemStorage.Inventory?.Adjustments;
            if (adjustments == null)
            {
                continue;
            }

            foreach (Adjustment adjustment in adjustments)
            {
                if (adjustment.HasStorage && adjustment.Storage.StorageArea.Equals(Id) && adjustment.ChangeQty < 0 && (adjustment.AdjustmentType == InventoryDefs.EAdjustmentType.ForecastDemand 
                    || adjustment.AdjustmentType == InventoryDefs.EAdjustmentType.MaterialRequirement || adjustment.AdjustmentType == InventoryDefs.EAdjustmentType.SalesOrderDemand))
                {
                    totalDemand += (-1* adjustment.ChangeQty);
                }
            }
        }
        return totalDemand;
    }

    /// <summary>
    /// Sum of all production, purchases, and imported lots that add inventory to this storage area.
    /// </summary>
    /// <returns></returns>
    public decimal GetTotalSupply()
    {
        decimal totalSupply = 0;
        foreach (ItemStorage itemStorage in Storage)
        {
            var adjustments = itemStorage.Inventory?.Adjustments;
            if (adjustments == null)
            {
                continue;
            }

            foreach (Adjustment adjustment in adjustments)
            {
                //Adjustment is a production adjustment it is a positive change
                if (adjustment.HasStorage && adjustment.Storage!.StorageArea.Equals(Id))
                {
                    totalSupply += adjustment.ChangeQty;
                }
            }
        }
        return totalSupply;
    }

    /// <summary>
    /// Sum of all inventory across lots in this storage area that expired.
    /// </summary>
    /// <returns></returns>
    public decimal GetTotalLotExpiration()
    {
        decimal totalExpired = 0;
        foreach (ItemStorage itemStorage in Storage)
        {
            var adjustments = itemStorage.Inventory?.Adjustments;
            if (adjustments == null)
            {
                continue;
            }

            foreach (Adjustment adjustment in adjustments)
            {
                if (adjustment.HasLotStorage && adjustment.Storage.StorageArea.Equals(Id) && adjustment.Expired)
                {
                    decimal change = adjustment.ChangeQty;
                    if (adjustment.ChangeQty < 0)
                    {
                        change *= -1;
                    }
                    totalExpired += change;
                }
            }
        }
        return totalExpired;
    }

    /// <summary>
    /// Sum of all inventory across lots in this storage area that expired.
    /// </summary>
    /// <returns></returns>
    public decimal GetTotalTransfersIn()
    {
        decimal totalExpired = 0;
        foreach (ItemStorage itemStorage in Storage)
        {
            var adjustments = itemStorage.Inventory?.Adjustments;
            if (adjustments == null)
            {
                continue;
            }

            foreach (Adjustment adjustment in adjustments)
            {
                if (adjustment.HasStorage && adjustment.Storage.StorageArea.Equals(Id) && adjustment.GetReason() is TransferOrderDistribution && adjustment.ChangeQty > 0)
                {
                    totalExpired += adjustment.ChangeQty;
                }
            }
        }
        return totalExpired;
    }

    /// <summary>
    /// Sum of all inventory transferred out of this storage area to another via Transfer Orders.
    /// </summary>
    /// <returns></returns>
    public decimal GetTotalTransfersOut()
    {
        decimal totalExpired = 0;
        foreach (ItemStorage itemStorage in Storage)
        {
            var adjustments = itemStorage.Inventory?.Adjustments;
            if (adjustments == null)
            {
                continue;
            }

            foreach (Adjustment adjustment in adjustments)
            {
                if (adjustment.HasStorage && adjustment.Storage.StorageArea.Equals(Id) && adjustment.GetReason() is TransferOrderDistribution && adjustment.ChangeQty < 0)
                {
                    totalExpired += adjustment.ChangeQty;
                }
            }
        }
        return totalExpired;
    }

    /// <summary>
    /// Whether the Storage Area can store this item. It's based on SA mapping.
    /// </summary>
    /// <param name="a_itemId"></param>
    /// <returns></returns>
    public bool CanStoreItem(BaseId a_itemId)
    {
        return m_storage.ContainsKey(a_itemId);
    }

    public override string ToString()
    {
        return $"{Name} for {Warehouse}";
    }

    /// <summary>
    /// Parse through adjustments to find OnHand adjustments for this StorageArea
    /// </summary>
    /// <param name="a_endOfSearchTime"></param>
    /// <returns></returns>
    public decimal GetOnHandQty(DateTime a_endOfSearchTime)
    {
        decimal onHand = 0m;
        //foreach (Inventory inv in Warehouse.Inventories)
        //{
        //    foreach (Lot invLot in inv.Lots)
        //    {
        //        if (invLot.LotSource == ItemDefs.ELotSource.Inventory)
        //        {
        //            onHand += invLot.GetItemStorageQty(this);
        //        }
        //    }
        //}

        foreach (ItemStorage itemStorage in Storage)
        {
            var adjustments = itemStorage.Inventory?.Adjustments;
            if (adjustments == null)
            {
                continue;
            }

            foreach (Adjustment adjustment in adjustments)
            {
                if (adjustment.Time > a_endOfSearchTime.Ticks)
                {
                    //Adjustments are ordered. If we past the date, no need to search other adjustments.
                    break;
                }

                if (adjustment.AdjustmentType == InventoryDefs.EAdjustmentType.OnHand 
                    && adjustment.HasStorage 
                    && adjustment.Storage.StorageArea == this)
                {
                    onHand += adjustment.ChangeQty;
                }
            }
        }

        return onHand;
    }

    /// <summary>
    /// Validates whether items can be deleted from the storage area.
    /// </summary>
    /// <param name="a_items"></param>
    internal void ValidateItemDelete(ItemDeleteProfile a_items)
    {
        foreach (ItemStorage itemStorage in Storage)
        {
            itemStorage.ValidateItemDelete(a_items);
        }
    }

    /// <summary>
    /// Validates whether the specified inventory items can be deleted based on their presence in the storage area.
    /// </summary>
    internal void ValidateInventoryDelete(InventoryDeleteProfile a_deleteProfile)
    {
        if (a_deleteProfile.ClearingItemStorages)
        {
            return;
        }

        foreach (Inventory inventory in a_deleteProfile)
        {
            if (Storage.ContainsKey(inventory.Item.Id))
            {
                PTValidationException validationException = new("3105", new object[] { inventory.Item.Name, Warehouse.Name, Name });
                a_deleteProfile.AddValidationException(inventory, validationException);
            }
        }
    }
    /// <summary>
    /// Calculates the maximum storage quantity available for the current storage area.
    /// If no defined max quantity is found, it returns the max quantity of production adjustments
    /// associated with this storage area that add to inventory (positive changes).
    /// </summary>
    /// <returns>The highest defined maximum quantity of its ItemStorages, or the highest supply quantity from production adjustments if none is defined.</returns>
    public decimal GetStorageMax()
    {
        decimal storageMax = 0m;
        decimal maxSupply = 0m;
        foreach (ItemStorage itemStorage in Storage)
        {
            storageMax = decimal.Max(storageMax, itemStorage.MaxQty);

            decimal currentSupply = 0m;

            var adjustments = itemStorage.Inventory?.Adjustments;
            if (adjustments == null)
            {
                continue;
            }

            foreach (Adjustment adjustment in adjustments)
            {
                if (adjustment.AdjustmentType == InventoryDefs.EAdjustmentType.PurchaseOrderAvailable
                    || adjustment.AdjustmentType == InventoryDefs.EAdjustmentType.ActivityProductionAvailable)
                {
                    continue;
                }

                //Adjustment is a production adjustment it is a positive change
                if (adjustment.HasStorage && adjustment.Storage!.StorageArea.Equals(Id))
                {
                    currentSupply += adjustment.ChangeQty;

                    maxSupply = decimal.Max(maxSupply, currentSupply);
                }
            }
        }

        if (storageMax > 0m && storageMax > maxSupply)
        {
            return storageMax;
        }

        return maxSupply;
    }
    /// <summary>
    /// Total number of adjustments made to inventory levels in this storage area over the entire schedule.
    /// </summary>
    /// <returns></returns>
    public AdjustmentArray GetAdjustments()
    {
        AdjustmentArray storageAdjustmentArray = new AdjustmentArray();

        foreach (ItemStorage itemStorage in Storage)
        {
            var adjustments = itemStorage.Inventory?.Adjustments;
            if (adjustments == null)
            {
                continue;
            }

            foreach (Adjustment adjustment in adjustments)
            {
                if (adjustment.HasStorage && adjustment.Storage.StorageArea.Equals(Id))
                {
                    storageAdjustmentArray.Add(adjustment);
                }
            }
        }

        return storageAdjustmentArray;
    }
    public decimal GetItemStorageMax(string a_itemExternalId)
    {
        return Storage.GetByItemExternalId(a_itemExternalId).MaxQty;
    }
    public void ClearItemStorages()
    {
        Storage.Clear();
    }
}
