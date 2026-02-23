using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Stores the serialized scenario bytes of the new scenario to add.
/// Serialized scenario is compressed on construction.
/// </summary>
public class ScenarioAddNewT : ScenarioBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 780;

    #region IPTSerializable Members
    public ScenarioAddNewT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 12544)
        {
            m_newId = new BaseId(reader);
            m_originalInstigatorId = new BaseId(reader);
            reader.Read(out m_scenarioBytes);
            reader.Read(out m_newScenarioName);
        } else if (reader.VersionNumber >= 12204)
        {
            m_newId = new BaseId(reader);
            reader.Read(out m_scenarioBytes);
            reader.Read(out m_newScenarioName);
        }
        else if (reader.VersionNumber >= 513)
        {
            m_newId = new BaseId(reader);
            reader.Read(out m_scenarioBytes);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
        m_newId.Serialize(writer);
        m_originalInstigatorId.Serialize(writer);
        writer.Write(m_scenarioBytes);
        writer.Write(m_newScenarioName);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioAddNewT() { }

    public ScenarioAddNewT(byte[] a_scenarioBytes, BaseId a_newScenarioId, string a_newScenarioName,BaseId a_originalInstigatorId = default)
    {
        m_newId = a_newScenarioId;
        m_newScenarioName = a_newScenarioName;
        m_originalInstigatorId = a_originalInstigatorId;
        m_scenarioBytes = a_scenarioBytes;
    }

    /// <summary>
    /// Scenario that has been serialized and compressed.
    /// </summary>
    protected byte[] m_scenarioBytes;

    public byte[] ScenarioBytes => m_scenarioBytes;

    private readonly BaseId m_newId;
    private readonly string m_newScenarioName;

    private BaseId m_originalInstigatorId;
    /// <summary>
    /// Id of instigator of the parent transmission (if this transmission was sent as part of the process of another transmission)
    /// </summary>
    public BaseId OriginalInstigatorId
    {
        get => m_originalInstigatorId;
        set => m_originalInstigatorId = value;
    }

    /// <summary>
    /// New Id for the scenario
    /// </summary>
    public BaseId NewScenarioId => m_newId;

    /// <summary>
    /// New name for the scenario
    /// </summary>
    public string NewScenarioName => m_newScenarioName;
}