using System.Collections;
using System.ComponentModel;

using Azure;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Exceptions;
using PT.ERPTransmissions;
using PT.Scheduler.Demand;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.Scheduler.Schedule.Storage;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// For purchasing a qty of an Item for delivery to stock for use by any Job using the Item.
/// </summary>
public partial class PurchaseToStock : BaseObject, ICloneable, IPTSerializable
{
    public new const int UNIQUE_ID = 543;

    #region IPTSerializable Members
    public PurchaseToStock(IReader a_reader)
        : base(a_reader)
    {
        m_restoreInfo = new RestoreInfo();
        int unusedInt;

        if (a_reader.VersionNumber >= 12511) //Added generatedLotIds
        {
            m_bools = new BoolVector32(a_reader);
            
            a_reader.Read(out qtyOrdered);
            a_reader.Read(out m_qtyReceived);
            a_reader.Read(out scheduledReceiptDate);
            a_reader.Read(out buyerExternalId);
            a_reader.Read(out vendorExternalId);
            a_reader.Read(out _unloadSpanTicks);
            a_reader.Read(out _transferSpanTicks);

            m_restoreInfo.WarehouseId = new BaseId(a_reader);
            m_restoreInfo.ItemId = new BaseId(a_reader);
            m_restoreInfo.StorageAreaId = new BaseId(a_reader);
            m_restoreInfo.ProducedLotId = new BaseId(a_reader);

            a_reader.Read(out bool tempHaveDemands);
            if (tempHaveDemands)
            {
                m_demands = new DemandCollection(a_reader);
            }

            a_reader.Read(out unusedInt);
            m_maintenanceMethod = (PurchaseToStockDefs.EMaintenanceMethod)unusedInt;
            a_reader.Read(out m_lotCode);
            a_reader.Read(out m_actualReceiptDate);

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new (a_reader);
                BaseId lotId = new (a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }
        }        
        else if (a_reader.VersionNumber >= 12055) //Added generatedLotIds
        {
            a_reader.Read(out qtyOrdered);
            a_reader.Read(out m_qtyReceived);
            a_reader.Read(out scheduledReceiptDate);
            a_reader.Read(out buyerExternalId);
            a_reader.Read(out vendorExternalId);
            a_reader.Read(out _unloadSpanTicks);
            a_reader.Read(out _transferSpanTicks);

            m_restoreInfo.WarehouseId = new BaseId(a_reader);
            m_restoreInfo.ItemId = new BaseId(a_reader);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out bool tempHaveDemands);
            if (tempHaveDemands)
            {
                m_demands = new DemandCollection(a_reader);
            }

            a_reader.Read(out unusedInt);
            m_maintenanceMethod = (PurchaseToStockDefs.EMaintenanceMethod)unusedInt;
            a_reader.Read(out m_lotCode);
            a_reader.Read(out m_actualReceiptDate);

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new (a_reader);
                BaseId lotId = new (a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }
        }

        #region 12000
        else if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out qtyOrdered);
            a_reader.Read(out m_qtyReceived);
            a_reader.Read(out scheduledReceiptDate);
            a_reader.Read(out buyerExternalId);
            a_reader.Read(out vendorExternalId);
            a_reader.Read(out _unloadSpanTicks);
            a_reader.Read(out _transferSpanTicks);

            m_restoreInfo.WarehouseId = new BaseId(a_reader);
            m_restoreInfo.ItemId = new BaseId(a_reader);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out bool tempHaveDemands);
            if (tempHaveDemands)
            {
                m_demands = new DemandCollection(a_reader);
            }

            a_reader.Read(out unusedInt);
            m_maintenanceMethod = (PurchaseToStockDefs.EMaintenanceMethod)unusedInt;
            a_reader.Read(out m_lotCode);
            a_reader.Read(out m_actualReceiptDate);
        }
        #endregion

        else if (a_reader.VersionNumber >= 756)
        {
            a_reader.Read(out qtyOrdered);
            a_reader.Read(out m_qtyReceived);
            a_reader.Read(out scheduledReceiptDate);
            a_reader.Read(out buyerExternalId);
            a_reader.Read(out vendorExternalId);
            a_reader.Read(out _unloadSpanTicks);
            a_reader.Read(out _transferSpanTicks);

            m_restoreInfo.WarehouseId = new BaseId(a_reader);
            m_restoreInfo.ItemId = new BaseId(a_reader);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out bool tempHaveDemands);
            if (tempHaveDemands)
            {
                m_demands = new DemandCollection(a_reader);
            }

            a_reader.Read(out unusedInt);
            m_maintenanceMethod = (PurchaseToStockDefs.EMaintenanceMethod)unusedInt;
            a_reader.Read(out m_lotCode);

            //Read all existing IDs
            a_reader.Read(out int idCount);
            for (int i = 0; i < idCount; i++)
            {
                BaseId inventoryId = new (a_reader);
                BaseId lotId = new (a_reader);

                m_generatedLotIds.Add(inventoryId, lotId);
            }
        }
        #region 680
        else if (a_reader.VersionNumber >= 680)
        {
            a_reader.Read(out qtyOrdered);
            a_reader.Read(out m_qtyReceived);
            a_reader.Read(out scheduledReceiptDate);
            a_reader.Read(out buyerExternalId);
            a_reader.Read(out vendorExternalId);
            a_reader.Read(out _unloadSpanTicks);
            a_reader.Read(out _transferSpanTicks);

            m_restoreInfo.WarehouseId = new BaseId(a_reader);
            m_restoreInfo.ItemId = new BaseId(a_reader);
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out bool tempHaveDemands);
            if (tempHaveDemands)
            {
                m_demands = new DemandCollection(a_reader);
            }
            a_reader.Read(out unusedInt);
            m_maintenanceMethod = (PurchaseToStockDefs.EMaintenanceMethod)unusedInt;
            a_reader.Read(out m_lotCode);
        }
        #endregion

        if (m_lotCode == null) // default value used to be null. Null is no longer allowed for LotCode.
        {
            m_lotCode = "";
        }
    }

    private RestoreInfo m_restoreInfo;

    private class RestoreInfo
    {
        internal BaseId WarehouseId = BaseId.NULL_ID;
        internal BaseId ItemId = BaseId.NULL_ID;
        internal BaseId StorageAreaId = BaseId.NULL_ID;
        internal BaseId ProducedLotId = BaseId.NULL_ID;
    }

    internal void RestoreReferences(ScenarioDetail a_sd)
    {
        warehouse = a_sd.WarehouseManager.GetById(m_restoreInfo.WarehouseId);
        m_inventory = warehouse.Inventories[m_restoreInfo.ItemId];

        if (m_restoreInfo.StorageAreaId != BaseId.NULL_ID)
        {
            m_storageArea = warehouse.StorageAreas.GetValue(m_restoreInfo.StorageAreaId);
        }

        if (m_restoreInfo.ProducedLotId != BaseId.NULL_ID)
        {
            m_producedLot = m_inventory.Lots.GetById(m_restoreInfo.ProducedLotId);
            //TODO:  we need to handle clearing the produced lot when deleting Lots
            m_producedLot?.RestoreSource(this);
        }

        m_restoreInfo = null;

        if (Demands != null)
        {
            Demands.RestoreReferences(a_sd.SalesOrderManager, Inventory, a_sd.TransferOrderManager, a_sd.WarehouseManager);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        
        a_writer.Write(qtyOrdered);
        a_writer.Write(m_qtyReceived);
        a_writer.Write(scheduledReceiptDate);
        a_writer.Write(buyerExternalId);
        a_writer.Write(vendorExternalId);
        a_writer.Write(_unloadSpanTicks);
        a_writer.Write(_transferSpanTicks);

        warehouse.Id.Serialize(a_writer);
        m_inventory.Item.Id.Serialize(a_writer);
        if (m_storageArea != null)
        {
            m_storageArea.Id.Serialize(a_writer);
        }
        else
        {
            BaseId.NULL_ID.Serialize(a_writer);
        }

        if (m_producedLot != null)
        {
            m_producedLot.Id.Serialize(a_writer);
        }
        else
        {
            BaseId.NULL_ID.Serialize(a_writer);
        }

        a_writer.Write(Demands != null);
        if (Demands != null)
        {
            Demands.Serialize(a_writer);
        }

        a_writer.Write((int)MaintenanceMethod);
        a_writer.Write(m_lotCode);
        a_writer.Write(m_actualReceiptDate);

        a_writer.Write(m_generatedLotIds.Count);
        foreach (KeyValuePair<BaseId, BaseId> ids in m_generatedLotIds)
        {
            ids.Key.Serialize(a_writer);
            ids.Value.Serialize(a_writer);
        }
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Declarations
    public class PurchaseToStockException : PTException
    {
        public PurchaseToStockException(string message)
            : base(message) { }
    }
    #endregion

    #region Construction
    public PurchaseToStock(BaseId id)
        : base(id) { }

    /// <summary>
    /// Sets the field values for an ERP transmission.
    /// </summary>
    public PurchaseToStock(BaseId id, PurchaseToStockT.PurchaseToStock purchaseToStock, WarehouseManager warehouses, ItemManager items, UserFieldDefinitionManager a_udfManager, PTTransmission t, ScenarioDetail a_sd)
        : base(id)
    {
        Update(purchaseToStock, warehouses, items, a_udfManager, t, a_sd, new ScenarioDataChanges());
    }
    #endregion

    #region Shared Properties
    private long scheduledReceiptDate;

    /// <summary>
    /// The date when the material is expected to arrive.
    /// </summary>
    public DateTime ScheduledReceiptDate
    {
        get => new (scheduledReceiptDate);
        internal set => scheduledReceiptDate = value.Ticks;
    }

    private long m_actualReceiptDate;

    public DateTime ActualReceiptDate
    {
        get => new (m_actualReceiptDate);
        set => m_actualReceiptDate = value.Ticks;
    }

    private decimal qtyOrdered;

    /// <summary>
    /// The qty of material expected to arrive.
    /// </summary>
    public decimal QtyOrdered
    {
        get => qtyOrdered;
        set => qtyOrdered = value;
    }

    private decimal m_qtyReceived;

    /// <summary>
    /// The qty of material already received.
    /// </summary>
    public decimal QtyReceived
    {
        get => m_qtyReceived;
        set => m_qtyReceived = value;
    }

    /// <summary>
    /// The remaining qty ordered that has not yet been received
    /// Min of 0
    /// </summary>
    public decimal QtyRemaining => Math.Max(0, QtyOrdered - QtyReceived);

    private string vendorExternalId;

    /// <summary>
    /// The company the parts are ordered from.
    /// </summary>
    public string VendorExternalId
    {
        get => vendorExternalId;
        set => vendorExternalId = value;
    }

    private string buyerExternalId;

    /// <summary>
    /// The individual responsible for this purchase.
    /// </summary>
    public string BuyerExternalId
    {
        get => buyerExternalId;
        set => buyerExternalId = value;
    }

    private long _unloadSpanTicks;

    /// <summary>
    /// If scheduling Docks, this is used to specify the amount of time it will take to unload the items.
    /// </summary>
    public TimeSpan UnloadSpan
    {
        get => new (_unloadSpanTicks);
        set => _unloadSpanTicks = value.Ticks;
    }

    internal long UnloadSpanTicks
    {
        get => _unloadSpanTicks;
        set => _unloadSpanTicks = value;
    }

    private long _transferSpanTicks;

    /// <summary>
    /// Material is not considered usable in production until this time has passed after the Scheduled Receipt Date.
    /// </summary>
    public TimeSpan TransferSpan
    {
        get => new (_transferSpanTicks);
        set => _transferSpanTicks = value.Ticks;
    }

    internal long TransferSpanTicks
    {
        get => _transferSpanTicks;
        set => _transferSpanTicks = value;
    }

    private BoolVector32 m_bools;
    private const int FirmIdx = 0;
    private const int ClosedIdx = 1;
    private const int c_limitMatlSrcToEligibleLotsIdx = 2;
    private const int c_useLimitMatlSrcToEligibleLotsIdx = 3;
    private const int c_overrideStorageConstraint = 4;
    private const int c_requireEmptyStorageIdx = 5;

    /// <summary>
    /// If the Purchase is Firm then the MRP logic will not modify or delete it.
    /// Users can still change Firm Purchases and imports can affect them.
    /// </summary>
    public bool Firm
    {
        get => m_bools[FirmIdx];
        set => m_bools[FirmIdx] = value;
    }

    /// <summary>
    /// If true then the Purchase has no effect on the plan.
    /// </summary>
    public bool Closed
    {
        get => m_bools[ClosedIdx];
        set => m_bools[ClosedIdx] = value;
    }

    /// <summary>
    /// If the QtyOrdered is less than the MinOrderQty then this value is true.  This is for a warning to the material planner.
    /// The Purchase Order is still treated as open for planning purposes so it should have its quantity adjusted or be deleted.
    /// </summary>
    public bool LessThanMinOrderQty => QtyOrdered < m_inventory.Item.MinOrderQty;

    private PurchaseToStockDefs.EMaintenanceMethod m_maintenanceMethod = PurchaseToStockDefs.EMaintenanceMethod.Manual;

    public PurchaseToStockDefs.EMaintenanceMethod MaintenanceMethod
    {
        get => m_maintenanceMethod;
        internal set => m_maintenanceMethod = value;
    }

    public bool LimitMatlSrcToEligibleLots
    {
        get => m_bools[c_limitMatlSrcToEligibleLotsIdx];
        internal set => m_bools[c_limitMatlSrcToEligibleLotsIdx] = value;
    }

    public bool UseLimitMatlSrcToEligibleLots
    {
        get => m_bools[c_useLimitMatlSrcToEligibleLotsIdx];
        set => m_bools[c_useLimitMatlSrcToEligibleLotsIdx] = value;
    }

    /// <summary>
    /// Whether this material will store in excess of the storage areas max quantity when received.
    /// If false, any material that can't be stored will be discarded.
    /// </summary>
    public bool OverrideStorageConstraint
    {
        get => m_bools[c_overrideStorageConstraint];
        set => m_bools[c_overrideStorageConstraint] = value;
    }
    #endregion

    #region Properties
    private Inventory m_inventory;

    /// <summary>
    /// The Inventory location where the Items will go when received.
    /// </summary>
    [Browsable(false)]
    public Inventory Inventory
    {
        get => m_inventory;
        internal set => m_inventory = value;
    }

    private Warehouse warehouse;

    /// <summary>
    /// The Warehouse where the Items will go when received.
    /// </summary>
    [Browsable(false)]
    public Warehouse Warehouse
    {
        get => warehouse;
        internal set => warehouse = value;
    }

    internal string de => string.Format("{0}: {1}", ExternalId, Description);

    internal long ScheduledReceiptDateTicks
    {
        get => scheduledReceiptDate;

        set => scheduledReceiptDate = value;
    }

    /// <summary>
    /// The datetime when the Purchase is expected to be finished unloading.
    /// This is calculated as the Scheduled Receipt Date plus the Unload Duration.
    /// </summary>
    public DateTime UnloadEndDate => new (UnloadEndDateTicks);

    internal long UnloadEndDateTicks => ScheduledReceiptDateTicks + UnloadSpan.Ticks;

    private DateTime m_needDate;

    /// <summary>
    /// The date and time of the requirement that triggered the creation of the PO.
    /// This is only valid after MRP is run. This value is not serialized yet.
    /// </summary>
    public DateTime NeedDate
    {
        get => m_needDate;
        internal set => m_needDate = value;
    }

    /// <summary>
    /// The time when material is available to production. This is the ScheduledReceiptDate+UnloadSpan+TransferSpan. That is, Material arrives, is unloaded, and is transferred to production.
    /// </summary>
    public DateTime AvailableDate => new (AvailableDateTicks);

    /// <summary>
    /// The time when material is available to production. This is the ScheduledReceiptDate+UnloadSpan+TransferSpan. That is, Material arrives, is unloaded, and is transferred to production.
    /// </summary>
    internal long AvailableDateTicks => UnloadEndDateTicks + TransferSpan.Ticks;

    private string m_lotCode = "";

    /// <summary>
    /// The lot code to associate with available material
    /// </summary>
    public string LotCode
    {
        get => m_lotCode;
        internal set
        {
            if (value != null)
            {
                m_lotCode = value;
            }
        }
    }

    private StorageArea m_storageArea;

    public StorageArea StorageArea
    {
        get => m_storageArea;
        internal set => m_storageArea = value;
    }

    public bool RequireEmptyStorageArea
    {
        get => m_bools[c_requireEmptyStorageIdx];
        set => m_bools[c_requireEmptyStorageIdx] = value;
    }
    #endregion

    #region Dock Scheduling
    internal void Move(Scenario s, PurchaseToStockMoveT t)
    {
        //Only allows moves within the same warehouse since the ERP is in charge of the Warehouse.
        ScenarioEvents se;
        if (t.newWarehouseId != Warehouse.Id)
        {
            //Fire event to undo move on UI if necessary
            ScenarioDetail sd;
            using (s.ScenarioDetailLock.EnterRead(out sd))
            {
                using (s.ScenarioEventsLock.EnterRead(out se))
                {
                    se.FirePurchaseToStockMoveFailedEvent(this, t, sd);
                }
            }

            throw new PTValidationException("2181");
        }

        //Need to update the Recipt date and dock assignment
        ScheduledReceiptDateTicks = t.newStart;
        //if (t.moveToDock)
        //{
        //    if (t.newDockNbr > Warehouse.NbrOfDocks)
        //        throw new APSCommon.PTValidationException("2182", new object[] { t.newDockNbr + 1, Warehouse.NbrOfDocks });
        //    this.ScheduledDockNbr = t.newDockNbr;
        //    ScheduledToDock = true;
        //}
        //else //scheduled to the Warehouse, not to a specific Dock.
        //{
        //    this.ScheduledDockNbr = -1;
        //    ScheduledToDock = false;
        //}
        //this.RescheduledInSchedulingBoard = true; //flag to not update schedule for po externally anymore

        ScenarioDetail sd1;
        using (s.ScenarioDetailLock.EnterRead(out sd1))
        {
            using (s.ScenarioEventsLock.EnterRead(out se))
            {
                se.FirePurchaseToStockMovedEvent(this, t, sd1);
            }
        }
    }

    /// <summary>
    /// Revert the Purchase back to the control of the ERP.
    /// </summary>
    /// <param name="t"></param>
    internal void Revert(PurchaseToStockRevertT t, IScenarioDataChanges a_dataChanges)
    {
        //this.RescheduledInSchedulingBoard = false;
        a_dataChanges.PurchaseToStockChanges.UpdatedObject(Id);
    }
    #endregion

    #region Overrides
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [Browsable(false)]
    public override string DefaultNamePrefix => "PO-";
    #endregion

    #region Demo Data
    /// <summary>
    /// Adjust values to update Demo Data.
    /// </summary>
    internal void AdjustDemoDataForClockAdvance(long clockAdvanceTicks)
    {
        ScheduledReceiptDate += new TimeSpan(clockAdvanceTicks);
    }
    #endregion

    #region ERP Transmissions
    internal bool Update(PurchaseToStockT.PurchaseToStock purchaseToStock, WarehouseManager warehouses, ItemManager items, UserFieldDefinitionManager a_udfManager, PTTransmission t, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        bool updated = Update(purchaseToStock, t, a_udfManager, UserField.EUDFObjectType.PurchasesToStock);

        if (warehouse?.ExternalId != purchaseToStock.WarehouseExternalId)
        {
            warehouse = warehouses.GetByExternalId(purchaseToStock.WarehouseExternalId);
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }
        
        if (m_storageArea?.ExternalId != purchaseToStock.StorageAreaExternalId)
        {
            m_storageArea = warehouse.StorageAreas.GetByExternalId(purchaseToStock.StorageAreaExternalId);
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        } 

        if (m_inventory?.Item.ExternalId != purchaseToStock.ItemExternalId)
        {
            Item item = items.GetByExternalId(purchaseToStock.ItemExternalId);
            m_inventory = warehouse.Inventories[item.Id];
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (BuyerExternalId != purchaseToStock.BuyerExternalId)
        {
            BuyerExternalId = purchaseToStock.BuyerExternalId;
            updated = true;
        }

        if (QtyOrdered != purchaseToStock.QtyOrdered)
        {
            QtyOrdered = purchaseToStock.QtyOrdered;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (QtyReceived != purchaseToStock.QtyReceived)
        {
            QtyReceived = purchaseToStock.QtyReceived;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }
        
        if (Closed != purchaseToStock.Closed)
        {
            Closed = purchaseToStock.Closed;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }
        
        if (OverrideStorageConstraint != purchaseToStock.OverrideStorageConstraint)
        {
            OverrideStorageConstraint = purchaseToStock.OverrideStorageConstraint;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }
        if (RequireEmptyStorageArea != purchaseToStock.RequireEmptyStorageArea)
        {
            RequireEmptyStorageArea = purchaseToStock.RequireEmptyStorageArea;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }
        
        if (Firm != purchaseToStock.Firm)
        {
            Firm = purchaseToStock.Firm;
            updated = true;
        }
        
        if (ScheduledReceiptDate != purchaseToStock.ScheduledReceiptDate)
        {
            ScheduledReceiptDate = PTDateTime.Max(a_sd.ClockDate, purchaseToStock.ScheduledReceiptDate);
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }
        
        if (ActualReceiptDate != purchaseToStock.ActualReceiptDate)
        {
            ActualReceiptDate = purchaseToStock.ActualReceiptDate;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }
        
        if (VendorExternalId != purchaseToStock.VendorExternalId)
        {
            VendorExternalId = purchaseToStock.VendorExternalId;
            updated = true;
        }
        
        if (purchaseToStock.UnloadSpanSet)
        {
            _unloadSpanTicks = purchaseToStock.UnloadSpan.Ticks;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (purchaseToStock.TransferSpanSet)
        {
            _transferSpanTicks = purchaseToStock.TransferSpan.Ticks;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (MaintenanceMethod != purchaseToStock.MaintenanceMethod)
        {
            MaintenanceMethod = purchaseToStock.MaintenanceMethod;
            updated = true;
        }
        
        if (LotCode != purchaseToStock.LotCode)
        {
            LotCode = purchaseToStock.LotCode;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (UseLimitMatlSrcToEligibleLots != purchaseToStock.UseLimitMatlSrcToEligibleLots)
        {
            UseLimitMatlSrcToEligibleLots = purchaseToStock.UseLimitMatlSrcToEligibleLots;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (UseLimitMatlSrcToEligibleLots)
        {
            if (LimitMatlSrcToEligibleLots != purchaseToStock.LimitMatlSrcToEligibleLots)
            {
                LimitMatlSrcToEligibleLots = purchaseToStock.LimitMatlSrcToEligibleLots;
                updated = true;
                a_dataChanges.FlagConstraintChanges(Id);
            }
        }
        else
        {
            if (!LimitMatlSrcToEligibleLots)
            {
                LimitMatlSrcToEligibleLots = true;
                updated = true;
                a_dataChanges.FlagConstraintChanges(Id);
            }
        }

        return updated;
    }
    #endregion

    #region Cloning
    public PurchaseToStock Clone()
    {
        return (PurchaseToStock)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion

    #region PT Database
    //		public static void SetPtDbCommand(SqlDataAdapter da, SqlConnection conn)
    //		{
    //			//INSERT Command
    //			string sql="INSERT PurchaseToStocks(PurchaseToStockId, Name,Description,BottleneckThreshold,HeavyLoadThreshold) " +
    //				"VALUES (@PurchaseToStockID, @Name,@Description,@BottleneckThreshold,@HeavyLoadThreshold);";// +
    ////				"SET @PurchaseToStockID=Scope_Identity(); " +
    ////				"SELECT @PurchaseToStockID PurchaseToStockID;";
    //			da.InsertCommand=new SqlCommand(sql,conn);
    //
    //			//INSERT Command Parameters
    //			SqlParameterCollection cparams=da.InsertCommand.Parameters;
    //			SqlParameter purchaseToStockId=cparams.Add("@PurchaseToStockID",SqlDbType.Int,0,"PurchaseToStockID");
    ////			purchaseToStockId.Direction=ParameterDirection.Output;
    //			cparams.Add("@Name",SqlDbType.NVarChar,30,"Name");
    //			cparams.Add("@Description",SqlDbType.NVarChar,255,"Description");
    //			cparams.Add("@BottleneckThreshold",SqlDbType.Float,0,"BottleneckThreshold");
    //			cparams.Add("@HeavyLoadThreshold",SqlDbType.Float,0,"HeavyLoadThreshold");	
    //		}
    #endregion PT Database

    #region Deleting
    internal void ValidateWarehouseDelete(Warehouse warehouse)
    {
        if (this.warehouse.Id == warehouse.Id)
        {
            throw new PTValidationException("2184", new object[] { warehouse.ExternalId, ExternalId });
        }
    }

    internal void ValidateStorageAreaDelete(StorageArea a_storageArea, StorageAreasDeleteProfile a_deleteProfile)
    {
        if (this.StorageArea.Id == a_storageArea.Id)
        {
            a_deleteProfile.AddValidationException(a_storageArea, new PTValidationException("3090", new object[] { m_storageArea.ExternalId, ExternalId }));
        }
    }

    internal void ValidateInventoryDelete(InventoryDeleteProfile a_deleteProfile)
    {
        if (a_deleteProfile.ContainsInventory(m_inventory.Id))
        {
            Inventory deletingInventory = a_deleteProfile.GetById(m_inventory.Id);
            a_deleteProfile.AddValidationException(deletingInventory, new PTValidationException("2185", new object[] { deletingInventory.Item.ExternalId, deletingInventory.Warehouse.ExternalId, ExternalId }));
        }
    }

    internal void ValidateItemStorageDelete(ItemStorageDeleteProfile a_deleteProfile)
    {
        if (a_deleteProfile.ContainsItems(m_storageArea.Id, m_inventory.Item.Id))
        {
            ItemStorage itemStorage = m_storageArea.Storage.GetValue(m_inventory.Item.Id);
            ItemStorage deletingItemStorage = a_deleteProfile.GetById(itemStorage.Id);
            a_deleteProfile.AddValidationException(deletingItemStorage, new PTValidationException("2185", new object[] { deletingItemStorage.Item.ExternalId, deletingItemStorage.Warehouse.ExternalId, ExternalId }));
        }
    }
    #endregion

    public override string ToString()
    {
        return string.Format("PurchaseToStock={0}".Localize(), ExternalId);
    }

    #region MRP
    private DemandCollection m_demands;

    /// <summary>
    /// Specifies the demands for which the Purchase was created.
    /// Null if not specified.
    /// </summary>
    public DemandCollection Demands
    {
        get => m_demands;
        internal set => m_demands = value;
    }

    internal void DeletingSalesOrder(SalesOrder a_so, PTTransmissionBase a_t)
    {
        if (Demands != null)
        {
            Demands.DeletingSalesOrder(a_so, a_t);
        }
    }

    internal void DeletingSalesOrderLineDistributions(SalesOrderLine a_soLine, PTTransmissionBase a_t)
    {
        if (Demands != null)
        {
            Demands.DeletingSalesOrderLine(a_soLine, a_t);
        }
    }

    internal void DeletingSalesOrderLineDistributions(SalesOrderLine a_soLine, BaseIdList a_distIdList, PTTransmissionBase a_t)
    {
        if (Demands != null)
        {
            Demands.DeletingSalesOrderLineDistributions(a_soLine, a_distIdList, a_t);
        }
    }

    internal void DeletingForecastShipments(Forecast a_forecast, PTTransmissionBase a_t)
    {
        if (Demands != null)
        {
            Demands.DeletingForecast(a_forecast, a_t);
        }
    }

    internal void DeletingForecastShipment(ForecastShipment a_forecastShipment, PTTransmissionBase a_t)
    {
        if (Demands != null)
        {
            Demands.DeletingForecastShipment(a_forecastShipment, a_t);
        }
    }

    internal void DeletingTransferOrderDistributions(TransferOrder a_transferOrder, PTTransmissionBase a_t)
    {
        if (Demands != null)
        {
            Demands.DeletingTransferOrder(a_transferOrder, a_t);
        }
    }

    internal void DeletingTransferOrderDistribution(TransferOrder a_transferOrder, BaseIdList a_distributionIdList, PTTransmissionBase a_t)
    {
        if (Demands != null)
        {
            Demands.DeletingTransferOrderDistributions(a_transferOrder, a_distributionIdList, a_t);
        }
    }
    #endregion MRP

    #region Drum Buffer Rope
    /// <summary>
    /// The Receiving Buffer of the Inventory.
    /// </summary>
    public TimeSpan ReceivingBuffer => m_inventory.ReceivingBuffer;

    /// <summary>
    /// Material should be received by this date to avoid penetrating the material receiving buffer. The ScheduledReceiptDate minus the Receiving Buffer.
    /// </summary>
    public DateTime ReceiptDate => ScheduledReceiptDate.Subtract(ReceivingBuffer);

    /// <summary>
    /// The percent of time between the ReceiptDate and the ScheduledReceiptDate that has been passed based on the Scenario Clock.
    /// </summary>
    public decimal GetCurrentBufferPenetrationPercent(ScenarioDetail a_sd)
    {
        if (ReceivingBuffer.TotalHours == 0)
        {
            return 0;
        }

        decimal hoursIntoBuffer = (decimal)a_sd.ClockDate.Subtract(ReceiptDate).TotalHours;
        return 100 * hoursIntoBuffer / (decimal)ReceivingBuffer.TotalHours;
    }
    #endregion Drum Buffer Rope

    /// <summary>
    /// Returns a list of all Manufacturing Order that use Material from this one or are Successor MOs including any number of BOM Levels away.
    /// POTENTIALLY SLOW -- LOOKS THROUGH ALL JOBS
    /// The same MO will occur multiple times in the list if it's both a Material Successor and a Successor MO.
    /// </summary>
    public List<ManufacturingOrder.ManufacturingOrderLevel> GetManufacturingOrdersSuppliedRecursively()
    {
        List<ManufacturingOrder> mos = GetManufacturingOrdersSupplied();
        List<ManufacturingOrder.ManufacturingOrderLevel> allSucMoLevels = new ();
        foreach (ManufacturingOrder mo in mos)
        {
            List<ManufacturingOrder.ManufacturingOrderLevel> sucMoLevels = mo.GetMaterialAndSucMOSuccessorsRecursively();
            allSucMoLevels.AddRange(sucMoLevels);
        }

        return allSucMoLevels;
    }

    /// <summary>
    /// Gets a list of the MOs supplied by this PurchaseToStock.
    /// SLOW -- This iterates throw the list of Jobs searching for supplied activities so it can be slow for large data sets.
    /// </summary>
    public List<ManufacturingOrder> GetManufacturingOrdersSupplied()
    {
        Dictionary<BaseId, BaseId> mosAdded = new ();
        List<ManufacturingOrder> mosSupplied = new ();

        List<MaterialRequirement.MaterialRequirementSupply> MRsSupplied = GetMaterialRequirementsSupplied();
        foreach (MaterialRequirement.MaterialRequirementSupply mr in MRsSupplied)
        {
            if (!mosAdded.ContainsKey(mr.SuppliedOperation.ManufacturingOrder.Id))
            {
                mosSupplied.Add(mr.SuppliedOperation.ManufacturingOrder);
                mosAdded.Add(mr.SuppliedOperation.ManufacturingOrder.Id, mr.SuppliedOperation.ManufacturingOrder.Id);
            }
        }

        return mosSupplied;
    }

    /// <summary>
    /// Gets a list of the Material Requirements supplied by this PurchaseToStock.
    /// SLOW -- This iterates throw the list of Jobs searching for supplied activities so it can be slow for large data sets.
    /// </summary>
    /// <param name="jobs">A list of Jobs to search.</param>
    /// <returns>The list of MaterialRequirements and qties supplied.</returns>
    public List<MaterialRequirement.MaterialRequirementSupply> GetMaterialRequirementsSupplied()
    {
        List<MaterialRequirement.MaterialRequirementSupply> list = new();
        HashSet<BaseId> mrIds = new(); //This is needed to track when a single MR has multiple adjustments for the same lot
        if (m_producedLot != null)
        {
            foreach (Adjustment adjustment in m_producedLot.GetAdjustmentArray())
            {
                if (adjustment is MaterialRequirementAdjustment mrAdjustment)
                {
                    if (!mrIds.Contains(mrAdjustment.Material.Id))
                    {
                        MaterialRequirement.MaterialRequirementSupply mrSupply = new (mrAdjustment.Material, mrAdjustment.Activity.Operation, mrAdjustment.Qty);
                        list.Add(mrSupply);
                        mrIds.Add(mrAdjustment.Material.Id);
                    }
                }
            }
        }

        return list;
    }

    public bool Edit(ScenarioDetail a_sd, PurchaseToStockEdit a_edit, IScenarioDataChanges a_dataChanges)
    {
        bool updated = base.Edit(a_edit);

        if (a_edit.FirmSet && Firm != a_edit.Firm)
        {
            Firm = a_edit.Firm;
            updated = true;
        }

        if (a_edit.ClosedSet && Closed != a_edit.Closed)
        {
            Closed = a_edit.Closed;
            updated = true;
        }
        if (a_edit.RequireEmptyStorageAreaSet && RequireEmptyStorageArea != a_edit.RequireEmptyStorageArea)
        {
            RequireEmptyStorageArea = a_edit.RequireEmptyStorageArea;
            updated = true;
        }
        if (a_edit.OverrideStorageConstraintSet && OverrideStorageConstraint != a_edit.OverrideStorageConstraint)
        {
            OverrideStorageConstraint = a_edit.OverrideStorageConstraint;
            updated = true;
        }

        if (a_edit.ActualReceiptDateSet && ActualReceiptDate != a_edit.ActualReceiptDate)
        {
            ActualReceiptDate = a_edit.ActualReceiptDate;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_edit.LotCodeSet && LotCode != a_edit.LotCode)
        {
            LotCode = a_edit.LotCode;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_edit.QtyOrderedSet && QtyOrdered != a_edit.QtyOrdered)
        {
            QtyOrdered = a_edit.QtyOrdered;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_edit.QtyReceivedSet && QtyReceived != a_edit.QtyReceived)
        {
            QtyReceived = a_edit.QtyReceived;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_edit.TransferSpanSet && TransferSpan != a_edit.TransferSpan)
        {
            TransferSpan = a_edit.TransferSpan;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_edit.UnloadSpanSet && UnloadSpan != a_edit.UnloadSpan)
        {
            UnloadSpan = a_edit.UnloadSpan;
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_edit.ScheduledReceiptDateSet && ScheduledReceiptDate != a_edit.ScheduledReceiptDate)
        {
            ScheduledReceiptDate = PTDateTime.Max(a_sd.ClockDate, a_edit.ScheduledReceiptDate);
            updated = true;
            a_dataChanges.FlagConstraintChanges(Id);
        }

        if (a_edit.VendorExternalIdSet && VendorExternalId != a_edit.VendorExternalId)
        {
            VendorExternalId = a_edit.VendorExternalId;
            updated = true;
        }

        if (a_edit.BuyerExternalIdSet && BuyerExternalId != a_edit.BuyerExternalId)
        {
            BuyerExternalId = a_edit.BuyerExternalId;
            updated = true;
        }

        if (a_edit.ItemExternalIdSet && a_edit.WarehouseExternalIdSet)
        {
            foreach (Inventory itemInventory in a_sd.WarehouseManager.GetByExternalId(a_edit.WarehouseExternalId).Inventories)
            {
                if (itemInventory.Item.ExternalId == a_edit.ItemExternalId)
                {
                    if (Inventory.Item.ExternalId != a_edit.ItemExternalId)
                    {
                        Inventory = itemInventory;
                        updated = true;
                        a_dataChanges.FlagConstraintChanges(Id);
                        break;
                    }
                }
            }
        }

        if (a_edit.MaintenanceMethodSet && MaintenanceMethod != a_edit.MaintenanceMethod)
        {
            MaintenanceMethod = a_edit.MaintenanceMethod;
            updated = true;
        }

        if (a_edit.StorageAreaExternalIdSet && m_storageArea?.ExternalId != a_edit.StorageAreaExternalId)
        {
            if (string.IsNullOrEmpty(a_edit.StorageAreaExternalId))
            {
                m_storageArea = null;
            }
            else
            {
                m_storageArea = warehouse?.StorageAreas.GetByExternalId(a_edit.StorageAreaExternalId);
            }

            updated = true;
        }

        return updated;
        //TODO: Everything should be validated the same way as in the standard receive function
    }
}

public class PurchaseToStockArrayList
{
    private readonly ArrayList purchaseToStockArrayList = new ();

    public void Add(PurchaseToStock purchaseToStock)
    {
        purchaseToStockArrayList.Add(purchaseToStock);
    }

    public void Clear()
    {
        purchaseToStockArrayList.Clear();
    }

    public int Count => purchaseToStockArrayList.Count;

    public PurchaseToStock this[int index] => (PurchaseToStock)purchaseToStockArrayList[index];

    public bool Contains(PurchaseToStock purchaseToStock)
    {
        return purchaseToStockArrayList.Contains(purchaseToStock);
    }

    public void Remove(PurchaseToStock purchaseToStock)
    {
        purchaseToStockArrayList.Remove(purchaseToStock);
    }

    public void Copy(PurchaseToStockArrayList copy)
    {
        Clear();
        purchaseToStockArrayList.AddRange(copy.purchaseToStockArrayList);
    }

    public void Copy(PurchaseToStockManager purchaseToStockManager)
    {
        Clear();
        for (int purchaseToStockManagerI = 0; purchaseToStockManagerI < purchaseToStockManager.Count; purchaseToStockManagerI++)
        {
            PurchaseToStock purchaseToStock = purchaseToStockManager.GetByIndex(purchaseToStockManagerI);
            purchaseToStockArrayList.Add(purchaseToStock);
        }
    }

    internal void Union(PurchaseToStockArrayList list)
    {
        for (int listIdx = 0; listIdx < list.Count; listIdx++)
        {
            PurchaseToStock purchaseToStock = list[listIdx];
            if (!purchaseToStockArrayList.Contains(purchaseToStock))
            {
                purchaseToStockArrayList.Add(purchaseToStock);
            }
        }
    }

    internal void Intersection(PurchaseToStockArrayList list)
    {
        for (int purchaseToStockArrayListIdx = 0; purchaseToStockArrayListIdx < purchaseToStockArrayList.Count; purchaseToStockArrayListIdx++)
        {
            PurchaseToStock purchaseToStock = (PurchaseToStock)purchaseToStockArrayList[purchaseToStockArrayListIdx];

            if (!list.purchaseToStockArrayList.Contains(purchaseToStock))
            {
                purchaseToStockArrayList.Remove(purchaseToStock);
            }
        }
    }
}