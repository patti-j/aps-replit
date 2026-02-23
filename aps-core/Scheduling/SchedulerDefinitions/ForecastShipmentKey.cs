namespace PT.SchedulerDefinitions;

public struct ForecastShipmentKey : IPTSerializable
{
    public readonly ForecastKey ShipmentForecastKey;
    public readonly long ShipmentId;

    public ForecastShipmentKey(ForecastKey a_forecastKey, long a_hipmentId)
    {
        ShipmentForecastKey = a_forecastKey;
        ShipmentId = a_hipmentId;
    }

    public ForecastShipmentKey(IReader a_reader)
    {
        ShipmentForecastKey = new ForecastKey(a_reader);
        a_reader.Read(out ShipmentId);
    }

    public static readonly int UNIQUE_ID = 810;

    public void Serialize(IWriter a_writer)
    {
        ShipmentForecastKey.Serialize(a_writer);
        a_writer.Write(ShipmentId);
    }

    public int UniqueId => UNIQUE_ID;
}