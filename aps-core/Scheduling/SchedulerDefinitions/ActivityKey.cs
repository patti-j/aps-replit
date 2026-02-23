using PT.APSCommon;

namespace PT.SchedulerDefinitions;

/// <summary>
/// Identifies an Activity.
/// </summary>
public class ActivityKey : IPTSerializable, IEquatable<ActivityKey>
{
    #region IPTSerializable Members
    public ActivityKey(IReader reader)
    {
        if (reader.VersionNumber >= 1)
        {
            m_jobId = new BaseId(reader);
            m_moId = new BaseId(reader);
            m_operationId = new BaseId(reader);
            m_activityId = new BaseId(reader);
        }
    }

    public void Serialize(IWriter writer)
    {
        m_jobId.Serialize(writer);
        m_moId.Serialize(writer);
        m_operationId.Serialize(writer);
        m_activityId.Serialize(writer);
    }

    public const int UNIQUE_ID = 170;
    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public ActivityKey(BaseId a_jobId, BaseId a_moId, BaseId a_operationId, BaseId a_activityId)
    {
        m_jobId = new BaseId(a_jobId);
        m_moId = new BaseId(a_moId);
        m_operationId = new BaseId(a_operationId);
        m_activityId = new BaseId(a_activityId);
    }

    public ActivityKey(ActivityKey a_k) : this(a_k.JobId, a_k.MOId, a_k.OperationId, a_k.ActivityId) { }

    private readonly BaseId m_jobId;
    public BaseId JobId => m_jobId;

    private readonly BaseId m_moId;
    public BaseId MOId => m_moId;

    private readonly BaseId m_operationId;
    public BaseId OperationId => m_operationId;

    private readonly BaseId m_activityId;
    public BaseId ActivityId => m_activityId;

    public bool Equals(ActivityKey a_other)
    {
        return this == a_other;
    }

    public static bool operator ==(ActivityKey ak1, ActivityKey ak2)
    {
        bool equal =
            ak1.JobId == ak2.JobId &&
            ak1.MOId == ak2.MOId &&
            ak1.OperationId == ak2.OperationId &&
            ak1.ActivityId == ak2.ActivityId;

        return equal;
    }

    public static bool operator !=(ActivityKey ak1, ActivityKey ak2)
    {
        return !(ak1 == ak2);
    }

    public override bool Equals(object a_obj)
    {
        return this == a_obj as ActivityKey;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(m_activityId.Value);
    }

    public override string ToString()
    {
        return string.Format("job:{0}; mo:{1}; op:{2}; act:{3}", JobId, MOId, OperationId, ActivityId);
    }

    public static ActivityKey NullKey => new ActivityKey(BaseId.NULL_ID, BaseId.NULL_ID, BaseId.NULL_ID, BaseId.NULL_ID);
}

public class ActivityKeyExternal : IPTSerializable, IEquatable<ActivityKeyExternal>
{
    #region IPTSerializable Members
    public ActivityKeyExternal(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out m_jobExternalId);
            a_reader.Read(out m_moExternalId);
            a_reader.Read(out m_operationExternalId);
            a_reader.Read(out m_activityExternalId);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_jobExternalId);
        a_writer.Write(m_moExternalId);
        a_writer.Write(m_operationExternalId);
        a_writer.Write(m_activityExternalId);
    }

    public const int UNIQUE_ID = 1103;
    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public ActivityKeyExternal(string a_jobExternalId, string a_moExternalId, string a_operationExternalId, string a_activityExternalId)
    {
        m_jobExternalId = a_jobExternalId;
        m_moExternalId = a_moExternalId;
        m_operationExternalId = a_operationExternalId;
        m_activityExternalId = a_activityExternalId;
    }

    public ActivityKeyExternal(ActivityKeyExternal a_k) : this(a_k.JobExternalId, a_k.MOExternalId, a_k.OperationExternalId, a_k.ActivityExternalId) { }

    private readonly string m_jobExternalId;
    public string JobExternalId => m_jobExternalId;

    private readonly string m_moExternalId;
    public string MOExternalId => m_moExternalId;

    private readonly string m_operationExternalId;
    public string OperationExternalId => m_operationExternalId;

    private readonly string m_activityExternalId;
    public string ActivityExternalId => m_activityExternalId;

    public bool Equals(ActivityKeyExternal a_other)
    {
        return this == a_other;
    }

    public static bool operator ==(ActivityKeyExternal a_k1, ActivityKeyExternal a_ak2)
    {
        bool equal =
            a_k1.JobExternalId == a_ak2.JobExternalId &&
            a_k1.MOExternalId == a_ak2.MOExternalId &&
            a_k1.OperationExternalId == a_ak2.OperationExternalId &&
            a_k1.ActivityExternalId == a_ak2.ActivityExternalId;

        return equal;
    }

    public static bool operator !=(ActivityKeyExternal a_k1, ActivityKeyExternal a_ak2)
    {
        return !(a_k1 == a_ak2);
    }

    public override bool Equals(object a_obj)
    {
        return this == a_obj as ActivityKeyExternal;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(m_activityExternalId);
    }

    public override string ToString()
    {
        return string.Format("Job External Id:{0}; Mo External Id:{1}; Op External Id:{2}; Activity External Id:{3}", JobExternalId, MOExternalId, OperationExternalId, ActivityExternalId);
    }

    public static ActivityKeyExternal NullKey => new ActivityKeyExternal(string.Empty, string.Empty, string.Empty, string.Empty);
}