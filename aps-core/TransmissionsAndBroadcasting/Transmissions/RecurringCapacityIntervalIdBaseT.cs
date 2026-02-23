using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base class for all RecurringCapacityInterval related transmissions.
/// </summary>
public abstract class RecurringCapacityIntervalIdBaseT : RecurringCapacityIntervalBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 105;

    #region IPTSerializable Members
    public RecurringCapacityIntervalIdBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 12051)
        {
            reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                BaseId rciId = new (reader);
                RecurringCapacityIntervalIds.Add(rciId);
            }
        }
        else if (reader.VersionNumber >= 1)
        {
            BaseId rciId = new (reader);
            RecurringCapacityIntervalIds.Add(rciId);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(RecurringCapacityIntervalIds.Count);

        foreach (BaseId recurringCapacityIntervalId in RecurringCapacityIntervalIds)
        {
            recurringCapacityIntervalId.Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public List<BaseId> RecurringCapacityIntervalIds = new ();

    protected RecurringCapacityIntervalIdBaseT() { }

    protected RecurringCapacityIntervalIdBaseT(BaseId a_scenarioId, List<BaseId> a_recurringCapacityIntervalIds)
        : base(a_scenarioId)
    {
        RecurringCapacityIntervalIds = a_recurringCapacityIntervalIds;
    }
}