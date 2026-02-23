using PT.APSCommon;
using PT.Common.ObjectHelpers;

namespace PT.SchedulerData.ObjectKeys;

public readonly struct StorageAreaKey : IEquatable<StorageAreaKey>
{
    public readonly BaseId WarehouseId;
    public readonly BaseId StorageAreaId;

    public StorageAreaKey(BaseId a_warehouseId)
    {
        WarehouseId = a_warehouseId;
        StorageAreaId = BaseId.NULL_ID;
    }

    public StorageAreaKey(BaseId a_warehouseId, BaseId a_storageAreaId)
    {
        WarehouseId = a_warehouseId;
        StorageAreaId = a_storageAreaId;
    }

    public bool Equals(StorageAreaKey a_other)
    {
        return WarehouseId.Value == a_other.WarehouseId.Value && StorageAreaId.Value == a_other.StorageAreaId.Value;
    }

    public override bool Equals(object a_other)
    {
        if (a_other is StorageAreaKey key)
        {
            return Equals(key);
        }

        return false;
    } 

    public override int GetHashCode()
    {
        return HashCodeHelper.GetHashCode(WarehouseId, StorageAreaId);
    }
}