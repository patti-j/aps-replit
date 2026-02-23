using System.Collections;

using PT.APSCommon;
using PT.APSCommon.Collections;
using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;
using static PT.Scheduler.CompatibilityCodeTable;

namespace PT.Scheduler;

/// <summary>
/// Stores a list of Setup Code from-to values.
/// </summary>
public partial class CompatibilityCodeTable : BaseObject, IPTSerializable, IEnumerable<CompatibilityCodeTableRow>
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 551;

    public CompatibilityCodeTable(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12320)
        {
            m_bools = new BoolVector32(a_reader);
            m_rows = new SortedCompatibilityCodeRowCollection(a_reader);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
#if DEBUG
        a_writer.DuplicateErrorCheck(this);
#endif
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);
        m_rows.Serialize(a_writer);
    }

    public override int UniqueId => 1104;
    #endregion

    public CompatibilityCodeTable(BaseId a_id) : base(a_id)
    {

    }

    private BoolVector32 m_bools;

    private const short c_allowListIdx = 0;

    public bool AllowList
    {
        get { return m_bools[c_allowListIdx]; }
        private set { m_bools[c_allowListIdx] = value; }
    }

    public CompatibilityCodeTable(BaseId id, Transmissions.CompatibilityCodeTable tTable, ScenarioDetail sd, IScenarioDataChanges a_dataChanges)
        : base(id)
    {
        Update(sd, tTable, a_dataChanges);
    }

    public CompatibilityCodeTable(BaseId id, CompatibilityCodeTable sourceTable)
        : base(id)
    {
        Copy(sourceTable);
    }

    public override string DefaultNamePrefix => "Compatibility Code Table";

    #region Transmissions
    public void Update(ScenarioDetail a_sd, PT.Transmissions.CompatibilityCodeTable a_table, IScenarioDataChanges a_dataChanges)
    {
        //Clear out the old rows and add the new ones
        Clear();
        Name = a_table.Name;

        if (a_table.DescriptionIsSet)
        {
            Description = a_table.Description;
        }

        if (a_table.AllowedListIsSet)
        {
            AllowList = a_table.AllowedList;
        }

        foreach (Transmissions.CompatibilityCodeTable.CompatibilityCodeTableRow row in a_table.GetEnumerator())
        {
            Add(new CompatibilityCodeTableRow(row));
        }

        UpdateResourceReferences(a_sd, a_table.AssignedResources, a_dataChanges);
    }

    private void Copy(CompatibilityCodeTable a_sourceTable)
    {
        Name = string.Format("Copy of {0}".Localize(), a_sourceTable.Name);
        Description = a_sourceTable.Description;

        foreach (CompatibilityCodeTableRow row in a_sourceTable.m_rows)
        {
            m_rows.Add(row);
        }
    }

    internal void UpdateResourcesForDelete(ScenarioDetail sd, IScenarioDataChanges a_dataChanges)
    {
        UpdateResourceReferences(sd, new PTLinkedList<ResourceKey>(), a_dataChanges); //unlink all
    }

    private void UpdateResourceReferences(ScenarioDetail sd, PTLinkedList<ResourceKey> assignedResources, IScenarioDataChanges a_dataChanges)
    {
        //Get a hashtable to use when determining which Resources should be linked
        HashSet<ResourceKey> resourceKeyListNodeHash = assignedResources.ToHashSet();

        for (int plantI = 0; plantI < sd.PlantManager.Count; plantI++)
        {
            Plant plant = sd.PlantManager[plantI];
            for (int deptI = 0; deptI < plant.Departments.Count; deptI++)
            {
                Department dept = plant.Departments.GetByIndex(deptI);
                for (int resI = 0; resI < dept.Resources.Count; resI++)
                {
                    Resource res = dept.Resources.GetByIndex(resI);
                    if (res.CompatibilityTables != null && res.CompatibilityTables.Any(t => t.Id == Id) && !resourceKeyListNodeHash.Contains(res.GetKey()))
                    {
                        //Unlink
                        res.RemoveCompatibilityCode(this);
                        a_dataChanges.MachineChanges.UpdatedObject(res.Id);
                    }
                    else if ((res.CompatibilityTables == null || !res.CompatibilityTables.Contains(this)) && resourceKeyListNodeHash.Contains(res.GetKey()))
                    {
                        //Link
                        res.AddCompatibilityCode(this);
                        a_dataChanges.MachineChanges.UpdatedObject(res.Id);
                    }
                }
            }
        }
    }
    #endregion

    #region Rows
    private readonly SortedCompatibilityCodeRowCollection m_rows = new();

    private void Add(CompatibilityCodeTableRow a_row)
    {
        m_rows.Add(a_row);
    }

    public int Count => m_rows.Count;

    private void Clear()
    {
        m_rows.Clear();
    }

    public bool Contains(string a_code)
    {
        return m_rows.ContainsValue(new CompatibilityCodeTableRow(a_code));
    }

    #endregion

    public IEnumerator<CompatibilityCodeTableRow> GetEnumerator()
    {
        foreach (CompatibilityCodeTableRow row in m_rows)
        {
            yield return row;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public class CompatibilityCodeTableRow : IPTSerializable, IComparable<CompatibilityCodeTableRow>
    {
        #region IPTSerializable Members
        public CompatibilityCodeTableRow(IReader a_reader)
        {
            if (a_reader.VersionNumber >= 1)
            {
                a_reader.Read(out m_compatibilityCode);
            }
        }

        public virtual void Serialize(IWriter a_writer)
        {
#if DEBUG
            a_writer.DuplicateErrorCheck(this);
#endif
            a_writer.Write(m_compatibilityCode);
        }

        public int UniqueId => 1108;
        #endregion

        internal CompatibilityCodeTableRow(string a_compatibilityCode)
        {
            m_compatibilityCode = a_compatibilityCode;
        }

        public CompatibilityCodeTableRow(CompatibilityCodeTableRow a_sourceRow)
        {
            m_compatibilityCode = a_sourceRow.CompatibilityCode;
        }

        public CompatibilityCodeTableRow(PT.Transmissions.CompatibilityCodeTable.CompatibilityCodeTableRow a_sourceRow)
        {
            m_compatibilityCode = a_sourceRow.CompatibilityCode;
        }

        #region Shared Properties
        private readonly string m_compatibilityCode;

        /// <summary>
        /// The compatibility code
        /// </summary>
        public string CompatibilityCode => m_compatibilityCode;
        #endregion

        #region Hash Key
        public override int GetHashCode()
        {
            return m_compatibilityCode.GetHashCode();
        }

        public override bool Equals(object a_obj)
        {
            if (a_obj is CompatibilityCodeTableRow row)
            {
                if (m_compatibilityCode == row.m_compatibilityCode)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion

        public int CompareTo(CompatibilityCodeTableRow a_other)
        {
            if (ReferenceEquals(this, a_other))
            {
                return 0;
            }

            if (ReferenceEquals(null, a_other))
            {
                return 1;
            }

            return string.Compare(m_compatibilityCode, a_other.m_compatibilityCode, StringComparison.Ordinal);
        }
    }

    internal class SortedCompatibilityCodeRowCollection : CustomSortedCollection<CompatibilityCodeTableRow>
    {
        internal SortedCompatibilityCodeRowCollection() : base()
        {
        }

        internal SortedCompatibilityCodeRowCollection(IReader a_reader) : base(a_reader)
        {
        }

        protected override CompatibilityCodeTableRow CreateInstance(IReader a_reader)
        {
            return new CompatibilityCodeTableRow(a_reader);
        }
    }
}