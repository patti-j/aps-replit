using System.Collections;

using PT.APSCommon;

namespace PT.Scheduler.Schedule.InventoryManagement;

public class InventoryTransferRuleManager : IPTSerializable, IEnumerable
{
    private readonly List<InventoryTransferRule> m_transferRules = new ();

    public InventoryTransferRuleManager() { }

    public InventoryTransferRuleManager(IReader a_reader)
    {
        int count;
        a_reader.Read(out count);
        for (int i = 0; i < count; i++)
        {
            InventoryTransferRule tr = new (a_reader);
            m_transferRules.Add(tr);
        }
    }

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(m_transferRules.Count);
        foreach (InventoryTransferRule tr in m_transferRules)
        {
            tr.Serialize(a_writer);
        }
    }

    public void RestoreReferences(ScenarioDetail a_sd)
    {
        foreach (InventoryTransferRule tr in m_transferRules)
        {
            tr.RestoreReferences(a_sd);
        }
    }

    public const int UNIQUE_ID = 804;

    public int UniqueId => UNIQUE_ID;

    public IEnumerator GetEnumerator()
    {
        return m_transferRules.GetEnumerator();
    }

    public void Receive(Transmissions.InventoryTransferRulesT a_t, ScenarioDetail a_sd)
    {
        m_transferRules.Clear();

        foreach (PT.Transmissions.InventoryTransferRulesT.InventoryTransferRule tRule in a_t)
        {
            Warehouse fromWarehouse = a_sd.WarehouseManager.GetById(new BaseId(tRule.FromWarehouseId));
            if (fromWarehouse == null)
            {
                throw new PTValidationException("2909", new object[] { tRule.FromWarehouseId });
            }

            Warehouse toWarehouse = a_sd.WarehouseManager.GetById(new BaseId(tRule.ToWarehouseId));
            if (toWarehouse == null)
            {
                throw new PTValidationException("2909", new object[] { tRule.ToWarehouseId });
            }

            m_transferRules.Add(new InventoryTransferRule(fromWarehouse, toWarehouse, TimeSpan.FromHours((double)tRule.TransferHrs)));
        }
    }

    public TimeSpan GetTransferSpan(Warehouse a_fromWarehouse, Warehouse a_toWarehouse)
    {
        if (a_fromWarehouse.Id == a_toWarehouse.Id)
        {
            return TimeSpan.Zero;
        }

        foreach (InventoryTransferRule tr in this)
        {
            if (tr.FromWarehouse.Id == a_fromWarehouse.Id && tr.ToWarehouse.Id == a_toWarehouse.Id)
            {
                return tr.TransferSpan;
            }
        }

        return TimeSpan.Zero;
    }

    internal void DeletingWarehouse(Warehouse a_warehouse)
    {
        for (int i = m_transferRules.Count - 1; i >= 0; i--)
        {
            InventoryTransferRule itr = m_transferRules[i];
            if (itr.FromWarehouse.Id == a_warehouse.Id || itr.ToWarehouse.Id == a_warehouse.Id)
            {
                m_transferRules.RemoveAt(i);
            }
        }
    }
}