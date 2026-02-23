using PT.APSCommon.Extensions;

namespace PT.ERPTransmissions;

//This class is now only used to support backwards compatibility with recordings and undo checkpoints.
//It is not otherwise constructed or serialized. Items are now sent with the WarehouseT.
public class ItemT : ERPMaintenanceTransmission<WarehouseT.Item>, IPTSerializable
{
    public new const int UNIQUE_ID = 523;

    #region PT Serialization
    public ItemT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            int count;
            reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                WarehouseT.Item node = new (reader);
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

    public override string Description => string.Format("Items updated ({0})".Localize(), Count);

    //public void Validate()
    //{
    //    for (int i = 0; i < this.Nodes.Count; i++)
    //    {
    //        ItemT.Item item = (ItemT.Item)this.Nodes[i];
    //        item.Validate();
    //    }
    //}

    public ItemT() { }
    //public ItemT(PtImportDataSet ds)
    //{
    //    for (int i = 0; i < ds.Items.Count; i++)
    //        Add(new ItemT.Item(ds.Items[i]));
    //}

    //public class Item : PTObjectBase, PT.Common.IPTSerializable
    //{
    //    public new const int UNIQUE_ID = 524;
    //    #region PT Serialization

    //    public Item(PT.Common.IReader reader)
    //        : base(reader)
    //    {
    //        if (reader.VersionNumber >= 422)
    //        {
    //            int val;
    //            reader.Read(out val);
    //            this.source = (ItemDefs.sources)val;
    //            reader.Read(out val);
    //            this.itemType = (ItemDefs.itemTypes)val;
    //            reader.Read(out this.shelfLifeTicks);

    //            reader.Read(out this.sourceSet);
    //            reader.Read(out this.itemTypeSet);
    //            reader.Read(out this.defaultLeadTime);
    //            reader.Read(out this.defaultLeadTimeSet);
    //            reader.Read(out this.shelfLifeSet);
    //            bools = new BoolVector32(reader);
    //            reader.Read(out transferQty);
    //            reader.Read(out itemGroup);
    //            reader.Read(out this.batchSize);
    //            reader.Read(out this.minNbrBatches);
    //            reader.Read(out this.maxNbrBatches);
    //            reader.Read(out this.batchWindow);
    //            reader.Read(out this._jobAutoSplitQty);
    //            reader.Read(out this._minOrderQty);
    //            reader.Read(out this._minOrderQtyRoundupLimit);
    //            reader.Read(out m_cost);
    //            reader.Read(out m_lotControlled);
    //            reader.Read(out val);
    //            this.m_lotUsability = (ItemDefs.LotUsability)val;
    //        }
    //        #region Version 412
    //        else if (reader.VersionNumber >= 412)
    //        {
    //            int val;
    //            reader.Read(out val);
    //            this.source = (ItemDefs.sources)val;
    //            reader.Read(out val);
    //            this.itemType = (ItemDefs.itemTypes)val;
    //            reader.Read(out this.shelfLifeTicks);

    //            reader.Read(out this.sourceSet);
    //            reader.Read(out this.itemTypeSet);
    //            reader.Read(out this.defaultLeadTime);
    //            reader.Read(out this.defaultLeadTimeSet);
    //            reader.Read(out this.shelfLifeSet);
    //            bools = new BoolVector32(reader);
    //            reader.Read(out transferQty);
    //            reader.Read(out itemGroup);
    //            reader.Read(out this.batchSize);
    //            reader.Read(out this.minNbrBatches);
    //            reader.Read(out this.maxNbrBatches);
    //            reader.Read(out this.batchWindow);
    //            reader.Read(out this._jobAutoSplitQty);
    //            reader.Read(out this._minOrderQty);
    //            reader.Read(out this._minOrderQtyRoundupLimit);
    //            reader.Read(out m_cost);
    //        }
    //        #endregion
    //        #region Version 340
    //        else if (reader.VersionNumber >= 340)
    //        {
    //            int val;
    //            reader.Read(out val);
    //            this.source = (ItemDefs.sources)val;
    //            reader.Read(out val);
    //            this.itemType = (ItemDefs.itemTypes)val;
    //            reader.Read(out this.shelfLifeTicks);

    //            reader.Read(out this.sourceSet);
    //            reader.Read(out this.itemTypeSet);
    //            reader.Read(out this.defaultLeadTime);
    //            reader.Read(out this.defaultLeadTimeSet);
    //            reader.Read(out this.shelfLifeSet);
    //            bools = new BoolVector32(reader);
    //            reader.Read(out transferQty);
    //            reader.Read(out itemGroup);
    //            reader.Read(out this.batchSize);
    //            reader.Read(out this.minNbrBatches);
    //            reader.Read(out this.maxNbrBatches);
    //            reader.Read(out this.batchWindow);
    //            reader.Read(out this._jobAutoSplitQty);
    //            reader.Read(out this._minOrderQty);
    //            reader.Read(out this._minOrderQtyRoundupLimit);

    //        }
    //        #endregion
    //        #region Version 189
    //        else if (reader.VersionNumber >= 189)
    //        {
    //            int val;
    //            reader.Read(out val);
    //            this.source = (ItemDefs.sources)val;
    //            reader.Read(out val);
    //            this.itemType = (ItemDefs.itemTypes)val;
    //            reader.Read(out this.shelfLifeTicks);

    //            reader.Read(out this.sourceSet);
    //            reader.Read(out this.itemTypeSet);
    //            reader.Read(out this.defaultLeadTime);
    //            reader.Read(out this.defaultLeadTimeSet);
    //            reader.Read(out this.shelfLifeSet);
    //            bools = new BoolVector32(reader);
    //            reader.Read(out transferQty);
    //            reader.Read(out itemGroup);
    //            reader.Read(out this.batchSize);
    //            reader.Read(out this.minNbrBatches);
    //            reader.Read(out this.maxNbrBatches);
    //            reader.Read(out this.batchWindow);

    //        }
    //        #endregion
    //        #region Version 163
    //        else if (reader.VersionNumber >= 163)
    //        {
    //            int val;
    //            reader.Read(out val);
    //            this.source = (ItemDefs.sources)val;
    //            reader.Read(out val);
    //            this.itemType = (ItemDefs.itemTypes)val;
    //            reader.Read(out this.shelfLifeTicks);

    //            reader.Read(out this.sourceSet);
    //            reader.Read(out this.itemTypeSet);
    //            reader.Read(out this.defaultLeadTime);
    //            reader.Read(out this.defaultLeadTimeSet);
    //            reader.Read(out this.shelfLifeSet);
    //            bools = new BoolVector32(reader);
    //            reader.Read(out transferQty);
    //            reader.Read(out itemGroup);
    //        }
    //        #endregion
    //        #region Version 131
    //        else if (reader.VersionNumber >= 131)
    //        {
    //            int val;
    //            reader.Read(out val);
    //            this.source = (ItemDefs.sources)val;
    //            reader.Read(out val);
    //            this.itemType = (ItemDefs.itemTypes)val;
    //            reader.Read(out this.shelfLifeTicks);

    //            reader.Read(out this.sourceSet);
    //            reader.Read(out this.itemTypeSet);
    //            reader.Read(out this.defaultLeadTime);
    //            reader.Read(out this.defaultLeadTimeSet);
    //            reader.Read(out this.shelfLifeSet);
    //            bools = new BoolVector32(reader);
    //            reader.Read(out transferQty);
    //        }
    //        #endregion
    //        #region Version 111
    //        else if (reader.VersionNumber >= 111)
    //        {
    //            int val;
    //            reader.Read(out val);
    //            this.source = (ItemDefs.sources)val;
    //            reader.Read(out val);
    //            this.itemType = (ItemDefs.itemTypes)val;
    //            reader.Read(out this.shelfLifeTicks);

    //            reader.Read(out this.sourceSet);
    //            reader.Read(out this.itemTypeSet);
    //            reader.Read(out this.defaultLeadTime);
    //            reader.Read(out this.defaultLeadTimeSet);
    //            reader.Read(out this.shelfLifeSet);
    //        }
    //        #endregion
    //        #region Version 89
    //        else if (reader.VersionNumber >= 89)
    //        {
    //            int val;
    //            reader.Read(out val);
    //            this.source = (ItemDefs.sources)val;
    //            reader.Read(out val);
    //            this.itemType = (ItemDefs.itemTypes)val;

    //            reader.Read(out this.sourceSet);
    //            reader.Read(out this.itemTypeSet);
    //            reader.Read(out this.defaultLeadTime);
    //            reader.Read(out this.defaultLeadTimeSet);
    //        }
    //        #endregion
    //        #region Version 1
    //        else if (reader.VersionNumber >= 1)
    //        {
    //            int val;
    //            reader.Read(out val);
    //            this.source = (ItemDefs.sources)val;
    //            reader.Read(out val);
    //            reader.Read(out this.sourceSet);
    //            reader.Read(out this.itemTypeSet);
    //        }
    //        #endregion
    //    }

    //    public override void Serialize(PT.Common.IWriter writer)
    //    {
    //        base.Serialize(writer);

    //        writer.Write((int)this.source);
    //        writer.Write((int)this.itemType);
    //        writer.Write(this.shelfLifeTicks);

    //        writer.Write(this.sourceSet);
    //        writer.Write(this.itemTypeSet);
    //        writer.Write(this.defaultLeadTime);
    //        writer.Write(this.defaultLeadTimeSet);
    //        writer.Write(this.shelfLifeSet);
    //        bools.Serialize(writer);
    //        writer.Write(this.transferQty);
    //        writer.Write(itemGroup);
    //        writer.Write(batchSize);
    //        writer.Write(minNbrBatches);
    //        writer.Write(maxNbrBatches);
    //        writer.Write(batchWindow);
    //        writer.Write(_jobAutoSplitQty);
    //        writer.Write(_minOrderQty);
    //        writer.Write(_minOrderQtyRoundupLimit);
    //        writer.Write(m_cost);
    //        writer.Write(m_lotControlled);
    //        writer.Write((int)m_lotUsability);
    //    }

    //    public override int UniqueId
    //    {
    //        get
    //        {
    //            return UNIQUE_ID;
    //        }
    //    }

    //    #endregion

    //    #region bools
    //    BoolVector32 bools;
    //    const int TransferQtySetIdx = 0;
    //    const int ItemGroupSetIdx = 1;
    //    const int BatchSizeSetIdx = 2;
    //    const int MinNbrBatchesSetIdx = 3;
    //    const int MaxNbrBatchesSetIdx = 4;
    //    const int BatchWindowSetIdx = 5;
    //    const int PlanInventoryIdx = 6;
    //    const int PlanInventorySetIdx = 7;
    //    const int MinOrderQtySetIdx = 8;
    //    const int MinOrderQtyRoundupLimitSetIdx = 9;
    //    const int JobAutoSplitQtySetIdx = 10;
    //    const int RollupIdx = 11;
    //    const int RollupAttributesToParentSetIdx = 12;
    //    const int CostIsSetIdx = 13;
    //    const int LotControlledIdx = 14;

    //    #endregion

    //    public Item() { }  // reqd. for xml serialization
    //    public Item(string externalId, string name, string description, string notes, string userFields)
    //        : base(externalId, name, description, notes, userFields)
    //    {
    //    }

    //    public Item(PtImportDataSet.ItemsRow aRow)
    //        : base(aRow.ExternalId, aRow.IsNameNull() ? null : aRow.Name, aRow.IsDescriptionNull() ? null : aRow.Description, aRow.IsNotesNull() ? null : aRow.Notes, aRow.IsUserFieldsNull() ? null : aRow.UserFields)
    //    {

    //        if (!aRow.IsDefaultLeadTimeDaysNull())
    //            DefaultLeadTime = TimeSpan.FromDays(aRow.DefaultLeadTimeDays);
    //        if (!aRow.IsItemTypeNull())
    //            ItemType = (ItemDefs.itemTypes)Enum.Parse(typeof(ItemDefs.itemTypes), aRow.ItemType);
    //        if (!aRow.IsSourceNull())
    //            Source = (ItemDefs.sources)Enum.Parse(typeof(ItemDefs.sources), aRow.Source);
    //        if (!aRow.IsBatchSizeNull())
    //            BatchSize = aRow.BatchSize;
    //        if (!aRow.IsBatchWindowHrsNull())
    //            BatchWindow = aRow.BatchWindowHrs == TimeSpan.MaxValue.TotalHours ? TimeSpan.MaxValue : TimeSpan.FromHours(aRow.BatchWindowHrs);
    //        if (!aRow.IsItemGroupNull())
    //            ItemGroup = aRow.ItemGroup;
    //        if (!aRow.IsMaxNbrBatchesNull())
    //            MaxNbrBatches = aRow.MaxNbrBatches;
    //        if (!aRow.IsMinNbrBatchesNull())
    //            MinNbrBatches = aRow.MinNbrBatches;
    //        if (!aRow.IsShelfLifeHrsNull())
    //            ShelfLife = aRow.ShelfLifeHrs == TimeSpan.MaxValue.TotalHours ? TimeSpan.MaxValue : TimeSpan.FromHours(aRow.ShelfLifeHrs); //Overflows doing the conversion otherwise
    //        if (!aRow.IsTransferQtyNull())
    //            TransferQty = aRow.TransferQty;
    //        if (!aRow.IsPlanInventoryNull())
    //            PlanInventory = aRow.PlanInventory;
    //        if (!aRow.IsMinOrderQtyNull())
    //            MinOrderQty = aRow.MinOrderQty;
    //        if (!aRow.IsMinOrderQtyRoundupLimitNull())
    //            MinOrderQtyRoundupLimit = aRow.MinOrderQtyRoundupLimit;
    //        if (!aRow.IsJobAutoSplitQtyNull())
    //            JobAutoSplitQty = aRow.JobAutoSplitQty;
    //        if (!aRow.IsRollupAttributesToParentNull())
    //            RollupAttributesToParent = aRow.RollupAttributesToParent;
    //        if (!aRow.IsCostNull())
    //            Cost = aRow.Cost;
    //        if (!aRow.IsLotControlledNull())
    //            LotControlled = aRow.LotControlled;
    //        if (!aRow.IsLotUsabilityNull())
    //            LotUsability = (ItemDefs.LotUsability)aRow.LotUsability;
    //    }

    //    #region Shared Properties

    //    ItemDefs.sources source;
    //    /// <summary>
    //    /// Where the Item originates from.  This is for information only and has no effect on scheduling.
    //    /// </summary>
    //    public ItemDefs.sources Source
    //    {
    //        get { return this.source; }
    //        set
    //        {
    //            this.source = value;
    //            this.sourceSet = true;
    //        }
    //    }
    //    bool sourceSet = false;
    //    public bool SourcSet
    //    {
    //        get { return this.sourceSet; }
    //    }

    //    ItemDefs.itemTypes itemType;
    //    /// <summary>
    //    /// For information only.
    //    /// </summary>
    //    public ItemDefs.itemTypes ItemType
    //    {
    //        get { return this.itemType; }
    //        set
    //        {
    //            this.itemType = value;
    //            this.itemTypeSet = true;
    //        }
    //    }
    //    bool itemTypeSet = false;
    //    /// <summary>
    //    /// Indicates the primary use of the Item.  This is for information only and has no effect on scheduling.
    //    /// </summary>				
    //    public bool ItemTypeSet
    //    {
    //        get { return this.itemTypeSet; }
    //    }

    //    ItemDefs.LotUsability m_lotUsability = ItemDefs.LotUsability.Uncontrolled;
    //    /// <summary>
    //    /// For information only.
    //    /// </summary>
    //    public ItemDefs.LotUsability LotUsability
    //    {
    //        get { return this.m_lotUsability; }
    //        set
    //        {
    //            this.m_lotUsability = value;
    //            this.lotUsabilitySet = true;
    //        }
    //    }
    //    bool lotUsabilitySet = false;
    //    /// <summary>
    //    /// Indicates the usability of the inventory's lots.
    //    /// </summary>				
    //    public bool LotUsabilitySet
    //    {
    //        get { return this.lotUsabilitySet; }
    //    }

    //    long defaultLeadTime;
    //    /// <summary>
    //    /// Used by Material Requirements when there is no Inventory record for the Item at any available Warehouse.
    //    /// </summary>
    //    public TimeSpan DefaultLeadTime
    //    {
    //        get { return TimeSpan.FromTicks(this.defaultLeadTime); }
    //        set
    //        {
    //            this.defaultLeadTime = value.Ticks;
    //            this.defaultLeadTimeSet = true;
    //        }
    //    }

    //    bool defaultLeadTimeSet = false;
    //    public bool DefaultLeadTimeSet
    //    {
    //        get { return this.defaultLeadTimeSet; }
    //    }

    //    long shelfLifeTicks;
    //    /// <summary>
    //    /// The maximum amount of time that the Item can remain in inventory before being used.
    //    /// </summary>
    //    public TimeSpan ShelfLife
    //    {
    //        get { return new TimeSpan(shelfLifeTicks); }
    //        set
    //        {
    //            this.shelfLifeTicks = value.Ticks;
    //            this.shelfLifeSet = true;
    //        }
    //    }
    //    bool shelfLifeSet = false;
    //    public bool ShelfLifeSet
    //    {
    //        get { return this.shelfLifeSet; }
    //    }

    //    public bool TransferQtySet
    //    {
    //        get
    //        {
    //            return bools[TransferQtySetIdx];
    //        }
    //    }

    //    decimal transferQty;
    //    /// <summary>
    //    /// As this product is produced it is transferred to Inventory in quantities of this size.
    //    /// </summary>
    //    public decimal TransferQty
    //    {
    //        get
    //        {
    //            return transferQty;
    //        }

    //        set
    //        {
    //            if (value < 0)
    //            {
    //                throw new ValidationException("2051");
    //            }

    //            bools[TransferQtySetIdx] = true;
    //            transferQty = value;
    //        }
    //    }

    //    string itemGroup;
    //    /// <summary>
    //    /// Can be used for visually grouping Items into categories.
    //    /// </summary>
    //    public string ItemGroup
    //    {
    //        get { return itemGroup; }
    //        set
    //        {
    //            itemGroup = value;
    //            bools[ItemGroupSetIdx] = true;
    //        }
    //    }

    //    public bool ItemGroupSet
    //    {
    //        get
    //        {
    //            return bools[ItemGroupSetIdx];
    //        }
    //    }

    //    decimal batchSize;
    //    /// <summary>
    //    /// Jobs and Purchases must be in integer multiples of this value.
    //    /// </summary>
    //    public decimal BatchSize
    //    {
    //        get { return batchSize; }
    //        set
    //        {
    //            batchSize = value;
    //            bools[BatchSizeSetIdx] = true;
    //        }
    //    }
    //    public bool BatchSizeSet
    //    {
    //        get
    //        {
    //            return bools[BatchSizeSetIdx];
    //        }
    //    }

    //    int minNbrBatches;
    //    /// <summary>
    //    /// Each Job and Purchase for this item must be at least this number of Batches (see Batch Size).
    //    /// </summary>
    //    public int MinNbrBatches
    //    {
    //        get { return minNbrBatches; }
    //        set
    //        {
    //            minNbrBatches = value;
    //            bools[MinNbrBatchesSetIdx] = true;
    //        }
    //    }
    //    public bool MinNbrBatchesSet
    //    {
    //        get
    //        {
    //            return bools[MinNbrBatchesSetIdx];
    //        }
    //    }

    //    int maxNbrBatches;
    //    /// <summary>
    //    /// Each Job and Purchase for this item must be at no more than this number of Batches (see Batch Size).
    //    /// </summary>
    //    public int MaxNbrBatches
    //    {
    //        get { return maxNbrBatches; }
    //        set
    //        {
    //            maxNbrBatches = value;
    //            bools[MaxNbrBatchesSetIdx] = true;
    //        }
    //    }
    //    public bool MaxNbrBatchesSet
    //    {
    //        get
    //        {
    //            return bools[MaxNbrBatchesSetIdx];
    //        }
    //    }

    //    TimeSpan batchWindow;
    //    /// <summary>
    //    /// When batching demand to create Jobs or Purchases the batch should not reach out further than this value in order to reach the Max Nbr Batches.
    //    /// </summary>
    //    public TimeSpan BatchWindow
    //    {
    //        get { return batchWindow; }
    //        set
    //        {
    //            batchWindow = value;
    //            bools[BatchWindowSetIdx] = true;
    //        }
    //    }
    //    public bool BatchWindowSet
    //    {
    //        get
    //        {
    //            return bools[BatchWindowSetIdx];
    //        }
    //    }

    //    /// <summary>
    //    /// Whether the Item should be included in the Inventory Plan.
    //    /// </summary>
    //    public bool PlanInventory
    //    {
    //        get { return bools[PlanInventoryIdx]; }
    //        set
    //        {
    //            bools[PlanInventoryIdx] = value;
    //            bools[PlanInventorySetIdx] = true;
    //        }
    //    }

    //    public bool PlanInventorySet
    //    {
    //        get { return bools[PlanInventorySetIdx]; }
    //    }

    //    decimal _minOrderQty;
    //    /// <summary>
    //    /// Jobs and Purchase Orders must be for at least this quantity when created by EMS.
    //    /// If a planned Job, after rounding up by MinOrderQtyRoundupLimit, is less than this amount then it will be marked as Do Not Schedule and remain unscheduled.
    //    /// If a planned Purchase Order, after rounding up by MinOrderQtyRoundupLimit, is less than this amount then it wll have "Less than Min Order Qty" set to true.
    //    /// </summary>
    //    public decimal MinOrderQty
    //    {
    //        get { return _minOrderQty; }
    //        set
    //        {
    //            _minOrderQty = value;
    //            bools[MinOrderQtySetIdx] = true;
    //        }
    //    }

    //    public bool MinOrderQtySet
    //    {
    //        get { return bools[MinOrderQtySetIdx]; }
    //    }

    //    decimal _minOrderQtyRoundupLimit;
    //    /// <summary>
    //    /// If a Job or Purchase Order created by EMS is less than this quantity then EMS is allowed to round-up the quantity by as much as this amount in order to reach the MinOrderQty.
    //    /// Take care when setting this to large values as it can create extra inventory.
    //    /// </summary>
    //    public decimal MinOrderQtyRoundupLimit
    //    {
    //        get { return _minOrderQtyRoundupLimit; }
    //        set
    //        {
    //            _minOrderQtyRoundupLimit = value;
    //            bools[MinOrderQtyRoundupLimitSetIdx] = true;
    //        }
    //    }

    //    public bool MinOrderQtyRoundupLimitSet
    //    {
    //        get { return bools[MinOrderQtyRoundupLimitSetIdx]; }
    //    }

    //    decimal _jobAutoSplitQty;
    //    /// <summary>
    //    /// Jobs generated by EMS that exceed this quantity have Manfacturing Orders split off into quantities of this size until no Manufacturing Order in the Job exceeds this quantity.
    //    /// </summary>
    //    public decimal JobAutoSplitQty
    //    {
    //        get { return _jobAutoSplitQty; }
    //        set
    //        {
    //            _jobAutoSplitQty = value;
    //            bools[JobAutoSplitQtySetIdx] = true;
    //        }
    //    }

    //    public bool JobAutoSplitQtySet
    //    {
    //        get { return bools[JobAutoSplitQtySetIdx]; }
    //    }

    //    /// <summary>
    //    /// If true then when MRP creates Jobs it will recursively copy Operation Attributes from supplying Job Templates to the Operation consuming the Material.
    //    /// This is often used to carry-up Attributes from "allergens" in materials.
    //    /// </summary>
    //    public bool RollupAttributesToParent
    //    {
    //        get { return bools[RollupIdx]; }
    //        set
    //        {
    //            bools[RollupIdx] = value;
    //            bools[RollupAttributesToParentSetIdx] = true;
    //        }
    //    }

    //    public bool RollupAttributesToParentSet
    //    {
    //        get { return bools[RollupAttributesToParentSetIdx]; }
    //    }

    //    decimal m_cost;
    //    /// <summary>
    //    /// Per unit purchase cost. This value is used to calculate the value of inventory.
    //    /// </summary>
    //    public decimal Cost
    //    {
    //        get { return m_cost; }
    //        internal set
    //        {
    //            m_cost = value;
    //            bools[CostIsSetIdx] = true;
    //        }
    //    }

    //    public bool CostIsSet
    //    {
    //        get { return bools[CostIsSetIdx]; }
    //    }

    //    public bool LotControlledIsSet
    //    {
    //        get { return bools[LotControlledIdx]; }
    //    }

    //    bool m_lotControlled = false;
    //    public bool LotControlled
    //    {
    //        get { return m_lotControlled; }
    //        internal set
    //        {
    //            m_lotControlled = value;
    //            bools[LotControlledIdx] = true;
    //        }
    //    }

    //    #endregion Shared Properties

    //    public override void Validate()
    //    {
    //        if (BatchSizeSet && BatchSize <= 0)
    //            throw new ValidationException("2052", new object[] { this.ExternalId });
    //        if (MinNbrBatchesSet && MinNbrBatches < 0)
    //            throw new ValidationException("2053", new object[] { this.ExternalId });
    //        if (MaxNbrBatchesSet && MaxNbrBatches <= 0)
    //            throw new ValidationException("2054", new object[] { this.ExternalId });
    //        if (BatchWindowSet && BatchWindow.Ticks == 0)
    //            throw new ValidationException("2055", new object[] { this.ExternalId });
    //        if (MinNbrBatchesSet || MaxNbrBatchesSet)
    //        {
    //            if (MinNbrBatches > MaxNbrBatches)
    //                throw new ValidationException("2056", new object[] { this.ExternalId });
    //        }
    //        if (JobAutoSplitQty > 0 && JobAutoSplitQty < MinOrderQty)
    //            throw new ValidationException("2057", new object[] { this.ExternalId });
    //    }
    //}

    //public Item GetByIndex(int i)
    //{
    //    return (Item)Nodes[i];
    //}

    //#region Database Loading

    //public void Fill(System.Data.IDbCommand cmd)
    //{
    //    PtImportDataSet.ItemsDataTable table = new PtImportDataSet.ItemsDataTable();

    //    base.FillTable(table, cmd);
    //    Fill(table);
    //}

    ///// <summary>
    ///// Fill the transmission with data from the DataSet.
    ///// </summary>
    //public void Fill(PtImportDataSet.ItemsDataTable aTable)
    //{
    //    for (int i = 0; i < aTable.Count; i++)
    //        this.Add(new Item(aTable[i]));
    //}
    //#endregion
}