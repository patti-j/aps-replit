using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Stores a Snapshot for the specified Scenario.
/// </summary>
public class ScenarioKpiSnapshotT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 711;

    #region IPTSerializable Members
    public ScenarioKpiSnapshotT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out m_description);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(m_description);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioKpiSnapshotT() { }

    public ScenarioKpiSnapshotT(BaseId aScenarioId, string aDescription)
        : base(aScenarioId)
    {
        m_description = aDescription;
    }

    private readonly string m_description;

    /// <summary>
    /// Describes what the Snapshot represents, why it was saved, etc.
    /// </summary>
    public override string Description => "KPI Snapshots Saved".Localize();
}