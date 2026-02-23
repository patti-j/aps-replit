using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Transmission for Locking or Unlocking a list of Jobs.
/// </summary>
public class ScenarioDetailLockJobsT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 129;

    #region IPTSerializable Members
    public ScenarioDetailLockJobsT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out lockit);

            m_jobs = new BaseIdList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(lockit);

        m_jobs.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailLockJobsT() { }

    public ScenarioDetailLockJobsT(BaseId scenarioId, BaseIdList jobs, bool lockit)
        : base(scenarioId)
    {
        this.lockit = lockit;
        m_jobs = jobs;
    }

    private readonly BaseIdList m_jobs;
    public BaseIdList Jobs => m_jobs;

    private readonly bool lockit;

    public bool Lockit => lockit;

    public override string Description
    {
        get
        {
            string lockString = Lockit ? "Locked".Localize() : "Unlocked".Localize();
            return Jobs.Count > 1 ? string.Format("Jobs {0} ({1})".Localize(), lockString, m_jobs.Count) : string.Format("Job {0}".Localize(), lockString);
        }
    }
}

/// <summary>
/// Transmission for Locking or Unlocking a list of MOs.
/// </summary>
public class ScenarioDetailLockMOsT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 278;

    #region IPTSerializable Members
    public ScenarioDetailLockMOsT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out lockit);

            mos = new MOKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(lockit);

        mos.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailLockMOsT() { }

    public ScenarioDetailLockMOsT(BaseId scenarioId, MOKeyList mos, bool lockit)
        : base(scenarioId)
    {
        this.mos = mos;
        this.lockit = lockit;
    }

    private readonly MOKeyList mos;

    public MOKeyList MOs => mos;

    private readonly bool lockit;

    public bool Lockit => lockit;

    public override string Description
    {
        get
        {
            string moLockString = Lockit ? "Locked".Localize() : "Unlocked".Localize();
            return MOs.Count > 1 ? string.Format("Manufacturing Orders {0} ({1})".Localize(), moLockString, MOs.Count) : string.Format("Manufacturing Order {0}".Localize(), moLockString);
        }
    }
}

/// <summary>
/// Transmission for Locking or Unlocking a list of MOs to a path.
/// </summary>
public class ScenarioDetailLockMOsToPathT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 281;

    #region IPTSerializable Members
    public ScenarioDetailLockMOsToPathT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out lockit);

            mos = new MOKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(lockit);

        mos.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailLockMOsToPathT() { }

    public ScenarioDetailLockMOsToPathT(BaseId scenarioId, MOKeyList mos, bool lockit)
        : base(scenarioId)
    {
        this.mos = mos;
        this.lockit = lockit;
    }

    private readonly MOKeyList mos;

    public MOKeyList MOs => mos;

    private readonly bool lockit;

    public bool Lockit => lockit;

    public override string Description
    {
        get
        {
            string moLockString = Lockit ? "Locked".Localize() : "Unlocked".Localize();
            return MOs.Count > 1 ? string.Format("Manufacturing Orders {0} ({1})".Localize(), moLockString, MOs.Count) : string.Format("Manufacturing Order {0}".Localize(), moLockString);
        }
    }
}

/// <summary>
/// Transmission for Locking or Unlocking a list of Operations.
/// </summary>
public class ScenarioDetailLockOperationsT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 279;

    #region IPTSerializable Members
    public ScenarioDetailLockOperationsT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out lockit);

            operations = new OperationKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(lockit);

        operations.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailLockOperationsT() { }

    public ScenarioDetailLockOperationsT(BaseId scenarioId, OperationKeyList operations, bool lockit)
        : base(scenarioId)
    {
        this.operations = operations;
        this.lockit = lockit;
    }

    private readonly OperationKeyList operations;

    public OperationKeyList Operations => operations;

    private readonly bool lockit;

    public bool Lockit => lockit;

    public override string Description
    {
        get
        {
            string operationLockStr = Lockit ? "Locked".Localize() : "Unlocked".Localize();
            return Operations.Count > 1 ? string.Format("Operations {0} ({1})".Localize(), operationLockStr, Operations.Count) : string.Format("Operation {0}".Localize(), operationLockStr);
        }
    }
}

/// <summary>
/// Transmission for Locking or Unlocking a list of Activitys.
/// </summary>
public class ScenarioDetailLockActivitiesT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 276;

    #region IPTSerializable Members
    public ScenarioDetailLockActivitiesT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out lockit);
            activitys = new ActivityKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(lockit);

        activitys.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailLockActivitiesT() { }

    public ScenarioDetailLockActivitiesT(BaseId scenarioId, ActivityKeyList activitys, bool lockit)
        : base(scenarioId)
    {
        this.activitys = activitys;
        this.lockit = lockit;
    }

    private readonly ActivityKeyList activitys;

    public ActivityKeyList Activitys => activitys;

    private readonly bool lockit;

    public bool Lockit => lockit;

    public override string Description
    {
        get
        {
            string activityLockStr = Lockit ? "Locked".Localize() : "UnLocked".Localize();
            return Activitys.Count > 1 ? string.Format("Activities {0} ({1})".Localize(), activityLockStr, Activitys.Count) : string.Format("Activity {0}", activityLockStr);
        }
    }
}

/// <summary>
/// Transmission for Locking or Unlocking a list of Blocks.
/// </summary>
public class ScenarioDetailLockBlocksT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 277;

    #region IPTSerializable Members
    public ScenarioDetailLockBlocksT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out lockit);

            blocks = new BlockKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(lockit);

        blocks.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailLockBlocksT() { }

    public ScenarioDetailLockBlocksT(BaseId scenarioId, BlockKeyList blocks, bool lockit)
        : base(scenarioId)
    {
        this.blocks = blocks;
        this.lockit = lockit;
    }

    private readonly BlockKeyList blocks;

    public BlockKeyList Blocks => blocks;

    private readonly bool lockit;

    public bool Lockit => lockit;

    public override string Description
    {
        get
        {
            string blockLockStr = Lockit ? "Locked".Localize() : "Unlocked".Localize();
            return Blocks.Count > 1 ? string.Format("Block {0} ({1})".Localize(), blockLockStr, Blocks.Count) : string.Format("Block {0}", blockLockStr);
        }
    }
}