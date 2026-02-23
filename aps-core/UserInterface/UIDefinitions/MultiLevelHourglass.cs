using System.Windows.Forms;

using PT.Common.Extensions;

namespace PT.UIDefinitions;

/// <summary>
/// Keeps track of how many nested calls have been made to show the hourglass and turns it on or off as needed.
/// </summary>
public class MultiLevelHourglass : IDisposable
{
    private static int s_level; //indicates the number of functions that have cumulatively called for the hourglass at any point in time.
    private static readonly object s_levelLock = new (); //Make threadsafe
    private static readonly HashSet<int> s_idSet = new ();
    public static event Action<bool> UseWaitCursor;
    private System.Timers.Timer m_delayTimer;
    private bool m_incrementedThisInstance;
    private readonly object m_timerLock;

    public MultiLevelHourglass()
    {
        m_incrementedThisInstance = true;
        IncrementWaitCursor();
    }

    /// <summary>
    /// Show the hourglass after a specified delay
    /// If the hourglass is disposed before the delay, no cursor will be shown
    /// </summary>
    /// <param name="a_delay"></param>
    public MultiLevelHourglass(TimeSpan a_delay)
    {
        m_timerLock = new object();
        if (a_delay > TimeSpan.Zero)
        {
            m_delayTimer = new System.Timers.Timer(a_delay.TotalMilliseconds);
            m_delayTimer.AutoReset = false;
            m_delayTimer.Elapsed += (a_sender, a_args) =>
            {
                lock (m_timerLock)
                {
                    if (m_delayTimer == null)
                    {
                        //We disposed before the event was processed;
                        return;
                    }

                    IncrementWaitCursor();
                    m_incrementedThisInstance = true;
                }
            };
            m_delayTimer.Start();
        }
        else
        {
            IncrementWaitCursor();
            m_incrementedThisInstance = true;
        }
    }

    public void Dispose()
    {
        //Dispose timer if in use
        if (m_timerLock != null)
        {
            lock (m_timerLock)
            {
                m_delayTimer?.Stop();
                m_delayTimer?.Dispose();
                m_delayTimer = null;
            }
        }

        //Only decrement if it was incremented. It may not be if the delay timer did not elapse
        if (m_incrementedThisInstance)
        {
            DecrementWaitCursor(false);
        }
    }

    [Obsolete("Use using block or pass in unique int ID")]
    public static void TurnOn()
    {
        IncrementWaitCursor();
    }

    public static void TurnOn(int a_id)
    {
        lock (s_levelLock)
        {
            if (!s_idSet.AddIfNew(a_id))
            {
                return; //this Id was already turned on
            }
        }

        IncrementWaitCursor();
    }

    [Obsolete("Use using block or pass in unique int ID")]
    public static void TurnOff(bool a_forceOff = false)
    {
        DecrementWaitCursor(a_forceOff);
    }

    public static void TurnOff(int a_id)
    {
        lock (s_levelLock)
        {
            bool removed = s_idSet.Remove(a_id);
            #if TEST
                if (!removed)
                {
                    throw new DebugException("HourGlass wait cursor out of sync");
                }
            #endif
        }

        DecrementWaitCursor(false);
    }

    private static void IncrementWaitCursor()
    {
        lock (s_levelLock)
        {
            s_level++;
            if (Cursor.Current != System.Windows.Forms.Cursors.WaitCursor)
            {
                Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
            }

            if (s_level == 1)
            {
                UseWaitCursor?.Invoke(true);
            }
        }
    }

    private static void DecrementWaitCursor(bool a_forceOff)
    {
        lock (s_levelLock)
        {
            s_level--;
            #if TEST
                if (s_level < 0)
                {
                    throw new DebugException("HourGlass wait cursor out of sync");
                }
            #endif

            if (s_level <= 0 || a_forceOff)
            {
                Cursor.Current = System.Windows.Forms.Cursors.Default;
                UseWaitCursor?.Invoke(false);
            }
        }
    }

    /// <summary>
    /// Makes sure the cursor is in the right state based on the IsOn value.
    /// </summary>
    public static void VerifyCursor()
    {
        lock (s_levelLock)
        {
            if (s_level > 0)
            {
                if (Cursor.Current != System.Windows.Forms.Cursors.WaitCursor)
                {
                    Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
                }
            }
            else if (Cursor.Current == System.Windows.Forms.Cursors.WaitCursor)
            {
                Cursor.Current = System.Windows.Forms.Cursors.Default;
            }

            UseWaitCursor?.Invoke(s_level > 0);
        }
    }
}