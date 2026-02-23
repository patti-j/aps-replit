using System.Collections;

using PT.APSCommon;

namespace PT.Transmissions;

public class InventoryTransferRulesT : ScenarioIdBaseT, IPTSerializable, IEnumerable
{
    private readonly List<InventoryTransferRule> m_transferRules = new ();

    public InventoryTransferRulesT() { }
    public InventoryTransferRulesT(BaseId a_scenarioId) : base(a_scenarioId) { }

    public InventoryTransferRulesT(IReader a_reader)
        : base(a_reader)
    {
        int count;
        a_reader.Read(out count);
        for (int i = 0; i < count; i++)
        {
            InventoryTransferRule tr = new (a_reader);
            Add(tr);
        }
    }

    public void Add(InventoryTransferRule a_transferOrderRule)
    {
        m_transferRules.Add(a_transferOrderRule);
    }

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);
        a_writer.Write(m_transferRules.Count);
        foreach (InventoryTransferRule tr in m_transferRules)
        {
            tr.Serialize(a_writer);
        }
    }

    public const int UNIQUE_ID = 805;

    public override int UniqueId => UNIQUE_ID;

    public IEnumerator GetEnumerator()
    {
        return m_transferRules.GetEnumerator();
    }

    public override string Description => "Inventory Transfer Rules saved";

    public class InventoryTransferRule : IPTSerializable
    {
        private readonly long m_fromWarehouseId;

        public long FromWarehouseId => m_fromWarehouseId;

        private readonly long m_toWarehouseId;

        public long ToWarehouseId => m_toWarehouseId;

        private readonly decimal m_transferHrs;

        public decimal TransferHrs => m_transferHrs;

        public InventoryTransferRule(long a_fromWarehouseId, long a_toWarehouseId, decimal a_transferHrs)
        {
            m_fromWarehouseId = a_fromWarehouseId;
            m_toWarehouseId = a_toWarehouseId;
            m_transferHrs = a_transferHrs;
        }

        public InventoryTransferRule(IReader a_reader)
        {
            a_reader.Read(out m_fromWarehouseId);
            a_reader.Read(out m_toWarehouseId);
            a_reader.Read(out m_transferHrs);
        }

        public void Serialize(IWriter a_writer)
        {
            a_writer.Write(m_fromWarehouseId);
            a_writer.Write(m_toWarehouseId);
            a_writer.Write(m_transferHrs);
        }

        public const int UNIQUE_ID = 808;

        public int UniqueId => UNIQUE_ID;
    }
}