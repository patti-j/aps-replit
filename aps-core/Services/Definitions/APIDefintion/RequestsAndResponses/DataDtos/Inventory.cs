using System.Text.Json.Serialization;

namespace PT.APIDefinitions.RequestsAndResponses.DataDtos
{
    public class Inventory
    {
        [JsonIgnore]
        public List<string> SelectableFields => new List<string>()
        {
            nameof(Id), nameof(ItemExternalId), nameof(Item), nameof(Warehouses), nameof(OnHandQty)
        };

        /// <summary>
        /// The internal id for the Inventory. This may not be needed, but provides a unique identifier for this entity. Using<see cref="ItemExternalId"/> may be better.
        /// </summary>
        public long Id { get; set; }
        public string ItemExternalId => Item?.ExternalId;
        public decimal OnHandQty { get; set; }
        public Item Item { get; set; }
        public List<Warehouse> Warehouses { get; set; }
    }
}
