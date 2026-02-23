using PT.APSCommon;
using PT.SchedulerDefinitions;

using PT.Scheduler.Demand;

namespace PT.Scheduler;

public class TransferOrderAdjustment : Adjustment
{
    #region IPTSerializable Members
    internal TransferOrderAdjustment(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12550)
        {
            m_restoreInfo = new RestoreInfo(a_reader);
        }
    }

    public override int UniqueId => UNIQUE_ID;

    public const int UNIQUE_ID = 1231;


    private RestoreInfo m_restoreInfo;

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        RestoreInfo info = new (m_transferOrderDistribution);
        info.Serialize(a_writer);
    }

    private class RestoreInfo
    {
        internal readonly BaseId m_toId;
        internal readonly BaseId m_toDistributionId;

        internal RestoreInfo(IReader a_reader)
        {
            m_toId = new BaseId(a_reader);
            m_toDistributionId = new BaseId(a_reader);
        }

        internal RestoreInfo(TransferOrderDistribution a_tod)
        {
            m_toId = a_tod.TransferOrder.Id;
            m_toDistributionId = a_tod.Id;
        }

        internal void Serialize(IWriter a_writer)
        {
            m_toId.Serialize(a_writer);
            m_toDistributionId.Serialize(a_writer);
        }
    }

    #endregion IPTSerializable Members

    internal TransferOrderAdjustment(Inventory a_inv, long a_time, decimal a_changeQty, StorageAdjustment a_storageAdjustment, TransferOrderDistribution a_tod)
        : base(a_inv, a_time, a_changeQty, a_storageAdjustment)
    {
        m_transferOrderDistribution = a_tod;
        m_type = a_changeQty >= 0 ? InventoryDefs.EAdjustmentType.TransferOrderIn : InventoryDefs.EAdjustmentType.TransferOrderOut;
    }

    private TransferOrderDistribution m_transferOrderDistribution;

    /// <summary>
    /// The storage area this adjustment was made in
    /// </summary>
    public TransferOrderDistribution TransferOrderDist => m_transferOrderDistribution;

    public override BaseIdObject GetReason() => m_transferOrderDistribution;

    internal override void RestoreReferences(ScenarioDetail a_sd)
    {
        base.RestoreReferences(a_sd);

        TransferOrder transferOrder = a_sd.TransferOrderManager.GetById(m_restoreInfo.m_toId);
        m_transferOrderDistribution = transferOrder.Distributions.GetById(m_restoreInfo.m_toDistributionId);
        if (m_type != InventoryDefs.EAdjustmentType.TransferOrderDemand)
        {
            m_transferOrderDistribution.LinkSimAdjustment(this);
        }
        m_restoreInfo = null;
    }
}

public class TransferOrderDiscardAdjustment : TransferOrderAdjustment
{
    #region IPTSerializable Members
    internal TransferOrderDiscardAdjustment(IReader a_reader)
        : base(a_reader)
    {

    }

    public override int UniqueId => UNIQUE_ID;

    public const int UNIQUE_ID = 1233;

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
    }

    #endregion IPTSerializable Members

    internal TransferOrderDiscardAdjustment(Inventory a_inv, long a_time, decimal a_changeQty, TransferOrderDistribution a_tod)
        : base(a_inv, a_time, a_changeQty, null, a_tod)
    {
        m_type = InventoryDefs.EAdjustmentType.Discarding;
    }
}

/// <summary>
/// This adjustment occurs when a material requirement empties a SA below it's disposal qty and the SA is set to dispose immediately
/// </summary>
public class TransferOrderDisposalAdjustment : TransferOrderAdjustment
{
    #region IPTSerializable Members
    internal TransferOrderDisposalAdjustment(IReader a_reader)
        : base(a_reader)
    {
    }

    public override int UniqueId => UNIQUE_ID;

    public const int UNIQUE_ID = 1239;

    #endregion IPTSerializable Members

    internal TransferOrderDisposalAdjustment(Inventory a_inv, long a_time, decimal a_changeQty, StorageAdjustment a_storageAdjustment, TransferOrderDistribution a_distribution)
        : base(a_inv, a_time, a_changeQty, a_storageAdjustment, a_distribution)
    {
        m_type = InventoryDefs.EAdjustmentType.Disposal;
    }
}

/// <summary>
/// Signals a SO demand, but does not mean any material was used by the material yet.
/// </summary>
public class TransferOrderMrpDemandAdjustment : TransferOrderAdjustment
{
    internal TransferOrderMrpDemandAdjustment(TransferOrderDistribution a_dist, Inventory a_inv, decimal a_changeQty)
        : base(a_inv, a_dist.ScheduledShipDateTicks, a_changeQty, null, a_dist)
    {
        m_type = InventoryDefs.EAdjustmentType.TransferOrderDemand;
    }

    public override int UniqueId => UNIQUE_ID;
    public const int UNIQUE_ID = 1240;

    internal TransferOrderMrpDemandAdjustment(IReader a_reader)
        : base(a_reader)
    {
    }

    internal override void RestoreReferences(ScenarioDetail a_sd)
    {
        base.RestoreReferences(a_sd);
        Inventory.AddSimulationAdjustment(this);
    }
}
