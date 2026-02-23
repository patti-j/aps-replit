namespace PT.Scheduler;

/// <summary>
/// An attempt to move a block may result in 1 or more of these problems.
/// </summary>
public enum MoveProblemEnum
{
    /// <summary>
    /// Activity is not be eligible on the target resource.
    /// </summary>
    NotEligibleOnTargetRes,

    /// <summary>
    /// The activities in the block are InProduction and can't be moved.
    /// </summary>
    InProduction,

    /// <summary>
    /// Activity is locked to the FromResource.
    /// </summary>
    LockedToRes,

    /// <summary>
    /// The activities in the block can be moved because the resource it's on disallows drag and drop.
    /// </summary>
    FromResDisallowDragAndDrop,

    /// <summary>
    /// The activities can't be merged with the batch because the from and to batches aren't compatible. For instance they have different resource requirements.
    /// </summary>
    NotCompatibleWithMergeBatch,

    /// <summary>
    /// The batch resource is not eligible for the activities because they can't be scheduled within a single cycle. The activity's required finish quantity is greater than its quantity per cycle.
    /// </summary>
    NotEligibleForBatchResReqCyclesGT1,

    /// <summary>
    /// The batch resource is not eligible for the activities because they can't be scheduled within a single cycle. The activity's required finish quantity is greater than the resource's batch volume.
    /// </summary>
    NotEligibleForBatchResReqFinQtyGTBatchVolume,

    /// <summary>
    /// Alternate Path Moves only support a single lead activity. This error occurs if
    /// the number of lead activities found isn't equal to 1.
    /// The lead activity is first
    /// </summary>
    AlternatePathMovesMustHave1LeadActivity,

    /// <summary>
    /// An alternate path with the specified external id couldn't be found.
    /// </summary>
    AlternatePathNotFound,

    /// <summary>
    /// The AlternatePath move was to the same path. When an AlternatePath move is performed, the current path and new AlternatePath must differ.
    /// The behavior of a regular move differs significantly from an AlternatePath move. In an AlternatePath move, the entire MO is rescheduled.
    /// </summary>
    AlternatePathMustDifferFromCurrentPath
}