using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PT.PlanetTogetherAPI.Server;

/// <summary>
/// Handles authorizing a route based on whether the user identity has an associated claim. The claim would be added during Authentication.
/// Will short-circuit with UnauthorizedResult if claim is not verified
/// </summary>
internal class ClaimAuthorization : IAuthorizationFilter
{
    private readonly Claim m_claim;

    public ClaimAuthorization(Claim a_claim)
    {
        m_claim = a_claim;
    }

    public void OnAuthorization(AuthorizationFilterContext a_context)
    {
        bool authorized;

        IEnumerable<Claim> userClaims = a_context.HttpContext?.User?.Claims;

        if (string.IsNullOrEmpty(m_claim.Value))
        {
            //Just validate on existence
            authorized = userClaims.Any(c => c.Type == m_claim.Type);
        }
        else
        {
            //Validate existence and value
            authorized = userClaims.Any(c => c.Type == m_claim.Type && c.Value == m_claim.Value);
        }

        if (!authorized)
        {
            a_context.Result = new UnauthorizedResult();
        }
    }
}

public class AuthorizeWithClaimAttribute : TypeFilterAttribute
{
    public AuthorizeWithClaimAttribute(string a_claimType, string a_claimValue) : base(typeof(ClaimAuthorization))
    {
        Arguments = new object[] { new Claim(a_claimType, a_claimValue) };
    }

    public AuthorizeWithClaimAttribute(string a_claimType) : base(typeof(ClaimAuthorization))
    {
        Arguments = new object[] { new Claim(a_claimType, string.Empty) };
    }
}