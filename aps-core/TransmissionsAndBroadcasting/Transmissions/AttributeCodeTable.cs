namespace PT.Transmissions;

/// <summary>
/// Stores a list of Setup Code from-to values.
/// </summary>
public class AttributeCodeTable : IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 573;

    public AttributeCodeTable(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12417)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_name);
            a_reader.Read(out m_description);
            a_reader.Read(out m_wildcard);

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Add(new AttributeCodeTableRow(a_reader));
            }

            AssignedResources = new Scheduler.ResourceKeyList(a_reader);
            AssignedResourcesExternalId = new Scheduler.ResourceKeyExternalIdList(a_reader);
        }
        else if (a_reader.VersionNumber >= 370)
        {
            a_reader.Read(out m_name);
            a_reader.Read(out m_description);

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Add(new AttributeCodeTableRow(a_reader));
            }

            AssignedResources = new Scheduler.ResourceKeyList(a_reader);
            AssignedResourcesExternalId = new Scheduler.ResourceKeyExternalIdList(a_reader);
        }
    }

    public virtual void Serialize(IWriter writer)
    {
#if DEBUG
        writer.DuplicateErrorCheck(this);
#endif
        m_bools.Serialize(writer);
        writer.Write(m_name);
        writer.Write(m_description);
        writer.Write(m_wildcard);
        writer.Write(Count);
        foreach (AttributeCodeTableRow row in Rows.Values)
        {
            row.Serialize(writer);
        }

        AssignedResources.Serialize(writer);
        AssignedResourcesExternalId.Serialize(writer);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public AttributeCodeTable()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public Scheduler.ResourceKeyExternalIdList AssignedResourcesExternalId = new();
    public Scheduler.ResourceKeyList AssignedResources = new();

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
            m_bools[c_wildCardIsSetIdx] = true;
        }
    }

    public bool PreviousPrecedence
    {
        get => m_bools[c_previousPrecedenceIdx];
        set
        {
            m_bools[c_previousPrecedenceIdx] = value;
            m_bools[c_previousPrecedenceIsSetIdx] = true;
        }
    }

    public bool WildcardIsSet => m_bools[c_wildCardIsSetIdx];
    public bool PreviousPrecedenceIsSet => m_bools[c_previousPrecedenceIsSetIdx];

    private BoolVector32 m_bools;
    private const short c_previousPrecedenceIdx = 0;
    private const short c_wildCardIsSetIdx = 1;
    private const short c_previousPrecedenceIsSetIdx = 2;
    #endregion

    #region Hashtable
    private readonly Dictionary<string, AttributeCodeTableRow> m_rows = new();
    public Dictionary<string, AttributeCodeTableRow> Rows => m_rows;

    public void Add(AttributeCodeTableRow a_row)
    {
        string rowKey = a_row.GetHashKey();
        m_rows.TryAdd(rowKey, a_row);
    }

    /// <summary>
    /// Returns the row defined by these codes.  Returns null if no such row exists.
    /// </summary>
    public AttributeCodeTableRow GetRow(string a_attributeExternalId, string a_previousOpCode, string a_nextOpCode)
    {
        string hashKey = AttributeCodeTableRow.GetHashKey(a_attributeExternalId, a_previousOpCode, a_nextOpCode);
        if (m_rows.TryGetValue(hashKey, out AttributeCodeTableRow row))
        {
            return row;
        }

        return null;
    }

    public int Count => m_rows.Count;
    #endregion

    public class AttributeCodeTableRow : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 574;

        public AttributeCodeTableRow(IReader reader)
        {
            if (reader.VersionNumber >= 1)
            {
                reader.Read(out m_attributeExternalId);
                reader.Read(out m_nextOpAttributeCode);
                reader.Read(out m_previousOpAttributeCode);
                reader.Read(out m_cost);
                reader.Read(out m_duration);
                reader.Read(out m_cleanoutGrade);
            }
        }

        public virtual void Serialize(IWriter writer)
        {
#if DEBUG
            writer.DuplicateErrorCheck(this);
#endif
            writer.Write(m_attributeExternalId);
            writer.Write(m_nextOpAttributeCode);
            writer.Write(m_previousOpAttributeCode);
            writer.Write(m_cost);
            writer.Write(m_duration);
            writer.Write(m_cleanoutGrade);
        }

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        public AttributeCodeTableRow(string a_attributeExternalId, string a_previousOpAttributeCode, string a_nextOpAttributeCode, TimeSpan a_duration, decimal a_cost, int a_cleanoutGrade)
        {
            m_attributeExternalId = a_attributeExternalId;
            m_previousOpAttributeCode = a_previousOpAttributeCode;
            m_nextOpAttributeCode = a_nextOpAttributeCode;
            m_duration = a_duration;
            m_cost = a_cost;
            m_cleanoutGrade = a_cleanoutGrade;
        }

        #region Shared Properties
        private string m_attributeExternalId;

        /// <summary>
        /// The row is to be used for calculating setup using the specified Attribute.
        /// </summary>
        public string AttributeExternalId
        {
            get => m_attributeExternalId;
            set => m_attributeExternalId = value;
        }

        private string m_previousOpAttributeCode;

        /// <summary>
        /// The Setup Code of the Operation that was previously running.
        /// </summary>
        public string PreviousOpAttributeCode
        {
            get => m_previousOpAttributeCode;
            set => m_previousOpAttributeCode = value;
        }

        private string m_nextOpAttributeCode;

        /// <summary>
        /// The Setup Code of the Operation that may run next.
        /// </summary>
        public string NextOpAttributeCode
        {
            get => m_nextOpAttributeCode;
            set => m_nextOpAttributeCode = value;
        }

        private TimeSpan m_duration;

        /// <summary>
        /// The amount of time to schedule for changeover between the Previous Op and Next Op when they have the specified Setup Codes.
        /// </summary>
        public TimeSpan Duration
        {
            get => m_duration;
            set => m_duration = value;
        }

        private decimal m_cost;

        /// <summary>
        /// The financial cost incurred when changing over from the Previous Op to the Next Op.
        /// </summary>
        public decimal Cost
        {
            get => m_cost;
            set => m_cost = value;
        }

        private int m_cleanoutGrade;

        /// <summary>
        /// The financial cost incurred when changing over from the Previous Op to the Next Op.
        /// </summary>
        public int CleanoutGrade
        {
            get => m_cleanoutGrade;
            set => m_cleanoutGrade = value;
        }
        #endregion

        #region Hash Key
        internal string GetHashKey()
        {
            return GetHashKey(AttributeExternalId, PreviousOpAttributeCode, NextOpAttributeCode);
        }

        internal static string GetHashKey(string a_attributeName, string a_previousOpAttributeCode, string a_nextOpAttributeCode)
        {
            return a_attributeName + "*()#" + a_previousOpAttributeCode + "(*&" + a_nextOpAttributeCode;
        }
        #endregion
    }
}