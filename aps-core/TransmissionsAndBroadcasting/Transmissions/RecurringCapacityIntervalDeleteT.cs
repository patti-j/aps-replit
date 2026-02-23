using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Deletes the RecurringCapacityInterval.
/// </summary>
public class RecurringCapacityIntervalDeleteT : RecurringCapacityIntervalIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 104;

    #region IPTSerializable Members
    public RecurringCapacityIntervalDeleteT(IReader reader)
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

    public RecurringCapacityIntervalDeleteT() { }

    public RecurringCapacityIntervalDeleteT(BaseId scenarioId, List<BaseId> a_recurringCapacityIntervalIds)
        : base(scenarioId, a_recurringCapacityIntervalIds) { }

    public override string Description => "Capacity(Recurring) deleted";
}