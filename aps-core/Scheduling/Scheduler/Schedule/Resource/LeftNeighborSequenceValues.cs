using PT.Scheduler.Simulation;

namespace PT.Scheduler;

public struct LeftNeighborSequenceValues
{
    //TODO: Review how these constructors can remain private without removing customization functionality
    internal LeftNeighborSequenceValues(ResourceBlockList.Node a_node)
    {
        if (a_node != null)
        {
            BlockNode = a_node;
            Init(a_node.Data.Activity, a_node.Data.EndTicks);
            ScheduledCleanout = a_node.Data.Batch.CleanSpan;
        }
        else
        {
            //unitialized
        }
    }

    public LeftNeighborSequenceValues(InternalActivity a_act, long a_endTicks)
    {
        Init(a_act, a_endTicks);
        if (a_act.Batched)
        {
            ScheduledCleanout = a_act.Batch.CleanSpan;
            BlockNode = a_act.Batch.PrimaryResourceBlock.MachineBlockListNode;
        }
        else
        {
            ScheduledCleanout = RequiredSpanPlusClean.s_notInit;
        }
    }

    private void Init(InternalActivity a_act, long a_endTicks)
    {
        Activity = a_act;
        EndDate = a_endTicks;
        SetupNumber = Activity.Operation.SetupNumber;
        Attributes = Activity.Operation.Attributes;
        Initialized = true;
    }

    internal static LeftNeighborSequenceValues NullValues { get; } = new LeftNeighborSequenceValues(null);

    public InternalActivity Activity { get; private set; }
    public long EndDate { get; private set; }

    public string SetupCode { get; private set; }

    public decimal SetupNumber { get; private set; }

    public AttributesCollection Attributes { get; private set; }

    public RequiredSpanPlusClean ScheduledCleanout { get; private set; }

    public ResourceBlockList.Node BlockNode { get; private set; }

    public bool Initialized { get; internal set; }
}