namespace WebAPI.Models.V2
{
    public class Inventory
    {
        public DateTime importDate { get; set; }
        public long ItemId { get; set; }
        public long WarehouseId { get; set; }
        public long InventoryId { get; set; }
    }
}
