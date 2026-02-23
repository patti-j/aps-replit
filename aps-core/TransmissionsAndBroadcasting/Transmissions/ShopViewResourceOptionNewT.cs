using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// For creating a new ShopViewResourceOption.
/// </summary>
public class ShopViewResourceOptionNewT : ShopViewResourceOptionBaseT, IPTSerializable
{
    #region IPTSerializable Members
    public new const int UNIQUE_ID = 505;

    public ShopViewResourceOptionNewT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            Options = new ShopViewResourceOptions(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        Options.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ShopViewResourceOptionNewT() { }

    public ShopViewResourceOptionNewT(ShopViewResourceOptions options, BaseId scenarioId)
        : base(scenarioId)
    {
        Options = options;
    }

    public ShopViewResourceOptions Options;

    public override string Description => "Shop View options created";
}