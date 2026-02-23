using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

using PT.APIDefinitions;
using PT.APSCommon.Interfaces;

namespace PT.PlanetTogetherAPI.Server;

/// <summary>
/// A SessionToken authorization scheme. This will authorize the user has logged in on the server
/// The Authorization claim will be set for the user identity so the SessionToken can be easily retrieved
/// </summary>
public class PTSessionAuthHandler : AuthenticationHandler<PTSessionAuthSchemeOptions>
{
    public PTSessionAuthHandler(IOptionsMonitor<PTSessionAuthSchemeOptions> options,
                                ILoggerFactory logger,
                                UrlEncoder encoder,
                                ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // handle authentication logic here

        // validation comes in here
        if (!Request.Headers.ContainsKey(HeaderNames.Authorization))
        {
            return Task.FromResult(AuthenticateResult.Fail("Header Not Found."));
        }

        if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out AuthenticationHeaderValue headerValue) || headerValue.Scheme != Scheme.Name || headerValue.Parameter == null)
        {
            return Task.FromResult(FailResult());
        }

        try
        {
            string token = headerValue.Parameter;
            if (!Options.Authorizer.ValidateAuthorization(token))
            {
                return Task.FromResult(FailResult());
            }

            //Add claims for 
            List<Claim> claims = new ()
            {
                new Claim(ClaimTypes.Authentication, token)
            };

            IUserPermissionSet permissionSet = Options.Authorizer.GetUserPermissions(token);
            if (permissionSet != null)
            {
                foreach (string permission in permissionSet.GetPermissions(true))
                {
                    claims.Add(new Claim(permission, string.Empty));
                }
            }

            //Add a claim for app connection. App users cannot access the data model, only APIs
            bool appConnection = Options.Authorizer.IsAppConnection(token);
            claims.Add(new Claim("AppUser", appConnection.ToString()));

            // generate claimsIdentity on the name of the class
            ClaimsIdentity claimsIdentity = new (claims, Scheme.Name);
            ClaimsPrincipal principle = new (claimsIdentity);

            // generate AuthenticationTicket from the Identity
            // and current authentication scheme
            AuthenticationTicket ticket = new (principle, Scheme.Name);

            // pass on the ticket to the middleware
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception Occured while Deserializing: " + ex);
            return Task.FromResult(FailResult());
        }

        // success branch
        // generate authTicket
        // authenticate the request

        /* todo */

        // failure branch
        // return failure with an optional message
        return Task.FromResult(FailResult());
    }

    private static AuthenticateResult FailResult()
    {
        return AuthenticateResult.Fail("Authorization Token is invalid");
    }
}

/// <summary>
/// A SessionToken authorization scheme. This will authorize the user has logged in on the server
/// The Authorization claim will be set for the user identity so the SessionToken can be easily retrieved
/// </summary>
public class PTServerAuthHandler : AuthenticationHandler<PTServerAuthSchemeOptions>
{
    public PTServerAuthHandler(IOptionsMonitor<PTServerAuthSchemeOptions> options,
                               ILoggerFactory logger,
                               UrlEncoder encoder,
                               ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // handle authentication logic here

        // validation comes in here
        if (!Request.Headers.ContainsKey(HeaderNames.Authorization))
        {
            return Task.FromResult(AuthenticateResult.Fail("Header Not Found."));
        }

        if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out AuthenticationHeaderValue headerValue) || headerValue.Scheme != Scheme.Name || headerValue.Parameter == null)
        {
            return Task.FromResult(FailResult());
        }

        try
        {
            string token = headerValue.Parameter;
            if (!Options.Authorizer.ValidateServerAuthorization(token))
            {
                return Task.FromResult(FailResult());
            }

            //Add claims for 
            List<Claim> claims = new ()
            {
                new Claim(ClaimTypes.Authentication, token)
            };

            // generate claimsIdentity on the name of the class
            ClaimsIdentity claimsIdentity = new (claims, Scheme.Name);
            ClaimsPrincipal principle = new (claimsIdentity);

            // generate AuthenticationTicket from the Identity
            // and current authentication scheme
            AuthenticationTicket ticket = new (principle, Scheme.Name);

            // pass on the ticket to the middleware
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception Occured while Deserializing: " + ex);
            return Task.FromResult(FailResult());
        }

        return Task.FromResult(FailResult());
    }

    private static AuthenticateResult FailResult()
    {
        return AuthenticateResult.Fail("Authorization Token is invalid");
    }
}