using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for updating a ShopViewResourceOption.
/// </summary>
public abstract class ShopViewResourceOptionBaseT : ScenarioIdBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public const int UNIQUE_ID = 500;

    public ShopViewResourceOptionBaseT(IReader reader)
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

    protected ShopViewResourceOptionBaseT() { }

    protected ShopViewResourceOptionBaseT(BaseId scenarioId)
        : base(scenarioId) { }

    /// <summary>
    /// Specifies the name of the type of object in whose history this Transmission should show.
    /// </summary>
    public override string Description => "Shop View Resource Options updated";
}