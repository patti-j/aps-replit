using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

public class RecurringCapacityIntervalUpdateMultiT : RecurringCapacityIntervalBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 1104;

    #region PT Serialization
    public RecurringCapacityIntervalUpdateMultiT(IReader a_reader)
        : base(a_reader)
    {
        if (a_reader.VersionNumber >= 12418)
        {
            m_bools = new BoolVector32(a_reader);

            a_reader.Read(out int count);
            for (int i = 0; i < count; i++)
            {
                RecurringCapacityIntervalDef node = new(a_reader);
                Add(node);
            }
        }
    }
    public List<RecurringCapacityIntervalDef> Nodes
    {
        get => m_nodes;
        set => m_nodes = value;
    }
    private List<RecurringCapacityIntervalDef> m_nodes = new();
    private void Add(RecurringCapacityIntervalDef node)
    {
        m_nodes.Add(node);
    }
    public int Count => m_nodes.Count;

    public void Clear()
    {
        m_nodes.Clear();
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        m_bools.Serialize(a_writer);

        a_writer.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            this[i].Serialize(a_writer);
        }
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    private BoolVector32 m_bools;

    private const short c_autoDeleteIdx = 0;
    private const short c_autoDeleteResourceIdx = 1;
    /// <summary>
    /// Whether to remove recurring capacity interval that are not specified in the transmission
    /// </summary>

    public bool AutoDelete
    {
        get => m_bools[c_autoDeleteIdx];
        set => m_bools[c_autoDeleteIdx] = value;
    }
    
    /// <summary>
    /// Whether to remove recurring capacity interval from resources that are not specified in the transmission
    /// </summary>
    private bool m_autoDeleteResourceAssociations = true;

    public bool AutoDeleteResourceAssociations
    {
        get => m_bools[c_autoDeleteResourceIdx];
        set => m_bools[c_autoDeleteResourceIdx] = value;
    }

    #region Database Loading
    /// <summary>
    /// Fill the transmission with data from the DataSet.
    /// </summary>
    /// <param name="ds"></param>
    public void FillFromDisplayData(RecurringCapacityIntervalTDataSet ds)
    {
        for (int i = 0; i < ds.RecurringCapacityIntervals.Count; i++)
        {
            RecurringCapacityIntervalDef rciDef = new(ds.RecurringCapacityIntervals[i], true);
            rciDef.Validate();
            Add(rciDef);
        }
    }

    #endregion Database Loading

    public RecurringCapacityIntervalUpdateMultiT() { }

    public RecurringCapacityIntervalUpdateMultiT(BaseId a_scenarioId) : base(a_scenarioId)
    {
    }

    public RecurringCapacityIntervalDef this[int i] => Nodes[i];
}