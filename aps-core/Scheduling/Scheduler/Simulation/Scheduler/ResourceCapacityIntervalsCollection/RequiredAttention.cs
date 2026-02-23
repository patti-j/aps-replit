using PT.Common.PTMath;

namespace PT.Scheduler.Simulation;

/// <summary>
/// Specifies an amount of attention required of a capacity interval of a multi-tasking resource.
/// </summary>
internal class RequiredAttention : Interval
{
    /// <summary>
    /// Used to specify the attention required of a capcity interval of a multi-tasking resource over a timespan.
    /// This is used by various functions that need to compute available attention and is used in sets such as the scheduled attention set.
    /// </summary>
    /// <param name="a_start">Start of the interval of required attention.</param>
    /// <param name="a_end">End of the interval of reqiured attention.</param>
    /// <param name="a_attention">The amount of attention required.</param>
    /// <param name="a_seqNbr">A unique number within a capacity interval.</param>
    internal RequiredAttention(InternalActivity a_act, ResourceRequirement a_resReq, long a_start, long a_end, decimal a_attention, long a_seqNbr)
    {
        Activity = a_act;
        ResReq = a_resReq;
        StartTicks = a_start;
        EndTicks = a_end;
        Attention = a_attention;
        SeqNbr = a_seqNbr;
    }

    internal readonly InternalActivity Activity;

    internal readonly ResourceRequirement ResReq;

    /// <summary>
    /// Set by the AttentionAvailable algorithm.
    /// Whether the attention has been consumed during Attentionavailable testing; the start of the required timespan has been reached.
    /// </summary>
    internal bool AttnConsumed;

    /// <summary>
    /// Set by the AttentionAvailable algorithm.
    /// Whether the attention has been released after being consumed; the end of the required timespan has passed.
    /// </summary>
    /// <returns></returns>
    internal bool AttenReleased;

    /// <summary>
    /// The start of the required attention.
    /// </summary>
    //internal readonly long StartTicks;

    /// <summary>
    /// The end of the required attention.
    /// </summary>
    //internal readonly long EndTicks;

    /// <summary>
    /// The amount of attention required of the capacity interval.
    /// </summary>
    internal readonly decimal Attention;

    /// <summary>
    /// A unique number within a capacity interval. This is used to make all instances within an interval unique.
    /// </summary>
    internal readonly long SeqNbr;

    public override string ToString()
    {
        return string.Format("Start={0}; End={1}; Attention={2}; SeqNbr={3}; Act:{4}; Op:{5}; Job:{6}", DateTimeHelper.ToLongLocalTimeFromUTCTicks(StartTicks), DateTimeHelper.ToLongLocalTimeFromUTCTicks(EndTicks), Attention, SeqNbr, Activity.Id, Activity.Operation.ExternalId, Activity.Job.ExternalId);
    }
}

/// <summary>
/// Used to compare required attentions by StartTicks with ties broken by SeqNbr.
/// </summary>
internal class RequiredAttentionComparer : IComparer<RequiredAttention>
{
    public int Compare(RequiredAttention x, RequiredAttention y)
    {
        int c = Comparer<long>.Default.Compare(x.StartTicks, y.StartTicks);
        if (c == 0)
        {
            c = Comparer<long>.Default.Compare(x.SeqNbr, y.SeqNbr);
        }

        return c;
    }
}