namespace PT.Scheduler;

/// <summary>
/// Used to specify a timespan on a resource is used by a block. One of these should be added to for each block scheduled that has max delay.
/// </summary>
internal class BlockResourceSpan : ResourceSpan
{
    internal BlockResourceSpan(ResourceBlock a_block, long a_start, long a_end) : base(a_block, a_start, a_end) { }
}