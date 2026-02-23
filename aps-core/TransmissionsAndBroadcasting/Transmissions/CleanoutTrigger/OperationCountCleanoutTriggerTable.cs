using System.Collections;

namespace PT.Transmissions.CleanoutTrigger;

public class OperationCountCleanoutTriggerTable : BaseCleanoutTriggerTable, IPTSerializable
{
    #region IPTSerializable Members
    public OperationCountCleanoutTriggerTable(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12305)
        {
            int count;
            a_reader.Read(out count);
            for (int i = 0; i < count; i++)
            {
                Add(new OperationCountCleanoutTriggerTableRow(a_reader));
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
        foreach (OperationCountCleanoutTriggerTableRow row in m_rows.Values)
        {
            row.Serialize(a_writer);
        }
    }

    public new const int UNIQUE_ID = 1071;
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    public OperationCountCleanoutTriggerTable() { }
    #endregion

    #region Shared Properties
    public string DefaultNamePrefix => "Operation Count Cleanout Trigger";
    #endregion

    #region Rows
    private readonly Hashtable m_rows = new ();

    public void Add(OperationCountCleanoutTriggerTableRow row)
    {
        if (!m_rows.Contains(row.GetHashKey()))
        {
            m_rows.Add(row.GetHashKey(), row);
        }
    }

    public int Count => m_rows.Count;

    public OperationCountCleanoutTriggerTableRow GetRow(int a_triggerValue)
    {
        string hashKey = OperationCountCleanoutTriggerTableRow.GetHashKey(a_triggerValue);
        if (m_rows.Contains(hashKey))
        {
            return (OperationCountCleanoutTriggerTableRow)m_rows[hashKey];
        }

        return null;
    }

    public IDictionaryEnumerator GetEnumerator()
    {
        return m_rows.GetEnumerator();
    }
    #endregion
}

public class OperationCountCleanoutTriggerTableRow : BaseCleanoutTriggerTable.BaseCleanoutTriggerTableRow, IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 1072;

    public OperationCountCleanoutTriggerTableRow(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12305)
        {
            a_reader.Read(out m_triggerValue);
        }
    }

    public virtual void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        a_writer.Write(m_triggerValue);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public OperationCountCleanoutTriggerTableRow(TimeSpan a_duration, int a_cleanoutGrade, decimal a_cleanCost, int a_triggerValue)
        : base(a_duration, a_cleanoutGrade, a_cleanCost)
    {
        m_triggerValue = a_triggerValue;
    }

    public OperationCountCleanoutTriggerTableRow(OperationCountCleanoutTriggerTableRow a_sourceRow)
        : base(a_sourceRow)

    {
        m_triggerValue = a_sourceRow.TriggerValue;
    }

    #region Shared Properties
    private int m_triggerValue;

    public int TriggerValue
    {
        get => m_triggerValue;
        set => m_triggerValue = value;
    }
    #endregion

    #region Hash Key
    internal string GetHashKey()
    {
        return GetHashKey(TriggerValue);
    }

    public static string GetHashKey(int a_triggerValue)
    {
        return a_triggerValue.GetHashCode().ToString();
    }
    #endregion
}