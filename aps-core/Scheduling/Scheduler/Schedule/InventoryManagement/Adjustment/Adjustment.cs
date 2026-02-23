using System.Text;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Scheduler.Demand;

using static PT.SchedulerDefinitions.InventoryDefs;

namespace PT.Scheduler;

public interface IAdjustment
{
    long Time { get; }

    Inventory Inventory { get; }

    decimal ChangeQty { get; }

    EAdjustmentType AdjustmentType { get; }

    DateTime AdjDate => new DateTime(Time);

    string ReasonDescription { get; }

    int ReasonPriority { get; }

    BaseIdObject GetReason();

    decimal Qty { get => Math.Abs(ChangeQty); }
    bool HasStorage => false;
    bool HasLotStorage => false;

    StorageAdjustment? Storage => null;
}

public abstract partial class Adjustment : BaseIdObject, IAdjustment, IComparable<Adjustment>
{
    #region IPTSerializable Members

    internal Adjustment(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12511)
        {
            a_reader.Read(out m_time);
            a_reader.Read(out m_changeQty);
            a_reader.Read(out short adjustmentType);
            m_type = (EAdjustmentType)adjustmentType;

            a_reader.Read(out bool hasStorage);
            if (hasStorage)
            {
                m_storage = new StorageAdjustment(a_reader);
            }

            m_restoreInfo = new RestoreInfo(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(m_time);
        a_writer.Write(m_changeQty);
        a_writer.Write((short)m_type);

        a_writer.Write(HasStorage);
        if (HasStorage)
        {
            m_storage.Serialize(a_writer);
        }

        m_restoreInfo = new RestoreInfo(m_inventory);
        m_restoreInfo.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    public const int UNIQUE_ID = 1221;

    private RestoreInfo m_restoreInfo;

    private class RestoreInfo
    {
        internal readonly BaseId m_itemId;
        internal readonly BaseId m_warehouseId;

        internal RestoreInfo(IReader a_reader)
        {
            m_itemId = new BaseId(a_reader);
            m_warehouseId = new BaseId(a_reader);
        }

        internal RestoreInfo(Inventory a_inv)
        {
            m_itemId = a_inv.Item.Id;
            m_warehouseId = a_inv.Warehouse.Id;
        }

        internal void Serialize(IWriter a_writer)
        {
            m_itemId.Serialize(a_writer);
            m_warehouseId.Serialize(a_writer);
        }
    }

    internal virtual void RestoreReferences(ScenarioDetail a_sd)
    {
        Warehouse warehouse = a_sd.WarehouseManager.GetById(m_restoreInfo.m_warehouseId);
        m_inventory = warehouse.Inventories[m_restoreInfo.m_itemId];
        m_restoreInfo = null;
        m_storage?.RestoreReferences(a_sd, m_inventory);
    }

    #endregion

    public virtual string ReasonDescription => "Inventory".Localize();

    public int CompareTo(Adjustment a_other)
    {
        int dateCompare = AdjDate.CompareTo(a_other.AdjDate);
        if (dateCompare != 0)
        {
            return dateCompare;
        }


        //Sort in the order the Scheduler created the adjustments
        return Id.CompareTo(a_other.Id);
    }

    private StorageAdjustment m_storage;
    private int m_reasonPriority;

    public StorageAdjustment Storage => m_storage;

    /// <summary>
    /// Whether any stored material had already expired
    /// </summary>
    public bool Expired => m_storage != null && m_storage.Lot.ShelfLifeData.Expirable && m_storage.Lot.ShelfLifeData.ExpirationTicks < m_time;

    public decimal Qty => Math.Abs(ChangeQty);

    //internal void AddStorage(Lot a_lot, StorageArea a_storageArea)
    //{
    //    m_storage = new StorageAdjustment(a_lot, a_storageArea);
    //}

    public override string ToString()
    {
        StringBuilder sb = new ();
        sb.AppendFormat("{0}; Qty {1}; Reason {2}".Localize(), Time, ChangeQty, ReasonDescription);
        return sb.ToString();
    }

    public string GetAdjustmentReason()
    {
        StringBuilder sb = new();

        BaseIdObject reason = GetReason();

        BaseOperation baseOp = reason as BaseOperation;
        if (baseOp != null)
        {
            return string.Format("Lead Time for Job '{0}' MO '{1}' Op '{2}'".Localize(), baseOp.ManufacturingOrder.Job.Name, baseOp.ManufacturingOrder.Name, baseOp.Name);
        }

        InternalActivity internalActivity = reason as InternalActivity;
        if (internalActivity != null)
        {
            return string.Format("Job '{0}' MO '{1}' Op '{2}'".Localize(), internalActivity.ManufacturingOrder.Job.Name, internalActivity.ManufacturingOrder.Name, internalActivity.Operation.Name);
        }

        PurchaseToStock pts = reason as PurchaseToStock;

        if (pts != null)
        {
            GetNameOrExternalIdMessage(sb, "PTS".Localize(), pts);
            return sb.ToString();
        }

        Inventory inv = reason as Inventory;
        if (inv != null)
        {
            return "On-Hand Inventory".Localize();
        }

        if (reason is ForecastShipment forecastShipment)
        {
            return string.Format("Forecast Shipment, Name: {0}, Desc: {1}, Customer: {2}, Required Date: {3}, Required Qty: {4}".Localize(),
                forecastShipment.Forecast.Name,
                forecastShipment.Forecast.Description,
                forecastShipment.Forecast.Customer != null ? forecastShipment.Forecast.Customer.ExternalId : "None".Localize(),
                forecastShipment.RequiredDate.ToDisplayTime(),
                forecastShipment.RequiredQty);
        }

        if (reason is SalesOrderLineDistribution dist)
        {
            return string.Format("SalesOrder Distribution, Name: {0}, Desc: {1}, Customer: {2}, Line: {3}, Qty Ordered: {4}, Qty Open to Ship: {5}, Required Available Date: {6}".Localize(),
                dist.SalesOrderLine.SalesOrder.Name,
                dist.SalesOrderLine.SalesOrder.Description,
                dist.SalesOrderLine.SalesOrder.Customer != null ? dist.SalesOrderLine.SalesOrder.Customer.ExternalId : "None".Localize(),
                dist.SalesOrderLine.LineNumber,
                dist.QtyOrdered,
                dist.QtyOpenToShip,
                dist.RequiredAvailableDate.ToDisplayTime());
        }

        if (reason is TransferOrderDistribution toDist)
        {
            return string.Format("TransferOrder Distribution, Name: {0}, Desc: {1}, Warehouse: {2}, Qty Ordered: {3}, Qty Shipped: {4}, Scheduled Ship Date: {5}".Localize(),
                toDist.TransferOrder.Name,
                toDist.TransferOrder.Description,
                toDist.ToWarehouse.Name,
                toDist.QtyOrdered,
                toDist.QtyShipped,
                toDist.ScheduledShipDate.ToDisplayTime());
        }

        //TODO: Storage Area
        //Lot lot = a_o as Lot;
        //if (lot != null)
        //{
        //    if (lot.Usability is WearLotData)
        //    {
        //        return "Lot Worn".Localize();
        //    }

        //    return "Lot Expired".Localize();
        //}

        return "UNKNOWN".Localize();
    }

    protected void GetNameOrExternalIdMessage(StringBuilder a_sB, string a_prefix, BaseObject a_bO)
    {
        string description = a_bO.Name.Trim();

        if (description.Length == 0)
        {
            description = a_bO.ExternalId.Trim();
            a_prefix = string.Format("{0} ExtId".Localize(), a_prefix);
        }

        a_sB.AppendFormat("{0}: {1}", a_prefix, description);
    }

    /// <summary>
    /// Combines another similar adjustment into this one, aggregating the qty
    /// </summary>
    public void Condense(Adjustment a_adjustment)
    {
        m_changeQty += a_adjustment.ChangeQty;
    }

}


