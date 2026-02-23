namespace PT.SchedulerDefinitions;

public struct InventoryKey : IPTSerializable
{
    public readonly long WarehouseId;
    public readonly long ItemId;

    public InventoryKey(long a_warehouseId, long a_ItemId)
    {
        WarehouseId = a_warehouseId;
        ItemId = a_ItemId;
    }

    public InventoryKey(IReader a_reader)
    {
        a_reader.Read(out WarehouseId);
        a_reader.Read(out ItemId);
    }

    public static readonly int UNIQUE_ID = 811;

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(WarehouseId);
        a_writer.Write(ItemId);
    }

    public int UniqueId => UNIQUE_ID;
}

public struct InventoryExternalIdKey : IPTSerializable
{
    public readonly string WarehouseExternalId;
    public readonly string ItemExternalId;

    public InventoryExternalIdKey(string a_warehouseExternalId, string a_ItemExternalId)
    {
        WarehouseExternalId = a_warehouseExternalId;
        ItemExternalId = a_ItemExternalId;
    }

    public InventoryExternalIdKey(IReader a_reader)
    {
        a_reader.Read(out WarehouseExternalId);
        a_reader.Read(out ItemExternalId);
    }

    public static readonly int UNIQUE_ID = 832;

    public void Serialize(IWriter a_writer)
    {
        a_writer.Write(WarehouseExternalId);
        a_writer.Write(ItemExternalId);
    }

    public int UniqueId => UNIQUE_ID;
}