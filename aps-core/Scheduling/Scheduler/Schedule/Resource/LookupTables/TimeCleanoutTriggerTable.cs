using System.Collections;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.Schedule.Resource.LookupTables;

public class TimeCleanoutTriggerTable : BaseCleanoutTriggerTable, IEnumerable<TimeCleanoutTriggerTableRow>
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

    public new const int UNIQUE_ID = 1057;
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    public TimeCleanoutTriggerTable(BaseId a_id) : base(a_id) { }

    internal TimeCleanoutTriggerTable(BaseId a_id, Transmissions.CleanoutTrigger.TimeCleanoutTriggerTable a_tTable, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
        : base(a_id)
    {
        Id = a_id;
        Update(a_sd, a_tTable, a_dataChanges);
    }

    internal TimeCleanoutTriggerTable(BaseId a_id, TimeCleanoutTriggerTable a_sourceTable)
        : base(a_id)
    {
        Id = a_id;
       Copy(a_sourceTable);
    }
    #endregion

    #region Transmissions
    internal void Update(ScenarioDetail a_sd, PT.Transmissions.CleanoutTrigger.TimeCleanoutTriggerTable a_table, IScenarioDataChanges a_dataChanges)
    {
        //Clear out the old rows and add the new ones
        Clear();
        Name = a_table.Name;
        if (a_table.DescriptionSet)
        {
            Description = a_table.Description;
        }

        IDictionaryEnumerator enumerator = a_table.GetEnumerator();
        while (enumerator.MoveNext())
        {
            DictionaryEntry de = (DictionaryEntry)enumerator.Current;
            PT.Transmissions.CleanoutTrigger.TimeCleanoutTriggerTableRow row = (PT.Transmissions.CleanoutTrigger.TimeCleanoutTriggerTableRow)de.Value;
            Add(new TimeCleanoutTriggerTableRow(row));
        }

        UpdateResourceReferences(a_sd, a_table.AssignedResources, a_dataChanges);
    }

    private void Copy(TimeCleanoutTriggerTable a_sourceTable)
    {
        Name = string.Format("Copy of {0}".Localize(), a_sourceTable.Name);
        Description = a_sourceTable.Description;

        foreach (TimeCleanoutTriggerTableRow row in a_sourceTable.m_rows.Values)
        {
            TimeCleanoutTriggerTableRow copyRow = new(row);
            m_rows.Add(copyRow, copyRow);
        }
    }

    internal override void UpdateResourcesForDelete(ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
    {
        UpdateResourceReferences(a_sd, new ResourceKeyList(), a_dataChanges); //unlink all
    }
    
    private void UpdateResourceReferences(ScenarioDetail a_sd, ResourceKeyList a_assignedResources, IScenarioDataChanges a_dataChanges)
    {
        //Get a hashtable to use when determining which Resources should be linked
        Hashtable resourceKeyListNodeHash = a_assignedResources.GetHashtable();

        for (int plantI = 0; plantI < a_sd.PlantManager.Count; plantI++)
        {
            Plant plant = a_sd.PlantManager[plantI];
            for (int deptI = 0; deptI < plant.Departments.Count; deptI++)
            {
                Department dept = plant.Departments.GetByIndex(deptI);
                for (int resI = 0; resI < dept.Resources.Count; resI++)
                {
                    Scheduler.Resource res = dept.Resources.GetByIndex(resI);
                    if (res.TimeCleanoutTriggerTable != null && res.TimeCleanoutTriggerTable.Id == Id && !resourceKeyListNodeHash.Contains(res.GetKey()))
                    {
                        //Unlink
                        res.TimeCleanoutTriggerTable = null;
                        a_dataChanges.MachineChanges.UpdatedObject(res.Id);
                    }
                    else if ((res.TimeCleanoutTriggerTable == null || res.TimeCleanoutTriggerTable.Id != Id) && resourceKeyListNodeHash.Contains(res.GetKey()))
                    {
                        //Link
                        res.TimeCleanoutTriggerTable = this;
                        a_dataChanges.MachineChanges.UpdatedObject(res.Id);
                    }
                }
            }
        }
    }
    #endregion

    #region Shared Properties
    public override string DefaultNamePrefix => "Fixed Time Cleanout Trigger";
    #endregion

    #region Rows
    private readonly Dictionary<TimeCleanoutTriggerTableRow, TimeCleanoutTriggerTableRow> m_rows = new ();

    private void Add(TimeCleanoutTriggerTableRow a_row)
    {
        m_rows.Add(a_row, a_row);
    }

    public int Count => m_rows.Count;

    private void Clear()
    {
        m_rows.Clear();
    }

    public IEnumerator<TimeCleanoutTriggerTableRow> GetEnumerator()
    {
        foreach (TimeCleanoutTriggerTableRow row in m_rows.Values)
        {
            yield return row;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    #endregion
}

public class TimeCleanoutTriggerTableRow : BaseCleanoutTriggerTable.BaseCleanoutTriggerTableRow, IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 1061;

    public TimeCleanoutTriggerTableRow(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12305)
        {
            m_bools = new BoolVector32(a_reader);
            a_reader.Read(out m_triggerValue);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        base.Serialize(a_writer);
        m_bools.Serialize(a_writer);
        a_writer.Write(m_triggerValue);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public TimeCleanoutTriggerTableRow(TimeCleanoutTriggerTableRow a_sourceRow)
        : base(a_sourceRow)
    {
        m_bools = a_sourceRow.m_bools;
        m_triggerValue = a_sourceRow.TriggerValue;
    }

    public TimeCleanoutTriggerTableRow(Transmissions.CleanoutTrigger.TimeCleanoutTriggerTableRow a_sourceRow)
        : base(a_sourceRow)
    {
        m_bools = a_sourceRow.m_bools;
        m_triggerValue = a_sourceRow.TriggerValue;
    }

    #region Shared Properties
    private BoolVector32 m_bools;

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
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = m_triggerValue.GetHashCode();
            hash = (hash * 397) ^ m_bools.GetHashCode();
            return hash;
        }
    }

    public override bool Equals(object a_obj)
    {
        if (a_obj is TimeCleanoutTriggerTableRow row)
        {
            if (m_triggerValue.Ticks == row.m_triggerValue.Ticks
                && UseProcessingTime == row.UseProcessingTime
                && UsePostprocessingTime == row.UseProcessingTime)
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}