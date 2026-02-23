using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Transmission for moving an existing RecurringCapacityInterval to a new time.
/// </summary>
public class RecurringCapacityIntervalMoveInTimeT : RecurringCapacityIntervalIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 107;

    #region IPTSerializable Members
    public RecurringCapacityIntervalMoveInTimeT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 171)
        {
            reader.Read(out newStartTime);
            reader.Read(out newEndTime);
            reader.Read(out oldStartTime);
            reader.Read(out oldEndTime);
            bool haveOccurrenceResource;
            reader.Read(out haveOccurrenceResource);
            if (haveOccurrenceResource)
            {
                occurrencResource = new ResourceKey(reader);
            }
        }
        else if (reader.VersionNumber >= 1)
        {
            reader.Read(out newStartTime);
            reader.Read(out newEndTime);
            reader.Read(out oldStartTime);
            reader.Read(out oldEndTime);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        writer.Write(newStartTime);
        writer.Write(newEndTime);
        writer.Write(oldStartTime);
        writer.Write(oldEndTime);
        writer.Write(occurrencResource != null);
        if (occurrencResource != null)
        {
            occurrencResource.Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public RecurringCapacityIntervalMoveInTimeT() { }

    public RecurringCapacityIntervalMoveInTimeT(BaseId scenarioId, BaseId capacityIntervalId, DateTime newStartTime, DateTime newEndTime, DateTime oldStartTime, DateTime oldEndTime, ResourceKey aOccurrenceResource)
        : base(scenarioId, new List<BaseId> { capacityIntervalId })
    {
        this.newStartTime = newStartTime;
        this.newEndTime = newEndTime;
        this.oldStartTime = oldStartTime;
        this.oldEndTime = oldEndTime;
        occurrencResource = aOccurrenceResource;
    }

    public DateTime newStartTime;
    public DateTime newEndTime;
    public DateTime oldStartTime;
    public DateTime oldEndTime;

    private readonly ResourceKey occurrencResource;

    /// <summary>
    /// Identifies the Resource the occurrence belongs to.
    /// </summary>
    public ResourceKey OccurrenceResource => occurrencResource;

    public override string Description => "Capacity series moved";
}