using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;
public class ScenarioReloadT : ScenarioBaseT
{
    public const int UNIQUE_ID = 1125;
    public override int UniqueId => UNIQUE_ID;
    private readonly BaseId m_scenarioToReloadId; 
    private readonly BaseId m_userRequestingReloadId;
    /// <summary>
    /// This Guid should correspond to the last transmission processed by the scenario that is being reloaded.
    /// It is meant to be used to deal with potential synchronization issues. For example, if the client is ahead
    /// of the server, then the scenario bytes retrieved from the server might be behind the client unless
    /// something is done to handle that.
    /// </summary>
    private readonly Guid m_lastTransmissionProcessedGuid;

    public ScenarioReloadT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_scenarioToReloadId = new BaseId(a_reader);
            m_userRequestingReloadId = new BaseId(a_reader);
            a_reader.Read(out m_lastTransmissionProcessedGuid);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_scenarioToReloadId.Serialize(a_writer);
        m_userRequestingReloadId.Serialize(a_writer);
        a_writer.Write(m_lastTransmissionProcessedGuid);
    }

    public ScenarioReloadT() { }
    public ScenarioReloadT(BaseId a_scenarioId, BaseId a_userId, Guid a_lastTransmissionProcessedGuid)
    {
        m_scenarioToReloadId = a_scenarioId;
        m_userRequestingReloadId = a_userId;
        m_lastTransmissionProcessedGuid = a_lastTransmissionProcessedGuid;
    }

    public BaseId ScenarioToReloadId => m_scenarioToReloadId;
    public BaseId UserRequestingReloadId => m_userRequestingReloadId;
    public Guid LastTransmissionsProcessedGuid => m_lastTransmissionProcessedGuid;

    public override string Description => "Scenario Reloaded".Localize();
}
