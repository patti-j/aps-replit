using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;
public class ScenarioLoadT : ScenarioBaseT
{
    public const int UNIQUE_ID = 1123;
    public override int UniqueId => UNIQUE_ID;
    private readonly BaseId m_scenarioToLoadId; 
    private readonly BaseId m_userRequestingLoadId;
    /// <summary>
    /// Since UndoSets are being moved to only being on the server instead of on both the client and server,
    /// the server will be sending the Scenario bytes to all clients after the server processes the requested UndoSets.
    /// This means that the load will need to be performed on all clients instead of being client specific like
    /// the load normally is. 
    /// </summary>
    private bool m_loadForAllClients;

    public ScenarioLoadT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_scenarioToLoadId = new BaseId(a_reader);
            m_userRequestingLoadId = new BaseId(a_reader);
            a_reader.Read(out m_loadForAllClients);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_scenarioToLoadId.Serialize(a_writer);
        m_userRequestingLoadId.Serialize(a_writer);
        a_writer.Write(m_loadForAllClients);
    }

    public ScenarioLoadT() { }
    public ScenarioLoadT(BaseId a_scenarioId, BaseId a_userId)
    {
        m_scenarioToLoadId = a_scenarioId;
        m_userRequestingLoadId = a_userId;
        m_loadForAllClients = false;
    }
    public ScenarioLoadT(BaseId a_scenarioId, BaseId a_userId, bool a_loadForAllClients)
    {
        m_scenarioToLoadId = a_scenarioId;
        m_userRequestingLoadId = a_userId;
        m_loadForAllClients = a_loadForAllClients;
    }

    public BaseId ScenarioToLoadId => m_scenarioToLoadId;
    public BaseId UserRequestingLoadId => m_userRequestingLoadId;
    public bool LoadForAllClients => m_loadForAllClients;

    public override string Description => "Scenario Loaded".Localize();
}
