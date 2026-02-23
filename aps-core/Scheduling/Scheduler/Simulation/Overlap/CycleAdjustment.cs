namespace PT.Scheduler;

/// <summary>
/// Basic information about a cycle such when it ends, the cycle number within a block, and the quantity produced within the cycle.
/// </summary>
public class CycleAdjustment
{
    internal CycleAdjustment(int a_cycle, long a_date, decimal a_qty)
    {
        Date = a_date;
        Cycle = a_cycle;
        Qty = a_qty;
    }

    /// <summary>
    /// The number of the cycle within the block.
    /// </summary>
    public readonly int Cycle;

    /// <summary>
    /// When the cycle ends.
    /// </summary>
    public readonly long Date;

    /// <summary>
    /// The quantity produced within the cycle.
    /// </summary>
    public decimal Qty { get; internal set; }

    public override string ToString()
    {
        return string.Format("Cycle {0}; Date {1}; Qty {2}", Cycle, Date, Qty);
    }
}