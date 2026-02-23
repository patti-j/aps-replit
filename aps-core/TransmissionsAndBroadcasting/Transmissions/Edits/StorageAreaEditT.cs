using PT.APSCommon.Extensions;
using PT.APSCommon;

namespace PT.Transmissions;
public class StorageAreaEditT : ScenarioIdBaseT, IPTSerializable
{
    #region PT Serialization
    public static int UNIQUE_ID => 1198;

    public StorageAreaEditT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12000)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                StorageAreaEdit node = new(a_reader);
                m_storageAreaEdits.Add(node);
            }

            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                WarehouseEdit node = new(a_reader);
                m_warehouseEdits.Add(node);
            }
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_storageAreaEdits);
        writer.Write(m_warehouseEdits);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public StorageAreaEditT() { }
    public StorageAreaEditT(BaseId a_scenarioId) : base(a_scenarioId) { }

    public void Validate(out string o_validationMessage)
    {
        o_validationMessage = string.Empty;
        HashSet<BaseId> referencedResourceIds = new ();
        foreach (StorageAreaEdit storageAreaEdit in m_storageAreaEdits)
        {
            if (storageAreaEdit.ResourceIdIsSet)
            {
                if (!referencedResourceIds.Add(storageAreaEdit.ResourceId))
                {
                    o_validationMessage = "Unable to save Storage Area changes. Multiple storage areas cannot reference the same resource.".Localize();
                    break;
                }
            }
        }

        if (!String.IsNullOrEmpty(o_validationMessage))
        {
            m_storageAreaEdits.Clear();
        }
    }

    public override string Description => string.Format("Warehouses updated ({0}); Storage Area updated ({1});".Localize(), m_warehouseEdits.Count, m_storageAreaEdits.Count);

    private readonly List<StorageAreaEdit> m_storageAreaEdits = new();
    public List<StorageAreaEdit> StorageAreaEdits => m_storageAreaEdits;

    private readonly List<WarehouseEdit> m_warehouseEdits = new();
    public List<WarehouseEdit> WarehouseEdits => m_warehouseEdits;
}
public class StorageAreaEdit : PTObjectBaseEdit, IPTSerializable
{
    public BaseId StorageAreaId;
    public BaseId WarehouseId;

    #region PT Serialization
    public StorageAreaEdit(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 13001)
        {
            WarehouseId = new BaseId(a_reader);
            StorageAreaId = new BaseId(a_reader);

            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            a_reader.Read(out m_storageInFlowLimit);
            a_reader.Read(out m_storageOutFlowLimit);
            a_reader.Read(out m_counterFlowLimit);

            m_resourceId = new BaseId(a_reader);
        }
        #region 12555
        else if (a_reader.VersionNumber >= 12555)
        {
            WarehouseId = new BaseId(a_reader);
            StorageAreaId = new BaseId(a_reader);

            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);

            a_reader.Read(out m_storageInFlowLimit);
            a_reader.Read(out m_storageOutFlowLimit);
            a_reader.Read(out m_counterFlowLimit);
        }
        else if (a_reader.VersionNumber >= 12523)
        {
            WarehouseId = new BaseId(a_reader);
            StorageAreaId = new BaseId(a_reader);

            m_bools = new BoolVector32(a_reader);
            m_setBools = new BoolVector32(a_reader);
        }
        #endregion        
    }

    public new void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        WarehouseId.Serialize(a_writer);
        StorageAreaId.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_setBools.Serialize(a_writer);

        a_writer.Write(m_storageInFlowLimit);
        a_writer.Write(m_storageOutFlowLimit);
        a_writer.Write(m_counterFlowLimit);

        m_resourceId.Serialize(a_writer);
    }

    public override bool HasEdits => base.HasEdits || m_setBools.AnyFlagsSet;

    public new int UniqueId => 1199;
    #endregion

    public StorageAreaEdit(BaseId a_warehouseId, BaseId a_storageAreaId)
    {
        WarehouseId = a_warehouseId;
        StorageAreaId = a_storageAreaId;
        m_externalId = null;
    }

    #region Shared Properties
    private BoolVector32 m_bools;
    private const short c_singleItemStorageIdx = 0;
    private const short c_constraintInFlowIdx = 1;
    private const short c_constraintOutFlowIdx = 2;
    private const short c_constraintCounterFlowIdx = 3;
    
    #region Set Bools
    private BoolVector32 m_setBools;
    private const short c_singleItemStorageSetIdx = 0;
    private const short c_storageInFlowLimitIsSetIdx = 1;
    private const short c_storageOutFlowLimitIsSetIdx = 2;
    private const short c_counterFlowLimitIsSetIdx = 3;

    private const short c_constraintInFlowIsSetIdx = 4;
    private const short c_constraintOutFlowIsSetIdx = 5;
    private const short c_constraintCounterFlowIsSetIdx = 6;

    private const short c_resIdIsSetIdx = 7;
    #endregion

    public bool SingleItemStorage
    {
        get => m_bools[c_singleItemStorageIdx];
        set
        {
            m_bools[c_singleItemStorageIdx] = value;
            m_setBools[c_singleItemStorageSetIdx] = true;
        }
    }
    private int m_storageInFlowLimit;

    public bool SingleItemStorageSet => m_setBools[c_singleItemStorageSetIdx];
    /// <summary>
    /// Indicates how many objects can store material into storage at the same time
    /// </summary>
    public int StorageInFlowLimit
    {
        get => m_storageInFlowLimit;
        set
        {
            if (value < 0)
            {
                throw new PTValidationException("3123", new object[] { ExternalId });
            }

            m_storageInFlowLimit = value;
            m_setBools[c_storageInFlowLimitIsSetIdx] = true;
        }
    }

    public bool StorageInFlowLimitIsSet => m_setBools[c_storageInFlowLimitIsSetIdx];

    private int m_storageOutFlowLimit;
    /// <summary>
    /// Indicates how many objects can withdraw material from storage at the same time
    /// </summary>
    public int StorageOutFlowLimit
    {
        get => m_storageOutFlowLimit;
        set
        {
            if (value < 0)
            {
                throw new PTValidationException("3124", new object[] { ExternalId });
            }

            m_storageOutFlowLimit = value;
            m_setBools[c_storageOutFlowLimitIsSetIdx] = true;
        }
    }

    public bool StorageOutFlowLimitIsSet => m_setBools[c_storageOutFlowLimitIsSetIdx];

    private int m_counterFlowLimit;
    /// <summary>
    /// Indicates the total limit of storing and withdrawing
    /// </summary>
    public int CounterFlowLimit
    {
        get => m_counterFlowLimit;
        set
        {
            if (value < 0)
            {
                throw new PTValidationException("3125", new object[] { ExternalId });
            }

            m_counterFlowLimit = value;
            m_setBools[c_counterFlowLimitIsSetIdx] = true;
        }
    }

    public bool CounterFlowLimitIsSet => m_setBools[c_counterFlowLimitIsSetIdx];

    public bool ConstrainInFlow
    {
        get => m_bools[c_constraintInFlowIdx];
        set
        {
            m_bools[c_constraintInFlowIdx] = value;
            m_setBools[c_constraintInFlowIsSetIdx] = true;
        }
    }

    public bool ConstrainInFlowIsSet => m_setBools[c_constraintInFlowIsSetIdx];

    public bool ConstrainOutFlow
    {
        get => m_bools[c_constraintOutFlowIdx];
        set
        {
            m_bools[c_constraintOutFlowIdx] = value;
            m_setBools[c_constraintOutFlowIsSetIdx] = true;
        }
    }

    public bool ConstrainOutFlowIsSet => m_setBools[c_constraintOutFlowIsSetIdx];

    public bool ConstrainCounterFlow
    {
        get => m_bools[c_constraintCounterFlowIdx];
        set
        {
            m_bools[c_constraintCounterFlowIdx] = value;
            m_setBools[c_constraintCounterFlowIsSetIdx] = true;
        }
    }

    public bool ConstrainCounterFlowIsSet => m_setBools[c_constraintCounterFlowIsSetIdx];

    private BaseId m_resourceId;
    public BaseId ResourceId
    {
        get => m_resourceId;
        set
        {
            m_resourceId = value;
            m_setBools[c_resIdIsSetIdx] = true;
        }
    }

    public bool ResourceIdIsSet => m_setBools[c_resIdIsSetIdx];
    #endregion
}