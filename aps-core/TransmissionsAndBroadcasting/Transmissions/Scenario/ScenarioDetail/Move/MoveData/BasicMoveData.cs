namespace PT.Transmissions;

/// <summary>
/// Holds basic data required by different Move structures; implements IMoveData.
/// </summary>
public class BasicMoveData : IBasicMoveData, IPTSerializable
{
    #region IPTSerializable Members
    public BasicMoveData(IReader reader)
    {
        if (reader.VersionNumber >= 483)
        {
            reader.Read(out m_startTicks);
            m_bools = new BoolVector32(reader);
        }
    }

    public virtual void Serialize(IWriter writer)
    {
        writer.Write(m_startTicks);
        m_bools.Serialize(writer);
    }

    public const int UNIQUE_ID = 790;

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public BasicMoveData() { }

    public BasicMoveData(IBasicMoveData a_data)
    {
        StartTicks = a_data.StartTicks;
        JoinWithBatchDroppedOntoIfPossible = a_data.JoinWithBatchDroppedOntoIfPossible;
        ExpediteSuccessors = a_data.ExpediteSuccessors;
        LockMove = a_data.LockMove;
        AnchorMove = a_data.AnchorMove;
        Exact = a_data.Exact;
        ResetHoldDate = a_data.ResetHoldDate;
        CampaignMove = a_data.CampaignMove;
    }

    public BasicMoveData(BasicMoveData a_t)
    {
        m_startTicks = a_t.m_startTicks;
        m_bools = new BoolVector32(a_t.m_bools);
    }

    /// <summary>
    /// Moves the block to a different time on the same Resource.
    /// </summary>
    public BasicMoveData(long a_startDate, bool a_joinWithBatchDroppedOntoIfPossible, bool a_expediteSuccessors, bool a_lockMove, bool a_anchorMove, bool a_resetMOReleaseDate, bool a_campaignMove, bool a_frozenZoneOkay)
    {
        StartTicks = a_startDate;
        JoinWithBatchDroppedOntoIfPossible = a_joinWithBatchDroppedOntoIfPossible;
        ExpediteSuccessors = a_expediteSuccessors;
        LockMove = a_lockMove;
        AnchorMove = a_anchorMove;
        ResetHoldDate = a_resetMOReleaseDate;
        CampaignMove = a_campaignMove;
    }

    private long m_startTicks;

    /// <summary>
    /// The time the moved blocks should attempt to moved to start at.
    /// </summary>
    public long StartTicks
    {
        get => m_startTicks;
        protected internal set => m_startTicks = value;
    }

    private BoolVector32 m_bools;
    private const int c_joinWithBatchDroppedOntoIfPossibleIdx = 0;
    private const int c_expediteSuccessorsIdx = 1;
    private const int c_lockMoveIdx = 2;
    private const int c_anchorMoveIdx = 3;
    private const int c_resetMOReleaseDateIdx = 4;
    private const int c_exactIdx = 5;
    private const int c_campaignMoveIdx = 6;

    /// <summary>
    /// If true the dropped activities will be merged with the block they're dropped on if they block is compatible with the activities.
    /// </summary>
    public bool JoinWithBatchDroppedOntoIfPossible
    {
        get => m_bools[c_joinWithBatchDroppedOntoIfPossibleIdx];
        protected internal set => m_bools[c_joinWithBatchDroppedOntoIfPossibleIdx] = value;
    }

    /// <summary>
    /// Whether to expedite the successors of the dropped blocks.
    /// </summary>
    public bool ExpediteSuccessors
    {
        get => m_bools[c_expediteSuccessorsIdx];
        protected internal set => m_bools[c_expediteSuccessorsIdx] = value;
    }

    /// <summary>
    /// If true and the block was locked before the move, remove the lock.
    /// If true lock the block to the new resource.
    /// If true but the block was already locked to a different resource, the activity won't be moved.
    /// </summary>
    public bool LockMove
    {
        get => m_bools[c_lockMoveIdx];
        protected internal set => m_bools[c_lockMoveIdx] = value;
    }

    /// <summary>
    /// Whether to anchor the activities at the time the move schedules them.
    /// </summary>
    public bool AnchorMove
    {
        get => m_bools[c_anchorMoveIdx];
        protected internal set => m_bools[c_anchorMoveIdx] = value;
    }

    /// <summary>
    /// Whether to set the moved ManufacturingOrder's release dates to the move drop date.
    /// </summary>
    public bool ResetHoldDate
    {
        get => m_bools[c_resetMOReleaseDateIdx];
        protected internal set => m_bools[c_resetMOReleaseDateIdx] = value;
    }

    public bool Exact
    {
        get => m_bools[c_exactIdx];
        internal set => m_bools[c_exactIdx] = value;
    }

    public bool CampaignMove
    {
        get => m_bools[c_campaignMoveIdx];
        internal set => m_bools[c_campaignMoveIdx] = value;
    }

    public override string ToString()
    {
        System.Text.StringBuilder sb = new ();

        sb.AppendFormat("StartTicks: {0}; Anchor: {1}; Lock: {2}; Expedite: {3}; JoinBatch: {4}; ResetReleaseDate: {5}", new DateTimeOffset(StartTicks, TimeSpan.Zero).ToDisplayTime(), AnchorMove, LockMove, ExpediteSuccessors, JoinWithBatchDroppedOntoIfPossible, ResetHoldDate);

        return sb.ToString();
    }
}