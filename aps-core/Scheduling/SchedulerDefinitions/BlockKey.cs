using PT.APSCommon;

namespace PT.SchedulerDefinitions;

/// <summary>
/// Identifies an Block.
/// </summary>
public class BlockKey : IComparable, IEquatable<BlockKey>, IPTSerializable
{
    #region IPTSerializable Members
    public BlockKey(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_jobId = new BaseId(a_reader);
            m_moId = new BaseId(a_reader);
            m_operationId = new BaseId(a_reader);
            m_activityId = new BaseId(a_reader);
            m_blockId = new BaseId(a_reader);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_jobId.Serialize(a_writer);
        m_moId.Serialize(a_writer);
        m_operationId.Serialize(a_writer);
        m_activityId.Serialize(a_writer);
        m_blockId.Serialize(a_writer);
    }

    public const int UNIQUE_ID = 173;
    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public BlockKey(BlockKey a_blockKey)
    {
        Init(ref a_blockKey.m_jobId, ref a_blockKey.m_moId, ref a_blockKey.m_operationId, ref a_blockKey.m_activityId, ref a_blockKey.m_blockId);
    }

    public BlockKey(BaseId r_jobId, BaseId r_moId, BaseId r_operationId, BaseId r_activityId, BaseId r_blockId)
    {
        Init(ref r_jobId, ref r_moId, ref r_operationId, ref r_activityId, ref r_blockId);
    }

    private void Init(ref BaseId a_jobId, ref BaseId a_moId, ref BaseId a_operationId, ref BaseId a_activityId, ref BaseId a_blockId)
    {
        m_jobId = new BaseId(a_jobId);
        m_moId = new BaseId(a_moId);
        m_operationId = new BaseId(a_operationId);
        m_activityId = new BaseId(a_activityId);
        m_blockId = new BaseId(a_blockId);
    }

    private BaseId m_jobId;
    public BaseId JobId => m_jobId;

    private BaseId m_moId;
    public BaseId MOId => m_moId;

    private BaseId m_operationId;
    public BaseId OperationId => m_operationId;

    private BaseId m_activityId;
    public BaseId ActivityId => m_activityId;
    private BaseId m_blockId;
    public BaseId BlockId => m_blockId;

    public override string ToString()
    {
        return string.Format("JobId: {0}; MOId: {1}; OpId: {2} ActId: {3} BlockId: {4}", JobId, MOId, OperationId, ActivityId, BlockId);
    }

    public bool Equals(BlockKey a_other)
    {
        return JobId == a_other.JobId && MOId == a_other.MOId && OperationId == a_other.OperationId && ActivityId == a_other.ActivityId && BlockId == a_other.BlockId;
    }

    public override bool Equals(object obj)
    {
        if (obj is BlockKey other)
        {
            return Equals(other);
        }

        return false;
    }

    //public override bool Equals(object a_obj)
    //{
    //    BlockKey bk = a_obj as BlockKey;

    //    if (bk == null)
    //    {
    //        return false;
    //    }
    //    else
    //    {
    //        return this == bk;
    //    }
    //}
    //public static bool operator ==(BlockKey a_a, BlockKey a_b)
    //{
    //    return a_a.m_jobId == a_b.JobId &&
    //        a_a.m_moId == a_b.m_moId &&
    //        a_a.m_operationId == a_b.m_operationId &&
    //        a_a.m_activityId == a_b.m_activityId &&
    //        a_a.m_blockId == a_b.m_blockId;
    //}

    //public static bool operator !=(BlockKey a_a, BlockKey a_b)
    //{
    //    return !(a_a == a_b);
    //}

    public override int GetHashCode()
    {
        int idx = (int)m_jobId.Value * 50;
        idx += (int)m_moId.Value;
        idx += (int)m_operationId.Value;
        idx += (int)m_activityId.Value;
        idx += (int)m_blockId.Value;
        return idx;
    }

    public ActivityKey CreateActivityKey()
    {
        return new ActivityKey(JobId, MOId, OperationId, ActivityId);
    }

    #region IComparable Members
    public int CompareTo(object a_obj)
    {
        BlockKey key = (BlockKey)a_obj;
        if (m_jobId == key.m_jobId && m_moId == key.m_moId && m_operationId == key.m_operationId && m_activityId == key.m_activityId && m_blockId == key.m_blockId)
        {
            return 0;
        }

        if (m_jobId.CompareTo(key.m_jobId) == -1)
        {
            return -1;
        }

        if (m_jobId == key.m_jobId && m_moId.CompareTo(key.m_moId) == -1)
        {
            return -1;
        }

        if (m_jobId == key.m_jobId && m_moId == key.m_moId && m_operationId.CompareTo(key.m_operationId) == -1)
        {
            return -1;
        }

        if (m_jobId == key.m_jobId && m_moId == key.m_moId && m_operationId == key.m_operationId && m_activityId.CompareTo(key.m_activityId) == -1)
        {
            return -1;
        }

        if (m_jobId == key.m_jobId && m_moId == key.m_moId && m_operationId == key.m_operationId && m_activityId == key.m_activityId && m_blockId.CompareTo(key.m_blockId) == -1)
        {
            return -1;
        }

        return 1;
    }
    #endregion
}