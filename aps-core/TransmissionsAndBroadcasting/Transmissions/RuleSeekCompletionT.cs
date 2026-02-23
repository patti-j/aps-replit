using PT.SchedulerDefinitions;

namespace PT.Transmissions;

public class RuleSeekCompletionT : ScenarioBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 769;

    public RuleSeekCompletionT() { }

    public RuleSeekCompletionT(RuleSeekEndReasons a_reason)
    {
        m_endReason = a_reason;
    }

    #region IPTSerializable Members
    public RuleSeekCompletionT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            short reason;
            reader.Read(out reason);
            m_endReason = (RuleSeekEndReasons)reason;
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write((short)m_endReason);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private readonly RuleSeekEndReasons m_endReason;

    /// <summary>
    /// The reason that the simulation was ended
    /// </summary>
    public RuleSeekEndReasons EndReason => m_endReason;
}