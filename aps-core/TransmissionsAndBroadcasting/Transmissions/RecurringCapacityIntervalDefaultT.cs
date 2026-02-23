using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Creates a new RecurringCapacityInterval in the specified Scenario using default values.
/// </summary>
public class RecurringCapacityIntervalDefaultT : RecurringCapacityIntervalBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 103;

    #region IPTSerializable Members
    public RecurringCapacityIntervalDefaultT(IReader reader)
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

    public RecurringCapacityIntervalDefaultT() { }

    public RecurringCapacityIntervalDefaultT(BaseId scenarioId)
        : base(scenarioId) { }

    public override string Description => "Capacity (Recurring) Created".Localize();
}