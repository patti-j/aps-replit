using System.Collections;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

public class MaterialEditT : ScenarioIdBaseT, IPTSerializable, IEnumerable<MaterialEdit>
{
    private readonly List<MaterialEdit> m_materialEdits = new ();

    #region PT Serialization
    public MaterialEditT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                MaterialEdit node = new (a_reader);
                m_materialEdits.Add(node);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_materialEdits);
    }

    public static int UNIQUE_ID => 1049;
    public override int UniqueId => UNIQUE_ID;
    #endregion

    public MaterialEditT() { }
    public MaterialEditT(BaseId a_scenarioId) : base(a_scenarioId) { }

    public MaterialEdit this[int i] => m_materialEdits[i];

    public void Validate()
    {
        foreach (MaterialEdit materialEdit in m_materialEdits)
        {
            materialEdit.Validate();
        }
    }

    public override string Description => string.Format("Materials updated ({0})".Localize(), m_materialEdits.Count);

    public IEnumerator<MaterialEdit> GetEnumerator()
    {
        return m_materialEdits.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(MaterialEdit a_materialEdit)
    {
        m_materialEdits.Add(a_materialEdit);
    }
}

public class MaterialEdit : PTObjectBaseEdit, IPTSerializable
{
    #region PT Serialization
    public MaterialEdit(IReader a_reader)
        : base(a_reader)
    {
        #region 12000
        if (a_reader.VersionNumber >= 12554)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OperationId = new BaseId(a_reader);
            RequirementId = new BaseId(a_reader);
            ItemId = new BaseId(a_reader);

            a_reader.Read(out m_availableDate);
            int val;
            a_reader.Read(out val);
            m_constraintType = (MaterialRequirementDefs.constraintTypes)val;

            a_reader.Read(out m_issuedQty);
            a_reader.Read(out m_materialName);
            a_reader.Read(out m_buyDirectSource);
            a_reader.Read(out m_totalCost);
            a_reader.Read(out m_totalRequiredQty);
            a_reader.Read(out m_nonAllocatedQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_leadTimeSpan);

            a_reader.Read(out val);
            m_tankStorageReleaseTiming = (MaterialRequirementDefs.TankStorageReleaseTimingEnum)val;
            a_reader.Read(out m_materialItemExternalId);
            a_reader.Read(out m_plannedScrapQty);
            a_reader.Read(out val);
            m_materialAllocation = (ItemDefs.MaterialAllocation)val;
            a_reader.Read(out m_minSourceQty);
            a_reader.Read(out m_maxSourceQty);
            a_reader.Read(out val);
            m_materialSourcing = (ItemDefs.MaterialSourcing)val;
            a_reader.Read(out m_allowedLotCodes);
        }
        else if (a_reader.VersionNumber >= 12000)
        {
            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            JobId = new BaseId(a_reader);
            MOId = new BaseId(a_reader);
            OperationId = new BaseId(a_reader);
            ActivityId = new BaseId(a_reader);
            RequirementId = new BaseId(a_reader);
            PlantId = new BaseId(a_reader);
            WarehouseId = new BaseId(a_reader);
            ItemId = new BaseId(a_reader);

            a_reader.Read(out m_availableDate);
            int val;
            a_reader.Read(out val);
            m_constraintType = (MaterialRequirementDefs.constraintTypes)val;

            a_reader.Read(out m_issuedQty);
            a_reader.Read(out m_materialName);
            a_reader.Read(out m_buyDirectSource);
            a_reader.Read(out m_totalCost);
            a_reader.Read(out m_totalRequiredQty);
            a_reader.Read(out m_nonAllocatedQty);
            a_reader.Read(out m_uom);
            a_reader.Read(out m_leadTimeSpan);

            a_reader.Read(out val);
            m_tankStorageReleaseTiming = (MaterialRequirementDefs.TankStorageReleaseTimingEnum)val;
            a_reader.Read(out m_materialItemExternalId);
            a_reader.Read(out m_plannedScrapQty);
            a_reader.Read(out val);
            m_materialAllocation = (ItemDefs.MaterialAllocation)val;
            a_reader.Read(out m_minSourceQty);
            a_reader.Read(out m_maxSourceQty);
            a_reader.Read(out val);
            m_materialSourcing = (ItemDefs.MaterialSourcing)val;
            a_reader.Read(out m_allowedLotCodes);
        }
        #endregion
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_setBools.Serialize(a_writer);

        JobId.Serialize(a_writer);
        MOId.Serialize(a_writer);
        OperationId.Serialize(a_writer);
        RequirementId.Serialize(a_writer);
        ItemId.Serialize(a_writer);

        a_writer.Write(m_availableDate);
        a_writer.Write((int)m_constraintType);
        a_writer.Write(m_issuedQty);
        a_writer.Write(m_materialName);
        a_writer.Write(m_buyDirectSource);
        a_writer.Write(m_totalCost);
        a_writer.Write(m_totalRequiredQty);
        a_writer.Write(m_nonAllocatedQty);
        a_writer.Write(m_uom);
        a_writer.Write(m_leadTimeSpan);
        a_writer.Write((int)m_tankStorageReleaseTiming);

        a_writer.Write(m_materialItemExternalId);

        //bool haveWarehouse = m_warehouse != null;
        //a_writer.Write(haveWarehouse);
        //if (haveWarehouse)
        //    m_warehouse.Id.Serialize(writer);

        a_writer.Write(m_plannedScrapQty);
        a_writer.Write((int)m_materialAllocation);
        a_writer.Write(m_minSourceQty);
        a_writer.Write(m_maxSourceQty);
        a_writer.Write((int)m_materialSourcing);
        a_writer.Write(m_allowedLotCodes);
    }

    public MaterialEdit(BaseId a_jobId, BaseId a_moId, BaseId a_operationId, BaseId a_requirementId, BaseId a_itemId)
    {
        JobId = a_jobId;
        MOId = a_moId;
        OperationId = a_operationId;
        RequirementId = a_requirementId;
        ItemId = a_itemId;
    }

    public new int UniqueId => 1049;

    public override bool HasEdits => base.HasEdits || m_setBools.AnyFlagsSet;
    #endregion

    #region Shared Properties
    private BoolVector32 m_bools;
    private const short c_issueFromInventoryIdx = 0;
    //private const short c_useOverlapActivitiesIdx = 1; Obsolete
    private const short c_useOverlapPOsIdx = 2;
    private const short c_multipleWarehouseSupplyAllowedIdx = 3;
    private const short c_fixedQtyIdx = 4;
    private const short c_allowPartialSupplyIdx = 5;
    private const short c_multipleStorageAreaAllowedIdx = 6;
    private const short c_requireFirstTransferAtSetupIdx = 7;
    private const short c_allowExpiredSupplyIdx = 8;

    private BoolVector32 m_setBools;
    private const short c_qtySetIdx = 0;
    private const short c_issueFromInventorySetIdx = 1;
    private const short c_materialNameSetIdx = 2;
    private const short c_materialItemExternalIdSetIdx = 3;
    private const short c_materialsConstraintTypeSetIdx = 4;
    private const short c_availableDateTimeSetIdx = 5;
    private const short c_buyDirectSourceSetIdx = 6;
    private const short c_totalCostSetIdx = 7;
    private const short c_totalRequiredQtySetIdx = 8;
    private const short c_nonAllocatedQtySetIdx = 9;
    private const short c_uomSetIdx = 10;
    private const short c_leadTimeSpanSetIdx = 11;
    private const short c_tankStorageReleaseTimingSetIdx = 12;
    private const short c_plannedScrapQtySetIdx = 13;
    private const short c_materialAllocationSetIdx = 14;
    private const short c_minSourceQtySetIdx = 15;
    private const short c_maxSourceQtySetIdx = 16;
    private const short c_materialSourcingSetIdx = 17;
    private const short c_warehouseExternalIdSetIdx = 18;
    private const short c_useOverlapActivitiesSetIdx = 19;
    private const short c_useOverlapPOsSetIdx = 20;
    private const short c_multipleWarehouseSupplyAllowedSetIdx = 21;
    private const short c_fixedQtySetIdx = 22;
    private const short c_allowedLotCodesSetIdx = 23;
    private const short c_allowPartialSupplySetIdx = 24;
    private const short c_multipleStorageAreaSupplyAllowedSetIdx = 25;
    private const short c_requireFirstTransferAtSetupIsSetIdx = 26;
    private const short c_allowExpiredSupplySetIdx = 27;

    public readonly BaseId JobId;
    public readonly BaseId MOId;
    public readonly BaseId OperationId;
    public readonly BaseId ActivityId;
    public readonly BaseId RequirementId;
    public readonly BaseId PlantId;
    public readonly BaseId WarehouseId;
    public readonly BaseId ItemId;

    private decimal m_issuedQty;

    public decimal IssuedQty
    {
        get => m_issuedQty;
        set
        {
            m_issuedQty = value;
            m_setBools[c_qtySetIdx] = true;
        }
    }

    public bool IssuedQtySet => m_setBools[c_qtySetIdx];

    public bool IssueFromInventory
    {
        get => m_bools[c_issueFromInventoryIdx];
        set
        {
            m_bools[c_issueFromInventoryIdx] = value;
            m_setBools[c_issueFromInventorySetIdx] = true;
        }
    }

    public bool IssueFromInventorySet => m_setBools[c_issueFromInventoryIdx];

    private string m_materialName;

    public string MaterialName
    {
        get => m_materialName;
        set
        {
            m_materialName = value;
            m_setBools[c_materialNameSetIdx] = true;
        }
    }

    public bool MaterialNameSet => m_setBools[c_materialNameSetIdx];

    private readonly string m_materialItemExternalId;

    public string MaterialItemExternalId
    {
        get => m_materialName;
        set
        {
            m_materialName = value;
            m_setBools[c_materialItemExternalIdSetIdx] = true;
        }
    }

    public bool MaterialItemExternalIdSet => m_setBools[c_materialItemExternalIdSetIdx];

    private string m_warehouseExternalId;

    public string WarehouseExternalId
    {
        get => m_warehouseExternalId;
        set
        {
            m_warehouseExternalId = value;
            m_setBools[c_warehouseExternalIdSetIdx] = true;
        }
    }

    public bool WarehouseExternalIdSet => m_setBools[c_warehouseExternalIdSetIdx];

    private MaterialRequirementDefs.constraintTypes m_constraintType;

    public MaterialRequirementDefs.constraintTypes ConstraintType
    {
        get => m_constraintType;
        set
        {
            m_constraintType = value;
            m_setBools[c_materialsConstraintTypeSetIdx] = true;
        }
    }

    public bool ConstraintTypeSet => m_setBools[c_materialsConstraintTypeSetIdx];

    private long m_availableDate = DateTime.MinValue.Ticks;

    public DateTime AvailableDateTime
    {
        get => new (m_availableDate);
        set
        {
            m_availableDate = value.Ticks;
            m_setBools[c_availableDateTimeSetIdx] = true;
        }
    }

    public bool AvailableDateSet => m_setBools[c_availableDateTimeSetIdx];

    private string m_buyDirectSource;

    public string BuyDirectSource
    {
        get => m_buyDirectSource;
        set
        {
            m_buyDirectSource = value;
            m_setBools[c_buyDirectSourceSetIdx] = true;
        }
    }

    public bool BuyDirectSourceSet => m_setBools[c_buyDirectSourceSetIdx];

    private decimal m_totalCost;

    public decimal TotalCost
    {
        get => m_totalCost;
        set
        {
            m_totalCost = value;
            m_setBools[c_totalCostSetIdx] = true;
        }
    }

    public bool TotalCostSet => m_setBools[c_totalCostSetIdx];

    private decimal m_totalRequiredQty;

    public decimal TotalRequiredQty
    {
        get => m_totalRequiredQty;
        set
        {
            m_totalRequiredQty = value;
            m_setBools[c_totalRequiredQtySetIdx] = true;
        }
    }

    public bool TotalRequiredQtySet => m_setBools[c_totalRequiredQtySetIdx];

    private decimal m_nonAllocatedQty;

    public decimal NonAllocatedQty
    {
        get => m_nonAllocatedQty;
        set
        {
            m_nonAllocatedQty = value;
            m_setBools[c_nonAllocatedQtySetIdx] = true;
        }
    }

    public bool NonAllocatedQtySet => m_setBools[c_nonAllocatedQtySetIdx];

    private string m_uom;

    public string UOM
    {
        get => m_uom;
        set
        {
            m_uom = value;
            m_setBools[c_uomSetIdx] = true;
        }
    }

    public bool UOMSet => m_setBools[c_uomSetIdx];

    private long m_leadTimeSpan;

    public TimeSpan LeadTimeSpan
    {
        get => new (m_leadTimeSpan);
        set
        {
            m_leadTimeSpan = value.Ticks;
            m_setBools[c_leadTimeSpanSetIdx] = true;
        }
    }

    public bool LeadTimeSpanSet => m_setBools[c_leadTimeSpanSetIdx];

    private MaterialRequirementDefs.TankStorageReleaseTimingEnum m_tankStorageReleaseTiming = MaterialRequirementDefs.TankStorageReleaseTimingEnum.NotTank;

    public MaterialRequirementDefs.TankStorageReleaseTimingEnum TankStorageReleaseTiming
    {
        get => m_tankStorageReleaseTiming;
        set
        {
            m_tankStorageReleaseTiming = value;
            m_setBools[c_tankStorageReleaseTimingSetIdx] = true;
        }
    }

    public bool TankStorageReleaseTimingSet => m_setBools[c_tankStorageReleaseTimingSetIdx];

    private decimal m_plannedScrapQty;

    /// <summary>
    /// The amount of extra material required that will be scrapped. This is not scaled like required quantity.
    /// Note: MRP increases the total required quantity by this amount.
    /// </summary>
    public decimal PlannedScrapQty
    {
        get => m_plannedScrapQty;
        set
        {
            m_plannedScrapQty = value;
            m_setBools[c_plannedScrapQtySetIdx] = true;
        }
    }

    public bool PlannedScrapQtySet => m_setBools[c_plannedScrapQtySetIdx];

    private ItemDefs.MaterialAllocation m_materialAllocation = ItemDefs.MaterialAllocation.NotSet;

    public ItemDefs.MaterialAllocation MaterialAllocation
    {
        get => m_materialAllocation;
        set
        {
            m_materialAllocation = value;
            m_setBools[c_materialAllocationSetIdx] = true;
        }
    }

    public bool MaterialAllocationSet => m_setBools[c_materialAllocationSetIdx];

    private decimal m_minSourceQty;

    public decimal MinSourceQty
    {
        get => m_minSourceQty;
        set
        {
            m_minSourceQty = value;
            m_setBools[c_minSourceQtySetIdx] = true;
        }
    }

    public bool MinSourceQtySet => m_setBools[c_minSourceQtySetIdx];

    private decimal m_maxSourceQty;

    public decimal MaxSourceQty
    {
        get => m_maxSourceQty;
        set
        {
            m_maxSourceQty = value;
            m_setBools[c_maxSourceQtySetIdx] = true;
        }
    }

    public bool MaxSourceQtySet => m_setBools[c_maxSourceQtySetIdx];

    private ItemDefs.MaterialSourcing m_materialSourcing;

    /// <summary>
    /// You can set this value to control certain aspects of how material is sourced.
    /// </summary>
    public ItemDefs.MaterialSourcing MaterialSourcing
    {
        get => m_materialSourcing;
        set
        {
            m_materialSourcing = value;
            m_setBools[c_materialSourcingSetIdx] = true;
        }
    }

    public bool MaterialSourcingSet => m_setBools[c_materialSourcingSetIdx];


    /// <summary>
    /// Material can come from POs whose date hasn't been reached by the time the activity with the material requirement starts.
    /// </summary>
    public bool UseOverlapPOs
    {
        get => m_bools[c_useOverlapPOsIdx];
        set
        {
            m_bools[c_useOverlapPOsIdx] = value;
            m_setBools[c_useOverlapPOsSetIdx] = true;
        }
    }

    public bool UseOverlapPOsSet => m_setBools[c_useOverlapPOsSetIdx];

    public bool MultipleWarehouseSupplyAllowed
    {
        get => m_bools[c_multipleWarehouseSupplyAllowedIdx];
        set
        {
            m_bools[c_multipleWarehouseSupplyAllowedIdx] = value;
            m_setBools[c_multipleWarehouseSupplyAllowedSetIdx] = true;
        }
    }

    public bool MultipleWarehouseSupplyAllowedSet => m_setBools[c_multipleWarehouseSupplyAllowedSetIdx];
    
    public bool MultipleStorageAreaSupplyAllowed
    {
        get => m_bools[c_multipleStorageAreaAllowedIdx];
        set
        {
            m_bools[c_multipleStorageAreaAllowedIdx] = value;
            m_setBools[c_multipleStorageAreaSupplyAllowedSetIdx] = true;
        }
    }

    public bool MultipleStorageAreaSupplyAllowedSet => m_setBools[c_multipleStorageAreaSupplyAllowedSetIdx];

    public bool FixedQty
    {
        get => m_bools[c_fixedQtyIdx];
        set
        {
            m_bools[c_fixedQtyIdx] = value;
            m_setBools[c_fixedQtySetIdx] = true;
        }
    }

    public bool RequireFirstTransferAtSetup
    {
        get => m_bools[c_requireFirstTransferAtSetupIdx];
        set
        {
            m_bools[c_requireFirstTransferAtSetupIdx] = value;
            m_setBools[c_requireFirstTransferAtSetupIsSetIdx] = true;
        }
    }

    public bool RequireFirstTransferAtSetupSet => m_setBools[c_requireFirstTransferAtSetupIsSetIdx];

    public bool FixedQtySet => m_setBools[c_fixedQtySetIdx];

    private string m_allowedLotCodes;

    public string AllowedLotCodes
    {
        get => m_allowedLotCodes;
        set
        {
            m_allowedLotCodes = value;
            m_setBools[c_allowedLotCodesSetIdx] = true;
        }
    }

    public bool AllowedLotCodesSet => m_setBools[c_allowedLotCodesSetIdx];

    public bool AllowPartialSupply
    {
        get => m_bools[c_allowPartialSupplyIdx];
        set
        {
            m_bools[c_allowPartialSupplyIdx] = value;
            m_setBools[c_allowPartialSupplySetIdx] = true;
        }
    }

    public bool AllowPartialSupplySet => m_setBools[c_allowPartialSupplySetIdx];

    public bool AllowExpiredSupply
    {
        get => m_bools[c_allowExpiredSupplyIdx];
        set
        {
            m_bools[c_allowExpiredSupplyIdx] = value;
            m_setBools[c_allowExpiredSupplySetIdx] = true;
        }
    }

    public bool AllowExpiredSupplySet => m_setBools[c_allowExpiredSupplySetIdx];
    #endregion

    public void Validate()
    {
        if (PlannedScrapQtySet && PlannedScrapQty < 0)
        {
            throw new PTValidationException("2931", new object[] { PlannedScrapQty });
        }
    }
}