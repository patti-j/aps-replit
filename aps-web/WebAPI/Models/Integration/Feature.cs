using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Newtonsoft.Json;

namespace WebAPI.Models.Integration
{
    public class Feature
    {
        [Key]
        public int Id { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public bool? Distinct { get; set; }
        public bool? AutoDelete { get; set; }
        [ForeignKey(nameof(IntegrationConfig))]
        [JsonProperty(Required = Required.Default)]
        public int IntegrationConfigId { get; set; }
        [JsonProperty(Required = Required.Default)]
        public virtual IntegrationConfig IntegrationConfig { get; set; }

        public FeatureDTO ToDTO()
        {
            return new FeatureDTO
            {
                Id = Id,
                Name = Name,
                Enabled = Enabled,
                Distinct = Distinct,
                AutoDelete = AutoDelete
            };
        }
    }
}
