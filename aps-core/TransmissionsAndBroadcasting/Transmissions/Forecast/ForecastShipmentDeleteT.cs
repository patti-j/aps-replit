using PT.APSCommon;

namespace PT.Transmissions.Forecast;

public class ForecastShipmentDeleteT : ForecastBaseT, IPTSerializable
{
    // inventory Key as Key and a List of Shipments for Value
    public readonly Dictionary<SchedulerDefinitions.InventoryKey, List<SchedulerDefinitions.ForecastShipmentKey>> ShipmentsToDelete = new ();

    public ForecastShipmentDeleteT() { }
    public ForecastShipmentDeleteT(BaseId a_scenarioId) : base(a_scenarioId) { }

    public ForecastShipmentDeleteT(IReader a_reader)
        : base(a_reader)
    {
        int count;
        a_reader.Read(out count);
        while (ShipmentsToDelete.Count < count)
        {
            SchedulerDefinitions.InventoryKey invKey = new (a_reader);
            List<SchedulerDefinitions.ForecastShipmentKey> invShipments = new ();
            int shipCount;
            a_reader.Read(out shipCount);
            while (invShipments.Count < shipCount)
            {
                invShipments.Add(new SchedulerDefinitions.ForecastShipmentKey(a_reader));
            }

            ShipmentsToDelete.Add(invKey, invShipments);
        }
    }

    public void Add(SchedulerDefinitions.InventoryKey a_invKey, SchedulerDefinitions.ForecastShipmentKey a_shipmentKey)
    {
        if (ShipmentsToDelete.ContainsKey(a_invKey))
        {
            ShipmentsToDelete[a_invKey].Add(a_shipmentKey);
        }
        else
        {
            ShipmentsToDelete.Add(a_invKey, new List<SchedulerDefinitions.ForecastShipmentKey> { a_shipmentKey });
        }
    }

    public new static readonly int UNIQUE_ID = 813;

    public override void Serialize(IWriter a_writer)
    {
        base.Serialize(a_writer);

        a_writer.Write(ShipmentsToDelete.Count);
        foreach (KeyValuePair<SchedulerDefinitions.InventoryKey, List<SchedulerDefinitions.ForecastShipmentKey>> kv in ShipmentsToDelete)
        {
            kv.Key.Serialize(a_writer);

            a_writer.Write(kv.Value.Count);
            foreach (SchedulerDefinitions.ForecastShipmentKey shipmentKey in kv.Value)
            {
                shipmentKey.Serialize(a_writer);
            }
        }
    }

    public override int UniqueId => UNIQUE_ID;

    public override string Description => "Forecast Shipments deleted";
}