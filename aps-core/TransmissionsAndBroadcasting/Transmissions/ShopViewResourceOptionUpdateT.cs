using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// For updating a specific ShopViewResourceOption.
/// </summary>
public class ShopViewResourceOptionUpdateT : ShopViewResourceOptionIdBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 502;

    public ShopViewResourceOptionUpdateT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            id = new BaseId(reader);
            Options = new ShopViewResourceOptions(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        id.Serialize(writer);
        Options.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ShopViewResourceOptions Options;

    public ShopViewResourceOptionUpdateT() { }

    public ShopViewResourceOptionUpdateT(ShopViewResourceOptions options, BaseId id, BaseId scenarioId)
        : base(id, scenarioId)
    {
        Options = options;
    }

    public override string Description => "Shop View options updated";
}