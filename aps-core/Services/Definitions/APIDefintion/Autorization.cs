using Microsoft.AspNetCore.Authentication;

namespace PT.APIDefinitions;

/// <summary>
/// The authentication scheme requires using a logged in session token
/// </summary>
public class PTSessionAuthSchemeOptions : AuthenticationSchemeOptions
{
    public const string SchemeName = "SessionTokenScheme";

    public IAuthorizer Authorizer { get; set; }
}

/// <summary>
/// The authentication scheme requires using a local server token for validation
/// </summary>
public class PTServerAuthSchemeOptions : AuthenticationSchemeOptions
{
    public const string SchemeName = "ServerTokenScheme";

    public IAuthorizer Authorizer { get; set; }
}