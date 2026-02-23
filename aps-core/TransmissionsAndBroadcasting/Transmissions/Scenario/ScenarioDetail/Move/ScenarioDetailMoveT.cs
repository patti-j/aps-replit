using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Create the transmission and then specify the activities to move by calling AddMoveBlock.
/// </summary>
public class ScenarioDetailMoveT : ScenarioIdBaseT, IBasicMoveData, IEnumerable<MoveBlockKeyData>
{
    #region IPTSerializable Members
    public ScenarioDetailMoveT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 12528)
        {
            m_moveData = new BasicMoveData(reader);

            m_bools = new BoolVector32(reader);

            reader.Read(out bool haveToResourceKey);
            if (haveToResourceKey)
            {
                m_toResourceKey = new ResourceKey(reader);
            }

            reader.Read(out int moveBlocksCount);
            for (int bI = 0; bI < moveBlocksCount; ++bI)
            {
                MoveBlockKeyData mbd = new(reader);
                m_moveBlocks.Add(mbd);
            }

            m_keepOnCurResActList = new ActivityKeyList(reader);

            reader.Read(out int successorOpDictionaryCount);
            m_expediteSuccessorOpIds = new(successorOpDictionaryCount);
            for (int i = 0; i < successorOpDictionaryCount; i++)
            {
                BaseId jobId = new BaseId(reader);

                reader.Read(out int successorsCount);
                HashSet<BaseId> successorOpIds = new(successorsCount);
                for (int j = 0; j < successorsCount; j++)
                {
                    BaseId successorOpId = new BaseId(reader);
                    successorOpIds.Add(successorOpId);
                }

                m_expediteSuccessorOpIds.Add(jobId, successorOpIds);
            }
        }
        else if (reader.VersionNumber >= 12500)
        {
            m_moveData = new BasicMoveData(reader);

            m_bools = new BoolVector32(reader);

            reader.Read(out bool haveToResourceKey);
            if (haveToResourceKey)
            {
                m_toResourceKey = new ResourceKey(reader);
            }

            reader.Read(out int moveBlocksCount);
            for (int bI = 0; bI < moveBlocksCount; ++bI)
            {
                MoveBlockKeyData mbd = new(reader);
                m_moveBlocks.Add(mbd);
            }

            m_keepOnCurResActList = new ActivityKeyList(reader);
        }
        else if (reader.VersionNumber >= 12438)
        {
            m_moveData = new BasicMoveData(reader);

            m_bools = new BoolVector32(reader);

            reader.Read(out bool haveToResourceKey);
            if (haveToResourceKey)
            {
                m_toResourceKey = new ResourceKey(reader);
            }

            reader.Read(out int moveBlocksCount);
            for (int bI = 0; bI < moveBlocksCount; ++bI)
            {
                MoveBlockKeyData mbd = new(reader);
                m_moveBlocks.Add(mbd);
            }

            m_keepOnCurResActList = new ActivityKeyList(reader);

            reader.Read(out int successorOpDictionaryCount);
            m_expediteSuccessorOpIds = new(successorOpDictionaryCount);
            for (int i = 0; i < successorOpDictionaryCount; i++)
            {
                BaseId jobId = new BaseId(reader);

                reader.Read(out int successorsCount);
                HashSet<BaseId> successorOpIds = new(successorsCount);
                for (int j = 0; j < successorsCount; j++)
                {
                    BaseId successorOpId = new BaseId(reader);
                    successorOpIds.Add(successorOpId);
                }

                m_expediteSuccessorOpIds.Add(jobId, successorOpIds);
            }
        }
        else if (reader.VersionNumber >= 12000)
        {
            m_moveData = new BasicMoveData(reader);

            m_bools = new BoolVector32(reader);

            reader.Read(out bool haveToResourceKey);
            if (haveToResourceKey)
            {
                m_toResourceKey = new ResourceKey(reader);
            }

            reader.Read(out int moveBlocksCount);
            for (int bI = 0; bI < moveBlocksCount; ++bI)
            {
                MoveBlockKeyData mbd = new(reader);
                m_moveBlocks.Add(mbd);
            }

            m_keepOnCurResActList = new ActivityKeyList(reader);
        }
        else if (reader.VersionNumber >= 749)
        {
            m_moveData = new BasicMoveData(reader);

            m_bools = new BoolVector32(reader);

            reader.Read(out bool haveToResourceKey);
            if (haveToResourceKey)
            {
                m_toResourceKey = new ResourceKey(reader);
            }

            reader.Read(out int moveBlocksCount);
            for (int bI = 0; bI < moveBlocksCount; ++bI)
            {
                MoveBlockKeyData mbd = new(reader);
                m_moveBlocks.Add(mbd);
            }

            m_keepOnCurResActList = new ActivityKeyList(reader);
            reader.Read(out string uiTrackingId);
        }
        else if (reader.VersionNumber >= 488)
        {
            m_moveData = new BasicMoveData(reader);

            m_bools = new BoolVector32(reader);

            reader.Read(out bool haveToResourceKey);
            if (haveToResourceKey)
            {
                m_toResourceKey = new ResourceKey(reader);
            }

            reader.Read(out int moveBlocksCount);
            for (int bI = 0; bI < moveBlocksCount; ++bI)
            {
                MoveBlockKeyData mbd = new(reader);
                m_moveBlocks.Add(mbd);
            }

            m_keepOnCurResActList = new ActivityKeyList(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        m_moveData.Serialize(writer);

        m_bools.Serialize(writer);

        writer.Write(m_toResourceKey != null);
        if (m_toResourceKey != null)
        {
            m_toResourceKey.Serialize(writer);
        }

        writer.Write(m_moveBlocks.Count);
        foreach (MoveBlockKeyData mbd in m_moveBlocks)
        {
            mbd.Serialize(writer);
        }

        m_keepOnCurResActList.Serialize(writer);

        writer.Write(m_expediteSuccessorOpIds.Count);

        foreach ((BaseId jobId, HashSet<BaseId> successorOpIds) in m_expediteSuccessorOpIds)
        {
            jobId.Serialize(writer);
            writer.Write(successorOpIds.Count);

            foreach (BaseId successorOpId in successorOpIds)
            {
                successorOpId.Serialize(writer);
            }
        }
    }

    public const int UNIQUE_ID = 130;

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ScenarioDetailMoveT() { }


    /// <summary>
    /// Create a MoveT. Move blocks will need to be added.
    /// </summary>
    /// <param name="a_scenarioId"></param>
    /// <param name="a_toResourceKey"></param>
    /// <param name="a_startTicks"></param>
    /// <param name="a_expediteSuccessors"></param>
    /// <param name="a_lockMove"></param>
    /// <param name="a_anchorMove"></param>
    /// <param name="a_exactMove"></param>
    public ScenarioDetailMoveT(BaseId a_scenarioId, ResourceKey a_toResourceKey, long a_startTicks, bool a_expediteSuccessors, bool a_lockMove, bool a_anchorMove, bool a_exactMove, bool a_campaignMove, bool a_stabalizeRightNeighbors = false)
        : base(a_scenarioId)
    {
        Init(a_toResourceKey, a_startTicks, a_expediteSuccessors, a_lockMove, a_anchorMove, a_exactMove, a_stabalizeRightNeighbors, a_campaignMove);
    }

    /// <summary>
    /// A deep copy is made of any arguments this object needs to maintain a reference to.
    /// </summary>
    /// <param name="a_scenarioId"></param>
    /// <param name="a_toResourceKey"></param>
    /// <param name="a_startTicks"></param>
    /// <param name="a_expediteSuccessors"></param>
    /// <param name="a_lockMove"></param>
    /// <param name="a_anchorMove"></param>
    public ScenarioDetailMoveT(ScenarioDetailMoveT a_t, ResourceKey a_toResourceKey, long a_startTicks, bool a_expediteSuccessors, bool a_lockMove, bool a_anchorMove, bool a_exactMove, bool a_campaignMove, bool a_stabalizeRightNeighbors = false)
        : base(a_t)
    {
        Init(a_toResourceKey, a_startTicks, a_expediteSuccessors, a_lockMove, a_anchorMove, a_exactMove, a_stabalizeRightNeighbors, a_campaignMove);
        DeepCopyMoveBlocks(a_t);
    }

    /// <summary>
    /// Creates a deep copy of this object.
    /// </summary>
    /// <param name="a_t"></param>
    internal ScenarioDetailMoveT(ScenarioDetailMoveT a_t)
        : base(a_t)
    {
        Init(a_t.ToResourceKey, a_t.StartTicks, a_t.ExpediteSuccessors, a_t.LockMove, a_t.AnchorMove, a_t.Exact, a_t.PrependSetupToMoveBlocksRightNeighbors, a_t.CampaignMove);
        DeepCopyMoveBlocks(a_t);
    }

    private void DeepCopyMoveBlocks(ScenarioDetailMoveT a_t)
    {
        foreach (MoveBlockKeyData mbkd in a_t)
        {
            AddMoveBlock(new MoveBlockKeyData(mbkd));
        }
    }

    /// <summary>
    /// Common constructor initialization code.
    /// </summary>
    private void Init(ResourceKey a_toResourceKey, long a_startTicks, bool a_expediteSuccessors, bool a_lockMove, bool a_anchorMove, bool a_exactMove, bool a_stabalizeRightNeighbors, bool a_campaignMove)
    {
        InitBasicMoveData(a_toResourceKey, a_startTicks, a_expediteSuccessors, a_lockMove, a_anchorMove, a_exactMove, a_stabalizeRightNeighbors, a_campaignMove);
        ReportAsEvent = false; //we have a special event to handle move failures]
    }

    /// <summary>
    /// Initialize BasicMoveData member.
    /// </summary>
    private void InitBasicMoveData(ResourceKey a_toResourceKey, long a_startTicks, bool a_expediteSuccessors, bool a_lockMove, bool a_anchorMove, bool a_exactMove, bool a_stabalizeRightNeighbors, bool a_campaignMove)
    {
        m_moveData = new BasicMoveData();

        ExpediteSuccessors = a_expediteSuccessors;
        LockMove = a_lockMove;
        AnchorMove = a_anchorMove;
        Exact = a_exactMove;
        CampaignMove = a_campaignMove;

        StartTicks = a_startTicks;
        PrependSetupToMoveBlocksRightNeighbors = a_stabalizeRightNeighbors;

        SetToResourceKey(a_toResourceKey);
    }

    public override string Description => m_moveBlocks.Count == 1 ? "Activity moved".Localize() : "Activities moved".Localize();

    private ResourceKey m_toResourceKey;

    /// <summary>
    /// The resource blocks and activities are to be moved to.
    /// </summary>
    public ResourceKey ToResourceKey
    {
        get => m_toResourceKey;
        private set => m_toResourceKey = value;
    }

    /// <summary>
    /// Sets the ToResourceKey to a copy of the value.
    /// </summary>
    /// <param name="a_toResourceKey"></param>
    public void SetToResourceKey(ResourceKey a_toResourceKey)
    {
        ToResourceKey = new ResourceKey(a_toResourceKey);
    }

    #region IMoveData
    private BasicMoveData m_moveData;

    /// <summary>
    /// The time the moved blocks should attempt to moved to start at.
    /// </summary>
    public long StartTicks
    {
        get => m_moveData.StartTicks;
        private set => m_moveData.StartTicks = value;
    }

    public void SetStartTicks(long a_startTicks)
    {
        StartTicks = a_startTicks;
    }

    /// <summary>
    /// If true the dropped activities will be merged with the block they're dropped on if they block is compatible with the activities.
    /// </summary>
    public bool JoinWithBatchDroppedOntoIfPossible
    {
        get => m_moveData.JoinWithBatchDroppedOntoIfPossible;
        set => m_moveData.JoinWithBatchDroppedOntoIfPossible = value;
    }

    /// <summary>
    /// Whether to expedite the successors of the dropped blocks.
    /// </summary>
    public bool ExpediteSuccessors
    {
        get => m_moveData.ExpediteSuccessors;
        set => m_moveData.ExpediteSuccessors = value;
    }

    /// <summary>
    /// If true lock the block to the new resource.
    /// Also, if the block was locked before the move, remove the lock.
    /// </summary>
    public bool LockMove
    {
        get => m_moveData.LockMove;
        private set => m_moveData.LockMove = value;
    }

    /// <summary>
    /// Whether to anchor the activities at the time the move schedules them.
    /// </summary>
    public bool AnchorMove
    {
        get => m_moveData.AnchorMove;
        private set => m_moveData.AnchorMove = value;
    }

    /// <summary>
    /// Whether to set the moved ManufacturingOrder's release dates to the move drop date.
    /// </summary>
    public bool ResetHoldDate
    {
        get => m_moveData.ResetHoldDate;
        set => m_moveData.ResetHoldDate = value;
    }

    /// <summary>
    /// Whether to move the block exactly where dropped.
    /// </summary>
    public bool Exact
    {
        get => m_moveData.Exact;
        set => m_moveData.Exact = value;
    }

    /// <summary>
    /// Whether to move the block exactly where dropped.
    /// </summary>
    public bool CampaignMove
    {
        get => m_moveData.CampaignMove;
        set => m_moveData.CampaignMove = value;
    }
    #endregion

    #region bools
    private BoolVector32 m_bools;

    private const int c_preBatchSerializedIdx = 0;
    private const int c_prependSetupToMoveBlocksRightNeighbors = 1;
    private const int c_expediteSpecificSuccessorsIdx = 2;

    /// <summary>
    /// This read-only value is true if this object was serialized prior to the changes for resource batching.
    /// Expect a single block and activity. The FromResourceKey in the move data will be null if serialized before resource batching was added.
    /// </summary>
    public bool PreBatchSerialized
    {
        get => m_bools[c_preBatchSerializedIdx];
        private set => m_bools[c_preBatchSerializedIdx] = value;
    }

    /// <summary>
    /// [StabalizeMoveBlocksRightNeighbor] 1:The feature is activated.
    /// Whether to attempt to stablize the end times of the right neighbors of the blocks being moved
    /// by adding any setup time the move causes before the current start time of the blocks' right
    /// neighbors instread of the standard behavior simply extending the block at its current start time.
    /// </summary>
    public bool PrependSetupToMoveBlocksRightNeighbors
    {
        get => m_bools[c_prependSetupToMoveBlocksRightNeighbors];
        private set => m_bools[c_prependSetupToMoveBlocksRightNeighbors] = value;
    }

    public bool ExpediteSpecificSuccessors
    {
        get => m_bools[c_expediteSpecificSuccessorsIdx];
        private set => m_bools[c_expediteSpecificSuccessorsIdx] = value;
    }
    #endregion

    private readonly List<MoveBlockKeyData> m_moveBlocks = new();

    /// <summary>
    /// Add a MoveBlockKeyData.
    /// None of the activities to be moved can in the KeepOn activity set. An IntersectionException will be thrown if
    /// this restriction is violoated.
    /// </summary>
    /// <param name="a_moveBlock">The argument passed in is copied, so changes to the argument will not have an affect on the value this object stores.</param>
    public void AddMoveBlock(MoveBlockKeyData a_moveBlock)
    {
        MoveBlockKeyData copyMoveBlock = new(a_moveBlock);
        m_moveBlocks.Add(copyMoveBlock);

        ValidateMoveAndKeepOnSetsAreDisjoint();
    }

    private ActivityKeyList m_keepOnCurResActList = new();

    /// <summary>
    /// Activities that will remain scheduled on the resources they were scheduled on before the move.
    /// None of the activities in this list can be in the set of activities to be moved.
    /// </summary>
    private ActivityKeyList KeepOnCurResActList
    {
        get => m_keepOnCurResActList;
        set
        {
            m_keepOnCurResActList = value;
            ValidateMoveAndKeepOnSetsAreDisjoint();
        }
    }

    /// <summary>
    /// Enumerate the set of activities that must remain on the resource they were scheduled on before the move.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<ActivityKey> GetKeepOnCurResActivitiesEnumerator()
    {
        return KeepOnCurResActList.GetEnumerator();
    }

    /// <summary>
    /// Specify a set of activities that must stay scheduled on the resources resources they were scheduled on before the move.
    /// None of the activities to be moved can be included in this set. An IntersectionException will be thrown if
    /// this restriction is violoated.
    /// </summary>
    /// <param name="a_lockToCurResActList">A copy of this data is stored in this object.</param>
    public void AddKeepOnCurResActList(ActivityKeyList a_lockToCurResActList)
    {
        ActivityKeyList copy = new(a_lockToCurResActList);
        KeepOnCurResActList = copy;
    }

    /// <summary>
    /// Whether the and set of activities to be moved and KeepOnCurResActList are disjoint.
    /// Activities in the move and KeepOn sets must be disjoint.
    /// </summary>
    /// <returns></returns>
    public ActivityKeyList GetIntersectionBetweenMoveSetAndKeepOnSet()
    {
        ActivityKeyList intersection = new();

        foreach (MoveBlockKeyData mbkd in m_moveBlocks)
        {
            foreach (ActivityKey moveAK in mbkd)
            {
                foreach (ActivityKey keepOnAK in KeepOnCurResActList)
                {
                    if (moveAK == keepOnAK)
                    {
                        intersection.Add(new ActivityKey(moveAK));
                    }
                }
            }
        }

        return intersection;
    }

    /// <summary>
    /// Throw an exception if the move and keep on activity sets aren't disjoint.
    /// </summary>
    public void ValidateMoveAndKeepOnSetsAreDisjoint()
    {
        ActivityKeyList intersection = GetIntersectionBetweenMoveSetAndKeepOnSet();
        if (intersection.Count > 0)
        {
            throw new IntersectionException(intersection);
        }
    }

    public void RemoveMoveBlock(MoveBlockKeyData a_moveBlock)
    {
        m_moveBlocks.Remove(a_moveBlock);
    }

    private Dictionary<BaseId, HashSet<BaseId>> m_expediteSuccessorOpIds = new();

    public void AddSuccessorOpsToExpedite(Dictionary<BaseId, HashSet<BaseId>> a_successorOpsToExpedite)
    {
        m_expediteSuccessorOpIds = a_successorOpsToExpedite;
        ExpediteSpecificSuccessors = true;
    }

    public HashSet<BaseId> GetSuccessorOpIdsToExpedite(BaseId a_jobId)
    {
        if (m_expediteSuccessorOpIds.TryGetValue(a_jobId, out HashSet<BaseId> successorOpIds))
        {
            return successorOpIds;
        }

        return new HashSet<BaseId>();
    }

    #region IEnumerable<MoveBlockKeyData>
    /// <summary>
    /// Enumerates the collection as it exists at the time of creation of the enumerator.
    /// The enumeration isn't tread safe.
    /// An error may occur if the enumertion is used after the collection is modified.
    /// </summary>
    /// <returns></returns>
    public IEnumerator<MoveBlockKeyData> GetEnumerator()
    {
        return m_moveBlocks.GetEnumerator();
    }

    /// <summary>
    /// See description of IEnumerator<MoveBlockKeyData> GetEnumerator().
    /// </summary>
    /// <returns></returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion

    public override string ToString()
    {
        System.Text.StringBuilder sb = new();
        sb.AppendFormat("MoveDate: {0}; ToRes: {1}; Blocks: {2}", new DateTimeOffset(StartTicks, TimeSpan.Zero).ToDisplayTime(), ToResourceKey, m_moveBlocks.Count);
        return sb.ToString();
    }
}