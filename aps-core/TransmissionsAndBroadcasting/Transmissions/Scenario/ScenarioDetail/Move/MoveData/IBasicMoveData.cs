namespace PT.Transmissions;

/// <summary>
/// Defines basic data required by different Move structures.
/// </summary>
public interface IBasicMoveData
{
    /// <summary>
    /// The time the moved blocks should attempt to moved to start at.
    /// </summary>
    long StartTicks { get; }

    /// <summary>
    /// If true the dropped activities will be merged with the block they're dropped on if they block is compatible with the activities.
    /// </summary>
    bool JoinWithBatchDroppedOntoIfPossible { get; }

    /// <summary>
    /// Whether to expedite the successors of the dropped blocks.
    /// </summary>
    bool ExpediteSuccessors { get; }

    /// <summary>
    /// Whether to lock the activities on the resource they've been moved to.
    /// </summary>
    bool LockMove { get; }

    /// <summary>
    /// Whether to anchor the activities at the time the move schedules them.
    /// </summary>
    bool AnchorMove { get; }

    /// <summary>
    /// Whether to set the moved ManufacturingOrder's release dates to the move drop date.
    /// </summary>
    bool ResetHoldDate { get; }

    /// <summary>
    /// Whether to move the block to the exact time it was dropped pushing whatever's there out of the way.
    /// If false, the move will be after anything that's in the way.
    /// </summary>
    bool Exact { get; }

    bool CampaignMove { get; }
}