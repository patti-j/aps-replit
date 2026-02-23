using PT.APSCommon;
using PT.Common.Attributes;
using PT.Common.Exceptions;
using PT.Common.File;
using PT.ERPTransmissions;
using PT.Scheduler.ErrorReporting;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.Scheduler.Schedule.Storage;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;
using PT.Transmissions;
using System.Collections;
using System.ComponentModel;

namespace PT.Scheduler;

/// <summary>
/// A location where inventories are stored.  Each Warehouse holds Inventories for various Items and can be accessed by various Plants.
/// </summary>
public partial class Warehouse : BaseObject, ICloneable, IPTSerializable
{
    public new const int UNIQUE_ID = 530;

    #region IPTSerializable Members
    internal Warehouse(IReader a_reader, BaseIdGenerator a_idGen)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12511)
        {
            m_inventories = new InventoryManager(a_reader, a_idGen);
            m_storageAreas = new StorageAreaCollection(a_reader, a_idGen); //Items Eligibility, Lots -> Adjustments
            m_storageAreaConnectors = new StorageAreaConnectorCollection(a_reader, a_idGen);
            a_reader.Read(out m_storageCapacity);
            a_reader.Read(out m_annualPercentageRate);
        }
        else if (a_reader.VersionNumber >= 628)
        {
            m_inventories = new InventoryManager(a_reader, a_idGen);
            a_reader.Read(out int docks);
            a_reader.Read(out m_storageCapacity);
            a_reader.Read(out m_annualPercentageRate);
            a_reader.Read(out bool m_tankWarehouse);

            m_storageAreas = new StorageAreaCollection(a_idGen);
            m_storageAreaConnectors = new StorageAreaConnectorCollection(a_idGen);
        }
    }

    internal void RestoreReferences(ScenarioDetail a_sd, CustomerManager a_cm, ItemManager a_itemManager, ISystemLogger a_errorReporter)
    {
        m_storageAreas.RestoreReferences(a_sd.PlantManager, a_cm, a_itemManager, this, a_errorReporter);

        Inventories.RestoreReferences(a_sd, a_cm, a_itemManager, this, a_errorReporter);
        m_storageAreaConnectors.RestoreReferences(this);

        //BACKWARDS Compatibility for scenarios prior to Storage Areas
        if (m_storageAreas.Count == 0)
        {
            StorageArea newSA = new StorageArea(a_sd.IdGen, new PTObjectBase("Default", "Default"), this, null);
            newSA.ExternalId += a_sd.IdGen.NextID().Value.ToString();
            m_storageAreas.Add(newSA);
            foreach (Inventory inventory in m_inventories)
            {
                ItemStorage itemStorage = new ItemStorage(a_sd.IdGen.NextID(), inventory.Item, newSA);
                newSA.Storage.Add(itemStorage);

                foreach (Lot lot in inventory.Lots)
                {
                    lot.BackwardsCompatibilityStorePreviousLotQty(itemStorage);
                }
            }
        }
    }

    internal void RestoreReferences(UserFieldDefinitionManager a_udfManager)
    {
        a_udfManager.RestoreReferences(this, UserField.EUDFObjectType.Warehouses);

        Inventories.RestoreReferences(a_udfManager);
    }

    internal void RestoreTemplateReferences(JobManager aJobManager)
    {
        Inventories.RestoreTemplateReferences(aJobManager);
    }

    internal void RestoreAdjustmentReferences(ScenarioDetail a_sd)
    {
        Inventories.RestoreAdjustmentReferences(a_sd);
    }    
    
    internal void AfterRestoreAdjustmentReferences()
    {
        Inventories.AfterRestoreAdjustmentReferences();
    }

    internal void SetImportedTemplateMoReferences(JobManager a_jobManager, PTTransmission a_t, ScenarioExceptionInfo a_sei, ISystemLogger a_errorReporter)
    {
        Inventories.SetImportedTemplateMoReferences(a_jobManager, a_t, a_sei, a_errorReporter);
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_inventories.Serialize(a_writer);
        m_storageAreas.Serialize(a_writer);
        m_storageAreaConnectors.Serialize(a_writer);
        a_writer.Write(m_storageCapacity);
        a_writer.Write(m_annualPercentageRate);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    private StorageAreaCollection m_storageAreas;

    #region Declarations
    public class WarehouseException : PTException
    {
        public WarehouseException(string a_message)
            : base(a_message) { }
    }
    #endregion

    #region Construction
    //public Warehouse(BaseId id, BaseIdGenerator idGen)
    //    : base(id)
    //{
    //    Init(idGen);
    //}
    /// <summary>
    /// Sets the field values for an ERP transmission.
    /// </summary>
    public Warehouse(BaseId a_id, WarehouseT.Warehouse a_warehouse, BaseIdGenerator a_idGen)
        : base(a_id, a_warehouse.ExternalId)
    {
        Init(a_idGen, a_warehouse);
    }

    private void Init(BaseIdGenerator a_idGen, WarehouseT.Warehouse a_warehouse)
    {
        m_storageAreas = new StorageAreaCollection(a_idGen);
        m_inventories = new InventoryManager(a_idGen);
        m_storageAreaConnectors = new StorageAreaConnectorCollection(a_idGen);

        for (int i = 0; i < a_warehouse.StorageAreaCount; i++)
        {
            WarehouseT.StorageArea storageArea = a_warehouse.GetStorageAreaByIndex(i);
            m_storageAreas.Add(new StorageArea(a_idGen, storageArea, this, null));
        }

    }
    #endregion

    #region Shared Properties
    private decimal m_storageCapacity;

    /// <summary>
    /// Can be used by a Scheduling Add-In to limit scheduling based upon availability of storage space.
    /// </summary>
    public decimal StorageCapacity
    {
        get => m_storageCapacity;
        set => m_storageCapacity = value;
    }

    private decimal m_annualPercentageRate = 10;

    /// <summary>
    /// APR for calculating carring cost.
    /// </summary>
    public decimal AnnualPercentageRate
    {
        get => m_annualPercentageRate;
        set
        {
            if (value > 100 || value < 0)
            {
                throw new PTHandleableException("APR should be a number between 0 and 100.".Localize());
            }

            m_annualPercentageRate = value;
        }
    }

    /// <summary>
    /// APR / (100 * 365)
    /// </summary>
    public decimal DailyInterestRate => AnnualPercentageRate / 36500;
    #endregion Shared Properties

    #region Inventories

    [AfterRestoreReferences.MasterCopyManager]
    private InventoryManager m_inventories;

    [DoNotAuditProperty]
    public InventoryManager Inventories => m_inventories;
    [DoNotAuditProperty]
    public StorageAreaCollection StorageAreas => m_storageAreas;

    private StorageAreaConnectorCollection m_storageAreaConnectors;
    [DoNotAuditProperty]
    public StorageAreaConnectorCollection StorageAreaConnectors => m_storageAreaConnectors;

    public void Receive(ScenarioDetailClearT a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        if (a_t.ClearStorageAreaConnectors)
        {
            StorageAreaConnectors.Clear();
        }

        Inventories.Receive(a_t, a_sd, this, a_dataChanges);
        StorageAreas.Receive(a_t, a_sd, this, a_dataChanges);
    }

    internal void ConsumeForecasts(ScenarioDetail a_sd, bool a_consumeForecasts)
    {
        Inventories.ConsumeForecasts(a_sd, a_consumeForecasts);
    }
    #endregion

    #region Overrides
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [Browsable(false)]
    public override string DefaultNamePrefix => "Warehouse";
    #endregion

    #region ERP Transmissions
    public void Edit(WarehouseEdit a_edit)
    {
        base.Edit(a_edit);

        if (a_edit.StorageCapacitySet)
        {
            StorageCapacity = a_edit.StorageCapacity;
        }

        if (a_edit.AnnualPercentageRateSet)
        {
            AnnualPercentageRate = a_edit.AnnualPercentageRate;
        }
    }

    internal void Update(WarehouseT.Warehouse a_warehouse,
                         bool a_autoDeleteInventories,
                         bool a_autoDeleteStorageAreas,
                         bool a_autoDeleteItemStorage,
                         bool a_autoDeleteLots,
                         bool a_autoDeleteItemStorageLots,
                         bool a_autoDeletePlantAssociations,
                         bool a_autoDeleteStorageAreaConnectors,
                         bool a_autoDeleteStorageAreaConnectorsIn,
                         bool a_autoDeleteStorageAreaConnectorsOut,
                         bool a_autoDeleteResourceStorageAreaConnectorsIn,
                         bool a_autoDeleteResourceStorageAreaConnectorsOut,
                         UserFieldDefinitionManager a_udfManager,
                         PTTransmission t,
                         ScenarioDetail a_sd,
                         ISystemLogger a_errorReporter,
                         IScenarioDataChanges a_dataChanges,
                         bool a_newWarehouse,
                         List<PostProcessingAction> a_actions,
                         ScenarioExceptionInfo a_sei)
    {
        ApplicationExceptionList errList = new ();

        bool warehouseUpdated = Update(a_warehouse, t, a_udfManager, UserField.EUDFObjectType.Warehouses);
        warehouseUpdated |= UpdateWarehouseFields(a_warehouse, a_sd.PurchaseToStockManager);

        if (warehouseUpdated && !a_newWarehouse)
        {
            a_dataChanges.WarehouseChanges.UpdatedObject(Id);
        }

        List<PostProcessingAction> inventoryAutoDeleteActions = new ();
        UpdateInventories(a_warehouse, a_udfManager, a_autoDeleteInventories, errList, t, a_sd, a_dataChanges, inventoryAutoDeleteActions);

        List<PostProcessingAction> itemStorageAutoDeleteActions = new();
        List<PostProcessingAction> storageAreaAutoDeleteActions = new();
        List<PostProcessingAction> storageAreaAddStorageResourceReferences = new();
        UpdateStorageAreas(a_warehouse, a_udfManager, a_autoDeleteStorageAreas, a_autoDeleteItemStorage, errList, t, a_sd, a_dataChanges, storageAreaAddStorageResourceReferences, storageAreaAutoDeleteActions, itemStorageAutoDeleteActions, a_sei);
        Inventories.UpdateLots(a_sd, a_warehouse, a_udfManager, a_autoDeleteLots, a_autoDeleteItemStorageLots, errList, a_dataChanges);
        UpdateStorageAreaConnectors(a_warehouse, a_udfManager, a_autoDeleteStorageAreaConnectors, a_autoDeleteStorageAreaConnectorsIn, a_autoDeleteStorageAreaConnectorsOut, a_autoDeleteResourceStorageAreaConnectorsIn,a_autoDeleteResourceStorageAreaConnectorsOut, errList, t, a_sd, a_dataChanges);
        UpdatePlantAssociations(a_warehouse, a_autoDeletePlantAssociations, a_sd.PlantManager, errList);

        a_actions.AddRange(storageAreaAddStorageResourceReferences);
        a_actions.AddRange(itemStorageAutoDeleteActions);
        a_actions.AddRange(storageAreaAutoDeleteActions);
        a_actions.AddRange(inventoryAutoDeleteActions);

        if (errList.Count > 0)
        {
            ScenarioExceptionInfo sei = new ();
            sei.Create(a_sd);
            a_errorReporter.LogException(errList, t, sei, ELogClassification.PtInterface, false);
        }
    }

    private bool UpdateWarehouseFields(WarehouseT.Warehouse a_warehouse, PurchaseToStockManager a_purchases)
    {
        bool changes = false;

        if (a_warehouse.StorageCapacitySet)
        {
            StorageCapacity = a_warehouse.StorageCapacity;
            changes = true;
        }

        if (a_warehouse.AnnualPercentageRateSet)
        {
            AnnualPercentageRate = a_warehouse.AnnualPercentageRate;
            changes = true;
        }

        return changes;
    }

    /// <summary>
    /// Add new Inventories, update Inventories that already exist and delete those omitted.
    /// </summary>
    private void UpdateInventories(WarehouseT.Warehouse a_warehouse, UserFieldDefinitionManager a_udfManager, bool a_inventoryAutoDelete, ApplicationExceptionList a_errList, PTTransmission a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges, List<PostProcessingAction> a_inventoryAutoDeleteActions)
    {
       Inventories.UpdateInventories(a_warehouse, a_udfManager, this, a_inventoryAutoDelete, this, a_errList, a_t, a_sd, a_dataChanges, a_inventoryAutoDeleteActions);
    }
    
    /// <summary>
    /// Add new Inventories, update Inventories that already exist and delete those omitted.
    /// </summary>
    private void UpdateStorageAreas(WarehouseT.Warehouse a_warehouse,
                                    UserFieldDefinitionManager a_udfManager, 
                                    bool a_autoDeleteStorageArea, 
                                    bool a_autoDeleteItemStorage, 
                                    ApplicationExceptionList a_errList, 
                                    PTTransmission a_t, 
                                    ScenarioDetail a_sd, 
                                    IScenarioDataChanges a_dataChanges,
                                    List<PostProcessingAction> a_storageAreaAddStorageResourceReferences, 
                                    List<PostProcessingAction> a_storageAreaAutoDeleteActions,
                                    List<PostProcessingAction> a_itemStorageAutoDeleteActions,
                                    ScenarioExceptionInfo a_sei)
    {
        StorageAreas.UpdateStorageAreas(a_warehouse, a_sd, this, a_t, a_autoDeleteStorageArea, a_autoDeleteItemStorage, a_errList, a_dataChanges, a_udfManager, a_storageAreaAddStorageResourceReferences, a_storageAreaAutoDeleteActions, a_itemStorageAutoDeleteActions, a_sei);
    }
    
    /// <summary>
    /// Add new Inventories, update Inventories that already exist and delete those omitted.
    /// </summary>
    private void UpdateStorageAreaConnectors(WarehouseT.Warehouse a_warehouse, UserFieldDefinitionManager a_udfManager, bool a_connectorAutoDelete, bool a_autoDeleteStorageAreaConnectorsIn, bool a_autoDeleteStorageAreaConnectorsOut, bool a_autoDeleteResourceStorageAreaConnectorsIn, bool a_autoDeleteResourceStorageAreaConnectorsOut, ApplicationExceptionList a_errList, PTTransmission a_t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        StorageAreaConnectors.UpdateConnectors(a_warehouse, a_sd, a_connectorAutoDelete, a_autoDeleteStorageAreaConnectorsIn, a_autoDeleteStorageAreaConnectorsOut, a_autoDeleteResourceStorageAreaConnectorsIn, a_autoDeleteResourceStorageAreaConnectorsOut, this, a_dataChanges, a_udfManager, a_errList);
    }

    private void ValidatePlantAssociation(WarehouseT.Warehouse a_transmissionWarehouse, PlantManager a_plants, ApplicationExceptionList a_errList)
    {
        for (int pIdx = 0; pIdx < a_transmissionWarehouse.SuppliedPlantsCount; pIdx++)
        {
            string plantExternalId = a_transmissionWarehouse.GetSuppliedPlantExternalIdFromIndex(pIdx);
            if (a_plants.GetByExternalId(plantExternalId) == null)
            {
                a_errList.Add(new PTValidationException("2195", new object[] { plantExternalId, ExternalId }));
            }
        }
    }

    private void UpdatePlantAssociations(WarehouseT.Warehouse a_warehouse, bool a_autoDeletePlantAssociations, PlantManager a_plants, ApplicationExceptionList a_errList)
    {
        for (int p = 0; p < a_plants.Count; p++)
        {
            Plant plant = a_plants[p];
            if (plant.ContainsWarehouse(this))
            {
                if (a_autoDeletePlantAssociations && !a_warehouse.ContainsPlant(plant.ExternalId))
                {
                    plant.RemoveWarehouseAssociation(this);
                }
            }
            else //no association yet
            {
                if (a_warehouse.ContainsPlant(plant.ExternalId))
                {
                    plant.AddWarehouseAssociation(this);
                }
            }
        }

        ValidatePlantAssociation(a_warehouse, a_plants, a_errList);
    }
    #endregion

    #region Cloning
    public Warehouse Clone()
    {
        return (Warehouse)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion

    #region Deleting
    /// <summary>
    /// Should be called before deleting a Warehouse to give the Warehouse a chance to remove any references to other objects.
    /// </summary>
    internal void ValidateDelete(JobManager a_jobs, PlantManager a_plants, PurchaseToStockManager a_purchases, Demand.SalesOrderManager a_salesOrderManager, Demand.TransferOrderManager a_transferOrderManager)
    {
        a_jobs.ValidateWarehouseDelete(this);
        a_purchases.ValidateWarehouseDelete(this);
        m_storageAreas.ValidateDelete(a_jobs, a_purchases, a_transferOrderManager, new StorageAreasDeleteProfile(false, false, false));
        a_salesOrderManager.ValidateDelete(this);
        a_transferOrderManager.ValidateWarehouseDelete(this);
        RemoveAllPlantAssociations(a_plants);
    }

    internal void ValidateItemStorageDelete(ItemStorageDeleteProfile a_itemStorageDeleteProfile)
    {
        foreach (Inventory inventory in Inventories)
        {
            inventory.Lots.ValidateDelete(a_itemStorageDeleteProfile);
        }
    }
    /// <summary>
    /// Remove any inventories for this Item.
    /// </summary>
    /// <param name="item"></param>
    internal void ValidateAndDeleteItems(ScenarioDetail a_sd, ItemDeleteProfile a_items, JobManager a_jobs, PurchaseToStockManager a_purchases, Demand.TransferOrderManager a_transferOrderManager, IScenarioDataChanges a_dataChanges)
    {
        StorageAreas.ValidateItemDelete(a_items);
        Inventories.ValidateAndDeleteItems(a_sd, a_items, a_jobs, a_purchases, a_transferOrderManager, this, a_dataChanges);
    }

    internal void ValidateInventoryDelete(InventoryDeleteProfile a_deleteProfile)
    {
        StorageAreas.ValidateInventoryDelete(a_deleteProfile);
    }

    /// <summary>
    /// Deleting the Warehouse so remove any references.
    /// </summary>
    internal void Deleting(ScenarioDetail a_sd)
    {
        Inventories.Deleting(a_sd);
        a_sd.InventoryTransferRuleManager.DeletingWarehouse(this);
    }

    /// <summary>
    /// Removes the association between all Plants and this Warehouse.
    /// </summary>
    private void RemoveAllPlantAssociations(PlantManager a_plants)
    {
        for (int i = 0; i < a_plants.Count; i++)
        {
            a_plants[i].RemoveWarehouseAssociation(this);
        }
    }
    #endregion

    public override string ToString()
    {
        return string.Format("Warehouse={0}; Inventories={1}".Localize(), ExternalId, Inventories.Count);
    }

    #region PT Import DB
    /// <summary>
    /// Load the Import DataSet with data.
    /// </summary>
    /// <param name="ds"></param>
    /// <param name="itemsTable">MUST BE POPULATED BEFORE CALLING THIS FUNCTION.</param>
    public void PopulateImportDataSet(PtImportDataSet a_ds, PtImportDataSet.ItemsDataTable a_itemsTable, ScenarioDetail a_sd, Hashtable a_plantRowsHash)
    {
        PtImportDataSet.WarehousesRow wRow = a_ds.Warehouses.AddWarehousesRow(
            ExternalId,
            Name,
            Description,
            Notes,
            StorageCapacity,
            UserFields == null ? "" : UserFields.GetUserFieldImportString(),
            AnnualPercentageRate);


        foreach (StorageArea storageArea in m_storageAreas)
        {
            storageArea.PopulateImportDataSet(a_ds, wRow, a_sd);
        }

        //Populate Inventories
        Inventories.PopulateImportDataSet(a_ds, wRow, a_itemsTable, a_ds.Lots);

        foreach (StorageAreaConnector storageAreaConnector in m_storageAreaConnectors)
        {
            storageAreaConnector.PopulateImportDataSet(a_ds, wRow, a_sd);
        }
        //Populate Plants
        List<Plant> plants = a_sd.PlantManager.GetPlantsSuppliedByWarehouse(this);

        for (int i = 0; i < plants.Count; i++)
        {
            Plant plant = plants[i];
            PtImportDataSet.PlantsRow plantRow = (PtImportDataSet.PlantsRow)a_plantRowsHash[plant.ExternalId];
            a_ds.SuppliedPlants.AddSuppliedPlantsRow(wRow, plantRow);
        }
    }
    #endregion

    /// <summary>
    /// This warehouses index in the warehouse TT matrix in WarehouseManager.
    /// It's value is reset during simulation initialization by WarehouseManager.
    /// </summary>
    internal int TTIdx { get; set; }

    /// <summary>
    /// Store material that came from an auto complete or other unconstrained source
    /// </summary>
    /// <param name="a_batchPrimaryResource"></param>
    /// <param name="a_qtyJustProduced"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    //public ItemStorage StoreInventoryWithoutConstraints(Resource a_primaryResource, decimal a_qtyJustProduced)
    //{
    //    if (m_storageAreas.Count == 1)
    //    {
    //        ItemStorage newStorage = new ItemStorage(m_storageAreas.GetByIndex(0), a_qtyJustProduced);
    //        return newStorage;
    //    }

    //    //TODO: determine eligible storage areas based on resource connectors

    //    throw new NotImplementedException();
    //}
}

#region WarehouseArrayList
public class WarehouseArrayList : IEnumerable<Warehouse>
{
    public WarehouseArrayList()
    {
        m_warehouseArrayList = new List<Warehouse>();
    }

    public WarehouseArrayList(int a_capacity)
    {
        m_warehouseArrayList = new List<Warehouse>(a_capacity);
    }

    private readonly List<Warehouse> m_warehouseArrayList;

    public void Add(Warehouse a_warehouse)
    {
        m_warehouseArrayList.Add(a_warehouse);
    }

    internal void Add(WarehouseArrayList a_warehouses)
    {
        for (int i = 0; i < a_warehouses.Count; ++i)
        {
            m_warehouseArrayList.Add(a_warehouses[i]);
        }
    }

    public void Clear()
    {
        m_warehouseArrayList.Clear();
    }

    public int Count => m_warehouseArrayList.Count;

    public Warehouse this[int a_index] => m_warehouseArrayList[a_index];

    public bool Contains(Warehouse warehouse)
    {
        return m_warehouseArrayList.Contains(warehouse);
    }

    public void Remove(Warehouse a_warehouse)
    {
        m_warehouseArrayList.Remove(a_warehouse);
    }

    public void Copy(WarehouseArrayList a_copy)
    {
        Clear();
        m_warehouseArrayList.AddRange(a_copy.m_warehouseArrayList);
    }

    public void Copy(WarehouseManager a_warehouseManager)
    {
        Clear();
        for (int warehouseManagerI = 0; warehouseManagerI < a_warehouseManager.Count; warehouseManagerI++)
        {
            Warehouse warehouse = a_warehouseManager.GetByIndex(warehouseManagerI);
            m_warehouseArrayList.Add(warehouse);
        }
    }

    internal void Union(WarehouseArrayList a_list)
    {
        for (int listIdx = 0; listIdx < a_list.Count; listIdx++)
        {
            Warehouse warehouse = a_list[listIdx];
            if (!m_warehouseArrayList.Contains(warehouse))
            {
                m_warehouseArrayList.Add(warehouse);
            }
        }
    }

    internal void Intersection(WarehouseArrayList a_list)
    {
        for (int warehouseArrayListIdx = 0; warehouseArrayListIdx < m_warehouseArrayList.Count; warehouseArrayListIdx++)
        {
            Warehouse warehouse = m_warehouseArrayList[warehouseArrayListIdx];

            if (!a_list.m_warehouseArrayList.Contains(warehouse))
            {
                m_warehouseArrayList.Remove(warehouse);
            }
        }
    }

    /// <summary>
    /// Sort the warehouses as desired.
    /// </summary>
    /// <param name="a_comparer"></param>
    internal void Sort(Comparison<Warehouse> a_comparer)
    {
        m_warehouseArrayList.Sort(a_comparer);
    }

    public IEnumerator<Warehouse> GetEnumerator()
    {
        for (var whI = 0; whI < m_warehouseArrayList.Count; whI++)
        {
            yield return m_warehouseArrayList[whI];
        }
    }

    public override string ToString()
    {
        return string.Format("Count={0}".Localize(), Count);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
#endregion