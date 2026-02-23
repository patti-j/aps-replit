using PT.APSCommon;
using PT.APSCommon.Collections;
using PT.Common.Localization;
using PT.Database;
using PT.Scheduler.MRP;
using PT.SchedulerDefinitions;
using PT.Transmissions;
using System.ComponentModel;

namespace PT.Scheduler.Demand;

/// <summary>
/// Base class for all Demand class.
/// </summary>
public abstract class BaseDemand : BaseIdObject, IPTSerializable
{
    public new static readonly int UNIQUE_ID = 730;

    #region IPTSerializable Members
    public BaseDemand(IReader reader)
        : base(reader)
    {
        reader.Read(out m_bomLevel);
        reader.Read(out m_notes);
        reader.Read(out m_requiredDate);
        reader.Read(out m_requiredQty);
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_bomLevel);
        writer.Write(m_notes);
        writer.Write(m_requiredDate);
        writer.Write(m_requiredQty);
    }
    #endregion Serialization

    /// <summary>
    /// Used for comparer of derived classes. Do not use to instantiate a new object for other purposes.
    /// </summary>
    internal BaseDemand() { }

    internal BaseDemand(decimal a_requiredQty, DateTime a_requiredDate, int a_bomLevel)
    {
        m_requiredQty = a_requiredQty;
        m_requiredDate = a_requiredDate;
        m_bomLevel = a_bomLevel;
    }

    private decimal m_requiredQty;

    /// <summary>
    /// The portion of the demand that this supply was created to cover.
    /// This is set when the supply is first created.
    /// </summary>
    public decimal RequiredQty
    {
        get => m_requiredQty;
        internal set => m_requiredQty = value;
    }

    private DateTime m_requiredDate;

    /// <summary>
    /// The need date of the demand that this supply was created to cover.
    /// This is set when the supply is first created.
    /// </summary>
    public DateTime RequiredDate
    {
        get => m_requiredDate;
        internal set => m_requiredDate = value;
    }

    private int m_bomLevel;

    /// <summary>
    /// The level at which the demand occurs relative to the Product.
    /// </summary>
    public int BOMLevel
    {
        get => m_bomLevel;
        internal set => m_bomLevel = value;
    }

    private string m_notes;

    public string Notes
    {
        get => m_notes;
        internal set => m_notes = value;
    }
}

public class SalesOrderDemand : BaseDemand, IPTSerializable
{
    public new const int UNIQUE_ID = 731;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    #region IPTSerializable Members
    public SalesOrderDemand(IReader reader)
        : base(reader)
    {
        BaseId temp_soI = new (reader);
        BaseId temp_lineId = new (reader);
        BaseId temp_distId = new (reader);
        m_soDistSerializationInfo = new SerializationInfo(temp_soI, temp_lineId, temp_distId);
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_salesOrderLineDistribution.SalesOrderLine.SalesOrder.Id.Serialize(writer);
        m_salesOrderLineDistribution.SalesOrderLine.Id.Serialize(writer);
        m_salesOrderLineDistribution.Id.Serialize(writer);
    }

    internal void RestoreReference(SalesOrderManager a_soManager)
    {
        SalesOrder salesOrder = a_soManager.GetById(m_soDistSerializationInfo.SalesOrderId);
        SalesOrderLine line = salesOrder.FindSalesOrderLine(m_soDistSerializationInfo.SalesOrderLineId);
        SalesOrderLineDistribution dist = line.FindDistribution(m_soDistSerializationInfo.DistributionId);
        m_salesOrderLineDistribution = dist;
        m_soDistSerializationInfo = null;
    }

    private SerializationInfo m_soDistSerializationInfo;

    internal SerializationInfo SoDistSerializationInfo
    {
        get => m_soDistSerializationInfo;
        set => m_soDistSerializationInfo = value;
    }

    internal class SerializationInfo
    {
        internal SerializationInfo(BaseId a_soId, BaseId a_lineId, BaseId a_distId)
        {
            SalesOrderId = a_soId;
            SalesOrderLineId = a_lineId;
            DistributionId = a_distId;
        }

        internal BaseId SalesOrderId;
        internal BaseId SalesOrderLineId;
        internal BaseId DistributionId;
    }
    #endregion Serialization

    internal SalesOrderDemand(decimal a_requiredQty, DateTime a_requiredDate, SalesOrderLineDistribution a_distribution, int a_bomLevel)
        : base(a_requiredQty, a_requiredDate, a_bomLevel)
    {
        m_salesOrderLineDistribution = a_distribution;
    }

    internal SalesOrderDemand(SalesOrderLineDistribution a_distribution)
    {
        m_salesOrderLineDistribution = a_distribution;
    }
    
    private SalesOrderLineDistribution m_salesOrderLineDistribution;

    public SalesOrderLineDistribution SalesOrderLineDistribution => m_salesOrderLineDistribution;
}

public class ForecastDemand : BaseDemand, IPTSerializable
{
    public new const int UNIQUE_ID = 732;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    #region IPTSerializable Members
    public ForecastDemand(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 397)
        {
            BaseId tempFcastId = new (a_reader);
            BaseId tempShipId = new (a_reader);
            BaseId tempItemId = new (a_reader);
            BaseId tempWarehouseId = new (a_reader);

            m_serializationInfo = new SerializationInfo(tempFcastId, tempShipId, tempItemId, tempWarehouseId);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_forecastShipment.Forecast.Id.Serialize(a_writer);
        m_forecastShipment.Id.Serialize(a_writer);
        m_inventory.Item.Id.Serialize(a_writer);
        m_inventory.Warehouse.Id.Serialize(a_writer);
    }

    internal SerializationInfo m_serializationInfo;

    internal class SerializationInfo
    {
        internal SerializationInfo(BaseId a_forecastId, BaseId a_shipmentId, BaseId a_itemId, BaseId a_warehouseId)
        {
            ForecastId = a_forecastId;
            ShipmentId = a_shipmentId;
            ItemId = a_itemId;
            WarehouseId = a_warehouseId;
        }

        internal BaseId ForecastId;
        internal BaseId ShipmentId;
        internal BaseId ItemId;
        internal BaseId WarehouseId;
    }

    internal void RestoreReference(WarehouseManager a_warehouseManager, Inventory inv)
    {
        if (m_serializationInfo.WarehouseId != BaseId.NULL_ID) //Once GUA upgrades past version 397 we can remove this passing of Inventory and rely on the WarehouseId and ItemId.
        {
            Warehouse warehouse = a_warehouseManager.GetById(m_serializationInfo.WarehouseId);
            inv = warehouse.Inventories[m_serializationInfo.ItemId];
        }

        //We don't store a reference from Forecast back to ForecastVersion so for now just iterate the versions; Versions not in use yet.
        m_inventory = inv;

        for (int vI = 0; vI < inv.ForecastVersions.Versions.Count; vI++)
        {
            ForecastVersion forecastVersion = inv.ForecastVersions.Versions[vI];
            for (int fI = 0; fI < forecastVersion.Forecasts.Count; fI++)
            {
                Forecast forecast = forecastVersion.Forecasts[fI];
                if (forecast.Id.CompareTo(m_serializationInfo.ForecastId) == 0)
                {
                    m_forecastShipment = forecast[m_serializationInfo.ShipmentId];
                    m_serializationInfo = null;
                    return;
                }
            }
        }
    }
    #endregion Serialization

    internal ForecastDemand(decimal a_requiredQty, DateTime a_requiredDate, ForecastShipment a_forecastShipment, Inventory aInv, int a_bomLevel)
        : base(a_requiredQty, a_requiredDate, a_bomLevel)
    {
        m_forecastShipment = a_forecastShipment;
        m_inventory = aInv;
    }

    internal ForecastDemand(ForecastShipment a_forecastShipment)
    {
        m_forecastShipment = a_forecastShipment;
    }

    private ForecastShipment m_forecastShipment;

    public ForecastShipment ForecastShipment => m_forecastShipment;

    private Inventory m_inventory;

    internal Inventory Inventory => m_inventory;
}

public class SafetyStockDemand : BaseDemand, IPTSerializable
{
    public new const int UNIQUE_ID = 733;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    #region IPTSerializable Members
    public SafetyStockDemand(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 399)
        {
            reader.Read(out m_projectedLevel);

            BaseId tempWarehouseId = new (reader);
            BaseId tempItemId = new (reader);
            BaseId inventoryId = new (reader);
            m_serializationInfo = new SerializationInfo(tempWarehouseId, tempItemId, inventoryId);
        }
        else
        {
            reader.Read(out m_projectedLevel);

            BaseId tempWarehouseId = new (reader);
            BaseId tempItemId = new (reader);
            m_serializationInfo = new SerializationInfo(tempWarehouseId, tempItemId, BaseId.NULL_ID);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_projectedLevel);

        m_inventory.Warehouse.Id.Serialize(writer);
        m_inventory.Item.Id.Serialize(writer);
        m_inventory.Id.Serialize(writer);
    }

    internal void RestoreReference(WarehouseManager a_warehouseManager)
    {
        Warehouse warehouse = a_warehouseManager.GetById(m_serializationInfo.WarehouseId);
        m_inventory = warehouse.Inventories[m_serializationInfo.ItemId];
        m_serializationInfo = null;
    }

    internal SerializationInfo m_serializationInfo;

    internal class SerializationInfo
    {
        internal SerializationInfo(BaseId a_warehouseId, BaseId a_itemId, BaseId a_inventoryId)
        {
            WarehouseId = a_warehouseId;
            ItemId = a_itemId;
            InventoryId = a_inventoryId;
        }

        internal BaseId WarehouseId;
        internal BaseId ItemId;
        internal BaseId InventoryId;
    }
    #endregion Serialization

    internal SafetyStockDemand(decimal a_requiredQty, DateTime a_requiredDate, Inventory a_inventory, decimal a_projectedLevel, int a_bomLevel)
        : base(a_requiredQty, a_requiredDate, a_bomLevel)
    {
        m_inventory = a_inventory;
        m_projectedLevel = a_projectedLevel;
    }

    internal SafetyStockDemand(Inventory a_inv)
    {
        m_inventory = a_inv;
    }

    private Inventory m_inventory;

    public Inventory Inventory => m_inventory;

    private readonly decimal m_projectedLevel;

    public decimal ProjectedLevel => m_projectedLevel;
}

public class TransferOrderDemand : BaseDemand, IPTSerializable
{
    public new const int UNIQUE_ID = 734;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    #region IPTSerializable Members
    public TransferOrderDemand(IReader reader)
        : base(reader)
    {
        BaseId tempTransferOrderId = new (reader);
        BaseId tempDistributionId = new (reader);
        m_serializationInfo = new SerializationInfo(tempTransferOrderId, tempDistributionId);
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_distribution.TransferOrder.Id.Serialize(writer);
        m_distribution.Id.Serialize(writer);
    }

    internal void RestoreReference(TransferOrderManager a_transferOrderManager)
    {
        TransferOrder transferOrder = a_transferOrderManager.GetById(m_serializationInfo.TransferOrderId);
        m_distribution = transferOrder.Distributions.GetById(m_serializationInfo.DistributionId);
        m_serializationInfo = null;
    }

    internal SerializationInfo m_serializationInfo;

    internal class SerializationInfo
    {
        internal SerializationInfo(BaseId a_transferOrderId, BaseId a_distributionId)
        {
            TransferOrderId = a_transferOrderId;
            DistributionId = a_distributionId;
        }

        internal BaseId TransferOrderId;
        internal BaseId DistributionId;
    }
    #endregion Serialization

    internal TransferOrderDemand(decimal a_requiredQty, DateTime a_requiredDate, TransferOrderDistribution a_distribution, int a_bomLevel)
        : base(a_requiredQty, a_requiredDate, a_bomLevel)
    {
        m_distribution = a_distribution;
    }

    internal TransferOrderDemand(TransferOrderDistribution a_dist)
    {
        m_distribution = a_dist;
    }

    private TransferOrderDistribution m_distribution;

    public TransferOrderDistribution Distribution => m_distribution;
}

/// <summary>
/// Stores information about a Demand that has been deleted.
/// </summary>
public class DeletedDemand : BaseDemand, IPTSerializable
{
    public new const int UNIQUE_ID = 735;

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;

    #region IPTSerializable Members
    public DeletedDemand(IReader reader)
        : base(reader)
    {
        reader.Read(out m_description);
        reader.Read(out m_dateDeleted);
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_description);
        writer.Write(m_dateDeleted);
    }
    #endregion Serialization

    internal DeletedDemand(decimal a_requiredQty, DateTime a_requiredDate, string a_description, DateTime a_dateDeleted, int a_bomLevel)
        : base(a_requiredQty, a_requiredDate, a_bomLevel)
    {
        m_description = a_description;
        m_dateDeleted = a_dateDeleted;
    }

    /// <summary>
    /// This is used for maintaining CustomOrderedListOptimized, don't use to instantiate an instance for other use.
    /// </summary>
    internal DeletedDemand() { }

    private readonly string m_description;

    /// <summary>
    /// Identifies the original demand.
    /// </summary>
    public string Description => m_description;

    private readonly DateTime m_dateDeleted;

    public DateTime DateDeleted => m_dateDeleted;
}

/// <summary>
/// Contains lists of different types of demands that a supply can be created to cover.
/// </summary>
public class DemandCollection : IPTSerializable
{
    public const int UNIQUE_ID = 729;

    [Browsable(false)]
    public int UniqueId => UNIQUE_ID;

    #region IPTSerializable Members
    public DemandCollection(IReader reader)
    {
        if (reader.VersionNumber >= 410)
        {
            bool salesOrderDExists;
            reader.Read(out salesOrderDExists);
            if (salesOrderDExists)
            {
                m_salesOrderDemands = new SalesOrderDemandList(reader);
            }

            bool forecastDExists;
            reader.Read(out forecastDExists);
            if (forecastDExists)
            {
                m_forecastDemands = new ForecastDemandList(reader);
            }

            bool transferDExists;
            reader.Read(out transferDExists);
            if (transferDExists)
            {
                m_transferDemands = new TransferOrderDemandList(reader);
            }

            bool safetyDExists;
            reader.Read(out safetyDExists);
            if (safetyDExists)
            {
                m_safetyStockDemands = new SafetyStockDemandList(reader);
            }

            bool deletedDExists;
            reader.Read(out deletedDExists);
            if (deletedDExists)
            {
                m_deletedDemands = new DeletedDemandList(reader);
            }
        }

        #region < 404
        else
        {
            int tempSalesOrderDemands;
            reader.Read(out tempSalesOrderDemands);
            if (tempSalesOrderDemands > 0)
            {
                m_salesOrderDemands = new SalesOrderDemandList();
                for (int i = 0; i < tempSalesOrderDemands; i++)
                {
                    SalesOrderDemand sod = new (reader);
                    SalesOrderDemands.Add(sod);
                }
            }

            int tempForecastDemands;
            reader.Read(out tempForecastDemands);
            if (tempForecastDemands > 0)
            {
                m_forecastDemands = new ForecastDemandList();
                for (int i = 0; i < tempForecastDemands; i++)
                {
                    ForecastDemand fd = new (reader);
                    AddForecastDemands(fd);
                }
            }

            int tempTransferOrderDemands;
            reader.Read(out tempTransferOrderDemands);
            if (tempTransferOrderDemands > 0)
            {
                m_transferDemands = new TransferOrderDemandList();
                for (int i = 0; i < tempTransferOrderDemands; i++)
                {
                    TransferOrderDemand td = new (reader);
                    AddTransferOrderDemands(td);
                }
            }

            int tempSafetyStockDemands;
            reader.Read(out tempSafetyStockDemands);
            if (tempSafetyStockDemands > 0)
            {
                m_safetyStockDemands = new SafetyStockDemandList();
                for (int i = 0; i < tempSafetyStockDemands; i++)
                {
                    SafetyStockDemand st = new (reader);
                    AddSafetyStockDemands(st);
                }
            }

            int tempDeletedDemands;
            reader.Read(out tempDeletedDemands);
            if (tempDeletedDemands > 0)
            {
                m_deletedDemands = new DeletedDemandList();
                for (int i = 0; i < tempDeletedDemands; i++)
                {
                    DeletedDemands.Add(new DeletedDemand(reader));
                }
            }
        }
        #endregion
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(m_salesOrderDemands != null);
        if (m_salesOrderDemands != null)
        {
            m_salesOrderDemands.Serialize(writer);
        }

        writer.Write(m_forecastDemands != null);
        if (m_forecastDemands != null)
        {
            m_forecastDemands.Serialize(writer);
        }

        writer.Write(m_transferDemands != null);
        if (m_transferDemands != null)
        {
            m_transferDemands.Serialize(writer);
        }

        writer.Write(m_safetyStockDemands != null);
        if (SafetyStockDemands != null)
        {
            m_safetyStockDemands.Serialize(writer);
        }

        writer.Write(m_deletedDemands != null);
        if (m_deletedDemands != null)
        {
            m_deletedDemands.Serialize(writer);
        }
    }

    internal void RestoreReferences(SalesOrderManager a_salesOrderManager, Inventory a_inventory, TransferOrderManager a_transferOrderManager, WarehouseManager a_warehousesManager)
    {
        if (SalesOrderDemands != null)
        {
            IEnumerator<SalesOrderDemand> etr = SalesOrderDemands.GetUnsortedEnumerator();
            while (etr.MoveNext())
            {
                etr.Current.RestoreReference(a_salesOrderManager);
            }
        }

        if (ForecastDemands != null)
        {
            IEnumerator<ForecastDemand> etr = ForecastDemands.GetUnsortedEnumerator();
            while (etr.MoveNext())
            {
                etr.Current.RestoreReference(a_warehousesManager, a_inventory);
            }
        }

        if (TransferOrderDemands != null)
        {
            IEnumerator<TransferOrderDemand> etr = m_transferDemands.GetUnsortedEnumerator();
            while (etr.MoveNext())
            {
                etr.Current.RestoreReference(a_transferOrderManager);
            }
        }

        if (SafetyStockDemands != null)
        {
            IEnumerator<SafetyStockDemand> etr = m_safetyStockDemands.GetUnsortedEnumerator();
            while (etr.MoveNext())
            {
                etr.Current.RestoreReference(a_warehousesManager);
            }
        }
        //No references for Deleted Demands.
    }
    #endregion Serialization

    public DemandCollection() { }

    #region SalesOrderDemand list
    public class SalesOrderDemandList : CustomSortedList<SalesOrderDemand>
    {
        public SalesOrderDemandList()
            : base(new SalesOrderDemandComparer()) { }

        public SalesOrderDemandList(IReader a_reader)
            : base(a_reader, new SalesOrderDemandComparer()) { }

        protected override SalesOrderDemand CreateInstance(IReader a_reader)
        {
            return new SalesOrderDemand(a_reader);
        }
    }

    private SalesOrderDemandList m_salesOrderDemands;

    public SalesOrderDemandList SalesOrderDemands => m_salesOrderDemands;

    /// <summary>
    /// return null if SalesOrderDemand isn't found.
    /// </summary>
    /// <param name="a_salesOrderDemandId"></param>
    /// <returns></returns>
    private SalesOrderDemand FindSODemands(BaseId a_salesOrderLineDistributionId)
    {
        return m_salesOrderDemands.GetValue(a_salesOrderLineDistributionId);
    }

    internal void AddSODemands(SalesOrderDemand a_sod)
    {
        m_salesOrderDemands.Add(a_sod);
    }

    private void RemoveSODemands(SalesOrderDemand a_sod)
    {
        m_salesOrderDemands.RemoveObject(a_sod);
    }

    public class SalesOrderDemandComparer : IKeyObjectComparer<SalesOrderDemand>
    {
        public int Compare(SalesOrderDemand a_x, SalesOrderDemand a_y)
        {
            return CompareSalesOrderDemand(a_x, a_y);
        }

        internal static int CompareSalesOrderDemand(SalesOrderDemand a_SoDemand, SalesOrderDemand a_anotherSoDemand)
        {
            return Comparison.Compare(a_SoDemand.SalesOrderLineDistribution.Id.Value, a_anotherSoDemand.SalesOrderLineDistribution.Id.Value);
        }

        public object GetKey(SalesOrderDemand a_sod)
        {
            return a_sod.SalesOrderLineDistribution == null ? a_sod.SoDistSerializationInfo.DistributionId : a_sod.SalesOrderLineDistribution.Id;
        }
    }
    #endregion

    #region ForecastDemand list
    public class ForecastDemandList : CustomSortedList<ForecastDemand>
    {
        public ForecastDemandList()
            : base(new ForecastDemandComparer()) { }

        public ForecastDemandList(IReader a_reader)
            : base(a_reader, new ForecastDemandComparer()) { }

        protected override ForecastDemand CreateInstance(IReader a_reader)
        {
            return new ForecastDemand(a_reader);
        }
    }

    private ForecastDemandList m_forecastDemands;

    public ForecastDemandList ForecastDemands => m_forecastDemands;

    /// <summary>
    /// return null if SalesOrderDemand isn't found.
    /// </summary>
    /// <param name="a_salesOrderDemandId"></param>
    /// <returns></returns>
    private ForecastDemand FindForecastDemands(BaseId a_shipmentId)
    {
        return m_forecastDemands.GetValue(a_shipmentId);
    }

    internal void AddForecastDemands(ForecastDemand a_forecastDemand)
    {
        m_forecastDemands.Add(a_forecastDemand);
    }

    private void RemoveForecastDemands(ForecastDemand a_forecastDemand)
    {
        m_forecastDemands.RemoveObject(a_forecastDemand);
    }

    public class ForecastDemandComparer : IKeyObjectComparer<ForecastDemand>
    {
        public int Compare(ForecastDemand a_x, ForecastDemand a_y)
        {
            return CompareForecastDemand(a_x, a_y);
        }

        internal static int CompareForecastDemand(ForecastDemand a_forecastDemand, ForecastDemand a_anotherForecastDemand)
        {
            return Comparison.Compare(a_forecastDemand.ForecastShipment.Id.Value, a_anotherForecastDemand.ForecastShipment.Id.Value);
        }

        public object GetKey(ForecastDemand a_fd)
        {
            return a_fd.ForecastShipment == null ? a_fd.m_serializationInfo.ShipmentId : a_fd.ForecastShipment.Id;
        }
    }
    #endregion

    #region TransferDemands list
    public class TransferOrderDemandList : CustomSortedList<TransferOrderDemand>
    {
        public TransferOrderDemandList()
            : base(new TransferOrderDemandComparer()) { }

        public TransferOrderDemandList(IReader a_reader)
            : base(a_reader, new TransferOrderDemandComparer()) { }

        protected override TransferOrderDemand CreateInstance(IReader a_reader)
        {
            return new TransferOrderDemand(a_reader);
        }
    }

    private TransferOrderDemandList m_transferDemands;

    public TransferOrderDemandList TransferOrderDemands => m_transferDemands;

    /// <summary>
    /// return null if SalesOrderDemand isn't found.
    /// </summary>
    /// <param name="a_salesOrderDemandId"></param>
    /// <returns></returns>
    private TransferOrderDemand FindTransferOrderDemands(BaseId a_distId)
    {
        return m_transferDemands.GetValue(a_distId);
    }

    internal void AddTransferOrderDemands(TransferOrderDemand a_trasferOrder)
    {
        m_transferDemands.Add(a_trasferOrder);
    }

    private void RemoveTransferOrderDemands(TransferOrderDemand a_transferOrder)
    {
        m_transferDemands.RemoveObject(a_transferOrder);
    }

    public class TransferOrderDemandComparer : IKeyObjectComparer<TransferOrderDemand>
    {
        public int Compare(TransferOrderDemand a_x, TransferOrderDemand a_y)
        {
            return CompareTransferOrderDemand(a_x, a_y);
        }

        internal static int CompareTransferOrderDemand(TransferOrderDemand a_transferOrderDemand, TransferOrderDemand a_anotherTransferOrder)
        {
            return Comparison.Compare(a_transferOrderDemand.Distribution.Id.Value, a_anotherTransferOrder.Distribution.Id.Value);
        }

        public object GetKey(TransferOrderDemand a_tod)
        {
            return a_tod.m_serializationInfo != null ? a_tod.m_serializationInfo.DistributionId : a_tod.Distribution.Id;
        }
    }
    #endregion

    #region SafetyStockDemand list
    public class SafetyStockDemandList : CustomSortedList<SafetyStockDemand>
    {
        public SafetyStockDemandList()
            : base(new SafetyStockDemandComparer()) { }

        public SafetyStockDemandList(IReader a_reader)
            : base(a_reader, new SafetyStockDemandComparer()) { }

        protected override SafetyStockDemand CreateInstance(IReader a_reader)
        {
            return new SafetyStockDemand(a_reader);
        }
    }

    private SafetyStockDemandList m_safetyStockDemands;

    public SafetyStockDemandList SafetyStockDemands => m_safetyStockDemands;

    /// <summary>
    /// return null if SalesOrderDemand isn't found.
    /// </summary>
    /// <param name="a_salesOrderDemandId"></param>
    /// <returns></returns>
    private SafetyStockDemand FindSafetyStockDemands(BaseId a_invId)
    {
        return m_safetyStockDemands.GetValue(a_invId);
    }

    internal void AddSafetyStockDemands(SafetyStockDemand a_sftyDemand)
    {
        m_safetyStockDemands.Add(a_sftyDemand);
    }

    private void RemoveSafetyStockDemands(SafetyStockDemand a_sftyDemand)
    {
        m_safetyStockDemands.RemoveObject(a_sftyDemand);
    }

    public class SafetyStockDemandComparer : IKeyObjectComparer<SafetyStockDemand>
    {
        public int Compare(SafetyStockDemand a_x, SafetyStockDemand a_y)
        {
            return CompareSafetyStockDemand(a_x, a_y);
        }

        internal static int CompareSafetyStockDemand(SafetyStockDemand a_safetyStockD, SafetyStockDemand a_anotherSafetyStockD)
        {
            return Comparison.Compare(a_safetyStockD.Inventory.Id.Value, a_anotherSafetyStockD.Inventory.Id.Value);
        }

        public object GetKey(SafetyStockDemand a_sfty)
        {
            return a_sfty.Inventory == null ? a_sfty.m_serializationInfo.InventoryId : a_sfty.Inventory.Id;
        }
    }
    #endregion

    #region DeletedDemands list
    public class DeletedDemandList : CustomSortedList<DeletedDemand>
    {
        public DeletedDemandList()
            : base(new DeletedDemandComparer()) { }

        public DeletedDemandList(IReader a_reader)
            : base(a_reader, new DeletedDemandComparer()) { }

        protected override DeletedDemand CreateInstance(IReader a_reader)
        {
            return new DeletedDemand(a_reader);
        }
    }

    private DeletedDemandList m_deletedDemands;

    public DeletedDemandList DeletedDemands => m_deletedDemands;

    internal void AddDelDemands(DeletedDemand a_dd)
    {
        m_deletedDemands.Add(a_dd);
    }

    private void RemoveDelDemands(DeletedDemand a_dd)
    {
        m_deletedDemands.RemoveObject(a_dd);
    }

    public class DeletedDemandComparer : IKeyObjectComparer<DeletedDemand>
    {
        public int Compare(DeletedDemand a_x, DeletedDemand a_y)
        {
            return CompareDeletedDemand(a_x, a_y);
        }

        internal static int CompareDeletedDemand(DeletedDemand a_delDemand, DeletedDemand a_anotherDelDemand)
        {
            return Comparison.Compare(a_delDemand.Id.Value, a_anotherDelDemand.Id.Value);
        }

        public object GetKey(DeletedDemand a_del)
        {
            return a_del.Id;
        }
    }
    #endregion


    internal void AddDemand(MrpDemand a_demand, decimal a_requiredQty, long a_requiredDate, int a_bomLevel, Inventory aInv)
    {
        a_requiredQty = Math.Abs(a_requiredQty);

        if (a_demand is SalesOrderMrpDemand soDemand)
        {
            if (SalesOrderDemands == null)
            {
                m_salesOrderDemands = new SalesOrderDemandList();
            }

            SalesOrderDemand distDemand = new(a_requiredQty, new DateTime(a_requiredDate), soDemand.Distribution, a_bomLevel);
            SalesOrderDemand existingSod = FindSODemands(distDemand.SalesOrderLineDistribution.Id);
            if (existingSod != null)
            {
                existingSod.RequiredQty = distDemand.RequiredQty;
            }
            else
            {
                m_salesOrderDemands.Add(distDemand);
            }
        }
        else if (a_demand is ForecastMrpDemand forecastDemand)
        {
            if (ForecastDemands == null)
            {
                m_forecastDemands = new ForecastDemandList();
            }

            ForecastDemand fDemand = new(a_requiredQty, new DateTime(a_requiredDate), forecastDemand.Shipment, aInv, a_bomLevel);
            ForecastDemand existingDemand = FindForecastDemands(fDemand.ForecastShipment.Id);
            if (existingDemand != null)
            {
                existingDemand.RequiredQty += fDemand.RequiredQty;
            }
            else
            {
                AddForecastDemands(fDemand);
            }
        }
        else if (a_demand is TransferOrderMrpDemand transferDemand)
        {
            if (TransferOrderDemands == null)
            {
                m_transferDemands = new TransferOrderDemandList();
            }

            TransferOrderDemand fTransfer = new(a_requiredQty, new DateTime(a_requiredDate), transferDemand.Distribution, a_bomLevel);
            TransferOrderDemand existingTransferDemand = FindTransferOrderDemands(fTransfer.Distribution.Id);
            if (existingTransferDemand != null)
            {
                existingTransferDemand.RequiredQty += fTransfer.RequiredQty;
            }
            else
            {
                AddTransferOrderDemands(fTransfer);
            }
        }
        else if (a_demand is SafetyStockMrpDemand ssAdj)
        {
            AddSafetyStock(a_requiredQty, a_demand.DemandDate.Ticks, ssAdj.Inventory, ssAdj.Inventory.SafetyStockWarningLevel, a_bomLevel);
        }
        else if (a_demand is ActivityMrpDemand actAdj)
        {
            InternalActivity activity = actAdj.Activity;
            List<BaseDemand> moDemandsList = activity.Operation.ManufacturingOrder.GetDemands();
            //Store any Demand from the parent Job.
            for (int i = 0; i < moDemandsList.Count; i++)
            {
                BaseDemand demand = moDemandsList[i];
                if (demand is SalesOrderDemand)
                {
                    if (SalesOrderDemands == null)
                    {
                        m_salesOrderDemands = new SalesOrderDemandList();
                    }

                    SalesOrderDemand sourceD = (SalesOrderDemand)demand;
                    SalesOrderDemand existingSoDemand = FindSODemands(sourceD.SalesOrderLineDistribution.Id);
                    if (existingSoDemand != null)
                    {
                        existingSoDemand.RequiredQty += sourceD.RequiredQty;
                        RemoveSODemands(existingSoDemand);
                        AddSODemands(new SalesOrderDemand(existingSoDemand.RequiredQty, existingSoDemand.RequiredDate, existingSoDemand.SalesOrderLineDistribution, existingSoDemand.BOMLevel + 1));
                    }
                    else
                    {
                        AddSODemands(new SalesOrderDemand(sourceD.RequiredQty, sourceD.RequiredDate, sourceD.SalesOrderLineDistribution, sourceD.BOMLevel + 1));
                    }
                }
                else if (demand is ForecastDemand)
                {
                    if (ForecastDemands == null)
                    {
                        m_forecastDemands = new ForecastDemandList();
                    }

                    ForecastDemand sourceD = (ForecastDemand)demand;
                    ForecastDemand existingD = FindForecastDemands(sourceD.ForecastShipment.Id);
                    if (existingD != null)
                    {
                        existingD.RequiredQty += sourceD.RequiredQty;
                        RemoveForecastDemands(existingD);
                        AddForecastDemands(new ForecastDemand(existingD.RequiredQty, existingD.RequiredDate, existingD.ForecastShipment, existingD.Inventory, existingD.BOMLevel + 1));
                    }
                    else
                    {
                        AddForecastDemands(new ForecastDemand(sourceD.RequiredQty, sourceD.RequiredDate, sourceD.ForecastShipment, sourceD.Inventory, sourceD.BOMLevel + 1));
                    }
                }
                else if (demand is TransferOrderDemand)
                {
                    if (TransferOrderDemands == null)
                    {
                        m_transferDemands = new TransferOrderDemandList();
                    }

                    TransferOrderDemand sourceD = (TransferOrderDemand)demand;
                    TransferOrderDemand existingTransferDemand = FindTransferOrderDemands(sourceD.Distribution.Id);
                    if (existingTransferDemand != null)
                    {
                        existingTransferDemand.RequiredQty += sourceD.RequiredQty;
                        RemoveTransferOrderDemands(existingTransferDemand);
                        AddTransferOrderDemands(new TransferOrderDemand(existingTransferDemand.RequiredQty, existingTransferDemand.RequiredDate, existingTransferDemand.Distribution, existingTransferDemand.BOMLevel + 1));
                    }
                    else
                    {
                        AddTransferOrderDemands(new TransferOrderDemand(sourceD.RequiredQty, sourceD.RequiredDate, sourceD.Distribution, sourceD.BOMLevel + 1));
                    }
                }
            }
        }
    }

    internal void AddSafetyStock(decimal a_requiredQty, long a_requiredDate, Inventory a_inventory, decimal a_projectedLevel, int a_bomLevel)
    {
        if (SafetyStockDemands == null)
        {
            m_safetyStockDemands = new SafetyStockDemandList();
        }

        SafetyStockDemand ssDemand = new (a_requiredQty, new DateTime(a_requiredDate), a_inventory, a_projectedLevel, a_bomLevel);
        SafetyStockDemand existingSftyDemand = FindSafetyStockDemands(ssDemand.Inventory.Id);
        if (existingSftyDemand != null)
        {
            existingSftyDemand.RequiredQty += ssDemand.RequiredQty;
        }
        else
        {
            AddSafetyStockDemands(ssDemand);
        }
    }

    private static readonly object s_demandLock = new ();

    internal void DeletingDemand(BaseIdObject a_demand, Transmissions.PTTransmissionBase a_t, BaseIdList a_distributionsToDelete = null)
    {
        //TODO: There is a threading issue in this function relating to at least forecasts. An enumerator is accessed by two threads at the same time. 
        //TODO: Can't determine the reason, so for now only one demand can be deleted at once among all threads.
        lock (s_demandLock)
        {
            if (a_demand is Forecast)
            {
                DeletingForecast((Forecast)a_demand, a_t);
            }
            else if (a_demand is ForecastShipment)
            {
                DeletingForecastShipment((ForecastShipment)a_demand, a_t);
            }
            else if (a_demand is SalesOrder)
            {
                DeletingSalesOrder((SalesOrder)a_demand, a_t);
            }
            else if (a_demand is SalesOrderLine)
            {
                if (a_distributionsToDelete == null)
                {
                    DeletingSalesOrderLine((SalesOrderLine)a_demand, a_t);
                }
                else // deleting specific distributions in a so line
                {
                    DeletingSalesOrderLineDistributions((SalesOrderLine)a_demand, a_distributionsToDelete, a_t);
                }
            }
            else if (a_demand is TransferOrder)
            {
                if (a_distributionsToDelete == null)
                {
                    DeletingTransferOrder((TransferOrder)a_demand, a_t);
                }
                else // deleting specific distributions in a so line
                {
                    DeletingTransferOrderDistributions((TransferOrder)a_demand, a_distributionsToDelete, a_t);
                }
            }
            else if (a_demand is Inventory inventory)
            {
                DeletingInventory(inventory, a_t);
            }
        }
    }

    private void DeletingInventory(Inventory a_inventory, PTTransmissionBase a_t)
    {
        foreach (ForecastVersion forecastVersion in a_inventory.ForecastVersions.Versions)
        {
            foreach (Forecast forecast in forecastVersion.Forecasts)
            {
                DeletingForecast(forecast, a_t);
            }
        }
    }

    internal void DeletingSalesOrder(SalesOrder a_so, Transmissions.PTTransmissionBase a_t)
    {
        if (SalesOrderDemands != null)
        {
            foreach (SalesOrderLine sol in a_so.SalesOrderLines)
            {
                DeletingSalesOrderLine(sol, a_t);
            }
        }
    }

    internal void DeletingSalesOrderLine(SalesOrderLine a_soLine, Transmissions.PTTransmissionBase a_t)
    {
        if (SalesOrderDemands != null)
        {
            BaseIdList distIdList = new ();
            foreach (SalesOrderLineDistribution sold in a_soLine.LineDistributions)
            {
                distIdList.Add(sold.Id);
            }

            DeletingSalesOrderLineDistributions(a_soLine, distIdList, a_t);
        }
    }

    internal void DeletingSalesOrderLineDistributions(SalesOrderLine a_soLine, BaseIdList a_distIdList, Transmissions.PTTransmissionBase a_t)
    {
        if (SalesOrderDemands != null)
        {
            foreach (BaseId demandId in a_distIdList)
            {
                SalesOrderDemand sod = FindSODemands(demandId);
                if (sod != null)
                {
                    AddDeletedDemand(sod, string.Format("{0}:{1}, {2}:{3}, {4}:{5}", Localizer.GetString("Sales Order"), a_soLine.SalesOrder.Name, Localizer.GetString("Line"), a_soLine.LineNumber, Localizer.GetString("Distribution"), demandId), a_t.TimeStamp.ToDateTime());
                    RemoveSODemands(sod);
                }
            }
        }
    }

    internal void DeletingForecast(Forecast a_forecast, Transmissions.PTTransmissionBase a_t)
    {
        if (ForecastDemands != null)
        {
            using (IEnumerator<ForecastShipment> enumerator = a_forecast.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    DeletingForecastShipment(enumerator.Current, a_t);
                }
            }
        }
    }

    internal void DeletingForecastShipment(ForecastShipment a_forecastShipment, Transmissions.PTTransmissionBase a_t)
    {
        if (ForecastDemands != null)
        {
            ForecastDemand fDemand = FindForecastDemands(a_forecastShipment.Id);
            if (fDemand != null)
            {
                AddDeletedDemand(fDemand, String.Format("{0}:{1}, {2}:{3}", Localizer.GetString("Forecast"), a_forecastShipment.Forecast.Name, Localizer.GetString("Forecast Shipment"), a_forecastShipment.Id), a_t == null ? PTDateTime.UtcNow.ToDateTime() : a_t.TimeStamp.ToDateTime());
                    RemoveForecastDemands(fDemand);
            }
        }
    }

    internal void DeletingTransferOrder(TransferOrder a_transferOrder, Transmissions.PTTransmissionBase a_t)
    {
        if (TransferOrderDemands != null)
        {
            for (int i = 0; i < a_transferOrder.Distributions.Count; i++)
            {
                TransferOrderDistribution toDist = a_transferOrder.Distributions.GetByIndex(i);
                TransferOrderDemand tferDemand = FindTransferOrderDemands(toDist.Id);
                if (tferDemand != null)
                {
                    AddDeletedDemand(tferDemand, string.Format("{0}:{1}", Localizer.GetString("Transfer Order"), a_transferOrder.Name), a_t.TimeStamp.ToDateTime());
                    RemoveTransferOrderDemands(tferDemand);
                }
            }
        }
    }

    internal void DeletingTransferOrderDistributions(TransferOrder a_transferOrder, BaseIdList a_distributionIdList, Transmissions.PTTransmissionBase a_t)
    {
        if (TransferOrderDemands != null)
        {
            foreach (BaseId demandId in a_distributionIdList)
            {
                TransferOrderDemand tferDemand = FindTransferOrderDemands(demandId);
                if (tferDemand != null)
                {
                    AddDeletedDemand(tferDemand, string.Format("{0}:{1}", Localizer.GetString("Transfer Order"), a_transferOrder.Name), a_t.TimeStamp.ToDateTime());
                    RemoveTransferOrderDemands(tferDemand);
                }
            }
        }
    }

    internal void AddDeletedDemand(BaseDemand a_baseDemand, string a_description, DateTime a_dateDeleted)
    {
        DeletedDemand del = new (a_baseDemand.RequiredQty, a_baseDemand.RequiredDate, a_description, a_dateDeleted, a_baseDemand.BOMLevel);
        if (DeletedDemands == null)
        {
            m_deletedDemands = new DeletedDemandList();
        }

        int index = m_deletedDemands.Count;
        del.Id = new BaseId(index);
        DeletedDemands.Add(del);
    }

    /// <summary>
    /// The total of all Required Quantities.
    /// </summary>
    public decimal QtyToSalesOrders
    {
        get
        {
            decimal qty = 0;
            if (SalesOrderDemands != null)
            {
                foreach (SalesOrderDemand sod in SalesOrderDemands)
                {
                    qty += sod.RequiredQty;
                }
            }

            return qty;
        }
    }

    /// <summary>
    /// The total of all Required Quantities.
    /// </summary>
    public decimal QtyToForecasts
    {
        get
        {
            decimal qty = 0;
            if (ForecastDemands != null)
            {
                foreach (ForecastDemand fd in m_forecastDemands)
                {
                    qty += fd.RequiredQty;
                }
            }

            return qty;
        }
    }

    /// <summary>
    /// The total of all Required Quantities.
    /// </summary>
    public decimal QtyToSafetyStock
    {
        get
        {
            decimal qty = 0;
            if (SafetyStockDemands != null)
            {
                foreach (SafetyStockDemand sftyD in m_safetyStockDemands)
                {
                    qty += sftyD.RequiredQty;
                }
            }

            return qty;
        }
    }

    public void AppendCollection(DemandCollection a_collectionToAppend)
    {
        if (a_collectionToAppend.SalesOrderDemands != null)
        {
            if (SalesOrderDemands == null)
            {
                m_salesOrderDemands = new SalesOrderDemandList();
            }

            foreach (SalesOrderDemand appendSod in a_collectionToAppend.SalesOrderDemands)
            {
                SalesOrderDemand sod = FindSODemands(appendSod.SalesOrderLineDistribution.Id);
                if (sod == null)
                {
                    AddSODemands(appendSod);
                }
            }
        }

        if (a_collectionToAppend.ForecastDemands != null)
        {
            if (ForecastDemands == null)
            {
                m_forecastDemands = new ForecastDemandList();
            }

            foreach (ForecastDemand appendForecastD in a_collectionToAppend.ForecastDemands)
            {
                ForecastDemand fod = FindForecastDemands(appendForecastD.ForecastShipment.Id);
                if (fod == null)
                {
                    AddForecastDemands(appendForecastD);
                }
            }
        }

        if (a_collectionToAppend.TransferOrderDemands != null)
        {
            if (TransferOrderDemands == null)
            {
                m_transferDemands = new TransferOrderDemandList();
            }

            foreach (TransferOrderDemand tod in a_collectionToAppend.TransferOrderDemands)
            {
                TransferOrderDemand existingTod = FindTransferOrderDemands(tod.Distribution.Id);
                if (existingTod == null)
                {
                    AddTransferOrderDemands(tod);
                }
            }
        }

        if (a_collectionToAppend.SafetyStockDemands != null)
        {
            if (SafetyStockDemands == null)
            {
                m_safetyStockDemands = new SafetyStockDemandList();
            }

            foreach (SafetyStockDemand sftyDemand in a_collectionToAppend.SafetyStockDemands)
            {
                SafetyStockDemand existingSafetyDemand = FindSafetyStockDemands(sftyDemand.Inventory.Id);
                if (existingSafetyDemand == null)
                {
                    AddSafetyStockDemands(sftyDemand);
                }
            }
        }

        if (a_collectionToAppend.DeletedDemands != null)
        {
            if (DeletedDemands == null)
            {
                m_deletedDemands = new DeletedDemandList();
            }

            foreach (DeletedDemand dd in a_collectionToAppend.m_deletedDemands)
            {
                m_deletedDemands.Add(dd);
            }
        }
    }

    public void PtDbPopulate(ref PtDbDataSet r_dataSet, PtDbDataSet.JobProductsRow jobProductRow, PTDatabaseHelper a_dbHelper)
    {
        if (SalesOrderDemands != null)
        {
            foreach (SalesOrderDemand d in SalesOrderDemands)
            {
                r_dataSet.JobProductSalesOrderDemands.AddJobProductSalesOrderDemandsRow(
                    jobProductRow.PublishDate,
                    jobProductRow.InstanceId,
                    jobProductRow.ProductId,
                    jobProductRow.OperationId,
                    d.SalesOrderLineDistribution.Id.ToBaseType(),
                    a_dbHelper.AdjustPublishTime(d.RequiredDate),
                    d.RequiredQty,
                    d.Notes
                );
            }
        }

        if (ForecastDemands != null)
        {
            foreach (ForecastDemand d in m_forecastDemands)
            {
                r_dataSet.JobProductForecastDemands.AddJobProductForecastDemandsRow(
                    jobProductRow.PublishDate,
                    jobProductRow.InstanceId,
                    jobProductRow.ProductId,
                    jobProductRow.OperationId,
                    d.ForecastShipment.Id.ToBaseType(),
                    a_dbHelper.AdjustPublishTime(d.RequiredDate),
                    d.RequiredQty,
                    d.Notes
                );
            }
        }

        if (TransferOrderDemands != null)
        {
            foreach (TransferOrderDemand d in m_transferDemands)
            {
                r_dataSet.JobProductTransferOrderDemands.AddJobProductTransferOrderDemandsRow(
                    jobProductRow.PublishDate,
                    jobProductRow.InstanceId,
                    jobProductRow.ProductId,
                    jobProductRow.OperationId,
                    d.Distribution.Id.ToBaseType(),
                    a_dbHelper.AdjustPublishTime(d.RequiredDate),
                    d.RequiredQty,
                    d.Notes
                );
            }
        }

        if (SafetyStockDemands != null)
        {
            foreach (SafetyStockDemand d in m_safetyStockDemands)
            {
                r_dataSet.JobProductSafetyStockDemands.AddJobProductSafetyStockDemandsRow(
                    jobProductRow.PublishDate,
                    jobProductRow.InstanceId,
                    jobProductRow.ProductId,
                    jobProductRow.OperationId,
                    d.Inventory.Id.ToBaseType(),
                    a_dbHelper.AdjustPublishTime(d.RequiredDate),
                    d.RequiredQty,
                    d.Notes
                );
            }
        }

        if (DeletedDemands != null)
        {
            foreach (DeletedDemand d in m_deletedDemands)
            {
                r_dataSet.JobProductDeletedDemands.AddJobProductDeletedDemandsRow(
                    jobProductRow.PublishDate,
                    jobProductRow.InstanceId,
                    jobProductRow.ProductId,
                    jobProductRow.OperationId,
                    a_dbHelper.AdjustPublishTime(d.RequiredDate),
                    d.RequiredQty,
                    d.Notes,
                    d.Description,
                    a_dbHelper.AdjustPublishTime(d.DateDeleted),
                    d.Id.Value
                );
            }
        }
    }

    public void PtDbPopulate(ref PtDbDataSet r_dataSet, PtDbDataSet.PurchasesToStockRow ptsRow, PTDatabaseHelper a_dbHelper)
    {
        if (SalesOrderDemands != null)
        {
            foreach (SalesOrderDemand d in SalesOrderDemands)
            {
                r_dataSet.PurchaseToStockSalesOrderDemands.AddPurchaseToStockSalesOrderDemandsRow(
                    ptsRow.PublishDate,
                    ptsRow.InstanceId,
                    ptsRow.PurchaseToStockId,
                    d.SalesOrderLineDistribution.Id.ToBaseType(),
                    a_dbHelper.AdjustPublishTime(d.RequiredDate),
                    d.RequiredQty,
                    d.Notes
                );
            }
        }

        if (ForecastDemands != null)
        {
            foreach (ForecastDemand d in m_forecastDemands)
            {
                r_dataSet.PurchaseToStockForecastDemands.AddPurchaseToStockForecastDemandsRow(
                    ptsRow.PublishDate,
                    ptsRow.InstanceId,
                    ptsRow.PurchaseToStockId,
                    d.ForecastShipment.Id.ToBaseType(),
                    a_dbHelper.AdjustPublishTime(d.RequiredDate),
                    d.RequiredQty,
                    d.Notes
                );
            }
        }

        if (TransferOrderDemands != null)
        {
            foreach (TransferOrderDemand d in m_transferDemands)
            {
                r_dataSet.PurchaseToStockTransferOrderDemands.AddPurchaseToStockTransferOrderDemandsRow(
                    ptsRow.PublishDate,
                    ptsRow.InstanceId,
                    ptsRow.PurchaseToStockId,
                    d.Distribution.Id.ToBaseType(),
                    a_dbHelper.AdjustPublishTime(d.RequiredDate),
                    d.RequiredQty,
                    d.Notes
                );
            }
        }

        if (SafetyStockDemands != null)
        {
            foreach (SafetyStockDemand d in m_safetyStockDemands)
            {
                r_dataSet.PurchaseToStockSafetyStockDemands.AddPurchaseToStockSafetyStockDemandsRow(
                    ptsRow.PublishDate,
                    ptsRow.InstanceId,
                    ptsRow.PurchaseToStockId,
                    d.Inventory.Id.ToBaseType(),
                    a_dbHelper.AdjustPublishTime(d.RequiredDate),
                    d.RequiredQty,
                    d.Notes
                );
            }
        }

        if (DeletedDemands != null)
        {
            foreach (DeletedDemand d in m_deletedDemands)
            {
                r_dataSet.PurchaseToStockDeletedDemands.AddPurchaseToStockDeletedDemandsRow(
                    ptsRow.PublishDate,
                    ptsRow.InstanceId,
                    ptsRow.PurchaseToStockId,
                    a_dbHelper.AdjustPublishTime(d.RequiredDate),
                    d.RequiredQty,
                    d.Notes,
                    d.Description,
                    a_dbHelper.AdjustPublishTime(d.DateDeleted),
                    d.Id.Value
                );
            }
        }
    }

    //Clear all demands, this object is being deleted
    internal void ClearAllDemands()
    {
        SalesOrderDemands.Clear();
        DeletedDemands.Clear();
        ForecastDemands.Clear();
        SafetyStockDemands.Clear();
        TransferOrderDemands.Clear();
    }
}

/// <summary>
/// All the Product Demands (recursively) for a Manufacturing Order.
/// </summary>
public class ManufacturingOrderDemandCollection : DemandCollection
{
    public ManufacturingOrderDemandCollection(ManufacturingOrder a_mo)
    {
        //Successor MOs
        List<ManufacturingOrder.ManufacturingOrderLevel> sucMOs = a_mo.GetMaterialAndSucMOSuccessorsRecursively();
        for (int sucMOI = 0; sucMOI < sucMOs.Count; sucMOI++)
        {
            ManufacturingOrder.ManufacturingOrderLevel moLevel = sucMOs[sucMOI];
            AddProductDemands(moLevel.MO, moLevel.BomLevels.Min() + 1);
        }

        //The MO's own demand
        AddProductDemands(a_mo, 0);
    }

    private void AddProductDemands(ManufacturingOrder a_mo, int a_bomLevel)
    {
        List<Product> products = a_mo.GetProducts(false);
        for (int pI = 0; pI < products.Count; pI++)
        {
            Product product = products[pI];
            if (product.Demands != null)
            {
                AppendCollection(product.Demands);
                //Set BOM Levels
                if (product.Demands.SalesOrderDemands != null)
                {
                    foreach (SalesOrderDemand sod in product.Demands.SalesOrderDemands)
                    {
                        sod.BOMLevel = a_bomLevel;
                    }
                }

                if (product.Demands.ForecastDemands != null)
                {
                    foreach (ForecastDemand fd in product.Demands.ForecastDemands)
                    {
                        fd.BOMLevel = a_bomLevel;
                    }
                }

                if (product.Demands.TransferOrderDemands != null)
                {
                    foreach (TransferOrderDemand tod in product.Demands.TransferOrderDemands)
                    {
                        tod.BOMLevel = a_bomLevel;
                    }
                }

                if (product.Demands.SafetyStockDemands != null)
                {
                    foreach (SafetyStockDemand sftyD in product.Demands.SafetyStockDemands)
                    {
                        sftyD.BOMLevel = a_bomLevel;
                    }
                }

                if (product.Demands.DeletedDemands != null)
                {
                    foreach (DeletedDemand dd in product.Demands.DeletedDemands)
                    {
                        dd.BOMLevel = a_bomLevel;
                    }
                }
            }
        }
    }
}

/// <summary>
/// All the Product Demands (recursively) for a Job.
/// </summary>
public class JobDemandCollection : DemandCollection
{
    public JobDemandCollection(Job a_job)
    {
        for (int i = 0; i < a_job.ManufacturingOrderCount; i++)
        {
            ManufacturingOrder mo = a_job.ManufacturingOrders[i];
            ManufacturingOrderDemandCollection moDmd = new (mo);
            AppendCollection(moDmd);
        }
    }
}