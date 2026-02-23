using PT.Scheduler;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Stores a list of Compatibility Code from-to values.
/// </summary>
public class CompatibilityCodeTable : IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 1106;

    public CompatibilityCodeTable(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 1)
        {
            m_bools = new BoolVector32(a_reader);
            m_isSetBools = new BoolVector32(a_reader);
            a_reader.Read(out m_name);
            a_reader.Read(out m_description);
            
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Add(new CompatibilityCodeTableRow(a_reader));
            }

            AssignedResources = new PTLinkedList<ResourceKey>(a_reader);
            AssignedResourcesExternalId = new ResourceKeyExternalIdList(a_reader);
        }
    }

    public virtual void Serialize(IWriter a_writer)
    {
        m_bools.Serialize(a_writer);
        m_isSetBools.Serialize(a_writer);
        a_writer.Write(m_name);
        a_writer.Write(m_description);

        a_writer.Write(Count);
       foreach (CompatibilityCodeTableRow row in m_rowsHash)
       {
           row.Serialize(a_writer);
       }

        AssignedResources.Serialize(a_writer);
        AssignedResourcesExternalId.Serialize(a_writer);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public CompatibilityCodeTable()
    {
    }

    public PTLinkedList<ResourceKey> AssignedResources = new ();
    public ResourceKeyExternalIdList AssignedResourcesExternalId = new ();

    #region bools
    private BoolVector32 m_bools;
    private const short c_allowedListIdx = 0;

    private BoolVector32 m_isSetBools;
    private const short c_descriptionIsSetIdx = 0;
    private const short c_allowedListIsSetIdx = 1;
    #endregion

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
        set
        {
            m_description = value;
            m_isSetBools[c_descriptionIsSetIdx] = true;
        }
    }

    private bool m_allowedList = true;

    public bool DescriptionIsSet => m_isSetBools[c_descriptionIsSetIdx];


    public bool AllowedList
    {
        get => m_bools[c_allowedListIdx];
        set
        {
            m_bools[c_allowedListIdx] = value;
            m_isSetBools[c_allowedListIsSetIdx] = true;
        }
    }

    public bool AllowedListIsSet => m_isSetBools[c_allowedListIsSetIdx];

    #endregion

    #region Hashtable
    private readonly HashSet<CompatibilityCodeTableRow> m_rowsHash = new ();

    public void Add(CompatibilityCodeTableRow a_row)
    {
        m_rowsHash.Add(a_row);
    }

    /// <summary>
    /// Returns an enumeration of CompatibilityCodeTableRows.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<CompatibilityCodeTableRow> GetEnumerator()
    {
        foreach (CompatibilityCodeTableRow compatibilityCodeTableRow in m_rowsHash)
        {
            yield return compatibilityCodeTableRow;
        }
    }

    public int Count => m_rowsHash.Count;
    #endregion

    public class CompatibilityCodeTableRow : IPTSerializable
    {
        #region IPTSerializable Members
        public const int UNIQUE_ID = 1107;

        public CompatibilityCodeTableRow(IReader a_reader)
        {
            if (a_reader.VersionNumber >= 1)
            {
                a_reader.Read(out m_compatibilityCode);
            }
        }

        public virtual void Serialize(IWriter a_writer)
        {
            a_writer.Write(m_compatibilityCode);
        }

        public virtual int UniqueId => UNIQUE_ID;
        #endregion

        public CompatibilityCodeTableRow(string a_compatibilityCode)
        {
            m_compatibilityCode = a_compatibilityCode;
        }

        #region Shared Properties
        private string m_compatibilityCode;

        /// <summary>
        /// The Compatibility Code.
        /// </summary>
        public string CompatibilityCode
        {
            get => m_compatibilityCode;
            set => m_compatibilityCode = value;
        }
        #endregion

    }
}