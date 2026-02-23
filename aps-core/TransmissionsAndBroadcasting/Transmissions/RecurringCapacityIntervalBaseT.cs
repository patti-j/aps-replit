using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base class for all RecurringCapacityInterval related transmissions.
/// </summary>
public abstract class RecurringCapacityIntervalBaseT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 99;

    #region IPTSerializable Members
    public RecurringCapacityIntervalBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 172) { }
        else if (reader.VersionNumber >= 69)
        {
            DateTime deprecatedCurrentDisplayTime;
            reader.Read(out deprecatedCurrentDisplayTime);
        }
        else if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    protected RecurringCapacityIntervalBaseT() { }

    protected RecurringCapacityIntervalBaseT(BaseId scenarioId)
        : base(scenarioId) { }
}