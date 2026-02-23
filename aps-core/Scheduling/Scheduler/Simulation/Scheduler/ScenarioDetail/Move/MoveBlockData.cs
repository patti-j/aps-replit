using System.Collections;
using System.Text;

namespace PT.Scheduler;

/// <summary>
/// Used tp specify the activities of a block to move.
/// </summary>
public class MoveBlockData : IEnumerable<InternalActivity>
{
    /// <summary>
    /// Specify the activities of a block to be moved.
    /// </summary>
    /// <param name="a_block">The block to move.</param>
    /// <param name="a_blockActivities">The activities of the block to move.</param>
    internal MoveBlockData(ResourceBlock a_block, List<InternalActivity> a_blockActivities)
    {
        Block = a_block;
        m_activities = new List<InternalActivity>(a_blockActivities);
    }

    /// <summary>
    /// The block whose activities are to be moved.
    /// </summary>
    internal ResourceBlock Block { get; private set; }

    private readonly List<InternalActivity> m_activities = new ();

    /// <summary>
    /// The number of activities of the block to move.
    /// </summary>
    internal int Count => m_activities.Count;

    /// <summary>
    /// An indexer to an activity to be moved.
    /// </summary>
    /// <param name="a_i"></param>
    /// <returns></returns>
    internal InternalActivity this[int a_i] => m_activities[a_i];

    #region IEnumerator
    public IEnumerator<InternalActivity> GetEnumerator()
    {
        return m_activities.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion

    /// <summary>
    /// Remove an activity to be moved.
    /// </summary>
    /// <param name="a_act"></param>
    internal void Remove(InternalActivity a_act)
    {
        m_activities.Remove(a_act);
    }

    /// <summary>
    /// Remove all activites from the set of activities to be moved.
    /// </summary>
    internal void Clear()
    {
        m_activities.Clear();
    }

    public override string ToString()
    {
        StringBuilder sb = new ();
        sb.AppendFormat("Block: {0}", Block);
        if (m_activities.Count == 1)
        {
            sb.AppendFormat(": Activity:{0}", m_activities[0]);
        }
        else
        {
            sb.AppendFormat(": ActivityCount={0}", m_activities.Count);
        }

        return sb.ToString();
    }
}