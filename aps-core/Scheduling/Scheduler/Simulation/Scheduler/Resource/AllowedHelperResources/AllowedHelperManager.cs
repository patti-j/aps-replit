namespace PT.Scheduler;

public partial class AllowedHelperManager
{
    /// <summary>
    /// If there are Allowed Helper relationships this tells whether a helper resource is on the list of allowed resources of a primary resource.
    /// </summary>
    public bool IsAllowedHelperResource(Resource a_primaryRes, Resource a_helperRes)
    {
        AllowedHelperRelation tempRelation = new (a_primaryRes, null);
        AllowedHelperRelation ahr = GetValue(tempRelation);
        if (ahr != null)
        {
            return ahr.GetValue(a_helperRes) != null;
        }

        return true;
    }

    private BoolMaxtrix m_allowedHelpers;

    /// <summary>
    /// The simulation version of IsAllowedHelperResource(). The datastructure it uses is only setup during the simulation.
    /// Run faster than the non-simulation version. In the initial test with Zenith Bags data
    /// this version (BoolMatrix) took 2:14:31 to Optimize their data.
    /// The non-simulation version (HashTable ) took 2:40:55; this is about 20% longer.
    /// But the time includes a lot of time not related to this function (all the other code that runs during an optimize)
    /// so this function might be hundreds or thousands of times faster than the non-simulation version.
    /// Using a Dictionary<string, HashSet
    /// <string>
    /// > _deTest; where the dictionary's key was the primary resources external id
    /// and the HashSet values were allowed helper resources took 2:57:06; about 32% longer.
    /// Dictionary<string, HashSet
    /// <string>
    /// > _deTest;
    /// internal bool IsAllowedHelperResourceSimVersion(BaseResource a_primaryRes, BaseResource a_helperRes)
    /// {
    /// HashSet
    /// <string>
    /// hs;
    /// if (_deTest.TryGetValue(a_primaryRes.ExternalId, out hs))
    /// {
    /// return hs.Contains(a_helperRes.ExternalId);
    /// }
    /// return true;
    /// }
    /// </summary>
    internal bool IsAllowedHelperResourceSimVersion(BaseResource a_primaryRes, BaseResource a_helperRes)
    {
        if (a_primaryRes.m_primaryResAllowedHelperIdx >= 0)
        {
            if (a_helperRes.m_helperResAllowedHelperIdx == -1)
            {
                return false;
            }

            if (IsAllowedHelper(a_primaryRes.m_primaryResAllowedHelperIdx, a_helperRes.m_helperResAllowedHelperIdx) == false)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsAllowedHelper(int a_primaryResAllowedHelperIdx, int a_helperResAllowedHelperIdx)
    {
        #if DEBUG
        if (a_primaryResAllowedHelperIdx >= m_allowedHelpers.Rows || a_helperResAllowedHelperIdx >= m_allowedHelpers.Columns || a_primaryResAllowedHelperIdx < 0 || a_helperResAllowedHelperIdx < 0)
        {
            throw new Exception("Bad index parameters to IsAllowed.");
        }
        #endif
        return m_allowedHelpers.Get(a_primaryResAllowedHelperIdx, a_helperResAllowedHelperIdx);
    }

    internal void ResetSimulationStateVariables()
    {
        int nextPrimaryResIdx = 0;
        int nextHelperResIdx = 0;
        foreach (AllowedHelperRelation allowedHelperRelation in ReadOnlyList)
        {
            if (allowedHelperRelation.PrimaryResource.m_primaryResAllowedHelperIdx == -1)
            {
                allowedHelperRelation.PrimaryResource.m_primaryResAllowedHelperIdx = nextPrimaryResIdx;
                ++nextPrimaryResIdx;
            }

            IEnumerator<Resource> enumerator = allowedHelperRelation.GetEnumerator();
            while (enumerator.MoveNext())
            {
                Resource res = enumerator.Current;
                if (res.m_helperResAllowedHelperIdx == -1)
                {
                    res.m_helperResAllowedHelperIdx = nextHelperResIdx;
                    ++nextHelperResIdx;
                }
            }
        }

        if (nextPrimaryResIdx == 0 || nextHelperResIdx == 0)
        {
            m_allowedHelpers = null;
        }
        else
        {
            m_allowedHelpers = new BoolMaxtrix(nextPrimaryResIdx, nextHelperResIdx);
            foreach (AllowedHelperRelation allowedHelperRelation in ReadOnlyList)
            {
                IEnumerator<Resource> enumerator = allowedHelperRelation.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Resource res = enumerator.Current;
                    m_allowedHelpers.SetTrue(allowedHelperRelation.PrimaryResource.m_primaryResAllowedHelperIdx, res.m_helperResAllowedHelperIdx);
                }
            }
        }
    }
}