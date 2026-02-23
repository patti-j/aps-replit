using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Creates a new RecurringCapacityInterval by converting from a pre-existing CapacityInterval.
/// </summary>
public class RecurringCapacityIntervalConvertT : RecurringCapacityIntervalBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 101;

    #region IPTSerializable Members
    public RecurringCapacityIntervalConvertT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            originalCapacityIntervalId = new BaseId(reader);
            recurringCapacityInterval = new RecurringCapacityInterval(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        originalCapacityIntervalId.Serialize(writer);
        recurringCapacityInterval.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId originalCapacityIntervalId; //Id of the CapacityInterval to delete.
    public RecurringCapacityInterval recurringCapacityInterval; //Provides the values to use when creating the new RecurringCapacityInterval.  Its Id should not be used.

    public RecurringCapacityIntervalConvertT() { }

    public RecurringCapacityIntervalConvertT(BaseId scenarioId, BaseId originalCapacityIntervalId, RecurringCapacityInterval recurringCapacityInterval)
        : base(scenarioId)
    {
        this.originalCapacityIntervalId = originalCapacityIntervalId;
        this.recurringCapacityInterval = recurringCapacityInterval;
    }

    public override string Description => "Recurring Capacity Interval converted";
}