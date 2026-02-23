namespace PT.Common;

/// <summary>
/// Summary description for AutoDisposer.
/// </summary>
public class AutoExiter : IDisposable
{
    public static readonly int THREAD_TRY_WAIT_MS = 2000; //milliseconds
    public static readonly int UiDefaultWait = 1000; //milliseconds
    public static readonly int UiQuickWait = 100; //milliseconds

    private readonly object m_o;
    private readonly LockSharer m_s;

    public AutoExiter(object a_o)
    {
        m_o = a_o;
    }

    public AutoExiter(object a_o, LockSharer a_s)
    {
        m_o = a_o;
        m_s = a_s;
    }

    public void Dispose()
    {
        if (m_s == null || !m_s.Sharing)
        {
            Monitor.Exit(m_o);
        }
    }

    public static void WaitForLock(int a_numOfAttempts)
    {
        a_numOfAttempts = Math.Max(1, a_numOfAttempts);
        Thread.Sleep(Math.Min(a_numOfAttempts * 2, 5000));
    }
}