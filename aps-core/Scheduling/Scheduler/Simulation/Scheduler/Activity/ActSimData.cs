namespace PT.Scheduler;

/// <summary>
/// Used to define and stored a dynamic amount of extra data used by simulation algorithms.
/// Space is only allocated on an as needed basis as opposed to declaring variables for all types of extra information even though some are unlikedly to ever be used, such as the batch data below.
/// This class uses a fix amount of space but the extra data needed by properties are only consumed as needed.
/// </summary>
internal class ActSimData : Simulation.ExtraSimData
{
    internal ActSimData()
        : base(c_totalNbrOfValues) { }

    private const int c_lastSimBatchIndexStartBytesIndex = 0;

    /// <summary>
    /// Used in the constructor to specify the total number of properties to store.
    /// This must be kept up to date as more properties are added.
    /// </summary>
    private const int c_totalNbrOfValues = 1;

    /// <summary>
    /// Used to store an index to the batch the activity was scheduled in in the last simulation.
    /// </summary>
    internal int LastSimBatchByteIndex
    {
        get => GetInt(c_lastSimBatchIndexStartBytesIndex);
        set => Set(value, c_lastSimBatchIndexStartBytesIndex);
    }
}