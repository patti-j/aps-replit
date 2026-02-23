using System;
using System.Collections.Generic;

namespace PT.APIDefinitions.RequestsAndResponses.License
{
    #region Requests / Responses

    public class LicenseServiceSessionDtos
    {
        public LicenseServiceSessionDtos() { }

        public LicenseServiceSessionDtos(string serialCode, string token, string environmentType, int? siteId = null)
        {
            SerialCode = serialCode;
            Token = token;
            EnvironmentType = environmentType;
            SiteId = siteId;
        }

        public string SerialCode { get; set; }
        public string? Token { get; set; }
        public int? SiteId { get; set; }
        public string EnvironmentType { get; set; }
    }

    /// <summary>
    /// Request that includes planning area authentication via HMAC signature
    /// </summary>
    public class PlanningAreaSessionRequest
    {
        public PlanningAreaSessionRequest() { }

        /// <summary>
        /// Public identifier for the planning area
        /// </summary>
        public string PlanningAreaId { get; set; }

        /// <summary>
        /// Unix timestamp when request was created (for replay attack prevention)
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// Random client-generated string to prevent replay attacks
        /// </summary>
        public string Nonce { get; set; }

        /// <summary>
        /// Hashed signature: $"{planningAreaKey}|{timestamp}|{nonce}"
        /// </summary>
        public string Signature { get; set; }
        public string? Token { get; set; }
        public int? SiteId { get; set; }
        public string EnvironmentType { get; set; }
    }

    /// <summary>
    /// Request to PlanningArea Validation API for planning area validation
    /// </summary>
    public class PlanningAreaValidationRequest
    {
        public string PlanningAreaId { get; set; }
        public long Timestamp { get; set; }
        public string Nonce { get; set; }
        public string Signature { get; set; }
    }

    /// <summary>
    /// Response from PlanningArea Validation API for planning area validation
    /// </summary>
    public class PlanningAreaValidationResponse
    {
        public bool Valid { get; set; }
        public string? Reason { get; set; }
        public int? PlanningAreaId { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? SerialCode { get; set; }
        public int? LicenseStatus { get; set; }
    }

    public class GetSessionResourcesRequest
    {
        public GetSessionResourcesRequest() { }
    }

    public class SerialCodeRequest
    {
        public string SerialCode { get; set; }
        public string? Token { get; set; }
    }

    public class SessionRequestResponse
    {
        public Guid? SessionId { get; set; }
        public List<GrantResponse> AvailableGrantTypes { get; set; }
        public List<PackageDto> AvailablePackages { get; set; }
        public List<FeatureDto> AvailableFeatures { get; set; }
    }

    /// <summary>
    /// Secure session response with detailed grant allocation information
    /// </summary>
    public class SecureSessionResponse
    {
        public Guid? SessionId { get; set; }
        public string? EnvironmentType { get; set; }
        public int? SiteId { get; set; }
        public List<SubGrantResponse> AvailableGrantTypes { get; set; } = new();
        public List<PackageDto> AvailablePackages { get; set; } = new();
        public List<FeatureDto> AvailableFeatures { get; set; } = new();
        public DateTime? SessionExpirationDate { get; set; }
        public string? CompanyName { get; set; }
        public string? SubscriptionName { get; set; }
    }

    /// <summary>
    /// Subscription grant response with detailed allocation information
    /// </summary>
    public class SubGrantResponse
    {
        public string GrantType { get; set; } = string.Empty;
        public int AvailableQuantity { get; set; }
        public int TotalQuantity { get; set; }
        public int ActiveQuantity { get; set; }
        public int SiteSpecificQuantity { get; set; }
        public int CommonQuantity { get; set; }
        public List<GrantDetailResponse> GrantDetails { get; set; } = new();
    }

    /// <summary>
    /// Detailed grant information for secure responses
    /// </summary>
    public class GrantDetailResponse
    {
        public int GrantId { get; set; }
        public int MaxQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public int? SiteId { get; set; }
        public DateTime ExpirationDate { get; set; }
    }

    public class GrantRequest
    {
        public GrantRequest() { }

        public GrantRequest(GrantTypeNames grantTypeNames, int grantQty)
        {
            GrantType = grantTypeNames.ToString();
            GrantQty = grantQty;
        }

        public string GrantType { get; set; }
        public int GrantQty { get; set; }
    }

    public class GrantResponse
    {
        public string GrantType { get; set; }
        public int AvailableQuantity { get; set; }
    }

    public class LockRequest
    {
        public LockRequest() { }

        public LockRequest(GrantTypeNames grantTypeNames, int grantQty)
        {
            GrantType = grantTypeNames.ToString();
            GrantQty = grantQty;
        }

        public string GrantType { get; set; }
        public int GrantQty { get; set; }
    }

    public class LockResponse
    {
        public string? LockResponseCode { get; set; }
        public string? LockResponseMessage { get; set; }
    }

    public class LicensePingRequest
    {
    }

    public class LicensePingResponse
    {
        public string? PingResponseCode { get; set; }
        public string? PingResponseMessage { get; set; }
        public DateTime? SessionExpirationDate { get; set; }
    }

    public class ReleaseGrantsRequest
    {
        public ReleaseGrantsRequest() { }

        public ReleaseGrantsRequest(List<GrantRequest> grantsToRelease)
        {
            GrantsToRelease = grantsToRelease;
        }

        public List<GrantRequest> GrantsToRelease { get; set; }
    }

    public class ReleaseGrantsResponse
    {
        public string? ReleaseGrantsResponseCode { get; set; }
        public string? ReleaseGrantsResponseMessage { get; set; }
    }

    public class ReleaseSessionRequest
    {
    }

    public class ReleaseSessionResponse
    {
        public string? ReleaseSessionResponseCode { get; set; }
        public string? ReleaseSessionResponseMessage { get; set; }
    }

    public class SessionResourcesResponse
    {
        public List<GrantResponse> AvailableGrantTypes { get; set; }
        public List<PackageDto> AvailablePackages { get; set; }
        public List<FeatureDto> AvailableFeatures { get; set; }
    }

    public class LicenseRow
    {
        public string Name { get; set; }
        public string SerialCode { get; set; }
        public string Description { get; set; }
        public string Edition { get; set; }
        public DateTime Expiration { get; set; }
    }

    /// <summary>
    /// Request to get Site ID for a specific planning area
    /// </summary>
    public class GetSiteIdRequest
    {
        public string PlanningAreaId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response containing Site ID information for a planning area
    /// </summary>
    public class GetSiteIdResponse
    {
        public bool Success { get; set; }
        public string? SiteId { get; set; }
        public string? Reason { get; set; }
        public int? PlanningAreaId { get; set; }
    }

    /// <summary>
    /// Request model for getting serial code from planning area by ID
    /// </summary>
    public class GetSerialCodeRequest
    {
        public string PlanningAreaId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for getting serial code from planning area by key
    /// </summary>
    public class GetSerialCodeByKeyRequest
    {
        public string PlanningAreaKey { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response containing Serial Code information for a planning area
    /// </summary>
    public class GetSerialCodeResponse
    {
        public bool Success { get; set; }
        public string? SerialCode { get; set; }
        public string? Reason { get; set; }
        public int? PlanningAreaId { get; set; }
    }

    /// <summary>
    /// Response containing Serial Code information for a planning area (by key)
    /// </summary>
    public class GetSerialCodeByKeyResponse
    {
        public bool Success { get; set; }
        public string? SerialCode { get; set; }
        public string? Reason { get; set; }
        public int? PlanningAreaId { get; set; }
    }

    #endregion

    #region DTOs

    public class PackageDto
    {
        public string Name { get; set; }
        public int PackageId { get; set; }
    }

    public class FeatureDto
    {
        public string Name { get; set; }
    }

    public class SiteDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    #endregion

    #region Enums

    public enum GrantTypeNames
    {
        Plant = 1,
        User = 2,
        PlanningArea = 3,
        Scenario = 4
    }

    public enum EnvironmentTypes
    {
        Dev = 0,
        QA = 1,
        Production = 2
    }

    #endregion
}