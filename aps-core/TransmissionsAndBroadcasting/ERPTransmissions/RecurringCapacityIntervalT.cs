using PT.Common.Exceptions;
using PT.SchedulerDefinitions;
using PT.Transmissions;

namespace PT.ERPTransmissions;

public class RecurringCapacityIntervalT : ERPMaintenanceTransmission<RecurringCapacityIntervalDef>, IPTSerializable
{
    public new const int UNIQUE_ID = 433;

    #region PT Serialization
    public RecurringCapacityIntervalT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12418)
        {
            a_reader.Read(out m_autoDeleteResourceAssociations);
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                RecurringCapacityIntervalDef node = new(a_reader);
                Add(node);
            }
        }
        else if (a_reader.VersionNumber >= 1)
        {
            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                RecurringCapacityIntervalDef node = new(a_reader);
                Add(node);
            }
        }
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(m_autoDeleteResourceAssociations);
        a_writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(a_writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    /// <summary>
    /// Whether to remove recurring capacity interval from resources that are not specified in the transmission
    /// </summary>
    private bool m_autoDeleteResourceAssociations = true;

    public bool AutoDeleteResourceAssociations
    {
        get => m_autoDeleteResourceAssociations;
        set => m_autoDeleteResourceAssociations = value;
    }

    #region Database Loading
    public void Fill(System.Data.IDbCommand recurringCapacityIntervalsCmd, System.Data.IDbCommand recurringCapacityIntervalsResourcesCmd)
    {
        RecurringCapacityIntervalTDataSet ds = new ();
        try
        {
            FillTable(ds.RecurringCapacityIntervals, recurringCapacityIntervalsCmd);
        }
        catch (Exception e)
        {
            throw new PTException("4048", e, new object[] { e.Message }); //Replace with Recurring Capacity Interval related error
        }

        try
        {
            FillTable(ds.Resources, recurringCapacityIntervalsResourcesCmd);
        }
        catch (Exception e)
        {
            throw new PTException("4049", e, new object[] { e.Message });
        }

        FillFromUtcData(ds);
    }

    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    /// <param name="ds"></param>
    public void FillFromDisplayData(RecurringCapacityIntervalTDataSet ds)
    {
        for (int i = 0; i < ds.RecurringCapacityIntervals.Count; i++)
        {
            RecurringCapacityIntervalDef rciDef = new (ds.RecurringCapacityIntervals[i], true);
            rciDef.Validate();
            Add(rciDef);
        }
    }

    public void FillFromUtcData(RecurringCapacityIntervalTDataSet a_dataset)
    {
        for (int i = 0; i < a_dataset.RecurringCapacityIntervals.Count; i++)
        {
            RecurringCapacityIntervalDef rciDef = new(a_dataset.RecurringCapacityIntervals[i], false);
            rciDef.Validate();
            Add(rciDef);
        }
    }
    #endregion Database Loading

    public RecurringCapacityIntervalT() { }

    public new RecurringCapacityIntervalDef this[int i] => Nodes[i];
}