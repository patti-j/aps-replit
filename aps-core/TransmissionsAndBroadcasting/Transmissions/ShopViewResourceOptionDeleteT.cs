using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// For deleting a specific ShopViewResourceOption.
/// </summary>
public class ShopViewResourceOptionDeleteT : ShopViewResourceOptionIdBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 503;

    public ShopViewResourceOptionDeleteT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            id = new BaseId(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        id.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ShopViewResourceOptionDeleteT() { }

    public ShopViewResourceOptionDeleteT(BaseId id, BaseId scenarioId)
        : base(id, scenarioId) { }

    public override string Description => "Shop Views options deleted";
}