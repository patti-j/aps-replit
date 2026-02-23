using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.Transmissions.CleanoutTrigger;

public class ItemCleanoutTable : IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 1109;

    public ItemCleanoutTable(IReader a_reader)
    {
        m_bools = new BoolVector32(a_reader);
        a_reader.Read(out m_name);
        a_reader.Read(out m_description);
        a_reader.Read(out m_wildcard);

        a_reader.Read(out int count);
        for (int i = 0; i < count; i++)
        {
            Add(new ItemCleanoutTableRow(a_reader));
        }

        AssignedResources = new PTLinkedList<ResourceKey>(a_reader);
        ResourceExternalIdKeyList = new ResourceKeyExternalIdList(a_reader);
    }

    public virtual void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        a_writer.Write(m_name);
        a_writer.Write(m_description);
        a_writer.Write(m_wildcard);
        a_writer.Write(Count);
        foreach (ItemCleanoutTableRow row in Rows.Values)
        {
            row.Serialize(a_writer);
        }

        AssignedResources.Serialize(a_writer);
        ResourceExternalIdKeyList.Serialize(a_writer);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public ItemCleanoutTable() { }

    public PTLinkedList<ResourceKey> AssignedResources = new();
    public ResourceKeyExternalIdList ResourceExternalIdKeyList = new();

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
    private readonly Dictionary<string, ItemCleanoutTableRow> m_rows = new();
    public Dictionary<string, ItemCleanoutTableRow> Rows => m_rows;

    public void Add(ItemCleanoutTableRow a_row)
    {
        string rowKey = a_row.GetHashKey();
        m_rows.TryAdd(rowKey, a_row);
    }

    /// <summary>
    /// Returns the row defined by these codes.  Returns null if no such row exists.
    /// </summary>
    public ItemCleanoutTableRow GetRow(string a_fromItemExternalId, string a_toItemExternalId)
    {
        string hashKey = ItemCleanoutTableRow.GetHashKey(a_fromItemExternalId, a_toItemExternalId);
        if (m_rows.TryGetValue(hashKey, out ItemCleanoutTableRow row))
        {
            return row;
        }

        return null;
    }

    public int Count => m_rows.Count;
    #endregion

    public class ItemCleanoutTableRow : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 1110;

        public ItemCleanoutTableRow(IReader a_reader)
        {
            a_reader.Read(out m_toItemExternalId);
            a_reader.Read(out m_fromItemExternalId);
            a_reader.Read(out m_cost);
            a_reader.Read(out m_duration);
        }

        public virtual void Serialize(IWriter a_writer)
        {
            a_writer.Write(m_toItemExternalId);
            a_writer.Write(m_fromItemExternalId);
            a_writer.Write(m_cost);
            a_writer.Write(m_duration);
        }

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        public ItemCleanoutTableRow(string a_fromItemExternalId, string a_toItemExternalId, TimeSpan a_duration, decimal a_cost)
        {
            m_fromItemExternalId = a_fromItemExternalId;
            m_toItemExternalId = a_toItemExternalId;
            m_duration = a_duration;
            m_cost = a_cost;
        }

        public ItemCleanoutTableRow() { }

        #region Shared Properties
        private string m_fromItemExternalId;
        /// <summary>
        /// The Setup Code of the Operation that was previously running.
        /// </summary>
        public string FromItemExternalId
        {
            get => m_fromItemExternalId;
            set => m_fromItemExternalId = value;
        }

        private string m_toItemExternalId;
        /// <summary>
        /// The Setup Code of the Operation that may run next.
        /// </summary>
        public string ToItemExternalId
        {
            get => m_toItemExternalId;
            set => m_toItemExternalId = value;
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
        #endregion

        #region Hash Key
        internal string GetHashKey()
        {
            return GetHashKey(FromItemExternalId, ToItemExternalId);
        }

        internal static string GetHashKey(string a_fromItemExternalId, string a_toItemExternalId)
        {
            return "*()#" + a_fromItemExternalId + "(*&" + a_toItemExternalId;
        }
        #endregion
    }
}