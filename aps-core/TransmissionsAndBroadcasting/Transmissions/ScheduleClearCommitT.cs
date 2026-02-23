using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Transmission for Committing Resource Schedules.
/// </summary>
public class ScheduleClearCommitT : JobBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 669;

    #region IPTSerializable Members
    public ScheduleClearCommitT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            resources = new ResourceKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        resources.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScheduleClearCommitT() { }

    public ScheduleClearCommitT(BaseId scenarioId, ResourceKeyList aResources)
        : base(scenarioId)
    {
        resources = aResources;
    }

    public ResourceKeyList resources;

    public override string Description => "Schedule Commit Dates cleared";
}