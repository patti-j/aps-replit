namespace PT.Scheduler;

internal class TimingWorkArounds
{
    // There are timing issues related in longs, doubles, division, in
    // InternalResource.FindCapacity() and CalculateCapacity.
    // Possible changes that are necessary before you can get rid of this work around
    // include limiting NbrOfPeople,Efficiency, and possibly splitting up capacity intervals
    // that are very long (for instance if there is only 1; though I'm not sure of this).
    internal static long OneSecond = TimeSpan.TicksPerSecond;
    internal static long OneMinute = TimeSpan.TicksPerMinute;
}