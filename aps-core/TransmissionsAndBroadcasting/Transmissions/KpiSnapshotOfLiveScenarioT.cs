using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Stores a Snapshot of the Live Scenario.
/// Sent from the AutomaticActions to save KPI automatically.
/// </summary>
public class KpiSnapshotOfLiveScenarioT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 710;

    #region IPTSerializable Members
    public KpiSnapshotOfLiveScenarioT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public KpiSnapshotOfLiveScenarioT()
        : base(BaseId.ERP_ID)
    {
        Destination = EDestinations.ToLiveScenario; //Need this to route it to the LiveScenario.
    }

    public override string Description => "KPI snapshot created";
}