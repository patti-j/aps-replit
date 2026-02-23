using WebAPI.Models.Integration;

namespace WebAPI.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for Company information returned by the API
    /// </summary>
    public class CompanyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Active { get; set; }
        public DateTime? CreationDate { get; set; }
        public string? CreatedBy { get; set; }
        public List<string> IntegrationNames { get; set; } = new();

        /// <summary>
        /// Creates a CompanyDto from a Company entity
        /// </summary>
        /// <param name="company">The Company entity</param>
        /// <returns>CompanyDto with safe data</returns>
        public static CompanyDto FromCompany(Company company)
        {
            return new CompanyDto
            {
                Id = company.Id,
                Name = company.Name ?? string.Empty,
                Email = company.Email ?? string.Empty,
                Active = company.Active,
                CreationDate = company.CreationDate,
                CreatedBy = company.CreatedBy,
                IntegrationNames = company.Integrations?.Select(i => i.Name ?? string.Empty).ToList() ?? new List<string>()
            };
        }
    }
}