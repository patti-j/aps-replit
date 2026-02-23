using PT.APSCommon;
using PT.Common.ObjectHelpers;

namespace PT.SchedulerData.ObjectKeys;

public readonly struct InventoryKey : IEquatable<InventoryKey>
{
    public readonly BaseId WarehouseId;
    public readonly BaseId ItemId;
    public readonly BaseId InventoryId;

    public InventoryKey(BaseId a_warehouseId)
    {
        WarehouseId = a_warehouseId;
        ItemId = BaseId.NULL_ID;
        InventoryId = BaseId.NULL_ID;
    }

    public InventoryKey(BaseId a_warehouseId, BaseId a_itemId)
    {
        WarehouseId = a_warehouseId;
        ItemId = a_itemId;
        InventoryId = BaseId.NULL_ID;
    }

    public InventoryKey(BaseId a_warehouseId, BaseId a_itemId, BaseId a_invId)
    {
        WarehouseId = a_warehouseId;
        ItemId = a_itemId;
        InventoryId = a_invId;
    }

    public bool Equals(InventoryKey a_other)
    {
        return ItemId.Value == a_other.ItemId.Value && WarehouseId.Value == a_other.WarehouseId.Value && InventoryId.Value == a_other.InventoryId.Value;
    }

    public override bool Equals(object a_other)
    {
        if (a_other is InventoryKey key)
        {
            return Equals(key);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCodeHelper.GetHashCode(WarehouseId, ItemId, InventoryId);
    }
}