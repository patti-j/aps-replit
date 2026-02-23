using PT.APSCommon;

namespace PT.Transmissions;

/// <summary>
/// Base object for updating a specific ShopViewResourceOption.
/// </summary>
public abstract class ShopViewResourceOptionIdBaseT : ShopViewResourceOptionBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 501;

    public ShopViewResourceOptionIdBaseT(IReader reader)
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

    public BaseId id;

    protected ShopViewResourceOptionIdBaseT() { }

    protected ShopViewResourceOptionIdBaseT(BaseId id, BaseId scenarioId)
        : base(scenarioId)
    {
        this.id = id;
    }
}