using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.ERPTransmissions;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.SchedulerDefinitions;
using PT.SystemDefinitions.Interfaces;

namespace PT.Scheduler;

public partial class LotManager : ScenarioBaseObjectManager<Lot>
{
    #region Serialization
    internal LotManager(IReader a_reader, BaseIdGenerator a_idGenerator)
        : base(a_idGenerator)
    {
        a_reader.Read(out int nbrOfLots);
        for (int lotI = 0; lotI < nbrOfLots; ++lotI)
        {
            Lot lot = new (a_reader, a_idGenerator);

            Add(lot, false);
        }
    }

    //For backwards compatibility before StorageAreas (12511)
    internal LotManager(IReader a_reader, BaseIdGenerator a_idGenerator, BaseId a_itemId)
        : base(a_idGenerator)
    {
        InitFastLookupByExternalId(); //For duplicate ExternalId checking
        a_reader.Read(out int nbrOfLots);
        for (int lotI = 0; lotI < nbrOfLots; ++lotI)
        {
            Lot lot = new (a_reader, a_itemId, a_idGenerator);
            Add(lot, false);
        }
        DeInitFastLookupByExternalId();
    }

    public new const int UNIQUE_ID = 745;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private Lot Add(Lot a_lot, bool a_performLicenseCheck = true)
    {
        if (a_performLicenseCheck && !PTSystem.LicenseKey.IncludeLotControlledPlanning)
        {
            throw new AuthorizationException("Add Lot".Localize(), AuthorizationType.LicenseKey, "IncludeLotControlledPlanning", PTSystem.LicenseKey.IncludeLotControlledPlanning.ToString());
        }

        return base.Add(a_lot);
    }

    internal new void Remove(Lot a_lot)
    {
        base.Remove(a_lot);
    }

    public override Type ElementType => typeof(Lot);

    internal LotManager(BaseIdGenerator a_idGenerator)
        : base(a_idGenerator) { }

    /// <summary>
    /// Create a lot from import
    /// </summary>
    /// <param name="a_importedLot"></param>
    /// <param name="a_usability"></param>
    /// <param name="a_shelfLife"></param>
    /// <param name="a_udfManager"></param>
    /// <returns></returns>
    internal Lot CreateAndAddLot(WarehouseT.Inventory.Lot a_importedLot, bool a_autoDeleteItemStorageLots, Inventory a_inv, TimeSpan a_shelfLife, TimeSpan a_shelfLifeWarning, UserFieldDefinitionManager a_udfManager, IScenarioDataChanges a_dataChanges)
    {
        BaseId newId = NextID();
        Lot lot = new (newId, a_importedLot, a_autoDeleteItemStorageLots, a_inv, a_shelfLife, a_shelfLifeWarning, IdGen, a_udfManager, a_dataChanges);
        return Add(lot);
    }

    internal Lot CreateAndAddLot(string a_lotExternalId, BaseId a_newId, Inventory a_inv, string a_code, long a_productionTicks, ISupplySource a_productionSource, long a_expirationDateTicks, long a_expirationDateWarningTicks, int a_wearDurability)
    {
        //Use an existing lotId unless there is no id from an activity source. In this case create a new Id
        BaseId newId = a_newId != BaseId.NULL_ID ? a_newId : NextID();
        Lot lot = new (a_lotExternalId, newId, a_code, a_inv, a_productionTicks, a_productionSource, a_expirationDateTicks, a_expirationDateWarningTicks, a_wearDurability, IdGen);
        return Add(lot);
    }

    internal Lot GetOnHandLot(Inventory a_inv)
    {
        if (m_onHandLot != null)
        {
            return m_onHandLot;
        }

        //Use an existing lotId unless there is no id from an activity source. In this case create a new Id
        BaseId newId = NextID();
        Lot lot = new ("", newId, string.Empty, a_inv, PTDateTime.MinDateTimeTicks, null, PTDateTime.InvalidDateTime.Ticks, PTDateTime.InvalidDateTime.Ticks, -1, IdGen);
        lot.LotSource = ItemDefs.ELotSource.OnHand;
        m_onHandLot = Add(lot);

        return m_onHandLot;
    }

    public override string ToString()
    {
        return string.Format("{0}: Has {1} lots.".Localize(), base.ToString(), Count);
    }

    public decimal CalcOnHandQty()
    {
        decimal sumLotQty = 0;
        foreach (Lot lot in this)
        {
            if (lot.LotSource == ItemDefs.ELotSource.Inventory)
            {
                sumLotQty += lot.Qty;
            }
        }

        return sumLotQty;
    }

    /// <summary>
    /// Find a lot by the creation source and lot source type.
    /// </summary>
    /// <param name="a_productionSource">THe object that created the lot</param>
    /// <param name="a_lotSource">The lot type</param>
    /// <returns></returns>
    internal Lot GetByProductionSource(ISupplySource a_productionSource, ItemDefs.ELotSource a_lotSource)
    {
        foreach (Lot lot in this)
        {
            if (lot.Source == a_productionSource && lot.LotSource == a_lotSource)
            {
                return lot;
            }
        }

        return null;
    }

    /// <summary>
    /// A Tank Activity was finished, the production lot is being moved to the specified Warehouse Inventory
    /// </summary>
    /// <param name="a_lotProd"></param>
    internal void MoveTankLot(Lot a_lotProd)
    {
        Add(a_lotProd);
    }

    internal void RestoreReferences(ItemManager a_itemManager, Warehouse a_warehouse, ISystemLogger a_errorReporter, Item a_item, ref Dictionary<BaseId, ItemStorage> a_itemStorageCollection)
    {
        foreach (Lot lot in this)
        {
            lot.RestoreReferences(a_itemManager, a_warehouse, a_errorReporter, a_item, ref a_itemStorageCollection);
            if (lot.LotSource == ItemDefs.ELotSource.OnHand)
            {
                m_onHandLot = lot;
            }
        }
    }

    private Lot m_onHandLot;

    internal void SetOnHandQty(decimal a_newQty, Inventory a_inventory)
    {
        if (m_onHandLot == null)
        {
            if (a_newQty <= 0m)
            {
                //No on-hand lot and nothing to set
                return;
            }

            //Generate the new lot to store the on-hand qty
            GetOnHandLot(a_inventory);
        }

        m_onHandLot.ResetQty(a_newQty);
    }

    /// <summary>
    /// </summary>
    /// <param name="a_qtyProfile"></param>
    /// <param name="a_lotCode"></param>
    /// <param name="a_lotSource"></param>
    /// <returns></returns>
    internal Lot AddLot(SupplyProfile a_qtyProfile, string a_lotCode, ILotGeneratorObject a_lotSource)
    {
        BaseId newLotId = a_lotSource.CreateLotId(a_qtyProfile.GetSupplyId(), IdGen);
        decimal qty = a_qtyProfile.CalculateQty();
        if (qty == 0)
        {
            return null;
        }

        long productionTicks = a_qtyProfile.First.Date;

        long expirationTicks = PTDateTime.InvalidDateTime.Ticks;
        long expirationWarningTicks = PTDateTime.InvalidDateTime.Ticks;
        if (a_qtyProfile.Inventory.Item.HasShelfLife)
        {
            expirationTicks = a_qtyProfile.Inventory.Item.CalcExpiration(productionTicks);
            expirationWarningTicks = a_qtyProfile.Inventory.Item.ShelfLifeWarningTicks;
        }

        //TODO: Possibly pass SupplyProfile into this function instead
        Lot lot = CreateAndAddLot("", newLotId, a_qtyProfile.Inventory, a_lotCode, productionTicks, a_qtyProfile.Source, expirationTicks, expirationWarningTicks, a_qtyProfile.Source.WearDurability); // ExternalId will be set by ExternalBaseIdObject

        a_qtyProfile.SetLot(lot);

        return lot;
    }

    public void SubtractOnHandQty(decimal a_qty)
    {
        if (m_onHandLot is Lot onHandLot)
        {
            onHandLot.ResetQty(Math.Max(0, onHandLot.Qty - a_qty));
        }
    }

    internal void ValidateDelete(ItemStorageDeleteProfile a_itemStorageDeleteProfile)
    {
        foreach (Lot lot in this)
        {
            lot.ValidateDelete(a_itemStorageDeleteProfile);
        }
    }

    internal void ClearLots(JobManager a_jobManager, PurchaseToStockManager a_purchaseToStockManager, IScenarioDataChanges a_dataChanges)
    {
        foreach (Lot lot in this)
        {
            //don't 
            //a_jobManager.DeletingLot(this);
            //a_purchaseToStockManager.DeletingLot(this);
            lot.Deleting(a_dataChanges);
        }

        Clear();
    }
}