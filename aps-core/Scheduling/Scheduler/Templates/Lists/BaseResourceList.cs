using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Summary description for BaseResourceList.
/// </summary>
public class BaseResourceSortedList
{
    private readonly SortedList<ResourceKey, BaseResource> m_resources = new ();

    public void Add(BaseResource resource)
    {
        m_resources.Add(resource.GetKey(), resource);
    }

    public int Count => m_resources.Count;

    public BaseResource this[int index] => m_resources.Values[index];

    public bool Contains(BaseResource resource)
    {
        return m_resources.ContainsKey(resource.GetKey());
    }

    /// <summary>
    /// Exception thrown if Resource is not found.  Use Contains() if necessary.
    /// </summary>
    public BaseResource Find(BaseResource resource)
    {
        return m_resources[resource.GetKey()];
    }

    public void Remove(BaseResource resource)
    {
        m_resources.Remove(resource.GetKey());
    }
}