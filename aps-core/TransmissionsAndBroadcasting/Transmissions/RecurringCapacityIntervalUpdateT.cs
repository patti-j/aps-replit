using PT.APSCommon;
using PT.APSCommon.Extensions;

namespace PT.Transmissions;

/// <summary>
/// Updates the values of the interval based on the values specified.
/// </summary>
public class RecurringCapacityIntervalUpdateT : RecurringCapacityIntervalIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 110;

    #region IPTSerializable Members
    public RecurringCapacityIntervalUpdateT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 171)
        {
            recurringCapacityInterval = new RecurringCapacityInterval(reader);
            reader.Read(out actOnSeries);
            reader.Read(out occurrenceStart);
            reader.Read(out occurrenceEnd);
            bool haveOccurrenceResouce;
            reader.Read(out haveOccurrenceResouce);
            if (haveOccurrenceResouce)
            {
                occurrencResource = new SchedulerDefinitions.ResourceKey(reader);
            }
        }
        else if (reader.VersionNumber >= 1)
        {
            recurringCapacityInterval = new RecurringCapacityInterval(reader);
            actOnSeries = true;
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        recurringCapacityInterval.Serialize(writer);
        writer.Write(actOnSeries);
        writer.Write(occurrenceStart);
        writer.Write(occurrenceEnd);
        writer.Write(occurrencResource != null);
        if (occurrencResource != null)
        {
            occurrencResource.Serialize(writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public RecurringCapacityInterval recurringCapacityInterval;

    public RecurringCapacityIntervalUpdateT() { }

    /// <summary>
    /// Constructor can only be used to act on a series. (not an occurrence)
    /// </summary>
    /// <param name="scenarioId"></param>
    /// <param name="recurringCapacityInterval"></param>
    /// <param name="recurringCapacityIntervalId"></param>
    public RecurringCapacityIntervalUpdateT(BaseId scenarioId, RecurringCapacityInterval recurringCapacityInterval, BaseId recurringCapacityIntervalId)
        : base(scenarioId, new List<BaseId> { recurringCapacityIntervalId })
    {
        this.recurringCapacityInterval = recurringCapacityInterval;
        actOnSeries = true;
    }

    /// <summary>
    /// Constructor to use when acting on an occurrence.  Can also be used when acting on a series.
    /// </summary>
    /// <param name="scenarioId"></param>
    /// <param name="recurringCapacityInterval"></param>
    /// <param name="recurringCapacityIntervalId"></param>
    /// <param name="aOccurrenceStart">The current scheduled start of the occurrence clicked on.</param>
    /// <param name="aOccurrenceEnd">The current scheduled end of the occurrence clicked on.</param>
    public RecurringCapacityIntervalUpdateT(BaseId scenarioId, RecurringCapacityInterval recurringCapacityInterval, BaseId recurringCapacityIntervalId, DateTime aOccurrenceStart, DateTime aOccurrenceEnd, bool aActOnSeries, SchedulerDefinitions.ResourceKey aOccurrenceResource)
        : base(scenarioId, new List<BaseId> { recurringCapacityIntervalId })
    {
        this.recurringCapacityInterval = recurringCapacityInterval;
        actOnSeries = aActOnSeries;
        occurrenceStart = aOccurrenceStart;
        occurrenceEnd = aOccurrenceEnd;
        occurrencResource = aOccurrenceResource;
    }

    public override string Description
    {
        get
        {
            if (recurringCapacityInterval?.Name != null)
            {
                return string.Format("Capacity series '{0}' updated".Localize(), recurringCapacityInterval.Name);
            }

            return "Capacity series updated";
        }
    }

    private readonly bool actOnSeries;

    /// <summary>
    /// If true then update the Recurring Capacity Interval itself.
    /// If false then create a Capacity Interval for each occurrence prior to and including the occurrence clicked on -- as specified by the OccurenceStart/OccurrenceEnd.
    /// </summary>
    public bool ActOnSeries => actOnSeries;

    private readonly DateTime occurrenceStart;

    /// <summary>
    /// The start date/time of the occurrence clicked on at the time it was clicked on.
    /// </summary>
    public DateTime OccurrenceStart => occurrenceStart;

    private readonly DateTime occurrenceEnd;

    /// <summary>
    /// The end date/time of the occurrence clicked on at the time it was clicked on.
    /// </summary>
    public DateTime OccurrenceEnd => occurrenceEnd;

    private readonly SchedulerDefinitions.ResourceKey occurrencResource;

    /// <summary>
    /// Identifies the Resource the occurrence belongs to.
    /// This may be null.
    /// </summary>
    public SchedulerDefinitions.ResourceKey OccurrenceResource => occurrencResource;
}