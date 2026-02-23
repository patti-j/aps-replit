using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Transmission for moving an existing CapacityInterval to a new time.
/// </summary>
public class CapacityIntervalMoveInTimeT : CapacityIntervalIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 44;

    #region IPTSerializable Members
    public CapacityIntervalMoveInTimeT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            reader.Read(out NewStartTime);
            reader.Read(out NewEndTime);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(NewStartTime);
        writer.Write(NewEndTime);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public DateTime NewStartTime;
    public DateTime NewEndTime;

    public CapacityIntervalMoveInTimeT() { }

    public CapacityIntervalMoveInTimeT(BaseId scenarioId, BaseId capacityIntervalId, DateTime newStartTime, DateTime newEndTime)
        : base(scenarioId, new List<BaseId> { capacityIntervalId })
    {
        NewStartTime = newStartTime;
        NewEndTime = newEndTime;
    }

    public override string Description => string.Format("Capacity interval moved to {0}".Localize(), NewStartTime.ToDisplayTime());
}