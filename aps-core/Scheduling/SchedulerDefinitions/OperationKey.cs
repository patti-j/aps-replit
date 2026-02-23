using PT.APSCommon;

namespace PT.SchedulerDefinitions;

/// <summary>
/// Identifies an Operation.
/// </summary>
public class OperationKey : IPTSerializable, IEquatable<OperationKey>
{
    public const int UNIQUE_ID = 180;

    #region IPTSerializable Members
    public OperationKey(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            m_jobId = new BaseId(reader);
            m_moId = new BaseId(reader);
            m_operationId = new BaseId(reader);
        }
    }

    public void Serialize(IWriter writer)
    {
        m_jobId.Serialize(writer);
        m_moId.Serialize(writer);
        m_operationId.Serialize(writer);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public OperationKey(BaseId a_jobId, BaseId a_moId, BaseId a_operationId)
    {
        m_jobId = a_jobId;
        m_moId = a_moId;
        m_operationId = a_operationId;
    }

    private readonly BaseId m_jobId;

    public BaseId JobId => m_jobId;

    private readonly BaseId m_moId;

    public BaseId MOId => m_moId;

    private readonly BaseId m_operationId;

    public BaseId OperationId => m_operationId;
    public bool Equals(OperationKey a_operationKey)
    {
        if (a_operationKey == null)
        {
            return false;
        }

        return m_jobId.Equals(a_operationKey.JobId) && m_moId.Equals(a_operationKey.MOId) && m_operationId.Equals(a_operationKey.OperationId);
    }
    public override bool Equals(object a_operationKey)
    {
        if (a_operationKey == null)
        {
            return false;
        }
        return Equals(a_operationKey as OperationKey);
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(m_jobId, m_moId, m_operationId);
    }
}

public class OperationKeyComparer : IComparer<OperationKey>
{
    public int Compare(OperationKey a_x, OperationKey a_y)
    {
        int compare = BaseId.Compare(a_x.JobId, a_y.JobId);
        if (compare != 0)
        {
            return compare;
        }

        compare = BaseId.Compare(a_x.MOId, a_y.MOId);
        if (compare != 0)
        {
            return compare;
        }

        return BaseId.Compare(a_x.OperationId, a_y.OperationId);
    }
}