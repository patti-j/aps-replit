using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Updates KPI Options.
/// </summary>
public class ScenarioKpiOptionsUpdateT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 709;

    #region IPTSerializable Members
    public ScenarioKpiOptionsUpdateT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            m_kpiOptions = new KpiOptions(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_kpiOptions.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioKpiOptionsUpdateT() { }

    public ScenarioKpiOptionsUpdateT(BaseId aScenarioId, KpiOptions aKpiOptions)
        : base(aScenarioId)
    {
        m_kpiOptions = aKpiOptions.Clone();
    }

    private readonly KpiOptions m_kpiOptions;

    /// <summary>
    /// The options that were set.
    /// </summary>
    public KpiOptions Options => m_kpiOptions;

    public override string Description => "KPI Options updated";
}