using Newtonsoft.Json;

namespace PT.APIDefinitions.Integration;

public enum EStatusStep { Started, Completed }

public class BaseRequest
{
    [JsonProperty("StatusStep")]
    public EStatusStep StatusStep { get; set; }
}

public class ImportRequest : BaseRequest
{
    [JsonProperty("ScenarioId")]
    public long ScenarioId { get; set; }
}

public class PublishRequest : BaseRequest
{
    [JsonProperty("ScenarioName")]
    public string ScenarioName { get; set; }
}