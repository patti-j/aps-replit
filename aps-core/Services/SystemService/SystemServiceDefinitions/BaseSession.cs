namespace PT.SystemServiceDefinitions;

public class BaseSession
{
    /// <summary>
    /// After calling the constructor, you must manually set the ConnectionNbr.
    /// </summary>
    public BaseSession(string a_sessionToken)
    {
        Description = ""; //TODO: Session
        m_sessionToken = a_sessionToken;

        lock (m_receiveTimingTicksLock)
        {
            m_receiveTiming.LastReceiveDate = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// All times and spans are in ticks.
    /// </summary>
    public struct ReceiveTiming
    {
        /// <summary>
        /// The last time the Receive function was called.
        /// </summary>
        public DateTime LastReceiveDate { get; set; }

        /// <summary>
        /// The total number of times the Recieve function was called.
        /// </summary>
        public int NbrOfReceiveCalls { get; set; }

        /// <summary>
        /// The maximum length of time between calls to the receive function.
        /// </summary>
        public TimeSpan MaxSpanBetweenReceivesTicks { get; set; }

        /// <summary>
        /// The maximum length of time it took to call the receive function.
        /// </summary>
        public TimeSpan MaxReceiveTicks { get; set; }
    }

    private ReceiveTiming m_receiveTiming;
    private readonly object m_receiveTimingTicksLock = new ();

    /// <summary>
    /// The last time the receive function was called.
    /// </summary>
    public DateTime LastReceiveDate
    {
        get
        {
            lock (m_receiveTimingTicksLock)
            {
                return m_receiveTiming.LastReceiveDate;
            }
        }
    }

    public ReceiveTiming ReceiveTimingMember
    {
        get
        {
            lock (m_receiveTimingTicksLock)
            {
                return m_receiveTiming;
            }
        }
    }

    public void NotifyLastReceiveCallStarted(DateTime a_nowUtc)
    {
        lock (m_receiveTimingTicksLock)
        {
            ++m_receiveTiming.NbrOfReceiveCalls;

            TimeSpan currentSpanBetweenReceiveCalls = a_nowUtc - m_receiveTiming.LastReceiveDate;
            if (currentSpanBetweenReceiveCalls > m_receiveTiming.MaxSpanBetweenReceivesTicks)
            {
                m_receiveTiming.MaxSpanBetweenReceivesTicks = currentSpanBetweenReceiveCalls;
            }

            m_receiveTiming.LastReceiveDate = a_nowUtc;
        }
    }

    public void NotifyReceiveCallFinished(DateTime a_utcReceiveCallStartDate)
    {
        lock (m_receiveTimingTicksLock)
        {
            TimeSpan spanOfReceiveCall = DateTime.UtcNow - a_utcReceiveCallStartDate;
            if (spanOfReceiveCall > m_receiveTiming.MaxReceiveTicks)
            {
                m_receiveTiming.MaxReceiveTicks = spanOfReceiveCall;
            }
        }
    }

    protected string m_sessionToken;

    /// <summary>
    /// A value that must be set after the Connection has been constructed. This in combination with the random id are used to help make
    /// sure connections are correctly identiifed by clients.
    /// </summary>
    public string SessionToken => m_sessionToken;

    private readonly DateTime m_creationTime = DateTime.UtcNow;

    /// <summary>
    /// The UTC Date ticks when this connection was created.
    /// The UTC DateTime ticks assigned when the connection is created. You can use this to help uniquely identify a connection. For example a connection can
    /// be identified by both its connection number and random id. Although the connection number is unique in the case where the broadcaster is restarted
    /// clients that were connected will refer to connection numbers that have become invalid. This number will insure the connection number is relates to the
    /// right instance of the broadcaster.
    /// </summary>
    public DateTime CreationDate => m_creationTime;

    private string m_description;

    internal string Description
    {
        get => m_description;
        set => m_description = value;
    }

    public TimeSpan ConnectionTimeout { get; set; }

    protected virtual BinaryMemoryWriter GenerateWriter()
    {
        return new BinaryMemoryWriter();
    }

    protected virtual BinaryMemoryReader GenerateReader(byte[] a_buffer)
    {
        return new BinaryMemoryReader(a_buffer);
    }

    protected static string GenerateNewToken()
    {
        return Guid.NewGuid().ToString();
    }
}