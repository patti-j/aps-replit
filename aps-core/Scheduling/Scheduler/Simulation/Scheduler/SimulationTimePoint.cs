namespace PT.Scheduler;

/// <summary>
/// This class represents a time on the schedule. It includes non-fixed time points such as the FrozenSpan or StableSpan.
/// </summary>
internal class SimulationTimePoint
{
    /// <summary>
    /// DateTimeTicks will always be returned when this constructor is used.
    /// </summary>
    /// <param name="a_specificDateTime"></param>
    internal SimulationTimePoint(long a_specificDateTime)
    {
        DateTimeTicks = a_specificDateTime;
    }

    internal SimulationTimePoint(ScenarioDetail a_sd, long a_defaultDateTime, OptimizeSettings.ETimePoints a_startTime)
    {
        m_clock = a_sd.Clock;
        switch (a_startTime)
        {
            case OptimizeSettings.ETimePoints.CurrentPTClock:
                DateTimeTicks = m_clock;
                break;
            case OptimizeSettings.ETimePoints.EndOfFrozenZone:
                DateTimeTicks = a_defaultDateTime;
                m_useFrozenZone = true;
                break;
            case OptimizeSettings.ETimePoints.EndOfStableZone:
                DateTimeTicks = a_defaultDateTime;
                m_useStableZone = true;
                break;
            case OptimizeSettings.ETimePoints.EndOfShortTerm:
                DateTimeTicks = a_sd.GetEndOfShortTerm().Ticks;
                break;
            case OptimizeSettings.ETimePoints.SpecificDateTime:
                DateTimeTicks = a_defaultDateTime;
                break;
            case OptimizeSettings.ETimePoints.EndOfPlanningHorizon:
                DateTimeTicks = a_sd.EndOfPlanningHorizon;
                break;
            case OptimizeSettings.ETimePoints.EntireSchedule:
                long maxBlockEnd = a_sd.PlantManager.GetResourceList().Where(r => r.Blocks.Count > 0).Max(r => r.Blocks.Last.Data.EndTicks);
                DateTimeTicks = maxBlockEnd;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(a_startTime), a_startTime, null);
        }
    }

    /// <summary>
    /// Typically set to the start of the optimize start time. If it's based on the Frozen Span, the SystemOptions.Frozen is used, not the department level frozen spans.
    /// </summary>
    internal long DateTimeTicks { get; }

    private readonly long m_clock;
    private readonly bool m_useFrozenZone;
    private readonly bool m_useStableZone;

    internal long GetTimeForResource(InternalResource a_res)
    {
        if (m_useFrozenZone)
        {
            return CalcEndOfFrozenSpan(a_res);
        }

        if (m_useStableZone)
        {
            return CalcEndOfStableSpan(a_res);
        }

        return DateTimeTicks;
    }

    private long CalcEndOfFrozenSpan(InternalResource a_res)
    {
        if (a_res != null)
        {
            return m_clock + a_res.Department.FrozenSpanTicks;
        }

        return DateTimeTicks;
    }

    private long CalcEndOfStableSpan(InternalResource a_res)
    {
        return CalcEndOfFrozenSpan(a_res) + a_res.Plant.StableSpanTicks;
    }

    public bool Equals(SimulationTimePoint a_sst)
    {
        if (a_sst != null)
        {
            if (ReferenceEquals(this, a_sst))
            {
                return true;
            }

            if (m_clock == a_sst.m_clock && DateTimeTicks == a_sst.DateTimeTicks && m_useFrozenZone == a_sst.m_useFrozenZone && m_useStableZone == a_sst.m_useStableZone)
            {
                return true;
            }
        }

        return false;
    }

    public override bool Equals(object obj)
    {
        if (obj is SimulationTimePoint)
        {
            return Equals((SimulationTimePoint)obj);
        }

        return false;
    }

    public override string ToString()
    {
        string msg = string.Format("OptimizeDefaultStartTicks={0}; UseFrozenSpan={1}; UseStableSpan={2}; ", DateTimeHelper.ToLocalTimeFromUTCTicks(DateTimeTicks), m_useFrozenZone, m_useStableZone);
        if (m_clock > 0)
        {
            msg += "Clock=" + DateTimeHelper.ToLocalTimeFromUTCTicks(m_clock);
        }

        return msg;
    }
}