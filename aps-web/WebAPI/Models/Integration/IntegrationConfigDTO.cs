using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models.Integration
{
    public class IntegrationConfigDTO
    {
        [Key]
        public int Id { get; set; }
        public string VersionNumber { get; set; }

        public int? UpgradedFromConfigId { get; set; }
        public string Name { get; set; }
        public virtual List<FeatureDTO> Features { get; set; }
        public virtual List<PropertyDTO> Properties { get; set; }

        /// <summary>
        /// Creates the local model from this DTO class. 
        /// API clients do not need to know company id, so it must be provided (caller should know and provide secure InstanceId, which can be used to get it.)
        /// </summary>
        /// <param name="a_companyId"></param>
        /// <returns></returns>
        public IntegrationConfig ToModel(int a_companyId)
        {
            return new IntegrationConfig
            {
                Id = Id,
                Name = Name,
                Features = Features.Select(x => x.ToModel(Id)).ToList(),
                Properties = Properties.Select(x => x.ToModel(Id)).ToList(),
                CompanyId = a_companyId,
                VersionNumber = VersionNumber,
                UpgradedFromConfigId = UpgradedFromConfigId
            };
        }
    }
}
