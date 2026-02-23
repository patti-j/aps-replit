namespace PT.Scheduler.Schedule;

public interface ICalculatedValueCache
{
    /// <summary>
    /// Whether the cache can be used. This will be disabled during scheduling.
    /// </summary>
    bool Enabled { get; set; }

    void Initialize();
}

public interface ICalculatedValueCache<T> : ICalculatedValueCache
{
    /// <summary>
    /// Try to return the cached value
    /// </summary>
    /// <param name="a_value"></param>
    /// <returns>The weather the cache is enabled and the value is stored</returns>
    bool TryGetValue(out T a_value);

    bool CacheValue(T a_value);

    void ClearCache();
}

internal interface ICalculatedValueCacheManager
{
    /// <summary>
    /// Returns a new initialized cache for use until the next simulation start
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    void InitializeCache<T>(ref ICalculatedValueCache<T> a_existingCache);
}

public class CalculatedValueCache<T> : ICalculatedValueCache<T>
{
    public bool Enabled { get; set; }
    private bool m_valueSet;
    private T m_value;

    internal CalculatedValueCache()
    {
        m_value = default(T);
    }

    public void Initialize()
    {
        Enabled = false;
        ClearCache();
    }

    public bool CacheValue(T a_value)
    {
        if (Enabled)
        {
            m_value = a_value;
            m_valueSet = true;
        }

        return Enabled;
    }

    public void ClearCache()
    {
        m_valueSet = false;
        m_value = default(T);
    }

    public bool TryGetValue(out T a_value)
    {
        a_value = m_value;
        return Enabled && m_valueSet;
    }
}