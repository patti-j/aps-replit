using PT.APSCommon;
using PT.Scheduler.Demand;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

public class ProductionDisposalAdjustment : ActivityAdjustment
{
    #region IPTSerializable Members
    internal ProductionDisposalAdjustment(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12511)
        {
            //m_restoreInfo = new RestoreInfo(a_reader);
        }
    }

    public override int UniqueId => UNIQUE_ID;

    public const int UNIQUE_ID = 1225;

    #endregion IPTSerializable Members

    internal ProductionDisposalAdjustment(Inventory a_inv, long a_time, decimal a_changeQty, StorageAdjustment a_storageAdjustment, InternalActivity a_act)
        : base(a_inv, a_time, a_changeQty, a_storageAdjustment, a_act)
    {
        m_type = InventoryDefs.EAdjustmentType.Disposal;
    }

}

public class PurchaseOrderDisposalAdjustment : PurchaseOrderAdjustment
{
    #region IPTSerializable Members
    internal PurchaseOrderDisposalAdjustment(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12511)
        {
            //m_restoreInfo = new RestoreInfo(a_reader);
        }
    }

    public override int UniqueId => UNIQUE_ID;

    public const int UNIQUE_ID = 1233;

    #endregion IPTSerializable Members

    internal PurchaseOrderDisposalAdjustment(Inventory a_inv, long a_time, decimal a_changeQty, StorageAdjustment a_storageAdjustment, PurchaseToStock a_po)
        : base(a_inv, a_time, a_changeQty, a_storageAdjustment, a_po)
    {
        m_type = InventoryDefs.EAdjustmentType.Disposal;
    }

}

/// <summary>
/// This adjustment occurs when a material requirement empties a SA below it's disposal qty and the SA is set to dispose immediately
/// </summary>
public class MaterialDisposalAdjustment : MaterialRequirementAdjustment
{
    #region IPTSerializable Members
    internal MaterialDisposalAdjustment(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12511)
        {
            //m_restoreInfo = new RestoreInfo(a_reader);
        }
    }

    public override int UniqueId => UNIQUE_ID;

    public const int UNIQUE_ID = 1234;

    #endregion IPTSerializable Members

    internal MaterialDisposalAdjustment(Inventory a_inv, long a_time, decimal a_changeQty, StorageAdjustment a_storageAdjustment, InternalActivity a_act, MaterialRequirement a_mr)
        : base(a_inv, a_time, a_changeQty, a_storageAdjustment, a_act, a_mr, PTDateTime.InvalidDateTimeTicks)
    {
        m_type = InventoryDefs.EAdjustmentType.Disposal;
    }

}

/// <summary>
/// This adjustment occurs when a material requirement empties a SA below it's disposal qty and the SA is set to dispose immediately
/// </summary>
public class SalesOrderDisposalAdjustment : SalesOrderAdjustment
{
    #region IPTSerializable Members
    internal SalesOrderDisposalAdjustment(IReader a_reader)
        : base(a_reader)
    {
    }

    public override int UniqueId => UNIQUE_ID;

    public const int UNIQUE_ID = 1238;

    #endregion IPTSerializable Members

    internal SalesOrderDisposalAdjustment(Inventory a_inv, long a_time, decimal a_changeQty, StorageAdjustment a_storageAdjustment, SalesOrderLineDistribution a_distribution)
        : base(a_distribution, a_inv, a_time, a_changeQty, a_storageAdjustment)
    {
        m_type = InventoryDefs.EAdjustmentType.Disposal;
    }
}

public class ProductionDiscardAdjustment : ActivityAdjustment
{
    #region IPTSerializable Members
    internal ProductionDiscardAdjustment(IReader a_reader)
        : base(a_reader)
    {

    }

    public override int UniqueId => UNIQUE_ID;

    public const int UNIQUE_ID = 1230;

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    #endregion IPTSerializable Members

    internal ProductionDiscardAdjustment(Inventory a_inv, long a_time, decimal a_changeQty, InternalActivity a_act)
        : base(a_inv, a_time, a_changeQty, null, a_act)
    {
        m_type = InventoryDefs.EAdjustmentType.Discarding;
    }
}

public class PurchaseOrderDiscardAdjustment : PurchaseOrderAdjustment
{
    #region IPTSerializable Members
    internal PurchaseOrderDiscardAdjustment(IReader a_reader)
        : base(a_reader)
    {
        
    }

    public override int UniqueId => UNIQUE_ID;

    public const int UNIQUE_ID = 1224;
    
    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    #endregion IPTSerializable Members

    internal PurchaseOrderDiscardAdjustment(Inventory a_inv, long a_time, decimal a_changeQty, PurchaseToStock a_po)
        : base(a_inv, a_time, a_changeQty, null, a_po)
    {
        m_type = InventoryDefs.EAdjustmentType.Discarding;
    }
}

public class PurchaseOrderAdjustment : Adjustment
{
    #region IPTSerializable Members
    internal PurchaseOrderAdjustment(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12511)
        {
            m_restoreInfo = new RestoreInfo(a_reader);
        }
    }

    public override int UniqueId => UNIQUE_ID;

    public const int UNIQUE_ID = 1223;


    private RestoreInfo m_restoreInfo;

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        RestoreInfo info = new(m_purchaseOrder);
        info.Serialize(a_writer);
    }

    private class RestoreInfo
    {
        internal readonly BaseId m_poId;

        internal RestoreInfo(IReader a_reader)
        {
            m_poId = new BaseId(a_reader);
        }

        internal RestoreInfo(PurchaseToStock a_po)
        {
            m_poId = a_po.Id;
        }

        internal void Serialize(IWriter a_writer)
        {
            m_poId.Serialize(a_writer);
        }
    }

    #endregion IPTSerializable Members

    internal PurchaseOrderAdjustment(Inventory a_inv, long a_time, decimal a_changeQty, StorageAdjustment a_storageAdjustment, PurchaseToStock a_po)
        : base(a_inv, a_time, a_changeQty, a_storageAdjustment)
    {
        m_purchaseOrder = a_po;
        m_type = InventoryDefs.EAdjustmentType.PurchaseOrderStoredAndAvailable;
    }

    private PurchaseToStock m_purchaseOrder;

    /// <summary>
    /// The storage area this adjustment was made in
    /// </summary>
    public PurchaseToStock PurchaseOrder => m_purchaseOrder;

    public override BaseIdObject GetReason() => m_purchaseOrder;

    internal override void RestoreReferences(ScenarioDetail a_sd)
    {
        base.RestoreReferences(a_sd);

        m_purchaseOrder = a_sd.PurchaseToStockManager.GetById(m_restoreInfo.m_poId);
        m_restoreInfo = null;
    }

    /// <summary>
    /// Update the type to indicate availability instead of storage
    /// </summary>
    internal void FlagStorageOnly()
    {
        m_type = InventoryDefs.EAdjustmentType.PurchaseOrderStored;
    }

    /// <summary>
    /// Update the type to indicate availability instead of storage
    /// </summary>
    internal void FlagAvailable()
    {
        m_type = InventoryDefs.EAdjustmentType.PurchaseOrderAvailable;
    }
}
