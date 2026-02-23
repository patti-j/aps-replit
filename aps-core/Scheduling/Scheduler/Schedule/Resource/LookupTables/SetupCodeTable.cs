using System.Collections;

using PT.APSCommon;

namespace PT.Scheduler;

/// <summary>
/// Stores a list of Setup Code from-to values.
/// </summary>
///
[Obsolete("This class exists only for backwards compatibility")]
public class SetupCodeTable : IEnumerable<SetupCodeTable.SetupCodeTableRow>
{

    public SetupCodeTable(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 713)
        {
            m_id = new BaseId(a_reader);
            a_reader.Read(out m_name);
            a_reader.Read(out m_description);
            a_reader.Read(out m_wildcard);
            a_reader.Read(out m_previousPrecedence);

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Add(new SetupCodeTableRow(a_reader));
            }
        }
    }

    private BaseId m_id;
    public BaseId Id
    {
        get => m_id;
        set => m_id = value;
    }

    #region Shared Properties
    private string m_name;
    public string Name
    {
        get => m_name;
        set => m_name = value;
    }

    private string m_description;
    public string Description
    {
        get => m_description;
        set => m_description = value;
    }

    private string m_wildcard;
    public string Wildcard
    {
        get => m_wildcard;
        set
        {
            m_wildcard = value;
        }
    }

    private bool m_previousPrecedence = true;

    public bool PreviousPrecedence
    {
        get => m_previousPrecedence;
        set
        {
            m_previousPrecedence = value;
        }
    }
    #endregion

    #region Rows
    private readonly Dictionary<SetupCodeTableRow, SetupCodeTableRow> m_rowsNoWild = new ();
    private readonly Dictionary<SetupCodeTableRow, SetupCodeTableRow> m_rowsWildPrevious = new ();
    private readonly Dictionary<SetupCodeTableRow, SetupCodeTableRow> m_rowsWildNext = new ();

    private void Add(SetupCodeTableRow row)
    {
        if (string.IsNullOrWhiteSpace(Wildcard))
        {
            m_rowsNoWild.Add(row, row);
        }
        else if (row.PreviousOpSetupCode == Wildcard)
        {
            m_rowsWildPrevious.Add(row, row);
        }
        else if (row.NextOpSetupCode == Wildcard)
        {
            m_rowsWildNext.Add(row, row);
        }
        else
        {
            m_rowsNoWild.Add(row, row);
        }
    }
    #endregion

    public IEnumerator<SetupCodeTableRow> GetEnumerator()
    {
        foreach (SetupCodeTableRow row in m_rowsNoWild.Values)
        {
            yield return row;
        }

        foreach (SetupCodeTableRow row in m_rowsWildPrevious.Values)
        {
            yield return row;
        }

        foreach (SetupCodeTableRow row in m_rowsWildNext.Values)
        {
            yield return row;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Obsolete("This class exists only for backwards compatibility")]
    public class SetupCodeTableRow
    {
        public SetupCodeTableRow(IReader a_reader)
        {
            if (a_reader.VersionNumber >= 1)
            {
                a_reader.Read(out m_nextOpSetupCode);
                a_reader.Read(out m_previousOpSetupCode);
                a_reader.Read(out setupCost);
                a_reader.Read(out setupTime);
            }
        }
     
        #region Shared Properties
        private readonly string m_previousOpSetupCode;

        /// <summary>
        /// The Setup Code of the Operation that was previously running.
        /// </summary>
        public string PreviousOpSetupCode => m_previousOpSetupCode;

        private readonly string m_nextOpSetupCode;

        /// <summary>
        /// The Setup Code of the Operation that may run next.
        /// </summary>
        public string NextOpSetupCode => m_nextOpSetupCode;

        private TimeSpan setupTime;

        /// <summary>
        /// The amount of time to schedule for changeover between the Previous Op and Next Op when they have the specified Setup Codes.
        /// </summary>
        public TimeSpan SetupTime
        {
            get => setupTime;
            set => setupTime = value;
        }

        private decimal setupCost;

        /// <summary>
        /// The financial cost incurred when changing over from the Previous Op to the Next Op.
        /// </summary>
        public decimal SetupCost
        {
            get => setupCost;
            set => setupCost = value;
        }
        #endregion
    }
}