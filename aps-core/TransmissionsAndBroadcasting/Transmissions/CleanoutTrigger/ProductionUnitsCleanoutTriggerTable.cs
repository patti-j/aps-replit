using PT.SchedulerDefinitions;
using System.Collections;

namespace PT.Transmissions.CleanoutTrigger;

public class ProductionUnitsCleanoutTriggerTable : BaseCleanoutTriggerTable, IPTSerializable
{
    #region IPTSerializable Members
    public ProductionUnitsCleanoutTriggerTable(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12305)
        {
            int count;
            a_reader.Read(out count);
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

    public new const int UNIQUE_ID = 1069;
    public override int UniqueId => UNIQUE_ID;
    #endregion

    #region Construction
    public ProductionUnitsCleanoutTriggerTable() { }
    #endregion

    #region Shared Properties
    public string DefaultNamePrefix => "Operation Count Cleanout Trigger";
    #endregion

    #region Rows
    private readonly Hashtable m_rows = new ();

    public void Add(ProductionUnitsCleanoutTriggerTableRow row)
    {
        if (!m_rows.Contains(row.GetHashKey()))
        {
            m_rows.Add(row.GetHashKey(), row);
        }
    }

    public int Count => m_rows.Count;

    public ProductionUnitsCleanoutTriggerTableRow GetRow(decimal a_triggerValue, int a_productionUnit)
    {
        string hashKey = ProductionUnitsCleanoutTriggerTableRow.GetHashKey(a_triggerValue, a_productionUnit);
        if (m_rows.Contains(hashKey))
        {
            return (ProductionUnitsCleanoutTriggerTableRow)m_rows[hashKey];
        }

        return null;
    }

    public IDictionaryEnumerator GetEnumerator()
    {
        return m_rows.GetEnumerator();
    }
    #endregion
}

public class ProductionUnitsCleanoutTriggerTableRow : BaseCleanoutTriggerTable.BaseCleanoutTriggerTableRow, IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 1070;

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

    public virtual void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        #if DEBUG
        a_writer.DuplicateErrorCheck(this);
        #endif
        a_writer.Write(m_triggerValue);
        a_writer.Write((short)m_productionUnit);
    }

    public virtual int UniqueId => UNIQUE_ID;
    #endregion

    public ProductionUnitsCleanoutTriggerTableRow(TimeSpan a_duration, int a_cleanoutGrade, decimal a_cleanCost, decimal a_triggerValue, CleanoutDefs.EProductionUnitsCleanType a_productionUnit)
        : base(a_duration, a_cleanoutGrade, a_cleanCost)
    {
        m_triggerValue = a_triggerValue;
        m_productionUnit = a_productionUnit;
    }

    public ProductionUnitsCleanoutTriggerTableRow(ProductionUnitsCleanoutTriggerTableRow a_sourceRow)
        : base(a_sourceRow)

    {
        m_triggerValue = a_sourceRow.TriggerValue;
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
    // TODO: stronger identity needed?
    internal string GetHashKey()
    {
        return GetHashKey(TriggerValue, (short)ProductionUnit);
    }

    public static string GetHashKey(decimal a_triggerValue, int a_productionUnit)
    {
        return a_triggerValue + "(*&" + a_productionUnit;
    }
    #endregion
}