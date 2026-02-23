using PT.APSCommon;
using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// Transmission for Committing Resource Schedules.
/// </summary>
public class ScheduleCommitT : JobBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 668;

    #region IPTSerializable Members
    public ScheduleCommitT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 711)
        {
            m_bools = new BoolVector32(a_reader);
            Resources = new ResourceKeyList(a_reader);
        }
        else if (a_reader.VersionNumber >= 1)
        {
            Resources = new ResourceKeyList(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_bools.Serialize(a_writer);
        Resources.Serialize(a_writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScheduleCommitT() { }

    public ScheduleCommitT(BaseId scenarioId, ResourceKeyList aResources)
        : base(scenarioId)
    {
        Resources = aResources;
    }

    public ResourceKeyList Resources;
    private BoolVector32 m_bools;

    private const short c_autoJoinMOsIdx = 0;

    public bool AutoJoinMOs
    {
        get => m_bools[c_autoJoinMOsIdx];
        set => m_bools[c_autoJoinMOsIdx] = value;
    }

    public override string Description => "Schedule committed";
}