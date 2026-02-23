using PT.Scheduler;

namespace PT.Transmissions;

/// <summary>
/// This was written for class ScenarioDetailMoveT.
/// The move activities set and Keep On activities intersect. The move and KeepOn activity sets must not intersect.
/// </summary>
public class IntersectionException : ValidationException
{
    internal IntersectionException(ActivityKeyList a_intersection)
    {
        Intersection = a_intersection;
    }

    /// <summary>
    /// The activities that are in both the
    /// </summary>
    public ActivityKeyList Intersection { get; private set; }
}