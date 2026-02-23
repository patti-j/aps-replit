using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

public class StorageAdjustment
{
    #region IPTSerializable Members
    internal StorageAdjustment(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12511)
        {
            m_restoreInfo = new RestoreInfo(a_reader);
        }
    }

    public int UniqueId => UNIQUE_ID;
    public const int UNIQUE_ID = 1220;

    private RestoreInfo m_restoreInfo;

    public void Serialize(IWriter a_writer)
    {
        RestoreInfo info = new (m_lot, m_storageArea);
        info.Serialize(a_writer);
    }

    private class RestoreInfo
    {
        internal readonly BaseId m_lotId;
        internal readonly BaseId m_StorageAreaId;

        internal RestoreInfo(IReader a_reader)
        {
            m_lotId = new BaseId(a_reader);
            m_StorageAreaId = new BaseId(a_reader);
        }

        internal RestoreInfo(Lot a_lot, StorageArea a_storageArea)
        {
            m_lotId = a_lot?.Id ?? BaseId.NULL_ID;
            m_StorageAreaId = a_storageArea.Id;
        }

        internal void Serialize(IWriter a_writer)
        {
            m_lotId.Serialize(a_writer);
            m_StorageAreaId.Serialize(a_writer);
        }
    }

    #endregion IPTSerializable Members

    internal StorageAdjustment(Lot a_lot, StorageArea a_storageArea)
    {
        m_lot = a_lot;
        m_storageArea = a_storageArea;
    }

    private StorageArea m_storageArea;

    /// <summary>
    /// The storage area this adjustment was made in
    /// </summary>
    public StorageArea StorageArea => m_storageArea;

    private Lot m_lot;

    /// <summary>
    /// All material for this adjustment is drawn from this Lot.
    /// </summary>
    public Lot Lot => m_lot;

    internal void RestoreReferences(ScenarioDetail a_sd, Inventory a_inv)
    {
        m_storageArea = a_inv.Warehouse.StorageAreas.GetValue(m_restoreInfo.m_StorageAreaId);
        m_lot = a_inv.Lots.GetById(m_restoreInfo.m_lotId);
        m_restoreInfo = null;
    }
}

public class MaterialRequirementAdjustment : ActivityAdjustment
{
    #region IPTSerializable Members
    internal MaterialRequirementAdjustment(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12548)
        {
            a_reader.Read(out m_supplyAvailableDate);
            m_restoreInfo = new RestoreInfo(a_reader);
        }
        else if (a_reader.VersionNumber >= 12511)
        {
            m_restoreInfo = new RestoreInfo(a_reader);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    public const int UNIQUE_ID = 1222;

    private RestoreInfo m_restoreInfo;
    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(m_supplyAvailableDate);
        RestoreInfo info = new (Material);
        info.Serialize(a_writer);
    }

    private class RestoreInfo
    {
        internal readonly BaseId MaterialId;

        internal RestoreInfo(IReader a_reader)
        {
            MaterialId = new BaseId(a_reader);
        }

        internal RestoreInfo(MaterialRequirement a_mr)
        {
            MaterialId = a_mr.Id;
        }

        internal void Serialize(IWriter a_writer)
        {
            MaterialId.Serialize(a_writer);
        }
    }

    #endregion IPTSerializable Members

    internal MaterialRequirementAdjustment(Inventory a_inv, long a_time, decimal a_changeQty, StorageAdjustment a_storageAdjustment, InternalActivity a_activity, MaterialRequirement a_mr, long a_supplyAvailableDate)
        : base(a_inv, a_time, a_changeQty, a_storageAdjustment, a_activity)
    {
        m_mr = a_mr;
        m_type = InventoryDefs.EAdjustmentType.MaterialRequirement;
        m_supplyAvailableDate = a_supplyAvailableDate;
    }

    private MaterialRequirement m_mr;

    public MaterialRequirement Material => m_mr;

    private readonly long m_supplyAvailableDate;

    public long SupplyAvailableDate => m_supplyAvailableDate;

    //public override BaseIdObject GetReason() => m_mr;

    internal override void RestoreReferences(ScenarioDetail a_sd)
    {
        base.RestoreReferences(a_sd);

        InternalActivity act = a_sd.JobManager.FindActivity(Activity.CreateActivityKey());
        m_mr = act.Operation.MaterialRequirements.GetById(m_restoreInfo.MaterialId);
        m_mr.LinkAdjustment(this); //Restore the MR links so they are valid before a simulation
        m_restoreInfo = null;
    }

    /// <summary>
    /// Returns the supply source for the material source if it's from an activity.
    /// </summary>
    /// <returns>Null if the supply source is not an activity</returns>
    public InternalActivity GetSupplyingActivity()
    {
        if (HasLotStorage)
        {
            if (Storage.Lot.Reason is InternalActivity supplyActivity)
            {
                return supplyActivity;
            }
        }

        return null;
    }
}

public class MaterialRequirementLeadTimeAdjustment : MaterialRequirementAdjustment
{
    #region IPTSerializable Members
    internal MaterialRequirementLeadTimeAdjustment(IReader a_reader)
        : base(a_reader)
    {
    }

    public override int UniqueId => UNIQUE_ID;
    public const int UNIQUE_ID = 1228;

    #endregion IPTSerializable Members

    internal MaterialRequirementLeadTimeAdjustment(Inventory a_inv, long a_time, decimal a_changeQty, InternalActivity a_activity, MaterialRequirement a_mr)
        : base(a_inv, a_time, a_changeQty, null, a_activity, a_mr, a_time)
    {
        m_type = InventoryDefs.EAdjustmentType.LeadTime;
    }
}

public class MaterialRequirementShortageAdjustment : MaterialRequirementAdjustment
{
    #region IPTSerializable Members
    internal MaterialRequirementShortageAdjustment(IReader a_reader)
        : base(a_reader)
    {
    }

    public override int UniqueId => UNIQUE_ID;
    public const int UNIQUE_ID = 1229;

    #endregion IPTSerializable Members

    internal MaterialRequirementShortageAdjustment(Inventory a_inv, long a_time, decimal a_changeQty, InternalActivity a_activity, MaterialRequirement a_mr, bool a_pastPlanningHorizon)
        : base(a_inv, a_time, a_changeQty, null, a_activity, a_mr, PTDateTime.InvalidDateTimeTicks)
    {
        m_type = a_pastPlanningHorizon ? InventoryDefs.EAdjustmentType.PastPlanningHorizon : InventoryDefs.EAdjustmentType.Shortage;
    }
}

public class ProductAdjustment : ActivityAdjustment
{
    #region IPTSerializable Members
    internal ProductAdjustment(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12511)
        {
            m_restoreInfo = new RestoreInfo(a_reader);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    public const int UNIQUE_ID = 1227;

    private RestoreInfo m_restoreInfo;
    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        RestoreInfo info = new (Product);
        info.Serialize(a_writer);
    }

    private class RestoreInfo
    {
        internal readonly BaseId ProductId;

        internal RestoreInfo(IReader a_reader)
        {
            ProductId = new BaseId(a_reader);
        }

        internal RestoreInfo(Product a_mr)
        {
            ProductId = a_mr.Id;
        }

        internal void Serialize(IWriter a_writer)
        {
            ProductId.Serialize(a_writer);
        }
    }

    #endregion IPTSerializable Members

    internal ProductAdjustment(Inventory a_inv, long a_time, decimal a_changeQty, StorageAdjustment a_storageAdjustment, InternalActivity a_activity, Product a_product)
        : base(a_inv, a_time, a_changeQty, a_storageAdjustment, a_activity)
    {
        m_product = a_product;
        m_type = InventoryDefs.EAdjustmentType.ActivityProductionStoredAndAvailable;
    }

    private Product m_product;

    public Product Product => m_product;

    //public override BaseIdObject GetReason() => m_product;

    internal override void RestoreReferences(ScenarioDetail a_sd)
    {
        base.RestoreReferences(a_sd);

        InternalActivity act = a_sd.JobManager.FindActivity(Activity.CreateActivityKey());
        m_product = act.Operation.Products.GetById(m_restoreInfo.ProductId);
        m_restoreInfo = null;
    }

    internal void FlagStorageOnly()
    {
        m_type = InventoryDefs.EAdjustmentType.ActivityProductionStored;
    }

    internal void FlagAvailable()
    {
        m_type = InventoryDefs.EAdjustmentType.ActivityProductionAvailable;
    }
}

public class OnHandLotAdjustment : Adjustment
{
    #region IPTSerializable Members
    internal OnHandLotAdjustment(IReader a_reader)
        : base(a_reader)
    {
    }

    public override int UniqueId => UNIQUE_ID;
    public new const int UNIQUE_ID = 1232;

    #endregion IPTSerializable Members

    internal OnHandLotAdjustment(Inventory a_inv, long a_time, decimal a_changeQty, StorageAdjustment a_storageAdjustment)
        : base(a_inv, a_time, a_changeQty, a_storageAdjustment)
    {
        m_type = InventoryDefs.EAdjustmentType.OnHand;
    }

    public override BaseIdObject GetReason() => Storage.Lot; //The lot
}

public class LotExpirationAdjustment : Adjustment
{
    internal LotExpirationAdjustment(Lot a_sourceLot, decimal a_changeQty, bool a_saveExpiredMaterial, StorageAdjustment a_storageAdjustment)
        : base(a_sourceLot.Inventory, a_sourceLot.ShelfLifeData.ExpirationTicks, a_changeQty, a_storageAdjustment)
    {
        SaveExpiredMaterial = a_saveExpiredMaterial;
        m_type = InventoryDefs.EAdjustmentType.Expiration;
    }

    internal LotExpirationAdjustment(IReader a_reader)
        : base(a_reader)
    {
        m_bools = new BoolVector32(a_reader);
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
    }

    public bool SaveExpiredMaterial
    {
        get => m_bools[c_saveExpiredMaterialIdx];
        private set => m_bools[c_saveExpiredMaterialIdx] = value;
    }

    private BoolVector32 m_bools;

    private const short c_saveExpiredMaterialIdx = 0;

    public override int UniqueId => UNIQUE_ID;
    public const int UNIQUE_ID = 1243;

    public override BaseIdObject GetReason() => Storage.Lot; //The lot
}

public class LotExpirationDisposalAdjustment : Adjustment
{
    internal LotExpirationDisposalAdjustment(Lot a_sourceLot, decimal a_changeQty, StorageAdjustment a_storageAdjustment)
        : base(a_sourceLot.Inventory, a_sourceLot.ShelfLifeData.ExpirationTicks, a_changeQty, a_storageAdjustment)
    {
        m_type = InventoryDefs.EAdjustmentType.Disposal;
    }

    internal LotExpirationDisposalAdjustment(IReader a_reader)
        : base(a_reader)
    {
    }
    
    public override int UniqueId => UNIQUE_ID;
    public const int UNIQUE_ID = 1244;
    
    public override BaseIdObject GetReason() => Storage.Lot; //The lot
}

public class StorageAreaCleanoutAdjustment : Adjustment
{
    internal StorageAreaCleanoutAdjustment(Inventory a_inv, long a_cleanoutTime, StorageAdjustment a_storageAdjustment, bool a_start)
        : base(a_inv, a_cleanoutTime, 0m, a_storageAdjustment)
    {
        m_type = a_start ? InventoryDefs.EAdjustmentType.StorageAreaCleanStart : InventoryDefs.EAdjustmentType.StorageAreaCleanEnd;
    }

    internal StorageAreaCleanoutAdjustment(IReader a_reader)
        : base(a_reader)
    {
    }

    public override int UniqueId => UNIQUE_ID;
    public const int UNIQUE_ID = 1246;

    public override BaseIdObject GetReason() => Storage.StorageArea; //The lot
}