using PT.Common.Collections;
using PT.Scheduler.Simulation.Events;

namespace PT.Scheduler.Simulation;

internal class PQOptimizeNode
{
    internal long SequenceNbr { get; private set; }

    internal KeyAndActivity Key { get; private set; }

    internal ResourceAvailableEvent ResourceAvailableEvent { get; private set; }

    public void InitKey(ResourceAvailableEvent a_rae, KeyAndActivity a_key, long a_sequenceNbr)
    {
        ResourceAvailableEvent = a_rae;
        Key = a_key;
        SequenceNbr = a_sequenceNbr;
    }

    public override string ToString()
    {
        string s;

        s = "Activity::" + Key.Activity.Id + ";; " + SequenceNbr + ";; " + ResourceAvailableEvent;

        return s;
    }
}

internal class PQOptimizeNodeComparer : IPTCollectionsComparer<PQOptimizeNode>
{
    internal PQOptimizeNodeComparer(IComparer<KeyAndActivity> a_comparer)
    {
        m_comparer = a_comparer;
    }

    private IComparer<KeyAndActivity> m_comparer;

    public void InitComparer(IComparer<KeyAndActivity> a_comparer)
    {
        m_comparer = a_comparer;
    }

    private int Compare(PQOptimizeNode a_n1, PQOptimizeNode a_n2)
    {
        PQOptimizeNode n2Temp = a_n2;
        int keyComparisonResult = m_comparer.Compare(a_n1.Key, n2Temp.Key);

        if (keyComparisonResult == 0)
        {
            if (a_n1.SequenceNbr > n2Temp.SequenceNbr)
            {
                return 1;
            }

            if (a_n1.SequenceNbr < n2Temp.SequenceNbr)
            {
                return -1;
            }

            return 0;
        }

        return keyComparisonResult;
    }

    bool IPTCollectionsComparer<PQOptimizeNode>.LessThan(PQOptimizeNode a_n1, PQOptimizeNode a_n2)
    {
        return Compare(a_n1, a_n2) == -1;
    }

    bool IPTCollectionsComparer<PQOptimizeNode>.LessThanOrEqual(PQOptimizeNode a_n1, PQOptimizeNode a_n2)
    {
        int t = Compare(a_n1, a_n2);
        return t == -1 || t == 0;
    }

    bool IPTCollectionsComparer<PQOptimizeNode>.GreaterThan(PQOptimizeNode a_n1, PQOptimizeNode a_n2)
    {
        return Compare(a_n1, a_n2) == 1;
    }

    bool IPTCollectionsComparer<PQOptimizeNode>.GreaterThanOrEqual(PQOptimizeNode a_n1, PQOptimizeNode a_n2)
    {
        int t = Compare(a_n1, a_n2);
        return t == 1 || t == 0;
    }

    bool IPTCollectionsComparer<PQOptimizeNode>.EqualTo(PQOptimizeNode a_n1, PQOptimizeNode a_n2)
    {
        return Compare(a_n1, a_n2) == 0;
    }

    bool IPTCollectionsComparer<PQOptimizeNode>.NotEqualTo(PQOptimizeNode a_n1, PQOptimizeNode a_n2)
    {
        return Compare(a_n1, a_n2) != 0;
    }
}