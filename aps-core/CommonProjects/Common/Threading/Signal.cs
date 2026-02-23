namespace PT.Common.Threading;

/// <summary>
/// Allows you to get and set a boolean value that is thread protected.
/// </summary>
public class Signal
{
    private bool m_signaledState;

    /// <summary>
    /// The set and get a thread protected boolean.
    /// </summary>
    public bool Signaled
    {
        get
        {
            lock (this)
            {
                return m_signaledState;
            }
        }

        set
        {
            lock (this)
            {
                m_signaledState = value;
            }
        }
    }
}