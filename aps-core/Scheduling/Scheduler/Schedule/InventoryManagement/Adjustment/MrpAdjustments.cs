using PT.APSCommon;
using PT.Scheduler.Demand;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// An adjustment that is used for MRP and inventory planning. This adjustment is not created by the scheduler nor serialized.
/// </summary>
public class MrpAdjustment : IAdjustment
{
    internal MrpAdjustment(MrpAdjustment a_original)
    {
        m_time = a_original.m_time;
        m_changeQty = a_original.m_changeQty;
        m_inventory = a_original.Inventory;
    }

    internal MrpAdjustment(Inventory a_inv, long a_time, decimal a_changeQty)
    {
        #if DEBUG
        // Adjustments must be one of the following types:
        //if (a_reason is not (InternalActivity or BaseOperation or PurchaseToStock or Inventory or Demand.ForecastShipment or Demand.SalesOrderLineDistribution or Demand.TransferOrderDistribution or Scheduler.Lot))
        //{
        //    throw new DebugException("Invalid Adjustment reason type specified in constructor.");
        //}
        #endif
        m_inventory = a_inv;
        m_time = a_time;
        m_changeQty = a_changeQty;
    }

    protected decimal m_changeQty;

    /// <summary>
    /// The amount of material produced or consumed.
    /// </summary>
    public decimal ChangeQty => m_changeQty;

    private readonly long m_time;

    /// <summary>
    /// The time the material is consumed.
    /// </summary>
    public long Time => m_time;

    /// <summary>
    /// The time the material is consumed.
    /// </summary>
    public DateTime AdjDate => new(m_time);

    public string ReasonDescription => throw new NotImplementedException("Not finished yet");
    public virtual int ReasonPriority => 5; //Default

    protected Inventory m_inventory;

    public Inventory Inventory => m_inventory;

    protected InventoryDefs.EAdjustmentType m_type;

    public InventoryDefs.EAdjustmentType AdjustmentType
    {
        get => m_type;
        set => m_type = value;
    }

    public virtual BaseIdObject GetReason() => m_inventory;
}


/// <summary>
/// A non-scheduler adjustment to create a demand for quantity below safety stock
/// </summary>
public class SafetyStockMrpAdjustment : Adjustment
{
    internal SafetyStockMrpAdjustment(Inventory a_inv, long a_date, decimal a_changeQty)
        : base(a_inv, a_date, a_changeQty)
    {
        m_type = InventoryDefs.EAdjustmentType.SafetyStock;
    }

    internal SafetyStockMrpAdjustment(IReader a_reader)
        : base(a_reader)
    {
    }

    public override int UniqueId => UNIQUE_ID;
    public const int UNIQUE_ID = 1237;
}

public class OnHandInventoryMrpAdjustment : MrpAdjustment
{
    internal OnHandInventoryMrpAdjustment(Inventory a_inv, long a_date, decimal a_changeQty)
        : base(a_inv, a_date, a_changeQty)
    {
        AdjustmentType = InventoryDefs.EAdjustmentType.OnHand;
    }
}

/// <summary>
/// Signals a SO demand, but does not mean any material was used by the material yet.
/// </summary>
public class SalesOrderMrpDemandAdjustment : SalesOrderAdjustment
{
    internal SalesOrderMrpDemandAdjustment(SalesOrderLineDistribution a_dist, Inventory a_inv, decimal a_changeQty)
        : base(a_dist, a_inv, a_dist.RequiredAvailableDateTicks, a_changeQty, null)
    {
        m_type = InventoryDefs.EAdjustmentType.SalesOrderDemand;
    }

    public override int UniqueId => UNIQUE_ID;
    public const int UNIQUE_ID = 1236;

    internal SalesOrderMrpDemandAdjustment(IReader a_reader)
        : base(a_reader)
    {
    }

    internal override void RestoreReferences(ScenarioDetail a_sd)
    {
        base.RestoreReferences(a_sd);
        Inventory.AddSimulationAdjustment(this);
    }
}

/// <summary>
/// Signals a SO demand, but does not mean any material was used by the material yet.
/// </summary>
public class SalesOrderAdjustment : Adjustment
{
    private SalesOrderLineDistribution m_dist;

    internal SalesOrderAdjustment(SalesOrderLineDistribution a_dist, Inventory a_inv, long a_date, decimal a_changeQty, StorageAdjustment a_storage)
        : base(a_inv, a_date, a_changeQty, a_storage)
    {
        m_dist = a_dist;
        m_type = InventoryDefs.EAdjustmentType.SalesOrderDistribution;
    }

    internal SalesOrderAdjustment(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12533)
        {
            m_restoreInfo = new RestoreInfo(a_reader);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    public const int UNIQUE_ID = 1235;

    private RestoreInfo m_restoreInfo;
    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        RestoreInfo info = new(m_dist);
        info.Serialize(a_writer);
    }

    private class RestoreInfo
    {
        internal readonly BaseId SoId;
        internal readonly BaseId LineId;
        internal readonly BaseId DistId;

        internal RestoreInfo(IReader a_reader)
        {
            SoId = new BaseId(a_reader);
            LineId = new BaseId(a_reader);
            DistId = new BaseId(a_reader);
        }

        internal RestoreInfo(SalesOrderLineDistribution a_dist)
        {
            SoId = a_dist.SalesOrderLine.SalesOrder.Id;
            LineId = a_dist.SalesOrderLine.Id;
            DistId = a_dist.Id;
        }

        internal void Serialize(IWriter a_writer)
        {
            SoId.Serialize(a_writer);
            LineId.Serialize(a_writer);
            DistId.Serialize(a_writer);
        }
    }

    internal override void RestoreReferences(ScenarioDetail a_sd)
    {
        base.RestoreReferences(a_sd);

        m_dist = a_sd.SalesOrderManager.FindDistribution(m_restoreInfo.SoId, m_restoreInfo.LineId, m_restoreInfo.DistId);
        if (m_type != InventoryDefs.EAdjustmentType.SalesOrderDemand) //Don't link demand adjustments
        {
            m_dist.LinkSimAdjustment(this);
        }
        m_restoreInfo = null;
    }

    public override int ReasonPriority => m_dist.Priority;

    internal SalesOrderLineDistribution Distribution => m_dist;

    public override BaseIdObject GetReason() => m_dist;
}

/// <summary>
/// Signals a SO demand, but does not mean any material was used by the material yet.
/// </summary>
public class ForecastMrpDemandAdjustment : ForecastAdjustment
{
    internal ForecastMrpDemandAdjustment(ForecastShipment a_shipment, Inventory a_inv, decimal a_changeQty)
        : base(a_shipment, a_inv, a_shipment.RequireDateTicks, a_changeQty)
    {
    }

    public override int UniqueId => UNIQUE_ID;
    public const int UNIQUE_ID = 1242;

    internal ForecastMrpDemandAdjustment(IReader a_reader)
        : base(a_reader)
    {
    }

    internal override void RestoreReferences(ScenarioDetail a_sd)
    {
        base.RestoreReferences(a_sd);
        Inventory.AddSimulationAdjustment(this);
    }
}

/// <summary>
/// Signals a SO demand, but does not mean any material was used by the material yet.
/// </summary>
public class ForecastAdjustment : Adjustment
{
    private ForecastShipment m_shipment;

    internal ForecastAdjustment(ForecastShipment a_shipment, Inventory a_inv, long a_date, decimal a_changeQty)
        : base(a_inv, a_date, a_changeQty, null)
    {
        m_shipment = a_shipment;
        m_type = InventoryDefs.EAdjustmentType.ForecastDemand;
    }

    internal ForecastAdjustment(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12533)
        {
            m_restoreInfo = new RestoreInfo(a_reader);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    public const int UNIQUE_ID = 1241;

    private RestoreInfo m_restoreInfo;
    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        RestoreInfo info = new(m_shipment);
        info.Serialize(a_writer);
    }

    private class RestoreInfo
    {
        internal readonly BaseId VersionId;
        internal readonly BaseId ForecastId;
        internal readonly BaseId ShipmentId;

        internal RestoreInfo(IReader a_reader)
        {
            VersionId = new BaseId(a_reader);
            ForecastId = new BaseId(a_reader);
            ShipmentId = new BaseId(a_reader);
        }

        internal RestoreInfo(ForecastShipment a_shipment)
        {
            VersionId = a_shipment.Forecast.ForecastVersion.Id;
            ForecastId = a_shipment.Forecast.Id;
            ShipmentId = a_shipment.Id;
        }

        internal void Serialize(IWriter a_writer)
        {
            VersionId.Serialize(a_writer);
            ForecastId.Serialize(a_writer);
            ShipmentId.Serialize(a_writer);
        }
    }

    internal override void RestoreReferences(ScenarioDetail a_sd)
    {
        base.RestoreReferences(a_sd);

        ForecastVersion version = Inventory.ForecastVersions.Find(m_restoreInfo.VersionId);
        Forecast forecast = version.GetById(m_restoreInfo.ForecastId);
        m_shipment = forecast[m_restoreInfo.ShipmentId];
        m_restoreInfo = null;
    }

    public override int ReasonPriority => m_shipment.Forecast.Priority;

    internal ForecastShipment Shipment => m_shipment;

    public override BaseIdObject GetReason() => m_shipment;
}
