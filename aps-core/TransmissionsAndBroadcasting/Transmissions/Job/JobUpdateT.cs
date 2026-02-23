using PT.APSCommon;

namespace PT.Transmissions.Job;

public class JobUpdateT : ScenarioIdBaseT
{
    private BaseId m_scenarioId;
    private readonly string m_jobExternalId;
    private readonly byte[] m_jobBytes;

    public JobUpdateT() { }

    public JobUpdateT(BaseId a_scenarioId, byte[] a_jobBytes, string a_jobExternalId) : base(a_scenarioId)
    {
        m_scenarioId = a_scenarioId;
        m_jobBytes = a_jobBytes;
        m_jobExternalId = a_jobExternalId;
    }

    #region IPTSerializable Members
    public JobUpdateT(IReader a_reader) : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            if (a_reader.VersionNumber >= 1)
            {
                a_reader.Read(out m_jobBytes);
                a_reader.Read(out m_jobExternalId);
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(JobBytes);
        a_writer.Write(ExternalId);
    }

    public override int UniqueId => UNIQUE_ID;
    public const int UNIQUE_ID = 1011;
    #endregion

    public byte[] JobBytes => m_jobBytes;

    public string ExternalId => m_jobExternalId;
}