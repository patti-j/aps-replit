using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Manages a sorted list of InternalResource objects that are stored according to their ResourceKeys (not their Id only).
/// </summary>
public class InternalResourceList
{
    public class ResourceKeyManagerException : ApplicationException
    {
        public ResourceKeyManagerException(string message)
            : base(message) { }
    }

    private readonly SortedList<ResourceKey, InternalResource> m_resources = new ();

    public InternalResource this[ResourceKey a_key] => m_resources[a_key];

    public bool Contains(ResourceKey a_key)
    {
        return m_resources.ContainsKey(a_key);
    }

    public InternalResource GetByIndex(int a_index)
    {
        return m_resources.Values[a_index];
    }

    public void Add(InternalResource a_m)
    {
        m_resources.Add(a_m.GetKey(), a_m);
    }

    public void Remove(ResourceKey a_key)
    {
        m_resources.Remove(a_key);
    }

    public int Count => m_resources.Count;
}