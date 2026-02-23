using System.Collections;

using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

public class EligibleResourceSet : IEnumerable<EligibleResourceSet.ResourceData>, IEquatable<EligibleResourceSet>
{
    internal EligibleResourceSet() { }

    /// <summary>
    /// Creates a deep copy.
    /// </summary>
    /// <param name="original"></param>
    internal EligibleResourceSet(EligibleResourceSet a_original)
    {
        for (int i = 0; i < a_original.m_resources.Count; ++i)
        {
            if (!a_original.m_resources[i].m_delete)
            {
                m_resources.Add(new ResourceData(a_original.m_resources[i]));
            }
        }
    }

    public class ResourceData : IEquatable<ResourceData>
    {
        internal ResourceData(InternalResource a_res)
        {
            m_res = a_res;
        }

        /// <summary>
        /// Create a deep copy.
        /// </summary>
        /// <param name="rd"></param>
        internal ResourceData(ResourceData a_rd)
        {
            m_res = a_rd.m_res;
        }

        internal bool m_delete;
        internal InternalResource m_res;
        internal int m_successorResourceConnectionCount;

        public override string ToString()
        {
            return string.Format("Res={0}; SucResConnectionCnt={1}; delete={2}", m_res, m_successorResourceConnectionCount, m_delete);
        }

        public bool Equals(ResourceData a_other)
        {
            if (a_other is null)
            {
                return false;
            }

            return m_res.Id == a_other.m_res.Id && m_successorResourceConnectionCount == a_other.m_successorResourceConnectionCount;
        }

        public override bool Equals(object a_obj)
        {
            if (a_obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, a_obj))
            {
                return true;
            }

            if (a_obj is ResourceData other)
            {
                return Equals(other);
            }

            return false;
        }
    }

    internal bool m_delete;
    private readonly List<ResourceData> m_resources = new ();

    internal void Add(InternalResource a_res)
    {
        m_resources.Add(new ResourceData(a_res));
    }

    // You need to call this function after all eligible resources have been added.
    // It arranges the sequence of the elements this structure maintains.
    internal void AddsComplete()
    {
        m_resources.Sort(s_comparer);
    }

    public bool Contains(InternalResource a_res)
    {
        ResourceData rd = new (a_res);
        return m_resources.BinarySearch(rd, s_comparer) >= 0;
    }

    private static readonly ElementComparer s_comparer = new ();

    // This is used to compare the resources in the set. It is used to perform the
    private class ElementComparer : IComparer<ResourceData>
    {
        int IComparer<ResourceData>.Compare(ResourceData a_x, ResourceData a_y)
        {
            if (a_x.m_res.Id > a_y.m_res.Id)
            {
                return 1;
            }

            if (a_x.m_res.Id < a_y.m_res.Id)
            {
                return -1;
            }

            return 0;
        }
    }

    public bool Contains(BaseId a_plantId, BaseId a_deptId, BaseId a_resId)
    {
        for (int i = 0; i < Count; i++)
        {
            InternalResource res = this[i];
            if (res.PlantId == a_plantId && res.DepartmentId == a_deptId && res.Id == a_resId)
            {
                return true;
            }
        }

        return false;
    }

    public int Count => m_resources.Count;

    public InternalResource this[int a_index] => m_resources[a_index].m_res;

    internal ResourceData GetResourceData(int a_index)
    {
        return m_resources[a_index];
    }

    internal void Remove(InternalResource a_res)
    {
        for (int resI = 0; resI < m_resources.Count; ++resI)
        {
            if (m_resources[resI].m_res == a_res)
            {
                m_resources.RemoveAt(resI);
                break;
            }
        }
    }

    internal bool ContainsInfiniteCapacityResource()
    {
        for (int i = 0; i < Count; ++i)
        {
            InternalResource res = this[i];
            if (res.CapacityType == InternalResourceDefs.capacityTypes.Infinite)
            {
                return true;
            }
        }

        return false;
    }

    internal bool ContainsMultitaskingCapacityResource()
    {
        for (int i = 0; i < Count; ++i)
        {
            InternalResource res = this[i];
            if (res.CapacityType == InternalResourceDefs.capacityTypes.MultiTasking)
            {
                return true;
            }
        }

        return false;
    }

    internal void FilterManualSchedulingOnlyResources(Resource a_curSchedRes, List<BaseId> a_doNotFilterResourceIds)
    {
        if (a_curSchedRes != null && a_curSchedRes.ManualSchedulingOnly)
        {
            // Replace the eligible resource with the manual resource.
            ResourceData manualResData = null;
            for (int resI = 0; resI < m_resources.Count; ++resI)
            {
                if (m_resources[resI].m_res == a_curSchedRes)
                {
                    manualResData = m_resources[resI];
                    break;
                }
            }

            m_resources.Clear();
            // The manual resource must be eligible, it must have been in m_resources.
            // It's possible that the resources set will be empty after this function, making the job unschedulable.
            if (manualResData != null)
            {
                m_resources.Add(manualResData);
            }
        }
        else
        {
            // Eliminate manual only resource from m_resources.
            for (int resI = Count - 1; resI >= 0; --resI)
            {
                InternalResource res = m_resources[resI].m_res;
                if (!a_doNotFilterResourceIds.Contains(res.Id))
                {
                    if (res.ManualSchedulingOnly)
                    {
                        m_resources.RemoveAt(resI);
                    }
                }
            }
        }
    }

    internal bool IsAnyEligibleResManualSchedulingOnly()
    {
        for (int resI = 0; resI < m_resources.Count; ++resI)
        {
            if (m_resources[resI].m_res.ManualSchedulingOnly)
            {
                return true;
            }
        }

        return false;
    }

    public override string ToString()
    {
        string s;

        s = string.Format("Contains {0} ResourceDataSets.", m_resources.Count);

        if (m_resources.Count == 1)
        {
            s = s + m_resources[0].m_res;
        }

        return s;
    }

    public IEnumerator<ResourceData> GetEnumerator()
    {
        return m_resources.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Create an EligibleResourceSet that contains the resources in both eligible resource sets passed to this function.
    /// </summary>
    /// <param name="a_x"></param>
    /// <param name="a_y"></param>
    /// <returns></returns>
    internal EligibleResourceSet Intersection(EligibleResourceSet a_x, EligibleResourceSet a_y)
    {
        EligibleResourceSet intersection = new ();

        HashSet<InternalResource> resourcesInX = new ();

        IEnumerator<ResourceData> rdEtxX = a_x.GetEnumerator();
        while (rdEtxX.MoveNext())
        {
            resourcesInX.Add(rdEtxX.Current.m_res);
        }

        IEnumerator<ResourceData> rdEtrY = a_y.GetEnumerator();
        while (rdEtrY.MoveNext())
        {
            if (resourcesInX.Contains(rdEtrY.Current.m_res))
            {
                intersection.Add(rdEtrY.Current.m_res);
            }
        }

        return intersection;
    }

    public bool Equals(EligibleResourceSet a_other)
    {
        if (a_other is null)
        {
            return false;
        }

        //Compare Resources
        if (m_resources.Count != a_other.m_resources.Count)
        {
            return false;
        }

        for (int i = 0; i < m_resources.Count; i++)
        {
            if (!m_resources[i].Equals(a_other.m_resources[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object a_other)
    {
        if (a_other is EligibleResourceSet other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(m_delete, m_resources);
    }
}