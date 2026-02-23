namespace PT.Scheduler;

public enum MoveFailureEnum
{
    /// <summary>
    /// The move didn't fail.
    /// </summary>
    None = 0,

    /// <summary>
    /// An activity specified in the Move transmissions couldn't be found.
    /// </summary>
    ActivityNotFound = 1,

    /// <summary>
    /// No valid move activities were specified in the move transmission.
    /// </summary>
    NoMoveActivitiesSpecified = 2,

    // The move To Resource wasn't specified in the transmission.
    ToResNotSpecified = 4,

    /// <summary>
    /// The move To Resource isn't active. The resource's Inactive flag is set.
    /// </summary>
    ToResNotActive = 8,

    /// <summary>
    /// A move block couldn't be found.
    /// </summary>
    BlockNotFound = 16,

    /// <summary>
    /// One of the move From Resources couldn't be found.
    /// </summary>
    ResourceNotFound = 32,

    /// <summary>
    /// Drag and drop on the To Resource isn't allowed. The resource's flag to disallow drag and drop has been set.
    /// </summary>
    ToResDisallowsDragAndDrop = 64,

    /// <summary>
    /// The batch merge move failed because none of the activities to be merged were compatible with the move to block.
    /// </summary>
    NoMovedActivitiesCompatibleWithMoveToBlocksBatch = 128,

    /// <summary>
    /// All of the blocks were excluded from the move because of problems (specifically the problems reported using MoveProblemEnum).
    /// </summary>
    AllBlocksHadProblems = 256,

    /// <summary>
    /// An alternate path move was attempted, but the move to Alternate Path couldn't be found.
    /// </summary>
    Unused = 512,

    /// <summary>
    /// One or more jobs failed to schedule. An undo should be performed to get back to the state prior to jobs ending up to the unscheduled state.
    /// </summary>
    JobsFailedToSchedule = 1024,

    /// <summary>
    /// Resource specified in transmission does not exist.
    /// </summary>
    ToResourceNotFound = 2048,

    /// <summary>
    /// The move couldn't be performed because the resource the block was dropped on was already being used for a different resource requirement of the same batch.
    /// </summary>
    MoveToResUsedByOtherReq = 4096
}

//1
//2
//4
//8
//16
//32
//64
//128
//256
//512
//1024
//2048
//4096
//8192
//16384
//32768
//65536
//131072
//262144
//524288
//1048576
//2097152
//4194304
//8388608
//16777216
//33554432
//67108864
//134217728
//268435456
//536870912
//1073741824