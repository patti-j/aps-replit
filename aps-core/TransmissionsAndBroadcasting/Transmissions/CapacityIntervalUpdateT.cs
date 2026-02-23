using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Updates the values of the interval based on the values specified.
/// </summary>
public class CapacityIntervalUpdateT : CapacityIntervalIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 48;

    #region IPTSerializable Members
    public CapacityIntervalUpdateT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            capacityInterval = new CapacityInterval(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        capacityInterval.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public CapacityInterval capacityInterval;

    public CapacityIntervalUpdateT() { }

    public CapacityIntervalUpdateT(BaseId scenarioId, CapacityInterval capacityInterval, BaseId capacityIntervalId)
        : base(scenarioId, new List<BaseId> { capacityIntervalId })
    {
        this.capacityInterval = capacityInterval;
    }

    public override string Description
    {
        get
        {
            if (!string.IsNullOrEmpty(capacityInterval?.Name))
            {
                return string.Format("Capacity interval '{0}' updated".Localize(), capacityInterval.Name);
            }

            return "Capacity Interval updated";
        }
    }
}