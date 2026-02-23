using PT.SchedulerDefinitions;

namespace PT.Scheduler.Simulation;

/// <summary>
/// This is one of the RequiredSpanPlus classes.
/// In this case, it's a required span plus some extra information about setup.
/// </summary>
public struct RequiredSpanPlusSetup : IRequiredSpan, IPTSerializable
{
    internal RequiredSpanPlusSetup(IReader reader)
    {
        if (reader.VersionNumber >= 12510)
        {
            m_remainingSetupTicks = new RequiredSpan(reader);
            m_grossSuTicks = new RequiredSpan(reader);
            reader.Read(out m_resourceSetupCost);
            reader.Read(out m_productionSetupCost);
            reader.Read(out m_sequencedSetupCost);
        }
        else
        {
            m_remainingSetupTicks = new RequiredSpan(reader);
            m_grossSuTicks = new RequiredSpan(reader);
            reader.Read(out m_sequencedSetupCost);
        }
    }

    internal const int UNIQUE_ID = 762;

    public int UniqueId => UNIQUE_ID;

    public void Serialize(IWriter a_writer)
    {
        m_remainingSetupTicks.Serialize(a_writer);
        m_grossSuTicks.Serialize(a_writer);
        a_writer.Write(m_resourceSetupCost);
        a_writer.Write(m_productionSetupCost);
        a_writer.Write(m_sequencedSetupCost);
    }

    private RequiredSpan m_remainingSetupTicks;

    /// <summary>
    /// Init values.
    /// </summary>
    /// <param name="a_totalSetupTicks">The net setup ticks.</param>
    /// <param name="a_overrun"></param>
    /// <param name="a_grossSetupTicks"></param>
    public RequiredSpanPlusSetup(long a_totalSetupTicks, bool a_overrun, long a_grossSetupTicks)
    {
        a_totalSetupTicks = Math.Max(a_totalSetupTicks, 0);
        m_remainingSetupTicks = new RequiredSpan(a_totalSetupTicks, a_overrun);
        m_grossSuTicks = new RequiredSpan(a_grossSetupTicks, false);
    }

    public RequiredSpanPlusSetup(RequiredSpanPlusSetup a_batchSetupCapacitySpan)
    {
        m_remainingSetupTicks = new RequiredSpan(a_batchSetupCapacitySpan);
        m_grossSuTicks = a_batchSetupCapacitySpan.GrossSetupTicks;
        m_productionSetupCost = a_batchSetupCapacitySpan.ProductionSetupCost;
        m_sequencedSetupCost = a_batchSetupCapacitySpan.SequencedSetupCost;
        m_resourceSetupCost = a_batchSetupCapacitySpan.SequencedSetupCost;
        m_remainingSetupTicks.SetCapacityCost(a_batchSetupCapacitySpan.CapacityCost);
    }

    /// <summary>
    /// Used for backcalculating with a different duration than the original setup span.
    /// </summary>
    /// <param name="a_batchSetupCapacitySpan"></param>
    /// <param name="a_durationOverride"></param>
    public RequiredSpanPlusSetup(RequiredSpanPlusSetup a_batchSetupCapacitySpan, long a_durationOverride)
    {
        m_remainingSetupTicks = new RequiredSpan(a_durationOverride, a_batchSetupCapacitySpan.Overrun);
        m_grossSuTicks = a_batchSetupCapacitySpan.GrossSetupTicks;
        m_productionSetupCost = a_batchSetupCapacitySpan.ProductionSetupCost;
        m_sequencedSetupCost = a_batchSetupCapacitySpan.SequencedSetupCost;
        m_resourceSetupCost = a_batchSetupCapacitySpan.SequencedSetupCost;
        m_remainingSetupTicks.SetCapacityCost(a_batchSetupCapacitySpan.CapacityCost);
    }

    /// <summary>
    /// The amount of setup time that needs to be scheduled.
    /// </summary>
    public long TimeSpanTicks => m_remainingSetupTicks.TimeSpanTicks;

    public TimeSpan TimeSpan => TimeSpan.FromTicks(m_remainingSetupTicks.TimeSpanTicks);

    /// <summary>
    /// Whether there's no required time left for the in processs status, but the activity is still in the status.
    /// For instance, if an activities production status is setting up but there is more reported setup time and the total length of setup time required in the activity may be in the overrun state.
    /// The state in which no time is required but since the activities production status is done the state a small amount of time is scheduled for the status. At the time of this writing,
    /// that time was one minute.
    /// </summary>
    public bool Overrun => m_remainingSetupTicks.Overrun;

    private RequiredSpan m_grossSuTicks;

    /// <summary>
    /// This is a gross setup time.Note overrun is always false since reported setup isn't subtracted from gross.
    /// The amount of setup that was or is required; this number is not reduced by the reported setup time.
    /// This is just a setup time without reportedSetupTime being subtracted from it. This may be of use to customizations that need to
    /// know the amount of setup that was required before subtracting the reported setup time. It was added because it's not possible to
    /// calculate this value by adding the required setup time and reported setup time since the required setup time has a minimum value of 0.
    /// For instance:
    /// If the setup time was 1 hour and the reported setup time was 2 hours, then the
    /// required setup time will be 0 hours, not -1 hours and it's impossible for a customization to
    /// recalculate what the setup time would have been by adding the required setup time and reported setup time.
    /// </summary>
    public RequiredSpan GrossSetupTicks
    {
        get => m_grossSuTicks;
        private set => m_grossSuTicks = value;
    }

    private decimal m_resourceSetupCost;
    public decimal ResourceSetupCost => m_resourceSetupCost;

    private decimal m_productionSetupCost;
    public decimal ProductionSetupCost => m_productionSetupCost;

    private decimal m_sequencedSetupCost;
    public decimal SequencedSetupCost => m_sequencedSetupCost;

    public decimal CapacityCost => m_remainingSetupTicks.CapacityCost;


    public static implicit operator RequiredSpan(RequiredSpanPlusSetup a_rs)
    {
        return a_rs.m_remainingSetupTicks;
    }

    internal readonly void SetCapacityCost(decimal a_capacityCost)
    {
        m_remainingSetupTicks.SetCapacityCost(a_capacityCost);
    }

    internal void SetStaticCosts(decimal a_sequencedSetupCost, decimal a_operationSetupCost, decimal a_resourceSetupCost)
    {
        m_sequencedSetupCost = a_sequencedSetupCost;
        m_productionSetupCost = a_operationSetupCost;
        m_resourceSetupCost = a_resourceSetupCost;
    }

    internal decimal GetTotalCost()
    {
        decimal totalCost = 0m;
        if (m_sequencedSetupCost > 0)
        {
            totalCost += m_sequencedSetupCost;
        }

        if (m_productionSetupCost > 0)
        {
            totalCost += m_productionSetupCost;
        }

        if (m_resourceSetupCost > 0)
        {
            totalCost += m_resourceSetupCost;
        }

        if (m_remainingSetupTicks.CapacityCost > 0)
        {
            totalCost += m_remainingSetupTicks.CapacityCost;
        }

        return totalCost;
    }

    internal void MergeSetup(RequiredSpanPlusSetup a_setupToMerge, bool a_consecutive)
    {
        long newSetupTicks;
        if (a_consecutive)
        {
            newSetupTicks = m_remainingSetupTicks.TimeSpanTicks + a_setupToMerge.TimeSpanTicks;
        }
        else
        {
            newSetupTicks = Math.Max(m_remainingSetupTicks.TimeSpanTicks, a_setupToMerge.TimeSpanTicks);
        }

        m_remainingSetupTicks = m_grossSuTicks = new RequiredSpan(newSetupTicks, false);
    }

    internal void SetOverrun(InternalActivityDefs.productionStatuses a_actProductionStatus)
    {
        if (a_actProductionStatus == InternalActivityDefs.productionStatuses.SettingUp && m_remainingSetupTicks.TimeSpanTicks <= 0)
        {
            m_remainingSetupTicks.Overrun = true;
        }
    }

    public static bool operator ==(RequiredSpanPlusSetup x, RequiredSpanPlusSetup y)
    {
        RequiredSpan xx = x;
        RequiredSpan yy = y;

        return xx == yy 
               && x.m_productionSetupCost == y.m_productionSetupCost
               && x.m_sequencedSetupCost == y.m_sequencedSetupCost
               && x.m_resourceSetupCost == y.m_resourceSetupCost;
    }

    public static bool operator !=(RequiredSpanPlusSetup x, RequiredSpanPlusSetup y)
    {
        return !(x == y);
    }

    public override bool Equals(object obj)
    {
        return obj is RequiredSpanPlusSetup span && this == span;
    }

    public override int GetHashCode()
    {
        return TimeSpanTicks.GetHashCode();
    }

    public override string ToString()
    {
        return $"Span={DateTimeHelper.PrintTimeSpan(TimeSpanTicks)}; Overrun Length={Overrun}; ResourceSetupCost={ResourceSetupCost}; ProductionSetupCost={ProductionSetupCost}; SequencedSetupCost={SequencedSetupCost}";
    }

    internal static readonly RequiredSpanPlusSetup s_overrun = new (RequiredSpan.OverrunRequiredSpan.TimeSpanTicks, true, RequiredSpan.OverrunRequiredSpan.TimeSpanTicks);

    // This should be used to represent an unitialized RequiredSpanPlusSetup. If you need to have an initialized object, create a copy of this. This should also be used for comparison puposes.
    internal static readonly RequiredSpanPlusSetup s_notInit = new (RequiredSpan.NotInit.TimeSpanTicks, false, RequiredSpan.NotInit.TimeSpanTicks);
}