using PT.Scheduler.Simulation;

namespace PT.Scheduler.Schedule.Resource;

public class LeftActivityInfo
{
    private readonly InternalActivity m_act;
    public readonly long StartTicks;
    public readonly long EndTicks;
    public readonly long ProcessingSpanTicks;
    public readonly long PostProcessingSpanTicks;
    public readonly decimal RemainingQty;
    public readonly decimal QtyPerCycle;
    public readonly decimal PrimaryProductQty;
    public InternalActivity Activity => m_act;

    public LeftActivityInfo(InternalActivity a_act)
    {
        m_act = a_act;
        RemainingQty = m_act.RemainingQty;
        QtyPerCycle = m_act.ScheduledOrDefaultProductionInfo.QtyPerCycle;

        if (m_act.Finished)
        {
            StartTicks = m_act.ReportedStartDateTicks;
            EndTicks = m_act.ReportedFinishDateTicks - m_act.ReportedClean;
            ProcessingSpanTicks = m_act.ReportedRun;
            PostProcessingSpanTicks = m_act.ReportedPostProcessing;

            PrimaryProductQty = m_act.Operation.Products?.PrimaryProduct?.CompletedQty ?? 0m;
        }
        else
        {
            StartTicks = m_act.ReportedStartDateSet ? m_act.ReportedStartDateTicks : m_act.Batch.StartTicks;

            EndTicks = m_act.Batch.EndOfStorageTicks;

            ProcessingSpanTicks = m_act.Batch.ProcessingCapacitySpan.TimeSpanTicks;
            PostProcessingSpanTicks = m_act.Batch.PostProcessingSpan.TimeSpanTicks;

            PrimaryProductQty = m_act.PrimaryProductQty;
        }
    }

    public RequiredSpanPlusClean GetCleanSpan()
    {
        if (m_act.Finished)
        {
            return new RequiredSpanPlusClean(m_act.ReportedClean, false, m_act.ReportedCleanoutGrade);
        }
        
        return m_act.Batch.CleanSpan;
    }

    public InternalOperation GetOperation()
    {
        return m_act.Operation;
    }
}


