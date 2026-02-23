using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new CapacityInterval by converting from a pre-existing RecurringCapacityInterval.
/// </summary>
public class CapacityIntervalConvertT : CapacityIntervalBaseT, IPTSerializable
{
    public override string Description => "Capacity Interval converted";

    public new const int UNIQUE_ID = 38;

    #region IPTSerializable Members
    public CapacityIntervalConvertT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            originalRecurringCapacityIntervalId = new BaseId(reader);
            capacityInterval = new CapacityInterval(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        originalRecurringCapacityIntervalId.Serialize(writer);
        capacityInterval.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId originalRecurringCapacityIntervalId; //Id of the RecurringCapacityInterval to delete.
    public CapacityInterval capacityInterval; //Provides the values to use when creating the new CapacityInterval.  Its Id should not be used.

    public CapacityIntervalConvertT() { }

    public CapacityIntervalConvertT(BaseId scenarioId, BaseId originalRecurringCapacityIntervalId, CapacityInterval capacityInterval)
        : base(scenarioId)
    {
        this.originalRecurringCapacityIntervalId = originalRecurringCapacityIntervalId;
        this.capacityInterval = capacityInterval;
    }
}