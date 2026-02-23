using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Common.Attributes;
using PT.ERPTransmissions;
using PT.SchedulerDefinitions;
using PT.Transmissions;
using System.ComponentModel;

namespace PT.Scheduler;

/// <summary>
/// Represents a part that is either made or purchased.  It can be a raw material, subassembly, or finished good.
/// </summary>
public partial class Item : BaseObject, ICloneable, IPTSerializable
{
    public new const int UNIQUE_ID = 529;

    #region IPTSerializable Members
    public Item(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12529)
        {
            a_reader.Read(out int val);
            m_itemType = (ItemDefs.itemTypes)val;

            a_reader.Read(out val);
            m_source = (ItemDefs.sources)val;

            a_reader.Read(out m_defaultLeadTime);
            a_reader.Read(out m_shelfLifeTicks);
            a_reader.Read(out m_transferQty);
            a_reader.Read(out m_itemGroup);
            a_reader.Read(out m_batchSize);
            a_reader.Read(out m_batchWindow);
            a_reader.Read(out m_planInventory);
            a_reader.Read(out m_jobAutoSplitQty);
            a_reader.Read(out m_minOrderQty);
            a_reader.Read(out m_minOrderQtyRoundupLimit);
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_cost);

            a_reader.Read(out m_maxOrderQty);

            a_reader.Read(out m_unitVolume);
            a_reader.Read(out m_shelfLifeWarningTicks);
        }
        else if (a_reader.VersionNumber >= 12511)
        {
            a_reader.Read(out int val);
            m_itemType = (ItemDefs.itemTypes)val;

            a_reader.Read(out val);
            m_source = (ItemDefs.sources)val;

            a_reader.Read(out m_defaultLeadTime);
            a_reader.Read(out m_shelfLifeTicks);
            a_reader.Read(out m_transferQty);
            a_reader.Read(out m_itemGroup);
            a_reader.Read(out m_batchSize);
            a_reader.Read(out m_batchWindow);
            a_reader.Read(out m_planInventory);
            a_reader.Read(out m_jobAutoSplitQty);
            a_reader.Read(out m_minOrderQty);
            a_reader.Read(out m_minOrderQtyRoundupLimit);
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_cost);

            a_reader.Read(out m_maxOrderQty);

            a_reader.Read(out m_unitVolume);
            a_reader.Read(out double m_shelfLifeWarningHrs);
            m_shelfLifeWarningTicks = TimeSpan.FromHours(m_shelfLifeWarningHrs).Ticks;
        }
        else if (a_reader.VersionNumber >= 12506)
        {
            int val;
            a_reader.Read(out val);
            m_itemType = (ItemDefs.itemTypes)val;

            a_reader.Read(out val);
            m_source = (ItemDefs.sources)val;

            a_reader.Read(out m_defaultLeadTime);
            a_reader.Read(out m_shelfLifeTicks);
            a_reader.Read(out m_transferQty);
            a_reader.Read(out m_itemGroup);
            a_reader.Read(out m_batchSize);
            a_reader.Read(out m_batchWindow);
            a_reader.Read(out m_planInventory);
            a_reader.Read(out m_jobAutoSplitQty);
            a_reader.Read(out m_minOrderQty);
            a_reader.Read(out m_minOrderQtyRoundupLimit);
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_cost);

            a_reader.Read(out int lotUsabilityVal);
            a_reader.Read(out m_maxOrderQty);

            a_reader.Read(out m_unitVolume);
            a_reader.Read(out double m_shelfLifeWarningHrs);
            m_shelfLifeWarningTicks = TimeSpan.FromHours(m_shelfLifeWarningHrs).Ticks;
        }
        else if (a_reader.VersionNumber >= 12106)
        {
            int val;
            a_reader.Read(out val);
            m_itemType = (ItemDefs.itemTypes)val;

            a_reader.Read(out val);
            m_source = (ItemDefs.sources)val;

            a_reader.Read(out m_defaultLeadTime);
            a_reader.Read(out m_shelfLifeTicks);
            a_reader.Read(out m_transferQty);
            a_reader.Read(out m_itemGroup);
            a_reader.Read(out m_batchSize);
            a_reader.Read(out m_batchWindow);
            a_reader.Read(out m_planInventory);
            a_reader.Read(out m_jobAutoSplitQty);
            a_reader.Read(out m_minOrderQty);
            a_reader.Read(out m_minOrderQtyRoundupLimit);
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_cost);

            a_reader.Read(out int lotUsabilityVal);
            a_reader.Read(out m_maxOrderQty);

            a_reader.Read(out m_unitVolume);
        }
        else if (a_reader.VersionNumber >= 731)
        {
            int val;
            a_reader.Read(out val);
            m_itemType = (ItemDefs.itemTypes)val;

            a_reader.Read(out val);
            m_source = (ItemDefs.sources)val;

            a_reader.Read(out m_defaultLeadTime);
            a_reader.Read(out m_shelfLifeTicks);
            a_reader.Read(out m_transferQty);
            a_reader.Read(out m_itemGroup);
            a_reader.Read(out m_batchSize);
            a_reader.Read(out m_batchWindow);
            a_reader.Read(out m_planInventory);
            a_reader.Read(out m_jobAutoSplitQty);
            a_reader.Read(out m_minOrderQty);
            a_reader.Read(out m_minOrderQtyRoundupLimit);
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_cost);

            a_reader.Read(out int lotUsabilityVal);
            a_reader.Read(out m_maxOrderQty);
        }
    }

    /// <summary>
    /// To avoid overflow issues with large shelflife values, this function caps shelflife to its maximum value.
    /// This is for backwards compatibility. Other validation should prevent large shelflife values from being set.
    /// </summary>
    private void ResetShelfLifeDefault()
    {
        if (m_shelfLifeTicks > 365 * TimeSpan.TicksPerDay * 100) //100 years
        {
            m_shelfLifeTicks = c_defaultShelfLifeTicks;
        }
    }

    /// <summary>
    /// this is uses deserialized MinNbrBatches and MaxNbrOfBatches to set MinOrderQty and MaxOrderQty.
    /// </summary>
    private void UpgradeBatchingProperties(decimal a_minNbrBatches, decimal a_maxNbrBatches)
    {
        if (a_minNbrBatches != 0 && MinOrderQty == 1)
        {
            MinOrderQty = a_minNbrBatches * BatchSize;
        }

        if (a_maxNbrBatches != 0)
        {
            MaxOrderQty = a_maxNbrBatches * BatchSize;
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write((int)m_itemType);
        a_writer.Write((int)m_source);
        a_writer.Write(m_defaultLeadTime);
        a_writer.Write(m_shelfLifeTicks);
        a_writer.Write(m_transferQty);
        a_writer.Write(m_itemGroup);
        a_writer.Write(m_batchSize);
        a_writer.Write(m_batchWindow);
        a_writer.Write(m_planInventory);
        a_writer.Write(m_jobAutoSplitQty);
        a_writer.Write(m_minOrderQty);
        a_writer.Write(m_minOrderQtyRoundupLimit);
        m_bools.Serialize(a_writer);
        a_writer.Write(m_cost);
        a_writer.Write(m_maxOrderQty);

        a_writer.Write(m_unitVolume);
        a_writer.Write(m_shelfLifeWarningTicks);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    /// <summary>
    /// Sets the field values for an ERP transmission.
    /// </summary>
    internal Item(UserFieldDefinitionManager a_udfManager, IScenarioDataChanges a_dataChanges, BaseId id, WarehouseT.Item item, PTTransmission t, JobManager a_jobs)
        : base(id)
    {
        a_dataChanges.ItemChanges.AddedObject(id);
        Update(a_udfManager, a_dataChanges, item, t, a_jobs);
    }

    public Item(BaseId a_id) : base(a_id) { }
    #endregion

    #region Shared Properties
    public decimal GetMinNbrBatches()
    {
        decimal minNbrBatches = 1;
        if (m_minOrderQty > m_batchSize)
        {
            minNbrBatches = m_batchSize > 0 ? Math.Ceiling(m_minOrderQty / m_batchSize) : 0;
        }

        return minNbrBatches;
    }

    public decimal GetMaxNbrBatches()
    {
        decimal maxNbrBatches = 1;
        if (m_maxOrderQty > m_batchSize)
        {
            maxNbrBatches = m_batchSize > 0 ? Math.Ceiling(m_maxOrderQty / m_batchSize) : 0;
        }

        return maxNbrBatches;
    }

    private ItemDefs.sources m_source;

    /// <summary>
    /// Where the Item originates from.  This is for information only and has no effect on scheduling.
    /// </summary>
    public ItemDefs.sources Source
    {
        get => m_source;
        private set => m_source = value;
    }

    private ItemDefs.itemTypes m_itemType;

    public ItemDefs.itemTypes ItemType
    {
        get => m_itemType;
        private set => m_itemType = value;
    }

    private long m_defaultLeadTime;

    /// <summary>
    /// Used by Material Requirements when there is no Inventory record for the Item at any available Warehouse.
    /// </summary>
    public TimeSpan DefaultLeadTime
    {
        get => TimeSpan.FromTicks(m_defaultLeadTime);
        private set => m_defaultLeadTime = value.Ticks;
    }

    private const long c_defaultShelfLifeTicks = 0;
    private long m_shelfLifeTicks = c_defaultShelfLifeTicks;
    [DoNotAuditProperty]
    internal bool HasShelfLife => ShelfLifeTicks > c_defaultShelfLifeTicks;
    [DoNotAuditProperty]
    internal long ShelfLifeTicks
    {
        get => m_shelfLifeTicks;
        private set => m_shelfLifeTicks = value;
    }

    internal long CalcExpiration(long a_productionTicks)
    {
        long expiration = a_productionTicks + ShelfLifeTicks;
        return expiration;
    }
    
    internal long CalcExpirationWarning(long a_productionTicks)
    {
        long warning = a_productionTicks + m_shelfLifeWarningTicks;
        return warning;
    }

    /// <summary>
    /// The maximum amount of time that the Item can remain in inventory before being used.
    /// This has no direct affect on scheduling but a Flag can be shown in the Gantt if this is violated.
    /// </summary>
    public TimeSpan ShelfLife
    {
        get => new (m_shelfLifeTicks);
        private set => m_shelfLifeTicks = value.Ticks;
    }

    private decimal m_transferQty;

    /// <summary>
    /// As this product is produced it is transferred to Inventory in quantities of this size.
    /// </summary>
    public decimal TransferQty
    {
        get => m_transferQty;
        private set => m_transferQty = value;
    }

    private string m_itemGroup;

    /// <summary>
    /// Can be used for visually grouping Items into categories.
    /// </summary>
    public string ItemGroup
    {
        get => m_itemGroup;
        private set => m_itemGroup = value;
    }

    private decimal m_batchSize;

    /// <summary>
    /// Jobs and Purchases must be in integer multiples of this value.
    /// For Batching to take place this must be greater than zero, MinNbrBatches must be greater than zero, and MaxNbrBatches must be greater than or equal to MinNbrBatches.
    /// </summary>
    public decimal BatchSize
    {
        get => m_batchSize;
        private set => m_batchSize = value;
    }

    private long m_batchWindow;

    /// <summary>
    /// When batching demand to create Jobs or Purchases Orders the batch should not reach out further than this value in order to reach the Max Nbr Batches.
    /// </summary>
    public TimeSpan BatchWindow
    {
        get => new (m_batchWindow);
        private set => m_batchWindow = value.Ticks;
    }
    [DoNotAuditProperty]
    internal long BatchWindowTicks
    {
        get => m_batchWindow;
        private set => m_batchWindow = value;
    }

    /// <summary>
    /// Whether the Item is able to be batched.
    /// To enable batching for an Item:
    /// 1.	Batch Size must be greater than zero
    /// 2.	Batch Window must be greater than zero
    /// 3.	MinOrderQty must be greater than zero
    /// 4.	MaxOrderQty must be greater than MinOrderQty
    /// </summary>
    /// <returns></returns>
    public bool CanBatch => BatchSize > 0 && BatchWindowTicks > 0 && MinOrderQty > 0 && MaxOrderQty >= MinOrderQty;

    private bool m_planInventory = true; //default to show all in plan.

    /// <summary>
    /// Whether the Item should be included in the Inventory Plan.
    /// </summary>
    public bool PlanInventory
    {
        get => m_planInventory;
        private set => m_planInventory = value;
    }

    private decimal m_minOrderQty;

    /// <summary>
    /// Jobs and Purchase Orders must be for at least this quantity when created by EMS.
    /// If a planned Job, after rounding up by MinOrderQtyRoundupLimit, is less than this amount then it will be marked as Do Not Schedule and remain unscheduled.
    /// If a planned Purchase Order, after rounding up by MinOrderQtyRoundupLimit, is less than this amount then it wll have "Less than Min Order Qty" set to true.
    /// JobAutoSplitQty can be used for a result similar to a MaxOrderQty. In MRP it will split MOs to enforce a Max quantity.
    /// </summary>
    public decimal MinOrderQty
    {
        get => m_minOrderQty;
        private set
        {
            if (value < 0)
            {
                throw new PTValidationException("2174");
            }

            m_minOrderQty = value;
        }
    }

    private decimal m_maxOrderQty;

    public decimal MaxOrderQty
    {
        get => m_maxOrderQty;
        set => m_maxOrderQty = value;
    }

    private decimal m_minOrderQtyRoundupLimit;

    /// <summary>
    /// If a Job or Purchase Order created by EMS is less than this quantity then EMS is allowed to round-up the quantity by as much as this amount in order to reach the MinOrderQty.
    /// Take care when setting this to large values as it can create extra inventory.
    /// </summary>
    public decimal MinOrderQtyRoundupLimit
    {
        get => m_minOrderQtyRoundupLimit;
        private set
        {
            if (value < 0)
            {
                throw new PTValidationException("2175");
            }

            m_minOrderQtyRoundupLimit = value;
        }
    }

    private decimal m_jobAutoSplitQty;

    /// <summary>
    /// Jobs generated by EMS that exceed this quantity have Manfacturing Orders split off into quantities of this size until no Manufacturing Order in the Job exceeds this quantity.
    /// </summary>
    public decimal JobAutoSplitQty
    {
        get => m_jobAutoSplitQty;
        private set
        {
            if (value < 0)
            {
                throw new PTValidationException("2176");
            }

            m_jobAutoSplitQty = value;
        }
    }

    private decimal m_cost;

    /// <summary>
    /// Per unit purchase cost. This value is used to calculate the value of inventory.
    /// </summary>
    public decimal Cost
    {
        get => m_cost;
        internal set => m_cost = value;
    }

    private long m_shelfLifeWarningTicks;
    
    /// <summary>
    ///  Represents the number of hours after an item is produced or received to trigger a warning
    /// </summary>
    public long ShelfLifeWarningTicks
    {
        get => m_shelfLifeWarningTicks;
        internal set => m_shelfLifeWarningTicks = value;
    }

    public TimeSpan ShelfLifeWarning => new TimeSpan(m_shelfLifeWarningTicks);
    
    /// <summary>
    /// Determines if an item was consumed after the defined threshold <see cref="ShelfLifeWarningHrs"/>
    /// has passed since its production or receipt. 
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_internalOperation"></param>
    /// <returns>True, if the first time the item was consumed exceeds the ShelfLifeWarningHrs after the production or receipt date
    /// </returns>
    public bool IsNearlyExpired(ScenarioDetail a_sd, InternalOperation a_internalOperation)
    {
        foreach (InternalActivity internalActivity in a_internalOperation.Activities)
        {
            if (IsNearlyExpired(a_sd, internalActivity))
            {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// Determines if an item was consumed after the defined threshold <see cref="ShelfLifeWarningHrs"/>
    /// has passed since its production or receipt. 
    /// </summary>
    /// <param name="a_sd"></param>
    /// <param name="a_activity"></param>
    /// <returns>True, if the first time the item was consumed exceeds the ShelfLifeWarningHrs after the production or receipt date
    /// </returns>
    public bool IsNearlyExpired(ScenarioDetail a_sd, InternalActivity a_activity)
    {
        foreach (Warehouse warehouse in a_sd.WarehouseManager)
        {
            DateTime received = PTDateTime.MinDateTime;
            DateTime consumptionDate = PTDateTime.MinDateTime;

            Inventory inventory = warehouse.Inventories[Id];
            if (inventory == null)
            {
                continue;
            }

            IEnumerator<Lot> enumerator = inventory.Lots.GetEnumerator();

            while (enumerator.MoveNext())
            {
                Lot current = enumerator.Current;
                if (received < current.ProductionDate)
                {
                    received = current.ProductionDate;
                }
            }
            enumerator.Dispose();

            List<PurchaseToStock> purchases = new();
            HashSet<BaseId> baseIds = new();
            for (int i = 0; i < inventory.Adjustments.Count; i++)
            {
                Adjustment adj = inventory.Adjustments[i];
                if (adj.ChangeQty < 0 && adj is ActivityAdjustment actAdj && actAdj.Activity.Id.Equals(a_activity.Id))
                {
                    consumptionDate = adj.AdjDate;
                    break;
                }
            }

            return consumptionDate > received.AddTicks(m_shelfLifeWarningTicks);
        }

        return false;
    }

    #region Bools
    private BoolVector32 m_bools;
    private const int RollupIdx = 0;
    private const int Unused1Idx = 1;

    /// <summary>
    /// If true then when MRP creates Jobs it will recursively copy Operation Attributes from supplying Job Templates to the Operation consuming the Material.
    /// This is often used to carry-up Attributes from "allergens" in materials.
    /// </summary>
    public bool RollupAttributesToParent
    {
        get => m_bools[RollupIdx];
        internal set => m_bools[RollupIdx] = value;
    }
    #endregion Bools
    #endregion Shared Properties

    #region Properties
    [DoNotAuditProperty]
    internal long DefaultLeadTimeTicks => m_defaultLeadTime;
    #endregion

    #region Overrides
    /// <summary>
    /// Used as a prefix for generating default names
    /// </summary>
    [Browsable(false)]
    public override string DefaultNamePrefix => "Item";
    #endregion

    #region ERP Transmissions
    private void Validate(WarehouseT.Item a_item)
    {
        ItemDefs.ValidateBatchingProperties(a_item.BatchSizeSet ? a_item.BatchSize : BatchSize, a_item.MinOrderQtySet ? a_item.MinOrderQty : MinOrderQty, a_item.MaxOrderQtySet ? a_item.MaxOrderQty : MaxOrderQty);

        if (a_item.JobAutoSplitQty > 0 && a_item.MinOrderQty > a_item.JobAutoSplitQty) //if using JobAutoSplit, can't have min greater than auto split qty or they conflict.
        {
            throw new PTValidationException("2177", new object[] { ExternalId, a_item.MinOrderQty, a_item.JobAutoSplitQty });
        }

        //Validate for shelflife values.
        if (a_item.ShelfLifeSet)
        {
            //must be set from 0 to 100 years
            if (a_item.ShelfLife > TimeSpan.FromDays(36500) || a_item.ShelfLife.Ticks < 0)
            {
                a_item.ShelfLife = TimeSpan.FromDays(36500);
            }
            else if (a_item.ShelfLife.Ticks < 0)
            {
                throw new PTValidationException("2940", new object[] { ExternalId });
            }
        }
    }

    public void Edit(IScenarioDataChanges a_dataChanges, ItemEdit a_edit, JobManager a_jobManager)
    {
        base.Edit(a_edit);

        if (a_edit.SourceSet)
        {
            Source = a_edit.Source;
        }

        if (a_edit.ItemTypeSet)
        {
            ItemType = a_edit.ItemType;
        }

        if (a_edit.BatchSizeSet)
        {
            BatchSize = a_edit.BatchSize;
        }

        if (a_edit.ItemGroupSet)
        {
            ItemGroup = a_edit.ItemGroup;
        }

        if (a_edit.MinOrderQtySet)
        {
            MinOrderQty = a_edit.MinOrderQty;
        }

        if (a_edit.MaxOrderQtySet)
        {
            MaxOrderQty = a_edit.MaxOrderQty;
        }

        if (a_edit.BatchWindowSet)
        {
            BatchWindow = a_edit.BatchWindow;
        }

        if (a_edit.MinOrderQtyRoundupLimitSet)
        {
            MinOrderQtyRoundupLimit = a_edit.MinOrderQtyRoundupLimit;
        }

        if (a_edit.JobAutoSplitQtySet)
        {
            JobAutoSplitQty = a_edit.JobAutoSplitQty;
        }

        if (a_edit.DefaultLeadTimeSet)
        {
            DefaultLeadTime = a_edit.DefaultLeadTime;
        }

        if (a_edit.PlanInventorySet)
        {
            PlanInventory = a_edit.PlanInventory;
        }

        if (a_edit.TransferQtySet)
        {
            m_transferQty = a_edit.TransferQty;
        }

        if (a_edit.CostIsSet)
        {
            Cost = a_edit.Cost;
        }

        if (a_edit.ShelfLifeSet)
        {
            ShelfLife = a_edit.ShelfLife;
        }

        if (a_edit.UnitVolumeIsSet && UnitVolume != a_edit.UnitVolume)
        {
            UnitVolume = a_edit.UnitVolume;
            a_dataChanges.FlagEligibilityChanges(Id);
        }

        if (a_edit.ShelfLifeWarningHrsIsSet)
        {
            m_shelfLifeWarningTicks = TimeSpan.FromHours(a_edit.ShelfLifeWarningHrs).Ticks;
        }
    }

    /// <summary>
    /// Updates Item and returns whether the item was lot controlled and after the update is not.
    /// </summary>
    internal void Update(UserFieldDefinitionManager a_udfManager, IScenarioDataChanges a_dataChanges, WarehouseT.Item a_item, PTTransmission a_t, JobManager a_jobs)
    {
        Validate(a_item);

        bool updated = base.Update(a_item, a_t, a_udfManager, UserField.EUDFObjectType.Items);

        if (a_item.SourcSet && Source != a_item.Source)
        {
            Source = a_item.Source;
            updated = true;
        }

        if (a_item.ItemTypeSet && ItemType != a_item.ItemType)
        {
            ItemType = a_item.ItemType;
            updated = true;
        }

        if (a_item.DefaultLeadTimeSet && m_defaultLeadTime != a_item.DefaultLeadTime.Ticks)
        {
            m_defaultLeadTime = a_item.DefaultLeadTime.Ticks;
            updated = true;
        }

        if (a_item.ShelfLifeSet && ShelfLife != a_item.ShelfLife)
        {
            ShelfLife = a_item.ShelfLife;
            updated = true;
        }

        if (a_item.TransferQtySet && m_transferQty != a_item.TransferQty)
        {
            m_transferQty = a_item.TransferQty;
            updated = true;
        }

        if (a_item.ItemGroupSet && ItemGroup != a_item.ItemGroup)
        {
            ItemGroup = a_item.ItemGroup;
            updated = true;
        }

        if (a_item.BatchWindowSet && BatchWindow != a_item.BatchWindow)
        {
            BatchWindow = a_item.BatchWindow;
            updated = true;
        }

        if (a_item.PlanInventorySet && PlanInventory != a_item.PlanInventory)
        {
            PlanInventory = a_item.PlanInventory;
            updated = true;
        }

        if (a_item.MinOrderQtySet && MinOrderQty != a_item.MinOrderQty)
        {
            MinOrderQty = a_item.MinOrderQty;
            updated = true;
        }

        if (a_item.MaxOrderQtySet && MaxOrderQty != a_item.MaxOrderQty)
        {
            MaxOrderQty = a_item.MaxOrderQty;
            updated = true;
        }

        if (a_item.BatchSizeSet && BatchSize != a_item.BatchSize)
        {
            BatchSize = a_item.BatchSize;
            updated = true;
        }

        if (a_item.MinOrderQtyRoundupLimitSet && MinOrderQtyRoundupLimit != a_item.MinOrderQtyRoundupLimit)
        {
            MinOrderQtyRoundupLimit = a_item.MinOrderQtyRoundupLimit;
            updated = true;
        }

        if (a_item.JobAutoSplitQtySet && JobAutoSplitQty != a_item.JobAutoSplitQty)
        {
            JobAutoSplitQty = a_item.JobAutoSplitQty;
            updated = true;
        }

        if (a_item.RollupAttributesToParentSet && a_item.RollupAttributesToParent != RollupAttributesToParent)
        {
            RollupAttributesToParent = a_item.RollupAttributesToParent;
            updated = true;
        }

        if (a_item.CostIsSet && a_item.Cost != Cost)
        {
            Cost = a_item.Cost;
            updated = true;
        }

        if (a_item.UnitVolumeIsSet && a_item.UnitVolume != UnitVolume)
        {
            UnitVolume = a_item.UnitVolume;
            a_dataChanges.FlagEligibilityChanges(Id);
            updated = true;
        }

        if (a_item.ShelfLifeWarningHrsIsSet)
        {
            ShelfLifeWarningTicks = TimeSpan.FromHours(a_item.ShelfLifeWarningHrs).Ticks;
            //a_dataChanges.FlagEligibilityChanges(Id); //CN: I'm not sure if this is necessary for this field, so I am leaving it commented out.
            updated = true;
        }

        if (updated)
        {
            a_dataChanges.ItemChanges.UpdatedObject(Id);
        }
    }
    #endregion

    #region Cloning
    public Item Clone()
    {
        return (Item)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion

    #region PT Import DB
    public void PopulateImportDataSet(PtImportDataSet.ItemsDataTable itemTable)
    {
        itemTable.AddItemsRow(
            ExternalId,
            Name,
            Description,
            Notes,
            Source.ToString(),
            ItemType.ToString(),
            DefaultLeadTime.TotalDays,
            BatchSize,
            BatchWindow.TotalHours,
            MinOrderQty,
            MaxOrderQty,
            MinOrderQtyRoundupLimit,
            JobAutoSplitQty,
            ShelfLife.TotalHours,
            TransferQty,
            ItemGroup,
            PlanInventory,
            RollupAttributesToParent,
            Cost,
            UserFields == null ? "" : UserFields.GetUserFieldImportString(),
            UnitVolume,
            TimeSpan.FromTicks(ShelfLifeWarningTicks).TotalHours);
    }
    #endregion

    public override string ToString()
    {
        return string.Format("Item={0}".Localize(), ExternalId);
    }

    private decimal m_unitVolume;

    /// <summary>
    /// The default volume this item takes up in production
    /// </summary>
    public decimal UnitVolume
    {
        get => m_unitVolume;
        set => m_unitVolume = value;
    }
}