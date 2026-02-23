using PT.APSCommon;
using PT.Scheduler;

namespace PT.SchedulerData;

internal class MsProjectSortableOp : IComparable
{
    public MsProjectSortableOp(BaseOperation bOp)
    {
        scheduledStart = bOp.StartDateTime;
        opId = bOp.Id;
    }

    public DateTime scheduledStart;
    public BaseId opId;

    #region IComparable Members
    public int CompareTo(object obj)
    {
        MsProjectSortableOp c = (MsProjectSortableOp)obj;

        if (scheduledStart.Ticks < c.scheduledStart.Ticks || (scheduledStart.Ticks == c.scheduledStart.Ticks && opId.ToBaseType() < c.opId.ToBaseType()))
        {
            return -1;
        }

        if (scheduledStart.Ticks == c.scheduledStart.Ticks && opId.ToBaseType() == c.opId.ToBaseType())
        {
            return 0;
        }

        return 1;
    }
    #endregion
}