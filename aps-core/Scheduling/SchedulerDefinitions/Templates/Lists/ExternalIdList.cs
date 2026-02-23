namespace PT.SchedulerDefinitions;

/// <summary>
/// Maintains a list of ExternalIds.
/// </summary>
public class ExternalIdList
{
    private readonly SortedList<string, string> m_list = new ();

    public int Count => m_list.Count;

    public void Clear()
    {
        m_list.Clear();
    }

    public void Add(string a_externalId)
    {
        m_list.Add(a_externalId, a_externalId);
    }

    public void Remove(string externalId)
    {
        m_list.Remove(externalId);
    }

    public void RemoveAt(int index)
    {
        m_list.RemoveAt(index);
    }

    public string this[int a_index] => m_list.Values[a_index];

    public string this[string a_externalId] => m_list[a_externalId];
}