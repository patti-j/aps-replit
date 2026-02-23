namespace PT.Scheduler;

internal class ResourceActivitySets
{
    internal ResourceActivitySets(MainResourceSet a_availableResources)
    {
        m_resActSets = new List<ResourceActivitySet>(a_availableResources.Count);

        for (int availableResourceI = 0; availableResourceI < a_availableResources.Count; ++availableResourceI)
        {
            BaseResource res = a_availableResources[availableResourceI];
            m_resActSets.Add(new ResourceActivitySet(res));
            res.m_sequentialResourceIdx = availableResourceI;
        }
    }

    internal ResourceActivitySets()
    {
        m_resActSets = new List<ResourceActivitySet>();
    }

    private readonly List<ResourceActivitySet> m_resActSets;

    internal int Count => m_resActSets.Count;

    internal void Add(ResourceActivitySet a_resourceActivitySet)
    {
        m_resActSets.Add(a_resourceActivitySet);
    }

    internal ResourceActivitySet this[int a_idx] => m_resActSets[a_idx];

    public override string ToString()
    {
        return string.Format("{0} ResourceActivitySets", Count);
    }
}