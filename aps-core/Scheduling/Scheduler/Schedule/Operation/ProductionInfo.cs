using System.Text;

using PT.APSCommon;
using PT.Scheduler.Simulation;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.Schedule.Operation;

public class ProductionInfo : IPTSerializable
{
    #region Serialization
    internal ProductionInfo(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12521)
        {
            m_flags = new BoolVector32(a_reader);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_cleanSpanTicks);
            a_reader.Read(out m_cleanoutGrade);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_transferQty);

            a_reader.Read(out m_materialPostProcessingSpanTicks);
            a_reader.Read(out m_productionSetupCost);
            a_reader.Read(out m_cleanoutCost);
            a_reader.Read(out m_storageSpanTicks);
        }
        else if (a_reader.VersionNumber >= 12510)
        {
            m_flags = new BoolVector32(a_reader);

            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_cleanSpanTicks);
            a_reader.Read(out m_cleanoutGrade);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_transferQty);

            a_reader.Read(out m_materialPostProcessingSpanTicks);
            a_reader.Read(out m_productionSetupCost);
            a_reader.Read(out m_cleanoutCost);
        }
        else if (a_reader.VersionNumber >= 12419)
        {
            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_cleanSpanTicks);
            a_reader.Read(out m_cleanoutGrade);
            a_reader.Read(out m_qtyPerCycle);
            a_reader.Read(out m_transferQty);

            m_flags = new BoolVector32(a_reader);

            a_reader.Read(out m_materialPostProcessingSpanTicks);
        }
        else if (a_reader.VersionNumber >= 12303)
        {
            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_cleanSpanTicks);
            a_reader.Read(out m_cleanoutGrade);
            a_reader.Read(out m_qtyPerCycle);

            m_flags = new BoolVector32(a_reader);

            a_reader.Read(out m_materialPostProcessingSpanTicks);
        }
        else if (a_reader.VersionNumber >= 713)
        {
            a_reader.Read(out m_planningScrapPercent);
            a_reader.Read(out m_setupSpanTicks);
            a_reader.Read(out m_cycleSpanTicks);
            a_reader.Read(out m_postProcessingSpanTicks);
            a_reader.Read(out m_qtyPerCycle);

            m_flags = new BoolVector32(a_reader);

            a_reader.Read(out m_materialPostProcessingSpanTicks);
        }
    }

    public virtual void Serialize(IWriter a_writer)
    {
        m_flags.Serialize(a_writer);

        a_writer.Write(m_planningScrapPercent);
        a_writer.Write(m_setupSpanTicks);
        a_writer.Write(m_cycleSpanTicks);
        a_writer.Write(m_postProcessingSpanTicks);
        a_writer.Write(m_cleanSpanTicks);
        a_writer.Write(m_cleanoutGrade);
        a_writer.Write(m_qtyPerCycle);
        a_writer.Write(m_transferQty);
        a_writer.Write(m_materialPostProcessingSpanTicks);
        a_writer.Write(m_productionSetupCost);
        a_writer.Write(m_cleanoutCost);
        a_writer.Write(m_storageSpanTicks);
    }

    public const int UNIQUE_ID = 675;

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    internal ProductionInfo()
    {
        m_flags = new BoolVector32();
    }

    public override string ToString()
    {
        return string.Format("ProductionInfo: Setup={0}; Cycle={1}; PostProcessing={2}; Clean={3}; CleanOutGrade={4}; QtyPerCycle={5}; PlanningScrapPercent={6}; TransferQty={7}; ChangeoverCost={8}; CleanoutCost={9}",
            TimeSpan.FromTicks(SetupSpanTicks),
            TimeSpan.FromTicks(CycleSpanTicks),
            TimeSpan.FromTicks(PostProcessingSpanTicks),
            TimeSpan.FromTicks(CleanSpanTicks),
            CleanoutGrade,
            QtyPerCycle,
            PlanningScrapPercent,
            TransferQty,
            ProductionSetupCost,
            CleanoutCost);
    }

    /// <summary>
    /// Copy all the production values of this instance into another.
    /// Return count of the number of value changes.
    /// </summary>
    /// <param name="a_copyTo">These values are copied into this instance</param>
    /// <param name="a_ignoreManualUpdatesSettings">If this is true, all the values are copied regardless of the manual updates settings. </param>
    /// <param name="a_scheduled"></param>
    /// <param name="a_dataChanges"></param>
    internal bool SyncProductionMembersOfThisInstanceInto(ProductionInfo a_copyTo, bool a_ignoreManualUpdatesSettings, bool a_scheduled, IScenarioDataChanges a_dataChanges)
    {
        bool updated = false;
        bool flagContrainingChanges = false;
        bool flagProductionChanges = false;
        if (a_copyTo.m_cycleSpanTicks != m_cycleSpanTicks)
        {
            if (a_ignoreManualUpdatesSettings || !a_copyTo.OnlyAllowManualUpdatesToCycleSpan)
            {
                a_copyTo.m_cycleSpanTicks = m_cycleSpanTicks;
                updated = true;
                flagProductionChanges = true;
            }
        }

        if (a_copyTo.m_planningScrapPercent != m_planningScrapPercent)
        {
            if (a_ignoreManualUpdatesSettings || !a_copyTo.OnlyAllowManualUpdatesToPlanningScrapPercent)
            {
                a_copyTo.m_planningScrapPercent = m_planningScrapPercent;
                updated = true;
                flagProductionChanges = true;
            }
        }

        if (a_copyTo.m_postProcessingSpanTicks != m_postProcessingSpanTicks)
        {
            if (a_ignoreManualUpdatesSettings || !a_copyTo.OnlyAllowManualUpdatesToPostProcessingSpan)
            {
                a_copyTo.m_postProcessingSpanTicks = m_postProcessingSpanTicks;
                updated = true;
                flagProductionChanges = true;
            }
        }

        if (a_copyTo.m_cleanSpanTicks != m_cleanSpanTicks)
        {
            if (a_ignoreManualUpdatesSettings || !a_copyTo.OnlyAllowManualUpdatesToCleanSpan)
            {
                a_copyTo.m_cleanSpanTicks = m_cleanSpanTicks;
                updated = true;
                flagProductionChanges = true;
            }
        }

        if (a_copyTo.m_cleanoutGrade != m_cleanoutGrade)
        {
            if (a_ignoreManualUpdatesSettings || !a_copyTo.OnlyAllowManualUpdatesToCleanSpan)
            {
                a_copyTo.m_cleanoutGrade = m_cleanoutGrade;
                updated = true;
                flagProductionChanges = true;
            }
        }

        if (a_copyTo.m_storageSpanTicks != m_storageSpanTicks)
        {
            if (a_ignoreManualUpdatesSettings || !a_copyTo.OnlyAllowManualUpdatesToStorageSpan)
            {
                a_copyTo.m_storageSpanTicks = m_storageSpanTicks;
                updated = true;
                flagProductionChanges = true;
            }
        }

        if (a_copyTo.m_qtyPerCycle != m_qtyPerCycle)
        {
            if (a_ignoreManualUpdatesSettings || !a_copyTo.OnlyAllowManualUpdatesToQtyPerCycle)
            {
                a_copyTo.m_qtyPerCycle = m_qtyPerCycle;
                updated = true;
                flagProductionChanges = true;
                a_dataChanges.FlagEligibilityChanges(BaseId.NULL_ID);
            }
        }

        if (a_copyTo.m_setupSpanTicks != m_setupSpanTicks)
        {
            if (a_ignoreManualUpdatesSettings || !a_copyTo.OnlyAllowManualUpdatesToSetupSpan)
            {
                a_copyTo.m_setupSpanTicks = m_setupSpanTicks;
                updated = true;
                flagProductionChanges = true;
            }
        }

        if (a_copyTo.m_materialPostProcessingSpanTicks != m_materialPostProcessingSpanTicks)
        {
            if (a_ignoreManualUpdatesSettings || !a_copyTo.OnlyAllowManualUpdatesToMaterialPostProcessingSpan)
            {
                if (a_copyTo.MaterialPostProcessingSpanTicks < m_materialPostProcessingSpanTicks)
                {
                    //Product will be available later, constraint
                    flagContrainingChanges = true;
                }
                else
                {
                    flagProductionChanges = true;
                }

                a_copyTo.m_materialPostProcessingSpanTicks = m_materialPostProcessingSpanTicks;
                updated = true;
            }
        }

        if (a_copyTo.m_transferQty != m_transferQty)
        {
            if (a_ignoreManualUpdatesSettings || !a_copyTo.OnlyAllowManualUpdatesToTransferQty)
            {
                a_copyTo.m_transferQty = m_transferQty;
                updated = true;
                flagContrainingChanges = true;
            }
        }

        if (a_copyTo.m_productionSetupCost != m_productionSetupCost)
        {
            if (a_ignoreManualUpdatesSettings || !a_copyTo.OnlyAllowManualUpdatesToProductionSetupCost)
            {
                a_copyTo.m_productionSetupCost = m_productionSetupCost;
                updated = true;
            }
        }

        if (a_copyTo.m_cleanoutCost != m_cleanoutCost)
        {
            if (a_ignoreManualUpdatesSettings || !a_copyTo.OnlyAllowManualUpdatesToCleanoutCost)
            {
                a_copyTo.m_cleanoutCost = m_cleanoutCost;
                updated = true;
            }
        }

        if (a_copyTo.CleanSpanOverride != CleanSpanOverride)
        {
            a_copyTo.CleanSpanOverride = CleanSpanOverride;
            updated = true;
            flagProductionChanges = true;
        }
        
        if (a_copyTo.SetupSpanOverride != SetupSpanOverride)
        {
            a_copyTo.SetupSpanOverride = SetupSpanOverride;
            updated = true;
            flagProductionChanges = true;
        }

        if (a_copyTo.CycleSpanOverride != CycleSpanOverride)
        {
            a_copyTo.CycleSpanOverride = CycleSpanOverride;
            updated = true;
            flagProductionChanges = true;
        }

        if (a_copyTo.MaterialPostProcessingSpanOverride != MaterialPostProcessingSpanOverride)
        {
            a_copyTo.MaterialPostProcessingSpanOverride = MaterialPostProcessingSpanOverride;
            updated = true;
            flagContrainingChanges = true;
        }

        if (a_copyTo.PlanningScrapPercentOverride != PlanningScrapPercentOverride)
        {
            a_copyTo.PlanningScrapPercentOverride = PlanningScrapPercentOverride;
            updated = true;
        }

        if (a_copyTo.PostProcessingSpanOverride != PostProcessingSpanOverride)
        {
            a_copyTo.PostProcessingSpanOverride = PostProcessingSpanOverride;
            updated = true;
            flagProductionChanges = true;
        }

        if (a_copyTo.QtyPerCycleOverride != QtyPerCycleOverride)
        {
            a_copyTo.QtyPerCycleOverride = QtyPerCycleOverride;
            updated = true;
            flagProductionChanges = true;
            a_dataChanges.FlagEligibilityChanges(BaseId.NULL_ID);
        }

        if (a_copyTo.TransferQtyOverride != TransferQtyOverride)
        {
            a_copyTo.TransferQtyOverride = TransferQtyOverride;
            updated = true;
            flagContrainingChanges = true;
        }

        if (a_copyTo.SplitOperationOverride != SplitOperationOverride)
        {
            a_copyTo.SplitOperationOverride = SplitOperationOverride;
            updated = true;
            flagProductionChanges = true;
        }

        if (a_copyTo.ProductionSetupCostOverride != ProductionSetupCostOverride)
        {
            a_copyTo.ProductionSetupCostOverride = ProductionSetupCostOverride;
            updated = true;
        }

        if (a_copyTo.CleanoutCostOverride != CleanoutCostOverride)
        {
            a_copyTo.CleanoutCostOverride = CleanoutCostOverride;
            updated = true;
        }
        
        if (a_copyTo.StorageSpanOverride != StorageSpanOverride)
        {
            a_copyTo.StorageSpanOverride = StorageSpanOverride;
            updated = true;
            flagProductionChanges = true;
        }

        if (a_scheduled)
        {
            if (flagProductionChanges)
            {
                a_dataChanges.FlagProductionChanges(BaseId.NULL_ID);
            }

            if (flagContrainingChanges)
            {
                a_dataChanges.FlagConstraintChanges(BaseId.NULL_ID);
            }
        }

        return updated;
    }

    private decimal m_planningScrapPercent;

    public decimal PlanningScrapPercent
    {
        get => m_planningScrapPercent;
        internal set
        {
            if (value >= 0 && value < 1)
            {
                m_planningScrapPercent = value;
            }
            else
            {
                throw new APSCommon.PTValidationException("2239", new object[] { value });
            }
        }
    }

    private long m_setupSpanTicks;

    public long SetupSpanTicks
    {
        get => m_setupSpanTicks;
        internal set => m_setupSpanTicks = value;
    }

    private long m_cycleSpanTicks;

    public long CycleSpanTicks
    {
        get => m_cycleSpanTicks;
        internal set => m_cycleSpanTicks = value;
    }

    private long m_postProcessingSpanTicks;

    public long PostProcessingSpanTicks
    {
        get => m_postProcessingSpanTicks;
        internal set => m_postProcessingSpanTicks = value;
    }

    private long m_cleanSpanTicks;

    public long CleanSpanTicks
    {
        get => m_cleanSpanTicks;
        internal set => m_cleanSpanTicks = value;
    }

    private int m_cleanoutGrade;

    public int CleanoutGrade
    {
        get => m_cleanoutGrade;
        internal set => m_cleanoutGrade = value;
    }

    private decimal m_qtyPerCycle;

    public decimal QtyPerCycle
    {
        get => m_qtyPerCycle;
        internal set => m_qtyPerCycle = value;
    }

    private long m_materialPostProcessingSpanTicks;

    public long MaterialPostProcessingSpanTicks
    {
        get => m_materialPostProcessingSpanTicks;
        internal set => m_materialPostProcessingSpanTicks = value;
    }

    private decimal m_transferQty;

    public decimal TransferQty
    {
        get => m_transferQty;
        internal set => m_transferQty = value;
    }

    private decimal m_productionSetupCost;

    public decimal ProductionSetupCost
    {
        get => m_productionSetupCost;
        internal set => m_productionSetupCost = value;
    }

    private decimal m_cleanoutCost;

    public decimal CleanoutCost
    {
        get => m_cleanoutCost;
        internal set => m_cleanoutCost = value;
    }

    private long m_storageSpanTicks;

    public long StorageSpanTicks
    {
        get => m_storageSpanTicks;
        internal set => m_storageSpanTicks = value;
    }
    #region flags
    // The flags in this region are used to indicate whether updates to the production values
    // can only be made through the user interface.
    // Updates from the ERP system are ignored.
    protected BoolVector32 m_flags;

    private const int c_onlyAllowManualUpdatesToPlanningScrapPercentIdx = 0;
    private const int c_onlyAllowManualUpdatesToSetupSpanIdx = 1;
    private const int c_onlyAllowManualUpdatesToCycleSpanIdx = 2;
    private const int c_onlyAllowManualUpdatesToPostProcessingSpanIdx = 3;
    private const int c_onlyAllowManualUpdatesToQtyPerCycleIdx = 4;
    private const int c_onlyAllowManualUpdatesToResReqtsIdx = 5;
    private const int c_onlyAllowManualUpdatesToMaterialPostProcessingSpanIdx = 6;
    private const int c_onlyAllowManualUpdatesToSplitOperationIdx = 7;
    private const int c_onlyAllowManualUpdatesToCleanSpanIdx = 8;
    private const int c_onlyAllowManualUpdatesToTransferQtyIdx = 9;
    private const int c_planningScrapPercentOverrideIdx = 10;
    private const int c_setupSpanOverrideIdx = 11;
    private const int c_cycleSpanOverrideIdx = 12;
    private const int c_postProcessingSpanOverrideIdx = 13;
    private const int c_qtyPerCycleOverrideIdx = 14;
    private const int c_resReqtsOverrideIdx = 15;
    private const int c_materialPostProcessingSpanOverrideIdx = 16;
    private const int c_splitOperationOverrideIdx = 17;
    private const int c_cleanSpanOverrideIdx = 18;
    private const int c_transferQtyOverrideIdx = 19;

    private const int c_onlyAllowManualUpdatesToProductionSetupCostIdx = 20;
    private const int c_onlyAllowManualUpdatesToCleanoutCostIdx = 21;

    private const int c_productionSetupCostOverrideIdx = 22;
    private const int c_cleanoutCostOverrideIdx = 23;
    private const int c_onlyAllowManualUpdatesToStorageSpanIdx = 24;
    private const int c_storageSpanOverrideIdx = 25;

    #region ManualUpdate Flags
    public bool OnlyAllowManualUpdatesToPlanningScrapPercent
    {
        get => m_flags[c_onlyAllowManualUpdatesToPlanningScrapPercentIdx];
        internal set => m_flags[c_onlyAllowManualUpdatesToPlanningScrapPercentIdx] = value;
    }

    public bool OnlyAllowManualUpdatesToSetupSpan
    {
        get => m_flags[c_onlyAllowManualUpdatesToSetupSpanIdx];
        internal set => m_flags[c_onlyAllowManualUpdatesToSetupSpanIdx] = value;
    }

    public bool OnlyAllowManualUpdatesToCycleSpan
    {
        get => m_flags[c_onlyAllowManualUpdatesToCycleSpanIdx];
        internal set => m_flags[c_onlyAllowManualUpdatesToCycleSpanIdx] = value;
    }

    public bool OnlyAllowManualUpdatesToPostProcessingSpan
    {
        get => m_flags[c_onlyAllowManualUpdatesToPostProcessingSpanIdx];
        internal set => m_flags[c_onlyAllowManualUpdatesToPostProcessingSpanIdx] = value;
    }

    public bool OnlyAllowManualUpdatesToMaterialPostProcessingSpan
    {
        get => m_flags[c_onlyAllowManualUpdatesToMaterialPostProcessingSpanIdx];
        internal set => m_flags[c_onlyAllowManualUpdatesToMaterialPostProcessingSpanIdx] = value;
    }

    public bool OnlyAllowManualUpdatesToQtyPerCycle
    {
        get => m_flags[c_onlyAllowManualUpdatesToQtyPerCycleIdx];
        internal set => m_flags[c_onlyAllowManualUpdatesToQtyPerCycleIdx] = value;
    }

    public bool OnlyAllowManualUpdatesToResourceRequirements
    {
        get => m_flags[c_onlyAllowManualUpdatesToResReqtsIdx];
        internal set => m_flags[c_onlyAllowManualUpdatesToResReqtsIdx] = value;
    }

    public bool OnlyAllowManualUpdatesToSplitOperation
    {
        get => m_flags[c_onlyAllowManualUpdatesToSplitOperationIdx];
        internal set => m_flags[c_onlyAllowManualUpdatesToSplitOperationIdx] = value;
    }

    public bool OnlyAllowManualUpdatesToCleanSpan
    {
        get => m_flags[c_onlyAllowManualUpdatesToCleanSpanIdx];
        internal set => m_flags[c_onlyAllowManualUpdatesToCleanSpanIdx] = value;
    }

    public bool OnlyAllowManualUpdatesToStorageSpan
    {
        get => m_flags[c_onlyAllowManualUpdatesToStorageSpanIdx];
        internal set => m_flags[c_onlyAllowManualUpdatesToStorageSpanIdx] = value;
    }

    public bool OnlyAllowManualUpdatesToTransferQty
    {
        get => m_flags[c_onlyAllowManualUpdatesToTransferQtyIdx];
        internal set => m_flags[c_onlyAllowManualUpdatesToTransferQtyIdx] = value;
    }

    public bool OnlyAllowManualUpdatesToProductionSetupCost
    {
        get => m_flags[c_onlyAllowManualUpdatesToProductionSetupCostIdx];
        internal set => m_flags[c_onlyAllowManualUpdatesToProductionSetupCostIdx] = value;
    }

    public bool OnlyAllowManualUpdatesToCleanoutCost
    {
        get => m_flags[c_onlyAllowManualUpdatesToCleanoutCostIdx];
        internal set => m_flags[c_onlyAllowManualUpdatesToCleanoutCostIdx] = value;
    }
    #endregion
    #region Override Flags
    public bool PlanningScrapPercentOverride
    {
        get => m_flags[c_planningScrapPercentOverrideIdx];
        internal set => m_flags[c_planningScrapPercentOverrideIdx] = value;
    }

    public bool SetupSpanOverride
    {
        get => m_flags[c_setupSpanOverrideIdx];
        internal set => m_flags[c_setupSpanOverrideIdx] = value;
    }

    public bool CycleSpanOverride
    {
        get => m_flags[c_cycleSpanOverrideIdx];
        internal set => m_flags[c_cycleSpanOverrideIdx] = value;
    }

    public bool PostProcessingSpanOverride
    {
        get => m_flags[c_postProcessingSpanOverrideIdx];
        internal set => m_flags[c_postProcessingSpanOverrideIdx] = value;
    }

    public bool MaterialPostProcessingSpanOverride
    {
        get => m_flags[c_materialPostProcessingSpanOverrideIdx];
        internal set => m_flags[c_materialPostProcessingSpanOverrideIdx] = value;
    }

    public bool QtyPerCycleOverride
    {
        get => m_flags[c_qtyPerCycleOverrideIdx];
        internal set => m_flags[c_qtyPerCycleOverrideIdx] = value;
    }

    public bool ResourceRequirementsOverride
    {
        get => m_flags[c_resReqtsOverrideIdx];
        internal set => m_flags[c_resReqtsOverrideIdx] = value;
    }

    public bool SplitOperationOverride
    {
        get => m_flags[c_splitOperationOverrideIdx];
        internal set => m_flags[c_splitOperationOverrideIdx] = value;
    }

    public bool CleanSpanOverride
    {
        get => m_flags[c_cleanSpanOverrideIdx];
        internal set => m_flags[c_cleanSpanOverrideIdx] = value;
    }

    public bool TransferQtyOverride
    {
        get => m_flags[c_transferQtyOverrideIdx];
        internal set => m_flags[c_transferQtyOverrideIdx] = value;
    }

    public bool ProductionSetupCostOverride
    {
        get => m_flags[c_productionSetupCostOverrideIdx];
        internal set => m_flags[c_productionSetupCostOverrideIdx] = value;
    }

    public bool CleanoutCostOverride
    {
        get => m_flags[c_cleanoutCostOverrideIdx];
        internal set => m_flags[c_cleanoutCostOverrideIdx] = value;
    }

    public bool StorageSpanOverride
    {
        get => m_flags[c_storageSpanOverrideIdx];
        internal set => m_flags[c_storageSpanOverrideIdx] = value;
    }
    #endregion
    #endregion

    /// <summary>
    /// Determines whether two objects are greater than (1), less than (-1), or equal to (0) each other.
    /// </summary>
    /// <param name="a_prodInfo">Another object of this type.</param>
    /// <returns>-1, 0, or -1 depending on how the objects compare.</returns>
    internal int SimilarityComparison(ProductionInfo a_prodInfo)
    {
        int v;

        if ((v = PlanningScrapPercent.CompareTo(a_prodInfo.PlanningScrapPercent)) != 0)
        {
            return v;
        }

        if ((v = SetupSpanTicks.CompareTo(a_prodInfo.SetupSpanTicks)) != 0)
        {
            return v;
        }

        if ((v = CycleSpanTicks.CompareTo(a_prodInfo.CycleSpanTicks)) != 0)
        {
            return v;
        }

        if ((v = PostProcessingSpanTicks.CompareTo(a_prodInfo.PostProcessingSpanTicks)) != 0)
        {
            return v;
        }

        if ((v = CleanSpanTicks.CompareTo(a_prodInfo.CleanSpanTicks)) != 0)
        {
            return v;
        }

        if ((v = CleanoutGrade.CompareTo(a_prodInfo.CleanoutGrade)) != 0)
        {
            return v;
        }

        if ((v = QtyPerCycle.CompareTo(a_prodInfo.QtyPerCycle)) != 0)
        {
            return v;
        }

        if ((v = TransferQty.CompareTo(a_prodInfo.TransferQty)) != 0)
        {
            return v;
        }

        if ((v = ProductionSetupCostOverride.CompareTo(a_prodInfo.ProductionSetupCostOverride)) != 0)
        {
            return v;
        }

        if ((v = CleanoutCostOverride.CompareTo(a_prodInfo.CleanoutCostOverride)) != 0)
        {
            return v;
        }

        if ((v = OnlyAllowManualUpdatesToCycleSpan.CompareTo(a_prodInfo.OnlyAllowManualUpdatesToCycleSpan)) != 0)
        {
            return v;
        }

        if ((v = OnlyAllowManualUpdatesToPlanningScrapPercent.CompareTo(a_prodInfo.OnlyAllowManualUpdatesToPlanningScrapPercent)) != 0)
        {
            return v;
        }

        if ((v = OnlyAllowManualUpdatesToPostProcessingSpan.CompareTo(a_prodInfo.OnlyAllowManualUpdatesToPostProcessingSpan)) != 0)
        {
            return v;
        }

        if ((v = OnlyAllowManualUpdatesToQtyPerCycle.CompareTo(a_prodInfo.OnlyAllowManualUpdatesToQtyPerCycle)) != 0)
        {
            return v;
        }

        if ((v = OnlyAllowManualUpdatesToResourceRequirements.CompareTo(a_prodInfo.OnlyAllowManualUpdatesToResourceRequirements)) != 0)
        {
            return v;
        }

        if ((v = OnlyAllowManualUpdatesToSetupSpan.CompareTo(a_prodInfo.OnlyAllowManualUpdatesToSetupSpan)) != 0)
        {
            return v;
        }

        if ((v = MaterialPostProcessingSpanTicks.CompareTo(a_prodInfo.MaterialPostProcessingSpanTicks)) != 0)
        {
            return v;
        }

        if ((v = OnlyAllowManualUpdatesToMaterialPostProcessingSpan.CompareTo(a_prodInfo.OnlyAllowManualUpdatesToMaterialPostProcessingSpan)) != 0)
        {
            return v;
        }

        if ((v = OnlyAllowManualUpdatesToSplitOperation.CompareTo(a_prodInfo.OnlyAllowManualUpdatesToSplitOperation)) != 0)
        {
            return v;
        }

        if ((v = OnlyAllowManualUpdatesToTransferQty.CompareTo(a_prodInfo.OnlyAllowManualUpdatesToTransferQty)) != 0)
        {
            return v;
        }

        if ((v = OnlyAllowManualUpdatesToProductionSetupCost.CompareTo(a_prodInfo.OnlyAllowManualUpdatesToProductionSetupCost)) != 0)
        {
            return v;
        }

        if ((v = OnlyAllowManualUpdatesToCleanoutCost.CompareTo(a_prodInfo.OnlyAllowManualUpdatesToCleanoutCost)) != 0)
        {
            return v;
        }

        if ((v = StorageSpanTicks.CompareTo(a_prodInfo.StorageSpanTicks)) != 0)
        {
            return v;
        }

        if ((v = OnlyAllowManualUpdatesToStorageSpan.CompareTo(a_prodInfo.OnlyAllowManualUpdatesToStorageSpan)) != 0)
        {
            return v;
        }

        return 0;
    }

    /// <summary>
    /// Create a string that describes the differences between the fields of two objects.
    /// </summary>
    /// <param name="a_prodInfo"></param>
    /// <param name="a_differenceTypes"></param>
    /// <param name="a_differences">The descriptions of the differences are added to this field.</param>
    internal void DetermineDifferences(ProductionInfo a_prodInfo, int a_differenceTypes, StringBuilder a_differences)
    {
        if ((a_differenceTypes & ManufacturingOrder.DifferenceTypes.timing) > 0 || (a_differenceTypes & ManufacturingOrder.DifferenceTypes.any) > 0)
        {
            DifferencesStatics.Differences("PlanningScrapPercent", PlanningScrapPercent, a_prodInfo.PlanningScrapPercent, a_differences);
            DifferencesStatics.TimespanDifferences("SetupSpanTicks", SetupSpanTicks, a_prodInfo.SetupSpanTicks, a_differences);
            DifferencesStatics.TimespanDifferences("CycleSpanTicks", CycleSpanTicks, a_prodInfo.CycleSpanTicks, a_differences);
            DifferencesStatics.TimespanDifferences("PostProcessingSpanTicks", PostProcessingSpanTicks, a_prodInfo.PostProcessingSpanTicks, a_differences);
            DifferencesStatics.TimespanDifferences("CleanSpanTicks", CleanSpanTicks, a_prodInfo.CleanSpanTicks, a_differences);
            DifferencesStatics.TimespanDifferences("CleanOutGrade", CleanoutGrade, a_prodInfo.CleanoutGrade, a_differences);
            DifferencesStatics.Differences("CleanSpanTicks", CleanoutGrade, a_prodInfo.CleanoutGrade, a_differences);
            DifferencesStatics.Differences("QtyPerCycle", QtyPerCycle, a_prodInfo.QtyPerCycle, a_differences);
            DifferencesStatics.Differences("TransferQty", TransferQty, a_prodInfo.TransferQty, a_differences);
            DifferencesStatics.Differences("ProductionSetupCost", ProductionSetupCost, a_prodInfo.ProductionSetupCost, a_differences);
            DifferencesStatics.Differences("CleanoutCost", CleanoutCost, a_prodInfo.CleanoutCost, a_differences);
            DifferencesStatics.TimespanDifferences("MaterialPostProcessingSpanTicks", MaterialPostProcessingSpanTicks, a_prodInfo.MaterialPostProcessingSpanTicks, a_differences);
            DifferencesStatics.TimespanDifferences("StorageSpanTicks", StorageSpanTicks, a_prodInfo.StorageSpanTicks, a_differences);
        }

        if ((a_differenceTypes & ManufacturingOrder.DifferenceTypes.any) > 0)
        {
            DifferencesStatics.Differences("CycleSpan", OnlyAllowManualUpdatesToCycleSpan, a_prodInfo.OnlyAllowManualUpdatesToCycleSpan, a_differences);
            DifferencesStatics.Differences("PlanningScrapPercent", OnlyAllowManualUpdatesToPlanningScrapPercent, a_prodInfo.OnlyAllowManualUpdatesToPlanningScrapPercent, a_differences);
            DifferencesStatics.Differences("PostProcessingSpan", OnlyAllowManualUpdatesToPostProcessingSpan, a_prodInfo.OnlyAllowManualUpdatesToPostProcessingSpan, a_differences);
            DifferencesStatics.Differences("CleanSpan", OnlyAllowManualUpdatesToCleanSpan, a_prodInfo.OnlyAllowManualUpdatesToCleanSpan, a_differences);
            DifferencesStatics.Differences("CleanOutGrade", OnlyAllowManualUpdatesToCleanSpan, a_prodInfo.OnlyAllowManualUpdatesToCleanSpan, a_differences);
            DifferencesStatics.Differences("QtyPerCycle", OnlyAllowManualUpdatesToQtyPerCycle, a_prodInfo.OnlyAllowManualUpdatesToQtyPerCycle, a_differences);
            DifferencesStatics.Differences("TransferQty", OnlyAllowManualUpdatesToTransferQty, a_prodInfo.OnlyAllowManualUpdatesToTransferQty, a_differences);
            DifferencesStatics.Differences("ProductionSetupCost", OnlyAllowManualUpdatesToProductionSetupCost, a_prodInfo.OnlyAllowManualUpdatesToProductionSetupCost, a_differences);
            DifferencesStatics.Differences("CleanoutCost", OnlyAllowManualUpdatesToCleanoutCost, a_prodInfo.OnlyAllowManualUpdatesToCleanoutCost, a_differences);
            DifferencesStatics.Differences("ResourceRequirements", OnlyAllowManualUpdatesToResourceRequirements, a_prodInfo.OnlyAllowManualUpdatesToResourceRequirements, a_differences);
            DifferencesStatics.Differences("SetupSpan", OnlyAllowManualUpdatesToSetupSpan, a_prodInfo.OnlyAllowManualUpdatesToSetupSpan, a_differences);
            DifferencesStatics.Differences("MaterialPostProcessingSpan", OnlyAllowManualUpdatesToMaterialPostProcessingSpan, a_prodInfo.OnlyAllowManualUpdatesToMaterialPostProcessingSpan, a_differences);
            DifferencesStatics.Differences("StorageSpan", OnlyAllowManualUpdatesToStorageSpan, a_prodInfo.OnlyAllowManualUpdatesToStorageSpan, a_differences);
        }
    }

    public ProductionInfo Clone()
    {
        ProductionInfo newInfo = new();
        newInfo.m_materialPostProcessingSpanTicks = m_materialPostProcessingSpanTicks;
        newInfo.m_cycleSpanTicks = m_cycleSpanTicks;
        newInfo.m_planningScrapPercent = m_planningScrapPercent;
        newInfo.m_postProcessingSpanTicks = m_postProcessingSpanTicks;
        newInfo.m_cleanSpanTicks = m_cleanSpanTicks;
        newInfo.m_cleanoutGrade = m_cleanoutGrade;
        newInfo.m_qtyPerCycle = m_qtyPerCycle;
        newInfo.m_setupSpanTicks = m_setupSpanTicks;
        newInfo.m_transferQty = m_transferQty;
        newInfo.m_productionSetupCost = m_productionSetupCost;
        newInfo.m_cleanoutCost = m_cleanoutCost;
        newInfo.m_storageSpanTicks = m_storageSpanTicks;

        newInfo.m_flags = new BoolVector32(m_flags);
        return newInfo;
    }

    /// <summary>
    /// Uses this production info as a base, and changes all values that are in use in the specified production info
    /// </summary>
    /// <param name="a_productionInfoOverride">The values to use as overrides if they are in use</param>
    /// <returns></returns>
    public ProductionInfo OverrideProductionInfo(ProductionInfo a_productionInfoOverride)
    {
        ProductionInfo clone = Clone();

        clone.SetupSpanOverride = a_productionInfoOverride.SetupSpanOverride;
        if (a_productionInfoOverride.SetupSpanOverride)
        {
            clone.SetupSpanTicks = a_productionInfoOverride.SetupSpanTicks;
        }

        clone.CycleSpanOverride = a_productionInfoOverride.CycleSpanOverride;
        if (a_productionInfoOverride.CycleSpanOverride)
        {
            clone.CycleSpanTicks = a_productionInfoOverride.CycleSpanTicks;
        }

        clone.PostProcessingSpanOverride = a_productionInfoOverride.PostProcessingSpanOverride;
        if (a_productionInfoOverride.PostProcessingSpanOverride)
        {
            clone.PostProcessingSpanTicks = a_productionInfoOverride.PostProcessingSpanTicks;
        }

        clone.CleanSpanOverride = a_productionInfoOverride.CleanSpanOverride;
        if (a_productionInfoOverride.CleanSpanOverride)
        {
            clone.CleanSpanTicks = a_productionInfoOverride.CleanSpanTicks;
        }

        clone.MaterialPostProcessingSpanOverride = a_productionInfoOverride.MaterialPostProcessingSpanOverride;
        if (a_productionInfoOverride.MaterialPostProcessingSpanOverride)
        {
            clone.MaterialPostProcessingSpanTicks = a_productionInfoOverride.MaterialPostProcessingSpanTicks;
        }

        clone.QtyPerCycleOverride = a_productionInfoOverride.QtyPerCycleOverride;
        if (a_productionInfoOverride.QtyPerCycleOverride)
        {
            clone.QtyPerCycle = a_productionInfoOverride.QtyPerCycle;
        }

        clone.PlanningScrapPercentOverride = a_productionInfoOverride.PlanningScrapPercentOverride;
        if (a_productionInfoOverride.PlanningScrapPercentOverride)
        {
            clone.PlanningScrapPercent = a_productionInfoOverride.PlanningScrapPercent;
        }

        clone.TransferQtyOverride = a_productionInfoOverride.TransferQtyOverride;
        if (a_productionInfoOverride.TransferQtyOverride)
        {
            clone.TransferQty = a_productionInfoOverride.TransferQty;
        }
        if (a_productionInfoOverride.ProductionSetupCostOverride)
        {
            clone.ProductionSetupCost = a_productionInfoOverride.ProductionSetupCost;
        }
        if (a_productionInfoOverride.CleanoutCostOverride)
        {
            clone.CleanoutCost = a_productionInfoOverride.CleanoutCost;
        }
        if (a_productionInfoOverride.StorageSpanOverride)
        {
            clone.StorageSpanTicks = a_productionInfoOverride.StorageSpanTicks;
        }
        
        return clone;
    }
}