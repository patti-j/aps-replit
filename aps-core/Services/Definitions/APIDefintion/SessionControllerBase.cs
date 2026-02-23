using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PT.APIDefinitions;

//Authorize using the SessionToken scheme
[Authorize(AuthenticationSchemes = PTSessionAuthSchemeOptions.SchemeName)]
public class SessionControllerBase : ControllerBase
{
    //The claim should be created during Authentication.
    protected string UserToken => User.Claims.First(c => c.Type == ClaimTypes.Authentication).Value;
}