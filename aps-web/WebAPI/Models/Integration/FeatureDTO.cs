using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Integration
{
    public class FeatureDTO
    {
        [Key]
        public int Id { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; }
        public bool? Distinct { get; set; }
        public bool? AutoDelete { get; set; }

        public Feature ToModel(int ConfigId)
        {
            return new Feature
            {
                Id = Id,
                Name = Name,
                Enabled = Enabled,
                Distinct = Distinct,
                AutoDelete = AutoDelete,
                IntegrationConfigId = ConfigId
            };
        }
    }
}
