using System.Collections;

namespace PT.Transmissions.CleanoutTrigger;

public class TimeCleanoutTriggerTable : BaseCleanoutTriggerTable, IPTSerializable
{
    #region IPTSerializable Members
    public TimeCleanoutTriggerTable(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12305)
        {
            int count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Add(new TimeCleanoutTriggerTableRow(a_reader));
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif

        base.Serialize(a_writer);
        a_writer.Write(Count);
        foreach (TimeCleanoutTriggerTableRow row in m_rows.Values)
        {
            row.Serialize(a_writer);
        }
    }

    public new const int UNIQUE_ID = 1066;
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    public TimeCleanoutTriggerTable() { }
    #endregion

    #region Shared Properties
    public string DefaultNamePrefix => "Fixed Time Cleanout Trigger";
    #endregion

    #region Rows
    private readonly Hashtable m_rows = new ();

    public void Add(TimeCleanoutTriggerTableRow row)
    {
        if (!m_rows.Contains(row.GetHashKey()))
        {
            m_rows.Add(row.GetHashKey(), row);
        }
    }

    public int Count => m_rows.Count;

    public TimeCleanoutTriggerTableRow GetRow(TimeSpan a_triggerValue)
    {
        string hashKey = TimeCleanoutTriggerTableRow.GetHashKey(a_triggerValue);
        if (m_rows.Contains(hashKey))
        {
            return (TimeCleanoutTriggerTableRow)m_rows[hashKey];
        }

        return null;
    }

    public IDictionaryEnumerator GetEnumerator()
    {
        return m_rows.GetEnumerator();
    }
    #endregion
}

public class TimeCleanoutTriggerTableRow : BaseCleanoutTriggerTable.BaseCleanoutTriggerTableRow, IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 1067;

    public TimeCleanoutTriggerTableRow(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12305)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_triggerValue);
        }
    }

    public virtual void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        m_bools.Serialize(a_writer);
        a_writer.Write(m_triggerValue);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public TimeCleanoutTriggerTableRow(TimeSpan a_duration,
                                       int a_cleanoutGrade,
                                       decimal a_cleanCost,
                                       TimeSpan a_triggerValue,
                                       bool a_useProcessingTime,
                                       bool a_usePostProcessingTime,
                                       bool a_triggerAtEnd)
        : base(a_duration, a_cleanoutGrade, a_cleanCost)
    {
        m_triggerValue = a_triggerValue;
        m_bools[c_useProcessingTimeIdx] = a_useProcessingTime;
        m_bools[c_usePostprocessingTimeIdx] = a_usePostProcessingTime;
        m_bools[c_triggerAtEndIdx] = a_triggerAtEnd;
    }

    public TimeCleanoutTriggerTableRow(TimeCleanoutTriggerTableRow a_sourceRow)
        : base(a_sourceRow)

    {
        m_triggerValue = a_sourceRow.TriggerValue;
        m_bools[c_useProcessingTimeIdx] = a_sourceRow.UseProcessingTime;
        m_bools[c_usePostprocessingTimeIdx] = a_sourceRow.UsePostprocessingTime;
        m_bools[c_triggerAtEndIdx] = a_sourceRow.TriggerAtEnd;
    }

    #region Shared Properties
    public BoolVector32 m_bools;

    private const short c_useProcessingTimeIdx = 0;
    private const short c_usePostprocessingTimeIdx = 1;
    private const short c_triggerAtEndIdx = 2;

    private TimeSpan m_triggerValue;

    public TimeSpan TriggerValue
    {
        get => m_triggerValue;
        set => m_triggerValue = value;
    }

    public bool UseProcessingTime
    {
        get => m_bools[c_useProcessingTimeIdx];
        set => m_bools[c_useProcessingTimeIdx] = value;
    }

    public bool UsePostprocessingTime
    {
        get => m_bools[c_usePostprocessingTimeIdx];
        set => m_bools[c_usePostprocessingTimeIdx] = value;
    }

    public bool TriggerAtEnd
    {
        get => m_bools[c_triggerAtEndIdx];
        set => m_bools[c_triggerAtEndIdx] = value;
    }
    #endregion

    #region Hash Key
    internal string GetHashKey()
    {
        return GetHashKey(TriggerValue);
    }

    public static string GetHashKey(TimeSpan a_triggerValue)
    {
        return a_triggerValue.GetHashCode().ToString();
    }
    #endregion
}