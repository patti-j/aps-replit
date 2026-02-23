namespace PT.Scheduler;

/// <summary>
/// Container for a dispatcher element's key and object.
/// </summary>
public class KeyAndActivity
{
    public KeyAndActivity(IDispatchKey a_key, InternalActivity a_activity)
    {
        m_key = a_key;
        m_activity = a_activity;
    }

    private readonly IDispatchKey m_key;
    internal IDispatchKey Key => m_key;

    private readonly InternalActivity m_activity;
    internal InternalActivity Activity => m_activity;

    public override string ToString()
    {
        string s;

        s = GetType().Name + "::" + m_key + "; " + m_activity;

        return s;
    }
}

public interface IDispatchKey
{
    decimal Score { get => 0m; } 
}