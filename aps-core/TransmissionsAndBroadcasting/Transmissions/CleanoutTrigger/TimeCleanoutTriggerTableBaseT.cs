using PT.APSCommon;

namespace PT.Transmissions.CleanoutTrigger;

/// <summary>
/// Base class for all TriggerCleanoutTable related Transmissions.
/// </summary>
public class TimeCleanoutTriggerTableBaseT : ScenarioIdBaseT
{
    #region IPTSerializable Members
    public TimeCleanoutTriggerTableBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 12305) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public const int UNIQUE_ID = 1062;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public TimeCleanoutTriggerTableBaseT() { }

    public TimeCleanoutTriggerTableBaseT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "Fixed Time Cleanout Trigger Table updated";
}