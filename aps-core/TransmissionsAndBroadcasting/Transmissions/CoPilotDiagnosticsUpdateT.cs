using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Stores CoPilotDiagnositcs information for use on the client
/// </summary>
public class CoPilotDiagnositcsUpdateT : ScenarioBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 785;

    #region IPTSerializable Members
    public CoPilotDiagnositcsUpdateT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 460)
        {
            m_insertJobsDiagnostics = new InsertJobsDiagnostics(reader);
            m_ruleSeekDiagnostics = new RuleSeekDiagnositcs(reader);
        }
        else if (reader.VersionNumber >= 1)
        {
            m_insertJobsDiagnostics = new InsertJobsDiagnostics(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        m_insertJobsDiagnostics.Serialize(writer);
        m_ruleSeekDiagnostics.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public CoPilotDiagnositcsUpdateT() { }

    public CoPilotDiagnositcsUpdateT(InsertJobsDiagnostics a_insertJobsDiagnostics, RuleSeekDiagnositcs a_ruleSeekDiagnostics)
    {
        m_insertJobsDiagnostics = a_insertJobsDiagnostics;
        m_ruleSeekDiagnostics = a_ruleSeekDiagnostics;
    }

    private readonly InsertJobsDiagnostics m_insertJobsDiagnostics;

    /// <summary>
    /// The latest InsertJobs diagnostics information from System ScenarioManager
    /// </summary>
    public InsertJobsDiagnostics InsertJobsDiagnostics => m_insertJobsDiagnostics;

    private readonly RuleSeekDiagnositcs m_ruleSeekDiagnostics;

    /// <summary>
    /// The latest RuleSeek diagnostics information from System ScenarioManager
    /// </summary>
    public RuleSeekDiagnositcs RuleSeekDiagnostics => m_ruleSeekDiagnostics;
}