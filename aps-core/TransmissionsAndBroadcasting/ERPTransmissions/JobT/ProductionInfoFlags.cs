namespace PT.ERPTransmissions;

public class ProductionInfoFlags : IPTSerializable
{
    #region Serialization
    internal ProductionInfoFlags(IReader reader)
    {
        if (reader.VersionNumber >= 12421)
        {
            m_flags = new BoolVector32(reader);
            m_flags2 = new BoolVector32(reader);
        }
        else if (reader.VersionNumber >= 1)
        {
            m_flags = new BoolVector32(reader);
        }
    }

    public void Serialize(IWriter writer)
    {
        m_flags.Serialize(writer);
        m_flags2.Serialize(writer);
    }

    public const int UNIQUE_ID = 665;

    public int UniqueId => UNIQUE_ID;
    #endregion

    internal ProductionInfoFlags()
    {
        m_flags = new BoolVector32();
        m_flags2 = new BoolVector32();
    }

    internal void Copy(ProductionInfoFlags a_flags)
    {
        m_flags = new BoolVector32(a_flags.m_flags);
        m_flags2 = new BoolVector32(a_flags.m_flags2);
    }

    private BoolVector32 m_flags;
    private BoolVector32 m_flags2;

    private const int c_planningScrapPercentIdx = 0;
    private const int c_setupSpanIdx = 1;
    private const int c_cycleSpanIdx = 2;
    private const int c_postProcessingSpanIdx = 3;
    private const int c_qtyPerCycleIdx = 4;
    private const int c_resReqtsIdx = 5;
    private const int c_planningScrapPercentSetIdx = 6;
    private const int c_setupSpanSetIdx = 7;
    private const int c_cycleSpanSetIdx = 8;
    private const int c_postProcessingSpanSetIdx = 9;
    private const int c_qtyPerCycleSetIdx = 10;
    private const int c_resReqtSetIdx = 11;
    //private const int c_endOfStorageIdx = 12;
    //private const int c_endOfStorageSetIdx = 13;
    private const int c_materialPostProcessingSpanIdx = 14;
    private const int c_materialPostProcessingSpanSetIdx = 15;
    private const int c_cleanSpanIdx = 16;
    private const int c_cleanSpanSetIdx = 17;
    private const int c_planningScrapPercentOverrideIdx = 18;
    private const int c_planningScrapPercentOverrideSetIdx = 19;
    private const int c_setupSpanOverrideIdx = 20;
    private const int c_setupSpanOverrideSetIdx = 21;
    private const int c_cycleSpanOverrideIdx = 22;
    private const int c_cycleSpanOverrideSetIdx = 23;
    private const int c_postProcessingSpanOverrideIdx = 24;
    private const int c_postProcessingSpanOverrideSetIdx = 25;
    private const int c_qtyPerCycleOverrideIdx = 26;
    private const int c_qtyPerCycleOverrideSetIdx = 27;
    private const int c_resReqtsOverrideIdx = 28;
    private const int c_resReqtsOverrideSetIdx = 29;
    private const int c_materialPostProcessingSpanOverrideIdx = 30;
    private const int c_materialPostProcessingSpanOverrideSetIdx = 31;


    //m_flags2
    private const int c_splitOperationOverrideIdx = 1;
    private const int c_splitOperationOverrideSetIdx = 2;
    private const int c_cleanSpanOverrideIdx = 3;
    private const int c_cleanSpanOverrideSetIdx = 4;
    private const int c_transferQtyOverrideIdx = 5;
    private const int c_transferQtyOverrideSetIdx = 6;
    private const int c_storageSpanOverrideIdx = 7;
    private const int c_storageSpanOverrideSetIdx = 8;
    private const int c_storageSpanIdx = 9;
    private const int c_storageSpanSetIdx = 10;

    #region m_flags


    public bool PlanningScrapPercent
    {
        get => m_flags[c_planningScrapPercentIdx];
        internal set
        {
            m_flags[c_planningScrapPercentIdx] = value;
            PlanningScrapPercentSet = true;
        }
    }

    public bool PlanningScrapPercentSet
    {
        get => m_flags[c_planningScrapPercentSetIdx];
        private set => m_flags[c_planningScrapPercentSetIdx] = value;
    }

    public bool SetupManualUpdateOnly
    {
        get => m_flags[c_setupSpanIdx];
        internal set
        {
            m_flags[c_setupSpanIdx] = value;
            SetupManualUpdateOnlySet = true;
        }
    }

    public bool SetupManualUpdateOnlySet
    {
        get => m_flags[c_setupSpanSetIdx];
        private set => m_flags[c_setupSpanSetIdx] = value;
    }

    public bool CycleManualUpdateOnly
    {
        get => m_flags[c_cycleSpanIdx];
        internal set
        {
            m_flags[c_cycleSpanIdx] = value;
            CycleManualUpdateOnlySet = true;
        }
    }

    public bool CycleManualUpdateOnlySet
    {
        get => m_flags[c_cycleSpanSetIdx];
        private set => m_flags[c_cycleSpanSetIdx] = value;
    }

    public bool PostProcessingManualUpdateOnly
    {
        get => m_flags[c_postProcessingSpanIdx];
        internal set
        {
            m_flags[c_postProcessingSpanIdx] = value;
            PostProcessingManualUpdateOnlySet = true;
        }
    }

    public bool PostProcessingManualUpdateOnlySet
    {
        get => m_flags[c_postProcessingSpanSetIdx];
        private set => m_flags[c_postProcessingSpanSetIdx] = value;
    }

    public bool CleanManualUpdateOnly
    {
        get => m_flags[c_cleanSpanIdx];
        internal set
        {
            m_flags[c_cleanSpanIdx] = value;
            CleanManualUpdateOnlySet = true;
        }
    }

    public bool CleanManualUpdateOnlySet
    {
        get => m_flags[c_cleanSpanSetIdx];
        private set => m_flags[c_cleanSpanSetIdx] = value;
    }

    public bool QtyPerCycle
    {
        get => m_flags[c_qtyPerCycleIdx];
        internal set
        {
            m_flags[c_qtyPerCycleIdx] = value;
            QtyPerCycleSet = true;
        }
    }

    public bool QtyPerCycleSet
    {
        get => m_flags[c_qtyPerCycleSetIdx];
        private set => m_flags[c_qtyPerCycleSetIdx] = value;
    }

    public bool ResourceRequirements
    {
        get => m_flags[c_resReqtsIdx];
        internal set
        {
            m_flags[c_resReqtsIdx] = value;
            ResourceRequirementsSet = true;
        }
    }

    public bool ResourceRequirementsSet
    {
        get => m_flags[c_resReqtSetIdx];
        private set => m_flags[c_resReqtSetIdx] = value;
    }

    //public bool MaterialPostProcessingManualUpdateOnly
    //{
    //    get => m_flags[c_materialPostProcessingSpanIdx];
    //    internal set
    //    {
    //        m_flags[c_materialPostProcessingSpanIdx] = value;
    //        MaterialPostProcessingManualUpdateOnlySet = true;
    //    }
    //}

    public bool MaterialPostProcessingManualUpdateOnlySet
    {
        get => m_flags[c_materialPostProcessingSpanSetIdx];
        private set => m_flags[c_materialPostProcessingSpanSetIdx] = value;
    }
    public bool PlanningScrapPercentOverride
    {
        get => m_flags[c_planningScrapPercentOverrideIdx];
        internal set
        {
            m_flags[c_planningScrapPercentOverrideIdx] = value;
            PlanningScrapPercentOverrideSet = true;
        }
    } 
    
    public bool PlanningScrapPercentOverrideSet
    {
        get => m_flags[c_planningScrapPercentOverrideSetIdx];
         set => m_flags[c_planningScrapPercentOverrideSetIdx] = value;
    } 
    public bool SetupSpanOverride
    {
        get => m_flags[c_setupSpanOverrideIdx];
        internal set
        {
            m_flags[c_setupSpanOverrideIdx] = value;
            SetupSpanOverrideSet = true;
        }
    }
    public bool SetupSpanOverrideSet
    {
        get => m_flags[c_setupSpanOverrideSetIdx];
        set => m_flags[c_setupSpanOverrideSetIdx] = value;
    }
    public bool CycleSpanOverride
    {
        get => m_flags[c_cycleSpanOverrideIdx];
        internal set
        {
            m_flags[c_cycleSpanOverrideIdx] = value;
            CycleSpanOverrideSet = true;
        }
    } 
    public bool CycleSpanOverrideSet
    {
        get => m_flags[c_cycleSpanOverrideSetIdx];
        set => m_flags[c_cycleSpanOverrideSetIdx] = value;
    }
    public bool PostProcessingSpanOverride
    {
        get => m_flags[c_postProcessingSpanOverrideIdx];
        internal set
        {
            m_flags[c_postProcessingSpanOverrideIdx] = value; 
            PostProcessingSpanOverrideSet = true;
        }
    }
    public bool PostProcessingSpanOverrideSet
    {
        get => m_flags[c_postProcessingSpanOverrideSetIdx];
        set => m_flags[c_postProcessingSpanOverrideSetIdx] = value;
    }
    public bool QtyPerCycleOverride
    {
        get => m_flags[c_qtyPerCycleOverrideIdx];
        internal set
        {
            m_flags[c_qtyPerCycleOverrideIdx] = value; 
            QtyPerCycleOverrideSet = true;
        }
    }
    public bool QtyPerCycleOverrideSet
    {
        get => m_flags[c_qtyPerCycleOverrideSetIdx];
        set => m_flags[c_qtyPerCycleOverrideSetIdx] = value;
    }
    public bool ResReqtsOverride
    {
        get => m_flags[c_resReqtsOverrideIdx];
        internal set
        {
            m_flags[c_resReqtsOverrideIdx] = value; 
            ResReqtsOverrideSet = true;
        }
    }
    public bool ResReqtsOverrideSet
    {
        get => m_flags[c_resReqtsOverrideSetIdx];
        set => m_flags[c_resReqtsOverrideSetIdx] = value;
    }
    public bool MaterialPostProcessingSpanOverride
    {
        get => m_flags[c_materialPostProcessingSpanOverrideIdx];
        internal set
        {
            m_flags[c_materialPostProcessingSpanOverrideIdx] = value; 
            MaterialPostProcessingSpanOverrideSet = true;
        }
    }
    public bool MaterialPostProcessingSpanOverrideSet
    {
        get => m_flags[c_materialPostProcessingSpanOverrideSetIdx];
        set => m_flags[c_materialPostProcessingSpanOverrideSetIdx] = value;
    }
    #endregion

    #region m_flags2
    public bool SplitOperationOverride
    {
        get => m_flags2[c_splitOperationOverrideIdx];
        internal set
        {
            m_flags2[c_splitOperationOverrideIdx] = value; 
            SplitOperationOverrideSet = true;
        }
    }
    public bool SplitOperationOverrideSet
    {
        get => m_flags2[c_splitOperationOverrideSetIdx];
        set => m_flags2[c_splitOperationOverrideSetIdx] = value;
    }
    public bool CleanSpanOverride
    {
        get => m_flags2[c_cleanSpanOverrideIdx];
        internal set
        {
            m_flags2[c_cleanSpanOverrideIdx] = value;
            CleanSpanOverrideSet = true;
        }
    }
    public bool CleanSpanOverrideSet
    {
        get => m_flags2[c_cleanSpanOverrideSetIdx];
        set => m_flags2[c_cleanSpanOverrideSetIdx] = value;
    }

    public bool StorageSpanOverride
    {
        get => m_flags2[c_storageSpanOverrideIdx];
        internal set
        {
            m_flags2[c_storageSpanOverrideIdx] = value;
            StorageSpanOverrideSet = true;
        }
    }

    public bool StorageSpanOverrideSet
    {
        get => m_flags2[c_storageSpanOverrideSetIdx];
        set => m_flags2[c_storageSpanOverrideSetIdx] = value;
    }

    public bool TransferQtyOverride
    {
        get => m_flags2[c_transferQtyOverrideIdx];
        internal set
        {
            m_flags2[c_transferQtyOverrideIdx] = value;
            TransferQtyOverrideSet = true;
        }
    }

    public bool TransferQtyOverrideSet
    {
        get => m_flags2[c_transferQtyOverrideSetIdx];
        set => m_flags2[c_transferQtyOverrideSetIdx] = value;
    }

    public bool StorageManualUpdateOnly
    {
        get => m_flags[c_storageSpanIdx];
        internal set
        {
            m_flags[c_storageSpanIdx] = value;
            StorageManualUpdateOnlySet = true;
        }
    }

    public bool StorageManualUpdateOnlySet
    {
        get => m_flags[c_storageSpanSetIdx];
        private set => m_flags[c_storageSpanSetIdx] = value;
    }

    #endregion
    public override string ToString()
    {
        return m_flags.ToString() + m_flags2.ToString();
    }
}