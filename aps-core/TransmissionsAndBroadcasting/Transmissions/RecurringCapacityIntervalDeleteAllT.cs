using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes all RecurringCapacityIntervals in the specified Scenario.
/// </summary>
public class RecurringCapacityIntervalDeleteAllT : RecurringCapacityIntervalBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 284;

    #region IPTSerializable Members
    public RecurringCapacityIntervalDeleteAllT(IReader reader)
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

    public RecurringCapacityIntervalDeleteAllT() { }

    public RecurringCapacityIntervalDeleteAllT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "All Capacity (Recurring) deleted";
}