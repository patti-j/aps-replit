namespace PT.SchedulerDefinitions;

/// <summary>
/// Control how KPIs are managed.
/// </summary>
public class KpiOptions : ICloneable, IPTSerializable
{
    public const int UNIQUE_ID = 708;

    #region IPTSerializable Members
    public KpiOptions(IReader reader)
    {
        if (reader.VersionNumber >= 665)
        {
            m_boolVector = new BoolVector32(reader);
            reader.Read(out m_maxKpiValuesToStore);
            reader.Read(out m_impactComparisonType);
            reader.Read(out m_impactComparisonId);
        }
        else if (reader.VersionNumber >= 362)
        {
            m_boolVector = new BoolVector32(reader);
            reader.Read(out m_maxKpiValuesToStore);
        }
        else
        {
            CalculateKpiAfterPublish = true;
            CalculateKpiAfterEveryScheduleChange = false;
            MaxKpiValuesToStore = 30;
        }
    }

    public void Serialize(IWriter a_writer)
    {
        m_boolVector.Serialize(a_writer);
        a_writer.Write(m_maxKpiValuesToStore);
        a_writer.Write(m_impactComparisonType);
        a_writer.Write(m_impactComparisonId);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public KpiOptions(bool a_calculateKpiAfterEveryScheduleChange, bool a_calculateKpiAfterPublish, bool a_calculateKpiAfterImport, int a_maxKpiValuesToStore, ESnapshotType a_impactComparisonType, ulong a_impactComparisonId)
    {
        CalculateKpiAfterEveryScheduleChange = a_calculateKpiAfterEveryScheduleChange;
        CalculateKpiAfterPublish = a_calculateKpiAfterPublish;
        CalculateKpiAfterImport = a_calculateKpiAfterImport;
        MaxKpiValuesToStore = a_maxKpiValuesToStore;
        ImpactComparisonType = a_impactComparisonType;
        ImpactComparisonId = a_impactComparisonId;
    }

    public KpiOptions()
    {
        m_boolVector[calcKpiAfterEveryChangeIdx] = true; // Turn on KPIs by default, especially since they're required for Impact Analysis
    }

    /// <summary>
    /// Whether KPI should be stored after every change to the schedule.
    /// </summary>
    public bool CalculateKpiAfterEveryScheduleChange
    {
        get => m_boolVector[calcKpiAfterEveryChangeIdx];
        set => m_boolVector[calcKpiAfterEveryChangeIdx] = value;
    }

    /// <summary>
    /// Whether KPI should be stored after every Publish.
    /// </summary>
    public bool CalculateKpiAfterPublish
    {
        get => m_boolVector[calcKpiAfterPublishIdx];
        set => m_boolVector[calcKpiAfterPublishIdx] = value;
    }

    /// <summary>
    /// Whether KPI should be stored after every Publish.
    /// </summary>
    public bool CalculateKpiAfterImport
    {
        get => m_boolVector[calcKpiAfterImportIdx];
        set => m_boolVector[calcKpiAfterImportIdx] = value;
    }

    private int m_maxKpiValuesToStore = 60;

    /// <summary>
    /// Specifies how many KPI values should be stored.  Higher values give more history but also consume memory.
    /// Old values are cleared automatically once this limit is passed.
    /// </summary>
    public int MaxKpiValuesToStore
    {
        get => m_maxKpiValuesToStore;
        set => m_maxKpiValuesToStore = value;
    }

    public void Update(KpiOptions a_newOptions)
    {
        Validate(a_newOptions);
    }

    private void Validate(KpiOptions a_newOptions)
    {
        throw new NotImplementedException();
    }

    private short m_impactComparisonType = (short)ESnapshotType.Latest;

    public ESnapshotType ImpactComparisonType
    {
        get => (ESnapshotType)m_impactComparisonType;
        set => m_impactComparisonType = (short)value;
    }

    private ulong m_impactComparisonId;

    public ulong ImpactComparisonId
    {
        get => m_impactComparisonId;
        set => m_impactComparisonId = value;
    }

    public enum ESnapshotType
    {
        Import,
        ClockAdvance,
        Optimize,
        Compress,
        Publish,
        User,
        Automatic,
        Latest,
        Specific,
        Today,
        Other,
        Move,
        Expedite,
        JitCompress,
        TimeAdjustment
    }

    #region BoolVector32
    private BoolVector32 m_boolVector;

    private const int calcKpiAfterEveryChangeIdx = 0;
    private const int calcKpiAfterPublishIdx = 1;
    private const int calcKpiAfterImportIdx = 2;
    #endregion

    #region ICloneable Members
    object ICloneable.Clone()
    {
        return Clone();
    }

    public KpiOptions Clone()
    {
        return (KpiOptions)MemberwiseClone();
    }
    #endregion

    public class KpiOptionsException : CommonException
    {
        public KpiOptionsException(string a_message)
            : base(a_message) { }
    }
}