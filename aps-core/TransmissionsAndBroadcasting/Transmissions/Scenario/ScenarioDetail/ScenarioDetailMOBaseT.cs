using PT.APSCommon;

namespace PT.Transmissions;

public abstract class ScenarioDetailMOBaseT : ScenarioIdBaseT
{
    #region IPTSerializable Members
    public ScenarioDetailMOBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            m_jobId = new BaseId(reader);
            m_moId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_jobId.Serialize(writer);
        m_moId.Serialize(writer);
    }
    #endregion

    protected ScenarioDetailMOBaseT() { }

    protected ScenarioDetailMOBaseT(BaseId a_scenarioId, BaseId a_jobId, BaseId a_moId)
        : base(a_scenarioId)
    {
        JobId = a_jobId;
        MOId = a_moId;
    }

    private BaseId m_jobId;

    public BaseId JobId
    {
        get => m_jobId;
        private set => m_jobId = value;
    }

    private BaseId m_moId;

    public BaseId MOId
    {
        get => m_moId;
        private set => m_moId = value;
    }
}