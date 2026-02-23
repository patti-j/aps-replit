using PT.APSCommon;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public partial class WarehouseT
{
    public class Item : PTObjectBase, IPTSerializable
    {
        public new const int UNIQUE_ID = 524;

        #region PT Serialization
        public Item(IReader a_reader)
            : base(a_reader)
        {
            if (a_reader.VersionNumber >= 12539)
            {
                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out int val);
                m_source = (ItemDefs.sources)val;
                a_reader.Read(out val);
                itemType = (ItemDefs.itemTypes)val;
                a_reader.Read(out m_shelfLifeTicks);

                a_reader.Read(out m_sourceSet);
                a_reader.Read(out m_itemTypeSet);
                a_reader.Read(out m_defaultLeadTime);
                a_reader.Read(out m_defaultLeadTimeSet);
                a_reader.Read(out m_shelfLifeSet);
                a_reader.Read(out m_transferQty);
                a_reader.Read(out m_itemGroup);
                a_reader.Read(out m_batchSize);
                a_reader.Read(out m_batchWindow);
                a_reader.Read(out m_jobAutoSplitQty);
                a_reader.Read(out m_minOrderQty);
                a_reader.Read(out m_minOrderQtyRoundupLimit);
                a_reader.Read(out m_cost);
                a_reader.Read(out m_maxOrderQty);

                a_reader.Read(out m_unitVolume);
                a_reader.Read(out m_shelfLifeWarningHrs);
            }
            else if (a_reader.VersionNumber >= 12511)
            {
                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out int val);
                m_source = (ItemDefs.sources)val;
                a_reader.Read(out val);
                itemType = (ItemDefs.itemTypes)val;
                a_reader.Read(out m_shelfLifeTicks);

                a_reader.Read(out m_sourceSet);
                a_reader.Read(out m_itemTypeSet);
                a_reader.Read(out m_defaultLeadTime);
                a_reader.Read(out m_defaultLeadTimeSet);
                a_reader.Read(out m_shelfLifeSet);
                a_reader.Read(out m_transferQty);
                a_reader.Read(out m_itemGroup);
                a_reader.Read(out m_batchSize);
                a_reader.Read(out m_batchWindow);
                a_reader.Read(out m_jobAutoSplitQty);
                a_reader.Read(out m_minOrderQty);
                a_reader.Read(out m_minOrderQtyRoundupLimit);
                a_reader.Read(out m_cost);
                a_reader.Read(out m_maxOrderQty);

                a_reader.Read(out m_unitVolume);
            }
            else if (a_reader.VersionNumber >= 12106)
            {
                a_reader.Read(out int val);
                m_source = (ItemDefs.sources)val;
                a_reader.Read(out val);
                itemType = (ItemDefs.itemTypes)val;
                a_reader.Read(out m_shelfLifeTicks);

                a_reader.Read(out m_sourceSet);
                a_reader.Read(out m_itemTypeSet);
                a_reader.Read(out m_defaultLeadTime);
                a_reader.Read(out m_defaultLeadTimeSet);
                a_reader.Read(out m_shelfLifeSet);
                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out m_transferQty);
                a_reader.Read(out m_itemGroup);
                a_reader.Read(out m_batchSize);
                a_reader.Read(out m_batchWindow);
                a_reader.Read(out m_jobAutoSplitQty);
                a_reader.Read(out m_minOrderQty);
                a_reader.Read(out m_minOrderQtyRoundupLimit);
                a_reader.Read(out m_cost);
                a_reader.Read(out int lotUsabilityVal);
                a_reader.Read(out m_maxOrderQty);

                a_reader.Read(out m_unitVolume);
            }
            else if (a_reader.VersionNumber >= 728)
            {
                a_reader.Read(out int val);
                m_source = (ItemDefs.sources)val;
                a_reader.Read(out val);
                itemType = (ItemDefs.itemTypes)val;
                a_reader.Read(out m_shelfLifeTicks);

                a_reader.Read(out m_sourceSet);
                a_reader.Read(out m_itemTypeSet);
                a_reader.Read(out m_defaultLeadTime);
                a_reader.Read(out m_defaultLeadTimeSet);
                a_reader.Read(out m_shelfLifeSet);
                m_bools = new BoolVector32(a_reader);
                a_reader.Read(out m_transferQty);
                a_reader.Read(out m_itemGroup);
                a_reader.Read(out m_batchSize);
                a_reader.Read(out m_batchWindow);
                a_reader.Read(out m_jobAutoSplitQty);
                a_reader.Read(out m_minOrderQty);
                a_reader.Read(out m_minOrderQtyRoundupLimit);
                a_reader.Read(out m_cost);
                a_reader.Read(out int lotUsabilityVal);
                a_reader.Read(out m_maxOrderQty);
            }
        }

        public override void Serialize(IWriter a_writer)
        {
            base.Serialize(a_writer);

            m_bools.Serialize(a_writer);
            a_writer.Write((int)m_source);
            a_writer.Write((int)itemType);
            a_writer.Write(m_shelfLifeTicks);

            a_writer.Write(m_sourceSet);
            a_writer.Write(m_itemTypeSet);
            a_writer.Write(m_defaultLeadTime);
            a_writer.Write(m_defaultLeadTimeSet);
            a_writer.Write(m_shelfLifeSet);
            a_writer.Write(m_transferQty);
            a_writer.Write(m_itemGroup);
            a_writer.Write(m_batchSize);
            a_writer.Write(m_batchWindow);
            a_writer.Write(m_jobAutoSplitQty);
            a_writer.Write(m_minOrderQty);
            a_writer.Write(m_minOrderQtyRoundupLimit);
            a_writer.Write(m_cost);
            a_writer.Write(m_maxOrderQty);

            a_writer.Write(m_unitVolume);
            a_writer.Write(m_shelfLifeWarningHrs);
        }

        public override int UniqueId => UNIQUE_ID;
        #endregion

        #region bools
        private BoolVector32 m_bools;
        private const int TransferQtySetIdx = 0;
        private const int ItemGroupSetIdx = 1;
        private const int BatchSizeSetIdx = 2;
        private const int MinNbrBatchesSetIdx = 3;
        private const int MaxNbrBatchesSetIdx = 4;
        private const int BatchWindowSetIdx = 5;
        private const int PlanInventoryIdx = 6;
        private const int PlanInventorySetIdx = 7;
        private const int MinOrderQtySetIdx = 8;
        private const int MinOrderQtyRoundupLimitSetIdx = 9;
        private const int JobAutoSplitQtySetIdx = 10;
        private const int RollupIdx = 11;
        private const int RollupAttributesToParentSetIdx = 12;
        private const int CostIsSetIdx = 13;
        private const int UnusedIdx = 14;
        //private const int LotUsabilitySetIdx = 15;
        private const int MaxOrderQtySetIdx = 16;
        private const short c_unitVolumeSetIdx = 17;
        private const short c_shelfLifeWarningHrsSetIdx = 18;
        #endregion

        public Item() { } // reqd. for xml serialization

        public Item(string externalId, string name, string description, string notes, string userFields)
            : base(externalId, name, description, notes, userFields) { }

        public Item(PtImportDataSet.ItemsRow aRow)
            : base(aRow.ExternalId, aRow.IsNameNull() ? null : aRow.Name, aRow.IsDescriptionNull() ? null : aRow.Description, aRow.IsNotesNull() ? null : aRow.Notes, aRow.IsUserFieldsNull() ? null : aRow.UserFields)
        {
            if (!aRow.IsDefaultLeadTimeDaysNull())
            {
                DefaultLeadTime = TimeSpan.FromDays(aRow.DefaultLeadTimeDays);
            }

            if (!aRow.IsItemTypeNull())
            {
                try
                {
                    ItemType = (ItemDefs.itemTypes)Enum.Parse(typeof(ItemDefs.itemTypes), aRow.ItemType);
                }
                catch (Exception err)
                {
                    throw new PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            aRow.ItemType, "Item", "Type",
                            string.Join(", ", Enum.GetNames(typeof(ItemDefs.itemTypes)))
                        });
                }
            }

            if (!aRow.IsSourceNull())
            {
                try
                {
                    Source = (ItemDefs.sources)Enum.Parse(typeof(ItemDefs.sources), aRow.Source);
                }
                catch (Exception err)
                {
                    throw new PTValidationException("2854",
                        err,
                        false,
                        new object[]
                        {
                            aRow.Source, "Item", "Source",
                            string.Join(", ", Enum.GetNames(typeof(ItemDefs.sources)))
                        });
                }
            }

            if (!aRow.IsBatchSizeNull())
            {
                BatchSize = aRow.BatchSize;
            }

            if (!aRow.IsBatchWindowHrsNull())
            {
                BatchWindow = aRow.BatchWindowHrs == TimeSpan.MaxValue.TotalHours ? TimeSpan.MaxValue : TimeSpan.FromHours(aRow.BatchWindowHrs);
            }

            if (!aRow.IsItemGroupNull())
            {
                ItemGroup = aRow.ItemGroup;
            }

            if (!aRow.IsShelfLifeHrsNull())
            {
                ShelfLife = aRow.ShelfLifeHrs == TimeSpan.MaxValue.TotalHours ? TimeSpan.MaxValue : TimeSpan.FromHours(aRow.ShelfLifeHrs); //Overflows doing the conversion otherwise
            }

            if (!aRow.IsTransferQtyNull())
            {
                TransferQty = aRow.TransferQty;
            }

            if (!aRow.IsPlanInventoryNull())
            {
                PlanInventory = aRow.PlanInventory;
            }

            if (!aRow.IsMinOrderQtyNull())
            {
                MinOrderQty = aRow.MinOrderQty;
            }

            if (!aRow.IsMinOrderQtyRoundupLimitNull())
            {
                MinOrderQtyRoundupLimit = aRow.MinOrderQtyRoundupLimit;
            }

            if (!aRow.IsMaxOrderQtyNull())
            {
                MaxOrderQty = aRow.MaxOrderQty;
            }

            if (!aRow.IsJobAutoSplitQtyNull())
            {
                JobAutoSplitQty = aRow.JobAutoSplitQty;
            }

            if (!aRow.IsRollupAttributesToParentNull())
            {
                RollupAttributesToParent = aRow.RollupAttributesToParent;
            }

            if (!aRow.IsCostNull())
            {
                Cost = aRow.Cost;
            }

            if (!aRow.IsShelfLifeWarningHrsNull())
            {
                ShelfLifeWarningHrs = aRow.ShelfLifeWarningHrs;
            }
        }

        #region Shared Properties
        private ItemDefs.sources m_source;

        /// <summary>
        /// Where the Item originates from.  This is for information only and has no effect on scheduling.
        /// </summary>
        public ItemDefs.sources Source
        {
            get => m_source;
            set
            {
                m_source = value;
                m_sourceSet = true;
            }
        }

        private bool m_sourceSet;

        public bool SourcSet => m_sourceSet;

        private ItemDefs.itemTypes itemType;

        /// <summary>
        /// For information only.
        /// </summary>
        public ItemDefs.itemTypes ItemType
        {
            get => itemType;
            set
            {
                itemType = value;
                m_itemTypeSet = true;
            }
        }

        private bool m_itemTypeSet;

        /// <summary>
        /// Indicates the primary use of the Item.  This is for information only and has no effect on scheduling.
        /// </summary>
        public bool ItemTypeSet => m_itemTypeSet;

        private long m_defaultLeadTime;

        /// <summary>
        /// Used by Material Requirements when there is no Inventory record for the Item at any available Warehouse.
        /// </summary>
        public TimeSpan DefaultLeadTime
        {
            get => TimeSpan.FromTicks(m_defaultLeadTime);
            set
            {
                m_defaultLeadTime = value.Ticks;
                m_defaultLeadTimeSet = true;
            }
        }

        private bool m_defaultLeadTimeSet;

        public bool DefaultLeadTimeSet => m_defaultLeadTimeSet;

        private long m_shelfLifeTicks;

        /// <summary>
        /// The maximum amount of time that the Item can remain in inventory before being used.
        /// </summary>
        public TimeSpan ShelfLife
        {
            get => new (m_shelfLifeTicks);
            set
            {
                m_shelfLifeTicks = value.Ticks;
                m_shelfLifeSet = true;
            }
        }

        private bool m_shelfLifeSet;

        public bool ShelfLifeSet => m_shelfLifeSet;

        public bool TransferQtySet => m_bools[TransferQtySetIdx];

        private decimal m_transferQty;

        /// <summary>
        /// As this product is produced it is transferred to Inventory in quantities of this size.
        /// </summary>
        public decimal TransferQty
        {
            get => m_transferQty;

            set
            {
                if (value < 0)
                {
                    throw new ValidationException("2051");
                }

                m_bools[TransferQtySetIdx] = true;
                m_transferQty = value;
            }
        }

        private string m_itemGroup;

        /// <summary>
        /// Can be used for visually grouping Items into categories.
        /// </summary>
        public string ItemGroup
        {
            get => m_itemGroup;
            set
            {
                m_itemGroup = value;
                m_bools[ItemGroupSetIdx] = true;
            }
        }

        public bool ItemGroupSet => m_bools[ItemGroupSetIdx];

        private decimal m_batchSize;

        /// <summary>
        /// Jobs and Purchases must be in integer multiples of this value.
        /// </summary>
        public decimal BatchSize
        {
            get => m_batchSize;
            set
            {
                m_batchSize = value;
                m_bools[BatchSizeSetIdx] = true;
            }
        }

        public bool BatchSizeSet => m_bools[BatchSizeSetIdx];

        private TimeSpan m_batchWindow;

        /// <summary>
        /// When batching demand to create Jobs or Purchases the batch should not reach out further than this value in order to reach the Max Nbr Batches.
        /// </summary>
        public TimeSpan BatchWindow
        {
            get => m_batchWindow;
            set
            {
                m_batchWindow = value;
                m_bools[BatchWindowSetIdx] = true;
            }
        }

        public bool BatchWindowSet => m_bools[BatchWindowSetIdx];

        /// <summary>
        /// Whether the Item should be included in the Inventory Plan.
        /// </summary>
        public bool PlanInventory
        {
            get => m_bools[PlanInventoryIdx];
            set
            {
                m_bools[PlanInventoryIdx] = value;
                m_bools[PlanInventorySetIdx] = true;
            }
        }

        public bool PlanInventorySet => m_bools[PlanInventorySetIdx];

        private decimal m_minOrderQty;

        /// <summary>
        /// Jobs and Purchase Orders must be for at least this quantity when created by EMS.
        /// If a planned Job, after rounding up by MinOrderQtyRoundupLimit, is less than this amount then it will be marked as Do Not Schedule and remain unscheduled.
        /// If a planned Purchase Order, after rounding up by MinOrderQtyRoundupLimit, is less than this amount then it wll have "Less than Min Order Qty" set to true.
        /// </summary>
        public decimal MinOrderQty
        {
            get => m_minOrderQty;
            set
            {
                m_minOrderQty = value;
                m_bools[MinOrderQtySetIdx] = true;
            }
        }

        public bool MinOrderQtySet => m_bools[MinOrderQtySetIdx];

        private decimal m_maxOrderQty;

        public decimal MaxOrderQty
        {
            get => m_maxOrderQty;
            set
            {
                m_maxOrderQty = value;
                m_bools[MaxOrderQtySetIdx] = true;
            }
        }

        public bool MaxOrderQtySet => m_bools[MaxOrderQtySetIdx];

        private decimal m_minOrderQtyRoundupLimit;

        /// <summary>
        /// If a Job or Purchase Order created by EMS is less than this quantity then EMS is allowed to round-up the quantity by as much as this amount in order to reach the MinOrderQty.
        /// Take care when setting this to large values as it can create extra inventory.
        /// </summary>
        public decimal MinOrderQtyRoundupLimit
        {
            get => m_minOrderQtyRoundupLimit;
            set
            {
                m_minOrderQtyRoundupLimit = value;
                m_bools[MinOrderQtyRoundupLimitSetIdx] = true;
            }
        }

        public bool MinOrderQtyRoundupLimitSet => m_bools[MinOrderQtyRoundupLimitSetIdx];

        private decimal m_jobAutoSplitQty;

        /// <summary>
        /// Jobs generated by EMS that exceed this quantity have Manfacturing Orders split off into quantities of this size until no Manufacturing Order in the Job exceeds this quantity.
        /// </summary>
        public decimal JobAutoSplitQty
        {
            get => m_jobAutoSplitQty;
            set
            {
                m_jobAutoSplitQty = value;
                m_bools[JobAutoSplitQtySetIdx] = true;
            }
        }

        public bool JobAutoSplitQtySet => m_bools[JobAutoSplitQtySetIdx];

        /// <summary>
        /// If true then when MRP creates Jobs it will recursively copy Operation Attributes from supplying Job Templates to the Operation consuming the Material.
        /// This is often used to carry-up Attributes from "allergens" in materials.
        /// </summary>
        public bool RollupAttributesToParent
        {
            get => m_bools[RollupIdx];
            set
            {
                m_bools[RollupIdx] = value;
                m_bools[RollupAttributesToParentSetIdx] = true;
            }
        }

        public bool RollupAttributesToParentSet => m_bools[RollupAttributesToParentSetIdx];

        private decimal m_cost;

        /// <summary>
        /// Per unit purchase cost. This value is used to calculate the value of inventory.
        /// </summary>
        public decimal Cost
        {
            get => m_cost;
            set
            {
                m_cost = value;
                m_bools[CostIsSetIdx] = true;
            }
        }

        public bool CostIsSet => m_bools[CostIsSetIdx];
        #endregion Shared Properties

        public void Validate(HashSet<string> a_ItemsIdList)
        {
            if (!a_ItemsIdList.Add(ExternalId))
            {
                //The item's external id has already been added to the transmission
                throw new PTValidationException("2825", new object[] { ExternalId });
            }

            Validate();

            //CAVAN:LOT should these be used?
            //if (BatchSizeSet && BatchSize <= 0)
            //    throw new ValidationException("2052", new object[] { this.ExternalId });
            //if (MinNbrBatchesSet && MinNbrBatches < 0)
            //    throw new ValidationException("2053", new object[] { this.ExternalId });
            //if (MaxNbrBatchesSet && MaxNbrBatches <= 0)
            //    throw new ValidationException("2054", new object[] { this.ExternalId });
            //if (BatchWindowSet && BatchWindow.Ticks == 0)
            //    throw new ValidationException("2055", new object[] { this.ExternalId });
            //if (MinNbrBatchesSet || MaxNbrBatchesSet)
            //{
            //    if (MinNbrBatches > MaxNbrBatches)
            //        throw new ValidationException("2056", new object[] { this.ExternalId });
            //}
            //if (JobAutoSplitQty > 0 && JobAutoSplitQty < MinOrderQty)
            //    throw new ValidationException("2057", new object[] { this.ExternalId });
        }

        public override void Validate()
        {
            base.Validate();
            ItemDefs.ValidateBatchingProperties(BatchSize, MinOrderQty, MaxOrderQty);

            if (ShelfLifeWarningHrsIsSet && (ShelfLifeWarningHrs < 0 || ShelfLifeWarningHrs > double.MaxValue))
            {
                throw new ValidationException("3114", new object[] { ExternalId, double.MaxValue });
            }
        }

        private decimal m_unitVolume;

        /// <summary>
        /// Per unit volume. This value is used to calculate the total required volume for a product during production.
        /// </summary>
        public decimal UnitVolume
        {
            get => m_unitVolume;
            set
            {
                m_unitVolume = value;
                m_bools[c_unitVolumeSetIdx] = true;
            }
        }

        public bool UnitVolumeIsSet => m_bools[c_unitVolumeSetIdx];

        private double m_shelfLifeWarningHrs;
        public double ShelfLifeWarningHrs
        {
            get => m_shelfLifeWarningHrs;
            set
            {
                m_shelfLifeWarningHrs = value;
                m_bools[c_shelfLifeWarningHrsSetIdx] = true;
            }
        }

        public bool ShelfLifeWarningHrsIsSet => m_bools[c_shelfLifeWarningHrsSetIdx];
    }
}