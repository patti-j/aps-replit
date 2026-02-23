using System.Collections;

using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Scheduler;

/// <summary>
/// Stores a list of Setup Code from-to values.
/// </summary>
public class ItemCleanoutTable : BaseObject, IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 1132;

    internal ItemCleanoutTable(IReader a_reader) : base (a_reader)
    {
        if (a_reader.VersionNumber >= 13000)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_wildcard);

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Add(new ItemCleanoutTableRow(a_reader));
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        a_writer.Write(m_wildcard);
        a_writer.Write(Count);
        foreach (ItemCleanoutTableRow itemCleanoutRow in this)
        {
            itemCleanoutRow.Serialize(a_writer);
        }
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    internal ItemCleanoutTable(BaseId a_id) : base(a_id) 
    {
       
    }

    internal ItemCleanoutTable(BaseId a_id, PT.Transmissions.CleanoutTrigger.ItemCleanoutTable a_table, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges) : base(a_id)
    {
        Update(a_sd, a_table, a_dataChanges);
    }

    internal ItemCleanoutTable(BaseId a_id, ItemCleanoutTable a_sourceTable) : base (a_id)
    {
        Copy(a_sourceTable);
    }

    #region Shared Properties
    
    public override string DefaultNamePrefix => "Item Cleanout Table";
    #endregion

    #region Transmissions
    internal void Update(ScenarioDetail a_sd, PT.Transmissions.CleanoutTrigger.ItemCleanoutTable a_table, IScenarioDataChanges a_dataChanges)
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

        foreach (PT.Transmissions.CleanoutTrigger.ItemCleanoutTable.ItemCleanoutTableRow tRow in a_table.Rows.Values)
        {
            Add(new ItemCleanoutTableRow(tRow));
        }

        UpdateResourceReferences(a_sd, a_table.AssignedResources, a_dataChanges);
    }

    internal void Copy(ItemCleanoutTable a_sourceTable)
    {
        Name = string.Format("Copy of {0}".Localize(), a_sourceTable.Name);
        Description = a_sourceTable.Description;
        Wildcard = a_sourceTable.Wildcard;
        PreviousPrecedence = a_sourceTable.PreviousPrecedence;

        foreach (ItemCleanoutTableRow itemCleanoutTableRow in a_sourceTable)
        {
            Add(new ItemCleanoutTableRow(itemCleanoutTableRow));
        }
    }

    internal void UpdateResourcesForDelete(ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        UpdateResourceReferences(a_sd, new PTLinkedList<ResourceKey>(), a_dataChanges); //unlink all
    }

    private void UpdateResourceReferences(ScenarioDetail a_sd, PTLinkedList<ResourceKey> a_assignedResources, IScenarioDataChanges a_dataChanges)
    {
        for (int plantI = 0; plantI < a_sd.PlantManager.Count; plantI++)
        {
            Plant plant = a_sd.PlantManager[plantI];
            for (int deptI = 0; deptI < plant.Departments.Count; deptI++)
            {
                Department dept = plant.Departments.GetByIndex(deptI);
                for (int resI = 0; resI < dept.Resources.Count; resI++)
                {
                    Resource res = dept.Resources.GetByIndex(resI);
                    if (res.ItemCleanoutTable == this && !a_assignedResources.Contains(res.GetKey()))
                    {
                        //Unlink
                        res.ItemCleanoutTable = null;
                        a_dataChanges.MachineChanges.UpdatedObject(res.Id);
                    }
                    else if (res.ItemCleanoutTable != this && a_assignedResources.Contains(res.GetKey()))
                    {
                        //Link
                        res.ItemCleanoutTable = this;
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
        private set
        {
            m_wildcard = value;
        }
    }

    public bool PreviousPrecedence
    {
        get => m_bools[c_previousPrecedenceIdx];
        private set => m_bools[c_previousPrecedenceIdx] = value;
    }

    private BoolVector32 m_bools;
    private const short c_previousPrecedenceIdx = 0;
    #endregion

    /// <summary>
    /// Returns the amount of clean time if going from an operation with the specified Setup Code
    /// to an Operation of another specified Setup Code.
    /// Returns 0s if the pair of Setup Codes is not found.
    /// </summary>
    internal void LookupCleanout(string a_fromItemExternalId, string a_toItemExternalId, out long a_cleanSpan, out decimal a_cleanCost)
    {
        ItemCleanoutTableRow row = GetRow(a_fromItemExternalId, a_toItemExternalId);
        if (row != null)
        {
            a_cleanSpan = row.Duration.Ticks;
            a_cleanCost = row.Cost;
        }
        else
        {
            a_cleanSpan = 0;
            a_cleanCost = 0m;
        }
    }

    #region Rows
    private readonly Dictionary<string, ItemCleanoutTableRow> m_rowsNoWild = new();
    private readonly Dictionary<string, ItemCleanoutTableRow> m_rowsWildPrevious = new();
    private readonly Dictionary<string, ItemCleanoutTableRow> m_rowsWildNext = new();

    private void Add(ItemCleanoutTableRow a_row)
    {
        string key = a_row.GetHashKey();

        if (string.IsNullOrWhiteSpace(Wildcard))
        {
            m_rowsNoWild.TryAdd(key, a_row);
        }
        else if (a_row.FromItemExternalId == Wildcard)
        {
            m_rowsWildPrevious.TryAdd(key, a_row);
        }
        else if (a_row.ToItemExternalId == Wildcard)
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
    internal ItemCleanoutTableRow GetRow(string a_previousOpCode, string a_nextOpCode)
    {
        //First try exact match without wild cards
        string rowKey = ItemCleanoutTableRow.GetHashKey(a_previousOpCode, a_nextOpCode);
        if (m_rowsNoWild.TryGetValue(rowKey, out ItemCleanoutTableRow row))
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
            rowKey = ItemCleanoutTableRow.GetHashKey(a_previousOpCode, Wildcard);
            if (m_rowsWildNext.TryGetValue(rowKey, out ItemCleanoutTableRow rowWildPrevious))
            {
                return rowWildPrevious;
            }

            //Previous wasn't found, try next precedence
            rowKey = ItemCleanoutTableRow.GetHashKey(Wildcard, a_nextOpCode);
            //Look for Wild To Next row, get No wild if not found
            if (m_rowsWildPrevious.TryGetValue(rowKey, out ItemCleanoutTableRow rowWildNext))
            {
                return rowWildNext;
            }

            rowKey = ItemCleanoutTableRow.GetHashKey(Wildcard, Wildcard);
            if (m_rowsWildNext.TryGetValue(rowKey, out ItemCleanoutTableRow veryWildRow))
            {
                return veryWildRow;
            }
        }
        else
        {
            //Prefer wild cards where the next value is known
            rowKey = ItemCleanoutTableRow.GetHashKey(Wildcard, a_nextOpCode);
            //Look for Wild To Next row, get No wild if not found
            if (m_rowsWildPrevious.TryGetValue(rowKey, out ItemCleanoutTableRow rowWildNext))
            {
                return rowWildNext;
            }

            //Next precedence wasn't found, try previous precedence
            rowKey = ItemCleanoutTableRow.GetHashKey(a_previousOpCode, Wildcard);
            if (m_rowsWildNext.TryGetValue(rowKey, out ItemCleanoutTableRow rowWildPrevious))
            {
                return rowWildPrevious;
            }

            rowKey = ItemCleanoutTableRow.GetHashKey(Wildcard, Wildcard);
            if (m_rowsWildPrevious.TryGetValue(rowKey, out ItemCleanoutTableRow veryWildRow))
            {
                return veryWildRow;
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

    public IEnumerator<ItemCleanoutTableRow> GetEnumerator()
    {
        foreach (ItemCleanoutTableRow row in m_rowsNoWild.Values)
        {
            yield return row;
        }

        foreach (ItemCleanoutTableRow row in m_rowsWildPrevious.Values)
        {
            yield return row;
        }

        foreach (ItemCleanoutTableRow row in m_rowsWildNext.Values)
        {
            yield return row;
        }
    }
    #endregion

    public class ItemCleanoutTableRow : IPTSerializable
    {
        #region IPTSerializable Members
        internal ItemCleanoutTableRow(IReader a_reader)
        {
            a_reader.Read(out m_toItemExternalId);
            a_reader.Read(out m_fromItemExternalId);
            a_reader.Read(out m_duration);
            a_reader.Read(out m_cost);
        }

        public virtual void Serialize(IWriter a_writer)
        {
            a_writer.Write(m_toItemExternalId);
            a_writer.Write(m_fromItemExternalId);
            a_writer.Write(m_duration);
            a_writer.Write(m_cost);
        }

        public virtual int UniqueId => 572;
        #endregion

        internal ItemCleanoutTableRow(ItemCleanoutTableRow a_sourceRow)
        {
            m_fromItemExternalId = a_sourceRow.FromItemExternalId;
            m_toItemExternalId = a_sourceRow.ToItemExternalId;
            m_cost = a_sourceRow.Cost;
            m_duration = a_sourceRow.Duration;
        }

        internal ItemCleanoutTableRow(PT.Transmissions.CleanoutTrigger.ItemCleanoutTable.ItemCleanoutTableRow a_sourceRow)
        {
            m_fromItemExternalId = a_sourceRow.FromItemExternalId;
            m_toItemExternalId = a_sourceRow.ToItemExternalId;
            m_cost = a_sourceRow.Cost;
            m_duration = a_sourceRow.Duration;
        }

        #region Shared Properties
        private readonly string m_fromItemExternalId;

        /// <summary>
        /// The external Id of the item we are changing over from
        /// </summary>
        public string FromItemExternalId
        {
            get => m_fromItemExternalId;
        }

        private readonly string m_toItemExternalId;

        /// <summary>
        /// The external Id of the item we are changing over to
        /// </summary>
        public string ToItemExternalId
        {
            get => m_toItemExternalId;
        }

        private readonly TimeSpan m_duration;

        /// <summary>
        /// The amount of time to schedule a cleanout after an item changeover.
        /// </summary>
        public TimeSpan Duration
        {
            get => m_duration;
        }

        private readonly decimal m_cost;

        /// <summary>
        /// The financial cost incurred when changing over from the Previous Op to the Next Op.
        /// </summary>
        public decimal Cost
        {
            get => m_cost;
        }
        #endregion

        #region Hash Key
        internal string GetHashKey()
        {
            return GetHashKey(FromItemExternalId, ToItemExternalId);
        }

        internal static string GetHashKey(string a_fromItemExternalId, string a_toItemExternalId)
        {
            return "#$(*)" + a_fromItemExternalId + "(*&" + a_toItemExternalId;
        }
        #endregion
    }
}