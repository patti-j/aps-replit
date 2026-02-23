using PT.SchedulerDefinitions;

namespace PT.Scheduler.Simulation;

/// <summary>
/// This is one of the RequiredSpanPlus classes.
/// In this case, it's a required span plus some extra information about setup.
/// </summary>
public struct RequiredSpanPlusClean : IRequiredSpan, IPTSerializable
{
    internal RequiredSpanPlusClean(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12536)
        {
            m_remainingCleanSpan = new RequiredSpan(a_reader);
            a_reader.Read(out m_cleanGrade);
            a_reader.Read(out m_resourceCleanoutCost);
            a_reader.Read(out m_productionCleanoutCost);
            a_reader.Read(out m_sequencedCleanoutCost);
        }
        else
        {
            m_remainingCleanSpan = new RequiredSpan(a_reader);
            a_reader.Read(out m_sequencedCleanoutCost);
            a_reader.Read(out m_cleanGrade);
        }
    }

    internal const int UNIQUE_ID = 981;
    public int UniqueId => UNIQUE_ID;

    public void Serialize(IWriter a_writer)
    {
        m_remainingCleanSpan.Serialize(a_writer);
        a_writer.Write(m_cleanGrade);
        a_writer.Write(m_resourceCleanoutCost);
        a_writer.Write(m_productionCleanoutCost);
        a_writer.Write(m_sequencedCleanoutCost);
    }

    private readonly RequiredSpan m_remainingCleanSpan;

    /// <summary>
    /// Init values.
    /// </summary>
    /// <param name="a_cleanTicks">The net setup ticks.</param>
    /// <param name="a_overrun"></param>
    /// <param name="a_cleanoutGrade"></param>
    public RequiredSpanPlusClean(long a_cleanTicks, bool a_overrun, int a_cleanoutGrade)
    {
        a_cleanTicks = Math.Max(a_cleanTicks, 0);
        m_remainingCleanSpan = new RequiredSpan(a_cleanTicks, a_overrun);
        m_cleanGrade = a_cleanoutGrade;
    }

    /// <summary>
    /// Create a new required span plus set up object from the actual required setup time and the setup time without subtracting
    /// value such as reports of time.
    /// </summary>
    /// <param name="a_netSuTicks">The amount of setup required.</param>
    /// the setup time before subtracting value such as reports of time.
    public RequiredSpanPlusClean(RequiredSpan a_netSuTicks)
    {
        m_remainingCleanSpan = a_netSuTicks;
        m_cleanGrade = -1;
        m_sequencedCleanoutCost = 0;
    }

    public RequiredSpanPlusClean(RequiredSpanPlusClean a_batchCleanSpan)
    {
        m_remainingCleanSpan = new RequiredSpan(a_batchCleanSpan);
        m_cleanGrade = a_batchCleanSpan.CleanoutGrade;
        m_resourceCleanoutCost = a_batchCleanSpan.ResourceCleanoutCost;
        m_productionCleanoutCost = a_batchCleanSpan.ProductionCleanoutCost;
        m_sequencedCleanoutCost = a_batchCleanSpan.SequencedCleanoutCost;
    }

    /// <summary>
    /// Merge two cleanouts
    /// This will return the cleanout with the highest grade followed by longest duration.
    /// </summary>
    /// <param name="a_cleanout"></param>
    /// <param name="a_keepLongest">Whether to keep the longest cleanout if the grades are the same</param>
    /// <returns></returns>
    internal RequiredSpanPlusClean Merge(RequiredSpanPlusClean a_cleanout, bool a_keepLongest)
    {
        //If Cleanout Grade is less than 0, it means the Cleanout span was overrun or not initialized
        if (a_cleanout.CleanoutGrade < 0)
        {
            return this;
        }

        if (CleanoutGrade > a_cleanout.CleanoutGrade)
        {
            return this;
        }

        if (CleanoutGrade < a_cleanout.CleanoutGrade)
        {
            return a_cleanout;
        }

        //Same grade, return based on duration. //TODO: have an option to return lowest cost
        if (TimeSpanTicks > a_cleanout.TimeSpanTicks)
        {
            if (a_keepLongest)
            {
                return this;
            }
        }

        return a_cleanout;
    }

    /// <summary>
    /// The amount of setup time that needs to be scheduled.
    /// </summary>
    public long TimeSpanTicks => m_remainingCleanSpan.TimeSpanTicks;

    public TimeSpan TimeSpan => TimeSpan.FromTicks(TimeSpanTicks);

    /// <summary>
    /// Whether there's no required time left for the in processs status, but the activity is still in the status.
    /// For instance, if an activities production status is setting up but there is more reported setup time and the total length of setup time required in the activity may be in the overrun state.
    /// The state in which no time is required but since the activities production status is done the state a small amount of time is scheduled for the status. At the time of this writing,
    /// that time was one minute.
    /// </summary>
    public bool Overrun => m_remainingCleanSpan.Overrun;

    private readonly int m_cleanGrade;
    public int CleanoutGrade => m_cleanGrade;

    private decimal m_resourceCleanoutCost;
    public decimal ResourceCleanoutCost => m_resourceCleanoutCost;

    private decimal m_productionCleanoutCost;
    public decimal ProductionCleanoutCost => m_productionCleanoutCost;
    private decimal m_sequencedCleanoutCost;
    public decimal SequencedCleanoutCost => m_sequencedCleanoutCost;

    public decimal CapacityCost => m_remainingCleanSpan.CapacityCost;

    private InternalActivity m_activityToIncurClean = null;
    public InternalActivity ActivityToIncurClean
    {
        get => m_activityToIncurClean;
        set => m_activityToIncurClean = value;
    }

    internal readonly void SetCapacityCost(decimal a_capacityCost)
    {
        m_remainingCleanSpan.SetCapacityCost(a_capacityCost);
    }

    internal void SetStaticCosts(decimal a_sequencedCleanoutCost, decimal a_operationCleanoutCost, decimal a_resourceCleanoutCost)
    {
        m_resourceCleanoutCost = a_sequencedCleanoutCost;
        m_productionCleanoutCost = a_operationCleanoutCost;
        m_sequencedCleanoutCost = a_resourceCleanoutCost;
    }

    internal void SetActivityToIncurKey(InternalActivity a_act)
    {
        ActivityToIncurClean = a_act;
    }

    internal decimal GetTotalCost()
    {
        decimal totalCost = 0m;
        if (m_sequencedCleanoutCost > 0)
        {
            totalCost += m_sequencedCleanoutCost;
        }

        if (m_productionCleanoutCost > 0)
        {
            totalCost += m_productionCleanoutCost;
        }

        if (m_resourceCleanoutCost > 0)
        {
            totalCost += m_resourceCleanoutCost;
        }

        if (m_remainingCleanSpan.CapacityCost > 0)
        {
            totalCost += m_remainingCleanSpan.CapacityCost;
        }

        return totalCost;
    }

    public static implicit operator RequiredSpan(RequiredSpanPlusClean a_rs)
    {
        return a_rs.m_remainingCleanSpan;
    }

    public static bool operator ==(RequiredSpanPlusClean x, RequiredSpanPlusClean y)
    {
        RequiredSpan xx = x;
        RequiredSpan yy = y;

        return xx == yy 
               && x.m_sequencedCleanoutCost == y.m_sequencedCleanoutCost 
               && x.m_productionCleanoutCost == y.m_productionCleanoutCost
               && x.m_resourceCleanoutCost == y.m_resourceCleanoutCost
               && x.m_cleanGrade == y.m_cleanGrade;
    }

    public static bool operator !=(RequiredSpanPlusClean x, RequiredSpanPlusClean y)
    {
        return !(x == y);
    }

    public override bool Equals(object obj)
    {
        return obj is RequiredSpanPlusClean span && this == span;
    }

    public override int GetHashCode()
    {
        return TimeSpanTicks.GetHashCode();
    }

    public override string ToString()
    {
        return "Span=" + DateTimeHelper.PrintTimeSpan(TimeSpanTicks) + "; Overrun Length=" + Overrun + "; ResourceCleanoutCost=" + ResourceCleanoutCost + "; ProductionCleanoutCost=" + ProductionCleanoutCost + "; SequencedCleanoutCost=" + SequencedCleanoutCost;
    }

    internal static readonly RequiredSpanPlusClean s_overrun = new (RequiredSpan.OverrunRequiredSpan);

    // This should be used to represent an uninitialized RequiredSpanPlusSetup. If you need to have an initialized object, create a copy of this. This should also be used for comparison puposes.
    public static readonly RequiredSpanPlusClean s_notInit = new (RequiredSpan.NotInit);
}