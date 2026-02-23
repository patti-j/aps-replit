namespace PT.Scheduler.Simulation;

/// <summary>
/// Everything is a failure reason except for Success and NotSet.
/// </summary>
internal enum SchedulableSuccessFailureEnum
{
    /// <summary>
    /// The value has no meaning.
    /// </summary>
    NotSet,
    /**************************************************************************************************************
     * For comparison purposes, NotSet must be the first enumeration value.
     **************************************************************************************************************/

    /// <summary>
    /// This result is only applicable to Primary resources.
    /// The operation's CanPause setting couldn't be satisfied.
    /// </summary>
    CanPause,

    /// <summary>
    /// This is a multitasking resources and at some point within the required time the available attention is less than required.
    /// </summary>
    AttentionNotAvailable,

    /// <summary>
    /// The capacity interval type is Occupied
    /// </summary>
    Occupied,

    /// <summary>
    /// The capacity would have crossed where a block is being moved.
    /// </summary>
    IntersectsReservedMoveDate,

    /// <summary>
    /// This shouldn't happen.
    /// </summary>
    LackCapacity,

    /// <summary>
    /// Cleanout intervals aren't crossable.
    /// </summary>
    HitCleanoutInterval,

    // [USAGE_CODE] SchedulableSuccessFailureEnum.IntersectsBlockReservation: The capacity would have intersected time allocated by a BlockReservation.
    /// <summary>
    /// The capacity would have intersected time allocated by a BlockReservation.
    /// </summary>
    IntersectsBlockReservation,

    /// <summary>
    /// A customization is preventing the activity from being schedulable.
    /// </summary>
    Customization,

    /// <summary>
    /// The resource the requirement is locked to is unavailable (offline).
    /// </summary>
    LockedToResUnavail,

    // [USAGE_CODE] SchedulableSuccessFailureEnum.ReservedForResReq: The resource has already been reserved by a different resource requirement.
    /// <summary>
    /// The resource has already been reserved by a different resource requirement.
    /// </summary>
    ReservedForResReq,

    /// <summary>
    /// There was enough capacity.
    /// </summary>
    Success,

    /// <summary>
    /// There was no resource. Possibly the RR wasn't needed anymore; for instance for a resource only required during setup.
    /// </summary>
    SuccessResNotRequired,

    /**************************************************************************************************************
     * For comparison purposes, Success has to be at the end of this enumeration
     *************************************************************************************************************/
    /// <summary>
    /// Another resource requirement for the same activity has already made claim to this resource and there's not enough attention to
    /// fulfill both resource requirements.
    /// </summary>
    AttentionConflictBetweenMultipleRRs,

    /// <summary>
    /// Lack of capacity, but an accurate retry date has been calculated
    /// </summary>
    LackCapacityWithRetry,

    /// <summary>
    /// Unavailable capacity due to a Clean before span
    /// </summary>
    FailedDueToCleanBeforeSpan,

    /// <summary>
    /// No material available
    /// </summary>
    MaterialUnavailable,

    /// <summary>
    /// No storage available
    /// </summary>
    StorageUnavailable,

    /// <summary>
    /// Unavailable capacity due to a Clean before span which was the only reason it couldn't schedule
    /// </summary>
    FailedOnlyDueToCleanBeforeSpan,
}

internal static class SchedulableSuccessFailureEnumExtensions
{
    /// <summary>
    /// Return true if the value is a failure reason.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    internal static bool IsFailure(this SchedulableSuccessFailureEnum x)
    {
        // Return true if the value is between NotSet and success.
        return x.CompareTo(SchedulableSuccessFailureEnum.NotSet) == 1 && x.CompareTo(SchedulableSuccessFailureEnum.Success) == -1;
    }

    /// <summary>
    /// Not set is the lowest.
    /// All Failure reasons are in the middle and considered equal.
    /// Success is the largest.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    internal static int Compare(this SchedulableSuccessFailureEnum x, SchedulableSuccessFailureEnum y)
    {
        int comp = x.CompareTo(y);

        if (comp != 0)
        {
            // Treat all failure reasons as equal.
            // Presume: NotSet is the first enumeration value.
            // Presume: Success is the lat enumeration value.
            if (x.IsFailure() && y.IsFailure())
            {
                comp = 0;
            }
        }

        return comp;
    }
}