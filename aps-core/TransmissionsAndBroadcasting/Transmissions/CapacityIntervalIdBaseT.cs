using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base class for all CapacityInterval related transmissions.
/// </summary>
public abstract class CapacityIntervalIdBaseT : CapacityIntervalBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 43;

    #region IPTSerializable Members
    public CapacityIntervalIdBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 12051)
        {
            reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                BaseId rciId = new (reader);
                CapacityIntervalIds.Add(rciId);
            }
        }
        else if (reader.VersionNumber >= 1)
        {
            BaseId rciId = new (reader);
            CapacityIntervalIds.Add(rciId);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(CapacityIntervalIds.Count);

        foreach (BaseId recurringCapacityIntervalId in CapacityIntervalIds)
        {
            recurringCapacityIntervalId.Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public List<BaseId> CapacityIntervalIds = new ();

    protected CapacityIntervalIdBaseT() { }

    public CapacityIntervalIdBaseT(BaseId scenarioId, List<BaseId> a_capacityIntervalId)
        : base(scenarioId)
    {
        CapacityIntervalIds = a_capacityIntervalId;
    }
}