namespace PT.SchedulerDefinitions;

public struct ForecastKey : IPTSerializable
{
    public readonly string Version;
    public readonly long ForecastId;

    public ForecastKey(string a_versionId, long a_forecastId)
    {
        Version = a_versionId;
        ForecastId = a_forecastId;
    }

    public ForecastKey(IReader a_reader)
    {
        a_reader.Read(out Version);
        a_reader.Read(out ForecastId);
    }

    public static readonly int UNIQUE_ID = 806;

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(Version);
        a_writer.Write(ForecastId);
    }

    public int UniqueId => UNIQUE_ID;
}