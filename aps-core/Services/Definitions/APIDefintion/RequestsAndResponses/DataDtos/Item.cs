using System.Text.Json.Serialization;

namespace PT.APIDefinitions.RequestsAndResponses.DataDtos;

// TODO: For now, this just returns the data needed for the CTP UI in the webapp.
// TODO: We'll either want to grow this to be more robust (and be prepared to handle changes to the underlying model) or keep using more specific Dto types.
public class Item : IDataDto
{
    [JsonIgnore]
    public List<string> SelectableFields => new List<string>()
    {
        nameof(Id), nameof(ExternalId), nameof(Name), nameof(Description), nameof(UnitVolume), nameof(MinOrderQty), nameof(MaxOrderQty), nameof(DefaultLeadTime), nameof(BatchSize), 
        nameof(TransferQty), nameof(ShelfLife), nameof(Cost), nameof(PlanInventory), nameof(RollupAttributesToParent), nameof(ItemGroup), nameof(Notes)
    };

    /// <summary>
    /// The internal id. Using<see cref="ExternalId"/> may be better.
    /// </summary>
    public long Id { get; set; }
    public string ExternalId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal UnitVolume { get; set; }
    public string ItemType { get; set; }
    public string Source { get; set; }
    public decimal MinOrderQty { get; set; }
    public decimal MaxOrderQty { get; set; }
    public TimeSpan DefaultLeadTime { get; set; }
    public decimal BatchSize { get; set; }
    public decimal TransferQty { get; set; }
    public TimeSpan ShelfLife { get; set; }
    public decimal Cost { get; set; }
    public bool PlanInventory { get; set; }
    public bool RollupAttributesToParent { get; set; }
    public string ItemGroup { get; set; }
    public string Notes { get; set; }

    // TODO: Path details?
}

