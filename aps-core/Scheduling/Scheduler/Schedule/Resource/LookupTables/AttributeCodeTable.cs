using System.Collections;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Stores a list of Setup Code from-to values.
/// </summary>
public class AttributeCodeTable : IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 571;

    public AttributeCodeTable(IReader a_reader)
    {
        if (a_reader.VersionNumber >= 12417)
        {
            m_bools = new BoolVector32(a_reader);
            m_id = new BaseId(a_reader);
            a_reader.Read(out m_name);
            a_reader.Read(out m_description);
            a_reader.Read(out m_wildcard);

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Add(new AttributeCodeTableRow(a_reader));
            }
        }
        else if (a_reader.VersionNumber >= 12303)
        {
            m_id = new BaseId(a_reader);
            a_reader.Read(out m_name);
            a_reader.Read(out m_description);

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Add(new AttributeCodeTableRow(a_reader));
            }
        }
        else if (a_reader.VersionNumber >= 1)
        {
            m_id = new BaseId(a_reader);
            a_reader.Read(out m_name);
            a_reader.Read(out m_description);

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                //Attributes are not backwards compatible
                new AttributeCodeTableRow(a_reader);
            }
        }
    }

    public virtual void Serialize(IWriter a_writer)
    {
#if DEBUG
        a_writer.DuplicateErrorCheck(this);
#endif
        m_bools.Serialize(a_writer);
        m_id.Serialize(a_writer);
        a_writer.Write(m_name);
        a_writer.Write(m_description);
        a_writer.Write(m_wildcard);
        a_writer.Write(Count);
        foreach (AttributeCodeTableRow attributeCodeTableRow in this)
        {
            attributeCodeTableRow.Serialize(a_writer);
        }
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public AttributeCodeTable()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public AttributeCodeTable(BaseId id, PT.Transmissions.AttributeCodeTable table, ScenarioDetail sd, AttributeManager a_attributeManager, IScenarioDataChanges a_dataChanges)
    {
        Id = id;
        Update(sd, table, a_attributeManager, a_dataChanges);
    }

    public AttributeCodeTable(BaseId id, AttributeCodeTable sourceTable)
    {
        Id = id;
        Copy(sourceTable);
    }

    public AttributeCodeTable(BaseId a_id, SetupCodeTable a_tableToConvert, AttributeManager a_attributeManager)
    {
        Id = a_id;

        Name = string.Format("Copy of {0}".Localize(), a_tableToConvert.Name);
        Description = a_tableToConvert.Description;
        Wildcard = a_tableToConvert.Wildcard;
        PreviousPrecedence = a_tableToConvert.PreviousPrecedence;
        string ptAttributeExternalId = a_attributeManager.AddAttributeForBackwardsCompatibility();

        foreach (SetupCodeTable.SetupCodeTableRow setupCodeTableRow in a_tableToConvert)
        {

            AttributeCodeTableRow convertedRow = new AttributeCodeTableRow(setupCodeTableRow, ptAttributeExternalId);
            Add(convertedRow);
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
    #endregion

    #region Transmissions
    public void Update(ScenarioDetail a_sd, PT.Transmissions.AttributeCodeTable a_table, AttributeManager a_attributeManager, IScenarioDataChanges a_dataChanges)
    {
        //Clear out the old rows and add the new ones
        Clear();
        Name = a_table.Name;
        Description = a_table.Description;
        if (a_table.WildcardIsSet)
        {
            Wildcard = a_table.Wildcard;
        }

        if (a_table.PreviousPrecedenceIsSet)
        {
            PreviousPrecedence = a_table.PreviousPrecedence;
        }

        try
        {
            a_attributeManager.InitFastLookupByExternalId();
            foreach (PT.Transmissions.AttributeCodeTable.AttributeCodeTableRow tRow in a_table.Rows.Values)
            {
                if (a_attributeManager.GetByExternalId(tRow.AttributeExternalId) == null)
                {
                    //TODO: Make new validation exception
                    throw new PTValidationException($"Attribute Code Table row is referencing an attribute that does not exist with ExternalId {tRow.AttributeExternalId}");
                }

                Add(new AttributeCodeTableRow(tRow));
            }

            UpdateResourceReferences(a_sd, a_table.AssignedResources, a_dataChanges);
        }
        finally
        {
            a_attributeManager.DeInitFastLookupByExternalId();
        }
    }

    public void Copy(AttributeCodeTable sourceTable)
    {
        Name = string.Format("Copy of {0}".Localize(), sourceTable.Name);
        Description = sourceTable.Description;
        Wildcard = sourceTable.Wildcard;
        PreviousPrecedence = sourceTable.PreviousPrecedence;

        IEnumerator<AttributeCodeTableRow> enumerator = sourceTable.GetEnumerator();
        while (enumerator.MoveNext())
        {
            AttributeCodeTableRow row = enumerator.Current;
            Add(new AttributeCodeTableRow(row));
        }
    }

    internal void UpdateResourcesForDelete(ScenarioDetail sd, IScenarioDataChanges a_dataChanges)
    {
        UpdateResourceReferences(sd, new ResourceKeyList(), a_dataChanges); //unlink all
    }

    private void UpdateResourceReferences(ScenarioDetail sd, ResourceKeyList assignedResources, IScenarioDataChanges a_dataChanges)
    {
        //Get a hashtable to use when determining which Resources should be linked
        Hashtable resourceKeyListNodeHash = assignedResources.GetHashtable();

        for (int plantI = 0; plantI < sd.PlantManager.Count; plantI++)
        {
            Plant plant = sd.PlantManager[plantI];
            for (int deptI = 0; deptI < plant.Departments.Count; deptI++)
            {
                Department dept = plant.Departments.GetByIndex(deptI);
                for (int resI = 0; resI < dept.Resources.Count; resI++)
                {
                    Resource res = dept.Resources.GetByIndex(resI);
                    if (res.AttributeCodeTable == this && !resourceKeyListNodeHash.Contains(res.GetKey()))
                    {
                        //Unlink
                        res.AttributeCodeTable = null;
                        a_dataChanges.MachineChanges.UpdatedObject(res.Id);
                    }
                    else if (res.AttributeCodeTable != this && resourceKeyListNodeHash.Contains(res.GetKey()))
                    {
                        //Link
                        res.AttributeCodeTable = this;
                        a_dataChanges.MachineChanges.UpdatedObject(res.Id);
                    }
                }
            }
        }
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

    public bool PreviousPrecedence
    {
        get => m_bools[c_previousPrecedenceIdx];
        set => m_bools[c_previousPrecedenceIdx] = value;
    }

    private BoolVector32 m_bools;
    private const short c_previousPrecedenceIdx = 0;
    #endregion

    #region Setup Calculations
    /// <summary>
    /// Returns the amount of setup time if going from an operation with the specified Setup Code
    /// to an Operation of another specified Setup Code.
    /// Returns 0s if the pair of Setup Codes is not found.
    /// </summary>
    public void LookupSetup(string attributeName, string previousOpCode, string nextOpCode, out long setupSpan, out decimal setupCost)
    {
        AttributeCodeTableRow row = GetRow(attributeName, previousOpCode, nextOpCode);
        if (row != null)
        {
            setupSpan = row.Duration.Ticks;
            setupCost = row.Cost;
        }
        else
        {
            setupSpan = 0;
            setupCost = 0m;
        }
    }
    #endregion

    /// <summary>
    /// Returns the amount of clean time if going from an operation with the specified Setup Code
    /// to an Operation of another specified Setup Code.
    /// Returns 0s if the pair of Setup Codes is not found.
    /// </summary>
    public void LookupCleanout(string a_attributeExternalId, string a_previousOpCode, string a_nextOpCode, out long a_cleanSpan, out decimal a_cleanCost, out int a_cleanoutGrade)
    {
        AttributeCodeTableRow row = GetRow(a_attributeExternalId, a_previousOpCode, a_nextOpCode);
        if (row != null)
        {
            a_cleanSpan = row.Duration.Ticks;
            a_cleanCost = row.Cost;
            a_cleanoutGrade = row.CleanoutGrade;
        }
        else
        {
            a_cleanSpan = 0;
            a_cleanCost = 0m;
            a_cleanoutGrade = 0;
        }
    }

    #region Rows
    private readonly Dictionary<string, AttributeCodeTableRow> m_rowsNoWild = new();
    private readonly Dictionary<string, AttributeCodeTableRow> m_rowsWildPrevious = new();
    private readonly Dictionary<string, AttributeCodeTableRow> m_rowsWildNext = new();

    private void Add(AttributeCodeTableRow a_row)
    {
        string key = a_row.GetHashKey();

        if (string.IsNullOrWhiteSpace(Wildcard))
        {
            m_rowsNoWild.TryAdd(key, a_row);
        }
        else if (a_row.PreviousOpAttributeCode == Wildcard)
        {
            m_rowsWildPrevious.TryAdd(key, a_row);
        }
        else if (a_row.NextOpAttributeCode == Wildcard)
        {
            m_rowsWildNext.TryAdd(key, a_row);
        }
        else
        {
            m_rowsNoWild.TryAdd(key, a_row);
        }
    }

    /// <summary>
    /// Returns the row defined by these codes.  Returns null if no such row exists.
    /// </summary>
    public AttributeCodeTableRow GetRow(string a_attributeExternalId, string a_previousOpCode, string a_nextOpCode)
    {
        //First try exact match without wild cards
        string rowKey = AttributeCodeTableRow.GetHashKey(a_attributeExternalId, a_previousOpCode, a_nextOpCode);
        if (m_rowsNoWild.TryGetValue(rowKey, out AttributeCodeTableRow row))
        {
            return row;
        }

        if (string.IsNullOrWhiteSpace(Wildcard))
        {
            //We aren't using wild card, and the row wasn't found
            return null;
        }

        if (PreviousPrecedence) //Prefer wild cards where the previous value is known
        {
            rowKey = AttributeCodeTableRow.GetHashKey(a_attributeExternalId, a_previousOpCode, Wildcard);
            if (m_rowsWildNext.TryGetValue(rowKey, out AttributeCodeTableRow rowWildPrevious))
            {
                return rowWildPrevious;
            }

            //Previous wasn't found, try next precedence
            rowKey = AttributeCodeTableRow.GetHashKey(a_attributeExternalId, Wildcard, a_nextOpCode);
            //Look for Wild To Next row, get No wild if not found
            if (m_rowsWildPrevious.TryGetValue(rowKey, out AttributeCodeTableRow rowWildNext))
            {
                return rowWildNext;
            }
        }
        else
        {
            //Prefer wild cards where the next value is known
            rowKey = AttributeCodeTableRow.GetHashKey(a_attributeExternalId, Wildcard, a_nextOpCode);
            //Look for Wild To Next row, get No wild if not found
            if (m_rowsWildPrevious.TryGetValue(rowKey, out AttributeCodeTableRow rowWildNext))
            {
                return rowWildNext;
            }

            //Next precedence wasn't found, try previous precedence
            rowKey = AttributeCodeTableRow.GetHashKey(a_attributeExternalId, a_previousOpCode, Wildcard);
            if (m_rowsWildNext.TryGetValue(rowKey, out AttributeCodeTableRow rowWildPrevious))
            {
                return rowWildPrevious;
            }
        }
        
        return null;
    }

    public int Count => m_rowsNoWild.Count + m_rowsWildPrevious.Count + m_rowsWildNext.Count;


    private void Clear()
    {
        m_rowsNoWild.Clear();
        m_rowsWildPrevious.Clear();
        m_rowsWildNext.Clear();
    }

    public IEnumerator<AttributeCodeTableRow> GetEnumerator()
    {
        foreach (AttributeCodeTableRow row in m_rowsNoWild.Values)
        {
            yield return row;
        }

        foreach (AttributeCodeTableRow row in m_rowsWildPrevious.Values)
        {
            yield return row;
        }

        foreach (AttributeCodeTableRow row in m_rowsWildNext.Values)
        {
            yield return row;
        }
    }
    #endregion

    public class AttributeCodeTableRow : IPTSerializable
    {
        #region IPTSerializable Members
        public AttributeCodeTableRow(IReader a_reader)
        {
            if (a_reader.VersionNumber >= 12302)
            {
                a_reader.Read(out m_ptAttributeExternalId);
                a_reader.Read(out m_nextOpAttributeCode);
                a_reader.Read(out m_previousOpAttributeCode);
                a_reader.Read(out m_duration);
                a_reader.Read(out m_cost);
                a_reader.Read(out m_cleanoutGrade);
            }
            else if (a_reader.VersionNumber >= 1)
            {
                a_reader.Read(out m_ptAttributeExternalId);
                a_reader.Read(out m_nextOpAttributeCode);
                a_reader.Read(out m_previousOpAttributeCode);
                a_reader.Read(out m_cost);
                a_reader.Read(out m_duration);
            }
        }

        public virtual void Serialize(IWriter a_writer)
        {
#if DEBUG
            a_writer.DuplicateErrorCheck(this);
#endif
            a_writer.Write(m_ptAttributeExternalId);
            a_writer.Write(m_nextOpAttributeCode);
            a_writer.Write(m_previousOpAttributeCode);
            a_writer.Write(m_duration);
            a_writer.Write(m_cost);
            a_writer.Write(m_cleanoutGrade);
        }

        public virtual int UniqueId => 572;
        #endregion

        public AttributeCodeTableRow(AttributeCodeTableRow a_sourceRow)
        {
            m_ptAttributeExternalId = a_sourceRow.PTAttributeExternalId;
            m_previousOpAttributeCode = a_sourceRow.PreviousOpAttributeCode;
            m_nextOpAttributeCode = a_sourceRow.NextOpAttributeCode;
            m_cost = a_sourceRow.Cost;
            m_duration = a_sourceRow.Duration;
            m_cleanoutGrade = a_sourceRow.CleanoutGrade;
        }

        public AttributeCodeTableRow(PT.Transmissions.AttributeCodeTable.AttributeCodeTableRow a_sourceRow)
        {
            m_ptAttributeExternalId = a_sourceRow.AttributeExternalId;
            m_previousOpAttributeCode = a_sourceRow.PreviousOpAttributeCode;
            m_nextOpAttributeCode = a_sourceRow.NextOpAttributeCode;
            m_cost = a_sourceRow.Cost;
            m_duration = a_sourceRow.Duration;
            m_cleanoutGrade = a_sourceRow.CleanoutGrade;
        }

        /// <summary>
        /// This constructor is to be used only for converting old SetupCodeTableRows in Backwards Compatibility
        /// </summary>
        /// <param name="a_rowToConvert"></param>
        /// <param name="a_ptAttributeExternalId"></param>
        public AttributeCodeTableRow(SetupCodeTable.SetupCodeTableRow a_rowToConvert, string a_ptAttributeExternalId)
        {
            m_ptAttributeExternalId = a_ptAttributeExternalId;
            m_previousOpAttributeCode = a_rowToConvert.PreviousOpSetupCode;
            m_nextOpAttributeCode = a_rowToConvert.NextOpSetupCode;
            m_cost = a_rowToConvert.SetupCost;
            m_duration = a_rowToConvert.SetupTime;
        }

        #region Shared Properties
        private string m_ptAttributeExternalId;

        /// <summary>
        /// The row is to be used for calculating setup using the specified Attribute.
        /// </summary>
        public string PTAttributeExternalId
        {
            get => m_ptAttributeExternalId;
            set => m_ptAttributeExternalId = value;
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
            return GetHashKey(PTAttributeExternalId, PreviousOpAttributeCode, NextOpAttributeCode);
        }

        internal static string GetHashKey(string a_attributeExternalId, string previousOpAttributeCode, string nextOpAttributeCode)
        {
            return a_attributeExternalId + "#$(*)" + previousOpAttributeCode + "(*&" + nextOpAttributeCode;
        }
        #endregion
    }
}