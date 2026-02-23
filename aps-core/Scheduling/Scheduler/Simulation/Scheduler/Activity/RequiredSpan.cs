namespace PT.Scheduler.Simulation;

public class RequiredSpan : IPTSerializable, IRequiredSpan
{
    #region Serialization
    public RequiredSpan(IReader reader)
    {
        if (reader.VersionNumber >= 12510)
        {
            reader.Read(out m_ticks);
            reader.Read(out m_overrun);
            reader.Read(out m_capacityCost);
        }
        else
        {
            reader.Read(out m_ticks);
            reader.Read(out m_overrun);
        }
    }

    /// <summary>
    /// Creating new RequiredSpan whose values are equal to another RequiredSpan .
    /// </summary>
    /// <param name="a_rs"></param>
    public RequiredSpan(RequiredSpan a_rs)
    {
        m_ticks = a_rs.m_ticks;
        m_overrun = a_rs.Overrun;
        m_capacityCost = a_rs.m_capacityCost;
    }

    public void Serialize(IWriter writer)
    {
        writer.Write(TimeSpanTicks);
        writer.Write(Overrun);
        writer.Write(CapacityCost);
    }

    internal const int UNIQUE_ID = 761;

    public int UniqueId => UNIQUE_ID;
    #endregion

    public RequiredSpan()
    {
        m_ticks = NotInit.m_ticks;
        m_overrun = NotInit.m_overrun;
        m_capacityCost = NotInit.m_capacityCost;
    }

    /// <summary>
    /// Create a RequiredSPan specifying its values.
    /// </summary>
    /// <param name="a_timeSpanTicks">A length of time in ticks. This will b eoverridden if it's overrun.</param>
    /// <param name="a_overrun">
    /// whether the production status has overrun it's expected time. In the case of overrun ticks is set to Resource.OverrunTicks, which is the length of time an overrun producation
    /// state will be scheduled for.
    /// </param>
    public RequiredSpan(long a_timeSpanTicks, bool a_overrun)
    {
        if (a_overrun)
        {
            m_ticks = BaseResource.OverrunTicks;
        }
        else
        {
            m_ticks = a_timeSpanTicks;
        }

        m_overrun = a_overrun;
    }

    private long m_ticks;

    public long TimeSpanTicks
    {
        get => m_ticks;
        set => m_ticks = value;
    }

    public TimeSpan TimeSpan => TimeSpan.FromTicks(m_ticks);

    private bool m_overrun;

    /// <summary>
    /// Zero length means an activities segment production status and reported values might be out of sync. That is the reported values exceed the expected times, so 0 time is required.
    /// But since the status indicates still indicates the segment is still in process, the segment must still be scheduled. 0 time can't be scheduled, so a very small time is stored
    /// in place of 0 and this flag is set.
    /// It's only true when a combination of ProductionStatus and Reported cause there to be no time required of the  production state but the activity is still in the state.
    /// For instance:
    /// ProductionStatus == setup but it's take longer than expected. Reported setup exceeds the usual setup time.
    /// ProductionStatus == InProcess but it's taking longer than planned or
    /// more product is being produced than is scheduled. Reported time and or reported quantity exceed what's needed.
    /// ProductionStatus == PostProcessing bit it's taking longer than expected.
    /// In all cases the activities segment will still schedule for a tiny amount of time even though the calculated amount of time remaining is 0.
    /// </summary>
    public bool Overrun
    {
        get => m_overrun;
        set => m_overrun = value;
    }

    private decimal m_capacityCost;
    public decimal CapacityCost => m_capacityCost;

    public void SetCapacityCost(decimal a_capacityCost)
    {
        m_capacityCost = a_capacityCost;
    }

    public static bool operator ==(RequiredSpan x, RequiredSpan y)
    {
        return x.TimeSpanTicks == y.TimeSpanTicks && x.Overrun == y.Overrun;
    }

    public static bool operator !=(RequiredSpan x, RequiredSpan y)
    {
        return !(x == y);
    }

    public override bool Equals(object obj)
    {
        if (!(obj is RequiredSpan))
        {
            return false;
        }

        return this == (RequiredSpan)obj;
    }

    public override int GetHashCode()
    {
        return TimeSpanTicks.GetHashCode();
    }

    public override string ToString()
    {
        return "Span=" + DateTimeHelper.PrintTimeSpan(TimeSpanTicks) + " Zero Length=" + Overrun;
    }

    public static readonly RequiredSpan OverrunRequiredSpan = new (BaseResource.OverrunTicks, true);
    public static readonly RequiredSpan NotInit = new (0, false);
}