using System.Collections;
using System.Text;

using PT.Transmissions;

namespace PT.Scheduler;

/// <summary>
/// The result of a move. Either no problems,  a failure, or problems that caused specific activities to be excluded from the move.
/// </summary>
public class MoveResult : SimulationResult, IEnumerable<MoveBlockProblems>
{
    /// <summary>
    /// Create a MoveResult.
    /// </summary>
    /// <param name="a_t">The transmission that a Move was attempted for.</param>
    internal MoveResult(ScenarioDetailMoveT a_t)
    {
        ScenarioDetailMoveT = a_t;
    }

    /// <summary>
    /// The transmission that a Move was attempted for.
    /// </summary>
    public ScenarioDetailMoveT ScenarioDetailMoveT { get; private set; }

    private int m_failureReason = (int)MoveFailureEnum.None;

    /// <summary>
    /// Whether the Move couldn't be performed.
    /// </summary>
    public override bool Failed => m_failureReason != 0;

    /// <summary>
    /// The number of failures that occurred that caused the Move to fail.
    /// </summary>
    public int FailureCount => BitHelper.CountBitsSet(m_failureReason);

    /// <summary>
    /// Add another Move failure reason to the causes why the Move couldn't be performed. Failure reasons don't accumulate.
    /// If the same failure reason is set multiple times, only 1 failure of that type will be returned by GetFailureReasons().
    /// </summary>
    /// <param name="a_reason">The reason for the failure.</param>
    internal void SetFailureReason(MoveFailureEnum a_reason)
    {
        m_failureReason |= (int)a_reason;
    }

    /// <summary>
    /// Get an array of the reasons why the move failed.
    /// </summary>
    /// <returns>An array of reasons why the move failed.</returns>
    public MoveFailureEnum[] GetFailureReasons()
    {
        MoveFailureEnum[] failureReasons = new MoveFailureEnum[FailureCount];
        int nextFailureIdx = 0;
        foreach (MoveFailureEnum moveFailEnum in (MoveFailureEnum[])Enum.GetValues(typeof(MoveFailureEnum)))
        {
            if ((m_failureReason & (int)moveFailEnum) != 0)
            {
                failureReasons[nextFailureIdx] = moveFailEnum;
                ++nextFailureIdx;
            }
        }

        return failureReasons;
    }

    /// <summary>
    /// Each element in the list represents all of the problems found with a block and its activities (a MoveBlockData).
    /// </summary>
    private readonly List<MoveBlockProblems> m_moveActRRProblems = new ();

    /// <summary>
    /// Add a MoveProblem and the activities with the problem. Problems of a MoveBlockData are merged into one; added to the
    /// existing MoveBlockProblems (for the MoveBlockData).
    /// </summary>
    /// <param name="a_problem"></param>
    /// <param name="a_moveBlockData"></param>
    internal void Add(MoveProblem a_problem, MoveBlockData a_moveBlockData)
    {
        MoveBlockProblems problems = FindBlockProblems(a_moveBlockData);
        if (problems == null)
        {
            // The MoveBlockData had no previous problems reported.
            problems = new MoveBlockProblems(a_moveBlockData);
            problems.Add(a_problem);

            m_moveActRRProblems.Add(problems);
        }
        else
        {
            // The MoveBlockData already has problems reported against it.
            // Attempt to Merge the reported problem.
            MoveProblem mp = problems.Find(a_problem.MoveProblemEnum);
            if (mp != null && mp.Mergeable)
            {
                mp.Merge(a_problem);
            }
            else
            {
                problems.Add(a_problem);
            }
        }
    }

    /// <summary>
    /// Find a MoveBlockData within the set of MoveBlockProblems.
    /// </summary>
    /// <param name="a_moveBlockData">The MoveBlockData to search for.</param>
    /// <returns></returns>
    private MoveBlockProblems FindBlockProblems(MoveBlockData a_moveBlockData)
    {
        foreach (MoveBlockProblems problems in m_moveActRRProblems)
        {
            if (problems.MoveBlockData == a_moveBlockData)
            {
                return problems;
            }
        }

        return null;
    }

    #region Enumerable
    /// <summary>
    /// Enumerate through the MoveBlockProblems.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<MoveBlockProblems> GetEnumerator()
    {
        return m_moveActRRProblems.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion

    /// <summary>
    /// Remove activities that should be excluded from the move from the MoveBlockData.
    /// </summary>
    internal void RemoveProblemedActivities()
    {
        foreach (MoveBlockProblems mbp in this)
        {
            mbp.RemoveProblemsFromMoveBlockData();
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new ();

        MoveFailureEnum[] failureReasons = GetFailureReasons();

        if (failureReasons.Length == 0)
        {
            sb.AppendLine("Move didn't fail.");
        }
        else
        {
            sb.Append("Move Failed:");
            bool firstFailureAdded = false;
            foreach (MoveFailureEnum failureReason in failureReasons)
            {
                if (!firstFailureAdded)
                {
                    firstFailureAdded = true;
                }
                else
                {
                    sb.Append(", ");
                }

                sb.Append(failureReason.ToString());
            }

            sb.AppendLine();
        }

        if (m_moveActRRProblems.Count == 0)
        {
            sb.AppendLine("No activities excluded from move.");
        }
        else
        {
            if (m_moveActRRProblems.Count == 1)
            {
                sb.AppendFormat("MoveBlockProblem:{0}", m_moveActRRProblems[0]);
                sb.AppendLine();
            }
            else
            {
                sb.AppendFormat("NumberOfMoveBlockProblems: {0}", m_moveActRRProblems.Count);
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }
}