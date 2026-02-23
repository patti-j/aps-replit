using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using PT.APIDefinitions.RequestsAndResponses.License;
using PT.Common.Http;

namespace PT.LicenseService.Client;

/// <summary>
/// Exposes endpoints for the Planet Together License Service's Session API.
/// Utilize the SessionApiRequestFactory class for needed request parameters.
/// </summary>
public class PTLicenseServiceSessionClient : PTHttpClient
{
    private const string c_serverSideLSAddress = "https://localhost:7060/";
    private const string c_controller = "session";


    public PTLicenseServiceSessionClient(string a_serverAddress = c_serverSideLSAddress)
        : base($"api/{c_controller}/", a_serverAddress) { }


    public PTLicenseServiceSessionClient(AuthenticationHeaderValue a_authHeaderValue, string a_serverAddress = c_serverSideLSAddress)
        : base($"api/{c_controller}/", a_serverAddress, TimeSpan.FromMinutes(5.0), a_authHeaderValue) { }

    public void SetTimeout(TimeSpan a_timeout)
    {
        m_httpClient.Timeout = a_timeout;
    }

    /// <summary>
    /// Creates a secure session using planning area authentication with HMAC signature.
    /// This is the recommended approach for new implementations requiring planning area authentication.
    /// Returns detailed grant allocation information including site-specific and common grants.
    /// </summary>
    /// <param name="planningAreaId">Public identifier for the planning area</param>
    /// <param name="planningAreaKey">The planning area key (used as HMAC secret)</param>
    /// <param name="securityToken">The security token</param>
    /// <param name="environmentType">The environment type for the session</param>
    /// <param name="siteId">The site the session is being built for</param>
    /// <returns>SecureSessionResponse with detailed session and grant allocation information</returns>
    public SecureSessionResponse CreateSecureSession(string planningAreaId, string planningAreaKey, string securityToken, string environmentType, int? siteId = null)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var nonce = Guid.NewGuid().ToString();
        var signature = CreateSha256Signature(planningAreaKey, timestamp, nonce);

        PlanningAreaSessionRequest request = new PlanningAreaSessionRequest()
        {
            PlanningAreaId = planningAreaId,
            Timestamp = timestamp,
            Nonce = nonce,
            Signature = signature,
            Token = securityToken,
            SiteId = siteId,
            EnvironmentType = environmentType,
        };

        SecureSessionResponse response = MakePostRequest<SecureSessionResponse>("createSecureSession", request);

        if (response?.SessionId != null)
        {
            Authenticate(new AuthenticationHeaderValue(response.SessionId.ToString()));
        }

        return response;
    }

    /// <summary>
    /// Creates a secure session using planning area authentication with HMAC signature (async version).
    /// This is the recommended approach for new implementations requiring planning area authentication.
    /// Returns detailed grant allocation information including site-specific and common grants.
    /// </summary>
    /// <param name="planningAreaId">Public identifier for the planning area</param>
    /// <param name="planningAreaKey">The planning area key (used as HMAC secret)</param>
    /// <param name="securityToken">The security token</param>
    /// <param name="environmentType">The environment type for the session</param>
    /// <param name="siteId">The site the session is being built for</param>
    /// <returns>SecureSessionResponse with detailed session and grant allocation information</returns>
    public async Task<SecureSessionResponse> CreateSecureSessionAsync(string planningAreaId, string planningAreaKey, string securityToken, string environmentType, int? siteId = null)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var nonce = Guid.NewGuid().ToString();
        var signature = CreateSha256Signature(planningAreaKey, timestamp, nonce);

        PlanningAreaSessionRequest request = new PlanningAreaSessionRequest()
        {
            PlanningAreaId = planningAreaId,
            Timestamp = timestamp,
            Nonce = nonce,
            Signature = signature,
            Token = securityToken,
            SiteId = siteId,
            EnvironmentType = environmentType,
        };

        SecureSessionResponse response = await MakePostRequestAsync<SecureSessionResponse>("createSecureSession", request);

        if (response?.SessionId != null)
        {
            Authenticate(new AuthenticationHeaderValue(response.SessionId.ToString()));
        }

        return response;
    }

    /// <summary>
    /// Creates a new session using planning area authentication with HMAC signature.
    /// The client sends planningAreaId, timestamp, nonce, and HMAC signature for validation.
    /// </summary>
    /// <param name="planningAreaId">Public identifier for the planning area</param>
    /// <param name="planningAreaKey">The planning area key (used as HMAC secret)</param>
    /// <param name="securityToken">The security token</param>
    /// <param name="environmentType">The environment type for the session</param>
    /// <param name="siteId">The site the session is being built for</param>
    /// <returns>SessionRequestResponse with session details</returns>
    public SessionRequestResponse CreatePlanningAreaSession(string planningAreaId, string planningAreaKey, string securityToken, string environmentType, int? siteId = null)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var nonce = Guid.NewGuid().ToString();
        var signature = CreateSha256Signature(planningAreaKey, timestamp, nonce);

        PlanningAreaSessionRequest request = new PlanningAreaSessionRequest()
        {
            PlanningAreaId = planningAreaId,
            Timestamp = timestamp,
            Nonce = nonce,
            Signature = signature,
            Token = securityToken,
            SiteId = siteId,
            EnvironmentType = environmentType,
        };

        SessionRequestResponse response = MakePostRequest<SessionRequestResponse>("createPlanningAreaSession", request);

        if (response?.SessionId != null)
        {
            Authenticate(new AuthenticationHeaderValue(response.SessionId.ToString()));
        }

        return response;
    }

    /// <summary>
    /// Creates a new session using planning area authentication with HMAC signature (async version).
    /// The client sends planningAreaId, timestamp, nonce, and HMAC signature for validation.
    /// </summary>
    /// <param name="planningAreaId">Public identifier for the planning area</param>
    /// <param name="planningAreaKey">The planning area key (used as HMAC secret)</param>
    /// <param name="securityToken">The security token</param>
    /// <param name="environmentType">The environment type for the session</param>
    /// <param name="siteId">The site the session is being built for</param>
    /// <returns>SessionRequestResponse with session details</returns>
    public async Task<SessionRequestResponse> CreatePlanningAreaSessionAsync(string planningAreaId, string planningAreaKey, string securityToken, string environmentType, int? siteId = null)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var nonce = Guid.NewGuid().ToString();
        var signature = CreateSha256Signature(planningAreaKey, timestamp, nonce);

        PlanningAreaSessionRequest request = new PlanningAreaSessionRequest()
        {
            PlanningAreaId = planningAreaId,
            Timestamp = timestamp,
            Nonce = nonce,
            Signature = signature,
            Token = securityToken,
            SiteId = siteId,
            EnvironmentType = environmentType,
        };

        SessionRequestResponse response = await MakePostRequestAsync<SessionRequestResponse>("createPlanningAreaSession", request);

        if (response?.SessionId != null)
        {
            Authenticate(new AuthenticationHeaderValue(response.SessionId.ToString()));
        }

        return response;
    }

    /// <summary>
    /// Legacy method - use CreatePlanningAreaSession for new implementations
    /// Creates a new session for an existing Subscription.
    /// Once created, the session can allow and track active users by allotting a quantity of the Subscription's inactive Grants (via LockGrant below).
    /// The session will be automatically closed after a period of inactivity (without any pings or requests).
    /// </summary>
    /// <param name="serialCode">A PlanetTogether subscription serial code.</param>
    /// <param name="securityToken">The security token associated with that subscription.</param>
    /// <param name="siteId">The site the session is being built for, as identified in the License Service database. Can be null to create a session with access to the subscription's common grants only.</param>
    /// <returns>
    /// The SessionId for the newly created session, as well as a list of all grants (grouped by environment), packages and features for the corresponding subscription.
    /// The SessionId is needed to authorize any other API calls, all of which manage a particular session and its grants. 
    /// </returns>
    [Obsolete("Use CreatePlanningAreaSession for new implementations")]
    public SessionRequestResponse CreateNewSession(string serialCode, string securityToken, string environmentType, int? siteId = null)
    {
        LicenseServiceSessionDtos request = new LicenseServiceSessionDtos()
        {
            SerialCode = serialCode,
            Token = securityToken,
            SiteId = siteId,
            EnvironmentType = environmentType,
        };

        SessionRequestResponse response = MakePostRequest<SessionRequestResponse>("createNewSession", request);

        if (response?.SessionId != null)
        {
            Authenticate(new AuthenticationHeaderValue(response.SessionId.ToString()));
        }

        return response;
    }

    /// <summary>
    /// Legacy method - use CreatePlanningAreaSessionAsync for new implementations
    /// Creates a new session for an existing Subscription.
    /// Once created, the session can allow and track active users by allotting a quantity of the Subscription's inactive Grants (via LockGrant below).
    /// The session will be automatically closed after a period of inactivity (without any pings or requests).
    /// </summary>
    /// <param name="serialCode">A PlanetTogether subscription serial code.</param>
    /// <param name="securityToken">The security token associated with that subscription.</param>
    /// <param name="siteId">The site the session is being built for, as identified in the License Service database. Can be null to create a session with access to the subscription's common grants only.</param>
    /// <returns>
    /// The SessionId for the newly created session, as well as a list of all grants (grouped by environment), packages and features for the corresponding subscription.
    /// The SessionId is needed to authorize any other API calls, all of which manage a particular session and its grants. 
    /// </returns>
    [Obsolete("Use CreatePlanningAreaSessionAsync for new implementations")]
    public async Task<SessionRequestResponse> CreateNewSessionAsync(string serialCode, string securityToken, string environmentType, int? siteId = null)
    {
        LicenseServiceSessionDtos request = new LicenseServiceSessionDtos()
        {
            SerialCode = serialCode,
            Token = securityToken,
            SiteId = siteId,
            EnvironmentType = environmentType
        };

        SessionRequestResponse response = await MakePostRequestAsync<SessionRequestResponse>("createNewSession", request);

        if (response?.SessionId != null)
        {
            Authenticate(new AuthenticationHeaderValue(response.SessionId.ToString()));
        }

        return response;
    }

    /// <summary>
    /// Gets all Grant Types, Packages, and Features available to the Subscription of the session this client is authorized for.
    /// Grant Types will return quantities available (ie, the maximum amount less those currently in use).
    /// Note that other sessions making use of these resources may consume them without the current client being aware. Do not treat returned Grant Type quantities as fact if not immediately used.
    /// </summary>
    /// <param name="environmentType">The environment type to return grants for. Subscriptions have distinct grants for each environment (Production, Test, QA).</param>
    /// <returns>A list of grant quantity available by type, available to those using the session with the given environment type.</returns>
    /// <seealso cref="EnvironmentTypes"/>
    public SessionResourcesResponse GetSessionResources()
    {
        GetSessionResourcesRequest request = new GetSessionResourcesRequest() { };

        SessionResourcesResponse response = MakePostRequest<SessionResourcesResponse>("getSessionResources", request);

        return response;
    }

    /// <summary>
    /// Gets all Grant Types, Packages, and Features available to the Subscription of the session this client is authorized for.
    /// Grant Types will return quantities available (ie, the maximum amount less those currently in use).
    /// Note that other sessions making use of these resources may consume them without the current client being aware. Do not treat returned Grant Type quantities as fact if not immediately used.
    /// </summary>
    /// <param name="environmentType">The environment type to return grants for. Subscriptions have distinct grants for each environment (Production, Test, QA).</param>
    /// <returns>A list of grant quantity available by type, available to those using the session with the given environment type.</returns>
    /// <seealso cref="EnvironmentTypes"/>
    public async Task<SessionResourcesResponse> GetSessionResourcesAsync()
    {
        GetSessionResourcesRequest request = new GetSessionResourcesRequest() { };

        SessionResourcesResponse response = await MakePostRequestAsync<SessionResourcesResponse>("getSessionResources", request);

        return response;
    }

    /// <summary>
    /// Pings the server to check the status of the session. If still active, this will refresh its last modified status.
    /// Requires the targeted Session's Id value in the request's Authorization header.
    /// </summary>
    /// <returns>
    /// True if ping succeeds, otherwise throws an exception with the reason.
    /// </returns>
    public bool Ping()
    {
        LicensePingRequest request = new LicensePingRequest();

        LicensePingResponse response = MakePostRequest<LicensePingResponse>("ping", request);
        return true;
    }

    /// <summary>
    /// Pings the server to check the status of the session. If still active, this will refresh its last modified status.
    /// Requires the targeted Session's Id value in the request's Authorization header.
    /// </summary>
    /// <returns>
    /// True if ping succeeds, otherwise throws an exception with the reason.
    /// </returns>
    public async Task<bool> PingAsync()
    {
        LicensePingRequest request = new LicensePingRequest();

        LicensePingResponse response = await MakePostRequestAsync<LicensePingResponse>("ping", request);
        return true;
    }

    /// <summary>
    /// Adds active grant locks to a session using an identified Grant from the corresponding Subscription.
    /// Requires the targeted Session's Id value in the request's Authorization header.
    /// </summary>
    /// <param name="grantToLock">A GrantRequest containing the grant type and quantity to lock. If the quantity exceeds the available quantity for the subscription, the request will fail. </param>
    /// <param name="environmentType">The type of environment (Production, Test, QA) to lock grants for.</param>
    /// <returns>
    /// True if grants successfully lock, otherwise throws an exception and returns a reason message.
    /// </returns>
    /// <seealso cref="GrantTypeNames"/>
    /// <seealso cref="EnvironmentTypes"/>
    public bool LockGrant(GrantRequest grantToLock)
    {
        LockRequest request = new LockRequest()
        {
            GrantType = grantToLock.GrantType,
            GrantQty = grantToLock.GrantQty,
        };

        LockResponse response = MakePostRequest<LockResponse>("lockGrant", request);
        return true;
    }

    /// <summary>
    /// Adds active grant locks to a session using an identified Grant from the corresponding Subscription.
    /// Requires the targeted Session's Id value in the request's Authorization header.
    /// </summary>
    /// <param name="grantToLock">A GrantRequest containing the grant type and quantity to lock. If the quantity exceeds the available quantity for the subscription, the request will fail. </param>
    /// <param name="environmentType">The type of environment (Production, Test, QA) to lock grants for.</param>
    /// <returns>
    /// True if grants successfully lock, otherwise throws an exception and returns a reason message.
    /// </returns>
    /// <seealso cref="GrantTypeNames"/>
    /// <seealso cref="EnvironmentTypes"/>
    public async Task<bool> LockGrantAsync(GrantRequest grantToLock)
    {
        LockRequest request = new LockRequest()
        {
            GrantType = grantToLock.GrantType,
            GrantQty = grantToLock.GrantQty
        };

        LockResponse response = await MakePostRequestAsync<LockResponse>("lockGrant", request);
        return true;
    }

    /// <summary>
    /// Releases a quantity of active locks on a Session which come from one or more identified Grant Types.
    /// Requesting to release more of a grant quantity than is locked will unlock as many as possible and return successfully.
    /// </summary>
    /// <param name="request">A list of one or more request objects containing a targeted Grant's Id, and the quantity of locks to release from the session.</param>
    /// <param name="environmentType">The type of environment (Production, Test, QA) to lock grants for.</param>
    /// <returns>
    /// True as long as the service succeeds in checking the session and attempting to release the provided quantities.
    /// </returns>
    public bool ReleaseGrants(List<GrantRequest> grantsToRelease)
    {
        var request = new ReleaseGrantsRequest()
        {
            GrantsToRelease = grantsToRelease
        };

        ReleaseGrantsResponse response = MakePostRequest<ReleaseGrantsResponse>("releaseGrants", request);
        return true;
    }

    /// <summary>
    /// Releases a quantity of active locks on a Session which come from one or more identified Grant Types.
    /// Requesting to release more of a grant quantity than is locked will unlock as many as possible and return successfully.
    /// </summary>
    /// <param name="request">A list of one or more request objects containing a targeted Grant's Id, and the quantity of locks to release from the session.</param>
    /// <param name="environmentType">The type of environment (Production, Test, QA) to lock grants for.</param>
    /// <returns>
    /// True as long as the service succeeds in checking the session and attempting to release the provided quantities.
    /// </returns>
    public async Task<bool> ReleaseGrantsAsync(List<GrantRequest> grantsToRelease)
    {
        var request = new ReleaseGrantsRequest()
        {
            GrantsToRelease = grantsToRelease,
        };

        ReleaseGrantsResponse response = await MakePostRequestAsync<ReleaseGrantsResponse>("releaseGrants", request);
        return true;
    }

    /// <summary>
    /// Closes and removes an active session, releasing all active grant locks.
    /// </summary>
    /// <returns>
    /// True if the session and its grants were successfully released.
    /// </returns>
    public bool ReleaseSession()
    {
        var request = new ReleaseSessionRequest();

        ReleaseSessionResponse response = MakePostRequest<ReleaseSessionResponse>("releaseSession", request);
        return true;
    }

    /// <summary>
    /// Closes and removes an active session, releasing all active grant locks.
    /// </summary>
    /// <returns>
    /// True if the session and its grants were successfully released.
    /// </returns>
    public async Task<ReleaseSessionResponse> ReleaseSessionAsync()
    {
        var request = new ReleaseSessionRequest();

        ReleaseSessionResponse response = await MakePostRequestAsync<ReleaseSessionResponse>("releaseSession", request);
        return response;
    }

    /// <summary>
    /// Creates a SHA256 signature for license validation
    /// </summary>
    /// <param name="planningAreaKey">The planning area key to use as the secret</param>
    /// <param name="timestamp">The timestamp</param>
    /// <param name="nonce">The nonce</param>
    /// <returns>Base64 encoded SHA256 signature</returns>
    private static string CreateSha256Signature(string planningAreaKey, long timestamp, string nonce)
    {
        var combinedString = $"{planningAreaKey}|{timestamp}|{nonce}";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedString));
        return Convert.ToBase64String(hashBytes);
    }
}