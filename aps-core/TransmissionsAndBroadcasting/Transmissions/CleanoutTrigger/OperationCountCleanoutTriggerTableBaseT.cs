using PT.APSCommon;

namespace PT.Transmissions.CleanoutTrigger;

/// <summary>
/// Base class for all TriggerCleanoutTable related Transmissions.
/// </summary>
public abstract class OperationCountCleanoutTriggerTableBaseT : ScenarioIdBaseT
{
    #region IPTSerializable Members
    public OperationCountCleanoutTriggerTableBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 12305) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public const int UNIQUE_ID = 1084;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    protected OperationCountCleanoutTriggerTableBaseT() { }

    protected OperationCountCleanoutTriggerTableBaseT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "Operation Count Cleanout Trigger Table updated";
}