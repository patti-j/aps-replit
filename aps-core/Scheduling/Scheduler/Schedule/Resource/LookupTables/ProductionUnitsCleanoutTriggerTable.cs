using System.Collections;

using PT.APSCommon;
using PT.APSCommon.Extensions;
using PT.SchedulerDefinitions;

namespace PT.Scheduler.Schedule.Resource.LookupTables;

public class ProductionUnitsCleanoutTriggerTable : BaseCleanoutTriggerTable, IEnumerable<ProductionUnitsCleanoutTriggerTableRow>
{
    #region IPTSerializable Members
    public ProductionUnitsCleanoutTriggerTable(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12305)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                Add(new ProductionUnitsCleanoutTriggerTableRow(a_reader));
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
        foreach (ProductionUnitsCleanoutTriggerTableRow row in m_rows.Values)
        {
            row.Serialize(a_writer);
        }
    }

    public int Count => m_rows.Count;

    public new const int UNIQUE_ID = 1058;
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    public ProductionUnitsCleanoutTriggerTable(BaseId a_id) : base(a_id) { }

    internal ProductionUnitsCleanoutTriggerTable(BaseId a_id, PT.Transmissions.CleanoutTrigger.ProductionUnitsCleanoutTriggerTable a_tTable, ScenarioDetail a_sd, IScenarioDataChanges a_dataChanges)
        : base(a_id)
    {
        Id = a_id;
        Update(a_sd, a_tTable, a_dataChanges);
    }

    internal ProductionUnitsCleanoutTriggerTable(BaseId a_id, ProductionUnitsCleanoutTriggerTable a_sourceTable)
        : base(a_id)
    {
        Id = a_id;
        Copy(a_sourceTable);
    }
    #endregion

    #region Shared Properties
    public override string DefaultNamePrefix => "Production Units Cleanout TriggerTable";
    #endregion

    #region Transmissions
    internal void Update(ScenarioDetail a_sd, PT.Transmissions.CleanoutTrigger.ProductionUnitsCleanoutTriggerTable a_table, IScenarioDataChanges a_dataChanges)
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
            PT.Transmissions.CleanoutTrigger.ProductionUnitsCleanoutTriggerTableRow row = (PT.Transmissions.CleanoutTrigger.ProductionUnitsCleanoutTriggerTableRow)de.Value;
            Add(new ProductionUnitsCleanoutTriggerTableRow(row));
        }

        UpdateResourceReferences(a_sd, a_table.AssignedResources, a_dataChanges);
    }

    private void Copy(ProductionUnitsCleanoutTriggerTable a_sourceTable)
    {
        Name = string.Format("Copy of {0}".Localize(), a_sourceTable.Name);
        Description = a_sourceTable.Description;

        foreach (ProductionUnitsCleanoutTriggerTableRow sourceRow in a_sourceTable)
        {
            Add(sourceRow);
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
                    if (res.ProductionUnitsCleanoutTriggerTable != null && res.ProductionUnitsCleanoutTriggerTable.Id == Id && !resourceKeyListNodeHash.Contains(res.GetKey()))
                    {
                        //Unlink
                        res.ProductionUnitsCleanoutTriggerTable = null;
                        a_dataChanges.MachineChanges.UpdatedObject(res.Id);
                    }
                    else if ((res.ProductionUnitsCleanoutTriggerTable == null || res.ProductionUnitsCleanoutTriggerTable.Id != Id) && resourceKeyListNodeHash.Contains(res.GetKey()))
                    {
                        //Link
                        res.ProductionUnitsCleanoutTriggerTable = this;
                        a_dataChanges.MachineChanges.UpdatedObject(res.Id);
                    }
                }
            }
        }
    }
    #endregion


    #region Rows
    private readonly Dictionary<ProductionUnitsCleanoutTriggerTableRow, ProductionUnitsCleanoutTriggerTableRow> m_rows = new();

    private void Add(ProductionUnitsCleanoutTriggerTableRow a_row)
    {
        m_rows.Add(a_row, a_row);
    }

    private void Clear()
    {
        m_rows.Clear();
    }

    public IEnumerator<ProductionUnitsCleanoutTriggerTableRow> GetEnumerator()
    {
        foreach (ProductionUnitsCleanoutTriggerTableRow row in m_rows.Values)
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

public class ProductionUnitsCleanoutTriggerTableRow : BaseCleanoutTriggerTable.BaseCleanoutTriggerTableRow, IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 1068;

    public ProductionUnitsCleanoutTriggerTableRow(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12305)
        {
            a_reader.Read(out m_triggerValue);
            a_reader.Read(out short eVal);
            m_productionUnit = (CleanoutDefs.EProductionUnitsCleanType)eVal;
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        base.Serialize(a_writer);
        a_writer.Write(m_triggerValue);
        a_writer.Write((short)m_productionUnit);
    }

    public virtual int UniqueId => UNIQUE_ID;

    public string DefaultNamePrefix => "Production Units Cleanout Trigger";
    #endregion

    public ProductionUnitsCleanoutTriggerTableRow(ProductionUnitsCleanoutTriggerTableRow a_sourceRow)
        : base(a_sourceRow)
    {
        m_triggerValue = a_sourceRow.TriggerValue;
        m_productionUnit = a_sourceRow.ProductionUnit;
    }

    public ProductionUnitsCleanoutTriggerTableRow(PT.Transmissions.CleanoutTrigger.ProductionUnitsCleanoutTriggerTableRow a_sourceRow)
        : base(a_sourceRow)
    {
        m_triggerValue = a_sourceRow.TriggerValue;
        m_productionUnit = a_sourceRow.ProductionUnit;
    }

    #region Shared Properties
    private decimal m_triggerValue;

    public decimal TriggerValue
    {
        get => m_triggerValue;
        set => m_triggerValue = value;
    }

    private CleanoutDefs.EProductionUnitsCleanType m_productionUnit;

    public CleanoutDefs.EProductionUnitsCleanType ProductionUnit
    {
        get => m_productionUnit;
        set => m_productionUnit = value;
    }
    #endregion

    #region Hash Key
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = m_triggerValue.GetHashCode();
            hash = (hash * 397) ^ m_productionUnit.GetHashCode();
            return hash;
        }
    }

    public override bool Equals(object a_obj)
    {
        if (a_obj is ProductionUnitsCleanoutTriggerTableRow row)
        {
            if (m_triggerValue == row.m_triggerValue && m_productionUnit == row.ProductionUnit)
            {
                return true;
            }
        }

        return false;
    }
    #endregion
}