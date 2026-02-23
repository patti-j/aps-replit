using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Creates a new RecurringCapacityInterval by copying the specified RecurringCapacityInterval.
/// </summary>
public class RecurringCapacityIntervalCopyT : RecurringCapacityIntervalBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 102;

    #region IPTSerializable Members
    public RecurringCapacityIntervalCopyT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            originalId = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        originalId.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public BaseId originalId; //Id of the RecurringCapacityInterval to copy.

    public RecurringCapacityIntervalCopyT() { }

    public RecurringCapacityIntervalCopyT(BaseId scenarioId, BaseId originalId)
        : base(scenarioId)
    {
        this.originalId = originalId;
    }

    public override string Description => "Capacity (Recurring) Copied".Localize();
}