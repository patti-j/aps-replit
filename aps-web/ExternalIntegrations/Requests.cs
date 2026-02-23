using System.Text.Json.Serialization;

namespace ExternalIntegrations
{
    public enum EStatusStep
    {
        Started,
        Complete,
    }

    public class BaseRequest
    {
        [JsonPropertyName("StatusStep")]
        public EStatusStep StatusStep { get; set; }
    }

    public class ImportRequest : BaseRequest
    {
        [JsonPropertyName("CompanyId")] public int CompanyId { get; set; }

        [JsonPropertyName("InstanceId")] public int InstanceId { get; set; }

        [JsonPropertyName("IntegrationId")] public int IntegrationId { get; set; }

    }

    public class PublishRequest : BaseRequest
    {
        [JsonPropertyName("ScenarioName")]
        public string ScenarioName { get; set; }
    }
}
