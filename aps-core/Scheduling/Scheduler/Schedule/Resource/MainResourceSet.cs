using System.Collections;

namespace PT.Scheduler;

internal class MainResourceSet
{
    private readonly List<InternalResource> m_mainResources = new ();

    internal int Count => m_mainResources.Count;

    internal void Clear()
    {
        m_mainResources.Clear();
    }

    internal void Add(InternalResource resource)
    {
        m_mainResources.Add(resource);
    }

    internal InternalResource this[int index] => m_mainResources[index];

    /// <summary>
    /// Creates a Hashtable containing all the resources in this MainResourceSet.
    /// </summary>
    /// <returns></returns>
    internal Hashtable CreateHash()
    {
        Hashtable ht = new ();

        for (int i = 0; i < Count; ++i)
        {
            ht.Add(this[i], null);
        }

        return ht;
    }

    public override string ToString()
    {
        System.Text.StringBuilder sb = new ();
        sb.Append(string.Format("Count={0}", Count));

        if (Count < 4)
        {
            for (int i = 0; i < Count; ++i)
            {
                sb.AppendFormat("; {0}", this[i].Name);
            }
        }

        return sb.ToString();
    }

    #region Testing
    /// <summary>
    /// Whether the set contains the resource with a specific id.
    /// </summary>
    /// <param name="id">The base id of a resource.</param>
    /// <returns></returns>
    internal bool Contains(long id)
    {
        for (int i = 0; i < Count; ++i)
        {
            if (this[i].Id == id)
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}