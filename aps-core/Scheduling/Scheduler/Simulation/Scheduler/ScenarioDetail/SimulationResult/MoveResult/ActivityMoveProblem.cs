using System.Collections;
using System.Text;

namespace PT.Scheduler;

/// <summary>
/// Indicates all the activities that couldn't schedule due to the same MoveProblemEnum.
/// </summary>
public class ActivityMoveProblem : MoveProblem, IEnumerable<(InternalActivity, int)>
{
    /// <summary>
    /// Create a ActivityMoveProblem.
    /// </summary>
    /// <param name="a_moveProblem">The problem that prevented an activity from being moved.</param>
    /// <param name="a_ia">The activity that couldn't be moved.</param>
    internal ActivityMoveProblem(MoveProblemEnum a_moveProblem, InternalActivity a_ia, int a_resReqIdx)
        : base(a_moveProblem)
    {
        m_activities.Add((a_ia, a_resReqIdx));
    }

    private readonly List<(InternalActivity, int)> m_activities = new ();

    /// <summary>
    /// Whether contents of a MoveProblem of the same subtype can be merged with the problems in this object.
    /// </summary>
    internal override bool Mergeable => true;

    /// <summary>
    /// Merge activity problems into this object. The same ActivityMoveProblem can be for many activities.
    /// </summary>
    /// <param name="a_mp">The activities of this problem are merged (added) to this objects set of activities.</param>
    internal override void Merge(MoveProblem a_mp)
    {
        base.Merge(a_mp);

        ActivityMoveProblem mp = (ActivityMoveProblem)a_mp;
        foreach ((InternalActivity, int) ia in mp.m_activities)
        {
            m_activities.Add(ia);
        }
    }

    #region IEnumerator
    /// <summary>
    /// Iterate through the activities that have the same problem.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<(InternalActivity, int)> GetEnumerator()
    {
        return m_activities.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion

    public override string ToString()
    {
        StringBuilder sb = new ();

        sb.Append(base.ToString());
        if (m_activities.Count == 1)
        {
            sb.AppendFormat("; Activity={0}", m_activities[0]);
        }
        else
        {
            sb.AppendFormat("; ActivityCount={0}", m_activities.Count);
        }

        return sb.ToString();
    }
}