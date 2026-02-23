using PT.APSCommon;
using PT.SchedulerDefinitions;

namespace PT.Transmissions;

/// <summary>
/// Deletes the Resource.
/// </summary>
public class ResourceSetShopViewersT : ResourceIdBaseT, IPTSerializable
{
    public new const int UNIQUE_ID = 493;

    #region IPTSerializable Members
    public ResourceSetShopViewersT(IReader reader)
        : base(reader)
    {
        if (reader.VersionNumber >= 1)
        {
            shopViewUsers = new ShopViewUsers(reader);
        }
    }

    public override void Serialize(IWriter writer)
    {
        base.Serialize(writer);

        shopViewUsers.Serialize(writer);
    }

    public override int UniqueId => UNIQUE_ID;
    #endregion

    public ResourceSetShopViewersT() { }

    public ResourceSetShopViewersT(BaseId scenarioId, BaseId plantId, BaseId departmentId, BaseId resourceId, ShopViewUsers shopViewUsers)
        : base(scenarioId, plantId, departmentId, resourceId)
    {
        this.shopViewUsers = shopViewUsers;
    }

    public ShopViewUsers shopViewUsers;

    public override string Description => "Shop views users assigned to Resource";
}