using System.Drawing;

using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Transmission for creating a CapacityInterval
/// </summary>
public class CapacityIntervalNewT : CapacityIntervalBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 465;

    #region IPTSerializable Members
    public CapacityIntervalNewT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 12422)
        {
            m_bools = new BoolVector32(reader);
            int val;
            reader.Read(out val);
            intervalType = (CapacityIntervalDefs.capacityIntervalTypes)val;
            reader.Read(out val);
            scope = (CapacityIntervalDefs.capacityIntervalAdditionScopes)val;

            reader.Read(out intervalStart);
            reader.Read(out duration);

            plantId = new BaseId(reader);
            departmentId = new BaseId(reader);
            resourceId = new BaseId(reader);
            reader.Read(out color);
        }
        else if (reader.VersionNumber >= 12303)
        {
            m_bools = new BoolVector32(reader);
            int val;
            reader.Read(out val);
            intervalType = (CapacityIntervalDefs.capacityIntervalTypes)val;
            reader.Read(out val);
            scope = (CapacityIntervalDefs.capacityIntervalAdditionScopes)val;

            reader.Read(out intervalStart);
            reader.Read(out duration);

            plantId = new BaseId(reader);
            departmentId = new BaseId(reader);
            resourceId = new BaseId(reader);
        }
        else if (reader.VersionNumber >= 1)
        {
            int val;
            reader.Read(out val);
            intervalType = (CapacityIntervalDefs.capacityIntervalTypes)val;
            reader.Read(out val);
            scope = (CapacityIntervalDefs.capacityIntervalAdditionScopes)val;

            reader.Read(out intervalStart);
            reader.Read(out duration);

            plantId = new BaseId(reader);
            departmentId = new BaseId(reader);
            resourceId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_bools.Serialize(writer);
        writer.Write((int)intervalType);
        writer.Write((int)scope);

        writer.Write(intervalStart);
        writer.Write(duration);

        plantId.Serialize(writer);
        departmentId.Serialize(writer);
        resourceId.Serialize(writer);
        writer.Write(color);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private BoolVector32 m_bools;

    private const short c_preventSpanningIdx = 0;
    private const short c_clearChangeoversIdx = 1;
    private const short c_usedForSetupIdx = 2;
    private const short c_usedForRunIdx = 3;
    private const short c_usedForPostProcessingIdx = 4;
    private const short c_usedForCleanIdx = 5;
    private const short c_usedForStoragePostProcessingIdx = 6;
    private const short c_overtimeIdx = 7;
    private const short c_canStartActivityIdx = 8;
    private const short c_useOnlyWhenLateIdx = 9;
    private const short c_canDragAndResizeIdx = 10;
    private const short c_canDeleteIdx = 11;

    public bool PreventSpanning
    {
        get => m_bools[c_preventSpanningIdx];
        private set => m_bools[c_preventSpanningIdx] = value;
    }

    public bool ClearChangeovers
    {
        get => m_bools[c_clearChangeoversIdx];
        private set => m_bools[c_clearChangeoversIdx] = value;
    }
    public bool UsedForSetup
    {
        get => m_bools[c_usedForSetupIdx];
        private set => m_bools[c_usedForSetupIdx] = value;
    }
    public bool UsedForRun
    {
        get => m_bools[c_usedForRunIdx];
        private set => m_bools[c_usedForRunIdx] = value;
    }
    public bool UsedForPostProcessing
    {
        get => m_bools[c_usedForPostProcessingIdx];
        private set => m_bools[c_usedForPostProcessingIdx] = value;
    }
    public bool UsedForClean
    {
        get => m_bools[c_usedForCleanIdx];
        private set => m_bools[c_usedForCleanIdx] = value;
    }
    public bool UsedForStoragePostProcessing
    {
        get => m_bools[c_usedForStoragePostProcessingIdx];
        private set => m_bools[c_usedForStoragePostProcessingIdx] = value;
    }

    public bool Overtime
    {
        get => m_bools[c_overtimeIdx];
        private set => m_bools[c_overtimeIdx] = value;
    }

    public bool CanStartActivity
    {
        get => m_bools[c_canStartActivityIdx];
        private set => m_bools[c_canStartActivityIdx] = value;
    }

    public bool UseOnlyWhenLate
    {
        get => m_bools[c_useOnlyWhenLateIdx];
        private set => m_bools[c_useOnlyWhenLateIdx] = value;
    }

    public bool CanDragAndResize
    {
        get => m_bools[c_canDragAndResizeIdx];

        private set
        {
            m_bools[c_canDragAndResizeIdx] = value;
        }
    }

    public bool CanDelete
    {
        get => m_bools[c_canDeleteIdx];

        private set
        {
            m_bools[c_canDeleteIdx] = value;
        }
    }

    public BaseId plantId;
    public BaseId departmentId;
    public BaseId resourceId;
    public CapacityIntervalDefs.capacityIntervalTypes intervalType;
    public CapacityIntervalDefs.capacityIntervalAdditionScopes scope;
    public DateTime intervalStart;
    public TimeSpan duration;
    public readonly Color color;

    public CapacityIntervalNewT() { }

    public CapacityIntervalNewT(BaseId scenarioId, 
                                CapacityIntervalDefs.capacityIntervalTypes intervalType, 
                                BaseId plantId, 
                                BaseId departmentId, 
                                BaseId resourceId, 
                                DateTime intervalStart, 
                                TimeSpan duration, 
                                CapacityIntervalDefs.capacityIntervalAdditionScopes scope,
                                bool preventSpanning, 
                                bool clearChangeovers,
                                bool a_canStartActivity,
                                bool a_usedForSetup,
                                bool a_usedForRun,
                                bool a_usedForPostProcessing,
                                bool a_usedForClean,
                                bool a_usedForStoragePostProcessing,
                                bool a_overtime,
                                bool a_useOnlyWhenLate,
                                Color a_intervalColor,
                                bool a_canBeDragged,
                                bool a_canBeDeleted
                                )
        : base(scenarioId)
    {
        this.plantId = plantId;
        this.departmentId = departmentId;
        this.resourceId = resourceId;
        this.intervalStart = intervalStart;
        this.intervalType = intervalType;
        this.duration = duration;
        this.scope = scope;
        ClearChangeovers = clearChangeovers;
        PreventSpanning = preventSpanning;
        CanStartActivity = a_canStartActivity;
        UsedForSetup = a_usedForSetup;
        UsedForRun = a_usedForRun;
        UsedForPostProcessing = a_usedForPostProcessing;
        UsedForClean = a_usedForClean;
        UsedForStoragePostProcessing = a_usedForStoragePostProcessing;
        Overtime = a_overtime;
        UseOnlyWhenLate = a_useOnlyWhenLate;
        color = a_intervalColor;
        CanDragAndResize = a_canBeDragged;
        CanDelete = a_canBeDeleted;
    }

    public override string Description => "Default capacity added";
}