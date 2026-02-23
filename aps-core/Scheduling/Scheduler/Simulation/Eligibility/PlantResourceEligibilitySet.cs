using System.Text;

using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// This contains a set of plants. Each plant is associated with a set of resources. The set of resources are stored as an EligibleResourceSet.
/// For example:
/// P1: Res1, Res2, Res3
/// P2: ResA, ResB
/// P3: Res100
/// P4: Res2000, Res3000, Res 4000
/// </summary>
public class PlantResourceEligibilitySet : IEquatable<PlantResourceEligibilitySet>
{
    internal bool m_delete;
    private readonly SortedDictionary<BaseId, EligibleResourceSet> m_plantsAndResourceEligibilitySets = new (Comparer<BaseId>.Default);

    internal PlantResourceEligibilitySet() { }

    /// <summary>
    /// Creates a deep copy
    /// </summary>
    /// <param name="primaryResourcePRES"></param>
    internal PlantResourceEligibilitySet(PlantResourceEligibilitySet a_pres)
    {
        SortedDictionary<BaseId, EligibleResourceSet>.Enumerator etr = a_pres.GetEnumerator();
        while (etr.MoveNext())
        {
            EligibleResourceSet ers = etr.Current.Value;
            if (!ers.m_delete)
            {
                BaseId plantId = etr.Current.Key;
                Add(plantId, new EligibleResourceSet(ers));
            }
        }
    }

    internal void Clear()
    {
        m_plantsAndResourceEligibilitySets.Clear();
    }

    internal void Add(BaseId a_plantId, EligibleResourceSet a_eligibleResourceSet)
    {
        m_plantsAndResourceEligibilitySets.Add(a_plantId, a_eligibleResourceSet);
    }

    internal bool Contains(BaseId a_plantId)
    {
        return m_plantsAndResourceEligibilitySets.ContainsKey(a_plantId);
    }

    /// <summary>
    /// You're indexing on Plant Base Id. You're getting the set of resources within the plant.
    /// </summary>
    /// <param name="plantId">The base id of a plant.</param>
    /// <returns></returns>
    internal EligibleResourceSet this[BaseId a_plantId] => m_plantsAndResourceEligibilitySets[a_plantId];

    public int Count => m_plantsAndResourceEligibilitySets.Count;

    public SortedDictionary<BaseId, EligibleResourceSet>.Enumerator GetEnumerator()
    {
        return m_plantsAndResourceEligibilitySets.GetEnumerator();
    }

    /// <summary>
    /// The resource count amount all the EligibleResourceSets in this set.
    /// </summary>
    public int ResourceCountInAllEligibleResourceSets
    {
        get
        {
            int resourceCount = 0;

            SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = GetEnumerator();
            while (ersEtr.MoveNext())
            {
                resourceCount += ersEtr.Current.Value.Count;
            }

            return resourceCount;
        }
    }

    internal List<Resource> GetResources()
    {
        List<Resource> eligResources = new ();

        SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = GetEnumerator();
        while (ersEtr.MoveNext())
        {
            EligibleResourceSet resourceSet = ersEtr.Current.Value;
            for (int i = 0; i < resourceSet.Count; i++)
            {
                eligResources.Add((Resource)resourceSet[i]);
            }
        }

        return eligResources;
    }

    internal bool Contains(InternalResource a_res)
    {
        SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = GetEnumerator();
        while (ersEtr.MoveNext())
        {
            if (ersEtr.Current.Value.Contains(a_res))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Among all the plants and eligible resources, the first eligible resource is returned or null if there are no eligible resources.
    /// </summary>
    /// <returns></returns>
    internal InternalResource GetFirstEligibleResource()
    {
        SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = GetEnumerator();

        if (ersEtr.MoveNext() && ersEtr.Current.Value.Count > 0)
        {
            return ersEtr.Current.Value[0];
        }

        return null;
    }

    internal void Remove(BaseId a_resId)
    {
        m_plantsAndResourceEligibilitySets.Remove(a_resId);
    }

    internal void FilterManualSchedulingOnlyResources(Resource a_curSchedRes, List<BaseId> a_doNotFilterResourceIds)
    {
        SortedDictionary<BaseId, EligibleResourceSet>.Enumerator etr = GetEnumerator();
        while (etr.MoveNext())
        {
            etr.Current.Value.FilterManualSchedulingOnlyResources(a_curSchedRes, a_doNotFilterResourceIds);
        }
    }

    internal bool IsAnyEligibleResManualSchedulingOnly()
    {
        SortedDictionary<BaseId, EligibleResourceSet>.Enumerator etr = GetEnumerator();
        while (etr.MoveNext())
        {
            if (etr.Current.Value.IsAnyEligibleResManualSchedulingOnly())
            {
                return true;
            }
        }

        return false;
    }

    public override string ToString()
    {
        StringBuilder sb = new ();

        sb.AppendLine(Count + " Plants.");
        foreach (KeyValuePair<BaseId, EligibleResourceSet> kvp in m_plantsAndResourceEligibilitySets)
        {
            sb.AppendLine("Plant " + kvp.Key);
            for (int i = 0; i < kvp.Value.Count; ++i)
            {
                InternalResource res = kvp.Value[i];
                sb.AppendLine(i + ". Resource : " + res.Name);
            }
        }

        return sb.ToString();
    }

    public bool Equals(PlantResourceEligibilitySet a_other)
    {
        if (a_other == null)
        {
            return false;
        }

        if (m_plantsAndResourceEligibilitySets.Count != a_other.m_plantsAndResourceEligibilitySets.Count)
        {
            return false;
        }

        // Compare each EligibleResourceSet for equality
        if (!m_plantsAndResourceEligibilitySets.Keys.SequenceEqual(a_other.m_plantsAndResourceEligibilitySets.Keys))
        {
            return false;
        }

        foreach (BaseId plantId in m_plantsAndResourceEligibilitySets.Keys)
        {
            EligibleResourceSet thisErs = m_plantsAndResourceEligibilitySets[plantId];
            EligibleResourceSet otherErs = a_other.m_plantsAndResourceEligibilitySets[plantId];
            if (!thisErs.Equals(otherErs))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object a_other)
    {
        if (a_other is PlantResourceEligibilitySet other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other);
        }

        return false;
    }
}