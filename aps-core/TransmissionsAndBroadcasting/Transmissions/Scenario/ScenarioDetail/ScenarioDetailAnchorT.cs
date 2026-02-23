using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Transmission for Freezeing or Unfreezeing a list of Jobs.
/// </summary>
public class ScenarioDetailAnchorJobsT : ScenarioIdBaseT, IPTSerializable, IHistoryJobIds
{
    public const int UNIQUE_ID = 127;

    #region IPTSerializable Members
    public ScenarioDetailAnchorJobsT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out anchor);

            jobs = new BaseIdList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(anchor);

        jobs.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailAnchorJobsT() { }

    public ScenarioDetailAnchorJobsT(BaseId scenarioId, BaseIdList jobs, bool anchor)
        : base(scenarioId)
    {
        this.anchor = anchor;
        this.jobs = jobs;
    }

    private readonly BaseIdList jobs;

    public BaseIdList Jobs => jobs;

    private readonly bool anchor;

    public bool Anchor => anchor;

    #region IHistoryJobIds Members
    public BaseIdList JobIds => Jobs;
    #endregion

    #region IHistory Members
    public override string Description
    {
        get
        {
            if (Anchor)
            {
                return jobs.Count == 1 ? "Job anchored" : string.Format("Jobs anchored ({0})".Localize(), jobs.Count);
            }

            return jobs.Count == 1 ? "Job unanchored" : string.Format("Jobs unanchored ({0})".Localize(), jobs.Count);
        }
    }
    #endregion
}

/// <summary>
/// Transmission for Freezeing or Unfreezeing a list of MOs.
/// </summary>
public class ScenarioDetailAnchorMOsT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 272;

    #region IPTSerializable Members
    public ScenarioDetailAnchorMOsT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out anchor);
            mos = new MOKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(anchor);
        mos.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailAnchorMOsT() { }

    public ScenarioDetailAnchorMOsT(BaseId scenarioId, MOKeyList mos, bool anchor)
        : base(scenarioId)
    {
        this.mos = mos;
        this.anchor = anchor;
    }

    private readonly MOKeyList mos;

    public MOKeyList MOs => mos;

    private readonly bool anchor;

    public bool Anchor => anchor;

    public override string Description
    {
        get
        {
            string anchorString = Anchor ? "Anchored" : "Unanchored";
            return MOs.Count > 1 ? string.Format("Jobs {0} ({1})".Localize(), anchorString, MOs.Count) : string.Format("Job {0}".Localize(), anchorString);
        }
    }
}

/// <summary>
/// Transmission for Freezeing or Unfreezeing a list of Operations.
/// </summary>
public class ScenarioDetailAnchorOperationsT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 273;

    #region IPTSerializable Members
    public ScenarioDetailAnchorOperationsT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out anchor);
            operations = new OperationKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(anchor);
        operations.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailAnchorOperationsT() { }

    public ScenarioDetailAnchorOperationsT(BaseId scenarioId, OperationKeyList operations, bool anchor)
        : base(scenarioId)
    {
        this.operations = operations;
        this.anchor = anchor;
    }

    private readonly OperationKeyList operations;

    public OperationKeyList Operations => operations;

    private readonly bool anchor;

    public bool Anchor => anchor;

    public override string Description
    {
        get
        {
            string anchorOpString = Anchor ? "Anchored" : "Unanchored";
            return Operations.Count > 1 ? string.Format("Operation {0} ({1})".Localize(), anchorOpString, Operations.Count) : string.Format("Operation {0}".Localize(), anchorOpString);
        }
    }
}

/// <summary>
/// Transmission for Freezeing or Unfreezeing a list of Activitys.
/// </summary>
public class ScenarioDetailAnchorActivitiesT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 271;

    #region IPTSerializable Members
    public ScenarioDetailAnchorActivitiesT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out anchor);
            activitys = new ActivityKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(anchor);
        activitys.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailAnchorActivitiesT() { }

    public ScenarioDetailAnchorActivitiesT(BaseId scenarioId, ActivityKeyList activitys, bool anchor)
        : base(scenarioId)
    {
        this.activitys = activitys;
        this.anchor = anchor;
    }

    private readonly ActivityKeyList activitys;

    public ActivityKeyList Activitys => activitys;

    private readonly bool anchor;

    public bool Anchor => anchor;

    public override string Description
    {
        get
        {
            string activityAnchorString = Anchor ? "Anchored" : "Unanchored";
            return Activitys.Count > 1 ? string.Format("Activities {0} ({1})".Localize(), activityAnchorString, Activitys.Count) : string.Format("Activity {0}", activityAnchorString);
        }
    }
}