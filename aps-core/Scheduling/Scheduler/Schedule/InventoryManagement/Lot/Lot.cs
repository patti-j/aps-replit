
using PT.APSCommon;
using PT.APSCommon.Collections;
using PT.ERPTransmissions;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;

using static PT.SchedulerDefinitions.ItemDefs;

namespace PT.Scheduler;

/// <summary>
/// This class represents inventory material from a single source
/// Any change to inventory quantities in storage areas will add an adjustment here
/// Imported lots will contain all required fields and will not be re-used during the simulation
/// </summary>
public partial class Lot : BaseObject, IMaterialAllocation, ISupplySource
{
    #region Serialization
    internal Lot(IReader a_reader, BaseIdGenerator a_adjustmentIdGenerator)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12511)
        {
            m_bools = new BoolVector32(a_reader);
            m_itemId = new BaseId(a_reader);

            a_reader.Read(out m_productionTicks);

            a_reader.Read(out int eval);
            m_lotSource = (ELotSource)eval;

            m_shelfLifeLotData = new ShelfLifeLotData(a_reader);
            m_wearLotData = new WearLotData(a_reader);

            a_reader.Read(out m_code);

            a_reader.Read(out eval);
            MaterialAllocation = (MaterialAllocation)eval;
            a_reader.Read(out m_minSourceQty);
            a_reader.Read(out m_maxSourceQty);
            a_reader.Read(out eval);
            m_materialSourcing = (MaterialSourcing)eval;

            m_adjustments = new AdjustmentArray(a_reader, a_adjustmentIdGenerator);

            m_storageProfile = new LotStorageProfile(a_reader);
        }
    }

    /// <summary>
    /// For backwards compatibility before ItemId was serialized
    /// </summary>
    /// <param name="a_reader"></param>
    /// <param name="a_itemId"></param>
    internal Lot(IReader a_reader, BaseId a_itemId, BaseIdGenerator a_idGen)
        : base(a_reader)
    {
        m_itemId = a_itemId;
        m_adjustments = new AdjustmentArray(a_idGen);
        m_storageProfile = new LotStorageProfile();

        if (a_reader.VersionNumber >= 731)
        {
            a_reader.Read(out m_obsoleteQty);
            a_reader.Read(out m_productionTicks);

            a_reader.Read(out int eval);
            m_lotSource = (ELotSource)eval;

            a_reader.Read(out int usabilityVal);
            if (usabilityVal == 1 /*ShelfLife*/ || usabilityVal == 4 /*ShelfLifeNonConstraint*/)
            {
                m_shelfLifeLotData = new ShelfLifeLotData(a_reader);
                m_wearLotData = new WearLotData(-1);
            }
            else if (usabilityVal == 2 /*Wear*/)
            {
                m_shelfLifeLotData = new ShelfLifeLotData(PTDateTime.InvalidDateTime.Ticks, PTDateTime.InvalidDateTime.Ticks);
                m_wearLotData = new WearLotData(a_reader);
            }
            else
            {
                m_shelfLifeLotData = new ShelfLifeLotData(PTDateTime.InvalidDateTime.Ticks, PTDateTime.InvalidDateTime.Ticks);
                m_wearLotData = new WearLotData(-1);
            }

            a_reader.Read(out m_code);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out eval);
            MaterialAllocation = (MaterialAllocation)eval;
            a_reader.Read(out m_minSourceQty);
            a_reader.Read(out m_maxSourceQty);
            a_reader.Read(out eval);
            m_materialSourcing = (MaterialSourcing)eval;
        }

        if (m_code == null) // default value used to be null. But this is not allowed.
        {
            m_code = "";
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_itemId.Serialize(a_writer);
        
        a_writer.Write(ProductionTicks);
        a_writer.Write((int)LotSource);

        m_shelfLifeLotData.Serialize(a_writer);
        m_wearLotData.Serialize(a_writer);

        a_writer.Write(m_code);

        a_writer.Write((int)MaterialAllocation);
        a_writer.Write(m_minSourceQty);
        a_writer.Write(m_maxSourceQty);
        a_writer.Write((int)m_materialSourcing);

        m_adjustments.Serialize(a_writer);

        m_storageProfile.Serialize(a_writer);
    }

    //Creates a new dataset row for the ImportDataSet. This sataset is used in the ResourceOperationControl
    public void PopulateImportDataSet(string a_itemExternalId, string a_warehouseExternalId, PtImportDataSet.LotsDataTable lotTable, PtImportDataSet.ItemStorageLotsDataTable a_dsItemStorageLots)
    {
        DateTime expirationDate = PTDateTime.MinDateTime;


        if (m_shelfLifeLotData.Expirable)
        {
            expirationDate = new DateTime(m_shelfLifeLotData.ExpirationTicks);
        }

        PtImportDataSet.LotsRow LotsRow = lotTable.AddLotsRow(ProductionDate.ToDisplayTime().ToDateTime(),
            //Qty,
            a_itemExternalId,
            ExternalId,
            a_warehouseExternalId,
            Code,
            expirationDate.ToDisplayTime().ToDateTime(),
            LimitMatlSrcToEligibleLots,
            UserFields == null ? "" : UserFields.GetUserFieldImportString());

        foreach (LotStorage lotStorage in m_storageProfile)
        {
            a_dsItemStorageLots.AddItemStorageLotsRow(a_itemExternalId, a_warehouseExternalId, lotStorage.StorageArea.ExternalId, LotsRow.ExternalId, lotStorage.Qty);
        }
    }

    public new const int UNIQUE_ID = 744;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public override string DefaultNamePrefix => "Lot";

    /// <summary>
    /// Create a lot with a shelf life.
    /// </summary>
    /// <param name="a_externalId"></param>
    /// <param name="a_baseId"></param>
    /// <param name="a_qty"></param>
    /// <param name="a_productionDateTicks"></param>
    /// <param name="a_expirationTicks"></param>
    internal Lot(string a_externalId, BaseId a_baseId, string a_code, Inventory a_inv, long a_productionDateTicks, ISupplySource a_source, long a_expirationTicks, long a_expirationWarningTicks, int a_wear, BaseIdGenerator a_adjustmentIdGenerator)
        : base(a_baseId, a_externalId)
    {
        Code = a_code;
        if (a_source is Product)
        {
            LotSource = ELotSource.Production;
        }
        else if (a_source is PurchaseToStock)
        {
            LotSource = ELotSource.PurchaseToStock;
        }
        ProductionTicks = a_productionDateTicks;
        m_supplySource = a_source;

        m_inventory = a_inv;
        m_item = a_inv.Item;
        m_adjustments = new AdjustmentArray(a_adjustmentIdGenerator);

        m_shelfLifeLotData = new ShelfLifeLotData(a_expirationTicks, a_expirationWarningTicks);
        m_wearLotData = new WearLotData(a_wear);

        m_storageProfile = new LotStorageProfile();
    }

    /// <summary>
    /// Create a Lot from imported data
    /// </summary>
    internal Lot(BaseId a_newId,
                 WarehouseT.Inventory.Lot a_importedLot, 
                 bool a_autoDeleteItemStorageLots,
                 Inventory a_inventory,
                 TimeSpan a_shelfLife, 
                 TimeSpan a_shelfLifeWarning, 
                 BaseIdGenerator a_adjustmentIdGenerator, 
                 UserFieldDefinitionManager a_udfManager,
                 IScenarioDataChanges a_dataChanges)
        : base(a_newId, a_importedLot.ExternalId)
    {
        m_item = a_inventory.Item;
        m_inventory = a_inventory;
        m_adjustments = new AdjustmentArray(a_adjustmentIdGenerator);
        m_shelfLifeLotData = new ShelfLifeLotData();
        m_wearLotData = new WearLotData();
        m_lotSource = ELotSource.Inventory; //All imported lots are 'inventory' and won't be deleted on sim initialization
        m_storageProfile = new LotStorageProfile();

        Update(a_importedLot, a_autoDeleteItemStorageLots, a_shelfLife, a_shelfLifeWarning, a_udfManager, a_dataChanges);
    }

    /// <summary>
    /// Updates a Lot in the system from one from a transmission that has the same ExternalId
    /// </summary>
    internal void Update(WarehouseT.Inventory.Lot a_importedLot, bool a_autoDeleteItemStorageLots, TimeSpan a_shelfLife, TimeSpan a_shelfLifeWarning, UserFieldDefinitionManager a_udfManager, IScenarioDataChanges a_dataChanges)
    {
        UpdateUserFields(a_importedLot.UserFields, a_udfManager, UserField.EUDFObjectType.Lots);

        if (a_shelfLife > TimeSpan.Zero)
        {
            long expirationDateTicks = a_importedLot.ProductionDate.Add(a_shelfLife).Ticks;

            if (expirationDateTicks > DateTime.MaxValue.Ticks || expirationDateTicks < DateTime.MinValue.Ticks)
            {
                throw new PTValidationException("3039", new object[] { a_importedLot.ExternalId });
            }
            
            long expirationWarningDateTicks = a_importedLot.ProductionDate.Add(a_shelfLifeWarning).Ticks;

            if (expirationWarningDateTicks > DateTime.MaxValue.Ticks || expirationWarningDateTicks < DateTime.MinValue.Ticks)
            {
                throw new PTValidationException("3112", new object[] { a_importedLot.ExternalId });
            }

            m_shelfLifeLotData = new ShelfLifeLotData(expirationDateTicks, expirationWarningDateTicks);
        }

        if (a_importedLot.WearSet)
        {
            m_wearLotData = new WearLotData(a_importedLot.Wear);
        }

        if (a_importedLot.CodeSet)
        {
            Code = a_importedLot.Code;
        }

        switch (a_importedLot.ProductionDateSet)
        {
            case true:
                ProductionDate = a_importedLot.ProductionDate;
                break;
            case false:
                // throw new PTValidationException("2953", new object[] { a_importedLot.ExternalId });
                if (ProductionDate < PTDateTime.MinDateTime)
                {
                    ProductionDate = PTDateTime.MinDateTime;
                }
                break;
        }

        if (a_importedLot.LimitMatlSrcToEligibleLotsSet)
        {
            if (a_importedLot.LimitMatlSrcToEligibleLots)
            {
                if (string.IsNullOrEmpty(Code))
                {
                    throw new PTValidationException("3002", new object[] { a_importedLot.ExternalId });
                }
            }

            LimitMatlSrcToEligibleLots = a_importedLot.LimitMatlSrcToEligibleLots;
        }

        UpdateItemStorageLot(a_importedLot.ItemStorageLots, a_autoDeleteItemStorageLots, a_dataChanges);
    }

    private void UpdateItemStorageLot(HashSet<WarehouseT.ItemStorageLot> a_itemStorageLots, bool a_autoDelete, IScenarioDataChanges a_dataChanges)
    {
        HashSet<ItemStorage> updateLotStorage = new HashSet<ItemStorage>();

        foreach (WarehouseT.ItemStorageLot itemStorageLot in a_itemStorageLots)
        {
            StorageArea storageArea = m_inventory.Warehouse.StorageAreas.GetByExternalId(itemStorageLot.StorageAreaExternalId);

            if (storageArea == null)
            {
                throw new PTValidationException("3084", new object[] { itemStorageLot.StorageAreaExternalId });
            }

            if (itemStorageLot.Qty <= 0)
            {
                throw new PTValidationException("4499", new object[] { itemStorageLot.ExternalId, storageArea.ExternalId, ExternalId });
            }

            ItemStorage itemStorage = storageArea.Storage.GetByItemExternalId(itemStorageLot.ItemExternalId);

            if (itemStorage == null)
            {
                throw new PTValidationException("3107", new object[] { itemStorageLot.StorageAreaExternalId, itemStorageLot.ItemExternalId});
            }

            if (!itemStorage.UnConstrained)
            {
                if (itemStorageLot.Qty > itemStorage.MaxQty)
                {
                    throw new PTValidationException("3086", new object[] { itemStorageLot.ExternalId, itemStorageLot.Qty, itemStorage.MaxQty });
                }

                decimal remainingQty = itemStorage.MaxQty - (itemStorage.QtyProfile?.Qty ?? 0);
                if (m_storageProfile.TryGetValue(itemStorage.Id, out LotStorage lotStorage))
                {
                    remainingQty += lotStorage.Qty;

                }

                if (itemStorageLot.Qty > remainingQty)
                {
                    throw new PTValidationException("3087", new object[] { itemStorageLot.ExternalId, itemStorageLot.Qty, remainingQty });
                }
            }

            LinkOnHandStorage(itemStorage, itemStorageLot.Qty);

            updateLotStorage.Add(itemStorage);
        }

        if (a_autoDelete)
        {
            for (int i = m_storageProfile.Count - 1; i >= 0; i--)
            {
                LotStorage lotStorage = m_storageProfile[i];

                if (!updateLotStorage.Contains(lotStorage.ItemStorage))
                {
                    m_storageProfile.RemoveObject(lotStorage);
                    a_dataChanges.FlagConstraintChanges(Inventory.Id);
                }
            }
        }
    }

    private BaseId m_itemId;

    private Item m_item;
    internal Item Item => m_item;

    private Warehouse m_warehouse;
    internal Warehouse Warehouse => m_warehouse;
    private Inventory m_inventory;
    internal Inventory Inventory => m_inventory;

    /// <summary>
    /// The quantity of material in the lot.
    /// </summary>
    public decimal Qty => m_storageProfile.TotalQty;

    /// <summary>
    /// Add scheduled production to an existing lot. For example auto finish qty
    /// </summary>
    /// <param name="a_quantity"></param>
    internal void ProduceToLot(decimal a_quantity)
    {
    }

    internal void ResetQty(decimal a_newQty)
    {
    }

    /// <summary>
    /// Tracks how much qty on the lot is for partial production.
    /// </summary>
    private decimal m_partialProduction;

    private long m_productionTicks;

    internal long ProductionTicks
    {
        get => m_productionTicks;
        private set => m_productionTicks = value;
    }

    /// <summary>
    /// When the material was produced.
    /// </summary>
    public DateTime ProductionDate
    {
        get => new (m_productionTicks);
        private set => m_productionTicks = value.Ticks;
    }

    private ELotSource m_lotSource;

    public ELotSource LotSource
    {
        get => m_lotSource;
        internal set
        {
            m_lotSource = value;
            if (value != ELotSource.PartialProduction)
            {
                //If we change from PartialProduction, clear the stored value
                m_partialProduction = 0m;
            }
        }
    }

    private ISupplySource m_supplySource;
    /// <summary>
    /// The source of the lot. Either a product, purchase order, or a transfer order.
    /// </summary>
    public ISupplySource Source => m_supplySource;

    /// <summary>
    /// The reason for the lot. Either an Activity, Purchase Order, or a transfer order.
    /// </summary>
    public BaseIdObject Reason
    {
        get
        {
            if (LotSource == ELotSource.PartialProduction)
            {
                //The first adjustment will be inventory, find the op source instead
                for (int i = m_adjustments.Count - 1; i >= 0; i--)
                {
                    Adjustment adjustment = m_adjustments[i];
                    if (adjustment.AdjustmentType is InventoryDefs.EAdjustmentType.ActivityProductionStored 
                        or InventoryDefs.EAdjustmentType.ActivityProductionStoredAndAvailable
                        or InventoryDefs.EAdjustmentType.PurchaseOrderStored
                        or InventoryDefs.EAdjustmentType.PurchaseOrderStoredAndAvailable
                        or InventoryDefs.EAdjustmentType.TransferOrderIn)
                    {
                        return adjustment.GetReason();
                    }
                }
            }

            return m_adjustments[0].GetReason();
        }
    }

    private ShelfLifeLotData m_shelfLifeLotData;

    public ShelfLifeLotData ShelfLifeData
    {
        get => m_shelfLifeLotData;
        private set => m_shelfLifeLotData = value;
    }

    private WearLotData m_wearLotData;

    public WearLotData WearData
    {
        get => m_wearLotData;
        private set => m_wearLotData = value;
    }

    private string m_code = "";

    public string Code
    {
        get => m_code;
        internal set
        {
            if (value != null)
            {
                m_code = value;
            }
        }
    }
    
    private BoolVector32 m_bools;

    private const int c_limitMatlSrcToEligibleLots = 0;

    /// <summary>
    /// If this value is set to true, the material in this lot will be limited 
    //to requirements that specify this lot as an eligible lot.
    //Any material in this lot that's not used to satisfy a material requirement that specified this lot as an eligible source won't be able to be
    //used by any other material requirement; left over material won't be eligible to be used by other material requirements.
    //The default of this value is true.

    //If this value is changed to false, any leftover material (material remaining in the lot after all eligible lot requirements have been fulfilled) becomes generally available
    //to all other material requirements.
    //For example,
    //If Lot A produces 100 units of item X
    //A material requirement for Job 1 is for 25 units of X from eligible lots "A"
    //and a material requriement for Job 2 is is for 25 units of X from eligible lots "A"
    //After both jobs have been fulfilled by Lot A, the unused 50 units become free to be used by
    //any other material requirements.

    //If this value is set to true, the 50 unused units don't become available for other requirements
    //after material requirements for jobs 1 and 2 are satisfied.
    /// </summary>
    public bool LimitMatlSrcToEligibleLots
    {
        get => m_bools[c_limitMatlSrcToEligibleLots];
        private set => m_bools[c_limitMatlSrcToEligibleLots] = value;
    }

    //ISupplySource fields
    public Lot SupplyLot => this;
    public int WearDurability => WearData.WearAmount;
    public string LotCode => Code;

    //Lots can't create other lots
    public void LinkCreatedLot(Lot a_newLot)
    {
        throw new NotImplementedException();
    }

    public long MaterialPostProcessingTicks => 0;
    
    //Lots don't discard
    public Adjustment GenerateDiscardAdjustment(long a_simClock, decimal a_decimal)
    {
        throw new NotImplementedException();
    }

    //************************************************************************************
    // I decided not to add the code data below because it consumes extra memory that
    // the simulation never uses. Any reports that might need this type of information can
    // build it up from the LotAdjustments.
    // 
    // If you need to add it.
    // 1. Go to the LotAdjustment class.
    // 2. Do a references search on the constructor.
    // 3. Call AddLotAdjustment() at each point where LotAdjustment is created.
    //************************************************************************************

    //List<LotAdjustment> m_consumptions = new List<LotAdjustment>();

    ///// <summary>
    ///// This function should be called for each LotAdjustment created for this Lot.
    ///// </summary>
    ///// <param name="a_adj">An adjustment that indicates usage of this Lot.</param>
    //internal void AddLotAdjustment(LotAdjustment a_adj)
    //{
    //    m_consumptions.Add(a_adj);
    //}

    ///// <summary>
    ///// Get enumerator of LotAdjustments that consume material from this lot.
    ///// </summary>
    ///// <returns></returns>
    //public IEnumerator<LotAdjustment> GetAllocationsEnumerator()
    //{
    //    return m_consumptions.GetEnumerator();
    //}
    private MaterialAllocation m_matlAlloc = MaterialAllocation.NotSet;

    public MaterialAllocation MaterialAllocation
    {
        get => m_matlAlloc;
        private set => m_matlAlloc = value;
    }

    /// <summary>
    /// If specified, each source of material must have at least this much quantity available to be used to fulfill this material requirement.
    /// </summary>
    private decimal m_minSourceQty;

    public decimal MinSourceQty
    {
        get => m_minSourceQty;
        private set => m_minSourceQty = value;
    }

    private decimal m_maxSourceQty;

    /// <summary>
    /// If specified, each source of material must have at least this much quantity available to be used to fulfil this material requirement.
    /// </summary>
    public decimal MaxSourceQty
    {
        get => m_maxSourceQty;
        private set => m_maxSourceQty = value;
    }

    private MaterialSourcing m_materialSourcing;

    /// <summary>
    /// You can set this value to control certain aspects of how material is sourced.
    /// </summary>
    public MaterialSourcing MaterialSourcing
    {
        get => m_materialSourcing;
        private set => m_materialSourcing = value;
    }

    public BaseId DemandId => Id;

    internal void IssueMaterial(decimal a_qty)
    {
        //TODO: Find the appropriate storage to issue. Probably needs a new parameter to indicate issuing from which SA
        //Qty -= a_qty;

        m_storageProfile.IssueMaterial(a_qty);
    }

    internal void RestoreReferences(ItemManager a_itemManager, Warehouse a_warehouse, ISystemLogger a_errorReporter, Item a_item, ref Dictionary<BaseId, ItemStorage> a_itemStorageCollection)
    {
        m_warehouse = a_warehouse;
        m_itemId = a_item.Id;
        m_item = a_item;
        m_inventory = a_warehouse.Inventories[a_item.Id];

        m_storageProfile.RestoreReferences(m_inventory, ref a_itemStorageCollection);
    }

    internal void RestoreAdjustmentReferences(ScenarioDetail a_sd)
    {
        m_adjustments.RestoreReferences(a_sd);
    }

    //Sort the data so we don't have to worry about sort timing when accessing the collection first after loading.
    internal void AfterRestoreAdjustmentReferences()
    {
        m_adjustments.Sort();
    }

    /// <summary>
    /// This is every positive and negative change to the lot qty
    /// It will include:
    /// - StorageArea where the adjustment occurred
    /// - Qty change
    /// - Source
    ///
    /// The creation of the lot will be the first positive adjustment
    /// Further positive adjustments could occur from different storage areas or from transfer qty
    /// 
    /// </summary>
    private readonly AdjustmentArray m_adjustments;

    /// <summary>
    /// This is for access outside of this project.
    /// </summary>
    /// <returns></returns>
    public AdjustmentArray GetAdjustmentArray()
    {
        m_adjustments.Sort();
        return m_adjustments;
    }

    public BaseId SupplySourceId => m_supplySource.Id;

    /// <summary>
    /// Restore reference to the object that created this lot
    /// </summary>
    /// <param name="a_source"></param>
    internal void RestoreSource(ISupplySource a_source)
    {
        m_supplySource = a_source;
    }

    private LotStorageProfile m_storageProfile;

    internal void LinkOnHandStorage(ItemStorage a_itemStorage, decimal a_qty)
    {
        m_storageProfile.AddOrUpdate(new LotStorage(a_itemStorage, a_qty));
    }
    
    internal void ValidateDelete(ItemStorageDeleteProfile a_itemStorage)
    {
        for (int i = m_storageProfile.Count - 1; i >= 0; i--)
        {
            LotStorage lotStorage = m_storageProfile[i];
         
            if (a_itemStorage.ContainsItemStorage(lotStorage.ItemStorage.Id))
            {
                a_itemStorage.AddValidationException(lotStorage.ItemStorage, new PTValidationException("4498", new object[]{ lotStorage.ItemStorage.ExternalId, lotStorage.StorageArea.ExternalId, ExternalId}));
            }
        }
    }
    
    public IEnumerable<LotStorage> GetLotStorages()
    {
        foreach (LotStorage lotStorage in m_storageProfile)
        {
            yield return lotStorage;
        }
    }

    /// <summary>
    /// Convert this Lot to Inventory during Auto-Report Progress and update the storage profile.
    /// </summary>
    /// <param name="a_productionDateTicks"></param>
    internal void ConvertToInventory(long a_productionDateTicks)
    {
        if (LotSource != ELotSource.PartialProduction)
        {
            //Only update production ticks on the first production
            m_productionTicks = a_productionDateTicks;
        }

        LotSource = ELotSource.Inventory;
        
        Dictionary<StorageArea, decimal> productionQuantities = new ();
        foreach (Adjustment adj in m_adjustments)
        {
            if (adj.HasLotStorage)
            {
                if (adj.AdjustmentType is InventoryDefs.EAdjustmentType.ActivityProductionStored 
                    or InventoryDefs.EAdjustmentType.ActivityProductionStoredAndAvailable)
                {
                    //we need to store each production adjustment for this lot by storage area so that we 
                    //can update the storage profile and maintain the new on hand lot.
                    StorageArea storageArea = adj.Storage.StorageArea;
                    if (productionQuantities.TryGetValue(storageArea, out decimal prodQty))
                    {
                        productionQuantities[storageArea] += adj.ChangeQty;
                    }
                    else
                    {
                        productionQuantities.Add(storageArea, adj.ChangeQty);
                    }
                }
            }
        }

        foreach ((StorageArea sa, decimal qty) in productionQuantities)
        {
            ItemStorage itemStorage = sa.Storage[Item];
            m_storageProfile.AddOrUpdate(new LotStorage(itemStorage, qty));
        }
    }

    /// <summary>
    /// Stores a part of this lot's quantity into storage and update the storage profile.
    /// </summary>
    /// <param name="a_productionDateTicks"></param>
    /// <param name="a_qtyMovingToInventory"></param>
    internal void StorePartialInInventory(long a_productionDateTicks, decimal a_qtyMovingToInventory)
    {
        //TODO: Partial storage
        if (LotSource != ELotSource.PartialProduction)
        {
            //Only update production ticks on the first production
            m_productionTicks = a_productionDateTicks;
        }
        LotSource = ELotSource.PartialProduction;

        decimal qtyStored = 0m;
        Dictionary<StorageArea, decimal> productionQuantities = new ();
        foreach (Adjustment adj in m_adjustments)
        {
            if (adj.HasLotStorage)
            {
                if (adj.AdjustmentType is InventoryDefs.EAdjustmentType.ActivityProductionStored 
                    or InventoryDefs.EAdjustmentType.ActivityProductionStoredAndAvailable )
                {
                    //we need to store each production adjustment for this lot by storage area so that we 
                    //can update the storage profile and maintain the new on hand lot.
                    StorageArea storageArea = adj.Storage.StorageArea;

                    decimal qtyToStore = adj.ChangeQty; //We try to store the full qty of the adjustment

                    if (qtyToStore + qtyStored > a_qtyMovingToInventory)
                    {
                        //We cannot store the full adjustment
                        qtyToStore = a_qtyMovingToInventory - qtyStored;
                    }

                    qtyStored += qtyToStore;

                    if (productionQuantities.TryGetValue(storageArea, out decimal prodQty))
                    {
                        productionQuantities[storageArea] += qtyToStore;
                    }
                    else
                    {
                        productionQuantities.Add(storageArea, qtyToStore);
                    }

                    if (qtyStored >= a_qtyMovingToInventory)
                    {
                        break;
                    }
                }
            }
        }

        foreach ((StorageArea sa, decimal qty) in productionQuantities)
        {
            ItemStorage itemStorage = sa.Storage[Item];
            m_storageProfile.AddOrUpdate(new LotStorage(itemStorage, qty));
        }
    }

    //Returns quantity of this lot stored in the Storage Area.
    internal decimal GetItemStorageQty(StorageArea a_sa)
    {
        foreach (LotStorage lotStorage in m_storageProfile)
        {
            if (lotStorage.StorageArea == a_sa)
            {
                return lotStorage.Qty;
            }
        }

        return 0m;
    }

    internal void Deleting(IScenarioDataChanges a_dataChanges)
    {
        //not needed for now as we can still flag constraint change and do a time adjustment
        //foreach (Adjustment adjustment in m_adjustments)
        //{
        //    if (adjustment.AdjustmentType == InventoryDefs.EAdjustmentType.ActivityProductionStored 
        //        || adjustment.AdjustmentType == InventoryDefs.EAdjustmentType.PurchaseOrderStored 
        //        || adjustment.AdjustmentType == InventoryDefs.EAdjustmentType.ActivityProductionStoredAndAvailable 
        //        || adjustment.AdjustmentType == InventoryDefs.EAdjustmentType.PurchaseOrderStoredAndAvailable)
        //    {
        //        ItemStorage storage = adjustment.Storage.StorageArea.Storage.GetValue(Inventory.Item.Id);
        //        storage.ClearOnHandLot();
        //        a_dataChanges.FlagConstraintChanges(Inventory.Id);
        //    }
        //}

        a_dataChanges.FlagConstraintChanges(Inventory.Id);
    }

    /// <summary>
    /// Remove reference to the demand that is being deleted
    /// </summary>
    /// <param name="a_demandAdjustment"></param>
    internal void DeleteDemand(Adjustment a_demandAdjustment)
    {
        m_adjustments.RemoveObject(a_demandAdjustment);
    }

    private readonly decimal m_obsoleteQty;
    //For backwards compatibility to store the original Lot Qty into the default ItemStorage
    internal void BackwardsCompatibilityStorePreviousLotQty(ItemStorage a_itemStorage)
    {
        m_storageProfile.Add(new LotStorage(a_itemStorage, m_obsoleteQty));
    }
}

public class LotStorageProfile : CustomSortedDictionary<BaseId, LotStorage>
{
    internal LotStorageProfile() { }

    internal LotStorageProfile(IReader a_reader) : base(a_reader) { }

    public decimal TotalQty => this.Sum(s => s.Qty);

    protected override LotStorage CreateInstance(IReader a_reader)
    {
        return new LotStorage(a_reader);
    }

    internal void IssueMaterial(decimal a_qty)
    {
        decimal remainingQty = a_qty;
        foreach (LotStorage lotStorage in this)
        {
            decimal min = Math.Min(remainingQty, lotStorage.Qty);
            lotStorage.Qty -= min;
            remainingQty -= min;

            if (remainingQty == 0m)
            {
                break;
            }
        }
    }

    internal void RestoreReferences(Inventory a_inventory, ref Dictionary<BaseId, ItemStorage> a_itemStorageCollection)
    {
        if (Count == 0) { return; }

        foreach (LotStorage lotStorage in this)
        {
            lotStorage.RestoreReferences(a_inventory, ref a_itemStorageCollection);
        }
    }

    internal void AddOrUpdate(LotStorage a_lotStorage)
    {
        LotStorage lotStorage = GetValue(a_lotStorage.GetKey());
        if (lotStorage != null)
        {
            lotStorage.SetQty(a_lotStorage);
        }
        else
        {
            Add(a_lotStorage);
        }
    }

    internal void SimulationInitialization(Lot a_lot, long a_clockDate)
    {
        foreach (LotStorage lotStorage in this)
        {
            if (a_lot.ShelfLifeData.Expirable && !a_lot.Item.SaveExpiredMaterial)
            {
                if (a_lot.ShelfLifeData.ExpirationTicks <= a_clockDate)
                {
                    continue; //No need to track already expired material
                }
            }

            lotStorage.SimulationInitialization(a_lot);
        }
    }
}

public class LotStorage : IKey<BaseId>, IPTSerializable, IComparable<LotStorage>
{
    internal LotStorage(ItemStorage a_itemStorage, decimal a_qty)
    {
        ItemStorage = a_itemStorage;
        StorageArea = a_itemStorage.StorageArea;
        Qty = a_qty;
    }

    internal ItemStorage ItemStorage;
    public StorageArea StorageArea;
    public decimal Qty;

    private BaseId m_itemStorageId = BaseId.NULL_ID;
    private BaseId m_storageAreaId = BaseId.NULL_ID;

    internal LotStorage(IReader a_reader)
    {
        m_itemStorageId = new BaseId(a_reader);
        m_storageAreaId = new BaseId(a_reader);
        a_reader.Read(out Qty);
    }

    public bool Equals(BaseId a_other)
    {
        BaseId currentId = m_itemStorageId != BaseId.NULL_ID ? m_itemStorageId : ItemStorage.Id;

        return currentId == a_other.NextId;
    }
    public BaseId GetKey()
    {
        return m_itemStorageId != BaseId.NULL_ID ? m_itemStorageId : ItemStorage.Id;
    }

    public void Serialize(IWriter a_writer)
    {
        ItemStorage.Id.Serialize(a_writer);
        StorageArea.Id.Serialize(a_writer);
        a_writer.Write(Qty);
    }

    public int UniqueId => 1231;

    public int CompareTo(LotStorage a_other)
    {
        BaseId currentId = m_itemStorageId != BaseId.NULL_ID ? m_itemStorageId : ItemStorage.Id;
        BaseId otherId = a_other.m_itemStorageId != BaseId.NULL_ID ? a_other.m_itemStorageId : a_other.ItemStorage.Id;

        return currentId.CompareTo(otherId);
    }

    internal void RestoreReferences(Inventory a_inv, ref Dictionary<BaseId, ItemStorage> a_itemStorageCollection)
    {
        if (a_itemStorageCollection.TryGetValue(m_itemStorageId, out ItemStorage itemStorage))
        {
            ItemStorage = itemStorage;
            StorageArea = ItemStorage.StorageArea;
        }
        else
        {
            //Lookup the ItemStorage and cache it
            StorageArea = a_inv.Warehouse.StorageAreas.GetValue(m_storageAreaId);
            ItemStorage = StorageArea.Storage.GetValue(a_inv.Item.Id);
            a_itemStorageCollection.Add(ItemStorage.Id, ItemStorage);
        }

        m_itemStorageId = BaseId.NULL_ID;
        m_storageAreaId = BaseId.NULL_ID;
    }

    internal void Update(LotStorage a_lotStorage)
    {
        Qty += a_lotStorage.Qty;
    }

    internal void SetQty(LotStorage a_lotStorage)
    {
        Qty = a_lotStorage.Qty;
    }

    internal void SimulationInitialization(Lot a_lot)
    {
        StorageArea.StoreOnHandLot(a_lot, ItemStorage, Qty);
    }
}
