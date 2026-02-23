using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for all Plant related transmissions.
/// </summary>
public abstract class PlantBaseT : ScenarioIdBaseT, IPTSerializable
{
    public const int UNIQUE_ID = 88;
    
    #region IPTSerializable Members
    public PlantBaseT(IReader reader)
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

    protected PlantBaseT() { }
    public PlantBaseT(BaseId scenarioId)
        : base(scenarioId) { }
}