using System.ComponentModel;

using PT.APSCommon;
using PT.Common.Exceptions;
using PT.Common.Extensions;
using PT.Database;
using PT.ERPTransmissions;
using PT.Scheduler.Schedule.Demand;
using PT.Scheduler.Schedule.InventoryManagement;
using PT.Scheduler.Simulation;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// Specifies a material needed to perform an Operation.
/// Material Requirements are used for purchased materials and stocked manufactured materials (but not for Predecessor Manufacturing Orders).
/// </summary>
public partial class MaterialRequirement : ExternalBaseIdObject, ICloneable, IPTSerializable, IMaterialAllocation, IComparable<MaterialRequirement>
{
    public new const int UNIQUE_ID = 304;

    #region IPTSerializable Members
    internal MaterialRequirement(IReader a_reader, BaseIdGenerator a_baseIdGenerator)
        : base(a_reader)
    {
        m_restoreInfo = new RestoreInfo();
        m_mrSupply = new MRSupply(a_baseIdGenerator);

        if (a_reader.VersionNumber >= 12511)
        {
            m_bools = new BoolVector32(a_reader);
            m_restoreInfo = new RestoreInfo(a_reader);

            a_reader.Read(out m_latestSourceDate);
            a_reader.Read(out short eval);
            m_constraintType = (MaterialRequirementDefs.constraintTypes)eval;

            a_reader.Read(out m_issuedQty);
            a_reader.Read(out m_materialDescription);
            a_reader.Read(out m_materialName);
            a_reader.Read(out m_buyDirectSource);
            a_reader.Read(out m_totalCost);
            a_reader.Read(out m_totalRequiredQty);
            a_reader.Read(out m_nonAllocatedQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_leadTimeSpan);

            a_reader.Read(out eval);
            m_tankStorageReleaseTiming = (MaterialRequirementDefs.TankStorageReleaseTimingEnum)eval;

            m_shelfLifeRequirement = new ShelfLifeRequirement(a_reader);
            m_wearRequirement = new WearRequirement(a_reader);

            a_reader.Read(out m_plannedScrapQty);
            m_eligibleLots = new EligibleLots(a_reader);
            m_eligibleLots.RestoreReferences(this);
            a_reader.Read(out eval);
            m_materialAllocation = (ItemDefs.MaterialAllocation)eval;
            a_reader.Read(out m_minSourceQty);
            a_reader.Read(out m_maxSourceQty);
            a_reader.Read(out eval);
            m_materialSourcing = (ItemDefs.MaterialSourcing)eval;
            a_reader.Read(out eval);
            m_maintenanceMethod = (JobDefs.EMaintenanceMethod)eval;
            a_reader.Read(out eval);
            m_materialUsedTiming = (MaterialRequirementDefs.EMaterialUsedTiming)eval;
        }
        else if(a_reader.VersionNumber >= 12201)
        {
            a_reader.Read(out m_latestSourceDate);
            
            a_reader.Read(out short eval);
            m_constraintType = (MaterialRequirementDefs.constraintTypes)eval;

            a_reader.Read(out m_issuedQty);
            a_reader.Read(out m_materialDescription);
            a_reader.Read(out m_materialName);
            a_reader.Read(out m_buyDirectSource);
            a_reader.Read(out m_totalCost);
            a_reader.Read(out m_totalRequiredQty);
            a_reader.Read(out m_nonAllocatedQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_leadTimeSpan);

            a_reader.Read(out eval);
            m_tankStorageReleaseTiming = (MaterialRequirementDefs.TankStorageReleaseTimingEnum)eval;

            //Read in Item and Warehouse if they were saved
            a_reader.Read(out m_restoreInfo.haveItem);
            if (m_restoreInfo.haveItem)
            {
                m_restoreInfo.itemId = new BaseId(a_reader);
            }

            a_reader.Read(out m_restoreInfo.haveWarehouse);
            if (m_restoreInfo.haveWarehouse)
            {
                m_restoreInfo.warehouseId = new BaseId(a_reader);
            }

            a_reader.Read(out short lotUsabilityVal);
            if (lotUsabilityVal == 1 /*ShelfLife*/ || lotUsabilityVal == 4 /*ShelfLifeNonConstraint*/)
            {
                m_shelfLifeRequirement = new ShelfLifeRequirement(a_reader);
                m_wearRequirement = new WearRequirement();
            }
            else if (lotUsabilityVal == 2 /*Wear*/)
            {
                m_shelfLifeRequirement = new ShelfLifeRequirement();
                m_wearRequirement = new WearRequirement(a_reader);
            }
            else
            {
                m_shelfLifeRequirement = new ShelfLifeRequirement();
                m_wearRequirement = new WearRequirement();
            }

            m_bools = new BoolVector32(a_reader);
            //Old MaterialOverlapOptions class was just a bool vector
            BoolVector32 overlapBools = new BoolVector32(a_reader);
            UseOverlapPOs = overlapBools[1];
            if (overlapBools[0])
            {
                m_materialUsedTiming = MaterialRequirementDefs.EMaterialUsedTiming.ByProductionCycle;
            }

            a_reader.Read(out m_plannedScrapQty);
            m_eligibleLots = new EligibleLots(a_reader);
            m_eligibleLots.RestoreReferences(this);
            a_reader.Read(out eval);
            m_materialAllocation = (ItemDefs.MaterialAllocation)eval;
            a_reader.Read(out m_minSourceQty);
            a_reader.Read(out m_maxSourceQty);
            a_reader.Read(out eval);
            m_materialSourcing = (ItemDefs.MaterialSourcing)eval;
            a_reader.Read(out eval);
            m_maintenanceMethod = (JobDefs.EMaintenanceMethod)eval;
        }
        else if (a_reader.VersionNumber >= 737)
        {
            a_reader.Read(out m_latestSourceDate);
            int val;
            a_reader.Read(out val);
            m_constraintType = (MaterialRequirementDefs.constraintTypes)val;

            a_reader.Read(out m_issuedQty);
            a_reader.Read(out m_materialDescription);
            a_reader.Read(out m_materialName);
            a_reader.Read(out m_buyDirectSource);
            a_reader.Read(out m_totalCost);
            a_reader.Read(out m_totalRequiredQty);
            a_reader.Read(out m_nonAllocatedQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_leadTimeSpan);

            a_reader.Read(out val);
            m_tankStorageReleaseTiming = (MaterialRequirementDefs.TankStorageReleaseTimingEnum)val;

            //Read in Item and Warehouse if they were saved
            a_reader.Read(out m_restoreInfo.haveItem);
            if (m_restoreInfo.haveItem)
            {
                m_restoreInfo.itemId = new BaseId(a_reader);
            }

            a_reader.Read(out m_restoreInfo.haveWarehouse);
            if (m_restoreInfo.haveWarehouse)
            {
                m_restoreInfo.warehouseId = new BaseId(a_reader);
            }

            a_reader.Read(out int lotUsabilityVal);
            if (lotUsabilityVal == 1 /*ShelfLife*/ || lotUsabilityVal == 4 /*ShelfLifeNonConstraint*/)
            {
                m_shelfLifeRequirement = new ShelfLifeRequirement(a_reader);
                m_wearRequirement = new WearRequirement();
            }
            else if (lotUsabilityVal == 2 /*Wear*/)
            {
                m_shelfLifeRequirement = new ShelfLifeRequirement();
                m_wearRequirement = new WearRequirement(a_reader);
            }
            else
            {
                m_shelfLifeRequirement = new ShelfLifeRequirement();
                m_wearRequirement = new WearRequirement();
            }

            m_bools = new BoolVector32(a_reader);
            BoolVector32 overlapBools = new BoolVector32(a_reader);
            UseOverlapPOs = overlapBools[1];
            if (overlapBools[0])
            {
                m_materialUsedTiming = MaterialRequirementDefs.EMaterialUsedTiming.ByProductionCycle;
            }

            a_reader.Read(out m_plannedScrapQty);
            m_eligibleLots = new EligibleLots(a_reader);
            m_eligibleLots.RestoreReferences(this);
            a_reader.Read(out val);
            m_materialAllocation = (ItemDefs.MaterialAllocation)val;
            a_reader.Read(out m_minSourceQty);
            a_reader.Read(out m_maxSourceQty);
            a_reader.Read(out val);
            m_materialSourcing = (ItemDefs.MaterialSourcing)val;
        }
    }

    private RestoreInfo m_restoreInfo;

    private class RestoreInfo
    {
        internal bool haveItem;
        internal BaseId itemId;

        internal bool haveWarehouse;
        internal BaseId warehouseId;

        public bool HasStorageArea;
        public BaseId StorageAreaId;

        public RestoreInfo(IReader a_reader)
        {
            a_reader.Read(out haveItem);
            if (haveItem)
            {
                itemId = new BaseId(a_reader);
            }
            
            a_reader.Read(out haveWarehouse);
            if (haveWarehouse)
            {
                warehouseId = new BaseId(a_reader);
            }

            a_reader.Read(out HasStorageArea);
            if (HasStorageArea)
            {
                StorageAreaId = new BaseId(a_reader);
            }
        }

        public RestoreInfo() { }

        public static void Serialize(IWriter a_writer, Item a_item, Warehouse a_warehouse, StorageArea a_storageArea)
        {
            a_writer.Write(a_item != null);
            if (a_item != null)
            {
                a_item.Id.Serialize(a_writer);
            }

            a_writer.Write(a_warehouse != null);
            if (a_warehouse != null)
            {
                a_warehouse.Id.Serialize(a_writer);
            }

            a_writer.Write(a_storageArea != null);
            if (a_storageArea != null)
            {
                a_storageArea.Id.Serialize(a_writer);
            }
        }
    }

    internal void RestoreReferences(WarehouseManager aWarehouses, ItemManager aItems, InternalOperation a_operation)
    {
        Operation = a_operation; //Not set when created from serialization

        if (m_restoreInfo.haveItem)
        {
            m_item = aItems.GetById(m_restoreInfo.itemId);
        }

        if (m_restoreInfo.haveWarehouse)
        {
            m_warehouse = aWarehouses.GetById(m_restoreInfo.warehouseId);
        }

        if (m_restoreInfo.HasStorageArea)
        {
            if (m_warehouse.StorageAreas.TryGetValue(m_restoreInfo.StorageAreaId, out StorageArea storageArea))
            {
                m_storageArea = storageArea;
            }
        }

        m_restoreInfo = null;
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);


        m_bools.Serialize(writer);
        RestoreInfo.Serialize(writer, Item, Warehouse, StorageArea);
        writer.Write(m_latestSourceDate);
        writer.Write((short)m_constraintType);
        writer.Write(m_issuedQty);
        writer.Write(m_materialDescription);
        writer.Write(m_materialName);
        writer.Write(m_buyDirectSource);
        writer.Write(m_totalCost);
        writer.Write(m_totalRequiredQty);
        writer.Write(m_nonAllocatedQty);
        writer.Write(m_uom);

        writer.Write(m_leadTimeSpan);
        writer.Write((short)m_tankStorageReleaseTiming);
        
        m_shelfLifeRequirement.Serialize(writer);
        m_wearRequirement.Serialize(writer);
        
        writer.Write(m_plannedScrapQty);
        m_eligibleLots.Serialize(writer);
        writer.Write((short)m_materialAllocation);
        writer.Write(m_minSourceQty);
        writer.Write(m_maxSourceQty);
        writer.Write((short)m_materialSourcing);
        writer.Write((short)m_maintenanceMethod);
        writer.Write((short)m_materialUsedTiming);
    }

    [Browsable(false)]
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    internal MaterialRequirement(BaseId id, JobT.MaterialRequirement a_jobTMaterial, ScenarioDetail a_sd, WarehouseManager a_warehouses, ScenarioOptions a_so, InternalOperation a_op)
        : base(a_jobTMaterial.ExternalId, id)
    {
        ValidateTotalRequiredQty(a_so, a_jobTMaterial.TotalRequiredQty, a_op.Job.ExternalId, a_op.ManufacturingOrder.ExternalId, a_op.ExternalId, a_jobTMaterial.ExternalId);

        Operation = a_op;
        m_mrSupply = new MRSupply(a_sd.IdGen);

        if (a_jobTMaterial.RequirementType == MaterialRequirementDefs.requirementTypes.FromStock)
        {
            Item aItem = a_sd.ItemManager.GetByExternalId(a_jobTMaterial.ItemExternalId);
            if (aItem == null)
            {
                throw new PTValidationException("2789", new object[] { a_jobTMaterial.ItemExternalId, a_jobTMaterial.ExternalId });
            }

            m_item = aItem;

            if (a_jobTMaterial.WarehouseExternalIdSet)
            {
                Warehouse warehouse = a_warehouses.GetByExternalId(a_jobTMaterial.WarehouseExternalId);
                if (warehouse == null)
                {
                    throw new PTValidationException("2790", new object[] { a_jobTMaterial.WarehouseExternalId });
                }

                if (a_jobTMaterial.StorageAreaExternalIdIsSet)
                {
                    StorageArea storageArea = warehouse.StorageAreas.GetByExternalId(a_jobTMaterial.StorageAreaExternalId);
                    if (StorageArea != storageArea)
                    {
                        StorageArea = storageArea;
                    }
                }

                //Make sure the Warehouse stocks the Item.
                Inventory inventory = warehouse.Inventories[m_item.Id];
                if (inventory == null)
                {
                    throw new PTValidationException("2791", new object[] { warehouse.ExternalId, m_item.ExternalId });
                }

                m_warehouse = warehouse;
            }
            else
            {
                //Validate that the required item exists in a warehouse
                bool invExists = false;
                foreach (Warehouse warehouse in a_warehouses)
                {
                    if (a_jobTMaterial.StorageAreaExternalIdIsSet)
                    {
                        StorageArea storageArea = warehouse.StorageAreas.GetByExternalId(a_jobTMaterial.StorageAreaExternalId);
                        if (storageArea != null && StorageArea != storageArea)
                        {
                            StorageArea = storageArea;
                            m_warehouse = warehouse;
                        }
                    }
                    if (warehouse.Inventories.Contains(m_item.Id))
                    {
                        invExists = true;
                        break;
                    }
                }

                if (!invExists)
                {
                    throw new PTValidationException("3022", new object[] { m_item.ExternalId });
                }
            }
        }
        else //Buy Direct
        {
            m_latestSourceDate = a_jobTMaterial.AvailableDateTime.Ticks;
            m_leadTimeSpan = a_jobTMaterial.LeadTimeSpan.Ticks;
            m_materialDescription = a_jobTMaterial.MaterialDescription;
            m_materialName = a_jobTMaterial.MaterialName;
            m_buyDirectSource = a_jobTMaterial.Source;
        }

        m_constraintType = a_jobTMaterial.ConstraintType;
        m_issuedQty = a_so.RoundQty(a_jobTMaterial.IssuedQty);
        m_totalCost = a_jobTMaterial.TotalCost;
        m_totalRequiredQty = a_so.RoundQty(a_jobTMaterial.TotalRequiredQty);
        m_uom = a_jobTMaterial.UOM;

        m_wearRequirement = new WearRequirement();
        m_shelfLifeRequirement = new ShelfLifeRequirement();

        if (!BuyDirect)
        {
            UseOverlapPOs = a_jobTMaterial.UseOverlapPOs;
            MultipleWarehouseSupplyAllowed = a_jobTMaterial.MultipleWarehouseSupplyAllowed;
            AllowExpiredSupply = a_jobTMaterial.AllowExpiredSupply;
            MultipleStorageAreaSupplyAllowed = a_jobTMaterial.MultipleStorageAreaSupplyAllowed;
            TankStorageReleaseTiming = a_jobTMaterial.TankStorageReleaseTiming;
            FixedQty = a_jobTMaterial.FixedQty;
            m_materialUsedTiming = a_jobTMaterial.MaterialUsedTiming;

            //if (a_jobTMaterial.MinAge == TimeSpan.Zero && a_jobTMaterial.MinRemainingShelfLife == TimeSpan.Zero)
            //{
            //    m_wearRequirement = new WearRequirement(a_jobTMaterial.MaxEligibleWearAmount);
            //}
            //else
            //{
            //    throw new PTValidationException("2994", new object[] { "MinAgeHrs", "MinRemainingShelfLifeHrs", m_item.ExternalId, m_item.LotUsability.ToString() });
            //}


            //if (a_jobTMaterial.MinAge.TotalDays >= 0 && a_jobTMaterial.MinAge.TotalDays < 365)
            //{
            //    if (a_jobTMaterial.MaxEligibleWearAmount == 0 && string.IsNullOrEmpty(a_jobTMaterial.ProductRelease))
            //    {
            //        m_shelfLifeRequirement = new ShelfLifeRequirement(a_jobTMaterial.MinRemainingShelfLife, a_jobTMaterial.MinAge, m_item.LotUsability == ItemDefs.LotUsability.ShelfLifeNonConstraint);
            //    }
            //    else
            //    {
            //        throw new PTValidationException("2994", new object[] { "MaxEligibleWearAmount", "ProductRelease", m_item.ExternalId, m_item.LotUsability.ToString() });
            //    }
            //}

            PlannedScrapQty = a_jobTMaterial.PlannedScrapQty;
            m_allowedLotCodeIsSet = a_jobTMaterial.AllowedLotCodesIsSet;
            foreach (string allowedLotCode in a_jobTMaterial.AllowedLotCodes)
            {
                m_eligibleLots.Add(allowedLotCode, this);
            }

            AllowPartialSupply = a_jobTMaterial.AllowPartialSupply;
            MaterialAllocation = a_jobTMaterial.MaterialAllocation;
            MinSourceQty = a_jobTMaterial.MinSourceQty;
            MaxSourceQty = a_jobTMaterial.MaxSourceQty;
            MaterialSourcing = a_jobTMaterial.MaterialSourcing;
            RequireFirstTransferAtSetup = a_jobTMaterial.RequireFirstTransferAtSetup;
        }
    }

    /// <summary>
    /// validates TotalRequired qty against being set to zero
    /// </summary>
    private void ValidateTotalRequiredQty(ScenarioOptions a_so, decimal a_totalRequiredQty, string a_jobExternalId, string a_moExternalId, string a_opExternalId, string a_mrExternalId)
    {
        if (a_so.IsApproximatelyZeroOrLess(a_totalRequiredQty))
        {
            if (a_totalRequiredQty == 0)
            {
                // 2963: TotalRequiredQty of MaterialRequirement cannot be 0. Job '{0}' ManufacturingOrder '{1}' Operation '{2}' MaterialRequirement '{3}'
                throw new PTValidationException("2963", new object[] { a_jobExternalId, a_moExternalId, a_opExternalId, a_mrExternalId });
            }

            // 2964: TotalRequiredQty of MaterialRequirement '{0}' which will round to zero. Either increase the quantity or increase precisions in System Options. Job '{1}' ManufacturingOrder '{2}' Operation '{3}' MaterialRequirement '{4}'
            throw new PTValidationException("2964", new object[] { a_totalRequiredQty, a_jobExternalId, a_moExternalId, a_opExternalId, a_mrExternalId });
        }
    }

    internal MaterialRequirement(MaterialRequirement a_mr)
        : base(a_mr.Id)
    {
        m_latestSourceDate = a_mr.m_latestSourceDate;
        m_constraintType = a_mr.m_constraintType;
        m_issuedQty = a_mr.m_issuedQty;
        m_leadTimeSpan = a_mr.m_leadTimeSpan;
        m_materialDescription = a_mr.m_materialDescription;
        m_materialName = a_mr.m_materialName;
        m_buyDirectSource = a_mr.m_buyDirectSource;
        m_totalCost = a_mr.m_totalCost;
        m_totalRequiredQty = a_mr.m_totalRequiredQty;
        m_uom = a_mr.m_uom;
        UseOverlapPOs = a_mr.UseOverlapPOs;
        MultipleWarehouseSupplyAllowed = a_mr.MultipleWarehouseSupplyAllowed;
        AllowExpiredSupply = a_mr.AllowExpiredSupply;
        ShelfLifeRequirement = a_mr.ShelfLifeRequirement;
        WearRequirement = a_mr.WearRequirement;
        m_item = a_mr.m_item;
        m_warehouse = a_mr.m_warehouse;
        FixedQty = a_mr.FixedQty;
        m_plannedScrapQty = a_mr.PlannedScrapQty;
        m_eligibleLots = a_mr.m_eligibleLots;
        AllowPartialSupply = a_mr.AllowPartialSupply;
        MaterialAllocation = a_mr.MaterialAllocation;
        MinSourceQty = a_mr.MinSourceQty;
        MaxSourceQty = a_mr.MaxSourceQty;
        MaterialSourcing = a_mr.MaterialSourcing;
        StorageArea = a_mr.StorageArea;
        Operation = a_mr.Operation;
        RequireFirstTransferAtSetup = a_mr.RequireFirstTransferAtSetup;
    }

    internal MaterialRequirement(BaseId a_id, string a_externalId, MaterialRequirement a_originalMR, BaseIdGenerator a_idGen)
        : base(a_externalId, a_id)
    {
        m_latestSourceDate = a_originalMR.LatestSourceDateTime.Ticks;
        m_constraintType = a_originalMR.ConstraintType;
        m_issuedQty = a_originalMR.IssuedQty;
        m_leadTimeSpan = a_originalMR.LeadTimeSpan.Ticks;
        m_materialDescription = a_originalMR.MaterialDescription;
        m_materialName = a_originalMR.MaterialName;
        m_buyDirectSource = a_originalMR.Source;
        m_totalCost = a_originalMR.TotalCost;
        m_totalRequiredQty = a_originalMR.TotalRequiredQty;
        m_uom = a_originalMR.UOM;
        UseOverlapPOs = a_originalMR.UseOverlapPOs;
        MultipleWarehouseSupplyAllowed = a_originalMR.MultipleWarehouseSupplyAllowed;
        AllowExpiredSupply = a_originalMR.AllowExpiredSupply;

        ShelfLifeRequirement = a_originalMR.ShelfLifeRequirement;
        WearRequirement = a_originalMR.WearRequirement;

        if (!a_originalMR.BuyDirect)
        {
            m_item = a_originalMR.Item;
        }

        if (a_originalMR.Warehouse != null)
        {
            m_warehouse = a_originalMR.Warehouse;
        }

        FixedQty = a_originalMR.FixedQty;
        m_plannedScrapQty = a_originalMR.PlannedScrapQty;
        m_eligibleLots = a_originalMR.m_eligibleLots;
        AllowPartialSupply = a_originalMR.AllowPartialSupply;
        m_materialAllocation = a_originalMR.m_materialAllocation;
        m_minSourceQty = a_originalMR.MinSourceQty;
        m_maxSourceQty = a_originalMR.MaxSourceQty;
        m_materialSourcing = a_originalMR.MaterialSourcing;
        m_storageArea = a_originalMR.StorageArea;
        m_materialUsedTiming = a_originalMR.MaterialUsedTiming;
        Operation = a_originalMR.Operation;
        RequireFirstTransferAtSetup = a_originalMR.RequireFirstTransferAtSetup;

        //Don't copy 
        m_mrSupply = new MRSupply(a_idGen);
    }

    private class MaterialRequirementException : PTException
    {
        private MaterialRequirementException(string e)
            : base(e) { }
    }
    #endregion

    #region Bools vector
    private BoolVector32 m_bools;

    private const int c_fixedQtyIdx = 0;

    //const int available = 1; // The value is reusalbe; it's set to its default of false.
    private const int c_releaseTankAtEndOfCycleThatRemovesTheRemainingMaterialIdx = 2;
    private const int c_multipleWarehouseSupplyAllowedIdx = 3;
    private const int c_allowPartialSupplyIdx = 4;
    private const int c_allowMultiStorageAreaSupplyIdx = 5;
    private const int c_overlapPOs = 6;
    private const int c_requireFirstTransferAtSetup = 7;
    private const int c_allowExpiredSupplyIdx = 8;

    #endregion

    #region Shared Properties
    /// <summary>
    /// If true TotalRequiredQty will not change if the RequiredQuantity of the Manufacturing Order is changed for any reason (MRP, Split, etc).
    /// </summary>
    public bool FixedQty
    {
        get => m_bools[c_fixedQtyIdx];
        internal set => m_bools[c_fixedQtyIdx] = value;
    }

    private long m_latestSourceDate = DateTime.MinValue.Ticks;

    /// <summary>
    /// When the material is expected to be available for use in production.
    /// The usage of this depends on the setting for Constraint Type.
    /// This is calculated automatically during reschedules for Material Requirements for stocked Items.
    /// This can be set externally for buy-direct materials.
    /// </summary>
    public DateTime LatestSourceDateTime
    {
        get => new (LatestSourceDate);
        private set => m_latestSourceDate = value.Ticks;
    }

    /// <summary>
    /// Use this function to set available date for BuyDirect Materials. Exception is thrown if material is
    /// not BuyDirect.
    /// </summary>
    /// <param name="a_dateTime"></param>
    public void SetBuyDirectAvailableDate(DateTime a_dateTime)
    {
        if (!BuyDirect)
        {
            throw new PTHandleableException("2958");
        }

        m_latestSourceDate = a_dateTime.Ticks;
    }

    /// <summary>
    /// Returns information about the supply of the Item consumed by this Material Requirement.
    /// Only valid for From Stock MRs.  If called for a BuyDirect, an exception is thrown.
    /// </summary>
    /// <returns></returns>
    public void GetMostRecentSupplyOfItemBeingConsumed(ScenarioDetail sd, out DateTime lastSupplyDate, out decimal lastSupplyQty, out string lastSupplyDescription)
    {
        lastSupplyDate = DateTime.MinValue;
        lastSupplyQty = 0;
        lastSupplyDescription = "No Supply".Localize();

        if (Warehouse != null)
        {
            Inventory inv = Warehouse.Inventories[Item.Id];
            if (inv != null)
            {
                inv.GetMostRecentSupplyPriorToDate(sd, LatestSourceDateTime, out lastSupplyDate, out lastSupplyQty, out lastSupplyDescription);
            }
        }
        else //May not be assigned to a specific Warehouse so need to check all Warehouse that stock the item
        {
            DateTime lastSupplyDateSoFar;
            decimal lastSupplyQtySoFar;
            string lastSupplyDescriptionSoFar;
            for (int wI = 0; wI < sd.WarehouseManager.Count; wI++)
            {
                Warehouse whs = sd.WarehouseManager.GetByIndex(wI);
                if (whs.Inventories.Contains(Item.Id))
                {
                    Inventory inv = whs.Inventories[Item.Id];
                    inv.GetMostRecentSupplyPriorToDate(sd, LatestSourceDateTime, out lastSupplyDateSoFar, out lastSupplyQtySoFar, out lastSupplyDescriptionSoFar);
                    if (lastSupplyDateSoFar > lastSupplyDate)
                    {
                        lastSupplyDate = lastSupplyDateSoFar;
                        lastSupplyQty = lastSupplyQtySoFar;
                        lastSupplyDescription = lastSupplyDescriptionSoFar;
                    }
                }
            }
        }
    }

    /// <summary>
    /// If the Supply is On-Hand Inventory only or it is Issued Complete then this value is set to true.  Otherwise it is false.
    /// </summary>
    public bool Available => IssuedComplete || (MRSupply != null && UnIssuedQtyIsSuppliedByInventory);

    private MaterialRequirementDefs.constraintTypes m_constraintType;

    /// <summary>
    /// Indicates whether the Material Requirment should prevent its Operation from starting before the material arrives.
    /// NonConstraint:
    /// Doesn't have any affect on its Operation.
    /// ConstrainedByEarlierOfLeadTimeOrAvailableDate:
    /// In the case where there is no Item associated with the MaterialRequirement:
    /// The Operation can't start until the earlier of the Material's AvailableDate and Clock+LeadTimeSpan.
    /// In the case where there is an Item associated with the MaterialRequirement.
    /// The AvailableDate is determined by Inventory levels not the AvailableDate field within this class. So it's the earlier of
    /// what can be supplied through inventory or the lead-time.
    /// ConstrainedByAvailableDate:
    /// In the case where there is no Item associated with the MaterialRequirement:
    /// Operation can't start until the AvailableDate defined within this MaterialRequirement.
    /// In the case where there is an Item associated with the MaterialRequirement:
    /// Wait on Inventory. If no material becomes available before PurchaseToStocks and Tasks run out then use the LeadTime.
    /// </summary>
    public MaterialRequirementDefs.constraintTypes ConstraintType
    {
        get => m_constraintType;
        internal set => m_constraintType = value;
    }

    public bool NonConstraint => m_constraintType is MaterialRequirementDefs.constraintTypes.NonConstraint
                                                  or MaterialRequirementDefs.constraintTypes.IgnoredConstrainedByAvailableDate
                                                  or MaterialRequirementDefs.constraintTypes.IgnoredConstrainedByEarlierOfLeadTimeOrAvailableDate;

    private MaterialRequirementDefs.constraintTypes? m_originalConstraintTypeFromFrozenSpan;

    internal void SetConstraintIgnoredForFrozenSpan()
    {
        switch (ConstraintType)
        {
            case MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate:
                m_originalConstraintTypeFromFrozenSpan ??= ConstraintType;
                ConstraintType = MaterialRequirementDefs.constraintTypes.IgnoredConstrainedByAvailableDate;
                break;
            case MaterialRequirementDefs.constraintTypes.ConstrainedByEarlierOfLeadTimeOrAvailableDate:
                m_originalConstraintTypeFromFrozenSpan ??= ConstraintType;
                ConstraintType = MaterialRequirementDefs.constraintTypes.IgnoredConstrainedByEarlierOfLeadTimeOrAvailableDate;
                break;
        }
    }

    internal void RestoreConstraintAfterFrozenSpan()
    {
        if (m_originalConstraintTypeFromFrozenSpan.HasValue)
        {
            ConstraintType = m_originalConstraintTypeFromFrozenSpan.Value;
            m_originalConstraintTypeFromFrozenSpan = null;
        }
    }

    /// <summary>
    /// Whether the full Total Required Qty has been issed to the floor for the Job.
    /// If this value is true then the Material is considered available and is no longer treated as a constraint.
    /// </summary>
    public bool IssuedComplete => IssuedQty >= TotalRequiredQty;

    private decimal m_issuedQty;

    /// <summary>
    /// Quantity of material that has been Issued to the Operation, physically removing the material from storage and delivering it to the floor for production.
    /// </summary>
    public decimal IssuedQty
    {
        get => m_issuedQty;
        internal set => m_issuedQty = value;
    }

    /// <summary>
    /// Total Required Qty minus Issued Qty.
    /// If more is Issued than is Required then this returns zero.
    /// </summary>
    public decimal UnIssuedQty => Math.Max(0, TotalRequiredQty - IssuedQty);

    private long m_leadTimeSpan;

    /// <summary>
    /// Minimum time span needed to procure the material.
    /// If the Constraint Type is ConstrainedByEarlierOfLeadTimeOrAvailableDate then the minimum of Available Date and Now + Lead Time is used as the constraint date.
    /// If the Constraint Type is ConstrainedByAvailableDate then the Available Date is used instead and Lead Time is only used if at the end of the scheduling procsss there is no supply.
    /// </summary>
    public TimeSpan LeadTimeSpan
    {
        get => new (m_leadTimeSpan);
        private set => m_leadTimeSpan = value.Ticks;
    }

    private string m_materialDescription;

    /// <summary>
    /// Description of the required material.  This is the Description of the Item for stocked Items.  For buy-direct Material Requirements this can be set externally.
    /// </summary>
    public string MaterialDescription
    {
        get
        {
            if (!BuyDirect)
            {
                return Item.Description;
            }

            return m_materialDescription;
        }
        private set => m_materialDescription = value;
    }

    private string m_materialName;

    /// <summary>
    /// Name of the required material.  This is the Name of the Item for stocked Items.  For buy-direct Material Requirements this can be set externally.
    /// </summary>
    public string MaterialName
    {
        get
        {
            if (!BuyDirect)
            {
                return Item.Name;
            }

            return m_materialName;
        }
        private set => m_materialName = value;
    }

    private string m_buyDirectSource;

    /// <summary>
    /// Can be used to describe where this material is coming from. ('Purchase Order XYZ' or 'from stock', etc.)
    /// For stocked Items, this is set automatically.
    /// For buy-direct materials, this can be set externally.
    /// </summary>
    public string Source
    {
        get
        {
            if (RequirementType == MaterialRequirementDefs.requirementTypes.BuyDirect)
            {
                return m_buyDirectSource;
            }

            if (MRSupply != null)
            {
                return MRSupply.GetDescription();
            }

            return string.Empty;
        }
        private set => m_buyDirectSource = value;
    }

    public DateTime GetLatestSupplySourceDate(InternalOperation a_iOp, ScenarioDetail a_sd)
    {
        return MRSupply?.GetLatestSupplySourceDate(a_iOp, a_sd) ?? PTDateTime.MinDateTime;
    }

    public DateTime GetEarliestSupplySourceDate()
    {
        return MRSupply?.GetEarliestSupplySourceDate() ?? PTDateTime.MaxDateTime;
    }

    public Inventory GetLatestSupplySourceInventory(ScenarioDetail a_sd)
    {
        return MRSupply?.GetLatestSupplyInventory(a_sd);
    }

    public decimal GetSupplyFromActivities => MRSupply?.GetSupplyFromActivities ?? 0;

    public decimal GetSupplyFromPurchases => MRSupply?.GetSupplyFromPurchases ?? 0;

    public decimal GetSupplyFromInventories => MRSupply?.GetSupplyFromInventories ?? 0;

    public decimal GetSupplyFromAllSources => MRSupply?.GetSupplyFromAllSources ?? 0;

    public bool UnIssuedQtyIsSuppliedByInventory => GetSupplyFromInventories >= UnIssuedQty;

    private StorageArea m_storageArea;
    /// <summary>
    /// If specified, use the specified Storage Area
    /// </summary>
    public StorageArea StorageArea
    {
        get => m_storageArea;
        set => m_storageArea = value;
    }

    /// <summary>
    /// returns true if any of the material sources are Estimate or Planned (or not Firm for POs)
    /// </summary>
    /// <returns></returns>
    public bool AreAnySuppliesPlanned()
    {
        return MRSupply?.AreAnySuppliesPlanned() ?? true;
    }

    /// <summary>
    /// Calculates the latest non-job based material source.
    /// Returns the date and description of the latest material receipt.
    /// </summary>
    public Tuple<DateTime, string> CalcExpectedMaterialReceipt(ScenarioDetail a_sd, ResourceOperation a_op)
    {
        if (BuyDirect)
        {
            return new Tuple<DateTime, string>(LatestSourceDateTime, string.Format("Buy-Direct {0}".Localize(), MaterialName));
        }

        DateTime supplyDate = PTDateTime.MinDateTime;
        string supplyDesc = "";

        //Check for purchase orders
        if (MRSupply != null)
        {
            foreach (MRSupplyNode node in MRSupply)
            {
                if (node.Supply is PurchaseToStock po)
                {
                    if (po.AvailableDate > supplyDate)
                    {
                        supplyDate = po.AvailableDate;
                        supplyDesc = string.Format("PO {0} for {1}".Localize(), po.Name, Item.Name);
                    }
                }
            }
        }

        //Check for lead times
        foreach (Adjustment adj in MRSupply)
        {
            if (adj.AdjustmentType == InventoryDefs.EAdjustmentType.LeadTime)
            {
                supplyDate = adj.AdjDate;
                supplyDesc = string.Format("Lead Time {0}".Localize(), Item.Name);
            }
        }

        return new Tuple<DateTime, string>(supplyDate, supplyDesc);
    }

    public IEnumerable<string> GetSuppliedWarehouses => MRSupply?.GetSuppliedWarehouseNames ?? new List<string>();
    public IEnumerable<string> GetSuppliedStorageAreas => MRSupply?.GetSuppliedStorageAreaNames ?? new List<string>();

    private decimal m_totalCost = 100; //to keep costing information interesting.

    /// <summary>
    /// Can be used in KPIs and simulation rules to calculate WIP cost.
    /// </summary>
    public decimal TotalCost
    {
        get => m_totalCost;
        internal set => m_totalCost = value;
    }

    private decimal m_totalRequiredQty;

    /// <summary>
    /// The quantity of material to be consumed by the Operation for this requirment.
    /// The full quantity is requred at the beginning of each Activity for it to be able to schedule to start. (If constraining by materials.)
    /// </summary>
    public decimal TotalRequiredQty
    {
        get => m_totalRequiredQty;
        internal set => m_totalRequiredQty = value;
    }

    private string m_uom;

    /// <summary>
    /// The measuring unit in which the quantity fields are specified. For information only.
    /// </summary>
    public string UOM
    {
        get => m_uom;
        private set => m_uom = value;
    }

    private ShelfLifeRequirement m_shelfLifeRequirement;

    public ShelfLifeRequirement ShelfLifeRequirement
    {
        get => m_shelfLifeRequirement;
        private set => m_shelfLifeRequirement = value;
    }

    private WearRequirement m_wearRequirement;

    public WearRequirement WearRequirement
    {
        get => m_wearRequirement;
        private set => m_wearRequirement = value;
    }
    
    /// <summary>
    /// Whether to allow this Material Request to depend on material from Purchase Orders that haven't arrived yet but whose material is
    /// projected to arrive in stock on time to satisfy the cycles of the operation. This may allow the operation and other operations to start earlier.
    /// If this is not checked then this Material Request may end up waiting until the expected receive date of Purchase Order whose material is needed.
    /// </summary>
    public bool UseOverlapPOs
    {
        get => m_bools[c_overlapPOs];
        set => m_bools[c_overlapPOs] = value;
    }
    
    /// <summary>
    /// Whether the first transfer of material will be used at setup instead of based on the MaterialUsedTimings.
    /// For example if the resource is being primed during setup.
    /// </summary>
    public bool RequireFirstTransferAtSetup
    {
        get => m_bools[c_requireFirstTransferAtSetup];
        set => m_bools[c_requireFirstTransferAtSetup] = value;
    }

    // [BL:61]
    public bool ReleaseTankAtEndOfCycleThatRemovesTheRemainingMaterial
    {
        get => m_bools[c_releaseTankAtEndOfCycleThatRemovesTheRemainingMaterialIdx];
        private set => m_bools[c_releaseTankAtEndOfCycleThatRemovesTheRemainingMaterialIdx] = value;
    }

    public bool MultipleWarehouseSupplyAllowed
    {
        get => m_bools[c_multipleWarehouseSupplyAllowedIdx];
        private set => m_bools[c_multipleWarehouseSupplyAllowedIdx] = value;
    }
    
    public bool MultipleStorageAreaSupplyAllowed
    {
        get => m_bools[c_allowMultiStorageAreaSupplyIdx];
        private set => m_bools[c_allowMultiStorageAreaSupplyIdx] = value;
    }

    /// <summary>
    /// Whether demand for this inventory can supplied by multiple supplies.
    /// </summary>
    public bool AllowPartialSupply
    {
        get => m_bools[c_allowPartialSupplyIdx];
        private set => m_bools[c_allowPartialSupplyIdx] = value;
    }

    /// <summary>
    /// Whether demand for this inventory can supplied by a material source that has expired (default false).
    /// </summary>
    public bool AllowExpiredSupply
    {
        get => m_bools[c_allowExpiredSupplyIdx];
        private set => m_bools[c_allowExpiredSupplyIdx] = value;
    }

    internal long LatestSourceDate
    {
        get
        {
            if (!BuyDirect)
            {
                if (MRSupply != null) //It's possible for the MRSupplied to be null when the material requirement is temporarily created and never scheduled.
                {
                    m_latestSourceDate = MRSupply.CalcLatestSourceValue();
                }
            }

            return m_latestSourceDate;
        }
        set => m_latestSourceDate = value;
    }

    private decimal m_plannedScrapQty;

    /// <summary>
    /// The amount of extra material required that will be scrapped. This is not scaled like required quantity.
    /// Note: MRP increases the total required quantity by this amount.
    /// </summary>
    public decimal PlannedScrapQty
    {
        get => m_plannedScrapQty;
        private set
        {
            if (value < 0)
            {
                throw new PTValidationException("2931", new object[] { value });
            }

            m_plannedScrapQty = value;
        }
    }

    private Item m_item;

    /// <summary>
    /// The Item to be used as a material for the Operation.
    /// </summary>
    public Item Item => m_item;

    private decimal m_nonAllocatedQty;

    /// <summary>
    /// The total quantity of material that is scheduled to be satisfied from lead time (backorder) as opposed
    /// to coming from on-hand inventory or a planned supply.
    /// </summary>
    public decimal NonAllocatedQty
    {
        get => m_nonAllocatedQty;
        internal set => m_nonAllocatedQty = value;
    }

    /// <summary>
    /// True if the Supply of the material is from any combination of On-Hand inventory, Jobs, and Purchases.  False if partially or fully satisfied from Lead Time.
    /// </summary>
    public bool Planned
    {
        get
        {
            if (MRSupply.Empty)
            {
                return false;
            }
            
            //Adjustment types that mean the MR is not fully satisfied
            foreach (Adjustment adj in MRSupply)
            {
                switch (adj.AdjustmentType)
                {
                    case InventoryDefs.EAdjustmentType.LeadTime:
                    case InventoryDefs.EAdjustmentType.Shortage:
                    case InventoryDefs.EAdjustmentType.PastPlanningHorizon:
                        return false;
                }
            }

            return true;
        }
    }

    private Warehouse m_warehouse;

    /// <summary>
    /// (optional) The Warehouse from which this material must be supplied.  If omitted then the Material Requirement can be satisfied from any Warehouse accessible by the Plant of the Primary Resource
    /// performing the work.
    /// </summary>
    public Warehouse Warehouse => m_warehouse;

    public long LeadTimeSpanTicks
    {
        get => m_leadTimeSpan;
        set => m_leadTimeSpan = value;
    }

    // [TANK_STORAGE]
    private MaterialRequirementDefs.TankStorageReleaseTimingEnum m_tankStorageReleaseTiming = MaterialRequirementDefs.TankStorageReleaseTimingEnum.NotTank;

    /// <summary>
    /// If the material is drawn from a tank, this indicates when the tank should be considered empty.
    /// </summary>
    public MaterialRequirementDefs.TankStorageReleaseTimingEnum TankStorageReleaseTiming
    {
        get => m_tankStorageReleaseTiming;
        private set => m_tankStorageReleaseTiming = value;
    }

    /// <summary>
    /// Whether material is being directly purchased for this requirement. In this case the material has no affect on Inventory and Inventory has no affect
    /// on when this MR is satisfied.
    /// </summary>
    public MaterialRequirementDefs.requirementTypes RequirementType
    {
        get
        {
            if (m_item != null)
            {
                return MaterialRequirementDefs.requirementTypes.FromStock;
            }

            return MaterialRequirementDefs.requirementTypes.BuyDirect;
        }
    }

    /// <summary>
    /// Whether the MR does not draw material from inventory and instead uses limited imported constraints
    /// </summary>
    public bool BuyDirect => m_item == null;
    #endregion

    #region Update
    internal bool Update(MaterialRequirement a_updateMR, long a_clock, bool a_erpUpdate, BaseOperation a_op, WarehouseManager a_wm, IScenarioDataChanges a_dataChanges)
    {
        base.Update(a_updateMR);
        bool flagConstraintChanges = false;
        bool flagProductionChanges = false;
        bool buyDirectChange = false; //Set if the material requirement is being changed from a stock material to buy direct
        bool updated = false;
        //Determine if the constraint has become more constraining and is therefore a significant change
        DateTime oldConstraintDate = GetConstraintDate(a_clock);
        DateTime newConstraintDate = a_updateMR.GetConstraintDate(a_clock);
        
        if (newConstraintDate > oldConstraintDate)
        {
            flagConstraintChanges = true;
            updated = true;
        }
        else if (newConstraintDate < oldConstraintDate)
        {
            updated = true;
        }

        if (ConstraintType != a_updateMR.ConstraintType)
        {
            updated = true;
            ConstraintType = a_updateMR.ConstraintType;
            FlagItemForNetChangeMRP(a_wm);
            if (a_updateMR.ConstraintType is MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate 
                                        or MaterialRequirementDefs.constraintTypes.ConstrainedByEarlierOfLeadTimeOrAvailableDate)
            {
                flagConstraintChanges = true;
            }
            else
            {
                flagProductionChanges = true;
            }
        }

        if (IssuedQty != a_updateMR.IssuedQty)
        {
            if (IssuedQty < a_updateMR.IssuedQty)
            {
                flagProductionChanges = true;
            }
            else
            {
                flagConstraintChanges = true;
            }

            IssuedQty = a_op.ScenarioDetail.ScenarioOptions.RoundQty(a_updateMR.IssuedQty);
            FlagItemForNetChangeMRP(a_wm);
            updated = true;
        }

        if (LeadTimeSpan != a_updateMR.LeadTimeSpan)
        {
            LeadTimeSpan = a_updateMR.LeadTimeSpan;
            updated = true;
        }

        if (MaterialDescription != a_updateMR.MaterialDescription)
        {
            MaterialDescription = a_updateMR.MaterialDescription;
            updated = true;
        }

        if (MaterialName != a_updateMR.MaterialName)
        {
            MaterialName = a_updateMR.MaterialName;
            flagConstraintChanges = true;
            updated = true;
        }

        if (Item != a_updateMR.Item)
        {
            if (Item != null && a_updateMR.Item == null)
            {
                //TODO: This is not desirable, potentially validate against this change
                buyDirectChange = true;
            }

            FlagItemForNetChangeMRP(a_wm); //flag old item and new one
            m_item = a_updateMR.Item;
            flagConstraintChanges = true;
            FlagItemForNetChangeMRP(a_wm);
            updated = true;
        }

        if (Warehouse != a_updateMR.Warehouse)
        {
            m_warehouse = a_updateMR.Warehouse;
            flagConstraintChanges = true;
            FlagItemForNetChangeMRP(a_wm);
            updated = true;
        }

        if (Source != a_updateMR.Source)
        {
            Source = a_updateMR.Source;
            updated = true;
        }

        if (TotalCost != a_updateMR.TotalCost)
        {
            TotalCost = a_updateMR.TotalCost;
            updated = true;
        }

        if (TotalRequiredQty != a_updateMR.TotalRequiredQty)
        {
            if (!(a_erpUpdate && a_op.ManufacturingOrder.PreserveRequiredQty))
            {
                decimal originalQty = TotalRequiredQty;
                ValidateTotalRequiredQty(a_op.ScenarioDetail.ScenarioOptions, a_updateMR.TotalRequiredQty, a_op.Job.ExternalId, a_op.ManufacturingOrder.ExternalId, a_op.ExternalId, ExternalId);
                TotalRequiredQty = a_op.ScenarioDetail.ScenarioOptions.RoundQty(a_updateMR.TotalRequiredQty);

                if (Item != null)//from stock so qty change can be significant
                {
                    if (TotalRequiredQty > originalQty)
                    {
                        flagConstraintChanges = true;
                    }
                    else
                    {
                        flagProductionChanges = true;
                    }
                }

                FlagItemForNetChangeMRP(a_wm);
            }

            updated = true;
        }

        if (UOM != a_updateMR.UOM)
        {
            UOM = a_updateMR.UOM;
            updated = true;
        }

        if (UseOverlapPOs != a_updateMR.UseOverlapPOs)
        {
            UseOverlapPOs = a_updateMR.UseOverlapPOs;
            if (!UseOverlapPOs)
            {
                flagConstraintChanges = true;
            }
            else
            {
                flagProductionChanges = true;
            }

            updated = true;
        }

        if (MultipleWarehouseSupplyAllowed != a_updateMR.MultipleWarehouseSupplyAllowed)
        {
            MultipleWarehouseSupplyAllowed = a_updateMR.MultipleWarehouseSupplyAllowed;
            if (!MultipleWarehouseSupplyAllowed)
            {
                flagConstraintChanges = true;
            }
            else
            {
                flagProductionChanges = true;
            }

            updated = true;
        }

        if (AllowExpiredSupply != a_updateMR.AllowExpiredSupply)
        {
            AllowExpiredSupply = a_updateMR.AllowExpiredSupply;
            if (!AllowExpiredSupply)
            {
                flagConstraintChanges = true;
            }
        
            updated = true;
        }

        if (MultipleStorageAreaSupplyAllowed != a_updateMR.MultipleStorageAreaSupplyAllowed)
        {
            MultipleStorageAreaSupplyAllowed = a_updateMR.MultipleStorageAreaSupplyAllowed;
            if (!MultipleStorageAreaSupplyAllowed)
            {
                flagConstraintChanges = true;
            }

            updated = true;
        }

        if (m_tankStorageReleaseTiming != a_updateMR.m_tankStorageReleaseTiming)
        {
            m_tankStorageReleaseTiming = a_updateMR.m_tankStorageReleaseTiming;
            updated = true;
        }

        if (!ShelfLifeRequirement.Equals(a_updateMR.ShelfLifeRequirement))
        {
            ShelfLifeRequirement = a_updateMR.ShelfLifeRequirement;
            flagConstraintChanges = true;
            updated = true;
        }

        if (WearRequirement != a_updateMR.WearRequirement)
        {
            WearRequirement = a_updateMR.WearRequirement;
            updated = true;
        }

        if (FixedQty != a_updateMR.FixedQty)
        {
            FixedQty = a_updateMR.FixedQty;
            updated = true;
        }
        
        if (PlannedScrapQty != a_updateMR.PlannedScrapQty)
        {
            PlannedScrapQty = a_updateMR.PlannedScrapQty;
            updated = true;
        }

        //The addition of the equality check is to allow instances where allowed lot codes are cleared when
        //they previous;y had values
        if (a_updateMR.m_allowedLotCodeIsSet && !m_eligibleLots.Equals(a_updateMR.m_eligibleLots))
        {
            m_eligibleLots = a_updateMR.m_eligibleLots;
            flagConstraintChanges = true;
            updated = true;
        }

        if (AllowPartialSupply != a_updateMR.AllowPartialSupply)
        {
            AllowPartialSupply = a_updateMR.AllowPartialSupply;
            if (!a_updateMR.AllowPartialSupply)
            {
                flagConstraintChanges = true;
            }
        
            updated = true;
        }

        if (MaterialAllocation != a_updateMR.MaterialAllocation)
        {
            MaterialAllocation = a_updateMR.MaterialAllocation;
            flagConstraintChanges = true;
            updated = true;
        }

        if (MinSourceQty != a_updateMR.MinSourceQty)
        {
            MinSourceQty = a_updateMR.MinSourceQty;
            updated = true;
        }

        if (MaxSourceQty != a_updateMR.MaxSourceQty)
        {
            MaxSourceQty = a_updateMR.MaxSourceQty;
            updated = true;
        }

        if (MaterialSourcing != a_updateMR.MaterialSourcing)
        {
            MaterialSourcing = a_updateMR.MaterialSourcing;
            updated = true;
        }

        if (StorageArea != a_updateMR.StorageArea)
        {
            StorageArea = a_updateMR.StorageArea;
            flagConstraintChanges = true;
            updated = true;
        }

        if (MaterialUsedTiming != a_updateMR.MaterialUsedTiming)
        {
            m_materialUsedTiming = a_updateMR.MaterialUsedTiming;
            flagConstraintChanges = true;
            updated = true;
        }

        if (RequireFirstTransferAtSetup != a_updateMR.RequireFirstTransferAtSetup)
        {
            RequireFirstTransferAtSetup = a_updateMR.RequireFirstTransferAtSetup;
            if (RequireFirstTransferAtSetup)
            {
                flagProductionChanges = true;
            }

            updated = true;
        }

        if (buyDirectChange)
        {
            //The stock material was changed to buy-direct. Clear out all item and inventory related values
            ShelfLifeRequirement = new ShelfLifeRequirement();
            WearRequirement = new WearRequirement();
            m_warehouse = null;
            m_eligibleLots.Clear();
            UseOverlapPOs = false;
            RequireFirstTransferAtSetup = false;
            m_bools.Clear();

            //Update any fields that changed
            if (LatestSourceDate != a_updateMR.LatestSourceDate)
            {
                m_latestSourceDate = a_updateMR.m_latestSourceDate;
            }

            flagConstraintChanges = true; ;
            updated = true;
        }

        if (a_op.Scheduled)
        {
            if (flagConstraintChanges)
            {
                a_dataChanges.FlagConstraintChanges(a_op.Job.Id);
            }

            if (flagProductionChanges)
            {
                a_dataChanges.FlagProductionChanges(a_op.Job.Id);
            }
        }

        return updated;
    }
    #endregion

    internal void FlagItemForNetChangeMRP(WarehouseManager aWarehouseManager)
    {
        if (Item != null)
        {
            List<Inventory> inventories = aWarehouseManager.GetInventoriesThatMaySupplyMR(this);
            for (int invI = 0; invI < inventories.Count; invI++)
            {
                inventories[invI].IncludeInNetChangeMRP = true;
            }
        }
    }

    /// <summary>
    /// Returns the date that should be used as a constraint for operations, based on the ConstraintType, AvailableDate and LeadTime.
    /// </summary>
    /// <param name="scenarioDetailClock">ScenarioDetail clock.</param>
    /// <returns></returns>
    public DateTime GetConstraintDate(long scenarioDetailClock)
    {
        if (IssuedComplete)
        {
            return DateTime.MinValue;
        }

        switch (ConstraintType)
        {
            case MaterialRequirementDefs.constraintTypes.ConstrainedByEarlierOfLeadTimeOrAvailableDate:
                return new DateTime(Math.Min(LatestSourceDate, LeadTimeSpan.Ticks + scenarioDetailClock));
            case MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate:
                return LatestSourceDateTime;
            default:
                return DateTime.MinValue;
        }
    }

    internal void PopulateJobDataSet(ref JobDataSet dataSet, BaseOperation operation)
    {
        JobDataSet.MaterialRequirementRow row = dataSet.MaterialRequirement.NewMaterialRequirementRow();
        row.JobExternalId = operation.ManufacturingOrder.Job.ExternalId;
        row.MoExternalId = operation.ManufacturingOrder.ExternalId;
        row.OpExternalId = operation.ExternalId;
        row.ExternalId = ExternalId;
        row.LatestSourceDateTime = LatestSourceDateTime.ToDisplayTime().ToDateTime();
        row.ConstraintType = ConstraintType.ToString();
        row.Id = Id.ToBaseType();
        row.IssuedQty = IssuedQty;
        row.IssuedComplete = IssuedComplete;
        row.ItemDescription = MaterialDescription;
        row.Source = Source;
        row.TotalCost = TotalCost;
        row.TotalRequiredQty = TotalRequiredQty;
        row.UOM = UOM;
        row.Available = Available;
        row.FixedQty = FixedQty;
        row.PlannedScrapQty = PlannedScrapQty;

        if (RequirementType == MaterialRequirementDefs.requirementTypes.BuyDirect)
        {
            row.MaterialName = MaterialName;
            row.MaterialDescription = MaterialDescription;
            row.LeadTimeHrs = LeadTimeSpan.TotalHours;
            row.RequirementType = MaterialRequirementDefs.requirementTypes.BuyDirect.ToString();
        }
        else //Stocked
        {
            row.ItemExternalId = Item.ExternalId;
            row.MaterialName = Item.Name;
            row.MaterialDescription = Item.Description;
            row.UseOverlapPurchases = UseOverlapPOs;
            row.TankStorageReleaseTiming = TankStorageReleaseTiming.ToString();
            row.MultipleWarehouseSupplyAllowed = MultipleWarehouseSupplyAllowed;
            row.AllowExpiredSupply = AllowExpiredSupply;
            row.MultipleStorageAreaSupplyAllowed = MultipleStorageAreaSupplyAllowed;
            if (Warehouse != null)
            {
                row.WarehouseExternalId = Warehouse.ExternalId;
            }

            row.MinRemainingShelfLifeHrs = new TimeSpan(ShelfLifeRequirement.MinRemainingShelfLife).TotalHours;
            row.MinAgeHrs = new TimeSpan(ShelfLifeRequirement.MinAgeTicks).TotalHours;
            
            row.MaxEligibleWearAmount = WearRequirement.MaxWearAmount;

            row.RequirementType = MaterialRequirementDefs.requirementTypes.FromStock.ToString();
            string eligibleLotsString = "";
            foreach (string lotCode in EligibleLotCodesEnumerator)
            {
                eligibleLotsString += lotCode + ",";
            }

            row.AllowedLotCodes = eligibleLotsString.TrimEnd(',');
            row.AllowPartialSupply = AllowPartialSupply;
            row.MaterialAllocation = MaterialAllocation.ToString();
            row.MinSourceQty = MinSourceQty;
            row.MaxSourceQty = MaxSourceQty;
            row.MaterialSourcing = MaterialSourcing.ToString();
            row.StorageAreaExternalId = StorageArea?.ExternalId ?? "";
            row.MaterialUsedTiming = MaterialUsedTiming.ToString();
            row.RequireFirstTransferAtSetup = RequireFirstTransferAtSetup;
        }

        dataSet.MaterialRequirement.AddMaterialRequirementRow(row);
    }

    #region PT Database
    internal void PtDbPopulate(ref PtDbDataSet dataSet, BaseOperation op, PtDbDataSet.JobOperationsRow jobOpRow, PTDatabaseHelper a_dbHelper)
    {
        string sourceMax4000Char;
        if (Source != null && Source.Length > 4000) // Source can be null if RequirementType is BuyDirect
        {
            sourceMax4000Char = Source.Substring(0, 4000); //so SQL server maxlength not overfilled
        }
        else
        {
            sourceMax4000Char = Source;
        }

        bool onlyOnHandInventory = true;
        string supplyDescription = MRSupply.GetDescription();
        foreach (Adjustment adj in MRSupply)
        {
            if (adj.AdjustmentType != InventoryDefs.EAdjustmentType.OnHand)
            {
                onlyOnHandInventory = false;
            }
        }

        string eligibleLotsString = "";
        Dictionary<string, EligibleLot>.Enumerator lotsEnumerator = GetEligibleLotsEnumerator();
        while (lotsEnumerator.MoveNext())
        {
            eligibleLotsString += lotsEnumerator.Current.Key + ",";
        }

        double minAge = ShelfLifeRequirement.MinAge.TotalHours;

        //Add MaterialRequirement row
        PtDbDataSet.JobMaterialsRow materialRow = dataSet.JobMaterials.AddJobMaterialsRow(
            jobOpRow.PublishDate,
            jobOpRow.InstanceId,
            op.ManufacturingOrder.Job.Id.ToBaseType(),
            op.ManufacturingOrder.Id.ToBaseType(),
            op.Id.ToBaseType(),
            Id.ToBaseType(),
            ExternalId,
            onlyOnHandInventory,
            a_dbHelper.AdjustPublishTime(LatestSourceDateTime),
            ConstraintType.ToString(),
            IssuedComplete,
            IssuedQty,
            LeadTimeSpan.TotalDays,
            MaterialName,
            MaterialDescription,
            sourceMax4000Char,
            TotalCost,
            TotalRequiredQty,
            MinSourceQty,
            MaxSourceQty,
            MaterialSourcing.ToString(),
            UOM,
            BuyDirect,
            NonAllocatedQty,
            Available,
            Planned,
            RequirementType.ToString(),
            supplyDescription,
            UseOverlapPOs,
            TankStorageReleaseTiming.ToString(),
            MultipleWarehouseSupplyAllowed,
            FixedQty,
            Item == null ? "" : Item.ExternalId,
            Warehouse == null ? "" : Warehouse.ExternalId,
            StorageArea == null ? "" : StorageArea.ExternalId,
            eligibleLotsString.TrimEnd(','),
            AllowPartialSupply,
            PlannedScrapQty,
            minAge,
            GetShelfLifePenetrationPercent(op),
            GetWipDuration(op).TotalHours,
            MaterialAllocation.ToString(),
            MaterialUsedTiming.ToString(),
            MultipleStorageAreaSupplyAllowed,
            RequireFirstTransferAtSetup,
            AllowExpiredSupply);

        if (Item != null)
        {
            Dictionary<long, MaterialRequirementSupplyingActivity> supplyingActivityList = new ();
            GetSupplyingActivitiesAcrossBomLevels(0, ref supplyingActivityList, 1, Item.Id);
            foreach (MaterialRequirementSupplyingActivity mrsa in supplyingActivityList.Values)
            {
                if (op is InternalOperation)
                {
                    InternalOperation iOp = (InternalOperation)op;

                    for (int actI = 0; actI < iOp.Activities.Count; actI++)
                    {
                        InternalActivity activity = iOp.Activities.GetByIndex(actI);
                        //We'll allocate the material to Activities based on their qty.  Eventually we'll track the allocate at the Activity level but
                        //  now it's at the op level.
                        decimal qtySuppliedToThisActivity = mrsa.SuppliedQty * activity.RequiredFinishQty / iOp.RequiredFinishQty;

                        mrsa.PtDbPopulate(ref dataSet, op, materialRow, activity.Id, qtySuppliedToThisActivity);
                    }
                }
                else
                {
                    BaseId dummyActivityId = new (-1);
                    mrsa.PtDbPopulate(ref dataSet, op, materialRow, dummyActivityId, mrsa.SuppliedQty);
                }
            }
        }
    }
    #endregion

    #region Cloning
    private MaterialRequirement Clone()
    {
        return (MaterialRequirement)MemberwiseClone();
    }

    object ICloneable.Clone()
    {
        return Clone();
    }
    #endregion

    #region Demo Data
    /// <summary>
    /// Adjust values to update Demo Data for clock advance so good relative dates are maintained.
    /// </summary>
    internal void AdjustDemoDataForClockAdvance(long clockAdvanceTicks)
    {
        if (BuyDirect)
        {
            m_latestSourceDate += clockAdvanceTicks;
        }
    }
    #endregion

    #region Analysis
    public override string Analysis
    {
        get
        {
            if (ConstraintType == MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate)
            {
                if (MRSupply.Empty)
                {
                    return string.Format("Material {0} has a Constraint Type that requires a supply and there is none so the Operation cannot be scheduled.".Localize(), MaterialName);
                }
            }

            return "";
        }
    }
    #endregion Analysis

    public Dictionary<long, MaterialRequirementSupplyingActivity> GetSupplyingActivitiesAcrossBomLevels()
    {
        Dictionary<long, MaterialRequirementSupplyingActivity> mrSupplySet = new ();

        GetSupplyingActivitiesAcrossBomLevels(1, ref mrSupplySet, 1, Item.Id);

        return mrSupplySet;
    }

    /// <summary>
    /// Returns a list of all supplying activites for the MaterialRequirement going down through the current material pegging hierarchy based on the current schedule.
    /// </summary>
    private void GetSupplyingActivitiesAcrossBomLevels(int aBomLevel, ref Dictionary<long, MaterialRequirementSupplyingActivity> r_mrSupplyHash, decimal pctOfQtyToTopLevel, BaseId materialItemId)
    {
        if (MRSupply != null)
        {
            foreach (Adjustment adjustment in MRSupply)
            {
                if (adjustment is MaterialRequirementAdjustment mrAdjustment)
                {
                    //Add this activity as a supplying activity
                    InternalActivity supplyingActivity = mrAdjustment.GetSupplyingActivity();
                    if (supplyingActivity == null)
                    {
                        continue;
                    }

                    //Limit the recursion so that an activity can't supply itself
                    if (r_mrSupplyHash.ContainsKey(supplyingActivity.Id.Value))
                    {
                        continue;
                    }

                    decimal suppliedQty = adjustment.ChangeQty * pctOfQtyToTopLevel; //only a portion of this qty is going to the top level if this is not level 0.
                    MaterialRequirementSupplyingActivity mrSupply = new (supplyingActivity, suppliedQty, aBomLevel);

                    r_mrSupplyHash.Add(supplyingActivity.Id.Value, mrSupply);

                    //A smaller portion of this activity's materials supply the top level.
                    decimal nextPctOfQtyToTopLevel = pctOfQtyToTopLevel * suppliedQty / GetProductQtySupplyingMaterial(supplyingActivity, materialItemId);

                    // Recursively add the activities that are supplying this activity
                    GetSupplyingActivitiesAcrossBomLevelsHelper(aBomLevel, ref r_mrSupplyHash, supplyingActivity.Operation, nextPctOfQtyToTopLevel);

                    // Recursively add the activities that are supplying this activity's predecessors operations.
                    for (int predI = 0; predI < supplyingActivity.Operation.Predecessors.Count; predI++)
                    {
                        GetSupplyingActivitiesAcrossBomLevelsHelper(aBomLevel, ref r_mrSupplyHash, supplyingActivity.Operation.Predecessors[predI].Predecessor.Operation, nextPctOfQtyToTopLevel);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Returns the qty of the material supplied by this Activity.
    /// </summary>
    private decimal GetProductQtySupplyingMaterial(InternalActivity activity, BaseId suppliedItemId)
    {
        decimal qtySupplied = 0;
        for (int i = 0; i < activity.Operation.Products.Count; i++)
        {
            Product product = activity.Operation.Products[i];
            if (product.Item.Id == suppliedItemId)
            {
                qtySupplied += product.TotalOutputQty * (activity.RequiredFinishQty / activity.Operation.RequiredFinishQty);
            }
        }

        return qtySupplied;
    }

    /// <summary>
    /// Add the activities supplying any materials for the operation
    /// </summary>
    private void GetSupplyingActivitiesAcrossBomLevelsHelper(int aBomLevel, ref Dictionary<long, MaterialRequirementSupplyingActivity> r_mrSupplyHash, BaseOperation op, decimal pctOfQtyToTopLevel)
    {
        if (op.Scheduled)
        {
            for (int i = 0; i < op.MaterialRequirements.Count; i++)
            {
                MaterialRequirement mr = op.MaterialRequirements[i];
                if (mr.Item != null && mr.Item.ItemType != ItemDefs.itemTypes.Resource) // don't include materials of type resource (tool). Unless we exlude these types of materials, many jobs could be considered connected.
                {
                    mr.GetSupplyingActivitiesAcrossBomLevels(aBomLevel + 1, ref r_mrSupplyHash, pctOfQtyToTopLevel, mr.Item.Id);
                }
            }
        }
    }

    public class MaterialRequirementSupplyingActivity
    {
        /// <summary>
        /// Used to create a list of supplying Activities for use in SQL export and UI.
        /// Not serialized.
        /// </summary>
        /// <param name="aSupplyingActivity">An Activity that is listed as a supply to the MR.</param>
        /// <param name="aSuppliedQty">The qty listed on the SupplyList to be suppled to the MR from the Activity.</param>
        /// <param name="aSupplyBomLevel">Level 0 means this Activity is directly supplying the target MR.  Level 1 means it's supplying an Activity that is a Level 0, etc.</param>
        internal MaterialRequirementSupplyingActivity(InternalActivity aSupplyingActivity, decimal aSuppliedQty, int aSupplyBomLevel)
        {
            m_supplyingActivity = aSupplyingActivity;
            m_suppliedQty = aSuppliedQty;
            m_supplyBomLevel = aSupplyBomLevel;
        }

        private readonly InternalActivity m_supplyingActivity;

        public InternalActivity SupplyingActivity => m_supplyingActivity;

        private readonly decimal m_suppliedQty;

        public decimal SuppliedQty => m_suppliedQty;

        private readonly int m_supplyBomLevel;

        public int SupplyBomLevel => m_supplyBomLevel;

        public void PtDbPopulate(ref PtDbDataSet dataSet, BaseOperation op, PtDbDataSet.JobMaterialsRow mrRow, BaseId activityId, decimal qtySuppliedToTheActivity)
        {
            dataSet.JobMaterialSupplyingActivities.AddJobMaterialSupplyingActivitiesRow(
                mrRow.PublishDate,
                mrRow.InstanceId,
                mrRow.JobId,
                mrRow.ManufacturingOrderId,
                mrRow.OperationId,
                mrRow.MaterialRequirementId,
                activityId.ToBaseType(),
                SupplyingActivity.Operation.ManufacturingOrder.Job.Id.ToBaseType(),
                SupplyingActivity.Operation.ManufacturingOrder.Id.ToBaseType(),
                SupplyingActivity.Operation.Id.ToBaseType(),
                SupplyingActivity.Id.ToBaseType(),
                qtySuppliedToTheActivity,
                SupplyBomLevel);
        }
    }

    /// <summary>
    /// Specifies a qty and reference to a MaterialRequirement.  Used by supplying objects to specify a list of MaterialRequirements being supplied.
    /// </summary>
    public class MaterialRequirementSupply
    {
        public MaterialRequirementSupply(MaterialRequirement aSuppliedMR, BaseOperation aSuppliedOp, decimal aSuppliedQty)
        {
            m_suppliedMR = aSuppliedMR;
            m_suppliedOp = aSuppliedOp;
            m_suppliedQty = aSuppliedQty;
        }

        private readonly MaterialRequirement m_suppliedMR;

        /// <summary>
        /// The Material Requirement to be supplied by the supplier.
        /// </summary>
        public MaterialRequirement SuppliedMaterialRequirement => m_suppliedMR;

        private readonly BaseOperation m_suppliedOp;

        /// <summary>
        /// The Operation that contains the Material Requirement being supplied.
        /// </summary>
        public BaseOperation SuppliedOperation => m_suppliedOp;

        private readonly decimal m_suppliedQty;

        /// <summary>
        /// The quantity being supplied to the Material Requirement.
        /// </summary>
        public decimal SuppliedQty => m_suppliedQty;
    }

    /// <summary>
    /// Returns a list containing any Internal Activities that are supplying material to this Material Requirement.
    /// </summary>
    public IEnumerable<InternalActivity> SupplyingActivities
    {
        get
        {
            if (MRSupply != null)
            {
                foreach (Adjustment adjustment in MRSupply)
                {
                    if (adjustment.Storage?.Lot?.Reason is InternalActivity act)
                    {
                        yield return act;
                    }
                }
            }
        }
    }

    public IEnumerable<(InternalActivity, decimal qty, bool expired)> SupplyingActivityAdjustments
    {
        get
        {
            if (MRSupply != null)
            {
                foreach (Adjustment adjustment in MRSupply)
                {
                    if (adjustment is ActivityAdjustment actAdj && adjustment.Storage?.Lot?.Reason is InternalActivity act)
                    {
                        yield return (act, actAdj.Qty, adjustment.Expired);
                    }
                }
            }
        }
    }

    //Create a SupplyingOrderNodes
    public IEnumerable<(PurchaseToStock po, decimal qty)> SupplyingOrderAdjustments
    {
        get
        {
            List<MRSupplyNode> list = new ();
            if (MRSupply != null)
            {
                foreach (Adjustment adjustment in MRSupply)
                {
                    if (adjustment.Storage?.Lot?.Reason is PurchaseToStock po)
                    {
                        yield return (po, adjustment.Qty);
                    }
                }
            }
        }
    }

    public IEnumerable<Adjustment> SupplyingWarehouseAdjustments
    {
        get
        {
            if (MRSupply != null)
            {
                foreach (Adjustment adjustment in MRSupply)
                {
                    if (adjustment.Storage != null && adjustment.Storage.Lot.LotSource == ItemDefs.ELotSource.Inventory)
                    {
                        yield return adjustment;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Returns a list containing any Lots that are supplying material to this Material Requirement along with the quantity supplied.
    /// </summary>
    public IEnumerable<Tuple<Lot, decimal>> SupplyingLots
    {
        get
        {
            if (MRSupply != null)
            {
                foreach (Adjustment adjustment in MRSupply)
                {
                    if (adjustment.Storage?.Lot is Lot lot)
                    {
                        yield return new Tuple<Lot, decimal>(lot, -adjustment.ChangeQty);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Returns a list containing any Lots that are supplying material to this Material Requirement along with the quantity supplied, by demand id
    /// </summary>
    public IEnumerable<Tuple<Lot, decimal>> GetSupplyingLotByDemandId(BaseId a_demandId)
    {
        if (MRSupply != null)
        {
            foreach (Adjustment adjustment in MRSupply)
            {
                if (adjustment.GetReason().Id == a_demandId && adjustment.Storage?.Lot is Lot lot)
                {
                    yield return new Tuple<Lot, decimal>(lot, -adjustment.ChangeQty);
                }
            }
        }
    }


    /// <summary>
    /// Returns a list containing any Internal Activities that are supplying material to this Material Requirement.
    /// </summary>
    public IEnumerable<PurchaseToStock> SupplyingOrders
    {
        get
        {
            List<PurchaseToStock> list = new ();
            if (MRSupply != null)
            {
                foreach (Adjustment adjustment in MRSupply)
                {
                    if (adjustment is PurchaseOrderAdjustment poAdjustment)
                    {
                        yield return poAdjustment.PurchaseOrder;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Compare two Item.ItemGroup values for equivalent Item Names and having a length greater than 0.
    /// If either MaterialRequirement or Item is null false is returned.
    /// </summary>
    /// <param name="mr1">Can be null. The Code is examined.</param>
    /// <param name="mr2">Can be null. The Code is examined.</param>
    public static bool ItemGroupsEqualNotNullAndLenGTZero(MaterialRequirement mr1, MaterialRequirement mr2)
    {
        if (mr1 != null && mr2 != null && mr1.Item != null && mr2.Item != null)
        {
            return Common.Text.TextUtil.EqualAndLengthsGreaterThanZero(mr1.Item.Name, mr2.Item.Name);
        }

        return false;
    }

    /// <summary>
    /// A material requirement that is not currently satisfied.
    /// </summary>
    public class MaterialShortage
    {
        /// <summary>
        /// </summary>
        /// <param name="aMaterialRequirement"></param>
        /// <param name="aOperation"></param>
        /// <param name="aClock"></param>
        /// <param name="aEarliestDelivery">
        /// If the MR must be satisfied from one Warehouse then this is based on the Warehouse's leadtime for the Item.  Otherwise it's based on the shortest lead time possible
        /// from any Warehouses that stock the Item.
        /// </param>
        public MaterialShortage(MaterialRequirement aMaterialRequirement, BaseOperation aOperation, DateTime aEarliestDelivery)
        {
            m_mr = aMaterialRequirement;
            m_op = aOperation;
            m_earliestDelivery = aEarliestDelivery;
        }

        private readonly MaterialRequirement m_mr;

        /// <summary>
        /// The material that is short.
        /// </summary>
        public MaterialRequirement Material => m_mr;

        private readonly BaseOperation m_op;

        /// <summary>
        /// The Operation that will consume the material.
        /// </summary>
        public BaseOperation Operation => m_op;

        /// <summary>
        /// The quantity currently covered by On-Hand inventory or scheduled receipts.
        /// </summary>
        public decimal QtyPlanned => m_mr.TotalRequiredQty - m_mr.NonAllocatedQty;

        /// <summary>
        /// The TotalQtyRequired minus the QtyPlanned.
        /// </summary>
        public decimal QtyShort => m_mr.TotalRequiredQty - m_mr.GetSupplyFromAllSources - m_mr.NonAllocatedQty;

        /// <summary>
        /// The earlier of the JIT Start Date and the Scheduled Usage Date.
        /// </summary>
        public DateTime NeedByDate => new (Math.Min(JITStartDate.Ticks, ScheduledUsageDate.Ticks));

        /// <summary>
        /// The JIT Start date of the Operation to consume the material.
        /// </summary>
        public DateTime JITStartDate => m_op.DbrJitStartDate;

        /// <summary>
        /// The scheduled start date of the Operation to consume the material.
        /// </summary>
        public DateTime ScheduledUsageDate => m_op.StartDateTime;

        private readonly DateTime m_earliestDelivery;

        /// <summary>
        /// If the Material Requirement must be satisfied from one Warehouse then this is based on the Warehouse's leadtime for the Item.  Otherwise it's based on the shortest lead time possible from any
        /// Warehouses that stock the Item.</param>
        /// </summary>
        public DateTime EarliestDelivery => m_earliestDelivery;

        /// <summary>
        /// Whether the JIT start time of the Material Requirement is within the LeadTime for the Item or the (Inventory if Warehouse is specified in the Material Requirement).
        /// </summary>
        public bool WithinLeadTime => EarliestDelivery > NeedByDate;
    }

    public int CompareTo(MaterialRequirement a_other)
    {
        return a_other.Id.CompareTo(Id);
    }

    public override string ToString()
    {
        return string.Format("{0} of {1}: {2}".Localize(), TotalRequiredQty, MaterialName, MaterialDescription);
    }

    /// <summary>
    /// Adjust Total Required Qty and TotalCost based on the Planned Scrap Qty
    /// </summary>
    public void ApplyPlannedScrapQty()
    {
        decimal costRatio = PlannedScrapQty / TotalRequiredQty;
        TotalRequiredQty += PlannedScrapQty;
        TotalCost = Math.Round(TotalCost + TotalCost * Convert.ToDecimal(costRatio), 2);
    }

    private readonly bool m_allowedLotCodeIsSet; // this is used during update process to ensure AllowedLotCodes are not overwritten

    private EligibleLots m_eligibleLots = new ();
    internal EligibleLots EligibleLots => m_eligibleLots;

    private ItemDefs.MaterialAllocation m_materialAllocation = ItemDefs.MaterialAllocation.NotSet; // The default is NotSet. In this case, the corresponding Inventory's value will be used.

    /// <summary>
    /// This value allows you to override the same Item property. If this value is set to NotSet, which is the default value, this value in Item is used to determine whether to use the newest or oldest
    /// material.
    /// </summary>
    public ItemDefs.MaterialAllocation MaterialAllocation
    {
        get => m_materialAllocation;
        private set => m_materialAllocation = value;
    }

    /// <summary>
    /// If specified, each source of material must have at least this much quantity available to be used to fulfill this material requirement.
    /// </summary>
    private decimal m_minSourceQty;

    public decimal MinSourceQty
    {
        get => m_minSourceQty;
        private set => m_minSourceQty = value;
    }

    private decimal m_maxSourceQty;

    /// <summary>
    /// If specified, each source of material must have at least this much quantity available to be used to fulfil this material requirement.
    /// </summary>
    public decimal MaxSourceQty
    {
        get => m_maxSourceQty;
        private set => m_maxSourceQty = value;
    }

    private ItemDefs.MaterialSourcing m_materialSourcing;

    /// <summary>
    /// You can set this value to control certain aspects of how material is sourced.
    /// </summary>
    public ItemDefs.MaterialSourcing MaterialSourcing
    {
        get => m_materialSourcing;
        private set => m_materialSourcing = value;
    }

    public BaseId DemandId => Id;

    /// <summary>
    /// Adds lots this materialRequirement is eligible to draw material from.
    /// Don't edit a_eligibleLotExternalIds after calling this function.
    /// </summary>
    /// <param name="a_eligibleLotExternalIds">This value should only be created for use by this class. Don't edit it after calling this function.</param>
    internal void SetEligibleLots(HashSet<string> a_eligibleLotExternalIds)
    {
        foreach (string lotCode in a_eligibleLotExternalIds)
        {
            m_eligibleLots.Add(lotCode, this);
        }
    }

    /// <summary>
    /// Clears lots this materialRequirement is eligible to draw material from.
    /// </summary>
    internal void ClearEligibleLots()
    {
        m_eligibleLots.Clear();
    }

    internal void AddEligibleLotCode(string a_eligibleLotCode)
    {
        m_eligibleLots.Add(a_eligibleLotCode, this);
    }

    public IEnumerable<string> EligibleLotCodesEnumerator
    {
        get
        {
            foreach (string lotCode in m_eligibleLots.LotCodesEnumerator)
            {
                yield return lotCode;
            }
        }
    }

    internal Dictionary<string, EligibleLot>.Enumerator GetEligibleLotsEnumerator()
    {
        return m_eligibleLots.GetEligibleLotsEnumerator();
    }

    public int GetEligibleLotCount()
    {
        return m_eligibleLots.Count;
    }

    /// <summary>
    /// Whether a lot code is in this material requirement's set of eligible lot codes.
    /// </summary>
    /// <param name="a_lotCode"></param>
    /// <returns></returns>
    public bool ContainsEligibleLot(string a_lotCode)
    {
        return m_eligibleLots.Contains(a_lotCode);
    }

    internal void AddEligibleLotCodes(EligibleLots a_eligibleLotCodes)
    {
        Dictionary<string, EligibleLot>.Enumerator elEtr = m_eligibleLots.GetEligibleLotsEnumerator();
        while (elEtr.MoveNext())
        {
            a_eligibleLotCodes.Add(elEtr.Current.Key, this);
        }
    }

    /// <summary>
    /// Returns the penetration percent of the shelflife. This is WIP duration / shelflife
    /// -1 if not defined
    /// </summary>
    public double GetShelfLifePenetrationPercent(BaseOperation a_op)
    {
        if (Item != null && a_op.Scheduled)
        {
            TimeSpan shelfLifeDuration = Item.ShelfLife;
            if (shelfLifeDuration.Ticks > 0)
            {
                DateTime earliestSupplySourceDate = GetEarliestSupplySourceDate();
                if (earliestSupplySourceDate < PTDateTime.MaxDateTime)
                {
                    TimeSpan wipDruation = a_op.ScheduledStartDate - earliestSupplySourceDate;
                    return wipDruation.TotalHours / shelfLifeDuration.TotalHours * 100;
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// Returns the duration in which the supply source was complete before the material is used
    /// TimeSpan.MinValue if no supply
    /// </summary>
    public TimeSpan GetWipDuration(BaseOperation a_op)
    {
        DateTime earliestWipSupplySourceDate = GetEarliestSupplySourceDate();
        if (earliestWipSupplySourceDate < PTDateTime.MaxDateTime)
        {
            return a_op.ScheduledStartDate - earliestWipSupplySourceDate;
        }

        return TimeSpan.MinValue;
    }
    
    internal void Edit(ScenarioDetail a_sd, MaterialEdit a_edit, IScenarioDataChanges a_dataChanges)
    {
        //Update any fields that changed
        if (a_edit.AvailableDateSet)
        {
            LatestSourceDateTime = a_edit.AvailableDateTime;
        }

        if (a_edit.ConstraintTypeSet && ConstraintType != a_edit.ConstraintType)
        {
            ConstraintType = a_edit.ConstraintType;
            FlagItemForNetChangeMRP(a_sd.WarehouseManager);
            if (a_edit.ConstraintType is MaterialRequirementDefs.constraintTypes.ConstrainedByAvailableDate
                                      or MaterialRequirementDefs.constraintTypes.ConstrainedByEarlierOfLeadTimeOrAvailableDate)
            {
                a_dataChanges.FlagConstraintChanges(Operation.Job.Id);
            }
        }

        if (a_edit.IssuedQtySet)
        {
            IssuedQty = a_sd.ScenarioOptions.RoundQty(a_edit.IssuedQty);
            FlagItemForNetChangeMRP(a_sd.WarehouseManager);
        }

        if (LeadTimeSpan != a_edit.LeadTimeSpan)
        {
            LeadTimeSpan = a_edit.LeadTimeSpan;
        }

        if (a_edit.DescriptionSet)
        {
            MaterialDescription = a_edit.Description;
        }

        if (a_edit.MaterialNameSet)
        {
            MaterialName = a_edit.MaterialName;
        }

        if (a_edit.MaterialItemExternalIdSet)
        {
            FlagItemForNetChangeMRP(a_sd.WarehouseManager); //flag old item and new one

            Item editItem = a_sd.ItemManager.GetByExternalId(a_edit.MaterialItemExternalId);
            if (editItem == null)
            {
                throw new PTHandleableException("Item not found."); //TODO: Add or use existing error code
            }

            m_item = editItem;
            FlagItemForNetChangeMRP(a_sd.WarehouseManager);
        }

        if (a_edit.WarehouseExternalIdSet)
        {
            Warehouse editWarehouse = a_sd.WarehouseManager.GetByExternalId(a_edit.WarehouseExternalId);
            if (editWarehouse == null)
            {
                throw new PTHandleableException("Warehouse not found");
            }

            m_warehouse = editWarehouse;
            FlagItemForNetChangeMRP(a_sd.WarehouseManager);
        }

        if (a_edit.BuyDirectSourceSet)
        {
            Source = a_edit.BuyDirectSource;
        }

        if (a_edit.TotalCostSet)
        {
            TotalCost = a_edit.TotalCost;
        }

        if (a_edit.TotalRequiredQtySet)
        {
            BaseOperation op = a_sd.JobManager.FindOperation(a_edit.JobId, a_edit.MOId, a_edit.OperationId);

            if (!op.ManufacturingOrder.PreserveRequiredQty) //TODO: Check if this is necessary
            {
                if (op == null)
                {
                    throw new PTHandleableException("Operation not found");
                }

                ValidateTotalRequiredQty(op.ScenarioDetail.ScenarioOptions, a_edit.TotalRequiredQty, op.Job.ExternalId, op.ManufacturingOrder.ExternalId, op.ExternalId, ExternalId);
                TotalRequiredQty = op.ScenarioDetail.ScenarioOptions.RoundQty(a_edit.TotalRequiredQty);
                FlagItemForNetChangeMRP(a_sd.WarehouseManager);
            }
        }

        if (UOM != a_edit.UOM)
        {
            UOM = a_edit.UOM;
        }

        if (UseOverlapPOs != a_edit.UseOverlapPOs)
        {
            UseOverlapPOs = a_edit.UseOverlapPOs;
        }

        if (a_edit.MultipleWarehouseSupplyAllowedSet)
        {
            MultipleWarehouseSupplyAllowed = a_edit.MultipleWarehouseSupplyAllowed;
        }

        if (a_edit.AllowExpiredSupplySet)
        {
            AllowExpiredSupply = a_edit.AllowExpiredSupply;
        }

        if (a_edit.MultipleStorageAreaSupplyAllowedSet)
        {
            MultipleStorageAreaSupplyAllowed = a_edit.MultipleStorageAreaSupplyAllowed;
        }

        if (a_edit.TankStorageReleaseTimingSet)
        {
            m_tankStorageReleaseTiming = a_edit.TankStorageReleaseTiming;
        }

        if (a_edit.FixedQtySet)
        {
            FixedQty = a_edit.FixedQty;
        }

        if (a_edit.PlannedScrapQtySet)
        {
            PlannedScrapQty = a_edit.PlannedScrapQty;
        }

        if (a_edit.AllowedLotCodesSet)
        {
            HashSet<string> eligibleLots = new (a_edit.AllowedLotCodes.Split(','));
            ClearEligibleLots();
            SetEligibleLots(eligibleLots);
        }

        if (a_edit.AllowPartialSupplySet)
        {
            AllowPartialSupply = a_edit.AllowPartialSupply;
        }

        if (a_edit.MaterialAllocationSet)
        {
            MaterialAllocation = a_edit.MaterialAllocation;
        }

        if (a_edit.MinSourceQtySet)
        {
            MinSourceQty = a_edit.MinSourceQty;
        }

        if (a_edit.MinSourceQtySet)
        {
            MaxSourceQty = a_edit.MaxSourceQty;
        }

        if (a_edit.MaterialSourcingSet)
        {
            MaterialSourcing = a_edit.MaterialSourcing;
        }
        
        if (a_edit.RequireFirstTransferAtSetupSet)
        {
            RequireFirstTransferAtSetup = a_edit.RequireFirstTransferAtSetup;
        }
    }

    private JobDefs.EMaintenanceMethod m_maintenanceMethod = JobDefs.EMaintenanceMethod.ERP;

    public JobDefs.EMaintenanceMethod MaintenanceMethod
    {
        get => m_maintenanceMethod;
        internal set => m_maintenanceMethod = value;
    }

    public MaterialRequirementDefs.EMaterialUsedTiming MaterialUsedTiming => m_materialUsedTiming;

    private MaterialRequirementDefs.EMaterialUsedTiming m_materialUsedTiming;
    
    /// <summary>
    /// Gets storage areas the item can be pulled from, based on item storage compatibility and resource connectors.
    /// </summary>
    /// <param name="a_sd"></param>
    /// <returns></returns>
    public IEnumerable<StorageArea> GetEligibleStorageAreas(ScenarioDetail a_sd)
    {
        HashSet<StorageArea> eligibleStorageAreas = new HashSet<StorageArea>();

        //If a storage area was set on the MR, then it is the only Eligible Storage Area
        if (StorageArea != null)
        {
            return new List<StorageArea> { StorageArea };
        }

        if (Warehouse?.StorageAreas != null)
        {
            foreach (StorageArea storageArea in Warehouse.StorageAreas)
            {
                //Mark Storage Area as eligible if it has ItemStorages which can hold the specified Item
                if (storageArea.Storage.ContainsKey(Item.Id))
                {
                    eligibleStorageAreas.Add(storageArea);
                }
            }
        }


        //Further restrict/filter Eligible Storage Areas by Resource Connectors if any
        foreach (ResourceRequirement resourceRequirement in Operation.ResourceRequirements.Requirements)
        {
            foreach (Resource eligibleResource in resourceRequirement.GetEligibleResources())
            {
                foreach (Warehouse warehouse in a_sd.WarehouseManager)
                {
                    if (Warehouse != null && !warehouse.Equals(Warehouse))
                    {
                        continue;
                    }

                    IEnumerable<StorageAreaConnector> connectors = warehouse.StorageAreaConnectors.GetStorageConnectorsForResourceOut(eligibleResource);
                    foreach (StorageAreaConnector storageAreaConnector in connectors)
                    {
                        foreach (StorageArea storageArea in warehouse.StorageAreas)
                        {
                            if (storageAreaConnector.StorageAreaOutList.Contains(storageArea.Id))
                            {
                                eligibleStorageAreas.AddIfNew(storageArea);
                            }
                            //If MR is constrained by Storage Area Connectors but Storage Area is not in the Out Collection remove from eligible collection to be returned
                            else if (eligibleStorageAreas.Contains(storageArea))
                            {
                                eligibleStorageAreas.Remove(storageArea);
                            }
                        }
                    }
                }
            }
        }

        return eligibleStorageAreas;
    }

    /// <summary>
    /// Remove all links to supply sources and inventory signals
    /// </summary>
    internal void Deleting()
    {
        foreach (MaterialRequirementAdjustment mrAdj in MRSupply)
        {
            if (mrAdj.HasLotStorage)
            {
                mrAdj.Storage.Lot.DeleteDemand(mrAdj);
            }
            else
            {
                mrAdj.Inventory.DeleteDemand(mrAdj);
            }
        }
    }

    public decimal GetUnavailableQty(DateTime a_clockDate, InternalOperation a_op, InternalActivity a_act)
    {
        if (a_act.ProductionStatus >= InternalActivityDefs.productionStatuses.Running)
        {
            return 0m;
        }

        decimal availableToMr = 0m;
        if (Item != null)
        {
            decimal atpQty = 0;
            if (Warehouse != null)
            {
                atpQty = Warehouse.Inventories[Item.Id].GetAtpQty(a_op.ScheduledStartDate, a_clockDate);
                availableToMr += atpQty;
                //Add already pegged material
                availableToMr += GetSupplyFromInventories;
            }
            else if (MultipleWarehouseSupplyAllowed)
            {
                //sum inventory from all warehouses that can satisfy the Operation's plant.
                for (int plantI = 0; plantI < a_op.ManufacturingOrder.EligiblePlants.Count; plantI++)
                {
                    Plant plant = a_op.ManufacturingOrder.EligiblePlants[plantI].Plant;
                    for (int wI = 0; wI < plant.WarehouseCount; wI++)
                    {
                        Warehouse warehouse = plant.GetWarehouseAtIndex(wI);
                        Inventory inventory = warehouse.Inventories[Item.Id];
                        if (inventory != null)
                        {
                            atpQty += inventory.GetAtpQty(a_op.ScheduledStartDate, a_clockDate);

                            if (atpQty > UnIssuedQty)
                            {
                                return 0m;
                            }
                        }
                    }
                }

                availableToMr += atpQty;
            }
            else // only one warehouse is allowed
            {
                for (int plantI = 0; plantI < a_op.ManufacturingOrder.EligiblePlants.Count; plantI++)
                {
                    Plant plant = a_op.ManufacturingOrder.EligiblePlants[plantI].Plant;
                    for (int wI = 0; wI < plant.WarehouseCount; wI++)
                    {
                        Warehouse warehouse = plant.GetWarehouseAtIndex(wI);
                        if (!GetSuppliedWarehouses.Contains(warehouse.Name))
                        {
                            continue;
                        }

                        Inventory inventory = warehouse.Inventories[Item.Id];
                        if (inventory != null)
                        {
                            atpQty += inventory.GetAtpQty(a_op.ScheduledStartDate, a_clockDate);
                        }
                    }
                }

                availableToMr += atpQty;
                //Add already pegged material
                availableToMr += GetSupplyFromInventories;
            }

            //decimal availableToMr = Math.Max(atpQty, 0);
            decimal qtyIssued = GetSupplyFromInventories;
            qtyIssued += GetSupplyFromActivities;
            qtyIssued += GetSupplyFromPurchases;

            return Math.Max(UnIssuedQty - qtyIssued - availableToMr, 0m);
        }

        return 0m;
    }
}
