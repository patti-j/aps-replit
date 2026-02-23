using System.Text.Json.Serialization;

namespace PT.APIDefinitions.RequestsAndResponses.DataDtos;

public class Warehouse : IDataDto
{
    [JsonIgnore]
    public List<string> SelectableFields => new List<string>()
    {
        nameof(Id),nameof(ExternalId), nameof(Name), nameof(Description)
    };

    /// <summary>
    /// The internal id. Using<see cref="ExternalId"/> may be better.
    /// </summary>
    public long Id { get; set; }
    public string ExternalId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}