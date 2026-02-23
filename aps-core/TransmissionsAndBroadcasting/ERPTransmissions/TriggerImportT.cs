using PT.APSCommon;
using PT.Common;

namespace PT.ERPTransmissions;

public class TriggerImportT : PerformImportBaseT
{
    [Obsolete("For older versions of the importer. A specific integration config id should now be used.")]
    public TriggerImportT(BaseId a_targetScenarioId)
    {
        //Todo Handle configId in places where this is called

        m_targetScenarioId = a_targetScenarioId;
    }

    /// <summary>
    /// Standard constructor for new TriggerImportTs using newer integration features.
    /// At least one of <see cref="a_targetScenarioId"/> or <see cref="a_targetConfigId"/> should be a concrete value.
    /// </summary>
    /// <param name="a_targetScenarioId"></param>
    /// <param name="a_targetConfigId"></param>
    public TriggerImportT(BaseId a_targetScenarioId, int a_targetConfigId) : this (a_targetScenarioId)
    {
        m_targetConfigId = a_targetConfigId;
    }

    #region IPTSerializable Members
    public TriggerImportT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12506)
        {
            m_targetScenarioId = new BaseId(a_reader);
            a_reader.Read(out m_targetConfigId);
        }
        else if (a_reader.VersionNumber > 12035)
        {
            m_targetScenarioId = new BaseId(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        m_targetScenarioId.Serialize(a_writer);
        a_writer.Write(m_targetConfigId);
    }

    public new const int UNIQUE_ID = 919;

    public override int UniqueId => UNIQUE_ID;

    private BaseId m_targetScenarioId = BaseId.NULL_ID;

    private int m_targetConfigId = -1;

    public int TargetConfigId
    {
        get => m_targetConfigId;
        set => m_targetConfigId = value;
    }

public BaseId TargetScenarioId
    {
        get => m_targetScenarioId;
        set => m_targetScenarioId = value;
    }
    #endregion
}