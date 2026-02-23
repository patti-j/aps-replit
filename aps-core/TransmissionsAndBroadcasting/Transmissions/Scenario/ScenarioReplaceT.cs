using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Stores the BaseId of the scenario to replace and the serialized scenario bytes.
/// Serialized scenario is compressed on construction.
/// </summary>
public class ScenarioReplaceT : ScenarioAddNewT, IPTSerializable
{
    public new const int UNIQUE_ID = 788;

    #region IPTSerializable Members
    public ScenarioReplaceT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 12515)
        {
            m_bools = new BoolVector32(reader);
            m_scenarioToReplaceId = new BaseId(reader);
            reader.Read(out m_instigatorTransmissionId);
        }
        else if (reader.VersionNumber >= 12511)
        {
            m_scenarioToReplaceId = new BaseId(reader);
            reader.Read(out m_instigatorTransmissionId);
        }
        else if (reader.VersionNumber >= 462)
        {
            m_scenarioToReplaceId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_bools.Serialize(writer);
        m_scenarioToReplaceId.Serialize(writer);
        writer.Write(m_instigatorTransmissionId);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioReplaceT() { }

    public ScenarioReplaceT(BaseId a_scenarioId, byte[] a_scenarioBytes)
        : base(a_scenarioBytes, a_scenarioId, null)
    {
        m_scenarioToReplaceId = a_scenarioId;
    }

    private int m_instigatorTransmissionId;
    /// <summary>
    /// Unique Id of parent transmission (if this transmission was sent as part of the process of another transmission)
    /// </summary>
    public int InstigatorTransmissionId
    {
        get => m_instigatorTransmissionId;
        set => m_instigatorTransmissionId = value;
    }
    private readonly BaseId m_scenarioToReplaceId;

    /// <summary>
    /// Id of the scenario to replace. This is a member because the inherited class is not ScenarioIdBaseT
    /// </summary>
    public BaseId ScenarioToReplaceId => m_scenarioToReplaceId;

    /// <summary>
    /// Removes the scenario bytes. This might be called when the system is broadcasting this transmission to itself.
    /// </summary>
    public void ClearScenarioBytes()
    {
        //this cannot be null because it is still serialized.
        m_scenarioBytes = new byte[0];
    }

    public bool ContainsScenario => m_scenarioBytes.Length > 0;

    private BoolVector32 m_bools;
    private const short c_cancellingSimulationIdx = 0;
    /// <summary>
    /// Flag with indicates a ScenarioReplaceT was sent as a result of undoing a cancelled simulation action
    /// </summary>
    public bool CancellingSimulation
    {
        get => m_bools[c_cancellingSimulationIdx];
        set => m_bools[c_cancellingSimulationIdx] = value;
    }
}