using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public class PurchaseToStockT : ERPMaintenanceTransmission<PurchaseToStockT.PurchaseToStock>, IPTSerializable
{
    public new const int UNIQUE_ID = 540;

    #region PT Serialization
    public PurchaseToStockT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                PurchaseToStock node = new (reader);
                Add(node);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public PurchaseToStockT() { }

    public new PurchaseToStock this[int i] => Nodes[i];

    public override void Validate()
    {
        for (int i = 0; i < Count; i++)
        {
            PurchaseToStock pts = this[i];
            pts.Validate();
        }

        base.Validate();
    }

    #region PurchaseToStock
    /// <summary>
    /// A standard Item to be purchased for stock.  The received Item will go to stock for use by any Job requiring the Item.
    /// </summary>
    public class PurchaseToStock : PTObjectBase, IPTSerializable
    {
        public new const int UNIQUE_ID = 538;

        #region PT Serialization
        public PurchaseToStock(IReader reader)
            : base(reader)
        {
            #region 12520
            if (reader.VersionNumber >= 12520)
            {
                reader.Read(out m_qtyOrdered);
                reader.Read(out m_qtyReceived);
                reader.Read(out m_scheduledReceiptDate);
                reader.Read(out m_buyerExternalId);
                reader.Read(out m_vendorExternalId);

                reader.Read(out m_itemExternalId);
                reader.Read(out m_warehouseExternalId);
                reader.Read(out m_storageAreaExternalId);

                reader.Read(out m_unloadSpan);
                reader.Read(out m_transferSpan);
                m_bools = new BoolVector32(reader);
                int tmp;
                reader.Read(out tmp);
                m_maintenanceMethod = (PurchaseToStockDefs.EMaintenanceMethod)tmp;
                reader.Read(out m_lotCode);
                reader.Read(out m_actualReceiptDate);
            }
            #endregion
            #region 12000
            else if (reader.VersionNumber >= 12000)
            {
                reader.Read(out m_qtyOrdered);
                reader.Read(out m_qtyReceived);
                reader.Read(out m_scheduledReceiptDate);
                reader.Read(out m_buyerExternalId);
                reader.Read(out m_vendorExternalId);

                reader.Read(out m_itemExternalId);
                reader.Read(out m_warehouseExternalId);

                reader.Read(out m_unloadSpan);
                reader.Read(out m_transferSpan);
                m_bools = new BoolVector32(reader);
                int tmp;
                reader.Read(out tmp);
                m_maintenanceMethod = (PurchaseToStockDefs.EMaintenanceMethod)tmp;
                reader.Read(out m_lotCode);
                reader.Read(out m_actualReceiptDate);
            }
            #endregion

            #region 680
            else if (reader.VersionNumber >= 680)
            {
                reader.Read(out m_qtyOrdered);
                reader.Read(out m_qtyReceived);
                reader.Read(out m_scheduledReceiptDate);
                reader.Read(out m_buyerExternalId);
                reader.Read(out m_vendorExternalId);

                reader.Read(out m_itemExternalId);
                reader.Read(out m_warehouseExternalId);

                reader.Read(out m_unloadSpan);
                reader.Read(out m_transferSpan);
                m_bools = new BoolVector32(reader);
                int tmp;
                reader.Read(out tmp);
                m_maintenanceMethod = (PurchaseToStockDefs.EMaintenanceMethod)tmp;
                reader.Read(out m_lotCode);
            }
            #endregion

            #region 677
            else if (reader.VersionNumber >= 677)
            {
                reader.Read(out m_qtyOrdered);
                reader.Read(out m_qtyReceived);
                reader.Read(out m_scheduledReceiptDate);
                reader.Read(out m_buyerExternalId);
                reader.Read(out m_vendorExternalId);

                reader.Read(out m_itemExternalId);
                reader.Read(out m_warehouseExternalId);

                reader.Read(out m_unloadSpan);
                reader.Read(out m_transferSpan);
                m_bools = new BoolVector32(reader);
                int tmp;
                reader.Read(out tmp);
                m_maintenanceMethod = (PurchaseToStockDefs.EMaintenanceMethod)tmp;
            }
            #endregion

            else if (reader.VersionNumber >= 670)
            {
                reader.Read(out m_qtyOrdered);
                reader.Read(out m_qtyReceived);
                reader.Read(out m_scheduledReceiptDate);
                reader.Read(out m_buyerExternalId);
                reader.Read(out m_vendorExternalId);

                reader.Read(out m_itemExternalId);
                reader.Read(out m_warehouseExternalId);

                reader.Read(out m_unloadSpan);
                reader.Read(out m_transferSpan);
                m_bools = new BoolVector32(reader);
            }
            else if (reader.VersionNumber >= 281)
            {
                reader.Read(out m_qtyOrdered);
                reader.Read(out m_scheduledReceiptDate);
                reader.Read(out m_buyerExternalId);
                reader.Read(out m_vendorExternalId);

                reader.Read(out m_itemExternalId);
                reader.Read(out m_warehouseExternalId);

                bool unusedBool;
                reader.Read(out m_unloadSpan);
                reader.Read(out unusedBool);
                m_bools[c_unloadSpanSetIdx] = unusedBool;
                reader.Read(out m_transferSpan);
                reader.Read(out unusedBool);
                m_bools[c_transferSpanSetIdx] = unusedBool;
                int unusedInt;
                reader.Read(out unusedBool);
                reader.Read(out unusedBool);
                reader.Read(out unusedInt);
                reader.Read(out unusedBool);
                m_bools = new BoolVector32(reader);
            }

            #region Version 104
            else if (reader.VersionNumber >= 104)
            {
                reader.Read(out m_qtyOrdered);
                reader.Read(out m_scheduledReceiptDate);
                reader.Read(out m_buyerExternalId);
                reader.Read(out m_vendorExternalId);

                reader.Read(out m_itemExternalId);
                reader.Read(out m_warehouseExternalId);

                bool unusedBool;
                reader.Read(out m_unloadSpan);
                reader.Read(out unusedBool);
                m_bools[c_unloadSpanSetIdx] = unusedBool;
                reader.Read(out m_transferSpan);
                reader.Read(out unusedBool);
                m_bools[c_transferSpanSetIdx] = unusedBool;
                int unusedInt;
                reader.Read(out unusedBool);
                reader.Read(out unusedBool);
                reader.Read(out unusedInt);
                reader.Read(out unusedBool);
            }
            #endregion

            else if (reader.VersionNumber >= 1)
            {
                reader.Read(out m_qtyOrdered);
                reader.Read(out m_scheduledReceiptDate);
                reader.Read(out m_buyerExternalId);
                reader.Read(out m_vendorExternalId);

                reader.Read(out m_itemExternalId);
                reader.Read(out m_warehouseExternalId);
            }
        }

        public override void Serialize(IWriter writer)
        {
            base.Serialize(writer);

            writer.Write(m_qtyOrdered);
            writer.Write(m_qtyReceived);
            writer.Write(m_scheduledReceiptDate);
            writer.Write(m_buyerExternalId);
            writer.Write(m_vendorExternalId);

            writer.Write(m_itemExternalId);
            writer.Write(m_warehouseExternalId);
            writer.Write(m_storageAreaExternalId);

            writer.Write(m_unloadSpan);
            writer.Write(m_transferSpan);
            m_bools.Serialize(writer);
            writer.Write((int)m_maintenanceMethod);
            writer.Write(m_lotCode);
            writer.Write(m_actualReceiptDate);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        public PurchaseToStock() { } // reqd. for xml serialization

        public PurchaseToStock(string externalId, string a_name, string description, string notes, string userFields, decimal qtyOrdered, decimal qtyReceived, 
                               DateTime scheduledReceiptDate, DateTime actualReceiptDate, string itemExternalId, string warehouseExternalId,  string a_storageAreaExternalId,
                               bool aFirm, bool aClosed, string aBuyer, string aVendor, double a_transferHrs, double a_unloadHrs, PurchaseToStockDefs.EMaintenanceMethod a_maintenanceMethod, 
                               string a_lotCode, bool a_overrideStorageConstraint,bool a_requireEmptyStorageArea)
            : base(externalId, a_name, description, notes, userFields)
        {
            m_qtyOrdered = qtyOrdered;
            m_qtyReceived = qtyReceived;
            m_scheduledReceiptDate = scheduledReceiptDate.ToServerTime().Ticks;
            m_actualReceiptDate = actualReceiptDate.ToServerTime().Ticks;
            m_itemExternalId = itemExternalId;
            m_warehouseExternalId = warehouseExternalId;
            m_storageAreaExternalId = a_storageAreaExternalId;

            Firm = aFirm;
            Closed = aClosed;
            BuyerExternalId = aBuyer;
            VendorExternalId = aVendor;
            TransferSpan = TimeSpan.FromHours(a_transferHrs);
            UnloadSpan = TimeSpan.FromHours(a_unloadHrs);
            MaintenanceMethod = a_maintenanceMethod;
            LotCode = a_lotCode;
            OverrideStorageConstraint = a_overrideStorageConstraint;
            RequireEmptyStorageArea = a_requireEmptyStorageArea;
        }

        public PurchaseToStock(string externalId, string a_name, string description, string notes, string userFields)
            : base(externalId, a_name, description, notes, userFields) { }

        public PurchaseToStock(PtImportDataSet.PurchaseToStocksRow aRow, PurchaseToStockDefs.EMaintenanceMethod a_maintenanceMethod)
            : base(aRow.ExternalId, aRow.IsNameNull() ? null : aRow.Name, aRow.IsDescriptionNull() ? null : aRow.Description, aRow.IsNotesNull() ? null : aRow.Notes, aRow.IsUserFieldsNull() ? null : aRow.UserFields)
        {
            m_itemExternalId = aRow.ItemExternalId;
            m_warehouseExternalId = aRow.WarehouseExternalId;
            m_storageAreaExternalId = aRow.StorageAreaExternalId;

            if (!aRow.IsQtyOrderedNull())
            {
                m_qtyOrdered = aRow.QtyOrdered;
            }

            if (!aRow.IsQtyReceivedNull())
            {
                QtyReceived = aRow.QtyReceived;
            }

            if (!aRow.IsScheduledReceiptDateNull())
            {
                m_scheduledReceiptDate = aRow.ScheduledReceiptDate.ToServerTime().Ticks;
            }

            if (!aRow.IsActualReceiptDateNull())
            {
                m_actualReceiptDate = aRow.ActualReceiptDate.ToServerTime().Ticks;
            }

            if (!aRow.IsFirmNull())
            {
                Firm = aRow.Firm;
            }

            if (!aRow.IsClosedNull())
            {
                Closed = aRow.Closed;
            }

            if (!aRow.IsBuyerExternalIdNull())
            {
                BuyerExternalId = aRow.BuyerExternalId;
            }

            if (!aRow.IsVendorExternalIdNull())
            {
                VendorExternalId = aRow.VendorExternalId;
            }

            if (!aRow.IsTransferHrsNull())
            {
                TransferSpan = TimeSpan.FromHours(aRow.TransferHrs);
            }

            if (!aRow.IsUnloadHrsNull())
            {
                UnloadSpan = TimeSpan.FromHours(aRow.UnloadHrs);
            }

            MaintenanceMethod = a_maintenanceMethod;
            if (!aRow.IsLotCodeNull())
            {
                LotCode = aRow.LotCode;
            }

            if (!aRow.IsUseLimitMatlSrcToEligibleLotsNull())
            {
                UseLimitMatlSrcToEligibleLots = aRow.UseLimitMatlSrcToEligibleLots;
            }

            if (!aRow.IsLimitMatlSrcToEligibleLotsNull())
            {
                LimitMatlSrcToEligibleLots = aRow.LimitMatlSrcToEligibleLots;
            }

            if (!aRow.IsOverrideStorageConstraintNull())
            {
                OverrideStorageConstraint = aRow.OverrideStorageConstraint;
            }

            if (!aRow.IsRequireEmptyStorageAreaNull())
            {
                RequireEmptyStorageArea = aRow.RequireEmptyStorageArea;
            }

        }

        #region Shared Properties
        private long m_scheduledReceiptDate;

        [Required(true)]
        public DateTime ScheduledReceiptDate
        {
            get => new (m_scheduledReceiptDate);
            set => m_scheduledReceiptDate = value.Ticks;
        }

        private long m_actualReceiptDate;

        public DateTime ActualReceiptDate
        {
            get => new (m_actualReceiptDate);
            set => m_actualReceiptDate = value.Ticks;
        }

        private decimal m_qtyOrdered;

        [Required(true)]
        public decimal QtyOrdered
        {
            get => m_qtyOrdered;
            set => m_qtyOrdered = value;
        }

        private decimal m_qtyReceived;

        public decimal QtyReceived
        {
            get => m_qtyReceived;
            set => m_qtyReceived = value;
        }

        private string m_vendorExternalId;

        /// <summary>
        /// The company the parts are ordered from.
        /// </summary>
        public string VendorExternalId
        {
            get => m_vendorExternalId;
            set => m_vendorExternalId = value;
        }

        private string m_buyerExternalId;

        /// <summary>
        /// The individual responsible for this purchase.
        /// </summary>
        public string BuyerExternalId
        {
            get => m_buyerExternalId;
            set => m_buyerExternalId = value;
        }

        private string m_warehouseExternalId;

        /// <summary>
        /// The Warehouse where the items will be delivered when received.
        /// </summary>
        [Required(true)]
        public string WarehouseExternalId
        {
            get => m_warehouseExternalId;
            set => m_warehouseExternalId = value;
        }
        private string m_storageAreaExternalId;

        /// <summary>
        /// The StorageArea where the items will be stored when received.
        /// </summary>
        public string StorageAreaExternalId
        {
            get => m_storageAreaExternalId;
            set => m_storageAreaExternalId = value;
        }

        private TimeSpan m_unloadSpan;

        /// <summary>
        /// If scheduling Docks, this is used to specify the amount of time it will take to unload the items.
        /// </summary>
        public TimeSpan UnloadSpan
        {
            get => m_unloadSpan;
            set
            {
                m_unloadSpan = value;
                m_bools[c_unloadSpanSetIdx] = true;
            }
        }

        public bool UnloadSpanSet => m_bools[c_unloadSpanSetIdx];

        private TimeSpan m_transferSpan;

        /// <summary>
        /// Material is not considered usable in production until this time has passed after the Scheduled Receipt Date.
        /// </summary>
        public TimeSpan TransferSpan
        {
            get => m_transferSpan;
            set
            {
                m_transferSpan = value;
                m_bools[c_transferSpanSetIdx] = true;
            }
        }

        public bool TransferSpanSet => m_bools[c_transferSpanSetIdx];

        private string m_itemExternalId;

        /// <summary>
        /// The Item to be ordered.
        /// </summary>
        [Required(true)]
        public string ItemExternalId
        {
            get => m_itemExternalId;
            set => m_itemExternalId = value;
        }

        private BoolVector32 m_bools;
        private const int c_firmIdx = 0;
        private const int c_closedIdx = 1;
        private const int c_transferSpanSetIdx = 2;
        private const int c_unloadSpanSetIdx = 3;
        private const int c_limitMatlSrcToEligibleLotsIdx = 4;
        private const int c_useLimitMatlSrcToEligibleLotsIdx = 5;
        private const int c_overrideStorageConstraintIdx = 6;
        private const int c_requireEmptyStorageAreaIdx = 7;
        
        public bool RequireEmptyStorageArea
        {
            get => m_bools[c_requireEmptyStorageAreaIdx];
            set
            {
                m_bools[c_requireEmptyStorageAreaIdx] = value;
            }
        }
        /// <summary>
        /// Whether this material will store in excess of the storage areas max quantity when received.
        /// If false, any material that can't be stored will be discarded.
        /// </summary>
        public bool OverrideStorageConstraint
        {
            get => m_bools[c_overrideStorageConstraintIdx];
            set
            {
                m_bools[c_overrideStorageConstraintIdx] = value;
            }
        }
        /// <summary>
        /// If the Purchase is Firm then the MRP logic will not modify or delete it.
        /// Users can still change Firm Purchases and imports can affect them.
        /// </summary>
        public bool Firm
        {
            get => m_bools[c_firmIdx];
            set => m_bools[c_firmIdx] = value;
        }

        /// <summary>
        /// If true then the Purchase has no effect on the plan.
        /// </summary>
        public bool Closed
        {
            get => m_bools[c_closedIdx];
            set => m_bools[c_closedIdx] = value;
        }

        private PurchaseToStockDefs.EMaintenanceMethod m_maintenanceMethod = PurchaseToStockDefs.EMaintenanceMethod.Manual;

        public PurchaseToStockDefs.EMaintenanceMethod MaintenanceMethod
        {
            get => m_maintenanceMethod;
            set => m_maintenanceMethod = value;
        }

        private string m_lotCode;

        public string LotCode
        {
            get => m_lotCode;
            set => m_lotCode = value;
        }

        public bool LimitMatlSrcToEligibleLots
        {
            get => m_bools[c_limitMatlSrcToEligibleLotsIdx];
            set => m_bools[c_limitMatlSrcToEligibleLotsIdx] = value;
        }

        public bool UseLimitMatlSrcToEligibleLots
        {
            get => m_bools[c_useLimitMatlSrcToEligibleLotsIdx];
            set => m_bools[c_useLimitMatlSrcToEligibleLotsIdx] = value;
        }
        #endregion Shared Properties

        public override void Validate()
        {
            if (string.IsNullOrEmpty(ItemExternalId))
            {
                throw new ValidationException("2115");
            }

            if (string.IsNullOrEmpty(WarehouseExternalId))
            {
                throw new ValidationException("2116");
            }

            if (QtyOrdered <= 0 && QtyReceived <= 0)
            {
                throw new ValidationException("2936");
            }

            if (string.IsNullOrEmpty(StorageAreaExternalId))
            {
                throw new ValidationException("3110", new []{ ExternalId});
            }
            //base.Validate();
        }
    }
    #endregion

    #region Database Loading
    public void Fill(System.Data.IDbCommand cmd, PurchaseToStockDefs.EMaintenanceMethod a_maintenanceMethod)
    {
        PtImportDataSet.PurchaseToStocksDataTable table = new ();
        FillTable(table, cmd);
        Fill(table, a_maintenanceMethod);
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    public void Fill(PtImportDataSet.PurchaseToStocksDataTable aTable, PurchaseToStockDefs.EMaintenanceMethod a_maintenanceMethod)
    {
        for (int i = 0; i < aTable.Count; i++)
        {
            Add(new PurchaseToStock(aTable[i], a_maintenanceMethod));
        }
    }
    #endregion

    public override string Description => string.Format("Purchase orders updated ({0})".Localize(), Count);
}