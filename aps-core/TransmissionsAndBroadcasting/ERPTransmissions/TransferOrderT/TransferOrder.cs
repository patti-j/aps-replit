using PT.SchedulerDefinitions;
using PT.Transmissions;
using System.ComponentModel;

namespace PT.ERPTransmissions;

public partial class TransferOrderT
{
    public class TransferOrder : PTObjectBase, IPTSerializable
    {
        #region IPTSerializable Members
        public new const int UNIQUE_ID = 649;

        public TransferOrder(IReader a_reader)
            : base(a_reader)
        {
            if (a_reader.VersionNumber >= 12555)
            {
                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out m_priority);
                a_reader.Read(out int distCount);
                for (int i = 0; i < distCount; i++)
                {
                    m_distributions.Add(new TransferOrderDistribution(a_reader));
                }

                a_reader.Read(out int val);
                m_maintenanceMethod = (JobDefs.EMaintenanceMethod)val;
            }
            else if (a_reader.VersionNumber >= 725)
            {
                a_reader.Read(out bool closedVal);
                Closed = closedVal;
                a_reader.Read(out m_priority);

                a_reader.Read(out int distCount);
                for (int i = 0; i < distCount; i++)
                {
                    m_distributions.Add(new TransferOrderDistribution(a_reader));
                }

                m_bools = new BoolVector32(a_reader);

                a_reader.Read(out int val);
                m_maintenanceMethod = (JobDefs.EMaintenanceMethod)val;
            }
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);

            m_bools.Serialize(a_writer);
            a_writer.Write(m_priority);
            a_writer.Write(m_distributions.Count);

            for (int i = 0; i < m_distributions.Count; i++)
            {
                m_distributions[i].Serialize(a_writer);
            }

            a_writer.Write((int)m_maintenanceMethod);
        }

        [Browsable(false)]
        public override int UniqueId => UNIQUE_ID;
        #endregion IPTSerializable

        public TransferOrder() { } // required. for xml serialization

        public TransferOrder(TransferOrderTDataSet.TransferOrderRow a_row, JobDefs.EMaintenanceMethod a_maintenanceMethod)
            : base(a_row.ExternalId, a_row.Name, a_row.IsDescriptionNull() ? null : a_row.Description, a_row.IsNotesNull() ? null : a_row.Notes, a_row.IsUserFieldsNull() ? null : a_row.UserFields)
        {
            Closed = a_row.Closed;
            if (!a_row.IsPriorityNull())
            {
                m_priority = a_row.Priority;
            }

            MaintenanceMethod = a_maintenanceMethod;

            TransferOrderTDataSet.TransferOrderDistributionRow[] distributionRows = a_row.GetTransferOrderDistributionRows();
            for (int i = 0; i < distributionRows.Length; i++)
            {
                m_distributions.Add(new TransferOrderDistribution((TransferOrderTDataSet.TransferOrderDistributionRow)distributionRows.GetValue(i)));
            }
        }

        #region Shared Properties
        private int m_priority;

        /// <summary>
        /// Sets the Priority for Jobs created by MRP to satisify this demand.
        /// </summary>
        public int Priority
        {
            get => m_priority;
            set => m_priority = value;
        }

        private BoolVector32 m_bools;
        private const int c_firmIdx = 0;
        private const int c_closedIdx = 1;

        /// <summary>
        /// If the Purchase is Firm then the MRP logic will not modify or delete it.
        /// Users can still change Firm Purchases and imports can affect them.
        /// </summary>
        public bool Firm
        {
            get => m_bools[c_firmIdx];
            set => m_bools[c_firmIdx] = value;
        }

        public bool Closed
        {
            get => m_bools[c_closedIdx];
            set => m_bools[c_closedIdx] = value;
        }
        #endregion Shared Properties

        private readonly List<TransferOrderDistribution> m_distributions = new ();

        public List<TransferOrderDistribution> Distributions => m_distributions;

        private JobDefs.EMaintenanceMethod m_maintenanceMethod = JobDefs.EMaintenanceMethod.ERP;

        /// <summary>
        /// How the TransferOrder was entered into the system.
        /// </summary>
        [Display(DisplayAttribute.displayOptions.ReadOnly)]
        public JobDefs.EMaintenanceMethod MaintenanceMethod
        {
            get => m_maintenanceMethod;
            internal set => m_maintenanceMethod = value;
        }

        public class TransferOrderDistribution : PTObjectIdBase, IPTSerializable
        {
            #region IPTSerializable Members
            public new const int UNIQUE_ID = 650;

            public TransferOrderDistribution(IReader a_reader) : base(a_reader)
            {
                if (a_reader.VersionNumber >= 12555)
                {
                    m_bools = new BoolVector32(a_reader);
                    m_isSetBools = new BoolVector32(a_reader);
                    a_reader.Read(out m_itemExternalId);
                    a_reader.Read(out m_fromWarehouseExternalId);
                    a_reader.Read(out m_toWarehouseExternalId);
                    a_reader.Read(out m_qtyOrdered);
                    a_reader.Read(out m_qtyShipped);
                    a_reader.Read(out m_qtyReceived);
                    a_reader.Read(out m_scheduledShipDate);
                    a_reader.Read(out m_scheduledReceiveDate);
                    a_reader.Read(out int val);
                    m_matlAlloc = (ItemDefs.MaterialAllocation)val;
                    a_reader.Read(out m_minSourceQty);
                    a_reader.Read(out m_maxSourceQty);
                    a_reader.Read(out val);
                    m_materialSourcing = (ItemDefs.MaterialSourcing)val;
                    a_reader.Read(out m_fromStorageAreaExternalId);
                    a_reader.Read(out m_toStorageAreaExternalId);
                }
                else if (a_reader.VersionNumber >= 12551)
                {
                    m_isSetBools = new BoolVector32(a_reader);
                    a_reader.Read(out m_itemExternalId);
                    a_reader.Read(out m_fromWarehouseExternalId);
                    a_reader.Read(out m_toWarehouseExternalId);
                    a_reader.Read(out m_qtyOrdered);
                    a_reader.Read(out m_qtyShipped);
                    a_reader.Read(out m_qtyReceived);
                    a_reader.Read(out m_scheduledShipDate);
                    a_reader.Read(out m_scheduledReceiveDate);
                    a_reader.Read(out bool closedVal);
                    Closed = closedVal;
                    a_reader.Read(out int val);
                    m_matlAlloc = (ItemDefs.MaterialAllocation)val;
                    a_reader.Read(out m_minSourceQty);
                    a_reader.Read(out m_maxSourceQty);
                    a_reader.Read(out val);
                    m_materialSourcing = (ItemDefs.MaterialSourcing)val;
                    a_reader.Read(out m_fromStorageAreaExternalId);
                    a_reader.Read(out m_toStorageAreaExternalId);
                }
                else if (a_reader.VersionNumber >= 731)
                {
                    a_reader.Read(out m_itemExternalId);
                    a_reader.Read(out m_fromWarehouseExternalId);
                    a_reader.Read(out m_toWarehouseExternalId);
                    a_reader.Read(out m_qtyOrdered);
                    a_reader.Read(out m_qtyShipped);
                    a_reader.Read(out m_qtyReceived);
                    a_reader.Read(out m_scheduledShipDate);
                    a_reader.Read(out m_scheduledReceiveDate);
                    a_reader.Read(out bool closedVal);
                    Closed = closedVal;
                    a_reader.Read(out int val);
                    m_matlAlloc = (ItemDefs.MaterialAllocation)val;
                    a_reader.Read(out m_minSourceQty);
                    a_reader.Read(out m_maxSourceQty);
                    a_reader.Read(out val);
                    m_materialSourcing = (ItemDefs.MaterialSourcing)val;
                }
            }

            public override void Serialize(IWriter a_writer)
            {
                base.Serialize(a_writer);

                m_bools.Serialize(a_writer);
                m_isSetBools.Serialize(a_writer);
                a_writer.Write(m_itemExternalId);
                a_writer.Write(m_fromWarehouseExternalId);
                a_writer.Write(m_toWarehouseExternalId);
                a_writer.Write(m_qtyOrdered);
                a_writer.Write(m_qtyShipped);
                a_writer.Write(m_qtyReceived);
                a_writer.Write(m_scheduledShipDate);
                a_writer.Write(m_scheduledReceiveDate);
                a_writer.Write((int)m_matlAlloc);
                a_writer.Write(m_minSourceQty);
                a_writer.Write(m_maxSourceQty);
                a_writer.Write((int)m_materialSourcing);
                a_writer.Write(m_fromStorageAreaExternalId);
                a_writer.Write(m_toStorageAreaExternalId);
            }

            [Browsable(false)]
            public override int UniqueId => UNIQUE_ID;
            #endregion IPTSerializable

            public TransferOrderDistribution() { } // required. for xml serialization

            public TransferOrderDistribution(TransferOrderTDataSet.TransferOrderDistributionRow a_row) : base(a_row.ExternalID)
            {
                m_itemExternalId = a_row.ItemExternalId;
                m_fromWarehouseExternalId = a_row.FromWarehouseExternalId;
                m_toWarehouseExternalId = a_row.ToWarehouseExternalId;
                m_qtyOrdered = a_row.QtyOrdered;
                m_qtyShipped = a_row.QtyShipped;
                m_qtyReceived = a_row.QtyReceived;
                m_scheduledShipDate = a_row.ScheduledShipDate.ToServerTime();
                m_scheduledReceiveDate = a_row.ScheduledReceiveDate.ToServerTime();
                Closed = a_row.Closed;
                if (!a_row.IsMaterialAllocationNull())
                {
                    m_matlAlloc = (ItemDefs.MaterialAllocation)Enum.Parse(typeof(ItemDefs.MaterialAllocation), a_row.MaterialAllocation);
                }
                //m_materialSourcing = (ItemDefs.MaterialSourcing)Enum.Parse(typeof(ItemDefs.MaterialSourcing), a_row.MaterialSourcing ?? "NotSet");
                m_minSourceQty = a_row.IsMinSourceQtyNull() ? 0 : a_row.MinSourceQty;
                m_maxSourceQty = a_row.IsMaxSourceQtyNull() ? 0 : a_row.MaxSourceQty;

                if (!a_row.IsFromStorageAreaExternalIdNull())
                {
                    FromStorageAreaExternalId = a_row.FromStorageAreaExternalId;
                }

                if (!a_row.IsToStorageAreaExternalIdNull())
                {
                    ToStorageAreaExternalId = a_row.ToStorageAreaExternalId;
                }
            }

            private BoolVector32 m_bools;
            private const int c_preferEmptyStorageAreaIdx = 0;
            private const int c_overrideStorageConstraintIdx = 1;
            private const int c_allowPartialAllocationsIdx = 2;
            private const int c_closedIdx = 3;

            private BoolVector32 m_isSetBools;
            private const int c_fromStorageAreasExternalIdIsSetIdx = 0;
            private const int c_toStorageAreasExternalIdIsSetIdx = 1;
            private const int c_preferEmptyStorageAreaIsSetIdx = 2;
            private const int c_overrideStorageConstraintIsSetIdx = 3;
            private const int c_allowPartialAllocationsIsSetIdx = 4;
            private const int c_closedIsSetIdx = 6;

            private string m_itemExternalId;

            /// <summary>
            /// The Item whose inventory will be transferred between Warehouses.
            /// </summary>
            public string ItemExternalId
            {
                get => m_itemExternalId;
                set => m_itemExternalId = value;
            }

            private readonly string m_fromWarehouseExternalId;

            /// <summary>
            /// The Warehouse from which the Inventory will be subtracted.
            /// </summary>
            public string FromWarehouseExternalId => m_fromWarehouseExternalId;

            private readonly string m_toWarehouseExternalId;

            /// <summary>
            /// The Warehouse to which the Inventory will be added.
            /// </summary>
            public string ToWarehouseExternalId => m_toWarehouseExternalId;

            #region Shared Properties
            private readonly DateTime m_scheduledShipDate;

            /// <summary>
            /// The date/time when the inventory is planned to be removed from the From Warehouse.
            /// </summary>
            public DateTime ScheduledShipDate => m_scheduledShipDate;

            private readonly DateTime m_scheduledReceiveDate;

            /// <summary>
            /// The date/time when the inventory is planned to be added to the To Warehouse.
            /// </summary>
            public DateTime ScheduledReceiveDate => m_scheduledReceiveDate;

            private readonly decimal m_qtyOrdered;

            /// <summary>
            /// The total quantity expected to be shipped.
            /// </summary>
            public decimal QtyOrdered => m_qtyOrdered;

            private readonly decimal m_qtyShipped;

            /// <summary>
            /// The quantity already removed from the inventory of the From Warehouse.
            /// </summary>
            public decimal QtyShipped => m_qtyShipped;

            private readonly decimal m_minSourceQty;

            public decimal MinSourceQty => m_minSourceQty;

            private readonly decimal m_maxSourceQty;

            public decimal MaxSourceQty => m_maxSourceQty;

            private ItemDefs.MaterialSourcing m_materialSourcing = ItemDefs.MaterialSourcing.NotSet;

            public ItemDefs.MaterialSourcing MaterialSourcing
            {
                get => m_materialSourcing;
                private set => m_materialSourcing = value;
            }

            private readonly decimal m_qtyReceived;

            /// <summary>
            /// The quantity already received into the To Warehouse.
            /// </summary>
            public decimal QtyReceived => m_qtyReceived;

            /// <summary>
            /// If Closed then the Transfer Order Distribution has no further affect on the Inventory Plan.
            /// </summary>
            public bool Closed
            {
                get => m_bools[c_closedIdx];
                set
                {
                    m_bools[c_closedIdx] = value;
                    m_isSetBools[c_closedIsSetIdx] = true;
                }
            }

            private ItemDefs.MaterialAllocation m_matlAlloc = ItemDefs.MaterialAllocation.NotSet;

            public ItemDefs.MaterialAllocation MaterialAllocation
            {
                get => m_matlAlloc;
                private set => m_matlAlloc = value;
            }

            private string m_fromStorageAreaExternalId;
            /// <summary>
            /// The Storage Area from which the Inventory will be added.
            /// </summary>
            public string FromStorageAreaExternalId
            {
                get => m_fromStorageAreaExternalId;
                set
                {
                    m_fromStorageAreaExternalId = value;
                    m_isSetBools[c_fromStorageAreasExternalIdIsSetIdx] = true;
                }
            }
            public bool FromStorageAreaExternalIdIsSet => m_isSetBools[c_fromStorageAreasExternalIdIsSetIdx];

            private string m_toStorageAreaExternalId;
            /// <summary>
            /// The Storage Area to which the Inventory will be added (optional).
            /// </summary>
            public string ToStorageAreaExternalId
            {
                get => m_toStorageAreaExternalId;
                set
                {
                    m_toStorageAreaExternalId = value;
                    m_isSetBools[c_toStorageAreasExternalIdIsSetIdx] = true;
                }
            }

            public bool ToStorageAreaExternalIdIsSet => m_isSetBools[c_toStorageAreasExternalIdIsSetIdx];

            public bool PreferEmptyStorageArea
            {
                get => m_bools[c_preferEmptyStorageAreaIdx];
                set
                {
                    m_bools[c_preferEmptyStorageAreaIdx] = value;
                    m_isSetBools[c_preferEmptyStorageAreaIsSetIdx] = true;
                }
            }

            public bool PreferEmptyStorageAreaIsSet => m_isSetBools[c_preferEmptyStorageAreaIsSetIdx];

            public bool OverrideStorageConstraint
            {
                get => m_bools[c_overrideStorageConstraintIdx];
                set
                {
                    m_bools[c_overrideStorageConstraintIdx] = value;
                    m_isSetBools[c_overrideStorageConstraintIsSetIdx] = true;
                }
            }

            public bool OverrideStorageConstraintIsSet => m_isSetBools[c_overrideStorageConstraintIsSetIdx];

            public bool AllowPartialAllocations
            {
                get => m_bools[c_allowPartialAllocationsIdx];
                set
                {
                    m_bools[c_allowPartialAllocationsIdx] = value;
                    m_isSetBools[c_allowPartialAllocationsIsSetIdx] = true;
                }
            }

            public bool AllowPartialAllocationsIsSet => m_isSetBools[c_allowPartialAllocationsIsSetIdx];

            #endregion Shared Properties
        }
    }

    public override string Description => string.Format("Updated {0} transfer orders", transferOrders.Count);
}