namespace ReportsWebApp.DB.Models.DTOs
{
    public class SerialCodeRequest
    {
        public string SerialCode { get; set; }
        public string Token { get; set; }
    }

    public class LicenseRow
    {
        public int SubscriptionId { get; set; }
        public string Name { get; set; }
        public DateTime? Expiration { get; set; }
        public string SerialCode { get; set; }
        public string Edition { get; set; }
        public string Description { get; set; }
        public string CompanyName { get; set; }
    }

    /// <summary>
    /// Live subscription data returned from external license service
    /// </summary>
    public class SubscriptionInfo
    {

        public int SubscriptionId { get; set; }        
        public string Name { get; set; }
        public DateTime? Expiration { get; set; }
        public string SerialCode { get; set; }
        public string Edition { get; set; }
        public string Description { get; set; }
        
        /// <summary>
        /// URL to view/manage this subscription in the license service application
        /// </summary>
        public string LicenseServiceUrl { get; set; }
    }

    /// <summary>
    /// Request to get companies from license service
    /// </summary>
    public class GetCompaniesRequest
    {
        public string Token { get; set; }
    }

    /// <summary>
    /// Company data returned from license service for external application consumption
    /// </summary>
    public class LicenseServiceCompanyDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContactName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}