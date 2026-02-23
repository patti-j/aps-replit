namespace PT.Scheduler;

/// <summary>
/// Why activity(ies) couldn't be moved.
/// Additional information that needs to be reported for a problem will be added as child classes.
/// </summary>
public class MoveProblem
{
    /// <summary>
    /// Specify why activity(ies) couldn't be moved.
    /// </summary>
    /// <param name="a_moveProblem"></param>
    internal MoveProblem(MoveProblemEnum a_moveProblem)
    {
        MoveProblemEnum = a_moveProblem;
    }

    /// <summary>
    /// Why activity(ies) couldn't be moved.
    /// </summary>
    public MoveProblemEnum MoveProblemEnum { get; private set; }

    /// <summary>
    /// Whether the contents of multiple objects of the same subtype can be merged.
    /// </summary>
    internal virtual bool Mergeable => false;

    /// <summary>
    /// Merge the contents of move problems. This base version verifies the problems are compatible; they have equal MoveProblemEnums.
    /// </summary>
    /// <param name="a_mp">The contents of this parameter are added to this object.</param>
    internal virtual void Merge(MoveProblem a_mp)
    {
        if (a_mp.MoveProblemEnum != MoveProblemEnum)
        {
            throw new Exception("Problems of different types can't be merged.");
        }
    }

    public override string ToString()
    {
        return string.Format("MoveProblem: {0}", MoveProblemEnum.ToString());
    }
}