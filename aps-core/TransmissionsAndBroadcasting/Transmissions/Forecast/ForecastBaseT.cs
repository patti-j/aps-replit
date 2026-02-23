using PT.APSCommon;

namespace PT.Transmissions.Forecast;

/// <summary>
/// Base object for all internal Forecast related transmissions.
/// </summary>
public abstract class ForecastBaseT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 600;

    #region IPTSerializable Members
    public ForecastBaseT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1) { }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    protected ForecastBaseT() { }

    protected ForecastBaseT(BaseId scenarioId)
        : base(scenarioId) { }
}