namespace PT.Transmissions;

/// <summary>
/// Signals the start of a new InsertJobs Simulation. Provides the ID of scenario to use.
/// </summary>
public class InsertJobsUserEndT : ScenarioBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 779;

    #region IPTSerializable Members
    public InsertJobsUserEndT(IReader reader) : base(reader)
    {
        if (reader.VersionNumber > 458)
        {
            reader.Read(out m_endType);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        writer.Write(m_endType);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public InsertJobsUserEndT() { }

    public InsertJobsUserEndT(EndTypes a_endType)
    {
        m_endType = (short)a_endType;
    }

    public enum EndTypes { Stop, Cancel }

    private readonly short m_endType;

    public EndTypes EndType => (EndTypes)m_endType;
}