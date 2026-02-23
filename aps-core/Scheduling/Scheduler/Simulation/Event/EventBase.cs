namespace PT.Scheduler.Simulation.Events;

/// <summary>
/// Base class for simulation events.
/// </summary>
public abstract class EventBase : ICloneable
{
//#if DEBUG
//        static int zs_uniqueId;
//        static object zs_lock = new object();
//        int m_deUniqueId;
//        /// <summary>
//        /// This is only for debugging purposes. It's a unique id. Each instance of an event is assigned a unique is when it's created.
//        /// </summary>
//        internal int zdeUniqueId
//        {
//            get
//            {
//                return m_deUniqueId;
//            }
//        }

//#else
//#endif
    protected EventBase(long a_time)
    {
        m_time = a_time;
//#if DEBUG
//            lock (zs_lock)
//            {
//                m_deUniqueId = ++zs_uniqueId;
//            }
//#endif
    }

    private long m_time;

    /// <summary>
    /// The time in ticks that the event takes place.
    /// </summary>
    public long Time
    {
        get => m_time;
        set => m_time = value;
    }

    private bool m_cancelled;

    /// <summary>
    /// Whether to ignore this event; not process it.
    /// </summary>
    internal virtual bool Cancelled
    {
        get => m_cancelled;

        set => m_cancelled = value;
    }

    internal abstract int UniqueId { get; }

    internal string deTime => DateTimeHelper.ToLocalTimeFromUTCTicks(Time).ToString();

    public override string ToString()
    {
        #if DEBUG
        //return "Event Unique Id=" + m_deUniqueId + ".. " + base.ToString() + ": " + deTime;
        #else
        #endif
        return base.ToString() + ": " + deTime;
    }

    public virtual object Clone()
    {
        throw new NotImplementedException();
    }
}