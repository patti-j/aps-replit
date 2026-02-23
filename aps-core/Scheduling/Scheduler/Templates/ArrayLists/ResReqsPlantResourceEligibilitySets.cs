using System;
using System.Text;

using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// Eligibility for each resource requirement. The index for each resource requirement contains a corresponding PlantResourceEligibilitySet in this set.
/// </summary>
public class ResReqsPlantResourceEligibilitySets : IEquatable<ResReqsPlantResourceEligibilitySets>
{
    internal ResReqsPlantResourceEligibilitySets() { }

    /// <summary>
    /// Creates a deep copy.
    /// </summary>
    /// <param name="a_presal"></param>
    internal ResReqsPlantResourceEligibilitySets(ResReqsPlantResourceEligibilitySets a_presal)
    {
        for (int i = 0; i < a_presal.Count; ++i)
        {
            PlantResourceEligibilitySet pres = a_presal[i];
            if (!pres.m_delete)
            {
                m_al.Add(new PlantResourceEligibilitySet(pres));
            }
        }
    }

    private readonly List<PlantResourceEligibilitySet> m_al = new ();

    internal void Add(PlantResourceEligibilitySet s)
    {
        m_al.Add(s);
    }

    internal void Clear()
    {
        m_al.Clear();
    }

    public int Count => m_al.Count;

    /// <summary>
    /// You're indexing on resource requirement. You'll get the set of plants capable of satisfying the resource requirement and the eligible resources in the plant.
    /// </summary>
    /// <param name="a_i">The index of a resource requirement.</param>
    /// <returns></returns>
    public PlantResourceEligibilitySet this[int a_i] => m_al[a_i];

    public PlantResourceEligibilitySet PrimaryEligibilitySet => m_al[0];

    internal void AddRange(ResReqsPlantResourceEligibilitySets a_original)
    {
        m_al.AddRange(a_original.m_al);
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    internal bool IsSatisfiable()
    {
        if (Count == 0)
        {
            return false;
        }

        for (int i = 0; i < Count; ++i)
        {
            PlantResourceEligibilitySet plantResourceEligibiltySet = this[i];
            if (plantResourceEligibiltySet.ResourceCountInAllEligibleResourceSets == 0)
            {
                return false;
            }
        }

        return true;
    }

    internal struct ExcludeFromManualFilter
    {
        internal Dictionary<BaseId, List<BaseId>> ExcludedManualFiltersDictionary;

        internal static ExcludeFromManualFilter NoFilter()
        {
            ExcludeFromManualFilter excludeFromManualFilter;
            excludeFromManualFilter.ExcludedManualFiltersDictionary = new(0);
            return excludeFromManualFilter;
        }
    }

    internal void FilterManualSchedulingOnlyResources(InternalActivity a_act, List<BaseId> a_doNotFilterResourceIds)
    {
        for (int rrI = 0; rrI < Count; ++rrI)
        {
            PlantResourceEligibilitySet pres = this[rrI];
            ResourceSatiator rs = a_act.m_resReqSatiators[rrI];
            Resource res = null;
            if (rs != null)
            {
                res = rs.Resource;
            }

            pres.FilterManualSchedulingOnlyResources(res, a_doNotFilterResourceIds);
        }
    }

    internal bool IsAnyEligibleResManualSchedulingOnly()
    {
        for (int rrI = 0; rrI < Count; ++rrI)
        {
            if (this[rrI].IsAnyEligibleResManualSchedulingOnly())
            {
                return true;
            }
        }

        return false;
    }

    internal void FilterDownToSpecificPlant(BaseId a_plantId)
    {
        ResReqsPlantResourceEligibilitySets filteredPlantResourceEligibilitySetArrayList = new ();

        for (int rrI = 0; rrI < Count; ++rrI)
        {
            PlantResourceEligibilitySet pres = this[rrI];
            bool found = false;

            SortedDictionary<BaseId, EligibleResourceSet>.Enumerator ersEtr = pres.GetEnumerator();
            while (ersEtr.MoveNext())
            {
                if (ersEtr.Current.Key == a_plantId)
                {
                    found = true;
                    PlantResourceEligibilitySet filteredPres = new ();
                    filteredPres.Add(ersEtr.Current.Key, ersEtr.Current.Value);
                    filteredPlantResourceEligibilitySetArrayList.Add(filteredPres);
                    break;
                }
            }

            if (!found)
            {
                Clear();
                return;
            }
        }

        Clear();
        AddRange(filteredPlantResourceEligibilitySetArrayList);
    }

    public override string ToString()
    {
        StringBuilder sb = new ();
        sb.AppendLine("Contains " + Count + " ResReqsPlantResourceElibibilitySets.");
        if (Count == 1)
        {
            sb.Append(m_al[0]);
        }

        return sb.ToString();
    }

    public bool Equals(ResReqsPlantResourceEligibilitySets other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (m_al.Count != other.m_al.Count)
        {
            return false;
        }

        //Check the lists
        for (int i = 0; i < m_al.Count; i++)
        {
            if (!m_al[i].Equals(other.m_al[i]))
            {
                return false;
            }
        }

        return true;
    }

    public override bool Equals(object a_other)
    {
        if (a_other is ResReqsPlantResourceEligibilitySets set)
        {
            if (ReferenceEquals(this, set))
            {
                return true;
            }

            return Equals((ResReqsPlantResourceEligibilitySets)a_other);
        }

        return false;
    }
}