namespace PT.Common;

/// <summary>
/// Calls a delegate when dispose is run. This class is intended to be used within a using clause to execute some delegate when the clause completes.
/// </summary>
public class AutoDisposer : IDisposable
{
    private readonly Action m_delegate;
    private bool m_disposed;

    /// <summary>
    /// Specify the delegate to call.
    /// </summary>
    /// <param name="a_f">The delegate.</param>
    public AutoDisposer(Action a_f)
    {
        m_delegate = a_f;
    }

    /// <summary>
    /// Call the delegate if nto already called
    /// </summary>
    public void Dispose()
    {
        if (!m_disposed)
        {
            m_delegate();
            m_disposed = true;
        }
    }
}