using PT.APSCommon;

namespace PT.Transmissions.Forecast;

public class ForecastShipmentAdjustmentT : ForecastBaseT, IPTSerializable
{
    public override string Description => "Forecast Shipments updated";

    // inventory Key as Key and a dictionary of Shipmentkeys and new requiredQties for Value
    public readonly Dictionary<SchedulerDefinitions.InventoryKey, Dictionary<SchedulerDefinitions.ForecastShipmentKey, decimal>> m_shipmentAdjustments = new ();

    public ForecastShipmentAdjustmentT() { }
    public ForecastShipmentAdjustmentT(BaseId a_scenarioId) : base(a_scenarioId) { }

    public ForecastShipmentAdjustmentT(IReader a_reader)
        : base(a_reader)
    {
        int count;
        a_reader.Read(out count);
        while (m_shipmentAdjustments.Count < count)
        {
            SchedulerDefinitions.InventoryKey invKey = new (a_reader);
            Dictionary<SchedulerDefinitions.ForecastShipmentKey, decimal> invAdjustments = new ();
            int shipCount;
            a_reader.Read(out shipCount);
            while (invAdjustments.Count < shipCount)
            {
                SchedulerDefinitions.ForecastShipmentKey key = new (a_reader);
                decimal reqQty;
                a_reader.Read(out reqQty);
                invAdjustments.Add(key, reqQty);
            }

            m_shipmentAdjustments.Add(invKey, invAdjustments);
        }
    }

    public void Add(SchedulerDefinitions.InventoryKey a_invKey, SchedulerDefinitions.ForecastShipmentKey a_shipmentKey, decimal a_requiredQty)
    {
        if (!m_shipmentAdjustments.ContainsKey(a_invKey))
        {
            Dictionary<SchedulerDefinitions.ForecastShipmentKey, decimal> shipmentAdjustments = new ();
            m_shipmentAdjustments.Add(a_invKey, shipmentAdjustments);
        }

        m_shipmentAdjustments[a_invKey].Add(a_shipmentKey, a_requiredQty);
    }

    public new static readonly int UNIQUE_ID = 814;

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(m_shipmentAdjustments.Count);
        foreach (KeyValuePair<SchedulerDefinitions.InventoryKey, Dictionary<SchedulerDefinitions.ForecastShipmentKey, decimal>> kv in m_shipmentAdjustments)
        {
            kv.Key.Serialize(a_writer);

            a_writer.Write(kv.Value.Count);
            foreach (KeyValuePair<SchedulerDefinitions.ForecastShipmentKey, decimal> shipmentAdj in kv.Value)
            {
                shipmentAdj.Key.Serialize(a_writer);
                a_writer.Write(shipmentAdj.Value);
            }
        }
    }

    public override int UniqueId => UNIQUE_ID;
}