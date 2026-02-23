using System.Collections;

using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// Stores a list of SetupCode Tables.
/// </summary>
[Obsolete("This class only exists for backwards compatibility")]
public class SetupCodeTableManager 
{
    public SetupCodeTableManager(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            new BaseId(a_reader);

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Add(new SetupCodeTable(a_reader));
            }
        }
    }

    #region list maintenance
    private readonly SortedList<BaseId, SetupCodeTable> m_tables = new ();

    public int Count => m_tables.Count;

    public SetupCodeTable this[int index] => m_tables.Values[index];

    private void Add(SetupCodeTable a_table)
    {
        m_tables.Add(a_table.Id, a_table);
    }
    #endregion

}