using System.Collections;

namespace PT.Scheduler;

/// <summary>
/// Summary description for TransferQtyCompletionProfile.
/// </summary>
public class TransferQtyCompletionProfile : IEnumerable<TransferQtyCompletion>
{
    internal readonly ArrayList m_profile = new ();

    public int Count => m_profile.Count;

    internal void Add(TransferQtyCompletion a_tqc)
    {
        m_profile.Add(a_tqc);
    }

    public TransferQtyCompletion this[int a_index] => (TransferQtyCompletion)m_profile[a_index];
    
    public IEnumerator<TransferQtyCompletion> GetEnumerator()
    {
        foreach (TransferQtyCompletion tqc in m_profile)
        {
            yield return tqc;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}