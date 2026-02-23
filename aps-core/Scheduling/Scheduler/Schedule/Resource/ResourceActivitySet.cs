namespace PT.Scheduler;

internal class ResourceActivitySet
{
    internal ResourceActivitySet(BaseResource a_res)
    {
        m_res = a_res;
    }

    internal BaseResource Resource => m_res;

    internal int Count => m_actList.Count;

    internal List<InternalActivity> this[int a_idx] => m_actList[a_idx];

    /// <summary>
    /// Adds the activity to a new list at the end of this set.
    /// </summary>
    /// <param name="a_activity"></param>
    internal void Add(List<InternalActivity> a_actList)
    {
        m_actList.Add(a_actList);
    }

    private readonly BaseResource m_res;
    private readonly List<List<InternalActivity>> m_actList = new ();

    public override string ToString()
    {
        System.Text.StringBuilder sb = new ();
        sb.Append("Res=" + m_res.Name);
        sb.Append("; Count=" + Count);
        return sb.ToString();
    }
}