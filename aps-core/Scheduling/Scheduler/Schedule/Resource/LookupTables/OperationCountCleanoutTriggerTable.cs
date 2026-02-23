using System.Collections;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.Schedule.Resource.LookupTables;

public class OperationCountCleanoutTriggerTable : BaseCleanoutTriggerTable, IEnumerable<OperationCountCleanoutTriggerTableRow>
{
    #region IPTSerializable Members
    public OperationCountCleanoutTriggerTable(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12305)
        {
            a_reader.Read(out int count);
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

    public new const int UNIQUE_ID = 1059;
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    public OperationCountCleanoutTriggerTable(BaseId a_id) : base(a_id) { }

    internal OperationCountCleanoutTriggerTable(BaseId a_id, PT.Transmissions.CleanoutTrigger.OperationCountCleanoutTriggerTable a_tTable, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
        : base(a_id)
    {
        Id = a_id;
        Update(a_sd, a_tTable, a_dataChanges);
    }

    internal OperationCountCleanoutTriggerTable(BaseId a_id, OperationCountCleanoutTriggerTable a_sourceTable)
        : base(a_id)
    {
        Id = a_id;
        Copy(a_sourceTable);
    }
    #endregion

    #region Shared Properties
    public override string DefaultNamePrefix => "Operation Count Cleanout Trigger";
    #endregion

    #region Transmissions
    internal void Update(ScenarioDetail a_sd, PT.Transmissions.CleanoutTrigger.OperationCountCleanoutTriggerTable a_table, IScenarioDataChanges a_dataChanges)
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
            PT.Transmissions.CleanoutTrigger.OperationCountCleanoutTriggerTableRow row = (PT.Transmissions.CleanoutTrigger.OperationCountCleanoutTriggerTableRow)de.Value;
            Add(new OperationCountCleanoutTriggerTableRow(row));
        }

        UpdateResourceReferences(a_sd, a_table.AssignedResources, a_dataChanges);
    }

    private void Copy(OperationCountCleanoutTriggerTable a_sourceTable)
    {
        Name = string.Format("Copy of {0}".Localize(), a_sourceTable.Name);
        Description = a_sourceTable.Description;

        foreach (OperationCountCleanoutTriggerTableRow row in a_sourceTable.m_rows.Values)
        {
            OperationCountCleanoutTriggerTableRow copyRow = new (row);
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
                    if (res.OperationCountCleanoutTriggerTable != null && res.OperationCountCleanoutTriggerTable.Id == Id && !resourceKeyListNodeHash.Contains(res.GetKey()))
                    {
                        //Unlink
                        res.OperationCountCleanoutTriggerTable = null;
                        a_dataChanges.MachineChanges.UpdatedObject(res.Id);
                    }
                    else if ((res.OperationCountCleanoutTriggerTable == null || res.OperationCountCleanoutTriggerTable.Id != Id) && resourceKeyListNodeHash.Contains(res.GetKey()))
                    {
                        //Link
                        res.OperationCountCleanoutTriggerTable = this;
                        a_dataChanges.MachineChanges.UpdatedObject(res.Id);
                    }
                }
            }
        }
    }
    #endregion

    #region Rows
    private readonly Dictionary<OperationCountCleanoutTriggerTableRow, OperationCountCleanoutTriggerTableRow> m_rows = new ();

    private void Add(OperationCountCleanoutTriggerTableRow a_row)
    {
        m_rows.Add(a_row, a_row);
    }

    public int Count => m_rows.Count;

    private void Clear()
    {
        m_rows.Clear();
    }

    public IEnumerator<OperationCountCleanoutTriggerTableRow> GetEnumerator()
    {
        foreach (OperationCountCleanoutTriggerTableRow row in m_rows.Values)
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

public class OperationCountCleanoutTriggerTableRow : BaseCleanoutTriggerTable.BaseCleanoutTriggerTableRow, IPTSerializable
{
    #region IPTSerializable Members
    public OperationCountCleanoutTriggerTableRow(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12305)
        {
            a_reader.Read(out m_triggerValue);
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        base.Serialize(a_writer);
        a_writer.Write(m_triggerValue);
    }

    public const int UNIQUE_ID = 1063;
    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public OperationCountCleanoutTriggerTableRow(OperationCountCleanoutTriggerTableRow a_sourceRow)
        : base(a_sourceRow)
    {
        m_triggerValue = a_sourceRow.TriggerValue;
    }

    public OperationCountCleanoutTriggerTableRow(PT.Transmissions.CleanoutTrigger.OperationCountCleanoutTriggerTableRow a_sourceRow)
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
    public override int GetHashCode()
    {
        return m_triggerValue.GetHashCode();
    }

    public override bool Equals(object a_obj)
    {
        if (a_obj is OperationCountCleanoutTriggerTableRow row)
        {
            if (m_triggerValue == row.m_triggerValue)
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}