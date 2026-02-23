using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// For copying a specific ShopViewResourceOption.
/// </summary>
public class ShopViewResourceOptionCopyT : ShopViewResourceOptionIdBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 504;

    public ShopViewResourceOptionCopyT(IReader reader)
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

    public ShopViewResourceOptionCopyT() { }

    public ShopViewResourceOptionCopyT(BaseId id, BaseId scenarioId)
        : base(id, scenarioId) { }

    public override string Description => "Shop View options copied";
}