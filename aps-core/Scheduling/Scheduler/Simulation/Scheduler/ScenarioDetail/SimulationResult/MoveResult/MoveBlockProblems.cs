using System.Collections;
using System.Text;

namespace PT.Scheduler;

/// <summary>
/// This is used to specify all the problems associated with an attempt to move a block.
/// 1 or more problems may be reported for the MoveBlockData. The set of problems can be enumerated.
/// </summary>
public class MoveBlockProblems : IEnumerable<MoveProblem>
{
    /// <summary>
    /// Create a new instance specifying the problem source MoveBlockData.
    /// </summary>
    /// <param name="a_moveBlockData">The MoveBlockData whose activity(ies) couldn't be scheduled due to problems.</param>
    internal MoveBlockProblems(MoveBlockData a_moveBlockData)
    {
        MoveBlockData = a_moveBlockData;
    }

    private readonly List<MoveProblem> m_problems = new ();

    /// <summary>
    /// Add a problem to the list of problems.
    /// </summary>
    /// <param name="a_moveProblem"></param>
    internal void Add(MoveProblem a_moveProblem)
    {
        m_problems.Add(a_moveProblem);
    }

    /// <summary>
    /// The number of problems.
    /// </summary>
    internal int Count => m_problems.Count;

    /// <summary>
    /// The MoveBlockData with activities that have problems that prevent them from being scheduled.
    /// </summary>
    internal MoveBlockData MoveBlockData { get; private set; }

    /// <summary>
    /// Find a MoveProblem of a specific move type. There can only be a MoveProblem of a type.
    /// </summary>
    /// <param name="a_mpe">Search for a MoveProblem with the same MoveProblemEnum.</param>
    /// <returns>null if a matching MoveProblem can't be found.</returns>
    internal MoveProblem Find(MoveProblemEnum a_mpe)
    {
        foreach (MoveProblem mp in m_problems)
        {
            if (mp.MoveProblemEnum == a_mpe)
            {
                return mp;
            }
        }

        return null;
    }

    #region IEnumerable<MoveProblem>
    /// <summary>
    /// Enumerate throught the move problems. The enumerator becomes invalid if this collection is altered.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<MoveProblem> GetEnumerator()
    {
        return m_problems.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion

    /// <summary>
    /// Remove activities with problems that are to be excluded from the move from the MoveBlockData.
    /// </summary>
    internal void RemoveProblemsFromMoveBlockData()
    {
        foreach (MoveProblem mp in m_problems)
        {
            ActivityMoveProblem amp = mp as ActivityMoveProblem;
            if (amp == null)
            {
                // The problem applies to all the activities in the MoveBlockData.
                MoveBlockData.Clear();
            }
            else
            {
                // The problem applies to specific activities within the block to be moved.
                foreach ((InternalActivity Act, int rrIdx) actInfo in amp)
                {
                    MoveBlockData.Remove(actInfo.Act);
                }
            }
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new ();

        sb.Append(MoveBlockData);

        if (m_problems.Count == 0)
        {
            sb.Append("There are no problems.");
        }
        else if (m_problems.Count == 1)
        {
            string s = m_problems[0].ToString();
            sb.AppendFormat("; {0}", s);
        }
        else
        {
            sb.AppendFormat("NumberOfMoveProblmes: {0}", m_problems.Count);
        }

        return sb.ToString();
    }
}